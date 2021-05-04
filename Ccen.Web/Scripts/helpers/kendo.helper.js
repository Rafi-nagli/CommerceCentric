registerNamespace("kendo.helper");

kendo.helper.getRowById = function (grid, idFieldName, id) {
    var gridData = grid.dataSource.data();
    var rows = $.grep(gridData, function(row) {
        return row[idFieldName] == id;
    });
    if (rows.length > 0)
        return rows[0];
    return null;
};
