var textFields = document.querySelectorAll('.mdc-text-field');
textFields.forEach(function (textField) {
    mdc.textField.MDCTextField.attachTo(textField);
});

var lists = document.querySelectorAll('.mdc-list');
lists.forEach(function (list) {
    mdc.list.MDCList.attachTo(list);
});

var circularProgresses = document.querySelectorAll('.mdc-circular-progress');
circularProgresses.forEach(function (circularProgress) {
    var m = mdc.circularProgress.MDCCircularProgress.attachTo(circularProgress);
    circularProgress.setProgress = function (value) {
        m.progress = value;
    };
});