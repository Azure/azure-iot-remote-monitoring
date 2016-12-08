IoTApp.createModule('IoTApp.DeviceFilter', function () {
    "use strict";
    
    var self = this;

    self.model = {
        id: ko.observable(null),
        name: ko.observable(resources.allDevices),
        isAdvanced: ko.observable(false),
        advancedClause: ko.observable(null),
        clauses: ko.observableArray([]),

        isFilterLoaded: ko.observable(false),
        isFilterLoadedFromServer: ko.observable(false),
        isChanged: ko.observable(false),
        filters: ko.observableArray([]),
        filterNameList: ko.observableArray([]),
        viewSelectedClausesOnly: ko.observable(false),
        currentClause: ko.observable(null),
        defaultClauses: [],
        recordsFiltered: ko.observable(0),
        recordsTotal: ko.observable(0),
        lookupBox: ko.observable(null),
        saveAsName: ko.observable(null),
        checkedClauses: ko.pureComputed(function () {
            return self.model.clauses().filter(function (clause) {
                return clause.checked();
            });
        }),
        filteredInfo: ko.pureComputed(function () {
            if (self.model.isFilterLoaded()) {
                return resources.infoFiltered.replace('{0}', self.model.recordsFiltered()).replace('{1}', self.model.recordsTotal());
            }
            else {
                return self.model.recordsTotal();
            }
        }),
        showPanel: function () {
            if (self.model.name() == resources.allDevices)
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
            $('.filter_panel_dialog_container').show();
            $('.filter_panel_filtername_saveas_input').focus();
        },
        closeSaveAsDialog: function () {
            $('.filter_panel_dialog_container').hide();
        },
        startEditFilterName: function () {
            if (self.model.isFilterLoaded()) {
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
                var name = self.model.lookupBox();
                var filterId;
                self.model.filterNameList().forEach(function (item) {
                    if (item.name === name) {
                        filterId = item.id;
                        return false;
                    }
                });
                self.model.loadFilter(filterId, true);
                return false;
            }
        },
        loadFilters: function() {
            api.getFilters(function (data) {
                self.model.filters.removeAll();
                self.model.filters.push({ id: null, name: resources.allDevices });
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
            self.model.id(null);
            self.model.name(resources.allDevices);
            self.model.isAdvanced(false);
            self.model.advancedClause(null);
            self.model.clauses.removeAll();
            self.model.isFilterLoaded(false);
            self.model.isFilterLoadedFromServer(false);
            self.model.isChanged(false);
        },
        loadFilter: function (filterId, execute, callback) {
            $('#btnOpen').popover('hide');
            if (filterId && filterId !== resources.allDevices) {
                api.findFilter(filterId, function (filter) {
                    self.model.setFilter(filter, true);
                    self.model.isChanged(false);
                    if (execute) {
                        IoTApp.DeviceIndex.reloadGrid();
                    }
                    if ($.isFunction(callback)) {
                        callback();
                    }
                });
            }
            else {
                self.model.resetState();
                if (execute) {
                    IoTApp.DeviceIndex.reloadGrid();
                }
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
        deleteFilter: function () {
            api.deleteFilter(self.model.id(), function () {
                self.model.loadFilters();
                self.model.resetState();
                IoTApp.DeviceIndex.reloadGrid();
            });
        },
        setFilter: function (filter, isLoadedFromServer) {
            if (filter) {
                self.model.isFilterLoaded(true);
                self.model.isFilterLoadedFromServer(isLoadedFromServer);

                self.model.id(filter.id);
                self.model.name(filter.name);
                self.model.isAdvanced(filter.isAdvanced);
                self.model.advancedClause(filter.advancedClause);
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
                    advancedClause: ""
                });
                self.model.isChanged(true);
            });
        },
        executeFilter: function () {
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
        addClause: function () {
            self.model.clauses.push(self.model.currentClause());
            self.model.currentClause(self.model.newClause());
            self.model.isChanged(true);
        },
        newClause: function (field, operator, value, checked) {
            var clause = {
                field: ko.observable(field),
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
            var prefix = "MyNewFilter";
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
        deleteFilter: function (filterId, callback) {
            var url = "/api/v1/filters/" + filterId;
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

                if (resources.filterId) {
                    self.model.loadFilter(resources.filterId, false, callback);
                }
                else {
                    callback();
                }

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


    return {
        init: init,
        initToolbar: initToolbar,
        fillFilterModel: fillFilterModel,
        updateFilterResult: updateFilterResult,
        getFilterId: getFilterId,
        getFilterName: getFilterName,
        saveFilterIfNeeded: saveFilterIfNeeded
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