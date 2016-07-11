describe('typescript test', () => {
    describe('get devices', () => {
        it('should return list of devices', (done) => {
            let a: boolean = true;
            let b: number = 3;
            let c: string = 'str';
            expect(a).toBeTruthy();
            expect(b).toEqual(3);
            expect(c).toBeTruthy('str');
            done();
        });

        it('should return list of devices', (done) => {
            let a = true;
            let b = 3;
            let c = 'str';
            expect(a).toBeTruthy();
            expect(b).toEqual(3);
            expect(c).toBeTruthy('str');
            done();
        });
    });
});