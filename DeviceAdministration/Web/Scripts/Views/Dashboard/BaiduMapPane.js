IoTApp.createModule('IoTApp.BaiduMapPane', (function () {
    "use strict";

    var self = this;
    var map;
    var pinInfobox;
    var boundsSet = false;

    var init = function () {
        self.map = new BMap.Map("deviceMap");
        self.map.centerAndZoom(new BMap.Point(116.404, 39.915), 11);

        self.map.addControl(new BMap.MapTypeControl());
        self.map.addControl(new BMap.ScaleControl({
            anchor: BMAP_ANCHOR_TOP_LEFT
        }));
        self.map.addControl(new BMap.NavigationControl({
            anchor: BMAP_ANCHOR_TOP_LEFT,
            type: BMAP_NAVIGATION_CONTROL_SMALL
        }));

        self.map.setCurrentCity("北京");
        self.map.enableScrollWheelZoom(true);
    }

    var bindDeviceToMarker = function (marker, deviceId) {
        marker.addEventListener("click", function (e) {
            marker.openInfoWindow(new BMap.InfoWindow(deviceId));
            IoTApp.Dashboard.DashboardDevicePane.setSelectedDevice(deviceId);
        });
    }

    var setDeviceLocationData = function setDeviceLocationData(minLatitude, minLongitude, maxLatitude, maxLongitude, deviceLocations) {
        if (!self.map) {
            return;
        }

        if (!boundsSet) {
            boundsSet = true;

            var points = [
                new BMap.Point(minLongitude, minLatitude),
                new BMap.Point(maxLongitude, maxLatitude)
            ];

            self.map.setViewport(points);
        }

        self.map.clearOverlays();
        for (var i = 0; i < deviceLocations.length; i++) {
            var icon;
            switch (deviceLocations[i].status) {
                case 1:
                    icon = new BMap.Icon(resources.cautionStatusIcon, new BMap.Size(17, 17));
                    break;

                case 2:
                    icon = new BMap.Icon(resources.criticalStatusIcon, new BMap.Size(17, 17));
                    break;

                default:
                    icon = new BMap.Icon(resources.allClearStatusIcon, new BMap.Size(17, 17));
                    break;
            }

            var point = new BMap.Point(deviceLocations[i].longitude, deviceLocations[i].latitude);

            var marker = new BMap.Marker(point);
            marker.setIcon(icon);
            bindDeviceToMarker(marker, deviceLocations[i].deviceId);

            self.map.addOverlay(marker);
        }
    }

    return {
        init: init,
        setDeviceLocationData: setDeviceLocationData
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.BaiduMapPane.init();
});