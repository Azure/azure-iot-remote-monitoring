IoTApp.createModule('IoTApp.EditIcon', (function () {
    "use strict";

    var self = this;

    var init = function (deviceId) {
        self.viewModel = new viewModel(deviceId, jQuery)
        ko.applyBindings(self.viewModel);
    }

    var DeviceIcon = function (name, url) {
        var self = this;
        self.name = name;
        self.url = url;
    }

    var defaultDeviceIcon = new DeviceIcon('device_default_svg', '/Content/img/device_default.svg');

    var viewModel = function (deviceId) {
        var self = this;
        this.deviceId = deviceId;
        this.file = null;
        this.maxSizeInMB = 4;
        this.sizeOk = false;
        this.apiRoute = '/api/v1/devices/' + deviceId + '/icons/';
        this.tagApiRoute = '/api/v1/devices/' + deviceId + '/twin/tag';

        this.actionType = ko.observable(resources.uploadActionType);
        this.iconList = ko.observableArray([]);
        this.selectedIcon = ko.observable(null);
        this.previewIcon = ko.observable(defaultDeviceIcon);
        this.defaultIcon = ko.observable(defaultDeviceIcon);
        this.currentIcon = ko.observable(defaultDeviceIcon);
        this.removable = ko.observable(false);

        this.fileChanged = function (f) {
            self.file = f.files[0];
            $('#filePathBox').val(f.value);
            if (self.file && self.file.size > self.maxSizeInMB * 1024 * 1024) {
                IoTApp.Helpers.Dialog.displayError(resources.overSizedFile);
            } else {
                self.sizeOk = true;
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
                    var icon = new DeviceIcon(result.data.name, self.apiRoute + result.data.name + '/false');
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
                            $('#progressBar').hide();
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
        }

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
                url: self.apiRoute + currentIcon.name + '/' + self.actionType(),
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
        }

        this.selectable = function () {
            $(".device_icon_apply_image").click(function () {
                $(this).siblings().removeClass('device_icon_apply_image_selected');
                $(this).addClass('device_icon_apply_image_selected');
                var name = $(this).get(0).id;
                var icon = new DeviceIcon(name, self.apiRoute + name + '/true');
                self.selectedIcon(icon);
                self.actionType(resources.applyActionType);
            });
        }

        $("#file").change(function () {
            self.fileChanged(this);
        });

        $('#openImageBtn').click(function () {
            $("#file").click();
            if (self.sizeOk) {
                self.uploadImage();
            }
            self.actionType(resources.uploadActionType);
            return false;
        });

        $("#saveBtn").click(function () {
            self.saveIcon();
            return false;
        });

        $.ajax({
            url: self.apiRoute,
            type: 'GET',
            cache: false,
            success: function (result) {
                var icons = $.map(result.data, function (item) {
                    item.url = self.apiRoute + item.name + '/true';
                    return item;
                });
                self.iconList(icons);
                self.selectable();
            }
        });

        $.ajax({
            url: self.tagApiRoute,
            type: 'GET',
            cache: false,
            success: function (result) {
                result.data = $.map(result.data, function (item) {
                    var fullName = 'tags.' + resources.iconTagName;
                    if (item.key == fullName) {
                        self.removable(true);
                        var currentIcon = new DeviceIcon(item.value.value, self.apiRoute + item.value.value + '/true');
                        self.currentIcon(currentIcon);
                    }
                });
            }
        });
    }

    return {
        init: init
    }
}), [jQuery, resources]);