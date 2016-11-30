IoTApp.createModule('IoTApp.EditDesiredProperties', (function () {
    "use strict";

    var self = this;

    var init = function (deviceId) {
        self.viewModel = new viewModel(deviceId)
        IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.desiredProperty }, self.viewModel.cachepropertyList);
 
        ko.applyBindings(self.viewModel);
    }

    var viewModel = function (deviceId) {
        var self = this;
        var defaultData = [
            {
                "key": "",
                "value": {
                    "value": "",
                    "lastUpdated": ""
                }
            }
        ]

        this.properties = ko.mapping.fromJS(defaultData);
        this.backButtonClicked = function () {
            location.href = resources.redirectUrl;
        }
        this.propertieslist = {};

        this.createEmptyPropertyIfNeeded = function (property) {
                self.properties.push({ 'key': "",'value':{'lastUpdated':"",'value': ""}})
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

        this.formSubmit = function () {
            $("#loadingElement").show();
            $.ajax({
                url: '/api/v1/devices/' + deviceId + '/twin/desired',
                type: 'PUT',
                data: ko.mapping.toJS(self.properties),
                contentType:"application/json",
                success: function (result) {
                    location.href = resources.redirectUrl;
                }
            });
        }

        $.ajax({
            url: '/api/v1/devices/' + deviceId + '/twin/desired',
            type: 'GET',
            success: function (result) {
                //self.properties = ko.mapping.fromJS(result.data);
                ko.mapping.fromJS(result.data, self.properties);
            }
        });
    }


    return {
        init:init
    }
}), [jQuery, resources]);