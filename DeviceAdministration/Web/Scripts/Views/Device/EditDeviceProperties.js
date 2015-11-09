IoTApp.createModule('IoTApp.EditDeviceProperties', (function () {
    "use strict";

    var self = this;

    var init = function () {
        self.backButton = $(".header_main__button_back");
        self.backButton.show();
        self.backButton.off("click").click(backButtonClicked);
    }

    var backButtonClicked = function() {
        location.href = resources.redirectUrl;
    }

    $("form").submit(function () {
        $("#loadingElement").show();
    });


    return {
        init: init
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.EditDeviceProperties.init();
});