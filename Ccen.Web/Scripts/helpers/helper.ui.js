registerNamespace("helper.ui");

//http://stackoverflow.com/questions/13613098/refresh-a-single-kendo-grid-row
// Updates a single row in a kendo grid without firing a databound event.
// This is needed since otherwise the entire grid will be redrawn.
helper.ui.kendoFastRedrawRow = function(grid, dataItem, row, excludeColumns, columnNumberingOffset, extendButtons) {
    if ($(row).length == 0)
        return;

    var rowChildren = $(row)[0].cells;
    columnNumberingOffset = columnNumberingOffset || 0;

    console.log("offset=" + columnNumberingOffset);

    for (var i = 0; i < grid.columns.length; i++) {
        var index = i + columnNumberingOffset; 

        if (excludeColumns != null && excludeColumns.indexOf(index) >= 0)
            continue;

        var column = grid.columns[i];
        var template = column.template;
        var cell = $(rowChildren[index]);

        if (template !== undefined) {
            var kendoTemplate = kendo.template(template);

            // Render using template
            cell.html(kendoTemplate(dataItem));
        } else {
            var fieldValue = dataItem[column.field];

            var format = column.format;
            var values = column.values;

            if (values !== undefined && values != null) {
                // use the text value mappings (for enums)
                for (var j = 0; j < values.length; j++) {
                    var value = values[j];
                    if (value.value == fieldValue) {
                        cell.html(value.text);
                        break;
                    }
                }
            } else if (format !== undefined) {
                // use the format
                cell.html(kendo.format(format, fieldValue));
            } else {
                // Just dump the plain old value
                cell.html(fieldValue);
            }
        }
    }
};

helper.ui.highlightRow = function (tr, isHighlight) {
    console.log('highlight: ' + isHighlight);
    if (isHighlight) {
        tr.addClass("am-row-highlight");
    } else {
        tr.removeClass("am-row-highlight");
    }
}

helper.ui.disableRow = function(tr, isDisable, excludeInputIds) {
    if (isDisable) {
        tr.addClass("am-row-disabled");
        tr.find(".k-button").each(function() {
            var b = $(this);
            if (excludeInputIds == null
                || excludeInputIds.indexOf(b.attr('id')) == -1) // this.id != 'holdButton')
                b.attr('disabled', 'disabled').addClass("k-state-disabled");
        });
        tr.find("input").each(function () {
            var b = $(this);
            b.attr('disabled', 'disabled').addClass("k-state-disabled");
        });
    } else {
        tr.removeClass("am-row-disabled");
        tr.find("input").each(function() {
            var cb = $(this);
            cb.removeAttr('disabled', 'disabled').removeClass("k-state-disabled");
        });
        tr.find(".k-button").each(function() {
            var b = $(this);
            if (excludeInputIds == null
                || excludeInputIds.indexOf(b.attr('id')) == -1)  //if (this.id != 'holdButton')
                b.removeAttr('disabled', 'disabled').removeClass("k-state-disabled");
        });
    }
};

helper.ui.addTopPager = function (grid) {
    var wrapper = $('<div class="k-pager-wrap k-grid-pager pagerTop"/>').insertBefore(grid.element.children("table"));
    grid.pagerTop = new kendo.ui.Pager(wrapper, $.extend({}, grid.options.pageable, { dataSource: grid.dataSource }));
    grid.element.height("").find(".pagerTop").css("border-width", "0 0 1px 0");
};


helper.ui.showWarningPopup = function(text) {
    helper.ui.showPopup("WarningPopup", text, 2, null, null, { width: '400px' });
};

helper.ui.showPopup = function (windowId,
    html,
    type,
    yesCallback,
    closeCallback,
    extendOptions)
{
    type = type || 0;

    var buttonsHtml = "";
    var title = "Info";

    var closeHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnClose\" class=\"close-button k-button k-button-icontext\">Close</a>";
    var noHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnNo\" class=\"close-button k-button k-button-icontext\">No</a>";
    var yesHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnYes\" class=\"submit-button k-button k-button-icontext\">Yes</a>";

    switch (type) {
        case 1:
            title = "Confirm";
            buttonsHtml = "<div class='buttons'>" + yesHtml + noHtml + '</div>';
            break;
        default:
            title = "Warning";
            buttonsHtml = "<div class='buttons'>" + closeHtml + '</div>';
            break;
    }

    var window = $("#" + windowId);
    if (window.length == 0) {
        console.log("append, id=" + windowId);
        $("body").append("<div id='" + windowId + "' />");
        window = $("#" + windowId);
    }

    if (window.data("kendoWindow")) {
        console.log("kendoWindow destroy, id=" + windowId);
        window.data("kendoWindow").destroy();
    }
    //kWindow.empty();

    var options = {
        width: "500px",
        modal: true,
        animation: {
            open: {
                //effects: "expandVertical",
                duration: 0.3
            },
        }
    };

    if (extendOptions != null)
        $.extend(options, extendOptions);
    
    options.actions = ["Close"];
    options.title = title;
    options.close = function() {
        if (typeof closeCallback === "function") {
            closeCallback();
        }
    };

    console.log("kendoWindow options=");
    console.log(options);
    window = window.kendoWindow(options);

    window.data("kendoWindow").content("<div class='message'>" + html + buttonsHtml + '</div>');
    window.data("kendoWindow").center().open();

    window.data("kendoWindow").wrapper.find(".close-button").off("click").click(function(e) {
        window.data("kendoWindow").close();
    });

    window.data("kendoWindow").wrapper.find(".submit-button").off("click").click(function(e) {
        window.data("kendoWindow").close();

        if (typeof yesCallback === "function") {
            yesCallback();
        }
        e.preventDefault();
    });

    return window;
}

helper.ui.getByTargetId = function(ev) {
    var id = null;
    if (ev != null && ev.target != null)
        id = ev.target.id;
    var sender = $('#' + id);
    return sender;
};

helper.ui.showLoading = function(sender, text) {
    if (sender == null || sender.length == 0)
        return;
    console.log(sender);
    console.log(text);

    if (text == null)
        text = "updating...";

    sender = helper.ui.findClosestTags(sender, ['BUTTON', 'SELECT']);

    $(sender).prop('disabled', true).addClass('k-state-disabled');
    $(sender).after("<div class='loading'>" + text + "</div>");
};

helper.ui.hideLoading = function (sender, onlyLabel) {
    if (sender == null)
        return;

    console.log(sender);

    onlyLabel = onlyLabel || false;

    sender = helper.ui.findClosestTags(sender, ['BUTTON', 'SELECT']);

    if (!onlyLabel)
        $(sender).prop('disabled', false).removeClass('k-state-disabled');

    var loading = $(sender).next();
    if (loading != null && loading.hasClass('loading')) {
        loading.remove();
    }
}

helper.ui.findClosestTags = function (sender, tagNames) {
    if (tagNames.indexOf($(sender).prop("tagName")) == -1) { //CASE: with span into button
        if (tagNames.indexOf($(sender).parent().prop("tagName")) >= 0)
            sender = $(sender).parent();
    }

    if (tagNames.indexOf($(sender).prop("tagName")) == -1) {
        var buttons = $(sender).find("button");
        if (buttons.length > 0)
            sender = buttons[0];
    }
    return sender;
}