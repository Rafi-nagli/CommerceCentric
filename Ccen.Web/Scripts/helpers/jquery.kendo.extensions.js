(function ($) {

    $.fn.kDisable = function() {
        $(this).prop('disabled', true).addClass('k-state-disabled');
    };

    $.fn.kEnable = function() {
        $(this).prop('disabled', false).removeClass('k-state-disabled');
    };

    $.fn.showLoading = function (text) {
        var sender = $(this);

        if (sender == null || sender.length == 0)
            return;

        if (text == null)
            text = "updating...";

        if ($(sender).prop("tagName") != 'BUTTON') { //CASE: with span into button
            if ($(sender).parent().prop("tagName") == 'BUTTON')
                sender = $(sender).parent();
        }
        $(sender).prop('disabled', true).addClass('k-state-disabled');
        $(sender).after("<div class='loading'>" + text + "</div>");
    };

    $.fn.hideLoading = function () {
        var sender = $(this);

        if (sender == null || sender.length == 0)
            return;

        if ($(sender).prop("tagName") != 'BUTTON') { //CASE: with span into button
            if ($(sender).parent().prop("tagName") == 'BUTTON')
                sender = $(sender).parent();
        }

        $(sender).prop('disabled', false).removeClass('k-state-disabled');
        var loading = $(sender).next();
        if (loading != null && loading.hasClass('loading')) {
            loading.remove();
        }
    };

    $.fn.refreshRow = function (rowData, updateFields, forseGridRefresh, columnNumberOffset) {
        console.log("refreshRow");
        if (columnNumberOffset == null)
            columnNumberOffset = 1; //NOTE: not works replace 0 to 1: columnNumberOffset || 1

        var grid = this.data("kendoGrid");

        if (grid != null) {
            if (rowData == null || forseGridRefresh) {
                console.log("refresh grid");
                grid.dataSource.read();
                return;
            }
        }

        var idField = grid.dataSource.options.schema.model.id;
        console.log(idField);
        console.log(rowData[idField]);
        
        //Update dataItem
        var dataItem = grid.dataSource.get(rowData[idField]);

        var row = null;
        if (dataItem == null) { //When insert
            console.log("refresh grid (empty dataItem)");
            grid.dataSource.read();
        } else {
            console.log("refresh row");

            if (updateFields != null) {
                for (var i = 0; i < updateFields.length; i++) {
                    setFieldValue(dataItem, rowData, updateFields[i]);
                }
            } else {
                for (var prop in rowData) {
                    setFieldValue(dataItem, rowData, prop);
                }
            }

            row = grid.tbody.find('tr[data-uid="' + dataItem.uid + '"]');
            //if (row != null)
            //    grid.select(row);
            
            //Redraw row
            if (row.length > 0) //Displayed on active page
                helper.ui.kendoFastRedrawRow(grid, dataItem, row, null, columnNumberOffset);
        }
    };

    function setFieldValue(dataItem, data, prop) {
        console.log(prop);
        if (dataItem.fields[prop] != null) {
            //if (dataItem.fields[prop].editable == true) {
            console.log("set property=" + prop + ", value=" + data[prop]);
            //dataItem.set(prop, rowData[prop]);
            dataItem[prop] = data[prop];
            //}
        }
    }

}(jQuery));