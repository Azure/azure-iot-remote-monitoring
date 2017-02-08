IoTApp.createModule('IoTApp.DeviceCommand', (function () {
    "use strict";

    var self = this;

    var init = function (commands, deviceIsEnabled) {
        setDatatable();
        self._sendCommandButton = $('#sendCommand_button');
        self._backButton = $('.header_main__button_back');
        self.sendCommandForm = $("#command_form");
        self._backButton.show();
        self.commands = commands;
        self.deviceIsEnabled = deviceIsEnabled;
        self.commandsResponse = [];
        self.resendCommandbuttons = $('.resend_command');
        setNavigationEvents();
        commandHistoryErrors();
        $.validator.setDefaults({
            ignore: [],
        });
        self.resendCommandbuttons.on('click', onResendCommandClicked);
    }

        

    var commandHistoryErrors = function() {
        $("#commandHistory .error").parent().on("click", function () {
            var errorMessageElement = $(this).find(".command_history__error_message");
            var errorMessage = errorMessageElement.data("error-message");
            errorMessageElement.html(errorMessage);
            $(this).off("click");
        });

        $('#Command').on("change", function () {
            if ($(this).val() === "") {
                emptyForm();
                return;
            }
            $('#loadingElement').show();
            var command = getCommandByName(this.value);
            setCommandUi(command);

        });
    }

    var onResendCommandClicked = function() {
        var commandName = $(this).data('command-name');
        var deliveryType = $(this).data('command-deliverytype');
        var commandJson = $(this).data('command-json');

        var command = {
            deviceId: resources.deviceId,
            name: commandName,
            deliveryType: deliveryType,
            commandJson: JSON.stringify(commandJson)
        };

        $('#loadingElement').show();

        command["__RequestVerificationToken"] = $('input[name="__RequestVerificationToken"]').val();
        resendCommand(command)
            .done(function() {
                location.reload();
            })
            .fail(function() {
                Dialog.displayError(resources.resendCommandError);
            })
            .always(function() {
                $('#loadingElement').hide();
            });
    }

    var resendCommand = function (command) {
        return $.post(resources.resendCommand, command);
    }

    var setDatatable = function() {
        $.fn.dataTable.moment('M/D/YYYY, h:mm:ss A');

        $('#commandHistory').DataTable({
            "autoWidth": false,
            "pagingType": "simple",
            "lengthChange": false,
            "processing": true,
            "dom": "lrt?",
            "oLanguage": {
                "sInfo": "Devices List (_TOTAL_)"
            },
            "order": [resources.sortColumnIndex, "desc"],
            "pageLength": 1000,
            "columnDefs": [
                { "width": "200", "targets": 1 },
                { className: "table_truncate_with_max_width", targets: [2, 3] }
            ]
        });

        $('#content').show();

        IoTApp.Helpers.String.setupTooltipForEllipsis($('#commandHistory'));
    }


    var onBegin = function () {
        $('#sendCommand_button').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        self._sendCommandButton.removeAttr("disabled");
        if (result.data) {
            location.reload();
        } else {
            _setCommandUI(result);
            restoreDatetimes();
        }
        
    }

    var restoreDatetimes = function() {
        var datetime = $(".dateTimeValue");
        for (var i = 0; i < datetime.length; i++) {
            var datetimeElement = $(datetime[i]);
            var datetimeElementValue = datetimeElement.val();

            var dateoffsetElementId = datetimeElement.data('date-bound');
            var timeElementId = datetimeElement.data('time-bound');
            var timezoneElementId = datetimeElement.data('timezone-bound');

            var dateValues = getRestoredDatetime(datetimeElementValue);

            
            $('#' + dateoffsetElementId).val(dateValues.date);
            $('#' + timeElementId).val(dateValues.time);
            if (dateValues.timezone) {
                $('#' + timezoneElementId).val(dateValues.timezone);
            }
        }
    }

    var getRestoredDatetime = function(value) {
        if (moment(value, moment.ISO_8601, true).isValid()) {
            var date = moment(value, moment.ISO_8601);

            var timezone = 'local';

            if (value.lastIndexOf('Z') === value.length - 1) {
                timezone = 'UTC';
                return getRestoredDatetimeFromDate(date.utc(), timezone);
            }

            return getRestoredDatetimeFromDate(date, timezone);
        }

        return getRestoredDatetimeFromString(value);
    }

    var getRestoredDatetimeFromDate = function (date, timezone) {
        var datetimeValues = {
            "date":  date.format('YYYY-MM-DD'),
            "time": date.format('HH:mm:ss'),
            'timezone': timezone
        };

        return datetimeValues;
    }

    var getRestoredDatetimeFromString = function (value) {
        var splitValue = value.split(/\[0\]|\[1\]|\[2\]/);

        var datetimeValues = {
            "date": splitValue[1],
            "time": splitValue[2],
            'timezone': splitValue[3]
        };

        return datetimeValues;
    }

    var validateForms = function() {
        $("form").data("validator", null);
        $.validator.unobtrusive.parse($("form"));
    }

    var onFailure = function () {
        $('#sendCommand_button').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.commandSendError);
    }

    var setCommandUi = function (command) {
        if (!command) {
            var errorSpan = _commandError();
            self.sendCommandForm.html(errorSpan);
            return errorSpan;
        }

        command.deviceId = resources.deviceId;

        var responseHtml = _getCommandUIFromMemory(command.Name);

        if (responseHtml !== null) {
            $('#loadingElement').hide();
            _setCommandUI(responseHtml);
            return responseHtml;
        }

        $.when(_getCommandUI(command)).done(function (response) {
            self.commandsResponse[command.Name] = response;
            _setCommandUI(response);
            $('#loadingElement').hide();
            return response;
        }).fail(function () {
            $('#loadingElement').hide();
            IoTApp.Helpers.Dialog.displayError(_commandError);
        });
    }

    var datetimeoffsetEvents = function() {
        $('.dateoffset, .timeoffset, .timezone').change(function() {
            var boundTo = $(this).data('bound-to');
            setDateTimeValue(boundTo);
        });
    }

    var setDateTimeValue = function(datetimeElementName) {
        var dateTimeElement = $('[name="' + datetimeElementName + '"]');
        var dateoffsetElementId = dateTimeElement.data('date-bound');
        var timeElementId = dateTimeElement.data('time-bound');
        var timezoneElementId = dateTimeElement.data('timezone-bound');

        var dateoffset = $('#' + dateoffsetElementId);
        var time = $('#' + timeElementId);
        var timezone = $('#' + timezoneElementId);

        var date = mergeDateAndTimeOffsets(dateoffset.val(), time.val(), timezone.val());
        dateTimeElement.val(date);
    }

    var mergeDateAndTimeOffsets = function(date, time, timezone) {
        var datetime = date + " " + time;
        var momentDate = moment(datetime, 'YYYY-MM-DD HH:mm:ss', true);
        if (momentDate.isValid()) {
            if (timezone === "UTC") {
                datetime = datetime + 'Z';
                return datetime;
            }
            return momentDate.format();
        }

        return "[0]" + date + "[1]" + time + "[2]" + timezone;
    }

    var addDateTime = function ()
    {
        if (cultureInfo && $.datepicker.regional[cultureInfo]) {
            $('.datetime').datepicker($.datepicker.regional[cultureInfo]);
        } else if (cultureInfoShort && $.datepicker.regional[cultureInfoShort]) {
            $('.datetime').datepicker($.datepicker.regional[cultureInfoShort]);
        }

        $('.datetime').datepicker('option', 'dateFormat', 'yy-mm-dd');

        $('.datetime').datepicker({
            onClose: function() {
                var boundTo = $(this).data('bound-to');
                if (boundTo) {
                    setDateTimeValue(boundTo);
                }
            }
        });
    }

    var checkboxEvents = function() {
        $('input[type="checkbox"]').on('click', function() {
            var $this = $(this);
            if ($this.val() !== 'true') {
                $this.val('true');
                $('[name="' + $this.attr('id') + '"]').val('true');
            } else {
                $this.val('false');
                $('[name="' + $this.attr('id') + '"]').val('false');
            }
        });
    }

    var getCommandByName = function (commandName) {
        var command = null;
        self.commands.forEach(function (element) {
            if (element.Name === commandName) {
                command = element;
                return false;
            }
        });

        return command;
    }
    
    var emptyForm = function() {
        if (self.sendCommandForm) {
            self.sendCommandForm.empty();
        }
    }

    var _getCommandUI = function (command) {

        var data = {
            deviceId: resources.deviceId,
            command: command
        }
        data["__RequestVerificationToken"] = $('input[name="__RequestVerificationToken"]').val();
        return $.post(resources.commandUI, data, function (response) {
            return response;
        });
    }

    var _setCommandUI = function (html) {
        self.sendCommandForm.html(html);
        bindCommandUIBehaviors();
        validateForms();
    }


    var setNavigationEvents = function () {
        self._backButton.off("click").click(function () {
            location.href = resources.redirectToIndexUrl;
        });
    }

    var bindCommandUIBehaviors = function () {
        addDateTime();
        checkboxEvents();
        datetimeoffsetEvents();
        $('input[type=text]').tooltip().off("mouseover mouseout");
    }

    var _commandError = function() {
        var errorSpan = $("<span>");
        errorSpan.addClass("ui-state-error");
        errorSpan.html(resources.commandError);
        return errorSpan;
    }

    var _getCommandUIFromMemory = function(commandName) {
        var responseHTML = self.commandsResponse[commandName];
        if (responseHTML === undefined) {
            return null;
        }

        return responseHTML;
    }

    var onComplete = function() {
        $('#loadingElement').hide();
    }

    return {
        init: init,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete,
        getCommandByName: getCommandByName,
        setCommandUI: setCommandUi,
        emptyForm: emptyForm
    }
}), [jQuery, resources]);



$(function () {
    "use strict";

    var commands = JSON.parse(resources.commands);
    var deviceIsEnabled = resources.deviceIsEnabled;

    IoTApp.DeviceCommand.init(commands, deviceIsEnabled);
});
