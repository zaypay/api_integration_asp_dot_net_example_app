$("#country-validation").hide();
//CheckCountry();


function CheckCountry(ipIsNull) {

    var country = $("#countriesList").val();
    
    if (country == "" && (ipIsNull != 'True')) {
        $("#country-validation").html("<strong>Error!!</strong> Country not supported")
        $("#country-validation").show()
        $("#languagesList").val("en")
    }    

}

function SubmitForm() {

    
    var ele = $("#countriesList");
    var country = ele.val();

    if (country != "" && $("#paymentMethod").val() != null) {        
        ele[0].form.submit();
    }
    else {
        if (country == "") {
            $("#country-validation").html("<strong>Error ! </strong> Please Select a Country ")
            $("#country-validation").show();
        }
        else {
            ShowError("No payment method selected");            
        }
        
    }

}

function HideRadioDiv() {
    $("#radios").html("");
    $("#radios").hide();
    $("#radio-label").hide();
}

function ShowRadioDiv() {
    $("#radios").show();
    $("#radio-label").show();
    $("#country-validation").hide();
}

function HideElements() {        
    $("#country-validation").hide();
    HideRadioDiv();
}

function LanguageOnChange(name, id) {
    $("#languagesList").change(function () {
        UpdatePaymentMethods(name, id);
    });

}

function CountryOnChange(name, id) {
    $("#countriesList").change(function () {
        UpdatePaymentMethods(name, id);
    });
}

function UpdatePaymentMethods(name, id) {

    var country = $("#countriesList").val();

    if (country != "") {

        var language = $("#languagesList").val();
        var product = name;
        
        var dataToSend = {
            country: country,
            language: language,
            productId: id

        };

        $.ajax({
            url: "/Purchases/GetPaymentMethods",
            type: 'POST',
            data: dataToSend,
            success: function (data) {
                if (data["success"] == false) {
                    ShowError(data["message"]);

                    HideElements();
                }
                else {
                    $("#radios").html(data);
                    ShowRadioDiv();
                }

            },
            error: function (response, status, error) {
                HideRadioDiv();
            }
        });
    }
    else {
        HideRadioDiv();
    }
        
}


