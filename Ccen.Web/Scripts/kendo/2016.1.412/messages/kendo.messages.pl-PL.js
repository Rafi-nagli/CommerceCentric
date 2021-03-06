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
    "eq": "s?? r??wne",
    "gte": "s?? p????niejsze lub r??wne",
    "gt": "s?? p????niejsze ni??",
    "lte": "s?? wcze??niejsze lub r??wne",
    "lt": "s?? wcze??niejsze ni??",
    "neq": "s?? inne ni??"
  },
  "number": {
    "eq": "s?? r??wne",
    "gte": "s?? wi??ksze lub r??wne",
    "gt": "s?? wi??ksze ni??",
    "lte": "s?? mniejsze lub r??wne",
    "lt": "s?? mniejsze ni??",
    "neq": "s?? inne ni??"
  },
  "string": {
    "endswith": "ko??cz?? si?? na",
    "eq": "s?? r??wne",
    "neq": "s?? inne ni??",
    "startswith": "zaczynaj?? si?? od",
    "contains": "zawieraj??",
    "doesnotcontain": "nie zawieraj??"
  },
  "enums": {
    "eq": "s?? r??wne",
    "neq": "s?? inne ni??"
  }
});
}

/* Filter menu operator messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.operators =
$.extend(true, kendo.ui.FilterMenu.prototype.options.operators,{
  "date": {
    "eq": "s?? r??wne",
    "gte": "s?? p????niejsze lub r??wne",
    "gt": "s?? p????niejsze ni??",
    "lte": "s?? wcze??niejsze lub r??wne",
    "lt": "s?? wcze??niejsze ni??",
    "neq": "s?? inne ni??"
  },
  "number": {
    "eq": "s?? r??wne",
    "gte": "s?? wi??ksze lub r??wne",
    "gt": "s?? wi??ksze ni??",
    "lte": "s?? mniejsze lub r??wne",
    "lt": "s?? mniejsze ni??",
    "neq": "s?? inne ni??"
  },
  "string": {
    "endswith": "ko??cz?? si?? na",
    "eq": "s?? r??wne",
    "neq": "s?? inne ni??",
    "startswith": "zaczynaj?? si?? od",
    "contains": "zawieraj??",
    "doesnotcontain": "nie zawieraj??"
  },
  "enums": {
    "eq": "s?? r??wne",
    "neq": "s?? inne ni??"
  }
});
}

/* ColumnMenu messages */

if (kendo.ui.ColumnMenu) {
kendo.ui.ColumnMenu.prototype.options.messages =
$.extend(true, kendo.ui.ColumnMenu.prototype.options.messages,{
  "columns": "Kolumny",
  "sortAscending": "Sortuj Rosn??co",
  "sortDescending": "Sortuj malej??co",
  "settings": "Ustawienia kolumn",
  "done": "Sporz??dzono",
  "lock": "Zablokowa??",
  "unlock": "Odblokowa??"
});
}

/* RecurrenceEditor messages */

if (kendo.ui.RecurrenceEditor) {
kendo.ui.RecurrenceEditor.prototype.options.messages =
$.extend(true, kendo.ui.RecurrenceEditor.prototype.options.messages,{
  "daily": {
    "interval": "days(s)",
    "repeatEvery": "Repeat every:"
  },
  "end": {
    "after": "After",
    "occurrence": "occurrence(s)",
    "label": "End:",
    "never": "Never",
    "on": "On",
    "mobileLabel": "Ends"
  },
  "frequencies": {
    "daily": "Daily",
    "monthly": "Monthly",
    "never": "Never",
    "weekly": "Weekly",
    "yearly": "Yearly"
  },
  "monthly": {
    "day": "Day",
    "interval": "month(s)",
    "repeatEvery": "Repeat every:",
    "repeatOn": "Repeat on:"
  },
  "offsetPositions": {
    "first": "first",
    "fourth": "fourth",
    "last": "last",
    "second": "second",
    "third": "third"
  },
  "weekly": {
    "repeatEvery": "Repeat every:",
    "repeatOn": "Repeat on:",
    "interval": "week(s)"
  },
  "yearly": {
    "of": "of",
    "repeatEvery": "Repeat every:",
    "repeatOn": "Repeat on:",
    "interval": "year(s)"
  },
  "weekdays": {
    "day": "day",
    "weekday": "weekday",
    "weekend": "weekend day"
  }
});
}

/* Grid messages */

if (kendo.ui.Grid) {
kendo.ui.Grid.prototype.options.messages =
$.extend(true, kendo.ui.Grid.prototype.options.messages,{
  "commands": {
    "create": "Wstaw",
    "destroy": "Usu??",
    "canceledit": "Anuluj",
    "update": "Aktualizuj",
    "edit": "Edycja",
    "excel": "Export to Excel",
    "pdf": "Export to PDF",
    "select": "Zaznacz",
    "cancel": "Anuluj zmiany",
    "save": "Zapisz zmiany"
  },
  "editable": {
    "confirmation": "Czy na pewno chcesz usun???? ten rekord?",
    "cancelDelete": "Anuluj",
    "confirmDelete": "Usu??"
  }
});
}

