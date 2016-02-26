$(function () {
    "use strict";

    $(".header_main__head").text(resources.addDevice);
    $(".header_main__subhead").text(resources.stepThree);
    $(".header_main__button_back").hide();
    $(".header_main__button_back").off("click");

    $(".button_send_sms").on("click", function () {

        var data = {
            deviceId: $(".js-new_device_id .text_copy_container__input--add_device_copy_table").val().trim(),
            domain: $(".js-new_device_provider .text_copy_container__input--add_device_copy_table").val().trim(),
            deviceKey: $(".js-new_device_key .text_copy_container__input--add_device_copy_table").val().trim(),
            phoneNumber: $(".js-send_sms_phone_number").val().trim()
        }

        console.log(data);

        $.ajax({
            "dataType": "json",
            "type": "POST",
            "url": "/api/v1/devices/sendSMS",
            "cache": false,
            "data": data
        })
    });
});
