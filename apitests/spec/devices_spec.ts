import req = require('request');
import uuid = require('node-uuid');

function newDevice(deviceId: string): DeviceInfo {
    return <any>{
        deviceProperties: {
            deviceID: deviceId
        },
        systemProperties: {
            iccid: null
        },
        isSimulatedDevice: false
    };
}

function findDeviceWithDeviceProperties(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device 
            && device.deviceProperties
            && device.deviceProperties.deviceID
            && device.deviceProperties.createdTime
            && device.deviceProperties.deviceState) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'DeviceProperties'}`)
}

function createDeviceWithDeviceProperties(): DeviceInfo {
    // return {};
}

function checkDeviceWithDevicePropertiesExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithDeviceProperties(devices.data, done);
    // redundant but for safety sake
    checkDeviceProperties(device);
    done();
}

function checkDeviceProperties(device:DeviceInfo) {
    expect(device).toBeTruthy();

    // make sure device exists so tests dont blow up
    if (device) {
        expect(device.deviceProperties).toBeTruthy();
        expect(device.deviceProperties.deviceID).toBeTruthy();
        expect(device.deviceProperties.createdTime).toBeTruthy();
        expect(device.deviceProperties.deviceState).toBeTruthy();    
    }
}


function findDeviceWithSystemProperties(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device && device.systemProperties) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'SystemProperties'}`)
}

function createDeviceWithSystemProperties(): DeviceInfo {
    // return {};
}

function checkDeviceWithSystemPropertiesExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithSystemProperties(devices.data, done);
    // redundant but for safety sake
    checkSystemProperties(device);
    done();
}

function checkSystemProperties(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.systemProperties).toBeTruthy();        
    }
}


function findDeviceWithRequiredProperties(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device
            && device.deviceProperties
            && device.systemProperties
            && device.commands
            && device.commandHistory
            && device.id
            && device._rid
            && device._self
            && device._etag
            && device._ts
            && device._attachments) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'RequiredProperties'}`)
}

function createDeviceWithRequiredProperties(): DeviceInfo {
    // return {};
}

function checkDeviceWithRequiredPropertiesExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithRequiredProperties(devices.data, done);
    // redundant but for safety sake
    checkRequiredProperties(device);
    done();
}

function checkRequiredProperties(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device){
        expect(device.deviceProperties).toBeTruthy();
        expect(device.systemProperties).toBeTruthy();
        expect(device.commands).toBeTruthy();
        expect(device.commandHistory).toBeTruthy();
        expect(device.id).toBeTruthy();
        expect(device._rid).toBeTruthy();
        expect(device._self).toBeTruthy();
        expect(device._etag).toBeTruthy();
        expect(device._ts).toBeTruthy();
        expect(device._attachments).toBeTruthy();
    }
}


function findDeviceWithRequiredPropertiesEnabled(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device
            && (device.deviceProperties.hubEnabledState != null)
            && device.telemetry
            && device.version
            && device.objectType
            && device.ioTHub) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'RequiredPropertiesEnabled'}`)
}

function createDeviceWithRequiredPropertiesEnabled(): DeviceInfo {
    // return {};
}

function checkDeviceWithRequiredPropertiesEnabledExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithRequiredPropertiesEnabled(devices.data, done);
    // redundant but for safety sake
    checkRequiredPropertiesEnabledDevice(device);
    done();
}

function checkRequiredPropertiesEnabledDevice(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.deviceProperties.hubEnabledState).not.toBeNull();
        expect(device.telemetry).toBeTruthy();
        expect(device.version).toBeTruthy();
        expect(device.objectType).toBeTruthy();
        expect(device.ioTHub).toBeTruthy();
    }
}


