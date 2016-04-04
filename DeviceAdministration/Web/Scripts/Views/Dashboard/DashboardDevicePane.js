IoTApp.createModule(
    'IoTApp.Dashboard.DashboardDevicePane',
    function initDashboardDevicePane() {
        'use strict';
        $('#loadingElement').show();

        var currentDeviceId;
        var loadDataUrlBase;
        var refreshMilliseconds;
        var timerId;
        var telemetryDataUrl;
        var telemetryGridRefreshData;
        var telemetryHistoryRefreshData;

        var init = function init(settings) {

            loadDataUrlBase = settings.loadDataUrlBase;
            refreshMilliseconds = settings.refreshMilliseconds;
            telemetryGridRefreshData = settings.telemetryGridRefreshData;
            telemetryHistoryRefreshData = settings.telemetryHistoryRefreshData;

            settings.selectionDropDown.change(
                function () {
                    if (this.value) {
                        updateDeviceId(this.value);
                    }
                });
            };

        var onRequestComplete = function onRequestComplete(requestObj, status) {
            if (timerId) {
                clearTimeout(timerId);
                timerId = null;
            }

            if (refreshMilliseconds) {
                timerId = setTimeout(refreshData, refreshMilliseconds)
            }
        };

        var refreshData = function refreshData() {
            if (telemetryDataUrl) {

                $.ajax({
                    cache: false,
                    complete: onRequestComplete,
                    url: telemetryDataUrl
                }).done(
                    function telemetryReadDone(data) {
                        if (currentDeviceId !== data.deviceId) {
                            return;
                        }

                        if (telemetryGridRefreshData) {
                            if (data.deviceTelemetryModels) {
                                telemetryGridRefreshData(data.deviceTelemetryModels, data.deviceTelemetryFields);
                            } else {
                                telemetryGridRefreshData([], data.deviceTelemetryFields);
                            }
                        }

                        if (telemetryHistoryRefreshData) {
                            if (data.deviceTelemetrySummaryModel) {
                                telemetryHistoryRefreshData(
                                    data.deviceTelemetrySummaryModel.minimumHumidity || 0.0,
                                    data.deviceTelemetrySummaryModel.maximumHumidity || 0.0,
                                    data.deviceTelemetrySummaryModel.averageHumidity || 0.0);
                            } else {
                                telemetryHistoryRefreshData(0.0, 0.0, 0.0);
                            }
                        }

                        $('#loadingElement').hide();
                    }
                ).fail(function () {
                    if (timerId) {
                        clearTimeout(timerId);
                        timerId = null;
                    }

                    IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveDeviceTelemetryFromService);

                    if (refreshMilliseconds) {
                        timerId = setTimeout(refreshData, refreshMilliseconds)
                    }
                });
            }
        };

        var updateDeviceId = function updateDeviceId(deviceId) {
            $('#loadingElement').show();
            if (timerId) {
                clearTimeout(timerId);
                timerId = null;
            }

            if (deviceId === '') {

                currentDeviceId = '';
                telemetryGridRefreshData([], null);
                telemetryHistoryRefreshData(0.0, 0.0, 0.0);
                $('#loadingElement').hide();

            } else if (loadDataUrlBase && deviceId) {

                telemetryDataUrl =
                    loadDataUrlBase + encodeURIComponent(deviceId);
                currentDeviceId = deviceId;

                refreshData();
                IoTApp.AlertHistoryTable.setSelectedDevice(deviceId);
            }
        };

        var setSelectedDevice = function (deviceId) {
            $('#deviceSelection > option').each(function () {
                if (this.value === deviceId) {
                    $(this).prop("selected", true);
                } else {
                    $(this).removeProp("selected");
                }
            });
            updateDeviceId(deviceId);
        }

        return {
            init: init,
            updateDeviceId: updateDeviceId,
            setSelectedDevice: setSelectedDevice
        };
    },
    [jQuery, powerbi]);