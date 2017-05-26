IoTApp.createModule('IoTApp.DeviceIndex', function () {
    "use strict";

    var self = this;
    var selectedDeviceIds = [];

    var init = function (deviceDetails) {
        self.deviceDetails = deviceDetails;
        self.dataTableContainer = $('#deviceTable');
        self.deviceGrid = $(".details_grid");
        self.deviceGridClosed = $(".details_grid_closed");
        self.deviceGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");

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
                currentSortArray: [[3, "asc"]],
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
            uiState.filterId = data.name == resources.defaultFilterName ? '' : data.id;
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

            columns.push({
                defaultContent: '<input type="checkbox" class="datatable_checkbox_row" />',
                searchable: false,
                orderable: false,
                width: '1%',
                className: 'dt-body-center'
            });
            $('<th />')
                .html('<input type="checkbox" class="datatable_checkbox_all" />')
                .attr('title', '')
                .appendTo(header);

            columns.push({
                data: 'twin.tags.' + resources.iconTagName,
                mRender: function (data, type, row, meta) {
                    var defaultImage = row.isSimulatedDevice ? "/Content/img/IoT.svg" : "/Content/img/device_default.svg";
                    var image = data ? resources.iconBaseUrl + data : defaultImage;
                    return '<img class="device_list_cell_image" src="' + image + '" />';
                },
                searchable: false,
                orderable: false,
                className: 'image_column dt-body-center'
            });
            $('<th />')
                .text(resources.image)
                .attr('title', 'tags.' + resources.iconTagName)
                .appendTo(header);

            data.forEach(function (column, index) {
                var columnOption = {
                    data:"twin." + (column.name.indexOf("reported.") === 0 || column.name.indexOf("desired.") === 0 ? "properties." : "") + column.name,
                    mRender: function (data, type, row, meta) {
                        return htmlEncode(data);
                    },
                    name: column.alias || column.name,
                    rawName: column.name,
                };

                if (column.name === "tags.HubEnabledState") {
                    columnOption.mRender = function (data) {
                        if (data === "Disabled") {
                            return htmlEncode("false" );
                        } else if (data === "Running") {
                            return htmlEncode("true");
                        }
                        return htmlEncode(data);
                    };

                    columnDefs.push({ className: "table_status", "targets": [columns.length] });
                }
                else {
                    columnDefs.push({ className: "hide_characters", "targets": [columns.length] });
                }

                if (column.name === "deviceId") {
                    columnDefs.push({ "searchable": true, "targets": [columns.length] });
                }

                columns.push(columnOption);

                $('<th />')
                    .text(columnOption.name)
                    .attr('title', column.name)
                    .addClass('remove_text_transform')
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
            setTimeout(_setDefaultRowAndPage, 0);
            updateDataTableSelectAllCheckbox();

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
            stopMultiSelectionIfNeeded();
            _selectRowFromDataTable(self.dataTable.row($(this).parent()));
        }

        var options = {
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": cookieData.start,
            "pagingType": "simple_numbers",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": true,
            "dom": "<'dataTables_header'<'device_list_toolbar'><'device_list_button_area'>>lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": columns,
            "columnDefs": columnDefs,
            "order": cookieData.currentSortArray,
            "rowCallback": setupCheckbox
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
                $('#loadingElement').show();
                IoTApp.DeviceFilter.saveFilterIfNeeded(function () {
                    self.loader = self.deviceDetails.scheduleJob(IoTApp.DeviceFilter.getFilterId(), IoTApp.DeviceFilter.getFilterName());
                });
            }
        }).appendTo($buttonArea);

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function(e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveDeviceFromService);
        });

        self.dataTableContainer.find("tbody").delegate("td:not(:first-child)", "click", onTableRowClicked);
        
        initialMultiSelection();

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
        // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
        return data ? $('<div/>').text(data).html() : null;
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
                values[value] = value;
            }
        });

        return $.map(values, function (value, key) {
            return value;
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

    /* Multi-Selection */
    var initialMultiSelection = function () {
        self.dataTableContainer.delegate('thead input[type="checkbox"]', 'click', selectAllCheckboxClickHandler);
        self.dataTableContainer.delegate('tbody input[type="checkbox"]', 'click', checkboxClickHandler);
        self.dataTableContainer.delegate('tbody td:first-child, thead th:first-child', 'click', function (e) {
            $(this).parent().find('input[type="checkbox"]').trigger('click');
            e.stopPropagation();
        });

        $('#lnkSaveAsFilter').click(function () {
            IoTApp.DeviceFilter.openSaveAsDialogForSelectedDevices(selectedDeviceIds);
        });

        $('.multi_selection_job').click(function () {
            $('.loader_container').show();
            var jobType = $(this).data('jobType');
            IoTApp.DeviceFilter.saveFilterForSelectedDevices(null, selectedDeviceIds, function (filterId) {
                window.location = '/Job/' + jobType + '?filterId=' + filterId;
            });
        });
    }

    var updateDataTableSelectAllCheckbox = function () {
        var $table = self.dataTable.table().node();
        var $allCheckbox = $('tbody input[type="checkbox"]', $table);
        var $checkedCheckbox = $('tbody input[type="checkbox"]:checked', $table);
        var selectAllCheckbox = $('thead input[type="checkbox"]', $table).get(0);

        if($checkedCheckbox.length === 0) {
            selectAllCheckbox.checked = false;
            selectAllCheckbox.indeterminate = false;
        } 
        else if ($checkedCheckbox.length === $allCheckbox.length) {
            selectAllCheckbox.checked = true;
            selectAllCheckbox.indeterminate = false;
        } 
        else {
            selectAllCheckbox.checked = true;
            selectAllCheckbox.indeterminate = true;
        }
    }

    var setupCheckbox = function(row, data, dataIndex) {
        var deviceId = data.twin.deviceId;
        if(selectedDeviceIds.indexOf(deviceId) !== -1) {
            $(row).find('input[type="checkbox"]').prop('checked', true);
            $(row).addClass('selected');
        }

        if (isMultiSelectionMode()) {
            $(row).find('input[type="checkbox"]').addClass('datatable_checkbox_show');
        }
    }

    var selectAllCheckboxClickHandler = function (e) {
        var $table = self.dataTable.table().node();
        var $allCheckbox = $('tbody input[type="checkbox"]', $table);
        if (this.checked) {
            $('tbody input[type="checkbox"]:not(:checked)').trigger('click');
        } else {
            $('tbody input[type="checkbox"]:checked').trigger('click');
        }

        e.stopPropagation();
    }

    var checkboxClickHandler = function (e) {
        
        startMultiSelection();
        var $row = $(this).closest('tr');
        var deviceId = self.dataTable.row($row).data().twin.deviceId;

        var index = selectedDeviceIds.indexOf(deviceId);

        if (this.checked && index === -1) {
            selectedDeviceIds.push(deviceId);
        } 
        else if (!this.checked && index !== -1) {
            selectedDeviceIds.splice(index, 1);
        }

        var message = selectedDeviceIds.length > 1 ? resources.devicesSelected : resources.deviceSelected;
        $('#lblSelectedCount').text(message.replace('{0}', selectedDeviceIds.length));

        if( this.checked) {
            $row.addClass('selected');
        } 
        else {
            $row.removeClass('selected');
        }

        updateDataTableSelectAllCheckbox();
        stopMultiSelectionIfNoSelectedDevice();

        e.stopPropagation();
    }

    var startMultiSelection = function () {
        if (!isMultiSelectionMode()) { 
            unselectAllRows();
            closeAndClearDetails();
            var $table = self.dataTable.table().node();
            $('input[type="checkbox"]', $table).addClass('datatable_checkbox_show');
            showMultiSelectionPane();
            IoTApp.DeviceFilter.setMultiSelectionMode(true);
        }
    }

    var stopMultiSelectionIfNeeded = function () {
        if (isMultiSelectionMode()) {
            stopMultiSelection();
        }
    }
    
    var stopMultiSelectionIfNoSelectedDevice = function () {
        if (!isMultiSelectionMode()) {
            stopMultiSelection();
        }
    }
    
    var stopMultiSelection = function () {
        selectedDeviceIds = [];
        var $table = self.dataTable.table().node();
        $('input[type="checkbox"]', $table).prop('checked', false).removeClass('datatable_checkbox_show');
        unselectAllRows();
        updateDataTableSelectAllCheckbox();
        hideMultiSelectionPane();
        IoTApp.DeviceFilter.setMultiSelectionMode(false);
    }

    var isMultiSelectionMode = function () {
        return selectedDeviceIds.length > 0;
    }

    var showMultiSelectionPane = function () {
        $('.device_list_multi_selection_grid').css('right', 0);
    }

    var hideMultiSelectionPane = function () {
        $('.device_list_multi_selection_grid').css('right', $('.device_list_multi_selection_grid').width() * -1);
    }

    var getSelectedDeviceIds = function () {
        return selectedDeviceIds;
    }

    return {
        init: init,
        toggleDetails: toggleDetails,
        reloadGrid: reloadGrid,
        reinitializeDeviceList: reinitializeDeviceList,
        getAvailableValuesFromPath: getAvailableValuesFromPath,
        stopMultiSelectionIfNeeded: stopMultiSelectionIfNeeded
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceIndex.init(IoTApp.DeviceDetails);
});

