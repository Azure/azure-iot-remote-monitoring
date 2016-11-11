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
            var items = data.map(function (item) {
                return item.name;
            });

            $element.autocomplete({
                source: items,
                minLength: 0
            }).focus(function () {
                $(this).autocomplete("search", $(this).val());
            });
        }
    }

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
        var url = "/api/v1/namecache/list/" + options.type;
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
        NameListType: NameListType
    };
}, [jQuery]);