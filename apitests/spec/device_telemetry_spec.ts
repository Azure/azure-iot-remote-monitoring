import request = require('request');

describe('devices api - ', () => {
    describe('device telemetry - ', () => {
        var req: request.RequestAPI<request.Request, request.CoreOptions, Object>;
        // TODO: don't hardcode deviceId
        var deviceId: string;
        beforeAll(function (done) {
            req = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/telemetry' });
            done();
        });

        it('1. Get alert history', (done) => {
            req.get('alertHistory', (err, resp, result: AlertHistory) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.devices).toBeTruthy();
                expect(result.maxLatitude).toBeTruthy();
                expect(result.maxLongitude).toBeTruthy();
                expect(result.minLatitude).toBeTruthy();
                expect(result.minLongitude).toBeTruthy();
                expect(result.totalAlertCount).toBeTruthy();
                expect(result.totalFilteredCount).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0); 
                expect(result.data[0]).toBeTruthy();
                expect(result.data[0].deviceId).toBeTruthy();

                expect(result.data[0].ruleOutput).toBeTruthy();
                expect(result.data[0].timestamp).toBeTruthy();
                expect(result.data[0].value).toBeTruthy();

                expect(result.devices[0]).toBeTruthy();
                expect(result.devices.length).toBeGreaterThan(0);
                expect(result.devices[0].deviceId).toBeTruthy();
                expect(result.devices[0].latitude).toBeTruthy();
                expect(result.devices[0].longitude).toBeTruthy();
                expect(result.devices[0].status).not.toBeNull();
                done();
            });
        }, 10000);

        it('2. Get Device Location Data', (done) => {
            req.get('deviceLocationData', (err, resp, result: DeviceLocationData) => {
                expect(result).toBeTruthy();
                expect(result.deviceLocationList).toBeTruthy();
                expect(result.maximumLongitude).toBeTruthy();
                expect(result.maximumLatitude).toBeTruthy();
                expect(result.minimumLatitude).toBeTruthy();
                expect(result.minimumLongitude).toBeTruthy();

                expect(result.deviceLocationList.length).toBeGreaterThan(0);
                expect(result.deviceLocationList[0]).toBeTruthy();
                expect(result.deviceLocationList[0].deviceId).toBeTruthy();
                deviceId = result.deviceLocationList[0].deviceId;
                expect(result.deviceLocationList[0].latitude).toBeTruthy();
                expect(result.deviceLocationList[0].longitude).toBeTruthy();
                done();
            });
        });

        it('3. Get Device Telemetry Summary', (done) => {
            var options: request.CoreOptions = {
                qs: {
                    deviceId: "SampleDevice001_648"
                }
            }
            req.get('summary', options, (err, resp, result: TelemetrySummary) => {
                expect(result).toBeTruthy();
                expect(result.averageHumidity).toBeTruthy();
                expect(result.deviceId).toBeTruthy();
                expect(result.maximumHumidity).toBeTruthy();
                expect(result.minimumHumidity).toBeTruthy();
                expect(result.timeFrameMinutes).toBeTruthy();
                expect(result.timestamp).toBeTruthy();
                done();
            });
        });

        it('4. Get Device Telemetry', (done) => {
            var options: request.CoreOptions = {
                qs: {
                    deviceId: "SampleDevice001_648",
                    minTime: "2016-07-14T18:04:20.364-07:00"
                }
            }
            req.get('list', options, (err, resp, result: DeviceTelemetry[]) => {
                expect(result).toBeTruthy();
                expect(result.length).toBeGreaterThan(0);
                expect(result[0]).toBeTruthy();
                expect(result[0].deviceId).toBeTruthy();
                expect(result[0].timestamp).toBeTruthy();
                expect(result[0].values).toBeTruthy();
                expect(result[0].values.humidity).toBeTruthy();
                expect(result[0].values.temperature).toBeTruthy();
                done();
            });
        });

        it('5. Get Dashboard Device Pane Data', (done) => {
            var options: request.CoreOptions = {
                qs: {
                    deviceId: "SampleDevice001_648"
                }
            }
            req.get('dashboardDevicePane', options, (err, resp, result: DevicePaneData) => {
                expect(result).toBeTruthy();
                expect(result.deviceId).toBeTruthy();
                expect(result.deviceTelemetryFields).toBeTruthy();
                expect(result.deviceTelemetryFields[0].displayName).toBeTruthy();
                expect(result.deviceTelemetryFields[0].name).toBeTruthy();
                expect(result.deviceTelemetryFields[0].type).toBeTruthy();
                expect(result.deviceTelemetryModels).toBeTruthy();
                expect(result.deviceTelemetryModels[0].deviceId).toBeTruthy();
                expect(result.deviceTelemetryModels[0].timestamp).toBeTruthy();
                expect(result.deviceTelemetryModels[0].values).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.averageHumidity).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.deviceId).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.maximumHumidity).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.minimumHumidity).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.timeFrameMinutes).toBeTruthy();
                expect(result.deviceTelemetrySummaryModel.timestamp).toBeTruthy();
                done();
            });
        });

        it('6. Get Map Api Key', (done) => {
            req.get('mapApiKey', (err, resp, result: DeviceLocationData) => {
                expect(err).not.toBeTruthy();
                expect(resp.status).toEqual(200);
                done();
            });
        }, 10000);
    });
});
