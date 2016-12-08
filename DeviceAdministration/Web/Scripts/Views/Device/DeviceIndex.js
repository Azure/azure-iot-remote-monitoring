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
        self.imageNameList = ["tags.network.AT&T", "tags.network.T-Mobile", "tags.network.Verizon"];

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

        $('#findFilterBox').on(
            'keypress',
            function (e) {
                var ENTER_KEY_CODE = 13;
                if (e.keyCode === ENTER_KEY_CODE) {
                    var selectItem = IoTApp.Controls.NameSelector.getSelectedItem($(this));
                    findFilter(selectItem && selectItem.id || '');
                    return false;
                }
            });

        initFilterNameList();
        initNameCacheList();

        $('#addNewClause').click(function () {
            addClause();
        });

        $('#clearClauses').click(function () {
            clearClauses(false);
        });

        $('#searchTypeSelect').change(function () {
            $('.filter_display_group').toggle();
            $('.advanced_clause_display_group').toggle();
            updateSqlBox(getFilterDataModel());
        });

        showRecentFilters(null);

        $('#recent_filter').click(function () {
            $('#recentFilterNameList').toggle();
            $('.search_container__filter_recent_filter_label').toggle();
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
                filterId: '',
                filterName: '',
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
            uiState.filterId = data.filterId;
            uiState.filterName = data.filterName;
            uiState.clauses = data.clauses;
            uiState.advancedClause = data.advancedClause;
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

    var showHideSearchPane = function (show) {

        var currentState = self.searchPane.is(":visible");

        if (show !== currentState) {
            // NOTE: calling anything here that tries to calc sizes on the grid can throw
            self.searchPane.toggle();
            self.searchPaneClosed.toggle();
            setGridWidth();
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
                            validdata.value = validdata.value[twinSearchToken[i]];
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

    var populateSearchPane = function (callback) {
        
        if (resources.filterId === resources.allDevices)
        {
            callback();
        }
        else if (resources.filterId) {
            showHideSearchPane(true);
            findFilter(resources.filterId, callback);
        }
        else {
            var cookieData = getUiStateFromCookie();
            showHideSearchPane(cookieData.searchPaneOpen);
            updateFilterPanel(cookieData);
            callback();
        }
    } 

    var _initializeDatatableImpl = function (columns, columnDefs) {
        var cookieData = getUiStateFromCookie();

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
            return $('<img/>').attr("src", 'https://localrmea00a.blob.core.windows.net/uploadedimgs/' + data.columninfo + '.' + data.value + '.jpg').width(30).height(30).after($('<div/>').text(data.value).html()).get(0).outerHTML;
        }
        // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
        return data.value ? $('<div/>').text(data.value).html() : null;
    }

    var onDataTableAjaxCalled = function (data, fnCallback) {
        data = IoTApp.DeviceFilter.fillFilterModel(data);
        data.search.value = $('#searchQuery').val();

        saveUiStateIntoCookie(data);

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {
            IoTApp.DeviceFilter.updateFilterResult(json.recordsFiltered, json.recordsTotal);

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

        var gridWidth = windowWidth - deviceGridWidth - 98;
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
        clauseCount = 0;
        $('#filter_holder').html('');

        $('#filterNameBox').val('');
        $('#filterIdBox').val('');
        $('#sqlBox').val('');
        $('.filter_display_group').show();
        $('.advanced_clause_display_group').hide();
        $('#searchTypeSelect').val("FILTERS");
        $('.search_container__filter_links').removeClass('selected_filter');

        IoTApp.DeviceIndex.reloadGrid();
    }

    var clauseCount = 0;
    var addClause = function () {
        // add the new filter controls
        $('#filter_holder').append(getFilterHtml(clauseCount));

        // hide the status as soon as it is shown
        $('#filterStatusControl' + clauseCount).hide();

        applyNameCacheList($('#filterField' + clauseCount));

        // wire up for dynamic control changes
        $('#filterField' + clauseCount).change(function (i) {
            // create a closure for each filter clause for clauseCount (now i)
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
        }(clauseCount));
        
        clauseCount++;
    }

    var getFilterHtml = function (filterNum) {
        var templateHtml = $('#filter_template').html();
        // fix up the id and name values by replacing the template text with current #
        var filterHtml = templateHtml.replace(/REPLACE_ME/g, filterNum);
        return filterHtml;
    }

    var getFilterDataModel = function () {
        var clauses = [];
        for (var i = 0; i < clauseCount; ++i) {
            var columnName = $('#filterField' + i).val().trim();
            if (!columnName) continue;
            clauses.push({
                "ColumnName": columnName,
                "ClauseType": $('#filterOperator' + i).val(),
                "ClauseValue": $('#filterValue' + i).val(),
            });
        }
        return clauses;
    }

    var updateSqlBox = function (clauses) {
        $.ajax({
            url: "/api/v1/generateAdvanceClause",
            type: 'POST',
            dataType: 'json',
            data: { Name: 'any', Clauses: clauses },
            success: function (result) {
                $('#sqlBox').val(result.data);
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGenerateSql);
            }
        });

    }
    
    var buildNewFilter = function () {
        var prefix = "MyNewFilter";
        $.ajax({
            url: "/api/v1/defaultFilterName/" + prefix,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                $('#filterNameBox').val(result.data);
                $('#filterIdBox').val('');
                $('.search_container__filter_links').removeClass('selected_filter');
                clearClauses(false);
                updateSqlBox(getFilterDataModel());
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetDefaultFilters);
            }
        });
    }

    var saveFilter = function () {
        var filterName = $('#filterNameBox').val();
        var filterId = $('#filterIdBox').val();
        var sql = $('#sqlBox').val();
        var isAdvanced = $('#searchTypeSelect').val() === 'ADVANCED';
        var filters = getFilterDataModel();
        if (!filterName || sql === '' && filters.length == 0) {
            IoTApp.Helpers.Dialog.displayError(resources.filterIsEmpty);
            return;
        }
        else if (/[#%.*+:?<>&/\\]/g.test(filterName)) {
            IoTApp.Helpers.Dialog.displayError(resources.incorrectFilterName);
            return;
        }
        return $.ajax({
            url:  "/api/v1/filters",
            type: 'POST',
            data: {
                Id: filterId,
                Name: filterName,
                Clauses: filters,
                AdvancedClause: sql,
                IsTemporary: false,
                IsAdvanced: isAdvanced,
            },
            dataType: 'json',
            success: function (result) {
                showRecentFilters(filterName);
                initFilterNameList();
                return result.data;
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToSaveFilter);
            }
        });
    }

    var deleteFilter = function () {
        var url = "/api/v1/filters/" + $('#filterIdBox').val();
        return $.ajax({
            url: url,
            type: 'DELETE',
            dataType: 'json',
            success: function (result) {
                showRecentFilters(null);
                $('#filterNameBox').val('');
                $('#filterIdBox').val('');
                clearClauses(false);
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteFilter);
            }
        });
    }

    var showRecentFilters = function (filterName) {
        var url = "/api/v1/filters";
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                var recentFilterArea = $('#recentFilterNameList');
                recentFilterArea.empty();
                var filter = result.data;
                for (var i = 0; i < filter.length; i++) {
                    $('<a/>', {
                        id: filter[i].id,
                        "class": 'search_container__filter_links',
                        text: filter[i].name,
                        click: function () {
                            findFilter(this.id);
                        }
                    }).appendTo(recentFilterArea);
                };
                if (filterName) {
                    $('.search_container__filter_links').each(function (index) {
                        if(this.textContent === filterName) {
                            $(this).addClass('selected_filter');
                        }
                    });
                }
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetRecentFilter);
            }
        });
    }

    var findFilter = function (filterId, callback) {
        var url = "/api/v1/filters/" + filterId;
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                updateFilterPanel(result.data);
                if (callback) {
                    callback();
                }   
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetFilter + " : " + filterId);
                if (callback) {
                    callback();
                }
            }
        });
    }

    var updateFilterPanel = function (filter) {
        filter.name = filter.name || filter.filterName;
        filter.id = filter.id || filter.filterId;
        updateFiltersPanel(filter.clauses);
        $('#filterNameBox').val(filter.name);
        $('#filterIdBox').val(filter.id);
        $('.search_container__filter_links').each(function (index) {
            if (this.textContent === filter.name) {
                $(this).addClass('selected_filter');
            } else {
                $(this).removeClass('selected_filter');
            }
        });
        if (filter.isAdvanced) {
            $('.filter_display_group').hide();
            $('.advanced_clause_display_group').show();
            $('#searchTypeSelect').val('ADVANCED');
        } else {
            $('.filter_display_group').show();
            $('.advanced_clause_display_group').hide();
            $('#searchTypeSelect').val('FILTERS');
        }
        $('#sqlBox').val(filter.advancedClause);
    }

    var updateFiltersPanel = function (filters) {
        if (!filters) {
            clearClauses(false);
            return;
        }
        clearClauses(true);
        clauseCount = filters.length;
        filters.forEach(function (filter, i) {
            $('#filter_holder').append(getFilterHtml(i));
            applyNameCacheList($('#filterField' + i));
            var newColumn = $('#filterField' + i).val(filter.columnName);
            $('#filterOperator' + i).val(filter.clauseType);
            $('#filterValue' + i).val(filter.clauseValue);
            $('#filterStatusControl' + i).hide();
            // wire up for dynamic control changes
            $('#filterField' + i).change(function (i) {
                // create a closure for each filter clause for clauseCount (now i)
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
        clauseCount = 0;
        if (empty) return;
        $('#filter_holder').append(getFilterHtml(0));
        applyNameCacheList($('#filterField0'));
        $('#filterStatusControl0').hide();
        clauseCount = 1;
    }

    var initFilterNameList = function () {
        return $.ajax({
            url: '/api/v1/filterList?skip=0&take=1000',
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                IoTApp.Controls.NameSelector.create($('#findFilterBox'), null, result.data);
            }
        });
    }

    var initNameCacheList = function () {
        var url = "/api/v1/namecache/list/" + (IoTApp.Controls.NameSelector.NameListType.deviceInfo | IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property);
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                self.nameCacheList = result.data;
            }
        });
    }

    var applyNameCacheList = function ($element){
        if (self.nameCacheList){
            IoTApp.Controls.NameSelector.create($element, null, self.nameCacheList);
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
        showRecentFilters: showRecentFilters,
        saveFilter: saveFilter,
        deleteFilter: deleteFilter,
        buildNewFilter: buildNewFilter,
    }
}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.DeviceIndex.init(IoTApp.DeviceDetails);
});

