
var Message = (function () {
    var ERROR = 0;
    var SUCCESS = 1;
    var INFO = 2;

    var POPUP_CLOSE = 0;
    var POPUP_YES_NO = 1;
    var POPUP_INFO = 2;

    var pWindows = [];

    var getTextBox = function(type, id) {
        var baseName = "";
        switch (type) {
            case 0:
                baseName = "errorMessage";
                break;
            case 1:
                baseName = "successMessage";
                break;
            case 2:
                baseName = "infoMessage";
                break;
            default:
                baseName = "infoMessage";
                break;
        }

        if (id != null && id != '')
            baseName += "_" + id;
        
        return $("#" + baseName);
    };

    var getIcon = function (type, id) {
        var baseName = "";
        switch (type) {
            case 0:
                baseName = "errorMessageIcon";
                break;
            case 1:
                baseName = "successMessageIcon";
                break;
            case 2:
                baseName = "infoMessageIcon";
                break;
            default:
                baseName = "infoMessageIcon";
                break;
        }

        if (id != null && id != '')
            baseName += "_" + id;

        return $("#" + baseName);
    };

    var setText = function (type, id, text) {
        var el = getTextBox(type, id);
        var iconEl = getIcon(type, id);
        if (el != null)
            el.html(text);
        if (iconEl != null) {
            if (!dataUtils.isEmpty(text))
                iconEl.show();
            else
                iconEl.hide();
        }
    };
    
    var appendText = function (type, id, text) {
        var el = getTextBox(type, id);
        var iconEl = getIcon(type, id);
        if (el != null) {
            el.append(text);
        }
        if (iconEl != null && el != null) {
            if (!dataUtils.isEmpty(el.html()))
                iconEl.show();
            else
                iconEl.hide();
        }
    };

    var eraseText = function (type, id) {
        var el = getTextBox(type, id);
        var iconEl = getIcon(type, id);
        if (el != null)
            el.html("");
        if (iconEl != null)
            iconEl.hide();
    };

    var showPopupWithOptions = function(options) {
        var type = options.type || 0;
        var level = options.level || 0;
        var html = options.html || "";
        var yesCallback = options.yesCallback;
        var noCallback = options.noCallback;
        
        var buttonsHtml = "";
        var title = "Info";

        var closeHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnClose\" class=\"submit-button k-button k-button-icontext\">Close</a>";
        var noHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnNo\" class=\"close-button k-button k-button-icontext\">No</a>";
        var yesHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnYes\" class=\"submit-button k-button k-button-icontext\">Yes</a>";

        switch (type) {
        case POPUP_YES_NO:
            title = "Confirm";
            buttonsHtml = "<div class='buttons'>" + yesHtml + noHtml + '</div>';
            break;
        case POPUP_INFO:
            title = "Info";
            buttonsHtml = "<div class='buttons'>" + closeHtml + '</div>';
            break;
        default:
            title = "Warning";
            buttonsHtml = "<div class='buttons'>" + closeHtml + '</div>';
            break;
        }

        //if (kWindow.data("kendoWindow")) {
        //    console.log("kWindow destroy");
        //    kWindow.data("kendoWindow").destroy();
        //}

        //kWindow.empty();
        if (pWindows[level] == null) {
            pWindows[level] = $("#MessagePopupWindow" + level);

            pWindows[level].kendoWindow({
                width: "500px",
                actions: ["Close"],
                title: title,
                //minHeight: 100,
                modal: true,
                animation: {
                    open: {
                        //effects: "expandVertical",
                        duration: 0.3
                    },
                },
                close: function() {
                    //Nothing
                },
            });
        }

        pWindows[level].data("kendoWindow").content("<div class='message'>" + html + buttonsHtml + '</div>');
        pWindows[level].data("kendoWindow").center().open();

        pWindows[level].data("kendoWindow").wrapper.find(".close-button").off("click").click(function(e) {
            pWindows[level].data("kendoWindow").close();

            if (typeof noCallback === "function") {
                noCallback();
            }
        });

        pWindows[level].data("kendoWindow").wrapper.find(".submit-button").off("click").click(function(e) {
            pWindows[level].data("kendoWindow").close();

            if (typeof yesCallback === "function") {
                yesCallback();
            }
            e.preventDefault();
        });
    };

    var messager = {
        CLOSE: POPUP_CLOSE,
        YES_NO: POPUP_YES_NO,
        POPUP_INFO: POPUP_INFO,
        
        ERROR: ERROR,
        SUCCESS: SUCCESS,
        INFO: INFO,
        
        clear: function(id) {
            eraseText(ERROR, id);
            eraseText(SUCCESS, id);
            eraseText(INFO, id);
        },
        error: function(text, id) {
            setText(ERROR, id, text);
            eraseText(SUCCESS, id);
            eraseText(INFO, id);
        },
        appendError: function (text, id) {
            appendText(ERROR, id, text);
        },
        success: function(text, id) {
            eraseText(ERROR, id);
            setText(SUCCESS, id, text);
            eraseText(INFO, id);
        },
        appendSuccess: function(text, id) {
            appendText(SUCCESS, id, text);
        },
        info: function (text, id) {
            eraseText(ERROR, id);
            eraseText(SUCCESS, id);
            setText(INFO, id, text);
        },
        closePopup: function(id) {
            $('#' + id).data('kendoWindow').close();
        },

        popupAsync: function (options) {// html, type, yesCallback, level) {
            var defer = $.Deferred();
            var beenResolved = false;

            options = options || {};
            options.type = options.type || 0;
            options.title = options.title || "Info";
            options.message = options.message || "no message";

            var buttonsHtml = "";

            var closeHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnClose\" class=\"close-button k-button k-button-icontext\">Close</a>";
            var noHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnNo\" class=\"close-button k-button k-button-icontext\">No</a>";
            var yesHtml = "<a style=\"width:80px\" href=\"javascript:;\" name=\"btnYes\" class=\"submit-button k-button k-button-icontext\">Yes</a>";

            switch (options.type) {
                case POPUP_YES_NO:
                    buttonsHtml = "<div class='buttons'>" + yesHtml + noHtml + '</div>';
                    break;
                default:
                    buttonsHtml = "<div class='buttons'>" + closeHtml + '</div>';
                    break;
            }

            var node = $("<div />");
            $(document.body).append(node);

            node.kendoWindow({
                width: "500px",
                actions: ["Close"],
                title: options.title,
                //minHeight: 100,
                modal: true,
                animation: {
                    open: {
                        //effects: "expandVertical",
                        duration: 0.3
                    },
                },
                close: function () {
                    console.log('close, ' + beenResolved);

                    this.destroy();//.empty();

                    if (!beenResolved) {
                        console.log('call reject');
                        defer.reject();
                    }
                },
            });

            var wnd = node.data("kendoWindow");

            wnd.content("<div class='message'>"
                + options.message
                + buttonsHtml
                + '</div>');
            wnd.center().open();

            wnd.wrapper.find(".close-button").off("click").click(function (e) {
                wnd.close();
            });

            wnd.wrapper.find(".submit-button").off("click").click(function (e) {
                beenResolved = true;

                wnd.close();

                defer.resolve();

                e.preventDefault();
            });

            return defer;
        },

        popupEx: function (options) {
            showPopupWithOptions(options);
        },

        popup: function (html, type, yesCallback, level) {
            showPopupWithOptions({
                html: html,
                type: type,
                yesCallback: yesCallback,
                level: level
            });
        },

        showTop: function (message, type) {
            var bgColor = "red";
            switch (type) {
                case ERROR:
                    bgColor = "red";
                    break;
                case SUCCESS:
                    bgColor = "green";
                    break;
                case INFO:
                    bgColor = "gray";
                    break;
                default:
                    bgColor = "gray";
                    break;
            }
            $('.top-bar').css('background-color', bgColor);
            $('.top-bar').html(message);
            $('.top-bar').show();
        }
    };
    
    return messager;
})();