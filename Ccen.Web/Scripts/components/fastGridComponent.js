ko.components.register('fast-grid', {
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


var FastGridViewModel = function (settings) {
    var NOTLOADED = 0;
    var LOADING = 1;
    var LOADED = 2;

    var self = this;

    self.fields = settings.fields || [];
    self.gridId = settings.gridId;
    self.rowTemplate = settings.rowTemplate;
    self.getItemsAsync = settings.getItemsAsync;
    self.filterCallback = settings.filterCallback;
    self.isLocalMode = settings.isLocalMode;
    if (self.isLocalMode != false)
        self.isLocalMode = true;

    console.log("isLocalMode=" + self.isLocalMode);

    //Events
    self.onRedraw = settings.onRedraw;

    self.selectedId = ko.observable(null);

    self.isFirstLoaded = ko.observable(false);

    self.lastRequestTimeStamp = null;
    self.items = [];
    self.filteredItems = [];
    self.pageIndex = ko.observable(1);
    self.pageIndex.subscribe(function () {
        console.log('viewmodel pageIndex changed');

        if (!self.isLocalMode) {
            self.read(self.getOptions());
        } else {
            self.loadingStatus(LOADING);
            self.redrawAll();
            setTimeout(function () {
                self.loadingStatus(LOADED);
            });
        }
    });
    self.itemsPerPage = settings.itemsPerPage;
    self.itemCount = ko.observable(0);

    self.sortField = ko.observable(settings.sortField);
    self.sortField.subscribe(function () {
        console.log('viewmodel sortField changed, isLocal=' + self.isLocalMode);
        if (self.isLocalMode)
            self.refresh();
        else
            self.read(self.getOptions());
    });
    self.sortMode = ko.observable(settings.sortMode);
    self.sortMode.subscribe(function () {
        console.log('viewmodel sortMode changed, isLocal=' + self.isLocalMode);
        if (self.isLocalMode)
            self.refresh();
        else
            self.read(self.getOptions());
    });

    self.loadingStatus = ko.observable(NOTLOADED);

    self.sort = function () {
        console.log(self.sortField() + '-' + self.sortMode());
        var sortDir = self.sortMode();
        var sortField = self.sortField();
        var field = $.grep(self.fields, function (f) { return f.name == sortField; })[0];
        self.items.sort(function (item1, item2) {
            var val1 = null;
            var val2 = null;

            if (sortDir != null) {
                val1 = item1[field.name];
                val2 = item2[field.name];
            } else {
                val1 = item1['Id'];
                val2 = item2['Id'];
            }
            //if (field.type == 'string') {
            //    val1 = item1[field.name];
            //    val2 = item2[field.name];
            //}

            //if (field.type == 'int') {
            //    val1 = parseInt(item1[field.name]);
            //    val2 = parseInt(item2[field.name]);
            //}

            if (val1 == val2)
                return 0;

            if (val1 == null)
                return 1;

            if (val2 == null)
                return -1;

            if (sortDir == 'asc')
                return val1 > val2 ? 1 : -1;
            if (sortDir == 'desc' || sortDir == null)
                return val1 < val2 ? 1 : -1;
        });
    };

    self.filtering = function () {
        self.filteredItems = [];
        if (self.filterCallback != null) {
            for (var i = 0; i < self.items.length; i++) {
                if (self.filterCallback(self.items[i], i, self.filteredItems.length))
                    self.filteredItems.push(self.items[i]);
            }
            self.itemCount(self.filteredItems.length);
        } else {
            self.filteredItems = self.items;
        }
    };

    self.getOptions = function (userOptions) {
        var gridOptions = {
            Page: self.pageIndex(),
            ItemsPerPage: self.itemsPerPage,
            SortField: self.sortField(),
            SortMode: self.sortMode(),
            TimeStamp: Date.now(),
        };
        var params = gridOptions;
        if (userOptions != null)
            params = $.extend(params, userOptions);
        console.log("Params");
        console.log(params);
        return params;
    }

    self.setItems = function (items, totalCount) {
        self.items = items;
        self.itemCount(totalCount);
        self.isFirstLoaded(true);
    }

    self.showLoading = function () {
        self.loadingStatus(LOADING);
    }

    self.hideLoading = function () {
        self.loadingStatus(LOADED);
    }

    self.read = function (userOptions) {
        console.log(self.gridId + ": begin getAll, time=" + performance.now() + ", options=" + userOptions);
        self.loadingStatus(LOADING);
        return self.getItemsAsync(self.getOptions(userOptions)).done(function (result) {
            console.log(self.gridId + ": end getAll, time=" + performance.now());

            if (result.RequestTimeStamp != null) {
                if (self.lastRequestTimeStamp != null
                    && result.RequestTimeStamp < self.lastRequestTimeStamp) {
                    //Skip all updates, the request already is late
                    setTimeout(function () {
                        self.loadingStatus(LOADED);
                    });
                    console.log("result, skip response");
                    return;
                }
            }

            console.log("result, requestTimeStamp: " + result.RequestTimeStamp);

            self.lastRequestTimeStamp = result.RequestTimeStamp;

            self.setItems(result.Items, result.TotalCount);

            self.sort();
            self.filtering();

            self.setInitialPage();

            self.redrawAll();
            setTimeout(function () {
                self.loadingStatus(LOADED);
            });
        }).fail(function (result) {
            self.loadingStatus(NOTLOADED);
        });
    };

    self.getRowNodesById = function (id) {
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

    self.getFilteredRowIndexById = function (id) {
        for (var i = 0; i < self.filteredItems.length; i++) {
            if (self.filteredItems[i].Id == id)
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

    self.getPageNumberById = function (id) {
        var index = self.getFilteredRowIndexById(id);
        if (index > 0)
            return Math.ceil((index + 1) / self.itemsPerPage);
        return 1;
    };

    self.clear = function () {
        self.items = [];
        self.itemCount(0);
        self.sort();
        self.filtering();

        self.setInitialPage();

        self.redrawAll();
    }

    self.reload = function () {
        self.clear();
        self.read();
    }

    self.refresh = function () {
        self.sort();
        self.filtering();
        self.pageIndex(1);
        self.redrawAll();
    };

    self.pageItems = [];
    self.redrawAll = function () {
        console.log(self.gridId + ": begin redrawAll, time=" + performance.now());
        var tagId = self.gridId;
        $("#" + tagId).html('');

        var compile = tmpl.compileT(self.rowTemplate);

        var pageItems = [];
        var minIndex = (self.pageIndex() - 1) * self.itemsPerPage;
        var maxIndex = Math.min(self.itemCount(), self.pageIndex() * self.itemsPerPage);
        if (self.isLocalMode) {
            pageItems = self.filteredItems.slice(minIndex, maxIndex);
        } else {
            pageItems = self.items;
        }
        self.setAltRows(pageItems);
        self.setIndexes(pageItems, minIndex);
        self.pageItems = pageItems;

        var node = document.getElementById(tagId);
        if (node != null)
            node.innerHTML = kendo.render(compile, pageItems);
        else
            console.log(self.gridId + ": redrawAll, no grid node");

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

    self.setIndexes = function (items, minIndex) {
        for (var i = 0; i < items.length; i++) {
            items[i]._pageIndex = minIndex + i + 1;
            items[i]._index = i + 1;
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
            var rows = self.getRowNodesById(id);
            rows.remove();

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

        var index = self.getFilteredRowIndexById(id);
        console.log("selectRow, index=" + index);

        if (index > 0) {
            var page = Math.ceil((index + 1) / self.itemsPerPage);
            self.pageIndex(page);
            setTimeout(function () {
                var rows = self.getRowNodesById(id);

                //animate scroll
                if (rows.length > 0) {
                    console.log(rows);
                    $('html, body').scrollTop($(rows[0]).offset().top - 200); // = row.offset().top;
                }

                defer.resolve(index);
            });
        } else {
            defer.reject();
        }

        return defer;
    };

    self.redrawRow = function (row, insertable) {
        //TODO: check, remove if no needed
        if (self.filteredItems != null
            && self.filteredItems.firstOrDefault(function (n) {
                    return n.Id == row.Id;
        }) == null)
            return;

        var hasChanges = false;
        var compile = null;
        var rowNodes = self.getRowNodesById(row.Id);
        if (rowNodes.length == 0) {
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
            rowNodes.slice(1).remove();//.next().remove(); //Also remove Detailed view
            rowNodes.replaceWith(compile(row));
            hasChanges = true;
        }

        if (self.onRedraw != null && hasChanges) {
            self.onRedraw([row]);
        }
    };
}
