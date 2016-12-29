IoTApp.createModule('IoTApp.EditTags', (function () {
    "use strict";

    var self = this;

    var init = function (deviceId) {
        self.viewModel = new viewModel(deviceId, jQuery)
        IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.tag }, self.viewModel.cachepropertyList);

        ko.applyBindings(self.viewModel);
    }

    var propertyModel = function (data) {
        var self = this;
        self.key = ko.observable(data.key);
        self.value = ko.mapping.fromJS(data.value);
        self.isDeleted = ko.observable(data.isDeleted);
    }

    var viewModel = function (deviceId, $) {
        var self = this;
        var defaultData = [
            {
                "key": "",
                "value": {
                    "value": "",
                    "lastUpdated": ""
                },
                "isDeleted": false,
            }
        ]

        var mapping = {
            'isDeleted': {
                create: function (data) {
                    return ko.observable(false);
                }
            }
        }

        this.properties = ko.mapping.fromJS(defaultData, mapping);
        this.reported = [];
        this.backButtonClicked = function () {
            location.href = resources.redirectUrl;
        }
        this.propertieslist = {};

        this.createEmptyPropertyIfNeeded = function (property) {
            self.properties.push(new propertyModel({ "key": "", "value": { "value": "", "lastUpdated": "" }, "isDeleted": false }));
            return true;
        }

        this.makeproplist = function (elem, index, data) {
            self.refreshnamecontrol();
        }

        this.cachepropertyList = function (namelist) {
            self.propertieslist = namelist;
            self.refreshnamecontrol();
        }

        this.refreshnamecontrol = function () {
            jQuery('.edit_form__texthalf.edit_form__propertiesComboBox').each(function () {
                IoTApp.Controls.NameSelector.create(jQuery(this), { type: IoTApp.Controls.NameSelector.NameListType.tag }, self.propertieslist);
            });
        }

        this.fromNowValue = function (lastupdate, locale) {
            if (lastupdate() != "" && lastupdate() != null && lastupdate() != undefined) {
                return moment(lastupdate()).locale(locale).fromNow();
            }
            return 'N/A';
        }

        this.formSubmit = function () {
            $("#loadingElement").show();
            //set the 'value' to empty when try to delete the prop.
            var updatedata = $.map(self.properties(), function (item) { if (item.isDeleted() == true) { item.value.value = ""; return item; } else { return item; } })

            $.ajax({
                url: '/api/v1/devices/' + deviceId + '/twin/tag',
                type: 'PUT',
                data: ko.mapping.toJSON(updatedata),
                contentType: "application/json",
                success: function (result) {
                    location.href = resources.redirectUrl;
                },
                error: function (xhr, status, error) {
                    $("#loadingElement").hide();
                    IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateTwin);
                }
            });
        }

        $("form").validate({
            submitHandler: self.formSubmit
        });

        $.ajax({
            url: '/api/v1/devices/' + deviceId + '/twin/tag',
            type: 'GET',
            success: function (result) {

                //add 'isDeleted' field for model binding, default false
                result.data = $.map(result.data, function (item) {
                    item.isDeleted = false;
                    return item;
                });

                ko.mapping.fromJS(result.data, self.properties);

            }
        });
    }


    return {
        init: init
    }
}), [jQuery, resources]);