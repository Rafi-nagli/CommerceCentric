var StyleLiteCountingListViewModel = function(model, settings) {
    var self = this;

    self.model = model;
    self.settings = settings;

    self.isApproveMode = ko.observable(settings.isApproveMode || false);

    self.items = ko.observableArray([]);
    self.rowTotal = ko.computed(function() {
        return self.items.length;
    });
    self.itemsReceiveDate = ko.observable();

    //Search begin
    self.searchFilters = {
        styleId: ko.observable(''),
        countingStatus: ko.observable(''),
        fromRawLocation: ko.observable(''),
        toRawLocation: ko.observable(''),
        hideOldLocations: ko.observable(true),
        hideOutOfStock: ko.observable(true),
        countingStatusList: self.settings.liteCountingStatusList,
        isApproveMode: ko.observable(self.isApproveMode()),
    };
        
    self.searchFilters.styleId.subscribe(function() {
        console.log('redrawAll');
        self.search();
    });

    self.searchFilters.countingStatus.subscribe(function () {
        console.log('redrawAll');
        self.search();
    });

    self.searchFilters.hideOldLocations.subscribe(function () {
        console.log('redrawAll');
        self.search();
    });

    //Search end
    self.onEdit = function (sender, id) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editStyleCountingInfo + "?id=" + id,
            title: "Edit Counting Info",
            width: 400,
            //customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                self.prepareRow(result.Row);
                self.grid.insertRow(result.Row);
            }
        });
    };

    //self.onEdit = function (sender, id) {
    //    var displayRow = self.grid.getRowDataById(id);
    //    displayRow.IsEditMode = true;
    //    displayRow.NewCountingStatus = displayRow.CountingStatus;
    //    displayRow.NewCountingName = displayRow.CountingName;
    //    self.grid.updateRow(displayRow);
    //    self.prepareEdit(displayRow);
    //};

    //self.onSave = function (sender, id) {
    //    console.log("onSave, sender=" + sender + ", id=" + id);
    //    var displayRow = self.grid.getRowDataById(id);
    //    helper.ui.showLoading(sender);

    //    console.log("status: " + displayRow.NewCountingStatus + ", name: " + displayRow.NewCountingName);

    //    $.ajax({
    //        url: self.settings.urls.saveStatus,
    //        type: 'POST',
    //        data: {
    //            Id: id,
    //            CountingStatus: displayRow.NewCountingStatus,
    //            CountingName: displayRow.NewCountingName,
    //        },
    //        cahce: false,
    //        success: function (data) {
    //            helper.ui.hideLoading(sender);

    //            //self.prepareRow(data);
    //            //displayRow.OnHold = data.OnHold;
    //            displayRow.CountingStatus = displayRow.NewCountingStatus;
    //            displayRow.CountingName = displayRow.NewCountingName;
    //            displayRow.IsEditMode = false;
    //            self.grid.updateRow(displayRow);
    //        },
    //    });
    //}

    //self.onCancel = function(sender, id) {
    //    var displayRow = self.grid.getRowDataById(id);
    //    displayRow.IsEditMode = false;
    //    self.grid.updateRow(displayRow);
    //}

    self.setApproveStatus = function(sender, id, styleItemId, status) {
        console.log("onApprove, sender=" + sender + ", styleId=" + id + ", styleItemId=" + styleItemId + ", approveStatus=" + status);
        var displayRow = self.grid.getRowDataById(id);
        helper.ui.showLoading(sender);

        console.log("status: " + displayRow.ApproveStatus + ", name: " + displayRow.ApproveStatusName);

        $.ajax({
            url: self.settings.urls.setApproveStatus,
            type: 'POST',
            data: {
                StyleId: id,
                StyleItemId: styleItemId,
                approveStatus: status
            },
            cahce: false,
            success: function (data) {
                helper.ui.hideLoading(sender);
                var row = data.Row;
                self.prepareRow(row);
                displayRow.StyleItems = row.StyleItems;
                self.grid.updateRow(displayRow);
            },
        });
    }

    //BEGIN Boxes
    self.editStyleLocation = function (sender, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editStyleLocation + "?styleId=" + styleId,
            title: "Edit Style Locations",
            width: 400,
            //customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                self.prepareRow(result.Row);
                self.grid.updateRowField(result.Row, result.UpdateFields);
            }
        });
    };

    self.createSealedBox = function (sender, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createSealedBox + "?styleId=" + styleId + "&isMobileMode=true",
            title: "Sealed Box",
            width: 450,
            submitSuccess: function (data) {
                self.updateSealedBoxRows(styleId);

                //self.prepareRow(data);
                //displayRow.StyleItems = data.StyleItems;
                //self.grid.updateRow(displayRow);
            }
        });
    };

    self.editSealedBox = function (sender, sealedBoxId, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editSealedBox + "?sealedBoxId=" + sealedBoxId + "&isMobileMode=true",
            title: "Sealed Box",
            width: 450,
            submitSuccess: function (result) {
                self.updateSealedBoxRows(styleId);

                //self.prepareRow(data);
                //displayRow.StyleItems = data.StyleItems;
                //self.grid.updateRow(displayRow);
            }
        });
    };

    self.deleteSealedBox = function (sender, sealedBoxId, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        if (confirm('Are you sure you want to delete box?')) {
            helper.ui.showLoading(sender, "deleting...");
            $.ajax({
                url: self.settings.urls.deleteSealedBox,
                data: { id: sealedBoxId },
                success: function () {
                    helper.ui.hideLoading(sender);
                    self.updateSealedBoxRows(styleId);

                    //self.prepareRow(data);
                    //displayRow.StyleItems = data.StyleItems;
                    //self.grid.updateRow(displayRow);
                }
            });
        }
    };

    self.createOpenBox = function (sender, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.createOpenBox + "?styleId=" + styleId + "&isMobileMode=true",
            title: "Open Box",
            width: 450,
            submitSuccess: function (result) {
                console.log('create open box');
                self.updateOpenBoxRows(styleId);

                //self.prepareRow(data);
                //displayRow.StyleItems = data.StyleItems;
                //self.grid.updateRow(displayRow);
            }
        });
    };

    self.editOpenBox = function (sender, openBoxId, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        popupWindow.initAndOpenWithSettings(
            {
                content: self.settings.urls.editOpenBox + "?openBoxId=" + openBoxId + "&isMobileMode=true",
                title: "Open Box",
                width: 450,
                submitSuccess: function (result) {
                    self.updateOpenBoxRows(styleId);

                    //self.prepareRow(data);
                    //displayRow.StyleItems = data.StyleItems;
                    //self.grid.updateRow(displayRow);
                }
            });
    };

    self.deleteOpenBox = function (sender, openBoxId, styleId) {
        var displayRow = self.grid.getRowDataById(styleId);

        if (confirm('Are you sure you want to delete box?')) {
            helper.ui.showLoading(sender, "deleting...");
            $.ajax({
                url: self.settings.urls.deleteOpenBox,
                data: { id: openBoxId },
                success: function (result) {
                    helper.ui.hideLoading(sender);
                    self.updateOpenBoxRows(styleId);

                    //self.prepareRow(data);
                    //displayRow.StyleItems = data.StyleItems;
                    //self.grid.updateRow(displayRow);
                }
            });
        }
    };


    self.refreshChildsFor = function (id) {
        self.updateOpenBoxRows(id);
        self.updateSealedBoxRows(id);
    }

    self.updateOpenBoxRows = function (styleId) {
        var loadingTagId = "OpenBox_Loading_" + styleId;
        $("#" + loadingTagId).show();

        $.ajax({
            cache: false,
            url: self.settings.urls.getOpenBoxes + '?styleId=' + styleId,
            success: function (result) {
                console.log("end getall open, time=" + performance.now());
                var tagId = "OpenBox_" + styleId;
                $("#" + tagId).html('');

                var compile = tmpl.compileT('style-openbox-row-template');
                document.getElementById(tagId).innerHTML = kendo.render(compile, result.Data);

                $("#" + loadingTagId).hide();
            }
        });
    };

    self.updateSealedBoxRows = function (styleId) {
        var loadingTagId = "SealedBox_Loading_" + styleId;
        $("#" + loadingTagId).show();
        $.ajax({
            cache: false,
            url: self.settings.urls.getSealedBoxes + '?styleId=' + styleId,
            success: function (result) {
                console.log("end getall sealed, time=" + performance.now());
                var tagId = "SealedBox_" + styleId;
                $("#" + tagId).html('');

                var compile = tmpl.compileT('style-sealedbox-row-template');
                document.getElementById(tagId).innerHTML = kendo.render(compile, result.Data);

                $("#" + loadingTagId).hide();
            }
        });
    };

    self.toggleGridRow = function (id) {
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

    //END Boxes
    
    self.searchByKeyCmd = function(data, event) {
        console.log('searchByKeyCmd');
        if (event.keyCode == 13)
            self.search(true);
        return true;
    };

    self.checkUpdates = function() {
        var defer = $.Deferred();
        if (self.itemsReceiveDate() != null) //NOTE: Skip any change requests if loading not complete
        {
            console.log("itemsReceiveDate=" + self.itemsReceiveDate());
            $.ajax({
                url: self.settings.urls.getAllUpdates,
                data: { fromDate: self.itemsReceiveDate() },
                success: function (result) {
                    for (var i = 0; i < result.Items.length; i++) {
                        var item = result.Items[i];
                        self.prepareRow(item);
                        self.grid.updateRow(item);
                    }

                    console.log("receiveDate=" + result.GenerateDate);
                    self.itemsReceiveDate(result.GenerateDate);

                    defer.resolve();
                }
            });
        } else {
            console.log("itemsReceiveDate=null");
            defer.resolve();
        }

        return defer;
    }

    self.search = function () {
        self.checkUpdates().done(function() {
            self.grid.refresh();
        });
    };

    self.clear = function() {
        self.searchFilters.styleId('');
        
        self.grid.refresh();
    };

    self.refreshChildsFor = function (id) {
        self.updateOpenBoxRows(id);
        self.updateSealedBoxRows(id);
    }
    
    //self.toggleGridRow = function(id) {
    //    console.log('toggleGridRow, id=' + id);
    //    var tagId = '#Detail_' + id;
    //    var visible = $(tagId).is(":visible");
    //    if (!visible) {
    //        console.log('begin get boxes');
    //        $(tagId).show();

    //        self.updateSealedBoxRows(id);
    //        self.updateOpenBoxRows(id);
    //    } else {
    //        $(tagId).hide();
    //    }
    //};

    self.filterCallback = function (row) {
        var pass = true;
        if (self.searchFilters.isApproveMode()) {
            var anyHaveCountingStatus = $.grep(row.StyleItems, function (i) {
                return !dataUtils.isNullOrEmpty(i.CountingStatus);
            }).length > 0;

            if (Math.abs(row.TotalQuantity - row.TotalRemaining) < 10
                || !anyHaveCountingStatus)
                pass = false;
        }

        if (self.searchFilters.styleId() != null && self.searchFilters.styleId() != '')
        {
            var reg = new RegExp(self.searchFilters.styleId(), 'i');
            if (row != null && row.StyleId != null && row.StyleId != '') {
                if (!reg.test(row.StyleId))
                    pass = false;
            }
        }

        if (!dataUtils.isNullOrEmpty(self.searchFilters.countingStatus())) {
            if (row != null && row.CountingStatus != self.searchFilters.countingStatus())
                pass = false;
        }

        if (!dataUtils.isNullOrEmpty(self.searchFilters.fromRawLocation())) {
            var passLocation = false;
            if (row != null && row.Locations != null) {
                for (var i = 0; i < row.Locations.length; i++) {
                    if (row.Locations[i].Isle >= self.searchFilters.fromRawLocation())
                        passLocation = true;
                }
            }
            pass = pass && passLocation;
        }

        if (!dataUtils.isNullOrEmpty(self.searchFilters.toRawLocation())) {
            var passLocation = false;
            if (row != null && row.Locations != null) {
                for (var i = 0; i < row.Locations.length; i++) {
                    if (row.Locations[i].Isle <= self.searchFilters.toRawLocation())
                        passLocation = true;
                }
            }
            pass = pass && passLocation;
        }

        if (self.searchFilters.hideOldLocations()) {
            if (row.Locations.length > 0) {
                pass = pass && (row.Locations[0].Isle > 100);
            }
        }

        if (self.searchFilters.hideOutOfStock()) {
            pass = pass && (row.TotalRemaining > 0);
        }

        return pass;
    };

    self.getItemsAsync = function(params) {
        var defer = $.Deferred();
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
                self.itemsReceiveDate(result.GenerateDate);

                console.log("getAllAsync end: " + result.Items.length);
                defer.resolve(result);
            }
        });
        return defer;
    };

    self.prepareEdit = function (row) {
        console.log("prepareEdit, rowId=" + row.Id);
        $('tr[row-uid="' + row.Id + '"]').find('.edit-counting-status').width(120).kendoDropDownList(
        {
            "change": function () {
                var ind = this.selectedIndex;
                var value = this.dataItem(ind).Value;
                row.NewCountingStatus = value;
            },
            value: row.CountingStatus,
            "dataSource": self.settings.liteCountingStatusList,
            enabled: true,
            "dataTextField": "Text",
            "dataValueField": "Value"
        });

        $('tr[row-uid="' + row.Id + '"]').find('.edit-counting-name').width(120).kendoDropDownList(
        {
            "change": function () {
                var ind = this.selectedIndex;
                var value = this.dataItem(ind).Value;
                row.NewCountingName = value;
            },
            value: row.CountingName,
            "dataSource": self.settings.liteCountingByNameList,
            enabled: true,
            "dataTextField": "Text",
            "dataValueField": "Value"
        });
    }

    self.prepareRow = function(rowData) {
        rowData.Name = rowData.Name || '-';

        rowData.IsEdit = false;

        //rowData.SealedBoxList = ko.observableArray([]);
        //rowData.OpenBoxList = ko.observableArray([]);

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

    var sortField = self.isApproveMode() ? "LocationIndex" : "ApprovedStatus";
    
    self.grid = new FastGridViewModel({            
        gridId: 'StyleListGrid',
        rowTemplate: 'style-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        itemsPerPage: 50,
        sortField: sortField, 
        sortMode: 'asc',
        fields:[ 
            { name: "CreateDate", type: 'date'},
            { name: "StyleString", type: 'string' },
            { name: "LocationIndex", type: 'int' },
            { name: "Sizes", type: 'int' },
            { name: "CountingStatus", type: 'string' },
            { name: "ApprovedStatus", type: 'int' }
            //{ name: "CountingName", type: 'string' }
        ],
    });

    var columns = [
        { title: "#", width: "25px" },
        { title: "Style Id", width: "auto" },
        { title: "Location", width: "200px", field: "LocationIndex", sortable: true },
        { title: "Sizes", width: "290px", field: "TotalRemaining", sortable: true },
        //{ title: "Counting Status", width: "135px", field: "CountingStatus", sortable: true },
        //{ title: "By Name", width: "135px", field: "By Name", sortable: true },
        { title: "", width: "220px" },
    ];

    //if (self.isApproveMode()) {
    //    console.log("approve mode");
    //    columns.splice(4, 0, {
    //        title: "",
    //        width: "120px",
    //        filed: 'ApprovedStatus',
    //        sortable: true
    //    });
    //}


    self.fastGridSettings = {            
        gridId: self.grid.gridId, 
        hierarchy: { enable: true },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: columns,
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    self.grid.read().done(function() {
        
    });
};