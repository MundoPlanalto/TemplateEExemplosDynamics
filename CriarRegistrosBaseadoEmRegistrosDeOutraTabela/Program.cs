using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace NovaCobrancaIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("Iniciando processamento de cobranças baseadas nas parcelas adimplentes...");

            // String de conexão do Dynamics – ajuste conforme necessário
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

                    int totalProcessados = ProcessarCobrancas(serviceClient);
                    Log("Processamento concluído. Total de registros processados: " + totalProcessados);
                }
            }
            catch (Exception ex)
            {
                Log("Erro: " + ex.Message);
            }

            Log("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        /// <summary>
        /// Processa os registros da entidade dev_parcelainadimplente que não possuem cobrança vinculada:
        /// - Para cada registro, captura o campo dev_nutitulo.
        /// - Consulta registros da entidade dev_cobranca com o mesmo dev_nutitulo.
        /// - Se encontrado, atualiza o registro de dev_parcelainadimplente com a referência (lookup) à cobrança.
        /// - Se não encontrado, cria uma nova cobrança replicando os campos abaixo:
        ///     dev_cdcentrocusto, dev_cliente, dev_cpfcnpj, dev_dtemissao, dev_nmempresa,,
        ///     dev_nutitulo, dev_unidade, dev_valortitulo.
        /// - Antes de criar a nova cobrança, se o campo dev_cliente (lookup para contact) estiver preenchido,
        ///   recupera o fullname do contato para compor o campo subject como:
        ///   subject = fullname + " " + nmempresa + " " + nutitulo.
        /// - Após a criação, atualiza o registro de dev_parcelainadimplente para referenciar a cobrança criada.
        /// </summary>
        static int ProcessarCobrancas(ServiceClient serviceClient)
        {
            int registrosProcessados = 0;

            // Consulta os registros de dev_parcelainadimplente que NÃO possuem cobrança vinculada
            QueryExpression queryParcela = new QueryExpression("dev_parcelainadimplente")
            {
                ColumnSet = new ColumnSet("dev_nutitulo", "dev_cdcentrocusto", "dev_cliente", "dev_cpfcnpj",
                                            "dev_dtemissao", "dev_nmempresa", "dev_unidade", "dev_valortitulo", "dev_cobranca")
            };
            queryParcela.Criteria.AddCondition("dev_cobranca", ConditionOperator.Null);

            EntityCollection parcelas = serviceClient.RetrieveMultiple(queryParcela);
            Log("Total de registros de dev_parcelainadimplente sem cobrança vinculada: " + parcelas.Entities.Count);

            foreach (Entity parcela in parcelas.Entities)
            {
                if (!parcela.Contains("dev_nutitulo"))
                {
                    Log("Registro sem dev_nutitulo. Ignorado.");
                    continue;
                }

                int nutitulo = parcela.GetAttributeValue<int>("dev_nutitulo");
                Log("Processando registro com dev_nutitulo: " + nutitulo);

                // Consulta a entidade dev_cobranca filtrando pelo dev_nutitulo
                QueryExpression queryCobranca = new QueryExpression("dev_cobranca")
                {
                    ColumnSet = new ColumnSet("dev_nutitulo")
                };
                queryCobranca.Criteria.AddCondition("dev_nutitulo", ConditionOperator.Equal, nutitulo);

                EntityCollection cobrancas = serviceClient.RetrieveMultiple(queryCobranca);
                Guid cobrancaId = Guid.Empty;

                if (cobrancas != null && cobrancas.Entities.Count > 0)
                {
                    // Cobrança já existe; pega o primeiro registro
                    cobrancaId = cobrancas.Entities[0].Id;
                    Log("Cobrança existente encontrada para dev_nutitulo " + nutitulo + ". ID: " + cobrancaId);
                }
                else
                {
                    // Cria nova cobrança replicando os campos desejados
                    Entity novaCobranca = new Entity("dev_cobranca");

                    if (parcela.Contains("dev_cdcentrocusto"))
                        novaCobranca["dev_cdcentrocusto"] = parcela["dev_cdcentrocusto"];
                    if (parcela.Contains("dev_cliente"))
                        novaCobranca["dev_cliente"] = parcela["dev_cliente"];
                    if (parcela.Contains("dev_cpfcnpj"))
                        novaCobranca["dev_cpfcnpj"] = parcela["dev_cpfcnpj"];
                    if (parcela.Contains("dev_dtemissao"))
                        novaCobranca["dev_dtemissao"] = parcela["dev_dtemissao"];
                    // Usando o nome exato: "dev_nmempresa" (tudo minúsculo)
                    if (parcela.Contains("dev_nmempresa"))
                        novaCobranca["dev_nmempresa"] = parcela["dev_nmempresa"];

                    novaCobranca["dev_nutitulo"] = nutitulo;

                    if (parcela.Contains("dev_unidade"))
                        novaCobranca["dev_unidade"] = parcela["dev_unidade"];
                    if (parcela.Contains("dev_valortitulo"))
                        novaCobranca["dev_valortitulo"] = parcela["dev_valortitulo"];

                    // Recupera o fullname do cliente, se o lookup dev_cliente estiver preenchido.
                    string fullname = string.Empty;
                    if (parcela.Contains("dev_cliente"))
                    {
                        EntityReference clienteRef = parcela.GetAttributeValue<EntityReference>("dev_cliente");
                        if (clienteRef != null)
                        {
                            Entity contato = serviceClient.Retrieve("contact", clienteRef.Id, new ColumnSet("fullname"));
                            if (contato != null && contato.Attributes.Contains("fullname"))
                            {
                                fullname = contato["fullname"].ToString();
                            }
                        }
                    }

                    // Recupera nmempresa do registro (para compor o subject)
                    string nmEmpresa = string.Empty;
                    if (parcela.Contains("dev_nmempresa"))
                        nmEmpresa = parcela["dev_nmempresa"].ToString();

                    // Compoe o subject: fullname + " " + nmempresa + " " + nutitulo
                    string subject = $"{fullname} {nmEmpresa} {nutitulo}";
                    novaCobranca["subject"] = subject;

                    cobrancaId = serviceClient.Create(novaCobranca);
                    Log("Nova cobrança criada para dev_nutitulo " + nutitulo + ". ID: " + cobrancaId);
                }

                // Atualiza o registro de dev_parcelainadimplente para referenciar a cobrança encontrada/criada
                Entity updateParcela = new Entity("dev_parcelainadimplente", parcela.Id);
                updateParcela["dev_cobranca"] = new EntityReference("dev_cobranca", cobrancaId);
                serviceClient.Update(updateParcela);
                Log("Registro de dev_parcelainadimplente atualizado com dev_cobranca: " + cobrancaId);

                registrosProcessados++;
            }

            return registrosProcessados;
        }

        /// <summary>
        /// Log simples no console.
        /// </summary>
        static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
