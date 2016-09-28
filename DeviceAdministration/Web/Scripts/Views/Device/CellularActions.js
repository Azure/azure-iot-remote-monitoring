IoTApp.createModule("IoTApp.CellularActions", function () {
    "use strict";
    var self = this;
    self.deviceId = null;
    self.initialCellActionSettings = null;
    $.ajaxSetup({ cache: false });
    var resetDeviceConnection = function () {
        $("#loadingElement").show();
        var url = "/Device/ReconnectDevice";
        return $.ajax({
            url: url,
            type: "POST",
            data: {
                deviceId: self.deviceId
            },
            dataType: "json"
        });
    }
    var resetDeviceConnectionOnClick = function () {
        return resetDeviceConnection().then(function () {
            $("#loadingElement").hide();
        }, function () {
            $("#loadingElement").hide();
            console.error("There was a problem reconnecting the device.");
        });
    }
    var toggleInputDisabledProperty = function (disabled) {
        if (disabled) {
            $("#simStateSelect").attr("disabled", "disabled");
            $("#subscriptionPackageSelect").attr("disabled", "disabled");
            $("#resetDeviceConnection").attr("disabled", "disabled");
            $("#saveActions").attr("disabled", "disabled");
            $("#editActions").removeAttr("disabled");
        } else {
            $("#simStateSelect").removeAttr("disabled");
            $("#subscriptionPackageSelect").removeAttr("disabled");
            $("#resetDeviceConnection").removeAttr("disabled");
            $("#saveActions").removeAttr("disabled");
            $("#editActions").attr("disabled", "disabled");
        }
    }
    var retrieveActionFormValues = function () {
        var simStatus = $("#simStateSelect").val();
        var subscriptionPackage = $("#subscriptionPackageSelect").val();
        return {
            subscriptionPackage: subscriptionPackage,
            simStatus: simStatus
        }
    }
    var generateActionUpdateObject = function () {
        var cellularActionUpdateRequestModel = {
            cellularActions: []
        };
        var currentFormValues = retrieveActionFormValues();
        if (currentFormValues.subscriptionPackage !== self.initialCellActionSettings.subscriptionPackage) {
            cellularActionUpdateRequestModel.cellularActions.push({
                type: "UpdateSubscriptionPackage",
                currentValue: self.initialCellActionSettings.simStatus,
                newValue: currentFormValues.subscriptionPackage
            });
        }
        if (currentFormValues.simStatus !== self.initialCellActionSettings.simStatus) {
            cellularActionUpdateRequestModel.cellularActions.push({
                type: "UpdateStatus",
                currentValue: self.initialCellActionSettings.simStatus,
                newValue: currentFormValues.simStatus
            });
        }
        cellularActionUpdateRequestModel.deviceId = self.deviceId;
        return cellularActionUpdateRequestModel;
    }
    var enableActionsForm = function() {
        toggleInputDisabledProperty(false);
    }
    var saveActions = function () {
        $("#loadingElement").show();
        var data = generateActionUpdateObject();
        var url = "/Device/CellularActionUpdateRequest";
        return $.post(url, data)
            .then(function (data) {
                $("#loadingElement").hide();
                debugger
            }, function (error) {
                debugger
                console.error(error);
                $("#loadingElement").hide();
            });
    }
    var attachEventHandlers = function () {
        $("#editActions").click(enableActionsForm);
        $("#saveActions").click(saveActions);
    }
    var initActionForm = function () {
        if (!self.deviceId) throw new Error("You must call IoTApp.CellularActions.init(deviceId) with a valid device ID first.");
        self.initialCellActionSettings = retrieveActionFormValues();
        toggleInputDisabledProperty(true);
        attachEventHandlers();
    }
    var init = function (deviceId) {
        self.deviceId = deviceId;
    }
    return {
        init: init,
        initActionForm: initActionForm
    }
}, [jQuery, resources]);
