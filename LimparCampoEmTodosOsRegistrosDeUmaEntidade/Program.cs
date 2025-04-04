using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace LimparCampoDevParcelasInadimplentes
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("Iniciando a limpeza do campo dev_parcelasinadimplentes...");

            // String de conexão do Dynamics usando OAuth - ajuste conforme necessário.
            string connectionString = "AuthType=OAuth;" +
                                      "Url=https://orgcd82df8b.crm2.dynamics.com;" +
                                      "Username=ti@planaltosolucoes.onmicrosoft.com;" +
                                      "Password=Gbjardan1.4;" +
                                      "LoginPrompt=Auto;";

            try
            {
                using (ServiceClient serviceClient = new ServiceClient(connectionString))
                {
                    if (!serviceClient.IsReady)
                    {
                        Log("Erro de conexão: " + serviceClient.LastError);
                        return;
                    }

                    int totalAtualizados = LimparCampo(serviceClient);
                    Log("Total de registros atualizados: " + totalAtualizados);
                }
            }
            catch (Exception ex)
            {
                Log("Erro: " + ex.Message);
            }

            Log("Processo concluído. Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        /// <summary>
        /// Consulta os registros da entidade contact que possuem valor no campo dev_parcelasinadimplentes
        /// e atualiza este campo para null.
        /// </summary>
        /// <param name="serviceClient">Instância autenticada do ServiceClient.</param>
        /// <returns>Total de registros atualizados.</returns>
        static int LimparCampo(ServiceClient serviceClient)
        {
            int atualizados = 0;

            // Cria uma Query para buscar os registros onde dev_parcelasinadimplentes não é nulo.
            QueryExpression query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid", "dev_parcelasinadimplentes")
            };
            query.Criteria.AddCondition("dev_parcelasinadimplentes", ConditionOperator.NotNull);

            EntityCollection contatos = serviceClient.RetrieveMultiple(query);
            Log("Registros encontrados com valor em dev_parcelasinadimplentes: " + contatos.Entities.Count);

            foreach (Entity contato in contatos.Entities)
            {
                // Define o campo como null (limpando o valor)
                contato["dev_parcelasinadimplentes"] = null;
                serviceClient.Update(contato);
                atualizados++;
            }

            return atualizados;
        }

        /// <summary>
        /// Método simples para log no console.
        /// </summary>
        static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
