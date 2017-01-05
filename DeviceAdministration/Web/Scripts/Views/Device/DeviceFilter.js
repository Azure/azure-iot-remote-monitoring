IoTApp.createModule('IoTApp.DeviceFilter', function () {
    "use strict";
    
    var self = this;
    var defaultFilterId = "00000000-0000-0000-0000-000000000000";

    self.model = {
        id: ko.observable(defaultFilterId),
        name: ko.observable(resources.allDevices),
        isAdvanced: ko.observable(false),
        advancedClause: ko.observable(null),
        clauses: ko.observableArray([]),
        associatedJobsCount:ko.observable(0),

        isFilterLoaded: ko.observable(false),
        isFilterLoadedFromServer: ko.observable(false),
        isMultiSelectionMode: ko.observable(false),
        isChanged: ko.observable(false),
        filters: ko.observableArray([]),
        filterNameList: ko.observableArray([]),
        viewSelectedClausesOnly: ko.observable(false),
        currentClause: ko.observable(null),
        defaultClauses: [],
        recordsFiltered: ko.observable(0),
        recordsTotal: ko.observable(0),
        saveAsName: ko.observable(null),
        checkedClauses: ko.pureComputed(function () {
            return self.model.clauses().filter(function (clause) {
                return clause.checked();
            });
        }),
        filteredInfo: ko.pureComputed(function () {
            if (!self.model.isDefatulFilter()) {
                return resources.infoFiltered.replace('{0}', self.model.recordsFiltered()).replace('{1}', self.model.recordsTotal());
            }
            else {
                return self.model.recordsTotal();
            }
        }),
        isDefatulFilter: ko.pureComputed(function () {
            return self.model.id() == defaultFilterId;
        }),
        canSave: ko.pureComputed(function () {
            return !self.model.isDefatulFilter() && self.model.isFilterLoaded() && !self.model.isMultiSelectionMode();
        }),
        canDelete: ko.pureComputed(function () {
            return !self.model.isDefatulFilter() && self.model.isFilterLoadedFromServer() && !self.model.isMultiSelectionMode();
        }),
        canEdit: ko.pureComputed(function () {
            return !self.model.isMultiSelectionMode();
        }),
        showPanel: function () {
            self.model.backupState();
            if (self.model.isDefatulFilter())
            {
                self.model.newFilter();
            }
            $('.filter_panel_container').show();
        },
        hidePanel: function () {
            $('.filter_panel_container').hide();
        },
        openSaveAsDialog: function () {
            self.model.saveAsName(self.model.name());
            $('#saveAdFilterButtons').show();
            $('#saveAdFilterButtonsMultiSelection').hide();
            $('.filter_panel_dialog_container').show();
            $('.filter_panel_filtername_saveas_input').focus();
        },
        openSaveAsDialogForSelectedDevices: function (selectedDeviceIds) {
            self.selectedDeviceIds = selectedDeviceIds;
            self.model.saveAsName('');
            $('#defaultNameLoadingElement').show();
            api.getNewFilterName(function (name) {
                $('.filter_panel_filtername_saveas_input').focus();
                self.model.saveAsName(name);
                $('#defaultNameLoadingElement').hide();
            });
            $('#saveAdFilterButtons').hide();
            $('#saveAdFilterButtonsMultiSelection').show();
            $('.filter_panel_dialog_container').show();
        },
        closeSaveAsDialog: function () {
            $('.filter_panel_dialog_container').hide();
        },
        startEditFilterName: function () {
            if (self.model.canSave()) {
                self.originalFilterName = self.model.name();
                $('.device_list_toolbar_filtername').hide();
                $('.device_list_toolbar_filtername_input').show().focus();
            }
        },
        stopEditFilterName: function () {
            if (!self.model.name()) {
                self.model.name(self.originalFilterName);
            }
            if (self.originalFilterName != self.model.name()) {
                self.model.isChanged(true);
            }
            $('.device_list_toolbar_filtername').show();
            $('.device_list_toolbar_filtername_input').hide();
        },
        filterNameBoxKeypress: function (data, e) {
            if (e.keyCode == 13) {
                self.model.stopEditFilterName();
            }
            else if (e.keyCode == 27) {
                self.model.name(self.originalFilterName);
                self.model.stopEditFilterName();
            }

            return true;
        },
        lookupBoxKeypress: function (data, e){
            if (e.keyCode === 13) {
                var name = $(e.target).val();
                var filterId;
                self.model.filterNameList().forEach(function (item) {
                    if (item.name === name) {
                        filterId = item.id;
                        return false;
                    }
                });
                
                self.model.loadFilter(filterId, true);
            }
           
            return true;
        },
        loadFilters: function() {
            api.getFilters(function (data) {
                self.model.filters.removeAll();
                data.forEach(function (item) {
                    if (item.id !== resources.allDevices) {
                        self.model.filters.push({ id: item.id, name: item.name });
                    }
                });
            });

            api.getFilterNameList(function (list) {
                self.model.filterNameList(list);
            });

        },
        resetState: function () {
            self.model.loadFilter(defaultFilterId, true);
        },
        loadFilter: function (filterId, execute, callback) {
            $('#btnOpen').popover('hide');
            if (filterId) {
                api.findFilter(filterId, function (filter) {
                    if (filter) {
                        self.model.setFilter(filter, true);
                        self.model.isChanged(false);

                        if (execute) {
                            IoTApp.DeviceIndex.stopMultiSelectionIfNeeded();
                            IoTApp.DeviceIndex.reloadGrid();
                        }
                    }

                    if ($.isFunction(callback)) {
                        callback();
                    }
                });
            }
            else {
                self.model.resetState();

                if ($.isFunction(callback)) {
                    callback();
                }
            }
        },
        saveFilter: function () {
            var filter = self.model.getFilterModel();
            api.saveFilter(filter, function (result) {
                if (!self.model.id()) {
                    self.model.id(result.id);
                }
                self.model.isFilterLoadedFromServer(true);
                self.model.loadFilters();
                self.model.isChanged(false);
            })
        },
        saveAsFilter: function () {
            self.model.closeSaveAsDialog();
            var newName = self.model.saveAsName();
            self.model.id(null);
            self.model.name(newName);
            var filter = self.model.getFilterModel();

            api.saveFilter(filter, function (result) {
                self.model.id(result.id);
                self.model.isFilterLoadedFromServer(true);
                self.model.loadFilters();
                self.model.isChanged(false);
            })
        },
        saveAsFilterForSelectedDevices: function (open) {
            self.model.closeSaveAsDialog();
            var newName = self.model.saveAsName();
            $('.loader_container').show();
            self.model.saveFilterForSelectedDevices(newName, self.selectedDeviceIds, function (filterId) {
                if (open) {
                    self.model.loadFilter(filterId, true);
                }
                else {
                    $('.loader_container').hide();
                    IoTApp.DeviceIndex.stopMultiSelectionIfNeeded();
                }

                self.model.loadFilters();
            });
        },
        saveFilterForSelectedDevices: function (name, deviceIds, callback) {
            if (name == null) {
                api.getNewFilterName(function (name) {
                    self.model.saveFilterForSelectedDevices(name, deviceIds, callback);
                });
            }
            else {
                self.model.createAndSaveFilterWithClause(name, "deviceId", "IN", deviceIds.join(", "), function (filterId) {
                    if ($.isFunction(callback)) {
                        callback(filterId);
                    }
                });
            }
        },
        createAndSaveFilterWithClause: function (name, columnName, operator, value, callback) {
            var filter = {
                name: name,
                filterName: name,
                isAdvanced: false,
                clauses: [{
                    columnName: columnName,
                    clauseType: operator,
                    clauseValue: value
                }]
            };

            api.saveFilter(filter, function (result) {
                if ($.isFunction(callback)) {
                    callback(result.id);
                }
            })
        },
        deleteFilter: function (data, e, forceDelete) {
            if (self.model.associatedJobsCount()) {
                forceDelete = true;
            }

            var confirmMessage = forceDelete ? resources.deleteFilterWithJobsConfirmation : resources.deleteFilterConfirmation;

            if (IoTApp.Helpers.Dialog.confirm(confirmMessage, function (result) {
                if (result) {
                    api.deleteFilter(self.model.id(), forceDelete, function (isDeleted) {
                        if (isDeleted) {
                            self.model.loadFilters();
                            self.model.resetState();
                        }
                        else if (!forceDelete) {
                            // Try again with force delete flag.
                            self.model.deleteFilter(data, e, true);
                        }
                        else {
                            IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteFilter);
                        }
                    });
                }
            }));
        },
        setFilter: function (filter, isLoadedFromServer) {
            if (filter) {
                self.model.isFilterLoaded(true);
                self.model.isFilterLoadedFromServer(isLoadedFromServer);

                self.model.id(filter.id);
                self.model.name(filter.name);
                self.model.isAdvanced(filter.isAdvanced);
                self.model.advancedClause(filter.advancedClause);
                self.model.associatedJobsCount(filter.associatedJobsCount);
                self.model.viewSelectedClausesOnly(false);
                self.model.clauses.removeAll();
                self.model.defaultClauses.forEach(function (item) {
                    item.checked(false);
                    self.model.clauses.push(item);
                });

                if (filter.clauses) {
                    filter.clauses.forEach(function (item) {
                        var newClause = self.model.newClause(item.columnName, item.clauseType, item.clauseValue);

                        var existing = self.model.clauses().filter(function (clause) {
                            return clause.field() === newClause.field() &&
                                clause.operator() === newClause.operator() &&
                                clause.value() === newClause.value();
                        });

                        if (existing.length > 0) {
                            existing[0].checked(true);
                        }
                        else {
                            self.model.clauses.push(newClause);
                        }
                    });
                }
            }
        },
        newFilter: function (callback) {
            api.getNewFilterName(function (name) {
                self.model.setFilter({
                    id: null,
                    name: name,
                    clauses: [],
                    isAdvanced: false,
                    advancedClause: "",
                    associatedJobsCount: 0
                });
                self.model.isChanged(true);
            });
        },
        executeFilter: function () {
            if (self.model.canAddClause())
            {
                self.model.addClause();
            }

            IoTApp.DeviceIndex.reloadGrid();
            self.model.hidePanel();
        },
        resetFilter: function () {
            if (self.model.isFilterLoadedFromServer()) {
                self.model.loadFilter(self.model.id());
            }
            else {
                self.model.newFilter();
            }
        },
        cancelEdit: function () {
            self.model.restoreState();
            self.model.hidePanel();
        },
        backupState: function () {
            self.state = {
                filter: self.model.getFilterModel(),
                isChanged: self.model.isChanged(),
                isFilterLoaded: self.model.isFilterLoaded(),
                isFilterLoadedFromServer: self.model.isFilterLoadedFromServer()
            }
        },
        restoreState: function () {
            self.model.setFilter(self.state.filter);
            self.model.isChanged(self.state.isChanged);
            self.model.isFilterLoaded(self.state.isFilterLoaded);
            self.model.isFilterLoadedFromServer(self.state.isFilterLoadedFromServer);
        },
        getFilterModel: function () {
            return {
                id: self.model.id(),
                name: self.model.name(),
                filterName: self.model.name(),
                advancedClause: self.model.advancedClause(),
                isAdvanced: self.model.isAdvanced(),
                clauses: self.model.clauses().filter(function (clause) {
                    return clause.checked();
                }).map(function (clause) {
                    return {
                        columnName: clause.field(),
                        clauseType: clause.operator(),
                        clauseValue: clause.value()
                    };
                })
            };
        },
        canAddClause: ko.pureComputed(function () {
            return self.model.currentClause().field() && self.model.currentClause().value();
        }),
        addClause: function () {
            self.model.clauses.push(self.model.currentClause());
            self.model.currentClause(self.model.newClause());
            self.model.isChanged(true);
        },
        newClause: function (field, operator, value, checked) {
            var clause = {
                field: ko.observable(field || ""),
                operator: ko.observable(operator || "EQ"),
                value: ko.observable(value),
                checked: ko.observable(checked == null ? true : checked),
                shortDisplayName: ko.pureComputed(function () {
                    var parts = clause.field().split('.');
                    return parts[parts.length - 1] + " " + self.model.getOperatorText(clause.operator()) + " " + clause.value();
                }),
                displayName: ko.pureComputed(function () {
                    return clause.field() + " " + self.model.getOperatorText(clause.operator()) + " " + clause.value();
                }),
                toggle: function () {
                    clause.checked(!clause.checked());
                    self.model.isChanged(true);
                }
            };

            return clause;
        },
        newClauseValueFocus: function (data, e) {
            if (self.model.currentClause().field()) {
                var availableValues = IoTApp.DeviceIndex.getAvailableValuesFromPath(self.model.currentClause().field());
                if (self.model.currentClause().operator() == "IN") {
                    $(e.target).autocomplete({
                        source: function (request, response) {
                            var terms = util.split(this.term);
                            terms.pop();
                            var filteredValues = availableValues.filter(function (item) { return terms.indexOf(item) === -1 });
                            response($.ui.autocomplete.filter(filteredValues, util.extractLast(request.term)));
                        },
                        minLength: 0,
                        focus: function () {
                            return false;
                        },
                        select: function (event, ui) {
                            var terms = util.split(this.value);
                            terms.pop();
                            terms.push(ui.item.value);
                            terms.push("");
                            this.value = terms.join(", ");
                            $(this).change();
                            setTimeout(function () { $(e.target).autocomplete("search", ""); }, 0);

                            return false;
                        }
                    });
                }
                else {
                    $(e.target).autocomplete({
                        source: availableValues,
                        minLength: 0,
                        select: function (event, ui) {
                            $(this).val(ui.item.value).change();
                        }
                    });
                }

                $(e.target).autocomplete("search", "");
            }
        },
        newClauseValueBlur: function (data, e) {
            if (self.model.currentClause().operator() == "IN") {
                self.model.currentClause().value(util.trim(self.model.currentClause().value(), ", "));
            }

            if ($(e.target).data('ui-autocomplete')) {
                $(e.target).autocomplete("destroy");
            }
        },
        newClauseValueKeypress: function (data, e) {
            if (e.keyCode == 13 && self.model.canAddClause()) {
                self.model.newClauseValueBlur(data, e);
                self.model.addClause();
                return false;
            }
            
            return true;
        },
        newClauseValuePlaceHolder: ko.pureComputed(function () {
            return self.model.currentClause().operator() == "IN" ? resources.clauseMultipleValuesHint : resources.clauseSingleValueHint;
        }),
        unCheckClause: function (clause) {
            clause.checked(false);
            self.model.isChanged(true);
            IoTApp.DeviceIndex.reloadGrid();
        },
        unCheckAllClauses: function() {
            self.model.clauses().forEach(function (clause) {
                clause.checked(false);
            });
            self.model.isChanged(true);
            IoTApp.DeviceIndex.reloadGrid();
        },
        clearAdvancedClause: function () {
            self.model.advancedClause("");
            self.model.isChanged(true);
        },
        setAdvanced: function(flag) {
            self.model.isAdvanced(flag);
            if (flag) {
                var filter = self.model.getFilterModel();
                api.generateAdvanceClause(filter.clauses, function (sql) {
                    self.model.advancedClause(sql);
                });
            }
            self.model.isChanged(true);
        },
        advancedClauseChanged: function (data, e) {
            self.model.isChanged(true);
        },
        operators: {
            "EQ": "=",
            "NE": "!=",
            "LT": "<",
            "GT": ">",
            "LE": "<=",
            "GE": ">=",
            "IN": "in"
        },
        getOperatorText: function (operator) {
            return self.model.operators[operator] || "";
        }
    };

    self.model.currentClause(self.model.newClause());

    var util = {
        split: function (val) {
            return val.split(/,\s*/);
        },
        extractLast: function(term) {
            return util.split(term).pop();
        },
        trim: function (str, chars) {
            if (str) {
                chars = chars.replace(/[\[\](){}?*+\^$\\.|\-]/g, "\\$&");
                str = str.replace(new RegExp("^[" + chars + "]+|[" + chars + "]+$", "g"), '');
            }

            return str;
        }
    }
    var api = {
        GetSuggestClauses: function (callback) {
            var url = "/api/v1/suggestedClauses?skip=0&take=10";
            return $.ajax({
                url: url,
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetRecentFilter);
                }
            });
        },
        getFilters: function (callback) {
            var url = "/api/v1/filters?max=10";
            return $.ajax({
                url: url,
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetRecentFilter);
                }
            });
        },
        findFilter: function (filterId, callback) {
            var url = "/api/v1/filters/" + filterId;
            return $.ajax({
                url: url,
                type: 'GET',
                dataType: 'json',
                cache: false,
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetFilter + " : " + filterId);
                    if ($.isFunction(callback)) {
                        callback();
                    }
                }
            });
        },
        getNewFilterName: function (callback) {
            var prefix = "NewFilter";
            $.ajax({
                url: "/api/v1/defaultFilterName/" + prefix,
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetDefaultFilters);
                }
            });
        },
        generateAdvanceClause: function (clauses, callback) {
            $.ajax({
                url: "/api/v1/generateAdvanceClause",
                type: 'POST',
                dataType: 'json',
                data: { Name: 'any', Clauses: clauses },
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGenerateSql);
                }
            });
        },
        saveFilter: function (filter, callback) {
            var url = "/api/v1/filters";
            return $.ajax({
                url: url,
                type: 'POST',
                data: filter,
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToSaveFilter);
                }
            });
        },
        deleteFilter: function (filterId, forceDelete, callback) {
            var url = "/api/v1/filters/" + filterId + "/" + forceDelete;
            return $.ajax({
                url: url,
                type: 'DELETE',
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                },
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteFilter);
                }
            });
        },
        getFilterNameList: function (callback) {
            $.ajax({
                url: '/api/v1/filterList',
                type: 'GET',
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(callback)) {
                        callback(result.data);
                    }
                }
            });
        }
    }
    var init = function (uiState, callback) {
        if (!self.initialized) {
            self.initialized = true;

            ko.applyBindings(self.model, $('.filter_panel').get(0));
            ko.applyBindings(self.model, $('.filter_panel_dialog').get(0));

            api.GetSuggestClauses(function (data) {
                if (data) {
                    data.forEach(function (item) {
                        var newClause = self.model.newClause(item.columnName, item.clauseType, item.clauseValue);
                        self.model.defaultClauses.push(newClause);
                    });
                }

                var filterId = resources.filterId || uiState.filterId || defaultFilterId;
                self.model.loadFilter(filterId, false, callback);
            });

            self.model.loadFilters();

            IoTApp.Controls.NameSelector.create($('#txtField'), { type: IoTApp.Controls.NameSelector.NameListType.deviceInfo | IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property });
        }
        else {
            callback();
        }
    }
    var initToolbar = function ($container) {
        self.toolbarInitialized = true;
        $container.attr('data-bind', 'template: { name: "tableHeaderTemplate"}');
        ko.applyBindings(self.model, $container.get(0));
    }
    var fillFilterModel = function (data) {
        var filter = self.model.getFilterModel();
        return $.extend(data, filter);
    }
    var updateFilterResult = function (recordsFiltered, recordsTotal) {
        self.model.recordsFiltered(recordsFiltered);
        self.model.recordsTotal(recordsTotal);
    }
    var getFilterId = function () {
        return self.model.id();
    }
    var getFilterName = function () {
        return self.model.name();
    }
    var saveFilterIfNeeded = function () {
        if (self.model.isChanged()) {
            self.model.saveFilter();
        }
    }
    var setMultiSelectionMode = function (mode) {
        self.model.isMultiSelectionMode(mode);
    }

    return {
        init: init,
        initToolbar: initToolbar,
        fillFilterModel: fillFilterModel,
        updateFilterResult: updateFilterResult,
        getFilterId: getFilterId,
        getFilterName: getFilterName,
        saveFilterIfNeeded: saveFilterIfNeeded,
        openSaveAsDialogForSelectedDevices: self.model.openSaveAsDialogForSelectedDevices,
        saveFilterForSelectedDevices: self.model.saveFilterForSelectedDevices,
        setMultiSelectionMode: setMultiSelectionMode
    }
}, [jQuery, resources]);

