(function (ko, undefined) {
    ko.BaseViewModel = function () {
        
        //Row number
        this.rowNumber = ko.observable(0);
        this.rowTotal = ko.observable(0);
        this.renderNumber = function (data) {
            this.rowNumber(this.rowNumber() + 1);
            return this.rowNumber();
        };
        this.resetRowNumber = function (startNumber) {
            startNumber = startNumber || 0;
            this.rowNumber(startNumber);
        };
    };
}(ko));