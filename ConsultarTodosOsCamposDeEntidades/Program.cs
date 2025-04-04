using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DynamicsEntityMetadataWithOptions
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Conectando ao Dynamics...");

            // Ajuste a string de conexão conforme necessário.
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

                    // Defina o nome lógico da entidade desejada
                    string entityName = "contact"; // Altere para a entidade que deseja consultar

                    // Cria a requisição para recuperar os atributos da entidade
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityName,
                        EntityFilters = EntityFilters.Attributes,
                        RetrieveAsIfPublished = true
                    };

                    var response = (RetrieveEntityResponse)serviceClient.Execute(request);
                    EntityMetadata metadata = response.EntityMetadata;

                    Console.WriteLine($"Campos da entidade: {entityName}");
                    Console.WriteLine(new string('-', 60));

                    // Itera sobre os atributos
                    foreach (var attribute in metadata.Attributes)
                    {
                        // Obter o display name. Caso não esteja definido, usa o logical name.
                        string displayName = attribute.DisplayName?.UserLocalizedLabel?.Label;
                        if (string.IsNullOrEmpty(displayName))
                            displayName = attribute.LogicalName;

                        string attributeType = attribute.AttributeTypeName?.Value ?? "N/A";
                        Console.WriteLine($"{displayName} - {attribute.LogicalName} - {attributeType}");

                        // Se for um atributo de conjunto de opções, exibe as opções disponíveis.
                        // Pode ser Picklist, State, Status ou OptionSet.
                        if (attribute is PicklistAttributeMetadata picklistAttr)
                        {
                            DisplayOptionSet(picklistAttr.OptionSet);
                        }
                        else if (attribute is StateAttributeMetadata stateAttr)
                        {
                            DisplayOptionSet(stateAttr.OptionSet);
                        }
                        else if (attribute is StatusAttributeMetadata statusAttr)
                        {
                            DisplayOptionSet(statusAttr.OptionSet);
                        }
                        else if (attribute is BooleanAttributeMetadata boolAttr)
                        {
                            // Boolean attributes possuem opções "Yes/No" ou similares.
                            DisplayOptionSet(boolAttr.OptionSet);
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        /// <summary>
        /// Exibe as opções de um OptionSetMetadata, indentadas.
        /// </summary>
        /// <param name="optionSet">Instância de OptionSetMetadataBase.</param>
        static void DisplayOptionSet(OptionSetMetadataBase optionSet)
        {
            if (optionSet == null)
                return;

            // Alguns OptionSetMetadataBase podem ser do tipo OptionSetMetadata, portanto, a conversão é segura.
            if (optionSet is OptionSetMetadata optionsMetadata)
            {
                foreach (var option in optionsMetadata.Options)
                {
                    string optionLabel = option.Label?.UserLocalizedLabel?.Label ?? "Sem rótulo";
                    int value = option.Value.HasValue ? option.Value.Value : 0;
                    Console.WriteLine($"\t{optionLabel} - {value}");
                }
            }
        }
    }
}
