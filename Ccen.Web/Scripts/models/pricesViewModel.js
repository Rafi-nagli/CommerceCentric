 var PriceViewModel = function(model, settings) {
    var self = this;

    self.settings = settings;
    self.model = model;

    self.gridId = settings.gridId;

    self.canEdit = ko.observable(self.settings.canEdit);

    self.selectedASIN = ko.observable(self.model.selectedParentASIN);
    console.log(self.selectedASIN());

    var allAvailability = !dataUtils.isEmpty(self.selectedASIN())
        || !dataUtils.isEmpty(self.model.StyleId);

    self.searchFilters = {
        styleIdList: null,
        chlldItemIdList: null,

        market: ko.observable(self.model.Market),
        marketplaceId: ko.observable(self.model.MarketplaceId),

        keywords: ko.observable(""),
        styleName: ko.observable(self.model.StyleId),
        asin: ko.observable(""),

        listingsMode: ko.observable(self.model.ListingsMode), //0 - All
        listingsModeList: model.listingsModeList,
        
        onHoldMode: ko.observable(self.settings.onHoldModes.none),
        onHoldModeList: model.onHoldModeList,

        buyBoxWinMode: ko.observable(self.settings.buyBoxWinModes.notWin),
        buyBoxWinModeList: model.buyBoxWinModeList,

        availability: ko.observable(allAvailability ? self.settings.availabilities.all : self.settings.availabilities.inStock),
        availabilityList: model.availabilityList,

        noneSoldPeriod: ko.observable(""),
        noneSoldPeriodList: model.noneSoldPeriodList,

        gender: ko.observable(""),
        genderList: model.genderList,

        mainLicense: ko.observable(""),
        mainLicenseList: model.mainLicenseList,

        subLicense: ko.observable(""),
        allSubLicenseList: model.allSubLicenseList,
        priceFrom: ko.observable("").extend({ require: false, number: { message: " " } }),
        priceTo: ko.observable("").extend({ require: false, number: { message: " " } }),

        allSizesArray: model.allSizesArray,
        selectedSizes: ko.observable(""),
    }

    console.log(self.searchFilters.listingsMode());
    console.log(self.searchFilters.availability());
   

    self.isAmazon = ko.computed(function() {
        return self.model.Market == settings.markets.amazon;
    });

    self.isFBA = ko.computed(function() {
        return self.model.ListingsMode == settings.listingsModeOnlyFBA;
    });

    self.subLicenseList = ko.computed(function() {
        var selectedLicense = self.searchFilters.mainLicense();
        return ko.utils.arrayFilter(self.searchFilters.allSubLicenseList, function (l) {
            return selectedLicense == l.ParentValue;
        });
    });
    self.enableSubLicense = ko.computed(function() {
        return self.searchFilters.mainLicense() != null && self.searchFilters.mainLicense() != '';
    });
    self.searchFilters.mainLicense.subscribe(function () { self.searchFilters.subLicense(""); });

    self.title = ko.computed(function () {
        var name = "Items Prices ";
        if (self.model.Market == settings.markets.amazon) {
            if (self.model.MarketplaceId == settings.marketplaceIds.amazonCom) {
                if (self.model.ListingsMode == settings.listingsModeOnlyFBA)
                    return name + "FBA .com";
                return name + ".com";
            }
            if (self.model.MarketplaceId == settings.marketplaceIds.amazonCa) {
                return name + ".ca";
            }
            if (self.model.MarketplaceId == settings.marketplaceIds.amazonMx) {
                return name + ".mx";
            }
        }
        if (self.model.MarketplaceId == settings.marketplaceIds.amazonUk) {
            return name + ".uk";
        }
        if (self.model.Market == settings.markets.eBay) {
            return name + "eBay";
        }
        if (self.model.Market == settings.markets.magento) {
            return name + "Magento";
        }
        return name + "-";
    });

    self.getSearchParams = function(data) {
        return {
            listingsMode: self.searchFilters.listingsMode(),
            market: self.searchFilters.market(),
            marketplaceId: self.searchFilters.marketplaceId(),

            keywords: self.searchFilters.keywords(),
            styleName: self.searchFilters.styleName(),

            onHoldMode: self.searchFilters.onHoldMode(),
            buyBoxWinMode: self.searchFilters.buyBoxWinMode(),
            availability: self.searchFilters.availability(),
            noneSoldPeriod: self.searchFilters.noneSoldPeriod(),
            gender: self.searchFilters.gender(),
            mainLicense: self.searchFilters.mainLicense(),
            subLicense: self.searchFilters.subLicense(),
            priceTo: self.searchFilters.priceTo(),
            priceFrom: self.searchFilters.priceFrom(),
            sizes: self.searchFilters.selectedSizes(),
        };
    }

    self.styleIdSource = new kendo.data.DataSource({
        type: "aspnetmvc-ajax",
        //minLength: 3,
        transport: {
            read: self.settings.urls.getStyleIdList,
            parameterMap: function (data, action) {
                console.log("action=" + action);
                if (action === "read") {
                    console.log("filter=" + data.filter.filters[0].value);
                    return {
                        filter: data.filter.filters[0].value
                    };
                } else {
                    return data;
                }
            }
        },
        pageSize: 20,
        serverPaging: true,
        serverFiltering: true
    });

    self.searchByKeyCmd = function(data, event) {
        if (event.keyCode == 13)
            self.search();
        return true;
    };

    self.clear = function () {
        self.searchFilters.styleIdList = null;
        self.searchFilters.childItemIdList = null;

        self.searchFilters.listingsMode(self.model.ListingsMode);
        self.searchFilters.market(self.model.Market);
        self.searchFilters.marketplaceId(self.model.MarketplaceId);

        self.searchFilters.keywords("");
        self.searchFilters.styleName("");
        self.searchFilters.asin("");

        self.searchFilters.onHoldMode(self.settings.onHoldModes.none);
        self.searchFilters.buyBoxWinMode(self.settings.buyBoxWinMode.win);
        self.searchFilters.availability((self.selectedASIN() != null && self.selectedASIN() != '') ? 1 : 2);
        self.searchFilters.noneSoldPeriod("");
        self.searchFilters.gender("");
        self.searchFilters.mainLicense("");
        self.searchFilters.subLicense("");
        self.searchFilters.priceTo("");
        self.searchFilters.priceFrom("");
        self.searchFilters.selectedSizes("");

        self.grid.refresh();
    };

    self.searchFullRefresh = function () {
        self.grid.read({ ClearCache: true }).done(function () {
            self.search();
        });
    }

    self.search = function () {
        if (self.isValid()) {
            console.log(self.getSearchParams());

            //Check server filters
            if (!dataUtils.isEmpty(self.searchFilters.mainLicense())
                || !dataUtils.isEmpty(self.searchFilters.subLicense())
                || !dataUtils.isEmpty(self.searchFilters.gender())
                || self.searchFilters.noneSoldPeriod() > 0) {
                console.log('GetIdListByFilters begin, time=' + performance.now());
                self.grid.showLoading();
                
                $.ajax({
                    url: self.settings.urls.getIdListByFilters,
                    data: self.getSearchParams(),
                    success: function (result) {
                        console.log('GetIdListByFilters end, time=' + performance.now());
                        self.searchFilters.styleIdList = result.Data.StyleIdList;
                        self.searchFilters.childItemIdList = result.Data.ChildItemIdList;

                        self.grid.hideLoading();
                        self.grid.refresh();
                    }
                });
            } else {
                self.searchFilters.styleIdList = null;
                self.searchFilters.childItemIdList = null;
                self.grid.refresh();
            }
        }
    };

    self.filterCallback = function (row) {
        var pass = true;

        if (self.searchFilters.styleIdList != null) {
            pass = pass && $.grep(self.searchFilters.styleIdList, function (id) { return id == n.StyleId; }).length > 0;
        }

        if (self.searchFilters.childItemIdList != null) {
            pass = pass && $.grep(self.searchFilters.childItemIdList, function(id) { return id == n.Id; }).length > 0;
        }

        if (self.searchFilters.onHoldMode() != self.settings.onHoldModes.none) {
            var mode = self.searchFilters.onHoldMode();
            if (mode == self.settings.onHoldModes.onListing || mode == self.settings.onHoldModes.onBoth) {
                pass = pass && row.OnHold;
            }
            if (mode == self.settings.onHoldModes.onStyle || mode == self.settings.onHoldModes.onBoth) {
                pass = pass && row.StyleOnHold;
            }
            if (mode == self.settings.onHoldModes.onListingOrStyle) {
                pass = pass && (row.StyleOnHold || row.OnHold);
            }
            if (mode == self.settings.onHoldModes.onListingWithActiveStyle) {
                pass = pass && (row.OnHold && !row.StyleOnHold);
            }
        }
        
        if (self.searchFilters.availability() == self.settings.availabilities.inStock) {
            pass = pass && row.RealQuantity > 0;
        }

        if (!dataUtils.isEmpty(self.searchFilters.keywords())) {
            var keys = self.searchFilters.keywords();
            var isMatch = row.ASIN == keys
                || row.AmazonName.indexOf(keys) >= 0
                || row.SKU == keys;

            pass = pass && isMatch;
        }
        
        if (!dataUtils.isEmpty(self.searchFilters.styleName())) {
            pass = pass && row.StyleString.indexOf(self.searchFilters.styleName()) >= 0;
        }
        
        if (self.searchFilters.listingsMode() != self.settings.listingsModes.all) {
            if (self.searchFilters.listingsMode() == self.settings.listingsModes.onlyFBA) {
                pass = pass && row.IsFBA;
            }
            if (self.searchFilters.listingsMode() == self.settings.listingsModes.withoutFBA) {
                pass = pass && !row.IsFBA;
            }
        }

        console.log(self.searchFilters.selectedSizes());

        if (self.searchFilters.selectedSizes() != null && self.searchFilters.selectedSizes().length > 0) {
            pass = pass && self.searchFilters.selectedSizes().indexOf(row.StyleSize) >= 0;
        }

        return pass;
    };

    self.getItemsDuration = ko.observable(null);
    self.getItemsReceiveDate = ko.observable(null).extend({ format: 'MM.dd.yyyy HH:mm' });

    self.getItemsAsync = function(gridParams) {
        console.log(gridParams);
        var defer = $.Deferred();
        var filterParams = self.getSearchParams();
        var params = $.extend(gridParams, filterParams);

        var startTimespan = new Date().getTime();
        $.ajax({
            cache: false,
            data: params,
            url: self.settings.urls.getItems,
            success: function (result) {
                self.getItemsDuration((new Date().getTime() - startTimespan)/1000);
                self.getItemsReceiveDate(result.GenerateDate);

                for (var i = 0; i < result.Items.length; i++) {
                    var item = result.Items[i];
                    self.prepareRow(item);
                }

                console.log("getAllAsync end: " + result.Items.length);

                defer.resolve(result);
            }
        });
        return defer;
    };
    
    self.prepareRow = function (rowData) {
        rowData.OpenDate = dataUtils.parseDate(rowData.OpenDate);
        rowData.SaleStartDate = dataUtils.parseDate(rowData.SaleStartDate);
        rowData.SaleEndDate = dataUtils.parseDate(rowData.SaleEndDate);
    };

    self.grid = new FastGridViewModel({
        gridId: self.settings.gridId,
        rowTemplate: 'item-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        itemsPerPage: 50,
        sortField: 'ParentASIN',
        sortMode: 'asc',
        fields: [
            { name: "ParentASIN", type: 'string' },
            { name: "ASIN", type: 'string' },
            { name: "OpenDate", type: 'date' },
            { name: "StyleString", type: 'string' },
        ],
    });

    self.getItemGridColumns = function() {
        var columnList = [
            { title: "Parent ASIN", width: "110px" },
            { title: "ASIN", width: "110px" },
            { title: "Picture", width: "90px" },
            { title: "Size/Color", width: "140px" },
            { title: "Quantity", width: "160px" },
            { title: "Price", width: "120px" },
            { title: "Buy Box Price", width: "auto" },
            { title: "Open Date", width: "120px" },
            { title: "Style ID/SKU", width: "200px" },
            { title: "", width: "100px" },
        ];

        return columnList;
    }

    self.fastGridSettings = {
        gridId: self.grid.gridId,
        hierarchy: { enable: false },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: self.getItemGridColumns(),
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    self.errors = ko.validation.group(self, { observable: true, live: true });
    self.isValid = ko.computed(function () { return self.errors().length === 0; });
    
   
    self.editItem = function (sender, id, sku) {
        sku = sku || '';
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.updateItem + "?id=" + id + "&sku=" + sku,
            title: "Item/Listing Edit",
            width: 450
        });
    };

     self.setWinnerPrice = function() {
         
     }

    //Initializing
    self.grid.read();
 };
