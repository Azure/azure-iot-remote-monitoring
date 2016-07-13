const request = require('request').defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devicerules' });


describe('device rules api', () => {
    //create a new device for use in tests
    beforeAll(function() {
        var req = require('request').defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devices' });
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

        req(options, (err, resp, result) => {
               console.log(result);
            });


    });



    describe('get all device rules', () => {
        it('should return list of devices', (done) => {
            request.get('', (err, resp, result) => {
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
            request.post('list', (err, resp, result) => {
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

        //create rule for testing
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
            request(options, (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data.entity).toBeTruthy();
                expect(result.data.entity.ruleId).toBeTruthy();
 +              expect(result.data.entity.enabledState).toBeDefined();
                expect(result.data.entity.status).toEqual(2);
 +              done();
            });
        });


         it('should return a unique rule', (done) => {
            request.get('/testDevice/testRule', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data).toBeTruthy();
 +              expect(result.data.ruleId).toBeTruthy();
 +              expect(result.data.enabledState).toBeDefined();

                done();
            });
        });

        
         it('should return list of available fields', (done) => {
            request.get('/testDevice/testRule/availableFields', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.availableDataFields).toBeTruthy();
 +              expect(result.data.availableRuleOutputs).toBeTruthy();

                done();
            });
        });



         it('should return list of rules for a device', (done) => {
            request.get('/testDevice', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.ruleId).toBeTruthy();
                expect(result.data.deviceID).toBeTruthy();
 +              expect(result.data.enabledState).toBeDefined();
                done();
            });
        });

         it('should return list of devices', (done) => {
            request.put('/testDevice/testRule/false', (err, resp, result) => {
                expect(result.status).toEqual(2)
                done();
            });
        });

          it('should return list of devices', (done) => {
            request.delete('/testDevice/testRule/', (err, resp, result) => {
                expect(result.status).toEqual(2);
                done();
            });
        });
    });