/* Pager messages */

if (kendo.ui.Pager) {
kendo.ui.Pager.prototype.options.messages =
$.extend(true, kendo.ui.Pager.prototype.options.messages,{
  "allPages": "All",
  "page": "Strona",
  "display": "Wy??wietlanie element??w {0} - {1} z {2}",
  "of": "z {0}",
  "empty": "Brak danych",
  "refresh": "Od??wie??",
  "first": "Id?? do pierwszej strony",
  "itemsPerPage": "na stron??",
  "last": "Przejd?? do ostatniej strony",
  "next": "Przejd?? do nast??pnej strony",
  "previous": "Przejd?? do poprzedniej strony",
  "morePages": "Wi??cej stron"
});
}

/* FilterCell messages */

if (kendo.ui.FilterCell) {
kendo.ui.FilterCell.prototype.options.messages =
$.extend(true, kendo.ui.FilterCell.prototype.options.messages,{
  "filter": "Filtr",
  "clear": "Wyczy???? filtr",
  "isFalse": "fa??sz",
  "isTrue": "prawda",
  "operator": "Operator"
});
}

/* FilterMenu messages */

if (kendo.ui.FilterMenu) {
kendo.ui.FilterMenu.prototype.options.messages =
$.extend(true, kendo.ui.FilterMenu.prototype.options.messages,{
  "filter": "Filtr",
  "and": "Oraz",
  "clear": "Wyczy???? filtr",
  "info": "Poka?? wiersze o warto??ciach kt??re",
  "selectValue": "-Wybierz warto????-",
  "isFalse": "fa??sz",
  "isTrue": "prawda",
  "or": "lub",
  "cancel": "Anuluj",
  "operator": "Operator",
  "value": "Warto????"
});
}

/* FilterMultiCheck messages */

if (kendo.ui.FilterMultiCheck) {
kendo.ui.FilterMultiCheck.prototype.options.messages =
$.extend(true, kendo.ui.FilterMultiCheck.prototype.options.messages,{
  "search": "Szukaj"
});
}

/* Groupable messages */

if (kendo.ui.Groupable) {
kendo.ui.Groupable.prototype.options.messages =
$.extend(true, kendo.ui.Groupable.prototype.options.messages,{
  "empty": "Przeci??gnij nag????wek kolumny i upu???? go tutaj aby pogrupowa?? wed??ug tej kolumny"
});
}

/* Editor messages */

if (kendo.ui.Editor) {
kendo.ui.Editor.prototype.options.messages =
$.extend(true, kendo.ui.Editor.prototype.options.messages,{
  "bold": "Wyt??uszczenie",
  "createLink": "Wstaw link",
  "fontName": "Wybierz czcionk??",
  "fontNameInherit": "(czcionka odziedziczona)",
  "fontSize": "Wybierz rozmiar czcionki",
  "fontSizeInherit": "(rozmiar odziedziczony)",
  "formatBlock": "Wybierz rozmiar bloku",
  "indent": "Wci??cie",
  "insertHtml": "Wstaw HTML",
  "insertImage": "Wstaw obraz",
  "insertOrderedList": "Wstaw list?? numerowan??",
  "insertUnorderedList": "Wstaw list?? wypunktowan??",
  "italic": "Kursywa",
  "justifyCenter": "Centruj tekst",
  "justifyFull": "Wyr??wnaj tekst",
  "justifyLeft": "Wyr??wnaj tekst do lewej",
  "justifyRight": "Wyr??wnaj tekst do prawej",
  "outdent": "Zmniejsz wci??cie",
  "strikethrough": "Przekre??lenie",
  "styles": "Styl",
  "subscript": "Subscript",
  "superscript": "Superscript",
  "underline": "Podkre??lenie",
  "unlink": "Usu?? link",
  "deleteFile": "Czy na pewno chcesz usun???? \"{0}\"?",
  "directoryNotFound": "Folder o tej nazwie nie zosta?? znaleziony.",
  "emptyFolder": "Opr????nij folder",
  "invalidFileType": "Wybrany plik \"{0}\" nie jest prawid??owy. Obs??ugiwane typy plik??w to: {1}.",
  "orderBy": "Uporz??dkuj wg:",
  "orderByName": "Nazwa",
  "orderBySize": "Rozmiar",
  "overwriteFile": "Plik o nazwie \"{0}\" istnieje ju?? w bie????cym folderze. Czy chcesz go zast??pi???",
  "uploadFile": "Za??aduj",
  "backColor": "Kolor t??a",
  "foreColor": "Kolor",
  "dialogButtonSeparator": "or",
  "dialogCancel": "Cancel",
  "dialogInsert": "Insert",
  "imageAltText": "Alternate text",
  "imageWebAddress": "Web address",
  "linkOpenInNewWindow": "Open link in new window",
  "linkText": "Text",
  "linkToolTip": "ToolTip",
  "linkWebAddress": "Web address",
  "search": "Search",
  "createTable": "Tworzenie tabeli",
  "addColumnLeft": "Add column on the left",
  "addColumnRight": "Add column on the right",
  "addRowAbove": "Add row above",
  "addRowBelow": "Add row below",
  "deleteColumn": "Delete column",
  "deleteRow": "Delete row",
  "dropFilesHere": "drop files here to upload",
  "formatting": "Format",
  "viewHtml": "View HTML",
  "dialogUpdate": "Update",
  "insertFile": "Insert file"
});
}

