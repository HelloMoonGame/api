var circularProgresses = document.querySelectorAll('.mdc-circular-progress');
circularProgresses.forEach(function (circularProgress) {
    var m = mdc.circularProgress.MDCCircularProgress.attachTo(circularProgress);
    circularProgress.setProgress = function (value) {
        m.progress = value;
    };
});