IoTApp.createModule('IoTApp.DeviceDetails', function () {
    "use strict";

    $.ajaxSetup({ cache: false });
    var self = this;

    var getDeviceDetailsView = function (deviceId) {
        $('#loadingElement').show();
        $('.details_grid_closed__grid_subhead').text(resources.deviceDetailsPanelLabel);
        $('.details_grid__grid_subhead').text(resources.deviceDetailsPanelLabel);
        self.deviceId = deviceId;

        return $.get('/Device/GetDeviceDetails', { deviceId: deviceId }, function (response) {
            onDeviceDetailsDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
        });
    }

    var getScheduleJobView = function (filterId, filterName) {
        $('#loadingElement').show();
        $('.details_grid_closed__grid_subhead').text(resources.scheduleJobPanelLabel);
        $('.details_grid__grid_subhead').text(resources.scheduleJobPanelLabel);

        return $.get('/Job/ScheduleJob', { filterId: filterId, filterName: filterName }, function (response) {
            onScheduleJobReady(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.failedToScheduleJob, $('#details_grid_container'), function () { getScheduleJobView(); });
        });
    }

    var getCellularDetailsView = function (iccid) {
        return $.get("/Device/GetDeviceCellularDetails", { iccid: iccid });
    }

    var onCellularDetailsDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        $("#deviceExplorer_CellInformationBack").on("click", function () {
            $('#details_grid_container').empty();
            onDeviceDetailsDone(self.cachedDeviceHtml);
        });
        return $.Deferred().resolve().promise();
    }

    var displayCellularDetailsView = function () {
        $('#loadingElement').show();

        var iccid = IoTApp.Helpers.IccidState.getIccidFromCookie();
        if (iccid === null) {
            renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
            return;
        }

        getCellularDetailsView(iccid).then(function (response) {
            onCellularDetailsDone(response);
        }, function () {
            $('#loadingElement').hide();
            renderRetryError(resources.unableToRetrieveDeviceFromService, $('#details_grid_container'), function () { getDeviceDetailsView(deviceId); });
        });
    }

    var onDeviceDetailsDone = function (html) {

        if (self.cachedDeviceHtml === null) {
            self.cachedDeviceHtml = html;
        }

        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        IoTApp.Helpers.Dates.localizeDates();

        setDetailsPaneLoaderHeight();

        $("#deviceExplorer_cellInformation").on("click", function () {
            $('#details_grid_container').empty();
            displayCellularDetailsView();
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
                var selectedTableRowStatus = deviceTable.find('.selected').find('.table_status');

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

        if (localStorage.deviceDetailPanel_deviceDetailHidden === "true") {
            $(".devicedetails_toggle_target").toggle();
        }

        if (localStorage.deviceDetailPanel_tagHidden === "true") {
            $(".tag_toggle_target").toggle();
        }

        if (localStorage.deviceDetailPanel_desiredPropertyHidden === "true") {
            $(".desiredproperty_toggle_target").toggle();
        }

        if (localStorage.deviceDetailPanel_reportedPropertyHidden === "true") {
            $(".reportedproperty_toggle_target").toggle();
        }

        if (localStorage.deviceDetailPanel_jobHidden === "true") {
            $(".job_toggle_target").toggle();
        }

        $(".devicedetails_toggle_source").on("click", function () {
            $(".devicedetails_toggle_target").toggle();
            localStorage.deviceDetailPanel_deviceDetailHidden = $(".devicedetails_toggle_target").css("display") === "none";
        });

        $(".tag_toggle_source").on("click", function () {
            $(".tag_toggle_target").toggle();
            localStorage.deviceDetailPanel_tagHidden = $(".tag_toggle_target").css("display") === "none";
        });

        $(".desiredproperty_toggle_source").on("click", function () {
            $(".desiredproperty_toggle_target").toggle();
            localStorage.deviceDetailPanel_desiredPropertyHidden = $(".desiredproperty_toggle_target").css("display") === "none";
        });

        $(".reportedproperty_toggle_source").on("click", function () {
            $(".reportedproperty_toggle_target").toggle();
            localStorage.deviceDetailPanel_reportedPropertyHidden = $(".reportedproperty_toggle_target").css("display") === "none";
        });

        $(".job_toggle_source").on("click", function () {
            $(".job_toggle_target").toggle();
            localStorage.deviceDetailPanel_jobHidden = $(".job_toggle_target").css("display") === "none";
        });
    }

    var onScheduleJobReady = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        setDetailsPaneLoaderHeight();
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

    var loadDeviceJobsInternal = function (deviceId) {
        $.ajaxSetup({ cache: false });
        return $.get('/Device/GetDeviceJobs', { deviceId: deviceId }, function (response) {
            $('#deviceJobGrid').empty();
            $('#deviceJobGrid').html(response);
        });
    }

    var loadDeviceJobs = function (deviceId) {
        if (self.deviceJobLoader) {
            self.deviceJobLoader.abort();
        }

        self.deviceJobLoader = loadDeviceJobsInternal(deviceId);
    }

    var init = function (deviceId) {
        self.cachedDeviceHtml = null;
        getDeviceDetailsView(deviceId);
    }

    return {
        init: init,
        getCellularDetailsView: getCellularDetailsView,
        onCellularDetailsDone: onCellularDetailsDone,
        displayCellularDetailsView: displayCellularDetailsView,
        scheduleJob: getScheduleJobView,
        loadDeviceJobs: loadDeviceJobs
    }
}, [jQuery, resources]);
