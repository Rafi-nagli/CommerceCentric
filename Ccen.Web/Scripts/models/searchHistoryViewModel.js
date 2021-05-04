var SearchHistoryViewModel = function(settings) {
    var self = this;

    self.getSearchHistoryUrl = settings.getSearchHistoryUrl;
    self.orderNumber = ko.observable();

    self.lastSearchedOrderIdList = ko.observableArray(['loading...']);
    self.isLoaded = ko.observable(false);
    self.isLoading = ko.observable(false);

    self.requestSearchHistory = function () {
        console.log('requestSearchHistory');
        $.ajax({
            cache: false,
            url: self.getSearchHistoryUrl,
            success: function(data) {
                console.log('response search history, count = ' + data.length);
                self.lastSearchedOrderIdList(data);
                if (self.lastSearchedOrderIdList().length == 0) {
                    self.lastSearchedOrderIdList.push('empty');
                    self.isLoaded(false);
                } else {
                    self.isLoaded(true);
                }
                self.isLoading(false);
            }
        });
    };

    self.add = function(orderNumber) {
        if ((orderNumber != '' && orderNumber != null)
            && (self.lastSearchedOrderIdList().length == 0
                || self.lastSearchedOrderIdList()[0] != self.orderNumber())) {
            self.lastSearchedOrderIdList.unshift(self.orderNumber());
        }
    };

    self.init = function() {
        if (!self.isLoaded()) {
            if (!self.isLoading()) {
                self.isLoading(true);
                self.requestSearchHistory();
            }
        }
    };

    self.setSearchOrderId = function (orderNumber) {
        if (orderNumber != 'empty' && orderNumber != 'loading...')
            self.orderNumber(orderNumber);
    };
};