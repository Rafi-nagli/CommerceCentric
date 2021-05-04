///* Register a new namespace. */
function registerNamespace(ns) {
    var nsParts = ns.split(".");
    var root = window;

    for (var i = 0; i < nsParts.length; i++) {
        if (typeof root[nsParts[i]] == "undefined")
            root[nsParts[i]] = new Object();

        root = root[nsParts[i]];
    }
}

/* Returns the version of Internet Explorer or a -1 (indicating the use of another browser). */
function getInternetExplorerVersion() {
    var rv = -1; // Return value assumes failure.
    if (navigator.appName == 'Microsoft Internet Explorer') {
        var ua = navigator.userAgent;
        var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
        if (re.exec(ua) != null)
            rv = parseFloat(RegExp.$1);
    }
    return rv;
}

if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        //return this.slice(0, str.length) == str;
        if (str == null)
            return false;
        return this.indexOf(str) == 0;
    };
}

Date.prototype.withoutTime = function() {
    var d = new Date(this);
    d.setHours(0, 0, 0, 0);
    return d;
};

Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
}

Array.prototype.distinct = function(callback) {
    var result = this.reduce(function(memo, e1) {
        var matches = memo.filter(function(e2) {
            return callback(e1, e2);
        });
        if (matches.length == 0)
            memo.push(e1);
        return memo;
    }, []);

    return result;
};

Array.prototype.firstOrDefault = function(callback) {
    var result = $.grep(this, callback);
    if (result.length > 0)
        return result[0];
    return null;
};

Array.prototype.removeIf = function (callback) {
    var i = 0;
    while (i < this.length) {
        if (callback(this[i], i)) {
            this.splice(i, 1);
        }
        else {
            ++i;
        }
    }
};

Array.prototype.addRange = function(arr) {
    for (i = 0; i < arr.length; i++) {
        this.push(arr[i]);
    }
};

Array.max = function (array) {
    return Math.max.apply(Math, array);
};

// Function to get the Min value in Array
Array.min = function (array) {
    return Math.min.apply(Math, array);
};


function ajaxQuery(options) {
    var baseOptions = {
        cache: false,
        async: true,
        error: function (jqXHR, error, errorThrown) {
            console.log("ajaxQuery.error=");
            console.log(jqXHR);
            console.log(error);
            console.log(errorThrown);
            console.log(this);
            //TODO: send ajax request to loggin it

            var message = "Something went wrong with the request:";
            message += "<br/>Url: " + this.url;
            message += "<br/>Response status: " + jqXHR.status + " (" + jqXHR.statusText + ")";
            try {
                var err = eval("(" + jqXHR.responseText + ")");
                message += "<br/>Response text: " + err.Message;
            } catch(e) {

            }
            
            helper.ui.showWarningPopup(message);
        }
    };
    options = $.extend(baseOptions, options);
    $.ajax(options);
}