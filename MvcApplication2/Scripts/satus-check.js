var intervalId;

function TriggerInverval() {
    return window.setInterval(function () { CheckStatus(); }, 5000);
}

alert("DSF");
var a = "<%=Model.Status%>"
alert(a);
if ("@Model.Status" == "prepared") {
    
    $("#spin-div").spin("large");
    intervalId = TriggerInverval();
}


function CheckStatus() {

    var dataToSend = {
        purchaseId: "@Model.ID"
    };

    $.ajax({
        url: "/Purchases/GetStatus",
        type: 'GET',
        data: dataToSend,
        success: function (data) {
            //alert(data["Status"]);
            if (data["Status"] == "paid") {
                window.clearInterval(intervalId);
                $("#spin-div").spin(false);
                $("#content").load("/Purchases/ThankyouPage");
            }
            else
                alert("stauts is " + data["Status"])

        },
        error: function (response, status, error) {
            alert("faliure");
        }
    });
}