IoTApp.createModule(
    'IoTApp.Dashboard.TelemetryHistorySummary',
    function initTelemetryHistorySummary() {
        'use strict';

        var averageDeviceHumidityContainer;
        var averageDeviceHumidityControl;
        var averageDeviceHumidityLabel;
        var averageHumidityVisual;
        var lastAvgHumidity;
        var lastMaxHumidity;
        var lastMinHumidity;
        var maxDeviceHumidityContainer;
        var maxDeviceHumidityControl;
        var maxDeviceHumidityLabel;
        var maxHumidityVisual;
        var maxValue;
        var minDeviceHumidityContainer;
        var minDeviceHumidityControl;
        var minDeviceHumidityLabel;
        var minHumidityVisual;
        var minValue;

        var createDataView = function createDataView(indicatedValue) {

            var categoryMetadata;
            var dataView;
            var dataViewTransform;
            var graphMetadata;

            dataViewTransform = powerbi.data.DataViewTransform;

            graphMetadata = {
                columns: [
                    {
                        isMeasure: true,
                        roles: { 'Y': true },
                        objects: { general: { formatString: resources.telemetryGaugeNumericFormat } },
                    },
                    {
                        isMeasure: true,
                        roles: { 'MinValue': true },
                    },
                    {
                        isMeasure: true,
                        roles: { 'MaxValue': true },
                    }
                ],
                groups: [],
                measures: [0]
            };

            categoryMetadata = {
                values: dataViewTransform.createValueColumns([
                    {
                        source: graphMetadata.columns[0],
                        values: [indicatedValue],
                    }, {
                        source: graphMetadata.columns[1],
                        values: [minValue],
                    }, {
                        source: graphMetadata.columns[2],
                        values: [maxValue],
                    }])
            };

            dataView = {
                metadata: graphMetadata,
                single: { value: indicatedValue },
                categorical: categoryMetadata
            };

            return dataView;
        };

        var createDefaultStyles = function createDefaultStyles() {

            var dataColors = new powerbi.visuals.DataColorPalette();

            return {
                titleText: {
                    color: { value: 'rgba(51,51,51,1)' }
                },
                subTitleText: {
                    color: { value: 'rgba(145,145,145,1)' }
                },
                colorPalette: {
                    dataColors: dataColors,
                },
                labelText: {
                    color: {
                        value: 'rgba(51,51,51,1)',
                    },
                    fontSize: '11px'
                },
                isHighContrast: false,
            };
        };

        var createVisual = function createVisual(targetControl) {

            var height;
            var pluginService;
            var singleVisualHostServices;
            var visual;
            var width;

            height = $(targetControl).height();
            width = $(targetControl).width();

            pluginService = powerbi.visuals.visualPluginFactory.create();
            singleVisualHostServices = powerbi.visuals.singleVisualHostServices;

            // Get a plugin
            visual = pluginService.getPlugin('gauge').create();

            visual.init({
                element: targetControl,
                host: singleVisualHostServices,
                style: createDefaultStyles(),
                viewport: {
                    height: height,
                    width: width
                },
                settings: { slicingEnabled: true },
                interactivity: { isInteractiveLegend: false, selection: false },
                animation: { transitionImmediate: true }
            });

            return visual;
        };

        var init = function init(telemetryHistorySummarySettings) {

            maxValue = telemetryHistorySummarySettings.gaugeMaxValue;
            minValue = telemetryHistorySummarySettings.gaugeMinValue;

            averageDeviceHumidityContainer = telemetryHistorySummarySettings.averageDeviceHumidityContainer;
            averageDeviceHumidityControl = telemetryHistorySummarySettings.averageDeviceHumidityControl;
            averageDeviceHumidityLabel = telemetryHistorySummarySettings.averageDeviceHumidityLabel;
            maxDeviceHumidityContainer = telemetryHistorySummarySettings.maxDeviceHumidityContainer;
            maxDeviceHumidityControl = telemetryHistorySummarySettings.maxDeviceHumidityControl;
            maxDeviceHumidityLabel = telemetryHistorySummarySettings.maxDeviceHumidityLabel;
            minDeviceHumidityContainer = telemetryHistorySummarySettings.minDeviceHumidityContainer;
            minDeviceHumidityControl = telemetryHistorySummarySettings.minDeviceHumidityControl;
            minDeviceHumidityLabel = telemetryHistorySummarySettings.minDeviceHumidityLabel;

            averageHumidityVisual = createVisual(averageDeviceHumidityControl);
            maxHumidityVisual = createVisual(maxDeviceHumidityControl);
            minHumidityVisual = createVisual(minDeviceHumidityControl);
        };

        var redraw = function redraw() {
            var height;
            var width;

            if (minDeviceHumidityControl &&
                minHumidityVisual &&
                (lastMinHumidity || (lastMinHumidity === 0))) {
                height = minDeviceHumidityControl.height();
                width = minDeviceHumidityControl.width();

                minHumidityVisual.update({
                    dataViews: [createDataView(lastMinHumidity)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (maxDeviceHumidityControl &&
                maxHumidityVisual &&
                (lastMaxHumidity || (lastMaxHumidity === 0))) {
                height = maxDeviceHumidityControl.height();
                width = maxDeviceHumidityControl.width();

                maxHumidityVisual.update({
                    dataViews: [createDataView(lastMaxHumidity)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (averageDeviceHumidityControl &&
                averageHumidityVisual &&
                (lastAvgHumidity || (lastAvgHumidity === 0))) {
                height = averageDeviceHumidityControl.height();
                width = averageDeviceHumidityControl.width();

                averageHumidityVisual.update({
                    dataViews: [createDataView(lastAvgHumidity)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }
        };

        var resizeTelemetryHistorySummaryGuages =
            function resizeTelemetryHistorySummaryGuages() {

                var height;
                var padding;
                var width;

                padding = 0;

                if (averageDeviceHumidityContainer &&
                    averageDeviceHumidityLabel &&
                    averageDeviceHumidityControl) {

                    height =
                        averageDeviceHumidityContainer.height() -
                        averageDeviceHumidityLabel.height() -
                        padding;

                    width = averageDeviceHumidityContainer.width() - padding;

                    averageDeviceHumidityControl.height(height);
                    averageDeviceHumidityControl.width(width);
                }

                if (maxDeviceHumidityContainer &&
                    maxDeviceHumidityLabel &&
                    maxDeviceHumidityControl) {

                    height =
                        maxDeviceHumidityContainer.height() -
                        maxDeviceHumidityLabel.height() -
                        padding;

                    width = maxDeviceHumidityContainer.width() - padding;

                    maxDeviceHumidityControl.height(height);
                    maxDeviceHumidityControl.width(width);
                }

                if (minDeviceHumidityContainer &&
                    minDeviceHumidityLabel &&
                    minDeviceHumidityControl) {

                    height =
                        minDeviceHumidityContainer.height() -
                        minDeviceHumidityLabel.height() -
                        padding;

                    width = minDeviceHumidityContainer.width() - padding;

                    minDeviceHumidityControl.height(height);
                    minDeviceHumidityControl.width(width);
                }

                redraw();
            };

        var updateTelemetryHistorySummaryData =
            function updateTelemetryHistorySummaryData(
                minHumidity,
                maxHumidity,
                avgHumidity) {

                lastAvgHumidity = avgHumidity;
                lastMaxHumidity = maxHumidity;
                lastMinHumidity = minHumidity;

                redraw();
        };

        return {
            init: init,
            resizeTelemetryHistorySummaryGuages: resizeTelemetryHistorySummaryGuages,
            updateTelemetryHistorySummaryData: updateTelemetryHistorySummaryData
        };
    },
    [jQuery, resources, powerbi]);