
function ShowError(message) {
    $("#alert-box").html("<div id='error-mesg' class='alert alert-error fade in' style='width:500px;'><h4 class='alert-heading'>" + message + "</h4></div>");
    CloseAlert();
}

function CloseAlert() {    
    window.setTimeout(function () { $("#error-mesg").alert('close') }, 2000);
}
