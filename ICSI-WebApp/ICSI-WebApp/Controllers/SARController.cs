using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using ICSI_Library.Membership;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICSI_WebApp.Controllers
{
    public class SARController : Controller
    {
        // GET: SAR
        public ActionResult Index()
        {
            return View();
        }

        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private string PGUrl = Convert.ToString(ConfigurationManager.AppSettings["PGUrl"]);
        private string PGAppUrl = Convert.ToString(ConfigurationManager.AppSettings["PGAppUrl"]);
        private string PGHDFCUrl = Convert.ToString(ConfigurationManager.AppSettings["PGHDFCUrl"]);
        private ManagementData objApplicationObject;

        [HttpGet]
        public ActionResult SAR_login(int UserType = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult SAR_login(FormCollection obj)
        {
            string UserName = obj["email"];
            string Password = obj["HidPassVal"];
            string OrigPassword = obj["password"];
            string remember = obj["remember"];
            int UserId = 0;
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            string Message = string.Empty;
            try
            {

                #region Consume Service
                AppUrl = AppUrl + "/login";

                string sdata = UtilService.createRequestObject(AppUrl, UserName, Password, UtilService.createParameters("", "", "", "", "", "login", data), out Message);

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
                        if (HttpContext.Application["ManagementData"] == null) objApplicationObject = new ManagementData();

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

                    return RedirectToAction("Home", "SAR");
                }
            }
            catch (Exception ex)
            {
                string strEx = ex.Message;
            }
            return View(obj);
        }

        public ActionResult SAR(FormCollection frm)
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
                        return View("Home", screen);
                        //return View(screen);
                    }
                    else return View();
                }
                else return RedirectToAction("Home", "SAR");
            }
            else
                return RedirectToAction("logout");
        }

        private void login()
        {
            string UserName = "saranonymous";
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
            catch (Exception ex) {
                string strEx = ex.Message;
            }
        }

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("login", "Home");
        }

        [HttpGet]
        public ActionResult Home()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 && Convert.ToString(Session["LOGIN_ID"]) != "saranonymous")
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
                    return View(screen);
                }
                else return View();
            }
            else
                return RedirectToAction("logout");
        }

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

                if (!string.IsNullOrEmpty(screenId))
                {
                    if (screenId.Contains(","))
                    {
                        screenId = screenId.Split(',')[0];
                        frm["si"] = screenId;
                    }
                }

                if (frm["scrtype"] == "update")
                {
                    ScreenType = "update";
                    frm["s"] = ScreenType;
                }

                ActionClass act = new ActionClass();
                if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                {
                    if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"] != string.Empty)
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
            else
                return RedirectToAction("logout");
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
        /// <summary>
        /// Short list SAR application 
        /// </summary>
        /// <param name="appl_id"></param>
        /// <returns></returns>
        public JsonResult ShortlistApplication(int appl_id)
        {
            string strMsg = string.Empty, strHtml = string.Empty;
            int appl_Approval_id = 0,code=0;
            try
            {
                #region SAR approval Info
                Dictionary<string, object> cond = new Dictionary<string, object>();
                cond.Add("ACTIVE_YN", 1);
                cond.Add("APPL_ID", appl_id);
                DataTable dt = UtilService.getData("SAR", "SAR_APPROVAL_T", cond, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    appl_Approval_id = Convert.ToInt32(dt.Rows[0]["ID"]);

                }
                #endregion

                #region shortlist app by updating sar_approval_t table
                cond.Clear();
                Dictionary<string, object> data = new Dictionary<string, object>();
                string UserName = HttpContext.Session["LOGIN_ID"].ToString();
                string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
                ActionClass act = new ActionClass();
                cond.Add("ACTIVE_YN", 1);
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                data.Add("ID", appl_Approval_id);
                data.Add("APPROVE_NM", 2);
                data.Add("UPDATED_BY", UserName);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("SAR", "SAR_APPROVAL_T", 0, 0, lstData1, cond));
                AppUrl = AppUrl + "/AddUpdate";
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));

                if (Convert.ToInt32(act.StatCode) >= 0)
                {
                    strMsg = "Success";
                    code = 1;
                }
                else
                {
                    strMsg = "fail";
                    code = -1;
                }

                #endregion
                return Json(new
                {
                    strMsg = strMsg,
                    strHtml = strHtml,
                    code = code
                });
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    strMsg = "Un expected issue occurred",
                    strHtml = strHtml,
                    code = -1
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="uniqueid"></param>
        /// <returns></returns>
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
        /// Update SAR Winner
        /// </summary>
        /// <param name="appl_id"></param>
        /// <returns></returns>
        public JsonResult UpdateSARWinner(int appl_id)
        {
            string strMsg = string.Empty, strHtml = string.Empty;
            int appl_Approval_id = 0, code = 0,sar_reg_id = 0;
            try
            {
                #region get SAR approval Info
                Dictionary<string, object> cond = new Dictionary<string, object>();
                cond.Add("ACTIVE_YN", 1);
                cond.Add("APPL_ID", appl_id);
                DataTable dt = UtilService.getData("SAR", "SAR_APPROVAL_T", cond, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    appl_Approval_id = Convert.ToInt32(dt.Rows[0]["ID"]);
                    sar_reg_id = Convert.ToInt32(dt.Rows[0]["SAR_AWARDS_REG_ID"]);
                }
                #endregion

                #region Updating winner details
                cond.Clear();
                Dictionary<string, object> data = new Dictionary<string, object>();
                string UserName = HttpContext.Session["LOGIN_ID"].ToString();
                string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
                ActionClass act = new ActionClass();
                ActionClass act1 = new ActionClass();
                ActionClass act2 = new ActionClass();
                cond.Add("ACTIVE_YN", 1);
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                data.Add("ID", appl_Approval_id);
                data.Add("APPROVE_NM", 4);
                data.Add("UPDATED_BY", UserName);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("SAR", "SAR_APPROVAL_T", 0, 0, lstData1, cond));
                AppUrl = AppUrl + "/AddUpdate";
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));

                data.Clear();
                lstData1.Clear();
                lstData.Clear();
                data.Add("ID", appl_id);
                data.Add("STATUS_TX", "Winner");
                data.Add("UPDATED_BY", UserName);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("SAR", "SA_GENERAL_INFO_T", 0, 0, lstData1, cond));
                act1 = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                #endregion

                #region get Award details
                ICSI_WebApp.BusinessLayer.SARLayer.GetActiveYear();
                int icoolingPeriod = 0,iyear = 0;
                cond.Clear();
                cond.Add("ACTIVE_YN", 1);
                cond.Add("FIN_YEAR_ID",System.Web.HttpContext.Current.Session["ACTIVE_YR_ID"]);
                JObject jData = null;
                jData = DBTable.GetData("fetch", cond, "SAR_AWARDS_T", 0, 100, "SAR");
                dt = null;
                if (jData != null)
                {
                    foreach (JProperty property in jData.Properties())
                    {
                        if (property.Name.Equals("SAR_AWARDS_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["COOLING_PERIOD"] != DBNull.Value) icoolingPeriod = Convert.ToInt32(dr["COOLING_PERIOD"]);
                        if (dr["FIN_YEAR_ID"] != DBNull.Value) iyear = Convert.ToInt32(dr["FIN_YEAR_ID"]);
                    }
                }

                #endregion

                #region get registration details
                string strCop = string.Empty, strName = string.Empty, strFirmName = string.Empty;
                cond.Clear();
                cond.Add("ACTIVE_YN", 1);
                cond.Add("USER_ID", sar_reg_id);
                cond.Add("FIN_YEAR_ID", System.Web.HttpContext.Current.Session["ACTIVE_YR_ID"]);
                dt = UtilService.getData("SAR", "SAR_REGISTRATION_T", cond, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    strCop = Convert.ToString(dt.Rows[0]["COP_NO_TX"]);
                    strName = Convert.ToString(dt.Rows[0]["NAME_TX"]);
                    strFirmName = Convert.ToString(dt.Rows[0]["FIRM_NAME_TX"]);
                }

                #endregion

                #region insert into cooling period record
                data.Clear();
                lstData1.Clear();
                lstData.Clear();
                data.Add("COP_NO_TX", strCop);
                data.Add("MEMBERSHIP_NO_TX", string.Empty);
                data.Add("NAME_TX", strName);
                data.Add("FIRM_NAME_TX", strFirmName);
                data.Add("YEAR_AWARDED",iyear);
                data.Add("COOLING_PERIOD", icoolingPeriod);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("SAR", "SAR_COOLING_PERIOD_T", 0, 0, lstData1, cond));
                act2 = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                #endregion

                if (Convert.ToInt32(act.StatCode) >= 0 && Convert.ToInt32(act1.StatCode) >= 0 && Convert.ToInt32(act2.StatCode) >= 0)
                {
                    strMsg = "Success";
                    code = 1;
                }
                else
                {
                    strMsg = "fail";
                    code = -1;
                }

                return Json(new
                {
                    strMsg = strMsg,
                    strHtml = strHtml,
                    code = code
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    strMsg = "Un expected issue occurred",
                    strHtml = strHtml,
                    code = -1
                });
            }
        }

        public JsonResult ValidateUserReg(string copNo,string firmName,string name)
        {            
            try
            {
                ICSI_WebApp.BusinessLayer.SARLayer.GetActiveYear();
                int code = 0;

                #region get active year details
                Dictionary<string, object> cond = new Dictionary<string, object>();
                int activefinYear = 0;
                cond.Add("ACTIVE_YN", 1);
                cond.Add("ID", System.Web.HttpContext.Current.Session["ACTIVE_YR_ID"]);
                
                DataTable dt = UtilService.getData("SAR", "FINANCIAL_YEAR_T", cond, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["FIN_YEAR_TX"] != DBNull.Value) activefinYear = Convert.ToInt32(dt.Rows[0]["FIN_YEAR_TX"]);
                }
                #endregion
                
                #region chk user is in cooling period
                cond.Clear();
                dt = null;
                cond.Add("ACTIVE_YN", 1);
                cond.Add("COP_NO_TX", copNo);
                cond.Add("NAME_TX", name);                

                dt = UtilService.getData("SAR", "SAR_COOLING_PERIOD_T", cond, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int awardYr = 0;
                    if (dt.Rows[0]["YEAR_AWARDED"] != DBNull.Value) awardYr = Convert.ToInt32(dt.Rows[0]["YEAR_AWARDED"]);
                    if (dt.Rows[0]["COOLING_PERIOD"] != DBNull.Value)
                    {
                        if(activefinYear > awardYr + Convert.ToInt32(dt.Rows[0]["COOLING_PERIOD"]))
                        {
                            code = 1;
                        }
                        else
                        {
                            code = -1;
                        }
                    }
                }
                else
                {
                    code = 1;
                }
                #endregion

                #region chk firm is in cooling period
                if (code >= 0)
                {
                    cond.Clear();
                    dt = null;
                    cond.Add("ACTIVE_YN", 1);
                    cond.Add("FIRM_NAME_TX", firmName);

                    dt = UtilService.getData("SAR", "SAR_COOLING_PERIOD_T", cond, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        int awardYr = 0;
                        if (dt.Rows[0]["YEAR_AWARDED"] != DBNull.Value) awardYr = Convert.ToInt32(dt.Rows[0]["YEAR_AWARDED"]);
                        if (dt.Rows[0]["COOLING_PERIOD"] != DBNull.Value)
                        {
                            if (activefinYear > awardYr + Convert.ToInt32(dt.Rows[0]["COOLING_PERIOD"]))
                            {
                                code = 1;
                            }
                            else
                            {
                                code = -1;
                            }
                        }
                    }
                    else
                    {
                        code = 1;
                    }
                }
                #endregion
                return Json(new
                {
                    fin_year_id = System.Web.HttpContext.Current.Session["ACTIVE_YR_ID"],
                    code = code
                });
            }

            catch (Exception ex)
            {
                return Json(new
                {
                    fin_year_id = System.Web.HttpContext.Current.Session["ACTIVE_YR_ID"],
                    code = -1
                });
            }

        }

        public JsonResult GetMemberDetails(string cop_No, string dob)
        {
            string strCopNo = string.Empty, strMembershipNo = string.Empty, strDob = string.Empty, strMobileNum = string.Empty, strEmail = string.Empty;
            string strMemberName = string.Empty;
            bool bIsValid = false;
            Dictionary<string, object> conds = new Dictionary<string, object>();
            Service service = new Service();
            ICSIDataCOP membersdata = service.ValidateMemberCOP(cop_No, dob);

            if (membersdata != null && string.IsNullOrEmpty(membersdata.MSg))
            {
                if (!string.IsNullOrEmpty(membersdata.CP_NO)) strCopNo = membersdata.CP_NO;
                else strCopNo = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.MembershipNo)) strMembershipNo = membersdata.MembershipNo;
                else strMembershipNo = string.Empty;
                //if (!string.IsNullOrEmpty(membersdata.FirstName + " " + membersdata.LastName)) strMemberName = membersdata.FirstName + " " + membersdata.LastName;
                if (!string.IsNullOrEmpty(membersdata.Name)) strMemberName = membersdata.Name;
                else strMemberName = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.Mobile)) strMobileNum = membersdata.Mobile;
                else strMobileNum = string.Empty;
                if (!string.IsNullOrEmpty(membersdata.EmailID)) strEmail = membersdata.EmailID;
                else strEmail = string.Empty;
                bIsValid = true;

            }
            else bIsValid = false;
            conds.Clear();
            conds.Add("COP_NO_TX", cop_No);
            conds.Add("DATE_OF_BIRTH_TX", dob);
            DataTable dtRes = Util.UtilService.getData("SAR", "SAR_REGISTRATION_T", conds, null, 0, 10);
            bool isUserRegistered = false;
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0) isUserRegistered = true;
            var jsonobj = new
            {
                strCopNo = strCopNo,
                strMemberName = strMemberName,
                strMembershipNo = strMembershipNo,
                strMobileNum = strMobileNum,
                strEmail = strEmail,
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

        [HttpGet]
        public JsonResult ValidateRegistration(string email)
        {
            Dictionary<string, object> conds = new Dictionary<string, object>();
            Service service = new Service();
            bool isUserRegistered = false;

            conds.Add("LOGIN_ID", email);
            DataTable dtUserData = Util.UtilService.getData("Stimulate", "USER_T", conds, null, 0, 0);
            if (dtUserData != null && dtUserData.Rows != null && dtUserData.Rows.Count > 0)
            {
                if (dtUserData.Rows[0]["USER_TYPE_ID"] != DBNull.Value && Convert.ToInt32(dtUserData.Rows[0]["USER_TYPE_ID"]) == 24)

                {
                    isUserRegistered = true;
                }
            }

            if (!isUserRegistered)
            {
                conds.Clear();


                conds.Add("SHOW_YEAR_YN", "true");
                DataTable dtRes = Util.UtilService.getData("SAR", "FINANCIAL_YEAR_T", conds, null, 0, 0);
                int ID = Convert.ToInt32(dtRes.Rows[0]["ID"]);
                if (!string.IsNullOrEmpty(Convert.ToString(ID)))
                {
                    conds.Clear();
                    conds.Add("FIN_YEAR_ID", ID);
                    conds.Add("EMAIL_TX", email);
                    DataTable dtRest = Util.UtilService.getData("SAR", "SAR_REGISTRATION_T", conds, null, 0, 0);

                    if (dtRest != null && dtRest.Rows != null && dtRest.Rows.Count > 0) isUserRegistered = true;
                }
            }
            var jsonobj = new
            {
                isUserRegistered = isUserRegistered,

            };

            return Json(isUserRegistered, JsonRequestBehavior.AllowGet);
        }

    }
}