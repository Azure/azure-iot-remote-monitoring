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

    var PositionType = {
        leftBottom: { my: "left top", at: "left bottom" },
        rightBottom: { my: "right top", at: "right bottom" },
        leftTop: { my: "left bottom", at: "left top" },
        rightTop: { my: "right bottom", at: "right top" },
    };

    var prefixes = {};
    prefixes[NameListType.tag] = "tags.";
    prefixes[NameListType.desiredProperty] = "desired.";
    prefixes[NameListType.reportedProperty] = "reported.";

    var create = function ($element, options, data) {
        options = $.extend({
            type: NameListType.all,
            position: PositionType.leftBottom
        }, options);

        var defaultPrefix = prefixes[options.type];

        $element.autocomplete({
            source: [],
            select: function (event, ui) {
                $(this).val(ui.item.value).change();
            },
            minLength: 0,
            position: options.position 
        }).focus(function (e) {
            if (!$(this).val() && defaultPrefix) {
                $(this).val(defaultPrefix).change();
                var input = this;
                setTimeout(function () { setCaretPosition(input, defaultPrefix.length) }, 0);
            }

            $(this).autocomplete("search", $(this).val());
        }).keypress(function(e){ 
            if (e.keyCode == '13'){
                $(this).autocomplete('close');
            }
        });

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

            $element.autocomplete('option', 'source', items);
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
            cache: false,
            success: function (result) {
                callback(result.data);
            }
        });
    }

    var setCaretPosition = function (input, pos) {
        if (input.setSelectionRange) {
            input.setSelectionRange(pos, pos);
        }
        else if (input.createTextRange) {
            var range = input.createTextRange();
            range.collapse(true);
            range.moveEnd('character', pos);
            range.moveStart('character', pos);
            range.select();
        }
    };

    return {
        create: create,
        loadNameList: loadNameList,
        getSelectedItem: getSelectedItem,
        NameListType: NameListType,
        Position: PositionType,
        applyFilters: applyFilters
    };
}, [jQuery]);

IoTApp.createModule("IoTApp.Controls.Dialog", function () {
    "use strict";

    var self = this;
    var dialogHtml = '<div class="dialog_container"><div class="dialog_mask"><div class="dialog_dialog"><img src="/Content/img/column_delete.svg" class="dialog_close_img"/><div class="dialog_title"><h2 class="dialog_title_text"></h2></div><div class="dialog_content"></div></div></div></div>';
    
    var create = function (options) {
        
        options = $.extend({
            container: $('body')
        }, options);

        self.dialog = $(dialogHtml);
        if (options.dialogId) {
            self.dialog.attr('id', options.dialogId);
        }
        $('.dialog_title_text', self.dialog).html(options.title);
        $('.dialog_content', self.dialog).html($(options.templateId).html());

        $('.dialog_close_img', self.dialog).click(function () {
            destory();
        });

        options.container.append(self.dialog);

        self.dialog.show();

        return self.dialog;
    }

    var destory = function () {
        self.dialog.remove();
    }

    return {
        create: create
    };
}, [jQuery]);