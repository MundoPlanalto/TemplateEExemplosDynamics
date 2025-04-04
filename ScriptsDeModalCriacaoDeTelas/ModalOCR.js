// Função para abrir a tela suspensa (modal) e anexar documentos
function abrirModalAnexarDocumentos(primaryControl) {
    console.log("Abrindo modal para anexar documentos...");

    var topDocument = top.document;

    // Cria a modal com design moderno
    var modalHtml = `
    <div id="customModal" style="
        position: fixed;
        top: 0; left: 0;
        width: 100%; height: 100%;
        background-color: rgba(0, 0, 0, 0.5);
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    ">
        <div style="
            background: #fff;
            border-radius: 8px;
            padding: 20px;
            width: 500px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
        ">
            <h3 style="margin-top: 0; text-align: center;">Anexar Documentos</h3>
            <input type="file" id="fileInput" multiple style="margin-bottom: 20px; width: 100%; display: none;" />
            <div style="text-align: center; margin-bottom: 20px;">
                <!-- Área para exibição dos nomes dos arquivos selecionados -->
                <div id="fileList" style="min-height: 30px; font-size: 14px;"></div>
            </div>
            <div style="text-align: center;">
                <button id="attachBtn" style="
                    background-color: #0078d4;
                    color: #fff;
                    border: none;
                    padding: 10px 20px;
                    margin: 5px;
                    border-radius: 4px;
                    cursor: pointer;
                ">Anexar Documentos</button>
                <button id="clearBtn" style="
                    background-color: #ffc107;
                    color: #000;
                    border: none;
                    padding: 10px 20px;
                    margin: 5px;
                    border-radius: 4px;
                    cursor: pointer;
                ">Limpar Seleção</button>
                <button id="sendBtn" disabled style="
                    background-color: #28a745;
                    color: #fff;
                    border: none;
                    padding: 10px 20px;
                    margin: 5px;
                    border-radius: 4px;
                    opacity: 0.5;
                    cursor: not-allowed;
                ">Enviar Documentos</button>
                <button id="closeModal" style="
                    background-color: #dc3545;
                    color: #fff;
                    border: none;
                    padding: 10px 20px;
                    margin: 5px;
                    border-radius: 4px;
                    cursor: pointer;
                ">Fechar</button>
            </div>
        </div>
    </div>`;

    var modalDiv = topDocument.createElement("div");
    modalDiv.innerHTML = modalHtml;
    topDocument.body.appendChild(modalDiv);

    // Referências aos elementos
    var fileInput = topDocument.getElementById("fileInput");
    var attachBtn = topDocument.getElementById("attachBtn");
    var sendBtn = topDocument.getElementById("sendBtn");
    var clearBtn = topDocument.getElementById("clearBtn");
    var fileListDiv = topDocument.getElementById("fileList");

    // Atualiza o estado do botão "Enviar Documentos"
    function atualizarEstadoEnvio() {
        if (fileInput.files.length > 0) {
            sendBtn.disabled = false;
            sendBtn.style.opacity = "1";
            sendBtn.style.cursor = "pointer";
        } else {
            sendBtn.disabled = true;
            sendBtn.style.opacity = "0.5";
            sendBtn.style.cursor = "not-allowed";
        }
    }

    // Atualiza a lista dos arquivos selecionados
    function atualizarListaArquivos() {
        if (fileInput.files.length > 0) {
            var nomes = [];
            for (var i = 0; i < fileInput.files.length; i++) {
                nomes.push(fileInput.files[i].name);
            }
            fileListDiv.innerHTML = "Selecionados: " + nomes.join(", ");
        } else {
            fileListDiv.innerHTML = "";
        }
    }

    // Dispara o seletor de arquivos
    attachBtn.addEventListener("click", function () {
        console.log("Botão Anexar Documentos clicado.");
        fileInput.click();
    });

    // Atualiza a seleção ao escolher arquivos
    fileInput.addEventListener("change", function () {
        console.log("Arquivos selecionados:", fileInput.files);
        atualizarListaArquivos();
        atualizarEstadoEnvio();
    });

    // Limpa os arquivos selecionados
    clearBtn.addEventListener("click", function () {
        console.log("Limpando seleção de arquivos.");
        fileInput.value = ""; // Limpa o input
        atualizarListaArquivos();
        atualizarEstadoEnvio();
    });

    // Ao clicar em "Enviar Documentos"
    sendBtn.addEventListener("click", async function () {
        console.log("Botão Enviar Documentos clicado.");
        if (fileInput.files.length === 0) {
            console.log("Nenhum arquivo selecionado.");
            alert("Por favor, selecione ao menos um arquivo.");
            return;
        }

        // Recupera os dados da Opportunity
        var opportunityId = primaryControl.data.entity.getId()
            .replace("{", "")
            .replace("}", "")
            .replace(/-/g, '')
            .toUpperCase();
        var opportunityName = primaryControl.getAttribute("name").getValue();
        console.log("Opportunity - GUID:", opportunityId, "Nome:", opportunityName);

        // Função para ler um arquivo como Blob
        function readFile(file) {
            return new Promise((resolve, reject) => {
                var reader = new FileReader();
                reader.onload = function () {
                    resolve(file);
                };
                reader.onerror = function (e) {
                    reject(e);
                };
                reader.readAsArrayBuffer(file);
            });
        }

        var filesArray = [];
        try {
            // Lê todos os arquivos selecionados (obtém o objeto file)
            for (var i = 0; i < fileInput.files.length; i++) {
                var file = fileInput.files[i];
                await readFile(file);
                filesArray.push(file);
                console.log("Arquivo preparado:", file.name);
            }

            // Recupera os dados do Contato associado à Opportunity
            var parentContact = primaryControl.getAttribute("parentcontactid").getValue();
            if (!parentContact || parentContact.length === 0) {
                console.error("Nenhum contato associado à Opportunity.");
                alert("Não foi possível encontrar um contato associado à Opportunity.");
                return;
            }
            var contactGuid = parentContact[0].id.replace("{", "").replace("}", "").toLowerCase();
            var contactRecord = await Xrm.WebApi.retrieveRecord("contact", contactGuid, "?$select=fullname");
            var contactFullName = contactRecord.fullname;
            console.log("Contato:", contactFullName, contactGuid);

            // Monta o folderPath para salvar o arquivo
            var contactGuidNoDashes = contactGuid.replace(/-/g, '');
            var contactFolder = `${contactFullName}_${contactGuidNoDashes}`;
            var opportunityFolder = `${opportunityName}_${opportunityId}`;
            var folderPath = `/contact/${contactFolder}/opportunity/${opportunityFolder}`;
            console.log("Pasta SharePoint:", folderPath);

            // Envia cada arquivo para o Power Automate
            for (var i = 0; i < filesArray.length; i++) {
                var file = filesArray[i];
                console.log("Enviando arquivo para Power Automate:", file.name);
                await enviarArquivoParaPowerAutomate(folderPath, file.name, file);
            }

            // Após o envio de todos os arquivos, chama a lógica do OCR
            await ChamaAPIOCR(primaryControl);

            // Informa o usuário que os arquivos foram enviados com sucesso
            var nomesArquivos = Array.from(filesArray).map(f => f.name).join(", ");
            alert("Documentos enviados com sucesso:\n" + nomesArquivos);
            console.log("Documentos enviados com sucesso:", nomesArquivos);
        } catch (error) {
            console.error("Erro ao ler ou enviar os arquivos:", error);
            alert("Erro ao processar os arquivos.");
        }
    });

    // Fecha a modal
    topDocument.getElementById("closeModal").addEventListener("click", function () {
        console.log("Fechando modal.");
        var modalElement = topDocument.getElementById("customModal");
        if (modalElement) {
            modalElement.parentNode.removeChild(modalElement);
        }
    });
}

