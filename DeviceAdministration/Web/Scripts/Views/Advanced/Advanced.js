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
        selectApiProvider(config.apiRegistrationProvider);
    };

    var redirecToPartial = function redirecToPartial(partialUrl) {
        $(contentId).load(partialUrl);
    }

    var initRegistration = function (config) {
        // set up page
        $(document).tooltip();
        initApiRegistrationFields(config);

        if (config.apiRegistrationProvider) {
            $("#saveButton").prop("disabled", false);
            $("#editButton").prop("disabled", true);
            $("#changeApiRegistrationProviderButton").prop("disabled", false);
        }
        else {
            $("#editButton").prop("disabled", true);
            $("#changeApiRegistrationProviderButton").prop("disabled", true);
        }

        $("#saveButton").bind("click", function () {
            var apiProvider = $.trim($("#apiRegistrationProvider").val())
            var providerHasChanged = apiProvider && config.apiRegistrationProvider && apiProvider !== config.apiRegistrationProvider;
            var confirmSave = !providerHasChanged;          
            
            // if the provider is set and has changed then show warning message
            if (providerHasChanged) {
                confirmSave = confirm(config.apiProviderChangeWarningMessageOnSave);
            }

            // if not confirmed then bail
            if (!confirmSave) return;

            clearValidation();
            if (!validateAllInput()) {
                return;
            }

            var registrationModel = {
                BaseUrl: $.trim($("#BaseUrl").val()),
                LicenceKey: $.trim($("#LicenceKey").val()),
                Username: $.trim($("#Username").val()),
                Password: $.trim($("#Password").val()),
                apiRegistrationProvider: $.trim($("#apiRegistrationProvider").val())
            }

            $.post('/Advanced/SaveRegistration', { apiModel: registrationModel }, function(response) {

                if (response === "True") {
                    selectApiProvider(config.apiRegistrationProvider);
                    $("#registrationFailed").hide();
                    $("#registrationPassed").show();
                    $("#saveButton").prop("disabled", true);
                    $("#editButton").prop("disabled", false);
                    disableAllInput();
                    config.apiRegistrationProvider = registrationModel.apiRegistrationProvider;
                    $("#changeApiRegistrationProviderButton").prop("disabled", false);
                } else {
                    $("#registrationPassed").hide();
                    $("#registrationFailed").show();
                }
            });
        });

        $("#editButton").bind("click", function () {
            enableApiRegistrationEdit(config.apiRegistrationProvider, false);
        });

        $("#changeApiRegistrationProviderButton").bind("click", function () {
            var confirmed = confirm(config.apiProviderChangeWarningMessageInitial);
            if(confirmed){
                $('#apiRegistrationProvider').prop("disabled", false)
                enableApiRegistrationEdit(config.apiRegistrationProvider, true);
            }
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

    var selectApiProvider = function(providerName, changeProvider){
        if (providerName) {
            var providerSelectElement = $('#apiRegistrationProvider');
            var selectedOptionElement = providerSelectElement.find('option[value="' + providerName + '"]');
            if (selectedOptionElement.length > 0) {
                // if apiRegistrationProvider select option found select it and disable the input
                selectedOptionElement.attr('selected', 'selected');
                if (!changeProvider) {
                    providerSelectElement.attr('disabled', 'disabled');
                }               
            }
            else {
                // select the default option if apiRegistrationProvider did not mach any of the options
                providerSelectElement.find('option[value=""]').attr('selected', 'selected');
            }
        }
    }

    var initApiRegistrationFields = function (config) {
        $('#apiRegistrationProvider').on('change', function (event) {
            event.preventDefault();
            showApiRegistrationFields(this.value);
        });
        
        var selectedProvider = config.apiRegistrationProvider;
        if (selectedProvider) {
            showApiRegistrationFields(selectedProvider);
        }
        else {
            hideApiRegistrationFields();
        }
    }

    var hideApiRegistrationFields = function () {
        $("#BaseUrl").closest('fieldset').hide();
        $("#LicenceKey").closest('fieldset').hide();
        $("#Username").closest('fieldset').hide();
        $("#Password").closest('fieldset').hide();
    }

    var showApiRegistrationFields = function (selectedProvider) {
        function showSharedFields() {
            $("#BaseUrl").closest('fieldset').show();
            $("#Username").closest('fieldset').show();
            $("#Password").closest('fieldset').show();
        }
        switch(selectedProvider){
            case 'Jasper': {
                showSharedFields();
                $("#LicenceKey").closest('fieldset').show();
                break;
            }
            case 'Ericsson': {
                showSharedFields();
                $("#LicenceKey").closest('fieldset').hide();
                break;
            }
            default: {
                hideApiRegistrationFields();
                break;
            }
        }
    }

    var enableApiRegistrationEdit = function (apiRegistrationProvider, changeProvider) {
        enableAllInput();
        selectApiProvider(apiRegistrationProvider, changeProvider);
        $("#saveButton").prop("disabled", false);
        $("#editButton").prop("disabled", true);
    }

    return {
        init : init,
        initSubView: initSubView,
        redirecToPartial: redirecToPartial,
        initRegistration: initRegistration,
        initAssociation: initAssociation
    };
}, [jQuery]);