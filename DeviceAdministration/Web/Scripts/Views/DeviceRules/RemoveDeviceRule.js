IoTApp.createModule('IoTApp.RemoveDeviceRule', (function () {
    "use strict";

    var self = this;
    var init = function () {
        self.backButton = $(".header_main__button_back");
        self.removeRuleCheckbox = $("#removeRuleCheckbox");
        self.removeRule = $("#removeRule");
        self.cancelButton = $("#cancelButton");

        self.removeRuleCheckbox.on("click", removeRuleCheckboxClicked);

        self.cancelButton.off("click").click(cancelButtonClicked);

        self.backButton.off("click").click(backButtonClicked);
    }

    var cancelButtonClicked = function () {
        _redirectToIndex();
    }

    var backButtonClicked = function () {
        _redirectToIndex();
    }

    var _redirectToIndex = function () {
        location.href = resources.redirectToIndexUrl;
    }

    var removeRuleCheckboxClicked = function () {
        if ($(this).is(':checked')) {
            self.removeRule.removeAttr("disabled");
        } else {
            self.removeRule.attr("disabled", "disabled");
        }
    }

    var onSuccess = function () {
        _redirectToIndex();
    }

    var onFailure = function (data) {
        IoTApp.Helpers.Dialog.displayError(resources.errorRemoveRule);
    }

    return {
        init: init,
        onSuccess: onSuccess,
        onFailure: onFailure
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.RemoveDeviceRule.init();
});