using ICSI_WebApp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;
using ICSI_WebApp.Models;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using ICSI_Library.Util;
using System.Net;
using ICSI_Library.Models;
using ICSI_Library.Membership;
using ICSI_WebApp.BusinessLayer;
using Aspose.Cells;
using System.Web;

namespace ICSI_WebApp.Controllers
{
    public class PeerReviewController : Controller
    {
        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private string PGUrl = Convert.ToString(ConfigurationManager.AppSettings["PGUrl"]);
        private string PGAppUrl = Convert.ToString(ConfigurationManager.AppSettings["PGAppUrl"]);
        private string PGHDFCUrl = Convert.ToString(ConfigurationManager.AppSettings["PGHDFCUrl"]);
        private static string ANONYMOUS_PASSWORD = "FA6EC1E225B2420388203C6A67193A5E";
        private ManagementData objApplicationObject;

        [HttpGet]
        public ActionResult login(int UserType = 0)
        {
            return View("PRlogin");
        }

        [HttpPost]
        public ActionResult login(FormCollection obj)
        {
            string UserName = obj["email"];
            string Password = obj["HidPassVal"];
            string OrigPassword = obj["password"];
            string remember = obj["remember"];
            int UserId = 0;

            #region Consume Service
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            AppUrl = AppUrl + "/login";
            string Message = string.Empty;
            string sdata = UtilService.createRequestObject(AppUrl, UserName, Password, UtilService.createParameters("", "", "", "", "", "login", data), out Message);

            try
            {
                if (sdata != null && Message == "Success")
                {
                    JObject jdata = JObject.Parse(sdata);
                    foreach (var item in jdata["data"].First.Children())
                    {
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "LOGIN_ID")
                            Session["LOGIN_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "ID")
                        {
                            UserId = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)item).Value);
                            Session["USER_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_TYPE_ID")
                            Session["USER_TYPE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_NAME_TX")
                            Session["USER_NAME_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "SESSION_KEY")
                            Session["SESSION_KEY"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_ID")
                            Session["USER_ID_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "REGION_ID")
                            Session["REGION_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "OFFICE_ID")
                            Session["OFFICE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                    }
                }
                ViewBag.Message = Message.Replace("_", " ");
                #endregion

                #region Remember Me
                if (remember == "on")
                {
                    //create the authentication ticket
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Values.Add("EmailID", UserName);
                    cookie.Values.Add("Password", OrigPassword);
                    cookie.Expires = DateTime.Now.AddDays(15);
                    Response.Cookies.Add(cookie);
                }
                #endregion

                if (Message == "Success")
                {
                    #region Get User Data
                    UserData userData = new UserData();
                    int status = DBTable.GetUserData(UserId, userData);
                    if (status == 1)
                    {
                        if (HttpContext.Application["ManagementData"] == null)
                            objApplicationObject = new ManagementData();
                        if (Convert.ToInt32(Session["USER_TYPE_ID"]) == 3)
                        {
                            if (userData.STUDENT_T != null && userData.STUDENT_T.Rows != null && userData.STUDENT_T.Rows.Count > 0)
                            {
                                Session["STUDENT_ID"] = Convert.ToString(userData.STUDENT_T.Rows[0]["ID"]);
                                Session["STUDENT_NAME"] = Convert.ToString(userData.STUDENT_T.Rows[0]["STUDENT_NAME_TX"]);
                                Session["STUDENT_HDR_ID"] = Convert.ToString(userData.STUDENT_T.Rows[0]["STUDENT_HDR_ID"]);
                            }
                        }
                        if (Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6)
                        {
                            DataTable dt = new DataTable();
                            Dictionary<string, object> conditions = new Dictionary<string, object>();
                            conditions.Add("ACTIVE_YN", 1);
                            conditions.Add("UNIQUE_REG_ID", Session["LOGIN_ID"].ToString());
                            dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                object[] sessionvalues = new object[2];
                                Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                                sessionvalues[0] = dt.Rows[0]["UNIQUE_REG_ID"].ToString();
                                sessionvalues[1] = dt.Rows[0]["ID"].ToString();
                                if (HttpContext.Session["SESSION_OBJECTS"] != null)
                                {
                                    sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                                    sessionObjs.Remove("PCS_COMPANY_MASTER_DETAIL_T");
                                    sessionObjs.Add("PCS_COMPANY_MASTER_DETAIL_T", sessionvalues);
                                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                                }
                                else
                                {
                                    sessionObjs.Add("PCS_COMPANY_MASTER_DETAIL_T", sessionvalues);
                                    if (dt.Rows[0]["REG_COM_TYPE_ID"].ToString() == "1")
                                    {
                                        sessionObjs.Add("MEMBERSHIP_NUMBER", dt.Rows[0]["MEMBERSHIP_NM"].ToString());
                                    }
                                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                                }

                            }

                        }
                        DataTable resps = UtilService.fetchResponsibilities(userData);
                        if (resps != null && resps.Rows != null && resps.Rows.Count == 0)
                        {
                            return RedirectToAction("logout");
                        }
                        else
                        {
                            List<object> menu = UtilService.getMenu(WEB_APP_ID, resps);
                            Session["USER_MENU"] = menu;
                            Session["USER_DATA"] = userData;
                        }
                    }
                    #endregion

                    return RedirectToAction("Home");
                }
            }
            catch (Exception exx) { }
            return View(obj);
        }

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("login");
        }


        public ActionResult PRRegister(FormCollection frm)
        {
            if (Session["LOGIN_ID"] == null)
            {
                login();
            }

            string userid = frm["u"];
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null)
            {
                if (frm["scrtype"] == "update")
                {
                    ScreenType = "update";
                    frm["s"] = ScreenType;
                }

                ActionClass act = new ActionClass();
                if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                {
                    Screen_T screen = UtilService.homeAction(WEB_APP_ID, frm, userid, menuid, screenId, ScreenType, act, (List<object>)Session["USER_MENU"]);
                    if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                    if (screen != null)
                    {
                        ViewBag.Title = screen.Screen_Title_Tx;
                        ViewBag.MenuId = menuid;
                        ViewBag.ActionClass = act;
                        return View("PRHome", screen);
                    }
                    else return View();
                }
                else return RedirectToAction("Home", "PeerReview");
            }
            else
                return RedirectToAction("logout");
        }

        [HttpGet]
        public ActionResult Home()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 && Convert.ToString(Session["LOGIN_ID"]) != "csranonymous")
            {
                List<object> menu = (List<object>)Session["USER_MENU"];
                DataRow row = (DataRow)menu[0];
                Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, null, null);

