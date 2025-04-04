using System;
using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DeleteRecordsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando a exclusão dos registros da entidade dev_parcelainadimplente...");

            // String de conexão com o Dynamics usando OAuth
            string connectionString = "AuthType=OAuth;" +
                                      "Url=https://orgcd82df8b.crm2.dynamics.com;" +
                                      "Username=ti@planaltosolucoes.onmicrosoft.com;" +
                                      "Password=Gbjardan1.4;" +
                                      "LoginPrompt=Auto;";

            try
            {
                using (var serviceClient = new ServiceClient(connectionString))
                {
                    if (!serviceClient.IsReady)
                    {
                        Console.WriteLine("Erro de conexão: " + serviceClient.LastError);
                        return;
                    }
                    Console.WriteLine("Conexão com o Dynamics realizada com sucesso!");

                    // Configura a consulta para recuperar apenas o ID dos registros
                    QueryExpression query = new QueryExpression("dev_parcelainadimplente")
                    {
                        ColumnSet = new ColumnSet("dev_parcelainadimplenteid"),
                        PageInfo = new PagingInfo
                        {
                            Count = 1000, // Quantidade de registros por página
                            PageNumber = 1
                        }
                    };

                    int totalExcluidos = 0;

                    // Loop para paginação
                    while (true)
                    {
                        EntityCollection results = serviceClient.RetrieveMultiple(query);

                        // Se não houver registros, encerra o loop
                        if (results.Entities.Count == 0)
                        {
                            break;
                        }

                        // Exclui cada registro retornado e exibe o log do ID
                        foreach (Entity registro in results.Entities)
                        {
                            Console.WriteLine("Tentando excluir registro com ID: " + registro.Id);
                            serviceClient.Delete("dev_parcelainadimplente", registro.Id);
                            totalExcluidos++;
                        }

                        // Verifica se há mais registros a serem processados
                        if (results.MoreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = results.PagingCookie;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Console.WriteLine($"Total de registros excluídos: {totalExcluidos}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao excluir registros: " + ex.Message);
            }

            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
