IoTApp.createModule('IoTApp.AddDeviceCreate', (function () {
    "use strict";

    var init = function() {
        $(".header_main__head").text(resources.addDevice);
        $(".header_main__subhead").text(resources.stepTwoHeader);
        $(".content_outer").removeClass("content_outer--select_device");
        $(".content_inner").removeClass("content_inner--select_device");
        $(".header_main__button_back").show();
        $(".header_main__button_back").off("click").click(function () {
            location.href = resources.redirectToIndexUrl;
        });

        $("#availableIccidList").hide();

        //show or hide the device Id box based upon what
        //was selected when the partial loads/reloads
        if ($('#deviceGeneratedBySystemYes').is(':checked')) {
            $("#deviceId").prop("disabled", true);
            $("#checkIdButton").prop("disabled", true);
        } else {
            $("#deviceId").prop("disabled", false);
            $("#checkIdButton").prop("disabled", false);
        }

        $('input[type="radio"]').bind("click", function () {
            if ($(this).attr("value") == "true") {
                $(".unique_device_id__error_check_id").hide();
                $("#checkIdButton").prop("disabled", true);
                $("#deviceId").prop("disabled", true);
                $("#deviceId").val(resources.enterDeviceId);
            } else {
                $("#checkIdButton").prop("disabled", false);
                $("#deviceId").prop("disabled", false);
                $("#deviceId").val("");
                $("#deviceId").focus();
            }
        });

        $('#iccidFlagCheckbox').bind("click", function () {
            if ($('#iccidFlagCheckbox').is(":checked") === true) {
                if (resources.canHaveIccid === "True") {
                    $('#availableIccidList').show();
                    $("#hiddenIccidSelection").val(
                        $("#availableIccidList option:selected").text()
                    );
                } else {
                    $('#iccidFlagCheckbox').attr('checked', false);
                    $("#noRegistration").show();
                }
            } else {
                $("#availableIccidList").hide();
                $("#hiddenIccidSelection").val(null);
            }
        });

        $("#availableIccidList").change(function () {
            $("#hiddenIccidSelection").val($("#availableIccidList option:selected").text());
        });
    }

    var onFailure = function () {
        IoTApp.Helpers.Dialog.displayError(resources.createDeviceError);
    }

    return {
        init: init,
        onFailure: onFailure
    }

}));

$(function () {
    "use strict";

    IoTApp.AddDeviceCreate.init();
});



