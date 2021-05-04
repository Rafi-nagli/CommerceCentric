var StyleListViewModel = function(model, settings) {
    var self = this;

    self.model = model;
    self.settings = settings;

    self.gridName = 'StyleListGrid';

    self.selectedStyle = model.SelectedStyleId;

    self.items = ko.observableArray([]);
    self.rowTotal = ko.computed(function() {
        return self.items.length;
    });
    //self.itemsReceiveDate = ko.observable();

    //Search begin
    self.searchFilters = {
        barcode: ko.observable(''),
        styleId: ko.observable(''),
        keywords: ko.observable(''),
        idList: null,
        gender: ko.observable(""),
        mainLicense: ko.observable(""),
        subLicense: ko.observable(""),
        itemStyles: ko.observable([]),
        dropShipperId: ko.observable(""),
        sleeves: ko.observable([]),
        onlineStatus: ko.observable(""),
        excludeMarketplaceId: ko.observable(""),
        includeMarketplaceId: ko.observable(""),
        hasInitialQty: ko.observable(false),
        minQty: ko.observable(null),
        onlyInStock: ko.observable(false),
        includeKiosk: ko.observable(false),
        onlyOnHold: ko.observable(false),
        pictureStatus: ko.observable(""),
        fillingStatus: ko.observable(""),
        noneSoldPeriod: ko.observable(""),
        holidayId: ko.observable(""),
    };
        
    self.searchFilters.styleId.subscribe(function() {
        console.log('styleId.redrawAll');
        self.search();
    });

    self.searchFilters.includeKiosk.subscribe(function () {
        console.log('includeKiosk.redrawAll');
        self.search();
    });

    self.searchFilters.onlyOnHold.subscribe(function () {
        self.search();
    });

    self.searchFilters.genderList = model.GenderList;
    self.searchFilters.mainLicenseList = model.MainLicenseList;
    self.searchFilters.allSubLicenseList = model.SubLicenseList;
    self.searchFilters.itemStyleList = model.ItemStyleList;
    self.searchFilters.sleeveList = model.SleeveList;
    self.searchFilters.onlineStatusList = model.OnlineStatusList;
    self.searchFilters.marketplaceList = model.MarketplaceList;
    self.searchFilters.pictureStatusList = settings.pictureStatusList;
    self.searchFilters.fillingStatusList = settings.fillingStatusList;
    self.searchFilters.noneSoldPeriodList = settings.noneSoldPeriodList;
    self.searchFilters.dropShipperList = model.DropShipperList;
    self.searchFilters.holidayList = model.HolidayList;

    self.searchFilters.subLicenseList = ko.computed(function () {
        var selectedLicense = self.searchFilters.mainLicense();
        return ko.utils.arrayFilter(self.searchFilters.allSubLicenseList, function (l) {
            return selectedLicense == l.ParentValue;
        });
    });
    self.searchFilters.enableSubLicense = ko.computed(function () {
        return self.searchFilters.mainLicense() != null && self.searchFilters.mainLicense() != '';
    });
    self.searchFilters.mainLicense.subscribe(function () { self.searchFilters.subLicense(""); });
    //Search end

    //Begin group
    self.getRowById = function (id) {
        return self.grid.getRowDataById(id);
    };

    self.checkedListCache = [];
    self.checkedCount = ko.observable(0);

    self.onRedraw = function (rows) {
        console.log("onRedraw, rows count: " + rows.length);

        $.each(rows, function (i, row) {
            //row.UIChecked = false;
            self.afterUpdateRow(row);
        });
    }

    self.afterUpdateRow = function (row) {
        var rowNode = $('tr[row-uid="' + row.Id + '"]');
        if (rowNode.length > 0) {
            console.log("rowId=" + row.Id);
            if (self.checkedListCache[row.Id] == true) { //.indexOf(row.EntityId) >= 0) {
                console.log("checkedListCache=true, id=" + row.Id);
                $cb = rowNode.find(".check_row");
                if (!$cb.is(":disabled")) {
                    $cb.prop("checked", "checked");
                    row.UIChecked = true;
                }
            }
        }
    }

    self.calcChecked = function () {
        var count = $.grep(self.checkedListCache, function (i) {
            return i == true;
        }).length;

        self.checkedCount(count);
    };

    self.checkOne = function (sender, entityId) {
        var checked = $(sender).is(":checked");
        console.log("checkOne: " + checked + ',id=' + entityId);
        var displayRow = self.getRowById(entityId);

        if (displayRow != null) {
            console.log("Update UIChecked: " + checked);
            displayRow.UIChecked = checked;
        }
        self.checkedListCache[entityId] = checked;

        self.calcChecked();
    };

    self.toggleAllOnPage = function (sender) {
        var checked = $(sender).is(":checked");
        if (checked)
            self.checkAllOnPage();
        else
            self.uncheckAllOnPage();
    }

    self.checkAllOnPage = function () {
        var checked = true;        
        var items = self.grid.pageItems;//allOrderInfoList();
        console.log("Items: " + items.length);
        //var checked = true;
        //self.checkedListCache = [];

        $.each(items, function (i, row) {
            var displayRow = self.getRowById(row.Id);
            if (displayRow != null)
                displayRow.UIChecked = checked;

            self.checkedListCache[row.Id] = checked;
        });

        $("#" + self.gridName + " tr input:checkbox").prop("checked", checked);

        self.calcChecked();
    };

    self.uncheckAllOnPage = function (sender) {
        var checked = false;
        var items = self.grid.pageItems;//allOrderInfoList();
        //var checked = true;
        //self.checkedListCache = [];

        $.each(items, function (i, row) {
            var displayRow = self.getRowById(row.Id);
            if (displayRow != null)
                displayRow.UIChecked = checked;

            self.checkedListCache[row.Id] = checked;
        });

        $("#" + self.gridName + " tr input:checkbox").prop("checked", checked);

        self.calcChecked();
    };

    self.getAllChecked = function () {
        var list = [];

        $.each(self.checkedListCache, function (i, row) {
            if (row == true) {
                list.push(i);
            }
        });

        return list;
    };

    self.addToGroup = function (sender) {
        console.log("addInventoryGroup");
        var checkedStyleIds = self.getAllChecked();
        console.log("checkedStyleIds:" + checkedStyleIds.length);

        if (checkedStyleIds.length == 0) {
            Message.error("No selected styles");
            return;
        }

        var params = [];
        for (var i = 0; i < checkedStyleIds.length; i++)
            params.push("styleIds[" + i + "]=" + checkedStyleIds[i]);


        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.addInventoryGroup + "?" + params.join("&"),
            title: "Add Inventory Group",
            width: 450,
            submitSuccess: function (result) {
                //self.updateSealedBoxRows(styleId);
            }
        });
    };

    
    //End group


    self.addStyle = function() {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createDefaultStyle,
            title: "Add Style",
            width: 700,
            customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                console.log("addStyle.success");
                if (result.Row != null) //NOTE: can be null when filters was enabled
                {
                    self.prepareRow(result.Row);
                    self.grid.insertRow(result.Row);
                    self.grid.refresh();
                }
            }
        });
    };


    self.addReferenceStyle = function() {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createReferenceStyle,
            title: "Add Virtual Style",
            width: 670,
            customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                console.log("addReferenceStyle.success");
                self.prepareRow(result.Row);
                self.grid.insertRow(result.Row);
                self.grid.refresh();
            }
        });
    }

    self.editReferenceStyle = function (sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editReferenceStyle + "?id=" + id,
            title: "Edit Virtual Style",
            width: 670,
            customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                self.prepareRow(result.Row);
                self.grid.insertRow(result.Row);
            }
        });
    }


    self.mergeStyle = function() {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.mergeStyle,
            title: "Merge Style",
            width: 420,
            submitSuccess: function(result) {
                self.grid.refresh();
            }
        });
    };

    self.toggleHold = function (sender, id) {
        console.log("toggleHold, sender=" + sender + ", id=" + id);

        var displayRow = self.grid.getRowDataById(id);
        var newStatus = !displayRow.OnHold;

        console.log("new onHold status=" + newStatus);

        helper.ui.showLoading(sender);

        $.ajax({
            url: self.settings.urls.setOnHold,
            data: { id: id, onHold: newStatus },
            cahce: false,
            success: function(data) {
                helper.ui.hideLoading(sender);

                self.prepareRow(data);

                displayRow.OnHold = data.OnHold;

                self.grid.updateRow(displayRow);
            },
        });
    }

    self.defaultStyleAction = function(sender, type, id) {
        self.editStyleQuantity(sender, id);
    };

    self.editStyle = function (sender, type, id) {
        //if (type == self.settings.styleTypes.reference)
        //    self.editReferenceStyle(sender, id);
        //else
        self.editDefaultStyle(sender, id);
    };

    self.editDefaultStyle = function(sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editDefaultStyle + "?id=" + id,
            title: "Edit Style",
            width: 700,
            customAction: self.onPopupCustomAction,
            submitSuccess: function(result) {
                self.prepareRow(result.Row);
                self.grid.updateRowField(result.Row, result.UpdateFields);
            }
        });
    };

    self.editStyleQuantity = function(sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editStyleQuantity + "?styleId=" + id,
            title: "Style Quantity",
            width: 820,
            customAction: self.onPopupCustomAction,
            submitSuccess: function(result) {
                self.prepareRow(result.Row);
                self.grid.updateRowField(result.Row, result.UpdateFields);
            }
        });
    };

    self.editStylePrice = function (sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editStylePrice + "?styleId=" + id,
            title: "Style Price",
            width: 980,
            customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                self.prepareRow(result.Row);
                self.grid.updateRowField(result.Row, result.UpdateFields);
            }
        });
    };

    self.onPopupCustomAction = function (action, data) {
        console.log("onPopupCustomAction, type=" + data.type + ", id=" + data.id);
        if (action == 'openStyleEdit') {
            self.editStyle(this, data.type, data.id);
        }
        if (action == "openStyleQuantity") {
            self.editStyleQuantity(this, data.id);
        }
        if (action == "openStylePrice") {
            self.editStylePrice(this, data.id);
        }
    };

    self.deleteStyle = function(sender, id) {
        if (confirm('Are you sure you want to delete style?')) {
            $.ajax({
                url: self.settings.urls.deleteStyle,
                data: { id: id },
                async: false,
                success: function() {
                    console.log("deleteStyle success, id=" + id);
                    self.grid.deleteRow(id);
                }
            });
        }
    };

    self.createSealedBox = function(sender, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createSealedBox + "?styleId=" + styleId,
            title: "Sealed Box",
            width: 450,
            submitSuccess: function(result) {
                self.updateSealedBoxRows(styleId);
            }
        });
    };

    self.createBoxWizard = function (sender, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createBoxWizard + "?styleId=" + styleId,
            title: "Add Box Wizard",
            width: 600
        });
    };

    self.editSealedBox = function(sender, sealedBoxId, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editSealedBox + "?sealedBoxId=" + sealedBoxId,
            title: "Sealed Box",
            width: 450,
            submitSuccess: function(result) {
                self.updateSealedBoxRows(styleId);
            }
        });
    };

    self.deleteSealedBox = function(sender, sealedBoxId, styleId) {
        if (confirm('Are you sure you want to delete box?')) {
            helper.ui.showLoading(sender, "deleting...");
            $.ajax({
                url: self.settings.urls.deleteSealedBox,
                data: { id: sealedBoxId },
                success: function() {
                    helper.ui.hideLoading(sender);
                    self.updateSealedBoxRows(styleId);
                }
            });
        }
    };

    self.copySealedBox = function(sender, sealedBoxId, styleId) {
        helper.ui.showLoading(sender, "copying...");
        $.ajax({
            cache: false,
            url: self.settings.urls.copySealedBox,
            data: { id: sealedBoxId },
            success: function() {
                helper.ui.hideLoading(sender);
                self.updateSealedBoxRows(styleId);
            }
        });
    };

    self.createOpenBox = function(sender, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createOpenBox + "?styleId=" + styleId,
            title: "Open Box",
            width: 450,
            submitSuccess: function(result) {
                console.log('create open box');
                self.updateOpenBoxRows(styleId);
            }
        });
    };

    self.editOpenBox = function(sender, openBoxId, styleId) {
        popupWindow.initAndOpenWithSettings(
            {
                content: self.settings.urls.editOpenBox + "?openBoxId=" + openBoxId,
                title: "Open Box",
                width: 450,
                submitSuccess: function(result) {
                    self.updateOpenBoxRows(styleId);
                }
            });
    };

    self.deleteOpenBox = function(sender, openBoxId, styleId) {
        if (confirm('Are you sure you want to delete box?')) {
            helper.ui.showLoading(sender, "deleting...");
            $.ajax({
                url: self.settings.urls.deleteOpenBox,
                data: { id: openBoxId },
                success: function() {
                    helper.ui.hideLoading(sender);
                    self.updateOpenBoxRows(styleId);
                }
            });
        }
    };
        
    self.searchByKeyCmd = function(data, event) {
        console.log('searchByKeyCmd');
        if (event.keyCode == 13)
            self.search(true);
        return true;
    };

    self.getSearchParams = function () {
        return {
            styleString: self.searchFilters.styleId(),
            barcode: self.searchFilters.barcode(),
            keywords: self.searchFilters.keywords(),
            gender: self.searchFilters.gender(),
            itemStyles: self.searchFilters.itemStyles(),
            sleeves: self.searchFilters.sleeves(),
            mainLicense: self.searchFilters.mainLicense(),
            subLicense: self.searchFilters.subLicense(),
            dropShipperId: self.searchFilters.dropShipperId(),
            onlyInStock: self.searchFilters.onlyInStock(),
            onlineStatus: self.searchFilters.onlineStatus(),
            excludeMarketplaceId: self.searchFilters.excludeMarketplaceId(),
            includeMarketplaceId: self.searchFilters.includeMarketplaceId(),
            hasInitialQty: self.searchFilters.hasInitialQty(),
            minQty: self.searchFilters.minQty(),
            includeKiosk: self.searchFilters.includeKiosk(),
            onlyOnHold: self.searchFilters.onlyOnHold(),
            pictureStatus: self.searchFilters.pictureStatus(),
            fillingStatus: self.searchFilters.fillingStatus(),
            noneSoldPeriod: self.searchFilters.noneSoldPeriod(),
            holidayId: self.searchFilters.holidayId(),
        };
    }

    //self.checkUpdates = function() {
    //    var defer = $.Deferred();
    //    if (self.itemsReceiveDate() != null) //NOTE: Skip any change requests if loading not complete
    //    {
    //        console.log("itemsReceiveDate=" + self.itemsReceiveDate());
    //        $.ajax({
    //            url: self.settings.urls.getAllUpdates,
    //            data: { fromDate: self.itemsReceiveDate() },
    //            success: function (result) {
    //                console.log("items: " + result.Items.length);
    //                for (var i = 0; i < result.Items.length; i++) {
    //                    var item = result.Items[i];
    //                    self.prepareRow(item);
    //                    self.grid.updateRow(item);
    //                }

    //                console.log("receiveDate=" + result.GenerateDate);
    //                self.itemsReceiveDate(result.GenerateDate);

    //                defer.resolve();
    //            }
    //        });
    //    } else {
    //        console.log("itemsReceiveDate=null");
    //        defer.resolve();
    //    }

    //    return defer;
    //}

    self.searchInProgress = ko.observable(false);
    self.search = function () {
        self.searchInProgress(true);

        self.grid.read({ Page: 1 }).done(function () {
            self.searchInProgress(false);
        });
    };

    //self.search = function () {
    //    self.checkUpdates().done(function() {
    //        if (!dataUtils.isEmpty(self.searchFilters.styleId()))
    //            self.grid.refresh();

    //        console.log('GetIdListByFilters begin, time=' + performance.now());
    //        $.ajax({
    //            url: self.settings.urls.getIdListByFilters,
    //            data: {
    //                barcode: self.searchFilters.barcode(),
    //                gender: self.searchFilters.gender(),
    //                itemStyles: self.searchFilters.itemStyles(),
    //                sleeves: self.searchFilters.sleeves(),
    //                mainLicense: self.searchFilters.mainLicense(),
    //                subLicense: self.searchFilters.subLicense(),
    //                onlineStatus: self.searchFilters.onlineStatus(),
    //                excludeMarketplaceId: self.searchFilters.excludeMarketplaceId(),
    //                hasInitialQty: self.searchFilters.hasInitialQty(),
    //                minQty: self.searchFilters.minQty(),
    //                includeKiosk: self.searchFilters.includeKiosk(),
    //                pictureStatus: self.searchFilters.pictureStatus(),
    //            },
    //            success: function(result) {
    //                console.log('GetIdListByFilters end, time=' + performance.now());
    //                self.searchFilters.idList = result.Data;
    //                self.grid.refresh();
    //            }
    //        });
    //    });
    //};

    self.clear = function() {
        self.searchFilters.barcode('');
        self.searchFilters.styleId('');
        self.searchFilters.keywords('');
        self.searchFilters.idList = null;
            
        self.searchFilters.gender("");
        self.searchFilters.mainLicense("");
        self.searchFilters.subLicense("");
        self.searchFilters.itemStyles("");
        self.searchFilters.dropShipperId("");
        self.searchFilters.sleeves("");

        self.searchFilters.onlineStatus("");
        self.searchFilters.excludeMarketplaceId("");
        self.searchFilters.includeMarketplaceId("");

        self.searchFilters.hasInitialQty(false);
        self.searchFilters.minQty(null);
        self.searchFilters.includeKiosk(false);
        self.searchFilters.onlyOnHold(false);

        self.searchFilters.pictureStatus("");
        self.searchFilters.fillingStatus("");
        self.searchFilters.noneSoldPeriod("");
        self.searchFilters.holidayId("");

        self.search();
    };

    self.updateOpenBoxRows = function(styleId) {
        var loadingTagId = "OpenBox_Loading_" + styleId;
        $("#" + loadingTagId).show();
            
        $.ajax({
            cache: false,
            url: self.settings.urls.getOpenBoxes + '?styleId=' + styleId,
            success: function(result) {
                console.log("end getall open, time=" + performance.now());
                var tagId = "OpenBox_" + styleId;
                $("#" + tagId).html('');

                var compile = tmpl.compileT('style-openbox-row-template');
                document.getElementById(tagId).innerHTML = kendo.render(compile, result.Data);
                    
                $("#" + loadingTagId).hide();
            }
        });
    };

    self.updateSealedBoxRows = function(styleId) {
        var loadingTagId = "SealedBox_Loading_" + styleId;
        $("#" + loadingTagId).show();
        $.ajax({
            cache: false,
            url: self.settings.urls.getSealedBoxes + '?styleId=' + styleId,
            success: function(result) {
                console.log("end getall sealed, time=" + performance.now());
                var tagId = "SealedBox_" + styleId;
                $("#" + tagId).html('');
    
                var compile = tmpl.compileT('style-sealedbox-row-template');
                document.getElementById(tagId).innerHTML = kendo.render(compile, result.Data);
                    
                $("#" + loadingTagId).hide();
            }
        });
    };

    self.toggleGridRow = function(id) {
        console.log('toggleGridRow, id=' + id);
        var tagId = '#Detail_' + id;
        var visible = $(tagId).is(":visible");
        if (!visible) {
            console.log('begin get boxes');
            $(tagId).show();

            self.updateSealedBoxRows(id);
            self.updateOpenBoxRows(id);
        } else {
            $(tagId).hide();
        }
    };

    //self.filterCallback = function(row) {
    //    if (self.searchFilters.styleId() != null && self.searchFilters.styleId() != '')
    //    {
    //        var reg = new RegExp(self.searchFilters.styleId(), 'i');
    //        if (row != null && row.StyleId != null && row.StyleId != '') {
    //            if (reg.test(row.StyleId))
    //                return true;
    //        }
                
    //        return false;
    //    }

    //    var pass = true;

    //    if (row.DisplayMode == 1) {
    //        console.log("DisplayMode=" + row.DisplayMode);
    //    }

    //    if (!dataUtils.isEmpty(self.searchFilters.pictureStatus())) {
    //        pass = pass && (row.PictureStatus == self.searchFilters.pictureStatus());
    //    }

    //    if (!dataUtils.isEmpty(self.searchFilters.keywords())) {
    //        var reg = new RegExp(dataUtils.trim(self.searchFilters.keywords()), 'i');
    //        var isMatch = reg.test(row.StyleId || '')
    //            || reg.test(row.Name || '');

    //        pass = pass && isMatch;
    //    }

    //    if (self.searchFilters.idList != null) {
    //        pass = pass && self.searchFilters.idList.indexOf(row.Id) >= 0;
    //    }
    //    if (!dataUtils.isEmpty(self.searchFilters.onlineStatus())) {
    //        var onlineMarketplaces = $.grep(row.Marketplaces, function (l) {
    //            return l.Count > 0;
    //        });
    //        if (self.searchFilters.onlineStatus() == "Online")
    //            pass = pass && onlineMarketplaces.length > 0 && row.RemainingQuantity > 0;
    //        if (self.searchFilters.onlineStatus() == "Offline")
    //            pass = pass && onlineMarketplaces.length == 0 && row.RemainingQuantity > 0;
    //    }
    //    if (!self.searchFilters.includeKiosk()) {
    //        pass = pass && !row.IsHidden;
    //    }
    //    if (self.searchFilters.minQty() != null) {
    //        pass = pass && row.RemainingQuantity >= self.searchFilters.minQty();
    //    }
    //    if (!dataUtils.isEmpty(self.searchFilters.excludeMarketplaceId())) {
    //        pass = pass
    //            && $.grep(row.Marketplaces, function (l) {
    //                return l.Count > 0
    //                    && (l.Market + ';' + l.MarketplaceId) == self.searchFilters.excludeMarketplaceId();
    //            }).length == 0
    //            && row.RemainingQuantity > 0;
    //    }

    //    return pass;
    //};

    self.getItemsAsync = function(params) {
        var defer = $.Deferred();

        var searchParams = self.getSearchParams();
        $.extend(params, searchParams);

        $.ajax({
            cache: false,
            data: params,
            url: self.settings.urls.getStyles,
            success: function(result) {
                for (var i = 0; i < result.Items.length; i++) {
                    var item = result.Items[i];
                    self.prepareRow(item);
                }

                console.log("receiveDate=" + result.GenerateDate);
                //self.itemsReceiveDate(result.GenerateDate);

                console.log("getAllAsync end: " + result.Items.length);
                defer.resolve(result);
            }
        });
        return defer;
    };

    self.prepareRow = function(rowData) {
        rowData.Name = rowData.Name || '-';
        rowData.Status = rowData.Status || '-';

        rowData.SealedBoxList = ko.observableArray([]);
        rowData.OpenBoxList = ko.observableArray([]);

        if (rowData.Locations != null) {
            rowData.MainLocation = rowData.Locations.length > 0 ? rowData.Locations[0] : null;
            rowData.HasLocation = rowData.Locations.length > 0;

            if (rowData.MainLocation != null) {
                rowData.LocationIndex = (parseInt(rowData.MainLocation.Isle) * 1000000000 || 0)
                    + (parseInt(rowData.MainLocation.Section) || 0) * 10000
                    + (parseInt(rowData.MainLocation.Shelf) || 0);
            } else {
                rowData.LocationIndex = null;
            }
        }
    };
    
    self.grid = new FastGridViewModel({            
        gridId: self.gridName,
        rowTemplate: 'style-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        onRedraw: self.onRedraw,
        itemsPerPage: 50,
        isLocalMode: false,
        sortField: 'CreateDate', 
        sortMode: 'desc',
        fields:[ 
            { name: "CreateDate", type: 'date'},
            { name: "RemainingQuantity", type: 'int'},
            { name: "StyleString", type: 'string' },
            { name: "LocationIndex", type: 'int' }
        ],
    });

    var checkbox = "<input class='check_page' type='checkbox' onclick='javascript: styleListVm.toggleAllOnPage(this)' />";

    self.getGridColumns = function() {
        var columns = [
            { title: checkbox, width: "25px" },
            { title: "#", width: "35px" },
            { title: "Style Id", width: "auto" },
            { title: "DS", width: "60px", field: "DropShipperName", sortable: true },
            { title: "Total Qty", width: "115px" },
            { title: "Remaining", width: "120px", field: "RemainingQuantity", sortable: true },
            { title: "Boxes Cost", width: "100px" },
            //{ name: "QtyMode", title: "Qty Mode", width: "100px" },
            //{ name: "Status", title: "Status", width: "105px" },
            { title: "Sizes", width: "105px" },
            { title: "Location", width: "95px", field: "LocationIndex", sortable: true },
            { title: "Create Date", width: "110px", field: "CreateDate", sortable: true },
            { title: "", width: "100px" }
        ];

        console.log("doStyleOperation: " + self.settings.access.doStyleOperation);
        if (self.settings.access.doStyleOperation) {
            columns.push({ title: "", width: "115px" });
            columns.push({ title: "", width: "40px" });
        }

        return columns;
    }

    self.fastGridSettings = {            
        gridId: self.grid.gridId, 
        hierarchy: { enable: true },
        isLocalMode: false,
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: self.getGridColumns(),
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    if (!dataUtils.isEmpty(model.SelectedStyleId)) {
        self.searchFilters.styleId(model.SelectedStyleId);
        self.searchFilters.onlyInStock(false);
    }

    self.grid.read();

    $.subscribe("refresh-style-boxes", function (_, styleId) {
        console.log("refresh-style-boxes, styleId=" + styleId);
        self.updateSealedBoxRows(styleId);
        self.updateOpenBoxRows(styleId);
    });
};