const request = require('request').defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });

describe('devices api', () => {
    describe('get devices', () => {
        it('should return list of devices', (done) => {
            request.get('', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                done();
            });
        });

        it('should return device properties', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[0].DeviceProperties).toBeTruthy();
                expect(result.data[0].DeviceProperties.DeviceID).toBeTruthy();
                expect(result.data[0].DeviceProperties.HubEnabledState).toBeDefined();
                expect(result.data[0].DeviceProperties.CreatedTime).toBeTruthy();
                expect(result.data[0].DeviceProperties.DeviceState).toBeTruthy();
                expect(result.data[0].DeviceProperties.UpdatedTime).toBeDefined();
                done();
            });
        });

        it('should return system properties', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[0].SystemProperties).toBeTruthy();
                expect(result.data[0].SystemProperties.ICCID).toBeDefined();
                done();
            });
        });

        it('should always have these properties', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[0].DeviceProperties).toBeTruthy();
                expect(result.data[0].SystemProperties).toBeTruthy();
                expect(result.data[0].Commands).toBeTruthy();
                expect(result.data[0].CommandHistory).toBeTruthy();
                expect(result.data[0].IsSimulatedDevice).toBeDefined();
                expect(result.data[0].id).toBeTruthy();
                expect(result.data[0]._rid).toBeTruthy();
                expect(result.data[0]._self).toBeTruthy();
                expect(result.data[0]._etag).toBeTruthy();
                expect(result.data[0]._ts).toBeTruthy();
                expect(result.data[0]._attachments).toBeTruthy();
                done();
            });
        })

        it('should have these attributes for enabled device', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[1].DeviceProperties.HubEnabledState).not.toBeNull();
                expect(result.data[1].Telemetry).toBeTruthy();
                expect(result.data[1].Version).toBeTruthy();
                expect(result.data[1].ObjectType).toBeTruthy();
                expect(result.data[1].IoTHub).toBeTruthy();
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[0].DeviceProperties.HubEnabledState).toBeNull();
                expect(result.data[0].Commands).toBeTruthy();
                expect(result.data[0].Commands.length).toEqual(0);
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[1].DeviceProperties.HubEnabledState).not.toBeNull();
                expect(result.data[1].Commands).toBeTruthy();
                expect(result.data[1].Commands.length).toBeGreaterThan(0);
                expect(result.data[1].Commands[0]).toBeTruthy();
                expect(result.data[1].Commands[0].Name).toBeTruthy();
                expect(result.data[1].Commands[0].Parameters).toBeDefined();
                done();
            });
        });

        it('should return command history', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[4].CommandHistory).toBeTruthy();
                expect(result.data[4].CommandHistory.length).toBeGreaterThan(0);
                expect(result.data[4].CommandHistory[0]).toBeTruthy();
                expect(result.data[4].CommandHistory[0].Name).toBeTruthy();
                expect(result.data[4].CommandHistory[0].MessageId).toBeTruthy();
                expect(result.data[4].CommandHistory[0].CreatedTime).toBeTruthy();
                expect(result.data[4].CommandHistory[0].Parameters).toBeTruthy();
                expect(result.data[4].CommandHistory[0].UpdatedTime).toBeTruthy();
                expect(result.data[4].CommandHistory[0].Result).toBeTruthy();
                expect(result.data[4].CommandHistory[0].ErrorMessage).toBeDefined();
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[1].DeviceProperties.HubEnabledState).not.toBeNull();
                expect(result.data[1].Telemetry).toBeTruthy();
                expect(result.data[1].Telemetry.length).toBeGreaterThan(0);
                expect(result.data[1].Telemetry[0].Name).toBeTruthy();
                expect(result.data[1].Telemetry[0].DisplayName).toBeTruthy();
                expect(result.data[1].Telemetry[0].Type).toBeTruthy();
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('', (err, resp, result) => {
                expect(result.data[1].DeviceProperties.HubEnabledState).not.toBeNull();
                expect(result.data[1].IoTHub).toBeTruthy();
                expect(result.data[1].IoTHub.MessageId).toBeDefined();
                expect(result.data[1].IoTHub.CorrelationId).toBeDefined();
                expect(result.data[1].IoTHub.ConnectionDeviceId).toBeTruthy();
                expect(result.data[1].IoTHub.ConnectionDeviceGenerationId).toBeTruthy();
                expect(result.data[1].IoTHub.EnqueuedTime).toBeTruthy();
                expect(result.data[1].IoTHub.StreamId).toBeDefined();
                done();
            });
        });
    });
});