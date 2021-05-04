var tmpl = (function () {
    var compileTemplates = {};

    return {
        t: function (templateName, data) {
            if (compileTemplates[templateName] == null) {
                var templateContent = $("#" + templateName).html();
                var template = kendo.template(templateContent, { useWithBlock: true });
                compileTemplates[templateName] = template;
            }
            var result = compileTemplates[templateName](data);// template(data); //render the template
            return result;
        },
        compileT: function (templateName) {
            var templateContent = $("#" + templateName).html();
            return kendo.template(templateContent, { useWithBlock: false });
        },
        isNull: function (val, defVal) {
            if (val == null || typeof val == 'undefined')
                return defVal;
            return val;
        },
        isNullOrEmpty: function (val, defVal) {
            if (val == null || typeof val == 'undefined')
                return defVal;
            return val;
        },
        isNullFormat: function (val, format, defVal) {
            if (val == null || isNaN(val) || typeof val == 'undefined')
                return defVal;
            return kendo.toString(val, format);
        },
        f: function (val, format) {
            if (val === null || typeof val == 'undefined')
                return '';
            return kendo.toString(val, format);
        },
        yesNo: function (val) {
            if (val == true)
                return "Yes";
            return "No";
        },
        split: function (text, separator) {
            if (text == null)
                return [];
            return text.split(separator);
        },
        index: function (arr, index) {
            if (arr == null)
                return "";
            if (arr.length <= index || index < 0)
                return "";
            return arr[index];
        },
        replace: function (text, val, newVal) {
            return text.replace(new RegExp(val, 'g'), newVal);
        },
        joinNotEmpty: function (list, separator) {
            var noEmpty = $.grep(list, function (i) { return i != "" && i != null; });
            return noEmpty.join(separator);
        }
    };

})();