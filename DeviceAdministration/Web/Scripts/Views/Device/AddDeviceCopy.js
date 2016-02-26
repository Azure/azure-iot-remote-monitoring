$(function () {
    "use strict";

    $(".header_main__head").text(resources.addDevice);
    $(".header_main__subhead").text(resources.stepThree);
    $(".header_main__button_back").hide();
    $(".header_main__button_back").off("click");

    $(".button_send_sms").on("click", function () {
        $.post("https://trackiotathletes.azurewebsites.net/api/v1/devices/sendSMS", {});
    });
});
