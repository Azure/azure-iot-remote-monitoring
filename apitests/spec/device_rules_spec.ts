import request = require('request');
import uuid = require('node-uuid');

xdescribe('device rules api', () => {
    //create a new device for use in tests
    var req: request.RequestAPI<request.Request, request.CoreOptions, Object>;
    beforeAll(function( done) {
        req = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1' });
        var options = {
            uri: '/devices',
            method: 'POST',
            json: {
                "DeviceProperties": {
                    "DeviceID": "testDevice"
                },
                "SystemProperties": {
                    "ICCID": null
                },
                "IsSimulatedDevice": false
            }
        }

       req(options, (err, resp, result) => {
            done();
        });
    });

    afterAll((done) => {
        req.del('/devices/testDevice', (err, resp, result) => {
            done();
        });
    })


    describe('get all device rules', () => {
        it('should return list of devices', (done) => {
            req.get('/devicerules', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data[0]).toBeTruthy();
                expect(result.data[0].ruleId).toBeTruthy();
                expect(result.data[0].enabledState).toBeDefined();
                done();
            });
        });

        //POST list api is not using requestData
        it('should return list of device rules', (done) => {
            req.post('/devicerules/list', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data[0]).toBeTruthy();
                expect(result.data[0].ruleId).toBeTruthy();
                expect(result.data[0].enabledState).toBeDefined();
                expect(result.draw).toBeDefined();
                expect(result.recordsTotal).toBeDefined();
                expect(result.recordsFiltered).toBeDefined();
                done();
            });
        });
    });

    describe('create new device rule', () => {
        it('should create new rule', (done) => {
            var data:string = "tremor" + uuid.v4();
            var options = {
                uri: '/devicerules',
                method: 'POST',
                json: {
                    "RuleId": "testRule",
                    "EnabledState": 0,
                    "DeviceID": "testDevice",
                    "DataField": data,
                    "Operator": ">",
                    "Threshold": 1.2,
                    "RuleOutput": "alert",
                    "Etag": ""
                }
            }
            req(options, (err, resp, result) => {
                expect(result).toBeTruthy();
                console.log(result);
                expect(result.data.entity).toBeTruthy();
                expect(result.data.entity.ruleId).toBeTruthy();
                expect(result.data.entity.enabledState).toBeDefined();
                expect(result.data.entity.status).toEqual(2);
                done();
            });
        });
    });

    describe('return information on a unique rule', () => {
        it('should return a unique rule', (done) => {
            req.get('/devicerules/testDevice/testRule', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.ruleId).toBeTruthy();
                expect(result.data.enabledState).toBeDefined();

                done();
            });
        });
    });

    describe('list available data fields', () => {
        it('should return list of available fields', (done) => {
            req.get('/devicerules/testDevice/testRule/availableFields', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.availableDataFields).toBeTruthy();
                expect(result.data.availableRuleOutputs).toBeTruthy();

                done();
            });
        });
    });


    describe('all rules tied to a device', () => {
        it('should return list of rules for a device', (done) => {
            req.get('/devicerules/testDevice', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.ruleId).toBeTruthy();
                expect(result.data.deviceID).toBeTruthy();
                expect(result.data.enabledState).toBeDefined();
                done();
            });
        });
    });

    describe('change enabled state of a device', () => {
        it('should change enabled state to false', (done) => {
            req.put('/devicerules/testDevice/testRule/false', (err, resp, result) => {
                expect(result.status).toEqual(2)
                done();
            });
        });
    });

    describe('create new device rule', () => {
          it('should return list of devices', (done) => {
          req.del('/devicerules/testDevice/testRule', (err, resp, result) => {
                expect(result.status).toEqual(2);
                done();
            });
        });
    });
});
