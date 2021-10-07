using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ICSI_WebApp.Controllers
{
    public class CGAController : Controller
    {
        // GET: CGA
        // GET: CSRAwards
        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private string PGUrl = Convert.ToString(ConfigurationManager.AppSettings["PGUrl"]);
        private string PGAppUrl = Convert.ToString(ConfigurationManager.AppSettings["PGAppUrl"]);
        private string PGHDFCUrl = Convert.ToString(ConfigurationManager.AppSettings["PGHDFCUrl"]);
        private ManagementData objApplicationObject;

        [HttpGet]
        public ActionResult CGA_login(int UserType = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult CGA_login(FormCollection obj)
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

                    if (Session["USER_TYPE_ID"].ToString() == "16")
                    {
                        DataTable dtRDetails = new DataTable();
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        conditions.Add("QID", 98); //check records available or not   
                        conditions.Add("EMAIL_TX", UserName);
                        conditions.Add("FIN_YEAR_ID", obj["FIN_YEAR_ID"]);
                        JObject jdata1 = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

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

                    return RedirectToAction("Home", "CGA");
                }
            }
            catch (Exception exx) { }
            return View(obj);
        }

        public ActionResult CGA(FormCollection frm)
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
                        return View(screen);
                    }
                    else return View();
                }
                else return RedirectToAction("Home", "CGA");
            }
            else
                return RedirectToAction("logout");
        }

        private void login(string UserName = "cgaanonymous", string Password = "5f4dcc3b5aa765d61d8327deb882cf99")
        {
            //string UserName = "cgaanonymous";
            //string Password = "5f4dcc3b5aa765d61d8327deb882cf99";
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

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("CGA_login", "CGA");
        }

        [HttpGet]
        public ActionResult Home()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 && Convert.ToString(Session["LOGIN_ID"]) != "cgaanonymous")
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

        [HttpPost]
        public ActionResult Home(FormCollection frm)
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 || (Convert.ToString(Session["LOGIN_ID"]) == "pcscomp" || Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6))
            {
                string userid = frm["u"];
                string menuid = frm["m"];
                string ScreenType = frm["s"];
                string screenId = frm["si"];
                if (string.IsNullOrEmpty(Convert.ToString(frm["si"])))
                {
                    screenId = frm["screenid"];
                    frm["si"] = screenId;
                }
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
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, splitTBL[1]);
            }
            else
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, schema != null ? schema : Util.UtilService.getSchemaNameById(1));

            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                {
                    //dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            return Json(Util.UtilService.DataTableToJSON(dtData));
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
                if (!string.IsNullOrEmpty(Convert.ToString(Request.Form["ICSI_SCHEMA"])))
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

        public ActionResult ExportToExcel(int Id)
        {
            int CurrentYear = 0;
            string FileName = string.Empty;
            if (Session["ACTIVE_ARCHIVE_YEAR_ID"] == null)
                BusinessLayer.CGALayer.GetActiveYear();
            CurrentYear = Convert.ToInt32(Session["ACTIVE_ARCHIVE_YEAR"]);

            if (Id == 1)
                FileName = "CGA-Registered-Companies-" + CurrentYear + ".xls";
            else if (Id == 2)
                FileName = "Statutory-Auditor-Response-" + CurrentYear + ".xls";
            else if (Id == 3)
                FileName = "Secretarial-Auditor-Response-" + CurrentYear + ".xls";
            else if (Id == 4)
                FileName = "Independent-Director-Response-" + CurrentYear + ".xls";

            var gv = new GridView();
            if (Id == 1)
                gv.DataSource = BusinessLayer.CGALayer.RegisteredCompanies();
            else if (Id == 2 || Id == 3 || Id == 4)
                gv.DataSource = BusinessLayer.CGALayer.GetAuditorsDirectors(Id);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + FileName + "");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View("Home");
        }

        public ActionResult GetLink(string ut, string uid)
        {
            string UserName = "cgaanonymous";
            string Password = "5f4dcc3b5aa765d61d8327deb882cf99";
            if (Convert.ToInt32(ut) == 1)
            {
                var PasswordData = GetDropDownData("USER_T", "ID-" + uid + "", "Stimulate");
                //Password = PasswordData.Data.ToString().Split(',')[5].Split(':')[1].Replace("\"", "");
                UserName = PasswordData.Data.ToString().Split(',')[4].Split(':')[1].Replace("\"", "");
            }
            if (Session["LOGIN_ID"] == null)
            {
                login(UserName, Password);
            }
            FormCollection frm = new FormCollection();
            frm.Add("u", uid);
            frm.Add("ut", ut);
            frm.Add("m", "");
            frm.Add("s", "new");
            frm.Add("CGA_AWARDS_REG_ID", uid);
            switch (Convert.ToInt32(ut))
            {
                case 1:
                    frm.Add("si", "337");
                    break;
                case 2:
                    frm.Add("si", "316");
                    break;
                case 3:
                    frm.Add("si", "317");
                    break;
                case 4:
                    frm.Add("si", "325");
                    break;
                default:
                    break;
            }
            var UserType = string.IsNullOrEmpty(ut) ? 0 : Convert.ToInt32(ut);
            var UserId = string.IsNullOrEmpty(uid) ? 0 : Convert.ToInt32(uid);
            string userid = frm["u"];
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null)
            {
                ActionClass act = new ActionClass();
                if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                {
                    Screen_T screen = UtilService.homeAction(WEB_APP_ID, frm, userid, menuid, screenId, ScreenType, act, (List<object>)Session["USER_MENU"]);
                    if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                    if (screen != null)
                    {
                        ViewBag.Title = screen.Screen_Title_Tx;
                        ViewBag.MenuId = menuid;
                        ViewBag.uid = uid;
                        ViewBag.ut = ut;
                        ViewBag.ActionClass = act;
                        if (Convert.ToInt32(ut) == 1)
                            return RedirectToAction("Home", "CGA");
                        else
                            return View(screen);
                    }
                    else return View();
                }
                else return RedirectToAction("Home", "GetLink");
            }
            else
                return RedirectToAction("logout");
        }

        public ActionResult DownloadFile(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.Split('_')[1]);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            if (id.Split('_')[0] == "8")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.CGA_AWARDS_SUPPORTING_DOCS_T);
                FilePath = ConfigurationManager.AppSettings["DOCUMENT_ROOT"].ToString() + Convert.ToString(Util.UtilService.FilePath.CGAAwardsSupportingDocs.GetEnumDisplayName());
            }

            JObject jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, "CGA");

            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }

            string filepath = string.Empty;
            string FileName = string.Empty;
            int attID = 0;
            if (dtData.Rows.Count > 0)
            {
                filepath = Convert.ToString(dtData.Rows[0]["DOC_PATH_TX"]);
                attID = Convert.ToInt32(dtData.Rows[0]["ID"]);
                FileName = filepath.Replace(FilePath, "").Replace("\\", "");
                filepath = FilePath + attID + "_" + FileName;
            }
            //string filepath = UtilService.getDocumentPath(FilePath) + id.Split('_')[1] + "_" + filename;

            //string[] arr = filepath.Split('\\\\');

            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = FileName,
                    Inline = true,
                };

                return new FileStreamResult(new MemoryStream(filedata), contentType) { FileDownloadName = FileName };
            }
            return HttpNotFound();
        }
    }
}