function findDeviceWithNoCommandsDisabled(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];
        if(device
           && device.commands
           && (device.commands.length == 0)
           && (device.deviceProperties.hubEnabledState == null)) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'NoCommandsDisabled'}`)
}

function createDeviceWithNoCommandsDisabled(): DeviceInfo {
    // return {};
}

function checkDeviceWithNoCommandsDisabledDeviceExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithNoCommandsDisabled(devices.data, done);
    // redundant but for safety sake
    checkNoCommandsDisabledDevice(device);
    done();
}

function checkNoCommandsDisabledDevice(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.commands).toBeTruthy();
        expect(device.commands.length).toEqual(0);
        expect(device.deviceProperties.hubEnabledState).toBeNull();
    }
}


function findDeviceWithCommandsEnabled(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device
            && (device.deviceProperties.hubEnabledState != null)
            && device.commands
            && (device.commands.length > 0)
            && device.commands[0]
            && device.commands[0].name) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'CommandsEnabled'}`)
}

function createDeviceWithCommandsEnabled(): DeviceInfo {
    // return {};
}

function checkDeviceWithCommandsEnabledExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithCommandsEnabled(devices.data, done);
    // redundant but for safety sake
    checkCommandsEnabledDevice(device);
    done();
}

function checkCommandsEnabledDevice(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.deviceProperties.hubEnabledState).not.toBeNull();
        expect(device.commands).toBeTruthy();
        expect(device.commands.length).toBeGreaterThan(0);
        expect(device.commands[0]).toBeTruthy();
        expect(device.commands[0].name).toBeTruthy();
    }
}


function findDeviceWithCommandHistory(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device
            && device.commandHistory
            && (device.commandHistory.length > 0)) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'CommandHistory'}`)
}

function createDeviceWithCommandHistory(): DeviceInfo {
    // return {};
}

function checkDeviceWithCommandHistoryExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithCommandHistory(devices.data, done);
    // redundant but for safety sake
    checkCommandHistory(device);
    done();
}

function checkCommandHistory(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.commandHistory).toBeTruthy();
        expect(device.commandHistory.length).toBeGreaterThan(0);
        // Not testing command history
        // expect(device.commandHistory[0]).toBeTruthy();
        // expect(device.commandHistory[0].name).toBeTruthy();
        // expect(device.commandHistory[0].messageId).toBeTruthy();
        // expect(device.commandHistory[0].createdTime).toBeTruthy();
        // expect(device.commandHistory[0].parameters).toBeTruthy();
        // expect(device.commandHistory[0].updatedTime).toBeTruthy();
        // expect(device.commandHistory[0].result).toBeTruthy();
    }
}


function findDeviceWithTelemetry(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];

        if(device
           && (device.deviceProperties.hubEnabledState != null)
           && device.telemetry
           && (device.telemetry.length > 0)
           && device.telemetry[0].name
           && device.telemetry[0].displayName
           && device.telemetry[0].type) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'Telemetry'}`)
}

function createDeviceWithTelemetry(): DeviceInfo {
    // return {};
}

function checkDeviceWithTelemetryExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithTelemetry(devices.data, done);
    // redundant but for safety sake
    checkTelemetry(device);
    done();
}

function checkTelemetry(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.deviceProperties.hubEnabledState).not.toBeNull();
        expect(device.telemetry).toBeTruthy();
        expect(device.telemetry.length).toBeGreaterThan(0);
        expect(device.telemetry[0].name).toBeTruthy();
        expect(device.telemetry[0].displayName).toBeTruthy();
        expect(device.telemetry[0].type).toBeTruthy();
    }
}

function findDeviceWithIoTHubDetailsEnabled(devices: DeviceInfo[], done: DoneFunction): DeviceInfo {
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];
        if(device
           && (device.deviceProperties.hubEnabledState != null)
           && device.ioTHub
           && device.ioTHub.connectionDeviceId
           && device.ioTHub.connectionDeviceGenerationId
           && device.ioTHub.enqueuedTime) {
            return device;
        } 
    }

    done.fail(`Could not find a device with ${'IoTHubDetailsEnabled'}`)
}

function createDeviceWithIoTHubDetailsEnabled(): DeviceInfo {
    // return {};
}

