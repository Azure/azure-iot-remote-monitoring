IoTApp.createModule("IoTApp.Helpers.Highlight", function () {
    "use strict";

    var highlight = {
        highlightText: function (textBoxId) {
            var selector = '#' + textBoxId;
            $(selector).select();
        }

    }

    return highlight;
}, [jQuery]);

IoTApp.createModule("IoTApp.Helpers.Dates", function () {
    "use strict";

    var localizeDate = function localizeDate(date, format) {
        if (!date) return resources.notApplicableValue || 'n/a';
        var currentMoment = moment(date).locale(cultureInfo);
        if (currentMoment.year() == 9999) return resources.notApplicableValue || 'n/a';
        return currentMoment.format(format);
    };

    var localizeDates = function localizeDates() {
        $('[data-date]').each(function () {

            var date = new Date($(this).attr('data-date'));
            var utcDate = moment(date).utc();

            $(this).html(localizeDate(utcDate.local(), 'L, LTS'));
        });
    };

    var dates = {
        localizeDate: localizeDate,
        localizeDates: localizeDates
    };

    return dates;
}, [jQuery, moment, cultureInfo]);

// Helper to save the most-recently selected DeviceId in a cookie 
// (or save a blank string if no recently-selected DeviceId)
IoTApp.createModule("IoTApp.Helpers.DeviceIdState", function () {
    "use strict";

    Cookies.json = true;

    var cookieOptions = {
        "path": '/',
        "secure": true
    };

    // use a separate cookie here from other areas, as this cookie will often 
    // be changed independently of other cookie values

    var saveDeviceIdToCookie = function (deviceId) {
        Cookies.set('device-id', { "deviceId": deviceId }, cookieOptions);
    }

    var saveDeviceIdToCookieFromQueryString = function () {
        // get deviceId from current query string
        var deviceId = IoTApp.Helpers.QueryString.getParameter('deviceId');

        // write it to the cookie
        saveDeviceIdToCookie(deviceId);
    }

    var getDeviceIdFromCookie = function () {
        var c = Cookies.get('device-id');

        if (!c || !c.deviceId) {
            return null;
        }

        return c.deviceId;
    }

    return {
        cookieOptions: cookieOptions,
        saveDeviceIdToCookie: saveDeviceIdToCookie,
        saveDeviceIdToCookieFromQueryString: saveDeviceIdToCookieFromQueryString,
        getDeviceIdFromCookie: getDeviceIdFromCookie
    };
});

IoTApp.createModule("IoTApp.Helpers.Numbers", function () {
    "use strict";

    var localizeFromInvariant = function localizeFromInvariant(text) {
        var number = Globalize.parseFloat(text, null, 'en-US');
        if (isNaN(number)) {
            return text;
        } else {
            return localizeNumber(number);
        }
    };

    var localizeNumber = function localizeNumber(number) {
        return Globalize.format(number, 'N', cultureInfo);
    };

    return {
        localizeFromInvariant: localizeFromInvariant,
        localizeNumber: localizeNumber
    };
}, [jQuery, Globalize, cultureInfo]);

IoTApp.createModule("IoTApp.Helpers.IccidState", function () {
    "use strict";

    Cookies.json = true;

    var cookieOptions = {
        "path": '/',
        "secure": true
    };

    var saveIccidToCookie = function (iccid) {
        Cookies.set('iccid-id', { "iccid": iccid }, cookieOptions);
    }
    var getIccidFromCookie = function () {
        var c = Cookies.get('iccid-id');

        if (!c || !c.iccid) {
            return null;
        }

        return c.iccid;
    }

    return {
        cookieOptions: cookieOptions,
        saveIccidToCookie: saveIccidToCookie,
        getIccidFromCookie: getIccidFromCookie
    };
});

IoTApp.createModule("IoTApp.Helpers.QueryString", function () {

    // returns a single parameter from the current query string
    // adapted from SO: http://stackoverflow.com/a/901144
    var getParameter = function (name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    return {
        getParameter: getParameter
    }
});

IoTApp.createModule("IoTApp.Helpers.RenderRetryError", function () {

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

    return renderRetryError;
});

IoTApp.createModule("IoTApp.Helpers.String", function () {
    var renderLongString = function (message, maxSize, placeHolder) {
        if (!message) return '';
        if (placeHolder && maxSize < message.length) {
            var v = message.substring(0, maxSize);
            return v + placeHolder;
        }
        return message;
    }

    var capitalizeFirstLetter = function (string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    var setupTooltipForEllipsis = function (container, titleFunc) {
        $('*', container).filter(function () {
            return $(this).css('text-overflow') == 'ellipsis';
        }).each(function () {
            var $this = $(this);
            if (this.offsetWidth < this.scrollWidth && !$this.attr('title')) {
                var title = titleFunc ? titleFunc.call(this) : $this.html();
                $this.attr('title', title);
            }
        });
    }

    return {
        capitalizeFirstLetter:capitalizeFirstLetter,
        renderLongString: renderLongString,
        setupTooltipForEllipsis: setupTooltipForEllipsis
    }
});

IoTApp.createModule("IoTApp.Helpers.DataType", function () {
    var getDataType = function (value) {
        var type;
        if ($.isNumeric(value)) {
            return resources.twinDataType.number
        }
        else if (/^true$|^false$/i.test(value)) {
            return resources.twinDataType.boolean
        }
        else {
            return resources.twinDataType.string;
        }
    }

    return {
        getDataType: getDataType
    }
});

$(function () {
    "use strict";

    $(document).on("click", ".button_copy", function () {
        var textboxId = $(this).data('id');
        IoTApp.Helpers.Highlight.highlightText(textboxId);
    });


    /* tooltip */
    $(document).tooltip({
        hide: false,
        show: false,
        content: function () {
            return $(this).prop('title');
        }
    });
    var copy;
    $(document).on("mouseover", ".button_copy", function () {
        var inputSelector = '#' + $(this).data('id');
        copy = baseLayoutResources.clickToSelectAll;
        $(inputSelector).siblings().attr('title', copy);
    });
    $(document).on("click", ".button_copy", function () {
        var inputSelector = ".ui-tooltip-content";
        var isMac = (navigator.userAgent.toUpperCase().indexOf("MAC") !== -1);
        if (isMac) {
            copy = baseLayoutResources.commandCToCopy;
        }
        else {
            copy = baseLayoutResources.controlCToCopy;
        }
        $(inputSelector).html(copy);
    });

    //Catch any ajax call that has a 401 status and
    //take the user to the sign-in page
    $(document).ajaxError(function (e, xhr, settings) {
        if (xhr.status == 401) {
            window.location = '/Account/SignIn';
        }
    });

    IoTApp.Helpers.Dates.localizeDates();
}, baseLayoutResources);
