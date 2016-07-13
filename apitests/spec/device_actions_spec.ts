import request = require('request');

describe('devices api - ', () => {
    describe('device actions - ', () => {
        var req: request.RequestAPI<request.Request, request.CoreOptions, Object>;
        beforeAll(function (done) {
            req = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/actions' });
            done();
        });

        it('1. GetDeviceActions with HttpGet and update the first action', (done) => {
            req.get('', (err, resp, result: DeviceActions) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data[0]).toBeTruthy();
                expect(result.data[0].actionId).toBeTruthy();
                expect(result.data[0].numberOfDevices).toBeTruthy();
                expect(result.data[0].ruleOutput).toBeTruthy();
                var action: Action = result.data[0];

                var options: request.CoreOptions = {
                    qs: {
                        ruleOutput: action.ruleOutput,
                        actionId: action.actionId
                    }
                }
                req.put('/update', options, (err, resp, result) => {
                    expect(err).toBeNull();
                    done();
                });
            });
        });

        it('2. GetDeviceActions with HttpPost', (done) => {
            req.post('/list', (err, resp, result: DeviceActionsWithCount) => {
                expect(result).toBeTruthy();
                expect(result.draw).not.toBeNull();
                expect(result.recordsFiltered).toBeTruthy();
                expect(result.recordsTotal).toBeTruthy();
                expect(result.recordsTotal).toEqual(result.recordsFiltered);

                expect(result.data).toBeTruthy();
                expect(result.data[0]).toBeTruthy();
                expect(result.data[0].actionId).toBeTruthy();
                expect(result.data[0].numberOfDevices).toBeTruthy();
                expect(result.data[0].ruleOutput).toBeTruthy();
                expect(result.data.length).toEqual(result.recordsTotal);
                done();
            });
        });

        it('3. GetAvailableRuleOutputs and then GetActionIdFromRuleOutput', (done) => {
            req.get('/ruleoutputs', (err, resp, result: Rules) => {
                expect(result).toBeTruthy();
                expect(result.data).toBeTruthy();
                expect(result.data[0]).toBeTruthy();
                expect(result.data[1]).toBeTruthy();
                expect(result.data).toContain('AlarmTemp');
                expect(result.data).toContain('AlarmHumidity');

                req.get('/ruleoutputs/AlarmTemp', (err, resp, result: ActionId) => {
                    expect(result).toBeTruthy();
                    expect(result.data).toBeTruthy();
                    expect(result.data).toEqual('Send Message');
                    done();
                });
            });
        });
    });
});



