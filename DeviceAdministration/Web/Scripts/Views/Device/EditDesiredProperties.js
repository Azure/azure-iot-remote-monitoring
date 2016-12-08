IoTApp.createModule('IoTApp.EditDesiredProperties', (function () {
    "use strict";

    var self = this;

    var init = function (deviceId) {
        self.viewModel = new viewModel(deviceId,jQuery)
        IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.desiredProperty }, self.viewModel.cachepropertyList);

        ko.applyBindings(self.viewModel);
    }

    var propertyModel = function (data) {
        var self = this;
        self.key = ko.observable(data.key);
        self.value = ko.mapping.fromJS(data.value);
        self.isDeleted = ko.observable(data.isDeleted);
    }

    var viewModel = function (deviceId,$) {
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
            self.properties.push(new propertyModel( { "key": "", "value": { "value": "", "lastUpdated": "" }, "isDeleted": false }));
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
                IoTApp.Controls.NameSelector.create(jQuery(this), { type: IoTApp.Controls.NameSelector.NameListType.properties }, self.propertieslist);
            });
        }

        this.fromNowValue = function (lastupdate, locale) {
            if (lastupdate() != "" && lastupdate() != null && lastupdate() != undefined) {
                return moment(lastupdate()).locale(locale).fromNow();
            }
            return 'N/A';
        }

        this.MatchReportedProp = function (desired) {
            if (typeof(desired) == "function") {
                desired = desired();
            }
            if (desired == null || desired == undefined || desired == "") {
                return null
            }
            if (desired.indexOf("desired.") == 0) {
                desired = desired.slice(8, desired.length);
            }
            for (var i = 0; i < this.reported().length;i++) {

                if(this.reported()[i].key().toLowerCase().indexOf(desired.toLowerCase()) >= 0)
                {
                    return this.reported()[i];
                }
            }
        }

        this.formSubmit = function () {
            $("#loadingElement").show();

            //set the 'value' to empty when try to delete the prop.
            var updatedata = $.map(self.properties(), function (item) { if (item.isDeleted() == true) { item.value.value = ""; return item; } else { return item; } })

            $.ajax({
                url: '/api/v1/devices/' + deviceId + '/twin/desired',
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
            url: '/api/v1/devices/' + deviceId + '/twin/desired',
            type: 'GET',
            success: function (result) {
                self.reported = ko.mapping.fromJS(result.data.reported);

                //add 'isDeleted' field for model binding, default false
                result.data.desired = $.map(result.data.desired, function (item) {
                    item.isDeleted = false;
                    return item;
                });

                ko.mapping.fromJS(result.data.desired, self.properties);
            }
        });
    }


    return {
        init: init
    }
}), [jQuery, resources]);