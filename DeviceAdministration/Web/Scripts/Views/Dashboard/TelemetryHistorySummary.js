IoTApp.createModule(
    'IoTApp.Dashboard.TelemetryHistorySummary',
    function initTelemetryHistorySummary() {
        'use strict';

        var averageDeviceTremorLevelContainer;
        var averageDeviceTremorLevelControl;
        var averageDeviceTremorLevelLabel;
        var averageTremorLevelVisual;
        var lastAvgTremorLevel;
        var lastMaxTremorLevel;
        var lastMinTremorLevel;
        var maxDeviceTremorLevelContainer;
        var maxDeviceTremorLevelControl;
        var maxDeviceTremorLevelLabel;
        var maxTremorLevelVisual;
        var maxValue;
        var minDeviceTremorLevelContainer;
        var minDeviceTremorLevelControl;
        var minDeviceTremorLevelLabel;
        var minTremorLevelVisual;
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

            averageDeviceTremorLevelContainer = telemetryHistorySummarySettings.averageDeviceTremorLevelContainer;
            averageDeviceTremorLevelControl = telemetryHistorySummarySettings.averageDeviceTremorLevelControl;
            averageDeviceTremorLevelLabel = telemetryHistorySummarySettings.averageDeviceTremorLevelLabel;
            maxDeviceTremorLevelContainer = telemetryHistorySummarySettings.maxDeviceTremorLevelContainer;
            maxDeviceTremorLevelControl = telemetryHistorySummarySettings.maxDeviceTremorLevelControl;
            maxDeviceTremorLevelLabel = telemetryHistorySummarySettings.maxDeviceTremorLevelLabel;
            minDeviceTremorLevelContainer = telemetryHistorySummarySettings.minDeviceTremorLevelContainer;
            minDeviceTremorLevelControl = telemetryHistorySummarySettings.minDeviceTremorLevelControl;
            minDeviceTremorLevelLabel = telemetryHistorySummarySettings.minDeviceTremorLevelLabel;

            averageTremorLevelVisual = createVisual(averageDeviceTremorLevelControl);
            maxTremorLevelVisual = createVisual(maxDeviceTremorLevelControl);
            minTremorLevelVisual = createVisual(minDeviceTremorLevelControl);
        };

        var redraw = function redraw() {
            var height;
            var width;

            if (minDeviceTremorLevelControl &&
                minTremorLevelVisual &&
                (lastMinTremorLevel || (lastMinTremorLevel === 0))) {
                height = minDeviceTremorLevelControl.height();
                width = minDeviceTremorLevelControl.width();

                minTremorLevelVisual.update({
                    dataViews: [createDataView(lastMinTremorLevel)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (maxDeviceTremorLevelControl &&
                maxTremorLevelVisual &&
                (lastMaxTremorLevel || (lastMaxTremorLevel === 0))) {
                height = maxDeviceTremorLevelControl.height();
                width = maxDeviceTremorLevelControl.width();

                maxTremorLevelVisual.update({
                    dataViews: [createDataView(lastMaxTremorLevel)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (averageDeviceTremorLevelControl &&
                averageTremorLevelVisual &&
                (lastAvgTremorLevel || (lastAvgTremorLevel === 0))) {
                height = averageDeviceTremorLevelControl.height();
                width = averageDeviceTremorLevelControl.width();

                averageTremorLevelVisual.update({
                    dataViews: [createDataView(lastAvgTremorLevel)],
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

                if (averageDeviceTremorLevelContainer &&
                    averageDeviceTremorLevelLabel &&
                    averageDeviceTremorLevelControl) {

                    height =
                        averageDeviceTremorLevelContainer.height() -
                        averageDeviceTremorLevelLabel.height() -
                        padding;

                    width = averageDeviceTremorLevelContainer.width() - padding;

                    averageDeviceTremorLevelControl.height(height);
                    averageDeviceTremorLevelControl.width(width);
                }

                if (maxDeviceTremorLevelContainer &&
                    maxDeviceTremorLevelLabel &&
                    maxDeviceTremorLevelControl) {

                    height =
                        maxDeviceTremorLevelContainer.height() -
                        maxDeviceTremorLevelLabel.height() -
                        padding;

                    width = maxDeviceTremorLevelContainer.width() - padding;

                    maxDeviceTremorLevelControl.height(height);
                    maxDeviceTremorLevelControl.width(width);
                }

                if (minDeviceTremorLevelContainer &&
                    minDeviceTremorLevelLabel &&
                    minDeviceTremorLevelControl) {

                    height =
                        minDeviceTremorLevelContainer.height() -
                        minDeviceTremorLevelLabel.height() -
                        padding;

                    width = minDeviceTremorLevelContainer.width() - padding;

                    minDeviceTremorLevelControl.height(height);
                    minDeviceTremorLevelControl.width(width);
                }

                redraw();
            };

        var updateTelemetryHistorySummaryData =
            function updateTelemetryHistorySummaryData(
                minTremorLevel,
                maxTremorLevel,
                avgTremorLevel) {

                lastAvgTremorLevel = avgTremorLevel;
                lastMaxTremorLevel = maxTremorLevel;
                lastMinTremorLevel = minTremorLevel;

                redraw();
        };

        return {
            init: init,
            resizeTelemetryHistorySummaryGuages: resizeTelemetryHistorySummaryGuages,
            updateTelemetryHistorySummaryData: updateTelemetryHistorySummaryData
        };
    },
    [jQuery, resources, powerbi]);