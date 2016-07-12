var gulp = require('gulp');
var del = require('del');
var tsd = require("gulp-tsd");
var runSequence = require('run-sequence');
var ts = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmine = require('gulp-jasmine');
var gulp = require('gulp-help')(require('gulp'));
var tsconfig = require('./tsconfig.json');

gulp.task('build:clean', 'Deletes the dist folder', (done) => {
    return del('dist', done);
});

gulp.task('build:tsd', 'Installs typings', (done) => {
    return tsd({
        command: 'reinstall',
        config: 'tsd.json'
    }, done);
});

gulp.task('build:ts', 'Transpiles typescript to javascript', (done) => {
    var compilerOptions = tsconfig.compilerOptions;
    return gulp.src(['spec/**/*.ts', 'typings/**/*.ts'])
        .pipe(sourcemaps.init())
        .pipe(ts(compilerOptions))
        .js
        .pipe(sourcemaps.write('maps', { includeContent: false }))
        .pipe(gulp.dest('dist'));
});

gulp.task('build', 'Runs build:clean, tsd and ts in sequence', function (callback) {
    runSequence('build:clean',
        'build:tsd',
        'build:ts',
        callback);
});

gulp.task('test-jasmine', 'Starts jasmine to run tests', (done) => {
    return gulp.src(['dist/**/*.js'])
        .pipe(jasmine())
});

gulp.task('watch', 'Watches ts files and reruns the tests when they\'re changed', ['test-jasmine'], () => {
    gulp.watch('spec/**/*.ts', ['test']);
});

gulp.task('test', 'Runs build and test-jasmine in sequence', function (callback) {
    runSequence('build', 'test-jasmine', callback);
});

gulp.task('test-watch', 'Runs build and test-watch in sequence', function (callback) {
    runSequence('build', 'watch', callback);
});

gulp.task('default', ['help']);