const request = require('request').defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/devicerules' });


describe('device rules api', () => {
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
        
        //create a new rule for other tests
          it('should create new rule', (done) => {
            var options = {
                uri: '',
                method: 'POST',
                json: {
                    "RuleId": "test123",
                    "EnabledState": 0,
                    "DeviceID": "SampleDevice001_591",
                    "DataField": "tremor",
                    "Operator": ">",
                    "Threshold": 1.2,
                    "RuleOutput": "alsert",
                    "Etag": ""
                }

            }
            request(options, (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data.entity).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data.entity.ruleId).toBeTruthy();
 +              expect(result.data.entity.enabledState).toBeDefined();
                expect(result.data.entity.status).toEqual(2);
 +         
                done();
            });
        });


         it('should return a unique rule', (done) => {
            request.post('/SampleDevice001_591/test123', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data).toBeTruthy();
 +              expect(result.data.ruleId).toBeTruthy();
 +              expect(result.data.enabledState).toBeDefined();

                done();
            });
        });

        
         it('should return list of available fields', (done) => {
            request.post('/SampleDevice001_591/test123/availableFields', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data.length).toBeGreaterThan(0);
                expect(result.data.availableDataFields).toBeTruthy();
 +              expect(result.data.availableRuleOutputs).toBeTruthy();

                done();
            });
        });



         it('should return list of rules for a device', (done) => {
            request.post('/SampleDevice001_591', (err, resp, result) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data[0].ruleId).toBeTruthy();
                expect(result.data[0].deviceID).toBeTruthy();
 +              expect(result.data[0].enabledState).toBeDefined();

                done();
            });
        });

         it('should return list of devices', (done) => {
            request.put('/SampleDevice001_591/test123/false', (err, resp, result) => {
                expect(result.status).toEqual(2)

                done();
            });
        });

          it('should return list of devices', (done) => {
            request.delete('/SampleDevice001_591/test123/', (err, resp, result) => {
                expect(result.status).toEqual(2)

                done();
            });
        });






});





