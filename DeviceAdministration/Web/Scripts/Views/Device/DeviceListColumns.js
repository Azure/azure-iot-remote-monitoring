IoTApp.createModule('IoTApp.DeviceListColumns', function () {
    "use strict";
    
    var self = this;
    self.model = {
        columns: ko.observableArray([]),
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
        }
    };

    var getDeviceListColumnsView = function () {
        self.preHead = $('.details_grid__grid_subhead').html();
        self.preContent = $('#details_grid_container').html();

        $('.details_grid__grid_subhead').html(resources.editColumns);
        $('#loadingElement').show();

        $.get('/Device/GetDeviceListColumns', function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.DeviceIndex.toggleDetails();
            }
            onDeviceListColumnsDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            renderRetryError(resources.unableToRetrieveColumnsFromService, $('#details_grid_container'), function () { getDeviceListColumnsView(deviceId); });
        });
    };

    var onDeviceListColumnsDone = function (html) {

        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        $('.device_list_columns_button_container').appendTo($('.details_grid'));

        IoTApp.Controls.NameSelector.create($('.name_selector__text'), { type: IoTApp.Controls.NameSelector.NameListType.tag | IoTApp.Controls.NameSelector.NameListType.property });
        $('.name_add__button').click(addColumn);
        $('.name_selector__text').keydown(function (e) {
            if (e.keyCode === 13) {
                addColumn();
            }
        });
    };

    var setColumns = function (columns) {
        self.model.columns(columns);
        ko.applyBindings(self.model, $('#selectedColumnsGrid').get(0));
    };

    var addColumn = function () {
        var columnName = $('.name_selector__text').val();
        if (self.model.columns().filter(function (column) { return column.name === columnName }).length === 0) {
            self.model.columns.push({
                name: columnName,
                alias: createDefaultDisplayName(columnName)
            });
        }

        $('.name_selector__text').val('');
    };

    var createDefaultDisplayName = function (columnName) {
        var parts = columnName.split('.');

        return parts[parts.length - 1];
    };

    var updateColumns = function () {
        $('#loadingElement').show();
        var url = 'api/v1/deviceListColumns/UpdateDeviceListColumns'
        $.ajax({
            url: url,
            data: self.model.columns(),
            type: 'PUT',
            success: function (result) {
                $('#loadingElement').hide();
                close();
            },
            error: function () {
                $('#loadingElement').hide();
                IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateColumns);
            }
        });
    };

    var close = function () {
        $('details_grid__grid_subhead').html(self.preHead);
        self.preContent = $('details_grid_container').html(self.preContent);
        IoTApp.DeviceIndex.toggleDetails();
    };

    return {
        init: getDeviceListColumnsView,
        setColumns: setColumns,
        updateColumns: updateColumns,
        close: close
    }
}, [jQuery, resources]);