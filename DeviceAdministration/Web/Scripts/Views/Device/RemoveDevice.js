IoTApp.createModule('IoTApp.RemoveDevice', (function () {
    "use strict";

    var self = this;
    var init = function() {
        self.backButton = $(".header_main__button_back");
        self.removeDeviceCheckbox = $("#removeDeviceCheckbox");
        self.removeDevice = $("#removeDevice");
        self.cancelButton = $("#cancelButton");

        self.removeDeviceCheckbox.on("click", removeDeviceCheckboxClicked);

        self.cancelButton.off("click").click(cancelButtonClicked);

        self.backButton.off("click").click(backButtonClicked);
    }

    var cancelButtonClicked = function() {
        _redirectToIndex();
    }

    var backButtonClicked = function() {
        _redirectToIndex();
    }

    var _redirectToIndex = function() {
        location.href = resources.redirectToIndexUrl;
    }

    var removeDeviceCheckboxClicked = function() {
        if ($(this).is(':checked')) {
            self.removeDevice.removeAttr("disabled");
        } else {
            self.removeDevice.attr("disabled", "disabled");
        }
    }

    var onSuccess = function () {
        setTimeout(function () {
            $('#loadingElement').show();
        }, 0);
        // get rid of deviceId cookie value--we just deleted the device
        IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie('');

        location.href = resources.redirectToIndexUrl;
    }

    var onFailure = function (data) {
        $("content").html(data);
        IoTApp.Helpers.Dialog.displayError(resources.errorRemoveDevice);
    }

    return {
        init: init,
        onSuccess: onSuccess,
        onFailure: onFailure
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.RemoveDevice.init();
});