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
/* Filter menu operator messages */

if (kendo.ui.FilterCell) {
kendo.ui.FilterCell.prototype.options.operators =
$.extend(true, kendo.ui.FilterCell.prototype.options.operators,{
  "date": {
    "eq": "E??ittir",
    "gt": "Sonra",
    "gte": "Sonra ya da e??it",
    "lt": "??nce",
    "lte": "??nce ya da e??it",
    "neq": "E??it de??ildir"
  },
  "enums": {
    "eq": "E??ittir",
    "neq": "E??it de??ildir"
  },
  "number": {
    "eq": "E??ittir",
    "gt": "B??y??kt??r",
    "gte": "Daha b??y??k veya e??ittir",
    "lt": "Daha k??????k",
    "lte": "Daha k??????k veya e??it",
    "neq": "E??it de??ildir"
  },
  "string": {
    "contains": "????eriyor",
    "doesnotcontain": "????ermiyor",
    "endswith": "??le biter",
    "eq": "E??ittir",
    "neq": "E??it de??ildir",
    "startswith": "??le ba??lar"
  }
});
}

/* Filter menu operator messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.operators =
$.extend(true, kendo.ui.FilterMenu.prototype.options.operators,{
  "date": {
    "eq": "E??ittir",
    "gt": "Sonra",
    "gte": "Sonra ya da e??it",
    "lt": "??nce",
    "lte": "??nce ya da e??it",
    "neq": "E??it de??ildir"
  },
  "enums": {
    "eq": "E??ittir",
    "neq": "E??it de??ildir"
  },
  "number": {
    "eq": "E??ittir",
    "gt": "B??y??kt??r",
    "gte": "Daha b??y??k veya e??ittir",
    "lt": "Daha k??????k",
    "lte": "Daha k??????k veya e??it",
    "neq": "E??it de??ildir"
  },
  "string": {
    "contains": "????eriyor",
    "doesnotcontain": "????ermiyor",
    "endswith": "??le biter",
    "eq": "E??ittir",
    "neq": "E??it de??ildir",
    "startswith": "??le ba??lar"
  }
});
}

/* ColumnMenu messages */

if (kendo.ui.ColumnMenu) {
kendo.ui.ColumnMenu.prototype.options.messages =
$.extend(true, kendo.ui.ColumnMenu.prototype.options.messages,{
  "columns": "S??tunlar",
  "settings": "S??tun ayarlar??",
  "done": "Tamam",
  "lock": "Kilitle",
  "sortAscending": "Artan S??ralama",
  "sortDescending": "Azalan S??ralama",
  "unlock": "Kilidini A??"
});
}

/* RecurrenceEditor messages */

if (kendo.ui.RecurrenceEditor) {
kendo.ui.RecurrenceEditor.prototype.options.messages =
$.extend(true, kendo.ui.RecurrenceEditor.prototype.options.messages,{
  "daily": {
    "interval": "G??nler",
    "repeatEvery": "Her g??n tekrarla"
  },
  "end": {
    "after": "Sonra",
    "label": "Biti??",
    "mobileLabel": "Biti??",
    "never": "Asla/Hi??",
    "occurrence": "Olay",
    "on": "Anl??k"
  },
  "frequencies": {
    "daily": "G??nl??k",
    "monthly": "Ayl??k",
    "never": "Asla/Hi??",
    "weekly": "Haftal??k",
    "yearly": "Y??ll??k"
  },
  "monthly": {
    "day": "G??n",
    "interval": "Aylar",
    "repeatEvery": "Her ay tekrarla",
    "repeatOn": "Tekrarla"
  },
  "offsetPositions": {
    "first": "??lk",
    "fourth": "D??rd??nc??",
    "last": "Son",
    "second": "??kinci",
    "third": "??????nc??"
  },
  "weekdays": {
    "day": "G??n",
    "weekday": "???? g??n??",
    "weekend": "Haftasonu"
  },
  "weekly": {
    "interval": "Haftalar",
    "repeatEvery": "Her hafta tekrarla",
    "repeatOn": "Tekrarla"
  },
  "yearly": {
    "interval": "Y??llar",
    "of": "Aras??nda",
    "repeatEvery": "Her Y??l Tekrarla",
    "repeatOn": "Tekrarla"
  }
});
}

/* Editor messages */

