var DescriptionBufferModel = function() {
    var self = this;

    self.styleId = ko.observable("");
    self.styleString = ko.observable("");

    self.description = ko.observable("");
    self.searchTerms = ko.observable("");
    self.bulletPoint1 = ko.observable("");
    self.bulletPoint2 = ko.observable("");
    self.bulletPoint3 = ko.observable("");
    self.bulletPoint4 = ko.observable("");
    self.bulletPoint5 = ko.observable("");

    self.setValues = function(sender) {
        self.styleId(sender.Id);
        self.styleString(sender.StyleString);

        self.description(sender.Description);
        self.searchTerms(sender.SearchTerms);
        self.bulletPoint1(sender.BulletPoint1);
        self.bulletPoint2(sender.BulletPoint2);
        self.bulletPoint3(sender.BulletPoint3);
        self.bulletPoint4(sender.BulletPoint4);
        self.bulletPoint5(sender.BulletPoint5);
    }

    self.getModel = function() {
        return {
            Id: self.styleId(),
            StyleString: self.styleString(),

            Description: self.description(),
            SearchTerms: self.searchTerms(),
            BulletPoint1: self.bulletPoint1(),
            BulletPoint2: self.bulletPoint2(),
            BulletPoint3: self.bulletPoint3(),
            BulletPoint4: self.bulletPoint4(),
            BulletPoint5: self.bulletPoint5(),
        };
    }

    self.erase = function() {
        self.styleId(null);
        self.styleString(null);

        self.description(null);
        self.searchTerms(null);
        self.bulletPoint1(null);
        self.bulletPoint2(null);
        self.bulletPoint3(null);
        self.bulletPoint4(null);
        self.bulletPoint5(null);
    }

    self.isEmpty = ko.computed(function() {
        return dataUtils.isEmpty(self.styleId());
    });
};


