var popupWindow = (function () {

    // ************************************************************************
    // Private variables
    // ************************************************************************

    var dlgList = [];
    var options = {};
    var popupId = "popup";

    // ************************************************************************
    // Private methods
    // ************************************************************************

    var onClose = function (level) {
        
    };

    var onResize = function (level) {
        console.log("resized");
    };

    var onError = function (level, e) {
        console.log("error", e);
        dlgList[level].data("kendoWindow").close();
    };

    var onRefresh = function (level, e) {
        console.log("onRefresh");
        dlgList[level].data("kendoWindow").center();
        e.preventDefault();
    };

    var onOpen = function (level, e) {
        dlgList[level].html('<div class="loader-wrapper"><div></div></div>');
    };

    //var onClose = function() {
    //    dlg.data("kendoWindow").close();
    //};

    var onSubmitSuccess = function (e) {
        try {
            console.log("successSubmit");
            if (options.submitSuccess != null) {
                console.log("custom update");
                options.submitSuccess(e);
            } else {
                //Default update row
                console.log("default update");
                if (!dataUtils.isEmpty(e.GridName)) {
                    var grid = $("#" + e.GridName);
                    if (grid != null && grid.length > 0)
                        grid.refreshRow(e.Row, e.UpdateFields, e.ForseGridRefresh);
                }
            }
        } catch (ex) {
            var message = "<div><b>An error with popup windows</b>. Details: " + ex.message;
            Message.showTop(message, Message.ERROR);
            console.log("onSubmitSuccess");
            console.log(ex);
        }
    };

    var onCloseSuccess = function(e) {
        try {
            console.log("successClose");
            if (options.closeSuccess != null) {
                console.log("call after close update");
                options.closeSuccess(e);
            }
        } catch (ex) {
            var message = "<div><b>An error with popup windows</b>. Details: " + ex.message;
            Message.showTop(message, Message.ERROR);
            console.log("onCloseSuccess");
            console.log(ex);
        }
    }


    var initAndOpenWithSettings = function (useOptions) {
        console.log("initAndOpenWithSettings, count=" + $("#popup").length);
        var level = useOptions.level || 1;
        popupId = "popup" + level;
        if ($("#" + popupId).length > 0) {
            $("#" + popupId).data("kendoWindow").close();
            $("#" + popupId).data("kendoWindow").destroy();
            $("#" + popupId).remove();
        }

        $("#windowContainer").append("<div id='" + popupId + "'></div>");

        dlgList[level] = $("#" + popupId);
        options = useOptions;

        var windowOptions = {
            position: { top: 100 }, //fixing issue with page scrolling after page open (no matter what is specified)
            //width: width,
            minHeight: 200,
            //title: title,
            //content: contentUrl,
            modal: true,
            close: onClose,
            error: onError,
            refresh: onRefresh,
            open: onOpen,
            resize: onResize,
            deactivate: function () {
                this.destroy();
            },
            animation: {
                open: { effects: "fadeIn" },
                close: { effects: "fadeIn", reverse: true }
            }
        };

        $.extend(windowOptions, useOptions);


        //Redefine events (include base event call)
        windowOptions.close = function(e) {
            onClose(level, e);
            if (useOptions.close != null) {
                useOptions.close(e);
            }
        };

        windowOptions.error = function(e) {
            onError(level, e);
            if (useOptions.error != null) {
                useOptions.error(e);
            }
        };

        windowOptions.refresh = function(e) {
            onRefresh(level, e);
            if (useOptions.refresh != null) {
                useOptions.refresh(e);
            }
        };

        windowOptions.open = function (e) {
            onOpen(level, e);
            if (useOptions.open != null) {
                useOptions.open(e);
            }
        };

        windowOptions.resize = function(e) {
            onResize(level, e);
            if (useOptions.resize != null) {
                useOptions.resize(e);
            }
        };

        dlgList[level].kendoWindow(windowOptions);

        if (useOptions.content != "") {
            dlgList[level].data("kendoWindow").center();
        }
    };

    return {

        // ************************************************************************
        // Public methods
        // ************************************************************************
        initAndOpen: function (contentUrl, title, width) {
            initAndOpenWithSettings({
                content: contentUrl,
                title: title,
                width: width
            });
        },

        initAndOpenWithSettings: function (useOptions) {
            initAndOpenWithSettings(useOptions);
        },

        setContent: function (content, level) {
            level = level || 1;
            dlgList[level].data("kendoWindow").content(content);
            dlgList[level].data("kendoWindow").center();
        },

        setContentUrl: function (url, level) {
            level = level || 1;

            //http://www.telerik.com/forums/issue-with-conent-url-in-kendo-window
            var window = dlgList[level].data("kendoWindow");
            window.refresh({
                url: url,

            });

            //dlg.data("kendoWindow").content(url);
        },

        open: function (level) {
            level = level || 1;
            dlgList[level].data("kendoWindow").center();
            dlgList[level].data("kendoWindow").open();
        },

        submit: function (e, level) {
            level = level || 1;

            if (typeof e == 'object' && e != null && e.Row != undefined) {
                console.log('return gridName=' + e.GridName + ", updateFields=" + e.UpdateFields + ", forseGridRefresh=" + e.ForseGridRefresh);

                if (e.Url != undefined && e.Url != null && e.Url != '') {
                    console.log('return url=' + e.Url);
                    window.open(e.Url, "_blank", "");
                }

                onSubmitSuccess(e);

                dlgList[level].data("kendoWindow").close();

                return;
            }
            if (typeof e == 'object' && e != null && e.Url != undefined) {
                console.log('return url=' + e.Url);
                window.open(e.Url, "_blank", "");

                onSubmitSuccess(e);
                dlgList[level].data("kendoWindow").close();

                return;
            }
        },
        
        customAction: function(actionName, data) {
            console.log("customAction: " + actionName + ", data=" + data);
            if (options.customAction != null) {
                options.customAction(actionName, data);
            }
        },

        close: function (level, e) {
            level = level || 1;
            dlgList[level].data("kendoWindow").close();
            onCloseSuccess(e);
        },

        isOpen: function (level) {
            level = level || 1;

            if (dlgList[level] != null
                && dlgList[level].data("kendoWindow") != null)
                return true;
            return false;
        },

        getTitle: function() {
            if (options != null) {
                return options.title;
            }
            return null;
        }
    };
})();

