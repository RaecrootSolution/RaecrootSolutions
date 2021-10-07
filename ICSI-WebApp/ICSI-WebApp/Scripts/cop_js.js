
function COP_Issue_Validate_First_Screen() {

    $.ajax({
        type: "GET",
        url: "../COP/Validate_COP_First_Step",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert("Please try again.");
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "INACTIVE") {
                $("input.checkbx").prop("disabled", true);
                alert("The FCS Membership status not in Active state");
            }
            else if (data === "FEE") {
                $("input.checkbx").prop("disabled", true);
                alert("Please pay the annual membership fee");
            }
            else {
                data = $.parseJSON(data);
                if (data !== null) {
                    if (data.Validation_Status === "INACTIVE") {
                        $("input.checkbx").prop("disabled", true);
                        alert("The FCS Membership status not in Active state");
                    }
                    //if (data.Validation_Status === "SUCCESS") {                   
                    //}

                    if (data.Application_Status !== null) {
                        if (data.Application_Status !== "0" && data.Application_Status !== "1") {
                            $("input.checkbx").prop("disabled", true);
                            $("#submitbtn").prop("disabled", false);

                            $('#chk_01').prop('checked', true);
                            $('#chk_02').prop('checked', true);
                            $('#chk_03').prop('checked', true);
                        }
                    }
                }
            }
        },
        error: function (e, x) {
            alert("ErrorMessage", e.responseText);
        }
    });


}

