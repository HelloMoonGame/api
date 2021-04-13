function validateLoginAttempt() {
    $.post("/Account/CheckLoginApproval",
        {
            id: $('input#loginAttemptId').val(),
            returnUrl: $('input#returnUrl').val(),
            rememberLogin: $('input#rememberLogin').val()
        },
        function (data) {
            console.log(data);
            if (data.expired || data.approved) {
                window.location.href = data.returnUrl;
            } else {
                setTimeout(validateLoginAttempt, 2000);
            }
        });
}

var expiryDate = new Date($('input#expiryDate').val());
function updateTimeLeft() {
    var timeLeft = (expiryDate - new Date()) / 1000,
        secondsLeft = parseInt(timeLeft % 60),
        minutesLeft = parseInt(timeLeft / 60);

    $('#timeLeft').html(minutesLeft + ":" + ("0" + secondsLeft).slice(-2));

    setTimeout(updateTimeLeft, 1000);
}

$(function () {
    updateTimeLeft();
    validateLoginAttempt();
});