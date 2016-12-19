IoTApp.createModule('IoTApp.ScheduleTwinUpdate', function () {
    'use strict';

    var self = this;
    function PropertiesEditItem(name, value, isDeleted) {
        var self = this;
        self.PropertyName = ko.observable(name);
        self.PropertyValue = ko.observable(value);
        self.isDeleted = ko.observable(isDeleted);
        self.isEmptyValue = ko.computed(function () {
            return self.PropertyValue() == "" || self.PropertyValue() == null;
        })
    }

    function TagsEditItem(name, value, isDeleted) {
        var self = this;
        self.TagName = ko.observable(name);
        self.TagValue = ko.observable(value);
        self.isDeleted = ko.observable(isDeleted);
        self.isEmptyValue = ko.computed(function () {
            return self.TagValue() == "" || self.TagValue() == null;
        })
    }

    function viewModel() {
        var self = this;
        this.jobName = ko.observable("");
        this.properties = ko.observableArray();
        this.tags = ko.observableArray();
        this.startDate = ko.observable(moment());
        this.isPropertiesLoading = true;
        this.isTagsLoading = true;
        this.maxExecutionTime = ko.observable(30);
        this.cachetagList = {};
        this.cachepropertyList = {};
        this.onepropleft = ko.observable(true);
        this.onetagleft = ko.observable(true);
        this.propertieslist = {};
        this.tagslist = {};

        this.createEmptyPropertyIfNeeded = function (property) {
            if (self.properties.indexOf(property) == self.properties().length - 1 && !property.isEmptyValue()) {
                self.properties.push(new PropertiesEditItem("", "", false, false))
                self.onepropleft(false);
            }
            return true;
        }

        this.createEmptyTagIfNeeded = function (tag) {
            if (self.tags.indexOf(tag) == self.tags().length - 1 && !tag.isEmptyValue()) {
                self.tags.push(new TagsEditItem("", "", false, false))
                self.onetagleft(false);
            }
        }

        this.removeTag = function (tag) {
            self.tags.remove(tag);
            if (self.tags().length <= 1) {
                self.onetagleft(true);
            }
        }

        this.backButtonClicked = function () {
            location.href = resources.redirectUrl;
        }

        this.removeProperty = function (prop) {
            self.properties.remove(prop);
            if (self.properties().length <= 1) {
                self.onepropleft(true);
            }
        }

        this.beforePost = function (elem) {
            $(elem).find("#StartDateHidden").val(moment(this.startDate()).utc().format());
            return true;
        }

        this.maketaglist = function (elem, index, data) {
            self.refreshnamecontrol();
        }

        this.makeproplist = function (elem, index, data) {
            self.refreshnamecontrol();
        }

        this.cachepropertyList = function (namelist) {
            self.propertieslist = namelist;
            self.refreshnamecontrol();
        }

        this.cachetagList = function (namelist) {
            self.tagslist = namelist;
            self.refreshnamecontrol();
        }

        this.refreshnamecontrol = function () {
            jQuery('.edit_form__texthalf.edit_form__propertiesComboBox').each(function () {
                IoTApp.Controls.NameSelector.create(jQuery(this), { type: IoTApp.Controls.NameSelector.NameListType.properties }, self.propertieslist);
            });
            jQuery('.edit_form__texthalf.edit_form__tagsComboBox').each(function () {
                IoTApp.Controls.NameSelector.create(jQuery(this), { type: IoTApp.Controls.NameSelector.NameListType.tags }, self.tagslist);
            });
        }

        this.init = function (data) {
            if (data) {
                self.jobName(data.JobName);
                self.maxExecutionTime(data.MaxExecutionTimeInMinutes);

                if (!data.DesiredProperties || data.DesiredProperties.length == 0) {
                    self.properties.push(new PropertiesEditItem("", "", false));
                } else {
                    self.properties($.map(data.DesiredProperties, function (p) {
                        return new PropertiesEditItem(p.PropertyName, p.PropertyValue, false );
                    }));
                }

                if (!data.Tags || data.Tags.length == 0) {
                    self.tags.push(new TagsEditItem("", "", false));
                } else {
                    self.tags($.map(data.Tags, function (t) {
                        return new TagsEditItem(t.TagName, t.TagValue, false);
                    }));
                }
            }
            else {
                self.properties.push(new PropertiesEditItem("", "", false));
                self.tags.push(new TagsEditItem("", "", false));
            }

            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.tag }, self.cachetagList);
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.desiredProperty }, self.cachepropertyList);
        }
    }

    var vm = new viewModel();
    return {
        init: function (data) {
            vm.init(data);
            ko.applyBindings(vm, $("content").get(0));
        }
    }

}, [jQuery, resources]);