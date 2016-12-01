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
        self.filterNameList = [];

        Cookies.json = true;

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

        $('#findQueryBox').on(
            'keypress',
            function (e) {
                var ENTER_KEY_CODE = 13;
                if (e.keyCode === ENTER_KEY_CODE) {
                    findQuery($(this).val());
                    return false;
                }
            });

        initQueryNameList();
        _cacheFilterNameList();

        $('#addNewClause').click(function () {
            addFilter();
        });

        $('#clearClauses').click(function () {
            clearClauses(false);
        });

        $('#searchTypeSelect').change(function () {
            $('.filter_display_group').toggle();
            $('.query_display_group').toggle();
            updateSqlBox(getFilterDataModel());
        });

        $('.search_container__query_search_type_filter').click(function () {
            $('.filter_display_group').toggle();
            $('.query_display_group').toggle();
            updateSqlBox(getFilterDataModel());
        });

        $('.search_container__query_search_type_query').click(function () {
            $('.filter_display_group').toggle();
            $('.query_display_group').toggle();
        });

        showRecentQueries(null);

        $('#recent_query').click(function () {
            $('#recentQueryNameList').toggle();
            $('.search_container__query_recent_query_label').toggle();
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
                queryName: '',
                filters: [],
                sql: '',
                isAdvanced: false,
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
            uiState.queryName = data.queryName;
            uiState.filters = data.filters;
            uiState.sql = data.sql;
            uiState.isAdvanced = data.isAdvanced;
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
        $('#queryNameBox').val(uiState.queryName);

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
                setGridWidth();
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

    var _initializeDatatable = function () {
        var onDataLoaded = function (data) {
            var header = $("#deviceTable thead tr").empty();
            var columns = [];
            var columnDefs = [];
            data.forEach(function (column, index) {
                var columnOption = {
                    data: "twin." + (column.name.indexOf("reported.") === 0 || column.name.indexOf("desired.") === 0 ? "properties." : "") + column.name,
                    mRender: function (data) {
                        return htmlEncode(data);
                    },
                    name: column.alias || column.name
                };

                if (column.name === "tags.HubEnabledState") {
                    columnOption.mRender = function (data) {
                        if (data === "Disabled") {
                            return htmlEncode("false");
                        } else if (data === "Running") {
                            return htmlEncode("true");
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

            _initializeDatatableImpl(columns, columnDefs);
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

    var _initializeDatatableImpl = function(columns, columnDefs) {
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
            showDetails();
            self.loader = self.deviceDetails.init(data);

            IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie(data);
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
            "dom": "<'dataTables_header'i<'#button_area.pull-right'>p>lrt?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.deviceList + " (_TOTAL_)",
                "infoFiltered": resources.infoFiltered,
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

        var $buttonArea = $('#button_area');
        
        $('<button/>', {
            id: 'editColumnsButton',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray',
            text: resources.editColumns,
            click: function () {
                unselectAllRows();
                showDetails();
                self.loader = IoTApp.DeviceListColumns.init();
            }
        }).appendTo($buttonArea);

        $('<button/>', {
            id: 'scheduleJobButton',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray',
            text: resources.scheduleJob,
            click: function () {
                unselectAllRows();
                showDetails();
                self.loader = self.deviceDetails.scheduleJob($('#queryNameBox').val());
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
        // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
        return data ? $('<div/>').text(data).html() : null;
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {
        
        data.search.value = $('#searchQuery').val();
        data.queryName = $('#queryNameBox').val();
        data.sql = $('#sqlBox').val();
        data.isAdvanced = $('#searchTypeSelect').val() === 'QUERY';
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
                        data.deviceProperties &&
                        data.deviceProperties.deviceId === deviceId) {
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
        var fixedHeightVal = $(window).height() - $(".navbar").height();
        $(".height_fixed").height(fixedHeightVal);

        // set height of open search pane
        $(".search_height--fixed").height(fixedHeightVal);

        // set height of collapsed search pane
        $(".search_height--closed_fixed").height(fixedHeightVal);

        // set height of scrolling filter container inside search pane
        var fixedHeightFilterVal = $(window).height() -
            $(".search_container__search_details_button_container").height() -
            $(self.buttonSearchPane).height() -
            470;

        $("#filter_holder").height(fixedHeightFilterVal);
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

        $('#queryNameBox').val('');
        $('#sqlBox').val('');
        $('.filter_display_group').show();
        $('.query_display_group').hide();
        $('#searchTypeSelect').val("FILTERS");
        $('.search_container__query_links').removeClass('selected_query');

        IoTApp.DeviceIndex.reloadGrid();
    }

    var filterCount = 0;
    var addFilter = function () {
        // add the new filter controls
        $('#filter_holder').append(getFilterHtml(filterCount));

        // hide the status as soon as it is shown
        $('#filterStatusControl' + filterCount).hide();

        applyFilterNameList($('#filterField' + filterCount));

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

    var getFilterDataModel = function () {
        var filters = [];
        for (var i = 0; i < filterCount; ++i) {
            var columnName = $('#filterField' + i).val().trim();
            if (!columnName) continue;
            filters.push({
                "ColumnName": columnName,
                "FilterType": $('#filterOperator' + i).val(),
                "FilterValue": $('#filterValue' + i).val(),
            });
        }
        return filters;
    }

    var updateSqlBox = function (filters) {
        $.ajax({
            url: "/api/v1/generateSql",
            type: 'POST',
            dataType: 'json',
            data: { Name: 'any', Filters: filters },
            success: function (result) {
                $('#sqlBox').val(result.data);
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGenerateSql);
            }
        });

    }
    
    var buildNewQuery = function () {
        var prefix = "MyNewQuery";
        $.ajax({
            url: "/api/v1/availableQueryName/" + prefix,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                $('#queryNameBox').val(result.data);
                $('.search_container__query_links').removeClass('selected_query');
                clearClauses(false);
                updateSqlBox(getFilterDataModel());
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetAvailableQueryName);
            }
        });
    }

    var saveQuery = function () {
        var queryName = $('#queryNameBox').val();
        var sql = $('#sqlBox').val();
        var url = "/api/v1/queries";
        var isAdvanced = $('#searchTypeSelect').val() === 'QUERY';
        var filters = getFilterDataModel();
        if (!queryName || sql === '' && filters.length == 0) {
            IoTApp.Helpers.Dialog.displayError(resources.queryIsEmpty);
            return;
        }
        else if (/[#%.*+:?<>&/\\]/g.test(queryName)) {
            IoTApp.Helpers.Dialog.displayError(resources.incorrectQueryName);
            return;
        }
        return $.ajax({
            url: url,
            type: 'POST',
            data: {
                Name: queryName,
                Filters: filters,
                Sql: sql,
                IsTemporary: false,
                IsAdvanced: isAdvanced,
            },
            dataType: 'json',
            success: function (result) {
                showRecentQueries(queryName);
                initQueryNameList();
                return result.data;
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToSaveQuery);
            }
        });
    }

    var deleteQuery = function () {
        var url = "/api/v1/queries/" + $('#queryNameBox').val();
        return $.ajax({
            url: url,
            type: 'DELETE',
            dataType: 'json',
            success: function (result) {
                showRecentQueries(null);
                $('#queryNameBox').val('');
                clearClauses(false);
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteQuery);
            }
        });
    }

    var showRecentQueries = function (queryName) {
        var url = "/api/v1/queries";
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                var recentQueryArea = $('#recentQueryNameList');
                recentQueryArea.empty();
                var query = result.data;
                for (var i = 0; i < query.length; i++) {
                    $('<a/>', {
                        id: 'queryName' + i,
                        "class": 'search_container__query_links',
                        text: query[i].name,
                        click: function () {
                            findQuery(this.textContent);
                        }
                    }).appendTo(recentQueryArea);
                };
                if (queryName) {
                    $('.search_container__query_links').each(function (index) {
                        if(this.textContent === queryName) {
                            $(this).addClass('selected_query');
                        }
                    });
                }
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetRecentQuery);
            }
        });
    }

    var findQuery = function (queryName) {
        var url = "/api/v1/queries/" + queryName;
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                updateQueryPanel(result.data);
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetQuery + " : " + queryName);
            }
        });
    }

    var updateQueryPanel = function (query) {
        updateFiltersPanel(query.filters);
        $('#queryNameBox').val(query.name);
        $('.search_container__query_links').each(function (index) {
            if (this.textContent === query.name) {
                $(this).addClass('selected_query');
            } else {
                $(this).removeClass('selected_query');
            }
        });
        if (query.isAdvanced) {
            $('.filter_display_group').hide();
            $('.query_display_group').show();
            $('#searchTypeSelect').val('QUERY');
        } else {
            $('.filter_display_group').show();
            $('.query_display_group').hide();
            $('#searchTypeSelect').val('FILTERS');
        }
        $('#sqlBox').val(query.sql);
    }

    var updateFiltersPanel = function (filters) {
        if (!filters) {
            clearClauses(false);
            return;
        }
        clearClauses(true);
        filterCount = filters.length;
        filters.forEach(function (filter, i) {
            $('#filter_holder').append(getFilterHtml(i));
            applyFilterNameList($('#filterField' + i));
            var newColumn = $('#filterField' + i).val(filter.columnName);
            $('#filterOperator' + i).val(filter.filterType);
            $('#filterValue' + i).val(filter.filterValue);
            $('#filterStatusControl' + i).hide();
            // wire up for dynamic control changes
            $('#filterField' + i).change(function (i) {
                // create a closure for each filter clause for filterCount (now i)
                return function () {
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
            });
        });
    }

    var clearClauses = function (empty) {
        $('#filter_holder').empty();
        filterCount = 0;
        if (empty) return;
        $('#filter_holder').append(getFilterHtml(0));
        applyFilterNameList($('#filterField0'));
        $('#filterStatusControl0').hide();
        filterCount = 1;
    }

    var initQueryNameList = function () {
        return $.ajax({
            url: '/api/v1/queryList',
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                IoTApp.Controls.NameSelector.create($('#findQueryBox'), null, result.data);
            }
        });
    }

    var _cacheFilterNameList = function () {
        var url = "/api/v1/namecache/list/" + (IoTApp.Controls.NameSelector.NameListType.deviceInfo | IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property);
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                self.filterNameList = result.data;
            }
        });
    }

    var applyFilterNameList = function ($element){
        if (self.filterNameList){
            IoTApp.Controls.NameSelector.create($element, null, self.filterNameList);
        }else {
            IoTApp.Controls.NameSelector.create($element, { type: IoTApp.Controls.NameSelector.NameListType.deviceInfo | IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property });
        }
    }

    var unselectAllRows = function () {
        self.dataTable.$(".selected").removeClass("selected");
        IoTApp.Helpers.DeviceIdState.saveDeviceIdToCookie('');
    }

    return {
        init: init,
        toggleDetails: toggleDetails,
        reloadGrid: reloadGrid,
        resetSearch: resetSearch,
        reinitializeDeviceList: reinitializeDeviceList,
        showRecentQueries: showRecentQueries,
        saveQuery: saveQuery,
        deleteQuery: deleteQuery,
        buildNewQuery: buildNewQuery,
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceIndex.init(IoTApp.DeviceDetails);
});

