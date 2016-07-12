const request = require('request').defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });

var findEnabledDevice = function(devices:Devices) {
    var i;
    for (i = 0; i < devices.data.length; i++) {
        if (devices.data[i].DeviceProperties.HubEnabledState) {
            break;
        }
    }
    return devices.data[i];
}

var findDisabledDevice = function(devices:Devices) {
    var i;
    for (i = 0; i < devices.data.length; i++) {
        if (!devices.data[i].DeviceProperties.HubEnabledState) {
            break;
        }
    }
    return devices.data[i];
}

var findDeviceWithCommandHistory = function(devices:Devices) {
    var i;
    for (i = 0; i < devices.data.length; i++) {
        if (devices.data[i].CommandHistory.length>0) {
            break;
        }
    }
    return devices.data[i];
}

describe('devices api', () => {
    describe('get devices', () => {
        it('should return list of devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                done();
            });
        });

        it('should return device properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
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
            request.get('', (err, resp, result:Devices) => {
                expect(result.data[0].SystemProperties).toBeTruthy();
                expect(result.data[0].SystemProperties.ICCID).toBeDefined();
                done();
            });
        });

        it('should always have required properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
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

        it('should have required attributes for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findEnabledDevice(result);
                expect(device).toBeTruthy();
                expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
                expect(device.Telemetry).toBeTruthy();
                expect(device.Version).toBeTruthy();
                expect(device.ObjectType).toBeTruthy();
                expect(device.IoTHub).toBeTruthy();
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findDisabledDevice(result);
                expect(device).toBeTruthy();
                expect(device.Commands).toBeTruthy();
                expect(device.Commands.length).toEqual(0);
                expect(device.DeviceProperties.HubEnabledState).toBeNull();
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findEnabledDevice(result);
                expect(device).toBeTruthy();
                expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
                expect(device.Commands).toBeTruthy();
                expect(device.Commands.length).toBeGreaterThan(0);
                expect(device.Commands[0]).toBeTruthy();
                expect(device.Commands[0].Name).toBeTruthy();
                expect(device.Commands[0].Parameters).toBeDefined();
                done();
            });
        });

        it('should return command history', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findDeviceWithCommandHistory(result);
                expect(device).toBeTruthy();
                expect(device.CommandHistory).toBeTruthy();
                expect(device.CommandHistory.length).toBeGreaterThan(0);
                expect(device.CommandHistory[0]).toBeTruthy();
                expect(device.CommandHistory[0].Name).toBeTruthy();
                expect(device.CommandHistory[0].MessageId).toBeTruthy();
                expect(device.CommandHistory[0].CreatedTime).toBeTruthy();
                expect(device.CommandHistory[0].Parameters).toBeTruthy();
                expect(device.CommandHistory[0].UpdatedTime).toBeTruthy();
                expect(device.CommandHistory[0].Result).toBeTruthy();
                expect(device.CommandHistory[0].ErrorMessage).toBeDefined();
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findEnabledDevice(result);
                expect(device).toBeTruthy();
                expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
                expect(device.Telemetry).toBeTruthy();
                expect(device.Telemetry.length).toBeGreaterThan(0);
                expect(device.Telemetry[0].Name).toBeTruthy();
                expect(device.Telemetry[0].DisplayName).toBeTruthy();
                expect(device.Telemetry[0].Type).toBeTruthy();
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                let device:DeviceInfo = findEnabledDevice(result);
                expect(device).toBeTruthy();
                expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
                expect(device.IoTHub).toBeTruthy();
                expect(device.IoTHub.MessageId).toBeDefined();
                expect(device.IoTHub.CorrelationId).toBeDefined();
                expect(device.IoTHub.ConnectionDeviceId).toBeTruthy();
                expect(device.IoTHub.ConnectionDeviceGenerationId).toBeTruthy();
                expect(device.IoTHub.EnqueuedTime).toBeTruthy();
                expect(device.IoTHub.StreamId).toBeDefined();
                done();
            });
        });
    });
});