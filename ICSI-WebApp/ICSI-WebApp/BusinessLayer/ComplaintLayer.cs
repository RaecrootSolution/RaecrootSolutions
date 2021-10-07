using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static ICSI_WebApp.Util.UtilService;
namespace ICSI_WebApp.BusinessLayer
{
    public class ComplaintLayer
    {

        public static void SessionHandling(string TableName, string complaintNo, string ID = null)
        {
            Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
            complaintNo = complaintNo.PadLeft(10, '0');
            HttpContext.Current.Session["UNIQUE_REG_ID"] = complaintNo.ToString();
            object[] sessionvalues = new object[2];
            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
                object sessionval;
                if (HttpContext.Current.Session["SESSION_OBJECTS"] != null && (sessionObjs.TryGetValue(TableName, out sessionval)))
                {
                    if (sessionObjs.TryGetValue(TableName, out sessionval))
                    {
                        sessionvalues = sessionval as object[];
                        sessionvalues[0] = complaintNo.ToString();
                        sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                        sessionObjs.Remove(TableName);
                        sessionObjs.Add(TableName, sessionvalues);
                        HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                    }
                }
                else
                {
                    sessionvalues[0] = complaintNo.ToString();
                    sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                    sessionObjs.Add(TableName, sessionvalues);
                    HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                }
            }
            else
            {
                sessionvalues[0] = complaintNo.ToString();
                sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                sessionObjs.Add(TableName, sessionvalues);
                HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
            }
        }

        public ActionClass beforeComplaintRegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }
        public ActionClass afterComplaintRegistration(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";

            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string strComplaintNo = GetUqComplaintNo(screen, frm);
            if (strComplaintNo == string.Empty)
            {
                actionClass = Util.UtilService.GetMessage(-202);
            }
            else
            {
                frm["COMPLAINT_NO_TX"] = strComplaintNo;
                frm["STATUS_TX"] = "open";
                frm["COMPLAINT_OPEN_DT"] = DateTime.Now.ToShortDateString();

                //SessionHandling(screen.Table_Name_Tx, strComplaintNo);
                actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                int ID = 0;
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == screen.Table_Name_Tx)
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                if (ID > 0)
                {
                    frm["ui"] = Convert.ToString(ID);
                    frm["UniqueId"] = Convert.ToString(ID);
                    UpdateComplaintHistoryData(UserName, Session_Key, screen, frm, "Your complaint has been registered successfully !");
                }

                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            }
            return actionClass;
        }

        private void UpdateComplaintHistoryData(string UserName, string Session_Key, Screen_T screen, FormCollection frm, string remarks)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);

            if (!string.IsNullOrEmpty(frm["COMPLAINT_NO_TX"]))
            {

                //Store data in Complaint_Email_History
                data.Add("EMAIL_DESC_TX", remarks);
                data.Add("EMAIL_TO", frm["EMAIL_ID_TX"]);
                data.Add("EMAIL_SENT_DT", DateTime.Now);
                data.Add("COMPLAINT_ID", frm["ui"]);

                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "COMPLAINT_EMAIL_HISTORY_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                data.Clear();
                lstData.Clear();
                lstData1.Clear();
                data.Add("SMS_DESC_TX", remarks.ToString());
                data.Add("SMS_SENT_TO_NM", frm["MOBILE"]);
                data.Add("COMPLAINT_ID", frm["ui"]);
                data.Add("SMS_SENT_DT", DateTime.Now);

                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "COMPLAINT_SMS_HISTORY_T", 0, 0, lstData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
            }
        }

        private static string GetUqComplaintNo(Screen_T screen, FormCollection frm)
        {
            string strCompNo = string.Empty;
            bool isPassed = false;
            if (frm["COMPLAINT_NO_TX"] != null && !Convert.ToString(frm["COMPLAINT_NO_TX"]).Trim().Equals(""))
            {
                isPassed = true;
                strCompNo = frm["COMPLAINT_NO_TX"];
            }
            while (!isPassed)
            {
                string uq = UtilService.GenerateRandomComplaintNo();
                if (isComplaintNumExists(strCompNo, screen))
                {
                    isPassed = true;
                    strCompNo = uq;
                }
            }
            return strCompNo;
        }

        private static bool isComplaintNumExists(string strCompNo, Screen_T screen)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("COMPLAINT_NO_TX", strCompNo);
            DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else return false;
        }

        public ActionClass beforeComplaintDetailView(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }

        public ActionClass afterComplaintDetailView(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";

            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (frm["STATUS_TX"].ToString() == "close") frm["COMPLAINT_CLOSE_DT"] = DateTime.Now.ToShortDateString();
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            //string strStatus = frm["STATUS_TX"].ToString();
            UpdateComplaintHistoryData(UserName, Session_Key, screen, frm, frm["LAST_COMMENTS_TX"].ToString());
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return actionClass;
        }
    }
}