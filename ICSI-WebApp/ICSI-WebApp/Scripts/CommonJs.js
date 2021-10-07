function onlyChars(e) {

    var keyVal = 0;
    //if (!e) var e = window.event;
    if (!e.which)
        keyVal = e.keyCode;
    else
        keyVal = e.which;

    if (keyVal !== 27) {
        if ((keyVal >= 65) && (keyVal <= 90) || (keyVal >= 97) && (keyVal <= 122) || keyVal === 8 || keyVal === 9 || keyVal === 32 || keyVal === 38 || keyVal === 45) {
            return true;
        } else {
            keyVal = 0;
            return false;
        }
    }
    return false;
}

function onlydigits(e) {
    //var keyVal = 0;
    ////if (!e) var e = window.event;
    //if (!e.which) keyVal = e.keyCode;
    //else keyVal = e.which;
    //if (keyVal !== 27) {
    //    if ((keyVal >= 46 && keyVal <= 57) || (keyVal >= 35 && keyVal < 37) || (keyVal === 39 || keyVal === 8 || keyVal === 9)) {
    //        return true;
    //    } else {
    //        keyVal = 0;
    //        return false;
    //    }
    //}
    //return false;

    if (e.which !== 8 && e.which !== 0 && (e.which < 48 || e.which > 57)) {
        return false;
    }
}

function OnlyCharAndDigits(e) {
    var keyCode = e.keyCode || e.which;
    var regex = /^[A-Za-z0-9 ]+$/;

    if (!regex.test(String.fromCharCode(keyCode))) {
        return false;
    }      
}

function checkUrl(url) {
    if (url !== "") {
        var urlregex = /(http(s)?:\\)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?/;
        if (!urlregex.test(url)) {
            DialogWarningMessage("Please enter valid Web Address");
            return false;
        } else {
            return true;
        }
    }
}

function BindDropdown(Data, control) {

    $("#" + control + " :gt(0)").remove();
    //$("#" + control + " option").remove();
    if (Data !== null) {
        $.each(Data, function (i) {
            var optionhtml = '<option value="' +
                Data[i].Id + '">' + Data[i].Name + '</option>';
            $("#" + control).append(optionhtml);
        });
    }
}


function MakeDisableControls(divId) {

    //$("INPUT[type='text'],select").each(function () {
    //    $(this).attr("disabled", "disabled");
    //});


    $("INPUT[type='button']").each(function () {
        $(this).attr("disabled", "disabled");
    });

    $("INPUT[type='submit']").each(function () {
        $(this).attr("disabled", "disabled");
    });

    $("BUTTON[type='button']").each(function () {
        $(this).attr("disabled", "disabled");
        //$(this).hide();
    });

    //$('#txtManager').removeAttr('disabled'); // enable


    var obj = {};
    //Find all input elements
    var panel = $("#" + divId);
    var inpts = panel.find("input,textarea,select,button");
    // var txtArea = panel.find("textarea");

    if (inpts.length > 0) {
        $.each(inpts, function (key, value) {
            if (value.type === 'text' ||
                value.type === 'file' ||
                value.type === 'hidden' ||
                value.type === 'password' ||
                value.type === 'textarea' ||
                value.type === 'select' ||
                value.type === 'date' ||               
                value.type === 'select-one') {
                //if (value.value != '')
                //    obj[value.id] = value.value;

                $(this).attr("disabled", "disabled");
            }
            else if (value.type === 'checkbox' || value.type === 'radio') {
                //if ($("#" + value.id).is(":checked")) {
                //    obj[value.name] = value.value;
                //}
                $(this).attr("disabled", "disabled");
            }
        });

    }
}

function MakeEnableControls(divId) {
    $("#" + divId + " INPUT[type='button']").each(function () {
        $(this).removeAttr("disabled");
    });

    var obj = {};
    //Find all input elements
    var panel = $("#" + divId);
    var inpts = panel.find("input,textarea,select");
    // var txtArea = panel.find("textarea");

    if (inpts.length > 0) {
        $.each(inpts, function (key, value) {
            if (value.type === 'text' ||
                value.type === 'file' ||
                value.type === 'hidden' ||
                value.type === 'password' ||
                value.type === 'textarea' ||
                value.type === 'select' ||
                value.type === 'date' ||
                value.type === 'select-one') {
                //if (value.value != '')
                //    obj[value.id] = value.value;

                $(this).removeAttr("disabled");
            }
            else if (value.type === 'checkbox' || value.type === 'radio') {
                //if ($("#" + value.id).is(":checked")) {
                //    obj[value.name] = value.value;
                //}
                $(this).removeAttr("disabled");
            }
        });

    }
}

function IsEmail(email) {
    var regex = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z]{2,4})+$/;
    if (!regex.test(email)) {
        return false;
    } else {
        return true;
    }
}



function ClearControls(divId) {
    $("#" + divId + " INPUT[type='button']").each(function () {
        $(this).removeAttr("disabled");
    });

    var obj = {};

    var panel = $("#" + divId);
    var inpts = panel.find("input,textarea");


    if (inpts.length > 0) {
        $.each(inpts, function (key, value) {
            if (value.type === 'text' ||
                value.type === 'file' ||
                value.type === 'hidden' ||
                value.type === 'password' ||
                value.type === 'textarea' ||
                value.type === 'date' ||
                value.type === 'select-one') {
                $(this).val('');
            }
            else if (value.type === 'select') {
                $(this).val('0');
            }
            else if (value.type === 'checkbox' || value.type === 'radio') {
                $(this).attr('checked', false);
            }
            else if (value.type === 'select-multiple') {

                var obj = [];
                $('#' + this.id + ' option:selected').each(function () {
                    obj.push($(this).index());
                });

                for (var i = 0; i < obj.length; i++) {
                    $('#' + this.id)[0].sumo.unSelectItem(obj[i]);
                }
            }
        });

    }
}
