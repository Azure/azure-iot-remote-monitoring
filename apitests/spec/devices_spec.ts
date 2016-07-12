import req = require('request');

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

var checkDeviceProperties = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties).toBeTruthy();
    expect(device.DeviceProperties.DeviceID).toBeTruthy();
    expect(device.DeviceProperties.HubEnabledState).toBeDefined();
    expect(device.DeviceProperties.CreatedTime).toBeTruthy();
    expect(device.DeviceProperties.DeviceState).toBeTruthy();
    expect(device.DeviceProperties.UpdatedTime).toBeDefined();
}

var checkSystemProperties = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.SystemProperties).toBeTruthy();
    expect(device.SystemProperties.ICCID).toBeDefined();
}

var checkRequiredProperties = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties).toBeTruthy();
	expect(device.SystemProperties).toBeTruthy();
    expect(device.Commands).toBeTruthy();
    expect(device.CommandHistory).toBeTruthy();
    expect(device.IsSimulatedDevice).toBeDefined();
    expect(device.id).toBeTruthy();
    expect(device._rid).toBeTruthy();
    expect(device._self).toBeTruthy();
    expect(device._etag).toBeTruthy();
    expect(device._ts).toBeTruthy();
    expect(device._attachments).toBeTruthy();
}

var checkRequiredPropertiesEnabledDevice = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
    expect(device.Telemetry).toBeTruthy();
    expect(device.Version).toBeTruthy();
    expect(device.ObjectType).toBeTruthy();
    expect(device.IoTHub).toBeTruthy();
}

var checkNoCommandsDisabledDevice = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.Commands).toBeTruthy();
    expect(device.Commands.length).toEqual(0);
    expect(device.DeviceProperties.HubEnabledState).toBeNull();            
}

var checkCommandsEnabledDevice = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
    expect(device.Commands).toBeTruthy();
    expect(device.Commands.length).toBeGreaterThan(0);
    expect(device.Commands[0]).toBeTruthy();
    expect(device.Commands[0].Name).toBeTruthy();
    expect(device.Commands[0].Parameters).toBeDefined();
}

var checkCommandHistory = function(device:DeviceInfo) {
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
}

var checkTelemetry = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
    expect(device.Telemetry).toBeTruthy();
    expect(device.Telemetry.length).toBeGreaterThan(0);
    expect(device.Telemetry[0].Name).toBeTruthy();
    expect(device.Telemetry[0].DisplayName).toBeTruthy();
    expect(device.Telemetry[0].Type).toBeTruthy();
}
var checkIoTHubDetailsEnabledDevice = function(device:DeviceInfo) {
    expect(device).toBeTruthy();
    expect(device.DeviceProperties.HubEnabledState).not.toBeNull();
    expect(device.IoTHub).toBeTruthy();
    expect(device.IoTHub.MessageId).toBeDefined();
    expect(device.IoTHub.CorrelationId).toBeDefined();
    expect(device.IoTHub.ConnectionDeviceId).toBeTruthy();
    expect(device.IoTHub.ConnectionDeviceGenerationId).toBeTruthy();
    expect(device.IoTHub.EnqueuedTime).toBeTruthy();
    expect(device.IoTHub.StreamId).toBeDefined();
}

describe('devices api', () => {
    var request: req.RequestAPI<req.Request, req.CoreOptions, Object>;

    beforeAll(function() {
        request = req.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });
    });

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
                checkDeviceProperties(result.data[0]);
                done();
            });
        });

        it('should return system properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkSystemProperties(result.data[0]);
                done();
            });
        });

        it('should always have required properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkRequiredProperties(result.data[0]);
                done();
            });
        })

        it('should have required attributes for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkRequiredPropertiesEnabledDevice(findEnabledDevice(result));
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkNoCommandsDisabledDevice(findDisabledDevice(result));
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkCommandsEnabledDevice(findEnabledDevice(result));
                done();
            });
        });

        it('should return command history', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkCommandHistory(findDeviceWithCommandHistory(result));
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkTelemetry(findEnabledDevice(result));
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkIoTHubDetailsEnabledDevice(findEnabledDevice(result));
                done();
            });
        });
    });

    describe('get device by id', () => {
        const enabled_device = "SampleDevice001_648";
        const disabled_device = "D2";

        it('should return a device', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                done();
            });
        });

        it('should return device properties', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkDeviceProperties(result.data);
                done();
            });
        });

        it('should return system properties', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkSystemProperties(result.data);
                done();
            });
        });

        it('should always have required properties', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkRequiredProperties(result.data);
                done();
            });
        })

        it('should have required attributes for enabled devices', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkRequiredPropertiesEnabledDevice(result.data);
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('/'+disabled_device, (err, resp, result:SingleDevice) => {
                checkNoCommandsDisabledDevice(result.data);
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkCommandsEnabledDevice(result.data);
                done();
            });
        });

        it('should return command history', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkCommandHistory(result.data);
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkTelemetry(result.data);
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('/'+enabled_device, (err, resp, result:SingleDevice) => {
                checkIoTHubDetailsEnabledDevice(result.data);
                done();
            });
        });
    });

    describe('get hub keys', () => {
        const device_name = "SampleDevice001_648";

        it('should return Hub keys', (done) => {
            request.get('/'+device_name+'/hub-keys', (err, resp, result) => {
                expect(result).toBeTruthy();
                let keys:HubKeys = result.data;
                expect(keys).toBeTruthy();
                expect(keys.primaryKey).toBeTruthy();
                expect(keys.secondaryKey).toBeTruthy();
                done();
            });
        });
    });
});