IoTApp.createModule('IoTApp.MapPane', (function () {
    "use strict";

    var self = this;
    var map;
    var pinInfobox;

    var init = function () {
        getMap();
    }

    var getMap = function () {
        var locationData = resources.locationData;
        var minLat = locationData.minimumLatitude;
        var maxLat = locationData.maximumLatitude;
        var minLong = locationData.minimumLongitude;
        var maxLong = locationData.maximumLongitude;
        var key = resources.mapApiQueryKey;

        var initialViewBounds = Microsoft.Maps.LocationRect.fromCorners(new Microsoft.Maps.Location(maxLat, minLong), new Microsoft.Maps.Location(minLat, maxLong));
        var options = { credentials: key, bounds: initialViewBounds, mapTypeId: Microsoft.Maps.MapTypeId.aerial, animate: false };

        // Initialize the map
        self.map = new Microsoft.Maps.Map(document.getElementById("deviceMap"), options);
        
        var locationsArray = locationData.deviceLocationList;
        for (var i = 0; i < locationsArray.length; i++) {
            // Define the pushpin location
            var loc = new Microsoft.Maps.Location(locationsArray[i].latitude, locationsArray[i].longitude);

            // Add a pin to the map
            var pin = new Microsoft.Maps.Pushpin(loc, {id: locationsArray[i].deviceId});

            // Add handler for the pushpin click event.
            Microsoft.Maps.Events.addHandler(pin, 'click', displayInfobox);

            // Add the pushpin and infobox to the map
            self.map.entities.push(pin);
        }

        // Hide the infobox when the map is moved.
        Microsoft.Maps.Events.addHandler(self.map, 'viewchange', hideInfobox);
    }

    var displayInfobox = function (e) {
        // Create the infobox for the pushpin
        if (self.pinInfobox != null) {
            hideInfobox(null);
        }

        self.pinInfobox = new Microsoft.Maps.Infobox(e.target.getLocation(),
            {
                title: e.target.getId(),
                height: 35,
                visible: true,
                offset: new Microsoft.Maps.Point(0, 15)
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

    return {
        init: init
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.MapPane.init();
});