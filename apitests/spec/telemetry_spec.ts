import * as request from 'request';

describe('telemetry api', () => {
    let req: request.RequestAPI<request.Request, request.CoreOptions, Object>;

    beforeAll(function (done) {
        req = request.defaults({ json: true, baseUrl: 'https://localhost:44305/api/v1/telemetry' });
        done();
    });

    it('1. GetDashboardDevicePaneDataAsync', (done) => {
        req.get('/dashboardDevicePane', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });

    it('2. GetDeviceTelemetryAsync', (done) => {
        req.get('/list', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });


    it('3. GetDeviceTelemetrySummaryAsync', (done) => {
        req.get('/summary', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });

    it('4. GetLatestAlertHistoryAsync', (done) => {
        req.get('/alertHistory', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });

    it('5. GetDeviceLocationData', (done) => {
        req.get('/deviceLocationData', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });

    it('6. GetMapApiKey', (done) => {
        req.get('/mapApiKey', (err, resp, result: any) => {
            expect(result).toBeTruthy();
            done();
        });
    });        
});