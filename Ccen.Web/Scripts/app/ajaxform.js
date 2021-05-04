//enable unobtrusive validation for ajax loaded forms
$(document).ajaxSuccess(function (event, xhr, settings) {
    //process only if html was returned
    if ($.inArray('html', settings.dataTypes) >= 0) {
        //will parse the element with given id for unobtrusive validation
        function parseUnobtrusive(elementId) {
            if (elementId) {
                $.validator.unobtrusive.parse('#' + elementId);
            }
        }

        //get the form objects that were loaded.  Search within divs
        //in case the form is the root element in the string
        var forms = $('form', '<div>' + xhr.responseText + '</div>');

        //process each form retrieved by the ajax call
        $(forms).each(function () {
            //get the form id and trigger the parsing.
            //timout necessary for first time form loads to settle in
            var formId = this.id;
            setTimeout(function () { parseUnobtrusive(formId); }, 100);
        });
    }
});