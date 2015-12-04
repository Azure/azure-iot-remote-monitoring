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

        var currentMoment = moment(date).locale(cultureInfo);
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
        localizeDates:localizeDates
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

$(function () {
    "use strict";

    $(document).on("click", ".button_copy", function() {
        var textboxId = $(this).data('id');
        IoTApp.Helpers.Highlight.highlightText(textboxId);
    });


    /* tooltip */
    $(document).tooltip({
        hide: false,
        show: false
    });
    var copy;
    $(document).on("mouseover", ".button_copy", function() {
        var inputSelector = '#' + $(this).data('id');
        copy = baseLayoutResources.clickToSelectAll;
        $(inputSelector).siblings().attr('title', copy);
    });
    $(document).on("click", ".button_copy", function() {
        var inputSelector = ".ui-tooltip-content";
        var isMac = (navigator.userAgent.toUpperCase().indexOf("MAC") !== -1);
        if (isMac)
        {
            copy = baseLayoutResources.commandCToCopy;
        }
        else
        {
            copy = baseLayoutResources.controlCToCopy;
        }
        $(inputSelector).html(copy);
    });

    //Catch any ajax call that has a 401 status and
    //take the user to the sign-in page
    $(document).ajaxError(function(e, xhr, settings) {
        if (xhr.status == 401) {
            window.location = '/Account/SignIn';
        }
    });

    IoTApp.Helpers.Dates.localizeDates();
}, baseLayoutResources);
