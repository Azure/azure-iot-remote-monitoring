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
        self.imageNameList = ["tags.network.AT&T", "tags.network.T-Mobile", "tags.network.Verizon"];

        Cookies.json = true;

        _initializeDatatable();

        self.buttonDetailsGrid.on("click", function () {
            toggleDetails();
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

        initSpliter();
    }

    var initSpliter = function () {
        self.deviceGrid.resizable({
            handles: 'w',
            minWidth: self.deviceGrid.outerWidth(),
            maxWidth: '650',
            resize: function () {
                // Workaround: clear left to keep the panel on the right.
                $(this).css("left", "");
                setGridWidth();
            }
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
                filterId: ''
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
            uiState.filterId = data.id;
        } else {
            if (uiState.start === undefined) {
                uiState.start = 0;
            }
        }

        var grid = self.dataTableContainer.dataTable();
        uiState.currentSortArray = grid.fnSettings().aaSorting;

        Cookies.set('ui-state', uiState, IoTApp.Helpers.DeviceIdState.cookieOptions);
    }

    var _selectRowFromDataTable = function (row) {
        var rowData = row.data();
        if (rowData != null) {
            self.dataTable.$(".selected").removeClass("selected");
            row.nodes().to$().addClass("selected");
            var deviceId = rowData.deviceProperties.deviceID;
            self.selectedRow = row.index();
            IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie(deviceId);
            showDetails();
            self.loader = self.deviceDetails.init(deviceId);
        }
    }

    var _setDefaultRowAndPage = function() {
        if (self.isDefaultDeviceDetailsAvailable === true) {
            self.isDefaultDeviceDetailsAvailable = false;
            var node = self.dataTable.row(self.defaultSelectedRow);
            _selectRowFromDataTable(node);
        }
        else if (self.isDefaultDeviceIdAvailable) {
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

    var _initializeDatatable = function () {
        var onDataLoaded = function (data) {
            var header = $("#deviceTable thead tr").empty();
            var columns = [];
            var columnDefs = [];
            data.forEach(function (column, index) {
                var columnOption = {
                    data: function (row, type, set, meta) {
                        var validdata = {
                            value: {},
                            columninfo:""
                        };
                        if (column.name.indexOf("reported.") === 0 || column.name.indexOf("desired.") === 0) {
                            validdata.value = row.twin.properties;
                        }
                        else {
                            validdata.value = row.twin;
                        }
                        var twinSearchToken = column.name.split('.');

                        for (var i = 0; i < twinSearchToken.length; i++)
                        {
                            if (validdata.value) {
                                validdata.value = validdata.value[twinSearchToken[i]];
                            }
                            else {
                                validdata.value = "";
                            }
                        }
                        validdata.columninfo = column.name;

                        return validdata;
                    },//"twin." + (column.name.indexOf("reported.") === 0 || column.name.indexOf("desired.") === 0 ? "properties." : "") + column.name,
                    mRender: function (data, type, row, meta) {
                        return htmlEncode(data);
                    },
                    name: column.alias || column.name
                };

                if (column.name === "tags.HubEnabledState") {
                    columnOption.mRender = function (data) {
                        if (data.value === "Disabled") {
                            return htmlEncode({ value: "false" });
                        } else if (data.value === "Running") {
                            return htmlEncode({ value: "true" });
                        }
                        return htmlEncode(data);
                    };

                    columnDefs.push({ className: "table_status", "targets": [index] });
                }

                if (column.name === "deviceId") {
                    columnDefs.push({ "searchable": true, "targets": [index] });
                }

                columns.push(columnOption);

                $('<th />')
                    .text(columnOption.name)
                    .attr('title', column.name)
                    .appendTo(header);
            });
            
            IoTApp.DeviceFilter.init(getUiStateFromCookie(), function () {
                _initializeDatatableImpl(columns, columnDefs);
            });
        }

        $('.retry_message_container').empty();
        $('.loader_container').show();

        $.ajax({
            url: '/api/v1/deviceListColumns',
            type: 'GET',
            success: function (result) {
                onDataLoaded(result.data);
            },
            error: function () {
                $('.loader_container').hide();
                IoTApp.Helpers.RenderRetryError(resources.failedToRetrieveColumns, $('.retry_message_container'), function () { _initializeDatatable(); });
            }
        });
    }

    var _initializeDatatableImpl = function (columns, columnDefs) {
        var cookieData = getUiStateFromCookie();

        var onTableDrawn = function () {
            changeDeviceStatus();
            setImmediate(_setDefaultRowAndPage);

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
            _selectRowFromDataTable(self.dataTable.row(this));
        }

        var options = {
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": cookieData.start,
            "pagingType": "simple",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": true,
            "dom": "<'dataTables_header'<'device_list_toolbar'><'device_list_button_area'>p>lrt?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": columns,
            "columnDefs": columnDefs,
            "order": cookieData.currentSortArray
        };
        //$.fn.dataTable.ext.legacy.ajax = true;
        self.dataTable = self.dataTableContainer.DataTable(options);

        IoTApp.DeviceFilter.initToolbar($('.device_list_toolbar'));

        var $buttonArea = $('.device_list_button_area');
        
        $('<button/>', {
            id: 'editColumnsButton',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray device_list_button_edit_column',
            text: resources.editColumns,
            click: function () {
                unselectAllRows();
                showDetails();
                self.loader = IoTApp.DeviceListColumns.init();
            }
        }).appendTo($buttonArea);

        $('<button/>', {
            id: 'scheduleJobButton',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray device_list_button_schedule_job',
            text: resources.scheduleJob,
            click: function () {
                unselectAllRows();
                showDetails();
                IoTApp.DeviceFilter.saveFilterIfNeeded();
                self.loader = self.deviceDetails.scheduleJob(IoTApp.DeviceFilter.getFilterId(), IoTApp.DeviceFilter.getFilterName());
            }
        }).appendTo($buttonArea);

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function(e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveDeviceFromService);
        });

        self.dataTableContainer.find("tbody").delegate("tr", "click", onTableRowClicked);

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            if (processing) {
                $('.loader_container').show();
            }
            else {
                $('.loader_container').hide();
            }
            _setGridContainerScrollPositionIfRowIsSelected();
        });
        
        var _setGridContainerScrollPositionIfRowIsSelected = function() {
            if ($("tbody .selected").length > 0) {
                $('.grid_container')[0].scrollTop = $("tbody .selected").offset().top - $('.grid_container').offset().top - 50;
            }
        }
    }

    var reinitializeDeviceList = function () {
        $("#deviceTable thead tr").empty();
        self.dataTable.clear();
        self.dataTable.destroy();
        _initializeDatatable();
    }

    var htmlEncode = function (data) {
        if (self.imageNameList.indexOf(data.columninfo + '.' + data.value) >= 0)
        {
            return $('<img/>').attr("src", 'https://localrmea00a.blob.core.windows.net/uploadedimgs/' + data.columninfo + '.' + data.value + '.jpg')
                .addClass("device_list_cell_image").get(0).outerHTML +
                $('<div/>').addClass("device_list_cell_text").text(data.value).get(0).outerHTML;
        }
        // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
        return data.value ? $('<div/>').text(data.value).html() : null;
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {
        data = IoTApp.DeviceFilter.fillFilterModel(data);

        saveUiStateIntoCookie(data);

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {
            IoTApp.DeviceFilter.updateFilterResult(json.recordsFiltered, json.recordsTotal);

            // only do the following if we have a selected device
            var deviceId = IoTApp.Helpers.DeviceIdState.getDeviceIdFromCookie();

            // reset this value each time
            self.isDefaultDeviceDetailsAvailable = false;
            self.isDefaultDeviceIdAvailable = false;

            if (deviceId) {
                // iterate through the data before passing it on to grid, and try to
                // find and save the selected deviceID value

                self.isDefaultDeviceIdAvailable = true;

                for (var i = 0, len = json.data.length; i < len; ++i) {
                    var data = json.data[i];
                    if (data &&
                        data.deviceProperties &&
                        data.deviceProperties.deviceID === deviceId) {
                        self.defaultSelectedRow = i;
                        self.isDefaultDeviceDetailsAvailable = true;
                        break;
                    }
                }
            }

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        };

        clearRefeshTimeout();

        if (self.getDeviceList) {
            self.getDeviceList.abort();
        }

        self.getDeviceList = $.ajax({
            "dataType": 'json',
                'type': 'POST',
                'url': '/api/v1/devices/list',
                'cache': false,
                'data': data,
                'success': successCallback
       }).fail(function (xhr, status, error) {
            $('.loader_container').hide();
            if (status !== 'abort') {
                IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveDevices);
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
    var toggleDetails = function () {
        self.deviceGrid.toggle();
        self.deviceGridClosed.toggle();
        setGridWidth();
    }

    var showDetails = function () {
        if (self.loader) {
            self.loader.abort();
        }

        if (!self.deviceGrid.is(':visible')) {
            IoTApp.DeviceIndex.toggleDetails();
        }
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

        var deviceGridVisible = $(".details_grid").is(':visible');

        var deviceGridWidth = deviceGridVisible ? self.deviceGrid.width() : self.deviceGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - deviceGridWidth - 98;
        gridContainer.width(gridWidth);
    }

    var reloadGrid = function (keepPaging) {
        self.dataTable.ajax.reload(null, !keepPaging);
    }

    var unselectAllRows = function () {
        self.dataTable.$(".selected").removeClass("selected");
        IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie('');
    }

    var getAvailableValuesFromPath = function (path) {
        path = "twin." + (path.indexOf("reported.") === 0 || path.indexOf("desired.") === 0 ? "properties." : "") + path;

        var values = {};
        var data = self.dataTable.data();
        $.each(data, function (idx, row) {
            var value = getValueFromPath(row, path);
            if (value != null && !values.hasOwnProperty(value)) {
                values[value] = true;
            }
        });

        return $.map(values, function (value, key) {
            return key;
        });
    }
    var getValueFromPath = function (data, path) {
        var parts = path.split('.');
        $.each(parts, function (idx, part) {
            data = data[part];
            if (data == null) {
                return false;
            }
        });

        return data;
    }

    return {
        init: init,
        toggleDetails: toggleDetails,
        reloadGrid: reloadGrid,
        reinitializeDeviceList: reinitializeDeviceList,
        getAvailableValuesFromPath: getAvailableValuesFromPath
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceIndex.init(IoTApp.DeviceDetails);
});

