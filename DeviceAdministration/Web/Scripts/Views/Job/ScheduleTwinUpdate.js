

IoTApp.createModule('IoTApp.ScheduleTwinUpdate', function () {
    'use strict';

    var self = this;
    function PropertiesEditItem(name, value, isDeleted) {
        var self = this;
        self.PropertyName = name;
        self.PropertyValue = value;
        self.isDeleted = isDeleted;
    }

    function TagsEditItem(name, value, isDeleted) {
        var self = this;
        self.TagName = name;
        self.TagValue = value;
        self.isDeleted = isDeleted;
    }

    function viewModel() {
        var self = this;
        this.queryName = ko.observable("");
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
            if (self.properties.indexOf(property) == self.properties().length - 1) {
                self.properties.push(new PropertiesEditItem("", "", false, false))
                self.onepropleft(false);
            }
        }


        this.createEmptyTagIfNeeded = function (tag) {
            if (self.tags.indexOf(tag) == self.tags().length - 1) {
                self.tags.push(new TagsEditItem("", "", false, false))
                self.onetagleft(false);
            }
        }

        this.removeTag = function (tag) {
            self.tags.remove(tag);
            if (self.tags().length <=1) {
                self.onetagleft(true);
            }
        }

        this.removeProperty = function (prop) {
            self.properties.remove(prop);
            if (self.properties().length <= 1) {
                self.onepropleft(true);
            }
        }

        this.beforePost = function (elem) {
            $(elem).find("#StartDate").val(moment(this.startDate()).utc().format());
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

        this.init = function () {
            this.properties.push(new PropertiesEditItem("", "", false));
            this.tags.push(new TagsEditItem("", "", false));
            //IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.tag }, self.cachetagList);
            //IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.desiredProperty }, self.cachepropertyList);
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.method }, self.cachetagList);
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.method }, self.cachepropertyList);
        }
    }

    var vm = new viewModel();
    return {
        init: function () {
            vm.init();
            ko.applyBindings(vm, $("content").get(0));
        }
    }

}, [jQuery, resources]);