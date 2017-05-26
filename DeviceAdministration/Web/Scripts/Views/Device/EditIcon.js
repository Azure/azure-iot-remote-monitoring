IoTApp.createModule('IoTApp.EditIcon', (function () {
    "use strict";

    var self = this;

    var init = function (deviceId, isSimulatedDevice ) {
        self.viewModel = new viewModel(deviceId, isSimulatedDevice, jQuery)
        ko.applyBindings(self.viewModel);
    }

    var DeviceIcon = function (name, url) {
        var self = this;
        self.name = name;
        self.url = url;
    }


    var viewModel = function (deviceId, isSimulatedDevice ) {
        var self = this;
        this.deviceId = deviceId;
        this.file = null;
        this.maxSizeInMB = 4;
        this.pageSize = 10;
        this.apiRoute = '/api/v1/icons/';
        this.getIconApiRoute = '/api/v1/devices/' + deviceId + '/icon'
        var defaultImage = isSimulatedDevice ? "/Content/img/IoT.svg" : "/Content/img/device_default.svg";
        var defaultDeviceIcon = new DeviceIcon('device_default_svg', defaultImage);
        this.actionType = ko.observable(resources.uploadActionType);
        this.iconList = ko.observableArray([]);
        this.selectedIcon = ko.observable(null);
        this.previewIcon = ko.observable(defaultDeviceIcon);
        this.defaultIcon = ko.observable(defaultDeviceIcon);
        this.currentIcon = ko.observable(defaultDeviceIcon);
        this.removable = ko.observable(false);
        this.currentPage = ko.observable(0);
        this.totalCount = ko.observable(0);
        this.isFirstPage = ko.pureComputed(function () {
            return self.currentPage() == 0;
        });
        this.isLastPage = ko.pureComputed(function () {
            return self.currentPage() == Math.ceil(self.totalCount()/self.pageSize) - 1;
        });
        this.canSave = ko.pureComputed(function () {
            return self.actionType() == resources.uploadActionType && self.previewIcon() != self.defaultIcon()
                || self.actionType() == resources.applyActionType && self.selectedIcon()
                || self.actionType() == resources.removeActionType;
        });

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
            location.href = resources.redirectUrl;
        };

        this.saveIcon = function () {
            var currentIcon;
            if (self.actionType() === resources.uploadActionType) {
                currentIcon = self.previewIcon();
            } else if (self.actionType() == resources.applyActionType) {
                currentIcon = self.selectedIcon();
            } else if (self.actionType() == resources.removeActionType) {
                currentIcon = self.currentIcon();
            } else {
                return false;
            }

            $.ajax({
                url: self.apiRoute + currentIcon.name + '/' + self.deviceId + '/' + self.actionType(),
                type: 'PUT',
                data: {},
                dataType: 'json',
                success: function (result) {
                    location.href = resources.redirectUrl;
                },
                error: function (xhr, status, error) {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateTwin);
                }
            });

            return false;
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

        this.loadCurrentIcon = function () {
            $.ajax({
                url: self.apiRoute + self.deviceId,
                type: 'GET',
                cache: false,
                success: function (result) {
                    if (result.data) {
                        self.currentIcon(result.data);
                        self.removable(true);
                    }
                }
            });
        };

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

        $("#saveBtn").click(function () {
            self.saveIcon();
            return false;
        });

        this.loadIconList(0);
        this.loadCurrentIcon();
    }

    return {
        init: init
    }
}), [jQuery, resources]);