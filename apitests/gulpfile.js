var gulp = require('gulp');
var rimraf = require('rimraf');
var tsd = require("gulp-tsd");
var runSequence = require('run-sequence');
var ts = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmine = require('gulp-jasmine');
var tsconfig = require('./tsconfig.json');

gulp.task('build:clean', (done) => {
    return rimraf('dist', done);
});

gulp.task('build:tsd', (done) => {
    return tsd({
        command: 'reinstall',
        config: 'tsd.json'
    }, done);
});

gulp.task('build:ts', (done) => {
    var compilerOptions = tsconfig.compilerOptions;
    return gulp.src(['spec/**/*.ts', 'typings/**/*.ts'])
        .pipe(sourcemaps.init())
        .pipe(ts(compilerOptions))
        .js
        .pipe(sourcemaps.write('maps', { includeContent: false }))
        .pipe(gulp.dest('dist'));
});

gulp.task('build', function (callback) {
    runSequence('build:clean',
        'build:tsd',
        'build:ts',
        callback);
});

gulp.task('test-jasmine', (done) => {
    return gulp.src(['dist/**/*.js'])
        .pipe(jasmine())
});

gulp.task('watch', ['test-jasmine'], () => {
    gulp.watch('spec/**/*.ts', ['test']);
});

gulp.task('test', function (callback) {
    runSequence('build', 'test-jasmine', callback);
});

gulp.task('test-watch', function (callback) {
    runSequence('build', 'watch', callback);
});







