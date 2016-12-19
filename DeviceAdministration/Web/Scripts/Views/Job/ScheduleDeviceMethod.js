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
        this.jobName = ko.observable('');
        this.methodName = ko.observable('');
        this.methods = {};
        this.clonedMethodName = null;
        this.clonedMethodParameters = [];
        this.parameters = ko.pureComputed(function () {
            if (self.methodName != undefined
                && self.methodName().length != 0) {

                var rawparam = $.grep(self.methods, function (e) { return e.name == self.methodName(); });
                if (rawparam.length != 0) {
                    var params = $.map(rawparam[0].parameters, function (item) {
                        return new MethodParameterEditItem(item.name, item.type, item.value || "")
                    })
                    return params.length == 0 ? null : params;
                }
                return null;
            }
        }, this);

        this.backButtonClicked = function () {
            location.href = resources.redirectUrl;
        }

        this.startDate = ko.observable(moment());
        this.isParameterLoading = true;
        this.maxExecutionTime = ko.observable(30);

        this.getMatchedDevices = function (stringtemplate) {
            return stringtemplate.replace("{0}", "2").replace("{1}", "5");
        }
        this.getUnmatchedDevices = function (stringtemplate) {
            return stringtemplate.replace("{0}", "3").replace("{1}", "5");
        }

        this.beforePost = function (elem) {
            $(elem).find("#StartDateHidden").val(moment(this.startDate()).utc().format());
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
                self.jobName(data.JobName);
                self.clonedMethodName = data.MethodName;
                self.clonedMethodParameters = data.Parameters;
                self.maxExecutionTime(data.MaxExecutionTimeInMinutes);
            }
            IoTApp.Controls.NameSelector.loadNameList({ type: IoTApp.Controls.NameSelector.NameListType.method }, self.cacheNameList);
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