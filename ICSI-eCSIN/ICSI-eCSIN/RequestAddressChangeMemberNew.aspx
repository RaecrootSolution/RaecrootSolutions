<%@ Page Language="C#" AutoEventWireup="true" 
    Inherits="StudentMemberPages_RequestAddressChangeMemberNew" Codebehind="RequestAddressChangeMemberNew.aspx.cs" %>

<form runat="server">

<script type = "text/javascript" >
   function preventBack(){window.history.forward();}
    setTimeout("preventBack()", 0);
    window.onunload=function(){null};
</script>

<script type="text/javascript">
function txtVisible()
{
    var desigid = '<%=ddl_Desig.ClientID%>';
    var dobid = document.getElementById(desigid);
    var dateval = dobid.value;
    
    if(dateval =="select Not Available") 
    { 
        
      document.getElementById('<%=txtBoxNew_desig.ClientID%>').style.display="";  
      return false;
    } 
    else 
    {
    
    document.getElementById('<%=txtBoxNew_desig.ClientID%>').style.display="none";
    txtBoxNew_desig.focus();
    return true;
    }
  
}
    </script>

     
 <head runat="server">
   <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Change Address of Membership Record</title>

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.0/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.1/css/all.css"
          integrity="sha384-50oBUHEmvpQ+1lW4y57PTFmhCaXp0ML5d60M1M7uH2+nqUivzIebhndOJK28anvf"
          crossorigin="anonymous">
    <script src="https://cdn.rawgit.com/atatanasov/gijgo/master/dist/combined/js/gijgo.min.js" type="text/javascript">
    </script>
    <script src="https://cdn.rawgit.com/atatanasov/gijgo/master/dist/combined/js/gijgo.min.js" type="text/javascript"></script>
    <link href="https://cdn.rawgit.com/atatanasov/gijgo/master/dist/combined/css/gijgo.min.css" rel="stylesheet" type="text/css" />

    <script src="https://unpkg.com/gijgo@1.9.13/js/gijgo.min.js" type="text/javascript"></script>
    <link href="https://unpkg.com/gijgo@1.9.13/css/gijgo.min.css" rel="stylesheet" type="text/css" />
    <link href="~/Content/style.css" rel="stylesheet" />
    <link href="~/Content/themes/base/base.css" rel="stylesheet" />

    <!--@Scripts.Render("~/bundles/jquery")
   @Styles.Render("~/Content/style.css")
    @Styles.Render("~/Content/themes/base/base.css")-->