function Insert_COPIssue_Last_Employment_Details() {
    if ($("#CI_LE_Company_Name").val().trim() === "") {
        alert("Please enter company name");
        return false;
    }
    else if ($("#CI_LE_Designation").val().trim() === "") {
        alert("Please enter designation");
        return false;
    }
    else if ($("#CI_LE_Joining_Date").val().trim() === "") {
        alert("Please enter joining date");
        return false;
    }
    else if ($("#CI_LE_Last_End_Date").val().trim() === "") {
        alert("Please enter last end date");
        return false;
    }
    else if ($("#CI_LE_Employment_Duration").val().trim() === "") {
        alert("Please enter employement duration");
        return false;
    }

    $.ajax({
        type: "GET",
        url: "../COP/Insert_COPIssue_Last_Employment_Details",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            Company_Name: $("#CI_LE_Company_Name").val(), Designation: $("#CI_LE_Designation").val(),
            Joining_Date: $("#CI_LE_Joining_Date").val(), Last_End_Date: $("#CI_LE_Last_End_Date").val(),
            Employment_Duration: $("#CI_LE_Employment_Duration").val(), User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                $("#contact_form")[0].submit();
                alert("Details inserted successfully");
                $("#nextscreen").val("620");
                //window.location.href = "/Home/Search_Trainings";
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Personal_Details_Load() {
    $("#tblOrientation_Details tbody").html('');
    $("#tblLast_Employment_Details tbody").html('');
    //$("#tblDocument_Details tbody").html('');

    $.ajax({
        type: "GET",
        url: "../COP/Personal_Details_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);
                if (data !== null) {
                    if (data.lst_ECSIN_Exempted.length > 0) {
                        BindDropdown(data.lst_ECSIN_Exempted, "ddlExempted");
                    }

                    $("#hdnMEM_COP_DETAILS_ID").val(data.MEM_COP_DETAILS_ID);
                    $("#ddlExempted").find("option[value=" + data.ECSIN_EXEMPTION_CATERGORY_ID + "]").prop("selected", "selected");
                    if (data.ECSIN_NOT_APPLICABLE_YN === 1)
                        $("#chkNot_Applicable").prop("checked", true);
                    if (data.ECSIN_EXEMPTED === 1) {
                        $("#exempted").prop("checked", true);
                        $("#exempted_drpdown").show();
                    }

                    $("#spnMember_Name").append(data.Member_Name);
                    $("#spnMember_Father_Name").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMembership_No").append(data.Membership_No);

                    $("#spnAddress").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMobile").append(data.MOBILE_NO_TX);
                    $("#spnEmail").append(data.EMAIL_ID_TX);
                    $("#spnWebsite").append(data.WEBSITE_TX);

                    //Details of Orientation Programme
                    if (data.lst_Orientation_Details !== null) {
                        if (data.lst_Orientation_Details.length > 0) {
                            var html = '';
                            for (var i = 0; i < data.lst_Orientation_Details.length; i++) {
                                html = html + '<tr>'
                                    + '<td> ' + data.lst_Orientation_Details[i].ORGANIZED_BY + ' </td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].CERTIFICATE_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].FROM_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].TO_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].DURATION_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblOrientation_Details tbody").append(html);
                        }
                    }
                    //Details of last employee details
                    if (data.lst_Last_Employee_Details !== null) {
                        if (data.lst_Last_Employee_Details.length > 0) {
                            var le_html = '';
                            for (var j = 0; j < data.lst_Last_Employee_Details.length; j++) {
                                le_html = le_html + '<tr>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].COMPNAY_NAME_TX + ' </td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DIR_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].ECSIN_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DESIGNATION_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].JOINING_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].LAST_END_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DURATION_OF_EMPLOYEEMENT_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblLast_Employment_Details tbody").append(le_html);
                        }
                    }

                    ////Document details
                    if (data.lst_Document_Details.length > 0) {
                        var htmlstr = "";
                        htmlstr += "<table id='tbledu' class='table table-bordered tableSearchResults table-responsive mt-30'><thead><tr>";
                        htmlstr += "<th class='searchResultsHeading'>Document Type</th>";
                        htmlstr += "<th class='searchResultsHeading'>Uploaded On</th>";
                        htmlstr += "<th class='searchResultsHeading'>Preview</th>";
                        htmlstr += "<th class='searchResultsHeading'>Action</th></tr></thead><tbody>";
                        //var origin = window.location.origin;
                        for (var k = 0; k < data.lst_Document_Details.length; k++) {
                            var split = data.lst_Document_Details[k].FILE_PATH.split("\\");
                            var length = data.lst_Document_Details[k].FILE_PATH.split("\\").length;
                            var fullPath = "";
                            fullPath = "../Home/PreviewFileByIDFromSpecificTable?id=" + data.lst_Document_Details[k].ID + "&TableName=MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T&ColumnName=FILE_PATH";
                            htmlstr += "<tr>";
                            htmlstr += "<td>" + data.lst_Document_Details[k].DOCUMENT_TYPE_TX + " </td>";
                            htmlstr += "<td> " + data.lst_Document_Details[k].UPLODED + " </td>";
                            htmlstr += "<td><a target='_blank' href='" + fullPath + "'><button type='button' class='btn btn-primary btn-xs'>Preview</button></a></td>";
                            if (data.lst_Document_Details[k].Approved_Id === 0 || data.lst_Document_Details[k].Approved_Id === 4) {
                                htmlstr += "<td><input type='hidden' value='" + data.lst_Document_Details[k].ID + "' class='removedoc1'/><input type='hidden' value='" + data.lst_Document_Details[k].FILE_PATH + "' class='removedoc2'/><button type='button' class='btn btn-danger btn-xs' onclick=RemoveDOcfromLicentiate(this)>Remove</button></td>";
                            }
                            if (data.lst_Document_Details[k].Approved_Id === 3) {
                                htmlstr += "<td>Approved</td>";
                            }
                            else if (data.lst_Document_Details[k].Approved_Id === 4) {
                                htmlstr += "<td>Call for</td>";
                            }
                            else if (data.lst_Document_Details[k].Approved_Id === 5) {
                                htmlstr += "<td>Rejected</td>";
                            }
                            htmlstr += "</tr>";
                        }
                        htmlstr += "</tbody></table></div>";
                        $("#divBindTable").append(htmlstr);

                        //////var doc_html = '';
                        //////for (var k = 0; k < data.lst_Document_Details.length; k++) {
                        //////    var fullPath = "";
                        //////    fullPath = "../Home/PreviewFileByIDFromSpecificTable?id=" + data.lst_Document_Details[k].ID + "&TableName=MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T&ColumnName=FILE_PATH";

                        //////    doc_html = doc_html + '<tr>'
                        //////        + '<td> ' + data.lst_Document_Details[k].DOCUMENT_TYPE_TX + '</td>'
                        //////        + '<td> ' + data.lst_Document_Details[k].UPLODED + '</td>'
                        //////        + '<td><a target="_blank" href=' + fullPath + '><button type="button" class="btn btn - primary btn - xs">Preview</button></a></td>'
                        //////        + '<td><input type="hidden" value=' + data.lst_Document_Details[k].ID + ' class="removedoc1"/><input type="hidden" value=' + obj[i].FILE_PATH + ' class="removedoc2"/><button type="button" class="btn btn - danger btn - xs" onclick=RemoveDOcfromLicentiate(this)>Remove</button></td>";'
                        //////        + '</tr>';
                        //////}
                        //////$("#tblDocument_Details tbody").append(doc_html);
                    }

                    if (data.Application_Status !== null) {
                        if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                            $("#hdnAPP_STATUS").val(data.Application_Status_Id);

                            $("#ddlExempted").attr("disabled", true);
                            $("#chkNot_Applicable").attr("disabled", true);
                            $("#exempted").attr("disabled", true);

                            $("#btnupload").attr("disabled", true);
                            //$("#btnRemove").attr("disabled", true);
                            $("#btnECSIN_LastEmploy_Details").attr("disabled", true);

                            ////MakeDisableControls("divBindTable");
                        }
                        if (data.Application_Process_Status_Id === 4) {
                            $("#btnupload").attr("disabled", false);
                            $("#btnSubmit_CallFor_Docs").show();
                        }
                    }

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_Personal_Details_Of_COP() {

    if ($("#hdnAPP_STATUS").val() !== "0") {
        screenAction(658, 0);
    }
    else {

        var chk_ECSIN_NOT_APPLICABLE_YN = 0;
        var ECSIN_EXEMPTED = 0;

        if ($("#chkNot_Applicable").prop('checked') === true) {
            chk_ECSIN_NOT_APPLICABLE_YN = 1;
        }

        if ($("#exempted").prop('checked') === true) {
            ECSIN_EXEMPTED = 1;
        }

        $.ajax({
            type: "GET",
            url: "../COP/Insert_Personal_Details_Of_COP",
            contentType: 'application/json;charset=utf-8',
            //processData: false,
            data: {
                User_Id: $("#u").val(), Id: $("#hdnMEM_COP_DETAILS_ID").val(), ECSIN_EXEMPTION_CATERGORY_ID: $("#ddlExempted").val(),
                ECSIN_NOT_APPLICABLE_YN: chk_ECSIN_NOT_APPLICABLE_YN, ECSIN_EXEMPTED: ECSIN_EXEMPTED
            },
            success: function (data) {
                if (data === "ERROR") {
                    alert('Please try again.');
                }
                else if (data === "SESSION") {
                    window.location.href = "../Home/Index";
                }
                else if (data === "SUCCESS") {
                    alert("Details inserted successfully");

                    screenAction(658, 0);
                    //$("#contact_form")[0].submit();
                    //$("#nextscreen").val("622");
                    //window.location.href = "/Home/Search_Trainings";
                }
            },
            error: function (e, x) {
                alert('ErrorMessage', e.responseText);
            }
        });
    }
}

function Submit_CallFor_Docs() {

    $.ajax({
        type: "GET",
        url: "../COP/Submit_CallFor_Docs",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val(), Id: $("#hdnMEM_COP_DETAILS_ID").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                alert("Details submitted successfully");

                screenAction(656, 0);
                //$("#contact_form")[0].submit();
                //$("#nextscreen").val("622");
                //window.location.href = "/Home/Search_Trainings";
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

//Particulars of COP
function Particulars_Of_COP_Load() {

    $.ajax({
        type: "GET",
        url: "../COP/Particulars_Of_COP_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);
                if (data.lst_Area_Of_Practice.length > 0) {
                    BindDropdown(data.lst_Area_Of_Practice, "ddlArea_Of_Practice");
                    $("#hdnCOPID").val(data.COP_PARTICULARS_ID);
                    $("#txtOther_Service").val(data.Other_Service);

                    //$("#ddlArea_Of_Practice").val(['1', '2']);

                    if (data.Area_Of_Practice_Ids !== null) {
                        var selectedOptions = data.Area_Of_Practice_Ids.split(",");
                        for (var i in selectedOptions) {
                            var optionVal = selectedOptions[i];
                            $("#ddlArea_Of_Practice").find("option[value=" + optionVal + "]").prop("selected", "selected");
                        }
                        // $("ddlArea_Of_Practice").multiselect('reload');
                    }

                    if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                        $("#hdnAPP_STATUS").val(data.Application_Status_Id);

                        $("#txtOther_Service").attr("disabled", true);
                        $("#ddlArea_Of_Practice").attr("disabled", true);
                    }


                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_Particulars_Of_COP() {
    if ($("#hdnAPP_STATUS").val() !== "0") {
        screenAction(659, 0);
    }
    else {

        var AP_Ids = '';
        var Area_Of_Practice_Ids = $('#ddlArea_Of_Practice option:selected').toArray().map(item => item.value);
        if (Area_Of_Practice_Ids.length > 0) {
            if (Area_Of_Practice_Ids[0] === "0") {
                alert('Please select area of practice type');
                return false;
            }
            else {
                $("#ddlArea_Of_Practice option:selected").each(function () {
                    if (AP_Ids === "")
                        AP_Ids = this.value;
                    else
                        AP_Ids = AP_Ids + ',' + this.value;
                });
            }
        }
        else {
            alert('Please select area of practice type');
            return false;
        }

        if ($("#txtOther_Service").val().trim() === "") {
            alert("Please enter other service");
            return false;
        }

        $.ajax({
            type: "GET",
            url: "../COP/Insert_Particulars_Of_COP",
            contentType: 'application/json;charset=utf-8',
            //processData: false,
            data: {
                Id: $("#hdnCOPID").val(), Practice_Ids: AP_Ids, Other_Service: $("#txtOther_Service").val(), User_Id: $("#u").val()
            },
            success: function (data) {
                if (data === "ERROR") {
                    alert('Please try again.');
                }
                else if (data === "SESSION") {
                    window.location.href = "../Home/Index";
                }
                else if (data === "SUCCESS") {
                    alert("Details inserted successfully");

                    screenAction(659, 0);
                    //$("#contact_form")[0].submit();
                    //$("#nextscreen").val("622");
                    //window.location.href = "/Home/Search_Trainings";
                }
            },
            error: function (e, x) {
                alert('ErrorMessage', e.responseText);
            }
        });
    }
}

// Other instructions of COP
function COP_Issue_Other_Institution_Details_Load() {
    $("#IOID_tblFirmDetails tbody").html('');
    $.ajax({
        type: "GET",
        url: "../COP/COP_Issue_Other_Institution_Details_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data.lst_Firm_Details.length > 0) {
                    //$("#hdnCOPID").val(data.COP_PARTICULARS_ID);
                    var html = '';
                    for (var i = 0; i < data.lst_Firm_Details.length; i++) {
                        html = html + '<tr>'
                            + '<td> ' + data.lst_Firm_Details[i].FIRM_TYPE_TX + ' </td>'
                            + '<td> ' + data.lst_Firm_Details[i].FIRM_NAME_TX + '</td>'
                            + '<td></td>'
                            + '<td> ' + data.lst_Firm_Details[i].BUSINESS_NATURE_TX + '</td>'
                            + '<td>' + data.lst_Firm_Details[i].NAME_OF_PARTNERS_TX + '</td>'
                            + '<td> ' + data.lst_Firm_Details[i].OTHER_DETAILS_TX + '</td>'
                            + '<td> <button type="button" class="btn btn-danger btn-xs" onclick="IOID_Delete_Firm_Details(' + data.lst_Firm_Details[i].ID + ');"> Remove </button></td>'
                            + '</tr>';
                    }

                    $("#IOID_tblFirmDetails tbody").append(html);
                }

                $("#hdnCOP_OI_ID").val(data.ID);
                $("#IOID_ICAI_Membership_No").val(data.ICAI_MEMBERSHIP_NO_TX);
                $("#IOID_ICAI_Cost_Membership_No").val(data.COST_MEMBERSHIP_NO_TX);
                $("#IOID_Bar_Council_Membership_No").val(data.BAR_COUNCIL_NO_TX);
                $("#IOID_Professional_Membership_No").val(data.PROF_INSTITUTE_NO_TX);

                if (data.IS_ICAI_MEMBER !== null) {
                    $("input[name='IOID_ICAI_Member_radio1'][value='" + data.IS_ICAI_MEMBER + "']").prop('checked', true);
                }
                if (data.IS_ICAI_MEMBER_HC !== null) {
                    $("input[name='IOID_ICAI_Member_radio2'][value='" + data.IS_ICAI_MEMBER_HC + "']").prop('checked', true);
                }

                if (data.IS_COST_MEMBERSHIP !== null) {
                    $("input[name='IOID_ICAI_Membership_radio1'][value='" + data.IS_COST_MEMBERSHIP + "']").prop('checked', true);
                }
                if (data.COST_MEMBERSHIP_HC !== null) {
                    $("input[name='IOID_ICAI_Membership_radio2'][value='" + data.COST_MEMBERSHIP_HC + "']").prop('checked', true);
                }

                if (data.IS_BAR_COUNCIL !== null) {
                    $("input[name='IOID_Bar_Membership_radio1'][value='" + data.IS_BAR_COUNCIL + "']").prop('checked', true);
                }
                if (data.IS_BAR_COUNCIL_HC !== null) {
                    $("input[name='IOID_Bar_Membership_radio2'][value='" + data.IS_BAR_COUNCIL_HC + "']").prop('checked', true);
                }

                if (data.IS_PROF_INSTITUTE !== null) {
                    $("input[name='IOID_Prof_Membership_radio1'][value='" + data.IS_PROF_INSTITUTE + "']").prop('checked', true);
                }
                if (data.IS_PROF_INSTITUTE_HC !== null) {
                    $("input[name='IOID_Prof_Membership_radio2'][value='" + data.IS_PROF_INSTITUTE_HC + "']").prop('checked', true);
                }

                if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                    $("#hdnAPP_STATUS").val(data.Application_Status_Id);

                    MakeDisableControls("div_Other_Details");

                    $("#btn_Other_Back").attr("disabled", false);
                    $("#btn_Other_Next").attr("disabled", false);
                }



                //if ($("#IsSingleComplainant").val() == "True") {

                //    //$("input[name='rdbCgroup']:checked").val("1");
                //    $("input[name='rdbCgroup'][value='" + 1 + "']").prop('checked', true);

                //    $("input[name='rdbMIgroup']").attr('disabled', true);
                //}
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_COP_Other_Firm_Details() {

    if ($("#FIRM_TYPE_ID").val() === "0") {
        alert("Please select firm type.");
        return false;
    }
    else if ($("#FIRM_NAME_TX").val().trim() === "") {
        alert("Please enter firm name");
        return false;
    }

    if ($("#FIRM_TYPE_ID").val() === "2" || $("#FIRM_TYPE_ID").val() === "3") {
        if ($('#IOID_NAME_OF_PARTNERS_TX').val().trim() === "") {
            alert("Please enter partners.");
            return false;
        }
    }

    _Firm_Details = {};
    _Firm_Details["USER_ID"] = $("#u").val();
    _Firm_Details["FIRM_TYPE_ID"] = $('#FIRM_TYPE_ID').val();
    _Firm_Details["FIRM_NAME_TX"] = $('#FIRM_NAME_TX').val();
    _Firm_Details["ADDRESS1_TX"] = $('#IOID_ADDRESS1_TX').val();
    _Firm_Details["ADDRESS2_TX"] = $('#IOID_ADDRESS2_TX').val();
    _Firm_Details["ADDRESS3_TX"] = $('#IOID_ADDRESS3_TX').val();
    _Firm_Details["COUNTRY_ID"] = $('#COUNTRY_ID').val();
    _Firm_Details["STATE_ID"] = $('#STATE_ID').val();
    _Firm_Details["DISTRICT_ID"] = $('#DISTRICT_ID').val();
    _Firm_Details["CITY_ID"] = $('#CITY_ID').val();
    _Firm_Details["PINCODE_NM"] = $('#IOID_PINCODE_NM').val();
    _Firm_Details["BUSINESS_NATURE_TX"] = $('#IOID_BUSINESS_NATURE_TX').val();
    _Firm_Details["NAME_OF_PARTNERS_TX"] = $('#IOID_NAME_OF_PARTNERS_TX').val();
    _Firm_Details["OTHER_DETAILS_TX"] = $('#IOID_OTHER_DETAILS_TX').val();

    $.ajax({
        type: "GET",
        url: "../COP/Insert_COP_Other_Firm_Details",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            Firm_Details: JSON.stringify(_Firm_Details)
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                alert("Details are inserted successfully.");

                //$("#contact_form")[0].submit();
                //$("#nextscreen").val("622");
                screenAction(622, 0);
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function IOID_Delete_Firm_Details(ID) {
    $.ajax({
        type: "GET",
        url: "../COP/IOID_Delete_Firm_Details",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            ID: ID
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                alert("Details are deleted successfully.");
                //$("#contact_form")[0].submit();
                //$("#nextscreen").val("622");
                screenAction(622, 0);
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_COP_Other_Institutions_Details() {

    if ($("#hdnAPP_STATUS").val() !== "0") {
        screenAction(678, 0);
    }
    else {

        var IOID_ICAI_Member_radio1 = $("input[name='IOID_ICAI_Member_radio1']:checked").val();
        var IOID_ICAI_Member_radio2 = $("input[name='IOID_ICAI_Member_radio2']:checked").val();

        var IOID_ICAI_Membership_radio1 = $("input[name='IOID_ICAI_Membership_radio1']:checked").val();
        var IOID_ICAI_Membership_radio2 = $("input[name='IOID_ICAI_Membership_radio2']:checked").val();

        var IOID_Bar_Membership_radio1 = $("input[name='IOID_Bar_Membership_radio1']:checked").val();
        var IOID_Bar_Membership_radio2 = $("input[name='IOID_Bar_Membership_radio2']:checked").val();

        var IOID_Prof_Membership_radio1 = $("input[name='IOID_Prof_Membership_radio1']:checked").val();
        var IOID_Prof_Membership_radio2 = $("input[name='IOID_Prof_Membership_radio2']:checked").val();

        //Level 1
        if (IOID_ICAI_Member_radio1 === null || IOID_ICAI_Member_radio1 === undefined) {
            alert('Please select ICAI Member Yes/No.');
            return false;
        }
        else if (IOID_ICAI_Member_radio2 === null || IOID_ICAI_Member_radio2 === undefined) {
            alert('Please select ICAI Member Holding Certificate of Practice of ICAI as on date Yes/No.');
            return false;
        }
        else if (IOID_ICAI_Membership_radio1 === null || IOID_ICAI_Membership_radio1 === undefined) {
            alert('Please select ICAI (Cost) Membership Yes/No.');
            return false;
        }
        else if (IOID_ICAI_Membership_radio2 === null || IOID_ICAI_Membership_radio2 === undefined) {
            alert('Please select ICAI (Cost) Membership Holding Certificate of Practice of ICAI as on date Yes/No.');
            return false;
        }
        else if (IOID_Bar_Membership_radio1 === null || IOID_Bar_Membership_radio1 === undefined) {
            alert('Please select Bar Council Membership Yes/No.');
            return false;
        }
        else if (IOID_Bar_Membership_radio2 === null || IOID_Bar_Membership_radio2 === undefined) {
            alert('Please select Bar Council Membership Holding Certificate of Practice of ICAI as on date Yes/No.');
            return false;
        }
        else if (IOID_Prof_Membership_radio1 === null || IOID_Prof_Membership_radio1 === undefined) {
            alert('Please select Other Professional Institutes Membership Yes/No.');
            return false;
        }
        else if (IOID_Prof_Membership_radio2 === null || IOID_Prof_Membership_radio2 === undefined) {
            alert('Please select Other Professional Institutes Membership Holding Certificate of Practice of ICAI as on date Yes/No.');
            return false;
        }

        //Level2
        if (IOID_ICAI_Member_radio1 === "1" && $("#IOID_ICAI_Membership_No").val().trim() === "") {
            alert("Please enter ICAI Member Membershio no");
            return false;
        }
        else if (IOID_ICAI_Membership_radio1 === "1" && $("#IOID_ICAI_Cost_Membership_No").val().trim() === "") {
            alert('Please enter ICAI (Cost) Membership no');
            return false;
        }
        else if (IOID_Bar_Membership_radio1 === "1" && $("#IOID_Bar_Council_Membership_No").val().trim() === "") {
            alert('Please enter Bar Council Membership no');
            return false;
        }
        else if (IOID_Prof_Membership_radio1 === "1" && $("#IOID_Professional_Membership_No").val().trim() === "") {
            alert('Please enter Other Professional Institutes Membership no');
            return false;
        }

        //Level3
        if (IOID_ICAI_Member_radio2 === "1") {
            alert('You are not eligible to apply for CoP of ICSI till you surrenders the CoP of ICAI.');
            return false;
        }
        else if (IOID_ICAI_Membership_radio2 === "1") {
            alert('You are not eligible to apply for CoP of ICSI till you surrenders the CoP of ICAI.');
            return false;
        }
        else if (IOID_Bar_Membership_radio2 === "1") {
            alert('You are not eligible to apply for CoP of ICSI till you surrenders the CoP of ICAI.');
            return false;
        }
        else if (IOID_Prof_Membership_radio2 === "1") {
            alert('You are not eligible to apply for CoP of ICSI till you surrenders the CoP of ICAI.');
            return false;
        }

        if (!$("#IOID_Chk_Declaration").prop("checked")) {
            alert('Please select declaration.');
            return false;
        }

        _Institutions_Details = {};
        _Institutions_Details["ID"] = $("#hdnCOP_OI_ID").val();
        _Institutions_Details["USER_ID"] = $("#u").val();

        _Institutions_Details["IS_ICAI_MEMBER"] = IOID_ICAI_Member_radio1;
        _Institutions_Details["ICAI_MEMBERSHIP_NO_TX"] = $('#IOID_ICAI_Membership_No').val();
        _Institutions_Details["IS_ICAI_MEMBER_HC"] = IOID_ICAI_Member_radio2;

        _Institutions_Details["IS_COST_MEMBERSHIP"] = IOID_ICAI_Membership_radio1;
        _Institutions_Details["COST_MEMBERSHIP_NO_TX"] = $('#IOID_ICAI_Cost_Membership_No').val();
        _Institutions_Details["COST_MEMBERSHIP_HC"] = IOID_ICAI_Membership_radio2;

        _Institutions_Details["IS_BAR_COUNCIL"] = IOID_Bar_Membership_radio1;
        _Institutions_Details["BAR_COUNCIL_NO_TX"] = $('#IOID_Bar_Council_Membership_No').val();
        _Institutions_Details["IS_BAR_COUNCIL_HC"] = IOID_Bar_Membership_radio2;

        _Institutions_Details["IS_PROF_INSTITUTE"] = IOID_Prof_Membership_radio1;
        _Institutions_Details["PROF_INSTITUTE_NO_TX"] = $('#IOID_Professional_Membership_No').val();
        _Institutions_Details["IS_PROF_INSTITUTE_HC"] = IOID_Prof_Membership_radio2;

        $.ajax({
            type: "GET",
            url: "../COP/Insert_COP_Other_Institutions_Details",
            contentType: 'application/json;charset=utf-8',
            //processData: false,
            data: {
                Institutions_Details: JSON.stringify(_Institutions_Details)
            },
            success: function (data) {
                if (data === "ERROR") {
                    alert('Please try again.');
                }
                else if (data === "SESSION") {
                    window.location.href = "../Home/Index";
                }
                else if (data === "SUCCESS") {
                    alert("Details are saved successfully.");

                    //$("#contact_form")[0].submit();
                    //$("#nextscreen").val("622");
                    screenAction(678, 0);
                }
            },
            error: function (e, x) {
                alert('ErrorMessage', e.responseText);
            }
        });
    }
}

// Declaration
function COP_Issue_Declaration_Details_Load() {

    $.ajax({
        type: "GET",
        url: "../COP/COP_Issue_Declaration_Details_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data.lst_Dependants_Details.length > 0) {
                    //$("#hdnCOPID").val(data.COP_PARTICULARS_ID);                
                    var count = 0;
                    for (var i = 0; i < data.lst_Dependants_Details.length; i++) {
                        count = count + 1;

                        $("#hdnCID_DEP_ID" + count).val(data.lst_Dependants_Details[i].ID);
                        $("#CID_Name" + count).val(data.lst_Dependants_Details[i].NAME_TX);
                        $("#CID_Age" + count).val(data.lst_Dependants_Details[i].AGE_NM);
                        $("#CID_Relation" + count).val(data.lst_Dependants_Details[i].RELATION_TO_SUBSCRIBER_TX);
                        $("#CID_Email" + count).val(data.lst_Dependants_Details[i].EMAIL_TX);
                        $("#CID_Phone" + count).val(data.lst_Dependants_Details[i].MOBILE_TX);
                        $("#CID_Address" + count).val(data.lst_Dependants_Details[i].ADDRESS);
                    }
                }

                $("#Member_Name").append(data.Student_Name);
                $("#Member_Name1").append(data.Student_Name);

                if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                    $("#hdnAPP_STATUS").val(data.Application_Status_Id);

                    $("#dependentdetails").css("display", "block");

                    MakeDisableControls("dependentdetails");

                    $("#btn_Declaration_Back").attr("disabled", false);
                    $("#btn_Declaration_Next").attr("disabled", false);

                    $('#chk_Declaration').prop('checked', true);
                }


            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_COP_Declaration_Dependent() {

    if ($("#hdnAPP_STATUS").val() !== "0") {
        screenAction(679, 0);
    }
    else {
        var Dependants = [];
        var _Firm_Details = {};
        _Firm_Details["USER_ID"] = $("#u").val();


        if ($("#CID_Name1").val().trim() !== "") {
            _Firm_Details["ID"] = $("#hdnCID_DEP_ID1").val();
            _Firm_Details["NAME_TX"] = $("#CID_Name1").val();
            _Firm_Details["AGE_NM"] = $("#CID_Age1").val();
            _Firm_Details["RELATION_TO_SUBSCRIBER_TX"] = $("#CID_Relation1").val();
            _Firm_Details["EMAIL_TX"] = $("#CID_Email1").val();
            _Firm_Details["MOBILE_TX"] = $("#CID_Phone1").val();
            _Firm_Details["ADDRESS"] = $("#CID_Address1").val();
            Dependants.push(_Firm_Details);
        }
        if ($("#CID_Name2").val().trim() !== "") {
            _Firm_Details["ID"] = $("#hdnCID_DEP_ID2").val();
            _Firm_Details["NAME_TX"] = $("#CID_Name2").val();
            _Firm_Details["AGE_NM"] = $("#CID_Age2").val();
            _Firm_Details["RELATION_TO_SUBSCRIBER_TX"] = $("#CID_Relation2").val();
            _Firm_Details["EMAIL_TX"] = $("#CID_Email2").val();
            _Firm_Details["MOBILE_TX"] = $("#CID_Phone2").val();
            _Firm_Details["ADDRESS"] = $("#CID_Address2").val();
            Dependants.push(_Firm_Details);
        }
        if ($("#CID_Name3").val().trim() !== "") {
            _Firm_Details["ID"] = $("#hdnCID_DEP_ID3").val();
            _Firm_Details["NAME_TX"] = $("#CID_Name3").val();
            _Firm_Details["AGE_NM"] = $("#CID_Age3").val();
            _Firm_Details["RELATION_TO_SUBSCRIBER_TX"] = $("#CID_Relation3").val();
            _Firm_Details["EMAIL_TX"] = $("#CID_Email3").val();
            _Firm_Details["MOBILE_TX"] = $("#CID_Phone3").val();
            _Firm_Details["ADDRESS"] = $("#CID_Address3").val();
            Dependants.push(_Firm_Details);
        }
        if ($("#CID_Name4").val().trim() !== "") {
            _Firm_Details["ID"] = $("#hdnCID_DEP_ID4").val();
            _Firm_Details["NAME_TX"] = $("#CID_Name4").val();
            _Firm_Details["AGE_NM"] = $("#CID_Age4").val();
            _Firm_Details["RELATION_TO_SUBSCRIBER_TX"] = $("#CID_Relation4").val();
            _Firm_Details["EMAIL_TX"] = $("#CID_Email4").val();
            _Firm_Details["MOBILE_TX"] = $("#CID_Phone4").val();
            _Firm_Details["ADDRESS"] = $("#CID_Address4").val();
            Dependants.push(_Firm_Details);
        }
        if ($("#CID_Name5").val().trim() !== "") {
            _Firm_Details["ID"] = $("#hdnCID_DEP_ID5").val();
            _Firm_Details["NAME_TX"] = $("#CID_Name5").val();
            _Firm_Details["AGE_NM"] = $("#CID_Age5").val();
            _Firm_Details["RELATION_TO_SUBSCRIBER_TX"] = $("#CID_Relation5").val();
            _Firm_Details["EMAIL_TX"] = $("#CID_Email5").val();
            _Firm_Details["MOBILE_TX"] = $("#CID_Phone5").val();
            _Firm_Details["ADDRESS"] = $("#CID_Address5").val();
            Dependants.push(_Firm_Details);
        }

        if (Dependants.length > 0) {
            $.ajax({
                type: "GET",
                url: "../COP/Insert_COP_Declaration_Dependent",
                contentType: 'application/json;charset=utf-8',
                //processData: false,
                data: {
                    Dependants: JSON.stringify(Dependants)
                },
                success: function (data) {
                    if (data === "ERROR") {
                        alert('Please try again.');
                    }
                    else if (data === "SESSION") {
                        window.location.href = "../Home/Index";
                    }
                    else if (data === "SUCCESS") {
                        alert("Details are inserted successfully.");

                        //$("#contact_form")[0].submit();
                        //$("#nextscreen").val("622");
                        screenAction(679, 0);
                    }
                },
                error: function (e, x) {
                    alert('ErrorMessage', e.responseText);
                }
            });
        }
        else {
            screenAction(679, 0);
        }
    }

}

// Preview
function COP_Issue_Preview_Load() {
    $("#tblOrientation_Details tbody").html('');
    $("#tblLast_Employment_Details tbody").html('');
    $("#tblDocument_Details tbody").html('');
    $("#tblFirm_Registration_Details1 tbody").html('');

    $.ajax({
        type: "GET",
        url: "../COP/COP_Issue_Preview_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {
                    $("#spnMember_Name").append(data.Member_Name);
                    $("#spnMember_Name1").append(data.Member_Name);
                    $("#spnMember_Name2").append(data.Member_Name);

                    $("#spnMember_Father_Name").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMembership_No").append(data.Membership_No);

                    $("#spnAddress").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMobile").append(data.MOBILE_NO_TX);
                    $("#spnEmail").append(data.EMAIL_ID_TX);
                    $("#spnWebsite").append(data.WEBSITE_TX);

                    if (data.lst_Area_Of_Practice.length > 0) {
                        BindDropdown(data.lst_Area_Of_Practice, "ddlArea_Of_Practice");
                    }

                    //Details of Orientation Programme
                    if (data.lst_Orientation_Details !== null) {
                        if (data.lst_Orientation_Details.length > 0) {
                            var html = '';
                            for (var i = 0; i < data.lst_Orientation_Details.length; i++) {
                                html = html + '<tr>'
                                    + '<td> ' + data.lst_Orientation_Details[i].ORGANIZED_BY + ' </td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].CERTIFICATE_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].FROM_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].TO_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].DURATION_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblOrientation_Details tbody").append(html);
                        }
                    }

                    //Details of last employee details
                    if (data.lst_Last_Employee_Details !== null) {
                        if (data.lst_Last_Employee_Details.length > 0) {
                            var le_html = '';
                            for (var j = 0; j < data.lst_Last_Employee_Details.length; j++) {
                                le_html = le_html + '<tr>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].COMPNAY_NAME_TX + ' </td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DIR_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].ECSIN_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DESIGNATION_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].JOINING_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].LAST_END_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DURATION_OF_EMPLOYEEMENT_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblLast_Employment_Details tbody").append(le_html);
                        }
                    }

                    //Details of document details
                    if (data.lst_Document_Details.length > 0) {
                        var doc_html = '';
                        for (var k = 0; k < data.lst_Document_Details.length; k++) {
                            var fullPath = "";
                            fullPath = "../Home/PreviewFileByIDFromSpecificTable?id=" + data.lst_Document_Details[k].ID + "&TableName=MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T&ColumnName=FILE_PATH";

                            doc_html = doc_html + '<tr>'
                                + '<td> ' + data.lst_Document_Details[k].DOCUMENT_TYPE_TX + '</td>'
                                + '<td> ' + data.lst_Document_Details[k].UPLODED + '</td>'
                                + '<td><a target="_blank" href=' + fullPath + '><button type="button" class="btn btn - primary btn - xs">Preview</button></a></td>'
                                + '</tr>';
                        }
                        $("#tblDocument_Details tbody").append(doc_html);
                    }

                    if (data.Area_Of_Practice_Id !== null) {
                        var selectedOptions = data.Area_Of_Practice_Id.split(",");
                        for (var m in selectedOptions) {
                            var optionVal = selectedOptions[m];
                            $("#ddlArea_Of_Practice").find("option[value=" + optionVal + "]").prop("selected", "selected");
                        }
                        // $("ddlArea_Of_Practice").multiselect('reload');
                    }

                    // Other institutions
                    if (data.lst_Other_Institutions !== null) {
                        if (data.lst_Other_Institutions.length > 0) {
                            $("#IOID_ICAI_Membership_No").val(data.lst_Other_Institutions[0].ICAI_MEMBERSHIP_NO_TX);
                            $("#IOID_ICAI_Cost_Membership_No").val(data.lst_Other_Institutions[0].COST_MEMBERSHIP_NO_TX);
                            $("#IOID_Bar_Council_Membership_No").val(data.lst_Other_Institutions[0].BAR_COUNCIL_NO_TX);
                            $("#IOID_Professional_Membership_No").val(data.lst_Other_Institutions[0].PROF_INSTITUTE_NO_TX);

                            if (data.lst_Other_Institutions[0].IS_ICAI_MEMBER !== null) {
                                $("input[name='IOID_ICAI_Member_radio1'][value='" + data.lst_Other_Institutions[0].IS_ICAI_MEMBER + "']").prop('checked', true);
                            }
                            if (data.lst_Other_Institutions[0].IS_ICAI_MEMBER_HC !== null) {
                                $("input[name='IOID_ICAI_Member_radio2'][value='" + data.lst_Other_Institutions[0].IS_ICAI_MEMBER_HC + "']").prop('checked', true);
                            }

                            if (data.lst_Other_Institutions[0].IS_COST_MEMBERSHIP !== null) {
                                $("input[name='IOID_ICAI_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_COST_MEMBERSHIP + "']").prop('checked', true);
                            }
                            if (data.lst_Other_Institutions[0].COST_MEMBERSHIP_HC !== null) {
                                $("input[name='IOID_ICAI_Membership_radio2'][value='" + data.lst_Other_Institutions[0].COST_MEMBERSHIP_HC + "']").prop('checked', true);
                            }

                            if (data.lst_Other_Institutions[0].IS_BAR_COUNCIL !== null) {
                                $("input[name='IOID_Bar_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_BAR_COUNCIL + "']").prop('checked', true);
                            }
                            if (data.lst_Other_Institutions[0].IS_BAR_COUNCIL_HC !== null) {
                                $("input[name='IOID_Bar_Membership_radio2'][value='" + data.lst_Other_Institutions[0].IS_BAR_COUNCIL_HC + "']").prop('checked', true);
                            }

                            if (data.lst_Other_Institutions[0].IS_PROF_INSTITUTE !== null) {
                                $("input[name='IOID_Prof_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_PROF_INSTITUTE + "']").prop('checked', true);
                            }
                            if (data.lst_Other_Institutions[0].IS_PROF_INSTITUTE_HC !== null) {
                                $("input[name='IOID_Prof_Membership_radio2'][value='" + data.lst_Other_Institutions[0].IS_PROF_INSTITUTE_HC + "']").prop('checked', true);
                            }
                        }
                    }

                    //Firm details
                    if (data.lst_Firm_Details !== null) {
                        if (data.lst_Firm_Details.length > 0) {
                            var fi_html = '';
                            for (var n = 0; n < data.lst_Firm_Details.length; n++) {
                                fi_html = fi_html + '<tr>'
                                    + '<td> ' + data.lst_Firm_Details[n].FIRM_TYPE_TX + ' </td>'
                                    + '<td> ' + data.lst_Firm_Details[n].FIRM_NAME_TX + '</td>'
                                    + '<td> </td>'
                                    + '<td> ' + data.lst_Firm_Details[n].BUSINESS_NATURE_TX + '</td>'
                                    + '<td> ' + data.lst_Firm_Details[n].NAME_OF_PARTNERS_TX + '</td>'
                                    + '<td> ' + data.lst_Firm_Details[n].OTHER_DETAILS_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblFirm_Registration_Details1 tbody").append(fi_html);
                        }
                    }

                    if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                        $("#btn_Preview_Next").attr("disabled", true);
                    }

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}


function COP_ISSUE_PAYMENT_LOAD() {

    $.ajax({
        type: "GET",
        url: "../COP/COP_ISSUE_PAYMENT_LOAD",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {
                    if (data.TAXABLE_YN === 0) {
                        $("#spnAnnual_Fee").text(data.FEE_AMOUNT);
                        $("#spnGST").text('0');
                        $("#spnTotal_Fee").text(data.FEE_AMOUNT);
                    }
                    else if (data.TAXABLE_YN === 1) {
                        var _gstAmt = (data.FEE_AMOUNT * 18) / 100;
                        $("#spnAnnual_Fee").text(data.FEE_AMOUNT);
                        $("#spnGST").text(_gstAmt);
                        $("#spnTotal_Fee").text(parseInt(data.FEE_AMOUNT) + parseInt(_gstAmt));
                    }
                    else {
                        $("#btnPayment_Submit").hide();
                    }

                    //if (data.Application_Status_Id !== 0 && data.Application_Status_Id !== 1) {
                    //    $("#btnPayment_Submit").hide();
                    //}

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function COP_ISSUE_PROCESS_PAYMENT() {

    $.ajax({
        type: "GET",
        url: "../COP/COP_ISSUE_PROCESS_PAYMENT",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                //alert("Inserted");
                screenAction(699, $("#u").val());
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Get_COP_Issue_Payment_Response() {
    if ($("#status").val() === 1) {
        $.ajax({
            type: "GET",
            url: "../COP/Get_COP_Issue_Payment_Response",
            contentType: 'application/json;charset=utf-8',
            //processData: false,
            data: {
                User_Id: $("#u").val(), Status: $("#status").val()
            },
            success: function (data) {
                if (data === "ERROR") {
                    alert('Please try again.');
                }
                else if (data === "SESSION") {
                    window.location.href = "../Home/Index";
                }
                else if (data === "SUCCESS") {
                    //alert("Inserted");
                    screenAction(619, $("#u").val());
                }
            },
            error: function (e, x) {
                alert('ErrorMessage', e.responseText);
            }
        });
    }

}



// Admin Search

function Admin_Search_COP_Requests() {
    $("#tblACRS_Results tbody").html('');

    if ($("#txtACRS_Name").val().trim() === "" && $("#txtACRS_Request_From").val().trim() === "" && $("#txtACRS_Request_To").val().trim() === ""
        && $("#txtACRS_Email_Id").val().trim() === "" && $("#txtACRS_Admission_From").val().trim() === "" && $("#txtACRS_Admission_To").val().trim() === ""
        && $("#txtACRS_MemberShip_Number").val().trim() === "" && $("#txtACRS_MobileNo").val().trim() === "" && $("#txtACRS_COP_Number").val().trim() === ""
        && $("#ddlACRS_Application_Status").val() === "0" && $("#ddlACRS_Request_Type").val() === "0" && $("#ddlACRS_Internal_Dept_Status").val() === "0") {
        alert("Please enter or select any one search criteria");
        return false;
    }
    else if ($("#txtACRS_Request_From").val().trim() !== "" && $("#txtACRS_Request_To").val().trim() === "") {
        alert("Please enter request to date");
        return false;
    }
    else if ($("#txtACRS_Request_From").val().trim() === "" && $("#txtACRS_Request_To").val().trim() !== "") {
        alert("Please enter request from date");
        return false;
    }
    else if ($("#txtACRS_Admission_From").val().trim() !== "" && $("#txtACRS_Admission_To").val().trim() === "") {
        alert("Please enter admission to date");
        return false;
    }
    else if ($("#txtACRS_Admission_From").val().trim() === "" && $("#txtACRS_Admission_To").val().trim() !== "") {
        alert("Please enter admission from date");
        return false;
    }

    if ($("#txtACRS_Email_Id").val().trim() !== "") {
        var isEmail = IsEmail($("#txtACRS_Email_Id").val().trim());
        if (!isEmail) {
            bootbox.alert("Invalid email format.");
            return false;
        }
    }

    $.ajax({
        type: "GET",
        url: "../COP/Admin_COP_Review_Search",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#u").val(), Name: $("#txtACRS_Name").val().trim(), Request_From: $("#txtACRS_Request_From").val().trim(),
            Request_To: $("#txtACRS_Request_To").val().trim(), Email_Id: $("#txtACRS_Email_Id").val().trim(),
            Admission_From: $("#txtACRS_Admission_From").val().trim(), Admission_To: $("#txtACRS_Admission_To").val().trim(),
            MemberShip_Number: $("#txtACRS_MemberShip_Number").val().trim(), MobileNo: $("#txtACRS_MobileNo").val().trim(),
            COP_Number: $("#txtACRS_COP_Number").val().trim(), Application_Status_Id: $("#ddlACRS_Application_Status").val(),
            Request_Type_Id: $("#ddlACRS_Request_Type").val(), Internal_Dept_Status_Id: $("#ddlACRS_Internal_Dept_Status").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert("Please try again.");
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else {
                data = $.parseJSON(data);

                if (data.length > 0) {
                    var html = "";
                    for (var i = 0; i < data.length; i++) {
                        var vAction = '';
                        if (data[i].APPROVAL_LEVEL_NO === 0 && data[i].User_Role_ID === 16 && data[i].Next_Role_ID !== 20)
                            vAction = '<td><button type="button" class="btn btn-primary btn-xs" onclick="screenAction(681,' + data[i].User_Id + ');"> Action </button></td>';
                        ////else if (data[i].APPROVAL_LEVEL_NO === 2 && data[i].User_Role_ID === 16 && data[i].Next_Role_ID === 16 && (data[i].Application_Status_Id === 3 || data[i].Application_Status_Id === 5))
                        ////    vAction = '<td><button type="button" class="btn btn-primary btn-xs" onclick="screenAction(681,' + data[i].User_Id + ');"> Action </button></td>';
                        else if (data[i].APPROVAL_LEVEL_NO === 1 && data[i].User_Role_ID === 18)
                            vAction = '<td><button type="button" class="btn btn-primary btn-xs" onclick="screenAction(681,' + data[i].User_Id + ');"> Action </button></td>';
                        else if (data[i].APPROVAL_LEVEL_NO === 2 && data[i].User_Role_ID === 17 && (data[i].Application_Status_Id === 3 || data[i].Application_Status_Id === 5))
                            vAction = '<td></td>';
                        else if (data[i].APPROVAL_LEVEL_NO === 2 && data[i].User_Role_ID === 17)
                            vAction = '<td><button type="button" class="btn btn-primary btn-xs" onclick="screenAction(681,' + data[i].User_Id + ');"> Action </button></td>';
                        else
                            vAction = '<td></td>';

                        html = html + "<tr>"
                            + vAction
                            + "<td> " + data[i].Request_Type + " </td>"
                            + "<td> " + data[i].Name + "</td>"
                            + "<td>" + data[i].Email_Id + "</td>"
                            + "<td> " + data[i].MobileNo + "</td>"
                            + "<td>" + data[i].Request_Date + "</td>"
                            + "<td> " + data[i].Admission_Date + "</td>"
                            + "<td> " + data[i].Approval_Date + "</td>"
                            + "<td>" + data[i].Fee_Paid + "</td>"
                            + "<td> " + data[i].Application_Status + "</td>"
                            + "<td>" + data[i].COP_Number + "</td>"
                            + "<td> " + data[i].COP_Issue_Date + "</td>"
                            + "<td>" + data[i].Internal_Dept_Status + "</td>"
                            + "<td> " + data[i].MemberShip_Number + "</td>"
                            + "<td>" + data[i].Transcation_Id + "</td>"
                            + "<td> " + data[i].Transcation_Date + "</td>"
                            + "<td> " + data[i].Mode_Of_Payment + "</td>"
                            + "<td> " + data[i].Payment_Status + "</td>"
                            + "</tr>";
                    }

                    $("#tblACRS_Results tbody").append(html);
                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

//..Admin_COP_FormD_Personal_Details_Issue_View

function Admin_COP_Personal_Details_Issue_View_Load() {
    $("#tblOrientation_Details tbody").html('');
    $("#tblLast_Employment_Details tbody").html('');
    $("#tblDocument_Details tbody").html('');

    $.ajax({
        type: "GET",
        url: "../COP/Admin_COP_Personal_Details_Issue_View_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#hidUI").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {

                    $("#spnMember_Name").append(data.Member_Name);
                    $("#spnMember_Name1").append(data.Member_Name);
                    $("#spnMember_Name2").append(data.Member_Name);

                    $("#spnMember_Father_Name").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMembership_No").append(data.Membership_No);

                    $("#spnAddress").append(data.FATHER_HUSB_NAME_TX);
                    $("#spnMobile").append(data.MOBILE_NO_TX);
                    $("#spnEmail").append(data.EMAIL_ID_TX);
                    $("#spnWebsite").append(data.WEBSITE_TX);

                    if (data.ECSIN_NOT_APPLICABLE_YN === 1)
                        $("#chkNot_Applicable").prop("checked", true);

                    if (data.lst_Area_Of_Practice.length > 0) {
                        BindDropdown(data.lst_Area_Of_Practice, "ddlArea_Of_Practice");
                    }

                    //Details of Orientation Programme
                    if (data.lst_Orientation_Details != null) {
                        if (data.lst_Orientation_Details.length > 0) {
                            var html = '';
                            for (var i = 0; i < data.lst_Orientation_Details.length; i++) {
                                html = html + '<tr>'
                                    + '<td> ' + data.lst_Orientation_Details[i].ORGANIZED_BY + ' </td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].CERTIFICATE_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].FROM_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].TO_DT + '</td>'
                                    + '<td> ' + data.lst_Orientation_Details[i].DURATION_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblOrientation_Details tbody").append(html);
                        }
                    }

                    //Details of last employee details
                    if (data.lst_Last_Employee_Details != null) {
                        if (data.lst_Last_Employee_Details.length > 0) {
                            var le_html = '';
                            for (var j = 0; j < data.lst_Last_Employee_Details.length; j++) {
                                le_html = le_html + '<tr>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].COMPNAY_NAME_TX + ' </td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DIR_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].ECSIN_NO_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DESIGNATION_TX + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].JOINING_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].LAST_END_DT + '</td>'
                                    + '<td> ' + data.lst_Last_Employee_Details[j].DURATION_OF_EMPLOYEEMENT_TX + '</td>'
                                    + '</tr>';
                            }
                            $("#tblLast_Employment_Details tbody").append(le_html);
                        }
                    }

                    //Details of document details
                    if (data.lst_Document_Details.length > 0) {
                        var doc_html = '';
                        for (var k = 0; k < data.lst_Document_Details.length; k++) {
                            var fullPath = "";
                            fullPath = "../Home/PreviewFileByIDFromSpecificTable?id=" + data.lst_Document_Details[k].ID + "&TableName=MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T&ColumnName=FILE_PATH";
                            var docStatus = "";
                            var tdDocSTatus = "";
                            if (data.lst_Document_Details[k].Approved_Id === 0) {
                                tdDocSTatus = '<td><select name="" value="" class="form-control" id="ddlDocument_Status_' + data.lst_Document_Details[k].ID + '">'
                                    + '<option value = "3"> Approve </option ><option value="4"> Call For </option><option value="5"> Reject </option>'
                            }
                            else if (data.lst_Document_Details[k].Approved_Id === 3) {
                                tdDocSTatus = '<td><select name="" value="" class="form-control" id="ddlDocument_Status_' + data.lst_Document_Details[k].ID + '">'
                                    + '<option value = "3" selected> Approve </option ><option value="4"> Call For </option><option value="5"> Reject </option>'
                            }
                            else if (data.lst_Document_Details[k].Approved_Id === 4) {
                                tdDocSTatus = '<td><select name="" value="" class="form-control" id="ddlDocument_Status_' + data.lst_Document_Details[k].ID + '">'
                                    + '<option value = "3"> Approve </option ><option value="4" selected> Call For </option><option value="5"> Reject </option>'
                            }
                            else if (data.lst_Document_Details[k].Approved_Id === 5) {
                                tdDocSTatus = '<td><select name="" value="" class="form-control" id="ddlDocument_Status_' + data.lst_Document_Details[k].ID + '">'
                                    + '<option value = "3"> Approve </option ><option value="4"> Call For </option><option value="5" selected> Reject </option>'
                            }

                            doc_html = doc_html + '<tr>'
                                + '<td  style="display:none">' + data.lst_Document_Details[k].ID + '</td>'
                                + '<td> ' + data.lst_Document_Details[k].DOCUMENT_TYPE_TX + '</td>'
                                + '<td> ' + data.lst_Document_Details[k].UPLODED + '</td>'
                                + '<td><a target="_blank" href=' + fullPath + '><button type="button" class="btn btn - primary btn - xs">Preview</button></a></td>'
                                + tdDocSTatus
                                //+ '<td><select name="" value="" class="form-control" id="ddlDocument_Status_' + data.lst_Document_Details[k].ID + '">'
                                //+ '<option value = "3"> Approve </option ><option value="4"> Call For </option><option value="5"> Reject </option>'

                                + '</select ></td>'
                                + '</tr>';

                            //$("#ddlDocument_Status_" + data.lst_Document_Details[k].ID).val(data.lst_Document_Details[k].Approved_Id);
                        }
                        $("#tblDocument_Details tbody").append(doc_html);


                    }

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_Admin_COP_Personal_Details_Issue_View() {

    var arr_Details = [];
    $('#tblDocument_Details tbody tr').each(function () {
        var details = {};
        var Id = $(this).find("td").eq(0).html();
        details["User_Id"] = $("#hidUI").val();
        details["Id"] = Id;
        details["Approved_Id"] = $("#ddlDocument_Status_" + Id).val();

        arr_Details.push(details);
    });

    $.ajax({
        type: "GET",
        url: "../COP/Insert_Admin_COP_Personal_Details_Issue_View",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            Document_Details: JSON.stringify(arr_Details)
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                //alert("Details inserted successfully");                
                screenAction(682, $("#hidUI").val());
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

//.............
function Admin_COP_Particulars_View_Load() {

    $.ajax({
        type: "GET",
        url: "../COP/Admin_COP_Personal_Details_Issue_View_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#hidUI").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {

                    $("#txtOther_Service").val(data.Other_Service);

                    if (data.lst_Area_Of_Practice.length > 0) {
                        BindDropdown(data.lst_Area_Of_Practice, "ddlArea_Of_Practice");
                    }

                    if (data.Area_Of_Practice_Id !== null) {
                        var selectedOptions = data.Area_Of_Practice_Id.split(",");
                        for (var m in selectedOptions) {
                            var optionVal = selectedOptions[m];
                            $("#ddlArea_Of_Practice").find("option[value=" + optionVal + "]").prop("selected", "selected");
                        }
                        // $("ddlArea_Of_Practice").multiselect('reload');
                    }


                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Admin_COP_Other_Institution_View_Load() {
    $("#tblFirm_Registration_Details1 tbody").html('');

    $.ajax({
        type: "GET",
        url: "../COP/Admin_COP_Personal_Details_Issue_View_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#hidUI").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {

                    // Other institutions
                    if (data.lst_Other_Institutions.length > 0) {
                        $("#IOID_ICAI_Membership_No").val(data.lst_Other_Institutions[0].ICAI_MEMBERSHIP_NO_TX);
                        $("#IOID_ICAI_Cost_Membership_No").val(data.lst_Other_Institutions[0].COST_MEMBERSHIP_NO_TX);
                        $("#IOID_Bar_Council_Membership_No").val(data.lst_Other_Institutions[0].BAR_COUNCIL_NO_TX);
                        $("#IOID_Professional_Membership_No").val(data.lst_Other_Institutions[0].PROF_INSTITUTE_NO_TX);

                        if (data.lst_Other_Institutions[0].IS_ICAI_MEMBER !== null) {
                            $("input[name='IOID_ICAI_Member_radio1'][value='" + data.lst_Other_Institutions[0].IS_ICAI_MEMBER + "']").prop('checked', true);
                        }
                        if (data.lst_Other_Institutions[0].IS_ICAI_MEMBER_HC !== null) {
                            $("input[name='IOID_ICAI_Member_radio2'][value='" + data.lst_Other_Institutions[0].IS_ICAI_MEMBER_HC + "']").prop('checked', true);
                        }

                        if (data.lst_Other_Institutions[0].IS_COST_MEMBERSHIP !== null) {
                            $("input[name='IOID_ICAI_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_COST_MEMBERSHIP + "']").prop('checked', true);
                        }
                        if (data.lst_Other_Institutions[0].COST_MEMBERSHIP_HC !== null) {
                            $("input[name='IOID_ICAI_Membership_radio2'][value='" + data.lst_Other_Institutions[0].COST_MEMBERSHIP_HC + "']").prop('checked', true);
                        }

                        if (data.lst_Other_Institutions[0].IS_BAR_COUNCIL !== null) {
                            $("input[name='IOID_Bar_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_BAR_COUNCIL + "']").prop('checked', true);
                        }
                        if (data.lst_Other_Institutions[0].IS_BAR_COUNCIL_HC !== null) {
                            $("input[name='IOID_Bar_Membership_radio2'][value='" + data.lst_Other_Institutions[0].IS_BAR_COUNCIL_HC + "']").prop('checked', true);
                        }

                        if (data.lst_Other_Institutions[0].IS_PROF_INSTITUTE !== null) {
                            $("input[name='IOID_Prof_Membership_radio1'][value='" + data.lst_Other_Institutions[0].IS_PROF_INSTITUTE + "']").prop('checked', true);
                        }
                        if (data.lst_Other_Institutions[0].IS_PROF_INSTITUTE_HC !== null) {
                            $("input[name='IOID_Prof_Membership_radio2'][value='" + data.lst_Other_Institutions[0].IS_PROF_INSTITUTE_HC + "']").prop('checked', true);
                        }
                    }

                    //Firm details
                    if (data.lst_Firm_Details.length > 0) {
                        var fi_html = '';
                        for (var n = 0; n < data.lst_Firm_Details.length; n++) {
                            fi_html = fi_html + '<tr>'
                                + '<td> ' + data.lst_Firm_Details[n].FIRM_TYPE_TX + ' </td>'
                                + '<td> ' + data.lst_Firm_Details[n].FIRM_NAME_TX + '</td>'
                                + '<td> </td>'
                                + '<td> ' + data.lst_Firm_Details[n].BUSINESS_NATURE_TX + '</td>'
                                + '<td> ' + data.lst_Firm_Details[n].NAME_OF_PARTNERS_TX + '</td>'
                                + '<td> ' + data.lst_Firm_Details[n].OTHER_DETAILS_TX + '</td>'
                                + '</tr>';
                        }
                        $("#tblFirm_Registration_Details1 tbody").append(fi_html);
                    }

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Admin_COP_Declaration_Issue_View_Load() {
    $("#tblRequest_Details tbody").html('');
    $("#tblPayment_Details tbody").html('');
    $("#tblEmail_Details tbody").html('');
    $("#tblSMS_Details tbody").html('');
    $("#tblForward_Details tbody").html('');

    $.ajax({
        type: "GET",
        url: "../COP/Admin_COP_Declaration_Issue_View_Load",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            User_Id: $("#hidUI").val()
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data !== null) {
                data = $.parseJSON(data);

                if (data !== null) {

                    $("#spnMember_Name").append(data.Member_Name);
                    $("#spnMember_Name1").append(data.Member_Name);
                    $("#hdn_COP_Details_Id").val(data.COP_Details_Id);
                    $("#hdn_APPROVAL_LEVEL_NO").val(data.APPROVAL_LEVEL_NO);

                    $("#hdn_RES_STATE_ID").val(data.RES_STATE_ID);
                    $("#hdn_MOBILE_NO_TX").val(data.MOBILE_NO_TX);
                    $("#hdn_EMAIL_ID_TX").val(data.EMAIL_ID_TX);

                    var optionhtml = '';
                    //if (data.APPROVAL_LEVEL_NO === 0) {
                    //    optionhtml = '<option value="31"> Dealing Assistant(DA) </option >';
                    //}
                    if (data.APPROVAL_LEVEL_NO === 0) {
                        optionhtml = '<option value="18">JD/DD/AD</option >';
                        $("#div_HOD_Approval").hide();
                        $("#div_Low_Approval").show();
                    }
                    else if (data.APPROVAL_LEVEL_NO === 1) {
                        optionhtml = '<option value="17">HOD</option >';
                        $("#div_HOD_Approval").hide();
                        $("#div_Low_Approval").show();
                    }
                    else if (data.APPROVAL_LEVEL_NO === 2) {
                        $("#div_HOD_Approval").show();
                        $("#div_Low_Approval").hide();
                    }

                    $("#ddlForward_To").append(optionhtml);

                    //if (data.lst_Area_Of_Practice.length > 0) {
                    //    BindDropdown(data.lst_Area_Of_Practice, "ddlArea_Of_Practice");
                    //}

                    //Details of Orientation Programme
                    if (data.lst_COP_Details_Details.length > 0) {
                        var html = '';
                        var count = 0;
                        for (var i = 0; i < data.lst_COP_Details_Details.length; i++) {
                            count = count + 1;
                            html = html + '<tr>'
                                + '<td> ' + count + '</td>'
                                + '<td> ' + data.lst_COP_Details_Details[i].Request_Type + ' </td>'
                                + '<td> ' + data.lst_COP_Details_Details[i].MemberShip_Number + '</td>'
                                + '<td> ' + data.lst_COP_Details_Details[i].Duration + '</td>'
                                + '<td> ' + data.lst_COP_Details_Details[i].Request_Date + '</td>'
                                + '<td> ' + data.lst_COP_Details_Details[i].Application_Status + '</td>'
                                + '</tr>';
                        }
                        $("#tblRequest_Details tbody").append(html);
                    }


                    if (data.lst_Document_Details.length > 0) {
                        for (var k = 0; k < data.lst_Document_Details.length; k++) {
                            //if (data.lst_Document_Details[k].Approved_Id === 4 || data.lst_Document_Details[k].Approved_Id === 5) {
                            if (data.lst_Document_Details[k].Approved_Id === 4) {
                                $("#hdnIs_Document_CallFor").val(1);
                                break;
                            }
                        }
                    }

                    //Forward history
                    if (data.lst_COP_Forward_Detail !== null) {
                        if (data.lst_COP_Forward_Detail.length > 0) {
                            var html1 = '';
                            for (var j = 0; j < data.lst_COP_Forward_Detail.length; j++) {
                                var action = '';
                                if (data.lst_COP_Forward_Detail[j].APPLICATION_STATUS_ID === 3)
                                    action = "Approved";
                                else if (data.lst_COP_Forward_Detail[j].APPLICATION_STATUS_ID === 4)
                                    action = "Call For";
                                else if (data.lst_COP_Forward_Detail[j].APPLICATION_STATUS_ID === 5)
                                    action = "Rejected";

                                html1 = html1 + '<tr>'
                                    + '<td> ' + data.lst_COP_Forward_Detail[j].Forwarded_By + ' </td>'
                                    + '<td> ' + data.lst_COP_Forward_Detail[j].Forwarded_To + '</td>'
                                    + '<td> ' + data.lst_COP_Forward_Detail[j].Forwarded_Date + '</td>'
                                    + '<td> ' + data.lst_COP_Forward_Detail[j].Remarks + '</td>'
                                    + '<td> ' + action + '</td>'
                                    + '</tr>';
                            }
                            $("#tblForward_Details tbody").append(html1);

                            var rLength = data.lst_COP_Forward_Detail.length - 1;
                            $("#hdn_Logged_Role_Id").val(data.lst_COP_Forward_Detail[rLength].NEXT_ROLE_ID);
                        }
                        else {
                            $("#hdn_Logged_Role_Id").val(16);
                        }
                    }
                    else {
                        $("#hdn_Logged_Role_Id").val(16);
                    }

                }
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}

function Insert_Admin_COP_Approval_Details() {

    var radio = '';
    if ($("#hdn_APPROVAL_LEVEL_NO").val() === "0" || $("#hdn_APPROVAL_LEVEL_NO").val() === "1") {
        radio = $("input[name='radio1']:checked").val();
    }
    else if ($("#hdn_APPROVAL_LEVEL_NO").val() === "2") {
        radio = $("input[name='radio_HOD']:checked").val();
    }

    if (radio === null || radio === undefined) {
        alert('Please select approval recomendation.');
        return false;
    }

    if ($("#hdnIs_Document_CallFor").val() === "1" && radio === "3") {
        alert('Some documents are call for or rejected. Please select call for or reject');
        return false;
    }

    if (($("#hdnIs_Document_CallFor").val() === "0" || $("#hdnIs_Document_CallFor").val() === "") && radio === "4") {
        alert('There are no call for documents.');
        return false;
    }

    var details = {};
    //var Id = $(this).find("td").eq(0).html();
    //var Id = $("#hdn_COP_Details_Id").val();
    //details["COP_DETAILS_ID"] = Id;
    details["USER_ID"] = $("#hidUI").val();
    details["APPLICATION_STATUS_ID"] = radio;

    if ($("#hdn_APPROVAL_LEVEL_NO").val() === "0") {
        details["ROLE_ID"] = 16;
        details["ROLE_NAME"] = 'COP Dealing Assistant(DA)';
    }
    else if ($("#hdn_APPROVAL_LEVEL_NO").val() === "1") {
        details["ROLE_ID"] = 18;
        details["ROLE_NAME"] = 'COP JD/DD/AD';
    }
    else if ($("#hdn_APPROVAL_LEVEL_NO").val() === "2") {
        details["ROLE_ID"] = 17;
        details["ROLE_NAME"] = 'COP HOD';
    }

    if ($("#hdnIs_Document_CallFor").val() === "1" && radio === "4") {
        if ($("#hdn_Logged_Role_Id").val() === "16") {
            details["NEXT_ROLE_ID"] = 20;
            details["NEXT_ROLE_NAME"] = 'Member';
        }
        else if ($("#hdn_Logged_Role_Id").val() === "18" || $("#hdn_Logged_Role_Id").val() === "17") {
            details["NEXT_ROLE_ID"] = 16;
            details["NEXT_ROLE_NAME"] = 'COP Dealing Assistant(DA)';
        }
    }
    else {
        if ($("#hdn_APPROVAL_LEVEL_NO").val() === "2") {
            details["NEXT_ROLE_ID"] = 20;
            details["NEXT_ROLE_NAME"] = 'Member';
        }
        else {
            details["NEXT_ROLE_ID"] = $("#ddlForward_To").val();
            details["NEXT_ROLE_NAME"] = $("#ddlForward_To").text();
        }
    }

    details["INTERNAL_REMARKS"] = $("#txtInternal_Remarks").val();
    details["REMARKS_FOR_MEMBER"] = $("#txtMember_Remarks").val();

    details["SCREEN_ID"] = $("#screenid").val();
    details["RES_STATE_ID"] = $("#hdn_RES_STATE_ID").val();
    details["MOBILE_NO_TX"] = $("#hdn_MOBILE_NO_TX").val();
    details["EMAIL_ID_TX"] = $("#hdn_EMAIL_ID_TX").val();

    $.ajax({
        type: "GET",
        url: "../COP/Insert_Admin_COP_Approval_Details",
        contentType: 'application/json;charset=utf-8',
        //processData: false,
        data: {
            Approve_Details: JSON.stringify(details)
        },
        success: function (data) {
            if (data === "ERROR") {
                alert('Please try again.');
            }
            else if (data === "SESSION") {
                window.location.href = "../Home/Index";
            }
            else if (data === "SUCCESS") {
                alert("Details inserted successfully");
                screenAction(673, $("#hidUI").val());
            }
        },
        error: function (e, x) {
            alert('ErrorMessage', e.responseText);
        }
    });

}



function Admin_Search_ExportToExcel() {

    if ($("#txtACRS_Name").val().trim() === "" && $("#txtACRS_Request_From").val().trim() === "" && $("#txtACRS_Request_To").val().trim() === ""
        && $("#txtACRS_Email_Id").val().trim() === "" && $("#txtACRS_Admission_From").val().trim() === "" && $("#txtACRS_Admission_To").val().trim() === ""
        && $("#txtACRS_MemberShip_Number").val().trim() === "" && $("#txtACRS_MobileNo").val().trim() === "" && $("#txtACRS_COP_Number").val().trim() === ""
        && $("#ddlACRS_Application_Status").val() === "0" && $("#ddlACRS_Request_Type").val() === "0" && $("#ddlACRS_Internal_Dept_Status").val() === "0") {
        alert("Please enter or select any one search criteria");
        return false;
    }
    else if ($("#txtACRS_Request_From").val().trim() !== "" && $("#txtACRS_Request_To").val().trim() === "") {
        alert("Please enter request to date");
        return false;
    }
    else if ($("#txtACRS_Request_From").val().trim() === "" && $("#txtACRS_Request_To").val().trim() !== "") {
        alert("Please enter request from date");
        return false;
    }
    else if ($("#txtACRS_Admission_From").val().trim() !== "" && $("#txtACRS_Admission_To").val().trim() === "") {
        alert("Please enter admission to date");
        return false;
    }
    else if ($("#txtACRS_Admission_From").val().trim() === "" && $("#txtACRS_Admission_To").val().trim() !== "") {
        alert("Please enter admission from date");
        return false;
    }

    if ($("#txtACRS_Email_Id").val().trim() !== "") {
        var isEmail = IsEmail($("#txtACRS_Email_Id").val().trim());
        if (!isEmail) {
            bootbox.alert("Invalid email format.");
            return false;
        }
    }

    window.location.href = "../COP/Admin_Search_ExportToExcel?" + $.param({
        Name: $("#txtACRS_Name").val().trim(), Request_From: $("#txtACRS_Request_From").val().trim(),
        Request_To: $("#txtACRS_Request_To").val().trim(), Email_Id: $("#txtACRS_Email_Id").val().trim(),
        Admission_From: $("#txtACRS_Admission_From").val().trim(), Admission_To: $("#txtACRS_Admission_To").val().trim(),
        MemberShip_Number: $("#txtACRS_MemberShip_Number").val().trim(), MobileNo: $("#txtACRS_MobileNo").val().trim(),
        COP_Number: $("#txtACRS_COP_Number").val().trim(), Application_Status_Id: $("#ddlACRS_Application_Status").val(),
        Request_Type_Id: $("#ddlACRS_Request_Type").val(), Internal_Dept_Status_Id: $("#ddlACRS_Internal_Dept_Status").val()
    });

}

function Admin_COP_Action(Screen_Id) {
    screenAction(Screen_Id, $("#hidUI").val());
}


function Get_COP_Issue_Payment_Details() {
    var formdata_FORM_D = new FormData();
    formdata_FORM_D.append("MEMBERSHIP_NO", $("#userid_tx").val());
    formdata_FORM_D.append("Comp_ID", 296);
    $.ajax({
        type: "POST",
        url: "AjaxData",
        dataType: "json",
        cache: false,
        contentType: false,
        processData: false,
        async: false,
        data: formdata_FORM_D,
        success: function (res) {
            var obj = jQuery.parseJSON(res);
            if (obj.length > 0) {
                var frm = document.createElement("form");
                frm.method = "post";
                frm.action = "../Home/Home";
                var u = document.getElementById("u").value;
                frm.appendChild(createInputHid("u", u));
                frm.appendChild(createInputHid("si", 699));
                frm.appendChild(createInputHid("s", "pg"));
                frm.appendChild(createInputHid("pt", "initiate"));
                frm.appendChild(createInputHid("userid", u));
                frm.appendChild(createInputHid("amt", obj[0].TOTAL_FEE));
                frm.appendChild(createInputHid("offid", 74));
                frm.appendChild(createInputHid("desc", ""));
                frm.appendChild(createInputHid("custname", obj[0].MEMBER_NAME_TX));
                frm.appendChild(createInputHid("email", obj[0].EMAIL_ID_TX));
                frm.appendChild(createInputHid("mob", obj[0].MOBILE_NO_TX));
                frm.appendChild(createInputHid("billing", obj[0].CORR_ADDRESS_TX));
                frm.appendChild(createInputHid("shipping", obj[0].CORR_ADDRESS_TX));
                frm.appendChild(createInputHid("taxamt", obj[0].GST_AMOUNT));
                ////frm.appendChild(createInputHid("gstnm", obj[0].GST_NO_TX));
                if (obj[0].GST_AMOUNT > 0)
                    frm.appendChild(createInputHid("gstnm", 1));
                else
                    frm.appendChild(createInputHid("gstnm", 0));

                frm.appendChild(createInputHid("remarks", ""));
                frm.appendChild(createInputHid("stdregno", obj[0].MEMBERSHIP_NO_TX));
                frm.appendChild(createInputHid("hidUI", obj[0].HIDUI));
                frm.appendChild(createInputHid("addinfo1", obj[0].GST_NO_TX));
                frm.appendChild(createInputHid("stategstcd", obj[0].STATE_GST_CD_TX));
                frm.appendChild(createInputHid("recTmplId", 5));
                var itmstr = "{\"items\":[";
                var itms = "";
                for (var i = 0; i < obj.length; i++) {
                    var itemId = obj[i].ITEM_CODE;
                    var Fee_Amt = obj[i].FEE_HEAD_AMOUNT;
                    var Fee_GST_Amt = obj[i].FEE_HEAD_GST_AMOUNT;
                    var itemdtl = "{\"item\":" + itemId + ",\"itemamt\":" + Fee_Amt + "}";
                    if (itms.length > 0) {
                        itms = itms + "," + itemdtl;
                    }
                    else {
                        itms = itms + itemdtl;
                    }
                }
                itmstr = itmstr + itms + "],\"total\":" + obj[0].TOTAL_FEE + "}";
                frm.appendChild(createInputHid("items", itmstr));
            }

            document.body.appendChild(frm);
            frm.submit();
        }
    });
}



