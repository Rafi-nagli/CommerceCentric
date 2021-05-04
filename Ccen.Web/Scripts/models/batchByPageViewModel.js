
var warningTypes = {
    duplicate: 1,
    onHold: 2,
    unprinted: 3,
    invalidAddress: 4,
    woPackage: 5,
    woStampsPrice: 6,
    overdueShipDate: 7,
    todayShipDate: 9,
    sameDay: 8,
    miamiArea: 10,
    primeOrSecondDay: 11,
    dhl: 12,
    withNewStyles: 13,
    noIssues: 14,
    dismissedIssues: 15,
    upgraded: 16,
    oversold: 17,
    dhlECom: 18,
    ibc: 19,
    fedexOneRate: 20,
    noIssuesIBCGroupon: 21,
    skyPostal: 22,
}

var validationTypes = {
    withoutWeight: 1,
    withoutService: 2,
    withoutStampsPrice: 3,
    withoutPhoneNumber: 4,
    withoutPersonName: 5,
    hasCancellation: 6,
    withAddressFormatError: 7,
    usInsured: 8,
    withAddressLineLengthIssue: 9,
    withOnHold: 10,
    hasMailLabel: 11,
    hasPrintedLabel: 12,
    hasIBC: 14,
    noPostalService: 15,
    hasSkyPostal: 16,
}

var searchStatus = {
    none: 0,
    local: 1,
    global: 2
};