var StyleDescriptionListViewModel = function (model, settings) {
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
        keywords: ko.observable(''),
        idList: null,
        gender: ko.observable(""),
        mainLicense: ko.observable(""),
        subLicense: ko.observable(""),
        itemStyles: ko.observable([]),
        sleeves: ko.observable([]),
    };
        
    self.searchFilters.styleId.subscribe(function() {
        console.log('redrawAll');
        self.search();
    });

    self.searchFilters.genderList = model.GenderList;
    self.searchFilters.mainLicenseList = model.MainLicenseList;
    self.searchFilters.allSubLicenseList = model.SubLicenseList;
    self.searchFilters.itemStyleList = model.ItemStyleList;
    self.searchFilters.sleeveList = model.SleeveList;

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

    self.editStyleDescription = function (sender, styleId) {
        popupWindow.initAndOpenWithSettings({
            content: self.settings.urls.editStyleDescription + "?styleId=" + styleId,
            title: "Edit Style Description",
            width: 1000,
            //customAction: self.onPopupCustomAction,
            submitSuccess: function (result) {
                self.prepareRow(result.Row);
                self.grid.updateRowField(result.Row, result.UpdateFields);
            }
        });
    };

    self.buffer = new DescriptionBufferModel();

    self.copyDescription = function (sender, styleId) {
        console.log("copyDescription, styleId=" + styleId);
        var row = self.grid.getRowDataById(styleId);

        self.buffer.setValues(row);
        $.each(self.grid.items, function(i, row) {
            row.canCopy = false;
            row.canPaste = self.buffer.styleId() != row.Id;
        });
        self.grid.refresh();
    };

    self.cancelCopyDescription = function () {
        console.log("cancelCopyDescription");
        self.buffer.erase();
        $.each(self.grid.items, function (i, row) {
            row.canCopy = true;
            row.canPaste = false;
        });
        self.grid.refresh();
    };

    self.pasteDescription = function (sender, styleId) {
        console.log("pasteDescription, styleId=" + styleId);

        helper.ui.showLoading(sender);

        var data = self.buffer.getModel();
        data.toStyleId = styleId;

        console.log(self.settings.urls.setStyleDescription);

        $.ajax({
            url: self.settings.urls.setStyleDescription,
            data: {
                toStyleId: styleId,
                fromStyleId: data.Id,
            },
            type: "POST",
            success: function (result) {
                console.log("success setDescription");

                helper.ui.hideLoading(sender);

                if (result.IsSuccess) {
                    self.prepareRow(result.Data);
                    self.grid.updateRow(result.Data);
                }
            }
        });
    }
    
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
            if (!dataUtils.isEmpty(self.searchFilters.styleId()))
                self.grid.refresh();

            console.log('GetIdListByFilters begin, time=' + performance.now());
            $.ajax({
                url: self.settings.urls.getIdListByFilters,
                data: {
                    gender: self.searchFilters.gender(),
                    itemStyles: self.searchFilters.itemStyles(),
                    sleeves: self.searchFilters.sleeves(),
                    mainLicense: self.searchFilters.mainLicense(),
                    subLicense: self.searchFilters.subLicense(),
                    hasInitialQty: false,
                },
                success: function (result) {
                    console.log('GetIdListByFilters end, time=' + performance.now());
                    self.searchFilters.idList = result.Data;
                    self.grid.refresh();
                }
            });
        });
    };

    self.clear = function() {
        self.searchFilters.styleId('');
        self.searchFilters.keywords('');
        self.searchFilters.idList = null;

        self.searchFilters.gender("");
        self.searchFilters.mainLicense("");
        self.searchFilters.subLicense("");
        self.searchFilters.itemStyles("");
        self.searchFilters.sleeves("");

        self.grid.refresh();
    };
    
    self.filterCallback = function(row) {
        if (self.searchFilters.styleId() != null && self.searchFilters.styleId() != '') {
            var reg = new RegExp(self.searchFilters.styleId(), 'i');
            if (row != null && row.StyleString != null && row.StyleString != '') {
                if (reg.test(row.StyleString))
                    return true;
            }

            return false;
        }

        var pass = true;

        if (!dataUtils.isEmpty(self.searchFilters.keywords())) {
            var reg = new RegExp(dataUtils.trim(self.searchFilters.keywords()), 'i');
            var isMatch = reg.test(row.StyleString || '')
                || reg.test(row.Name || '')
                || reg.test(row.Description || '')
                || reg.test(row.SearchTerms || '')
                || reg.test(row.BulletPoint1 || '')
                || reg.test(row.BulletPoint2 || '')
                || reg.test(row.BulletPoint3 || '')
                || reg.test(row.BulletPoint4 || '')
                || reg.test(row.BulletPoint5 || '');

            pass = pass && isMatch;
        }

        if (self.searchFilters.idList != null) {
            pass = pass && self.searchFilters.idList.indexOf(row.Id) >= 0;
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

    self.prepareRow = function(rowData) {
        rowData.Name = rowData.Name || '-';

        rowData.canCopy = self.buffer.isEmpty();
        rowData.canPaste = !self.buffer.isEmpty() && self.buffer.styleId() != rowData.Id;
    };
    
    self.grid = new FastGridViewModel({            
        gridId: 'StyleListGrid',
        rowTemplate: 'style-row-template',
        getItemsAsync: self.getItemsAsync,
        filterCallback: self.filterCallback,
        itemsPerPage: 50,
        sortField: 'RemainingQuantity',
        sortMode: 'desc',
        fields:[ 
            { name: "CreateDate", type: 'date'},
            { name: "StyleString", type: 'string' },
            { name: "RemainingQuantity", type: 'int' },
        ],
    });

    self.fastGridSettings = {            
        gridId: self.grid.gridId, 
        hierarchy: { enable: false },
        sort: { field: self.grid.sortField, mode: self.grid.sortMode },
        columns: [ 
            { title: "#", width: "25px" },
                { title: "Style Id", width: "300" },
                { title: "Brand", width: "200" },
                { title: "Remaining", width: "100", field: "RemainingQuantity", sortable: true },
                { title: "Description", width: "auto" },
                { title: "Bullet Points", width: "300px" },
                { title: "Create Date", width: "110px", field: "CreateDate", sortable: true },
                { title: "", width: "130px" },
        ],
        loadingStatus: self.grid.loadingStatus,
        itemCount: self.grid.itemCount,
    };

    self.grid.read().done(function() {
        
    });
};