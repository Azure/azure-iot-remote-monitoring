IoTApp.createModule(
    'IoTApp.Dashboard.TelemetryHistorySummary',
    function initTelemetryHistorySummary() {
        'use strict';

        var maxValue;
        var minValue;

        var lastLeftGaugeValue;
        var lastMiddleGaugeValue;
        var lastRightGaugeValue;

        var leftGaugeContainer;
        var leftGaugeControl;
        var leftGaugeLabel;
        var leftGaugeVisual;

        var middleGaugeContainer;
        var middleGaugeControl;
        var middleGaugeLabel;
        var middleGaugeVisual;

        var rightGaugeContainer;
        var rightGaugeControl;
        var rightGaugeLabel;
        var rightGaugeVisual;

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

            rightGaugeContainer = telemetryHistorySummarySettings.rightGaugeContainer;
            rightGaugeControl = telemetryHistorySummarySettings.rightGaugeControl;
            rightGaugeLabel = telemetryHistorySummarySettings.rightGaugeLabel;
            leftGaugeContainer = telemetryHistorySummarySettings.leftGaugeContainer;
            leftGaugeControl = telemetryHistorySummarySettings.leftGaugeControl;
            leftGaugeLabel = telemetryHistorySummarySettings.leftGaugeLabel;
            middleGaugeContainer = telemetryHistorySummarySettings.middleGaugeContainer;
            middleGaugeControl = telemetryHistorySummarySettings.middleGaugeControl;
            middleGaugeLabel = telemetryHistorySummarySettings.middleGaugeLabel;

            rightGaugeVisual = createVisual(rightGaugeControl);
            leftGaugeVisual = createVisual(leftGaugeControl);
            middleGaugeVisual = createVisual(middleGaugeControl);
        };

        var redraw = function redraw() {
            var height;
            var width;

            if (middleGaugeControl &&
                middleGaugeVisual &&
                (lastMiddleGaugeValue || (lastMiddleGaugeValue === 0))) {
                height = middleGaugeControl.height();
                width = middleGaugeControl.width();

                middleGaugeVisual.update({
                    dataViews: [createDataView(lastMiddleGaugeValue)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (leftGaugeControl &&
                leftGaugeVisual &&
                (lastLeftGaugeValue || (lastLeftGaugeValue === 0))) {
                height = leftGaugeControl.height();
                width = leftGaugeControl.width();

                leftGaugeVisual.update({
                    dataViews: [createDataView(lastLeftGaugeValue)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (rightGaugeControl &&
                rightGaugeVisual &&
                (lastRightGaugeValue || (lastRightGaugeValue === 0))) {
                height = rightGaugeControl.height();
                width = rightGaugeControl.width();

                rightGaugeVisual.update({
                    dataViews: [createDataView(lastRightGaugeValue)],
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

                if (rightGaugeContainer &&
                    rightGaugeLabel &&
                    rightGaugeControl) {

                    height =
                        rightGaugeContainer.height() -
                        rightGaugeLabel.height() -
                        padding;

                    width = rightGaugeContainer.width() - padding;

                    rightGaugeControl.height(height);
                    rightGaugeControl.width(width);
                }

                if (leftGaugeContainer &&
                    leftGaugeLabel &&
                    leftGaugeControl) {

                    height =
                        leftGaugeContainer.height() -
                        leftGaugeLabel.height() -
                        padding;

                    width = leftGaugeContainer.width() - padding;

                    leftGaugeControl.height(height);
                    leftGaugeControl.width(width);
                }

                if (middleGaugeContainer &&
                    middleGaugeLabel &&
                    middleGaugeControl) {

                    height =
                        middleGaugeContainer.height() -
                        middleGaugeLabel.height() -
                        padding;

                    width = middleGaugeContainer.width() - padding;

                    middleGaugeControl.height(height);
                    middleGaugeControl.width(width);
                }

                redraw();
            };

        var updateTelemetryHistorySummaryData =
            function updateTelemetryHistorySummaryData(
                minTemperature,
                maxTemperature,
                avgTemperature) {

                lastRightGaugeValue = avgTemperature;
                lastLeftGaugeValue = maxTemperature;
                lastMiddleGaugeValue = minTemperature;

                redraw();
            };

        return {
            init: init,
            resizeTelemetryHistorySummaryGuages: resizeTelemetryHistorySummaryGuages,
            updateTelemetryHistorySummaryData: updateTelemetryHistorySummaryData
        };
    },
    [jQuery, resources, powerbi]);