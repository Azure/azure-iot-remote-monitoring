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
        saveCellularInformation: "#saveCellularInformation",
        editCellularInformation: "#editCellularInformation",
        loadingElement: "#loadingElement",
        updateCellularInformationResults: "#updateCellularInformationResults"
    }

    /*
     * Utility functions
     */

    /**
     * Toggle the actions for to between enabled and disabled
     * @param {boolean} disabled If true disables the form. If false enables the form.
     * @returns {void}
     */
    var toggleInputDisabledProperty = function (disabled) {
        if (disabled) {
            $(self.htmlElementIds.simStateSelect).attr("disabled", "disabled");
            $(self.htmlElementIds.subscriptionPackageSelect).attr("disabled", "disabled");
            $(self.htmlElementIds.saveCellularInformation).attr("disabled", "disabled");
            $(self.htmlElementIds.editCellularInformation).removeAttr("disabled");
            $(self.updateCellularInformationResults).hide();
        } else {
            $(self.htmlElementIds.simStateSelect).removeAttr("disabled");
            $(self.htmlElementIds.subscriptionPackageSelect).removeAttr("disabled");
            $(self.htmlElementIds.saveCellularInformation).removeAttr("disabled");
            $(self.htmlElementIds.editCellularInformation).attr("disabled", "disabled");
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
        return {
            subscriptionPackage: subscriptionPackage,
            simStatus: simStatus
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
     * @param {any} data the data returned by the api
     * @returns {any} returns the data passed in so you can chain to another function with .then()
     */
    var onActionRequestSuccess = function (data) {
        return IoTApp.DeviceDetails.getCellularDetailsView()
            .then(function () {
                return data;
            });
    }

    /**
     * Generic function for post action request error
     * @param {any} error The error returned from the api
     * @returns {void} 
     */
    var onActionRequestError = function (error) {
        toggleLoadingElement(false);
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
            return $.Deferred().resolve().promise();
        }
        IoTApp.CellularActions.postActionRequest(requestModel).then(function (response) {
            IoTApp.DeviceDetails.onCellularDetailsDone(response);
        }, function () {
            self.toggleLoadingElement(false);
            IoTApp.DeviceDetails.renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
        });
    }

    /**
     * Callback for the edit button on the actions form
     * @returns {void}
     */
    var editActionsOnClick = function () {
        toggleInputDisabledProperty(false);
    }

    var attachEventHandlers = function () {
        $(self.htmlElementIds.editCellularInformation).click(editActionsOnClick);
        $(self.htmlElementIds.saveCellularInformation).click(saveActionsOnClick);
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
