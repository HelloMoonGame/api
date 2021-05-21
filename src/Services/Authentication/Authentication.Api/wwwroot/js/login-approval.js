function validateLoginAttempt() {
    $.post("/Account/CheckLoginApproval",
        $("form#loginApproval").serialize(),
        function (data) {
            if (data.expired || data.approved) {
                window.location.href = data.returnUrl;
            } else {
                setTimeout(validateLoginAttempt, 2000);
            }
        });
}

var expiryDate = new Date($('input#ExpiryDate').val());
var originalTimeLeft = (expiryDate - new Date()) / 1000;
var timeLeftElement = document.getElementById('timeLeft');

function updateTimeLeft() {
    var timeLeft = Math.max(0, (expiryDate - new Date()) / 1000),
        secondsLeft = parseInt(timeLeft % 60),
        minutesLeft = parseInt(timeLeft / 60);

    var progress = timeLeft / originalTimeLeft;
    timeLeftElement.setProgress(progress);
    timeLeftElement.getElementsByClassName("text")[0].innerText = minutesLeft + ":" + ("0" + secondsLeft).slice(-2);
    
    setTimeout(updateTimeLeft, 1000);
}

$(function () {
    updateTimeLeft();
    validateLoginAttempt();
});