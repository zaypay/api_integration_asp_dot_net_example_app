

function StartUp(triesLeft) {

    if (typeof (intervalId) != "undefined") {
        window.clearInterval(intervalId);
    }

    if(triesLeft <= 2)                
        ShowError("You have "+triesLeft+" tries left");
    
}

function CheckTriesLeft(triesLeft) {        
    
    var filter  = /^[0-9]+$/

    if (parseInt(triesLeft) <= 0) {        
        ShowError("You have "+triesLeft+" tries left")            
    }
    else if (filter.test($("#code").val()) == false) {        
        ShowError("Verification code can only contain digits")                        
    }
    else{            

        var purchaseId = $("#purchaseId").val();
        var paymentId = $("#paymentId").val();
        var code =  $("#code").val();

        var dataToSend = {
            code: code,
            purchaseId: purchaseId,
            paymentId: paymentId

        };
        
        $.ajax({
            url: "/Purchases/SubmitVerificationCode",
            type: 'POST',
            data: dataToSend,
            success: function (data) {
                if (data["success"] == false) {

                    if (data["redirect"] == false) {
                        ShowError(data["message"]);                            
                    }
                    else {
                        $(window.location.replace("/Error/NotFound"));
                    }
                }
                else {                    
                    $("#content").html(data);
                }

            },
            error: function (response, status, error) {
                //HideRadioDiv();
            }
        });
        //$("#verForm").submit();  
    }
}