if (kendo.ui.Editor) {
kendo.ui.Editor.prototype.options.messages =
$.extend(true, kendo.ui.Editor.prototype.options.messages,{
  "addColumnLeft": "Sola kolon ekle",
  "addColumnRight": "Sa??a kolon ekle",
  "addRowAbove": "Yukar??daki sat??r ekle",
  "addRowBelow": "A??a????daki sat??r ekle",
  "backColor": "Arka plan rengi",
  "bold": "Kal??n ",
  "createLink": "K??pr?? ekleme",
  "createTable": "Tablo olu??tur",
  "deleteColumn": "S??tun silme",
  "deleteFile": "Silmek istedi??inizden emin misiniz ?",
  "deleteRow": "Sat??r sil",
  "dialogButtonSeparator": "ya da",
  "dialogCancel": "??ptal",
  "dialogInsert": "Ekle",
  "dialogUpdate": "G??ncelle",
  "directoryNotFound": "Bu isimde bir dizin bulunamad??.",
  "dropFilesHere": "Y??klemek i??in dosyalar?? buraya b??rak??n",
  "emptyFolder": "Bo?? klas??r",
  "fontName": "Font ailesi Se??iniz",
  "fontNameInherit": "Devral??nan Karakter",
  "fontSize": "Font boyutu Se??iniz",
  "fontSizeInherit": "Devral??nan Boyut",
  "foreColor": "Renk",
  "formatBlock": "Bi??im",
  "formatting": "Bi??imlendirme",
  "imageAltText": "Alternatif metin",
  "imageWebAddress": "Web adresi",
  "indent": "Aat??rba????",
  "insertHtml": "HTML ekle",
  "insertImage": "Resim ekle",
  "insertOrderedList": "S??ral?? liste ekleme",
  "insertUnorderedList": "S??ras??z liste ekleme",
  "invalidFileType": "Se??inizilen dosya \"{0}\" ge??erli de??il. Desteklenen dosya t??rleri {1} vard??r.",
  "italic": "??talik karakter",
  "justifyCenter": "Merkezi metin",
  "justifyFull": "Do??rulama",
  "justifyLeft": "Metni sola yasla",
  "justifyRight": "Metni sa??a yasla",
  "linkOpenInNewWindow": "Yeni pencerede a??",
  "linkText": "Metin",
  "linkToolTip": "Ara?? ??pucu",
  "linkWebAddress": "Web address",
  "orderBy": "D??zenleme ??l????t??:",
  "orderByName": "??sim",
  "orderBySize": "Boyut",
  "outdent": "????k??nt??",
  "overwriteFile": "??simde bir dosya \"{0}\" zaten dizinde mevcut. Bunu ??zerine yazmak istiyor musunuz?",
  "search": "Arama",
  "strikethrough": "??st?? ??izili",
  "styles": "Stiller",
  "subscript": "??ndis",
  "superscript": "??styaz??",
  "underline": "Alt??n?? ??izmek",
  "unlink": "K??pr??y?? Kald??r",
  "uploadFile": "Y??kle",
  "viewHtml": "HTML G??r??n??m?? ",
  "insertFile": "Insert file"
});
}

/* FilterCell messages */

if (kendo.ui.FilterCell) {
kendo.ui.FilterCell.prototype.options.messages =
$.extend(true, kendo.ui.FilterCell.prototype.options.messages,{
  "clear": "Temizle",
  "filter": "Filtre",
  "isFalse": "Yanl????",
  "isTrue": "Do??ru ",
  "operator": "Operat??r(i??letmen)"
});
}

/* FilterMenu messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.messages =
$.extend(true, kendo.ui.FilterMenu.prototype.options.messages,{
  "and": "Ve",
  "cancel": "??ptal",
  "clear": "Temizle",
  "filter": "Filtre",
  "info": "bu ile bu aras??ndaki de??erleri g??ster",
  "isFalse": "Yanl????",
  "isTrue": "Do??ru ",
  "operator": "Operat??r(i??letmen)",
  "or": "ya da",
  "selectValue": "De??er Se??iniz",
  "value": "De??er"
});
}

/* FilterMultiCheck messages */

if (kendo.ui.FilterMultiCheck) {
kendo.ui.FilterMultiCheck.prototype.options.messages =
$.extend(true, kendo.ui.FilterMultiCheck.prototype.options.messages,{
  "search": "Arama"
});
}

/* Grid messages */

if (kendo.ui.Grid) {
kendo.ui.Grid.prototype.options.messages =
$.extend(true, kendo.ui.Grid.prototype.options.messages,{
  "commands": {
    "canceledit": "??ptal",
    "cancel": "De??i??iklikleri iptal et",
    "create": "Yeni Kay??t Ekle",
    "destroy": "Sil",
    "edit": "D??zenle",
    "excel": "Export to Excel",
    "pdf": "Export to PDF",
    "save": "De??i??iklikleri Kaydet",
    "select": "Se??iniz",
    "update": "G??ncelle"
  },
  "editable": {
    "cancelDelete": "??ptal",
    "confirmation": "Kay??tlar?? silmek istedi??inizden emin misiniz ?",
    "confirmDelete": "Sil"
  }
});
}