function checkDeviceWithIoTHubDetailsEnabledExists(devices: Devices, done: DoneFunction) {
    let device = findDeviceWithIoTHubDetailsEnabled(devices.data, done);
    // redundant but for safety sake
    checkIoTHubDetailsEnabledDevice(device);
    done();
}

function checkIoTHubDetailsEnabledDevice(device:DeviceInfo) {
    expect(device).toBeTruthy();

    if (device) {
        expect(device.deviceProperties.hubEnabledState).not.toBeNull();
        expect(device.ioTHub).toBeTruthy();
        expect(device.ioTHub.connectionDeviceId).toBeTruthy();
        expect(device.ioTHub.connectionDeviceGenerationId).toBeTruthy();
        expect(device.ioTHub.enqueuedTime).toBeTruthy();
    }
}

let testDevices: DeviceInfo[] = [];

function populateDB(done: DoneFunction) {
    testDevices = [
        createDeviceWithDeviceProperties(),
        createDeviceWithSystemProperties(),
        createDeviceWithRequiredProperties(),
        createDeviceWithRequiredPropertiesEnabled(),
        createDeviceWithNoCommandsDisabled(),
        createDeviceWithCommandsEnabled(),
        createDeviceWithCommandHistory(),
        createDeviceWithTelemetry(),
        createDeviceWithIoTHubDetailsEnabled()
    ];

    let onDone = sync(testDevices.length, done);

    testDevices.forEach(device => addDevice(device, onDone, done.fail));
}

function drainDB(devices: DeviceInfo[], done: DoneFunction) {
    let onDone = sync(devices.length, done);

    devices.forEach(device => removeDevice(device.deviceProperties.deviceID, onDone, done.fail));
}

function addDevice(device: DeviceInfo, succeed: Function, fail: Function) {
    let options = {
        uri: 'https://localhost:44305/api/v1/devices',
        method: 'POST',
        json: device
    };

   req(options, (err, resp, result) => {
        if (err) {
            fail();
        } else {
            succeed();
        }
    });
}

function removeDevice(deviceId: string, succeed: Function, fail: Function) {
    let options = {
        uri: `https://localhost:44305/api/v1/devices/${deviceId}`,
        method: 'DELETE'
    };

   req(options, (err, resp, result) => {
        if (err) {
            fail();
        } else {
            succeed();
        }
    });
}

function sync(numEvents: number, cb: Function) {
    let finishedEvents = 0;

    return function() {
        finishedEvents++;
        if (finishedEvents == numEvents) {
            cb();
        }
    };
}

