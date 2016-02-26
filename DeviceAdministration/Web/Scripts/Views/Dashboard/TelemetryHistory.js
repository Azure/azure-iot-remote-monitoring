IoTApp.createModule(
    'IoTApp.Dashboard.TelemetryHistory',
    function initTelemetryHistory() {
        'use strict';

        var lastData;
        var height;
        var targetControl;
        var targetControlContainer;
        var targetControlSubtitle;
        var targetControlTitle;
        var visual;
        var width;
        var hasVisualBeenInitialized = false;
        
        var reservedDataColumns = [
            "timestamp",
            "deviceId",
            "externalTemperature" // TODO: remove this
        ];

        var createDataView = function createDataView(data) {

            var categoryIdentities;
            var categoryMetadata;
            var categoryValues;
            var columns;
            var dataValues;
            var dataView;
            var dataViewTransform;
            var fieldExpr;
            var graphData;
            var graphMetadata;

            dataViewTransform = powerbi.data.DataViewTransform;

            fieldExpr =
                powerbi.data.SQExprBuilder.fieldDef({
                    entity: 'table1',
                    column: 'time'
                });

            graphData = produceGraphData(data);

            categoryValues = graphData.timestamps;

            categoryIdentities =
                categoryValues.map(
                    function (value) {
                        var expr =
                            powerbi.data.SQExprBuilder.equal(
                                fieldExpr,
                                powerbi.data.SQExprBuilder.text(value));
                        return powerbi.data.createDataViewScopeIdentity(expr);
                    });
                    
            var graphMetadataColumns = [
                {
                    displayName: 'Time',
                    isMeasure: true,
                    queryName: 'timestamp',
                    type: powerbi.ValueType.fromDescriptor({ dateTime: true })
                }
            ];
            
            columns = [];
            
            // Create a new column for non-reserved values
            for (var field in data[0]) {
                if (reservedDataColumns.indexOf(field) === -1) {
                    graphMetadataColumns.push({
                        displayName: field,
                        isMeasure: true,
                        format: "0.0",
                        queryName: field,
                        type: powerbi.ValueType.fromDescriptor({ numeric: true })
                    });
                    
                    columns.push({
                        source: graphMetadataColumns[graphMetadataColumns.length - 1],
                        values: graphData[field]
                    })
                }
            }

            graphMetadata = {
                columns: graphMetadataColumns
            };

            dataValues = dataViewTransform.createValueColumns(columns);

            categoryMetadata = {
                categories: [{
                    source: graphMetadata.columns[0],
                    values: categoryValues,
                    identity: categoryIdentities
                }],
                values: dataValues
            };

            dataView = {
                metadata: graphMetadata,
                categorical: categoryMetadata
            };

            return dataView;
        };

        var createDefaultStyles = function createDefaultStyles() {

            var dataColors = new powerbi.visuals.DataColorPalette();

            return {
                titleText: {
                    color: { value: 'rgba(51,51,51,1)' },
                    fontFamily: 'Verdana'
                },
                subTitleText: {
                    color: { value: 'rgba(145,145,145,1)' }
                },
                colorPalette: {
                    dataColors: dataColors,
                },
                labelText: {
                    color: {
                        value: 'rgba(51,51,51,1)'
                    },
                    fontSize: '11px'
                },
                isHighContrast: false,
            };
        };

        var createVisual = function createVisual() {

            var height;
            var pluginService;
            var singleVisualHostServices;
            var width;

            pluginService = powerbi.visuals.visualPluginFactory.create();
            singleVisualHostServices = powerbi.visuals.defaultVisualHostServices;

            height = $(targetControl).height();
            width = $(targetControl).width();

            // Get a plugin
            visual = pluginService.getPlugin('lineChart').create();

            visual.init({
                // empty DOM element the visual should attach to.
                element: targetControl,
                // host services
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
            
            hasVisualBeenInitialized = true;
        };

        var init = function init(telemetryHistorySettings) {

            targetControl = telemetryHistorySettings.targetControl;

            targetControlContainer =
                telemetryHistorySettings.targetControlContainer;

            targetControlSubtitle =
                telemetryHistorySettings.targetControlSubtitle;

            targetControlTitle = telemetryHistorySettings.targetControlTitle;

            createVisual();
        };

        var produceGraphData = function produceGraphData(data) {

            var dateTime;
            var i;
            var item;
            var results;

            results = {
                timestamps: []
            };
            
            for (var field in data[0]) {
                if (reservedDataColumns.indexOf(field) === -1) {
                    results[field] = [];
                }
            }

            for (i = 0 ; i < data.length ; ++i) {
                item = data[i];
                for (var field in item) {
                    if (reservedDataColumns.indexOf(field) === -1) {
                        results[field].push(item[field]);
                    }
                }

                dateTime = new Date(item.timestamp);
                if (!dateTime.replace) {
                    dateTime.replace = ('' + this).replace;
                }

                results.timestamps.push(dateTime);
            }

            return results;
        };

        var redraw = function redraw() {

            var height;
            var width;

            if (!targetControl) {
                return;
            }

            height = $(targetControl).height();
            width = $(targetControl).width();

            if (lastData && hasVisualBeenInitialized) {
                visual.update({
                    dataViews: [createDataView(lastData)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }
        };

        var resizeTelemetryHistoryGrid =
            function resizeTelemetryHistoryGrid() {

                var height;
                var padding;
                var width;

                padding = 20;

                if (targetControlContainer &&
                    targetControlTitle &&
                    targetControlSubtitle &&
                    targetControl) {

                    height =
                        targetControlContainer.height() -
                        targetControlTitle.height() -
                        targetControlSubtitle.height() -
                        padding;

                    width = targetControlContainer.width() - padding;

                    targetControl.height(height);
                    targetControl.width(width);
                }

                redraw();
            };

        var updateTelemetryHistoryGridData = function updateTelemetryHistoryGridData(newData) {
            lastData = newData;
            redraw();
        };

        return {
            init: init,
            resizeTelemetryHistoryGrid: resizeTelemetryHistoryGrid,
            updateTelemetryHistoryGridData: updateTelemetryHistoryGridData
        };
    },
    [jQuery, resources, powerbi]);
