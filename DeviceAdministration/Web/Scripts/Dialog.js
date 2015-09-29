IoTApp.createModule("IoTApp.Helpers.Dialog", function() {
    "use strict";

    var displayError = function(message) {
        $("#dialog_error_text").html(message);

        var errorDialogButtons = getErrorDialogButtons();

        $("#dialog-error").dialog({
            title: $("#dialog_error_text").data("resource-error"),
            resizable: false,
            modal: true,
            closeText: "hide",
            open: function(event, ui) {
                $(".ui-dialog-titlebar-close").hide();
            },
            buttons: errorDialogButtons
        });
    }

    var getErrorDialogButtons = function() {
        var dialogButtons = {};
        var okResource = getOkResource();
        dialogButtons[okResource] = errorDialogOkButtonClicked;
        return dialogButtons;
    }

    var getOkResource = function() {
        var okResource = $("#dialog_error_text").data("resource-ok");
        if (!okResource) {
            return "OK";
        }

        return okResource;
    }

    var errorDialogOkButtonClicked = function() {
        $(this).dialog("close");
    }

    return {
        displayError: displayError
    }
});