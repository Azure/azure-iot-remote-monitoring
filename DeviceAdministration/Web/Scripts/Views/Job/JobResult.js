IoTApp.createModule('IoTApp.JobResult', function ($, resources) {
    "use strict";

    var self = this;
    var init = function (jobId, jobName, operationType) {
        self.jobId = jobId;
        self.operationType = operationType;
        IoTApp.Controls.Dialog.create({
            dialogId: 'jobResultDialog',
            templateId: '#jobResultTemplate',
            title: resources.jobResultsFor.replace('{0}', '<span class="item__description">' + jobName + '</span>')
        });
        self.dataTableContainer = $('#jobResultTable');
        
        $(window).on("resize", function () {
            updatePageSize();
        });

        _initializeDatatable();
    }

    var changeJobStatus = function () {
        self.dataTable.rows(function (index, data, node) {
            var className = 'status_' + data.status;
            var statusNode = self.dataTable.cell(index, 0).node();
            $(statusNode).addClass(className);
        });
    }

    var _initializeDatatable = function () {
        var onTableDrawn = function () {
            changeJobStatus();

            var pagingDiv = $('#jobResultTable_paginate');
            if (pagingDiv) {
                if (self.dataTable.page.info().pages > 1) {
                    $(pagingDiv).show();
                } else {
                    $(pagingDiv).hide();
                }
            }
        };

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return (data == 0 || data) ? $('<div/>').text(data).html() : null;
        }

        //$.fn.dataTable.ext.legacy.ajax = true;
        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "pageLength": getPageSize(),
            "displayStart": 0,
            "pagingType": "simple_numbers",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": false,
            "dom": "lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": [
                 {
                     "data": "status",
                     "mRender": function (data) {
                         return htmlEncode(data);
                     },
                     "name": "status"
                 },
                {
                    "data": "deviceId",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "deviceId"
                },
                {
                    "data": "result",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "result",
                    "visible": self.operationType == "Method"
                },
                {
                    "data": "valueReturned",
                    "mRender": function (data) {
                        return renderPayload(data);
                    },
                    "name": "valueReturned",
                    "visible": self.operationType == "Method",
                    "className": "table_truncate_with_max_width"
                },
                {
                    "data": "timeCreated",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Dates.localizeDate(data, 'L LTS');
                    },
                    "name": "timeCreated"
                },
                {
                    "data": "timeUpdated",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Dates.localizeDate(data, 'L LTS');
                    },
                    "name": "timeUpdated"
                }
            ],
            "columnDefs": [
                { className: "table_status", targets: [0] }
            ],
            "order": [[1, "asc"]],
            "drawCallback": function (settings) {
                IoTApp.Helpers.String.setupTooltipForEllipsis(self.dataTableContainer, function () {
                    return $(this).text().replace(/\n/g, "<br />");
                });
            }
        });

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function (e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveJobFromService);
        });

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        $('#loadingElementJobResult').show();
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            if (processing) {
                $('#loadingElementJobResult').show();
            }
            else {
                $('#loadingElementJobResult').hide();
            }
        });
    }

    var createFilterButton = function (text, status, count, selected) {
        var button = $('<a class="job_result_status_filter" />')
            .text(text + ' (' + count + ')')
            .click(function (e) {
                selectFilterButton($(this));
                self.dataTable.column(0).search(status, true, false);
                self.dataTable.page(0).draw();
            });

        if (selected) {
            selectFilterButton(button);
        }

        return button;
    }

    var selectFilterButton = function (button) {
        $('.job_result_status_filter').removeClass('job_result_status_filter_selected');
        button.addClass('job_result_status_filter_selected');
    }

    var renderPayload = function (json) {
        if (!json) return '';

        try {
            json = JSON.parse(json);
        } 
        catch (e) {
        }

        var content = '';

        if ($.isPlainObject(json)) {
            var items = [];
            $.each(json, function (key, value) {
                items.push(key + ": " + value);
            });

            content = items.join(', \n');
        }
        else {
            content = json;
        }
        
        return $('<div />').text(content).html();
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {
            json.data = json.data.map(function (item, index) {
                var methodResponse = item.outcome.deviceMethodResponse;
                var resp = (item.error && item.error.description) || (methodResponse && JSON.stringify(methodResponse.payload));
                var statusCode = (item.error && item.error.code) || (methodResponse && methodResponse.status);

                return {
                    status: item.status,
                    deviceId: item.deviceId,
                    result: statusCode,
                    valueReturned: resp,
                    timeCreated: item.createdDateTimeUtc,
                    timeUpdated: item.lastUpdatedDateTimeUtc
                };

            });

            var $filters = $('.job_result_status_filters').empty();
            createFilterButton(resources.all, '', json.data.length, true).appendTo($filters);

            resources.allDeviceJobStatus.forEach(function (status) {
                var count = json.data.filter(function (item) {
                    return item.status == status.toLowerCase();
                }).length;

                if (count > 0) {
                    createFilterButton(status, status.toLowerCase(), count).appendTo($filters);
                }
            });

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };

        self.getJobResultList = $.ajax({
            "dataType": 'json',
            'type': 'GET',
            'url': '/api/v1/jobs/' + self.jobId + '/results',
            'cache': false,
            'data': data,
            'success': successCallback
        }).fail(function (xhr, status, error) {
            $('#loadingElementJobResult').hide();
            if (status !== 'abort') {
                IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveJobFromService);
            }
        });
    }

    var getPageSize = function () {
        var rowHeight = 36;
        var totalHeight = $('.dialog_dialog').height() - $('.job_result_header').outerHeight() - 135;
        return Math.floor(totalHeight / rowHeight);
    }

    var updatePageSize = function () {
        self.dataTable.page.len(getPageSize()).draw();
    }

    return {
        init: init,
    }
}, [jQuery, jobResultResources]);
