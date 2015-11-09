IoTApp.createModule('IoTApp.AllRulesAssigned', (function () {
    "use strict";

    var self = this;

    var init = function () {
        self.backButton = $(".header_main__button_back");
        self.backButton.show();
        self.backButton.off("click").click(backButtonClicked);

        $(".view_rules_button").click(viewRules);
    }

    var backButtonClicked = function () {
        history.back();
    }

    var viewRules = function () {
        location.href = resources.redirectUrl;
    }

    return {
        init: init
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.AllRulesAssigned.init();
});