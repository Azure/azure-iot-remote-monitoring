IoTApp.createModule('IoTApp.DeviceRulesIndex', function () {
    "use strict";

    var self = this;
    var init = function (ruleProperties) {
        self.ruleProperties = ruleProperties;
        self.dataTableContainer = $('#ruleTable');
        self.ruleGrid = $(".details_grid");
        self.ruleGridClosed = $(".details_grid_closed");
        self.ruleGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");
        self.reloadGrid = this.reloadGrid;

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
            self.selectedRuleId = rowData["ruleId"];
            self.ruleProperties.init(rowData["deviceID"], rowData["ruleId"], self.reloadGrid);
        }
    }

    var _setDefaultRowAndPage = function () {
        if (self.isDefaultRuleAvailable === true) {
            var node = self.dataTable.row(self.defaultSelectedRow);
            _selectRowFromDataTable(node);
        } else {
            // if selected rule is no longer displayed in grid, then close the details pane
            closeAndClearProperties();
        }
    }

    var changeRuleStatus = function () {
        var tableStatus = self.dataTable;

        var cells_status_false = tableStatus.cells(".table_status:contains('false')").nodes();
        $(cells_status_false).addClass('status_false');
        $(cells_status_false).html(resources.disabled);

        var cells_status_true = tableStatus.cells(".table_status:contains('true')").nodes();
        $(cells_status_true).addClass('status_true');
        $(cells_status_true).html(resources.enabled);
    }

    var _initializeDatatable = function () {
        var onTableDrawn = function () {
            changeRuleStatus();
            _setDefaultRowAndPage();

            var pagingDiv = $('#ruleTable_paginate');
            if (pagingDiv) {
                if (self.dataTable.page.info().pages > 1) {
                    $(pagingDiv).show();
                } else {
                    $(pagingDiv).hide();
                }
            }
        };

        var onTableRowClicked = function () {
            _selectRowFromDataTable(self.dataTable.row(this))
        }

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return data ? $('<div/>').text(data).html() : null;
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
            "dom": "<'dataTables_header'i>lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.rulesList + " (_TOTAL_)",
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": [
                {
                    "data": "enabledState",
                    "mRender": function (data) {
                        if (data === false) {
                            return htmlEncode("false");
                        } else if (data) {
                            return htmlEncode("true");
                        }
                        return htmlEncode(data);
                    },
                    "name": "ruleEnabledState"
                },
                {
                    "data": "ruleId",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "ruleId"
                },
                {
                    "data": "deviceID",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "deviceID"
                },
                {
                    "data": "dataField",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "dataField"
                },
                {
                    "data": "operator",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "operator"
                },
                {
                    "data": "threshold",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Numbers.localizeNumber(data);
                    },
                    "name": "threshold"
                },
                {
                    "data": "ruleOutput",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "ruleOutput"
                }
            ],
            "columnDefs": [
                { className: "table_status", "targets": [0] },
                { "searchable": true, "targets": [1] }
            ],
            "order": [[2, "asc"]]
        });

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function (e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveRuleFromService);
        });

        self.dataTableContainer.find("tbody").delegate("tr", "click", onTableRowClicked);

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        $('.loader_container').css('display', 'block');
        $('.loader_container').css('background-color', '#ffffff');
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            $('.loader_container').css('display', processing ? 'block' : 'none');
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
            if (self.selectedRuleId) {
                // iterate through the data before passing it on to grid, and try to
                // find and save the selected deviceID value

                // reset this value each time
                self.isDefaultRuleAvailable = false;

                for (var i = 0, len = json.data.length; i < len; ++i) {
                    var data = json.data[i];
                    if (data &&
                        data.ruleId === self.selectedRuleId) {
                        self.defaultSelectedRow = i;
                        self.isDefaultRuleAvailable = true;
                        break;
                    }
                }
            } 

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };
        
        self.getRuleList = $.ajax({
            "dataType": 'json',
            'type': 'POST',
            'url': '/api/v1/devicerules/list',
            'cache': false,
            'data': data,
            'success': successCallback
        });

        self.getRuleList.fail(function () {
            $('.loader_container').hide();
            IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveRules);
        });
    }

    /* Set the heights of scrollable elements for correct overflow behavior */
    function fixHeights() {
        // set height of device details pane
        var fixedHeightVal = $(window).height() - $(".navbar").height();
        $(".height_fixed").height(fixedHeightVal);
    }

    /* Hide/show the Device Details pane */
    var toggleProperties = function () {
        self.ruleGrid.toggle();
        self.ruleGridClosed.toggle();
        setGridWidth();
    }

    // close the device details pane (called when device is no longer shown)
    var closeAndClearProperties = function () {
        // only toggle if we are already open!
        if (self.ruleGrid.is(":visible")) {
            toggleProperties();
        }
        
        // clear the details pane (so it's clean!)
        // Even though we're working with rules, we still use the no_device_selected class
        // So we don't have to duplicate a bunch of styling for now
        var noRuleSelected = resources.noRuleSelected;
        $('#details_grid_container').html('<div class="details_grid__no_selection">' + noRuleSelected + '</div>');
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

        var ruleGridVisible = $(".details_grid").is(':visible');

        var ruleGridWidth = ruleGridVisible ? self.ruleGrid.width() : self.ruleGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - ruleGridWidth - 98;
        gridContainer.width(gridWidth);
    }

    var reloadGrid = function () {
        self.dataTable.ajax.reload();
    }

    return {
        init: init,
        toggleProperties: toggleProperties,
        reloadGrid: reloadGrid
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceRulesIndex.init(IoTApp.DeviceRuleProperties);
});

