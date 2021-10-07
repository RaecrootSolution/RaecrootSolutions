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
//using PaymentGateway.Entity;
//using PaymentGateway.BAL;
using ICSI_Library.Util;
using System.Net;
using ICSI_Library.Models;
using ICSI_Library.Membership;
using ICSI_WebApp.BusinessLayer;
using Aspose.Cells;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Web.Configuration;
using System.Globalization;
using System.Web.Script.Serialization;

namespace ICSI_WebApp.Controllers
{
    public class HomeController : Controller
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
            Session["UserType"] = UserType;
            if (Session["LOGIN_ID"] != null)
                return RedirectToAction("Home");
            else if (Request.Cookies["Login"] != null)
            {
                TempData["EmailID"] = Request.Cookies["Login"].Values["EmailID"];
                TempData["Password"] = Request.Cookies["Login"].Values["Password"];
            }
            //else if (Request.Cookies["Login"] == null)
            //{
            SaltKeyHandler.SaltKeyRemove(ref UtilService.ObjSaltKey);
            var lStrSalt = UtilService.RandomString(16);
            // Session["strSalt"] = lStrSalt;
            SaltKeyHandler.SaltKeyAdd(ref UtilService.ObjSaltKey, lStrSalt);
            ViewBag.HidSKval = lStrSalt;
            //}
            if (ConfigurationManager.AppSettings["WEB_APP_ID"].ToString() == "3")
                return RedirectToAction("CSR_login", "Home");
            if (ConfigurationManager.AppSettings["WEB_APP_ID"].ToString() == "4")
                return RedirectToAction("CGA_login", "CGA");
            if (ConfigurationManager.AppSettings["WEB_APP_ID"].ToString() == "2")
                return RedirectToAction("login", "PeerReview");
            if (ConfigurationManager.AppSettings["WEB_APP_ID"].ToString() == "7")
                return RedirectToAction("RO_login", "Home");
            if (ConfigurationManager.AppSettings["WEB_APP_ID"].ToString() == "8")
                return RedirectToAction("SAR_login", "SAR");
            else
                return View();
            //return View();
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
                                string strnewstrdt = string.Empty;
                                if (WebConfigurationManager.AppSettings["NewTrainingStructureDate"] != null && WebConfigurationManager.AppSettings["NewTrainingStructureDate"] != "")
                                {
                                    strnewstrdt = WebConfigurationManager.AppSettings["NewTrainingStructureDate"];
                                }
                                
