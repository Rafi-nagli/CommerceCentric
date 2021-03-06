/** 
 * Copyright 2016 Telerik AD                                                                                                                                                                            
 *                                                                                                                                                                                                      
 * Licensed under the Apache License, Version 2.0 (the "License");                                                                                                                                      
 * you may not use this file except in compliance with the License.                                                                                                                                     
 * You may obtain a copy of the License at                                                                                                                                                              
 *                                                                                                                                                                                                      
 *     http://www.apache.org/licenses/LICENSE-2.0                                                                                                                                                       
 *                                                                                                                                                                                                      
 * Unless required by applicable law or agreed to in writing, software                                                                                                                                  
 * distributed under the License is distributed on an "AS IS" BASIS,                                                                                                                                    
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.                                                                                                                             
 * See the License for the specific language governing permissions and                                                                                                                                  
 * limitations under the License.                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       

*/

(function(f){
    if (typeof define === 'function' && define.amd) {
        define(["kendo.core"], f);
    } else {
        f();
    }
}(function(){
(function ($, undefined) {
/* Filter cell operator messages */

if (kendo.ui.FilterCell) {
kendo.ui.FilterCell.prototype.options.operators =
$.extend(true, kendo.ui.FilterCell.prototype.options.operators,{
  "date": {
    "eq": "?? igual a",
    "gt": "?? posterior a",
    "gte": "?? posterior ou igual a",
    "lt": "?? anterior a",
    "lte": "?? anterior ou igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "enums": {
    "eq": "?? igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "number": {
    "eq": "?? igual a",
    "gt": "?? maior que",
    "gte": "?? maior que ou igual a",
    "lt": "?? menor que",
    "lte": "?? menor que ou igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "string": {
    "contains": "Cont??m",
    "doesnotcontain": "N??o cont??m",
    "endswith": "Termina com",
    "eq": "?? igual a",
    "neq": "N??o ?? igual a",
    "startswith": "Come??a com",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo",
    "isempty": "?? vazio",
    "isnotempty": "?? n??o vazio"
  }
});
}

/* Filter menu operator messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.operators =
$.extend(true, kendo.ui.FilterMenu.prototype.options.operators,{
  "date": {
    "eq": "?? igual a",
    "gt": "?? posterior a",
    "gte": "?? posterior ou igual a",
    "lt": "?? anterior a",
    "lte": "?? anterior ou igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "enums": {
    "eq": "?? igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "number": {
    "eq": "?? igual a",
    "gt": "?? maior que",
    "gte": "?? maior que ou igual a",
    "lt": "?? menor que",
    "lte": "?? menor que ou igual a",
    "neq": "N??o ?? igual a",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo"
  },
  "string": {
    "contains": "Cont??m",
    "doesnotcontain": "N??o cont??m",
    "endswith": "Termina com",
    "eq": "?? igual a",
    "neq": "N??o ?? igual a",
    "startswith": "Come??a com",
    "isnull": "?? nulo",
    "isnotnull": "?? n??o nulo",
    "isempty": "?? vazio",
    "isnotempty": "?? n??o vazio"
  }
});
}

/* ColumnMenu messages */

if (kendo.ui.ColumnMenu) {
kendo.ui.ColumnMenu.prototype.options.messages =
$.extend(true, kendo.ui.ColumnMenu.prototype.options.messages,{
  "columns": "Colunas",
  "settings": "Configura????es de Colunas",
  "done": "Feito",
  "sortAscending": "Ordenar Ascendente",
  "sortDescending": "Ordenar Descendente",
  "lock": "Congelar",
  "unlock": "Descongelar"
});
}

/* RecurrenceEditor messages */

if (kendo.ui.RecurrenceEditor) {
kendo.ui.RecurrenceEditor.prototype.options.messages =
$.extend(true, kendo.ui.RecurrenceEditor.prototype.options.messages,{
  "daily": {
    "interval": "dia(s)",
    "repeatEvery": "Repetir todo:"
  },
  "end": {
    "endCountAfter": "Ap??s",
    "endCountOccurrence": "ocorr??ncia(s)",
    "endLabel": "Fim:",
    "endNever": "Nunca",
    "endUntilOn": "Em",
    "mobileLabel": "Ends"
  },
  "frequencies": {
    "daily": "Diariamente",
    "monthly": "Mensalmente",
    "never": "Nunca",
    "weekly": "Semanalmente",
    "yearly": "Anualmente"
  },
  "monthly": {
    "day": "Dia",
    "interval": "m??s(es)",
    "repeatEvery": "Repetir todo:",
    "repeatOn": "Repetir em:"
  },
  "offsetPositions": {
    "first": "quinto",
    "fourth": "quarto",
    "last": "??ltimo",
    "second": "segundo",
    "third": "terceiro"
  },
  "weekly": {
    "repeatEvery": "Repetir todo:",
    "repeatOn": "Repetir em:",
    "interval": "semana(s)"
  },
  "yearly": {
    "of": "de",
    "repeatEvery": "Repetir todo:",
    "repeatOn": "Repetir em:",
    "interval": "ano(s)"
  }
});
}

/* Editor messages */

if (kendo.ui.Editor) {
kendo.ui.Editor.prototype.options.messages =
$.extend(true, kendo.ui.Editor.prototype.options.messages,{
  "addColumnLeft": "Nova coluna ?? esquerda",
  "addColumnRight": "Nova coluna ?? direita",
  "addRowAbove": "Nova coluna acima",
  "addRowBelow": "Nova coluna abaixo",
  "backColor": "Cor de Fundo",
  "bold": "Negrito",
  "createLink": "Adicionar Link",
  "createTable": "Criar a tabela",
  "deleteColumn": "Excluir coluna",
  "deleteFile": "Tem certeza de que deseja remover \"{0}\"?",
  "deleteRow": "Excluir linha",
  "dialogButtonSeparator": "ou",
  "dialogCancel": "Cancelar",
  "dialogInsert": "Inserir",
  "directoryNotFound": "Um diret??rio com este nome n??o foi encontrado.",
  "dropFilesHere": "Arraste e solte arquyivos aqui para envi??-los",
  "emptyFolder": "Pasta vazia",
  "fontName": "Fonte",
  "fontNameInherit": "(fonte herdada)",
  "fontSize": "Tamanho",
  "fontSizeInherit": "(tamanho herdado)",
  "foreColor": "Cor",
  "formatBlock": "Formatar Bloco",
  "imageAltText": "Texto alternativo",
  "imageWebAddress": "Endere??o web",
  "indent": "Aumentar Recuo",
  "insertHtml": "Inserir HTML",
  "insertImage": "Inserir Imagem",
  "insertOrderedList": "Inserir Lista Ordenada",
  "insertUnorderedList": "Inserir Lista Aleat??ria",
  "invalidFileType": "O arquivo selecionado \"{0}\" n??o ?? v??lido. Os tipos de arquivo suportados s??o {1}.",
  "italic": "It??lico",
  "justifyCenter": "Centralizar",
  "justifyFull": "Justificar",
  "justifyLeft": "Alinhar ?? Esquerda",
  "justifyRight": "Alinhar ?? Direita",
  "linkOpenInNewWindow": "Abrir link em nova janela",
  "linkText": "Texto",
  "linkToolTip": "ToolTip",
  "linkWebAddress": "Endere??o web",
  "orderBy": "Ordenar por:",
  "orderByName": "Nome",
  "orderBySize": "Tamanho",
  "outdent": "Diminuir Recuo",
  "overwriteFile": "Um arquivo de nome \"{0}\" j?? existe no diret??rio atual. Deseja substitu??-lo?",
  "search": "Procurar",
  "strikethrough": "Tachado",
  "styles": "Estilo",
  "subscript": "Subscrito",
  "superscript": "Sobrescrito",
  "underline": "Sublinhado",
  "unlink": "Remove Hyperlink",
  "uploadFile": "Enviar arquivo",
  "formatting": "Formato",
  "viewHtml": "Exibir c??digo HTML",
  "dialogUpdate": "Atualizar",
  "insertFile": "Inserir arquivo"
});
}

/* FilterCell messages */

if (kendo.ui.FilterCell) {
kendo.ui.FilterCell.prototype.options.messages =
$.extend(true, kendo.ui.FilterCell.prototype.options.messages,{
  "clear": "Limpar",
  "filter": "Filtrar",
  "isFalse": "?? falso",
  "isTrue": "?? verdade",
  "operator": "Operador"
});
}

/* FilterMenu messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.messages =
$.extend(true, kendo.ui.FilterMenu.prototype.options.messages,{
  "and": "E",
  "cancel": "Cancelar",
  "clear": "Limpar",
  "filter": "Filtrar",
  "info": "Exibir linhas com valores que",
  "isFalse": "?? falso",
  "isTrue": "?? verdade",
  "operator": "Operador",
  "or": "Ou",
  "selectValue": "-Selecione uma op????o-",
  "value": "Valor"
});
}

/* FilterMultiCheck messages */

if (kendo.ui.FilterMultiCheck) {
kendo.ui.FilterMultiCheck.prototype.options.messages =
$.extend(true, kendo.ui.FilterMultiCheck.prototype.options.messages,{
  "search": "Procurar"
});
}

/* Grid messages */

if (kendo.ui.Grid) {
kendo.ui.Grid.prototype.options.messages =
$.extend(true, kendo.ui.Grid.prototype.options.messages,{
  "commands": {
    "canceledit": "Cancelar",
    "cancel": "Cancelar altera????es",
    "create": "Inserir",
    "destroy": "Excluir",
    "edit": "Editar",
    "excel": "Export to Excel",
    "pdf": "Export to PDF",
    "save": "Salvar altera????es",
    "select": "Selecionar",
    "update": "Atualizar"
  },
  "editable": {
    "cancelDelete": "Cancelar",
    "confirmation": "Voc?? tem certeza que deseja excluir este registro?",
    "confirmDelete": "Excluir"
  }
});
}

/* Groupable messages */

if (kendo.ui.Groupable) {
kendo.ui.Groupable.prototype.options.messages =
$.extend(true, kendo.ui.Groupable.prototype.options.messages,{
  "empty": "Arraste aqui o cabe??alho de uma coluna para agrupar por esta coluna"
});
}

/* Pager messages */

if (kendo.ui.Pager) {
kendo.ui.Pager.prototype.options.messages =
$.extend(true, kendo.ui.Pager.prototype.options.messages,{
  "allPages": "All",
  "display": "Exibindo itens {0} - {1} de {2}",
  "empty": "Nenhum registro encontrado.",
  "first": "Ir para a primeira p??gina",
  "itemsPerPage": "itens por p??gina",
  "last": "Ir para a ??ltima p??gina",
  "next": "Ir para a pr??xima p??gina",
  "of": "de {0}",
  "page": "P??gina",
  "previous": "Ir para a p??gina anterior",
  "refresh": "Atualizar",
  "morePages": "Mais p??ginas"
});
}

/* Scheduler messages */

if (kendo.ui.Scheduler) {
kendo.ui.Scheduler.prototype.options.messages =
$.extend(true, kendo.ui.Scheduler.prototype.options.messages,{
  "allDay": "dia inteiro",
  "cancel": "Cancelar",
  "editable": {
    "confirmation": "Tem certeza que deseja excluir este evento?"
  },
  "date": "Data",
  "deleteWindowTitle": "Excluir evento",
  "destroy": "Excluir",
  "editor": {
    "allDayEvent": "Evento de dia inteiro",
    "description": "Descri????o",
    "editorTitle": "Evento",
    "end": "Fim",
    "endTimezone": "Fuso-hor??rio final",
    "repeat": "Repetir",
    "separateTimezones": "Usar fuso-hor??rio diferente para o in??cio e fim",
    "start": "In??cio",
    "startTimezone": "Fuso-hor??rio inicial",
    "timezone": "",
    "timezoneEditorButton": "Fuso hor??rio",
    "timezoneEditorTitle": "Fusos-hor??rios",
    "title": "T??tulo",
    "noTimezone": "Sem fuso-hor??rio"
  },
  "event": "Evento",
  "recurrenceMessages": {
    "deleteRecurring": "Voc?? deseja excluir apenas este evento ou todas as ocorr??ncias?",
    "deleteWindowOccurrence": "Excluir ocorr??ncia atual",
    "deleteWindowSeries": "Excluir s??rie",
    "deleteWindowTitle": "Excluir Item Recorrente",
    "editRecurring": "Voc?? quer editar apenas este evento ou a s??rie inteira?",
    "editWindowOccurrence": "Editar ocorr??ncia atual",
    "editWindowSeries": "Editar s??rie",
    "editWindowTitle": "Editar item recorrente"
  },
  "save": "Gravar",
  "showFullDay": "Dia inteiro",
  "showWorkDay": "Hor??rio comercial",
  "time": "Hora",
  "today": "Hoje",
  "views": {
    "agenda": "Agenda",
    "day": "Dia",
    "month": "M??s",
    "week": "Semana",
    "workWeek": "Semana de trabalho"
  }
});
}

/* Upload messages */

if (kendo.ui.Upload) {
kendo.ui.Upload.prototype.options.localization =
$.extend(true, kendo.ui.Upload.prototype.options.localization,{
  "cancel": "Cancelar",
  "dropFilesHere": "arraste arquivos aqui para enviar",
  "headerStatusUploaded": "Pronto",
  "headerStatusUploading": "Carregando...",
  "remove": "Remover",
  "retry": "Tentar novamente",
  "select": "Selecionar...",
  "statusFailed": "falhou",
  "statusUploaded": "enviado",
  "statusUploading": "enviando",
  "uploadSelectedFiles": "Enviar arquivos"
});
}

/* Validator messages */

if (kendo.ui.Validator) {
kendo.ui.Validator.prototype.options.messages =
$.extend(true, kendo.ui.Validator.prototype.options.messages,{
  "required": "{0} ?? obrigat??rio",
  "pattern": "{0} n??o ?? v??lido",
  "min": "{0} deve ser maior ou igual a {1}",
  "max": "{0} deve ser menor ou igual a {1}",
  "step": "{0} n??o ?? v??lido",
  "email": "{0} n??o ?? um email v??lido",
  "url": "{0} n??o ?? um endere??o web v??lido",
  "date": "{0} n??o ?? uma data v??lida",
  "dateCompare": "A data final deve ser posterior ?? data inicial"
});
}
})(window.kendo.jQuery);
}));