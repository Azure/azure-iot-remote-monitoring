IoTApp.createModule("IoTApp.Controls.NameSelector", function () {
    "use strict";
    
    var self = this;
    self.dataCache = null;
    var NameListType = {
        deviceInfo: 1,
        tag: 2,
        desiredProperty: 4,
        reportedProperty: 8,
        method: 16
    };
    NameListType.property = NameListType.desiredProperty | NameListType.reportedProperty;
    NameListType.all = NameListType.deviceInfo | NameListType.tag | NameListType.desiredProperty | NameListType.reportedProperty | NameListType.method;

    var create = function ($element, options, data) {
        options = $.extend({
            type: NameListType.all
        }, options);

        if (data) {
            bindData($element, data);
        }
        else {
            loadNameList(options, function (data) {
                bindData($element, data);
            });
        }
    };

    var bindData = function ($element, data) {

        if (data && data.length > 0) {
            //save source data
            $element.data('nameList', data);

            var filters = $element.data('nameListFilters');
            var items = dataToStringArray(data, filters);

            $element.autocomplete({
                source: items,
                select: function(event,ui){
                    $(this).val(ui.item.value).change();
                },
                minLength: 0
            }).focus(function () {
                $(this).autocomplete("search", $(this).val());
            });
        }
    };

    var applyFilters = function ($element, filters) {
        var data = $element.data('nameList');
        if (data) {
            var items = dataToStringArray(data, filters);
            $element.autocomplete('option', 'source', items);
        }
        else {
            $element.data('nameListFilters', filters);
        }
    };

    var dataToStringArray = function (data, filters) {
        var result = data;

        if (filters) {
            result = result.filter(function (item) {
                return filters.indexOf(item.name) === -1;
            });
        }

        result = result.map(function (item) {
            return item.name || item;
        });

        return result;
    };

    var getSelectedItem = function ($element) {
        var result;
        var target = $element.val();
        var data = $element.data('nameList');
        data.forEach(function (item) {
            if (item.name === target) {
                result = item;
                return false;
            }
        });

        return result;
    };

    var loadNameList = function (options, callback) {
        var url = options.externalDataSourceUrl || ("/api/v1/namecache/list/" + options.type);
        return $.ajax({
            url: url,
            type: 'GET',
            success: function (result) {
                callback(result.data);
            }
        });
    }

    return {
        create: create,
        loadNameList: loadNameList,
        getSelectedItem: getSelectedItem,
        NameListType: NameListType,
        applyFilters: applyFilters
    };
}, [jQuery]);