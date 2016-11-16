IoTApp.createModule('IoTApp.ActionProperties', function () {
    "use strict";

    var self = this;
    var getActionPropertiesView = function (ruleOutput, actionId) {
        $('#loadingElement').show();

        self.ruleOutput = ruleOutput;
        self.actionId = actionId;
        self._updateActionButton = $('#updateAction_button');
        self.sendCommandForm = $("#command_form");

        $.get('/Actions/GetAvailableLogicAppActions', { ruleOutput: ruleOutput, actionId: actionId }, function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.ActionsIndex.toggleProperties();
            }
            onActionPropertiesDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveActionFromService, $('#details_grid_container'), function () { getActionPropertiesView(ruleOutput, actionId); });
        });
    }

    var onActionPropertiesDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        setDetailsPaneLoaderHeight();

        selectedActionIdDropdown(self.actionId);

        //get the initial ActionId for the ruleoutput
        self.selectedActionId = $("#ActionId option:selected").text();

        $('#ActionId').on("change", function () {
            self.selectedActionId = $("#ActionId option:selected").text();
        });

        $('#updateAction_button').on("click", function () {
            $.when(updateAction(self.ruleOutput, self.selectedActionId)).done(function () {
                $('#loadingElement').show();
                updateActionTable();
                $('#loadingElement').hide();
            }).fail(function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToUpdateActionId);
            });
        });
    };

    var selectedActionIdDropdown = function (actionId) {
        $("select option[value='" + actionId + "']").prop("selected", true);
    }

    var updateAction = function (ruleOutput, actionId) {
        var data = {
            ruleOutput: ruleOutput,
            actionId: actionId
        }
        data["__RequestVerificationToken"] = $('input[name="__RequestVerificationToken"]').val();
        return $.post('/Actions/UpdateAction', data, function (response) {
            return response;
        });
    }

    var updateActionTable = function () {
        IoTApp.ActionsIndex.reloadGrid();
    }

    var onBegin = function () {
        $('#updateAction_button').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        self._updateActionButton.removeAttr("disabled");
        if (result.data) {
            location.reload();
        } else {
            _setCommandUI(result);
            restoreDatetimes();
        }
    }

    var onFailure = function () {
        $('#updateAction_button').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.actionUpdateError);
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }
    
    var setDetailsPaneLoaderHeight = function () {
        /* Set the height of the Device Details progress animation background to accommodate scrolling */
        var progressAnimationHeight = $("#details_grid_container").height() + $(".details_grid__grid_subhead.button_details_grid").outerHeight();

        $(".loader_container_details").height(progressAnimationHeight);
    };

    var readonlyActionState = function (container) {
        container.empty();
        var $wrapper = $('<div />');
        var $paragraph = $('<p />');

        $wrapper.addClass('device_detail_error');
        $wrapper.append($paragraph);

        container.html($wrapper);
    }

    return {
        init: function (ruleOutput, actionId) {
            getActionPropertiesView(ruleOutput, actionId);
        },
        readonlyActionState: readonlyActionState,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete
    }
}, [jQuery, resources]);