</head>

 <section id="Icsi_top_header">
        <div class="container-fluid">
            <div class="Header">
                <div class="LogoSpacing">
                    <div class="header-part">
                        <ul class="list-inline">
                            <li class="header_logo_li_1">
                                <div class="" id="mainLogo">
                                    <a href="https://www.icsi.edu/home" target="_blank">
                                        <img src="Images/logo_full.png" alt="logo" class="img-responsive" />
                                    </a>
                                </div>
                            </li>
                            <li class="header_logo_li_2">
                                <div>
                                    <div class="helplineNum">
                                        <ul class="list-inline">

                                           <%  if (Session["TotaleCSIN"] != null && Session["TotalUsers"] != null)

                                            {%>

                                              <%--  <li class="Registered">Users Registered: @Session["TotalUsers"]</li>

                                                <li class="Generate">Generated: @Session["TotaleCSIN"]</li>--%>

                                           <% }%>


                                        </ul>
                                    </div>
                                </div>

                                <div class="ClickHerCentertxt text-center">
                                    For Any Query : <a href="mailto:ecsin@icsi.edu">ecsin@icsi.edu</a>
                                </div>

                                <div class="ClickHerCentertxt">
                                    <h3>To change / update the email and mobile number, please <span><a href="https://www.icsi.in/student/Home/Login/tabid/140/Default.aspx?returnurl=%2fstudent%2fdefault.aspx" target="_blank">Click Here </a></span></h3>
                                    <% if (Session["UserName"] != null)
                                    { %>

                                   <%  } %>


                                </div>
                            </li>
                        </ul>

                    </div>
                <div class="menuBar">
                        <nav class="navbar navbar-expand-md bg-dark navbar-dark justify-content-center">

                            <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#collapsibleNavbar">
                                <span class="navbar-toggler-icon"></span>
                            </button>
                            <div class="collapse navbar-collapse" id="collapsibleNavbar">
                                <ul class="navbar-nav">
                                    <li class="nav-item" >
                                        <a class="nav-link" href="https://www.icsi.edu" target="_blank">ICSI Home</a>
                                    </li>

                                    <li class="dropdown dropdown1" style="margin-top:-5px;">
                                        <a href="#" class="dropdown-toggle" data-toggle="dropdown">Generate eCSIN</a>

                                        <ul class="dropdown-menu" style="background-color:#2D4383; margin-left:initial;">
                                            <a class="dropdown-item" href="Home/eCSINGeneration">Generate eCSIN</a>

                                            <%if (Session["checkExistUserSub_Menu"] != null && Convert.ToBoolean(Session["checkExistUserSub_Menu"]) == true)
                                                {%>
                                            <a class="dropdown-item" href="Home/SubeCSINGeneration">Generate Subsidiary eCSIN</a>
                                            <% }%>
                                        </ul>
                                    </li>

                                
                                    <li >
                                        <a class="nav-link" href="Home/SearcheCSINI">Search eCSIN</a>
                                    </li>

                                    <li  class="nav-item">
                                        <a class="nav-link" href="Home/eCSINListM">Register of eCSIN Generated</a>
                                    </li>

                                    <li class="dropdown dropdown1" style="margin-top:-5px;">
                                        <a href="#" class="dropdown-toggle" data-toggle="dropdown"> Update eCSIN </a>
                                        <ul class="dropdown-menu" style="background-color:#2D4383; margin-left:initial;">
                                            <a class="dropdown-item" href="Home/editeCSINGeneration">Update Designation</a>
                                            <a class="dropdown-item" href="Home/SearcheCSINUpdate">Update Details</a>
                                            <a class="dropdown-item" href="Home/ChangePassword">Change Password</a>
                                            <a class="dropdown-item" href="RequestAddressChangeMemberNew.aspx">Update Address in Membership Record</a>
                                        </ul>
                                    </li>

                                    <li class="nav-item" style="margin-left:10px;">
                                        <a class="nav-link" href="Home/Index">LogOut</a>
                                    </li>

                                </ul>
                           
                                <ul class="navbar-nav" style="float:right;">

                                    <li style="color:#4cff00;">
                                      <asp:Label ID="labelWelcome" runat="server" class="navbar-nav"></asp:Label>
                                    </li>
                                </ul>

</div>
                        </nav>
                    </div>
                </div>
            </div>
        </div>
    </section>

 <section id="Ca_content_body" style="width:1000px; margin:0 auto;">
        <div class="my-1">
    <p class="p-1 text-center" style="background:#ccc;">
        <asp:Label ID="Label2" runat="server" CssClass="requestlabel" Text="Request for Change Address"></asp:Label>

    </p>
              