ko.bindingHandlers.openFilterPopover = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var settings = ko.utils.unwrapObservable(valueAccessor());
        var template = settings.template;
        var filterNameList = settings.filterNameList;
        var content = "<div id='filter-popver'>" + $(template).html() + "</div>";
        var options = settings.options;
        var defaultOptions = {
            html: true,
            placement: 'bottom',
            trigger: 'manual',
            content: function () {
                return content;
            }
        };
        options = $.extend(true, {}, defaultOptions, options);
        //Workaround: we don't need a title for popover, keep the orginal title and restore it back when the popover is configurated.
        var title = $(element).attr('title');
        $(element).attr('title', '');
        $(element).popover(options).click(function () {
            $(this).popover('toggle');
            var thePopover = $('#filter-popver');
            if (thePopover.is(':visible')) {
                IoTApp.Controls.NameSelector.create($('#lookupFilterBox', thePopover), null, filterNameList());
                ko.applyBindings(viewModel, thePopover.get(0));
            }
        });
        $(element).attr('title', title);

        $('body').on('click', function (e) {
            if (typeof $(e.target).data('original-title') == 'undefined' && !$(e.target).parents().is('.popover.in') && !$(e.target).parents().is('.ui-autocomplete')) {
                $(element).popover('hide').data('bs.popover').inState.click = false;
            }
        });
    }
};