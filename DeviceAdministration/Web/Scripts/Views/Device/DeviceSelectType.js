IoTApp.createModule('IoTApp.DeviceSelectType', (function () {
    "use strict";

    var init = function() {
        $(".header_main__head").text(resources.addDevice);
        $(".header_main__subhead").text(resources.stepOneHeader);
        $(".content_outer").addClass("content_outer--select_device");
        $(".content_inner").addClass("content_inner--select_device");
        $(".header_main__button_back").hide();
        $(".header_main__button_back").off("click");
    }

    var onFailure = function () {
        IoTApp.Helpers.Dialog.displayError(resources.selectDeviceTypeError);
    }

    return {
        init: init,
        onFailure: onFailure
    }

}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.DeviceSelectType.init();
});