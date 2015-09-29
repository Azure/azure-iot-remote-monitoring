IoTApp.createModule('IoTApp.AlertHistoryTable', function () {
    "use strict";

    var getDataUri;
    var refreshMilliseconds;
    var timerId;

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

        setId();
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
            "dom": "<'dataTables_header alertHeader'i>",
            "ajax": { 
                url: getDataUri, 
                error: onError,
                cache: false
            },
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
                        return htmlEncode(data);
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

    var onXhr = function onXhr(e, settings) {
        if (handleRequestError(settings)) {
            if (refreshMilliseconds) {
                if (timerId) {
                    clearTimeout(timerId);
                    timerId = null;
                }

                timerId = setTimeout(reloadGrid, refreshMilliseconds);
            }
        }
    };

    var reloadGrid = function () {
        self.dataTable.ajax.reload();
    }

    var setId = function () {
        var alertHistoryType = resources.alertHistoryType;
        $("div.dashboardAlertHistory").attr("id", alertHistoryType);
    }

    return {
        init: init,
        reloadGrid: reloadGrid,
    }
}, [jQuery, resources]);