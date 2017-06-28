IoTApp.createModule('IoTApp.MapPane', (function () {
    "use strict";

    var self = this;
    var mapApiKey = null;
    var map;
    var pinInfobox;
    var boundsSet = false;

    var init = function () {
        $.ajaxSetup({ cache: false });
        getMapKey();
    }

    var getMapKey = function () {
        $.get('/api/v1/telemetry/mapApiKey', {}, function (response) {
            self.mapApiKey = response;
            finishMap();
        });
    }

    var finishMap = function () {
        var options = {
            credentials: self.mapApiKey,
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
            animate: false,
            enableSearchLogo: false,
            enableClickableLogo: false,
            navigationBarMode: Microsoft.Maps.NavigationBarMode.minified,
            bounds: Microsoft.Maps.LocationRect.fromEdges(71, -28, -55, 28)
        };

        // Initialize the map
        self.map = new Microsoft.Maps.Map('#deviceMap', options);

        // Hide the infobox when the map is moved.
        Microsoft.Maps.Events.addHandler(self.map, 'viewchange', hideInfobox);
    }

    var onMapPinClicked = function () {
        IoTApp.Dashboard.DashboardDevicePane.setSelectedDevice(this.deviceId);
        displayInfobox(this.deviceId, this.location);
    }

    var displayInfobox = function (deviceId, location) {
        hideInfobox();

        var width = (deviceId.length * 7) + 35;
        var horizOffset = -(width / 2);

        var infobox = new Microsoft.Maps.Infobox(location, {
            title: deviceId,
            maxWidth: 1000,
            offset: new Microsoft.Maps.Point(horizOffset, 35),
            showPointer: false
        });
        infobox.setMap(self.map);
        $('.infobox-close').css('z-index', 1);

        self.pinInfobox = infobox;
    }

    var hideInfobox = function () {
        if (self.pinInfobox != null) {
            self.pinInfobox.setMap(null);
            self.pinInfobox = null;
        }
    }

    var setDeviceLocationData = function setDeviceLocationData(minLatitude, minLongitude, maxLatitude, maxLongitude, deviceLocations) {
        var i;
        var loc;
        var mapOptions;
        var pin;
        var pinOptions;

        if (!self.map) {
            return;
        }

        if (!boundsSet) {
            boundsSet = true;

            mapOptions = self.map.getOptions();
            mapOptions.bounds =
                Microsoft.Maps.LocationRect.fromCorners(
                    new Microsoft.Maps.Location(maxLatitude, minLongitude),
                    new Microsoft.Maps.Location(minLatitude, maxLongitude));
            self.map.setView(mapOptions);
        }

        self.map.entities.clear();
        if (deviceLocations) {
            for (i = 0; i < deviceLocations.length; ++i) {
                loc = new Microsoft.Maps.Location(deviceLocations[i].latitude, deviceLocations[i].longitude);

                pinOptions = {
                    zIndex: deviceLocations[i].status
                };

                switch (deviceLocations[i].status) {
                    case 1:
                        pinOptions.icon = resources.cautionStatusIcon;
                        break;

                    case 2:
                        pinOptions.icon = resources.criticalStatusIcon;
                        break;

                    default:
                        pinOptions.icon = resources.allClearStatusIcon;
                        break;
                }

                pin = new Microsoft.Maps.Pushpin(loc, pinOptions);
                Microsoft.Maps.Events.addHandler(pin, 'click', onMapPinClicked.bind({ deviceId: deviceLocations[i].deviceId, location: loc }));
                self.map.entities.push(pin);
            }
        }
    }

    return {
        init: init,
        setDeviceLocationData: setDeviceLocationData
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.MapPane.init();
});