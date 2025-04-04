function abrirModalParcelas(primaryControl) {
    // Obtenha os options dos campos de opção da cobrança
    var motivoOptions = [];
    var situacaoOptions = [];
    var ferramentaOptions = []; // Nova variável para o campo dev_ferramentautilizada

    var motivoAttr = primaryControl.getAttribute("dev_motivodainadimplenciafeedbackdocliente");
    if (motivoAttr) {
        motivoOptions = motivoAttr.getOptions();
    } else {
        console.error("Campo 'dev_motivodainadimplenciafeedbackdocliente' não encontrado. Verifique o nome lógico no formulário.");
    }
    
    var situacaoAttr = primaryControl.getAttribute("dev_estadodanegociacao");
    if (situacaoAttr) {
        situacaoOptions = situacaoAttr.getOptions();
    } else {
        console.error("Campo 'dev_estadodanegociacao' não encontrado. Verifique o nome lógico no formulário.");
    }
    
    // Novo campo: Ferramenta Utilizada
    var ferramentaAttr = primaryControl.getAttribute("dev_ferramentautilizada");
    if (ferramentaAttr) {
        ferramentaOptions = ferramentaAttr.getOptions();
    } else {
        console.error("Campo 'dev_ferramentautilizada' não encontrado. Verifique o nome lógico no formulário.");
    }
    
    // Obtenha o controle da subgrid (substitua "Subgrid_new_1" pelo nome real do controle)
    var subgridControl = primaryControl.getControl("Subgrid_new_1");
    if (!subgridControl) {
        alert("Subgrid não encontrada.");
        return;
    }

    // Recupere todos os registros (linhas) da subgrid
    var grid = subgridControl.getGrid();
    if (!grid) {
        alert("Não foi possível acessar os dados da subgrid.");
        return;
    }

    var rows = grid.getRows();
    if (rows.getLength() === 0) {
        alert("Nenhuma parcela encontrada na subgrid.");
        return;
    }

    // Monta uma lista com os dados necessários das parcelas
    var parcelas = [];
    rows.forEach(function (row) {
        var entity = row.getData().getEntity();
        var nuparcela = entity.attributes.get("dev_nuparcela") ? entity.attributes.get("dev_nuparcela").getValue() : "";
        var valor = entity.attributes.get("dev_valorcorrigido") ? entity.attributes.get("dev_valorcorrigido").getValue() : 0;
        var dtemissao = entity.attributes.get("dev_dtemissao") ? entity.attributes.get("dev_dtemissao").getValue() : "";
        var dtvencto = entity.attributes.get("dev_dtvencto") ? entity.attributes.get("dev_dtvencto").getValue() : "";
        var nmempresa = entity.attributes.get("dev_nmempresa") ? entity.attributes.get("dev_nmempresa").getValue() : "";
        var cdcentrocusto = entity.attributes.get("dev_cdcentrocusto") ? entity.attributes.get("dev_cdcentrocusto").getValue() : "";
        
        parcelas.push({
            id: entity.getId(),
            nuparcela: nuparcela,
            valor: valor,
            dtemissao: dtemissao,
            dtvencto: dtvencto,
            nmempresa: nmempresa,
            cdcentrocusto: cdcentrocusto
        });
    });

    // Gera os elementos <option> para os selects a partir dos options obtidos
    function gerarOptions(optionsArray) {
        var html = "";
        optionsArray.forEach(function(opt) {
            // opt.value é o valor lógico, opt.text é o rótulo exibido
            html += `<option value="${opt.value}">${opt.text}</option>`;
        });
        return html;
    }

    // Função auxiliar para obter o texto de exibição a partir do valor selecionado
    function getOptionText(optionsArray, selectedValue) {
        var option = optionsArray.find(function(opt) {
            return String(opt.value) === String(selectedValue);
        });
        return option ? option.text : "";
    }

    // Cria o HTML do modal com os novos campos de seleção e os demais elementos
    var topDocument = top.document;
    var modalHtml = `
    <style>
      /* Estilos para o modal moderno e responsivo */
      #modalParcelas {
          position: fixed;
          top: 0;
          left: 0;
          width: 100%;
          height: 100%;
          background-color: rgba(0, 0, 0, 0.6);
          z-index: 10000;
          display: flex;
          align-items: center;
          justify-content: center;
          padding: 10px;
          box-sizing: border-box;
      }
      #modalParcelas .modal-container {
          background: #fff;
          border-radius: 10px;
          padding: 20px;
          width: 100%;
          max-width: 800px;
          max-height: 90%;
          overflow-y: auto;
          box-shadow: 0 8px 16px rgba(0,0,0,0.3);
          animation: fadeIn 0.3s ease-in-out;
      }
      #modalParcelas h3 {
          text-align: center;
          margin-top: 0;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
          color: #333;
      }
      /* Seção de Informação da Negociação */
      .negociacao-section {
          margin-bottom: 20px;
          padding: 15px;
          border: 1px solid #ddd;
          border-radius: 5px;
          background-color: #f9f9f9;
      }
      .negociacao-section label {
          font-weight: 600;
          margin-right: 10px;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
          color: #333;
      }
      .negociacao-section select {
          padding: 5px;
          font-size: 1em;
          border-radius: 3px;
          border: 1px solid #ccc;
      }
      /* Lista de Parcelas */
      #listaParcelas table {
          width: 100%;
          border-collapse: collapse;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      }
      #listaParcelas th, #listaParcelas td {
          padding: 10px;
          border-bottom: 1px solid #ddd;
          text-align: left;
      }
      #listaParcelas th {
          background-color: #f8f8f8;
          font-weight: 600;
      }
      #listaParcelas tr:hover {
          background-color: #f1f1f1;
      }
      /* Seção de Totais */
      .totals-section {
          margin-top: 20px;
          border: 1px solid #ddd;
          border-radius: 5px;
          padding: 15px;
          background-color: #f9f9f9;
      }
      .totals-section h4 {
          margin: 0 0 10px;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
          color: #333;
      }
      .totals-table {
          width: 100%;
          border-collapse: collapse;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      }
      .totals-table th, .totals-table td {
          padding: 8px;
          text-align: left;
      }
      .totals-table th {
          background-color: #efefef;
          width: 50%;
      }
      .modal-footer {
          display: flex;
          flex-wrap: wrap;
          justify-content: flex-end;
          align-items: center;
          margin-top: 20px;
      }
      .modal-footer button {
          padding: 10px 20px;
          margin: 5px;
          border: none;
          border-radius: 5px;
          font-size: 1em;
          cursor: pointer;
      }
      .btn-confirm {
          background-color: #28a745;
          color: #fff;
      }
      .btn-cancel {
          background-color: #dc3545;
          color: #fff;
      }
      @keyframes fadeIn {
          from { opacity: 0; transform: translateY(-10px); }
          to { opacity: 1; transform: translateY(0); }
      }
      /* Responsividade */
      @media (max-width: 600px) {
          #listaParcelas th, #listaParcelas td {
              padding: 8px;
              font-size: 0.9em;
          }
          .modal-footer button {
              width: 100%;
              margin: 5px 0;
          }
          .totals-table th, .totals-table td {
              padding: 6px;
              font-size: 0.9em;
          }
      }
    </style>
    <div id="modalParcelas">
      <div class="modal-container">
         <h3>Selecionar Parcelas Inadimplentes</h3>
         <!-- Seção de Informação da Negociação -->
         <div class="negociacao-section">
             <div style="margin-bottom:10px;">
                 <label for="selMotivo">Motivo de Inadimplência:</label>
                 <select id="selMotivo">
                     ${gerarOptions(motivoOptions)}
                 </select>
             </div>
             <div style="margin-bottom:10px;">
                 <label for="selSituacao">Situação da Negociação:</label>
                 <select id="selSituacao">
                     ${gerarOptions(situacaoOptions)}
                 </select>
             </div>
             <!-- Novo campo de seleção para Ferramenta Utilizada -->
             <div>
                 <label for="selFerramenta">Ferramenta Utilizada:</label>
                 <select id="selFerramenta">
                     ${gerarOptions(ferramentaOptions)}
                 </select>
             </div>
         </div>
         <!-- Parte 1: Lista de Parcelas -->
         <div id="listaParcelas">
            <table>
               <thead>
                  <tr>
                     <th>
                        <input type="checkbox" id="chkSelectAll" style="margin-right: 5px;" />
                        No Parcela
                     </th>
                     <th>Valor Corrigido</th>
                     <th>Emissão</th>
                     <th>Vencimento</th>
                     <th>Empresa</th>
                     <th>Centro de Custo</th>
                  </tr>
               </thead>
               <tbody>
                  ${parcelas.map(function(parcela) {
                      return `<tr>
                          <td>
                              <label style="display: flex; align-items: center;">
                                  <input type="checkbox" class="chkParcela" data-valor="${parcela.valor}" data-dtvencto="${parcela.dtvencto}" style="margin-right: 8px;" />
                                  ${parcela.nuparcela}
                              </label>
                          </td>
                          <td>${parseFloat(parcela.valor).toFixed(2)}</td>
                          <td>${parcela.dtemissao ? new Date(parcela.dtemissao).toLocaleDateString() : ""}</td>
                          <td>${parcela.dtvencto ? new Date(parcela.dtvencto).toLocaleDateString() : ""}</td>
                          <td>${parcela.nmempresa}</td>
                          <td>${parcela.cdcentrocusto}</td>
                      </tr>`;
                  }).join('')}
               </tbody>
            </table>
         </div>
         <!-- Parte 2: Seção de Totais -->
         <div class="totals-section">
            <h4>Valores Corrigidos</h4>
            <table class="totals-table">
              <tr>
                <th>Valor Base</th>
                <td id="valorBase">0.00</td>
              </tr>
              <tr>
                <th>Total de Multas</th>
                <td id="totalMultas">0.00</td>
              </tr>
              <tr>
                <th>Total de Juros</th>
                <td id="totalJuros">0.00</td>
              </tr>
              <tr>
                <th>Valor Total</th>
                <td id="valorTotal">0.00</td>
              </tr>
            </table>
         </div>
         <!-- Parte 3: Botões -->
         <div class="modal-footer">
            <button id="btnConfirmar" class="btn-confirm">Confirmar</button>
            <button id="btnCancelar" class="btn-cancel">Cancelar</button>
         </div>
      </div>
    </div>
    `;

    var modalDiv = topDocument.createElement("div");
    modalDiv.innerHTML = modalHtml;
    topDocument.body.appendChild(modalDiv);

    // Função para recalcular os totais ao marcar/desmarcar checkboxes ou alterar a ferramenta utilizada
    function calcularTotais() {
        var totalBase = 0;
        var totalMultas = 0;
        var totalJuros = 0;
        var totalFinal = 0;
        var checkboxes = topDocument.getElementsByClassName("chkParcela");
        var hoje = new Date();
        // Taxa diária definida para o exemplo: 0,0000905 (aprox. 0,00905% ao dia)
        var taxaDiaria = 0.0000905;
        // Verifica se a opção de Ferramenta Utilizada é "Isenção de Multa e Juros"
        var selFerramenta = topDocument.getElementById("selFerramenta").value;
        var isencao = (selFerramenta === "622490000");

        Array.prototype.forEach.call(checkboxes, function (chk) {
            if (chk.checked) {
                var base = parseFloat(chk.getAttribute("data-valor"));
                var multa = 0;
                var juros = 0;
                if (!isencao) {
                    multa = base * 0.02;
                    var valorComMulta = base + multa;
                    var dtvenctoStr = chk.getAttribute("data-dtvencto");
                    var diasAtraso = 0;
                    if (dtvenctoStr) {
                        var dtvencto = new Date(dtvenctoStr);
                        var diffMs = hoje - dtvencto;
                        diasAtraso = Math.floor(diffMs / (1000 * 60 * 60 * 24));
                        if (diasAtraso < 0) diasAtraso = 0;
                    }
                    juros = valorComMulta * taxaDiaria * diasAtraso;
                }
                totalBase += base;
                totalMultas += multa;
                totalJuros += juros;
            }
        });
        totalFinal = totalBase + totalMultas + totalJuros;
        topDocument.getElementById("valorBase").innerText = totalBase.toFixed(2);
        topDocument.getElementById("totalMultas").innerText = totalMultas.toFixed(2);
        topDocument.getElementById("totalJuros").innerText = totalJuros.toFixed(2);
        topDocument.getElementById("valorTotal").innerText = totalFinal.toFixed(2);
    }

    // Adiciona o evento para recalcular os totais sempre que um checkbox for alterado
    var checkboxes = topDocument.getElementsByClassName("chkParcela");
    Array.prototype.forEach.call(checkboxes, function (chk) {
        chk.addEventListener("change", calcularTotais);
    });

    // Evento para o checkbox "Selecionar Tudo"
    var chkSelectAll = topDocument.getElementById("chkSelectAll");
    chkSelectAll.addEventListener("change", function () {
        var newState = this.checked;
        Array.prototype.forEach.call(checkboxes, function (chk) {
            chk.checked = newState;
        });
        calcularTotais();
    });

    // Recalcula os totais quando a ferramenta utilizada for alterada
    topDocument.getElementById("selFerramenta").addEventListener("change", calcularTotais);

    // Evento para cancelar e fechar o modal
    topDocument.getElementById("btnCancelar").addEventListener("click", function () {
        var modal = topDocument.getElementById("modalParcelas");
        if (modal) {
            modal.parentNode.removeChild(modal);
        }
    });

    // Evento para confirmar e atualizar a cobrança com os valores calculados e os campos de seleção,
    // além de montar a descrição e criar a anotação conforme o formato solicitado.
    topDocument.getElementById("btnConfirmar").addEventListener("click", function () {
        var totalFinal = parseFloat(topDocument.getElementById("valorTotal").innerText);
        if (isNaN(totalFinal) || totalFinal <= 0) {
            alert("Selecione ao menos uma parcela com valor maior que zero.");
            return;
        }
        // Obtém os valores selecionados nos selects (valor lógico)
        var selMotivo = topDocument.getElementById("selMotivo").value;
        var selSituacao = topDocument.getElementById("selSituacao").value;
        var selFerramenta = topDocument.getElementById("selFerramenta").value;
        
        // Obtém os textos de exibição dos selects
        var motivoDisplay = getOptionText(motivoOptions, selMotivo);
        var situacaoDisplay = getOptionText(situacaoOptions, selSituacao);
        var ferramentaDisplay = getOptionText(ferramentaOptions, selFerramenta);
        
        // Obtém os totais novamente
        var totalBase = parseFloat(topDocument.getElementById("valorBase").innerText);
        var totalMultas = parseFloat(topDocument.getElementById("totalMultas").innerText);
        var totalJuros = parseFloat(topDocument.getElementById("totalJuros").innerText);
        
        // Obtém o nome do usuário logado como atendente
        var owner = Xrm.Utility.getGlobalContext().userSettings.userName;
        
        // Data atual formatada
        var now = new Date();
        var formattedDate = now.toLocaleDateString();

        // Monta a descrição conforme o formato solicitado. A descrição já inicia com a data atual.
        var descricao = formattedDate + "\n";
        descricao += "Nova Negociação Registrada\n";
        descricao += "Atendente: " + owner + "\n";
        descricao += "Parcelas negociadas:\n";
        
        // Itera pelos checkboxes para obter as parcelas selecionadas
        Array.prototype.forEach.call(checkboxes, function (chk, index) {
            if (chk.checked) {
                // Usa o mesmo índice para recuperar os dados da parcela (assumindo que a ordem é preservada)
                var parcela = parcelas[index];
                descricao += "Parcela: " + parcela.nuparcela +
                             " | Valor: " + parseFloat(parcela.valor).toFixed(2) +
                             " | Emissão: " + (parcela.dtemissao ? new Date(parcela.dtemissao).toLocaleDateString() : "") +
                             " | Vencimento: " + (parcela.dtvencto ? new Date(parcela.dtvencto).toLocaleDateString() : "") +
                             " | Empresa: " + parcela.nmempresa +
                             " | Centro de Custo: " + parcela.cdcentrocusto + "\n";
            }
        });
        
        descricao += "\nMotivo de Inadimplência: " + motivoDisplay + "\n";
        descricao += "Estado da Negociação: " + situacaoDisplay + "\n";
        descricao += "Ferramenta Utilizada: " + ferramentaDisplay + "\n\n";
        descricao += "Valor Base: " + totalBase.toFixed(2) + "\n";
        descricao += "Total de Multas: " + totalMultas.toFixed(2) + "\n";
        descricao += "Total de Juros: " + totalJuros.toFixed(2) + "\n";
        descricao += "Valor Total: " + totalFinal.toFixed(2);
        
        // Atualiza a cobrança com o valor total e a descrição montada
        var cobrancaId = primaryControl.data.entity.getId().replace("{", "").replace("}", "");
        var updateData = {
            "dev_valordoacordo": totalFinal,
            "dev_motivodainadimplenciafeedbackdocliente": selMotivo,
            "dev_estadodanegociacao": selSituacao,
            "dev_ferramentautilizada": selFerramenta,
            "dev_descricaonegociacao": descricao
        };
        Xrm.WebApi.updateRecord("dev_cobranca", cobrancaId, updateData).then(
            function success(result) {
                // Cria a anotação usando o conteúdo da descrição sem duplicar a data
                var noteSubject = "Novo Registro de Negociação " + formattedDate;
                var noteBody = descricao; // Usa apenas o conteúdo já montado
                var annotation = {
                    "subject": noteSubject,
                    "notetext": noteBody,
                    // Realiza o bind da anotação com a cobrança; ajuste o nome do entity set se necessário.
                    "objectid_dev_cobranca@odata.bind": "/dev_cobrancas(" + cobrancaId + ")"
                };
                Xrm.WebApi.createRecord("annotation", annotation).then(
                    function successNote(noteResult) {
                        alert("Cobrança e anotação atualizadas com o valor total: " + totalFinal.toFixed(2));
                        var modal = topDocument.getElementById("modalParcelas");
                        if (modal) {
                            modal.parentNode.removeChild(modal);
                        }
                        primaryControl.data.refresh();
                    },
                    function (error) {
                        console.error("Erro ao criar a anotação: ", error);
                        alert("Cobrança atualizada, mas houve erro ao criar a anotação.");
                    }
                );
            },
            function (error) {
                console.error("Erro ao atualizar a cobrança: ", error);
                alert("Erro ao atualizar a cobrança.");
            }
        );
    });
}
