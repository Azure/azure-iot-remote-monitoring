IoTApp.createModule('IoTApp.ScheduleIconUpdate', (function () {
    "use strict";

    var self = this;

    var init = function (data) {
        self.viewModel = new viewModel(data, jQuery)
        ko.applyBindings(self.viewModel);
    }

    var DeviceIcon = function (name, url) {
        var self = this;
        self.name = name;
        self.url = url;
    }

    var defaultDeviceIcon = new DeviceIcon('device_default_svg', '/Content/img/device_default.svg');

    var viewModel = function (data) {
        var self = this;
        this.file = null;
        this.maxSizeInMB = 4;
        this.pageSize = 10;
        this.apiRoute = '/api/v1/icons/';

        this.jobName = ko.observable("");
        this.filterId = "";
        this.startDate = ko.observable(moment());
        this.maxExecutionTime = ko.observable(0);
        this.backUrl = ko.observable(resources.redirectToDeviceIndexUrl);

        this.getTotalFilterdCount = ko.pureComputed(function () {
            if (self.totalFilteredCount()) {
                return resources.TotalDeviceString.replace(/\{0\}/, self.totalFilteredCount());
            }
            else {
                return resources.TotalDeviceString.replace(/\{0\}/, resources.LoadingText);
            }
        }, this);

        this.actionType = ko.observable(resources.uploadActionType);
        this.iconList = ko.observableArray([]);
        this.selectedIcon = ko.observable(null);
        this.previewIcon = ko.observable(defaultDeviceIcon);
        this.defaultIcon = ko.observable(defaultDeviceIcon);
        this.removable = ko.observable(false);
        this.currentPage = ko.observable(0);
        this.totalCount = ko.observable(0);
        this.totalFilteredCount = ko.observable();

        this.isFirstPage = ko.pureComputed(function () {
            return self.currentPage() == 0;
        });
        this.isLastPage = ko.pureComputed(function () {
            return self.currentPage() == Math.ceil(self.totalCount() / self.pageSize) - 1;
        });
        this.canSave = ko.pureComputed(function () {
            return self.actionType() == resources.uploadActionType && self.previewIcon() != self.defaultIcon()
                || self.actionType() == resources.applyActionType && self.selectedIcon()
                || self.actionType() == resources.removeActionType;
        });

        this.init = function (data) {
            if (data) {
                self.filterId = data.FilterId;
                self.jobName(data.JobName);
                if (resources.originalJobId) {
                    self.backUrl(resources.redirectToJobIndexUrl + "?jobId=" + resources.originalJobId);
                    self.jobName(data.JobName);
                } else {
                    self.backUrl(resources.redirectToDeviceIndexUrl);
                }

                this.loadFilteredCount();
            }
        };

        this.fileChanged = function (f) {
            self.file = f.files[0];
            if (!f.value) return;
            $('#filePathBox').val(f.value);
            if (self.file && self.file.size > self.maxSizeInMB * 1024 * 1024) {
                IoTApp.Helpers.Dialog.displayError(resources.overSizedFile);
            } else {
                self.uploadImage();
                self.actionType(resources.uploadActionType);
            }
        };

        this.uploadImage = function () {
            var data = new FormData();
            data.append("file", this.file);
            $.ajax({
                type: 'post',
                url: self.apiRoute,
                data: data,
                success: function (result) {
                    var icon = new DeviceIcon(result.data.name, result.data.blobUrl);
                    self.previewIcon(icon);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToUploadImage);
                },
                xhr: function () {
                    var xhr = $.ajaxSettings.xhr();
                    xhr.upload.onprogress = function (evt) {
                        var percentage = Math.floor(evt.loaded / evt.total * 100);
                        self.setProgress(percentage);
                        if (percentage == 100) {
                            $('#progressBar').fadeOut(1000);
                        }
                    };
                    return xhr;
                },
                processData: false,
                contentType: false,
            });
        };

        this.setProgress = function (percentage) {
            $('#progressBar').show();
            $('#progressBar').progressbar({
                value: percentage
            });
        };

        this.backButtonClicked = function () {
            location.href = self.backUrl();
        };

        this.beforePost = function (elem) {
            $(elem).find("#StartDateHidden").val(moment(this.startDate()).utc().format());
            if (!self.actionableIcon() || self.actionableIcon() == self.defaultIcon()) {
                IoTApp.Helpers.Dialog.displayError(resources.noSelectedIcon);
                return false;
            }
            return true;
        }

        this.actionableIcon = function () {
            var icon;
            if (self.actionType() === resources.uploadActionType) {
                icon = self.previewIcon();
            } else if (self.actionType() == resources.applyActionType) {
                icon = self.selectedIcon();
            } else {
                icon = new DeviceIcon(null, null);
            }
            return icon;
        };

        this.deleteIcon = function (icon) {
            $.ajax({
                url: self.apiRoute + icon.name,
                type: 'DELETE',
                data: {},
                dataType: 'json',
                success: function (result) {
                    var pageN = self.iconList().length == 1 && self.currentPage() > 0 ? self.currentPage() - 1 : self.currentPage();
                    self.loadIconList(pageN);
                },
                error: function (xhr, status, error) {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteIcon);
                }
            });

            return false;
        };

        this.previousPage = function () {
            if (!self.isFirstPage()) {
                self.loadIconList(self.currentPage() - 1);
            }
        };

        this.nextPage = function () {
            if (!self.isLastPage()) {
                self.loadIconList(self.currentPage() + 1);
            }
        };

        this.loadIconList = function (page) {
            var skip = page * self.pageSize;
            $.ajax({
                url: self.apiRoute + '?skip=' + skip + '&take=' + self.pageSize,
                type: 'GET',
                cache: false,
                success: function (result) {
                    if (!result.data.results || result.data.totalCount == 0) {
                        return;
                    }
                    self.iconList(result.data.results);
                    self.selectedIcon(null);
                    self.totalCount(result.data.totalCount);
                    self.currentPage(page);
                }
            });
        };        

        this.loadFilteredCount = function () {
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

        this.selectIcon = function (icon, event) {
            $('.device_icon_apply_image').removeClass('device_icon_apply_image_selected');
            $(event.target).addClass('device_icon_apply_image_selected');
            self.selectedIcon(icon);
            self.actionType(resources.applyActionType);
        };

        $("#file").change(function () {
            self.fileChanged(this);
        });

        $('#chooseFileBtn').click(function () {
            $("#file").click();
            return false;
        });

        this.loadIconList(0);
        this.init(data);
    }

    return {
        init: init
    }
}), [jQuery, resources]);