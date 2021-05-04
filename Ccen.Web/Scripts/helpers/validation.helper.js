var validationUtils = (function() {
    return {
        callAsyncValidation: function(url, data) {
            var defer = $.Deferred();
            console.log("asyncValidation");

            $.ajax({
                url: url,
                data: JSON.stringify(data),
                type: 'POST',
                contentType: 'application/json',
                success: function (result) {
                    console.log(result);
                    var confirmationTask = $.Deferred().resolve();
                    for (var i = 0; i < result.Data.length; i++) {
                        var message = result.Data[i].Message;
                        console.log('show message=' + message);

                        confirmationTask = confirmationTask.then(function () {
                            return Message.popupAsync({
                                title: 'Confirm',
                                message: message,
                                type: Message.YES_NO
                            });
                        });
                    }

                    confirmationTask.done(function () {
                        console.log("resolve");
                        defer.resolve();
                    }).fail(function () {
                        console.log("reject");
                        defer.reject();
                    });
                }
            });

            return defer;
        }
    };
})();

ko.validation.rules['mustBeSmallestOrEqual'] = {
    validator: function (val, otherVal) {
        return val < otherVal;
    },
    message: 'The field must <='
};
ko.validation.rules['mustBeGreaterOrEqual'] = {
    validator: function (val, otherVal) {
        return val < otherVal;
    },
    message:  'The field must >='
};
ko.validation.registerExtenders();