</div>

        <div class="container" style="border:1px solid rgba(0,0,0,.125); padding-bottom:10px;">
         <%--   <table border="0" cellpadding="0" cellspacing="0" style="width: 100%; height: 27px;
        vertical-align: top;" class="requestheader">
        <tr>
            <td align="center">
                <asp:Label ID="Label2" runat="server" CssClass="requestlabel" Text="Request for Change of Address"></asp:Label></td>
        </tr>


    </table>
    <br />--%>
    <table>
        <tr>
            <td align="center" style="height: 15px">
                <asp:Label ID="lbl_add" runat="server" Font-Size="Medium" Visible="false" Text="This link is temporarily disabled. It will be activated shortly !"
                    Font-Bold="True" Font-Italic="True" ForeColor="#FF0000"></asp:Label></td>
        </tr>
    </table>
    <table style="width: 784px; height: 25px">
         <asp:Label ID="lblStatus" runat="server" Font-Size="Medium" ForeColor="Red" Font-Bold="True"
                    Font-Names="verdana"></asp:Label>
        <!-- 				<tr align="center">
					<td>
						<asp:Button id="btnLogin" runat="server" Visible="False" Width="80px" Text="Login" CssClass="Button"></asp:Button>
					</td>
					<td>
						<asp:Button id="btnLogout" runat="server" Text="Logout" Width="80px" CssClass="Button"></asp:Button>
					</td>
				</tr> -->
      <%-- <tr align="center">
            <td style="height: 0px">
                <asp:Label ID="lblStatus" runat="server" Font-Size="Medium" ForeColor="Red" Font-Bold="True"
                    Font-Names="verdana"></asp:Label></td>
        </tr>--%>
    </table>
    <table width="100%" align="center">
        <tr align="center">
            <td>
                <asp:Panel ID="Panel2" runat="server" HorizontalAlign="Center" Width="100%">
                    <table id="Table3" cellspacing="0" width="100%" align="center">
                        <tr>
                            <td align="center" style="height: 15px">
                                <asp:Label ID="Label1" runat="server" Font-Names="verdana" Font-Bold="True" BackColor="ControlLight">
                                    <h3 class="formgheader">SELECT THE ADDERESS TO BE CHANGED</h3></asp:Label></td>
                        </tr>
                       
                        <tr>
                            <td  style="height: 86px; margin-top:initial">
                                <blockquote>
                                    <ul class="RegistrnFormPara">
                                        <li><font color="#000099">Select the Appropriate&nbsp;Address Option , then Click on
                                            Go... button</font></li>
                                            <li><font color="#000099">Please do not type single quotes ( ' ) and double quotes (
                                                " ) in text boxes </font></li>
                                            <li><font color="#000099">Please note that chapter code will be determined based on
                                                the professional address only. </font></li>
                                            <li><font color="#000099">If the professional address is not available then the chapter
                                                code will be determined based on the Residential address.</font> </li>
                                    </ul>
                                </blockquote>
                            </td>
                        </tr>
                        <tr>
                            <td align="center" width="90%">
                                <asp:RadioButton ID="RadioButton1" runat="server" Font-Names="verdana" Font-Bold="True"
                                    Font-Size="Smaller" Text="Professional" GroupName="addtype" OnCheckedChanged="RadioButton1_CheckedChanged">
                                </asp:RadioButton>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:RadioButton ID="RadioButton2" runat="server" Font-Names="verdana" Font-Bold="True"
                                    Font-Size="Smaller" Text="Residential" GroupName="addtype" OnCheckedChanged="RadioButton2_CheckedChanged">
                                </asp:RadioButton>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="btnGo" runat="server" Text="Go..." CssClass="Button" OnClick="btnGo_Click">
                                </asp:Button></td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel ID="Panel1" runat="server" HorizontalAlign="Center" Width="100%" Visible="false">                
                  <table id="Table2" cellspacing="0" cellpadding="0" width="100%" border="1" runat="server">
                        <tr bgcolor="lightgrey" height="30">
                            <td width="15%">
                            </td>
                            <td align="center" width="30%">
                                <font face="Verdana" size="2"><strong>Existing Details as per records</strong></font></td>
                            <td align="center" style="width: 534px">
                                <font face="Verdana" size="2"><strong>Enter Changed Details</strong></font></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Label ID="Label7" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="Smaller">Region Code : &nbsp;</asp:Label></td>
                            <td align="left">
                                <asp:Label ID="lblRegion" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:DropDownList ID="ddl_Region" AutoPostBack="true" runat="server" Width="104px"
                                    CssClass="DropDown" OnSelectedIndexChanged="ddl_Region_SelectedIndexChanged" style="top:0px; margin-bottom:0px; border:1px solid #ccc">
                                    <asp:ListItem></asp:ListItem>
                                    <asp:ListItem Value="EIRC">EIRC</asp:ListItem>
                                    <asp:ListItem Value="NIRC">NIRC</asp:ListItem>
                                    <asp:ListItem Value="SIRC">SIRC</asp:ListItem>
                                    <asp:ListItem Value="WIRC">WIRC</asp:ListItem>
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Label ID="Label8" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="Smaller">Chapter : &nbsp;</asp:Label></td>
                            <td align="left">
                                <asp:Label ID="lblChapter" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:DropDownList ID="ddl_Chapter" runat="server" Width="296px" CssClass="DropDown" style="top:0px; margin-bottom:0px; border:1px solid #ccc">
                                </asp:DropDownList></td>
                        </tr>
                        <tr id="panel_Designation" runat="server">
                            <td align="right">
                                <font face="Verdana" size="2"><strong>
                                    <asp:Label ID="Label3" runat="server">Designation : &nbsp;</asp:Label></strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblDesig" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:DropDownList ID="ddl_Desig" runat="server" Width="360px" CssClass="DropDown"
                                    onchange="txtVisible(this);" style="top:0px; margin-bottom:0px; border:1px solid #ccc">
                                </asp:DropDownList><font face="verdana" size="0.5"></br>(Note: If Designation is not available,
                                    please select Not Available Option from the drop downlist)</font>
                                <asp:TextBox ID="txtBoxNew_desig" onkeyup="txtNewdesig()" runat="server" Width="328px"
                                    CssClass="TextBox" MaxLength="55"></asp:TextBox></td>
                        </tr>
                        <tr id="panel_Address" runat="server">
                            <td align="right">
                                <font face="Verdana" size="2"><strong>
                                    <asp:Label ID="Label4" runat="server">Organisation Name : &nbsp;</asp:Label></strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblOrgname" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtOrgname" onkeyup="txtOrg()" runat="server" Width="328px" CssClass="TextBox"
                                    MaxLength="55"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>Address : &nbsp; </strong>
                            </td>
                            <td align="left">
                                <asp:Label ID="lblAdd1" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtAdd1" onkeyup="txtA1()" runat="server" Width="280px" CssClass="TextBox"
                                    MaxLength="29"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                            </td>
                            <td align="left">
                                <asp:Label ID="lblAdd2" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtAdd2" onkeyup="txtA2()" runat="server" Width="280px" CssClass="TextBox"
                                    MaxLength="29"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                            </td>
                            <td align="left">
                                <asp:Label ID="lblAdd3" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtAdd3" onkeyup="txtA3()" runat="server" Width="280px" CssClass="TextBox"
                                    MaxLength="29"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>City : &nbsp;</strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblCity" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtCity" onkeyup="txtCty()" runat="server" Width="240px" CssClass="TextBox"
                                    MaxLength="19" OnTextChanged="txtCity_TextChanged" AutoPostBack="True"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>State : &nbsp;</strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblState" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:DropDownList ID="ddlState" runat="server" Width="264px" CssClass="DropDown" style="top:0px; margin-bottom:0px; border:1px solid #ccc">
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td align="right" style="height: 24px">
                                <font face="Verdana" size="2"><strong>Pin : &nbsp;</strong></font></td>
                            <td style="height: 24px" align="left">
                                <asp:Label ID="lblPin" runat="server" CssClass="Label"></asp:Label></td>
                            <td style="height: 24px; width: 534px;" align="left">
                                <asp:TextBox ID="txtPin" runat="server" Width="120px" CssClass="TextBox" MaxLength="6"
                                    AutoPostBack="false" OnTextChanged="txtPin_TextChanged"></asp:TextBox>
                                <asp:Label ID="lblpinmessage" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>
                                    <asp:Label ID="Label5" runat="server">STD Code : &nbsp;</asp:Label></strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblStd" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtStd" runat="server" Width="176px" CssClass="TextBox" MaxLength="6"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>Phone : &nbsp;</strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblPh1" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtPh1" runat="server" Width="240px" CssClass="TextBox" MaxLength="15"
                                    AutoPostBack="false" OnTextChanged="txtPh1_TextChanged"></asp:TextBox>
                                &nbsp;&nbsp;<asp:Label ID="lblph1message" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                            </td>
                            <td align="left">
                                <asp:Label ID="lblPh2" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtPh2" runat="server" Width="240px" CssClass="TextBox" MaxLength="15"
                                    AutoPostBack="false" OnTextChanged="txtPh2_TextChanged">										
                                </asp:TextBox>
                                &nbsp;&nbsp;<asp:Label ID="lblph2message" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>Fax : &nbsp;</strong></font>
                            </td>
                            <td align="left">
                                <asp:Label ID="lblFax" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtFax" runat="server" Width="240px" CssClass="TextBox" MaxLength="15"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"></font><strong>
                                    <asp:Label ID="Label6" runat="server">Email : &nbsp;</asp:Label></strong></td>
                            <td align="left">
                                <asp:Label ID="lblEmail" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtEmail" runat="server" Width="240px" CssClass="TextBox" MaxLength="99"
                                    AutoPostBack="false" OnTextChanged="txtEmail_TextChanged"></asp:TextBox>
                                <asp:Label ID="lblemailmessage" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>
                                    <asp:Label ID="Label9" runat="server">Website : &nbsp;</asp:Label></strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblWebsite" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtWebsite" runat="server" Width="240px" CssClass="TextBox" MaxLength="99"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <font face="Verdana" size="2"><strong>Mobile No. : &nbsp;</strong></font></td>
                            <td align="left">
                                <asp:Label ID="lblCellno" runat="server" CssClass="Label"></asp:Label></td>
                            <td align="left" style="width: 534px">
                                <asp:TextBox ID="txtCellno" runat="server" Width="240px" CssClass="TextBox" MaxLength="16"
                                    AutoPostBack="false" OnTextChanged="txtCellno_TextChanged"></asp:TextBox>
                                &nbsp;&nbsp;<asp:Label ID="lblcellmessage" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="left" bgcolor="lightgrey" height="35" colspan="3">
                                <asp:CheckBox ID="chkDeclaration" Font-Names="Verdana" Font-Bold="True" Font-Size="Small"
                                    Text="I Confirm that I have Updated the Chapter Code and the Region Code which Coorelates with Updated Address"
                                    runat="server"></asp:CheckBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="center" bgcolor="lightgrey" colspan="3">
                                <asp:Button ID="btnSubmit" runat="server" Width="90px" Text="Submit" CssClass="Button"
                                    OnClick="btnSubmit_Click"></asp:Button>&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="btnReset" runat="server" Width="90px" Text="Reset" CssClass="Button"
                                    CausesValidation="false" OnClick="btnReset_Click"></asp:Button></td>
                        </tr>
                     
                    </table>                  
                </asp:Panel>
            </td>
        </tr>
    </table>
        </div>
    </section>
  
 <section id="ca_footer">

        <div class="container-fluid py-2" style="background:rgba(241, 236, 236, 0.78);">

            <div class="text-center">
                <ul class="listinlineFooter" style="float: none; display: inline-block; margin: 0 auto; text-align: center; width: 100%;">
                    <li>
                    <li style="padding:0 10px;">
                        <a href="https://smash.icsi.in/Scripts/Complaint/ComplaintForm.aspx" target="_blank">Helpdesk</a>
                    </li>
                    <li style="padding:0 10px;">


                        <a href="PDF/UserManual_eCSIN_FAQ.pdf" target="_blank">FAQs</a>
                    </li>
                    <li style="padding:0 10px;">
                        <%--@*<a href="#">Guidelines</a>*@--%>

                        <a href="PDF/eCSIN-Guidlines.pdf" target="_blank">Guidelines</a>
                    </li>
                </ul>
            </div>

        </div>
        <div id="ca_footer_1">
            <div class="container">
                <div class="ca_disclaimer">
                    <h6 class="primary-color">DISCLAIMER</h6>
                    <p>
                        This eCSin System has been developed by ICSI to facilitate its members for verification of their appointment or cessation as the case may be.
                    </p>
                    <p>
                        However, ICSI assumes no responsibility for verification and authenticity of the information provided by the Members and the concerned member(s) shall alone be responsible thereof.
                    </p>

                </div>
            </div>
        </div>
        <div id="ca_footer_2">
            <div class="container">
                <div style="width:100%;">
                   

                    <div class="ca_copy_rights fr" style="float:none; font-style:italic;">
                        <p> Copyright 2019 All rights reserved to the ICSI</p>
                    </div>

                </div>
            </div>
        </div>
    </section> 
    
</form>