/* Groupable messages */

if (kendo.ui.Groupable) {
kendo.ui.Groupable.prototype.options.messages =
$.extend(true, kendo.ui.Groupable.prototype.options.messages,{
  "empty": "Bir s??tun ba??l??????n?? s??r??kleyin ve bu s??tuna g??re grupland??rmak i??in buraya b??rak??n"
});
}

/* Pager messages */

if (kendo.ui.Pager) {
kendo.ui.Pager.prototype.options.messages =
$.extend(true, kendo.ui.Pager.prototype.options.messages,{
  "allPages": "All",
  "display": "{0} - {1} aral?????? g??steriliyor. Toplam {2} ????e var",
  "empty": "G??r??nt??lenecek ????e yok",
  "first": "??lk sayfaya git",
  "itemsPerPage": "Sayfa ba????na ??r??n",
  "last": "Son sayfaya git",
  "morePages": "Daha fazla sayfa",
  "next": "Bir sonraki sayfaya git",
  "of": "{0}",
  "page": "Sayfa",
  "previous": "Sayfalar?? ??ncele",
  "refresh": "G??ncelle"
});
}

/* Scheduler messages */

if (kendo.ui.Scheduler) {
kendo.ui.Scheduler.prototype.options.messages =
$.extend(true, kendo.ui.Scheduler.prototype.options.messages,{
  "allDay": "T??m g??n",
  "cancel": "??ptal Et",
  "editable": {
    "confirmation": "Bu etkinli??i silmek istedi??inizden emin misiniz?"
  },
  "date": "Tarih",
  "deleteWindowTitle": "Etkinli??i sil",
  "destroy": "Sil",
  "editor": {
    "allDayEvent": "T??m g??n s??ren olay",
    "description": "Tan??m",
    "editorTitle": "Olay",
    "end": "Biti??",
    "endTimezone": "Biti?? saati",
    "noTimezone": "Zaman Aral?????? belirtilmemi??",
    "repeat": "Tekrar",
    "separateTimezones": "Ayr?? bir ba??lang???? ve biti?? Zaman aral?????? kullan",
    "start": "Ba??lang????",
    "startTimezone": "Ba??lang???? Saati",
    "timezone": "",
    "timezoneEditorButton": "Zaman Aral??????",
    "timezoneEditorTitle": "Zaman Aral??????",
    "title": "Tan??m"
  },
  "event": "Olay",
  "recurrenceMessages": {
    "deleteRecurring": "Sadece bu olay?? ya da b??t??n dizini mi silmek istiyor musunuz?",
    "deleteWindowOccurrence": "Ge??erli yinelemeyi Sil",
    "deleteWindowSeries": "Seriyi Sil",
    "deleteWindowTitle": "Tekrarlanan ????eyi Sil",
    "editRecurring": "Sadece bu olay olu??umunu veya t??m dizini d??zenlemek istiyor musunuz?",
    "editWindowOccurrence": "Ge??erli Olay?? D??zenle",
    "editWindowSeries": "Seriyi d??zenle",
    "editWindowTitle": "Tekrarlanan ????eyi D??zenle"
  },
  "save": "Kaydet",
  "showFullDay": "T??m g??n g??ster",
  "showWorkDay": "???? saatlerini g??ster",
  "time": "Zaman",
  "today": "Bug??n",
  "views": {
    "agenda": "G??ndem",
    "day": "G??n",
    "month": "Ay",
    "week": "Hafta",
    "workWeek": "??al????ma Haftas??"
  }
});
}

/* Upload messages */

if (kendo.ui.Upload) {
kendo.ui.Upload.prototype.options.localization =
$.extend(true, kendo.ui.Upload.prototype.options.localization,{
  "cancel": "??ptal Et",
  "dropFilesHere": "Y??klemek i??in dosyalar?? buraya b??rak??n",
  "headerStatusUploaded": "Tamaland??",
  "headerStatusUploading": "Y??kleniyor",
  "remove": "Kald??r",
  "retry": "Tekrar Dene",
  "select": "Se??iniz",
  "statusFailed": "Ba??ar??z Oldu",
  "statusUploaded": "Y??klendi",
  "statusUploading": "Y??kleniyor",
  "uploadSelectedFiles": "se??ilen dosyalar?? Y??kle"
});
}
})(window.kendo.jQuery);
}));