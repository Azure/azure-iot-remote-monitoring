IoTApp.createModule("IoTApp.Helpers.Dialog", function() {
    "use strict";

    var defaultOption = {
        resizable: false,
        modal: true,
        closeText: "hide",
        width: 400,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide();
        }
    };

    var displayError = function (message, callback) {
        var container = $("#dialog-error");
        $("div span", container).html(message);

        showDialog(container, {
            buttons: [
                {
                    text: container.data("resource-ok"),
                    click: function () {
                        $(this).dialog("close");
                        if ($.isFunction(callback)) {
                            callback();
                        }
                    },
                    "class": "button_base"
                }
            ]
        });
    }

    var confirm = function (message, callback) {
        var container = $("#dialog-confirm");
        $("div span", container).html(message);

        showDialog(container, {
            open: function (event, ui) {
                $(".ui-dialog-titlebar-close").hide();
            },
            buttons: [
                {
                    text: container.data("resource-cancel"),
                    click: function () {
                        $(this).dialog("close");
                        if ($.isFunction(callback)) {
                            callback(false);
                        }
                    },
                    "class": "button_base button_secondary"
                },
                {
                    text: container.data("resource-ok"),
                    click: function () {
                        $(this).dialog("close");
                        if ($.isFunction(callback)) {
                            callback(true);
                        }
                    },
                    "class": "button_base"
                }
            ]
        });
    }

    var showDialog = function (container, option) {
        option.title = container.data("resource-title");
        option = $.extend(defaultOption, option);
        container.dialog(option);
    }

    return {
        displayError: displayError,
        confirm: confirm
    }
});