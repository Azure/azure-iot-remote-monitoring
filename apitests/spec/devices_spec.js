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
    });
});