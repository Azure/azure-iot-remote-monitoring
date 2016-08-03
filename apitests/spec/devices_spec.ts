import req = require('request');
import uuid = require('node-uuid');

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

var findDeviceWithCommands = function(devices:Devices) {
    var i;
    for (i = 0; i < devices.data.length; i++) {
        if (devices.data[i].Commands.length>0) {
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

var getNewCustomDeviceOptions = function (deviceId: string) {
    var options = {
        uri: '',
        method: 'POST',
        json: {
            "DeviceProperties": {
                "DeviceID": deviceId
            },
            "SystemProperties": {
                "ICCID": null
            },
            "IsSimulatedDevice": false
        }
    }
    return options;
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
    // Not testing command history
    // expect(device.CommandHistory[0]).toBeTruthy();
    // expect(device.CommandHistory[0].Name).toBeTruthy();
    // expect(device.CommandHistory[0].MessageId).toBeTruthy();
    // expect(device.CommandHistory[0].CreatedTime).toBeTruthy();
    // expect(device.CommandHistory[0].Parameters).toBeTruthy();
    // expect(device.CommandHistory[0].UpdatedTime).toBeTruthy();
    // expect(device.CommandHistory[0].Result).toBeTruthy();
    // expect(device.CommandHistory[0].ErrorMessage).toBeDefined();
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
        });

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

        let enabled_device_id: string;
        let disabled_device_id: string;
        let device_with_cmd_history: string;

        beforeAll(function (done) {
            // Request for all devices to find enabled, disabled and device with command history
            request.get('', (err, resp, result: Devices) => {
                enabled_device_id = findEnabledDevice(result).DeviceProperties.DeviceID;
                disabled_device_id = findDisabledDevice(result).DeviceProperties.DeviceID;
                device_with_cmd_history = findDeviceWithCommandHistory(result).DeviceProperties.DeviceID;
                done();
            });
        });

        it('should return a device', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                done();
            });
        });

        // Testing the return payload here as well because get_device_by_id may take different code
        // path than get_all_devices

        it('should return device properties', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkDeviceProperties(result.data);
                done();
            });
        });

        it('should return system properties', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkSystemProperties(result.data);
                done();
            });
        });

        it('should always have required properties', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkRequiredProperties(result.data);
                done();
            });
        });

        it('should have required attributes for enabled devices', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkRequiredPropertiesEnabledDevice(result.data);
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('/'+disabled_device_id, (err, resp, result:SingleDevice) => {
                checkNoCommandsDisabledDevice(result.data);
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkCommandsEnabledDevice(result.data);
                done();
            });
        });

        it('should return command history', (done) => {
            request.get('/'+device_with_cmd_history, (err, resp, result:SingleDevice) => {
                checkCommandHistory(result.data);
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkTelemetry(result.data);
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('/'+enabled_device_id, (err, resp, result:SingleDevice) => {
                checkIoTHubDetailsEnabledDevice(result.data);
                done();
            });
        });
    });

    describe('create new device', () => {
        var newDeviceId: string;

        afterAll(function(done) {
            // Delete created device
            request({method: "DELETE", uri: "/" + newDeviceId}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not delete device " + newDeviceId);
                }
                done();
            });
        });

        it('should create new device', (done) => {
            newDeviceId = "C2C-TEST-" + uuid.v4();
            request(getNewCustomDeviceOptions(newDeviceId), (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not create device " + newDeviceId);
                }
                done();
            });
        });
    });

    describe('update device status', () => {
        var newDeviceId: string;
        
        beforeAll(function (done) {
            // Create new device
            newDeviceId = "C2C-TEST-" + uuid.v4();
            request(getNewCustomDeviceOptions(newDeviceId), (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not create device " + newDeviceId);
                }
                done();
            });
        });

        afterAll(function(done) {
            // Delete created device
            request({method: "DELETE", uri: "/" + newDeviceId}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not delete device " + newDeviceId);
                }
                done();
            });
        });

        it('should enable the device', (done) => {
            request.put({uri: "/" + newDeviceId + "/enabledstatus", json:{"isEnabled" : true}}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not enable device " + newDeviceId);
                }
                done();
            });
        });

        it('should disable the device', (done) => {
            request.put({uri: "/" + newDeviceId + "/enabledstatus", json:{"isEnabled" : false}}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not disable device " + newDeviceId);
                }
                done();
            });
        });
    });

    describe('get hub keys', () => {
        var newDeviceId: string;
        
        beforeAll(function (done) {
            // Get the first device
            // request.get('', (err, resp, result: Devices) => {
            //     expect(result.data.length).toBeGreaterThan(0);
            //     newDeviceId = result.data[0].DeviceProperties.DeviceID;
            //     done();
            // });
            newDeviceId = "C2C-TEST-" + uuid.v4();
            request(getNewCustomDeviceOptions(newDeviceId), (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not create device " + newDeviceId);
                }
                done();
            });
        });

        afterAll(function(done) {
            // Delete created device
            request({method: "DELETE", uri: "/" + newDeviceId}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not delete device " + newDeviceId);
                }
                done();
            });
        });

        it('should return Hub keys', (done) => {
            request.get('/' + newDeviceId + '/hub-keys', (err, resp, result) => {
                expect(result).toBeTruthy();
                let keys:HubKeys = result.data;
                expect(keys).toBeTruthy();
                expect(keys.primaryKey).toBeTruthy();
                expect(keys.secondaryKey).toBeTruthy();
                done();
            });
        });
    });

    describe('delete device by id', () => {
        var newDeviceId: string;

        beforeAll(function (done) {
            newDeviceId = "C2C-TEST-" + uuid.v4();
            request(getNewCustomDeviceOptions(newDeviceId), (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not create device " + newDeviceId);
                }
                done();
            });
        });

        it('should delete device id', (done) => {
            request({method: "DELETE", uri: "/" + newDeviceId}, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Could not delete device " + newDeviceId);
                }
                done();
            });
        });
    });

    describe('send command', () => {

        let newDeviceId:string;
        let command_name:string; 

        beforeAll(function(done) {
            // Get a device and a command
            request.get('', (err, resp, result: Devices) => {
                expect(result.data.length).toBeGreaterThan(0);
                let device:DeviceInfo = findDeviceWithCommands(result);
                newDeviceId = device.DeviceProperties.DeviceID;
                command_name = device.Commands[0].Name;
                done();
            });
        });

        it('should send command to device', (done) => {
            request.post("/" + newDeviceId + "/commands/" + command_name, (err, resp, result) => {
                if (err || resp.statusCode != 200) {
                    fail("Send " + command_name + " command failed for " + newDeviceId);
                }
                done();
            });
        });
    });

    describe('should delete all devices', () => {
        // Not implemented as it will affect others on the same Azure subscription
    });
});