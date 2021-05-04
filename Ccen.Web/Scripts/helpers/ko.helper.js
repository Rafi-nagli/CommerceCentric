(function (ko) {
    ko.validation.patterns = {};

    ko.validation.patterns.requredNumber = function (message) {
        message = message || '*';
        return {
            required: {
                message: message
            },
            pattern: {
                message: message,
                params: '^([0-9][-,.0-9]*)$'
            }
        };
    };

    ko.validation.patterns.requredPossitivePrice = function (message) {
        message = message || '*';
        return {
            required: {
                message: message
            },
            pattern: {
                message: message,
                params: '^([0-9][-,.0-9]*)$'
            },
            min: 0
        };
    };

    ko.validation.patterns.number = function (message) {
        return {
            pattern: {
                message: message,
                params: '^([0-9][-,.0-9]*)$'
            }
        };
    };

    ko.validation.patterns.requred = function (message) {
        message = message || '*';
        return {
            required: {
                message: message
            },
        };
    };

    ko.observableArray.fn.addRange = function (arr) {
        for (var i = 0; i < arr.length; i++)
            this.push(arr[i]);
    };

    ko.observableArray.fn.removeIf = function (callback) {
        var i = 0;
        while (i < this().length) {
            if (callback(this()[i], i)) {
                this.splice(i, 1);
            }
            else {
                ++i;
            }
        }
    };

    ko.bindingHandlers.stopBinding = {
        init: function () {
            return { controlsDescendantBindings: true };
        }
    };

    ko.virtualElements.allowedBindings.stopBinding = true;

    //ko.extenders.format = function (target, precision) {
    //    var result = ko.dependentObservable({
    //        read: function () {
    //            return tmpl.isNullFormat(target(), precision, '-');
    //        },
    //        write: target
    //    });

    //    result.raw = target;
    //    return result;
    //};
}(ko));

ko.extenders.format = function (target, precision) {
    var result = ko.dependentObservable({
        read: function () {
            return tmpl.isNullFormat(target(), precision, '-');
        },
        write: target
    });

    result.raw = target;
    return result;
};


function getFormatedOrPlainResult(value, allBindingsAccessor) {
    var pattern = allBindingsAccessor.get('pattern');
    var ifEmpty = allBindingsAccessor.get('ifEmpty');

    if (value === null || value === '') {
        return ifEmpty;
    }

    if (pattern == null || !/\S*/.test(pattern)) {
        return value;
    }
    var valueToFormat = pattern === 'd' ? new Date(value) : value;
    //return Globalize.format(valueToFormat, pattern, allBindingsAccessor.get('culture'));

    return tmpl.f(valueToFormat, pattern);
};

ko.bindingHandlers.textFormatted = {
    init: ko.bindingHandlers.text.init,
    update: function (element, valueAccessor, allBindingsAccessor) {
        var result = getFormatedOrPlainResult(ko.unwrap(valueAccessor()), allBindingsAccessor);
        ko.bindingHandlers.text.update(element, function () { return result; });
    }
};

ko.bindingHandlers.valueFormatted = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var result = getFormatedOrPlainResult(ko.unwrap(valueAccessor()), allBindingsAccessor);
        ko.bindingHandlers.value.init(element, function () { return result; }, allBindingsAccessor);
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var result = getFormatedOrPlainResult(ko.unwrap(valueAccessor()), allBindingsAccessor);
        ko.bindingHandlers.value.update(element, function () { return result; }, allBindingsAccessor);
    }
};

// Here's a custom Knockout binding that makes elements shown/hidden via jQuery's fadeIn()/fadeOut() methods
// Could be stored in a separate utility library
ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).fadeIn() : $(element).fadeOut();
    }
};

ko.bindingHandlers["bsChecked"] = {
    init: function (element, valueAccessor) {
        ko.utils.registerEventHandler(element, "change", function (event) {
            var check = $(event.target);
            valueAccessor()($(check).prop('checked'));
        });
    },
    update: function (element, valueAccessor, allBindings) {
        // First get the latest data that we're bound to
        var value = valueAccessor();

        // Next, whether or not the supplied model property is observable, get its current value
        var valueUnwrapped = ko.unwrap(value);

        // Grab some more data from another binding property
        //var duration = allBindings.get('slideDuration') || 400; // 400ms is default duration unless otherwise specified

        // Now manipulate the DOM element
        $(element).prop('checked', valueUnwrapped).change();
    }
};