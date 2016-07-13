import request = require('request');


describe('device rules api', () => {
    //create a new device for use in tests
    var req: request.RequestAPI<request.Request, request.CoreOptions, Object>;
    beforeAll(function() {
        req = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devicerules' });
        var createDevice = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });
        var options = {
            uri: '',
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

        createDevice(options, (err, resp, result) => {
              //console.log(result);
        });
    });


    describe('get all device rules', () => {
        it('should return list of devices', (done) => {
            req.get('', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data[0]).toBeTruthy();
 +              expect(result.data[0].ruleId).toBeTruthy();
 +              expect(result.data[0].enabledState).toBeDefined();
                done();
            });
        });

        //POST list api is not using requestData
        it('should return list of device rules', (done) => {
            req.post('list', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data[0]).toBeTruthy();
 +              expect(result.data[0].ruleId).toBeTruthy();
 +              expect(result.data[0].enabledState).toBeDefined();
                expect(result.draw).toBeDefined();
 +              expect(result.recordsTotal).toBeDefined();
 +              expect(result.recordsFiltered).toBeDefined();
                done();
            });
        });
    });

    describe('create new device rule', () => {
          it('should create new rule', (done) => {
            var options = {
                uri: '',
                method: 'POST',
                json: {
                    "RuleId": "testRule",
                    "EnabledState": 0,
                    "DeviceID": "testDevice",
                    "DataField": "tremor",
                    "Operator": ">",
                    "Threshold": 1.2,
                    "RuleOutput": "alert",
                    "Etag": ""
                }
            }
            req(options, (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data.entity).toBeTruthy();
                expect(result.data.entity.ruleId).toBeTruthy();
 +              expect(result.data.entity.enabledState).toBeDefined();
                expect(result.data.entity.status).toEqual(2);
 +              done();
            });
        });
    });

    describe('return information on a unique rule', () => {
         it('should return a unique rule', (done) => {
            req.get('/testDevice/testRule', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data).toBeTruthy();
 +              expect(result.data.ruleId).toBeTruthy();
 +              expect(result.data.enabledState).toBeDefined();

                done();
            });
        });
    });

    describe('list available data fields', () => {    
         it('should return list of available fields', (done) => {
            req.get('/testDevice/testRule/availableFields', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.availableDataFields).toBeTruthy();
 +              expect(result.data.availableRuleOutputs).toBeTruthy();

                done();
            });
        });
    });


    describe('all rules tied to a device', () => {
         it('should return list of rules for a device', (done) => {
            req.get('/testDevice', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.ruleId).toBeTruthy();
                expect(result.data.deviceID).toBeTruthy();
 +              expect(result.data.enabledState).toBeDefined();
                done();
            });
        });
    });
  
  describe('change enabled state of a device', () => {
         it('should change enabled state to false', (done) => {
            req.put('/testDevice/testRule/false', (err, resp, result) => {
                expect(result.status).toEqual(2)
                done();
            });
        });
  });

    describe('create new device rule', () => {
          it('should return list of devices', (done) => {
            req.del('/testDevice/testRule/', (err, resp, result) => {
                expect(result.status).toEqual(2);
                done();
            });
        });
    });
});
