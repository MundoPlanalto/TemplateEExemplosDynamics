using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages; // Para PublishAllXmlRequest

// Alias para resolver ambiguidade do tipo Label
using CrmLabel = Microsoft.Xrm.Sdk.Label;

namespace CreateFieldsTemplate
{
    class Program
    {
        // O componente custom field possui ComponentType = 2
        const int COMPONENT_TYPE_ATTRIBUTE = 2;

        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando o processo de criação de campos no Dynamics...");
            Console.WriteLine("Conectando ao Dynamics...");

            // Ajuste a string de conexão conforme seu ambiente.
            string connectionString = "AuthType=OAuth;" +
                                      "Url=https://orgcd82df8b.crm2.dynamics.com;" +
                                      "Username=ti@planaltosolucoes.onmicrosoft.com;" +
                                      "Password=Gbjardan1.4;" +
                                      "LoginPrompt=Auto;";

            // Se desejar inserir os campos em uma solução específica, informe o nome único da solução;
            // Caso contrário, deixe solutionUniqueName vazio para utilizar a solução padrão.
            string solutionUniqueName = "HubdeFluxos"; // ou string.Empty;

            // Defina o prefixo desejado para os campos (ex: "dev_"). Se não desejar prefixo, defina string.Empty.
            string prefix = "new_";

            using (ServiceClient serviceClient = new ServiceClient(connectionString))
            {
                if (!serviceClient.IsReady)
                {
                    Console.WriteLine("Erro de conexão: " + serviceClient.LastError);
                    return;
                }
                Console.WriteLine("Conexão realizada com sucesso!");

                // Nome lógico da entidade de destino (substitua pelo nome desejado)
                string entityName = "dev_entplaceholder";  // Sem prefixo aqui, pois o prefixo será adicionado nos campos
                Console.WriteLine($"Entidade alvo: {entityName}");

                // Criação dos campos. O prefixo será concatenado com o nome lógico de cada campo.
                CreateTextField(serviceClient, entityName, prefix + "textfield", "Text Field", 100, solutionUniqueName);
                CreateIntegerField(serviceClient, entityName, prefix + "integerfield", "Integer Field", solutionUniqueName);
                CreateDecimalField(serviceClient, entityName, prefix + "decimalfield", "Decimal Field", 8, 2, solutionUniqueName);
                CreateMoneyField(serviceClient, entityName, prefix + "moneyfield", "Money Field", 8, 2, solutionUniqueName);
                CreateDateTimeField(serviceClient, entityName, prefix + "datetimefield", "DateTime Field", DateTimeFormat.DateOnly, solutionUniqueName);
                CreateBooleanField(serviceClient, entityName, prefix + "booleanfield", "Boolean Field", "Yes", "No", solutionUniqueName);
                CreatePicklistField(serviceClient, entityName, prefix + "picklistfield", "Picklist Field", solutionUniqueName);
                CreateMemoField(serviceClient, entityName, prefix + "memofield", "Memo Field", 500, solutionUniqueName);

                // Publica as alterações
                PublishAllAttributes(serviceClient, entityName);
            }