describe('devices api', () => {
    var request: req.RequestAPI<req.Request, req.CoreOptions, Object>;

    beforeAll(function(done) {
        // get all the devices in the system
        request.get('', (err, resp, result:Devices) => {
            if (err) done.fail(err);

            // remove them all
            drainDB(result.data, done);

            // setup request for tests
            request = req.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });
            
            // add testDevices to DB
            populateDB(done);
        });
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
                checkDeviceWithDevicePropertiesExists(result, done);
            });
        });

        it('should return system properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithSystemPropertiesExists(result, done);
            });
        });

        it('should always have required properties', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithRequiredPropertiesExists(result, done);
            });
        });

        it('should have required attributes for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithRequiredPropertiesEnabledExists(result, done);
            });
        });

        it('should not return commands if device is disabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithNoCommandsDisabledDeviceExists(result, done);
            });
        });

        it('should return commands if device is enabled', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithCommandsEnabledExists(result, done);
            });
        });

        it('should return command history', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithCommandHistoryExists(result, done);
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithTelemetryExists(result, done);
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            request.get('', (err, resp, result:Devices) => {
                checkDeviceWithIoTHubDetailsEnabledExists(result, done);
            });
        });
    });

    describe('get device by id', () => {

        it('should return a device', (done) => {
            let deviceId = testDevices[0].deviceProperties.deviceID;

            request.get(`/${deviceId}`, (err, resp, result:SingleDevice) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                done();
            });
        });

        // Testing the return payload here as well because get_device_by_id may take different code
        // path than get_all_devices

        it('should return device properties', (done) => {
            let deviceId = findDeviceWithDeviceProperties(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkDeviceProperties(result.data);
                done();
            });
        });

        it('should return system properties', (done) => {
            let deviceId = findDeviceWithSystemProperties(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkSystemProperties(result.data);
                done();
            });
        });

        it('should always have required properties', (done) => {
            let deviceId = findDeviceWithRequiredProperties(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkRequiredProperties(result.data);
                done();
            });
        });

        it('should have required attributes for enabled devices', (done) => {
            let deviceId = findDeviceWithRequiredPropertiesEnabled(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkRequiredPropertiesEnabledDevice(result.data);
                done();
            });
        });

        it('should not return commands if device is disabled', (done) => {
            let deviceId = findDeviceWithNoCommandsDisabled(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result:SingleDevice) => {
                checkNoCommandsDisabledDevice(result.data);
                done();
            });
        });

        it('should return commands if device is enabled', (done) => {
            let deviceId = findDeviceWithCommandsEnabled(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkCommandsEnabledDevice(result.data);
                done();
            });
        });

        it('should return command history', (done) => {
            let deviceId = findDeviceWithCommandHistory(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result:SingleDevice) => {
                checkCommandHistory(result.data);
                done();
            });
        });

        it('should return telemetry for enabled devices', (done) => {
            let deviceId = findDeviceWithTelemetry(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkTelemetry(result.data);
                done();
            });
        });
        
        it('should return IoT Hub details for enabled devices', (done) => {
            let deviceId = findDeviceWithIoTHubDetailsEnabled(testDevices, done);

            request.get(`/${deviceId}`, (err, resp, result: SingleDevice) => {
                checkIoTHubDetailsEnabledDevice(result.data);
                done();
            });
        });
    });

    describe('create new device', () => {
        var newDeviceId: string;

        it('should create new device', (done) => {
            newDeviceId = "C2C-TEST-" + uuid.v4();
            addDevice(newDevice(newDeviceId), done, done.fail);
        });
    
        afterAll(function(done) {
            // Delete created device
            removeDevice(newDeviceId, done, done.fail);
        });
    });

    describe('update device status', () => {
        var newDeviceId: string;
        
        beforeAll(function (done) {
            // Create new device
            newDeviceId = "C2C-TEST-" + uuid.v4();
            addDevice(newDevice(newDeviceId), done, done.fail);
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

        afterAll(function(done) {
            // Delete created device
            removeDevice(newDeviceId, done, done.fail);
        });
    });

    describe('get hub keys', () => {
        var newDeviceId: string;
        
        beforeAll(function (done) {
            // Get the first device
            // request.get('', (err, resp, result: Devices) => {
            //     expect(result.data.length).toBeGreaterThan(0);
            //     newDeviceId = result.data[0].deviceProperties.deviceID;
            //     done();
            // });
            newDeviceId = "C2C-TEST-" + uuid.v4();
            addDevice(newDevice(newDeviceId), done, done.fail);
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

        afterAll(function(done) {
            // Delete created device
            removeDevice(newDeviceId, done, done.fail);
        });
    });

    describe('delete device by id', () => {
        var newDeviceId: string;

        beforeAll(function (done) {
            newDeviceId = "C2C-TEST-" + uuid.v4();
            addDevice(newDevice(newDeviceId), done, done.fail);
        });

        it('should delete device id', (done) => {
            removeDevice(newDeviceId, done, done.fail);
        });
    });

    describe('send command', () => {

        let newDeviceId:string;
        let command_name:string; 

        beforeAll(function(done) {
            // Get a device and a command
            let device: DeviceInfo = findDeviceWithCommandsEnabled(testDevices, done);
            
            newDeviceId = device.deviceProperties.deviceID;
            command_name = device.commands[0].name;
            
            done();
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

    afterAll((done) => {
        drainDB(testDevices, done);
    });
});