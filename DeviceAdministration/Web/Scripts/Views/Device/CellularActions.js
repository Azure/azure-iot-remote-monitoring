IoTApp.createModule("IoTApp.CellularActions", function () {
    "use strict";
    /*
     * Module variable initialization
     */
    var self = this;
    self.ActionRequestEndpoint = "/Device/CellularActionRequest";
    self.deviceId = null;
    self.initialCellActionSettings = null;
    self.actionTypes = {
        updateStatus: "UpdateStatus",
        updateSubscriptionPackage: "UpdateSubscriptionPackage",
        reconnectDevice: "ReconnectDevice",
        sendSms: "SendSms"
    }
    self.htmlElementIds = {
        simStateSelect: "#simStateSelect",
        subscriptionPackageSelect: "#subscriptionPackageSelect",
        resetDeviceConnection: "#resetDeviceConnection",
        saveActions: "#saveActions",
        editActions: "#editActions",
        sendSms: "#sendSms",
        loadingElement: "#loadingElement"
    }
    $.ajaxSetup({ cache: false });

    /*
     * API
     */

    /**
     * Post to the cellular actions request endpoint
     * @param {object} data A CellularActionRequestModel
     * @returns {Promise} A Promise that resolves/rejects on the api request
     */
    var postActionRequest = function (data) {
        return $.post(self.ActionRequestEndpoint, data);
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
            $(self.htmlElementIds.resetDeviceConnection).attr("disabled", "disabled");
            $(self.htmlElementIds.saveActions).attr("disabled", "disabled");
            $(self.htmlElementIds.editActions).removeAttr("disabled");
        } else {
            $(self.htmlElementIds.simStateSelect).removeAttr("disabled");
            $(self.htmlElementIds.subscriptionPackageSelect).removeAttr("disabled");
            $(self.htmlElementIds.resetDeviceConnection).removeAttr("disabled");
            $(self.htmlElementIds.saveActions).removeAttr("disabled");
            $(self.htmlElementIds.editActions).attr("disabled", "disabled");
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
                currentValue: self.initialCellActionSettings.simStatus,
                newValue: currentFormValues.subscriptionPackage
            });
        }
        if (currentFormValues.simStatus !== self.initialCellActionSettings.simStatus) {
            cellularCellularActionRequestModel.cellularActions.push({
                type: self.actionTypes.updateStatus,
                currentValue: self.initialCellActionSettings.simStatus,
                newValue: currentFormValues.simStatus
            });
        }
        cellularCellularActionRequestModel.deviceId = self.deviceId;
        return cellularCellularActionRequestModel;
    }

    /**
     * Generate an CellularActionRequestModel from an action type string.
     * @param {string} type : string representing the action type.
     * @returns {object} The CellularActionRequestModel
     */
    var generateActionUpdateRequestFromType = function (type) {
        var cellularCellularActionRequestModel = {
            cellularActions: []
        };
        switch (type) {
            case self.actionTypes.reconnectDevice:
                {
                    cellularCellularActionRequestModel.cellularActions.push({
                        type: self.actionTypes.reconnectDevice
                    });
                    break;
                }
            case self.actionTypes.sendSms:
                {
                    cellularCellularActionRequestModel.cellularActions.push({
                        type: self.actionTypes.sendSms
                    });
                    break;
                }
            default:
                {
                    break;
                }
        }
        return cellularCellularActionRequestModel;
    }

    var toggleLoadingElement = function (visible) {
        if (visible) {
            $(self.htmlElementIds.loadingElement).show();
        } else {
            $(self.htmlElementIds.loadingElement).hide();
        }
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
        return postActionRequest(requestModel)
            .then(function () {
                toggleLoadingElement(false);
            }, function () {
                console.error(error);
                toggleLoadingElement(false);
            });
    }

    /**
     * Callback for the reconnect device button
     *  @returns {Promise} The promise returned from posting to the api
     */
    var reconnectDeviceConnectionOnClick = function () {
        toggleLoadingElement(true);
        var requestModel = generateActionUpdateRequestFromType(self.actionTypes.reconnectDevice);
        return postActionRequest(requestModel)
            .then(function () {
                toggleLoadingElement(false);
            }, function () {
                console.error(error);
                toggleLoadingElement(false);
            });
    }

    /**
     * Callback for the send sms button
     *  @returns {Promise} The promise returned from posting to the api
     */
    var sendSmsOnClick = function () {
        toggleLoadingElement(true);
        var requestModel = generateActionUpdateRequestFromType(self.actionTypes.sendSms);
        return postActionRequest(requestModel)
            .then(function () {
                toggleLoadingElement(false);
            }, function () {
                console.error(error);
                toggleLoadingElement(false);
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
        $(self.htmlElementIds.editActions).click(editActionsOnClick);
        $(self.htmlElementIds.saveActions).click(saveActionsOnClick);
        $(self.htmlElementIds.sendSms).click(sendSmsOnClick);
    }

    /*
    * Initialization
    */
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