                if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                {
                    UtilService.executeMethod(screen.Screen_Class_Name_Tx, null, "before" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, null, screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                }
                screen.resActionClass = (ActionClass)TempData["actionClass"];

                ViewBag.Title = "Home";
                if (screen != null)
                {

                    return View("PRHome", screen);
                }
                else return View("PRHome");
            }
            else
                return RedirectToAction("logout");
        }

        [HttpPost]
        public ActionResult Home(FormCollection frm)
        {

            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 || (Convert.ToString(Session["LOGIN_ID"]) == "pcscomp" || Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6))
            {
                string userid = frm["u"];
                string menuid = frm["m"];
                string ScreenType = frm["s"];
                string screenId = frm["si"];
                string pgtype = frm["pt"];
                if (ScreenType.Equals("pg") && !string.IsNullOrEmpty(pgtype))
                {
                    Screen_T PGST = new Screen_T();
                    if (pgtype == "initiate")
                    {
                        string items = frm["items"];
                        Dictionary<string, object> itemObj = null;
                        double amt = 0;
                        if (items != null)
                        {
                            itemObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(items);
                            foreach (var itm in itemObj)
                            {
                                if (itm.Key.Equals("total")) amt = Convert.ToDouble(itm.Value);
                            }
                            itemObj.Remove("total");
                        }
                        string returnURL = "return loadConfirmScreen()";
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data.Add("ptype", pgtype);
                        data.Add("scid", screenId);
                        data.Add("sid", userid);
                        data.Add("amt", amt);
                        data.Add("officeid", frm["offid"]);
                        data.Add("desc", frm["desc"]);
                        data.Add("items", itemObj);
                        data.Add("custname", frm["custname"]);
                        data.Add("email", frm["email"]);
                        data.Add("mobile", frm["mob"]);
                        data.Add("billing", frm["billing"]);
                        data.Add("shipping", frm["shipping"]);
                        data.Add("taxamt", frm["taxamt"]);
                        data.Add("gstnm", frm["gstnm"]);
                        data.Add("remarks", frm["remarks"]);
                        data.Add("returnurl", returnURL);
                        data.Add("stdregno", frm["stdregno"]);
                        data.Add("session", Convert.ToString(HttpContext.Session["SESSION_KEY"]));
                        data.Add("loginId", Convert.ToString(HttpContext.Session["LOGIN_ID"]));
                        data.Add("refId", frm["hidUI"]);
                        data.Add("addinfo1", frm["addinfo1"]);
                        data.Add("addinfo2", frm["addinfo2"]);
                        data.Add("addinfo3", frm["addinfo3"]);
                        data.Add("stategstcd", frm["stategstcd"]);
                        if (frm["recTmplId"] != null)
                        {
                            data.Add("recTmplId", frm["recTmplId"]);
                        }
                        else
                        {
                            data.Add("recTmplId", 1);
                        }

                        Dictionary<string, object> strresDict = UtilService.createRequestForPaymentGateway(PGUrl, "Initiate", data);
                        string strres = Convert.ToString(strresDict["strInitiate"]);
                        if (!strres.Equals(""))
                        {
                            strres = strres + "<input type='hidden' name='pt' value='confirm'><input type='hidden' name='si' value='" + screenId + "'>";
                            string script = "<script> function LoadProceedScreen(tpp){$('#s').val('pg'); return true;}</script>";
                            PGST.Screen_Script_Tx = script;// PGST.Screen_Script_Tx == null ? script : (PGST.Screen_Script_Tx + script);
                            Session["PGOBJECT"] = strresDict["pgDetails"];
                        }
                        PGST.Screen_Content_Tx = strres;
                        return View(PGST);
                    }
                    else if (pgtype.Equals("receipt"))
                    {
                        Session["SESSION_KEY"] = frm["sskey"];
                        PGST.Screen_Content_Tx = frm["html"];
                    }
                    else if (pgtype.Equals("confirm"))
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        string Session_Key = Convert.ToString(Session["SESSION_KEY"]);

                        data.Add("StrBankMerchantId", frm["selMode"]);
                        data.Add("pgDetails", Session["PGOBJECT"]);
                        data.Add("scid", screenId);
                        data.Add("sid", userid);
                        Dictionary<string, object> strresDict = UtilService.createRequestForPaymentGateway(PGUrl, "LoadNextScreen", data);
                        string strres = Convert.ToString(strresDict["strInitiate"]);
                        if (!strres.Equals(""))
                        {
                            string jstr = JsonConvert.SerializeObject(strresDict["pgDetails"]);
                            string eval = CryptographyUtil.EncryptDataPKCS7(jstr, Session_Key);
                            strres = strres + "<input type='hidden' name='pt' value='confirm'><input type='hidden' name='si' value='" + screenId + "'>";
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<script> function processpayment() { ")
                                .Append("var url=''; ")
                                .Append("var pgMode = $('#lblpgMode').val(); ")
                                .Append("var bool = confirm('Are you sure to continue');if (bool){")
                                .Append("var frm=document.createElement('form');frm.method='post'; ")
                                .Append("frm.action='").Append(PGAppUrl).Append("';")
                                .Append("var pgdtls = '")
                                .Append(eval)
                                .Append("';var sskey='").Append(Session_Key).Append("';frm.appendChild(createInputHidden('sskey',sskey));frm.appendChild(createInputHidden('pgDetails',pgdtls));document.body.appendChild(frm);frm.submit();}}</script>");
                            PGST.Screen_Script_Tx = sb.ToString();// PGST.Screen_Script_Tx == null ? script : (PGST.Screen_Script_Tx + script);
                            Session["PGOBJECT"] = strresDict["pgDetails"];
                        }
                        PGST.Screen_Content_Tx = strres;
                        return View("PRHome", PGST);
                    }
                    return View("PRHome", PGST);
                }
                else
                {
                    if (!string.IsNullOrEmpty(screenId))
                    {
                        if (screenId.Contains(","))
                        {
                            screenId = screenId.Split(',')[0];
                            frm["si"] = screenId;
                        }
                    }
                    {
                        if (frm["scrtype"] == "update")
                        {
                            ScreenType = "update";
                            frm["s"] = ScreenType;
                        }

                        ActionClass act = new ActionClass();
                        if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"] != string.Empty && (screenId == "156" || screenId == "157" || screenId == "204" || screenId == "205"))
                            {
                                Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                            }
                            Screen_T screen = UtilService.homeAction(WEB_APP_ID, frm, userid, menuid, screenId, ScreenType, act, (List<object>)Session["USER_MENU"]);
                            if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                            if (screen != null)
                            {
                                ViewBag.Title = screen.Screen_Title_Tx;
                                ViewBag.MenuId = menuid;
                                ViewBag.ActionClass = act;
                                return View("PRHome", screen);
                            }
                            else return View("PRHome");
                        }
                        else return RedirectToAction("Home");
                    }
                }
            }
            else
                return RedirectToAction("logout");
        }

        private void login()
        {
            string UserName = "pranonymous";
            string Password = "5f4dcc3b5aa765d61d8327deb882cf99";
            int UserId = 0;

            #region Consume Service
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            AppUrl = AppUrl + "/login";
            string Message = string.Empty;
            string sdata = UtilService.createRequestObject(AppUrl, UserName, Password, UtilService.createParameters("", "", "", "", "", "login", data), out Message);

            try
            {
                if (sdata != null)
                {
                    JObject jdata = JObject.Parse(sdata);
                    foreach (var item in jdata["data"].First.Children())
                    {
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "LOGIN_ID")
                            Session["LOGIN_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "ID")
                        {
                            UserId = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)item).Value);
                            Session["USER_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_TYPE_ID")
                            Session["USER_TYPE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_NAME_TX")
                            Session["USER_NAME_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "SESSION_KEY")
                            Session["SESSION_KEY"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_ID")
                            Session["USER_ID_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "REGION_ID")
                            Session["REGION_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "OFFICE_ID")
                            Session["OFFICE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                    }
                }
                ViewBag.Message = Message.Replace("_", " ");
                #endregion

                if (Message == "Success")
                {
                    #region Get User Data
                    UserData userData = new UserData();
                    int status = DBTable.GetUserData(UserId, userData);
                    if (status == 1)
                    {
                        if (HttpContext.Application["ManagementData"] == null)
                            objApplicationObject = new ManagementData();
                        DataTable resps = UtilService.fetchResponsibilities(userData);
                        List<object> menu = UtilService.getMenu(WEB_APP_ID, resps);
                        Session["USER_MENU"] = menu;
                        Session["USER_DATA"] = userData;
                    }
                    #endregion
                }
            }
            catch (Exception exx) { }
        }

        /// <summary>
        /// Get Member Ship data
        /// </summary>
        /// <param name="membershipNM">Member ship number</param>
        /// <param name="dob">Date of birth</param>
        /// <returns>Member info in JSON format</returns>
        [Route("getmemberdetails")]
        [HttpGet]
        public JsonResult GetMemberDetails(string membershipNM, string dob)
        {
            string strMemberFName = string.Empty, strMemberMName = string.Empty, strMemberLName = string.Empty
                , strMemberStatus = string.Empty, strEmail = string.Empty, strMobile = string.Empty, strCopNo = string.Empty;
            string strDOB = string.Empty;
            bool bIsValid = false;
            Dictionary<string, object> conds = new Dictionary<string, object>();
            MembershipDetails ObjMemberShip = new MembershipDetails();
            ICSIDataMembers membersdata = ObjMemberShip.GetMembershipData(membershipNM);
            if (membersdata != null)
            {
                if (!string.IsNullOrEmpty(membersdata.FirstName)) strMemberFName = membersdata.FirstName;
                else strMemberFName = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.MiddleName)) strMemberMName = membersdata.MiddleName;
                else strMemberMName = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.LastName)) strMemberLName = membersdata.LastName;
                else strMemberLName = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.MemberStatus)) strMemberStatus = membersdata.MemberStatus;
                else strMemberStatus = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.ProfessionalEmailID)) strEmail = membersdata.ProfessionalEmailID;
                else strEmail = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.ProfessionalMobileNumber)) strMobile = membersdata.ProfessionalMobileNumber;
                else strMobile = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.CP_NO)) strCopNo = membersdata.CP_NO;
                else strCopNo = string.Empty;
                bIsValid = true;
            }
            else bIsValid = false;
            //check member is registered or not
            conds.Clear();
            conds.Add("LOGIN_ID", membershipNM);
            DataTable dtRes = Util.UtilService.getData("stimulate", "USER_T", conds, null, 0, 10);
            bool isUserRegistered = false;
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0) isUserRegistered = true;
            var jsonobj = new
            {
                strMemberName = strMemberFName + " " + strMemberMName + " " + strMemberLName,
                strMemberPhNo = strMobile,
                strMemberEmail = strEmail,
                strMemberStatus = strMemberStatus,
                strCopNo = strCopNo,
                isUserRegistered = isUserRegistered,
                bIsValid = bIsValid
            };

            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Get Member Ship data
        /// </summary>
        /// <param name="membershipNM">Member ship number</param>
        /// <param name="dob">Date of birth</param>
        /// <returns>Member info in JSON format</returns>
        [Route("GetPeerReviewers")]
        [HttpGet]
        public JsonResult GetPeerReviewers(string screen_comp_id, string PU_PROF_CITY,int appModId)
        {
            string strHtml = string.Empty;
            if (string.IsNullOrEmpty(PU_PROF_CITY))
                PU_PROF_CITY = Convert.ToString(System.Web.HttpContext.Current.Session["PU_PROF_CITY"]);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", screen_comp_id);
            conditions.Add("PU_PROF_CITY", PU_PROF_CITY);

            JObject jdata = DBTable.GetData("query", conditions, "SCREEN_COMP_T", 0, 100,Util.UtilService.getSchemaNameById(appModId));
            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "query")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                string strCompContentTx = "RADIO,MEMBERSHIP_NUMBER,PEER_REVIEWER_NAME_TX,COP_NM,MOBILE_NM,EMAIL,CITY";
                List<string> cols = new List<string>();
                cols = strCompContentTx.Split(',').ToList<string>();

                string strCompValueTx = "RADIO,Membership Number,Peer Reviewer Name,Cop Number,Mobile,Email,City";
                List<string> colvals = new List<string>();
                colvals = strCompValueTx.Split(',').ToList<string>();

                bool isTableStart = false;
                bool isColExists = false;
                bool isIdExists = false;
                
                string Radio = string.Empty;
                bool isRADIO = cols.IndexOf("RADIO") > -1;
                if (isRADIO)
                    Radio = "<th class='searchResultsHeading'>Select</th>";
                
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string cn = dt.Columns[i].ColumnName;
                    if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                    int pos = cols.IndexOf(cn);
                    if (pos > -1)
                    {
                        if (!isTableStart)
                        {
                            isTableStart = true;
                            sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>")
                                .Append("Peer Reviewers </span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");
                            sb.Append("><thead>");
                            sb.Append("<tr>");
                        }
                        cn = colvals[pos];

                        if (!string.IsNullOrEmpty(Radio))
                        {
                            sb.Append(Radio);
                            Radio = string.Empty;
                        }
                        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                        if (!isColExists) isColExists = true;
                    }
                }
                
                if (isTableStart)
                {
                    sb.Append("</thead></tr><tbody>");

                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                    {
                        sb.Append("<tr>");                    
                        if (isRADIO) sb.Append("<td><input type=\"radio\" name='rdoList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"rad_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string cn = dt.Columns[j].ColumnName;
                            string value = Convert.ToString(dt.Rows[i][cn]).Replace(" 00:00:00", "");
                            int pos = cols.IndexOf(cn);
                            if (pos > -1)
                                sb.Append("<td>" + value + "</td>");
                        }                        
                        sb.Append("</tr>");
                    }

                    sb.Append("</tbody></table></div></div></div></div>");
                }
                strHtml = sb.ToString();
            }
            else
                strHtml = "<b>No Peer Reviewers are found. For more information please contact system administrator<b>";
            var jsonobj = new
            {
                strHtml = strHtml
            };

            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }
    }
}