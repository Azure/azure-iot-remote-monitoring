IoTApp.createModule('IoTApp.ActionsIndex', function () {
    "use strict";

    var self = this;
    var init = function (actionProperties) {
        self.actionProperties = actionProperties;
        self.dataTableContainer = $('#actionTable');
        self.actionGrid = $(".details_grid");
        self.actionGridClosed = $(".details_grid_closed");
        self.actionGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");
        self.readonlyActions = resources.readonlyActions;

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

    var _selectRowFromDataTable = function (node) {
        node.addClass('selected');
    }

    var _setDefaultRowAndPage = function () {
        if (self.isDefaultDeviceDetailsAvailable === true) {
            self.isDefaultDeviceDetailsAvailable = false;
            var node = self.dataTable.row(self.defaultSelectedRow).nodes().to$();
            _selectRowFromDataTable(node);
        } else {
            // if selected device is no longer displayed in grid, then close the details pane
            closeAndClearProperties();
        }
    }

    var _initializeDatatable = function () {
        var onTableDrawn = function () {
            _setDefaultRowAndPage();
        };

        var onTableRowClicked = function () {
            var ruleOutput = this.cells[0].innerHTML;
            var actionId = this.cells[1].innerHTML;
            self.dataTable.$(".selected").removeClass("selected");
            $(this).addClass("selected");
            self.selectedRow = self.dataTable.row(this).index();
            if (self.readonlyActions == 'true') {
                self.actionProperties.init(ruleOutput, actionId);
            }
            else {
                self.actionProperties.readonlyActionState($('#details_grid_container'));
            }
        }

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return $('<div/>').text(data).html();
        }

        //$.fn.dataTable.ext.legacy.ajax = true;
        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": 0,
            "pagingType": "simple_numbers",
            "paging": false,
            "lengthChange": false,
            "processing": false,
            "serverSide": false,
            "dom": "<'dataTables_header'i>lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.actionsList + " (_TOTAL_)"
            },
            "columns": [
                {
                    "data": "ruleOutput",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "ruleOutput"
                },
                {
                    "data": "actionId",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "actionId"
                },
                {
                    "data": "numberOfDevices",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "numberOfDevices"
                }
            ],
            "columnDefs": [
                { className: "table_actions_status", "targets": [0] },
                { "searchable": true, "targets": [1] }
            ],
            "order": [[0, "asc"]]
        });

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function (e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveActionFromService);
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
            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };

        self.getActionList = $.ajax({
            "dataType": 'json',
            'type': 'POST',
            'url': '/api/v1/actions/list',
            'cache': false,
            'data': data,
            'success': successCallback
        });

        self.getActionList.fail(function () {
            $('.loader_container').hide();
            IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveActions);
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
        self.actionGrid.toggle();
        self.actionGridClosed.toggle();
        setGridWidth();
    }

    // close the device details pane (called when device is no longer shown)
    var closeAndClearProperties = function () {
        // only toggle if we are already open!
        if (self.actionGrid.is(":visible")) {
            toggleProperties();
        }

        // clear the details pane (so it's clean!)
        // Even though we're working with actions, we still use the no_device_selected class
        // So we don't have to duplicate a bunch of styling for now
        var noActionSelected = resources.noActionSelected;
        $('#details_grid_container').html('<div class="details_grid__no_selection">' + noActionSelected + '</div>');
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

        var actionGridVisible = $(".details_grid").is(':visible');

        var actionGridWidth = actionGridVisible ? self.actionGrid.width() : self.actionGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - actionGridWidth - 98;
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

    IoTApp.ActionsIndex.init(IoTApp.ActionProperties);
});