            Console.WriteLine("Processo concluído. Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static void CreateTextField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, int maxLength, string solutionUniqueName)
        {
            Console.WriteLine($"[TextField] Iniciando criação do campo de Texto '{schemaName}'...");
            try
            {
                StringAttributeMetadata attribute = new StringAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    MaxLength = maxLength,
                    FormatName = StringFormatName.Text
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[TextField] Campo de Texto '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateIntegerField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, string solutionUniqueName)
        {
            Console.WriteLine($"[IntegerField] Iniciando criação do campo Inteiro '{schemaName}'...");
            try
            {
                IntegerAttributeMetadata attribute = new IntegerAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None)
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[IntegerField] Campo Inteiro '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IntegerField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateDecimalField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, int precision, int scale, string solutionUniqueName)
        {
            Console.WriteLine($"[DecimalField] Iniciando criação do campo Decimal '{schemaName}'...");
            try
            {
                DecimalAttributeMetadata attribute = new DecimalAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    Precision = precision,
                    MinValue = 0,
                    MaxValue = 1000000
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[DecimalField] Campo Decimal '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DecimalField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateMoneyField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, int precision, int scale, string solutionUniqueName)
        {
            Console.WriteLine($"[MoneyField] Iniciando criação do campo Money '{schemaName}'...");
            try
            {
                MoneyAttributeMetadata attribute = new MoneyAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    Precision = precision,
                    MinValue = 0,
                    MaxValue = 1000000
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[MoneyField] Campo Money '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MoneyField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateDateTimeField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, DateTimeFormat format, string solutionUniqueName)
        {
            Console.WriteLine($"[DateTimeField] Iniciando criação do campo DateTime '{schemaName}'...");
            try
            {
                DateTimeAttributeMetadata attribute = new DateTimeAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    Format = format,
                    DateTimeBehavior = DateTimeBehavior.UserLocal
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[DateTimeField] Campo DateTime '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DateTimeField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateBooleanField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, string trueLabel, string falseLabel, string solutionUniqueName)
        {
            Console.WriteLine($"[BooleanField] Iniciando criação do campo Booleano '{schemaName}'...");
            try
            {
                BooleanAttributeMetadata attribute = new BooleanAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None)
                };

                // Define o OptionSet para o campo Booleano utilizando OptionMetadata
                BooleanOptionSetMetadata boolOptionSet = new BooleanOptionSetMetadata();
                boolOptionSet.TrueOption = new OptionMetadata(new CrmLabel(trueLabel, 1046), 1);
                boolOptionSet.FalseOption = new OptionMetadata(new CrmLabel(falseLabel, 1046), 0);
                attribute.OptionSet = boolOptionSet;

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[BooleanField] Campo Booleano '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BooleanField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreatePicklistField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, string solutionUniqueName)
        {
            Console.WriteLine($"[PicklistField] Iniciando criação do campo Picklist '{schemaName}'...");
            try
            {
                // Criação de um OptionMetadataCollection com as opções desejadas
                OptionMetadataCollection options = new OptionMetadataCollection();
                options.Add(new OptionMetadata(new CrmLabel("Option 1", 1046), 1));
                options.Add(new OptionMetadata(new CrmLabel("Option 2", 1046), 2));

                // Criação do OptionSetMetadata local (IsGlobal = false)
                OptionSetMetadata optionSetMetadata = new OptionSetMetadata(options)
                {
                    IsGlobal = false
                };

                PicklistAttributeMetadata attribute = new PicklistAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    OptionSet = optionSetMetadata
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[PicklistField] Campo Picklist '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PicklistField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void CreateMemoField(ServiceClient serviceClient, string entityName, string schemaName, string displayName, int maxLength, string solutionUniqueName)
        {
            Console.WriteLine($"[MemoField] Iniciando criação do campo Memo '{schemaName}'...");
            try
            {
                MemoAttributeMetadata attribute = new MemoAttributeMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new CrmLabel(displayName, 1046),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    MaxLength = maxLength
                };

                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = entityName,
                    Attribute = attribute
                };

                CreateAttributeResponse response = (CreateAttributeResponse)serviceClient.Execute(req);
                Console.WriteLine($"[MemoField] Campo Memo '{schemaName}' criado com sucesso.");

                if (!string.IsNullOrEmpty(solutionUniqueName))
                {
                    AddComponentToSolution(serviceClient, response.AttributeId, COMPONENT_TYPE_ATTRIBUTE, solutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MemoField] Erro ao criar o campo '{schemaName}': {ex.Message}");
            }
        }

        static void PublishAllAttributes(ServiceClient serviceClient, string entityName)
        {
            Console.WriteLine("[Publish] Iniciando publicação de todos os atributos da entidade...");
            try
            {
                PublishAllXmlRequest req = new PublishAllXmlRequest();
                serviceClient.Execute(req);
                Console.WriteLine($"[Publish] Publicação dos atributos da entidade '{entityName}' concluída.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Publish] Erro durante a publicação dos atributos: {ex.Message}");
            }
        }

        static void AddComponentToSolution(ServiceClient serviceClient, Guid componentId, int componentType, string solutionUniqueName)
        {
            try
            {
                AddSolutionComponentRequest addReq = new AddSolutionComponentRequest
                {
                    ComponentId = componentId,
                    ComponentType = componentType,
                    SolutionUniqueName = solutionUniqueName
                };
                serviceClient.Execute(addReq);
                Console.WriteLine($"[Solution] Componente {componentId} adicionado à solução '{solutionUniqueName}' com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Solution] Erro ao adicionar o componente à solução '{solutionUniqueName}': {ex.Message}");
            }
        }
    }
}
