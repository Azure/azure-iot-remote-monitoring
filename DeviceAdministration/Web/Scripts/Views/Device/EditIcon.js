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
        this.pageSize = 10;
        this.apiRoute = '/api/v1/devices/' + deviceId + '/icons/';
        this.getIconApiRoute = '/api/v1/devices/' + deviceId + '/icon'

        this.actionType = ko.observable(resources.uploadActionType);
        this.iconList = ko.observableArray([]);
        this.selectedIcon = ko.observable(null);
        this.previewIcon = ko.observable(defaultDeviceIcon);
        this.defaultIcon = ko.observable(defaultDeviceIcon);
        this.currentIcon = ko.observable(defaultDeviceIcon);
        this.removable = ko.observable(false);
        this.currentPage = ko.observable(0);

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
        };

        this.deleteIcon = function (item) {
            $.ajax({
                url: self.apiRoute + item.name,
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
            var pageN = self.currentPage();
            if (pageN > 0) {
                self.loadIconList(pageN - 1);
            }
        };

        this.nextPage = function () {
            var pageN = self.currentPage();
            self.loadIconList(pageN + 1);
        };

        this.loadIconList = function (page) {
            var skip = page * self.pageSize;
            $.ajax({
                url: '/api/v1/devices/' + deviceId + '/icons?skip=' + skip + '&take=' + self.pageSize,
                type: 'GET',
                cache: false,
                success: function (result) {
                    if (!result.data || result.data.length == 0) {
                        return;
                    }
                    self.iconList(result.data);
                    self.currentPage(page);
                    self.selectable();
                }
            });
        };

        this.selectable = function () {
            $(".device_icon_apply_image").click(function () {
                $('.device_icon_apply_image').removeClass('device_icon_apply_image_selected').siblings('a').hide();
                $(this).addClass('device_icon_apply_image_selected')
                $(this).siblings('a').show();
                var name = $(this).get(0).id;
                var icon = new DeviceIcon(name, self.apiRoute + name + '/true');
                self.selectedIcon(icon);
                self.actionType(resources.applyActionType);
            });
        };

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

        this.loadIconList(0);

        $.ajax({
            url: self.getIconApiRoute,
            type: 'GET',
            cache: false,
            success: function (result) {
                if (result.data) {
                    self.currentIcon(result.data);
                    self.removable(true);
                }
            }
        });
    }

    return {
        init: init
    }
}), [jQuery, resources]);