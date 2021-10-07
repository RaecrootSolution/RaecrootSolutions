using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace ICSI_WebApp.BusinessLayer
{
    public class SARLayer
    {
        
        public ActionClass beforeForgotPassword(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@RandomPassword", RandomPassword());
            return act;
        }

        public ActionClass afterForgotPassword(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string EncryptPass = frm["RANDOM_PASSWORD"];
            string OrignalPass = frm["RANDOM_ORG_PASSWORD"];

            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();            
            string _email = frm["EMAIL_TX"];
            int UID = 0;

            if (_email != null && !_email.Trim().Equals(""))

            {
                DataTable dtRegDtls = new DataTable();
                Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("EMAIL_TX", _email);
                conditions.Add("ACTIVE_YN", 1);
                dtRegDtls = UtilService.getData("SAR", "SAR_REGISTRATION_T", conditions, null, 0, 100);
                if (dtRegDtls.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRegDtls.Rows.Count; i++)
                    {
                        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        d["ID"] = dtRegDtls.Rows[i]["ID"].ToString();
                        d["PASSWORD_TX"] = EncryptPass;
                        d["ORG_PASSWORD_TX"] = OrignalPass;

                        list.Add(d);
                        actionClass = UtilService.insertOrUpdate("SAR", "SAR_REGISTRATION_T", list);

                        // Update password in USER_T
                        if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
                        {
                            List<Dictionary<string, object>> list_Screen_T = new List<Dictionary<string, object>>();
                            Dictionary<string, object> d1 = new Dictionary<string, object>();
                            d1["ID"] = dtRegDtls.Rows[i]["USER_ID"].ToString();
                            d1["LOGIN_PWD_TX"] = EncryptPass;
                            UID = Convert.ToInt32(dtRegDtls.Rows[i]["USER_ID"]);
                            list_Screen_T.Add(d1);

                            actionClass = UtilService.insertOrUpdate("STIMULATE", "USER_T", list_Screen_T);
                        }
                    }

                    #region For Email
                    if (s.is_Email_yn == true)
                        Util.UtilService.storeEmailData(actionClass, s, "update", AppUrl, Session_Key, UserName, UID);
                    #endregion
                }
                else
                {
                    s.Screen_Content_Tx = s.Screen_Content_Tx.Replace("#ERRMessage", _email + " is not registered for this award year. Click here to <a class='text-primary' href='..//SAR// logout'>login</a>");
                }
            }
            return actionClass;
        }

        public ActionClass beforeGeneralInstructions(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string strAwardToDate = string.Empty;
            int fin_yr_id = 0;
            GetActiveYear();

            if (!string.IsNullOrEmpty(frm["ID"])) HttpContext.Current.Session["APPL_Id"] = frm["ID"];

            #region Get SAR Awards Reg Id
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("USER_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));

            JObject jData = null;
            jData = DBTable.GetData("fetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
            DataTable dt = null;
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name.Equals(screen.Table_Name_Tx)) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                HttpContext.Current.Session["SAR_AWARDS_REG_ID"] = dt.Rows[0]["USER_ID"];
            #endregion

            #region Get latest SAR Award
            conditions.Clear();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("FIN_YEAR_ID", HttpContext.Current.Session["ACTIVE_YR_ID"]);
            jData = null;
            jData = DBTable.GetData("fetch", conditions, "SAR_AWARDS_T", 0, 100, applicationSchema);
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
                foreach(DataRow dr in dt.Rows)
                {
                    if(dr["LIVE_TO_DT"] != DBNull.Value)
                    {
                        HttpContext.Current.Session["AWARD_ID"] = Convert.ToInt32(dr["ID"]);
                        if (dr["FIN_YEAR_ID"] != DBNull.Value) fin_yr_id = Convert.ToInt32(dr["FIN_YEAR_ID"]);
                        if (dr["LIVE_TO_DT"] != DBNull.Value) strAwardToDate = Convert.ToDateTime(dr["LIVE_TO_DT"]).ToShortDateString();
                    }
                }
            }
            HttpContext.Current.Session["ACTIVE_YR_ID"] = fin_yr_id;
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@LIVE_FROM_DT", strAwardToDate);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YR_ID", Convert.ToString(fin_yr_id));            
            #endregion

            #region Proceed button hide and show
            conditions.Clear();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("SAR_AWARDS_REG_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));
            conditions.Add("FIN_YR_ID",fin_yr_id);

            jData = null;
            jData = DBTable.GetData("fetch", conditions, "SA_GENERAL_INFO_T", 0, 10, applicationSchema);
            dt = null;
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name.Equals("SA_GENERAL_INFO_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count >= 5)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "style='display:none;'");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "");

            if (!string.IsNullOrEmpty(frm["ID"])) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", frm["ID"]);
            else screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", string.Empty);
            #endregion

            return null;
        }

        public ActionClass beforeGeneralInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int SAR_AWARDS_REG_ID = GetSARAwardsRegId(Convert.ToString(frm["ui"]));
            if (HttpContext.Current.Session["ACTIVE_YR_ID"] == null) GetActiveYear();
            if (!string.IsNullOrEmpty(frm["ID"])) HttpContext.Current.Session["APPL_Id"] = frm["ID"];
            
            StringBuilder sbHtml = new StringBuilder();

            #region Get Information components
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>();

            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("FIN_YR_ID", 1);
            conditions.Add("SCREEN_ID", screen.ID);

            JObject jData = DBTable.GetData("fetch", conditions,"SAR_SCREEN_COMP_T", 0, 1000, "SAR");
            DataTable dtData = new DataTable();
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name == "SAR_SCREEN_COMP_T")
                    {
                        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }
            
            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
            {
                screenComponents = new List<Screen_Comp_T>();
                foreach (DataRow row in dtData.Rows)
                {
                    screenComponents.Add(getScreenComponent(row, "SAR","SAR","SAR","SAR"));
                }
            }
            screenComponents = screenComponents.OrderBy(o => o.Order_Nm).ToList<Screen_Comp_T>();
            string strHtml = string.Empty;
            strHtml = renderScreenComponent(screen, screenComponents, frm);            
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvInformationHtml", strHtml.ToString());

            #endregion

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", Convert.ToString(SAR_AWARDS_REG_ID));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YR_ID", Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@AWARD_ID", Convert.ToString(HttpContext.Current.Session["AWARD_ID"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@USER_TYPE_ID", Convert.ToString(HttpContext.Current.Session["USER_TYPE_ID"]));

            if (Convert.ToString(frm["s"])!="new") GetScreenData(frm, screen);

            //Get_Submitted_Data(frm, screen);
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterGeneralInformation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))frm["s"] = "update";
            if (string.IsNullOrEmpty(Convert.ToString(frm["STATUS_TX"])) && frm["STATUS_TX"] != "Submitted" && frm["STATUS_TX"] != "Winner")
            {
                frm["STATUS_TX"] = "Not Submitted";
            }
            HttpContext.Current.Session["STATUS_TX"] = frm["STATUS_TX"];
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            //if (frm["STATUS_TX"] == "" || frm["STATUS_TX"] == "Not Submitted")
            //{
                Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                int ID = 0;
                if (Convert.ToInt32(act.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(act.DecryptData));
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

                //Enter Partner information
                if (ID > 0)
                {
                    HttpContext.Current.Session["APPL_Id"] = ID;
                    string strAppRefNo = string.Concat(Convert.ToString(frm["PROPREITER_PARTNER_FIRM"]).Substring(0, 5)
                        , Convert.ToString(ID));
                    HttpContext.Current.Session["APPLICATION_REF_NO_TX"] = strAppRefNo;
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    Dictionary<string, object> data = new Dictionary<string, object>();

                    if (frm["NAME_TX"] != null && frm["NAME_TX"] != string.Empty)
                    {

                        string strPartnerName = frm["NAME_TX"];//frm["hNAME"];
                        string strMemNo = frm["MEMSHIP_NO_TX"];//frm["hMEM_NO"].ToString();
                        string strCopNo = frm["COP_NO_TX"];//frm["hCOP_NO"].ToString();
                        string strPartner = frm["PARTNER_ID"];

                        var name = (strPartnerName ?? string.Empty).Split(',');
                        var memNo = (strMemNo ?? string.Empty).Split(',');
                        var copNo = (strCopNo ?? string.Empty).Split(',');
                        var pId = (strPartner ?? string.Empty).Split(',');

                        string screenType = string.Empty;

                        for (int i = 0; i < name.Length; i++)
                        {
                            if (name[i].ToString() != string.Empty)
                            {
                                data.Add("APPL_ID", ID);
                                data.Add("NAME_TX", name[i].ToString());
                                data.Add("MEMSHIP_NO_TX", memNo[i].ToString());
                                data.Add("COP_NO_TX", copNo[i].ToString());
                                data.Add("SAR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
                                if (Convert.ToInt32(pId[i]) > 0)
                                {
                                    data.Add("ID", pId[i]);
                                    screenType = "update";
                                }
                                else
                                {
                                    screenType = "insert";
                                }
                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("SAR", "SA_PARTNERS_INFO_T", 0, 0, lstData1, conditions));
                                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType, lstData));

                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }
                }

            //}
            else
            {
                act.StatCode = "0";
                act.StatMessage = "success";
            }
            return act;
        }

        public ActionClass beforeObjectiveQues(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", Convert.ToString(GetSARAwardsRegId("")));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", Convert.ToString(HttpContext.Current.Session["APPL_Id"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@STATUS_TX", Convert.ToString(HttpContext.Current.Session["STATUS_TX"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.OBJECTIVE_QUESTIONS), 260, 1,frm);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());            
            return act;            
        }

        public ActionClass afterObjectiveQues(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeSubjectiveQues(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", Convert.ToString(HttpContext.Current.Session["APPL_Id"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@STATUS_TX", Convert.ToString(HttpContext.Current.Session["STATUS_TX"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.SUBJECTIVE_QUESIONS), 262, 2,frm);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterSubjectiveQues(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforePreview(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));            
            if (string.IsNullOrEmpty(frm["ID"]))
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", Convert.ToString(HttpContext.Current.Session["APPL_Id"]));
                frm["ID"] = Convert.ToString(HttpContext.Current.Session["APPL_Id"]);
            }            
            //else frm["ID"] = "1";
            
            #region Bind Gen Info
            int SAR_AWARDS_REG_ID = GetSARAwardsRegId(Convert.ToString(frm["ui"]));
            if (HttpContext.Current.Session["ACTIVE_YR_ID"] == null) GetActiveYear();

            StringBuilder sbHtml = new StringBuilder();

            #region Get Inform components
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>();

            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("FIN_YR_ID", 1);
            conditions.Add("SCREEN_ID", screen.ID);

            JObject jData = DBTable.GetData("fetch", conditions, "SAR_SCREEN_COMP_T", 0, 1000, "SAR");
            DataTable dtData = new DataTable();
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name == "SAR_SCREEN_COMP_T")
                    {
                        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
            {
                screenComponents = new List<Screen_Comp_T>();
                foreach (DataRow row in dtData.Rows)
                {
                    screenComponents.Add(getScreenComponent(row, "SAR", "SAR", "SAR", "SAR"));
                }
            }
            screenComponents = screenComponents.OrderBy(o => o.Order_Nm).ToList<Screen_Comp_T>();
            string strHtml = string.Empty;
            strHtml = renderScreenComponent(screen, screenComponents, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvInformationHtml", strHtml.ToString());
            #endregion

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", Convert.ToString(SAR_AWARDS_REG_ID));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YR_ID", Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@AWARD_ID", Convert.ToString(HttpContext.Current.Session["AWARD_ID"]));
            if(!string.IsNullOrEmpty(frm["EvalReq"])) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EvalReq", frm["EvalReq"]);

            GetScreenData(frm, screen);
            #endregion

            #region Objective
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.OBJECTIVE_QUESTIONS), 260, 1,frm);
            #endregion

            #region Subjective
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.SUBJECTIVE_QUESIONS), 262, 2,frm);
            #endregion

            #region Bind Disclosure
            Dictionary<string, object> cond = new Dictionary<string, object>();
            cond.Add("ACTIVE_YN", 1);
            cond.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_YR_ID"]));
            DataTable dt = Util.UtilService.getData("SAR", "MST_AWARD_INFO_T", cond, null, 0, 1);
            if(dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["DISCLOSURE_TX"] != DBNull.Value)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Disclosure", Convert.ToString(dt.Rows[0]["DISCLOSURE_TX"]));
                }
            }
            #endregion

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterPreview(int WEB_APP_ID, FormCollection frm)
        {
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);            
            if(string.IsNullOrEmpty(frm["ID"]))frm["ID"] = Convert.ToString(HttpContext.Current.Session["APPL_Id"]);
            if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["APPLICATION_REF_NO_TX"]))) frm["APPLICATION_REF_NO_TX"] = Convert.ToString(HttpContext.Current.Session["APPLICATION_REF_NO_TX"]);
            else
            {
                string strAppRefNo = string.Concat(Convert.ToString(frm["PROPREITER_PARTNER_FIRM"]).Substring(0, 5)
                        , Convert.ToString(frm["ID"]));
                frm["APPLICATION_REF_NO_TX"] = strAppRefNo;
            }
            frm["SUBMISSION_DT"] = DateTime.Now.ToShortDateString();

            ActionClass act = UtilService.afterSubmit(WEB_APP_ID, frm);            
            string strHtml = string.Empty,strStatus = string.Empty;
            if (act != null && Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject userdata = JObject.Parse(Convert.ToString(act.DecryptData));
                DataTable dtb = new DataTable();
                if (userdata.HasValues)
                {
                    foreach (JProperty val in userdata.Properties())
                    {

                        if (val.Name == screen.Table_Name_Tx)
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            if (dtb.Rows[0]["STATUS_TX"] != DBNull.Value) strStatus = Convert.ToString(dtb.Rows[0]["STATUS_TX"]);
                        }

                    }
                }
            }

            if (strStatus == "Submitted")
            {
                strHtml = "<div class='text-success'><p><b>Your application is submitted successfully</b></p></div>";

                #region Insert Approval record
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                Dictionary<string, object> data = new Dictionary<string, object>();

                string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                AppUrl = AppUrl + "/AddUpdate";

                string screenType = "insert";

                data.Add("APPL_ID", frm["ID"]);
                data.Add("FIN_YEAR_ID", HttpContext.Current.Session["FIN_YEAR_ID"]);                
                data.Add("SAR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));                
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("SAR", "SAR_APPROVAL_T", 0, 0, lstData1, conditions));
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType, lstData));
                #endregion
            }
            else strHtml = "<div class='text-success'><p><b>Your application is saved successfully</b></p></div>";
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Disclosure", strHtml);
            return act;
        }

        public ActionClass beforeSearchUserApplications(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            Dictionary<string, object> cond = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"])))
                GetActiveYear();

            cond.Clear();
            cond.Add("ACTIVE_YN", 1);
            cond.Add("SAR_AWARDS_REG_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));
            if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"])))
                cond.Add("FIN_YR_ID", Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"]));

            JObject jData = null;
            jData = DBTable.GetData("fetch", cond, "SA_GENERAL_INFO_T", 0, 10, "SAR");
            DataTable dt = new DataTable();
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name.Equals("SA_GENERAL_INFO_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count >= 5)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "style='display:none;'");
            else if (dt.Rows.Count == 0)
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", string.Empty);
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_Name", "Submit new application");
            }
            else
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", string.Empty);
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_Name", "Submit another application");
            }            
            return null;
        }

        public ActionClass beforeQuestionMarksApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int SAR_AWARDS_REG_ID = GetSARAwardsRegId("");
            if (string.IsNullOrEmpty(frm["ID"])) frm["ID"] = Convert.ToString(HttpContext.Current.Session["APPL_Id"]);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 261);
            conditions.Add("SAR_AWARDS_REG_ID", SAR_AWARDS_REG_ID);
            conditions.Add("APPL_ID", frm["ID"]);

            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "SAR");
            DataTable dt = null;

            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                if(dt.Rows[0]["ID"] != DBNull.Value)screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", dt.Rows[0]["ID"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@SAR_AWARDS_REG_ID", SAR_AWARDS_REG_ID.ToString());
                if (dt.Rows[0]["MARKS"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TOTAL_MARKS_NM", dt.Rows[0]["MARKS"].ToString());
                if (dt.Rows[0]["DISQUALIFIED_YN"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISQUALIFIED_YN", dt.Rows[0]["DISQUALIFIED_YN"].ToString());
                if (dt.Rows[0]["REMARKS_TX"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REMARKS_TX", dt.Rows[0]["REMARKS_TX"].ToString());
                else screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REMARKS_TX", string.Empty);
                if (dt.Rows[0]["EVAL_COUNT_NM"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EVAL_COUNT_NM", dt.Rows[0]["EVAL_COUNT_NM"].ToString());
                if (dt.Rows[0]["APPL_ID"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@APPL_ID", dt.Rows[0]["APPL_ID"].ToString());
                if (dt.Rows[0]["APPLICATION_REF_NO_TX"] != DBNull.Value) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@APPLICATION_REF_NO_TX", dt.Rows[0]["APPLICATION_REF_NO_TX"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@USER_NAME", Convert.ToString(HttpContext.Current.Session["USER_NAME_TX"]));
                
            }

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterQuestionMarksApproval(int WEB_APP_ID, FormCollection frm)
        {
            frm["ADMIN_MARKS_DT"] = DateTime.Now.ToString("dd/MM/yyyy");
            frm["s"] = "update";

            if(string.IsNullOrEmpty(frm["APPLICATION_REF_NO_TX"])) frm["APPLICATION_REF_NO_TX"] = Convert.ToString(HttpContext.Current.Session["APPLICATION_REF_NO_TX"]);
            ActionClass act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            
            #region Insert History
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();

            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            
            data.Add("SAR_AWARDS_REG_ID", frm["SAR_AWARDS_REG_ID"]);
            data.Add("APPL_ID", frm["APPL_ID"]);
            data.Add("APPLICATION_REF_NO_TX", frm["APPLICATION_REF_NO_TX"]);
            data.Add("REMARKS_TX", frm["REMARKS_TX"]);
            data.Add("TOTAL_MARKS_NM", frm["TOTAL_MARKS_NM"]);
            data.Add("ADMIN_MARKS_DT", frm["ADMIN_MARKS_DT"]);
            data.Add("USER_NAME", frm["USER_NAME"]);

            lstData1.Add(data);
            lstData.Add(Util.UtilService.addSubParameter("SAR", "SAR_HISTORY_T", 0, 0, lstData1, conditions));
            ActionClass act1 = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
            #endregion

            return act;
        }

        public ActionClass beforeConfigGenenralInstructions(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = new ActionClass();
            if (!string.IsNullOrEmpty(frm["ID"]))
            {
                frm["CONFIG_FIN_YEAR_ID"] = frm["ID"];
                HttpContext.Current.Session["CONFIG_FIN_YEAR_ID"] = frm["ID"];
            }

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YEAR_ID", frm["CONFIG_FIN_YEAR_ID"]);

            #region Get Instructions
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>();
                       
            conditions.Add("FIN_YEAR_ID", frm["ID"]);            

            JObject jData = DBTable.GetData("fetch", conditions, "MST_AWARD_INFO_T", 0, 1, "SAR");
            DataTable dtData = new DataTable();
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name == "MST_AWARD_INFO_T")
                    {
                        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
            {
                if (dtData.Rows[0]["GENERAL_INFOR_TX"] != DBNull.Value)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@GENERAL_INFOR_TX", Convert.ToString(dtData.Rows[0]["GENERAL_INFOR_TX"]));
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", Convert.ToString(dtData.Rows[0]["ID"]));
                }
                else
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@GENERAL_INFOR_TX", string.Empty);
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", string.Empty);
                }
            }
            else
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@GENERAL_INFOR_TX", string.Empty);
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", string.Empty);
            }
            #endregion

            act = Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }


        public ActionClass afterConfigGenenralInstructions(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }


        public ActionClass beforeConfigObjective(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = new ActionClass();
            if (string.IsNullOrEmpty(frm["FIN_YEAR_ID"])) frm["FIN_YEAR_ID"] = Convert.ToString(HttpContext.Current.Session["CONFIG_FIN_YEAR_ID"]);
            if (frm["s"] == "edit") frm["ID"] = frm["ui"];
            else frm["ID"] = string.Empty;

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YEAR_ID",Convert.ToString(HttpContext.Current.Session["CONFIG_FIN_YEAR_ID"]));

            act = Util.UtilService.beforeLoad(WEB_APP_ID, frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>();

            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("FIN_YR_ID", 0);
            conditions.Add("SCREEN_ID", screen.ID);

            JObject jData = DBTable.GetData("fetch", conditions, "SAR_SCREEN_COMP_T", 0, 1000, "SAR");
            DataTable dtData = new DataTable();
            if (jData != null)
            {
                foreach (JProperty property in jData.Properties())
                {
                    if (property.Name == "SAR_SCREEN_COMP_T")
                    {
                        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
            {
                screenComponents = new List<Screen_Comp_T>();
                foreach (DataRow row in dtData.Rows)
                {
                    screenComponents.Add(getScreenComponent(row, "SAR", "SAR", "SAR", "SAR"));
                }
            }
            screenComponents = screenComponents.OrderBy(o => o.Order_Nm).ToList<Screen_Comp_T>();
            string strHtml = string.Empty;
            strHtml = renderScreenComponent(screen, screenComponents, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvOptions", strHtml.ToString());
            return act;
        }

        public ActionClass afterConfigObjective(int WEB_APP_ID,FormCollection frm)
        {
            ActionClass act = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"]))) frm["s"] = "update";
            frm["Q_DESCRIPTION_TX"] = frm["QUESTION_NAME_TX"];
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            int ID = 0;
            if (Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject userdata = JObject.Parse(Convert.ToString(act.DecryptData));
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

            //Enter Objective information
            if (ID > 0)
            {                
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                Dictionary<string, object> data = new Dictionary<string, object>();

                if (frm["Q_OPTION_NAME_TX"] != null && frm["Q_OPTION_NAME_TX"] != string.Empty)
                {

                    string strOption = frm["Q_OPTION_NAME_TX"];
                    string strMarks = frm["MARKS_NM"];
                    string strOptId = frm["OPTION_ID"];

                    var optName = (strOption ?? string.Empty).Split(',');
                    var marks = (strMarks ?? string.Empty).Split(',');                    
                    var opId = (strOptId ?? string.Empty).Split(',');

                    string screenType = string.Empty;

                    for (int i = 0; i < optName.Length; i++)
                    {
                        if (optName[i].ToString() != string.Empty)
                        {
                            data.Add("QUESTION_ID", ID);
                            data.Add("Q_OPTION_NAME_TX", optName[i].ToString());
                            data.Add("MARKS_NM", marks[i].ToString());
                            data.Add("ORDER_NM", i+1);
                            data.Add("DISQUALIFIED_YN", 0);
                            if (Convert.ToInt32(opId[i]) > 0)
                            {
                                data.Add("ID", opId[i]);
                                screenType = "update";
                            }
                            else
                            {
                                screenType = "insert";
                            }
                            lstData1.Add(data);
                            lstData.Add(Util.UtilService.addSubParameter("SAR", "QUESTION_OPTIONS_T", 0, 0, lstData1, conditions));
                            act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType, lstData));

                            data.Clear();
                            lstData.Clear();
                            lstData1.Clear();
                        }
                    }
                }
            }
            else
            {
                act.StatCode = "0";
                act.StatMessage = "success";
            }
            return act;
        }

        public ActionClass beforeConfigSubjective(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = new ActionClass();
            if (string.IsNullOrEmpty(frm["FIN_YEAR_ID"])) frm["FIN_YEAR_ID"] = Convert.ToString(HttpContext.Current.Session["CONFIG_FIN_YEAR_ID"]);
            if (frm["s"] == "edit") frm["ID"] = frm["ui"];
            else frm["ID"] = string.Empty;

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FIN_YEAR_ID", Convert.ToString(HttpContext.Current.Session["CONFIG_FIN_YEAR_ID"]));

            act = Util.UtilService.beforeLoad(WEB_APP_ID, frm);            
            return act;
        }

        public ActionClass afterConfigSubjective(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"]))) frm["s"] = "update";
            frm["Q_DESCRIPTION_TX"] = frm["QUESTION_NAME_TX"];
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);            
            return act;
        }


        public enum QUESTION_CATEGORY_TYPE
        {
            OBJECTIVE_QUESTIONS = 1,
            SUBJECTIVE_QUESIONS = 2            
        }

        #region Random Password Generator
        // Instantiate random number generator.  
        // It is better to keep a single Random instance 
        // and keep using Next on the same instance.  
        private readonly Random _random = new Random();

        // Generates a random number within a range.      
        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public string RandomPassword()
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case   
            passwordBuilder.Append(RandomString(4, true));

            // 4-Digits between 1000 and 9999  
            passwordBuilder.Append(RandomNumber(1000, 9999));

            // 2-Letters upper case  
            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }

        // Generates a random string with a given size.    
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
        #endregion

        public static void GetActiveYear()
        {
            string TableName = "FINANCIAL_YEAR_T";
            JObject jdata = null;
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("SHOW_YEAR_YN", 1);

            jdata = DBTable.GetData("fetch", conditions, TableName, 0, 10, "SAR");
            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                {
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            
            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
                HttpContext.Current.Session["ACTIVE_YR_ID"] = Convert.ToInt32(dtData.Rows[0]["ID"]);
        }

        private void GetScreenData(FormCollection frm, Screen_T screen)
        {
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("SAR_AWARDS_REG_ID", GetSARAwardsRegId(""));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_YR_ID"]));
            if(!string.IsNullOrEmpty(frm["id"]) && Convert.ToInt32(frm["id"]) > 0)conditions.Add("ID", frm["id"]);

            JObject jdata = DBTable.GetData("fetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
            string TokenName = string.Empty;
            Object TokenValue = string.Empty;
            if (jdata != null && jdata.HasValues && jdata.First.First.First != null)
            {
                frm["s"] = "edit";
                foreach (JToken token in jdata.First.First.First)
                {
                    TokenName = ((Newtonsoft.Json.Linq.JProperty)token).Name;
                    TokenValue = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)token).Value);
                    if (TokenName == "STATUS_TX") HttpContext.Current.Session["STATUS_TX"] = TokenValue;
                    if (screen.Screen_Content_Tx != null)
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                }
            }
        }

        private int GetSARAwardsRegId(string UniqueId)
        {
            int SAR_AWARDS_REG_ID = 0;
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 24 && !string.IsNullOrEmpty(UniqueId) && UniqueId != "undefined")//For Admin
            {
                Dictionary<string, object> conds = new Dictionary<string, object>();
                conds.Add("ID", UniqueId);

                DataTable dt = Util.UtilService.getData("SAR", "SA_GENERAL_INFO_T", conds, null, 0, 10);

                if(dt!=null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["SAR_AWARDS_REG_ID"] != DBNull.Value) SAR_AWARDS_REG_ID = Convert.ToInt32(dt.Rows[0]["SAR_AWARDS_REG_ID"]);
                }                
                HttpContext.Current.Session["SAR_AWARDS_REG_ID"] = SAR_AWARDS_REG_ID;
            }
            else if (HttpContext.Current.Session["SAR_AWARDS_REG_ID"] != null)
                SAR_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["SAR_AWARDS_REG_ID"]);
            else
                SAR_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);

            return SAR_AWARDS_REG_ID;
        }

        private Screen_Comp_T getScreenComponent(DataRow row, string webAppSchema, string appSchema, string moduleSchema, string screenSchema)
        {
            Screen_Comp_T screen_Comp_T = new Screen_Comp_T();            
            screen_Comp_T.Id = Convert.ToInt32(row["ID"]);
            screen_Comp_T.categoryId = Convert.ToInt32(row["CATEGORY_ID"]);
            screen_Comp_T.finYrId = Convert.ToInt32(row["FIN_YR_ID"]);
            screen_Comp_T.Screen_Id = Convert.ToInt32(row["SCREEN_ID"]);
            screen_Comp_T.Ref_Id = Convert.ToInt32(row["REF_ID"]);
            screen_Comp_T.Order_Nm = Convert.ToInt32(row["ORDER_NM"]);
            screen_Comp_T.Comp_Type_Nm = Convert.ToInt32(row["COMP_TYPE_NM"]);            
            screen_Comp_T.ScreenSchemaNameTx = screenSchema;
            screen_Comp_T.ModuleSchemaNameTx = moduleSchema;
            screen_Comp_T.ApplicationSchemaNameTx = appSchema;
            screen_Comp_T.WebAppSchemaNameTx = webAppSchema;
            if (!string.IsNullOrEmpty(Convert.ToString(row["SCHEMA_NAME_TX"]))) screen_Comp_T.schemaNameTx = Convert.ToString(row["SCHEMA_NAME_TX"]); else screen_Comp_T.schemaNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["DYN_WHERE_TX"]))) screen_Comp_T.dynWhere = Convert.ToString(row["DYN_WHERE_TX"]); else screen_Comp_T.dynWhere = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_STYLE_TX"]))) screen_Comp_T.compStyleTx = Convert.ToString(row["COMP_STYLE_TX"]); else screen_Comp_T.compStyleTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_SCRIPT_TX"]))) screen_Comp_T.compScriptTx = Convert.ToString(row["COMP_SCRIPT_TX"]); else screen_Comp_T.compScriptTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_NAME_TX"]))) screen_Comp_T.compNameTx = Convert.ToString(row["COMP_NAME_TX"]); else screen_Comp_T.compNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_CONTENT_TX"]))) screen_Comp_T.compContentTx = Convert.ToString(row["COMP_CONTENT_TX"]); else screen_Comp_T.compContentTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_VALUE_TX"]))) screen_Comp_T.compValueTx = Convert.ToString(row["COMP_VALUE_TX"]); else screen_Comp_T.compValueTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_TEXT_TX"]))) screen_Comp_T.compTextTx = Convert.ToString(row["COMP_TEXT_TX"]); else screen_Comp_T.compTextTx = null;       
            if (!string.IsNullOrEmpty(Convert.ToString(row["SQL_TX"]))) screen_Comp_T.sql = Convert.ToString(row["SQL_TX"]); else screen_Comp_T.sql = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["WHERE_TX"]))) screen_Comp_T.where = Convert.ToString(row["WHERE_TX"]); else screen_Comp_T.where = null;            
            if (!string.IsNullOrEmpty(Convert.ToString(row["TABLE_NAME_TX"]))) screen_Comp_T.tableNameTx = Convert.ToString(row["TABLE_NAME_TX"]); else screen_Comp_T.tableNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COLUMN_NAME_TX"]))) screen_Comp_T.columnNameTx = Convert.ToString(row["COLUMN_NAME_TX"]); else screen_Comp_T.columnNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["MANDATORY_YN"]))) screen_Comp_T.isMandatoryYn = Convert.ToBoolean(row["MANDATORY_YN"]); else screen_Comp_T.isMandatoryYn = false;            
            if (!string.IsNullOrEmpty(Convert.ToString(row["READ_WRITE_YN"]))) screen_Comp_T.isReadWriteYn = Convert.ToBoolean(row["READ_WRITE_YN"]); else screen_Comp_T.isReadWriteYn = false;          
            
            
            var resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("REF_ID") == screen_Comp_T.Id && x.Field<long>("ID") != screen_Comp_T.Id).OrderBy(x => x.Field<long>("ORDER_NM")).ToList();
            List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>(); ;
            if (resScreen_Comp != null && resScreen_Comp.Count > 0)
            {
                DataTable dtScreen_Comp = resScreen_Comp.CopyToDataTable();
                foreach (DataRow row1 in dtScreen_Comp.Rows)
                {
                    screenComponents.Add(getScreenComponent(row1, webAppSchema, appSchema, moduleSchema, screenSchema));
                }
            }
            screen_Comp_T.ScreenCompList = screenComponents;
            return screen_Comp_T;
        }

        private string renderScreenComponent(Screen_T screen, List<Screen_Comp_T> screenComponents, FormCollection frm)
        {

            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach (Screen_Comp_T s in screenComponents)
            {
                if (counter == 0)
                {
                    sb.Append("<input type='hidden' id='altsi' value='").Append(screen.ID).Append("'>");
                    if (frm != null)
                    {
                        string uniqueId = frm["ui"];
                        if (!string.IsNullOrEmpty(uniqueId))
                            sb.Append("<input type='hidden' value='" + uniqueId + "' name='hidUI' id='hidUI' />");
                    }
                    counter++;
                }
                sb.Append("<div class='row'>");

                if (s.Comp_Type_Nm < (Int32)UtilService.HTMLTag.TEXTAREA)
                {
                    sb.Append("<div class='form-inline'>");
                    sb.Append("<label class='col-md-6 control-label AppliedTrainingnputsLabel FacltyNamTxt txtLabel'>")
                      .Append(s.Order_Nm).Append(". ").Append(s.compTextTx)
                      .Append("</label>");
                    sb.Append("<div class='col-md-4'><div class='input-group col-md-7 col-xs-11 col-sm-11 IpadInput mobilInput7 panelInputsSub'>");
                    sb.Append("<input class='form-control' type='").Append(s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.HIDDEN ?
                        "hidden" : (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.TEXT ? "text" :
                        (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.PASSWORD ? "password" :
                        (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.RADIO_GROUP ||
                        s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.RADIO_BUTTON ? "radio" : "checkbox"))))
                      .Append("' name='").Append(s.compNameTx).Append("' value='@").Append(s.compNameTx);
                    //if (s.tableNameTx != null && s.compNameTx.Equals(s.tableNameTx)) sb.Append("@").Append(s.compNameTx);
                    //else sb.Append(s.compValueTx);
                    sb.Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(" />");
                    sb.Append("</div></div>");
                }
                else if (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.TEXTAREA)
                {
                    sb.Append("<div class='form-inline'>");
                    sb.Append("<label class='col-md-6 control-label AppliedTrainingnputsLabel FacltyNamTxt txtLabel'>")
                      .Append(s.Order_Nm).Append(". ").Append(s.compTextTx)
                      .Append("</label>");
                    sb.Append("<div class='col-md-4'><div class='input-group col-md-7 col-xs-11 col-sm-11 IpadInput mobilInput7 panelInputsSub'>");
                    sb.Append("<textarea cols='50' class='form-Control' rows='6' name='")
                      .Append(s.compNameTx).Append("' value='@").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    if (s.compNameTx.Equals(s.tableNameTx)) sb.Append("@").Append(s.compNameTx);
                    else sb.Append(s.compValueTx);
                    sb.Append("@").Append(s.columnNameTx);
                    sb.Append(" </textarea>");
                    sb.Append("</div></div>");
                }
                else if (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.ADD_NEW_ROW)
                {
                    sb.Append("<div class='col-sm-12'>");
                    sb.Append("<label class='col-md-12 control-label AppliedTrainingnputsLabel FacltyNamTxt txtLabel'>")
                      .Append(s.Order_Nm).Append(". ").Append(s.compTextTx)
                      .Append("</label>");
                    sb.Append("<div class='col-md-12 FacltyNamTxt' style='clear: both; width: 100 %;'>")
                          .Append("<div style='overflow - x:auto;'><table class='table table-bordered tableSearchResults' name='")
                          .Append(s.compNameTx)
                           .Append("'")
                          .Append(" id=").Append("'").Append(s.compNameTx.ToString().Trim())
                          .Append("'")
                          .Append("><thead><tr>");
                    string[] strColTxtBxName = s.compValueTx.Split(',');
                    string[] strHdrVal = s.compContentTx.Split(',');
                    for (int i = 0; i < strHdrVal.Count(); i++)
                    {
                        sb.Append("<th class='searchResultsHeading'>").Append(strHdrVal[i]).Append("</th>");
                    }                 
                    sb.Append("</tr></thead>");
                    sb.Append("<tbody>");

                    //BIND PARTNER INFORMATION IF ANY PRESENT
                    #region partnerInfo
                    if (s.Screen_Id != 615)
                    {
                        Dictionary<string, object> cond = new Dictionary<string, object>();
                        cond.Add("APPL_ID", frm["ID"]);

                        JObject jdata = DBTable.GetData("fetch", cond, "SA_PARTNERS_INFO_T", 0, 100, "SAR");
                        DataTable dt = new DataTable();
                        foreach (JProperty property in jdata.Properties())
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                sb.Append("<tr>");

                                sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value='")
                                .Append(row["NAME_TX"]).Append("' name='NAME_TX' id='NAME_TX'>")
                                .Append("<input type='hidden' value='").Append(row["ID"]).Append("' name='PARTNER_ID'/>")
                                .Append("</td>");

                                sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value='")
                                .Append(row["COP_NO_TX"]).Append("' name='COP_NO_TX' id='COP_NO_TX'>")
                                .Append("</td>");

                                sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value='")
                                .Append(row["MEMSHIP_NO_TX"]).Append("' name='MEMSHIP_NO_TX' id='NAME_TX'>")
                                .Append("</td></tr>");
                            }
                        }
                        
                    }
                    #endregion


                    //BIND QUESTION OPTIONS IF ANY PRESENT
                    #region bind options
                    else
                    {
                        if (!string.IsNullOrEmpty(frm["ID"]))
                        {
                            Dictionary<string, object> cond = new Dictionary<string, object>();
                            cond.Add("QUESTION_ID", frm["ID"]);

                            JObject jdata = DBTable.GetData("fetch", cond, "QUESTION_OPTIONS_T", 0, 100, "SAR");
                            DataTable dt = new DataTable();
                            foreach (JProperty property in jdata.Properties())
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }

                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    sb.Append("<tr>");

                                    sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value='")
                                    .Append(row["Q_OPTION_NAME_TX"]).Append("' name='Q_OPTION_NAME_TX' id='Q_OPTION_NAME_TX'>")
                                    .Append("<input type='hidden' value='").Append(row["ID"]).Append("' name='OPTION_ID'/>")
                                    .Append("</td>");

                                    sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value='")
                                    .Append(row["MARKS_NM"]).Append("' name='MARKS_NM' id='MARKS_NM'>")
                                    .Append("</td></tr>");
                                }
                            }
                        }
                    }
                    #endregion  

                    sb.Append("<tr>");
                    for (int i = 0; i < strHdrVal.Count(); i++)
                    {
                        sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value=''");
                        sb.Append(" name=").Append("'").Append(strColTxtBxName[i].ToString().Trim()).Append("'");
                        sb.Append(" id=").Append("'").Append(strColTxtBxName[i].ToString().Trim()).Append("'");
                        sb.Append(">");
                        if (i == 0)
                        {
                            sb.Append("<input type = 'hidden' value = '0' name = '");
                            if (s.Screen_Id != 615) sb.Append("PARTNER_ID");
                            else sb.Append("OPTION_ID");
                            sb.Append("' /> ");
                        }
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                    
                    sb.Append("</tbody></table></div>");
                    sb.Append("<div class='col-md-2'></div></div><div class='mt-20'></div>");
                    //ADD NEW ROW BUTTON
                    sb.Append("<div class='row'><div class='form-inline col-xs-12 text-center'>")
                        .Append("<input type = 'button' class='btn btn-primary'")
                        .Append("value='Add New' id='btn").Append(s.compNameTx)
                        .Append("' onclick=").Append(s.compNameTx).Append("()></div></div>");
                }
                else if (s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.TABLE_WITH_TEXTBOXES)
                {
                    sb.Append("<div class='form-inline'>");
                    sb.Append("<label class='col-md-4 control-label AppliedTrainingTxt AppliedTrainingnputsLabel FacltyNamTxt'>")
                     .Append(s.Order_Nm).Append(". ").Append(s.compTextTx)
                     .Append("</label>");
                    //TO DO :: GET NO. OF YEARS AND EMPLOYEE INFORMATION FROM AWARD CONFIG TABLE
                    sb.Append("<div class='col-md-8'><table class='table tableSearchResults no-border'><tbody>");                   
                    if (s.columnNameTx == "YEARS")
                    {
                        string[] strYears = {string.Concat(DateTime.Now.AddYears(-3).Year.ToString(), " - ", DateTime.Now.AddYears(-2).Year.ToString())
                                                , string.Concat(DateTime.Now.AddYears(-2).Year.ToString(), " - ", DateTime.Now.AddYears(-1).Year.ToString())
                                                , string.Concat(DateTime.Now.AddYears(-1).Year.ToString(), " - ", DateTime.Now.Year.ToString())
                  
                          };
                        for(int i=0;i<strYears.Length;i++)
                        {
                            sb.Append("<tr><td style = 'border-top:none;'>");
                            sb.Append("<label class='col-md-5 control-label AppliedTrainingnputsLabel'>")
                              .Append(strYears[i])
                              .Append("</label>");
                            sb.Append("<div class='col-md-7'><div class='input-group col-md-12 col-xs-12 col-sm-12 panelInputsSub'>")
                           .Append("<input placeholder ='' class='form-control' type='text' id='SA_FY").Append(i+1)
                           .Append("' name='SA_FY").Append(i+1)
                           .Append("' value='@SA_FY").Append(i + 1)
                           .Append("'></input></div></div></td></tr>");
                        }
                    }
                    else if (s.columnNameTx == "EMPLOYEES")
                    {
                        string[] strEmployees = {"Partners( if any)","Qualified Company Secretaries"
                                ,"Qualified Assistant","Other Professionals(specify qualifications)"
                                ,"Trainees","Other than above" };
                        string[] strEmployeesName = {"PARTNERS_TX","QUALIFIED_CA_TX"
                                ,"QUALIFIED_ASSIST_TX","OTHER_PROF_TX","OTHER_TRAINEE_TX","OTHER_TX" };
                        for (int i = 0; i < strEmployees.Length; i++)
                        {
                            sb.Append("<tr><td style = 'border-top:none;'>");
                            sb.Append("<label class='col-md-5 control-label AppliedTrainingnputsLabel'>")
                              .Append(strEmployees[i])
                              .Append("</label>");
                            sb.Append("<div class='col-md-7'><div class='input-group col-md-12 col-xs-12 col-sm-12 panelInputsSub'>")
                           .Append("<input placeholder ='' class='form-control' type='text' id='").Append(strEmployeesName[i])
                           .Append("' name='").Append(strEmployeesName[i])
                           .Append("' value='@").Append(strEmployeesName[i])
                           .Append("'></div></div></td></tr>");
                        }
                    }
                    sb.Append("</tbody></table></div>");
                }
                else if(s.Comp_Type_Nm == (Int32)UtilService.HTMLTag.TABLE_WITH_QUERY)
                {
                    sb.Append("<div class='form-inline'>");
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("APPL_ID", frm["ID"]);

                    JObject jdata = DBTable.GetData("fetch", cond, "SA_PARTNERS_INFO_T", 0, 100,"SAR");
                    DataTable dt = new DataTable();
                    foreach (JProperty property in jdata.Properties())
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());                       
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        List<string> cols = new List<string>();
                        if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                        List<string> colvals = new List<string>();
                        if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();
                        bool isTableStart = false;

                        bool isColExists = false;
                        bool isIdExists = false;

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
                                    sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(s.compTextTx).Append("</span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");
                                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                                    sb.Append("><thead>");
                                    sb.Append("<tr>");
                                }
                                cn = colvals[pos];                                
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
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    string cn = dt.Columns[j].ColumnName;
                                    string value = Convert.ToString(dt.Rows[i][cn]).Replace(" 00:00:00", "");
                                    int pos = cols.IndexOf(cn);
                                    if (pos > -1)
                                        sb.Append("<td>" + value + "</td>");
                                }
                                //if (isEdit)
                                //{
                                //    sb.Append("<td><a href='#'");
                                //    if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(s.Screen_Id).Append(")' ");
                                //    string editcaption = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                                //    if (string.IsNullOrEmpty(editcaption))
                                //    {
                                //        editcaption = "Edit";
                                //    }
                                //    sb.Append(">" + editcaption + "</a></td>");
                                //}                                                                
                                sb.Append("</tr>");
                            }

                            sb.Append("</tbody></table></div></div></div></div>");
                        }
                    }
                    //Util.UtilService.buildTable(s, sb, frm);
                }
                sb.Append("<div class='col-md-2'></div></div></div><div class='mt-20'></div>");				
            }
        
            return sb.ToString();
        }

        private void BindDynamicPage(Screen_T screen, int CATEGORY_TYPE_ID, int AnswereID, int SrNo,FormCollection frm)
        {
            DataTable dtQ = new DataTable();
            DataTable dtOP = new DataTable();
            DataTable dtANS = new DataTable();
            string TableName = string.Empty;
            int UploadedFiles = 0;
            int TotalFileUploader = 0;

            if (Convert.ToInt32(QUESTION_CATEGORY_TYPE.OBJECTIVE_QUESTIONS) == 1)
                TableName = "OBJECTIVE_T";
            else if (Convert.ToInt32(QUESTION_CATEGORY_TYPE.SUBJECTIVE_QUESIONS) == 2)
                TableName = "SUBJECTIVE_T";

            if (string.IsNullOrEmpty(frm["ID"]) || string.IsNullOrEmpty(frm["ui"]))
            {
                frm["ID"] = Convert.ToString(HttpContext.Current.Session["APPL_Id"]);
            }

            GetActiveYear();

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("FIN_YEAR_ID", HttpContext.Current.Session["ACTIVE_YR_ID"]);
            conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
            JObject jdata = DBTable.GetData("fetch", conditions, "MST_QUESTIONS_T", 0, 100, "SAR");

            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "MST_QUESTIONS_T")
                        dtQ = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    if (dtQ != null && dtQ.Rows != null && dtQ.Rows.Count > 0)
                        TotalFileUploader = dtQ.Select("Q_ENABLE_FILEUPLOADER_YN = " + true).Count();
                }

                conditions = new Dictionary<string, object>();
                conditions.Add("QID", 75);
                conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
                JObject jdata1 = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

                if (jdata1 != null && jdata1.First.First.First != null)
                {
                    foreach (JProperty property in jdata1.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtOP = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                conditions = new Dictionary<string, object>();
                conditions.Add("QID", AnswereID);
                conditions.Add("SAR_AWARDS_REG_ID", Convert.ToString(GetSARAwardsRegId("")));
                conditions.Add("APPL_Id", frm["ID"]);                
                conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
                JObject jAnswers = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

                if (jAnswers != null && jAnswers.First != null)
                {
                    foreach (JProperty property in jAnswers.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtANS = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                StringBuilder sbQuestions = new StringBuilder();
                sbQuestions.Append("<input type='hidden' id='hAnsCount' value='")
                    .Append(dtANS.Rows.Count)
                    .Append("' name= 'hAnsCount'/>");
                if (dtANS != null && dtANS.Rows != null && dtANS.Rows.Count > 0)
                {
                    #region With Ans Bind
                    int count = 0;
                    int AdminMarks = 0;                
                    for (int i = 0; i < dtQ.Rows.Count; i++)
                    {
                        string strAns = string.Empty,strAnsDesc = string.Empty
                            ,strAnsFileName = string.Empty, strAnsId=string.Empty
                            ,strAnsAdminMarks=string.Empty,strAnsAdminRemarks=string.Empty;
                        if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                            count++;
                        for (int j = 0; j < dtANS.Rows.Count; j++)
                        {
                            if (Convert.ToInt32(dtANS.Rows[j]["QUESTION_ID"]) == Convert.ToInt32(dtQ.Rows[i]["ID"]))                                
                            {

                                sbQuestions.Append("<div><input type='hidden' value='");
                                if (dtANS.Rows[j]["ID"] != DBNull.Value) sbQuestions.Append(dtANS.Rows[j]["ID"]);
                                sbQuestions.Append("' name='");
                                if (dtANS.Rows[j]["QUESTION_ID"] != DBNull.Value) sbQuestions.Append(dtANS.Rows[j]["QUESTION_ID"]);
                                sbQuestions.Append("-ID'/></div>");

                                if(dtANS.Rows[j]["ANSWER_TX"] != DBNull.Value) strAns = Convert.ToString(dtANS.Rows[j]["ANSWER_TX"]);
                                if(dtANS.Rows[j]["QUES_DESCRIPTION_TX"] != DBNull.Value) strAnsDesc = Convert.ToString(dtANS.Rows[j]["QUES_DESCRIPTION_TX"]);
                                if (dtANS.Rows[j]["QUES_FILENAME_TX"] != DBNull.Value) strAnsFileName = Convert.ToString(dtANS.Rows[j]["QUES_FILENAME_TX"]);
                                if (dtANS.Rows[j]["ID"] != DBNull.Value) strAnsId = Convert.ToString(dtANS.Rows[j]["ID"]);
                                if (dtANS.Rows[j]["ADMIN_MARKS"] != DBNull.Value) strAnsAdminMarks = Convert.ToString(dtANS.Rows[j]["ADMIN_MARKS"]);
                                if (dtANS.Rows[j]["EVAL_REMARKS_TX"] != DBNull.Value) strAnsAdminRemarks = Convert.ToString(dtANS.Rows[j]["EVAL_REMARKS_TX"]);
                            }
                        }

                        if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                        {
                            if (string.IsNullOrEmpty(strAnsAdminMarks) || strAnsAdminMarks == "0")
                            {
                                if (dtQ.Rows[i]["MARKS_NM"] != DBNull.Value) strAnsAdminMarks = Convert.ToString(dtQ.Rows[i]["MARKS_NM"]);
                            }
                            sbQuestions.Append("<div class='col-xs-12'>");
                            sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                            //sbQuestions.Append("<div class='text-right'>Marks: " + dtANS.Rows[i]["MARKS_NM"].ToString() + " </div>");
                            sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value=''")
                                .Append("class='form-control' onkeydown='maxlength(this,")
                                .Append(dtQ.Rows[i]["WORD_LIMIT_NM"])
                                .Append(")'>");
                            if (!string.IsNullOrEmpty(strAns)) sbQuestions.Append(strAns);
                            sbQuestions.Append(" </textarea></div>");
                            sbQuestions.Append("</div>");
                        }
                        else if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "1")
                        {
                            sbQuestions.Append("<div class='col-xs-12'>");
                            if (!string.IsNullOrEmpty(Convert.ToString(dtQ.Rows[i]["REF_ID"])) && Convert.ToString(dtQ.Rows[i]["REF_ID"]) != "0")
                                sbQuestions.Append("<div class='fontBold pt-20'>" + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                            else
                                sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");

                            sbQuestions.Append("<div class='col-md-9'>");
                            for (int j = 0; j < dtOP.Rows.Count; j++)
                            {
                                if (dtQ.Rows[i]["ID"].ToString() == dtOP.Rows[j]["QUESTION_ID"].ToString())
                                {
                                    if (Convert.ToString(dtOP.Rows[j]["ID"]) == strAns)
                                    {
                                        if (string.IsNullOrEmpty(strAnsAdminMarks) || strAnsAdminMarks == "0")
                                        {
                                            if (dtOP.Rows[j]["MARKS_NM"] != DBNull.Value) strAnsAdminMarks = Convert.ToString(dtOP.Rows[j]["MARKS_NM"]);
                                        }
                                        sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' checked='checked' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                    }
                                    else
                                        sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                }
                            }
                            sbQuestions.Append("</div>");

                            if (dtQ.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                            {
                                sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                sbQuestions.Append("<label>" + dtQ.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");               
                                sbQuestions.Append("<textarea type='textarea' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX'")
                                    .Append("class='form-control' onkeydown='maxlength(this,")
                                    .Append(dtQ.Rows[i]["WORD_LIMIT_NM"])
                                    .Append(")'>")
                                    .Append(strAnsDesc)
                                    .Append("</textarea>");
                                sbQuestions.Append("</div>");
                            }

                            if (dtQ.Rows[i]["Q_ENABLE_FILEUPLOADER_YN"].ToString().ToUpper() == "TRUE")
                            {
                                sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                if (!string.IsNullOrEmpty(strAnsFileName))
                                {
                                    UploadedFiles++;
                                    string[] ArrFileName = strAnsFileName.Split('\\');
                                    string FileName = ArrFileName[ArrFileName.Length - 1];
                                    sbQuestions.Append("&nbsp;&nbsp;<a target='_blank' style='color:blue;'")
                                        .Append("title='Click to view' href='../SAR/DownloadFileByIDFromSpecificTable?id=")
                                        .Append(strAnsId)
                                        .Append("&TableName=")
                                        .Append(TableName)
                                        .Append("&ColumnName=QUES_FILENAME_TX&schema=CSR'>")
                                        .Append(FileName)
                                        .Append("</a>");
                                    if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 24)
                                    {
                                        sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                        sbQuestions.Append("<input type='file' onchange='ValidateSize(this,")
                                            .Append(dtQ.Rows[i]["ID"])
                                            .Append(")' name='")
                                            .Append(dtQ.Rows[i]["ID"])
                                            .Append("-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                        sbQuestions.Append("</div>");
                                    }
                                }
                                else if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 24)
                                {
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                    sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtQ.Rows[i]["ID"].ToString() + ")' name='" + dtQ.Rows[i]["ID"].ToString() + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                    sbQuestions.Append("</div>");
                                    sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtQ.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label><br/>");
                                    sbQuestions.Append("<label id='" + dtQ.Rows[i]["ID"].ToString() + "_lblFile' style='font-weight:normal; font-size:12px;color:red;'>File is not attached</label>");
                                }
                                sbQuestions.Append("</div>");
                            }
                            sbQuestions.Append("</div>");                     
                        }
                        else if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                        {
                            if (screen.ID == 540 || screen.ID == 543)
                               sbQuestions.Append(BindStaticTableForSARPolicy(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'U',strAns));                            
                        }
                        if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 24)
                        {
                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() != "3" || 
                                Convert.ToInt32(dtQ.Rows[i]["REF_ID"]) == 0)
                            {
                                AdminMarks++;
                                sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='")
                                    .Append(AdminMarks).Append("-Marks' name='").Append(dtQ.Rows[i]["ID"])
                                    .Append("-ADMIN_MARKS_NM' value='").Append(strAnsAdminMarks)
                                    .Append("' class='form-control' onkeypress='return isDMNumberKey(event)'>");
                                sbQuestions.Append("</div>");

                                sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                sbQuestions.Append("<label> Remarks: </label><textarea id='")
                                    .Append(AdminMarks)
                                    .Append("-EVAL_REMARKS_TX' type='text' rows='5' name='")
                                    .Append(dtQ.Rows[i]["ID"])
                                    .Append("-EVAL_REMARKS_TX' class='form-control'>")
                                    .Append(strAnsAdminRemarks)
                                    .Append("</textarea>");
                                sbQuestions.Append("</div>");
                            }
                            //if (i == dtANS.Rows.Count - 1)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Evaluation Remarks: </label><textarea id='" + AdminMarks + "-Marks' type='text' rows='5' name='EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[0]["EVAL_REMARKS_TX"] + "</textarea>");
                            //    sbQuestions.Append("</div>");
                            //}
                        }
                    }
                    if(CATEGORY_TYPE_ID == 1)screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvObjective", sbQuestions.ToString());
                    if (CATEGORY_TYPE_ID == 2) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvSubjective", sbQuestions.ToString());
                    #endregion
                }
                else
                {
                    if (dtQ != null && dtQ.Rows != null && dtQ.Rows.Count > 0)
                    {
                        #region Without Ans bind
                        int count = 0;                        
                        for (int i = 0; i < dtQ.Rows.Count; i++)
                        {
                            if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                                count++;

                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                            {
                                sbQuestions.Append("<div class='col-xs-12'>");
                                sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                                //sbQuestions.Append("<div class='text-right'>Marks: " + dtQ.Rows[i]["MARKS_NM"].ToString() + " </div>");
                                sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value ='' ")
                                    .Append("class='form-control' onkeydown='maxlength(this,")
                                    .Append(dtQ.Rows[i]["WORD_LIMIT_NM"])
                                    .Append(")'></textarea>");
                                
                                sbQuestions.Append("</div></div>");
                            }
                            else if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "1")
                            {
                                sbQuestions.Append("<div class='col-xs-12'>");
                                if (!string.IsNullOrEmpty(Convert.ToString(dtQ.Rows[i]["REF_ID"])) && Convert.ToString(dtQ.Rows[i]["REF_ID"]) != "0")
                                    sbQuestions.Append("<div class='fontBold pt-20'>" + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                                else
                                    sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");

                                sbQuestions.Append("<div class='col-md-9'>");
                                for (int j = 0; j < dtOP.Rows.Count; j++)
                                {
                                    if (dtQ.Rows[i]["ID"].ToString() == dtOP.Rows[j]["QUESTION_ID"].ToString())
                                        sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                }
                                sbQuestions.Append("</div>");

                                if (dtQ.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                                {
                                    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                    sbQuestions.Append("<label>" + dtQ.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");
                                    //sbQuestions.Append("<input type='text' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'>");
                                    sbQuestions.Append("<textarea type='textarea' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='")
                                        .Append("' class='form-control' onkeydown='maxlength(this,");
                                    if (dtQ.Rows[i]["WORD_LIMIT_NM"] != DBNull.Value) sbQuestions.Append(dtQ.Rows[i]["WORD_LIMIT_NM"]);
                                    else sbQuestions.Append("100");
                                    sbQuestions.Append(")'></textarea>");
                                    sbQuestions.Append("</div>");
                                }

                                if (dtQ.Rows[i]["Q_ENABLE_FILEUPLOADER_YN"].ToString().ToUpper() == "TRUE")
                                {
                                    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'><input type='file' onchange='ValidateSize(this," + dtQ.Rows[i]["ID"] + ")' name='" + dtQ.Rows[i]["ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'></div>");
                                    sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtQ.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label><br/>");
                                    sbQuestions.Append("<label id='" + dtQ.Rows[i]["ID"] + "_lblFile' style='font-weight:normal; font-size:12px;color:red;'>File is not attached</label>");
                                    sbQuestions.Append("</div>");
                                }
                                sbQuestions.Append("</div>");
                            }
                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                            {
                                if (screen.ID == 540)
                                    sbQuestions.Append(BindStaticTableForSARPolicy(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'I',string.Empty));                                
                            }
                        }
                        //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Questions", sbQuestions.ToString());

                        if (CATEGORY_TYPE_ID == 1) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvObjective", sbQuestions.ToString());
                        if (CATEGORY_TYPE_ID == 2) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@dvSubjective", sbQuestions.ToString());
                        #endregion
                    }
                }

                if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 24 || screen.ID == 543 )
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@UPLOADED_FILE", "0");
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TOTAL_FILE_UPLOADER", "0");
                }
                else
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@UPLOADED_FILE", UploadedFiles.ToString());
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TOTAL_FILE_UPLOADER", TotalFileUploader.ToString());
                }
            }
        }

        private StringBuilder BindStaticTableForSARPolicy(int QuesID, string QuesName, char Type,string strStaticTblAns)
        {
            string[] strAns = { };
            DataTable dtStaticTable = new DataTable();
            
            #region Bind Static Table
            StringBuilder sbStaticTable = new StringBuilder();

            sbStaticTable.Append("<div class='col-xs-12'>");
            sbStaticTable.Append("<div class='fontBold pt-20'>1.3 (a)" + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>S.No</th>");
            sbStaticTable.Append("<th>Team Members</ th>");
            sbStaticTable.Append("<th>Tick Here</th>");            
            sbStaticTable.Append("</tr></thead><tbody>");

            if (!string.IsNullOrEmpty(strStaticTblAns))
            {                
                string[] strQue = { "Partner", "Qualified Company Secretary", "Trainee" };
                strAns = strStaticTblAns.Split(',');
                
                for (int i = 0; i < strQue.Length; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><label class='col-md-2 control-label AppliedTrainingnputsLabel FacltyNamTxt'>")
                        .Append(Convert.ToString(i+1)).Append("</label></td>");
                    sbStaticTable.Append("<td><label class='col-md-12 control-label AppliedTrainingnputsLabel FacltyNamTxt'>")
                        .Append(strQue[i]).Append("</label></td>");
                    sbStaticTable.Append("<td><input class='form-control'")
                        .Append(" name='chkPartnerInfo'")
                        .Append("id='chk").Append(strQue[i]).Append("' type='checkbox' value='")
                        .Append(strQue[i]).Append("'");
                    if(strAns.Length > 0 && strAns.Contains(strQue[i]))
                    {
                        sbStaticTable.Append("checked= checked");
                    }
                    sbStaticTable.Append("></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            else
            {                
                string[] strQue = { "Partner", "Qualified Company Secretary", "Trainee" };                
                for (int i = 0; i < strQue.Length; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><label class='col-md-2 control-label AppliedTrainingnputsLabel FacltyNamTxt'>")
                        .Append(Convert.ToString(i + 1)).Append("</label></td>");
                    sbStaticTable.Append("<td><label class='col-md-12 control-label AppliedTrainingnputsLabel FacltyNamTxt'>")
                        .Append(strQue[i]).Append("</label></td>");
                    sbStaticTable.Append("<td><input class='form-control'")
                        .Append(" name='chkPartnerInfo'")
                        .Append(" id='chk").Append(strQue[i]).Append("' type='checkbox' value='")
                        .Append(strQue[i]).Append("'")                        
                        .Append("></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        private ActionClass DMLOPsDyamicPage(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            Dictionary<string, object> Mul_tblData_Update,Mul_tblData_Insert;
            string screenType = frm["s"];
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData2 = new List<Dictionary<string, object>>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> FileUploader = new Dictionary<string, object>();
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            string QuestionIds = string.Empty;
            int SAR_AWARDS_REG_ID = 0;

            if (frm["FIN_YEAR_ID"] == "")
            {
                GetActiveYear();
                frm["FIN_YEAR_ID"] = Convert.ToString(HttpContext.Current.Session["ACTIVE_YR_ID"]);
            }            

            #region For Insertion and updation
            foreach (var key in frm.AllKeys)
            {
                if (key.EndsWith("ANSWERE_TX") && Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 24)
                {
                    Mul_tblData_Update = new Dictionary<string, object>();
                    Mul_tblData_Insert = new Dictionary<string, object>();

                    string[] split = key.Split('-');
                    int appId = Convert.ToInt32(HttpContext.Current.Session["APPL_Id"]);
                    int QUESTION_ID = Convert.ToInt32(split[0]);
                    SAR_AWARDS_REG_ID = Convert.ToInt32(frm["SAR_AWARDS_REG_ID"]);
                    string ANSWERE_TX = Convert.ToString(frm[QUESTION_ID + "-ANSWERE_TX"]);
                    string QUES_DESCRIPTION_TX = string.Empty;
                    if (frm[QUESTION_ID + "-QUES_DESCRIPTION_TX"] != null)
                        QUES_DESCRIPTION_TX = Convert.ToString(frm[QUESTION_ID + "-QUES_DESCRIPTION_TX"]);
                    HttpPostedFile hFileName = HttpContext.Current.Request.Files[QUESTION_ID + "-QUES_FILEUPLOADER"] as HttpPostedFile;

                    int ID = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ID"])))
                        ID = Convert.ToInt32(frm[QUESTION_ID + "-ID"]);
                    if (ID > 0)
                    {
                        screenType = "update";
                        Mul_tblData_Update.Add("APPL_Id", appId);
                        Mul_tblData_Update.Add("QUESTION_ID", QUESTION_ID);
                        Mul_tblData_Update.Add("SAR_AWARDS_REG_ID", SAR_AWARDS_REG_ID);
                        Mul_tblData_Update.Add("ANSWER_TX", ANSWERE_TX);
                        Mul_tblData_Update.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                        Mul_tblData_Update.Add("ID", ID);
                        if (!string.IsNullOrEmpty(QUES_DESCRIPTION_TX))
                            Mul_tblData_Update.Add("QUES_DESCRIPTION_TX", QUES_DESCRIPTION_TX);
                    }
                    else
                    {
                        screenType = "insert";
                        Mul_tblData_Insert.Add("APPL_Id", appId);
                        Mul_tblData_Insert.Add("QUESTION_ID", QUESTION_ID);
                        Mul_tblData_Insert.Add("SAR_AWARDS_REG_ID", SAR_AWARDS_REG_ID);
                        Mul_tblData_Insert.Add("ANSWER_TX", ANSWERE_TX);
                        Mul_tblData_Insert.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                        if (!string.IsNullOrEmpty(QUES_DESCRIPTION_TX))
                            Mul_tblData_Insert.Add("QUES_DESCRIPTION_TX", QUES_DESCRIPTION_TX);
                    }

                    #region For File Uploading
                    if (hFileName != null && !string.IsNullOrEmpty(hFileName.FileName))
                    {
                        QuestionIds += QUESTION_ID + ",";
                        string FolderName = "SAR\\Documents\\" + SAR_AWARDS_REG_ID+"\\"+appId;
                        string _path = Util.UtilService.getDocumentPath(FolderName);
                        string _FullPath = _path + "\\" + QUESTION_ID + "_" + Convert.ToString(hFileName.FileName);
                        if(ID > 0)Mul_tblData_Update.Add("QUES_FILENAME_TX", _FullPath);
                        else Mul_tblData_Insert.Add("QUES_FILENAME_TX", _FullPath);
                    }
                    #endregion                    

                    if (!string.IsNullOrEmpty(ANSWERE_TX.Trim()))
                    {
                        if(ID > 0) lstData1.Add(Mul_tblData_Update);
                        else lstData2.Add(Mul_tblData_Insert);
                    }
                }
                else if (key.EndsWith("ADMIN_MARKS_NM"))
                {
                    #region For Admin Marks
                    Mul_tblData_Update = new Dictionary<string, object>();
                    string[] split = key.Split('-');
                    int QUESTION_ID = Convert.ToInt32(split[0]);
                    string EVAL_REMARKS_TX = string.Empty;
                    int ID = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ID"])))
                        ID = Convert.ToInt32(frm[QUESTION_ID + "-ID"]);
                    if (ID > 0)
                        screenType = "update";

                    Mul_tblData_Update.Add("ID", ID);
                    decimal ADMIN_MARKS_NM = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ADMIN_MARKS_NM"])))
                    {
                        ADMIN_MARKS_NM = Convert.ToDecimal(frm[QUESTION_ID + "-ADMIN_MARKS_NM"]);
                        EVAL_REMARKS_TX = Convert.ToString(frm[QUESTION_ID + "-EVAL_REMARKS_TX"]);
                        Mul_tblData_Update.Add("ADMIN_MARKS_NM", ADMIN_MARKS_NM);
                        Mul_tblData_Update.Add("ADMIN_MARKS_DT", DateTime.Now);
                        Mul_tblData_Update.Add("EVAL_REMARKS_TX", EVAL_REMARKS_TX);                        
                    }
                    #endregion

                    //if (ADMIN_MARKS_NM > 0)
                    if (ID > 0)
                        lstData1.Add(Mul_tblData_Update);
                }
            }
            AppUrl = AppUrl + "/AddUpdate";
            if (lstData1.Count > 0)
            {
                lstData.Clear();
                lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
            }
            if(lstData2.Count > 0)
            {
                lstData.Clear();
                lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData2, conditions));
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
            }
            if(lstData1.Count > 0 || lstData2.Count > 0)
            {

                #region File upload on server
                if (act.StatMessage.ToLower() == "success" && HttpContext.Current.Request.Files.Count > 0
                    && act.DecryptData != null)
                {
                    int QuesId = 0;
                    string[] splitids = QuestionIds.Split(',');
                    for (int j = 0; j < splitids.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(splitids[j]))
                        {
                            QuesId = Convert.ToInt32(splitids[j]);
                            string FolderName = "SAR\\Documents\\" + SAR_AWARDS_REG_ID;
                            string _path = Util.UtilService.getDocumentPath(FolderName);
                            HttpPostedFile hFileName = HttpContext.Current.Request.Files[QuesId + "-QUES_FILEUPLOADER"] as HttpPostedFile;
                            string _FullPath = _path + "\\" + QuesId + "_" + Path.GetFileName(hFileName.FileName);

                            //REMOVE CODE AFTER ISSUE IS FIXED

                            if (!(Directory.Exists(_path)))
                                Directory.CreateDirectory(_path);

                            if (File.Exists(_FullPath))
                            {
                                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                                string NewFileName = QuesId + "_" + Timestamp.ToString() + Path.GetExtension(hFileName.FileName);
                                File.Move(_FullPath, _path + "\\" + NewFileName);

                            }
                            HttpContext.Current.Request.Files[QuesId + "-QUES_FILEUPLOADER"].SaveAs(_FullPath);
                        }
                    }
                }
                #endregion

                #region insert and update for SAR policy static table
                if (frm.AllKeys.Contains("SA_AUDIT_TEAM_MEM_T"))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    int QuestionId = Convert.ToInt32(frm["SA_AUDIT_TEAM_MEM_T"]);

                    //TODO STORE STATIC TABLE DATA :::: MEMBERS DATA

                    //for (int i = 0; i < 4; i++)
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-DATE_OF_METTING_DT"])) &&
                    //        !string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_TX"])))
                    //    {
                    //        int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                    //        Mul_tblData = new Dictionary<string, object>();
                    //        if (ID > 0 && i == 0)
                    //            screenType = "update";
                    //        Mul_tblData.Add("ID", ID);
                    //        Mul_tblData.Add("QUESTION_ID", QuestionId);
                    //        Mul_tblData.Add("SAR_AWARDS_REG_ID", SAR_AWARDS_REG_ID);
                    //        Mul_tblData.Add("DATE_OF_METTING_DT", Convert.ToDateTime(frm["" + i + "-DATE_OF_METTING_DT"]));
                    //        Mul_tblData.Add("NO_OF_COMM_MEMBER_TX", Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_TX"]));
                    //        Mul_tblData.Add("NO_OF_ATT_MEMBER_TX", Convert.ToString(frm["" + i + "-NO_OF_ATT_MEMBER_TX"]));
                    //        Mul_tblData.Add("PER_OF_ATTENDANCE_TX", Convert.ToString(frm["" + i + "-PER_OF_ATTENDANCE_TX"]));
                    //        Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                    //        lstData1.Add(Mul_tblData);
                    //    }
                    //}

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "SA_AUDIT_TEAM_MEM_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    }
                }
                #endregion               
            }
            else
            {
                act.StatCode = "0";
                act.StatMessage = "No record for insert";
            }
            #endregion

            return act;
        }
    }
}