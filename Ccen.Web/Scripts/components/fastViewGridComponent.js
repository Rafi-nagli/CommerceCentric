ko.components.register('fast-view-grid', {
    synchronous: true,
    viewModel: function (params) {
        var self = this;

        self.itemCount = params.itemCount;
        self.loadingStatus = params.loadingStatus;

        self.hasHierarchy = params.hierarchy.enable;
        self.hierarchyCallback = params.hierarchy.callback;

        self.itemsTagId = params.gridId;

        self.noItems = ko.computed(function () {
            return self.itemCount() == 0;
        });

        self.sort = params.sort;
        //Column processing
        self.columns = params.columns;

        if (self.hasHierarchy) {
            self.columns.unshift({
                isHierarchy: true,
                width: '25px',
                title: '&nbsp;',
                sort: false
            });
        }

        for (var i = 0; i < self.columns.length; i++) {
            var column = self.columns[i];

            column.isHierarchy = column.isHierarchy || false;
            column.sortable = column.sortable || false;
            column.isAsc = ko.pureComputed(function () {
                return this.field == self.sort.field() && self.sort.mode() == 'asc';
            }, column);
            column.isDesc = ko.pureComputed(function () {
                return this.field == self.sort.field() && self.sort.mode() == 'desc';
            }, column);
            column.sort = function () {
                console.log('sort, mode=' + self.sort.mode() + ', field=' + self.sort.field());
                console.log(this);
                if (this.field == null)
                    return;

                if (this.field != self.sort.field()) {
                    self.sort.field(this.field);
                    self.sort.mode('desc');
                    console.log('set desc - ' + this.field);
                    return;
                }

                if (self.sort.mode() == 'asc') {
                    self.sort.mode(null);
                    return;
                }
                if (self.sort.mode() == 'desc') {
                    self.sort.mode('asc');
                    return;
                }
                if (self.sort.mode() == null) {
                    self.sort.mode('desc');
                    return;
                }
            }
        };

        self.isLoading = ko.computed(function () {
            return self.loadingStatus() == 1;
        });
    },
    template: { element: 'fast-grid-template' },
});


