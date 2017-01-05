/**
 * IoTApp.CellularActions module that is used in the device information secion of the solution 
 */
IoTApp.createModule("IoTApp.CellularInformation", function () {
    "use strict";
    /*
     * Module variable initialization
     */
    var self = this;
    self.ActionRequestEndpoint = "/Device/CellularActionRequest";
    self.deviceId = null;
    self.initialCellActionSettings = null;
    self.actionTypes = IoTApp.CellularActions.actionTypes;
    self.htmlElementIds = {
        simStateSelect: "#simStateSelect",
        subscriptionPackageSelect: "#subscriptionPackageSelect",
        localeSelect: "#localeSelect",
        saveCellularInformation: "#saveCellularInformation",
        editCellularInformation: "#editCellularInformation",
        cancelEditCellularInformation: "#cancelEditCellularInformation",
        loadingElement: "#loadingElement",
        updateCellularInformationResults: "#updateCellularInformationResults",
        goBackToDeviceDetails: "#goBackToDeviceDetails"
    }

    /*
     * Utility functions
     */

   var resetInputsToInitial = function() {
       $(self.htmlElementIds.simStateSelect).val(self.initialCellActionSettings.simStatus);
       $(self.htmlElementIds.subscriptionPackageSelect).val(self.initialCellActionSettings.subscriptionPackage);
       $(self.htmlElementIds.localeSelect).val(self.initialCellActionSettings.locale);
   }

    /**
     * Toggle the actions for to between enabled and disabled
     * @param {boolean} disabled If true disables the form. If false enables the form.
     * @returns {void}
     */
    var toggleInputDisabledProperty = function (disabled) {
        if (disabled) {
            $(self.htmlElementIds.simStateSelect).attr("disabled", "disabled");
            $(self.htmlElementIds.subscriptionPackageSelect).attr("disabled", "disabled");
            $(self.htmlElementIds.localeSelect).attr("disabled", "disabled");
            $(self.htmlElementIds.saveCellularInformation).hide();
            $(self.htmlElementIds.cancelEditCellularInformation).hide();
            $(self.htmlElementIds.editCellularInformation).show();
            $(self.updateCellularInformationResults).hide();
        } else {
            $(self.htmlElementIds.simStateSelect).removeAttr("disabled");
            $(self.htmlElementIds.subscriptionPackageSelect).removeAttr("disabled");
            $(self.htmlElementIds.localeSelect).removeAttr("disabled");
            $(self.htmlElementIds.saveCellularInformation).show();
            $(self.htmlElementIds.cancelEditCellularInformation).show();
            $(self.htmlElementIds.editCellularInformation).hide();
            $(self.updateCellularInformationResults).show();
        }
    }

    /**
     * Retrieves the relevant form values for the cellular actions and 
     * returns it as an object
     * @returns {object} Object that represents the values in the form. 
     */
    var retrieveActionFormValues = function () {
        var simStatus = $(self.htmlElementIds.simStateSelect).val();
        var subscriptionPackage = $(self.htmlElementIds.subscriptionPackageSelect).val();
        var locale = $(self.htmlElementIds.localeSelect).val();

        return {
            subscriptionPackage: subscriptionPackage,
            simStatus: simStatus,
            locale: locale
        }
    }

    /**
     * Generate an CellularActionRequestModel object from the form inputs. Used
     * to send to the CellularActionUpdateRequest api end point.
     * @returns {object} The CellularActionRequestModel
     */
    var generateActionUpdateRequestFromInputs = function () {
        var cellularCellularActionRequestModel = {
            cellularActions: []
        };
        var currentFormValues = retrieveActionFormValues();
        if (currentFormValues.subscriptionPackage !== self.initialCellActionSettings.subscriptionPackage) {
            cellularCellularActionRequestModel.cellularActions.push({
                type: self.actionTypes.updateSubscriptionPackage,
                previousValue: self.initialCellActionSettings.subscriptionPackage,
                value: currentFormValues.subscriptionPackage
            });
        }
        if (currentFormValues.simStatus !== self.initialCellActionSettings.simStatus) {
            cellularCellularActionRequestModel.cellularActions.push({
                type: self.actionTypes.updateStatus,
                previousValue: self.initialCellActionSettings.simStatus,
                value: currentFormValues.simStatus
            });
        }
        if (currentFormValues.locale != self.initialCellActionSettings.locale) {
            cellularCellularActionRequestModel.cellularActions.push({
                type: self.actionTypes.updateLocale,
                previousValue: self.initialCellActionSettings.locale,
                value: currentFormValues.locale
            });
        }
        cellularCellularActionRequestModel.deviceId = self.deviceId;
        return cellularCellularActionRequestModel;
    }

    var toggleLoadingElement = function (visible) {
        if (visible) {
            $(self.htmlElementIds.loadingElement).show();
        } else {
            $(self.htmlElementIds.loadingElement).hide();
        }
    }

    /**
     * Generic function for post action request success. Will reload the cellular information details.
     * @param {any} response the data returned by the api
     * @returns {Promise} Returns the promise returned by IoTApp.DeviceDetails.onCellularDetailsDone
     */
    var onActionRequestSuccess = function (response) {
        return IoTApp.DeviceDetails.onCellularDetailsDone(response);
    }

    /**
     * Generic function for post action request error
     * @param {any} error The error returned from the api
     * @returns {void} 
     */
    var onActionRequestError = function (error) {
        self.toggleLoadingElement(false);
        IoTApp.DeviceDetails.renderRetryError(resources.unableToRetrieveDeviceFromService,
            $('#details_grid_container'),
            function () { IoTApp.DeviceDetails.getDeviceDetailsView(deviceId); });
        console.error(error);
    }

    /*
     * Event Handlers and event handler registration
     */

    /**
     * Callback for the action form save button.
     * @returns {Promise} The promise returned from posting to the api
     */
    var saveActionsOnClick = function () {
        toggleLoadingElement(true);
        var requestModel = generateActionUpdateRequestFromInputs();
        if (requestModel.cellularActions.length <= 0) {
            toggleLoadingElement(false);
            toggleInputDisabledProperty(true);
            return $.Deferred().resolve().promise();
        }
        IoTApp.CellularActions.postActionRequest(requestModel).then(onActionRequestSuccess, onActionRequestError);
    }

    /**
     * Callback for the edit button on the actions form
     * @returns {void}
     */
    var editActionsOnClick = function () {
        toggleInputDisabledProperty(false);
    }

    var cancelEditOnClick = function() {
        toggleInputDisabledProperty(true);
        resetInputsToInitial();
    }

    var goBackOnClick = function() {
        IoTApp.DeviceDetails.getDeviceDetailsView();
    }

    var attachEventHandlers = function () {
        $(self.htmlElementIds.editCellularInformation).click(editActionsOnClick);
        $(self.htmlElementIds.saveCellularInformation).click(saveActionsOnClick);
        $(self.htmlElementIds.cancelEditCellularInformation).click(cancelEditOnClick);
        $(self.htmlElementIds.goBackToDeviceDetails).click(goBackOnClick);
    }

    /*
    * Initialization
    */
    var init = function () {
        var deviceId = IoTApp.Helpers.DeviceIdState.getDeviceIdFromCookie();
        if (deviceId) {
            self.deviceId = IoTApp.Helpers.DeviceIdState.getDeviceIdFromCookie();
        }
        if (!self.deviceId) throw new Error("No device ID found in cookie.");
        self.initialCellActionSettings = retrieveActionFormValues();
        toggleInputDisabledProperty(true);
        attachEventHandlers();
    }
    return {
        init: init
    }
}, [jQuery, resources]);
