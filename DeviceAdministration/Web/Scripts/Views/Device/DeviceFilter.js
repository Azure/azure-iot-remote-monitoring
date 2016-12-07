IoTApp.createModule('IoTApp.DeviceFilter', function () {
    "use strict";
    
    var self = this;

    self.model = {
        name: ko.observable("All Devices"),
        filters: ko.observableArray([]),
        clauses: ko.observableArray([]),
        viewSelectedClauseOnly: ko.observable(false),
        isAdvanced: ko.observable(false),
        currentClause: ko.observable(null),
        defaultClauses: [],
        filteredCount: ko.observable(0),
        totalCount: ko.observable(0),
        filteredInfo: ko.pureComputed(function () {
            if (self.model.clauses().filter(function () { }).length > 0) {
                return self.model.filteredCount() + " filtered from " + self.model.totalCount();
            }
            else {
                return self.model.totalCount();
            }
        }),
        showPanel: function () {
            $('.filter_panel_container').show();
        },
        hidePanel: function () {
            $('.filter_panel_container').hide();
        },
        loadFilter: function (data, callback) {
            $('#btnOpen').popover('hide');
            findFilter(data.id, function (filter) {
                self.model.setFilter(filter);
                if (callback) {
                    callback();
                }
            });
        },
        setFilter: function (filter) {
            if (filter) {
                self.model.name(filter.name);
                self.model.isAdvanced(filter.isAdvanced);
                self.model.viewSelectedClauseOnly(false);
                self.model.clauses.removeAll();
                self.model.defaultClauses.forEach(function (item) {
                    item.checked(false);
                    self.model.clauses.push(item);
                });

                filter.filters.forEach(function (item) {
                    var newClause = self.model.newClause(item.columnName, item.filterType, item.filterValue);

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
        },
        addClause: function () {
            self.model.clauses.push(self.model.currentClause());
            self.model.currentClause(self.model.newClause());
        },
        newClause: function (field, operator, value, checked) {
            var clause = {
                field: ko.observable(field),
                operator: ko.observable(operator || "EQ"),
                value: ko.observable(value),
                checked: ko.observable(checked == null ? true : checked),
                shortDisplayName: ko.pureComputed(function () {
                    var parts = clause.field().split('.');
                    return parts[parts.length - 1] + self.model.getOperatorText(clause.operator()) + clause.value();
                }),
                displayName: ko.pureComputed(function () {
                    return clause.field() + self.model.getOperatorText(clause.operator()) + clause.value();
                }),
                toggle: function () {
                    clause.checked(!clause.checked());
                }
            };

            return clause;
        },
        operators: {
            "EQ": "=",
            "NE": "&ne;",
            "LT": "&lt;",
            "GT": "&gt;",
            "LE": "&le;",
            "GE": "&ge;",
            "IN": "in"
        },
        getOperatorText: function (operator) {
            return self.model.operators[operator] || "";
        }
    };

    self.model.currentClause(self.model.newClause());

    var init = function (uiState, callback) {
        
        ko.applyBindings(self.model, $('.filter_panel').get(0));

        getDefaultClauses(function (data) {
            if (data) {
                data.forEach(function (item) {
                    var newClause = self.model.newClause(item.columnName, item.filterType, item.filterValue);
                    self.model.defaultClauses.push(newClause);
                });
            }

            if (resources.queryName === resources.allDevices) {
                callback();
            }
            else if (resources.queryName) {
                self.model.loadFilter({ id: resources.queryName }, callback);
            }
            else {
                callback();
            }
           
        });

        getFilters(function (data) {
            self.model.filters.removeAll();
            self.model.filters.push({ id: "", name: "All Devices" });
            data.forEach(function (item) {
                self.model.filters.push({ id: item.name, name: item.name });
            });
        });

        IoTApp.Controls.NameSelector.create($('#txtField'), { type: IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property });
    }
    var initToolbar = function ($container) {
        $container.attr('data-bind', 'template: { name: "tableHeaderTemplate"}');
        ko.applyBindings(self.model, $container.get(0));
    }
    var getDefaultClauses = function (callback) {
        //Mock
        var data = [{ columnName: "reported.HubEnabledState", filterType: "=", filterValue: "Running" }];
        callback(data);
    }
    var getFilters = function (callback) {
        var url = "/api/v1/queries";
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                if (callback) {
                    callback(result.data);
                }
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetRecentQuery);
            }
        });
    }
    var findFilter = function (filterId, callback) {
        var url = "/api/v1/queries/" + filterId;
        return $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (result) {
                if (callback) {
                    callback(result.data);
                }
            },
            error: function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToGetQuery + " : " + filterId);
                if (callback) {
                    callback();
                }
            }
        });
    }

    return {
        init: init,
        initToolbar: initToolbar
    }
}, [jQuery, resources]);

ko.bindingHandlers.popover = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var settings = ko.utils.unwrapObservable(valueAccessor());
        var template = settings.template;
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
        $(element).popover(options).click(function () {
            $(this).popover('toggle');
            var thePopover = $('#filter-popver');
            if (thePopover.is(':visible')) {
                ko.applyBindings(viewModel, thePopover.get(0));
            }
        });

        $('body').on('click', function (e) {
            if (typeof $(e.target).data('original-title') == 'undefined' && !$(e.target).parents().is('.popover.in')) {
                $(element).popover('hide').data('bs.popover').inState.click = false;
            }
        });
    }
};