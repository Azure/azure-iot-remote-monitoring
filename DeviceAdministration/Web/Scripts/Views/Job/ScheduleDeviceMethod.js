IoTApp.createModule('IoTApp.ScheduleDeviceMethod', function () {
    'use strict';

    var self = this;

    function MethodParameterEditItem(name, type, value) {
        var self = this;
        self.ParameterName = name;
        self.Type = type
        self.ParameterValue = value;
    }

    function viewModel($) {
        var self = this;
        this.filterId = "";
        this.jobName = ko.observable("");
        this.methodName = ko.observable("");
        this.applicableDevices = ko.observable(0);
        this.inapplicableDevices = ko.observable(0);
        this.totalDevices = ko.observable(0);
        this.methods = {};
        this.clonedMethodName = null;
        this.clonedMethodParameters = [];
        this.currentMethodData = {};

        this.parameters = ko.pureComputed(function () {
            if (self.methodName != undefined
                && self.methodName().length != 0) {

                var rawparam = $.grep(self.methods, function (e) { return e.name == self.methodName(); });
                //Matched method founded
                if (rawparam.length != 0) {
                    var params = $.map(rawparam[0].parameters, function (item) {
                        return new MethodParameterEditItem(item.name, item.type, "")
                    });

                    //Search applicable devices
                    self.isLoading(true);
                    self.currentMethodData = {
                        methodName : rawparam[0].name.replace(/\(\S+|\s+\)/,""),
                        params : params,
                    }
                    self.searchApplicableDevices(self.currentMethodData.methodName,self.currentMethodData.params);

                    return params.length == 0 ? null : params;
                }
                return null;
            }
        }, this);

        this.MatchedDevices = ko.pureComputed(function () {
            if (this.totalDevices() != 0) {
                return resources.SomeDevicesApplicable.replace("{0}", self.applicableDevices()).replace("{1}", self.totalDevices());
            }
        }, this);

        this.UnmatchedDevices = ko.pureComputed(function () {
            if (this.totalDevices() != 0) {
                return resources.SomeDeviceInapplicable.replace("{0}", self.inapplicableDevices()).replace("{1}", self.totalDevices());
            }
        }, this);

        this.gotoDeviceList = function (isMatched, data) {
            $('#countloadingElement').show();
            $.ajax({
                url: '/api/v1/devices/count/' + self.filterId + "/save?isMatched="+ isMatched,
                type: 'POST',
                data: ko.mapping.toJSON({ 'methodName': self.currentMethodData.methodName, 'parameters': self.currentMethodData.params }),
                contentType: "application/json",
                success: function (result) {
                    location.href = resources.redirectUrl + "?filterId=" + result.data.filterId;
                },
                error: function (xhr, status, error) {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateTwin);
                }

            });
        }

        this.searchApplicableDevices = function (methodName, param) {
            $('#countloadingElement').show();
            $.ajax({
                url: '/api/v1/devices/count/' + self.filterId,
                type: 'POST',
                data: ko.mapping.toJSON({ 'methodName': methodName, 'parameters': param }),
                contentType: "application/json",
                success: function (result) {
                    self.isLoading(false);
                    $('#countloadingElement').hide();
                    self.applicableDevices(result.data.applicable);
                    self.inapplicableDevices(result.data.total - result.data.applicable);
                    self.totalDevices(result.data.total)
                },
                error: function (xhr, status, error) {
                    self.isLoading(true);
                    $('#countloadingElement').hide();
                    IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateTwin);
                }

            });
        };


        this.backButtonClicked = function () {
            location.href = resources.redirectUrl;
        }

        this.startDate = ko.observable(moment());
        this.isLoading = ko.observable(true);
        this.maxExecutionTime = ko.observable(30);

        this.beforePost = function (elem) {
            $(elem).find("#StartDateHidden").val(moment(this.startDate()).utc().format());

            $("<input>").attr({
                type: 'hidden',
                name: 'filterName',
                value: this.filterName
            }).appendTo($(elem));

            return true;
        }

        this.cacheNameList = function (namelist) {
            self.methods = namelist;
            self.methods.forEach(function (m) {
                if (self.clonedMethodName && self.clonedMethodName == m.name) {
                    $.map(m.parameters, function (p) {
                        p.value = $.map(self.clonedMethodParameters, function (cp) {
                            return p.name == cp.ParameterName ? cp.ParameterValue : '';
                        })
                    });
                    // notify ko to update computed properties
                    self.methodName(m.name);
                }
            });

            IoTApp.Controls.NameSelector.create(jQuery('.edit_form__methodComboBox'), { type: IoTApp.Controls.NameSelector.NameListType.method }, self.methods);
        }

        this.init = function (data) {
            if (data) {
                self.filterName = data.FilterName;
                self.filterId = data.FilterId;
                self.jobName(data.JobName);
                self.clonedMethodName = data.MethodName;
                self.clonedMethodParameters = data.Parameters;
                self.maxExecutionTime(data.MaxExecutionTimeInMinutes);
            }
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.method }, self.cacheNameList);
        }
    }


    var vm = new viewModel(jQuery);
    return {
        init: function (data) {
            vm.init(data);
            ko.applyBindings(vm, $("content").get(0));
        }
    }

}, [jQuery, resources]);