                                string us = DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("en-GB"));
                                DateTimeFormatInfo dateTimeFormatterProvider = DateTimeFormatInfo.CurrentInfo.Clone() as DateTimeFormatInfo;
                                dateTimeFormatterProvider.ShortDatePattern = "dd/MM/yyyy"; //source date format
                                DateTime NewDate = DateTime.Parse(strnewstrdt, dateTimeFormatterProvider);
                               
                                if (NewDate <= DateTime.Now)
                                {
                                    if (Convert.ToDateTime(userData.STUDENT_T.Rows[0]["REG_DT"]) <= Convert.ToDateTime("02/02/2020"))
                                    {
                                        DataTable dt = new DataTable();
                                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                                        conditions.Add("ACTIVE_YN", 1);
                                        conditions.Add("STUDENT_ID", Session["STUDENT_ID"]);
                                        conditions.Add("NEW_STR_ID", 3);
                                        dt = UtilService.getData("Training", "SWITCH_OVER_T", conditions, null, 0, 1);
                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                        }
                                        else
                                        {
                                            dt = null;
                                            conditions = new Dictionary<string, object>();
                                            conditions.Add("ACTIVE_YN", 1);
                                            conditions.Add("STUDENT_ID", Session["STUDENT_ID"]);
                                            dt = UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_LONGTERM_T", conditions, null, 0, 1);
                                            if (dt != null && dt.Rows.Count > 0)
                                            {
                                            }
                                            else
                                            {
                                                dt = null;
                                                conditions = new Dictionary<string, object>();
                                                conditions.Add("ACTIVE_YN", 1);
                                                conditions.Add("REJECT_YN", 0);
                                                conditions.Add("EXEMPTION_TYPE_ID", 3);
                                                conditions.Add("STUDENT_ID", Session["STUDENT_ID"]);
                                                dt = UtilService.getData("Training", "STUDENT_TRAINING_EXEMPTION_T", conditions, null, 0, 1);
                                                if (dt != null && dt.Rows.Count > 0)
                                                {
                                                }
                                                else
                                                {
                                                    dt = null;
                                                    conditions = new Dictionary<string, object>();
                                                    conditions.Add("ACTIVE_YN", 1);
                                                    conditions.Add("REJECT_YN", 0);
                                                    conditions.Add("EXEMPTION_TYPE_ID", 4);
                                                    conditions.Add("STUDENT_ID", Session["STUDENT_ID"]);
                                                    dt = UtilService.getData("Training", "STUDENT_TRAINING_EXEMPTION_T", conditions, null, 0, 1);
                                                    if (dt != null && dt.Rows.Count > 0)
                                                    {
                                                    }
                                                    else
                                                    {
                                                        string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                                                        string UserName1 = Convert.ToString(Session["LOGIN_ID"]);
                                                        string Session_Key = Convert.ToString(Session["SESSION_KEY"]);
                                                        List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                                                        List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();

                                                        Dictionary<string, object> Mul_tblData = new Dictionary<string, object>();
                                                        Mul_tblData.Add("STUDENT_ID", Convert.ToString(Session["STUDENT_ID"]));
                                                        Mul_tblData.Add("APPROVE_NM", "1");
                                                        Mul_tblData.Add("NEW_STR_ID", "3");
                                                        Mul_tblData.Add("REMARKS_TX", "System Approved");
                                                        Mul_tblData.Add("ACTION_BY", "1");
                                                        Mul_tblData.Add("APPROVE_YN", "1");                                                        
                                                        Mul_tblData.Add("ACTION_DT", DateTime.Now.ToString("MM/dd/yyyy").Replace("-", "/"));
                                                        lstData1.Add(Mul_tblData);

                                                        if (lstData1.Count > 0)
                                                        {
                                                            AppUrl = AppUrl + "/AddUpdate";
                                                            lstData.Add(Util.UtilService.addSubParameter("Training", "SWITCH_OVER_T", 0, 0, lstData1, conditions));
                                                            ActionClass act1 = UtilService.createRequestObject(AppUrl, UserName1, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                                        }                                                        
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

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

        [HttpGet]
        public ActionResult Home()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 && Convert.ToString(Session["LOGIN_ID"]) != "csranonymous" && Convert.ToString(Session["LOGIN_ID"]) != "memberrenewal")
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
                    //if (screen.Screen_File_Name_Tx != null && !screen.Screen_File_Name_Tx.Trim().Equals("")) return RedirectToAction(screen.Screen_File_Name_Tx);
                    //else
                    return View(screen);
                }
                else return View();
            }
            else
                return RedirectToAction("logout");
        }

        /*[HttpPost]
        public ActionResult DownloadSearchResult(FormCollection frm)
        {

        }*/

        [HttpPost]
        public JsonResult AjaxPagenationSearch(FormCollection frm)
        {
            string scrcontent = string.Empty;
            try
            {
                scrcontent = UtilService.AjaxSearch(WEB_APP_ID, frm);
            }
            catch (Exception ex) { scrcontent = string.Empty; }
            var json = Json(new
            {
                resMessage = scrcontent.Trim().Equals("") ? "Fail" : "Success",
                resCode = scrcontent.Trim().Equals("") ? -1 : 0,
                htmlstring = scrcontent
            });
            return json;
        }

        [HttpPost]
        public ActionResult Home(FormCollection frm)
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 || (Convert.ToString(Session["LOGIN_ID"]) == "pcscomp" || Convert.ToInt32(Session["USER_TYPE_ID"]) ==19 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6))
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
                            //Dictionary<string, object> dataa = (Dictionary<string, object>)Session["PGOBJECT"];
                            //string json = JsonConvert.SerializeObject(dataa, Formatting.Indented);
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
                        return View(PGST);
                    }
                    return View(PGST);
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

                    /*if (ScreenType.ToLower() == "payment")
                    {
                        string ReqID = Convert.ToString(frm["PROCESS_REQUEST_ID"]);
                        int transactionreqid = Convert.ToInt32(Convert.ToString(frm["ONLINE_PAYMENT_DETAIL_T.ID"]));
                        string TotAmount = Convert.ToString(frm["AMOUNT_NM"]);
                        string PayMode = "9";
                        PayMode = "9";
                        string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
                        PaymentGatewayData objPGData = new PaymentGatewayData();
                        string RequestMsg = string.Empty;
                        TotAmount = "2.00";
                        PaymentTransactionEntity objTxnEntity = objPGData.GetPaymentRequestData(TotAmount, ReqID, PayMode, ProcessRoute, "Dummy", transactionreqid, ref RequestMsg);
                        Session["RequetID"] = objTxnEntity.RequestID;
                        Session["UserID"] = frm["u"];
                        return View("PGMiddleView", new PGMiddleURL { CheckoutUrl = RequestMsg });
                    }
                    else*/
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
                            if (ScreenType == "reports")
                            {
                                //string strRpt = string.Empty;

                                //strRpt = screen.ScreenComponents.AsEnumerable().Where(x => x.Comp_Type_Nm == 24).Select(x => x.compNameTx).FirstOrDefault();
                                //ReportDocument rd = new ReportDocument();
                                //rd.Load(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Reports"), strRpt));

                                //////ICSI_Library.Models. rpt = new Report_Comp_T();

                                //rd.SetDatabaseLogon("sa", "happy@123", "192.168.2.121", "ICSI_PROD_TRAINING");

                                ////rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                                ////rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(1, 1, 1, 1));
                                ////rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA4;

                                //string strReportName = string.Empty;
                                //strReportName = screen.Screen_Title_Tx.ToString();
                                //ExportFormatType Ep = ExportFormatType.PortableDocFormat;
                                ////CrystalDecisions.Shared.ExportFormatType Ep = CrystalDecisions.Shared.ExportFormatType.PortableDocFormat;
                                //string strRptType = string.Empty;
                                //strRptType = "application/pdf";

                                //string strRptNameWithExt = string.Empty;
                                //strRptNameWithExt = strReportName + ".pdf";

                                //if (frm["REPORT_TYPE_NM"] != null)
                                //{
                                //    if (frm["REPORT_TYPE_NM"] == "1")
                                //    {
                                //        strRptType = "application/pdf";
                                //        strRptNameWithExt = strReportName + ".pdf";
                                //        Ep = ExportFormatType.PortableDocFormat;
                                //    }
                                //    else if (frm["REPORT_TYPE_NM"] == "2")
                                //    {
                                //        strRptType = "application/Excel";
                                //        strRptNameWithExt = strReportName + ".xls";
                                //        Ep = ExportFormatType.Excel;
                                //    }
                                //}

                                //Response.Buffer = false;
                                //Response.ClearContent();
                                //Response.ClearHeaders();


                                ////rd.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, @"D:\ASD.pdf");
                                //Stream stream = rd.ExportToStream(Ep);
                                //stream.Seek(0, SeekOrigin.Begin);

                                //return File(stream, strRptType, strRptNameWithExt);


                                string UserName = WebConfigurationManager.AppSettings["UserName"];
                                string Pass = WebConfigurationManager.AppSettings["UserPass"];
                                string Serv = WebConfigurationManager.AppSettings["ServerName"];
                                string DBase = WebConfigurationManager.AppSettings["DataBaseName"];

                                ReportDocument rd = new ReportDocument();
                                rd.Load(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Reports"), "MISEdpReport.rpt"));

                                rd.SetDatabaseLogon(UserName, Pass, Serv, DBase);

                                rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
                                rd.PrintOptions.ApplyPageMargins(new PageMargins(1, 1, 1, 1));
                                rd.PrintOptions.PaperSize = PaperSize.PaperA4;
                                if (frm["File_Type"] == "2")
                                {
                                    rd.ExportToDisk(ExportFormatType.Excel, Server.MapPath("Files/ExportedReport" + userid + ".xls"));
                                    Response.Write(@"<script type='text/javascript' language='javascript'>window.open('/Home/Files/ExportedReport" + userid + ".xls','_blank').focus();</script>");
                                }
                                else
                                {
                                    rd.ExportToDisk(ExportFormatType.PortableDocFormat, Server.MapPath("Files/ExportedReport" + userid + ".pdf"));
                                    Response.Write(@"<script type='text/javascript' language='javascript'>window.open('/Home/Files/ExportedReport" + userid + ".pdf','_blank').focus();</script>");
                                }
                                //return Redirect("/Reports/ReportViewer.aspx");
                                if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                                if (screen != null)
                                {
                                    ViewBag.Title = screen.Screen_Title_Tx;
                                    ViewBag.MenuId = menuid;
                                    ViewBag.ActionClass = act;
                                    return View(screen);
                                }
                                else return View();

                            }
                            else
                            {
                                if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                                if (screen != null)
                                {
                                    ViewBag.Title = screen.Screen_Title_Tx;
                                    ViewBag.MenuId = menuid;
                                    ViewBag.ActionClass = act;
                                    return View(screen);
                                }
                                else return View();
                            }
                        }
                        else return RedirectToAction("Home");
                    }
                }
            }
            else
                return RedirectToAction("logout");
        }

        public JsonResult GetDropDownData(string TableName, string condition, string schema)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);

            if (!string.IsNullOrEmpty(condition))
            {
                string[] splitfirst = condition.Split('/');
                for (int i = 0; i < splitfirst.Length; i++)
                {
                    string[] splitSecond = splitfirst[i].Split('-');
                    if (splitSecond.Length > 0)
                        conditions.Add(splitSecond[0], splitSecond[1]);
                }
            }

            string[] splitTBL = TableName.Split('-');
            JObject jdata = null;
            if (splitTBL.Length > 1)
            {
                TableName = splitTBL[0];
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 1000, splitTBL[1]);
            }
            else
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 1000, schema != null ? schema : Util.UtilService.getSchemaNameById(1));

            DataTable dtData = new DataTable();
            if (jdata != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == TableName)
                    {
                        //dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            return Json(Util.UtilService.DataTableToJSON(dtData));
        }

        public ActionResult DownloadFile(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.Split('_')[1]);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            if (id.Split('_')[0] == "1")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.FACULTY_T);
                FilePath = Convert.ToString(Util.UtilService.FilePath.FacultyMaster.GetEnumDisplayName());
            }
            else if (id.Split('_')[0] == "2")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.TRAINING_DOC_T);
                FilePath = Convert.ToString(Util.UtilService.FilePath.TrainingDoc.GetEnumDisplayName());
            }
            else if (id.Split('_')[0] == "3")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.STUDENT_REGISTER_TRAINING_CERTIFICATE);
                FilePath = Convert.ToString(Util.UtilService.FilePath.Certificate.GetEnumDisplayName());
            }
            else if (id.Split('_')[0] == "4")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.TRAINING_SESS_ATTANDACE_T);
                FilePath = Convert.ToString(Util.UtilService.FilePath.ATTENDANCE.GetEnumDisplayName());
            }
            else if (id.Split('_')[0] == "6")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.COMPLETION_CERTIFICATE_T);
                FilePath = Convert.ToString(Util.UtilService.FilePath.CompletionCertificates.GetEnumDisplayName());
            }

            JObject jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, Util.UtilService.getSchemaNameById(1));

            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                {
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            string filename = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                if (TableName == Convert.ToString(Util.UtilService.TableName.FACULTY_T))
                    filename = Convert.ToString(dtData.Rows[0]["BIODATA_FILE_URL_TX"]);
                else 
                    filename = Convert.ToString(dtData.Rows[0]["FILE_NAME_TX"]);
            }

           string filepath = UtilService.getDocumentPath(FilePath) + id.Split('_')[1] + "_" + filename;

            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = filename,
                    Inline = true,
                };

              return new FileStreamResult(new MemoryStream(filedata), contentType) { FileDownloadName = filename };
            }
            return HttpNotFound();
        }

        public JsonResult FilterData(int screen_comp_id, int uID, int appModId = 1)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", screen_comp_id);
            conditions.Add("UID", uID);

            JObject jdata = DBTable.GetData("query", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getSchemaNameById(appModId));
            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "query")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null)
                return Json(Util.UtilService.DataTableToJSON(dt));
            else
                return Json("");
        }


        public JsonResult GetStudentDetail(int Sid, string tname = "")

        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", Sid);
            conditions.Add("ACTIVE_YN", 1);

            DataTable dt = new DataTable();
            dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);


            if (dt != null)
                return Json(Util.UtilService.DataTableToJSON(dt));
            else
                return Json("");
        }

        public JsonResult getstudetail(string reg_number)

        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("REG_NUMBER_TX", reg_number);
            conditions.Add("ACTIVE_YN", 1);

            DataTable dt = new DataTable();
            dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);


            if (dt != null)
                return Json(Util.UtilService.DataTableToJSON(dt));
            else
                return Json("");
        }

        //public JsonResult GetStudentDetail(int Sid,string tname)
        //{
        //    Dictionary<string, object> conditions = new Dictionary<string, object>();
        //    conditions.Add("ID", Sid);
        //    conditions.Add("ACTIVE_YN", 1);

        //    DataTable dt = new DataTable();
        //    dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);


        //    if (dt != null)
        //        return Json(Util.UtilService.DataTableToJSON(dt));
        //    else
        //        return Json("");
        //}

        [HttpPost]
        public JsonResult GetScreenData(string screenId, string uniqueid)
        {
            FormCollection frm = new FormCollection();
            frm["ui"] = uniqueid;
            frm["ID"] = uniqueid;
            frm["s"] = "edit";
            frm["u"] = Convert.ToString(HttpContext.Session["LOGIN_ID"]);
            frm["si"] = screenId;
            frm["UNIQUE_REG_ID"] = "";
            Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, null, screenId, frm);

            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null)
            {
                if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                {
                    screen.resActionClass = (ActionClass)UtilService.executeMethod(screen.Screen_Class_Name_Tx, null, "before" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                }
                else screen.resActionClass = UtilService.beforeLoad(WEB_APP_ID, frm);
                if (((screen.Action_Tx != null && screen.Action_Tx.Equals("edit"))) && screen.resActionClass != null && screen.resActionClass.jObject != null)
                {
                    if (screen.resActionClass.jObject.HasValues)
                    {
                        string TokenName = string.Empty;
                        Object TokenValue = string.Empty;
                        if (screen.resActionClass.jObject.First != null)
                        {
                            var o = screen.resActionClass.jObject.First.First;
                            if (o != null)
                            {
                                foreach (JToken token in screen.resActionClass.jObject.First.First.First)
                                {
                                    TokenName = ((Newtonsoft.Json.Linq.JProperty)token).Name;
                                    TokenValue = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)token).Value);

                                    if (TokenName == "START_DT" || TokenName == "EDN_DT")
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(TokenValue)))
                                            TokenValue = Convert.ToDateTime(TokenValue).ToString("dd/MM/yyyy").Replace("-", "/");
                                        //TokenValue = DateTime.ParseExact(TokenValue.ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                                    }
                                    else if (TokenName == "QUATER_PERIOD_FROM_DT" || TokenName == "QUATER_PERIOD_TO_DT")
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(TokenValue)))
                                            TokenValue = Convert.ToDateTime(TokenValue).ToString("dd/MM/yyyy").Replace("-", "/");
                                    }
                                    if (screen.Screen_Content_Tx != null)
                                    {
                                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                return Json(screen.Screen_Script_Tx + screen.Screen_Style_Tx + screen.Screen_Content_Tx);
            }
            else
                return Json("");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        public ActionResult PaymentDone(FormCollection frm)
        {
            string html = frm["html"];
            int trid = Convert.ToInt32(frm["trid"]);
            int userid = Convert.ToInt32(frm["u"]);
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            string pgtype = frm["pt"];
            string ofid = frm["ofid"];
            string sskey = frm["sskey"];
            string pt = frm["pt"];
            string refId = frm["refId"];
            int uniqueId = Convert.ToInt32(frm["uq"]);
            int nxtScnId = Convert.ToInt32(frm["nextScreen"]);
            string loginId = frm["loginid"];
            int recTmpID = Convert.ToInt32(frm["recTmpID"]);

            bool isTxSuccess = Convert.ToBoolean(frm["txSuccess"]);



            ViewBag.userId = userid;
            ViewBag.uniqueId = uniqueId;
            ViewBag.nxtScnId = nxtScnId;

            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            Dictionary<string, object> receData = new Dictionary<string, object>();
            Dictionary<string, object> conds = new Dictionary<string, object>();
            string strUserName = Convert.ToString(HttpContext.Session["LOGIN_ID"]);
            string strSession = Convert.ToString(HttpContext.Session["SESSION_KEY"]);

            ViewBag.UserName = strUserName;
            AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/fetch";
            //receData.Add("SP_NAME", "PROC_UPDATE_TRAINING_TRANSACTION_PG");
            //conds.Clear();
            //conds.Add("trId", trid);
            StringBuilder sbRow = new StringBuilder();
            //lstAddData.Add(receData);
            //lstData.Add(Util.UtilService.addSubParameter("Stimulate", string.Empty, 0, 0, lstAddData, conds));
            ActionClass act = new ActionClass();
            if (!string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strUserName))
            {
                //act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
                html = getPaymentReceipt(recTmpID, trid, isTxSuccess, strUserName, strSession);
            }
            else
            {
                //act = UtilService.createRequestObject(AppUrl, loginId, sskey, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
                html = getPaymentReceipt(recTmpID, trid, isTxSuccess, loginId, sskey);
            }

            ViewBag.Html = html;
            #region Get User Data
            if (Session["LOGIN_ID"] == null || Convert.ToString(Session["LOGIN_ID"]).Trim().Equals(""))
            {
                Session["LOGIN_ID"] = loginId;
                Session["SESSION_KEY"] = sskey;

                Dictionary<string, object> cond = new Dictionary<string, object>();
                cond.Add("ACTIVE_YN", 1);
                cond.Add("ID", userid);
                DataTable sdata = Util.UtilService.getData("Stimulate", "USER_T", cond, null, 0, 1);

                try
                {
                    if (sdata != null && sdata.Rows != null && sdata.Rows.Count > 0)
                    {
                        Session["LOGIN_ID"] = Convert.ToString(sdata.Rows[0]["LOGIN_ID"]);
                        Session["USER_ID"] = Convert.ToInt32(sdata.Rows[0]["ID"]);
                        Session["USER_TYPE_ID"] = Convert.ToInt32(sdata.Rows[0]["USER_TYPE_ID"]);
                        Session["USER_NAME_TX"] = Convert.ToString(sdata.Rows[0]["USER_NAME_TX"]);
                        Session["SESSION_KEY"] = Convert.ToString(sdata.Rows[0]["SESSION_KEY"]);
                        Session["USER_ID_TX"] = Convert.ToString(sdata.Rows[0]["USER_ID"]);
                        if (sdata.Rows[0]["REGION_ID"] != null)
                        {
                            Session["REGION_ID"] = Convert.ToInt32(sdata.Rows[0]["REGION_ID"]);
                        }
                        if (sdata.Rows[0]["OFFICE_ID"] != null)
                        {
                            Session["OFFICE_ID"] = Convert.ToInt32(sdata.Rows[0]["OFFICE_ID"]);
                        }
                        UserData userData = new UserData();
                        int status = DBTable.GetUserData(userid, userData);
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
                            List<object> menu = UtilService.getMenu(WEB_APP_ID, resps);
                            Session["USER_MENU"] = menu;
                            Session["USER_DATA"] = userData;
                        }
                    }

                    //Send Email and SMS
                    #region Old Code
                    //cond.Clear();
                    //cond.Add("ACTIVE_YN", 1);
                    //cond.Add("PAYMENT_STATUS_ID", 2);
                    //cond.Add("ID", trid);
                    //DataTable transtatusdt = Util.UtilService.getData("Stimulate", "PG_TRANSACTION_T", cond, null, 0, 1);

                    //if(transtatusdt!=null && transtatusdt.Rows.Count>0)
                    //{
                    //    string StudentID = string.Empty;
                    //    if (HttpContext.Session["STUDENT_ID"] != null)
                    //    {
                    //        StudentID = HttpContext.Session["STUDENT_ID"].ToString();
                    //    }
                    //    if (  StudentID != string.Empty)
                    //    {
                    //        Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
                    //    }
                    //}
                    #endregion
                }
                catch (Exception ex) { }
                #endregion
            }

            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="recTmpID"></param>
        /// <param name="tran_Id"></param>
        /// <returns></returns>
        public string getPaymentReceipt(int recTmpID, int tran_Id, bool txSuccess, string strUserName, string strSession)
        {
            string strHtml = string.Empty;
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            Dictionary<string, object> receData = new Dictionary<string, object>();
            string strProcName = string.Empty;
            Dictionary<string, object> conds = new Dictionary<string, object>();
            if (txSuccess)
            {
                //GET RECEIPT HTML TEMPLATE 
                conds.Clear();
                conds.Add("ID", recTmpID);
                DataTable dtRes = Util.UtilService.getData("stimulate", "RECEIPT_TEMPLATE_T", conds, null, 0, 10);
                if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
                {

                    if (dtRes.Rows[0]["TEMPLATE_CONTENT_TX"] != null)
                    {
                        strHtml = Convert.ToString(dtRes.Rows[0]["TEMPLATE_CONTENT_TX"]);
                    }
                    if (dtRes.Rows[0]["SP_OBJ_TX"] != null)
                        strProcName = Convert.ToString(dtRes.Rows[0]["SP_OBJ_TX"]);
                }

                AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/fetch";
                receData.Add("SP_NAME", strProcName);
                conds.Clear();
                conds.Add("PG_TRAN_ID", tran_Id);

                DataTable dtData = new DataTable();
                lstAddData.Add(receData);
                lstData.Add(Util.UtilService.addSubParameter("Stimulate", string.Empty, 0, 0, lstAddData, conds));
                string strAddInfo1 = string.Empty;
                string strColName = string.Empty, strColVal = string.Empty;

                //GET RECEIPT DATA
                ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
                if (Convert.ToInt32(act.StatCode) >= 0)
                {
                    JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));

                    if (jData.HasValues)
                    {
                        foreach (JProperty val in jData.Properties())
                        {
                            if (!val.Name.Equals("STATUS"))
                            {
                                dtData = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
                                {
                                    foreach (DataColumn dtCol in dtData.Columns)
                                    {
                                        strColName = "@" + dtCol.ColumnName;
                                        if (dtData.Rows[0][dtCol] != null) strColVal = Convert.ToString(dtData.Rows[0][dtCol]);
                                        else strColVal = string.Empty;
                                        strHtml = strHtml.Replace(strColName, strColVal);
                                    }
                                }
                                strHtml = strHtml + GetItemInfoForReceipt(tran_Id, Convert.ToString(dtData.Rows[0]["TRAINING_NM"]), strUserName, strSession, dtData) + "</div></div>";
                            }
                        }
                    }
                }
                if (recTmpID == 0) { }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                conds.Clear();
                conds.Add("ID", tran_Id);
                DataTable dtRes = Util.UtilService.getData("stimulate", "PG_TRANSACTION_T", conds, null, 0, 10);
                if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
                {
                    sb.Append("<div class='contain-all'><div class='container form-field'><form class='well form-horizontal' action=' ' method='post' id='contact_form'>")
                         .Append("<div class='addTrainingStructure paymentStructure'><h4 class=''>Payment Status</h4></div><div class=''><div class='row'><div class='col-md-9'>")
                         .Append("<p class='PaymentPara bg-success'>Your request Id is: <label>").Append(dtRes.Rows[0]["ACKNOWLEDGEMENT_ID"]).Append("</label></p>")
                         .Append("<p class='PaymentPara bg-success'>Your transaction Id is: <label>").Append(dtRes.Rows[0]["TRANSACTION_ID"]).Append(" </label></p>")
                         .Append("<p class='PaymentPara bg-success'>Your Payment of Rs. <label>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtRes.Rows[0]["AMOUNT"]), 2))).Append(" </label> has been failed.</p></div></div>")
                         .Append("<div class='row'><div class='col-md-9'><ul class='list-unstyled'><li>Payment Type:- <label class='PaymntLabel'>Training</label>")
                         .Append("</li><li>Name:- <label class='PaymntLabel'>").Append(dtRes.Rows[0]["CUSTOMER_NAME_TX"]).Append(" </label></li>")
                         .Append("<li>Mobile Number:- <label class='PaymntLabel'>").Append(dtRes.Rows[0]["MOBILE_TX"]).Append(" </label> </li>")
                         .Append("<li>Email Address:- <label class='PaymntLabel'>").Append(dtRes.Rows[0]["EMAIL_TX"]).Append(" </label> </li>")
                         .Append("</ul></div></div></div></div></form></div> <!-- /.container -->");
                }
                strHtml = sb.ToString();
            }
            return strHtml;
        }

        public ActionResult Error()
        {
            return this.View();
        }

        public static string createRequest(string URL, string strRequest, string strPGUrl)
        {
            //URL = "https://localhost:44335/api/icsistimulate/login";
            string response = string.Empty;
            try
            {
                HttpWebRequest webRequest = WebRequest.Create(URL) as HttpWebRequest;
                webRequest.Method = "POST";
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.PreAuthenticate = true;
                webRequest.ContentType = "application/json";
                //webRequest.ContentLength = 0;
                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("strRequest", strRequest);
                d.Add("strPGUrl", strPGUrl);
                string strJsonData = JsonConvert.SerializeObject(d, Formatting.Indented);
                StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
                streamWriter.Write(strJsonData);
                streamWriter.Flush();
                streamWriter.Close();
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;

                using (System.IO.StreamReader readResponse = new System.IO.StreamReader(webResponse.GetResponseStream()))
                {
                    response = readResponse.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string strErrMsg = ex.Message;
            }
            return response;
        }


        [HttpPost]
        public JsonResult AjaxData()
        {
            if (Request.Form["Comp_ID"] != null)
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                int compid = 0;
                int.TryParse(Convert.ToString(Request.Form["Comp_ID"]), out compid);
                conditions.Add("ID", compid);
                for (int i = 0; i < Request.Form.AllKeys.Count(); i++)
                {
                    if (Request.Form.GetKey(i).Trim() != "Comp_ID" && Request.Form.GetKey(i).Trim() != "ICSI_SCHEMA")
                    {
                        conditions.Add(Request.Form.GetKey(i), Request.Form[Request.Form.GetKey(i)]);
                    }
                }
                string schema = string.Empty;
                schema = Util.UtilService.getSchemaNameById(1);
                if (Convert.ToString(Request.Form["ICSI_SCHEMA"]) == "true")
                    schema = "ICSI";
                else if (!string.IsNullOrEmpty(Request.Form["ICSI_SCHEMA"]))
                    schema = Convert.ToString(Request.Form["ICSI_SCHEMA"]);

                JObject jdata = DBTable.GetData("ajax", conditions, "SCREEN_COMP_T", 0, 100, schema);
                DataTable dt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "ajax")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dt != null)
                    return Json(Util.UtilService.DataTableToJSON(dt));
                else
                    return Json("");
            }
            else
            {
                return Json("");
            }
        }

        [HttpPost]
        public JsonResult AjaxDataRO()
        {
            if (Request.Form["Comp_ID"] != null)
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                int compid = 0;
                int.TryParse(Convert.ToString(Request.Form["Comp_ID"]), out compid);
                conditions.Add("ID", compid);
                for (int i = 0; i < Request.Form.AllKeys.Count(); i++)
                {
                    if (Request.Form.GetKey(i).Trim() != "Comp_ID" && Request.Form.GetKey(i).Trim() != "ICSI_SCHEMA")
                    {
                        conditions.Add(Request.Form.GetKey(i), Request.Form[Request.Form.GetKey(i)]);
                    }
                }
                string schema = string.Empty;
                schema = "RO";
                //if (Convert.ToString(Request.Form["ICSI_SCHEMA"]) == "true")
                //{
                //    schema = Convert.ToString(Request.Form["ICSI_SCHEMA"]);
                //}
                JObject jdata = DBTable.GetData("ajax", conditions, "SCREEN_COMP_T", 0, 100, schema);
                DataTable dt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "ajax")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dt != null)
                    return Json(Util.UtilService.DataTableToJSON(dt));
                else
                    return Json("");
            }
            else
            {
                return Json("");
            }
        }

        [HttpPost]
        public JsonResult RemoveDoc()
        {
            string res = string.Empty;
            string resmessage = string.Empty;
            int rescode = 0;
            List<DocumentReference> docs = null;
            try
            {
                if (Session["UPLOAD_DOCUMENTS"] != null)
                {
                    docs = (List<DocumentReference>)Session["UPLOAD_DOCUMENTS"];
                    if (docs.Count > 0)
                    {
                        if (Request.Form["FILE_NAME_TX"] != null)
                        {
                            var itemToRemove = docs.SingleOrDefault(r => r.FILE_NAME_TX == Convert.ToString(Request.Form["FILE_NAME_TX"]));
                            docs.Remove(itemToRemove);
                            Session["UPLOAD_DOCUMENTS"] = docs;
                            string htmlstring = string.Empty;
                            string tblid = "tableuploaded";
                            if (!string.IsNullOrEmpty(Convert.ToString(Request.Form["Component_Id"])))
                            {
                                tblid = tblid + Convert.ToString(Request.Form["Component_Id"]);
                            }
                            if (docs.Count > 0)
                            {
                                htmlstring = "<table id='" + tblid + "' class='table table-bordered tableSearchResults'> <thead>  <tr class='bg'>  <th class='searchResultsHeading'>SL.No.</th>  <th class='searchResultsHeading'>DOCUMENT TYPE</th><th class='searchResultsHeading'>UPLOAD ON</th> <th class='searchResultsHeading'>DOWNLOAD</th><th class='searchResultsHeading'>DELETE</th></tr></thead><tbody>";
                            }
                            for (int i = 0; i < docs.Count; i++)
                            {
                                htmlstring += "<tr><td>";
                                htmlstring += (i + 1).ToString() + "</td>";
                                htmlstring += "<td>" + docs[i].DOC_TYPES_TX + "</td>";
                                htmlstring += "<td>" + docs[i].UPLOADED_ON + "</td>";
                                htmlstring += "<td><input type='hidden' value='" + docs[i].FILE_NAME_TX + "' class='downloaddoc' /><button type='button' class='btn btn-primary btn - xs' onclick=openDocDownload(this)>Download</button></td>";
                                htmlstring += "<td><input type='hidden' value='" + docs[i].FILE_NAME_TX + "' class='removedoc' /><button type='button' class='btn btn-primary btn - xs' onclick=openDocRemove(this)>Remove</button></td></tr>";
                            }
                            if (docs.Count > 0)
                            {
                                htmlstring += "</tbody></ table ></ div > ";
                            }
                            rescode = 1;
                            resmessage = "Success";
                            res = htmlstring;
                        }
                    }
                }
            }
            catch (Exception ex) { rescode = 0; resmessage = "Error in removing item"; }
            return Json(new
            {
                resMessage = resmessage,
                resCode = rescode,
                htmlstring = res
            });
        }

        [HttpPost]
        public JsonResult RemoveDocFromFolder()
        {
            try
            {
                string res = string.Empty;
                string resmessage = string.Empty;
                string filePath = string.Empty;
                filePath = Request.Form["FILE_NAME_TX"];
                System.IO.File.Delete(filePath);
                return Json(new
                {
                    tr_id = Request.Form["trid"],
                    resMessage = "Success",
                    resCode = 1
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    resMessage = ex.Message.ToString(),
                    resCode = 0
                });
            }

        }



        [HttpPost]
        public JsonResult DownloadFileAJAX(string FILE_NAME_TX)
        {
            return Json(new
            {
                htmlstring = FILE_NAME_TX
            });
        }

        public ActionResult DownloadAttachment(string FILE_NAME_TX)
        {
            try
            {
                List<DocumentReference> docs = null;
                string FileNameToDownload = string.Empty;
                byte[] result = null;
                if (Session["UPLOAD_DOCUMENTS"] != null)
                {
                    docs = (List<DocumentReference>)Session["UPLOAD_DOCUMENTS"];
                    if (docs.Count > 0)
                    {
                        if (FILE_NAME_TX != null)
                        {
                            byte[] itemToDownload;
                            string _fullpath = docs.Where(r => r.FILE_NAME_TX == Convert.ToString(FILE_NAME_TX)).Select(x => x.FULL_PATH).FirstOrDefault();
                            if (string.IsNullOrEmpty(_fullpath))
                            {
                                itemToDownload = docs.Where(r => r.FILE_NAME_TX == Convert.ToString(FILE_NAME_TX)).Select(x => x.FILE_BLB).FirstOrDefault();
                            }
                            else
                            {
                                FileStream stream = System.IO.File.OpenRead(@"" + _fullpath + "");
                                byte[] fileBytes = new byte[stream.Length];

                                stream.Read(fileBytes, 0, fileBytes.Length);
                                stream.Close();
                                itemToDownload = fileBytes;
                            }
                            FileNameToDownload = docs.Where(r => r.FILE_NAME_TX == Convert.ToString(FILE_NAME_TX)).Select(x => x.FILE_NAME_TX).FirstOrDefault();

                            /*Stream InputStream = new ByteArrayInputStream(itemToDownload);

                            using (var streamReader = new MemoryStream())
                            {
                                InputStream.CopyTo(streamReader);
                                result = streamReader.ToArray();
                            }*/
                            result = itemToDownload;
                        }
                    }
                }
                return File(result, System.Net.Mime.MediaTypeNames.Application.Octet, FileNameToDownload);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ActionResult DownloadAttachmentFromFolder(string FILE_NAME_TX)
        {
            try
            {
                if (FILE_NAME_TX != null)
                {
                    string fname = "";
                    string _path = "";
                    fname = FILE_NAME_TX.Split('~')[0];
                    _path = FILE_NAME_TX.Split('~')[1];
                    byte[] fileBytes = System.IO.File.ReadAllBytes(_path);

                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fname);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult UploadDoc()
        {
            string res = string.Empty;
            string resmessage = string.Empty;
            int rescode = 0;
            try
            {
                DocumentReference doc = new DocumentReference();
                int ref_id = 0;
                List<DocumentReference> docs = null;
                bool flag = true;

                if (Request.Files.Count > 0)
                {
                    if (string.IsNullOrEmpty(Request.Files[0].FileName))
                    {
                        resmessage = "Please choose file to upload";
                        rescode = 0;
                    }
                }
                else
                {
                    resmessage = "Please choose file to upload";
                    rescode = 0;
                }

                if (Session["UPLOAD_DOCUMENTS"] != null)
                {
                    docs = (List<DocumentReference>)Session["UPLOAD_DOCUMENTS"];
                    ref_id = docs.Select(x => x.REF_ID).FirstOrDefault();
                    string flname = string.Empty;
                    flname = docs.Where(x => x.FILE_NAME_TX == Convert.ToString(Request.Files[0].FileName)).Select(y => y.FILE_NAME_TX).FirstOrDefault();
                    if (flname == Convert.ToString(Request.Files[0].FileName))
                    {
                        flag = false;
                        resmessage = "This file already exist";
                        rescode = 0;
                    }

                }
                else
                {
                    docs = new List<DocumentReference>();
                }
                if (flag)
                {
                    Stream file;
                    if (Request.Form["DOC_MOD_TRANS_ID"] != null)
                    {
                        doc.DOC_MOD_TRANS_ID = Convert.ToInt32(Request.Form["DOC_MOD_TRANS_ID"]);
                    }
                    if (Request.Form["SCREEN_ID"] != null)
                    {
                        doc.SCREEN_ID = Convert.ToInt32(Request.Form["SCREEN_ID"]);
                    }
                    if (Request.Form["DOC_TYPES_TX"] != null)
                    {
                        doc.DOC_TYPES_TX = Convert.ToString(Request.Form["DOC_TYPES_TX"]);
                    }
                    if (Request.Files.Count > 0)
                    {
                        file = Request.Files[0].InputStream;
                        doc.FILE_BLB = UtilService.GetBytes(file);
                        doc.FILE_NAME_TX = Request.Files[0].FileName;
                        doc.FILE_TYPES_TX = "";
                        doc.UPLOADED_ON = DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    doc.REF_ID = 0;
                    docs.Add(doc);
                    Session["UPLOAD_DOCUMENTS"] = docs;
                    string htmlstring = string.Empty;
                    string tblid = "tableuploaded";
                    if (!string.IsNullOrEmpty(Convert.ToString(Request.Form["Component_Id"])))
                    {
                        tblid = tblid + Convert.ToString(Request.Form["Component_Id"]);
                    }
                    if (docs.Count > 0)
                    {
                        htmlstring = "<table id='" + tblid + "' class='table table-bordered tableSearchResults'> <thead>  <tr class='bg'>  <th class='searchResultsHeading'>SL.No.</th>  <th class='searchResultsHeading'>DOCUMENT TYPE</th><th class='searchResultsHeading'>UPLOAD ON</th> <th class='searchResultsHeading'>DOWNLOAD</th><th class='searchResultsHeading'>DELETE</th></tr></thead><tbody>";
                    }
                    for (int i = 0; i < docs.Count; i++)
                    {
                        htmlstring += "<tr><td>";
                        htmlstring += (i + 1).ToString() + "</td>";
                        htmlstring += "<td>" + docs[i].DOC_TYPES_TX + "</td>";
                        htmlstring += "<td>" + docs[i].UPLOADED_ON + "</td>";
                        htmlstring += "<td><input type='hidden' value='" + docs[i].FILE_NAME_TX + "' class='downloaddoc' /><button type='button' class='btn btn-primary btn - xs' onclick=openDocDownload(this)>Download</button></td>";
                        htmlstring += "<td><input type='hidden' value='" + docs[i].FILE_NAME_TX + "' class='removedoc' /><button type='button' class='btn btn-primary btn - xs' onclick=openDocRemove(this)>Remove</button></td></tr>";
                    }
                    if (docs.Count > 0)
                    {
                        htmlstring += "</tbody></ table ></ div > ";
                    }
                    rescode = 1;
                    resmessage = "Success";
                    res = htmlstring;
                }
            }
            catch (Exception ex)
            { rescode = 0; resmessage = "Uploading failed"; }
            var result = new
            {
                resMessage = resmessage,
                resCode = rescode,
                htmlstring = res
            };
            return Json(result);
        }

        public string GetFileName(string folder_Name)
        {
            string str = string.Empty;
            string _path = string.Empty;
            string _FullPath = string.Empty;
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] filesarray = file.FileName.Split(new char[] { '\\' });
                            fname = filesarray[filesarray.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }


                        // Get the complete folder path and store the file inside it.  
                        // _path = Server.MapPath("~/Uploads/" + Request.Form["Folder_Name"] + "");
                        folder_Name = "Uploads\\" + folder_Name;
                        _path = UtilService.getDocumentPath(folder_Name);
                        _FullPath = Path.Combine(_path, fname);

                        if (!(Directory.Exists(_path)))
                            Directory.CreateDirectory(_path);
                        file.SaveAs(_FullPath);
                    }
                }
                catch (Exception ex)
                {
                    str = ex.Message;
                }
            }
            return _FullPath;
        }

        [HttpPost]
        public ActionResult UploadFilesinFolder()
        {
            string res = string.Empty;
            string resmessage = string.Empty;
            int rescode = 0;
            string return_file_name = string.Empty;
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] filesarray = file.FileName.Split(new char[] { '\\' });
                            fname = filesarray[filesarray.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        return_file_name = fname;

                        string _path = string.Empty;
                        string _FullPath = string.Empty;

                        string FolderName = "Uploads" + "\\" + Request.Form["Folder_Name"] + "\\";
                        // Get the complete folder path and store the file inside it.  
                        //_path = Server.MapPath("~/Uploads/" + Request.Form["Folder_Name"] + "");
                        _path = UtilService.getDocumentPath(FolderName);
                        _FullPath = Path.Combine(_path, fname);

                        if (!(Directory.Exists(_path)))
                            Directory.CreateDirectory(_path);

                        checkgain:

                        if (System.IO.File.Exists(_FullPath))
                        {
                            return_file_name = "1_" + return_file_name;
                            _FullPath = Path.Combine(_path, return_file_name);
                            goto checkgain;
                        }
                        else
                        {
                            file.SaveAs(_FullPath);
                            // Returns message that successfully uploaded  
                            resmessage = "Success";
                            rescode = 1;
                            res = return_file_name + "~" + _FullPath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    resmessage = "Error occurred. Error details: " + ex.Message;
                    rescode = 0;
                }
            }
            else
            {
                resmessage = "No files selected.";
                rescode = 0;
            }

            var result = new
            {
                resMessage = resmessage,
                resCode = rescode,
                htmlstring = res,
                doctypename = Convert.ToString(Request.Form["doctype_name"]),
                doctypeid = Convert.ToString(Request.Form["doctype_id"]),
                trid = Convert.ToString(Request.Form["trid"]),
            };
            return Json(result);
        }

        /*public ActionResult Index()
         {
             if (Session["LOGIN_ID"] == null)
                 return RedirectToAction("login");
             else
                 return RedirectToAction("Home");
         }


         [HttpPost]
         [ActionName("ScreenEdit")]
         public ActionResult ScreenPost(FormCollection frm)
         {
             string userid = frm["u"];
             string menuid = frm["m"];
             string SreenType = frm["s"];
             string uniqueId = frm["ui"];
             if (userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals("") && uniqueId != null && !uniqueId.Trim().Equals(""))
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = UtilService.fetchMenuItem(Convert.ToInt32(menuid));
                 if (row != null)
                 {
                     Screen_T screen = UtilService.GetScreenData(row);
                     // get the unique id data from table using schema from application_t table
                     if (screen != null)
                     {
                         long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == screen.App_Module_Id).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();
                         string applicationSchema = DBTable.APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == appid).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
                         if (applicationSchema == null) applicationSchema = "";

                         Dictionary<string, object> conditions = new Dictionary<string, object>();
                         conditions.Add("ACTIVE_YN", 1);
                         conditions.Add("ID", uniqueId);
                         List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                         data.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 10, data, conditions));
                         string Message = string.Empty;
                         string UserName = Convert.ToString(Session["LOGIN_ID"]);
                         string Session_Key = Convert.ToString(Session["SESSION_KEY"]);
                         string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "fetch", data), out Message); // TODO - needs to fill the empty variables
                         JObject jdata = JObject.Parse(sdata);
                         if (screen.ScreenComponents != null)
                         {
                             StringBuilder sb = new StringBuilder();
                             Util.UtilService.renderScreenComponent(screen.ScreenComponents, sb);
                             screen.Screen_Content_Tx = screen.Screen_Content_Tx + "\n" + sb.ToString();
                         }
                         //sb.Append(screen.Screen_Content_Tx == null ? "" : screen.Screen_Content_Tx);
                         if (jdata.HasValues)
                         {
                             foreach (JProperty property in jdata.Properties())
                             {
                                 if (screen.Screen_Content_Tx != null)
                                 {
                                     screen.Screen_Content_Tx.Replace("@" + property.Name, property.Value.ToString());
                                 }
                             }
                         }

                         ViewBag.Title = screen.Screen_Title_Tx;
                         ViewBag.ScreenType = SreenType;
                         return View(screen);
                     }
                     else return View();
                 }
                 else return View();
             }
             else return RedirectToAction("logout");
         }

         [HttpPost]
         [ActionName("ScreenInsert")]
         public ActionResult ScreenPostInsert(FormCollection frm)
         {
             string userid = frm["u"];
             string menuid = frm["m"];
             string SreenType = frm["s"];
             if (userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals(""))
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = UtilService.fetchMenuItem(Convert.ToInt32(menuid));
                 if (row != null)
                 {
                     Screen_T screen = UtilService.GetScreenData(row);
                     // get the unique id data from table using schema from application_t table
                     if (screen != null)
                     {
                         long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == screen.App_Module_Id).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();
                         string applicationSchema = DBTable.APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == appid).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
                         if (applicationSchema == null) applicationSchema = "";

                         Dictionary<string, object> conditions = new Dictionary<string, object>();
                         //conditions.Add("ACTIVE_YN", 1);
                         //conditions.Add("ID", uniqueId);
                         List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                         data.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 50, data, conditions));
                         string Message = string.Empty;
                         string UserName = Convert.ToString(Session["LOGIN_ID"]);
                         string Session_Key = Convert.ToString(Session["SESSION_KEY"]);
                         string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "mfetch", data), out Message); // TODO - needs to fill the empty variables
                         JObject jdata = JObject.Parse(sdata);
                         StringBuilder sb = new StringBuilder();
                         //sb.Append(screen.Screen_Content_Tx == null ? "" : screen.Screen_Content_Tx);
                         List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                         Dictionary<string, object> tableData = new Dictionary<string, object>();
                         if (jdata.HasValues)
                         {
                             foreach (JProperty property in jdata.Properties())
                             {
                                 if (property.Name == "META_DATA")
                                 {
                                     DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                     for (int i = 0; i < dt.Rows.Count; i++)
                                     {
                                         if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                             && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                             && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                         {
                                             tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                             lstData.Add(tableData);
                                         }
                                     }
                                     List<Dictionary<string, object>> data1 = new List<Dictionary<string, object>>();
                                     data1.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 50, lstData, conditions));
                                     string sdata1 = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", data1), out Message);
                                 }
                             }
                         }

                         ViewBag.Title = screen.Screen_Title_Tx;
                         ViewBag.ScreenType = SreenType;
                         return View(screen);
                     }
                     else return View();
                 }
                 else return View();
             }
             else return RedirectToAction("logout");
         }

         [HttpPost]
         [ActionName("ScreenUpdate")]
         public ActionResult ScreenPostUpdate(FormCollection frm)
         {
             string userid = frm["u"];
             string menuid = frm["m"];
             string SreenType = frm["s"];
             string uniqueId = frm["ui"];
             if (userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals("") && uniqueId != null && !uniqueId.Trim().Equals(""))
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = UtilService.fetchMenuItem(Convert.ToInt32(menuid));
                 if (row != null)
                 {
                     Screen_T screen = UtilService.GetScreenData(row);
                     // get the unique id data from table using schema from application_t table
                     if (screen != null)
                     {
                         long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == screen.App_Module_Id).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();
                         string applicationSchema = DBTable.APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == appid).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
                         if (applicationSchema == null) applicationSchema = "";

                         Dictionary<string, object> conditions = new Dictionary<string, object>();
                         //conditions.Add("ACTIVE_YN", 1);
                         //conditions.Add("ID", uniqueId);
                         List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                         data.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 50, data, conditions));
                         string Message = string.Empty;
                         string UserName = Convert.ToString(Session["LOGIN_ID"]);
                         string Session_Key = Convert.ToString(Session["SESSION_KEY"]);
                         string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "mfetch", data), out Message); // TODO - needs to fill the empty variables
                         JObject jdata = JObject.Parse(sdata);
                         StringBuilder sb = new StringBuilder();
                         //sb.Append(screen.Screen_Content_Tx == null ? "" : screen.Screen_Content_Tx);
                         List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                         Dictionary<string, object> tableData = new Dictionary<string, object>();
                         if (jdata.HasValues)
                         {
                             foreach (JProperty property in jdata.Properties())
                             {
                                 if (property.Name == "META_DATA")
                                 {
                                     DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                     tableData.Add("ID", uniqueId);
                                     for (int i = 0; i < dt.Rows.Count; i++)
                                     {
                                         if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                             && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                             && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                         {
                                             tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                             lstData.Add(tableData);
                                         }
                                     }
                                     List<Dictionary<string, object>> data1 = new List<Dictionary<string, object>>();
                                     data1.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 50, lstData, conditions));
                                     string sdata1 = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", data1), out Message);
                                 }
                             }
                         }

                         ViewBag.Title = screen.Screen_Title_Tx;
                         ViewBag.ScreenType = SreenType;
                         return View(screen);
                     }
                     else return View();
                 }
                 else return View();
             }
             else return RedirectToAction("logout");
         }

         [HttpGet]
         public ActionResult Menu()
         {
             string userid = Convert.ToString(TempData["userid"]);
             string menuid = Convert.ToString(TempData["menuid"]);
             //string SreenType = "insert";

             if (userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals(""))
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = UtilService.fetchMenuItem(Convert.ToInt32(menuid));
                 if (row != null)
                 {
                     Screen_T screen = UtilService.GetScreenData(row);
                     if (screen != null)
                     {
                         ViewBag.Title = screen.Screen_Title_Tx;
                         //ViewBag.ScreenType = SreenType;
                         ViewBag.MenuId = menuid;
                         return View(screen);
                     }
                     else return View();
                 }
                 else return View();
             }
             else return RedirectToAction("logout");
         }

         [HttpPost]
         [ActionName("Menu")]
         public ActionResult HomePost(FormCollection frm)
         {
             string userid = frm["u"];
             string menuid = frm["m"];
             //string SreenType = "insert";

             if (userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals(""))
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = UtilService.fetchMenuItem(Convert.ToInt32(menuid));
                 if (row != null)
                 {
                     Screen_T screen = UtilService.GetScreenData(row);
                     if (screen != null)
                     {
                         ViewBag.Title = screen.Screen_Title_Tx;
                         //ViewBag.ScreenType = SreenType;
                         ViewBag.MenuId = menuid;
                         return View(screen);
                     }
                     else return View();
                 }
                 else return View();
             }
             else return RedirectToAction("logout");
         }

         [HttpGet]
         public ActionResult Dashboard()
         {
             if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null)
             {
                 List<object> menu = (List<object>)Session["USER_MENU"];
                 DataRow row = (DataRow)menu[0];
                 ViewBag.Title = "Home";
             }
             else
                 return RedirectToAction("logout");

             return View();
         }

         [HttpGet]
         public ActionResult logout()
         {
             Session.Clear();
             Session.Abandon();
             return RedirectToAction("login");
         }

         [HttpPost]
         public ActionResult LoadPage()
         {
             return null;
         }

         [HttpGet]
         public ActionResult Training(int id = 0)
         {
             if (Session["LOGIN_ID"] != null && HttpContext.Application["ManagementData"] != null)
             {
                 Screen_T objScreen_T = new Screen_T();
                 if (DBTable.SCREEN_T == null)
                     DBTable.SetManagementData();
                 var dt = DBTable.SCREEN_T.AsEnumerable();
                 foreach (var item in dt)
                 {
                     int screenid = Convert.ToInt32(item[0]);
                     if (screenid == id)
                     {
                         #region Set Screen Value
                         objScreen_T.App_Module_Id = Convert.ToInt32(item[1]);
                         objScreen_T.Screen_Name_Tx = Convert.ToString(item[2]);
                         objScreen_T.Screen_Sym_Name_Tx = Convert.ToString(item[3]);
                         objScreen_T.Screen_Title_Tx = Convert.ToString(item[4]);
                         objScreen_T.Screen_Script_Tx = Convert.ToString(item[7]);
                         objScreen_T.Screen_Content_Tx = Convert.ToString(item[8]);
                         objScreen_T.Active_YN = Convert.ToBoolean(item[17]);
                         #endregion
                     }
                 }

                 return View(objScreen_T);
             }
             else
                 return RedirectToAction("Index");
         }

         [HttpPost]
         public ActionResult actionPost(FormCollection frm)
         {
             TempData["userid"] = frm["u"];
             TempData["menuid"] = frm["m"];
             TempData["Message"] = Util.UtilService.afterSubmit(frm);
             return RedirectToAction("Menu");
         }
         */

        public JsonResult UpdateHistoryData(string remarks, string email, string mobileNm = null, string status = null, string registration = null, string userTypeID = null, string isPCSExtension = null, string uniqueRegID = null)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            DataTable docStatusdt = new DataTable();
            StringBuilder docStatus = new StringBuilder();
            docStatus.Append(remarks);
            string documentsStatus = string.Empty;
            Dictionary<string, object> docConditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
            ActionClass actionClass = new ActionClass();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();

            if (uniqueRegID != null)
            {

                object sessionobjvalues;
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                if (sessionObjs.TryGetValue(uniqueRegID, out sessionobjvalues))
                {
                    string[] DocIDs = sessionobjvalues.ToString().Split(',');


                    foreach (string docid in DocIDs)
                    {
                        documentsStatus = string.Empty;
                        docConditions.Clear();
                        docConditions.Add("ACTIVE_YN", 1);
                        docConditions.Add("UNIQUE_REG_ID", uniqueRegID);
                        docConditions.Add("ID", docid);
                        docStatusdt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", docConditions, null, 0, 1);
                        if (docStatusdt != null && docStatusdt.Rows.Count > 0)
                        {
                            documentsStatus = " Your document " + docStatusdt.Rows[0]["FILE_NAME_TX"].ToString() + " status changed to " + docStatusdt.Rows[0]["STATUS_TX"].ToString();
                            docStatus.AppendLine(documentsStatus);
                        }
                    }

                }
            }
            if (registration == null)
            {


                data.Add("EMAIL_DESC_TX", docStatus.ToString());
                data.Add("EMAIL_TX", email);
                data.Add("EMAIL_SENT_DT", DateTime.Now);
                data.Add("UNIQUE_REG_ID", uniqueRegID);
                if (isPCSExtension != null)
                {
                    data.Add("IS_PCS_EXTENSION_YN", 1);
                }
                //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_EMAIL_HISTORY_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                //}

                if (status != null && status != "" && status == "forward")
                {
                    //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                    data.Clear();
                    lstData.Clear();
                    lstData1.Clear();
                    data.Add("EMAIL_DESC_TX", remarks.ToString());
                    data.Add("EMAIL_TX", email);
                    data.Add("EMAIL_SENT_DT", DateTime.Now);
                    data.Add("UNIQUE_REG_ID", uniqueRegID);
                    lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_FORWARDING_HISTORY_T", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                }

                data.Clear();
                lstData.Clear();
                lstData1.Clear();
                data.Add("SMS_DESC_TX", remarks.ToString());
                data.Add("SMS_SENT_TO_NM", mobileNm);
                data.Add("UNIQUE_REG_ID", uniqueRegID);
                data.Add("SMS_SENT_DT", DateTime.Now);
                if (isPCSExtension != null)
                {
                    data.Add("IS_PCS_EXTENSION_YN", 1);
                }
                //bool value= PCSLayer.sendSMS("", "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_SMS_HISTORY_T", 0, 0, lstData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                //}
            }
            if (registration != null && Session["LOGIN_ID"].ToString() == "pcscomp")
            {

                data.Add("USER_TYPE_ID", Int64.Parse(userTypeID));
                data.Add("USER_ID", uniqueRegID);
                data.Add("LOGIN_ID", uniqueRegID);
                data.Add("CREATED_BY", 1);
                data.Add("ACTIVE_YN", 1);
                data.Add("LOGIN_PWD_TX", "5f4dcc3b5aa765d61d8327deb882cf99");
                data.Add("CREATED_DT", DateTime.Now);
                //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Stimulate", "USER_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                int? ID = null;
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "USER_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                if (ID != null)
                {
                    lstData.Clear();
                    lstData1.Clear();
                    data.Clear();
                    data.Add("CREATED_BY", 1);
                    data.Add("ACTIVE_YN", 1);
                    data.Add("ROLE_ID", Int64.Parse(userTypeID) + 1);
                    data.Add("CREATED_DT", DateTime.Now);
                    data.Add("USER_ID", ID);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Stimulate", "USER_ROLE_T", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                }
            }

            return this.Json("Success");
        }
        public JsonResult Validate(string unqid)
        {
            string result = "Failure";
            if (HttpContext.Session["LOGIN_ID"] != null && HttpContext.Session["LOGIN_ID"].ToString() != "pcscomp")
            {
                DataTable dt = new DataTable();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("UNIQUE_REG_ID", unqid);
                dt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", conditions, null, 0, 1);
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0]["ID"].ToString();
                    return Json(result);
                }

            }
            return Json(result);
            //else
            //{
            //    DataTable dt = new DataTable();
            //    Dictionary<string, object> conditions = new Dictionary<string, object>();
            //    conditions.Add("ACTIVE_YN", 1);
            //    conditions.Add("UNIQUE_REG_ID", unqid);
            //    dt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", conditions, null, 0, 1);
            //    if (dt.Rows.Count > 0)
            //    {
            //        if (dt.Rows[0]["STATUS_TX"].ToString() == "pending" || dt.Rows[0]["STATUS_TX"].ToString() == "approved")
            //        {
            //            HttpContext.Session["SESSION_OBJECTS"] = null;
            //            return Json(("Failure"));
            //        }
            //        else
            //        {
            //            return Json(dt.Rows[0]["ID"].ToString());
            //        }

            //    }
            //    return Json(("Failure"));
            //}
        }
        public JsonResult GetCurrentID(string uniqueRegID = null)
        {
            string ID = "Failure";
            if (uniqueRegID != null)
            {
                DataTable dt = new DataTable();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("UNIQUE_REG_ID", uniqueRegID);
                dt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    ID = dt.Rows[0]["ID"].ToString();
                }
            }
            else
            {
                ID = PCSLayer.GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
                ID = (ID != "" && ID != null) ? ID : "Failure";
            }
            return Json(ID);
        }
        public JsonResult UpdateStatusPCS(string condition, string uniqueRegID)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
            ActionClass actionClass = new ActionClass();
            conditions.Add("ACTIVE_YN", 1);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            data.Add("ID", condition.Split('_')[0].ToString());
            data.Add("STATUS_TX", condition.Split('_')[1].ToString().Trim(' '));
            data.Add("UPDATED_BY", UserName);
            lstData1.Add(data);
            lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_REGISTRATION_DOCUMENT_T", 0, 0, lstData1, conditions));
            AppUrl = AppUrl + "/AddUpdate";
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
            condition = condition.Split('_')[0].ToString();
            if (uniqueRegID != string.Empty)
            {
                if (HttpContext.Session["SESSION_OBJECTS"] != null)
                {
                    Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                    object sessionobjvalues;
                    if (sessionObjs.TryGetValue(uniqueRegID, out sessionobjvalues))
                    {
                        sessionobjvalues = (sessionobjvalues.ToString().Contains(condition.ToString())) ? sessionobjvalues.ToString().Replace(condition.ToString(), "") : sessionobjvalues;
                        condition += "," + sessionobjvalues.ToString();
                    }
                    sessionObjs.Remove(uniqueRegID);
                    sessionObjs.Add(uniqueRegID, condition);
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                }

                else
                {
                    Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                    sessionObjs.Add(uniqueRegID, condition.Split('_')[0].ToString());
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                }
            }
            return Json((""));
        }
        public JsonResult GetPCDetails(string membershipNM = null, string cop = null)
        {
            if (membershipNM == null && HttpContext.Session["SESSION_OBJECTS"] != null)
            {
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                object sessionobjvalues;
                if (sessionObjs.TryGetValue("MEMBERSHIP_NUMBER", out sessionobjvalues))
                {
                    membershipNM = sessionobjvalues.ToString();
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                }
                else if (sessionObjs.TryGetValue("PCS_COMPANY_MASTER_DETAIL_T", out sessionobjvalues))
                {
                    object[] arr = sessionobjvalues as object[];
                    DataTable dt = new DataTable();
                    string uniqueRegID = arr[0].ToString();
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("UNIQUE_REG_ID", uniqueRegID);
                    conditions.Add("ACTIVE_YN", 1);
                    dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        membershipNM = dt.Rows[0]["MEMBERSHIP_NM"].ToString();
                    }
                }
            }
            var resp = string.Empty;
            MembershipDetails membershipData = new MembershipDetails();
            ICSIDataMembers data = membershipData.GetMembershipData(membershipNM);
            ICSI datacop = membershipData.GetMembershipDataGST(membershipNM);
            if (data.MembershipNo != null)
            {
                if (HttpContext.Session["SESSION_OBJECTS"] != null)
                {
                    Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                    sessionObjs.Remove("MEMBERSHIP_NUMBER");
                    sessionObjs.Add("MEMBERSHIP_NUMBER", membershipNM);
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                    resp = (cop != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(datacop) : Newtonsoft.Json.JsonConvert.SerializeObject(data);
                }

                else
                {
                    Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                    sessionObjs.Add("MEMBERSHIP_NUMBER", membershipNM);
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                    resp = (cop != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(datacop) : Newtonsoft.Json.JsonConvert.SerializeObject(data);
                }
            }
            else
            {
                resp = "Failure";
            }

            return Json(resp);
        }
        public ActionResult DownloadFilePCS(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.Split('_')[1]);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            string fileName = string.Empty;
            TableName = Convert.ToString(Util.UtilService.TableName.PCS_REGISTRATION_DOCUMENT_T);
            FilePath = Convert.ToString(Util.UtilService.FilePath.PCSCOMPDOCS.GetEnumDisplayName());
            DataTable dtData = new DataTable();

            dtData = UtilService.getData("Training", TableName, conditions, null, 0, 1);


            //foreach (JProperty property in jdata.Properties())
            //{
            //    if (property.Name == TableName)
            //    {
            //        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            //    }
            //}

            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                fileName = Convert.ToString(dtData.Rows[0]["FILE_NAME_TX"]);
            }

            // string filepath = AppDomain.CurrentDomain.BaseDirectory + FilePath + id.Split('_')[1] + "_" + filename;
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            return RedirectToAction("PCS");
        }
        public ActionResult DownloadFileByID(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            string fileName = string.Empty;
            TableName = "DOCUMENT_T";
            DataTable dtData = new DataTable();

            dtData = UtilService.getData("Training", TableName, conditions, null, 0, 1);
            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                //filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                filepath = Convert.ToString(dtData.Rows[0]["FILE_BLB"]);
            }
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    //FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                if (fileName != null && !fileName.Trim().Equals(""))
                {
                    int pos = filepath.LastIndexOf(fileName);
                    if (pos > -1)
                    {
                        string path = filepath.Substring(0, pos);
                        string[] filePaths = Directory.GetFiles(path, fileName + "*");
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            byte[] filedata = System.IO.File.ReadAllBytes(filePaths[0]);
                            string contentType = MimeMapping.GetMimeMapping(filePaths[0]);

                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                //FileName = fileName,
                                Inline = false,
                            };

                            Response.AppendHeader("Content-Disposition", cd.ToString());

                            return File(filedata, contentType);
                        }
                    }
                }
            }
            return RedirectToAction("Home");
        }
        public ActionResult DownloadFileByIDFromSpecificTable(string id, string TableName, string ColumnName, string schema)
        {
            if (string.IsNullOrEmpty(schema))
                schema = "Training";

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);
            string FilePath = string.Empty;
            string fileName = string.Empty;
            DataTable dtData = new DataTable();

            dtData = UtilService.getData(schema, TableName, conditions, null, 0, 1);
            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                //filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                filepath = Convert.ToString(dtData.Rows[0][ColumnName]);
            }
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    //FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                if (fileName != null && !fileName.Trim().Equals(""))
                {
                    int pos = filepath.LastIndexOf(fileName);
                    if (pos > -1)
                    {
                        string path = filepath.Substring(0, pos);
                        string[] filePaths = Directory.GetFiles(path, fileName + "*");
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            byte[] filedata = System.IO.File.ReadAllBytes(filePaths[0]);
                            string contentType = MimeMapping.GetMimeMapping(filePaths[0]);

                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                //FileName = fileName,
                                Inline = false,
                            };

                            Response.AppendHeader("Content-Disposition", cd.ToString());

                            return File(filedata, contentType);
                        }
                    }
                }
            }
            return RedirectToAction("Home");
        }

        public ActionResult PreviewFileByID(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            string fileName = string.Empty;
            TableName = "DOCUMENT_T";
            DataTable dtData = new DataTable();

            dtData = UtilService.getData("Training", TableName, conditions, null, 0, 1);
            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                //filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                filepath = Convert.ToString(dtData.Rows[0]["FILE_BLB"]);
            }
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    //FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                if (fileName != null && !fileName.Trim().Equals(""))
                {
                    int pos = filepath.LastIndexOf(fileName);
                    if (pos > -1)
                    {
                        string path = filepath.Substring(0, pos);
                        string[] filePaths = Directory.GetFiles(path, fileName + "*");
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            byte[] filedata = System.IO.File.ReadAllBytes(filePaths[0]);
                            string contentType = MimeMapping.GetMimeMapping(filePaths[0]);

                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                //FileName = fileName,
                                Inline = false,
                            };

                            Response.AppendHeader("Content-Disposition", cd.ToString());

                            return File(filedata, contentType);
                        }
                    }
                }
            }
            return RedirectToAction("Home");
        }

        public ActionResult PreviewFileByIDFromSpecificTable(string id, string TableName, string ColumnName, string schema)
        {
            if (string.IsNullOrEmpty(schema))
                schema = "Training";

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);
            string FilePath = string.Empty;
            string fileName = string.Empty;
            DataTable dtData = new DataTable();

            dtData = UtilService.getData(schema, TableName, conditions, null, 0, 1);
            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                //filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                filepath = Convert.ToString(dtData.Rows[0][ColumnName]);
            }
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    //FileName = fileName,
                    Inline = true,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                if (fileName != null && !fileName.Trim().Equals(""))
                {
                    int pos = filepath.LastIndexOf(fileName);
                    if (pos > -1)
                    {
                        string path = filepath.Substring(0, pos);
                        string[] filePaths = Directory.GetFiles(path, fileName + "*");
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            byte[] filedata = System.IO.File.ReadAllBytes(filePaths[0]);
                            string contentType = MimeMapping.GetMimeMapping(filePaths[0]);

                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                //FileName = fileName,
                                Inline = true,
                            };

                            Response.AppendHeader("Content-Disposition", cd.ToString());

                            return File(filedata, contentType);
                        }
                    }
                }
            }
            return RedirectToAction("Home");
        }

        public JsonResult DeleteFile(string id)
        {
            ViewBag.Error = "";
            DataTable dt = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.ToString());
            dt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", conditions, null, 0, 1);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["STATUS_TX"].ToString() == "Approve")
                {
                    ViewBag.Error = "Cannot Delete an approved document.";
                    return Json("Failure");
                }
                else
                {
                    conditions.Clear();
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    string UserName = HttpContext.Session["LOGIN_ID"].ToString();
                    string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
                    ActionClass actionClass = new ActionClass();
                    conditions.Add("ACTIVE_YN", 1);
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    data.Add("ID", id.Split('_')[0].ToString());
                    data.Add("ACTIVE_YN", 0);
                    data.Add("UPDATED_BY", UserName);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_REGISTRATION_DOCUMENT_T", 0, 0, lstData1, conditions));
                    AppUrl = AppUrl + "/AddUpdate";
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    return Json("Success");
                }
            }
            return Json("Failure");
        }

        [HttpPost]
        public ActionResult UploadFilePCS(FormCollection frm)
        {
            string result = "Failure";
            // Verify that the user selected a file
            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];

                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    PCSLayer.UploadFile(Request.Files[file], frm);
                    result = "Success";
                }
            }
            // redirect back to the index action to show the form once again
            //return RedirectToAction("PCS");
            return Json(result);
        }

        public ActionResult GetQuaterlyDetails()
        {
            DataTable dt = new DataTable();
            string ID = string.Empty;
            string compPCSID = string.Empty;
            string studentName = string.Empty;
            string regisNum = string.Empty;
            string category = string.Empty;
            string trainerName = string.Empty;
            string traineAddress = string.Empty;
            string trainerEmail = string.Empty;
            string trainerContactNm = string.Empty;
            string trainingCommencementDate = string.Empty;
            string uniqueRegID = string.Empty;
            string studentID = string.Empty;
            string sponsorShipLetterNum = string.Empty;
            string sponsorShipLetterDate = string.Empty;
            string trainer_signature = string.Empty;
            if (HttpContext.Session["stdQty_TRN_ID"] != null)
            {
                ID = HttpContext.Session["stdQty_TRN_ID"].ToString();
            }
            /*Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", ID);
            conditions.Add("ACTIVE_YN", 1);
            dt = UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_LONGTERM_T", conditions, null, 0, 1);
            if (dt != null && dt.Rows.Count > 0)
            {
                studentID = dt.Rows[0]["STUDENT_ID"].ToString();
                compPCSID = dt.Rows[0]["COMPANY_PCS_ID"].ToString();
                trainingCommencementDate = Convert.ToDateTime(dt.Rows[0]["TRAINING_COMMENCE_DT"]).ToString("MM'/'dd'/'yyyy");
                conditions.Clear();
                dt.Clear();
                conditions.Add("ID", studentID);
                conditions.Add("ACTIVE_YN", 1);
                dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    studentName = dt.Rows[0]["STUDENT_NAME_TX"].ToString();
                    regisNum = dt.Rows[0]["REG_NUMBER_TX"].ToString();
                    conditions.Clear();
                    dt.Clear();
                    conditions.Add("ID", compPCSID);
                    conditions.Add("ACTIVE_YN", 1);
                    dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        category = dt.Rows[0]["REG_COM_TYPE_ID"].ToString();
                        uniqueRegID = dt.Rows[0]["UNIQUE_REG_ID"].ToString();
                        if (category == "1")
                        {
                            trainerName = dt.Rows[0]["NAME_TX"].ToString();
                            trainerEmail = dt.Rows[0]["EMAIL_TX"].ToString();
                            trainerContactNm = dt.Rows[0]["PERSON_MOBILE_NM"].ToString();

                        }
                        conditions.Clear();
                        dt.Clear();
                        conditions.Add("UNIQUE_REG_ID", uniqueRegID);
                        conditions.Add("ACTIVE_YN", 1);
                        if (category == "2")
                        {
                            conditions.Add("OFFICE_TYPE_ID", 3);
                        }
                        dt = UtilService.getData("Training", "PCS_ADDRESS_DETAILS_T", conditions, null, 0, 1);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            traineAddress = dt.Rows[0]["ADDRESS_TX"].ToString();
                        }
                    }
                }
            }*/

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 46); // 
            conditions.Add("ID", Convert.ToInt32(ID));

            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
            DataTable dtt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt.Rows.Count > 0)
            {
                studentName = Convert.ToString(dt.Rows[0]["STUDENT_NAME_TX"]);
                regisNum = Convert.ToString(dt.Rows[0]["REGISTRATION_NM"]);
                category = Convert.ToString(Convert.ToInt32(dt.Rows[0]["TRAINER_TYPE_ID"]));
                trainerName = Convert.ToString(dt.Rows[0]["TRAINER_NAME_TX"]);
                traineAddress = Convert.ToString(dt.Rows[0]["TRAINER_ADDRESS_TX"]);
                trainerEmail = Convert.ToString(dt.Rows[0]["TRAINER_EMAIL_TX"]);
                trainerContactNm = Convert.ToString(dt.Rows[0]["TRAINER_CONTACT_NM"]);
                if (dt.Rows[0]["TRAINER_SIGNATURE_TX"] != null)
                    trainer_signature = Convert.ToString(dt.Rows[0]["TRAINER_SIGNATURE_TX"]);
                if (dt.Rows[0].Field<DateTime?>("TRAINING_COMMENCEMENT_DT").HasValue)
                {
                    DateTime ddd = Convert.ToDateTime(dt.Rows[0]["TRAINING_COMMENCEMENT_DT"]);
                    trainingCommencementDate = ddd.ToString("dd/MM/yyyy");
                }
                studentID = Convert.ToString(Convert.ToInt32(dt.Rows[0]["STUDENT_ID"]));
                sponsorShipLetterNum = Convert.ToString(dt.Rows[0]["SPONSERSHIP_LETTER_NM"]);
                if (dt.Rows[0].Field<DateTime?>("SPONSERSHIP_LETTER_DT").HasValue)
                {
                    DateTime ddd = Convert.ToDateTime(dt.Rows[0]["SPONSERSHIP_LETTER_DT"]);
                    sponsorShipLetterDate = ddd.ToString("dd/MM/yyyy");
                }
            }


            var quaterlyJson = new[]
            {
                    new { SponsorShipLetterNumber = sponsorShipLetterNum, Trainer_Signature=trainer_signature, SponsorShipLetterDate=sponsorShipLetterDate, StudentName = studentName, RegistrationNumber = regisNum, Category = category, TrainerName=trainerName,TrainerAddress=traineAddress,TrainerEmail=trainerEmail,TrainerContactNumber=trainerContactNm,TrainingCommencementDate=trainingCommencementDate,StudentID=studentID}
                };

            var jsonData = JsonConvert.SerializeObject(quaterlyJson);
            return Json(jsonData);
        }
        public ActionResult GetAdminReportApprovalDetails()
        {
            DataTable dt = new DataTable();
            string ID = string.Empty;
            if (HttpContext.Session["stdQty_TRN_ID"] != null)
            {
                ID = HttpContext.Session["stdQty_TRN_ID"].ToString();
            }
            string MethodName = "qfetch";

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", ID);
            conditions.Add("QID", 55);//11
            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, "Training");

            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            var jsonData = JsonConvert.SerializeObject(dt);
            return Json(jsonData);
        }
        public ActionResult ExistingIDCheck(string quarterlyID)
        {
            DataTable dt = new DataTable();
            string trnID = string.Empty;
            var jsonData = "Failure";
            if (HttpContext.Session["stdQty_TRN_ID"] != null && HttpContext.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                trnID = HttpContext.Session["stdQty_TRN_ID"].ToString();
            }

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QUARTER_ID", quarterlyID);
            conditions.Add("TRAINING_ID", trnID);
            conditions.Add("ACTIVE_YN", 1);
            dt = UtilService.getData("Training", "STUDENT_QUATERLY_REPORT_T", conditions, null, 0, 1);

            if (dt != null && dt.Rows.Count > 0)
            {
                jsonData = JsonConvert.SerializeObject(dt);
            }
            return Json(jsonData);
        }
        public JsonResult UpdateStatusTrainingAdmin(string condition)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
            string tableName = string.Empty;
            // Id + "_" + s.compTextTx + "_" + unqueID + "_Call>
            string Trn_ID = condition.Split('_')[2].ToString();
            switch (condition.Split('_')[1].ToString())
            {
                case "Quarterly Reports Uploaded":
                    tableName = "STUDENT_QUATERLY_REPORT_T";
                    break;
                case "Project Report Uploaded":
                    tableName = "PROJECT_REPORT_APPROVAL_T";
                    break;
                case "Training Completion Certificate Uploaded":
                    tableName = "TRAINNING_CERT_APPROVAL_T";
                    break;

            }

            string uniqueRegID = tableName + "$" + Trn_ID;

            ActionClass actionClass = new ActionClass();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("TRAINING_ID", 1);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            data.Add("ID", condition.Split('_')[0].ToString());
            data.Add("STATUS_TX", condition.Split('_')[3].ToString().Trim(' '));
            data.Add("UPDATED_BY", UserName);
            lstData1.Add(data);
            lstData.Add(Util.UtilService.addSubParameter("Training", tableName, 0, 0, lstData1, conditions));
            AppUrl = AppUrl + "/AddUpdate";
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
            condition = condition.Split('_')[0].ToString();
            if (uniqueRegID != string.Empty)
            {
                if (HttpContext.Session["SESSION_OBJ"] != null)
                {
                    Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJ"];
                    object sessionobjvalues;
                    if (sessionObjs.TryGetValue(uniqueRegID, out sessionobjvalues))
                    {
                        sessionobjvalues = (sessionobjvalues.ToString().Contains(condition.ToString())) ? sessionobjvalues.ToString().Replace(condition.ToString(), "") : sessionobjvalues;
                        condition += "," + sessionobjvalues.ToString();
                    }
                    sessionObjs.Remove(uniqueRegID);
                    sessionObjs.Add(uniqueRegID, condition);
                    HttpContext.Session["SESSION_OBJ"] = sessionObjs;
                }

                else
                {
                    Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                    sessionObjs.Add(uniqueRegID, condition.Split('_')[0].ToString());
                    HttpContext.Session["SESSION_OBJ"] = sessionObjs;
                }
            }
            return Json((""));
        }
        public JsonResult UpdateHistoryDataAdmin(string remarks, string email, string mobileNm = null)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            DataTable docStatusdt = new DataTable();
            StringBuilder docStatus = new StringBuilder();
            docStatus.Append(remarks);
            string documentsStatus = string.Empty;
            Dictionary<string, object> docConditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
            ActionClass actionClass = new ActionClass();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            string Training_ID = string.Empty;

            if (HttpContext.Session["stdQty_TRN_ID"] != null && HttpContext.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Training_ID = HttpContext.Session["stdQty_TRN_ID"].ToString();

            }
            if (HttpContext.Session["SESSION_OBJ"] != null)
            {
                object sessionobjvalues;
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJ"];
                foreach (KeyValuePair<string, object> entry in sessionObjs)
                {
                    //Quarterly table
                    if (entry.Key.Split('$')[0].ToString() == "STUDENT_QUATERLY_REPORT_T")
                    {
                        if (sessionObjs.TryGetValue(entry.Key, out sessionobjvalues))
                        {
                            string[] DocIDs = sessionobjvalues.ToString().Split(',');


                            foreach (string docid in DocIDs)
                            {
                                documentsStatus = string.Empty;
                                docConditions.Clear();
                                docStatusdt.Clear();
                                docConditions.Add("ACTIVE_YN", 1);
                                docConditions.Add("TRAINING_ID", Training_ID);
                                docConditions.Add("ID", docid);
                                docStatusdt = UtilService.getData("Training", "STUDENT_QUATERLY_REPORT_T", docConditions, null, 0, 1);
                                if (docStatusdt != null && docStatusdt.Rows.Count > 0)
                                {
                                    documentsStatus = " Your quarterly report Quarter " + docStatusdt.Rows[0]["QUARTER_ID"].ToString() + " status changed to " + docStatusdt.Rows[0]["STATUS_TX"].ToString();
                                    docStatus.AppendLine(documentsStatus);
                                }
                            }

                        }
                    }
                    if (entry.Key.Split('$')[0].ToString() == "PROJECT_REPORT_APPROVAL_T")
                    {
                        if (sessionObjs.TryGetValue(entry.Key, out sessionobjvalues))
                        {
                            string[] DocIDs = sessionobjvalues.ToString().Split(',');

                            foreach (string docid in DocIDs)
                            {
                                documentsStatus = string.Empty;
                                docConditions.Clear();
                                docStatusdt.Clear();
                                docConditions.Add("ACTIVE_YN", 1);
                                docConditions.Add("REF_ID", Training_ID);
                                docConditions.Add("ID", docid);
                                docStatusdt = UtilService.getData("Training", "PROJECT_REPORT_APPROVAL_T", docConditions, null, 0, 1);
                                if (docStatusdt != null && docStatusdt.Rows.Count > 0)
                                {
                                    documentsStatus = " Your Project Report status changed to " + docStatusdt.Rows[0]["STATUS_TX"].ToString();
                                    docStatus.AppendLine(documentsStatus);
                                }
                            }

                        }
                    }

                    if (entry.Key.Split('$')[0].ToString() == "TRAINNING_CERT_APPROVAL_T")
                    {
                        if (sessionObjs.TryGetValue(entry.Key, out sessionobjvalues))
                        {
                            string[] DocIDs = sessionobjvalues.ToString().Split(',');

                            foreach (string docid in DocIDs)
                            {
                                documentsStatus = string.Empty;
                                docConditions.Clear();
                                docStatusdt.Clear();
                                docConditions.Add("ACTIVE_YN", 1);
                                docConditions.Add("REF_ID", Training_ID);
                                docConditions.Add("ID", docid);
                                docStatusdt = UtilService.getData("Training", "TRAINNING_CERT_APPROVAL_T", docConditions, null, 0, 1);
                                if (docStatusdt != null && docStatusdt.Rows.Count > 0)
                                {
                                    documentsStatus = " Your Training Certificate  status changed to " + docStatusdt.Rows[0]["STATUS_TX"].ToString();
                                    docStatus.AppendLine(documentsStatus);
                                }
                            }

                        }
                    }

                }

            }
            if (Training_ID != null)
            {
                data.Add("EMAIL_DESC_TX", docStatus.ToString());
                data.Add("EMAIL_TX", email);
                data.Add("EMAIL_SENT_DT", DateTime.Now);
                data.Add("REF_ID", Training_ID);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "EMAIL_HISTORY_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                data.Clear();
                lstData.Clear();
                lstData1.Clear();
                data.Add("SMS_DESC_TX", remarks.ToString());
                data.Add("SMS_SENT_TO_NM", mobileNm);
                data.Add("REF_ID", Training_ID);
                data.Add("SMS_SENT_DT", DateTime.Now);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "SMS_HISTORY_T", 0, 0, lstData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
            }






            return this.Json("Success");
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
            string strMemberFName = string.Empty, strMemberMName = string.Empty, strMemberLName = string.Empty;
            string strDOB = string.Empty;
            bool bIsValid = false;
            MembershipDetails ObjMemberShip = new MembershipDetails();
            ICSI data = ObjMemberShip.GetMembershipDataGST(membershipNM);
            if (data.MembershipNo != null)
            {
                strDOB = data.DateofBirth;
                if (strDOB.Equals(dob))
                {
                    ICSIDataMembers membersdata = ObjMemberShip.GetMembershipData(membershipNM);
                    if (!string.IsNullOrEmpty(membersdata.FirstName)) strMemberFName = membersdata.FirstName;
                    else strMemberFName = string.Empty;
                    if (!string.IsNullOrEmpty(membersdata.MiddleName)) strMemberMName = membersdata.MiddleName;
                    else strMemberMName = string.Empty;
                    if (!string.IsNullOrEmpty(membersdata.LastName)) strMemberLName = membersdata.LastName;
                    else strMemberMName = string.Empty;
                    bIsValid = true;
                }
            }

            var jsonobj = new
            {
                strMemberFName = strMemberFName,
                strMemberMName = strMemberMName,
                strMemberLName = strMemberLName,
                bIsValid = bIsValid
            };

            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Generates the receipts
        /// </summary>
        /// <param name="frm">Form Collection</param>
        /// <returns></returns>
        public ActionResult GenerateReceipts(FormCollection frm)
        {
            string strHtml = string.Empty;
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            Dictionary<string, object> receData = new Dictionary<string, object>();
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            string strProcName = string.Empty;
            
            Dictionary<string, object> conds = new Dictionary<string, object>();
            DataTable dtRes = null;
            //GET RECEIPT_TYPE
            conds.Clear();
            conds.Add("ID", frm["pg_id"]);
            dtRes = Util.UtilService.getData("stimulate", "PG_TRANSACTION_T", conds, null, 0, 10);
            string strItemType = string.Empty;
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {
                if (dtRes.Rows[0]["ITEM_TYPE_TX"] != DBNull.Value) strItemType = Convert.ToString(dtRes.Rows[0]["ITEM_TYPE_TX"]);
            }
            //GET RECEIPT HTML TEMPLATE 
            conds.Clear();
            conds.Add("ITEM_TYPE_TX", strItemType);
            //conds.Add("ID", frm["receipt_tmp_id"]);
            dtRes = Util.UtilService.getData("stimulate", "RECEIPT_TEMPLATE_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["TEMPLATE_CONTENT_TX"] != null)
                {
                    strHtml = Convert.ToString(dtRes.Rows[0]["TEMPLATE_CONTENT_TX"]);
                }
                if (dtRes.Rows[0]["SP_OBJ_TX"] != null)
                    strProcName = Convert.ToString(dtRes.Rows[0]["SP_OBJ_TX"]);
            }

            AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/fetch";
            receData.Add("SP_NAME", strProcName);
            conds.Clear();
            conds.Add("PG_TRAN_ID", frm["pg_id"]);

            DataTable dtData = new DataTable();
            lstAddData.Add(receData);
            lstData.Add(Util.UtilService.addSubParameter("Stimulate", string.Empty, 0, 0, lstAddData, conds));
            string strAddInfo1 = string.Empty;
            string strColName = string.Empty, strColVal = string.Empty;

            //GET RECEIPT DATA
            ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
            if (Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));

                if (jData.HasValues)
                {
                    foreach (JProperty val in jData.Properties())
                    {
                        if (!val.Name.Equals("STATUS"))
                        {
                            dtData = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
                            {
                                foreach (DataColumn dtCol in dtData.Columns)
                                {
                                    strColName = "@" + dtCol.ColumnName;
                                    if (dtData.Rows[0][dtCol] != null) strColVal = Convert.ToString(dtData.Rows[0][dtCol]);
                                    else strColVal = string.Empty;
                                    strHtml = strHtml.Replace(strColName, strColVal);
                                }
                            }
                        }
                        else
                        {
                            if (!val.Value.ToString().Equals("Success"))
                            {
                                strHtml = val.Value.ToString();
                            }
                        }
                    }
                }
            }
            string strTrnNm = string.Empty;
            int nxtScid = 0;
            if (dtData.Rows[0]["TRAINING_NM"] != null) strTrnNm = Convert.ToString(dtData.Rows[0]["TRAINING_NM"]);
            if (dtData.Rows[0]["NXT_SCRN_ID"] != null) nxtScid = Convert.ToInt32(dtData.Rows[0]["NXT_SCRN_ID"]);
            strHtml = strHtml + GetItemInfoForReceipt(Convert.ToInt32(frm["pg_id"]), strTrnNm,strUserName,strSession,dtData) + "</div></div>";
            ViewBag.Html = strHtml;
            ViewBag.uniqueId = "";
            if (Convert.ToInt32(Session["USER_TYPE_ID"]) == 1)
            {
                ViewBag.nxtScnId = nxtScid;
            }
            else if (Convert.ToInt32(Session["USER_TYPE_ID"]) == 12)
            {
                ViewBag.nxtScnId = 192;
            }
            else
            {
                ViewBag.nxtScnId = nxtScid;
            }
            ViewBag.Title = "Receipts";
            ViewBag.uniqueId = frm["pg_id"];
            ViewBag.UserName = strUserName;
            return View("PaymentDone");
        }

        /// <summary>
        /// Item level receipt generation
        /// </summary>
        /// <param name="pg_id">transaction id</param>
        /// <param name="strTrnNm">Training name</param>
        /// <returns></returns>
        private string GetItemInfoForReceipt(int pg_id, string strTrnNm, string strUserName, string strSession, DataTable dtReceiptData)
        {
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            Dictionary<string, object> receData = new Dictionary<string, object>();
            Dictionary<string, object> conds = new Dictionary<string, object>();
            string receipt_dt = string.Empty;
            int screenId = 0;
            string strItemType = string.Empty;
            //GET TRANSACTION DETAILS
            conds.Clear();
            conds.Add("ID", pg_id);
            DataTable dtRes = Util.UtilService.getData("stimulate", "PG_TRANSACTION_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {
                if (dtRes.Rows[0]["RESPONSE_DATE_DT"] != DBNull.Value) receipt_dt = Convert.ToDateTime(dtRes.Rows[0]["PAYMENT_INITIATION_DATE_TX"]).ToShortDateString();
                if (dtRes.Rows[0]["SCREEN_ID"] != DBNull.Value) screenId = Convert.ToInt32(dtRes.Rows[0]["SCREEN_ID"]);
                if (dtRes.Rows[0]["ITEM_TYPE_TX"] != DBNull.Value) strItemType = Convert.ToString(dtRes.Rows[0]["ITEM_TYPE_TX"]);
            }


            AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/fetch";
            receData.Add("SP_NAME", "PMT_GET_ITEM_INFO");
            conds.Clear();
            conds.Add("PG_TRAN_ID", pg_id);
            StringBuilder sbRow = new StringBuilder();
            lstAddData.Add(receData);
            lstData.Add(Util.UtilService.addSubParameter("Stimulate", string.Empty, 0, 0, lstAddData, conds));

            ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
            if (Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                DataTable dtData = new DataTable();
                if (jData.HasValues)
                {
                    foreach (JProperty val in jData.Properties())
                    {
                        try
                        {
                            if (!val.Name.Equals("STATUS"))
                            {
                                dtData = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                StringBuilder sbHtml = new StringBuilder();

                                int i = 0;
                                if (dtData.Rows.Count > 0 && dtData != null)
                                {
                                    sbHtml.Append("<table class='table table-bordered tableSearchResults'>");
                                    if (strItemType == "Licentiate" || strItemType == "SEC" || strItemType == "ACSFCSCOP")
                                    {
                                        int Total_Amt = 0;
                                        int total_tax = 0, item_tax = 0, tax_rate = 0;
                                        sbHtml.Append("<thead><tr class='active'><th class='searchResultsHeading'>S.No.</th><th class='searchResultsHeading'>Item Description</th>")
                                            .Append("<th class='searchResultsHeading'>SAC Code</th><th class='searchResultsHeading'>Qty.</th><th class='searchResultsHeading'>Rate / Item(Rs.)</th>")
                                            .Append("<th class='searchResultsHeading'>Taxable Amt(Rs.) </th>");


                                        if (Convert.ToInt32(dtData.Rows[0]["IGST_AMT"]) > 0 && dtData.Rows[0]["IGST_AMT"] != DBNull.Value)
                                        {
                                            sbHtml.Append("<th class='searchResultsHeading'>IGST @</th><th class='searchResultsHeading'>IGST Amt @</th>");
                                        }
                                        else if (Convert.ToInt32(dtData.Rows[0]["CGST_AMT"]) > 0 && dtData.Rows[0]["CGST_AMT"] != DBNull.Value
                                            && Convert.ToInt32(dtData.Rows[0]["SGST_AMT"]) > 0 && dtData.Rows[0]["SGST_AMT"] != DBNull.Value)
                                        {
                                            sbHtml.Append("<th class='searchResultsHeading'>CGST @</th><th class='searchResultsHeading'>CGST Amt @</th>");
                                            sbHtml.Append("<th class='searchResultsHeading'>SGST @</th><th class='searchResultsHeading'>SGST Amt @</th>");
                                        }
                                        sbHtml.Append("<th class='searchResultsHeading'>Round Off(Rs.) </th>");

                                        sbHtml.Append("<th class='searchResultsHeading'>Total Amount(Rs.)</th></tr></thead>");
                                        foreach (DataRow row in dtData.Rows)
                                        {

                                            i = i + 1;
                                            string sacCode = string.Empty;
                                            if (row["SAC_CODE"] != DBNull.Value) sacCode = Convert.ToString(row["SAC_CODE"]);
                                            sbRow.Append("<tr>");
                                            sbRow.Append("<td class='receiptTable'>").Append(i).Append("</td>");
                                            sbRow.Append("<td class='receiptTable'>").Append(row["SUB_ITEM_DESC_TX"])
                                                .Append("</td><td class='receiptTable'>").Append(sacCode).Append("</td>");
                                            sbRow.Append("<td class='receiptTable'>").Append(1).Append("</td>");
                                            sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                            if (Convert.ToInt32(row["IGST_AMT"]) > 0 && row["IGST_AMT"] != DBNull.Value)
                                            {
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(row["TAX_RATE_NM"])).Append("%</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["IGST_AMT"]), 2))).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>0.00</td>");
                                            }
                                            else if (Convert.ToInt32(row["CGST_AMT"]) > 0 && row["CGST_AMT"] != DBNull.Value
                                                && Convert.ToInt32(row["SGST_AMT"]) > 0 && row["SGST_AMT"] != DBNull.Value)
                                            {
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Convert.ToInt32(row["TAX_RATE_NM"]))).Append("%</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["CGST_AMT"]), 2))).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Convert.ToInt32(row["TAX_RATE_NM"]))).Append("%</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SGST_AMT"]), 2))).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>0.00</td>");
                                            }
                                            else
                                            {
                                                sbRow.Append("<td class='receiptTable'>0.00</td>");
                                                if (tax_rate == 18)
                                                {
                                                    sbRow.Append("<td colspan = '2' class='receiptTable'></td>");
                                                }
                                                else if (tax_rate == 9)
                                                {
                                                    sbRow.Append("<td colspan = '4' class='receiptTable'></td>");
                                                }
                                                sbRow.Append("<td class='receiptTable'>0.00</td>");
                                            }

                                            total_tax = total_tax + Convert.ToInt32(row["IGST_AMT"]) + Convert.ToInt32(row["CGST_AMT"]) + Convert.ToInt32(row["SGST_AMT"]);
                                            item_tax = Convert.ToInt32(row["IGST_AMT"]) + Convert.ToInt32(row["CGST_AMT"]) + Convert.ToInt32(row["SGST_AMT"]);
                                            Total_Amt = Convert.ToInt32(row["SUB_AMT_NM"]) + item_tax;
                                            sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(Total_Amt), 2))).Append("</td>");


                                            sbRow.Append("</tr>");
                                            tax_rate = Convert.ToInt32(row["TAX_RATE_NM"]);
                                        }
                                        sbHtml.Append(sbRow.ToString());
                                        sbHtml.Append("<tr class='fontBold'>");
                                        if (Convert.ToInt32(dtData.Rows[0]["IGST_AMT"]) > 0 && dtData.Rows[0]["IGST_AMT"] != DBNull.Value) sbHtml.Append("<td colspan = '10'>");
                                        else if (Convert.ToInt32(dtData.Rows[0]["CGST_AMT"]) > 0 && dtData.Rows[0]["CGST_AMT"] != DBNull.Value &&
                                            Convert.ToInt32(dtData.Rows[0]["SGST_AMT"]) > 0 && dtData.Rows[0]["SGST_AMT"] != DBNull.Value) sbHtml.Append("<td colspan = '12'>");
                                        else sbHtml.Append("<td colspan = '6'>");
                                        sbHtml.Append("<table border='0' style='width:100%; text-align:left;'><tr><td style = 'width:40%;'> </td>")
                                            .Append("<td><table><tr><td><table><tr><td style='padding:5px 15px;'>Total Amount Rs.</td><td style = 'padding:5px 0;'>")
                                            .Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2)))
                                            .Append("</td></tr>");
                                        if (Convert.ToInt32(dtData.Rows[0]["TAX_RATE_NM"]) > 0 && dtData.Rows[0]["TAX_RATE_NM"] != DBNull.Value)
                                        {
                                            sbHtml.Append("<tr><td style='padding:5px 15px;'>Total Tax Rs.</td><td style = 'padding:5px 0;'>")
                                                .Append(Convert.ToString(Math.Round(Convert.ToDecimal(total_tax), 2)))
                                                .Append("</td></tr>");
                                        }
                                        sbHtml.Append("<tr><td style='padding:5px 15px;'>Round Off Rs.</td><td style = 'padding:5px 0;'>0.00 </td></tr><tr><td style='padding:5px 15px;'>Invoice Total(In Figure) Rs.</td>")
                                            .Append("<td style = 'padding:5px 0;'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td></tr>");

                                        sbHtml.Append("<tr><td style='padding:5px 15px;'>Invoice Total(In words)</td><td style = 'padding:5px 0;' > Rupees ")
                                            .Append(Util.UtilService.ConvertToWords(Convert.ToInt32(dtData.Rows[0]["AMT_NM"])))
                                            .Append("Only</td></tr>");
                                        sbHtml.Append("</table></td></tr></table></td></tr></table></td></tr><tr>");
                                        if (Convert.ToInt32(dtData.Rows[0]["IGST_AMT"]) > 0 && dtData.Rows[0]["IGST_AMT"] != DBNull.Value) sbHtml.Append("<td colspan = '10'>");
                                        else if (Convert.ToInt32(dtData.Rows[0]["CGST_AMT"]) > 0 && dtData.Rows[0]["CGST_AMT"] != DBNull.Value &&
                                            Convert.ToInt32(dtData.Rows[0]["SGST_AMT"]) > 0 && dtData.Rows[0]["SGST_AMT"] != DBNull.Value) sbHtml.Append("<td colspan = '12'>");
                                        else sbHtml.Append("<td colspan = '6'>");
                                        sbHtml.Append("<table width= '100%' class='text-left'><tr><td style = 'width:50%; padding:5px 0;'><strong>Receipt No.:")
                                                .Append(Convert.ToString(dtReceiptData.Rows[0]["RECEIPT_ID"]))
                                                .Append("</strong></td><td style = 'width:50%; padding:5px 0; text-align:right;'><strong> Receipt Date: ")
                                                .Append(Convert.ToString(dtReceiptData.Rows[0]["RECEIPT_DATE_DT"]))
                                                .Append("</strong></td></tr><tr><td colspan = '2' style='padding-top:20px;'><p> Dear Sir/ Madam, </p><p class='text-justify'> We acknowledge with thanks the receipt of Online Transaction ID: ")
                                                .Append(Convert.ToString(dtRes.Rows[0]["TRANSACTION_ID"]))
                                                .Append(" for Rs.").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append(" towards above. </p></td></tr><tr><td style = 'width:50%; padding:5px 0;'> This is computer generated invoice and do not require any stamp or signature.</td><td style = 'width:50%; padding:5px 0; text-align:right;'>")
                                                .Append("for THE INSTITUTE OF COMPANY SECRETARIES OF INDIA<br>Authorised Signatory</td></tr></table></td></tr></tbody></table></div></form></div></div>");
                                    }
                                    else //TRAINING/EXEMPTION
                                    {
                                        sbHtml.Append("<thead><tr><th class='receiptTable'>SI. No</th><th class='receiptTable'>Description</th><th class='receiptText'>Amount</th></tr></thead><tbody>");
                                        if (!string.IsNullOrEmpty(strTrnNm))
                                        {
                                            sbRow.Append("<tr>");
                                            sbRow.Append("<td class='receiptTable'>").Append(1).Append("</td>");
                                            sbRow.Append("<td class='receiptTable'>").Append(strTrnNm).Append("</td>");
                                            sbRow.Append("<td class='receiptTable'>")
                                                .Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td>");
                                            sbRow.Append("</tr>");
                                        }
                                        else
                                        {
                                            foreach (DataRow row in dtData.Rows)
                                            {

                                                i = i + 1;

                                                sbRow.Append("<tr>");
                                                sbRow.Append("<td class='receiptTable'>").Append(i).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(row["SUB_ITEM_DESC_TX"]).Append("</td>");
                                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                                sbRow.Append("</tr>");
                                            }
                                        }
                                        sbHtml.Append(sbRow.ToString()).Append("</tbody></table>");
                                        sbHtml.Append("</div><div class='row'><div class='col-xs-10 col-xs-offset-1 text-right'><b>Total Amount: ").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</b></div>")
                                            .Append("</div><div class='row mt-10'><div class='col-xs-10 col-xs-offset-1' style='border:#666 1px solid; padding:5px;'><ul class='list-unstyled'><li><h6>Accepted Fees <b>")
                                            .Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</h6></b></li></ul></div></div>")
                                            .Append("<div class='row mt-30'><div class='col-xs-10 col-xs-offset-1 text-right no-padding'>FOR THE INSTITUTE OF COMPANY SECRETARIES OF INDIA</div>")
                                            .Append("</div><div class='row  mt-30'><div class='col-xs-10 col-xs-offset-1 text-right no-padding'>Authorised Signatory</div></div> ")
                                            .Append("<div class='row  mt-30'><div class='col-xs-10 col-xs-offset-1 no-padding'>This is computer generated invoice and do not require any stamp or signature")
                                            .Append("</div></div></section>");
                                    }


                                    return sbHtml.ToString();
                                }
                                else
                                {
                                    return "<strong><p>No entries found</p></strong>";
                                }
                            }
                            else
                            {
                                if (!val.Value.ToString().Equals("Success"))
                                {
                                    return val.Value.ToString();
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Headers of an uploaded file
        /// </summary>
        /// <param name="pg_id"></param>
        /// <param name="strFileName"></param>
        /// <param name="templateName"></param>
        /// <param name="tblName"></param>
        /// <param name="txtdelimiter"></param>
        /// <returns></returns>
        [Route("GetHeadersCols")]
        [HttpPost]
        public JsonResult GetHeadersCols()
        {
            int pg_id = Convert.ToInt32(Request.Form["pg_id"]);
            string strFileName = string.Empty;
            string templateName = Request.Form["templateName"];
            string tblName = Request.Form["tblName"];
            string txtdelimiter = Request.Form["txtdelimiter"];
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            string[] headers;
            StringBuilder sbHtml = new StringBuilder();
            string strColumnName = string.Empty;
            Dictionary<string, object> conds = new Dictionary<string, object>();

            strFileName = GetFileName("Reconcile_Files\\" + Convert.ToString(pg_id));

            if (System.IO.File.Exists(strFileName))
            {
                sbHtml.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'>")
                           .Append("<h4 class='text-on-pannel'><span class='fontBold'>Headers Columns").Append("</span></h4>")
                           .Append("<div style='overflow-x:auto;'><table id='tblColumns' class='table table-bordered tableSearchResults'><thead><tr class='bg'>")
                           .Append("<th class='searchResultsHeading'>Excel Header Columns</th>")
                           .Append("<th class='searchResultsHeading'>Table Columns</th><tbody>");
                if (strFileName.Contains(".csv"))
                {
                    using (StreamReader streamRdr = new StreamReader(strFileName))
                    {
                        headers = streamRdr.ReadLine().Split(txtdelimiter.ToCharArray());
                        string strColName = string.Empty;
                        foreach (string str in headers)
                        {
                            strColumnName = str.Replace(" ", "_").ToUpper() + "_TX";
                            sbHtml.Append("<tr>")
                           .Append("<td>").Append(str).Append("</td>")
                           .Append("<td><input type='text' name='").Append(strColumnName).Append("' placeholder='' class='form-control' value='").Append(strColumnName).Append("'/></td>")
                           .Append("</tr>");
                        }
                    }
                }
                else if (strFileName.Contains(".xlsx") || strFileName.Contains(".xls"))
                {
                    Workbook workBook = new Workbook(strFileName);
                    Worksheet worksheet = workBook.Worksheets[0];
                    Range range = worksheet.Cells.MaxDisplayRange;
                    int colCount = range.ColumnCount;
                    string strCol = string.Empty;
                    for (int i = 0; i < colCount; i++)
                    {
                        strCol = worksheet.Cells[0, i].Value.ToString();
                        strColumnName = strCol.Replace(" ", "_").ToUpper() + "_TX";
                        sbHtml.Append("<tr>")
                       .Append("<td>").Append(strCol).Append("</td>")
                       .Append("<td><input type='text' name='").Append(strColumnName).Append("' placeholder='' class='form-control' value='").Append(strColumnName).Append("'/></td>")
                       .Append("</tr>");
                    }
                }
                sbHtml.Append("</tbody></table></div></div></div></div></div>");
            }
            else
            {
                sbHtml.Append("File upload issue, Please contact administrator.");
            }
            var jsonobj = new
            {
                strHtml = sbHtml.ToString()
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        [Route("CreateTemplate")]
        [HttpPost]
        public JsonResult CreateTemplate(int pg_id, string templateName, string tblName, string txtSpName, string txtRecSpName, string strExcelCols, string strTblCols, string txtdelimiter)
        {
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            List<string> lstExcelCols = JsonConvert.DeserializeObject<List<string>>(strExcelCols);
            List<string> lstTblCols = JsonConvert.DeserializeObject<List<string>>(strTblCols);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            Dictionary<string, object> tempData = new Dictionary<string, object>();
            int tempId = 0, orderNo = 0;
            Dictionary<string, object> conds = new Dictionary<string, object>();
            string strMsg = CheckSysObjsExists(tblName, txtRecSpName, txtSpName);

            if (string.IsNullOrEmpty(strMsg))
            {
                //INSERT DATA INTO PG_RECONCILE_TEMPLATE_T
                conds.Add("PG_ID", pg_id);
                DataTable dtRes = Util.UtilService.getData("stimulate", "PG_RECONCILE_TEMPLATE_T", conds, null, 0, 10);
                if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
                {
                    DataView dvTempView = new DataView();
                    dvTempView = dtRes.DefaultView;
                    dvTempView.Sort = "ORDER_ID DESC";
                    dtRes = dvTempView.ToTable();
                    orderNo = Convert.ToInt32(dtRes.Rows[0]["ORDER_ID"]) + 1;
                }
                else
                {
                    orderNo = 1;
                }


                tempData.Add("PG_ID", pg_id);
                tempData.Add("TEMPLATE_NAME_TX", templateName);
                tempData.Add("TABLE_NAME_TX", tblName);
                tempData.Add("SP_NAME_TX", txtSpName);
                tempData.Add("RECONCILE_SP_NAME_TX", txtRecSpName);
                tempData.Add("SEPARATOR_TX", txtdelimiter);
                tempData.Add("ORDER_ID", orderNo);

                lstAddData.Add(tempData);
                lstData.Add(Util.UtilService.addSubParameter("Stimulate", "PG_RECONCILE_TEMPLATE_T", 0, 0, lstAddData, null));
                AppUrl = AppUrl + "/AddUpdate";
                ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                if (Convert.ToInt32(act.StatCode) >= 0)
                {
                    JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                    DataTable dtb = new DataTable();
                    if (jData.HasValues)
                    {
                        foreach (JProperty val in jData.Properties())
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                            {
                                tempId = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                            else
                            {
                                tempId = 0;
                            }

                        }
                    }
                }


                //INSERT DATA INTO PG_REC_TEMP_DTL_T TABLE
                if (tempId > 0)
                {

                    Dictionary<string, object> dictData = new Dictionary<string, object>();
                    if (lstExcelCols.Count == lstTblCols.Count)
                    {
                        for (int i = 0; i < lstExcelCols.Count; i++)
                        {
                            tempData = new Dictionary<string, object>();
                            tempData.Add("PG_REC_ID", tempId);
                            tempData.Add("EXCEL_COLUMN_NAME_TX", lstExcelCols[i]);
                            tempData.Add("COLUMN_NAME_TX", lstTblCols[i]);
                            tempData.Add("ORDER_NM", i + 1);
                            dictData.Add(i.ToString(), tempData);

                        }
                    }
                    lstAddData.Clear();
                    lstData.Clear();
                    dictData.Add("PG_REC_ID", tempId);
                    lstAddData.Add(dictData);
                    AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/Reconcile";
                    lstData.Add(Util.UtilService.addSubParameter("Stimulate", "PG_REC_TEMP_DTL_T", 0, 0, lstAddData, null));
                    act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "bulkinsert", lstData));
                    if (Convert.ToInt32(act.StatCode) >= 0)
                    {
                        JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                        DataTable dtb = new DataTable();
                        if (jData.HasValues)
                        {
                            foreach (JProperty val in jData.Properties())
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                                {
                                    //CREATE TABLE 
                                    StringBuilder sbTbl = new StringBuilder();
                                    sbTbl.Append("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = '")
                                        .Append(tblName).Append("') ");
                                    sbTbl.Append("BEGIN DROP TABLE ").Append(tblName).Append(" END");
                                    sbTbl.Append(" CREATE TABLE [dbo].").Append(tblName).Append("(");
                                    for (int i = 0; i < lstTblCols.Count; i++)
                                    {
                                        if (i == 0)
                                        {
                                            sbTbl.Append(lstTblCols[i]).Append(" varchar(100)").Append(" NULL");
                                        }
                                        else
                                        {
                                            sbTbl.Append(", ").Append(lstTblCols[i]).Append(" varchar(100)").Append(" NULL");
                                        }
                                    }
                                    sbTbl.Append(", REC_STATUS_TX varchar(3) NULL");
                                    sbTbl.Append(", ACTIVE_YN bit NOT NULL CONSTRAINT DF_").Append(tblName).Append("_ACTIVE_YN  DEFAULT ((0))");
                                    sbTbl.Append(", CREATED_DT datetime NOT NULL CONSTRAINT DF_").Append(tblName).Append("_CREATED_DT  DEFAULT (getdate())");
                                    sbTbl.Append(", CREATED_BY int NOT NULL");
                                    sbTbl.Append(", UPDATED_DT datetime NOT NULL CONSTRAINT DF_").Append(tblName).Append("UPDATED_DT  DEFAULT (getdate())");
                                    sbTbl.Append(", UPDATED_BY int NOT NULL");
                                    sbTbl.Append(", REF_ID int NULL");
                                    sbTbl.Append(") ON [PRIMARY]");

                                    tempData.Clear();
                                    lstAddData.Clear();
                                    tempData.Add("TBL_QUERY_STR", sbTbl.ToString());
                                    tempData.Add("TBL_NAME", tblName);
                                    lstAddData.Add(tempData);
                                    AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/Reconcile";
                                    lstData.Add(Util.UtilService.addSubParameter("Stimulate", "", 0, 0, lstAddData, null));
                                    act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "createTbl", lstData));
                                    if (Convert.ToInt32(act.StatCode) >= 0)
                                    {
                                        jData = JObject.Parse(Convert.ToString(act.DecryptData));
                                        dtb = new DataTable();
                                        if (jData.HasValues)
                                        {
                                            foreach (JProperty propVal in jData.Properties())
                                            {
                                                dtb = JsonConvert.DeserializeObject<DataTable>(propVal.Value.ToString());
                                                if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                                                {
                                                    strMsg = templateName + " is created successfully";
                                                }
                                                else
                                                {
                                                    strMsg = "Not able to create template. Please contact system administrator";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        strMsg = "Not able to create template. Please contact system administrator";
                                    }
                                }
                                else
                                {
                                    strMsg = "Not able to create template. Please contact system administrator";
                                }
                            }
                        }
                    }
                }
                else
                {
                    strMsg = "Not able to create template. Please contact system administrator";
                }

            }
            var jsonobj = new
            {
                strHtml = strMsg
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        private string CheckSysObjsExists(string tblName, string txtRecSpName, string txtSpName)
        {
            string strMsg = string.Empty;

            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Clear();
            conds.Add("TABLE_NAME", tblName);

            DataTable dtRes = Util.UtilService.getData("stimulate", "INFORMATION_SCHEMA.TABLES", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["TABLE_NAME"] != null && tblName.ToUpper() == Convert.ToString(dtRes.Rows[0]["TABLE_NAME"]).ToUpper())
                {
                    strMsg = tblName + " already exists. Please provide a different name for a new template";
                }
            }

            conds.Clear();
            conds.Add("type", "P");
            conds.Add("name", txtRecSpName);
            dtRes = Util.UtilService.getData("stimulate", "sys.all_objects", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["name"] != null && txtRecSpName.ToUpper() == Convert.ToString(dtRes.Rows[0]["name"]).ToUpper())
                {
                    strMsg = txtRecSpName + " already exists. Please provide a different name for a new template";
                }
            }

            conds.Clear();
            conds.Add("type", "P");
            conds.Add("name", txtSpName);
            dtRes = Util.UtilService.getData("stimulate", "sys.all_objects", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["name"] != null && txtSpName.ToUpper() == Convert.ToString(dtRes.Rows[0]["name"]).ToUpper())
                {
                    strMsg = txtSpName + " already exists. Please provide a different name for a new template";
                }
            }
            return strMsg;
        }

        [Route("DeleteTemp")]
        [HttpPost]
        public JsonResult DeleteTemp(int pg_id, int temp_id, string templateName, string tblName, string txtSpName, string txtRecSpName)
        {
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            string strMsg = string.Empty;
            AppUrl = AppUrl + "/AddUpdate";

            Dictionary<string, object> dictData = new Dictionary<string, object>();
            dictData.Add("ACTIVE_YN", 0);
            dictData.Add("ID", temp_id);
            Dictionary<string, object> dictDataConds = new Dictionary<string, object>();
            dictDataConds.Add("ID", temp_id);
            lstAddData.Add(dictData);
            lstData.Add(Util.UtilService.addSubParameter("Stimulate", "PG_RECONCILE_TEMPLATE_T", 0, 0, lstAddData, dictDataConds));
            ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "update", lstData));
            if (Convert.ToInt32(act.StatCode) >= 0 && act != null)
            {
                JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                DataTable dtb = new DataTable();
                if (jData.HasValues)
                {
                    foreach (JProperty propVal in jData.Properties())
                    {
                        dtb = JsonConvert.DeserializeObject<DataTable>(propVal.Value.ToString());
                        if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                        {
                            if (Convert.ToInt32(dtb.Rows[0]["ACTIVE_YN"]) == 0)
                                strMsg = templateName + " is deleted successfully";
                        }
                        else
                        {
                            strMsg = "Not able to delete template. Please contact system administrator";
                        }
                    }
                }
            }
            var jsonobj = new
            {
                strHtml = strMsg
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        [Route("GetTempDetails")]
        [HttpGet]
        public JsonResult GetTempDetails(int pg_id, int tempId)
        {
            char chSeparator = new char();
            string strTblName = string.Empty;
            string strProcName = string.Empty;
            string strReconcileSPName = string.Empty;
            StringBuilder sbHtml = new StringBuilder();
            string strProcNameData = string.Empty;
            string strRecProcNameData = string.Empty;
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            string strTempName = string.Empty;
            string pg_Name = string.Empty;

            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Add("ID", pg_id);

            DataTable dtRes = Util.UtilService.getData("stimulate", "PAYMENT_GATEWAY_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["PGNAME_TX"] != null) pg_Name = Convert.ToString(dtRes.Rows[0]["PGNAME_TX"]);
            }

            //GET TEMPLATE DATA
            conds = new Dictionary<string, object>();
            conds.Add("PG_ID", pg_id);
            conds.Add("ID", tempId);
            dtRes = Util.UtilService.getData("stimulate", "PG_RECONCILE_TEMPLATE_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {
                if (dtRes.Rows[0]["SEPARATOR_TX"] != null)
                {
                    if (dtRes.Rows[0]["SEPARATOR_TX"] != null) chSeparator = Convert.ToChar(dtRes.Rows[0]["SEPARATOR_TX"]);
                    else chSeparator = new char();
                }
                if (dtRes.Rows[0]["TABLE_NAME_TX"] != null) strTblName = Convert.ToString(dtRes.Rows[0]["TABLE_NAME_TX"]);
                if (dtRes.Rows[0]["SP_NAME_TX"] != null) strProcName = Convert.ToString(dtRes.Rows[0]["SP_NAME_TX"]);
                if (dtRes.Rows[0]["RECONCILE_SP_NAME_TX"] != null) strReconcileSPName = Convert.ToString(dtRes.Rows[0]["RECONCILE_SP_NAME_TX"]);
                if (dtRes.Rows[0]["TEMPLATE_NAME_TX"] != null) strTempName = Convert.ToString(dtRes.Rows[0]["TEMPLATE_NAME_TX"]);
            }

            //GET TEMPLATE DETAIL DATA
            conds.Clear();
            conds.Add("PG_REC_ID", tempId);
            dtRes = null;
            dtRes = Util.UtilService.getData("stimulate", "PG_REC_TEMP_DTL_T", conds, null, 0, 200);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {
                string strColName = string.Empty;
                sbHtml.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'>")
                    .Append("<h4 class='text-on-pannel'><span class='fontBold'>Headers Columns").Append("</span></h4>")
                    .Append("<div style='overflow-x:auto;'><table id='tblColumns' class='table table-bordered tableSearchResults'><thead><tr class='bg'>")
                    .Append("<th class='searchResultsHeading'>Bank/PG Name</th>")
                    .Append("<th class='searchResultsHeading'>Excel Header Columns</th>")
                    .Append("<th class='searchResultsHeading'>Table Columns</th></tr></thead><tbody>");
                foreach (DataRow row in dtRes.Rows)
                {
                    sbHtml.Append("<tr>")
                   .Append("<td>").Append(pg_Name).Append("</td>")
                   .Append("<td>").Append(Convert.ToString(row["EXCEL_COLUMN_NAME_TX"])).Append("</td>")
                   .Append("<td>").Append(Convert.ToString(row["COLUMN_NAME_TX"])).Append("</td>")
                   .Append("</tr>");
                }
                sbHtml.Append("</tbody></table></div></div></div></div>");
            }



            var jsonobj = new
            {
                sbHtml = sbHtml.ToString()
                ,
                strTblName = strTblName
                ,
                strProcName = strProcName
                ,
                strReconcileSPName = strReconcileSPName
                ,
                chSeparator = chSeparator
                ,
                strTempName = strTempName
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        [Route("ImportReconcileData")]
        [HttpPost]
        public JsonResult ImportReconcileData()
        {
            int pg_id = Convert.ToInt32(Request.Form["pg_id"]);
            string strFileName = Request.Form["fileName"];
            int tempId = Convert.ToInt32(Request.Form["temp_id"]);
            string[] headers;
            char chSeparator = new char();
            Dictionary<string, object> dictReconData = new Dictionary<string, object>();
            Dictionary<string, object> dictData = new Dictionary<string, object>();
            string strTblName = string.Empty;
            string strProcName = string.Empty;
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            string strHdr = string.Empty;
            int iReconFileID = 0;
            StringBuilder sbHtml = new StringBuilder();
            string strReconcileSPName = string.Empty;
            int ifailTrans = 0;


            //GET TEMPLATE DATA
            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Add("PG_ID", pg_id);
            conds.Add("ID", tempId);
            DataTable dtRes = Util.UtilService.getData("stimulate", "PG_RECONCILE_TEMPLATE_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                if (dtRes.Rows[0]["SEPARATOR_TX"] != null)
                {
                    chSeparator = Convert.ToChar(dtRes.Rows[0]["SEPARATOR_TX"]);
                }
                strTblName = Convert.ToString(dtRes.Rows[0]["TABLE_NAME_TX"]);
                strProcName = Convert.ToString(dtRes.Rows[0]["SP_NAME_TX"]);
                strReconcileSPName = Convert.ToString(dtRes.Rows[0]["RECONCILE_SP_NAME_TX"]);
            }

            //GET TEMPLATE DETAIL DATA
            conds.Clear();
            conds.Add("PG_REC_ID", tempId);
            dtRes = null;
            dtRes = Util.UtilService.getData("stimulate", "PG_REC_TEMP_DTL_T", conds, null, 0, 200);

            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {

                //INSERT FILE INFORMATION IN PG_RECONCILE_T TABLE
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
                Dictionary<string, object> fileData = new Dictionary<string, object>();

                fileData.Add("PG_TEMPLATE_ID", tempId.ToString());
                fileData.Add("FILE_NAME_TX", strFileName);
                fileData.Add("STATUS_TX", "I");
                lstAddData.Add(fileData);
                lstData.Add(Util.UtilService.addSubParameter("Stimulate", "PG_RECONCILE_T", 0, 0, lstAddData, null));
                AppUrl = AppUrl + "/AddUpdate";
                ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                if (Convert.ToInt32(act.StatCode) >= 0)
                {
                    JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                    DataTable dtb = new DataTable();
                    if (jData.HasValues)
                    {
                        foreach (JProperty val in jData.Properties())
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            iReconFileID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                        }
                    }

                    strFileName = GetFileName("Reconcile_Files\\" + Convert.ToString(pg_id));
                    if (System.IO.File.Exists(strFileName))
                    {
                        string strColName = string.Empty;
                        lstData.Clear();
                        lstAddData.Clear();
                        if (strFileName.Contains(".csv"))
                        {
                            //IMPORT DATA TO SQL FROM CSV FILE.
                            using (StreamReader streamRdr = new StreamReader(strFileName))
                            {
                                headers = streamRdr.ReadLine().Split(chSeparator);

                                //COPY CONTENT TO CSV TABLE

                                int j = 0;
                                while (!streamRdr.EndOfStream)
                                {
                                    dictReconData = new Dictionary<string, object>();
                                    string[] rows = streamRdr.ReadLine().Split(chSeparator);
                                    for (int i = 0; i < headers.Length; i++)
                                    {
                                        if (headers[i] == Convert.ToString(dtRes.Rows[i]["EXCEL_COLUMN_NAME_TX"]))
                                        {
                                            strColName = Convert.ToString(dtRes.Rows[i]["COLUMN_NAME_TX"]);
                                            dictReconData.Add(strColName, rows[i]);
                                        }
                                    }

                                    dictReconData.Add("REF_ID", iReconFileID);
                                    dictReconData.Add("REC_STATUS_TX", "I");
                                    dictData.Add(j.ToString(), dictReconData);
                                    j++;
                                }
                            }
                        }
                        else if (strFileName.Contains(".xlsx") || strFileName.Contains(".xls"))
                        {
                            Workbook workBook = new Workbook(strFileName);
                            Worksheet worksheet = workBook.Worksheets[0];
                            Range range = worksheet.Cells.MaxDisplayRange;
                            int rowCount = range.RowCount;
                            int colCount = range.ColumnCount;
                            string strColVal = string.Empty, strExcHdr = string.Empty;

                            for (int i = 1; i < rowCount; i++)
                            {
                                dictReconData = new Dictionary<string, object>();
                                for (int j = 0; j < colCount; j++)
                                {
                                    strColVal = worksheet.Cells[i, j].Value.ToString();
                                    strExcHdr = worksheet.Cells[0, j].Value.ToString();
                                    if (strExcHdr.Equals(Convert.ToString(dtRes.Rows[j]["EXCEL_COLUMN_NAME_TX"])))
                                    {
                                        strColName = Convert.ToString(dtRes.Rows[j]["COLUMN_NAME_TX"]);
                                        dictReconData.Add(strColName, strColVal);
                                    }
                                }
                                dictReconData.Add("REF_ID", iReconFileID);
                                dictReconData.Add("REC_STATUS_TX", "I");
                                dictData.Add((i - 1).ToString(), dictReconData);
                            }
                        }


                        dictData.Add("IS_RECONCILE", true);
                        dictData.Add("SP_NAME", strProcName);
                        dictData.Add("REF_ID", iReconFileID);

                        lstAddData.Add(dictData);
                        AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/Reconcile";
                        lstData.Add(Util.UtilService.addSubParameter("Stimulate", strTblName, 0, 0, lstAddData, null));
                        act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "bulkinsert", lstData));

                        if (Convert.ToInt32(act.StatCode) >= 0)
                        {
                            jData = JObject.Parse(Convert.ToString(act.DecryptData));
                            dtb = new DataTable();
                            if (jData.HasValues)
                            {
                                foreach (JProperty val in jData.Properties())
                                {
                                    dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                    DataTable dtFilterData = new DataTable();
                                    if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                                    {
                                        if (dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "S").Count() > 0)
                                        {
                                            dtFilterData = dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "S").CopyToDataTable();
                                            sbHtml.Append(Util.UtilService.BuildReconTable(dtFilterData, "Success and Matched Transactions", dtFilterData.Rows.Count));
                                        }

                                        if (dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "F").Count() > 0)
                                        {
                                            dtFilterData = dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "F").CopyToDataTable();
                                            ifailTrans = dtFilterData.Rows.Count;
                                            sbHtml.Append("<div></div>");
                                            sbHtml.Append(Util.UtilService.BuildReconTable(dtFilterData, "Success and UnMatched Transactions", dtFilterData.Rows.Count));
                                        }

                                        if (dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "N").Count() > 0)
                                        {
                                            dtFilterData = dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "N").CopyToDataTable();
                                            sbHtml.Append("<div></div>");
                                            sbHtml.Append(Util.UtilService.BuildReconTable(dtFilterData, "UnKnown Transactions", dtFilterData.Rows.Count));
                                        }

                                        if (dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "Z").Count() > 0)
                                        {
                                            dtFilterData = dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "Z").CopyToDataTable();
                                            sbHtml.Append("<div></div>");
                                            sbHtml.Append(Util.UtilService.BuildReconTable(dtFilterData, "Reconcillation already processed", dtFilterData.Rows.Count));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        sbHtml.Append("Please check folder path specified in database.");
                    }
                }
                else
                {
                    act.StatMessage = "Unknown error occurred, while processing the request. Please contact system administrator";
                }


            }
            else
            {
                sbHtml.Append("Unknown error occurred, while processing the request. Please contact system administrator");
            }

            var jsonobj = new
            {
                strHtml = sbHtml.ToString()
                ,
                TABLE_NAME_TX = strTblName
                ,
                REF_ID = iReconFileID
                ,
                REC_SP_NAME_TX = strReconcileSPName
                ,
                SP_NAME_TX = strProcName
                ,
                FAIL_TRANS_COUNT = ifailTrans
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }


        [Route("ProcessReconcile")]
        [HttpPost]
        public JsonResult ProcessReconcile(int ref_id, string sp_name, string rec_sp_name)
        {
            string strUserName = HttpContext.Session["LOGIN_ID"].ToString();
            string strSession = HttpContext.Session["SESSION_KEY"].ToString();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
            Dictionary<string, object> recData = new Dictionary<string, object>();
            StringBuilder sbHtml = new StringBuilder();

            AppUrl = AppUrl + "/Reconcile";
            recData.Add("REF_ID", ref_id);
            recData.Add("SP_NAME", sp_name);
            recData.Add("REC_SP_NAME", rec_sp_name);
            lstAddData.Add(recData);
            lstData.Add(Util.UtilService.addSubParameter("Stimulate", string.Empty, 0, 0, lstAddData, null));

            ActionClass act = UtilService.createRequestObject(AppUrl, strUserName, strSession, UtilService.createParameters("", "", "", "", "", "execsp", lstData));
            if (Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject jData = JObject.Parse(Convert.ToString(act.DecryptData));
                DataTable dtb = new DataTable();
                if (jData.HasValues)
                {
                    foreach (JProperty val in jData.Properties())
                    {
                        if (!val.Name.Equals("REC_STATUS"))
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            DataTable dtFilterData = new DataTable();
                            if (dtb != null && dtb.Rows != null && dtb.Rows.Count > 0)
                            {
                                if (dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "CF").Count() > 0)
                                {
                                    dtFilterData = dtb.AsEnumerable().Where(x => x.Field<string>("TYPE_TRANS") == "CF").CopyToDataTable();
                                    sbHtml.Append(Util.UtilService.BuildReconTable(dtFilterData, "Reconciliation Processed Transactions", dtb.Rows.Count));
                                }
                            }
                        }
                        else
                        {
                            if (!val.Value.ToString().Equals("Success"))
                            {
                                sbHtml.Append(val.Value.ToString());
                            }
                        }
                    }
                }
            }
            var jsonobj = new
            {
                strHtml = sbHtml.ToString()
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public ActionResult DownloadReport(string id)
        {
            string report = Convert.ToString(Session[id]);
            if (!string.IsNullOrEmpty(report))
            {
                Dictionary<string, object> dictRptEle = JsonConvert.DeserializeObject<Dictionary<string, object>>(report);
                string fileName = string.Empty;
                if (dictRptEle.ContainsKey("REPORT_NAME"))
                {
                    fileName = Convert.ToString(dictRptEle["REPORT_NAME"]);
                }
                string msg = string.Empty;

                if (!string.IsNullOrEmpty(fileName))
                {
                    dictRptEle.Remove("REPORT_NAME");
                }

                byte[] array = CreateExcelSheet(fileName, dictRptEle);
                //string strType = "pdf";
                string contentType = string.Empty;
                //if (strType.Equals("pdf"))
                //{
                //    string strfolderName = "Uploads\\Finance\\" + fileName;
                //    string _path = UtilService.getDocumentPath(strfolderName);
                //    string _FullPath = Path.Combine(_path, fileName + ".pdf");

                //    if (!(Directory.Exists(_path)))
                //    {
                //        Directory.CreateDirectory(_path);
                //        System.IO.File.WriteAllBytes(_FullPath, array);
                //    }

                //    byte[] filedata = System.IO.File.ReadAllBytes(_FullPath);
                //    contentType = MimeMapping.GetMimeMapping(_FullPath);

                //    var cd = new System.Net.Mime.ContentDisposition
                //    {
                //        FileName = fileName+".pdf",
                //        Inline = false,
                //    };

                //    Response.AppendHeader("Content-Disposition", cd.ToString());
                //}
                //else if (strType == "xlsx")
                //{
                contentType = MimeMapping.GetMimeMapping(fileName + ".xlsx");
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName + ".xlsx",
                    Inline = true,
                };
                Response.AppendHeader("Content-Disposition", cd.ToString());
                //}
                return File(array, contentType);
            }

            return RedirectToAction("Home");
        }

        public byte[] CreateExcelSheet(string fileName, Dictionary<string, object> dictRpts)
        {
            string strFilePath = string.Empty;
            Workbook wbk = new Workbook(FileFormatType.Xlsx);
            wbk.FileName = fileName;
            wbk.Worksheets.RemoveAt("Sheet1");
            System.Collections.ArrayList arrTotal = new System.Collections.ArrayList();
            DataTable dtParams = new DataTable();
            DataColumn dc = new DataColumn();
            dc.ColumnName = "Parameter";
            dtParams.Columns.Add(dc);
            dc = new DataColumn();
            dc.ColumnName = "Value";
            dtParams.Columns.Add(dc);
            DataRow dr = dtParams.NewRow();
            dr[0] = "<b>Report Run Time</b>";
            dr[1] = DateTime.Now.ToString("dd/MM/yyyy hh:ss:mm");
            dtParams.Rows.Add(dr);

            if (dictRpts.ContainsKey("PARAMS"))
            {
                Dictionary<string, string> dictParams =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(dictRpts["PARAMS"]));
                //arrParams.Add("REPORT PARAMETERS");
                foreach (var param in dictParams)
                {
                    //arrParams.Add("<b>"+param.Key+"</b>" + " : " + param.Value);
                    dr = dtParams.NewRow();
                    dr[0] = "<b>" + param.Key + "</b>";
                    dr[1] = param.Value;
                    dtParams.Rows.Add(dr);
                }
                dictRpts.Remove("PARAMS");
            }
            foreach (var report in dictRpts)
            {
                string strSheetName = report.Key;
                if (!strSheetName.Contains("_TOTAL"))
                {
                    if (strSheetName.Length > 25)
                    {
                        strSheetName = strSheetName.Substring(0, 25);
                    }
                    Worksheet wks = wbk.Worksheets.Add(strSheetName);
                    DataTable dtReport = JsonConvert.DeserializeObject<DataTable>(Convert.ToString(report.Value));
                    ImportTableOptions options = new ImportTableOptions();
                    options.IsHtmlString = true;
                    //if (arrParams.Count > 0) wks.Cells.ImportArrayList(arrParams, 1, 1, true);
                    if (dtParams != null && dtParams.Rows != null && dtParams.Rows.Count > 1)
                    {
                        options.TotalColumns = dtParams.Columns.Count;
                        options.TotalRows = dtParams.Rows.Count;
                        wks.Cells.ImportData(dtParams, 1, 1, options);
                    }
                    options.TotalColumns = dtReport.Columns.Count;
                    options.TotalRows = dtReport.Rows.Count;
                    wks.Cells.ImportData(dtReport, dtParams.Rows.Count + 5, dtParams.Columns.Count + 2, options);

                    if (dictRpts.ContainsKey(strSheetName + "_TOTAL"))
                    {
                        arrTotal.Add("Total Amount: " + dictRpts[strSheetName + "_TOTAL"]);
                        wks.Cells.ImportArrayList(arrTotal, dtParams.Rows.Count + dtReport.Rows.Count + 7, dtParams.Columns.Count + 3, true);
                    }
                }

            }
            byte[] array;
            MemoryStream ms = new MemoryStream();
            wbk.Save(ms, SaveFormat.Xlsx);
            // wbk.Save(ms, SaveFormat.Pdf);
            array = ms.ToArray();
            return array;
        }

        [HttpGet]
        public ActionResult CSR_login(int UserType = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult CSR_login(FormCollection obj)
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

                    if (Session["USER_TYPE_ID"].ToString() == "13")
                    {
                        DataTable dtRDetails = new DataTable();
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        conditions.Add("QID", 80); //check records available or not   
                        conditions.Add("EMAIL_TX", UserName);
                        conditions.Add("FIN_YEAR_ID", obj["FIN_YEAR_ID"]);
                        JObject jdata1 = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");

                        if (jdata1 != null && jdata1.First.First.First != null)
                        {
                            foreach (JProperty property in jdata1.Properties())
                            {
                                if (property.Name == "qfetch")
                                    dtRDetails = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }
                        else
                        {
                            Session.Clear();
                            Session.Abandon();
                            ViewBag.Message = "You are not registered in Current Year. Please do register first using below link.";
                            Message = "You are not registered in Current Year.";
                        }
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

                    return RedirectToAction("Home", "CSR");
                }
            }
            catch (Exception exx) { }
            return View(obj);
        }

        [HttpGet]
        public ActionResult RO_login(int UserType = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult RO_login(FormCollection obj)
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

                    return RedirectToAction("Home", "RO");
                }
            }
            catch (Exception exx) { }
            return View(obj);
        }

        [HttpGet]
        public ActionResult DelegatePortal(int UserType = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult DelegatePortal(FormCollection obj)
        {
            return RedirectToAction("Delegate", "RO");
        }

        [HttpGet]
        public JsonResult GetExamQuestions(string condition, string schema)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);

            if (!string.IsNullOrEmpty(condition))
            {
                string[] splitfirst = condition.Split('/');
                for (int i = 0; i < splitfirst.Length; i++)
                {
                    string[] splitSecond = splitfirst[i].Split('-');
                    if (splitSecond.Length > 0)
                        conditions.Add(splitSecond[0], splitSecond[1]);
                }
            }

            string TableName = "OBJECTIVE_QUESTIONS_T";
            JObject jdata = null;       
            jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, schema != null ? schema : Util.UtilService.getSchemaNameById(1));

            DataTable dtData = new DataTable();
            DataTable newdtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName )
                {
               
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                     dtData.Select().ToList<DataRow>()
                      .ForEach(r => {
                          r["ANSWER_NM"] = 0;
                      });
                }
            }
            object jit = Util.UtilService.DataTableToJSON(dtData);


            return Json(Util.UtilService.DataTableToJSON(dtData), JsonRequestBehavior.AllowGet);
            //return Json(Util.UtilService.DataTableToJSON(dtData));
        }

        public ActionResult SubmitExam(string examid,string studentId, string mylist)
        {
            FormCollection frm = new FormCollection();
            frm["u"] = studentId;
            frm["EXAM_ID"] = examid;  
            frm["MyList"] = mylist;
            StudentLayer s = new StudentLayer();
            s.SubmitExam(5, frm);
            return RedirectToAction("Home");
        }

        public JsonResult CreteExamMaster(string examid,string studentId,string schema)
        {
            FormCollection frm = new FormCollection();
            frm["u"] = studentId;
            frm["EXAM_ID"] = examid;           
            StudentLayer s = new StudentLayer();
            string masterExamId = "";
            s.CreateStudentExam(5, frm,ref masterExamId);
            DataTable dtData = new DataTable();
            dtData.Columns.Add("MasterExam_Id", typeof(int));
            dtData.Rows.Add(Convert.ToInt32(masterExamId));  
            return Json(Util.UtilService.DataTableToJSON(dtData), JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateExamResult(string examid, string studentId, string MarksObtained, string Status)
        {
            FormCollection frm = new FormCollection();
            frm["u"] = studentId;
            frm["EXAM_ID"] = examid;
            frm["TOTAL_MARKS_OBTAINED_NM"] = MarksObtained;
            frm["PASS_YN"] = (Status == "Passed") ? "True" : "False";
            StudentLayer s = new StudentLayer();
            s.UpdateExamResult(5, frm);
            return RedirectToAction("Home");
        }
        public JsonResult ApplyforCoaching(string coachingbatch, string studentId)
        {
            FormCollection frm = new FormCollection();
            frm["u"] = studentId;
            frm["BATCH_ID"] = coachingbatch;
            StudentLayer s = new StudentLayer();
            int AppliedCoachingId = 0;
            int paymentStatus = 0;
            s.ApplyforCoaching(5, frm, ref AppliedCoachingId, ref paymentStatus);
            DataTable dtData = new DataTable();
            dtData.Columns.Add("ID", typeof(int));
            dtData.Columns.Add("PAYMENT_STATUS", typeof(int));
            dtData.Rows.Add(Convert.ToInt32(AppliedCoachingId), Convert.ToInt32(paymentStatus));
            return Json(Util.UtilService.DataTableToJSON(dtData), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 || (Convert.ToString(Session["LOGIN_ID"]) == "pcscomp" || Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6))
            {
                return new JsonResult
                {
                    Data = "Postback on" + " " + DateTime.Now
                };
            }
            else
            {
                return new JsonResult
                {
                    Data = "Session lost  on" + " " + DateTime.Now
                };
            }
        }


        [HttpPost]

        public JsonResult UploadExamQuestions(string datajson)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            // object item = json_serializer.DeserializeObject(datajson);
            var item = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(datajson);
            FormCollection frm = new FormCollection();
            frm["u"] = Convert.ToString(HttpContext.Session["LOGIN_ID"]);
            frm["MODULE_ID"] = item["MODULE_ID"];
            frm["SUBJECT_ID"] = item["SUBJECT_ID"];
            frm["PAPER_ID"] = item["PAPER_ID"];
            frm["TOPIC_ID"] = item["TOPIC_ID"];
            frm["QUESTION_TYPE_ID"] = item["QUESTION_TYPE_ID"];
            frm["QUESTION_TX"] = item["QUESTION_TX"];
            frm["NO_OF_OPTION_NM"] = item["NO_OF_OPTION_NM"];
            frm["OPTION_1_TX"] = item["OPTION_1_TX"];
            frm["OPTION_2_TX"] = item["OPTION_2_TX"];
            frm["OPTION_3_TX"] = item["OPTION_3_TX"];
            frm["OPTION_4_TX"] = item["OPTION_4_TX"];
            frm["OPTION_5_TX"] = item["OPTION_5_TX"];
            frm["ANSWER_NM"] = item["ANSWER_NM"];
            ROLayer rol = new ROLayer();
            ActionClass aclass = rol.InsertQuestions(5, frm);

            return Json(aclass.StatMessage.ToLower());

        }
    }
}
