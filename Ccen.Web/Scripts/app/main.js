
//Initializing calls

//console.log(kendo.cultures["en-US"]);


console.log("init main");

$(document).ajaxError(function (event, request, settings, thrownError) {

    var message = "<div><b>An error occurred, please try to reload this page and repeat the operation.</b></div>"
        + "<div>" + thrownError + ": " + settings.url + "</div>"
        + "<div>Details: " + request.responseText;
    Message.showTop(message, Message.ERROR);
    console.log("ajaxError");
    console.log("url: " + settings.url);

    //Raven.captureMessage(thrownError, { logger: 'ajax', tags: { response: request.responseText, url: settings.url } });
});


//http://stackoverflow.com/questions/1119289/how-to-show-the-are-you-sure-you-want-to-navigate-away-from-this-page-when-ch
var confirmOnPageExit = function (e) {
    // If we haven't been passed the event get the window.event
    e = e || window.event;
    var message = 'You have unsaved changes.  Do you want to leave this page and lose your changes?';

    console.log("windows. isOpen=" + popupWindow.isOpen() + ", title=" + popupWindow.getTitle());
    //Check if edit enabled
    if (popupWindow == null
        || !popupWindow.isOpen()
        || (popupWindow.getTitle() != "Generate UK Style Excel"
        && popupWindow.getTitle() != "Generate US Style Excel")) {
        console.log('return nothing');
        return;
    }
    
    // For IE6-8 and Firefox prior to version 4
    if (e) {
        e.returnValue = message;
    }
    // For Chrome, Safari, IE8+ and Opera 12+
    return message;
};

console.log('onbeforeunload');
$(window).on('beforeunload', confirmOnPageExit);





//jQuery.ajax({// just showing error property
//    error: function (jqXHR, error, errorThrown) {
//        console.log("$error");
//        console.log(jqXHR);
//        if (jqXHR.status && jqXHR.status == 400) {
//            alert(jqXHR.responseText);
//        } else {
//            alert("Something went wrong");
//        }
//    }
//});