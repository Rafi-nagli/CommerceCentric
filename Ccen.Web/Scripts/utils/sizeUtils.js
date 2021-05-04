var sizeUtils = (function () {
    function isNumXSmallSize(size) {
        if (size == "4"
            || size == "4/5")
            return true;
        return false;
    };

    function isNumSmallSize(size) {
        if (size == "6/6x"
            || size == "6/7"
            || size == "6")
            return true;
        return false;
    };

    function isNumMediumSize(size) {
        if (size == "7/8"
            || size == "8")
            return true;
        return false;
    };

    function isNumLargeSize(size) {
        if (size == "10"
            || size == "10/12")
            return true;
        return false;
    };

    function isNumXLargeSize (size) {
        if (size == "12/14"
            || size == "14"
            || size == "14/16"
            || size == "16")
            return true;
        return false;
    };

    return {
        isConvertableSize: function(size) {
            if (size == "4/5"
                || size == "6/6x"
                || size == "6/7"
                || size == "7/8"
                || size == "10/12"
                || size == "12/14"
                || size == "14/16")
                return true;
            return false;
        },

        convertSizeForSKU: function (size) {
            console.log("convert size: " + size);

            if (size == null)
                return "";

            if (isNumXSmallSize(size))
                return "XS";
            if (isNumSmallSize(size))
                return "S";
            if (isNumMediumSize(size))
                return "M";
            if (isNumLargeSize(size))
                return "L";
            if (isNumXLargeSize(size))
                return "XL";

            return size.replace(/-/g , "");
        },
    };
})();