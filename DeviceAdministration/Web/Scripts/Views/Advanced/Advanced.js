IoTApp.createModule('IoTApp.Advanced', function () {
    "use strict";

    var backButtonId;
    var contentId;
    var subheadId;

    function disableAllInput() {
        $("select").each(function () {
            $("#" + this.id).prop("disabled", true);
        });
        $("input[type=text]").each(function () {
            $("#" + this.id).prop("disabled", true);
        });
        $("input[type=password]").each(function () {
            $("#" + this.id).prop("disabled", true);
        });
    }

    function enableAllInput() {
        $("select").each(function () {
            $("#" + this.id).prop("disabled", false);
        });
        $("input[type=text]").each(function () {
            $("#" + this.id).prop("disabled", false);
        });
        $("input[type=password]").each(function () {
            $("#" + this.id).prop("disabled", false);
        });
    }

    function clearValidation() {
        $("select").each(function () {
            $("#" + this.id + "Required").hide();
        });
        $("input[type=text]").each(function () {
            $("#" + this.id + "Required").hide();
        });
        $("input[type=password]").each(function () {
            $("#" + this.id + "Required").hide();
        });
        $("#registrationFailed").hide();
        $("#registrationPassed").hide();
    }

    function checkIfPageHasInputAlready() {
        $("input[type=text]").each(function () {
            if (this.value) {
                return true;
            }
        });
        return false;
    }

    function validateAllInput() {
        var valOk = true;
        $("select").each(function () {
            if (!$("#" + this.id).val()) {
                $("#" + this.id + "Required").show();
                valOk = false;
            }
        });
        $("input[type=text]").each(function () {
            if (!$("#" + this.id).val()) {
                $("#" + this.id + "Required").show();
                valOk = false;
            }
        });
        $("input[type=password]").each(function () {
            if (!$("#" + this.id).val()) {
                $("#" + this.id + "Required").show();
                valOk = false;
            }
        });
        return valOk;
    }

    var init = function init(config) {
        backButtonId = config.backButtonId;
        contentId = config.contentId;
        subheadId = config.subheadId;

        $(backButtonId).hide();
    }

    var initSubView = function initSubView(config) {
        $(subheadId).text(config.subheadContent);
        // configure the back button
        if (config.goBackUrl) {
            $(backButtonId).show();
            $(backButtonId).off('click').click(function () {
                $(contentId).load(config.goBackUrl);
            });
        } else {
            $(backButtonId).hide();
        }
        
        // configure the selected provider dropdown
        debugger
        if (config.selectedProvider) {
            var providerSelectElement = $('#apiProvider');          
            var selectedOptionElement = providerSelectElement.find('option[value="' + config.selectedProvider + '"]');
            if (selectedOptionElement.length > 0) {
                // if selectedProvider select option found select it and disable the input
                selectedOptionElement.attr('selected', 'selected');
                providerSelectElement.attr('disabled', 'disabled');
            }
            else {
                // select the default option if selectedProvider did not mach any of the options
                providerSelectElement.find('option[value=""]').attr('selected', 'selected');
            }
        }
    };

    var redirecToPartial = function redirecToPartial(partialUrl) {
        $(contentId).load(partialUrl);
    }

    var initRegistration = function (config) {
        // set up page
        $(document).tooltip();

        if (config.selectedProvider) {
            $("#saveButton").prop("disabled", true);
        }
        else {
            $("#editButton").prop("disabled", true);
        }

        $("#saveButton").bind("click", function () {

            clearValidation();
            if (!validateAllInput()) {
                return;
            }

            var registrationModel = {
                BaseUrl: $.trim($("#BaseUrl").val()),
                LicenceKey: $.trim($("#LicenceKey").val()),
                Username: $.trim($("#Username").val()),
                Password: $.trim($("#Password").val()),
                ApiRegistrationProvider: $.trim($("#ApiRegistrationProvider").val())
            }

            $.post('/Advanced/SaveRegistration', { apiModel: registrationModel }, function(response) {

                if (response === "True") {
                    $("#registrationFailed").hide();
                    $("#registrationPassed").show();
                    $("#saveButton").prop("disabled", true);
                    $("#editButton").prop("disabled", false);
                    disableAllInput();
                } else {
                    $("#registrationPassed").hide();
                    $("#registrationFailed").show();
                }
            });
        });

        $("#editButton").bind("click", function () {
            enableAllInput();
            $("#saveButton").prop("disabled", false);
            $("#editButton").prop("disabled", true);
        });
    }

    var initAssociation = function() {
        $("#associateButton").bind("click", function () {
            var deviceId = $("#UnassignedDeviceIds option:selected").text();
            var iccid = $("#UnassignedIccids option:selected").text();

            $.ajax({
                url: '/Advanced/AssociateIccidWithDevice',
                data: { deviceId: deviceId, iccid: iccid },
                async : true,
                type: "post",
                success: function () {
                    $("#UnassignedDeviceIds option:contains('" + deviceId + "')").remove();
                    $("#UnassignedIccids option:contains('" + iccid + "')").remove();

                    $("#associateSucceeded").fadeOut(250, function () {
                        $("#associateSucceededText").text(resources.simAssociationSucceeded.replace("{0}", deviceId).replace("{1}", iccid));
                    }).fadeIn(250);
                }
            });
        });

        $("#associateSucceeded").hide();
    }

    return {
        init : init,
        initSubView: initSubView,
        redirecToPartial: redirecToPartial,
        initRegistration: initRegistration,
        initAssociation: initAssociation
    };
}, [jQuery]);