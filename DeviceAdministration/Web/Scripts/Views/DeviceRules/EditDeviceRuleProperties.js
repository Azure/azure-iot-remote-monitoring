IoTApp.createModule('IoTApp.EditDeviceRuleProperties', (function () {
    "use strict";

    var self = this;

    var init = function () {
        self.backButton = $(".button_back");
        self.backButton.show();
        self.backButton.off("click").click(backButtonClicked);
    }

    var backButtonClicked = function () {
        history.back();
    }

    var onBegin = function () {
        $('#update_rule_properties').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        $('#update_rule_properties').removeAttr("disabled");
        if (result.success) {
            location.href = resources.redirectUrl;
        } else if (result.error) {
            IoTApp.Helpers.Dialog.displayError(result.error);
        } else {
            IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
        }
    }

    var onFailure = function (result) {
        $('#update_rule_properties').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }

    return {
        init: init,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.EditDeviceRuleProperties.init();
});