IoTApp.createModule('IoTApp.DeviceIndex', function () {
    "use strict";

    var self = this;
    var init = function (deviceDetails) {
        self.deviceDetails = deviceDetails;
        self.dataTableContainer = $('#deviceTable');
        self.deviceGrid = $(".details_grid");
        self.deviceGridClosed = $(".details_grid_closed");
        self.deviceGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");
        self.buttonSearchPane = $(".search_container__search_subhead");
        self.searchPane = $(".search_container");
        self.searchPaneClosed = $(".search_container_closed");

        Cookies.json = true;

        // close this initially
        self.searchPane.hide();

        _initializeDatatable();

        self.buttonDetailsGrid.on("click", function () {
            toggleDetails();
            fixHeights();
        });

        self.buttonSearchPane.on("click", function () {
            toggleSearchPane();
        });

        self.searchPaneClosed.on("click", function () {
            toggleSearchPane();
        });

        $(window).on("load", function () {
            fixHeights();
            setGridWidth();
        });

        $(window).on("resize", function () {
            fixHeights();
            setGridWidth();
        });

        // reload devices with new search term and filters if ENTER pressed on any in search/filter pane
        $('.search_container__search_details_container').on(
            'keypress',
            'input[type=text]',
            function (e) {

                var ENTER_KEY_CODE = 13;
                if (e.keyCode === ENTER_KEY_CODE) {
                    IoTApp.DeviceIndex.reloadGrid();
                    return false;
                }
            });

        $('.search_container a').click(function () {
            addFilter();
        });
    }

    var initDetails = function () {
        var deviceId = IoTApp.Helpers.DeviceIdState.getDeviceIdFromCookie();
        if (deviceId) {
            self.deviceDetails.init(deviceId);
        }
    }

    var getUiStateFromCookie = function () {
        var c = Cookies.get('ui-state');

        // if c is not populated, just set defaults
        if (!c) {
            c = {
                currentSortArray: [[1, "asc"]],
                start: 0,
                searchQuery: '',
                filters: [],
                searchPaneOpen: false
            };
        }

        return c;
    }

    var saveUiStateIntoCookie = function (data) {
        // get current starting point for data
        var uiState = Cookies.get('ui-state') || {};

        if (data) {
            uiState.start = data.start;
            uiState.searchQuery = data.search.value;
            uiState.filters = data.filters;
        } else {
            if (uiState.start === undefined) {
                uiState.start = 0;
            }
        }

        var grid = self.dataTableContainer.dataTable();
        uiState.currentSortArray = grid.fnSettings().aaSorting;

        uiState.searchPaneOpen = self.searchPane.is(":visible");

        Cookies.set('ui-state', uiState, IoTApp.Helpers.DeviceIdState.cookieOptions);
    }

    var populateSearchPaneFromCookieData = function (uiState) {

        if (!uiState) {
            return;
        }

        // reset search query
        $('#searchQuery').val(uiState.searchQuery);

        // clear existing filters
        filterCount = 0;
        $('#filter_holder').html('');

        // rebuild filters
        var numberOfPreviousFilterClauses = uiState.filters.length;
        for (var i = 0; i < numberOfPreviousFilterClauses; ++i) {
            addFilter();

            var filterData = uiState.filters[i];

            $('#filterField' + i).val(filterData.columnName);

            switch (filterData.columnName) {
                case "Status":
                    $('#filterOperatorControl' + i).hide();
                    $('#filterValueControl' + i).hide();
                    $('#filterStatusControl' + i).show();

                    $('#filterStatusSelect' + i).val(filterData.filterValue);
                    $('#filterOperator' + i).val("Status");
                    break;

                default:
                    $('#filterOperator' + i).val(filterData.filterType);
                    $('#filterValue' + i).val(filterData.filterValue);
                    break;
            }
        }

        // show search pane based on previous state
        if (uiState.searchPaneOpen) {
            var alreadyOpen = self.searchPane.is(":visible");

            if (!alreadyOpen) {
                // NOTE: calling anything here that tries to calc sizes on the grid can throw
                self.searchPane.toggle();
                self.searchPaneClosed.toggle();
            }
        }
    }

    var _selectRowFromDataTable = function (node) {
        node.addClass('selected');
    }

    var _setDefaultRowAndPage = function() {
        if (self.isDefaultDeviceDetailsAvailable === true) {
            self.isDefaultDeviceDetailsAvailable = false;
            var node = self.dataTable.row(self.defaultSelectedRow).nodes().to$();
            _selectRowFromDataTable(node);
        } else {
            // if selected device is no longer displayed in grid, then close the details pane
            closeAndClearDetails();
        }
    }

    var changeDeviceStatus = function() {
        var tableStatus = self.dataTable;

        var cells_status_false = tableStatus.cells(".table_status:contains('false')").nodes();
        $(cells_status_false).addClass('status_false');
        $(cells_status_false).html(resources.disabled);

        var cells_status_true = tableStatus.cells(".table_status:contains('true')").nodes();
        $(cells_status_true).addClass('status_true');
        $(cells_status_true).html(resources.running);

        var cells_status_pending = tableStatus.cells(".table_status:empty").nodes();
        $(cells_status_pending).addClass('status_pending');
        $(cells_status_pending).html(resources.pending);
    }

    var _initializeDatatable = function() {
        var cookieData = getUiStateFromCookie();

        populateSearchPaneFromCookieData(cookieData);

        var onTableDrawn = function () {
            changeDeviceStatus();
            _setDefaultRowAndPage();

            var pagingDiv = $('#deviceTable_paginate');
            if (pagingDiv) {
                if (self.dataTable.page.info().pages > 1) {
                    $(pagingDiv).show();
                } else {
                    $(pagingDiv).hide();
                }
            }
        };

        var onTableRowClicked = function () {
            var data = this.cells[1].innerHTML;
            self.dataTable.$(".selected").removeClass("selected");
            $(this).addClass("selected");
            self.selectedRow = self.dataTable.row(this).index();
            self.deviceDetails.init(data);

            IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie(data);
        }

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return data ? $('<div/>').text(data).html() : null;
        }

        //$.fn.dataTable.ext.legacy.ajax = true;
        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": cookieData.start,
            "pagingType": "simple",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": true,
            "dom": "<'dataTables_header'ip>lrt?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.deviceList + " (_TOTAL_)",
                "infoFiltered": resources.infoFiltered,
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": [
                {
                    "data": "DeviceProperties.HubEnabledState",
                    "mRender": function (data) {
                        if (data === false) {
                            return htmlEncode("false");
                        } else if (data) {
                            return htmlEncode("true");
                        }
                        return htmlEncode(data);
                    },
                    "name": "hubEnabledState"
                },
                {
                    "data": "DeviceProperties.DeviceID",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "deviceId"
                },
                {
                    "data": "DeviceProperties.Manufacturer",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "manufacturer"
                },
                {
                    "data": "DeviceProperties.ModelNumber",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "modelNumber"
                },
                {
                    "data": "DeviceProperties.SerialNumber",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "serialNumber"
                },
                {
                    "data": "DeviceProperties.FirmwareVersion",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "firmwareVersion"
                },
                {
                    "data": "DeviceProperties.Platform",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "platform"
                },
                {
                    "data": "DeviceProperties.Processor",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "processor"
                },
                {
                    "data": "DeviceProperties.InstalledRAM",
                    "mRender": function (data) {
                        return htmlEncode(data);
                    },
                    "name": "installedRAM"
                }
            ],
            "columnDefs": [
                { className: "table_status", "targets": [0] },
                { "searchable": true, "targets": [1] }
            ],
            "order": cookieData.currentSortArray
        });

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function(e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveDeviceFromService);
        });

        self.dataTableContainer.find("tbody").delegate("tr", "click", onTableRowClicked);
        initDetails();

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        $('.loader_container').css('display', 'block');
        $('.loader_container').css('background-color', '#ffffff');
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            $('.loader_container').css('display', processing ? 'block' : 'none');
            _setGridContainerScrollPositionIfRowIsSelected();
        });
        
        var _setGridContainerScrollPositionIfRowIsSelected = function() {
            if ($("tbody .selected").length > 0) {
                $('.grid_container')[0].scrollTop = $("tbody .selected").offset().top - $('.grid_container').offset().top - 50;
            }
        }
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {
        
        data.search.value = $('#searchQuery').val();
        data.filters = [];
        for (var i = 0; i < filterCount; ++i) {

            data.filters[i] = {
                "columnName": $('#filterField' + i).val(),
            };
            
            switch (data.filters[i].columnName) {
                case "Status":
                    data.filters[i].filterType = "Status";
                    data.filters[i].filterValue = $('#filterStatusSelect' + i).val();
                    break;

                default: // (all text-based columns)
                    data.filters[i].filterType = $('#filterOperator' + i).val();
                    data.filters[i].filterValue = $('#filterValue' + i).val();
                    break;
            }
        };

        saveUiStateIntoCookie(data);

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {
            // only do the following if we have a selected device
            var deviceId = IoTApp.Helpers.DeviceIdState.getDeviceIdFromCookie();
            if (deviceId) {
                // iterate through the data before passing it on to grid, and try to
                // find and save the selected deviceID value

                // reset this value each time
                self.isDefaultDeviceDetailsAvailable = false;

                for (var i = 0, len = json.data.length; i < len; ++i) {
                    var data = json.data[i];
                    if (data &&
                        data.DeviceProperties &&
                        data.DeviceProperties.DeviceID === deviceId) {
                        self.defaultSelectedRow = i;
                        self.isDefaultDeviceDetailsAvailable = true;
                        break;
                    }
                }
            }

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };

        self.getDeviceList = $.ajax({
            "dataType": 'json',
                'type': 'POST',
                'url': '/api/v1/devices/list',
                'cache': false,
                'data': data,
                'success': successCallback
       });

       self.getDeviceList.fail(function () {
           $('.loader_container').hide();
           IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveDevices);
        });
    }

    /* Set the heights of scrollable elements for correct overflow behavior */
    function fixHeights() {
        // set height of device details pane
        var fixedHeightVal = $(window).height() - $(".header_page").height();
        $(".height_fixed").height(fixedHeightVal);

        // set height of open search pane
        var fixedHeightSearchVal = $(window).height() -
            $(".search_container__search_details_button_container").height() -
            $(self.buttonSearchPane).height() -
            80;

        $(".search_height--fixed").height(fixedHeightSearchVal);

        // set height of collapsed search pane
        var fixedHeightSearchClosedVal = $(window).height() - 51;
        $(".search_height--closed_fixed").height(fixedHeightSearchClosedVal);

        // set height of scrolling filter container inside search pane
        var fixedHeightFilterVal = $(window).height() -
            $(".search_container__search_details_button_container").height() -
            $(self.buttonSearchPane).height() -
            270;

        $("#filter_holder").height(fixedHeightFilterVal);
    }

    /* Hide/show the Device Details pane */
    var toggleDetails = function () {
        self.deviceGrid.toggle();
        self.deviceGridClosed.toggle();
        setGridWidth();
    }

    // close the device details pane (called when device is no longer shown)
    var closeAndClearDetails = function () {
        // only toggle if we are already open!
        if (self.deviceGrid.is(":visible")) {
            toggleDetails();
        }
        // clear the cookie so we don't unexpectedly pop the details pane open later
        IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie('');

        // clear the details pane (so it's clean!)
        var noDeviceSelected = resources.noDeviceSelected;
        $('#details_grid_container').html('<div class="details_grid__no_selection">' + noDeviceSelected + '</div>');
    }

    var toggleSearchPane = function () {
        self.searchPane.toggle();
        self.searchPaneClosed.toggle();
        setGridWidth();
        fixHeights();
        saveUiStateIntoCookie(null);
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

        var searchPaneVisible = self.searchPane.is(':visible');
        var deviceGridVisible = $(".details_grid").is(':visible');

        var searchPaneWidth = searchPaneVisible ? self.searchPane.width() : self.searchPaneClosed.width();

        var deviceGridWidth = deviceGridVisible ? self.deviceGrid.width() : self.deviceGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - searchPaneWidth - deviceGridWidth - 98;
        gridContainer.width(gridWidth);
    }

    var reloadGrid = function () {
        self.dataTable.ajax.reload();
    }

    var resetSearch = function () {
        // clear search textbox
        var searchQueryTextbox = $('#searchQuery');
        searchQueryTextbox.val('');

        // delete all filters
        filterCount = 0;
        $('#filter_holder').html('');

        IoTApp.DeviceIndex.reloadGrid();
    }

    var filterCount = 0;
    var addFilter = function () {
        // add the new filter controls
        $('#filter_holder').append(getFilterHtml(filterCount));

        // hide the status as soon as it is shown
        $('#filterStatusControl' + filterCount).hide();

        // wire up for dynamic control changes
        $('#filterField' + filterCount).change(function (i) {
            // create a closure for each filter clause for filterCount (now i)
            return function () {
                var newColumn = $('#filterField' + i).val();

                if (newColumn === 'Status') {
                    $('#filterOperatorControl' + i).hide();
                    $('#filterValueControl' + i).hide();
                    $('#filterStatusControl' + i).show();
                } else {
                    $('#filterOperatorControl' + i).show();
                    $('#filterValueControl' + i).show();
                    $('#filterStatusControl' + i).hide();
                }
            }
        }(filterCount));
        
        filterCount++;
    }

    var getFilterHtml = function (filterNum) {
        var templateHtml = $('#filter_template').html();
        // fix up the id and name values by replacing the template text with current #
        var filterHtml = templateHtml.replace(/REPLACE_ME/g, filterNum);
        return filterHtml;
    }

    return {
        init: init,
        toggleDetails: toggleDetails,
        reloadGrid: reloadGrid,
        resetSearch: resetSearch
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceIndex.init(IoTApp.DeviceDetails);
});

