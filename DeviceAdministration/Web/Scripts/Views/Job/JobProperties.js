IoTApp.createModule('IoTApp.JobProperties', function () {
    "use strict";

    var self = this;

    var init = function (jobId, updateCallback) {
        self.jobId = jobId;
        self.updateCallback = updateCallback;
        getJobPropertiesView();
    }

    var getJobPropertiesView = function () {
        $('#loadingElement').show();

        $.ajaxSetup({ cache: false });
        $.get('/Job/GetJobProperties', { jobId: self.jobId }, function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.JobIndex.toggleProperties();
            }
            onJobPropertiesDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveRuleFromService, $('#details_grid_container'), function () { getJobPropertiesView(); });
        });
    }

    var onJobPropertiesDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        var cancelButton = $('#cancelJobAction');
        if (cancelButton != null) {
            cancelButton.on("click", function () {
                $.ajax({
                    "dataType": 'json',
                    'type': 'PUT',
                    'url': '/api/v1/jobs/' + self.jobId + '/cancel',
                    'cache': false,
                    'success': self.successCallback
                });
            });
        }

        setDetailsPaneLoaderHeight();
    }

    var setDetailsPaneLoaderHeight = function () {
        /* Set the height of the Device Details progress animation background to accommodate scrolling */
        var progressAnimationHeight = $("#details_grid_container").height() + $(".details_grid__grid_subhead.button_details_grid").outerHeight();

        $(".loader_container_details").height(progressAnimationHeight);
    };

    var onBegin = function () {
        $('#button_job_status').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        $('#button_job_status').removeAttr("disabled");
        if (result.success) {
            self.updateCallback();
        } else if (result.error) {
            IoTApp.Helpers.Dialog.displayError(result.error);
        } else {
            IoTApp.Helpers.Dialog.displayError(resources.jobUpdateError);
        }
    }

    var onFailure = function (result) {
        $('#button_job_status').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.jobUpdateError);
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }

    return {
        init: init,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete
    }
}, [jQuery, resources]);
