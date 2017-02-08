IoTApp.createModule('IoTApp.JobIndex', function () {
    "use strict";

    var self = this;
    var init = function (jobProperties) {
        self.jobProperties = jobProperties;
        self.dataTableContainer = $('#jobTable');
        self.jobGrid = $(".details_grid");
        self.jobGridClosed = $(".details_grid_closed");
        self.jobGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");
        self.reloadGrid = this.reloadGrid;
        self.selectedJobId = resources.jobId;

        _initializeDatatable();

        self.buttonDetailsGrid.on("click", function () {
            toggleProperties();
            fixHeights();
        });

        $(window).on("load", function () {
            fixHeights();
            setGridWidth();
        });

        $(window).on("resize", function () {
            fixHeights();
            setGridWidth();
        });
    }

    var _selectRowFromDataTable = function (row) {
        var rowData = row.data();
        if (rowData != null) {
            self.dataTable.$(".selected").removeClass("selected");
            row.nodes().to$().addClass("selected");
            self.selectedRow = row.index();
            self.selectedJobId = rowData["jobId"];
            self.jobProperties.init(rowData["jobId"], rowData["jobName"], rowData["operationType"], self.reloadGrid);
        }
    }

    var _setDefaultRowAndPage = function () {
        if (self.isDefaultJobAvailable === true) {
            var node = self.dataTable.row(self.defaultSelectedRow);
            _selectRowFromDataTable(node);
        } else {
            // if selected job is no longer displayed in grid, then close the details pane
            closeAndClearProperties();
        }
    }

    var changeJobStatus = function () {
        self.dataTable.rows(function (index, data, node) {
            var className = 'status_' + data.status;
            if (data.status === resources.allJobStatus[3].toLowerCase() && data.failedCount > 0) {
                className = className + '_with_errors';
            }
            var statusNode = self.dataTable.cell(index, 0).node();
            $(statusNode).addClass(className);
        });
    }

    var _initializeDatatable = function () {
        var onTableDrawn = function () {
            changeJobStatus();
            _setDefaultRowAndPage();

            var pagingDiv = $('#jobTable_paginate');
            if (pagingDiv) {
                if (self.dataTable.page.info().pages > 1) {
                    $(pagingDiv).show();
                } else {
                    $(pagingDiv).hide();
                }
            }
        };

        var onTableRowClicked = function () {
            _selectRowFromDataTable(self.dataTable.row(this));
        }

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return (data == 0 || data) ? $('<div/>').text(data).html() : null;
        }

        //$.fn.dataTable.ext.legacy.ajax = true;
        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": 0,
            "pagingType": "simple_numbers",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": false,
            "dom": "<'dataTables_header'i<'#toolbar_area.job_list_toolbar'>>lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.jobsList + " (_TOTAL_)",
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
                    "data": "jobName",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "jobName"
                },
                {
                    "data": "filterName",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "filterName"
                },
                {
                    "data": "operationType",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "operations"
                },
                {
                    "data": "startTime",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Dates.localizeDate(data, 'L LTS');
                    },
                    "name": "startTime"
                },
                {
                    "data": "endTime",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Dates.localizeDate(data, 'L LTS');
                    },
                    "name": "endTime"
                },
                {
                    "data": "deviceCount",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "deviceCount"
                },
                {
                    "data": "succeededCount",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "succeededCount"
                },
                {
                    "data": "failedCount",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "failedCount"
                }
            ],
            "columnDefs": [
                { className: "table_status", targets: [0] },
                { className: "table_truncate_with_max_width", targets: [1, 2] },
                { searchable: true, targets: [1] }
            ],
            "order": [[4, "desc"]],
            "drawCallback": function (settings) {
                IoTApp.Helpers.String.setupTooltipForEllipsis(self.dataTableContainer);
            }
        });

        $('#toolbar_area').html($('#toolbarTemplate').html());

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function (e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveJobFromService);
        });

        self.dataTableContainer.find("tbody").delegate("tr", "click", onTableRowClicked);

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        $('.loader_container').show();
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            if (processing) {
                $('.loader_container').show();
            }
            else {
                $('.loader_container').hide();
            }
            _setGridContainerScrollPositionIfRowIsSelected();
        });

        var _setGridContainerScrollPositionIfRowIsSelected = function () {
            if ($("tbody .selected").length > 0) {
                $('.grid_container')[0].scrollTop = $("tbody .selected").offset().top - $('.grid_container').offset().top - 50;
            }
        }
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {
            if (self.selectedJobId) {
                // iterate through the data before passing it on to grid, and try to
                // find and save the selected JobId value

                // reset this value each time
                self.isDefaultJobAvailable = false;

                for (var i = 0, len = json.data.length; i < len; ++i) {
                    var data = json.data[i];
                    if (data &&
                        data.jobId === self.selectedJobId) {
                        self.defaultSelectedRow = i;
                        self.isDefaultJobAvailable = true;
                        break;
                    }
                }
            }

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };

        clearRefeshTimeout();

        if (self.getJobList) {
            self.getJobList.abort();
        }

        self.getJobList = $.ajax({
            "dataType": 'json',
            'type': 'GET',
            'url': '/api/v1/jobs',
            'cache': false,
            'data': data,
            'success': successCallback
        }).fail(function (xhr, status, error) {
            $('.loader_container').hide();
            if (status !== 'abort') {
                IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveJobs);
                setupRefreshTimeout();
            }
        }).done(function () {
            setupRefreshTimeout();
        });
    }

    function setupRefreshTimeout() {
        // Disable auto refresh feature for now. Can be enable when the detail panel can be refreshed in client side
        if (self.autoRefresh) {
            clearRefeshTimeout();
            self.refreshTimeout = setTimeout(reloadGrid, 10000, true);
        }
    }

    function clearRefeshTimeout() {
        if (self.refreshTimeout) {
            clearTimeout(self.refreshTimeout);
        }
    }

    /* Set the heights of scrollable elements for correct overflow behavior */
    function fixHeights() {
        // set height of device details pane
        var fixedHeightVal = $(window).height() - $(".navbar").height();
        $(".height_fixed").height(fixedHeightVal);
    }

    /* Hide/show the Device Details pane */
    var toggleProperties = function () {
        self.jobGrid.toggle();
        self.jobGridClosed.toggle();
        setGridWidth();
    }

    // close the device details pane (called when device is no longer shown)
    var closeAndClearProperties = function () {
        // only toggle if we are already open!
        if (self.jobGrid.is(":visible")) {
            toggleProperties();
        }

        // clear the details pane (so it's clean!)
        // Even though we're working with jobs, we still use the no_device_selected class
        // So we don't have to duplicate a bunch of styling for now
        var noJobSelected = resources.noJobSelected;
        $('#details_grid_container').html('<div class="details_grid__no_selection">' + noJobSelected + '</div>');
    }

    var setGridWidth = function () {
        var gridContainer = $(".grid_container");

        // Set the grid VERY NARROW initially--otherwise if panels are expanding, 
        // the existing grid will be too wide to fit, and it will be pushed *below* the 
        // side panes--roughly doubling the height of the content. In this case, 
        // the browser will add a vertical scrollbar on the window.
        //
        // If this happens, the code in this function will collect data
        // with the grid pushed below and a scrollbar on the right--so 
        // $(window).width() will be too narrow (by the width of the scrollbar).
        // When the grid is correctly sized, it will move back up, and the 
        // browser will remove the scrollbar. But at that point there will be a gap
        // the width of the scrollbar, as the final measurement will be off by 
        // the width of the scrollbar.

        // set grid container to 1 px width--see comment block above
        gridContainer.width(1);

        var jobGridVisible = $(".details_grid").is(':visible');

        var jobGridWidth = jobGridVisible ? self.jobGrid.width() : self.jobGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - jobGridWidth - 98;
        gridContainer.width(gridWidth);
    }

    var reloadGrid = function (keepPaging) {
        self.dataTable.ajax.reload(null, !keepPaging);
    }

    return {
        init: init,
        toggleProperties: toggleProperties,
        reloadGrid: reloadGrid
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.JobIndex.init(IoTApp.JobProperties);
});

