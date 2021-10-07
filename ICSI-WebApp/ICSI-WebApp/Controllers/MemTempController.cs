using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json.Linq;
using ICSI_Library.Membership;
using Newtonsoft.Json;

namespace ICSI_WebApp.Controllers
{
    public class MemTempController : Controller
    {
        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private ManagementData objApplicationObject;

        public ActionResult Renewal(FormCollection frm)
        {
            if (Session["LOGIN_ID"] == null)
                login();

            if (string.IsNullOrEmpty(frm["u"]) || string.IsNullOrEmpty(frm["s"]) || string.IsNullOrEmpty(frm["si"]))
            {
                frm["u"] = "memberrenewal";
                if (!string.IsNullOrEmpty(frm["si"]))
                {
                    frm["si"] = frm["si"];
                }
                else
                {
                    frm["si"] = "507";
                }
                frm["s"] = "new";
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
                else return RedirectToAction("logout");
                //else return RedirectToAction("Renewal");
            }
            else
                return RedirectToAction("logout");
        }

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("login");
        }

        private void login()
        {
            string UserName = "memberrenewal";
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
                if (jdata != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "ajax")
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
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

        public JsonResult GetMembershipDetails(string membershipNM = null, string DOB = null)
        {
            MembershipDetails membershipData = new MembershipDetails();
            ICSIDataMembersRenewalStimulate data = membershipData.GetMemberDetails(membershipNM, DOB);
            var resp = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Json(resp);
        }

        /// <summary>
        /// get all payment transactions done by member
        /// </summary>
        /// <param name="membershipNM"></param>
        /// <returns></returns>
        public JsonResult GetTransactions(string membershipNM,int screen_comp_id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", screen_comp_id);
            conditions.Add("UID", membershipNM);


            JObject jdata = DBTable.GetData("query", conditions, "SCREEN_COMP_T", 0, 100,"stimulate");

            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "query")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                List<string> cols = new List<string>();                
                string strCompContentTx = "RADIO,TRANSACTION_ID,RECEIPT_FK_ID,AMOUNT,PAYMENT_INITIATION_DATE_TX,PG_RETURN_TRANSACTION_ID,STATUS_TX";
                cols = strCompContentTx.Split(',').ToList<string>();

                List<string> colvals = new List<string>();
                string strcompValueTx = "RADIO,Transaction id,Receipt Number,Amount,Payment date,Bank Reference Number,Status";
                colvals = strcompValueTx.Split(',').ToList<string>();

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
                            sb.Append("<div class='panelSpacing' id='dvSearchResultsPanel'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>")
                                .Append("Transaction History</span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");                            
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
                    sb.Append("</thead></tr><tbody id='pagesearchtbid'>");

                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                    {
                            sb.Append("<tr>");                            
                            if (isRADIO) sb.Append("<td><input type=\"radio\" name='radioList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"radio_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string cn = dt.Columns[j].ColumnName;
                                int pos = cols.IndexOf(cn);
                                if (pos > -1)
                                    sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                            }
                            sb.Append("</tr>");
                        
                    }

                    sb.Append("</tbody></table></div></div></div></div>");                    
                }
                return Json(sb.ToString());
            }            
            else
                return Json("");
        }

        public JsonResult GetPCHDetails(string membershipNM)
        {
            MembershipDetails membershipData = new MembershipDetails();
            CREIDTHOURS data = membershipData.GetPCHDetails(membershipNM);
            var resp = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Json(resp);
        }
    }
}