var FastViewGridViewModel = function (settings) {
    var NOTLOADED = 0;
    var LOADING = 1;
    var LOADED = 2;

    var self = this;

    self.fields = settings.fields || [];
    self.gridId = settings.gridId;
    self.rowTemplate = settings.rowTemplate;
    self.getItemsAsync = settings.getItemsAsync;

    //Events
    self.onRedraw = settings.onRedraw;

    self.selectedId = ko.observable(null);

    self.items = [];

    self.pageIndex = ko.observable(1);
    self.pageIndex.subscribe(function () {
        console.log('viewmodel pageIndex changed');
        self.loadingStatus(LOADING);
        self.requestShowItems();
        setTimeout(function () {
            self.loadingStatus(LOADED);
        });
    });
    self.itemsPerPage = settings.itemsPerPage;
    self.itemCount = ko.observable(0);

    self.sortField = ko.observable(settings.sortField);
    self.sortField.subscribe(function () {
        console.log('viewmodel sortField changed');
        self.refresh();
    });
    self.sortMode = ko.observable(settings.sortMode);
    self.sortMode.subscribe(function () {
        console.log('viewmodel sortMode changed');
        self.refresh();
    });

    self.loadingStatus = ko.observable(NOTLOADED);

    self.getOptions = function () {
        return {
            Page: self.pageIndex(),
            ItemsPerPage: self.itemsPerPage,
            SortField: self.sortField(),
            SortMode: self.sortMode()
        }
    }

    self.requestShowItems = function () {
        console.log(self.gridId + ": begin getAll, time=" + performance.now());
        self.loadingStatus(LOADING);
        return self.getItemsAsync(self.getOptions()).done(function (result) {
            console.log(self.gridId + ": end getAll, time=" + performance.now());
            self.items = result.Items;
            self.itemCount(result.TotalCount);

            self.redrawAll();
            setTimeout(function () {
                self.loadingStatus(LOADED);
            });
        }).fail(function (result) {
            self.loadingStatus(NOTLOADED);
        });
    };

    self.getRowNodeById = function (id) {
        var rows = $("#" + self.gridId).find("tr[row-uid=" + id + "]");
        return rows;
    };

    self.getRowDataById = function (id) {
        for (var i = 0; i < self.items.length; i++) {
            if (self.items[i].Id == id)
                return self.items[i];
        }
        return null;
    };

    self.getRowIndexById = function (id) {
        for (var i = 0; i < self.items.length; i++) {
            if (self.items[i].Id == id)
                return i;
        }
        return -1;
    };

    self.setInitialPage = function () {
        if (self.selectedId() != null) {
            var page = self.getPageNumberById(self.selectedId());
            console.log("setInitialPage, page=" + page);
            self.pageIndex(page);
        }
    };

    self.clear = function () {
        self.items = [];
        self.itemCount(0);

        self.setInitialPage();

        self.redrawAll();
    }

    self.reload = function () {
        self.clear();
        self.requestShowItems();
    }

    self.refresh = function () {
        self.pageIndex(1);
        self.requestShowItems();
    };

    self.redrawAll = function () {
        console.log(self.gridId + ": begin redrawAll, time=" + performance.now());
        var tagId = self.gridId;
        $("#" + tagId).html('');

        var compile = tmpl.compileT(self.rowTemplate);

        var pageItems = self.items;
        self.setAltRows(pageItems);
        document.getElementById(tagId).innerHTML = kendo.render(compile, pageItems);

        console.log(self.gridId + ": end redrawAll, time=" + performance.now());

        if (self.onRedraw != null) {
            self.onRedraw(pageItems);
        }
    };

    self.setAltRows = function (items) {
        for (var i = 0; i < items.length; i++) {
            items[i]._isAlt = i % 2 == 1;
        }
    }

    self.insertRow = function (row) {
        if (row.Id == 0)
            throw new Error('row.Id == 0');
        self.items.unshift(row);
        self.redrawRow(row, true);

        self.itemCount(self.itemCount() + 1);
    };

    self.deleteRow = function (id) {
        var index = self.getRowIndexById(id);
        if (index >= 0) {
            self.items.splice(index, 1);
            var row = self.getRowNodeById(id);
            row.remove();

            self.itemCount(self.itemCount() - 1);

            console.log(self.gridId + ": row removed, id=" + id);
        }
    };

    self.updateRow = function (row) {
        for (var i = 0; i < self.items.length; i++) {
            if (self.items[i].Id == row.Id) {
                self.items[i] = row;
                self.redrawRow(self.items[i], false);
                break;
            }
        }
    };

    self.updateRowField = function (row, fieldList) {
        if (fieldList == null) {
            self.updateRow(row);
        } else {
            var index = self.getRowIndexById(row.Id);
            var rowData = self.items[index];
            for (var i = 0; i < fieldList.length; i++) {
                console.log(fieldList[i]);
                rowData[fieldList[i]] = row[fieldList[i]];
            }
            self.redrawRow(rowData, false);
        }
    };

    self.selectRow = function (id) {
        var defer = $.Deferred();

        var index = self.getRowIndexById(id);
        console.log("selectRow, index=" + index);

        if (index > 0) {
            setTimeout(function () {
                var row = self.getRowNodeById(id);

                //animate scroll
                if (row != null) {
                    console.log(row);
                    $('html, body').scrollTop(row.offset().top - 200); // = row.offset().top;
                }

                defer.resolve(index);
            });
        } else {
            defer.reject();
        }

        return defer;
    };

    self.redrawRow = function (row, insertable) {
        var rowNode = self.getRowNodeById(row.Id);
        var compile = null;
        var hasChanges = false;
        if (rowNode.length == 0) {
            if (insertable) {
                console.log("insert new row");
                compile = tmpl.compileT(self.rowTemplate);
                $("#" + self.gridId).prepend(compile(row));
                hasChanges = true;
            }
        }
        else {
            console.log("redraw row");
            compile = tmpl.compileT(self.rowTemplate);
            rowNode.next().remove(); //Also remove Detailed view
            rowNode.replaceWith(compile(row));
            hasChanges = true;
        }

        if (self.onRedraw != null && hasChanges) {
            self.onRedraw([row]);
        }
    };
}
