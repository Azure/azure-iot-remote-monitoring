IoTApp.createModule('IoTApp.ScheduleDeviceMethod', function () {
    'use strict';

    var self = this;

    function MethodParameterEditItem(name, type, value) {
        var self = this;
        self.ParameterName = name;
        self.Type = type
        self.ParameterValue = value;
    }

    function viewModel() {
        var self = this;
        this.backUrl = ko.observable(resources.redirectToDeviceIndexUrl);
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
        this.startDate = ko.observable(moment());
        this.isLoading = ko.observable(true);
        this.maxExecutionTime = ko.observable(30);
        this.totalFilteredCount = ko.observable();


        this.parameters = ko.pureComputed(function () {
            if (self.methodName != undefined
                && self.methodName().length != 0) {

                var rawMethod = $.grep(self.methods, function (e) { return e.name == self.methodName(); });
                //Matched method founded
                if (rawMethod.length != 0) {
                    var params = $.map(rawMethod[0].parameters, function (item) {
                        return new MethodParameterEditItem(item.name, item.type, item.value || "")
                    });

                    //Search applicable devices
                    self.isLoading(true);
                    self.currentMethodData = {
                        methodName: rawMethod[0].name.replace(/\(\S+|\s+\)/, ""),
                        params: params,
                    }
                    self.searchApplicableDevices(self.currentMethodData.methodName, self.currentMethodData.params);

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
            $('#loadingElement').show();

            if (self.saveMethodFilterQuery) {
                self.saveMethodFilterQuery.abort()
            }
            self.saveMethodFilterQuery = $.ajax({
                url: '/api/v1/devices/count/' + self.filterId + "/save?isMatched=" + isMatched,
                type: 'POST',
                cache: false,
                data: ko.mapping.toJSON({ 'methodName': self.currentMethodData.methodName, 'parameters': self.currentMethodData.params }),
                contentType: "application/json",
                success: function (result) {
                    location.href = resources.redirectToDeviceIndexUrl + "?filterId=" + result.data.filterId;
                },
                error: function (xhr, status, error) {
                    $('#loadingElement').hide();
                    IoTApp.Helpers.Dialog.displayError(resources.failedToCreateTempFilter);
                }

            });
        }

        this.searchApplicableDevices = function (methodName, param) {
            $('#countloadingElement').show();

            if (self.getApplicableDevice) {
                self.getApplicableDevice.abort()
            }
            self.getApplicableDevice = $.ajax({
                url: '/api/v1/devices/count/' + self.filterId,
                type: 'POST',
                cache: false,
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
                    IoTApp.Helpers.Dialog.displayError(resources.failedToSearchApplicableDevice);
                }

            });
        };


        this.backButtonClicked = function () {
            location.href = self.backUrl();
        }

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

        this.getTotalFilterdCount = ko.pureComputed(function () {
            if (self.totalFilteredCount()) {
                return resources.TotalDeviceString.replace(/\{0\}/, self.totalFilteredCount());
            }
            else {
                return resources.TotalDeviceString.replace(/\{0\}/, resources.LoadingText);
            }
        }, this);

        this.init = function (data) {
            if (data) {
                self.filterName = data.FilterName;
                self.filterId = data.FilterId;
                if (resources.originalJobId) {
                    self.backUrl(resources.redirectToJobIndexUrl + "?jobId=" + resources.originalJobId);
                } else {
                    self.backUrl(resources.redirectToDeviceIndexUrl);
                }
                self.jobName(data.JobName);
                self.clonedMethodName = data.MethodName;
                self.clonedMethodParameters = data.Parameters;
                self.maxExecutionTime(data.MaxExecutionTimeInMinutes);

                $.ajax({
                    url: '/api/v1/devices/count/' + self.filterId,
                    type: 'GET',
                    cache: false,
                    success: function (result) {
                        self.totalFilteredCount(result.data);
                    },
                    error: function (xhr, status, error) {
                        IoTApp.Helpers.Dialog.displayError(resources.failedToGetDeviceCount);
                    }
                });
            }
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.method }, self.cacheNameList);
        }
    }


    var vm = new viewModel();
    return {
        init: function (data) {
            vm.init(data);
            ko.applyBindings(vm);
        }
    }

}, [resources]);
