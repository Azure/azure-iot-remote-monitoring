IoTApp.createModule('IoTApp.DeviceDetails', function () {
    "use strict";

    $.ajaxSetup({ cache: false });
    var self = this;

    var getDeviceDetailsView = function (deviceId) {
        $('#loadingElement').show();
        self.deviceId = deviceId;

        $.get('/Device/GetDeviceDetails', { deviceId: deviceId }, function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.DeviceIndex.toggleDetails();
            }
            onDeviceDetailsDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
        });

    }

    var getCellularDetailsView = function () {
        $('#loadingElement').show();

        var iccid = IoTApp.Helpers.IccidState.getIccidFromCookie();
        if (iccid == null) {
            renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
            return;
        }

        $.get('/Device/GetDeviceCellularDetails', { iccid: iccid }, function (response) {
            onCellularDetailsDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
        });

    }

    var onCellularDetailsDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        $("#deviceExplorer_CellInformationBack").on("click", function () {
            $('#details_grid_container').empty();
            onDeviceDetailsDone(self.cachedDeviceHtml);
        });
    }

    var onDeviceDetailsDone = function (html) {

        if (self.cachedDeviceHtml  == null) {
            self.cachedDeviceHtml = html;
        }

        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        IoTApp.Helpers.Dates.localizeDates();

        setDetailsPaneLoaderHeight();

        $("#deviceExplorer_cellInformation").on("click", function () {
            $('#details_grid_container').empty();
            getCellularDetailsView();
        });

        $('#deviceExplorer_authKeys').on('click', function () {
            getDeviceKeys(self.deviceId);
        });

        $("#deviceExplorer_deactivateDevice").on("click", function () {

            var anchor = $(this);
            var isEnabled = anchor.data('hubenabledstate');
            isEnabled = !isEnabled;

            $.when(updateDeviceStatus(self.deviceId, isEnabled)).done(function (result) {
                var data = result.data;
                if (result.error || !data) {
                    IoTApp.Helpers.Dialog.displayError(resources.FailedToUpdateDeviceStatus);
                    return;
                }

                var deviceTable = $('#deviceTable').dataTable();
                var selectedTableRowStatus = deviceTable.find('.selected').find('td:eq(0)');

                if (isEnabled) {
                    _enableDisableDetailsLinks(true);
                    selectedTableRowStatus.removeClass('status_false');
                    selectedTableRowStatus.addClass('status_true');
                    selectedTableRowStatus.html(resources.running);
                    anchor.html(resources.deactivateDevice);
                } else {
                    _enableDisableDetailsLinks(false);
                    selectedTableRowStatus.removeClass('status_true');
                    selectedTableRowStatus.addClass('status_false');
                    selectedTableRowStatus.html(resources.disabled);
                    anchor.html(resources.activateDevice);
                }

                var hubDetailsField = $("#deviceDetailsGrid > [name=deviceField_HubEnabledState]");
                if (hubDetailsField) {
                    hubDetailsField.text(isEnabled ? "True" : "False");
                }

                anchor.data('hubenabledstate', isEnabled);
            }).fail(function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateDeviceStatus);
            });

            return false;
        });

        $("#deviceExplorer_removeSimAssociation").on("click", function () {
            $.ajax({
                url: '/Advanced/RemoveIccidFromDevice',
                data: { deviceId: self.deviceId },
                async: true,
                type: "post",
                success: function () {
                    getDeviceDetailsView(self.deviceId);
                }
            });
        });
    }

    var setDetailsPaneLoaderHeight = function () {
        /* Set the height of the Device Details progress animation background to accommodate scrolling */
        var progressAnimationHeight = $("#details_grid_container").height() + $(".details_grid__grid_subhead.button_details_grid").outerHeight();

        $(".loader_container_details").height(progressAnimationHeight);
    };

    var _enableDisableDetailsLinks = function (enabled) {
        if (enabled) {
            $(".link_grid_subheadhead_detail").removeClass("hidden");
            $("#edit_metadata_link").show();
            $('#editConfigLink').show();
            $('#removeDeviceLink').hide();
        } else {
            $(".link_grid_subheadhead_detail").addClass("hidden");
            $("#edit_metadata_link").hide();
            $('#editConfigLink').hide();
            $('#removeDeviceLink').show();
        }
    }

    var updateDeviceStatus = function (deviceId, isEnabled) {
        $('#loadingElement').show();
        var url = "/api/v1/devices/" + self.deviceId + "/enabledstatus";
        var data = {
            deviceId: self.deviceId,
            isEnabled: isEnabled
        };
        return $.ajax({
            url: url,
            type: 'PUT',
            data: data,
            dataType: 'json',
            success: function (result) {
                $('#loadingElement').hide();
                return result.data;
            },
            error: function () {
                $('#loadingElement').hide();
            }
        });
    }

    var getDeviceKeys = function (deviceId) {
        $('#loadingElement').show();
        $.get('/Device/GetDeviceKeys', { deviceId: deviceId }, function (response) {
            onDeviceKeysDone(response);
            // details pane just got longer--make the spinner fully cover it
            setDetailsPaneLoaderHeight();
        }).fail(function () {
            $('#loadingElement').hide();
            IoTApp.Helpers.Dialog.displayError(resources.errorWhileRetrievingKeys);
        });
    }

    var onDeviceKeysDone = function (html) {
        $('#loadingElement').hide();
        $('.deviceExplorer_detailLevel_authKeys').remove();
        $('#deviceExplorer_authKeys').parent().html(html);
    }

    var renderRetryError = function (errorMessage, container, retryCallback) {
        var $wrapper = $('<div />');
        var $paragraph = $('<p />');

        $wrapper.addClass('device_detail_error');
        $wrapper.append($paragraph);
        var node = document.createTextNode(errorMessage);
        $paragraph.append(node);
        $paragraph.addClass('device_detail_error__information');

        var button = $('<button class="button_base device_detail_error__retry_button">' + resources.retry + '</button>');

        button.on("click", function () {
            retryCallback();
        });

        $wrapper.append(button);
        container.html($wrapper);
    }

    return {
        init: function (deviceId) {
            getDeviceDetailsView(deviceId);
        }
    }
}, [jQuery, resources]);
