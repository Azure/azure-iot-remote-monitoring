IoTApp.createModule('IoTApp.DeviceListColumns', function () {
    "use strict";
    
    var self = this;
    self.reservedColumnNames = ['deviceId', 'tags.HubEnabledState'];
    self.model = {
        columns: ko.observableArray([]),
        nameSelectorText: ko.observable(''),
        editingItem: ko.observable(null),
        originalValue: null,
        moveUp: function (column) {
            var idx = self.model.columns.indexOf(column);
            if (idx > 0) {
                self.model.swap(idx, idx - 1);
            }
        },
        moveDown: function (column) {
            var idx = self.model.columns.indexOf(column);
            if (idx < self.model.columns().length - 1) {
                self.model.swap(idx, idx + 1);
            }
        },
        swap: function (idx1, idx2) {
            var low = Math.min(idx1, idx2);
            var high = Math.max(idx1, idx2);
            this.columns.splice(low, 2, this.columns()[high], this.columns()[low]);
        },
        remove: function (column) {
            self.model.columns.remove(column);
            applyFilters();
        },
        edit: function (column) {
            self.model.editingItem(column);
            self.model.originalValue = column.alias;
            $('.device_list_columns_displayname_text').select();
        },
        close: function () {
            self.model.editingItem(null);
        },
        cancel: function () {
            self.model.editingItem().alias = self.model.originalValue;
            self.model.editingItem(null);
        },
        checkKey: function (data, e) {
            if (e.keyCode == 27) {
                self.model.cancel();
            }
            else if (e.keyCode == 13) {
                self.model.close();
            }

            return true;
        },
        isReserved: function (column) {
            return self.reservedColumnNames.indexOf(column.name) > -1;
        }
    };

    var getDeviceListColumnsView = function () {

        $('.details_grid_closed__grid_subhead').text(resources.editColumnsPanelLabel);
        $('.details_grid__grid_subhead').html(resources.editColumnsPanelLabel);
        $('#loadingElement').show();

        return $.get('/Device/GetDeviceListColumns', function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.DeviceIndex.toggleDetails();
            }
            onDeviceListColumnsDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveColumnsFromService, $('#details_grid_container'), function () { getDeviceListColumnsView(); });
        });
    };

    var onDeviceListColumnsDone = function (html) {

        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        IoTApp.Controls.NameSelector.create($('.name_selector__text'), {
            type: IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property,
            position: IoTApp.Controls.NameSelector.Position.rightBottom
        });
        applyFilters();
        $('.name_add__button').click(addColumn);
        $('.name_selector__text').keyup(function (e) {
            if (e.keyCode === 13) {
                addColumn();
            }
        });
        $('.device_list_columns_loaddefault_text').click(function () {
            IoTApp.Helpers.Dialog.confirm(resources.loadDefaultConfirmation, function (result) {
                if (result) {
                    $('#loadingElement').show();
                    $.ajax({
                        url: '/api/v1/deviceListColumns/global',
                        type: 'GET',
                        success: function (result) {
                            $('#loadingElement').hide();
                            self.model.columns(result.data);
                        },
                        error: function () {
                            $('#loadingElement').hide();
                            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveColumnsFromService);
                        }
                    });
                }
            });
        });
    };

    var setColumns = function (columns) {
        self.model.columns(columns);
        ko.applyBindings(self.model, $('.device_list_column_editor_container').get(0));
    };

    var addColumn = function () {
        var columnName = self.model.nameSelectorText();
        if (columnName) {
            if (self.model.columns().filter(function (column) { return column.name === columnName }).length === 0) {
                self.model.columns.push({
                    name: columnName,
                    alias: createDefaultDisplayName(columnName)
                });
            }

            self.model.nameSelectorText('');
            applyFilters();
        }
    };

    var applyFilters = function () {
        var filters = self.model.columns().map(function (column) {
            return column.name;
        });

        IoTApp.Controls.NameSelector.applyFilters($('.name_selector__text'), filters);
    };

    var createDefaultDisplayName = function (columnName) {
        var parts = columnName.split('.');

        return parts[parts.length -1].toUpperCase();
    };

    var updateColumns = function (saveAsGlobal) {
        $('#loadingElement').show();
        var url = '/api/v1/deviceListColumns?saveAsGlobal=' +saveAsGlobal;
        $.ajax({
            url: url,
            data: { '': self.model.columns() },
            type: 'PUT',
            success: function (result) {
                $('#loadingElement').hide();
                close();
                IoTApp.DeviceIndex.reinitializeDeviceList();
            },
            error: function () {
                $('#loadingElement').hide();
                IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateColumns);
            }
        });
    };

    var close = function () {
        IoTApp.DeviceIndex.toggleDetails();
    };

    return {
        init: getDeviceListColumnsView,
        setColumns: setColumns,
        updateColumns: updateColumns,
        close: close
    }
}, [jQuery, resources]);