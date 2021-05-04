var StyleCountingListViewModel = function(model, settings) {
    var self = this;

    self.model = model;
    self.settings = settings;

    self.items = ko.observableArray([]);
    self.rowTotal = ko.computed(function() {
        return self.items.length;
    });
    self.itemsReceiveDate = ko.observable();

    //Search begin
    self.searchFilters = {
        styleId: ko.observable(''),
    };
        
    self.searchFilters.styleId.subscribe(function() {
        console.log('redrawAll');
        self.search();
    });

    //Search end
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

    self.filterCallback = function(row) {
        if (self.searchFilters.styleId() != null && self.searchFilters.styleId() != '')
        {
            var reg = new RegExp(self.searchFilters.styleId(), 'i');
            if (row != null && row.StyleId != null && row.StyleId != '') {
                if (reg.test(row.StyleId))
                    return true;
            }
                
            return false;
        }

        return true;
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

    self.prepareRow = function(rowData) {
        rowData.Name = rowData.Name || '-';

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
        gridId: 'StyleListGrid',
        rowTemplate: 'style-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        itemsPerPage: 50,
        sortField: 'CreateDate', 
        sortMode: 'desc',
        fields:[ 
            { name: "CreateDate", type: 'date'},
            { name: "StyleString", type: 'string' },
            { name: "LocationIndex", type: 'int' },
            { name: "SealedBoxQuantity", type: 'int' },
            { name: "OpenBoxQuantity", type: 'int' }
        ],
    });

    self.fastGridSettings = {            
        gridId: self.grid.gridId, 
        hierarchy: { enable: true },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: [ 
            { title: "#", width: "25px" },
                { title: "Style Id", width: "auto" },
                { title: "Location", width: "200px", field: "LocationIndex", sortable: true },
                { title: "Sealed Box Qty", width: "120px", field: "SealedBoxQuantity", sortable: true },
                { title: "Open Box Qty", width: "120px", field: "OpenBoxQuantity", sortable: true },
                { title: "", width: "300px" },
        ],
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    self.grid.read().done(function() {
        
    });
};