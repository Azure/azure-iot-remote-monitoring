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
        suggestedClauses: [],
        recordsFiltered: ko.observable(0),
        recordsTotal: ko.observable(0),
        saveAsName: ko.observable(null),
        twinDataTypeOptions: resources.twinDataTypeOptions,
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
            return self.model.canSaveAs() && !self.model.associatedJobsCount();
        }),
        canSaveAs: ko.pureComputed(function () {
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
            self.model.saveAsName('');
            $('#saveAdFilterButtons').show();
            $('#saveAdFilterButtonsMultiSelection').hide();
            $('.filter_panel_dialog_container').show();
            $('.filter_panel_filtername_saveas_input').select();
        },
        openSaveAsDialogForSelectedDevices: function (selectedDeviceIds) {
            self.selectedDeviceIds = selectedDeviceIds;
            self.model.saveAsName('');
            $('#saveAdFilterButtons').hide();
            $('#saveAdFilterButtonsMultiSelection').show();
            $('.filter_panel_dialog_container').show();
            $('.filter_panel_filtername_saveas_input').select();
        },
        closeSaveAsDialog: function () {
            $('.filter_panel_dialog_container').hide();
        },
        startEditFilterName: function () {
            if (self.model.canSave()) {
                self.originalFilterName = self.model.name();
                $('.device_list_toolbar_filtername').hide();
                $('.device_list_toolbar_filtername_input').show().select();
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
        saveFilter: function (callback, notAllowDefaultFilterName) {
            if (notAllowDefaultFilterName && self.model.name() == resources.defaultFilterName) {
                IoTApp.Helpers.Dialog.displayError(resources.pleaseNameYourFilter, function () {
                    self.model.startEditFilterName();
                });
                return;
            }

            var filter = self.model.getFilterModel();
            $('.loader_container').show();
            api.saveFilter(filter, function (result) {
                if (!self.model.id()) {
                    self.model.id(result.id);
                }
                self.model.isFilterLoadedFromServer(true);
                self.model.loadFilters();
                self.model.isChanged(false);
                $('.loader_container').hide();

                if ($.isFunction(callback)) {
                    callback();
                }
            }, function () {
                $('.loader_container').hide();
            });
        },
        saveAsFilter: function () {
            if (self.model.saveAsName() == resources.defaultFilterName) {
                IoTApp.Helpers.Dialog.displayError(resources.pleaseNameYourFilter, function () {
                    $('.filter_panel_filtername_saveas_input').select();
                });
                return;
            }

            self.model.closeSaveAsDialog();
            var newName = self.model.saveAsName();
            self.model.id(null);
            self.model.name(newName);
            var filter = self.model.getFilterModel();

            $('.loader_container').show();
            api.saveFilter(filter, function (result) {
                self.model.id(result.id);
                self.model.isFilterLoadedFromServer(true);
                self.model.loadFilters();
                self.model.isChanged(false);
                $('.loader_container').hide();
            }, function () {
                $('.loader_container').hide();
            });
        },
        saveAsFilterForSelectedDevices: function (open) {
            if (self.model.saveAsName() == resources.defaultFilterName) {
                IoTApp.Helpers.Dialog.displayError(resources.pleaseNameYourFilter, function () {
                    $('.filter_panel_filtername_saveas_input').select();
                });
                return;
            }

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
            }, function () {
                $('.loader_container').hide();
            });
        },
        saveFilterForSelectedDevices: function (name, deviceIds, successCallback, errorCallback) {
            name = name || resources.defaultFilterName;
            
            self.model.createAndSaveFilterWithClause(name, "deviceId", "IN", deviceIds.join(", "), function (filterId) {
                if ($.isFunction(successCallback)) {
                    successCallback(filterId);
                }
            }, function (exceptionType) {
                if ($.isFunction(errorCallback)) {
                    errorCallback(exceptionType);
                }
            });
        },
        createAndSaveFilterWithClause: function (name, columnName, operator, value, successCallback, errorCallback) {
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
                if ($.isFunction(successCallback)) {
                    successCallback(result.id);
                }
            }, function (exceptionType) {
                if ($.isFunction(errorCallback)) {
                    errorCallback(exceptionType);
                }
            });
        },
        deleteFilter: function (data, e, forceDelete) {
            if (self.model.associatedJobsCount()) {
                forceDelete = true;
            }

            var confirmMessage = forceDelete ? resources.deleteFilterWithJobsConfirmation : resources.deleteFilterConfirmation;

            IoTApp.Helpers.Dialog.confirm(confirmMessage, function (result) {
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
                self.model.associatedJobsCount(filter.associatedJobsCount);
                self.model.viewSelectedClausesOnly(false);
                self.model.clauses.removeAll();
                self.model.suggestedClauses.forEach(function (item) {
                    item.checked(false);
                    self.model.clauses.push(item);
                });

                if (filter.clauses) {
                    filter.clauses.forEach(function (item) {
                        var newClause = self.model.newClause(item.columnName, item.clauseType, item.clauseValue, item.clauseDataType);

                        var existing = self.model.clauses().filter(function (clause) {
                            return clause.field() === newClause.field() &&
                                clause.operator() === newClause.operator() &&
                                clause.value() === newClause.value() && 
                                clause.dataType() === newClause.dataType();
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
            self.model.setFilter({
                id: null,
                name: resources.defaultFilterName,
                clauses: [],
                isAdvanced: false,
                advancedClause: "",
                associatedJobsCount: 0
            });
            self.model.isChanged(true);
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
                isFilterLoadedFromServer: self.model.isFilterLoadedFromServer(),
                associatedJobsCount: self.model.associatedJobsCount()
            }
        },
        restoreState: function () {
            self.model.setFilter(self.state.filter);
            self.model.isChanged(self.state.isChanged);
            self.model.isFilterLoaded(self.state.isFilterLoaded);
            self.model.isFilterLoadedFromServer(self.state.isFilterLoadedFromServer);
            self.model.associatedJobsCount(self.state.associatedJobsCount);
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
                    return self.model.getClauseModel(clause);
                })
            };
        },
        getClauseModel: function (clause) {
            return {
                columnName: clause.field(),
                clauseType: clause.operator(),
                clauseValue: clause.value(),
                clauseDataType: clause.dataType()
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
        removeClause: function (clause) {
            self.model.clauses.remove(clause);
        },
        newClause: function (field, operator, value, dataType, checked) {
            var clause = {
                field: ko.observable(field || ""),
                operator: ko.observable(operator || "EQ"),
                value: ko.observable(value),
                dataType: ko.observable(dataType || resources.twinDataType.string),
                dataTypeConfirmed: false,
                checked: ko.observable(checked == null ? true : checked),
                shortDisplayName: ko.pureComputed(function () {
                    var parts = clause.field().split('.');
                    return clause.formatDisplayName(parts[parts.length - 1], clause.operator(), clause.value(), clause.dataType());
                }),
                displayName: ko.pureComputed(function () {
                    return clause.formatDisplayName(clause.field(), clause.operator(), clause.value(), clause.dataType());
                }),
                formatDisplayName: function (field, operator, value, dataType) {
                    if (operator == 'IN') {
                        var items = util.split(value);

                        $.each(items, function(idx, item) {
                            items[idx] = util.addQuoteIfNeeded(dataType, item);
                        })

                        value = items.join(', ');
                    }
                    else {
                        value = util.addQuoteIfNeeded(dataType, value);
                    }

                    return field + " " + self.model.getOperatorText(operator) + " " + value;
                },
                toggle: function () {
                    clause.checked(!clause.checked());
                    self.model.isChanged(true);
                },
                remove: function (data, e) {
                    IoTApp.Helpers.Dialog.confirm(resources.deleteClauseConfirmation, function (result) {
                        if (result) {
                            api.deleteSuggestedClause(self.model.getClauseModel(clause), function () {
                                getSuggestedClauses();
                            });

                            self.model.removeClause(clause);
                        }
                    });

                    e.stopPropagation();
                }
            };

            return clause;
        },
        newClauseFieldBlur: function (data, e) {
            if (self.model.currentClause().field()) {
                var availableValues = IoTApp.DeviceIndex.getAvailableValuesFromPath(self.model.currentClause().field());
                self.model.setDataTypeFromAvailableValues(availableValues);

                // Convert to string array for autocomplete
                self.availableValues = availableValues.map(String);
            }
        },
        newClauseValueFocus: function (data, e) {
            if (self.model.currentClause().field() && self.availableValues) {
                if (self.model.currentClause().operator() == "IN") {
                    $(e.target).autocomplete({
                        source: function (request, response) {
                            var terms = util.split(this.term);
                            terms.pop();
                            var filteredValues = self.availableValues.filter(function (item) { return terms.indexOf(item) === -1 });
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
                        source: self.availableValues,
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
        newClauseValueKeyup: function (data, e) {
            if (e.keyCode == 13 && self.model.canAddClause()) {
                self.model.newClauseValueBlur(data, e);
                self.model.addClause();
                return false;
            }

            self.model.setDataTypeFromCurrentValue();

            return true;
        },
        newClauseValueChange: function (data, e) {
            self.model.setDataTypeFromCurrentValue();

            return true;
        },
        newClauseValuePlaceHolder: ko.pureComputed(function () {
            return self.model.currentClause().operator() == "IN" ? resources.clauseMultipleValuesHint : resources.clauseSingleValueHint;
        }),
        setDataTypeFromAvailableValues: function (values) {
            var finalType = null;
            $.each(values, function (idx, value) {
                var type = self.model.getDataType(value);

                if (finalType && finalType != type) {
                    finalType = null;
                    return false;
                }

                finalType = type;
            });

            if (finalType) {
                self.model.currentClause().dataType(finalType);
                self.model.currentClause().dataTypeConfirmed = true;
            }
            else {
                self.model.currentClause().dataTypeConfirmed = false;
            }
        },
        getDataType: function (value) {
            var type = $.type(value);

            if (type == "boolean" || type == "number") {
                return resources.twinDataType[type];
            }
            else {
                return resources.twinDataType.string;
            }
        },
        setDataTypeFromCurrentValue: function () {
            if (self.model.currentClause().dataTypeConfirmed) {
                return;
            }

            var value = $('#txtValue').val();
            var values;
            if (self.model.currentClause().operator() == "IN") {
                values = util.split(util.trim(value, ', '));
            }
            else {
                values = [value];
            }

            var finalType = null;
            $.each(values, function (idx, value) {
                var type = self.model.getDataTypeFromString(value);

                if (finalType && finalType != type) {
                    finalType = resources.twinDataType.string;
                    return false;
                }

                finalType = type;
            });

            if (finalType) {
                self.model.currentClause().dataType(finalType);
            }
        },
        getDataTypeFromString: function (value) {
            var type = resources.twinDataType.string;
            if (value === "true" || value === "false") {
                type = resources.twinDataType.boolean;
            }
            else if ($.isNumeric(value)) {
                type = resources.twinDataType.number;
            }

            return type;
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

    var util = {
        split: function (val) {
            return val.split(/,\s*/); f
        },
        extractLast: function (term) {
            return util.split(term).pop();
        },
        trim: function (str, chars) {
            if (str) {
                chars = chars.replace(/[\[\](){}?*+\^$\\.|\-]/g, "\\$&");
                str = str.replace(new RegExp("^[" + chars + "]+|[" + chars + "]+$", "g"), '');
            }

            return str;
        },
        addQuoteIfNeeded: function (dataType, value) {
            if (dataType == resources.twinDataType.string && value.indexOf("\'") != 0 && value.lastIndexOf("\'") != value.length - 1) {
                value = "\'" + value + "\'";
            }

            return value;
        }
    };

    var api = {
        getSuggestedClauses: function (callback) {
            var url = "/api/v1/suggestedClauses?skip=0&take=15";
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
        deleteSuggestedClause: function (clause, callback) {
            var url = "/api/v1/suggestedClauses";
            return $.ajax({
                url: url,
                type: 'DELETE',
                dataType: 'json',
                data: { '': [ clause ] },
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
        saveFilter: function (filter, successCallback, errorCallback) {
            var url = "/api/v1/filters";
            return $.ajax({
                url: url,
                type: 'POST',
                data: filter,
                dataType: 'json',
                success: function (result) {
                    if ($.isFunction(successCallback)) {
                        successCallback(result.data);
                    }
                },
                error: function (xhr, status, error) {
                    var exceptionType;
                    var message;
                    if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.length > 0) {
                        exceptionType = xhr.responseJSON.error[0].exceptionType;
                    }

                    switch(exceptionType) {
                        case "FilterDuplicatedNameException":
                            message = resources.filterNameMustBeUnique;
                            break;
                        default:
                            message = resources.failedToSaveFilter;
                            break;
                    }
                    
                    IoTApp.Helpers.Dialog.displayError(message);
                    
                    if ($.isFunction(errorCallback)) {
                        errorCallback(exceptionType);
                    }
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

            getSuggestedClauses(function (data) {
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
    var getSuggestedClauses = function (callback) {
        api.getSuggestedClauses(function (data) {
            if (data) {
                self.model.suggestedClauses = [];
                data.forEach(function (item) {
                    var newClause = self.model.newClause(item.columnName, item.clauseType, item.clauseValue, item.clauseDataType);
                    self.model.suggestedClauses.push(newClause);
                });
            }

            if ($.isFunction(callback)) {
                callback(data);
            }
        });
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
    var saveFilterIfNeeded = function (callback) {
        if (self.model.isChanged()) {
            self.model.saveFilter(callback);
        }
        else if ($.isFunction(callback)) {
            callback();
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
        setMultiSelectionMode: setMultiSelectionMode,        
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