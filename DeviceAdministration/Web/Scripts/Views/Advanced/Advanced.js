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

    function clearAllInputs() {
        $("input[type=text]").each(function () {
            $("#" + this.id).val("");
        });
        $("input[type=password]").each(function () {
            $("#" + this.id).val("");
        });
    }
    
    function enableAllInput(excludedInputIds) {
        disableAllInput();
        function isExcludedInput(inputId, excludedInputIds){
            return typeof excludedInputIds && jQuery.inArray(inputId, excludedInputIds) > -1;
        }
        $("select").each(function () {
            if (isExcludedInput(this.id, excludedInputIds)) return;
            $("#" + this.id).prop("disabled", false);
        });
        $("input[type=text]").each(function () {
            if (isExcludedInput(this.id, excludedInputIds)) return;
            $("#" + this.id).prop("disabled", false);
        });
        $("input[type=password]").each(function () {
            if (isExcludedInput(this.id, excludedInputIds)) return;
            $("#" + this.id).prop("disabled", false);
        });
    }

    function clearValidation() {
        $("select:enabled").each(function () {
            $("#" + this.id + "Required").hide();
        });
        $("input[type=text]:enabled").each(function () {
            $("#" + this.id + "Required").hide();
        });
        $("input[type=password]:enabled").each(function () {
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
        $("select:enabled").each(function () {
            if (!$("#" + this.id).val()) {
                $("#" + this.id + "Required").show();
                valOk = false;
            }
        });
        $("input[type=text]:enabled").each(function () {
            if (!$("#" + this.id).val()) {
                $("#" + this.id + "Required").show();
                valOk = false;
            }
        });
        $("input[type=password]:enabled").each(function () {
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
                $.ajax({
                    url: config.goBackUrl,
                    cache: false,
                    success: function (data) {
                        $(contentId).html(data);
                    }
                });
            });
        } else {
            $(backButtonId).hide();
        }
        
        // configure the selected provider dropdown
        selectApiProvider(config.apiRegistrationProvider);
    };

    var redirecToPartial = function redirecToPartial(partialUrl) {
        $.ajax({
            url: partialUrl,
            cache: false,
            success: function (data) {
                $(contentId).html(data);
            }
        });
    }

    var initRegistration = function (config) {
        // set up page
        $(document).tooltip();
        initApiRegistrationFields(config);

        $("#saveButton").bind("click", function () {
            var apiProvider = $.trim($("#apiRegistrationProvider").val())
            var providerHasChanged = apiProvider && config.apiRegistrationProvider && apiProvider !== config.apiRegistrationProvider;
            var confirmSave = !providerHasChanged;          
            
            // if the provider is set and has changed then show warning message
            if (providerHasChanged) {
                confirmSave = confirm(config.apiProviderChangeWarningMessage);
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
                    $("#changeApiRegistrationProviderButton").show();
                } else {
                    $("#registrationPassed").hide();
                    $("#registrationFailed").show();
                }
            });
        });

        $("#editButton").bind("click", function () {
            enableApiRegistrationEdit(config.apiRegistrationProvider, true);
        });

        $("#changeApiRegistrationProviderButton").bind("click", function () {
            $('#changeApiRegistrationProviderButton').prop("disabled", true);
            $("#saveButton").prop("disabled", false);
            $("#editButton").prop("disabled", true);
            showApiRegistrationFields($("#apiRegistrationProvider").val(), true, false);
        });
    }

    var iccidFileUploadOnChange = function () {
        var url = '/Advanced/DeviceAssociation';
        IoTApp.Advanced.processCsvFileInput(this)
        .then(IoTApp.Advanced.postIccidFileData)
        .then(function () {
            IoTApp.Advanced.redirecToPartial(url);
        }, function (error) {
            console.error(error);
        });
    }

    var uploadIccidsButtonOnClick = function () {
        $("#iccidFileUpload").click();
    }

    var initAssociation = function () {
        $("#iccidFileUpload").on("change", iccidFileUploadOnChange);
        $("#uploadIccidButton").on("click", uploadIccidsButtonOnClick);
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
        disableAllInput();
        $('#apiRegistrationProvider').on('change', function (event) {
            event.preventDefault();
            clearAllInputs();
            showApiRegistrationFields(this.value, true);
            $("#saveButton").prop("disabled", false);
        });

        $("#saveButton").prop("disabled", true);
        $("#editButton").prop("disabled", false);
        
        var selectedProvider = config.apiRegistrationProvider;
        if (selectedProvider) {
            showApiRegistrationFields(selectedProvider, false, true);
            $("#changeApiRegistrationProviderButton").show();
            $("#saveButton").prop("disabled", true);
            $("#editButton").prop("disabled", false);
        }
        else {
            hideApiRegistrationFields();
            $("#changeApiRegistrationProviderButton").hide();
            $("#apiRegistrationProviderWarning").hide();
            $('#apiRegistrationProvider').prop('disabled', false);
            $("#saveButton").prop("disabled", true);
            $("#editButton").prop("disabled", true);
        }
    }

    var hideApiRegistrationFields = function () {
        $("#BaseUrl").closest('fieldset').hide();
        $("#LicenceKey").closest('fieldset').hide();
        $("#Username").closest('fieldset').hide();
        $("#Password").closest('fieldset').hide();
    }

    var showApiRegistrationFields = function (selectedProvider, enableFields, disableApiProvider) {
        function showSharedFields() {
            $("#BaseUrl").closest('fieldset').show();
            $("#Username").closest('fieldset').show();
            $("#Password").closest('fieldset').show();
        }
        var disabledFields = []
        switch(selectedProvider){
            case 'Jasper': {
                showSharedFields();
                if (enableFields) {
                    if (disableApiProvider) {
                        disabledFields.push('apiRegistrationProvider');
                    }
                    enableAllInput(disabledFields);
                }
                $("#LicenceKey").closest('fieldset').show();
                $(".api_registration_help_link").show();
                break;
            }
            case 'Ericsson': {
                showSharedFields();
                $("#LicenceKey").closest('fieldset').hide();
                if (enableFields) {
                    disabledFields.push('LicenceKey');
                    if (disableApiProvider) {
                        disabledFields.push('apiRegistrationProvider');
                    }
                    enableAllInput(disabledFields)
                }
                $(".api_registration_help_link").hide();
                break;
            }
            default: {
                hideApiRegistrationFields();
                $(".api_registration_help_link").hide();
                break;
            }
        }
    }

    var deleteApiRegistration = function () {
        $.ajax({
            url: '/Advanced/DeleteRegistration',
            data: {},
            async: true,
            type: "post",
            success: function () {
                window.location.reload();
            }
        });
    }

    var enableApiRegistrationEdit = function (apiRegistrationProvider, changeProvider) {
        selectApiProvider(apiRegistrationProvider, changeProvider);
        showApiRegistrationFields(apiRegistrationProvider, true, true);
        $("#saveButton").prop("disabled", false);
        $("#editButton").prop("disabled", true);
    }

    var getValidIccidRecords = function (csvResults) {
        return csvResults.data.filter(function (record) {
            return record.Id;
        });
    }

    var processCsvFileInput = function(fileInput) {
        var deferred = jQuery.Deferred();
        $(fileInput).parse({
            config: {
                complete: function (results) {
                    results = getValidIccidRecords(results);
                    deferred.resolve(results);
                },
                header: true
            },
            error: function (err) {
                deferred.reject(err);
            }
        });
        return deferred.promise();
    }

    var postIccidFileData = function (data) {
        return $.ajax({
            url: '/Advanced/AddIccids',
            data: JSON.stringify(data),
            async: true,
            type: "post",
            contentType: "application/json"
        });
    }

    return {
        init : init,
        initSubView: initSubView,
        redirecToPartial: redirecToPartial,
        initRegistration: initRegistration,
        initAssociation: initAssociation,
        deleteApiRegistration: deleteApiRegistration,
        processCsvFileInput: processCsvFileInput,
        postIccidFileData: postIccidFileData
    };
}, [jQuery]);