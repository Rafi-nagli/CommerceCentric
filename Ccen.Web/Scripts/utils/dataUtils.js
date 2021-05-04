var dataUtils = (function() {
    return {
        isInternational: function(country) {
            return country != 'US';
        },
        concatArray: function(array1, array2) {
            if (array1 == null)
                return array2;
            if (array2 == null)
                return array1;
            for (var i = 0; i < array2.length; i++)
                array1.push(array2[i]);
            return array1;
        },
        combineString: function(str1, str2, separator) {
            var result = "";
            if (str1 != null && str1 != '')
                result = str1;
            if (str2 != null && str2 != '') {
                if (result != '')
                    result += separator + str2;
                else
                    result = str2;
            }
            return result;
        },

        removeUnsafe: function (str) {
            return str.replace(/#/g, " ");
        },

        parseDate: function (str) {
            if (str == null || str == '')
                return str;

            if (typeof str !== "string" || !str) {
                return str;
            }

            return new Date(parseInt(str.replace("/Date(", "").replace(")/", ""), 10));
        },
        getHoursAndDaysCount: function (date1, date2) {
            var timeDiff = date1.getTime() - date2.getTime();
            var diffDays = Math.floor(timeDiff / (1000 * 3600 * 24.0));
            var diffHours = Math.ceil(timeDiff / (1000 * 3600)) - diffDays * 24;
            var text = "";
            if (diffDays > 0)
                text = diffDays + " days and ";
            text += diffHours + " hours";

            return text;
        },
        getDayCount: function(date1, date2) {
            var timeDiff = date1.getTime() - date2.getTime();
            var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
            return diffDays;
        },
        getHourCount: function (date1, date2) {
            var timeDiff = date1.getTime() - date2.getTime();
            var diffHours = Math.ceil(timeDiff / (1000 * 3600));
            return diffHours;
        },
        isInt: function(str) {
            return /^\+?(0|[1-9]\d*)$/.test(str);
        },
        isEmpty: function(str) {
            if (str == null || str == '')
                return true;
            return false;
        },
        isNullOrEmpty: function (str) {
            if (str == null || str == '')
                return true;
            return false;
        },
        trim: function(str) {
            if (str == null)
                return null;
            return str.trim();
        },
        floorPrice: function (num) {
            if (num == null)
                return null;
            return +num.toFixed(2);
        },
        roundPrice: function (num) {
            if (num == null)
                return null;
            return Math.round(num * 100) / 100;
        },
        roundToTwoPrecision: function (num) {
            if (num == null)
                return null;
            return Math.round(num * 100) / 100;
        },
        substring: function(text, maxLen, postfix) {
            if (text == null)
                return text;
            if (text.length < maxLen)
                return text;

            return text.substring(0, maxLen) + postfix;
        }


        ////*****************************
        ////TIMEZONE CONVERTATION FOR TELERIK GRID
        ////*****************************

        //Date.prototype.getDST = function () {
        //    var year = this.getFullYear();
        //    var daylightSaving = $.grep(date.helper.daylightSavingByYears, function (e) { return e.year == year; });
        //    if (daylightSaving.length == 1) {
        //        var begin = daylightSaving[0].begin;
        //        var end = daylightSaving[0].end;
        //        console.log("begin: " + begin + ", end: " + end);
        //        //var mar = new Date(this.getFullYear(), 2, 10, 0, 0, 0); // w Date(this.getFullYear(), 0, 1);
        //        //var nov = new Date(this.getFullYear(), 10, 3, 0, 0, 0);
        //        return this.between(begin, end);
        //        //return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
        //    } else {
        //        console.log("empty daylight saving");
        //    }
        //    return false;
        //};

        ////http://www.timeanddate.com/time/zone/usa/new-york
        ////Reset 2 hour, date always have 0 hours
        //date.helper.daylightSavingByYears = [
        //    { year: 2014, begin: new Date(2014, 2, 9, 0, 0, 0), end: new Date(2014, 10, 2, 0, 0, 0) },
        //    { year: 2015, begin: new Date(2015, 2, 8, 0, 0, 0), end: new Date(2015, 10, 1, 0, 0, 0) },
        //    { year: 2016, begin: new Date(2016, 2, 13, 0, 0, 0), end: new Date(2016, 10, 6, 0, 0, 0) },
        //    { year: 2017, begin: new Date(2017, 2, 12, 0, 0, 0), end: new Date(2017, 10, 5, 0, 0, 0) },
        //    { year: 2018, begin: new Date(2018, 2, 11, 0, 0, 0), end: new Date(2018, 10, 4, 0, 0, 0) }
        //];

        //date.helper.toServerDate = function (date, timezone, format) {
        //    console.log(date + " - " + timezone);
        //    if (date == null)
        //        return "";
        //    var utcDate = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds(), date.getUTCMilliseconds());
        //    // offset measured in milliseconds
        //    //var isDaylightSaving = date.isDaylightSavingTime();
        //    //var offset = Date.getTimezoneOffset(timezone, isDaylightSaving);
        //    //var serverOffset = offset;// (offset == null ? -5 : offset) * 60 * 60 * 1000;
        //    var serverOffset = -5 * 60 * 60 * 1000;
        //    var isDst = false;
        //    try {
        //        isDst = date.getDST();
        //    }
        //    catch (ex) {
        //    }
        //    console.log(isDst);

        //    if (isDst)
        //        serverOffset = -4 * 60 * 60 * 1000;
        //    /* time zone offset measured in hours */
        //    var serverTime = new Date(utcDate.getTime() + serverOffset);
        //    if (format == null || format == '')
        //        return $.telerik.formatString("{0:G}", serverTime);
        //    else
        //        return $.telerik.formatString(format, serverTime);
        //};
    };
})();