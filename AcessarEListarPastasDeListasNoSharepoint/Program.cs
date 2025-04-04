using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointDynamicFolderListing
{
    // Representação do site
    public class Site
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    // Representação da lista (biblioteca de documentos)
    public class SharePointList
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class ListCollection
    {
        [JsonProperty("value")]
        public List<SharePointList> Value { get; set; }
    }

    // Representação do drive associado à biblioteca de documentos
    public class Drive
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    // Representação de um item (pasta ou arquivo) no drive
    public class DriveItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        // "name" é o nome do item
        [JsonProperty("name")]
        public string Name { get; set; }
        // Se existir a propriedade "folder", então é uma pasta
        [JsonProperty("folder")]
        public JObject Folder { get; set; }
    }

    public class DriveItemCollection
    {
        [JsonProperty("value")]
        public List<DriveItem> Value { get; set; }
    }

    class Program
    {
        // Credenciais e configurações (atenção: evite hardcoding em produção)
        static readonly string tenantId = "a42e67f5-9efb-4c24-abad-f9685218188c";
        static readonly string clientId = "47f06494-4fb5-4642-8f4e-c17aa2183283";
        static readonly string clientSecret = "7VQ8Q~RhAAToh7gbvRFWShroMmyzfuvHUlVqbdeq";

        static async Task Main(string[] args)
        {
            // Exemplo: biblioteca (lista) "Contato" e caminho da pasta com delimitador "/"
            string listName = "Conta";
            string folderPath = "testando_41D62FCE689CEF11A72D00224836AE3D";
            await ListarItensPorCaminho(listName, folderPath);
        }

        public static async Task ListarItensPorCaminho(string listName, string folderPath)
        {
            using var client = new HttpClient();

            // Obter token de acesso
            string token = await ObterToken(client);
            if (string.IsNullOrEmpty(token))
                return;
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Obter o ID do site usando hostname e caminho
            string siteId = await ObterSiteId(client, "planaltosolucoes.sharepoint.com", "/sites/DesenvolvimentoAppDynamics");
            if (string.IsNullOrEmpty(siteId))
                return;

            // Obter o ID da lista (biblioteca de documentos) pelo displayName
            string listId = await ObterListaId(client, siteId, listName);
            if (string.IsNullOrEmpty(listId))
                return;

            // Obter o drive associado à biblioteca (lista)
            string driveId = await ObterDriveId(client, siteId, listId);
            if (string.IsNullOrEmpty(driveId))
                return;

            // Dividir o folderPath em partes (usando '/' ou '\' como delimitador)
            var pathParts = folderPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string currentFolderId = null; // null indica a raiz do drive

            // Navegar por cada parte do caminho
            foreach (var folderName in pathParts)
            {
                // Listar o conteúdo do nível atual para depuração
                Console.WriteLine($"\nConteúdo da pasta atual (FolderID = {currentFolderId ?? "raiz"})");
                await ListarItensDeUmaPasta(client, driveId, currentFolderId);

                // Procurar a pasta com o nome desejado no nível atual
                currentFolderId = await ObterPastaPorNome(client, driveId, currentFolderId, folderName);
                if (string.IsNullOrEmpty(currentFolderId))
                {
                    Console.WriteLine($"Pasta '{folderName}' não encontrada no caminho especificado.");
                    return;
                }
            }

            // Listar os itens finais da pasta encontrada
            Console.WriteLine("\nConteúdo da pasta final:");
            await ListarItensDeUmaPasta(client, driveId, currentFolderId);
        }

        // Função para obter token via client_credentials
        static async Task<string> ObterToken(HttpClient client)
        {
            Console.WriteLine("Solicitando token de acesso...");
            var tokenResponse = await client.PostAsync(
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "scope", "https://graph.microsoft.com/.default" },
                    { "client_secret", clientSecret },
                    { "grant_type", "client_credentials" }
                })
            );
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
            if (!tokenResult.ContainsKey("access_token"))
            {
                Console.WriteLine("Erro ao obter token: " + tokenJson);
                return null;
            }
            Console.WriteLine("Token obtido com sucesso.");
            return tokenResult["access_token"];
        }

        // Função para obter o ID do site
        static async Task<string> ObterSiteId(HttpClient client, string hostName, string sitePath)
        {
            var siteUrl = $"https://graph.microsoft.com/v1.0/sites/{hostName}:{sitePath}";
            var siteResponse = await client.GetAsync(siteUrl);
            if (!siteResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao obter site: {siteResponse.StatusCode}");
                return null;
            }
            var siteJson = await siteResponse.Content.ReadAsStringAsync();
            var siteObj = JsonConvert.DeserializeObject<Site>(siteJson);
            if (siteObj == null || string.IsNullOrEmpty(siteObj.Id))
            {
                Console.WriteLine("Site não encontrado ou sem ID.");
                return null;
            }
            Console.WriteLine($"Site encontrado: ID = {siteObj.Id}");
            return siteObj.Id;
        }

        // Função para obter o ID da lista (biblioteca de documentos) pelo displayName
        static async Task<string> ObterListaId(HttpClient client, string siteId, string listaNome)
        {
            var listsUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists";
            var listsResponse = await client.GetAsync(listsUrl);
            if (!listsResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao obter listas: {listsResponse.StatusCode}");
                return null;
            }
            var listsJson = await listsResponse.Content.ReadAsStringAsync();
            var listsObj = JsonConvert.DeserializeObject<ListCollection>(listsJson);
            var lista = listsObj.Value.FirstOrDefault(l =>
                l.DisplayName.Equals(listaNome, StringComparison.OrdinalIgnoreCase));
            if (lista == null)
            {
                Console.WriteLine($"Lista '{listaNome}' não encontrada.");
                return null;
            }
            Console.WriteLine($"Lista '{listaNome}' encontrada: ID = {lista.Id}");
            return lista.Id;
        }

        // Função para obter o drive associado à biblioteca (lista)
        static async Task<string> ObterDriveId(HttpClient client, string siteId, string listId)
        {
            var driveUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/drive";
            var driveResponse = await client.GetAsync(driveUrl);
            if (!driveResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao obter drive: {driveResponse.StatusCode}");
                return null;
            }
            var driveJson = await driveResponse.Content.ReadAsStringAsync();
            var driveObj = JsonConvert.DeserializeObject<Drive>(driveJson);
            if (driveObj == null || string.IsNullOrEmpty(driveObj.Id))
            {
                Console.WriteLine("Drive não encontrado ou sem ID.");
                return null;
            }
            Console.WriteLine($"Drive obtido: ID = {driveObj.Id}");
            return driveObj.Id;
        }

        // Função para listar os itens (arquivos e pastas) dentro de uma pasta do drive.
        // Se folderId for null, lista itens do nível raiz do drive.
        static async Task ListarItensDeUmaPasta(HttpClient client, string driveId, string folderId)
        {
            string url;
            if (string.IsNullOrEmpty(folderId))
            {
                url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root/children";
            }
            else
            {
                url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{folderId}/children";
            }

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao listar itens da pasta: {response.StatusCode}");
                return;
            }
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<DriveItemCollection>(json);
            if (data?.Value == null || data.Value.Count == 0)
            {
                Console.WriteLine("Nenhum item encontrado nesta pasta.");
                return;
            }

            Console.WriteLine("Itens nesta pasta:");
            foreach (var item in data.Value)
            {
                // Se a propriedade 'folder' não for nula, trata-se de uma pasta
                string tipo = item.Folder != null ? "Pasta" : "Arquivo";
                Console.WriteLine($"  ID: {item.Id} | {tipo}: {item.Name}");
            }
        }

        // Função para obter uma pasta pelo nome dentro do nível atual (folderId)
        // Se folderId for null, procura no nível raiz do drive
        static async Task<string> ObterPastaPorNome(HttpClient client, string driveId, string folderId, string folderName)
        {
            string url;
            if (string.IsNullOrEmpty(folderId))
            {
                url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root/children";
            }
            else
            {
                url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{folderId}/children";
            }

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao listar itens: {response.StatusCode}");
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<DriveItemCollection>(json);
            if (data?.Value == null)
                return null;

            // Exibir todos os itens para depuração
            Console.WriteLine("Itens encontrados:");
            foreach (var item in data.Value)
            {
                string tipo = item.Folder != null ? "Pasta" : "Arquivo";
                Console.WriteLine($"  ID: {item.Id} | {tipo}: {item.Name}");
            }

            // Filtrar pelo nome da pasta (comparação ignorando caixa)
            var pasta = data.Value.FirstOrDefault(i =>
                i.Folder != null && i.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (pasta == null)
            {
                Console.WriteLine($"Pasta '{folderName}' não encontrada.");
                return null;
            }
            Console.WriteLine($"Pasta '{folderName}' encontrada: ID = {pasta.Id}");
            return pasta.Id;
        }
    }
}