/* FileBrowser and ImageBrowser messages */

var browserMessages = {
  "uploadFile" : "Wy??lij",
  "orderBy" : "Sortuj wg",
  "orderByName" : "Nazwy",
  "orderBySize" : "Rozmiaru",
  "directoryNotFound" : "Folder o podanej nazwie nie zosta?? odnaleziony.",
  "emptyFolder" : "Pusty folder",
  "invalidFileType" : "Wybrany plik \"{0}\" jest nieprawid??owy. Obs??ugiwane pliki {1}.",
  "deleteFile" : 'Czy napewno chcesz usun???? plik "{0}"?',
  "overwriteFile" : 'Plik o nazwie "{0}" ju?? istnieje w bie????cym folderze. Czy zast??pi???',
  "dropFilesHere" : "umie???? pliki tutaj, aby je wys??a??",
  "search" : "Szukaj"
};

if (kendo.ui.FileBrowser) {
kendo.ui.FileBrowser.prototype.options.messages =
$.extend(true, kendo.ui.FileBrowser.prototype.options.messages, browserMessages);
}

if (kendo.ui.ImageBrowser) {
kendo.ui.ImageBrowser.prototype.options.messages =
$.extend(true, kendo.ui.ImageBrowser.prototype.options.messages, browserMessages);
}

/* Upload messages */

if (kendo.ui.Upload) {
kendo.ui.Upload.prototype.options.localization =
$.extend(true, kendo.ui.Upload.prototype.options.localization,{
  "cancel": "Anuluj",
  "dropFilesHere": "przeci??gnij tu pliki aby je za??adowa??",
  "remove": "Usu??",
  "retry": "Pon??w",
  "select": "Wybierz...",
  "statusFailed": "niepowodzenie",
  "statusUploaded": "za??adowane",
  "statusUploading": "trwa ??adowanie",
  "uploadSelectedFiles": "Za??aduj pliki",
  "headerStatusUploaded": "Done",
  "headerStatusUploading": "Uploading..."
});
}

/* Scheduler messages */

if (kendo.ui.Scheduler) {
kendo.ui.Scheduler.prototype.options.messages =
$.extend(true, kendo.ui.Scheduler.prototype.options.messages,{
  "allDay": "all day",
  "cancel": "Anuluj",
  "editable": {
    "confirmation": "Are you sure you want to delete this event?"
  },
  "date": "Date",
  "destroy": "Delete",
  "editor": {
    "allDayEvent": "All day event",
    "description": "Description",
    "editorTitle": "Event",
    "end": "End",
    "endTimezone": "End timezone",
    "repeat": "Repeat",
    "separateTimezones": "Use separate start and end time zones",
    "start": "Start",
    "startTimezone": "Start timezone",
    "timezone": " ",
    "timezoneEditorButton": "Time zone",
    "timezoneEditorTitle": "Timezones",
    "title": "Title",
    "noTimezone": "No timezone"
  },
  "event": "Event",
  "recurrenceMessages": {
    "deleteRecurring": "Do you want to delete only this event occurrence or the whole series?",
    "deleteWindowOccurrence": "Delete current occurrence",
    "deleteWindowSeries": "Delete the series",
    "deleteWindowTitle": "Delete Recurring Item",
    "editRecurring": "Do you want to edit only this event occurrence or the whole series?",
    "editWindowOccurrence": "Edit current occurrence",
    "editWindowSeries": "Edit the series",
    "editWindowTitle": "Edit Recurring Item"
  },
  "save": "Save",
  "time": "Time",
  "today": "Today",
  "views": {
    "agenda": "Agenda",
    "day": "Day",
    "month": "Month",
    "week": "Week",
    "workWeek": "Work Week"
  },
  "deleteWindowTitle": "Delete event",
  "showFullDay": "Show full day",
  "showWorkDay": "Show business hours"
});
}
})(window.kendo.jQuery);
}));