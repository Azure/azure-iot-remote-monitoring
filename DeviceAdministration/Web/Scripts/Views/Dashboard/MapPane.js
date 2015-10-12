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
            startMap();
        });
    }

    var startMap = function () {
        Microsoft.Maps.loadModule('Microsoft.Maps.Themes.BingTheme', { callback: finishMap });
    };

    var finishMap = function() {
        var options = {
            credentials: self.mapApiKey,
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
            animate: false,
            enableSearchLogo: false,
            enableClickableLogo: false
        };

        // Initialize the map
        self.map = new Microsoft.Maps.Map(document.getElementById("deviceMap"), options);

        // Hide the infobox when the map is moved.
        Microsoft.Maps.Events.addHandler(self.map, 'viewchange', hideInfobox);
    }

    var onMapPinClicked = function (e) {
        IoTApp.Dashboard.DashboardDevicePane.setSelectedDevice(e.target.getId());
        displayInfobox(e);
    }

    var displayInfobox = function (e) {
        // Create the infobox for the pushpin
        if (self.pinInfobox != null) {
            hideInfobox(null);
        }

        var id = e.target.getId();
        var width = (id.length * 7) + 35;
        var horizOffset = -(width / 2);

        self.pinInfobox = new Microsoft.Maps.Infobox(e.target.getLocation(),
            {
                title: id,
                typeName: Microsoft.Maps.InfoboxType.mini,
                width: width,
                height: 25,
                visible: true,
                offset: new Microsoft.Maps.Point(horizOffset, 35)
            });

        self.map.entities.push(self.pinInfobox);
    }

    var hideInfobox = function (e) {
        if (self.pinInfobox != null) {
            self.pinInfobox.setOptions({ visible: false });
            self.map.entities.remove(self.pinInfobox);
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
            for (i = 0 ; i < deviceLocations.length; ++i) {
                loc = new Microsoft.Maps.Location(deviceLocations[i].latitude, deviceLocations[i].longitude);

                pinOptions = {
                    id: deviceLocations[i].deviceId,
                    height: 17,
                    width: 17,
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
                Microsoft.Maps.Events.addHandler(pin, 'click', onMapPinClicked);
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