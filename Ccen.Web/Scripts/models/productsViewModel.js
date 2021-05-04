 var ProductViewModel = function(model, settings) {
    var self = this;

    self.settings = settings;
    self.model = model;

    self.gridId = settings.gridId;

    self.selectedASIN = ko.observable(self.model.selectedParentASIN);

    self.canEdit = ko.observable(self.settings.canEdit);

    console.log(self.selectedASIN());

    var allAvailability = !dataUtils.isEmpty(self.selectedASIN())
        || !dataUtils.isEmpty(self.model.StyleId)
        || self.model.Market == self.settings.markets.walmart
        || self.model.Market == self.settings.markets.shopify;

    self.searchFilters = {
        styleIdList: null,
        chlldItemIdList: null,

        market: ko.observable(self.model.Market),
        marketplaceId: ko.observable(self.model.MarketplaceId),

        keywords: ko.observable(""),
        styleName: ko.observable(self.model.StyleId),
        asin: ko.observable(self.selectedASIN()),

        listingsMode: ko.observable(self.model.ListingsMode), //0 - All
        listingsModeList: model.listingsModeList,
        
        onHoldMode: ko.observable(self.settings.onHoldModes.none),
        onHoldModeList: model.onHoldModeList,

        availability: ko.observable(allAvailability ? self.settings.availabilities.all : self.settings.availabilities.inStock),
        availabilityList: model.availabilityList,

        noneSoldPeriod: ko.observable(""),
        noneSoldPeriodList: model.noneSoldPeriodList,

        gender: ko.observable(""),
        genderList: model.genderList,

        mainLicense: ko.observable(""),
        mainLicenseList: model.mainLicenseList,

        publishedStatus: ko.observable(model.PublishedStatus),
        publishedStatusList: model.publishedStatusList,

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


    self.exportASIN = ko.observable("");
    self.useStyleImage = ko.observable(true);

    self.generateForMarket = ko.observable(settings.marketplaceIds.amazonCom);
    self.exportToExcelUrl = ko.computed(function () {
        var baseUrl = self.settings.urls.exportToExcelUS;
        if (self.generateForMarket() == self.settings.marketplaceIds.amazonCom)
            baseUrl = self.settings.urls.exportToExcelUS;
        if (self.generateForMarket() == self.settings.marketplaceIds.amazonCa)
            baseUrl = self.settings.urls.exportToExcelCA;
        if (self.generateForMarket() == self.settings.marketplaceIds.amazonUk)
            baseUrl = self.settings.urls.exportToExcelUK;

        return baseUrl + "?asin=" + self.exportASIN()
            + "&market=" + self.searchFilters.market()
            + "&marketplaceId=" + self.searchFilters.marketplaceId()
            + "&useStyleImage=" + self.useStyleImage();
     });
     self.exportReturnExemptionsUrl = ko.computed(function () {
         return self.settings.urls.exportReturnExemptions;
     });

    self.title = ko.computed(function() {
        return self.model.MarketName;
    });

    self.getSearchParams = function(data) {
        return {
            listingsMode: self.searchFilters.listingsMode(),
            market: self.searchFilters.market(),
            marketplaceId: self.searchFilters.marketplaceId(),

            keywords: self.searchFilters.keywords(),
            styleName: self.searchFilters.styleName(),

            onHoldMode: self.searchFilters.onHoldMode(),
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

        self.searchFilters.publishedStatus("");
        self.searchFilters.onHoldMode(self.settings.onHoldModes.none);
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

    self.searchInProgress = ko.observable(false);
    self.search = function () {
        if (self.isValid()) {
            console.log(self.getSearchParams());

            self.searchInProgress(true);

            self.grid.read({ Page: 1 }).done(function () {
                self.searchInProgress(false);
            });
        }
    };

    self.getItemsDuration = ko.observable(null);
    self.getItemsReceiveDate = ko.observable(null).extend({ format: 'MM.dd.yyyy HH:mm' });

    self.getItemsAsync = function(gridParams) {
        console.log(gridParams);
        var defer = $.Deferred();
        var filterParams = self.getSearchParams();
        var exParams = $.extend(gridParams, filterParams);

        var startTimespan = new Date().getTime();
        $.ajax({
            cache: false,
            data: exParams,
            url: self.settings.urls.getParentItems,
            success: function (result) {
                self.getItemsDuration((new Date().getTime() - startTimespan)/1000);
                self.getItemsReceiveDate(result.GenerateDate);

                for (var i = 0; i < result.Items.length; i++) {
                    var item = result.Items[i];
                    self.prepareRow(item);
                }

                console.log("getAllAsync end: " + result.Items.length);

                self.onDataBound(result.Items);

                defer.resolve(result);
            }
        });
        return defer;
    };
    
    self.updateChildItemRows = function (parentAsin) {
        var loadingTagId = "ChildItems_Loading_" + parentAsin;
        $("#" + loadingTagId).show();
        $.ajax({
            cache: false,
            data: {
                parentAsin: parentAsin,
                market: self.searchFilters.market(),
                marketplaceId: self.searchFilters.marketplaceId(),
                listingsMode: self.searchFilters.listingsMode()
            },
            url: self.settings.urls.getChildItems,
            success: function (result) {
                console.log("end get all child, time=" + performance.now());
                var tagId = "ChildItems_" + parentAsin;
                $("#" + tagId).html('');

                for (var i = 0; i < result.Items.length; i++) {
                    var item = result.Items[i];
                    self.prepareChildRow(item);
                }

                var compile = tmpl.compileT('child-item-row-template');
                document.getElementById(tagId).innerHTML = kendo.render(compile, result.Items);

                $("#" + loadingTagId).hide();
            }
        });
    };


    self.prepareRow = function(rowData) {
        rowData.LastChildOpenDate = dataUtils.parseDate(rowData.LastChildOpenDate);
        rowData.SaleStartDate = dataUtils.parseDate(rowData.SaleStartDate);
        rowData.SaleEndDate = dataUtils.parseDate(rowData.SaleEndDate);
    };

    self.prepareChildRow = function (rowData) {
        rowData.OpenDate = dataUtils.parseDate(rowData.OpenDate);
        rowData.SaleStartDate = dataUtils.parseDate(rowData.SaleStartDate);
        rowData.SaleEndDate = dataUtils.parseDate(rowData.SaleEndDate);
    };

    self.grid = new FastGridViewModel({
        gridId: self.settings.gridId,
        rowTemplate: 'item-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        isLocalMode: false,
        itemsPerPage: 50,
        sortField: 'TotalRemaining',
        sortMode: 'desc',
        fields: [
            { name: "ASIN", type: 'string' },
            { name: "Rank", type: 'int' },
            { name: "LastChildOpenDate", type: 'date' },
            { name: "TotalRemaining", type: 'int' },
            { name: "FirstStyleString", type: 'string' },
        ],
    });

    self.getItemGridColumns = function() {
        var columnList = [
            { title: "#", width: "25px" },
            { title: "ASIN", width: "auto", field: "ASIN", sortable: true },
            { title: "Name", width: "auto" }, 
        ];

        if (self.searchFilters.market() == self.settings.markets.amazon
            || self.searchFilters.market() == self.settings.markets.amazonUk) {
            columnList.push({ title: "Rank", width: "auto", field: "Rank", sortable: true });
        }

        columnList.push({ title: "Price Range", width: "auto" });
        columnList.push({ title: "Open Date", width: "auto", field: "LastChildOpenDate", sortable: true });
        columnList.push({ title: "Remaining", width: "330px", field: "TotalRemaining", sortable: true });
        if (self.canEdit()) {
            columnList.push({ title: "", width: "115px" });
        }
        columnList.push({ title: "Comments", width: "100px" });

        return columnList;
    }

    self.fastGridSettings = {
        gridId: self.grid.gridId,
        hierarchy: { enable: true },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: self.getItemGridColumns(),
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    self.errors = ko.validation.group(self, { observable: true, live: true });
    self.isValid = ko.computed(function () { return self.errors().length === 0; });


    self.addProduct = function() {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.addProduct + "?market=" + self.searchFilters.market() + "&marketplaceId=" + self.searchFilters.marketplaceId(),
            title: "Add Product",
            width: 1010,
            submitSuccess: self.searchFullRefresh,
        });
     }

     self.actualizeGrouponQty = function () {
         console.log("actualizeGrouponQty");

         var model = {
             marketplaceId: self.model.MarketplaceId
         };

         var popupModel = new ActualizeGrouponQtyModel(model,
             null);
         popupModel.show();
     }

     self.importCatalogFeed = function () {
         console.log("importCatalogFeed");

         var popupModel = new ImportCatalogFeedModel(null,
             null);
         popupModel.show();
     }

    self.editProduct = function(sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editProduct + "?id=" + id,
            title: "Edit Product",
            width: 1010,
            submitSuccess: self.searchFullRefresh,
        });
     }

     self.copyProduct = function (sender, id) {
         popupWindow.initAndOpenWithSettings({
             content: self.settings.urls.copyProduct + "?id=" + id,
             title: "Copy Product",
             width: 500,
             //submitSuccess: self.searchFullRefresh,
         });
     }
     
     self.cloneProduct = function (sender, id) {
         if (confirm('Are you sure you want to close this Listing?')) {
             $.ajax({
                 url: self.settings.urls.cloneProduct + "?id=" + id,
                 success: function (result) {
                     if (result.IsSuccess) {
                         console.log("clone success");
                         self.searchFilters.keywords(result.Data.ASIN);
                         self.searchFullRefresh();
                     }
                 }
             });
         }
     }

     self.sendProductUpdate = function (sender, id) {
         if (confirm('Are you sure you want to send updates to the marketplace?')) {
             $.ajax({
                 url: self.settings.urls.sendProductUpdate + "?id=" + id,
                 success: function (result) {
                     if (result.IsSuccess) {
                         console.log("send updates success");
                     }
                 }
             });
         }
     }

     self.deleteProduct = function (sender, id) {
         if (confirm('Are you sure you want to delete this Listing?')) {
             $.ajax({
                 url: self.settings.urls.deleteProduct + "?id=" + id,
                 success: function (result) {
                     if (result.IsSuccess) {
                         console.log("delete success");
                         self.searchFullRefresh();
                     }
                 }
             });
         }
     }

     self.lookupBarcode = function() {
         console.log("lookupBarcode");
         popupWindow.initAndOpenWithSettings(
         {
             content: self.settings.urls.lookupBarcode,
             title: "Lookup Walmart Barcodes",
             width: 700,
         });
     }

    self.editParent = function(sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.updateParent + "?id=" + id,
            title: "Parent Edit",
            width: 450,
            submitSuccess: self.onParentPopupSubmitSuccess,
        });
    };

    self.exportProduct = function (sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.exportProduct + "?id=" + id,
            title: "Export Product",
            width: 950,
        });
    }
    
    self.editItem = function (sender, id, sku) {
        sku = sku || '';
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.updateItem + "?id=" + id + "&sku=" + sku,
            title: "Item/Listing Edit",
            width: 450
        });
    };

    self.onParentPopupSubmitSuccess = function (e) {
        console.log("onParentPopupSubmitSuccess");
        var data = e.Row;

        self.grid.updateRowField(e.Row, e.UpdateFields);

        var asin = data['ASIN'];
        console.log('asin=' + asin);
        self.refreshChildsFor(asin);
    }

    self.showSales = function (sender, asin) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.salesPopup + "?asin=" + asin + "&market=" + self.searchFilters.market() + "&marketplaceId=" + self.searchFilters.marketplaceId(),
            title: "Sales",
            width: 900,
            resize: function (e) {

            }
        });
    };

    self.requestUpdateStatus = ko.observable('');
    self.requestListingsUpdate = function () {
        $.ajax({
            url: self.settings.urls.requestUpdate,
            cache: false,
            data: {
                market: self.searchFilters.market(),
                marketplaceId: self.searchFilters.marketplaceId()
            },
            success: function (result) {
                self.requestUpdateStatus('Update requested');
            }
        });
    };

    self.syncPauseStatus = ko.observable(model.SyncPauseStatus);
    self.syncPauseText = ko.computed(function() {
        if (self.syncPauseStatus() == 0)
            return "Pause sync";
        if (self.syncPauseStatus() == 1)
            return "Unpause sync";
        return "Pause sync";
    });

    self.pauseListingsUpdate = function () {
        $.ajax({
            url: self.settings.urls.pauseUpdate,
            cache: false,
            data: {
                market: self.searchFilters.market(),
                marketplaceId: self.searchFilters.marketplaceId()
            },
            success: function (result) {
                self.syncPauseStatus(result.Data);
            }
        });
    };


    self.warningProductQtyList = ko.observableArray();
    self.warningProductQtySummary = ko.computed(function() {
        return "Listings with quantity warnings: " + self.warningProductQtyList().length;
    });

    self.warningProductDefectList = ko.observableArray();
    self.warningProductDefectSummary = ko.computed(function () {
        return "Listings with defect warnings: " + self.warningProductDefectList().length;
    });



    self.onDataBound = function (items) {
        // Retrieve all data from the DataSource
        var data = items;

        self.warningProductQtyList([]);

        $.each(data, function(i, row) {
            if (row.ChildItems != null) {
                $.each(row.ChildItems, function(j, listing) {
                    var alreadyInList = $.grep(self.warningProductQtyList(), function(item) { return item.ASIN == listing.ASIN; });
                    var qty = listing.RealQuantity;
                    if (listing.DisplayQuantity != null)
                        qty = Math.min(listing.RealQuantity, listing.DisplayQuantity);

                    if (alreadyInList.length == 0
                        && Math.abs(qty - listing.AmazonRealQuantity) > 0
                        && !listing.IsFBA) {
                        self.warningProductQtyList.push({
                            ASIN: listing.ASIN,
                            parentASIN: row.ASIN,
                            message: " diff with market"
                                + (listing.AmazonRealQuantity == 0 ? " (out of stock on Amazon!)" : "")
                                + ": " + (qty - listing.AmazonRealQuantity),
                            clickWarningProduct: function(data, event) {
                                self.scrollToASIN(data.parentASIN);
                            },
                        });
                    }
                });
            }
        });

        self.warningProductDefectList([]);

        $.each(data, function (i, row) {
            if (row.ChildItems != null) {
                $.each(row.ChildItems, function (j, listing) {
                    var alreadyInList = $.grep(self.warningProductDefectList(), function (item) { return item.ASIN == listing.ASIN; });
                    var defectList = listing.ListingDefects;

                    if (listing.RealQuantity > 0) {
                        if (alreadyInList.length == 0
                            && defectList != null
                            && defectList.length > 0) {
                            $.each(defectList, function(k, defect) {
                                self.warningProductDefectList.push({
                                    ASIN: listing.ASIN,
                                    parentASIN: row.ASIN,
                                    message: defect.AlertType + " - " + defect.FieldName + ", <span class='text-danger'>Status:</span> " + defect.Status + ", <span class='text-danger'>Details:</span> " + defect.Explanation,
                                    clickWarningProduct: function(data, event) {
                                        self.scrollToASIN(data.parentASIN);
                                    },
                                });
                            });
                        }
                    }
                });
            }
        });
    };

    //self.onGridReadCompleteScroll = function () {
    //    console.log("onGridReadCompleteScroll, asin=" + self.selectedASIN());
    //    if (self.selectedASIN() != '')
    //        self.scrollToASIN(self.selectedASIN());
        
    //    self.selectedASIN(''); //NOTE: called once
    //}

    self.scrollToASIN = function (asin) {
        var rows = $.grep(self.grid.items, function (n, i) {
            return n.ASIN == asin
                || $.grep(n.ChildItems, function (n) { return n.ASIN == asin; }).length > 0;
        });
        if (rows.length > 0) {
            console.log("selectedRow, id=" + rows[0].Id);
            self.grid.selectRow(rows[0].Id)
                .done(function (index) {
                    setTimeout(function () {
                        self.toggleGridRow(rows[0].ASIN);
                    }, 400);
                });
        }
    };

    self.refreshChildsFor = function(asin) {
        self.updateChildItemRows(asin);
    }

    self.toggleGridRow = function (asin) {
        console.log('toggleGridRow, id=' + asin);
        var tagId = '#Detail_' + asin;
        var visible = $(tagId).is(":visible");
        if (!visible) {
            console.log('begin get boxes');
            $(tagId).show();

            self.updateChildItemRows(asin);
        } else {
            $(tagId).hide();
        }
    };


    //Initializing
     self.grid.read();
};