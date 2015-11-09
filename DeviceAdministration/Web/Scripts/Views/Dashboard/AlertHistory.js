IoTApp.createModule('IoTApp.AlertHistoryTable', function () {
    "use strict";

    var getDataUri;
    var refreshMilliseconds;
    var timerId;
    var selectedDeviceId;
    var deviceIdsByRow;
    var deviceIdCellIndex = 1;

    var self = this;

    var handleRequestError = function handleRequestError(settings) {
        if (settings &&
            settings.jqXHR &&
            ((settings.jqXHR.status === 401 ||
                settings.jqXHR.status === 403))) {
            window.location('/Account/SignIn');
            return false;
        }
        else {
            return true;
        }
    };

    var init = function init(alertHistoryTableSettings) {

        setAlertHistoryTypeClass();
        self.dataTableContainer = alertHistoryTableSettings.dataTable;
        getDataUri = alertHistoryTableSettings.getDataUri;
        refreshMilliseconds = alertHistoryTableSettings.refreshMilliseconds;

        _initializeDatatable();
    };

    var _initializeDatatable = function () {

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return data ? $('<div/>').text(data).html() : null;
        }

        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "bSort": false,
            "displayStart": 0,
            "paging": false,
            "lengthChange": false,
            "processing": true,
            "serverSide": true,
            "dom": "<'dataTables_header dashboard_alert_history__alertHeader'i>",
            "ajax": {
                url: getDataUri,
                error: onError,
                cache: false,
                "fnDrawCallback": onTableDrawn,
            },
            "fnDrawCallback": onTableDrawn,
            "language": {
                "info": resources.alarmHistory,
                "infoEmpty": resources.alarmHistory,
                "infoFiltered": ''
            },
            "columns": [
                {
                    "data": "timestamp",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Dates.localizeDate(data, 'L LTS');
                    },
                    "name": "timestamp"
                },
                {
                    "data": "deviceId",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "deviceId"
                },
                {
                    "data": "ruleOutput",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "ruleOutput"
                },
                {
                    "data": "value",
                    "mRender": function (data) {
                        return htmlEncode(IoTApp.Helpers.Numbers.localizeFromInvariant(data));
                    },
                    "name": "value"
                },
            ],
            "columnDefs": [
                {
                    "targets": [0, 1, 2, 3],
                    "className": 'table_alertHistory_issueType',
                    "width": "20%"

                }
            ],
        });

        $(self.dataTableContainer).on("xhr.dt", onXhr);
        $(self.dataTableContainer).on("error.dt", onError);
    }

    var onError = function onError(args, settings) {
        if (handleRequestError(settings)) {
            if (timerId) {
                clearTimeout(timerId);
                timerId = null;
            }

            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveAlertsHistoryFromService);

            if (refreshMilliseconds) {
                timerId = setTimeout(reloadGrid, refreshMilliseconds);
            }
        }
    };

    var onXhr = function onXhr(e, settings, data) {
        if (handleRequestError(settings)) {
            if (typeof IoTApp.MapPane === "object" && data) {
                IoTApp.MapPane.setDeviceLocationData(
                    data.minLatitude,
                    data.minLongitude,
                    data.maxLatitude,
                    data.maxLongitude,
                    data.devices
                );
            }

            if (refreshMilliseconds) {
                if (timerId) {
                    clearTimeout(timerId);
                    timerId = null;
                }

                timerId = setTimeout(reloadGrid, refreshMilliseconds);
            }
        }
    };

    var onTableDrawn = function (settings) {
        stashIdsByRow(settings);
        updateSelectedRows();
    }

    var stashIdsByRow = function (settings) {
        self.deviceIdsByRow = [];
        if (settings.aoData !== null && settings.aoData.length > 0) {
            var dataArray = settings.aoData;
            var length = dataArray.length;
            for (var i = 0; i < length; i++) {
                if (dataArray[i].anCells !== null && dataArray[i].anCells.length > deviceIdCellIndex &&
                    dataArray[i].anCells[deviceIdCellIndex] !== null) {
                    var cell = dataArray[i].anCells[deviceIdCellIndex];
                    self.deviceIdsByRow.push(cell.textContent);
                }
            }
        }
    }

    var setSelectedDevice = function (deviceId) {
        self.selectedDeviceId = deviceId;
        updateSelectedRows();
    }

    var updateSelectedRows = function () {
        if (self.selectedDeviceId && self.deviceIdsByRow) {
            var length = self.deviceIdsByRow.length;
            for (var i = 0; i < length; i++) {
                var row = self.dataTable.row(i);
                var idForRow = self.deviceIdsByRow[i];
                if (self.deviceIdsByRow[i] === self.selectedDeviceId) {
                    row.nodes().to$().addClass("selectedAlert");
                } else {
                    row.nodes().to$().removeClass("selectedAlert");
                }
            }
        }
    }

    var reloadGrid = function () {
        self.dataTable.ajax.reload();
    }

    var setAlertHistoryTypeClass = function () {
        var alertHistoryType = resources.alertHistoryType;
        $("div.dashboard_alert_history").addClass(alertHistoryType);
    }

    return {
        init: init,
        reloadGrid: reloadGrid,
        setSelectedDevice: setSelectedDevice
    }
}, [jQuery, resources, IoTApp.MapPane]);