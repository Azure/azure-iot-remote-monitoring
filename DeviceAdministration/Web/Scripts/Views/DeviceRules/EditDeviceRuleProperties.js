IoTApp.createModule('IoTApp.EditDeviceRuleProperties', (function () {
    "use strict";

    var self = this;

    var init = function () {
        self.backButton = $(".header_main__button_back");
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
        } else {
            if (result.error) {
                IoTApp.Helpers.Dialog.displayError(result.error);
            } else {
                IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
            }
            if (result.entity != null) {
                // since the data may have changed on the server, update the data
                updateLayout(result.entity);
            }
        }
    }

    var onFailure = function (result) {
        $('#update_rule_properties').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
        if (result.entity != null) {
            updateLayout(result.entity);
        }
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }

    var updateLayout = function (rule) {
        rule = JSON.parse(rule);
        $('#Etag').val(rule.Etag);
        $('#EnabledState').attr({ "data-val": rule.EnabledState.toString(), "value": rule.EnabledState.toString() });
        if (rule.EnabledState == true) {
            $('#state').val(resources.enabledString);
        } else {
            $('#state').val(resources.disabledString);
        }
        $('#DataField > option').each(function () {
            if (this.value == rule.DataField) {
                $(this).prop("selected", true);
            } else {
                $(this).removeProp("selected");
            }
        });
        $('#Threshold').val(rule.Threshold.toString());
        $('#RuleOutput > option').each(function () {
            if (this.value == rule.RuleOutput) {
                $(this).prop("selected", true);
            } else {
                $(this).removeProp("selected");
            }
        });

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