var BatchByPageModel = function (gridId, model, settings) {
    var self = this;
    ko.BaseViewModel.call(self);

    self.settings = settings;
    self.model = model;
    self.gridId = ko.observable(gridId);

    self.isDemo = model.IsDemo;

    self.batchId = model.batchId;
    self.batchNumber = model.batchNumber;
    self.isClosed = model.IsClosed;
    self.isBatchLocked = ko.observable(model.IsLocked);

    self.batchCreateDate = model.batchCreateDate;

    self.gridName = gridId + '_' + self.batchId;

    //self.isFullReload = ko.observable(true);

    self.isSearchResult = ko.observable(false);
    self.lastSearchStatus = ko.observable(searchStatus.none);
    self.isPrintLabelsInProgress = ko.observable(model.IsPrintInProgress);
    self.printActionId = ko.observable(null);

    self.defaultMarket = settings.defaultMarket;
    self.defaultMarketplaceId = settings.defaultMarketplaceId;
    self.defaultDropShipperId = settings.defaultDropShipperId;

    var batches = settings.batchList;
    self.activeBatch = ko.observable('');
    self.activeBatchList = ko.observableArray(batches.filter(function (n) {
        return n.Value != self.batchId;
    }));

    if (self.batchId > 0) {
        self.activeBatchList.unshift({
            Text: 'Order Page',
            Value: null
        });
    } else {
        self.activeBatchList.unshift({
            Text: 'New',
            Value: null
        });
    }

    //Search
    self.marketValue = ko.observable(self.defaultMarket + "_" + self.defaultMarketplaceId);
    self.marketList = settings.marketList;
    self.market = ko.computed(function () {
        return self.marketValue().split('_')[0];
    });
    self.marketplaceId = ko.computed(function () {
        return self.marketValue().split('_')[1];
    });

    self.searchHistory = new SearchHistoryViewModel({
        getSearchHistoryUrl: settings.searchHistoryUrl,
    });
    self.searchHistory.orderNumber.subscribe(function () {
        self.orderNumber(self.searchHistory.orderNumber());
        self.search();
    });

    self.buyerName = ko.observable('');
    self.orderNumber = ko.observable(model.SearchOrderId);
    self.labelNumber = ko.observable('');

    self.bayList = settings.bayList;
    self.bayNumber = ko.observable('');
    self.maxOrders = ko.observable('');

    self.styleId = ko.observable('');
    self.styleItemId = ko.observable('');

    self.sizeList = ko.observable([]);

    self.minDate = new Date(2000, 1, 1);
    self.maxDate = new Date();
    self.maxDate.setHours(23);
    self.maxDate.setMinutes(59);

    self.dateFrom = ko.observable('');
    self.dateTo = ko.observable('');

    self.shippingStatus = ko.observable('');
    self.shippingStatus.subscribe(function () {
        self.clear(true);
        self.search();
    });
    self.shippingStatusList = settings.shippingStatusList;

    self.dropShipperId = ko.observable(self.defaultDropShipperId);
    self.dropShipperList = settings.dropShipperList;

    self.countByMarket = ko.observable();
    self.countByBay = ko.observableArray();

    self.allOrdersCount = ko.observable(0);
    self.allOrdersWeight = ko.observable(0);
    self.checkedOrdersWeight = ko.observable(0);
    self.allOrdersCost = ko.observable(0);

    self.isOrderInfoLoaded = ko.observable(false);

    self.warningList = ko.observableArray([]);
    self.allOrderInfoList = ko.observableArray([]);

    self.isBaySelected = function (bayNumber) {
        return self.bayNumber() == bayNumber;
    }
    self.setBay = function (bayNumber) {
        self.bayNumber(bayNumber);
    }

    console.log("LabelPrintPackUrl: " + model.batchPrintPackUrl);
    //self.batchPrintPackFile = ko.observable(model.LabelPrintPackUrl);
    //self.batchPrintPackUrl = ko.computed(function() {
    //return self.settings.viewPrintPackUrl + "?filename=" + self.batchPrintPackFile();
    //});
    self.batchPrintPackUrl = ko.observable(model.PrintPackUrl);

    self.allOrdersWeightLb = ko.computed(function () {
        return self.allOrdersWeight() / 16;
    });

    self.checkedOrdersWeightLb = ko.computed(function () {
        return self.checkedOrdersWeight() / 16;
    });

    self.batchHasPrintPack = ko.computed(function () {
        return !dataUtils.isEmpty(self.batchPrintPackUrl());
    });
    self.isAllLabelPurchased = ko.computed(function () {
        //return self.model.CanArchive;
        if (!self.batchHasPrintPack())
            return false;

        var withLabels = $.grep(self.allOrderInfoList(), function (o) {
            var activeLabels = $.grep(o.Labels, function (l) {
                return !dataUtils.isEmpty(l.TrackingNumber) && !l.IsCanceled;
            });
            if (activeLabels.length > 0)
                return true;
            return false;
        });
        if (withLabels.length == self.allOrderInfoList().length)
            return true;
        return false;
    });


    self.enabledPrintLabels = ko.computed(function () {
        return !self.isPrintLabelsInProgress() && self.isBatchLocked();
    });

    self.enabledPackingSlip = ko.computed(function () {
        if (self.isDemo)
            return true;

        return self.isBatchLocked() && (self.isPrintLabelsInProgress() || self.isAllLabelPurchased());
    });


    self.labelPurchasedFromDate = ko.computed(function () {
        var purchaseDateList = $.map(self.allOrderInfoList(), function (o) {
            var purchaseDateList = $.map(o.Labels, function (l) {
                return l.PurchaseDate;
            });
            if (purchaseDateList.length == 0)
                return null;
            return new Date(Array.min(purchaseDateList));
        });
        if (purchaseDateList.length == 0)
            return null;
        return new Date(Array.min(purchaseDateList));
    })
        .extend({ format: 'MM.dd.yyyy HH:mm' });

    self.labelPurchasedToDate = ko.computed(function () {
        var purchaseDateList = $.map(self.allOrderInfoList(), function (o) {
            var purchaseDateList = $.map(o.Labels, function (l) {
                return l.PurchaseDate;
            });
            if (purchaseDateList.length == 0)
                return null;
            return new Date(Array.max(purchaseDateList));
        });
        if (purchaseDateList.length == 0)
            return null;
        return new Date(Array.max(purchaseDateList));
    })
        .extend({ format: 'MM.dd.yyyy HH:mm' });

    self.totalOrderCount = ko.computed(function () {
        return self.grid != null ? self.grid.items.length : 0;
    });

    self.onViewComments = function (id) {
        console.log("onViewComments, id=" + id);
        popupWindow.initAndOpenWithSettings(
            {
                content: self.settings.viewCommentsUrl + "?orderId=" + id,
                title: "View Comments",
                width: 510,
            });
    };

    //BEGIN: edit
    self.onEditOrder = function (id) {
        console.log("onEditOrder, id=" + id);
        popupWindow.initAndOpenWithSettings(
            {
                content: self.settings.editOrderUrl + "?id=" + id,
                title: "Order Edit",
                width: 910,
                customAction: self.onPopupCustomAction,
                submitSuccess: self.onPopupSubmitSuccess,
            });
    };

    self.requestOrderInfo = function (orderId) {
        console.log("requestOrderRow, orderId=" + orderId);
        $.ajax({
            url: self.settings.getOrderByIdUrl,
            data: {
                orderId: orderId
            },
            success: function (result) {
                console.log("requestOrderRow.success");
                if (result.IsSuccess) {
                    self.grid.updateRow(result.Data);
                }
            }
        });
    }

    self.onPopupCustomAction = function (action, data) {
        console.log("onPopupCustomAction: " + action);
        if (action == 'setTrackingNumber') {
            self.requestOrderInfo(data.id);
        }

        if (action == 'toggleHold') {
            var infoRow = self.getRowInfoById(data.id);
            if (infoRow != null) {
                infoRow.OnHold = data.onHold;
                infoRow.OnHoldUpdateDate = data.onHoldUpdateDate;

                self.updateRowWarnings(infoRow);
                self.calcChecked(infoRow);
            }
            var displayRow = self.getRowById(data.id);
            if (displayRow != null) {
                displayRow.OnHold = data.onHold;
                displayRow.OnHoldUpdateData = data.onHoldUpdateDate;

                self.grid.updateRow(displayRow);
            }
        }

        if (action == 'toggleOversold') {
            var infoRow = self.getRowInfoById(data.id);
            if (infoRow != null) {
                infoRow.IsOversold = data.isOversold;
                infoRow.HasOversoldItems = data.isOversold;
                self.updateRowWarnings(infoRow);
                self.calcChecked(infoRow);
            }
            var displayRow = self.getRowById(data.id);
            if (displayRow != null) {
                displayRow.IsOversold = data.isOversold;
                displayRow.HasOversoldItems = data.isOversold;
                console.log("set displayRow");
                self.grid.updateRow(displayRow);
            }
        }

        if (action == 'toggleRefundLocked') {
            var infoRow = self.getRowInfoById(data.id);
            if (infoRow != null) {
                infoRow.IsRefundLocked = data.isRefundLocked;

                self.updateRowWarnings(infoRow);
                self.calcChecked(infoRow);
            }
            var displayRow = self.getRowById(data.id);
            if (displayRow != null) {
                displayRow.IsRefundLocked = data.isRefundLocked;

                self.grid.updateRow(displayRow);
            }
        }
        if (action == 'cancelOrder') {
            var displayRow = self.getRowById(data.id);
            if (displayRow != null) {
                displayRow.HasCancelationRequest = true;

                self.grid.updateRow(displayRow);
            }
        }
        if (action == "dismissAddressValidationError") {
            var infoRow = self.getRowInfoById(data.id);
            if (infoRow != null) {
                infoRow.IsDismissAddressValidation = true;

                self.updateRowWarnings(infoRow);
                self.calcChecked(infoRow);
            }
            var displayRow = self.getRowById(data.id);
            if (displayRow != null) {
                displayRow.IsDismissAddressValidation = true;

                self.grid.updateRow(displayRow);
            }
        }
    };

    self.onPopupSubmitSuccess = function (e) {

        //Pre processing
        self.prepareRow(e.Row);

        //Update
        //self.gridNode.refreshRow(e.Row, e.UpdateFields, e.ForseGridRefresh);
        self.grid.updateRowField(e.Row, e.UpdateFields);

        //Post processing
        var data = e.Row;
        console.log('onPopupSubmitSuccess');
        var entityId = data['EntityId'];
        var rowInfo = self.getRowInfoById(entityId);
        var displayRow = self.getRowById(entityId);

        self.updateRowWarnings(rowInfo);
        if (rowInfo != null)
            self.calcChecked();
    };
    //END: edit


    //#region BEGIN: warning helper
    self.getRowInfoById = function (entityId) {
        return self.allOrderInfoList().firstOrDefault(function (n) {
            return n.EntityId == entityId;
        });
    }

    self.getRowById = function (entityId) {
        return self.grid.getRowDataById(entityId);
    };

    self.updateRowWarnings = function (row) {
        if (row == null)
            return;

        row = self.getRowInfoById(row.EntityId);
        if (row == null)
            return;

        self.removeOrderWarnings(row.EntityId);
        var newRowWarnings = self.getRowWarnings(row);
        self.warningList.addRange(newRowWarnings);
    };

    self.removeOrderWarnings = function (entityId) {
        self.warningList.removeIf(function (item, i) {
            return item.entityId == entityId;
        });
    };
    //#endregion END: warning helper


    //BEGIN: processing checked
    self.checkedListCache = [];
    self.checkedCount = ko.observable(0);

    self.totalByProviderType = ko.observableArray([]);

    self.calcChecked = function () {
        var items = self.allOrderInfoList();
        var count = 0;

        var providerList = self.settings.providerTypeList;
        var totalByProvider = providerList.map(function (n, i) {
            var model = {
                type: n.Item1,
                name: n.Item2,
                totalPrice: ko.observable(0),
                checkedPrice: ko.observable(0),
            };

            model.formattedTotalPrice = ko.computed(function () {
                return model.totalPrice().toFixed(2);
            });
            model.formattedCheckedPrice = ko.computed(function () {
                return model.checkedPrice().toFixed(2);
            });

            return model;
        });

        var providerPrices = [];
        var totalWeight = 0;

        $.each(items, function (i, row) {
            if (providerPrices[row.ShipmentProviderType] == null) {
                providerPrices[row.ShipmentProviderType] = {
                    type: row.ShipmentProviderType,
                    totalPrice: 0,
                    checkedPrice: 0,
                };
            }

            if (self.checkedListCache[row.EntityId] == true) {
                if (row.IsInternational)
                    totalWeight += (row.Weight || 0);

                count++;
                providerPrices[row.ShipmentProviderType].checkedPrice += row.StampsShippingCost;
            }

            providerPrices[row.ShipmentProviderType].totalPrice += row.StampsShippingCost;
        });

        console.log(providerPrices);

        providerPrices.forEach(function (n, k) {
            var provider = totalByProvider.firstOrDefault(function (p) {
                return p.type == n.type;
            });
            if (provider != null) {
                provider.totalPrice(n.totalPrice);
                provider.checkedPrice(n.checkedPrice);
            };
        });

        self.checkedCount(count);
        self.checkedOrdersWeight(totalWeight);
        self.totalByProviderType(totalByProvider);
    };

    self.checkAllOnPage = function (sender) {
        var checked = $(sender).is(":checked");
        var items = self.grid.pageItems;//allOrderInfoList();
        //var checked = true;
        //self.checkedListCache = [];

        $.each(items, function (i, row) {
            var displayRow = self.getRowById(row.EntityId);
            if (displayRow != null)
                displayRow.UIChecked = checked;

            self.checkedListCache[row.EntityId] = checked;
        });

        $("#" + self.gridName + " tr input:checkbox").prop("checked", checked);

        self.calcChecked();
    };

    self.checkAllOnAllPages = function (sender) {
        var items = self.grid.filteredItems;//allOrderInfoList();
        var checked = true;
        self.checkedListCache = [];

        $.each(items, function (i, row) {
            var displayRow = self.getRowById(row.EntityId);
            if (displayRow != null)
                displayRow.UIChecked = checked;

            self.checkedListCache[row.EntityId] = checked;
        });

        $("#" + self.gridName + " tr input:checkbox").prop("checked", checked);

        self.calcChecked();
    };

    self.unCheckAllOnAllPages = function (sender) {
        var items = self.grid.filteredItems;// self.allOrderInfoList();
        var checked = false;
        self.checkedListCache = [];

        $.each(items, function (i, row) {
            var displayRow = self.getRowById(row.EntityId);
            if (displayRow != null)
                displayRow.UIChecked = checked;

            self.checkedListCache[row.EntityId] = checked;
        });

        //NOTE: m.b. uncheck header checkbox
        $("#" + self.gridName + " input:checkbox").prop("checked", checked);

        self.calcChecked();
    };

    self.checkOne = function (sender, entityId) {
        var checked = $(sender).is(":checked");
        console.log("checkOne: " + checked + ',id=' + entityId);
        var displayRow = self.getRowById(entityId);

        if (displayRow != null) {
            displayRow.UIChecked = checked;
        }
        self.checkedListCache[entityId] = checked;

        self.calcChecked();
    };

    self.getAllChecked = function () {
        var list = [];

        $.each(self.checkedListCache, function (i, row) {
            if (row == true)
                list.push(i);
        });

        return list;
    };
    //END: processing checked


    //BEGIN: request/processing data
    self.prepareRow = function (row) {
        if (row.hasOwnProperty('Id'))
            row.uid = row.Id;

        if (row.ShippingState == 'AA'
            || row.ShippingState == 'AP'
            || row.ShippingState == 'AE') {
            row.AddressValidationStatus = self.settings.addressValidationStatus.valid;
        }

        if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.dhl
            && row.HasAddressLengthIssue) {
            if (row.AddressValidationStatus < self.settings.addressValidationStatus.invalid) {
                row.AddressValidationStatus = self.settings.addressValidationStatus.dhlAddressLengthExceeded;
            }
        }
        if (row.IsInternational && !row.HasPhoneNumber) {
            if (row.AddressValidationStatus < self.settings.addressValidationStatus.invalid) {
                row.AddressValidationStatus = self.settings.addressValidationStatus.missingPhoneNumber;
            }
        }
    }


    self.showPopoverDetails = function (url, tagId, sender) {
        $.ajax({
            url: url,
            success: function (result) {
                var content = '';
                if (result.IsSuccess) {
                    $.each(result.Data.StyleItems, function (i, n) {
                        content += "<div>" + n.Size + ': ' + n.Quantity + "</div>";
                    });
                    content += "<div style='padding-top: 5px; margin-top: 5px; border-top: 1px solid grey'>";
                    if (result.Data.Locations.length > 0) {
                        $.each(result.Data.Locations, function (i, n) {
                            content += "<div>" + n.Isle + "<span style='margin: 0px 2px'>/</span>" + n.Section + "<span style='margin: 0px 2px'>/</span>" + n.Shelf + "</div>";
                        });
                    } else {
                        content += "-";
                    }
                    content += "</div>";
                } else {
                    content = 'Error getting info';
                }
                $('#' + tagId).html(content);
                sender.loadedContent = content;
            }
        });
        return '<div id="' + tagId + '">Loading...</div>';
    }

    self.onRedraw = function (rows) {
        console.log("onRedraw");

        $.each(rows, function (i, row) {
            self.afterUpdateRow(row);

            var index = self.grid.getFilteredRowIndexById(row.Id);
            var rowLabel = $(this).find(".row-number");
            $(rowLabel).html(index);

            $('[row-uid=' + row.Id + ']').find('*[data-poload]').popover({
                html: true,
                trigger: 'hover',
                placement: 'bottom',
                content: function () {
                    if (typeof (this.loadedContent) == 'undefined') {
                        var popoverId = "ppv-id-" + $.now();
                        return self.showPopoverDetails($(this).data('poload'), popoverId, this);
                    }
                    return this.loadedContent;
                }
            });
        });
    }

    self.afterUpdateRow = function (row) {
        var rowNode = $('tr[row-uid="' + row.uid + '"]');
        if (rowNode.length > 0) {
            if (self.checkedListCache[row.EntityId] == true) { //.indexOf(row.EntityId) >= 0) {
                $cb = rowNode.find(".check_row");
                if (!$cb.is(":disabled")) {
                    $cb.prop("checked", "checked");
                    row.UIChecked = true;
                }
            }

            var excludeButtons = ['editOrderButton', 'removeButton', 'historyOrderButton', 'emailButton', 'recalculateButton'];
            if (!row.IsDisabled)
                excludeButtons.push('holdButton');
            helper.ui.disableRow(rowNode, row.OnHold, excludeButtons);

            ////NOTE: global func, TODO:
            paintOrderRow(row, 5, 12, 4, 14);// self.grid.columns.length);

            ////NOTE: global func, TODO:
            self.drawOrderRowShippingOptions(row);
        }

        row.isSearchResult = self.isSearchResult();
    };

    self.drawOrderRowShippingOptions = function (row) {
        var options = row.ShippingOptions;
        if (options != null) {
            var val = row.Selected;

            if (options.length > 1) {
                var dataSource = [{ "Text": "Select...", "Value": "0" }];
                for (var i = 0; i < options.length; i++)
                    dataSource.push({ "Text": options[i].Text, "Value": options[i].Value });

                $('tr[row-uid="' + row.uid + '"]').find('.shipping-options').width(160).kendoDropDownList(
                    {
                        "change": function () {
                            var ind = this.selectedIndex;
                            self.updateOverweightRow(this, row.uid, row.EntityId, row.OrderId, this.dataItem(ind).Value);

                        },
                        value: val,
                        "dataSource": dataSource,
                        enabled: !row.HasBatchLabel,
                        "dataTextField": "Text",
                        "dataValueField": "Value"
                    });

                //var options = $('tr[row-uid="' + row.uid + '"]').find('.shipping-options option');
                //console.log(options.length);
                //console.log(options);
                //options.each(function(o) {
                //    var val = $(this).val();;
                //    $(this).attr("title", val);
                //});
            }
        }
    };

    self.infoCalculation = function (e) {
        console.log("BatchModel dataBound");

        var items = self.allOrderInfoList();
        var totalCost = 0;
        var totalWeight = 0;
        //Calculate counts

        var markets = self.settings.markets;
        $.each(markets, function (i, m) {
            m.count = 0;
        });
        $.each(items, function (i, row) {
            $.each(markets, function (i, m) {
                if (row.Market == m.Market) {
                    if (!dataUtils.isEmpty(m.MarketplaceId)) {
                        if (row.MarketplaceId == m.MarketplaceId)
                            m.count++;
                    } else {
                        m.count++;
                    }
                }
            });
            totalCost += row.ItemPriceInUSD + row.ShippingPriceInUSD;
            if (row.IsInternational)
                totalWeight += (row.Weight || 0);
        });
        $.each(markets, function (i, entry) {
            entry.setMarket = function () {
                self.clear();
                self.marketValue(entry.Market + '_' + entry.MarketplaceId);
                self.search();
            };
            entry.flagCss = "flag-" + entry.ShortName;
            entry.isSelected = ko.computed(function () {
                return entry.Market == self.market() && entry.MarketplaceId == self.marketplaceId();
            });
        });
        self.countByMarket(markets);

        var bayList = $.grep(self.settings.bayList, function (b) {
            return b.Value != null && b.Value != 0;
        });
        $.each(bayList, function (i, m) {
            m.count = 0;
        });
        var bayCounts = [];
        $.each(items, function (i, row) {
            bayCounts[row.BayNumber.toString()] = (bayCounts[row.BayNumber.toString()] || 0) + 1;
        });
        for (var i = 0; i < bayList.length; i++) {
            bayList[i].count = bayCounts[bayList[i].Value] || 0;
        }
        self.countByBay.removeAll();
        for (var i = 0; i < bayList.length; i++)
            self.countByBay.push(bayList[i]);

        self.allOrdersCount(items.length);
        self.allOrdersCost(totalCost);
        self.allOrdersWeight(totalWeight);
    };

    //NOTE: can use entity data instead of row
    self.getRowWarnings = function (row) {
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        var endToday = new Date();
        endToday.setHours(23, 59, 59, 0);
        var week2Ago = today.addDays(-14);

        var warnings = [];

        $.each(row.Notifies, function (i, n) {
            if (n.Type == self.settings.duplicateNotifyType) {
                var existWarning = warnings.firstOrDefault(function (w) {
                    return w.type == n.Type;
                });
                if (existWarning == null) {
                    var newWarning = {
                        type: warningTypes.duplicate,
                        entityId: row.EntityId,
                        orderId: row.OrderId,
                        message: row.Message,
                    };

                    warnings.push(newWarning);
                }
            }
        });

        if (row.HasOversoldItems) {
            var newWarning = {
                type: warningTypes.oversold,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: row.Message,
            };

            warnings.push(newWarning);
        }

        if (row.OnHold) {
            warnings.push({
                type: warningTypes.onHold,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.Selected == 0) { // || !row.AllItemIsWeight)
            warnings.push({
                type: warningTypes.woPackage,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.StampsShippingCost == '' || row.StampsShippingCost == 0) {
            warnings.push({
                type: warningTypes.woStampsPrice,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.AlignedExpectedShipDate != null
            && row.AlignedExpectedShipDate < today
            && !row.HasLabel
            && row.OrderStatus == self.settings.unshippedOrderStatus) {
            warnings.push({
                type: warningTypes.overdueShipDate,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.AlignedExpectedShipDate == null
            || row.AlignedExpectedShipDate <= endToday
            //&& !row.HasLabel
            //&& row.OrderStatus == self.settings.unshippedOrderStatus
        ) {
            warnings.push({
                type: warningTypes.todayShipDate,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShippingMethodId == self.settings.shippingMethodIds.sameDay) {
            warnings.push({
                type: warningTypes.sameDay,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShippingMethodId == self.settings.shippingMethodIds.dhl
            || row.ShippingMethodId == self.settings.shippingMethodIds.dhlMx) {
            warnings.push({
                type: warningTypes.dhl,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.dhlECom) {
            warnings.push({
                type: warningTypes.dhlECom,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.ibc) {
            warnings.push({
                type: warningTypes.ibc,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.skyPostal) {
            warnings.push({
                type: warningTypes.skyPostal,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.fedexOneRate
            || row.ShipmentCarrier == 'FEDEX') {
            warnings.push({
                type: warningTypes.fedexOneRate,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.InitialServiceType == self.settings.shippingNames.secondDay
            || row.InitialServiceType == self.settings.shippingNames.nextDay
            || row.IsPrime) {
            console.log(row.OrderId + ", " + row.InitialServiceType + ", " + row.IsPrime);
            warnings.push({
                type: warningTypes.primeOrSecondDay,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.ShippingState == 'FL' && (row.ShippingCity || "").toLowerCase().indexOf("miami") >= 0) {
            warnings.push({
                type: warningTypes.miamiArea,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.UpgradeLevel > 0) {
            warnings.push({
                type: warningTypes.upgraded,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        var hasNewStyle = $.grep(row.Items, function (n) { return n.ListingCreateDate != null && n.ListingCreateDate >= week2Ago; }).length > 0;
        if (hasNewStyle) {
            warnings.push({
                type: warningTypes.withNewStyles,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if (row.HasAllLabels == false) {
            var msg = "";
            if (row.Labels != null) {
                for (var i = 0; i < row.Labels.length; i++) {
                    msg = "";
                    var label = row.Labels[i];
                    if (label.PurchaseMessage != null && label.PurchaseMessage != '')
                        msg = label.PurchaseMessage;

                    if (msg != "") {
                        warnings.push({
                            type: warningTypes.unprinted,
                            entityId: row.EntityId,
                            orderId: row.OrderId,
                            message: msg
                        });
                    }
                }
            };
        }

        if (row.AddressValidationStatus >= self.settings.addressValidationStatus.invalid
            || row.HasAddressLengthIssue
            || row.HasAddressInvalidSymbols) {
            if (!row.IsDismissAddressValidation) {
                var stampsNotify = row.Notifies.firstOrDefault(function (n) {
                    return n.Type == self.settings.addressCheckStampsNotifyType;
                });
                var googleNotify = row.Notifies.firstOrDefault(function (n) {
                    return n.Type == self.settings.addressCheckGoogleNotifyType;
                });
                var addressStampsValidationMessage = stampsNotify != null ? stampsNotify.Message : "";
                var addressGoogleValidationMessage = googleNotify != null ? googleNotify.Message : "";

                if (row.HasAddressLengthIssue)
                    addressStampsValidationMessage = 'Has issue with address length';

                if (row.HasAddressInvalidSymbols)
                    addressStampsValidationMessage = 'Has issue with invalid symbols';

                if (row.AddressValidationStatus == self.settings.addressValidationStatus.invalidRecipientName)
                    addressStampsValidationMessage = 'Recipient name is incomplete. Buyer Name: ' + row.BuyerName;

                if (row.AddressValidationStatus == self.settings.addressValidationStatus.missingPhoneNumber)
                    addressStampsValidationMessage = 'Phone number missing for international order';

                if (row.AddressValidationStatus == self.settings.addressValidationStatus.dhlAddressLengthExceeded)
                    addressStampsValidationMessage = 'Dhl address line length exceeded';

                var stampsError = row.AddressValidationStatus == self.settings.addressValidationStatus.exceptionCommunication;

                var message = ""
                    + (stampsError ? " (address API unavailable)" : "")
                    + (row.OnHold ? " - On Hold" : "")
                    + (row.HasComment ? " - Has comment" : "")
                    + ((addressGoogleValidationMessage || "").indexOf("Rooftop") >= 0 ? " - Geocode: True" : " - Geocode: False")
                    + (!dataUtils.isEmpty(addressStampsValidationMessage) ? " <br/>&nbsp;&nbsp;&nbsp;- Stamps details: " + addressStampsValidationMessage : "");

                warnings.push({
                    type: warningTypes.invalidAddress,
                    entityId: row.EntityId,
                    orderId: row.OrderId,
                    message: message,
                    onHold: row.OnHold,
                    isBold: row.OnHold,
                    isWarning: row.AlignedExpectedShipDate != null && row.AlignedExpectedShipDate.withoutTime() <= today
                });
            } else {
                warnings.push({
                    type: warningTypes.dismissedIssues,
                    entityId: row.EntityId,
                    orderId: row.OrderId,
                    message: ''
                });
            }
        }

        if ($.grep(warnings, function (w) {
            return w.entityId == row.EntityId
                && (w.type == warningTypes.invalidAddress
                    || w.type == warningTypes.onHold
                    || w.type == warningTypes.duplicate
                    || w.type == warningTypes.oversold
                    || w.type == warningTypes.woPackage
                    || w.type == warningTypes.woStampsPrice);
        }).length == 0) {
            warnings.push({
                type: warningTypes.noIssues,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        if ($.grep(warnings, function (w) {
            return w.entityId == row.EntityId
                && (w.type == warningTypes.invalidAddress
                    || w.type == warningTypes.onHold
                    || w.type == warningTypes.duplicate
                    || w.type == warningTypes.oversold
                    || w.type == warningTypes.woPackage
                    || w.type == warningTypes.woStampsPrice
                    || w.type == warningTypes.ibc
                    || w.type == warningTypes.skyPostal);
        }).length == 0
            || row.Market == self.settings.marketNames.groupon) {
            warnings.push({
                type: warningTypes.noIssuesIBCGroupon,
                entityId: row.EntityId,
                orderId: row.OrderId,
                message: ''
            });
        }

        return warnings;
    };
    //END: request/processing data


    //BEGIN: page actions
    self.updateOverweightRow = function (sender, uid, id, orderId, selected) {
        helper.ui.showLoading(sender.element.parent());

        $.ajax({
            url: self.settings.setShippingOptionsUrl,
            data: { id: id, orderId: orderId, groupId: selected },
            //async: false,
            cache: false,
            success: function (data) {
                var displayRow = self.getRowById(id);
                var infoRow = self.getRowInfoById(id);

                if (displayRow != null) {
                    displayRow.StampsShippingCost = data.StampsShippingCost;
                    displayRow.Selected = data.Selected;
                    displayRow.ShippingGroupId = data.ShippingGroupId;
                    displayRow.ShippingMethodId = data.ShippingMethodId;
                    displayRow.ShippingMethodName = data.ShippingMethodName;
                    displayRow.FormattedShippingMethodName = data.FormattedShippingMethodName;
                    displayRow.ShipmentProviderName = data.ShipmentProviderName;
                    displayRow.ShipmentCarrier = data.ShipmentCarrier;

                    self.grid.updateRow(displayRow);
                    //helper.ui.kendoFastRedrawRow(self.grid, displayRow, $('tr[data-uid="' + displayRow.uid + '"]'), [2, 10], 1);
                }

                if (infoRow != null) {
                    infoRow.StampsShippingCost = data.StampsShippingCost;
                    infoRow.Selected = data.Selected;
                    infoRow.ShippingGroupId = data.ShippingGroupId;
                    infoRow.ShippingMethodId = data.ShippingMethodId;
                    infoRow.ShippingMethodName = data.ShippingMethodName;
                    infoRow.FormattedShippingMethodName = data.FormattedShippingMethodName;
                    infoRow.ShipmentProviderName = data.ShipmentProviderName;
                    infoRow.ShipmentCarrier = data.ShipmentCarrier;

                    self.updateRowWarnings(infoRow);
                    self.calcChecked();
                }

                helper.ui.hideLoading(sender.element.parent());
            }
        });
    };

    self.onDismissAddressValidationError = function (id) {
        var displayRow = self.getRowById(id);
        var infoRow = self.getRowById(id);

        if (displayRow.LastCommentMessage == null || displayRow.LastCommentMessage == '') {
            Message.popup('Please add comment before Dismiss',
                Message.POPUP_INFO);
            return;
        }

        $.ajax({
            url: self.settings.setDismissAddressValidationUrl,
            data: {
                id: id,
                dismiss: true
            },
            cache: false,
            success: function (data) {
                if (data.IsSuccess) {
                    console.log("success dismiss, id=" + data.Data);
                    displayRow.IsDismissAddressValidation = true;

                    infoRow.IsDismissAddressValidation = true;
                    self.updateRowWarnings(infoRow);
                }
            }
        });
    };

    self.setDefaultMarket = function () {
        self.clear();
        self.marketValue(self.defaultMarket + '_' + self.defaultMarketplaceId);
        self.search();
    };

    self.getDayBatchNumber = function () {
        var defer = $.Deferred();

        $.ajax({
            url: self.settings.getDayBatchNumberUrl,
            cache: false,
            success: function (result) {
                defer.resolve(result);
            }
        });
        return defer;
    }

    self.createBatch = function (defaultBatchName, orderIds, addToBatchButton) {
        var batchName = prompt("Batch name: ", defaultBatchName);
        console.log('batchName=' + batchName);
        if (batchName) {
            helper.ui.showLoading(addToBatchButton, 'creating...');
            $.ajax({
                url: self.settings.createBatchUrl,
                cache: false,
                type: "POST",
                data: {
                    batchName: batchName,
                    orderIds: orderIds.join(',')
                },
                success: function (result) {
                    if (result.IsSuccess) {
                        Message.success("Batch was created");

                        var newBatch = {
                            Value: result.Data,
                            Text: batchName
                        };
                        console.log("Add new batch to list");
                        console.log(newBatch);
                        self.activeBatchList.splice(1, 0, newBatch);

                        window.open(self.settings.activeBatchUrl + '?batchId=' + result.Data, "_blank", "");
                    } else {
                        Message.error(result.Message);
                    }

                    helper.ui.hideLoading(addToBatchButton);
                    self.isOrderInfoLoaded(false);
                    self.search();
                }
            });
        }
    }

    self.checkOrdersAddToBatch = function (orderIds) {
        var defer = $.Deferred();
        $.ajax({
            url: self.settings.checkAddOrdersToBatchUrl,
            cache: false,
            type: "POST",
            data: {
                orderIds: orderIds.join(',')
            },
            success: function (result) {
                if (result.IsSuccess) {
                    defer.resolve();
                } else {
                    defer.reject();
                }
            }
        });

        return defer;
    }

    self.addToBatch = function (item, e) {
        var addToBatchButton = $(e.target);

        var orderIds = self.getAllChecked();
        if (orderIds.length == 0) {
            Message.error("No selected orders");
            return;
        }

        self.checkOrdersAddToBatch(orderIds)
            .done(function () {
                var batch = self.activeBatch();
                if (batch == null || batch == '') {
                    var date = new Date();

                    var isPriority = self.shippingStatus() == "PaidPriority"
                        || self.shippingStatus() == 'AllPriority';

                    self.getDayBatchNumber().done(function (result) {
                        var number = result.Data;

                        if (date.getHours() > 18)
                            date = date.addDays(1);

                        var defaultBatchName = (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear();

                        if (number == 0)
                            defaultBatchName += ' Main';
                        if (number == 1)
                            defaultBatchName += isPriority ? ' Priorities' : ' Second';
                        if (number == 2)
                            defaultBatchName += ' Third';
                        if (number == 3)
                            defaultBatchName += ' Forth';
                        if (number == 4)
                            defaultBatchName += ' Fifth ';

                        self.createBatch(defaultBatchName, orderIds, addToBatchButton);
                    });
                } else {
                    $.ajax({
                        url: self.settings.addOrdersToBatchUrl,
                        cache: false,
                        type: "POST",
                        data: {
                            batchId: batch,
                            orderIds: orderIds.join(',')
                        },
                        success: function (result) {
                            if (result.IsSuccess) {
                                Message.success("Orders have been added to the batch");
                                window.open(self.settings.activeBatchUrl + '?batchId=' + result.Data, "_blank", "");
                            } else {
                                Message.error(result.Message);
                            }

                            self.isOrderInfoLoaded(false);
                            self.search();
                        }
                    });
                }
            })
            .fail(function () {
                Message.error("Batch can’t be created, because some orders already were processed. Please refresh orders page and try again", self.batchId);
            });
    };

    self.removeFromBatch = function (orderId) {
        if (self.isBatchLocked() && !self.settings.isAdmin) {
            Message.error("Batch is locked, you can not remove orders from it", self.batchId);
            return;
        }

        $.ajax({
            url: self.settings.removeFromBatchUrl,
            data: { batchId: self.batchId, orderId: orderId },
            cache: false,
            success: function (result) {
                if (result.IsSuccess) {
                    Message.clear(self.batchId);
                    self.removeOrderWarnings(orderId);
                    self.grid.deleteRow(orderId);
                    self.search();
                } else {
                    Message.error(result.Message, self.batchId);
                }
            }
        });
    };

    self.onRemoveFromBatch = function (m, e) {
        var sender = $(e.target);

        if (self.isBatchLocked() && !self.settings.isAdmin) {
            Message.error("Batch is locked, you can not remove orders from it", self.batchId);
            return;
        }

        var orderIds = self.getAllChecked();

        if (orderIds.length == 0) {
            Message.error("No selected orders", self.batchId);
            return;
        }

        helper.ui.showLoading(sender, 'removing...');

        $.ajax({
            url: self.settings.removeFromBatchMultiUrl,
            cache: false,
            data: {
                batchId: self.batchId,
                toBatchId: self.activeBatch(),
                orderIds: orderIds.join(',')
            },
            success: function (data) {
                helper.ui.hideLoading(sender);

                self.activeBatch(null);
                if (data.IsSuccess) {
                    Message.clear(self.batchId);
                    $.each(orderIds, function (i, o) { self.removeOrderWarnings(o); });

                    self.isOrderInfoLoaded(false);
                    self.search();
                } else {
                    Message.error(data.Message, self.batchId);
                }
            }
        });
    };

    self.onUpgradeShippingService = function (m, e) {
        var sender = $(e.target);

        var orderIds = self.getAllChecked();

        if (orderIds.length == 0) {
            Message.error("No selected orders");
            return;
        }

        helper.ui.showLoading(sender, 'upgrading...');

        $.ajax({
            url: self.settings.upgradeShippingServiceUrl,
            //async: false,
            cache: false,
            data: {
                batchId: self.batchId,
                orderIds: orderIds.join(',')
            },
            success: function (data) {
                helper.ui.hideLoading(sender);

                if (data.IsSuccess) {
                    var msg = "Upgrade was successful";
                    if (data.Data != null && data.Data != '')
                        msg += " (skipped: " + data.Data + ")";
                    Message.success(msg, self.batchId);
                } else {
                    Message.error("Upgrade fails: " + data.Message, self.batchId);
                }

                self.isOrderInfoLoaded(false);
                self.search();
            },
        });
    };

    self.onReCalcShippingService = function (m, e) {
        var sender = $(e.target);

        Message.clear(self.batchId);
        var orderIds = self.getAllChecked();

        if (orderIds.length == 0) {
            Message.error("No selected orders", self.batchId);
            return;
        }

        self.onReCalcShippingServiceForOrderIds(sender, orderIds);
    }

    self.onReCalcShippingServiceForOrder = function (sender, orderId) {
        self.onReCalcShippingServiceForOrderIds(sender, [orderId]);
    }

    self.onReCalcShippingServiceForOrderIds = function (sender, orderIds) {
        //var sender = $(e.target);
        helper.ui.showLoading(sender, 'calculation...');

        $.ajax({
            url: self.settings.recalcShippingServiceUrl,
            //async: false,
            cache: false,
            type: "POST",
            data: {
                batchId: self.batchId,
                orderIds: orderIds.join(','),
                switchToMethodId: null,
            }
        }).done(function (data) {
            helper.ui.hideLoading(sender);

            if (data.IsSuccess) {
                var failedList = data.Data[0];
                var successList = data.Data[1];
                if (!dataUtils.isEmpty(failedList)) {
                    Message.appendError("Recalculation was failed/skipped for: " + failedList, self.batchId);
                }
                if (!dataUtils.isEmpty(successList)) {
                    Message.appendSuccess("Recalculation was successful for: " + successList, self.batchId);
                }
            }
            else {
                Message.error("Recalculate fails: " + data.Message, self.batchId);
            }

            self.isOrderInfoLoaded(false);
            self.search();
        }).fail(function (data, error) {
            console.log("fail");
            console.log(data.responseText);
            console.log(error);
        });
    };

    self.bulkShippingUpdatedCallback = function (m, e) {
        var sender = $(e.target);

        if (m.shippingProviderId == null)
            return;

        helper.ui.showLoading(sender, 'recalculation...');
        console.log("bulkShippingUpdatedCallback: " + m.shippingProviderId + " - " + m.shippingMethodId);

        var orderIds = self.getAllChecked();

        $.ajax({
            url: self.settings.recalcShippingServiceUrl,
            //async: false,
            cache: false,
            type: "POST",
            data: {
                batchId: self.batchId,
                orderIds: orderIds.join(','),
                switchToProviderid: m.shippingProviderId,
                switchToMethodId: m.shippingMethodId,
            }
        }).done(function (data) {
            helper.ui.hideLoading(sender);

            if (data.IsSuccess) {
                var failedList = data.Data[0];
                var successList = data.Data[1];
                if (!dataUtils.isEmpty(failedList)) {
                    Message.appendError("Recalculation was failed/skipped for: " + failedList, self.batchId);
                }
                if (!dataUtils.isEmpty(successList)) {
                    Message.appendSuccess("Recalculation was successful for: " + successList, self.batchId);
                }
            }
            else {
                Message.error("Recalculate fails: " + data.Message, self.batchId);
            }

            self.isOrderInfoLoaded(false);
            self.search();
        }).fail(function (data, error) {
            console.log("fail");
            console.log(data.responseText);
            console.log(error);
        });
    };

    self.onSetShippingService = function (m, e) {
        var sender = $(e.target);

        Message.clear(self.batchId);
        var orderIds = self.getAllChecked();

        if (orderIds.length == 0) {
            Message.error("No selected orders", self.batchId);
            return;
        }

        var popupModel = new SetBulkShippingPopupModel(self.model,
            self.settings,
            function (r) { self.bulkShippingUpdatedCallback(r, e); });
        popupModel.show();
    };

    self.lockBatch = function (m, e) {
        console.log("lockBatch: " + self.batchId);

        var sender = $(e.target);

        var result = self._validateLockBatchOrders();
        if (result.isSuccess) {
            self._lockBatchBase(sender);
        } else {
            console.log('lockBatch, popup');

            var message = self._validationResultToString(result);
            message += "<div>Continue locking the batch ignoring these warnings?</div>";

            Message.popup(message,
                Message.YES_NO,
                function () {
                    self._lockBatchBase(sender);
                },
                1);
        }
    };

    self._lockBatchBase = function (sender) {
        helper.ui.showLoading(sender, 'processing...');

        $.ajax({
            url: self.settings.lockBatchUrl,
            data: { batchId: self.batchId },
            cache: false,
            success: function (data) {
                helper.ui.hideLoading(sender);

                self.isBatchLocked(true);
            }
        });
    }

    self.toggleHold = function (id, onHold) {
        console.log("toggleHold: " + id + ", onHold=" + onHold);

        var toggledHold = onHold == "false" ? true : false;

        var displayRow = self.getRowById(id);
        var infoRow = self.getRowInfoById(id);

        var $tr = $('tr[row-uid="' + id + '"]');
        var holdButton = $tr.find("#holdButton");

        helper.ui.showLoading(holdButton);

        $.ajax({
            url: self.settings.setOnHoldUrl,
            data: { id: id, onHold: toggledHold },
            cache: false,
            success: function (data) {
                helper.ui.hideLoading(holdButton);

                self.prepareRow(data);

                if (displayRow != null) {
                    displayRow.OnHold = data.OnHold;
                    displayRow.OnHoldUpdateDate = data.OnHoldUpdateDate;

                    self.grid.updateRow(displayRow);
                }

                if (infoRow != null) {
                    infoRow.OnHold = data.OnHold;
                    infoRow.OnHoldUpdateDate = data.OnHoldUpdateDate;

                    self.calcChecked();
                    self.updateRowWarnings(infoRow);
                }
            }
        });
    };

    self.setFilter = function (filterName) {
        console.log('setFilter');
        self.clear();
        self.shippingStatus(filterName);
        self.search();
    };
    //END: page actions


    //BEGIN: Page Alerts
    self.duplicateOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.duplicate; }).length;
    });

    self.oversoldOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.oversold; }).length;
    });

    self.onHoldOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.onHold; }).length;
    });

    self.withoutPackageOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.woPackage; }).length;
    });

    self.withoutStampsPriceOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.woStampsPrice; }).length;
    });

    self.overdueShipDateOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.overdueShipDate; }).length;
    });

    self.todayShipDateOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.todayShipDate; }).length;
    });

    self.sameDayOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.sameDay; }).length;
    });

    self.dhlOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.dhl; }).length;
    });

    self.dhlEComOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.dhlECom; }).length;
    });

    self.ibcOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.ibc; }).length;
    });

    self.skyPostalOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.skyPostal; }).length;
    });

    self.fedexOneRateOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.fedexOneRate; }).length;
    });

    self.primeOrSecondDayOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.primeOrSecondDay; }).length;
    });

    self.miamiAreaOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.miamiArea; }).length;
    });

    self.upgradedOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.upgraded; }).length;
    });

    self.withNewStylesOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.withNewStyles; }).length;
    });

    self.invalidAddressOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.invalidAddress; }).length;
    });

    self.unprintedOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.unprinted; }).length;
    });

    self.noIssuesOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.noIssues; }).length;
    });

    self.noIssuesIBCGrouponOrdersCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.noIssuesIBCGroupon; }).length;
    });

    self.dismissedIssuesCount = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.dismissedIssues; }).length;
    });

    self.invalidAddressOrderList = ko.computed(function () {
        var items = $.grep(self.warningList(), function (i) { return i.type == warningTypes.invalidAddress; });

        items.sort(function (a, b) {
            if (a.onHold > b.onHold)
                return 1;
            if (a.onHold < b.onHold)
                return 0;

            if (a.AlignedExpectedShipDate == b.AlignedExpectedShipDate)
                return 0;

            if (a.AlignedExpectedShipDate > b.AlignedExpectedShipDate)
                return 1;
            return -1;
        });

        return items;
    });

    self.unprintedOrderList = ko.computed(function () {
        return $.grep(self.warningList(), function (i) { return i.type == warningTypes.unprinted; });
    });


    self.isOnHoldFilter = ko.computed(function () {
        return self.shippingStatus() == "OnHold";
    });

    self.isDuplicateFilter = ko.computed(function () {
        return self.shippingStatus() == "Duplicate";
    });

    self.isOversoldFilter = ko.computed(function () {
        return self.shippingStatus() == "Oversold";
    });

    self.isWoPackageFilter = ko.computed(function () {
        return self.shippingStatus() == "WoPackage";
    });

    self.isWithAddressIssuesFilter = ko.computed(function () {
        return self.shippingStatus() == "WithAddressIssues";
    });

    self.isWoStampsPriceFilter = ko.computed(function () {
        return self.shippingStatus() == "WoStampsPrice";
    });

    self.isOverdueShipDateFilter = ko.computed(function () {
        return self.shippingStatus() == "OverdueShipDate";
    });

    self.isTodayShipDateFilter = ko.computed(function () {
        return self.shippingStatus() == "TodayShipDate";
    });

    self.isSameDayFilter = ko.computed(function () {
        return self.shippingStatus() == "SameDay";
    });

    self.isDhlFilter = ko.computed(function () {
        return self.shippingStatus() == "DHL";
    });

    self.isDhlEComFilter = ko.computed(function () {
        return self.shippingStatus() == "DHLECom";
    });

    self.isIBCFilter = ko.computed(function () {
        return self.shippingStatus() == "IBC";
    });

    self.isSkyPostalFilter = ko.computed(function () {
        return self.shippingStatus() == "SKYPOSTAL";
    });

    self.isFedexOneRateFilter = ko.computed(function () {
        return self.shippingStatus() == "FedexOneRate";
    });

    self.isPrimeOrSecondDayFilter = ko.computed(function () {
        return self.shippingStatus() == "PrimeOrSecondDay";
    });

    self.isMiamiAreaFilter = ko.computed(function () {
        return self.shippingStatus() == "MiamiArea";
    });

    self.isUpgradedFilter = ko.computed(function () {
        return self.shippingStatus() == "Upgraded";
    });

    self.isWithNewStylesFilter = ko.computed(function () {
        return self.shippingStatus() == "WithNewStyles";
    });

    self.isNoIssuesFilter = ko.computed(function () {
        return self.shippingStatus() == "NoIssues";
    });

    self.isNoIssuesFilter = ko.computed(function () {
        return self.shippingStatus() == "NoIssuesIBCGroupon";
    });

    self.isDismissedIssuesFilter = ko.computed(function () {
        return self.shippingStatus() == "DismissedIssues";
    });
    //END: page alerts


    //BEGIN: search
    self.filterCallback = function (row, index, passedCount) {
        var filters = self.getFilters();

        return self.filterItem(filters, row, index, passedCount);
    }


    self.filterItem = function (filters, row, index, passedCount) {
        var pass = true;

        var isEmpty = filters.isEmpty;
        if (!isEmpty)
            return true;

        //debugger;
        if (!dataUtils.isEmpty(filters.bayNumber)) {
            //pass = pass && $.grep(row.Items, function (i) {
            //    return i.BayNumber == filters.bayNumber;
            //}).length > 0;
            pass = pass && row.BayNumber == filters.bayNumber;
        }

        if (!dataUtils.isEmpty(filters.maxOrders)) {
            pass = pass && passedCount < filters.maxOrders;
        }

        if (self.batchId > 0) {
            if (!dataUtils.isEmpty(filters.orderNumber)) {
                pass = pass && row.OrderId == filters.orderNumber;
            }
            if (!dataUtils.isEmpty(filters.labelNumber)) {
                pass = pass && $.grep(row.Labels, function (i) {
                    return i.NumberInBatch == filters.labelNumber;
                }).length > 0;
            }
            if (!dataUtils.isEmpty(filters.styleId)) {
                pass = pass && $.grep(row.Items, function (i) {
                    return i.StyleID == filters.styleId;
                }).length > 0;
            }
            if (!dataUtils.isEmpty(filters.styleItemId)) {
                pass = pass && $.grep(row.Items, function (i) {
                    console.log("item styleItemId=" + i.StyleItemId + ", filterId=" + filters.styleItemId);
                    return i.StyleItemId == filters.styleItemId;
                }).length > 0;
            }
        }

        if (filters.market != null && filters.market != 0) {
            pass = pass && row.Market == filters.market;
        }
        if (!dataUtils.isEmpty(filters.marketplaceId)) {
            pass = pass && row.MarketplaceId == filters.marketplaceId;
        }
        if (!dataUtils.isEmpty(filters.shippingStatus)) {
            var today = new Date();
            today.setHours(0, 0, 0, 0);
            var endToday = new Date();
            endToday.setHours(23, 59, 59, 0);
            var week2Ago = today.addDays(-14);

            if (filters.shippingStatus == 'AllPriority') {
                pass = pass && (row.Selected == 0
                    || row.IsShippingPriority);
                //|| row.Market == self.settings.walmartMarket);
            }

            if (filters.shippingStatus == 'AllStandard') {
                pass = pass && (row.Selected == 0 || !row.IsShippingPriority);
            }

            if (filters.shippingStatus == 'Duplicate') {
                pass = pass && row.IsPossibleDuplicate;
            }

            if (filters.shippingStatus == 'Oversold') {
                pass = pass && row.HasOversoldItems;
            }

            if (filters.shippingStatus == 'WoPackage') {
                pass = pass && row.Selected == 0;
            }

            if (filters.shippingStatus == 'WoStampsPrice') {
                pass = pass && (row.StampsShippingCost == 0 || row.StampsShippingCost == null);
            }

            if (filters.shippingStatus == 'UpgradeCandidates') {
                //TODO:
            }

            if (filters.shippingStatus == "NoWeight") {
                pass = pass && $.grep(row.Items, function (i) {
                    return i.Weight == 0 || i.Weight == null;
                }).length > 0;
            }

            if (filters.shippingStatus == "PaidPriority") {
                pass = pass && (row.InitialServiceType.indexOf("Standard") == -1);
            }

            if (filters.shippingStatus == 'OverdueShipDate') {
                pass = pass && (row.AlignedExpectedShipDate < today);
            }

            if (filters.shippingStatus == 'TodayShipDate') {
                pass = pass && (row.AlignedExpectedShipDate <= endToday || row.AlignedExpectedShipDate == null);
            }

            if (filters.shippingStatus == 'NotOnHold') {
                pass = pass && !row.OnHold;
            }

            if (filters.shippingStatus == 'OnHold') {
                pass = pass && row.OnHold;
            }

            if (filters.shippingStatus == 'SameDay') {
                pass = pass && row.ShippingMethodId == self.settings.shippingMethodIds.sameDay;
            }

            if (filters.shippingStatus == 'DHL') {
                pass = pass && (row.ShippingMethodId == self.settings.shippingMethodIds.dhl || row.ShippingMethodId == self.settings.shippingMethodIds.dhlMx);
            }

            if (filters.shippingStatus == 'DHLECom') {
                pass = pass && (row.ShipmentProviderType == self.settings.shipmentProviderTypes.dhlECom);
            }

            if (filters.shippingStatus == 'IBC') {
                pass = pass && (row.ShipmentProviderType == self.settings.shipmentProviderTypes.ibc);
            }

            if (filters.shippingStatus == 'SKYPOSTAL') {
                pass = pass && (row.ShipmentProviderType == self.settings.shipmentProviderTypes.skyPostal);
            }

            if (filters.shippingStatus == 'FedexOneRate') {
                pass = pass && (row.ShipmentProviderType == self.settings.shipmentProviderTypes.fedexOneRate
                    || row.ShipmentCarrier == 'FEDEX');
            }

            if (filters.shippingStatus == 'PrimeOrSecondDay') {
                pass = pass && (row.InitialServiceType == self.settings.shippingNames.secondDay
                    || row.InitialServiceType == self.settings.shippingNames.nextDay
                    || row.IsPrime);
            }

            if (filters.shippingStatus == 'MiamiArea') {
                pass = pass && row.ShippingState == "FL" && (row.ShippingCity || "").toLowerCase().indexOf("miami") >= 0;
            }

            if (filters.shippingStatus == 'Upgraded') {
                pass = pass && row.UpgradeLevel > 0;
            }

            if (filters.shippingStatus == 'WithNewStyles') {
                var hasNewStyle = $.grep(row.Items, function (n) { return n.ListingCreateDate != null && n.ListingCreateDate >= week2Ago }).length > 0;
                pass = pass && hasNewStyle;
            }

            if (filters.shippingStatus == 'ExpeditedNotOnHold') {
                pass = pass && (!row.OnHold && row.InitialServiceType.indexOf("Expedited") >= 0);
            }

            if (filters.shippingStatus == 'PrioritiesNotHold') {
                pass = pass && (!row.OnHold && row.InitialServiceType.indexOf("Standard") >= 0);
            }

            if (filters.shippingStatus == 'NoAddressIssues') {
                pass = pass && (row.AddressValidationStatus < self.settings.addressValidationStatus.invalid
                    || row.IsDismissAddressValidation)
                    && !row.HasAddressLengthIssue
                    && !row.HasAddressInvalidSymbols;
            }

            if (filters.shippingStatus == 'WithAddressIssues') {
                pass = pass && ((row.AddressValidationStatus >= self.settings.addressValidationStatus.invalid
                    && !row.IsDismissAddressValidation)
                    || row.HasAddressLengthIssue
                    || row.HasAddressInvalidSymbols);
            }

            if (filters.shippingStatus == 'DismissedIssues') {
                pass = pass && (row.IsDismissAddressValidation
                    || (row.IsOverchargedShipping && row.ShippingPriceInUSD + row.TotalExcessiveShipmentThreshold < row.StampsShippingCost && !row.OnHold));
            }

            if (filters.shippingStatus == 'NoIssues') {
                pass = pass && ($.grep(self.warningList(), function (w) {
                    return w.entityId == row.EntityId
                        && (w.type == warningTypes.invalidAddress
                            || w.type == warningTypes.onHold
                            || w.type == warningTypes.duplicate
                            || w.type == warningTypes.oversold
                            || w.type == warningTypes.woPackage
                            || w.type == warningTypes.woStampsPrice
                            || w.type == warningTypes.ibc
                            || w.type == warningTypes.skyPostal);
                }).length == 0);
            }

            if (filters.shippingStatus == 'NoIssuesIBCGroupon') {
                pass = pass && ($.grep(self.warningList(), function (w) {
                    return w.entityId == row.EntityId
                        && (w.type == warningTypes.invalidAddress
                            || w.type == warningTypes.onHold
                            || w.type == warningTypes.duplicate
                            || w.type == warningTypes.oversold
                            || w.type == warningTypes.woPackage
                            || w.type == warningTypes.woStampsPrice
                            || w.type == warningTypes.ibc
                            || w.type == warningTypes.skyPostal);
                }).length == 0) && row.Market != self.settings.marketNames.groupon;
            }

            if (filters.shippingStatus == 'NoIssuesIBCGrouponToday') {
                pass = pass && ($.grep(self.warningList(), function (w) {
                    return w.entityId == row.EntityId
                        && (w.type == warningTypes.invalidAddress
                            || w.type == warningTypes.onHold
                            || w.type == warningTypes.duplicate
                            || w.type == warningTypes.oversold
                            || w.type == warningTypes.woPackage
                            || w.type == warningTypes.woStampsPrice
                            || w.type == warningTypes.ibc
                            || w.type == warningTypes.skyPostal);
                }).length == 0)
                    //&& row.Market != self.settings.marketNames.groupon
                    && (row.AlignedExpectedShipDate <= endToday || row.AlignedExpectedShipDate == null);
            }
        }

        return pass;
    };

    self.getItemsAsync = function (gridParams) {
        var defer = $.Deferred();
        var filterParams = self.getFilters();
        var params = $.extend(gridParams, filterParams);

        $.ajax({
            cache: false,
            url: self.settings.getOrdersUrl,
            data: params,
            success: function (result) {
                for (var i = 0; i < result.Items.length; i++) {
                    var item = result.Items[i];
                    self.prepareRow(item);
                }

                if (filterParams.isEmpty) {
                    self.allOrderInfoList(result.Items);
                    self.warningList.removeAll();

                    $.each(result.Items, function (i, row) {
                        self.updateRowWarnings(row); //NOTE: for Recalculate/Upgrade shippings cases
                    });

                    self.infoCalculation();
                    self.calcChecked();

                    if (self.getFilters().isEmpty) {
                        self.isOrderInfoLoaded(true);
                    }
                }

                console.log("getAllAsync end: " + result.Items.length);

                defer.resolve(result);
            }
        });
        //}

        return defer;
    };


    self.search = function () {
        self.isSearchResult(!self.getFilters().isEmpty);

        self.resetRowNumber(0);
        self.checkedListCache = [];
        self.calcChecked();

        var filters = self.getFilters();

        if (!dataUtils.isEmpty(filters.maxOrders)) {
            self.grid.sortField("LocationIndex");
            self.grid.sortMode("asc");
        }

        if (self.isOrderInfoLoaded()
            && filters.isEmpty) {
            if (self.lastSearchStatus() != searchStatus.local) { //Was global search
                self.grid.setItems(self.allOrderInfoList(), self.allOrderInfoList().length);
            }

            self.grid.refresh();
        }
        else {
            self.grid.pageIndex(1);
            self.grid.read().done(function (result) {
                if (filters.isEmpty) {
                    self.allOrderInfoList(result.Items); //self.grid.items
                }
            });

            self.searchHistory.add(self.orderNumber());
        }

        self.lastSearchStatus(filters.isEmpty ? searchStatus.local : searchStatus.global);
    };

    self.orderIdSource = new kendo.data.DataSource({
        type: "aspnetmvc-ajax",
        //minLength: 3,
        transport: {
            read: self.settings.getOrderIdListUrl,
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

    self.styleIdSource = new kendo.data.DataSource({
        type: "aspnetmvc-ajax",
        //minLength: 3,
        transport: {
            read: self.settings.getStyleIdListUrl,
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

    self.styleErrorMessage = ko.observable([]);
    self.styleIdLoading = ko.observable(false);
    self.styleId.subscribe(function () {
        self.styleErrorMessage('');
        self.styleIdLoading(true);
        self.sizeList([{ Text: 'Select...', Value: '' }]);
        self.styleItemId(null);
        $.ajax({
            url: self.settings.getStyleSizeListUrl,
            data: { styleString: self.styleId(), onlyWithQty: false },
            cache: false,
            success: function (data) {
                self.styleIdLoading(false);
                if (data.Data != null && data.Data.length > 0) {
                    data.Data.unshift({ Text: 'Select...', Value: '' });
                    self.sizeList(data.Data);
                } else {
                    self.styleErrorMessage('StyleId is not found');
                    self.sizeList([{ Text: 'Select...', Value: '' }]);
                }
            }
        });
    });

    self.searchByKeyCmd = function (data, event) {
        if (event.keyCode == 13)
            self.search();
        return true;
    };

    self.getFilters = function () {
        var st = self.shippingStatus();
        var market = self.market();
        var marketplaceId = self.marketplaceId();
        var dropShipperId = self.dropShipperId();

        var from = kendo.toString(self.dateFrom(), 'MM/dd/yyyy');
        var to = kendo.toString(self.dateTo(), 'MM/dd/yyyy');
        var orderNumber = self.orderNumber();
        var labelNumber = self.labelNumber();
        var buyerName = self.buyerName();
        var styleId = self.styleId();
        var styleItemId = self.styleItemId();
        var bayNumber = self.bayNumber();
        var maxOrders = self.maxOrders();

        var isEmpty = dataUtils.isEmpty(from)
            && dataUtils.isEmpty(to)
            && dataUtils.isEmpty(buyerName)
            && dataUtils.isEmpty(styleId)
            && dataUtils.isEmpty(styleItemId)
            && dataUtils.isEmpty(orderNumber)
            && dataUtils.isEmpty(labelNumber);
        if (self.batchId > 0) {
            isEmpty = true;
        }
        return {
            dateFrom: from,
            dateTo: to,
            buyerName: buyerName,
            orderNumber: orderNumber,
            labelNumber: labelNumber,
            styleId: styleId,
            styleItemId: styleItemId,
            bayNumber: bayNumber,
            maxOrders: maxOrders,

            dropShipperId: dropShipperId,
            shippingStatus: st,
            market: market,
            marketplaceId: marketplaceId,

            batchId: self.batchId,

            isEmpty: isEmpty,
        };
    };

    self.clear = function (exlcudeLocal) {
        //withFullReload = withFullReload || false;

        //self.isFullReload(withFullReload);
        self.isSearchResult(false);

        if (exlcudeLocal != true) {
            self.shippingStatus('');
            self.marketValue(self.defaultMarket + "_" + self.defaultMarketplaceId);
        }

        self.dropShipperId(self.defaultDropShipperId);
        self.dateFrom('');
        self.dateTo('');
        self.buyerName('');
        self.orderNumber('');
        self.labelNumber('');
        self.styleId('');
        self.styleItemId('');
        self.bayNumber('');
        self.maxOrders('');
    };

    self.clearAndSearch = function () {
        self.clear();
        self.isOrderInfoLoaded(false);
        self.search();
    };

    self.searchByButton = function () {
        self.search();
    };
    //END: search


    //self.grid.bind("dataBound", self.dataBound);



    //BEGIN GRID

    console.log("itemsPerPage: 5");
    self.grid = new FastGridViewModel({
        gridId: self.gridName,
        rowTemplate: 'order-row-template' + self.batchId,
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        onRedraw: self.onRedraw,
        itemsPerPage: 5,
        isLocalMode: false,
        sortField: 'OrderDate', //'AlignedExpectedShipDate', 
        sortMode: 'desc',
        fields: [
            { name: "PersonName", type: 'string' },
            { name: "TotalItemQuantity", type: 'int' },
            { name: "ShippingMethodId", type: 'int' },
            { name: "ShippingMethodIndex", type: 'int' },
            { name: "ShippingCountry", type: 'string' },
            { name: "ItemPrice", type: 'int' },
            { name: "Weight", type: 'int' },
            { name: "StampsShippingCost", type: 'int' },
            { name: "AlignedExpectedShipDate", type: 'date' },
            { name: "OrderDate", type: 'date' },
            { name: "MainNumberInBatch", type: 'int' },
            { name: "LocationIndex", type: 'int' },
        ]
    });

    var batchIdName = "batch" + (self.batchId > 0 ? self.batchId : "");
    var checkbox = "<input class='check_page' type='checkbox' onclick='javascript: " + batchIdName + ".checkAllOnPage(this)' />";

    self.fastGridSettings = {
        gridId: self.grid.gridId,
        hierarchy: { enable: false },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: [
            { title: checkbox, width: "20px" },
            { title: "#", width: "20" },
            { title: "Status", width: "100px" },
            { title: "Person name / Order details", width: "auto", field: "PersonName", sortable: true },
            { title: "Qty", width: "40px", field: "TotalItemQuantity", sortable: true },
            { title: "Ship. Service", width: "60px", field: "ShippingMethodIndex", sortable: true },
            { title: "Cntry", width: "40px", field: "ShippingCountry", sortable: true },
            { title: "Price", width: "80px", field: "ItemPrice", sortable: true },
            { title: "Weight", width: "80px", field: "Weight", sortable: true },
            { title: "Stamps price", width: "150px", field: "StampsShippingCost", sortable: true },
            { title: "Exp. Ship", width: "95px", field: "AlignedExpectedShipDate", sortable: true },
            { title: "Order Date", width: "95px", field: "OrderDate", sortable: true },
            { title: "Actions", width: "118px", field: "MainNumberInBatch", sortable: true },
            { title: "Comment", width: "95px" },
        ],
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    //END GRID



    //BEGIN: buy postage / print
    self._checkFullName = function (name, country, state) {
        console.log("name=" + name + ", country=" + country + ", state=" + state);
        var requireFullName = false;
        if (country == 'US'
            && (state == 'AE' || state == 'AP' || state == 'AA')) {
            requireFullName = true;
        }
        if (country != 'US') {
            requireFullName = true;
        }
        if (requireFullName)
            return self.hasFullName(name);
        return true;
    };

    self.hasFullName = function (name) {
        name = name || "";
        var parts = name.split(' ');
        var longParts = 0;
        for (var i = 0; i < parts.length; i++)
            if (parts[i].length > 1)
                longParts++;

        return longParts > 1;
    };

    self.validationMessageList = [
        { type: validationTypes.withoutWeight, message: "Without weight" },
        { type: validationTypes.withoutService, message: "Without selected shipping service" },
        { type: validationTypes.noPostalService, message: "Without Postal Service label" },
        { type: validationTypes.withoutStampsPrice, message: "Without calculated stamps price" },
        { type: validationTypes.withoutPhoneNumber, message: "International or Fedex orders without phone numbers" },
        { type: validationTypes.withoutPersonName, message: "With person name lass then 2 characters or fullname required" },
        { type: validationTypes.hasCancellation, message: "Has cancellation requests" },
        { type: validationTypes.withAddressFormatError, message: "With address format error (in most cases with person name or zip code format)" },
        { type: validationTypes.withAddressLineLengthIssue, message: "Exceeded the address line length (max 35-DHL, 40-IBC characters)" },
        { type: validationTypes.withOnHold, message: "Has onHold orders" },
        { type: validationTypes.usInsured, message: "US insured" },
        { type: validationTypes.hasMailLabel, message: "Has mailing labels" },
        { type: validationTypes.hasPrintedLabel, message: "Has printed labels" },
        { type: validationTypes.hasIBC, message: "Has a mix of IBC/non-IBC orders" },
    ];

    self._validateLockBatchOrders = function () {
        var data = self.grid.items;

        var notValidList = [];
        $.each(data, function (i, row) {
            if (row.HasCancalationRequest)
                notValidList.push({
                    type: validationTypes.hasCancellation,
                    orderId: row.OrderId,
                });
            if (row.HasMailLabel)
                notValidList.push({
                    type: validationTypes.hasMailLabel,
                    orderId: row.OrderId,
                });
            if (row.HasAllLabel)
                notValidList.push({
                    type: validationTypes.hasPrintedLabel,
                    orderId: row.OrderId,
                });
        });

        var success = notValidList.length == 0;

        return {
            isSuccess: success,
            notValidList: notValidList
        };
    }

    self._validateBatchOrders = function () {
        var data = self.grid.items;

        var ibcOrdersCount = $.grep(data, function (row) {
            if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.ibc)
                return true;
            return false;
        }).length;
        console.log("nonIBC: " + data.length + "=" + ibcOrdersCount);
        var nonIbcBatch = ibcOrdersCount != data.length;

        var notValidList = [];
        $.each(data, function (i, row) {
            if (row.Selected == null || row.Selected == '') {
                notValidList.push({
                    type: validationTypes.withoutService,
                    orderId: row.OrderId,
                });
            } else {
                if (row.StampsShippingCost == null || row.StampsShippingCost == '' || row.StampsShippingCost == 0)
                    notValidList.push({
                        type: validationTypes.withoutStampsPrice,
                        orderId: row.OrderId,
                    });
            }
            if ((row.IsInternational
                || row.ShipmentProviderType == self.settings.shipmentProviderTypes.fedexOneRate)
                && !row.HasPhoneNumber)
                notValidList.push({
                    type: validationTypes.withoutPhoneNumber,
                    orderId: row.OrderId,
                });
            //if (row.ShipmentProviderType != self.settings.shipmentProviderTypes.amazon) { //Skip checking for Amazon provider
            if (row.PersonName == null
                || (//row.ShipmentProviderType == self.settings.shipmentProviderTypes.stamps &&
                    row.PersonName.length < 2)
                || !self._checkFullName(row.PersonName, row.ShippingCountry, row.ShippingState))
                notValidList.push({
                    type: validationTypes.withoutPersonName,
                    orderId: row.OrderId,
                });
            //}
            if (row.AddressValidationStatus == self.settings.addressValidationStatus.exception
                || row.AddressValidationStatus == self.settings.addressValidationStatus.invalidRecipientName
                || row.HasAddressInvalidSymbols) {
                console.log("addressFormatError: " + row.OrderId + ", " + row.AddressValidationStatus);
                notValidList.push({
                    type: validationTypes.withAddressFormatError,
                    orderId: row.OrderId,
                });
            }
            if ((row.ShipmentProviderType == self.settings.shipmentProviderTypes.dhl
                || row.ShipmentProviderType == self.settings.shipmentProviderTypes.ibc)
                && (row.HasAddressLengthIssue))
                notValidList.push({
                    type: validationTypes.withAddressLineLengthIssue,
                    orderId: row.OrderId,
                });

            if (nonIbcBatch) {
                if (row.ShipmentProviderType == self.settings.shipmentProviderTypes.ibc) {
                    notValidList.push({
                        type: validationTypes.hasIBC,
                        orderId: row.OrderId,
                    });
                }
            }

            if (row.OnHold)
                notValidList.push({
                    type: validationTypes.withOnHold,
                    orderId: row.OrderId,
                });
            if (row.HasCancalationRequest)
                notValidList.push({
                    type: validationTypes.hasCancellation,
                    orderId: row.OrderId,
                });
            if (row.IsInsured == true && row.ShippingCountry == 'US')
                notValidList.push({
                    type: validationTypes.usInsured,
                    orderId: row.OrderId,
                });
            if (row.HasMailLabel)
                notValidList.push({
                    type: validationTypes.hasMailLabel,
                    orderId: row.OrderId,
                });
            if (row.HasAllLabel)
                notValidList.push({
                    type: validationTypes.hasPrintedLabel,
                    orderId: row.OrderId,
                });
            if (row.IsPostalService
                && (row.ShipmentProviderType == self.settings.shipmentProviderTypes.fedexOneRate
                    || row.ShipmentProviderType == self.settings.shipmentProviderTypes.fedexStandardRate))
                notValidList.push({
                    type: validationTypes.noPostalService,
                    orderId: row.OrderId,
                });
        });

        var success = notValidList.length == 0;

        return {
            isSuccess: success,
            notValidList: notValidList
        };
    };

    self.checkForUpdate = function () {
        console.log('checkForUpdate');
        $.ajax({
            url: self.settings.checkPurchaseProgressUrl,
            data: { batchId: self.batchId },
            cache: false,
            success: function (result) {
                if (self.isPrintLabelsInProgress())
                    self.showBuyPostageProgress(result.Data);

                var data = result.Data;
                var isFinished = data == null || data.EndDate != null;
                console.log("isFinished: " + isFinished);
                if (isFinished) {
                    self.getPrintResult();
                }
            }
        });
    };

    self.showBuyPostageProgress = function (data) {
        if (data == null) {
            Message.info("Buying postage: 0 of ... orders", self.batchId);
            return;
        }
        var printed = data.ProcessedTotal;
        var total = data.CountToProcess;
        if (total == null) //For historical data
            total = "...";

        console.log("Buying postage: " + printed + " of " + total + " orders");
        Message.info("Buying postage: " + printed + " of " + total + " orders", self.batchId);
    };

    self.getPrintResult = function () {
        console.log('getPrintResult, printActionId=' + self.printActionId());

        if (self.printActionId() != null) {
            $.ajax({
                url: self.settings.getPrintResultUrl,
                data: { printActionId: self.printActionId() },
                cache: false,
                success: function (result) {
                    console.log("printResult received");
                    console.log(result);
                    if (result.IsSuccess && self.printActionId() != null) { //NOTE: prevent double processing results
                        self.printActionId(null);

                        window.clearInterval(self.interval);

                        var data = result.Data;

                        console.log(data.Url);

                        self.isPrintLabelsInProgress(false);
                        Message.clear(self.batchId); //Hide in-progress message

                        if (data.NoPdf)
                            Message.appendError("Labels can't be purchased<br/>", self.batchId);

                        if (data.Message != null && data.Message != '')
                            Message.appendError(data.Message, self.batchId);

                        if (data.Url != null && data.Url != "") {
                            //debugger;
                            var message = "Labels were printed<br/>";
                            if (!dataUtils.isEmpty(data.PickupConfirmationNumber))
                                message += "Pickup was scheduled, ready by time: " + data.PickupReadyByTime + ", confirmation number: " + data.PickupConfirmationNumber + "<br/>";
                            Message.appendSuccess(message, self.batchId);
                            window.open(data.Url, "_blank", "");

                            self.batchPrintPackUrl(data.Url);
                        }

                        self.isOrderInfoLoaded(false);
                        self.search();
                    } else {
                        //Nothing, no results
                    }
                }
            });
        }
    }

    self.getStartupPrintStatus = function () {
        console.log('getStartupPrintStatus');
        $.ajax({
            url: self.settings.checkPurchaseProgressUrl,
            data: { batchId: self.batchId },
            cache: false,
            success: function (result) {
                var data = result.Data;
                var isFinished = data == null || data.EndDate != null;
                console.log("isFinished: " + isFinished);
                if (isFinished == 0) {
                    self.isPrintLabelsInProgress(true);
                    self.interval = window.setInterval(function () { self.checkForUpdate(); }, 5000);
                    self.showBuyPostageProgress(data);
                }
            }
        });
    };

    self._downgradeOrdersAsync = function (orderIds) {
        var defer = $.Deferred();
        $.ajax({
            url: self.settings.downgradeShippingServiceUrl,
            //async: false,
            cache: false,
            data: {
                batchId: self.batchId,
                orderIds: orderIds.join(',')
            },
            success: function (data) {
                defer.resolve(data);
            }
        });
        return defer;
    };

    self._getToUpdateSecondDayOrders = function () {
        var data = self.grid.items;

        var today = new Date();
        today.setHours(0, 0, 0, 0);

        var toUpdate = [];
        $.each(data, function (i, row) {
            if ((row.ShippingMethodId == self.settings.shippingMethodIds.amzExpressFlat
                || row.ShippingMethodId == self.settings.shippingMethodIds.amzExpressRegular
                || row.ShippingMethodId == self.settings.shippingMethodIds.stampsExpressFlat
                || row.ShippingMethodId == self.settings.shippingMethodIds.stampsExpressRegular)
                && row.AlignedExpectedShipDate.addDays(-1) > today) {
                toUpdate.push(row.Id);
            }
        });

        return toUpdate;
    };

    self._printLabels = function () {
        console.log('_printLabels');
        self.isPrintLabelsInProgress(true);
        self.showBuyPostageProgress(null);

        self.interval = window.setInterval(function () { self.checkForUpdate(); }, 5000);

        $.ajax({
            url: self.settings.printLabelsForBatchUrl,
            cache: false,
            data: { batchId: self.batchId },
            success: function (result) {
                console.log("_printLabel, results=");
                console.log(result);
                if (result.IsSuccess) {
                    self.printActionId(result.Data);
                    self.isPrintLabelsInProgress(true);
                } else {
                    Message.error(result.Message);
                }
            }
        });
    };

    self.OnRebuildLabelsPdf = function () {
        self._printLabels();
    }

    self.OnPrintLabels = function () {
        //debugger;
        Message.popup('Are you sure you want to buy postage?',
            Message.YES_NO,
            function () {
                console.log('onPrintLabels, yes, batchId=' + self.batchId);

                var printFunc = function () {
                    var result = self._validateBatchOrders();
                    if (result.isSuccess) {
                        self._printLabels();
                    } else {
                        console.log('onPrintLabels, popup');

                        var message = self._validationResultToString(result);
                        message += "<div>Continue printing the batch ignoring these errors/warnings?</div>";

                        Message.popup(message, Message.YES_NO, function () { self._printLabels(); }, 1);
                    }
                };

                var toUpdateList = self._getToUpdateSecondDayOrders();
                if (toUpdateList.length > 0) {
                    Message.popupEx({
                        html: 'The following orders: ' + toUpdateList.join(', ') + ' have Exp. Ship. Date yesterday. Do you want to change the shipping method to Priority for them?',
                        type: Message.YES_NO,
                        yesCallback: function () {
                            self._downgradeOrdersAsync(toUpdateList).done(function (updateResult) {
                                if (updateResult.IsSuccess) {
                                    console.log("donwgrade success");
                                    printFunc();
                                } else {
                                    Message.popup("System can't downgrade shipping method, details: " + updateResult.Message, Message.CLOSE);
                                }
                            });
                        },
                        noCallback: function () {
                            printFunc();
                        }
                    });
                } else {
                    printFunc();
                }
            }
        );
    };

    self.OnPrintPackingSlip = function () {
        var result = self._validateBatchOrders();
        if (result.isSuccess) {
            self._onPrintPackingSlip();
        } else {
            var message = self._validationResultToString(result);
            message += "<div>Continue printing the Packing Slips?</div>";

            Message.popup(message, Message.YES_NO, function () { self._onPrintPackingSlip(); }, 1);
        }
    };

    self._validationResultToString = function (result) {
        //Warnings
        var warningMessage = "";
        warningMessage += self._validationResultForType(result.notValidList, validationTypes.withoutWeight);
        warningMessage += self._validationResultForType(result.notValidList, validationTypes.usInsured);
        warningMessage += self._validationResultForType(result.notValidList, validationTypes.hasCancellation);
        warningMessage += self._validationResultForType(result.notValidList, validationTypes.noPostalService);

        if (warningMessage != "")
            warningMessage = "<div style='padding-top: 5px'>Orders have the following warning:</div>" + warningMessage;

        //Error
        var errorMessage = "";
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withoutService);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withoutStampsPrice);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withoutPersonName);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withAddressFormatError);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withAddressLineLengthIssue);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withOnHold);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.withoutPhoneNumber);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.hasMailLabel);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.hasPrintedLabel);
        errorMessage += self._validationResultForType(result.notValidList, validationTypes.hasIBC);

        if (errorMessage != "")
            errorMessage = "<div style='padding-top: 5px'>Orders have the following errors:</div>" + errorMessage;

        return warningMessage + errorMessage;
    };

    self._validationResultForType = function (notValidList, type) {
        var typeItem = self.validationMessageList.firstOrDefault(function (n) { return n.type == type; });
        var messages = $.grep(notValidList, function (n) { return n.type == type; });
        if (messages.length > 0) {
            var orderIdList = $.map(messages, function (n) { return n.orderId; });
            return "<div>- " + typeItem.message + ": <div class='red-warning list'>" + orderIdList.join("<br/>") + "</div></div>";
        }
        return "";
    }

    self._onPrintPackingSlip = function () {
        window.open(self.settings.getPackingSlipUrl + '?batchId=' + self.batchId, "_blank", "");
    };

    //END: buy postage / print



    self.isSearchCalled = 0;
    self.load = function () {
        console.log("load, batchId=" + self.batchId);
        if (self.isSearchCalled == 0) {
            self.search();
            if (self.batchId > 0)
                self.getStartupPrintStatus();
        }
        self.isSearchCalled = 1;
    };
};