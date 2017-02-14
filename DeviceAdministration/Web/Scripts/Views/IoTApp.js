var IoTApp =
{
    resources: {},
    createModule: function (namespace, module, dependencies) {
        "use strict";

        var nsparts = namespace.split(".");
        var parent = IoTApp;
        // we want to be able to include or exclude the root namespace so we strip
        // it if it's in the namespace
        if (nsparts[0] === "IoTApp") {
            nsparts = nsparts.slice(1);
        }

        function f() {
            return module.apply(this, dependencies);
        }

        f.prototype = module.prototype;

        var innerModule = new f();

        // loop through the parts and create a nested namespace if necessary
        for (var i = 0, namespaceLength = nsparts.length; i < namespaceLength; i++) {
            var partname = nsparts[i];
            // check if the current parent already has the namespace declared
            // if it isn't, then create it
            if (typeof parent[partname] === "undefined") {
                parent[partname] = (i === namespaceLength - 1) ? innerModule : {};
            }
            // get a reference to the deepest element in the hierarchy so far
            parent = parent[partname];
        }

        return parent;
    }
}

ko.bindingHandlers.dateTimePicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize datepicker with some optional options
        var options = allBindingsAccessor().dateTimePickerOptions || {  };
        $(element).datetimepicker(options);

        //when a user changes the date, update the view model
        ko.utils.registerEventHandler(element, "dp.change", function (event) {
            var value = valueAccessor();
            if (ko.isObservable(value)) {
                if (event.date != null && !(event.date instanceof Date)) {
                    value(event.date.toDate());
                } else {
                    value(event.date);
                }
            }
        });

        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            var picker = $(element).data("DateTimePicker");
            if (picker) {
                picker.destroy();
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {

        var picker = $(element).data("DateTimePicker");
        //when the view model is updated, update the widget
        if (picker) {
            var koDate = ko.utils.unwrapObservable(valueAccessor());

            //in case return from server datetime i am get in this form for example /Date(93989393)/ then fomat this
            koDate = (typeof (koDate) !== 'object') ? new Date(parseFloat(koDate.replace(/[^0-9]/g, ''))) : koDate;

            picker.date(koDate);
        }
    }
};

function byteCount(value) {
    return encodeURI(value).split(/%..|./).length - 1;
}

function getCulture() {
    var name = "_culture=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            var localename = c.substring(name.length, c.length);
            //Match to closet culture
            switch (localename) {
                case "zh-Hans": return "zh-cn";
                case "zh-Hant": return "zh-tw";
                case "pt-PT": return "pt-br";
                default: return localename;
            }            
        }
    }
    return window.navigator.language;
}