/**
 * Converte um Blob para Base64.
 */
function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => {
            const base64String = reader.result.split(",")[1];
            resolve(base64String);
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

/**
 * Função para enviar o arquivo para o Power Automate.
 * O fluxo do Power Automate espera um JSON com:
 *  - folderPath: Caminho da pasta onde o arquivo será salvo.
 *  - fileName: Nome do arquivo.
 *  - fileContent: Conteúdo do arquivo em Base64.
 */
async function enviarArquivoParaPowerAutomate(folderPath, fileName, fileBlob) {
    try {
        const base64File = await blobToBase64(fileBlob);
        const payload = {
            folderPath: folderPath,
            fileName: fileName,
            fileContent: base64File
        };

        const powerAutomateUrl = "https://prod-05.brazilsouth.logic.azure.com:443/workflows/492e78aca5a94acf9734e5eb59112ba3/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=gjvha3myWT8CRbsJBjH5qlsEEhPlAYLRgCJSfACTF2Y";

        console.log("Enviando para Power Automate com payload:");
        console.log(payload);

        const response = await fetch(powerAutomateUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            console.log(`Arquivo '${fileName}' enviado com sucesso para o Power Automate.`);
        } else {
            let errorText = await response.text();
            console.error(`Erro ao enviar o arquivo '${fileName}' para o Power Automate:`, errorText);
        }
    } catch (error) {
        console.error("Erro ao converter ou enviar o arquivo:", error);
    }
}

/**
 * Função para chamar a API OCR.
 * Após o envio dos arquivos, esta função captura os campos necessários e monta um payload com:
 *   - listName: Nome de exibição da entidade (obtido via primaryControl.data.entity.getEntityName())
 *   - folderPath: {nome do registro}_{GUID} (GUID em maiúsculas e sem separadores)
 * Os dados do payload são logados no console para conferência antes de serem enviados via Xrm.WebApi.execute.
 */
async function ChamaAPIOCR(primaryControl) {
    try {
        // Captura o nome da entidade
        var listName = primaryControl.data.entity.getEntityName();
        // Recupera o nome do registro (campo primário)
        var recordName = primaryControl.getAttribute("name").getValue();
        // Recupera o GUID do registro, remove chaves e traços e converte para maiúsculas
        var recordGuid = primaryControl.data.entity.getId()
            .replace("{", "")
            .replace("}", "")
            .replace(/-/g, "")
            .toUpperCase();
        // Monta o folderPath para o OCR
        var folderPathOCR = recordName + "_" + recordGuid;

        // Cria o payload para a API OCR
        const payload = {
            listName: listName,
            folderPath: folderPathOCR
        };

        console.log("Enviando para OCR com payload:");
        console.log(payload);

        // Configura a chamada via Xrm.WebApi.execute
        const request = {
            jsonEntrada: JSON.stringify(payload),
            getMetadata: function () {
                return {
                    parameterTypes: {
                        jsonEntrada: { typeName: "Edm.String", structuralProperty: 1 }
                    },
                    operationType: 0,
                    operationName: "dev_OCR_API_PROCESSO"
                };
            }
        };

        console.log("Request configurado para OCR:");
        console.log(request);

        Xrm.WebApi.execute(request).then(
            function success(response) {
                if (response.ok) {
                    response.text().then(function (text) {
                        try {
                            const result = text ? JSON.parse(text) : {};
                            console.log("Resultado da API OCR:", result);
                        } catch (e) {
                            console.error("Erro ao processar JSON da resposta da OCR:", e);
                        }
                    });
                } else {
                    console.error("Resposta da API OCR não está OK. Código de status:", response.status);
                }
            },
            function error(error) {
                console.error("Erro ao executar ação personalizada da API OCR:", error);
            }
        );
    } catch (error) {
        console.error("Erro ao chamar API OCR:", error);
    }
}
