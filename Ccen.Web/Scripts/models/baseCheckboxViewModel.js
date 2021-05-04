(function (ko, undefined) {
    ko.BaseCheckboxViewModel = function (gridId, idFieldName) {
        var self = this;

        self.gridId = gridId;
        self.grid = $(gridId).data("kendoGrid");

        self.checkedListCache = [];
        self.checkedCount = ko.observable(0);

        self.calcChecked = function() {
            var count = 0;
            $.each(self.checkedListCache, function (i, v) {
                console.log(v);
                if (v)
                    count++;
            });
            self.checkedCount(count);
        }

        self.checkAllOnAllPages = function (sender) {
            var data = self.grid.dataSource.data();
            var checked = true;
            $.each(data, function (i, row) {
                row.UIChecked = checked;
                self.checkedListCache[row[idFieldName]] = checked;
            });

            $(self.gridId + " tbody input:checkbox:not([disabled])").prop("checked", checked);

            self.calcChecked();
        };

        self.unCheckAllOnAllPages = function (sender) {
            var data = self.grid.dataSource.data();
            var checked = false;
            $.each(data, function (i, row) {
                row.UIChecked = checked;
                self.checkedListCache[row[idFieldName]] = checked;
            });

            $(self.gridId + " tbody input:checkbox:not([disabled])").prop("checked", checked);

            self.calcChecked();
        };



        self.checkOne = function (sender, entityId) {
            var checked = $(sender).is(":checked");
            console.log("checkOne: " + checked + ',id=' + entityId);
            var view = self.grid.dataSource.view();
            var dataItem = $.grep(view, function (item) {
                return item[idFieldName] == entityId;
            });

            if (dataItem.length > 0) {
                dataItem[0].UIChecked = checked;
                console.log(dataItem[0][idFieldName]);
                self.checkedListCache[dataItem[0][idFieldName]] = checked;
            }

            self.calcChecked();
        };

        self.resetAll = function(checkStatus) {
            $(self.gridId + " input:checkbox:not([disabled])").prop("checked", checkStatus);

            var view = self.grid.dataSource.view();
            $.each(view, function (i, row) {
                row.UIChecked = checkStatus;
                self.checkedListCache[row[idFieldName]] = row.UIChecked;
            });

            self.calcChecked();
        }

        self.checkAll = function (sender) {
            var checked = $(sender).is(":checked");
            console.log("checkAll: " + checked);
            console.log($(self.gridId + " tbody input:checkbox:not([disabled])").length);
            $(self.gridId + " tbody input:checkbox:not([disabled])").prop("checked", checked);

            var view = self.grid.dataSource.view();
            $.each(view, function (i, row) {
                row.UIChecked = checked;
                self.checkedListCache[row[idFieldName]] = row.UIChecked;
            });

            self.calcChecked();
        };

        self.getAllChecked = function () {
            var list = [];

            var grid = self.grid;
            var data = grid.dataSource.data();

            $.each(data, function (i, row) {
                if (row.UIChecked)
                    list.push(row[idFieldName]);
            });

            return list;
        };

    };
}(ko));