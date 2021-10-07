using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using System.IO;

namespace ICSI_WebApp.BusinessLayer
{
    public class CGALayer
    {
        CSRLayer objCSRLayer = new CSRLayer();
        public CGALayer()
        {
            if (HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"] == null)
                GetActiveYear();
        }

        public ActionClass beforeForgotPassword(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@RandomPassword", objCSRLayer.RandomPassword());
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
            //actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string _email = frm["EMAIL_TX"];
            int UID = 0;

            if (_email != null && !_email.Trim().Equals(""))
            {
                DataTable dtRDetails = new DataTable();
                Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 98); //check records available or not     
                //conditions.Add("USER_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
                conditions.Add("EMAIL_TX", _email);
                conditions.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(s));

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtRDetails = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dtRDetails != null && dtRDetails.Rows != null && dtRDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRDetails.Rows.Count; i++)
                    {
                        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        d["ID"] = dtRDetails.Rows[i]["ID"].ToString();
                        //d["PASSWORD_TX"] = "5f4dcc3b5aa765d61d8327deb882cf99";
                        d["PASSWORD_TX"] = EncryptPass;
                        d["ORG_PASSWORD_TX"] = OrignalPass;

                        list.Add(d);
                        actionClass = UtilService.insertOrUpdate("CGA", "CGA_REGISTRATION_T", list);

                        // Update password in USER_T
                        if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
                        {
                            List<Dictionary<string, object>> list_Screen_T = new List<Dictionary<string, object>>();
                            Dictionary<string, object> d1 = new Dictionary<string, object>();
                            d1["ID"] = dtRDetails.Rows[i]["USER_ID"].ToString();
                            //d1["LOGIN_PWD_TX"] = "5f4dcc3b5aa765d61d8327deb882cf99";
                            d1["LOGIN_PWD_TX"] = EncryptPass;
                            UID = Convert.ToInt32(dtRDetails.Rows[i]["USER_ID"]);
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
                    actionClass.StatMessage = "You are not registered in Current Year.";
            }
            return actionClass;
        }

        public ActionClass beforeGeneralInstructions(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            #region Get CGA Awards Reg Id
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("USER_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

            JObject jdata = DBTable.GetData("fetch", conditions, "CGA_REGISTRATION_T", 0, 10, applicationSchema);
            DataTable dt = null;
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name.Equals("CGA_REGISTRATION_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                HttpContext.Current.Session["CGA_AWARDS_REG_ID"] = dt.Rows[0]["USER_ID"];
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", dt.Rows[0]["USER_ID"].ToString());
            }
            #endregion

            #region Proceed button hide and show
            conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("CGA_AWARDS_REG_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

            JObject jdata1 = DBTable.GetData("fetch", conditions, "CGA_AWARDS_APPROVAL_T", 0, 10, applicationSchema);
            dt = null;
            foreach (JProperty property in jdata1.Properties())
            {
                if (property.Name.Equals("CGA_AWARDS_APPROVAL_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "style='display:none;'");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "");
            #endregion

            return null;
        }

        public ActionClass afterGeneralInstructions(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            //if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))
            //    frm["s"] = "update";

            //act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass beforeGeneralInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CGA_AWARDS_REG_ID = GetCGAAwardsRegId(Convert.ToString(frm["ui"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(CGA_AWARDS_REG_ID));
            GetScreenData(frm, screen, out int GeneralInfId);
            Get_Submitted_Data(frm, screen);
            GetGeneralInfoAuditor(frm, screen, GeneralInfId);
            GetGeneralInfoChangeAuditor(frm, screen, GeneralInfId);
            GetGeneralInfoDirector(frm, screen, GeneralInfId);
            GetGeneralInfoChangeDirector(frm, screen, GeneralInfId);

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterGeneralInformation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))
                frm["s"] = "update";

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            if (act.StatMessage.ToLower() == "success")
            {
                int ID = 0;
                if (!string.IsNullOrEmpty(act.DecryptData))
                {
                    JObject res = JObject.Parse(act.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            ID = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                        }
                    }
                }

                #region Insert and update for Audiors
                List<Dictionary<string, object>> lstAuditors = new List<Dictionary<string, object>>();
                Dictionary<string, object> dAuditors;
                string TYPE_OF_AUDITOR = string.Empty;

                for (int i = 1; i <= 3; i++)
                {
                    if (!string.IsNullOrEmpty(frm["" + i + "-AUDITOR_NAME_TX"]))
                    {
                        if (i == 1)
                            TYPE_OF_AUDITOR = "Secretarial Auditor";
                        else if (i == 2)
                            TYPE_OF_AUDITOR = "Statutory Auditor";
                        else if (i == 3)
                            TYPE_OF_AUDITOR = "Cost Auditor";

                        dAuditors = new Dictionary<string, object>();
                        dAuditors["ID"] = frm["" + i + "-AUDTIOR_ID"];
                        dAuditors["GENERAL_INFO_ID"] = ID;
                        dAuditors["TYPE_OF_AUDITOR_TX"] = TYPE_OF_AUDITOR;
                        dAuditors["AUDITOR_NAME_TX"] = frm["" + i + "-AUDITOR_NAME_TX"];
                        dAuditors["APPOINTMENT_DT"] = frm["" + i + "-APPOINTMENT_DT"];
                        dAuditors["EMAIL_ID_TX"] = frm["" + i + "-EMAIL_ID_TX"];
                        dAuditors["MEMBERSHIP_NO_TX"] = frm["" + i + "-MEMBERSHIP_NO_TX"];
                        dAuditors["TELEPHONE_NO_TX"] = frm["" + i + "-TELEPHONE_NO_TX"];
                        dAuditors["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];

                        lstAuditors.Add(dAuditors);
                    }
                }

                if (lstAuditors.Count > 0)
                    act = UtilService.insertOrUpdate("CGA", "GENERAL_INFO_AUDITOR_T", lstAuditors);
                #endregion

                #region Insert and update for Change Audiors
                lstAuditors = new List<Dictionary<string, object>>();

                for (int i = 1; i <= 3; i++)
                {
                    if (!string.IsNullOrEmpty(frm["" + i + "-CHG_AUDITOR_NAME_TX"]))
                    {
                        if (i == 1)
                            TYPE_OF_AUDITOR = "Secretarial Auditor";
                        else if (i == 2)
                            TYPE_OF_AUDITOR = "Statutory Auditor";
                        else if (i == 3)
                            TYPE_OF_AUDITOR = "Cost Auditor";

                        dAuditors = new Dictionary<string, object>();
                        dAuditors["ID"] = frm["" + i + "-CHG_AUDTIOR_ID"];
                        dAuditors["GENERAL_INFO_ID"] = ID;
                        dAuditors["TYPE_OF_AUDITOR_TX"] = TYPE_OF_AUDITOR;
                        dAuditors["AUDITOR_NAME_TX"] = frm["" + i + "-CHG_AUDITOR_NAME_TX"];
                        dAuditors["DATE_OF_CHG_CESS_DT"] = frm["" + i + "-DATE_OF_CHG_CESS_DT"];
                        dAuditors["REASON_OF_CHG_CESS_TX"] = frm["" + i + "-REASON_OF_CHG_CESS_TX"];
                        dAuditors["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];

                        lstAuditors.Add(dAuditors);
                    }
                }

                if (lstAuditors.Count > 0)
                    act = UtilService.insertOrUpdate("CGA", "GENERAL_INFO_CHG_AUDITOR_T", lstAuditors);
                #endregion

                #region Insert and update for Directors
                List<Dictionary<string, object>> lstDirectors = new List<Dictionary<string, object>>();
                Dictionary<string, object> dDirectors;

                for (int i = 1; i <= 20; i++)
                {
                    if (!string.IsNullOrEmpty(frm["" + i + "-DIRECTOR_NAME_TX"]))
                    {
                        dDirectors = new Dictionary<string, object>();
                        dDirectors["ID"] = frm["" + i + "-DIR_ID"];
                        dDirectors["GENERAL_INFO_ID"] = ID;
                        dDirectors["DIRECTOR_NAME_TX"] = frm["" + i + "-DIRECTOR_NAME_TX"];
                        dDirectors["APPOINTMENT_DT"] = frm["" + i + "-DIR_APPOINTMENT_DT"];
                        dDirectors["ADDRESS_TX"] = frm["" + i + "-ADDRESS_TX"];
                        dDirectors["TELEPHONE_NO_TX"] = frm["" + i + "-DIR_TELEPHONE_NO_TX"];
                        dDirectors["EMAIL_ID_TX"] = frm["" + i + "-DIR_EMAIL_ID_TX"];
                        dDirectors["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];

                        lstDirectors.Add(dDirectors);
                    }
                }

                if (lstDirectors.Count > 0)
                    act = UtilService.insertOrUpdate("CGA", "GENERAL_INFO_DIRECTOR_T", lstDirectors);
                #endregion

                #region Insert and update for Change Directors
                lstDirectors = new List<Dictionary<string, object>>();

                for (int i = 1; i <= 5; i++)
                {
                    if (!string.IsNullOrEmpty(frm["" + i + "-CHG_DIRECTOR_NAME_TX"]))
                    {
                        dDirectors = new Dictionary<string, object>();
                        dDirectors["ID"] = frm["" + i + "-CHG_DIR_ID"];
                        dDirectors["GENERAL_INFO_ID"] = ID;
                        dDirectors["DIRECTOR_NAME_TX"] = frm["" + i + "-CHG_DIRECTOR_NAME_TX"];
                        dDirectors["DOJ_DT"] = frm["" + i + "-DIR_DOJ_DT"];
                        dDirectors["DOR_DT"] = frm["" + i + "-DIR_DOR_DT"];
                        dDirectors["REASON_OF_CESS_TX"] = frm["" + i + "-REASON_OF_CESS_TX"];
                        dDirectors["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];

                        lstDirectors.Add(dDirectors);
                    }
                }

                if (lstDirectors.Count > 0)
                    act = UtilService.insertOrUpdate("CGA", "GENERAL_INFO_CHG_DIRECTOR_T", lstDirectors);
                #endregion
            }

            return act;
        }

        private void GetScreenData(FormCollection frm, Screen_T screen, out int GeneralInfId, bool PrintPDF = false)
        {
            GeneralInfId = 0;
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            if (PrintPDF)
                conditions.Add("CGA_AWARDS_REG_ID", frm["ui"]);
            else
                conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId(""));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

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
                    if (TokenName == "ID")
                        GeneralInfId = Convert.ToInt32(TokenValue);
                    if (screen.Screen_Content_Tx != null)
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                }
            }
        }

        private void Get_Submitted_Data(FormCollection frm, Screen_T screen)
        {
            DataTable dtAD = new DataTable();
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 99);
            conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId(""));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

            JObject jdata = DBTable.GetData("qfetch", conditions, "CGA_AWARDS_APPROVAL_T", 0, 10, applicationSchema);
            string TokenName = string.Empty;
            Object TokenValue = string.Empty;
            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtAD = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }

                if (dtAD != null && dtAD.Rows != null && dtAD.Rows.Count > 0)
                {
                    HttpContext.Current.Session["CGA_APPROVE_NM"] = dtAD.Rows[0]["APPROVE_NM"].ToString();
                    HttpContext.Current.Session["CGA_APPROVE2_NM"] = dtAD.Rows[0]["APPROVE2_NM"].ToString();
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", dtAD.Rows[0]["APPROVE_NM"].ToString());
                    //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@NAME_OF_COMPANY_TX", dtAD.Rows[0]["NAME_OF_COMPANY_TX"].ToString());
                }
                else
                {
                    HttpContext.Current.Session["CGA_APPROVE_NM"] = "";
                    HttpContext.Current.Session["CGA_APPROVE2_NM"] = "";
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", "");
                }
            }
            else
            {
                HttpContext.Current.Session["CGA_APPROVE_NM"] = "";
                HttpContext.Current.Session["CGA_APPROVE2_NM"] = "";
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", "");
            }
        }

        private int GetCGAAwardsRegId(string UniqueId)
        {
            int CGA_AWARDS_REG_ID = 0;
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 15 && !string.IsNullOrEmpty(UniqueId) && UniqueId != "undefined")//For Admin
            {
                CGA_AWARDS_REG_ID = Convert.ToInt32(UniqueId);
                HttpContext.Current.Session["CGA_AWARDS_REG_ID"] = CGA_AWARDS_REG_ID;
            }
            else if (HttpContext.Current.Session["CGA_AWARDS_REG_ID"] != null)
                CGA_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["CGA_AWARDS_REG_ID"]);
            else
                CGA_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);

            return CGA_AWARDS_REG_ID;
        }

        private void GetGeneralInfoAuditor(FormCollection frm, Screen_T screen, int GeneralInfId)
        {
            StringBuilder sbAuditors = new StringBuilder();
            string TYPE_OF_AUDITOR = string.Empty;
            DataTable dtAuditors = new DataTable();

            if (GeneralInfId > 0)
            {
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("GENERAL_INFO_ID", GeneralInfId);
                conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

                JObject jdata = DBTable.GetData("fetch", conditions, "GENERAL_INFO_AUDITOR_T", 0, 10, applicationSchema);
                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "GENERAL_INFO_AUDITOR_T")
                            dtAuditors = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            int totalAuditorRows = 0;
            if (dtAuditors != null && dtAuditors.Rows != null && dtAuditors.Rows.Count > 0)
                totalAuditorRows = dtAuditors.Rows.Count;

            for (int i = 1; i <= 3; i++)
            {
                if (i == 1)
                    TYPE_OF_AUDITOR = "Secretarial Auditor";
                else if (i == 2)
                    TYPE_OF_AUDITOR = "Statutory Auditor";
                else if (i == 3)
                    TYPE_OF_AUDITOR = "Cost Auditor";

                if (totalAuditorRows > 0 && i <= totalAuditorRows)
                {
                    int j = i - 1;
                    string APPOINTMENT_DT = string.Empty;
                    if (!string.IsNullOrEmpty(Convert.ToString(dtAuditors.Rows[j]["APPOINTMENT_DT"])))
                    {
                        APPOINTMENT_DT = Convert.ToDateTime(dtAuditors.Rows[j]["APPOINTMENT_DT"]).Date.ToString("yyyy-MM-dd");
                        if (APPOINTMENT_DT == "1900-01-01".Trim())
                            APPOINTMENT_DT = string.Empty;
                    }

                    sbAuditors.Append("<tr>");
                    sbAuditors.Append("<td><input type='hidden' value='" + dtAuditors.Rows[j]["ID"] + "' name='" + i + "-AUDTIOR_ID'/><lable name='" + i + "-TYPE_OF_AUDITOR_TX'>" + TYPE_OF_AUDITOR + "</lable></td>");
                    sbAuditors.Append("<td><input name='" + i + "-AUDITOR_NAME_TX' type='text' value='" + dtAuditors.Rows[j]["AUDITOR_NAME_TX"] + "' class='form-control'></td>");
                    sbAuditors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-APPOINTMENT_DT' value='" + APPOINTMENT_DT + "' class='form-control'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-EMAIL_ID_TX' type='email' value='" + dtAuditors.Rows[j]["EMAIL_ID_TX"] + "' class='form-control'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-TELEPHONE_NO_TX' type='text' value='" + dtAuditors.Rows[j]["TELEPHONE_NO_TX"] + "' class='form-control' maxlength='15' onkeypress='return isNumberKey(event)'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-MEMBERSHIP_NO_TX' type='text' value='" + dtAuditors.Rows[j]["MEMBERSHIP_NO_TX"] + "' class='form-control'></td>");
                    sbAuditors.Append("</tr>");
                }
                else
                {
                    sbAuditors.Append("<tr>");
                    sbAuditors.Append("<td><input type='hidden' value='0' name='" + i + "-AUDTIOR_ID'/><lable name='" + i + "-TYPE_OF_AUDITOR_TX'>" + TYPE_OF_AUDITOR + "</lable></td>");
                    sbAuditors.Append("<td><input name='" + i + "-AUDITOR_NAME_TX' type='text' value='' class='form-control'></td>");
                    sbAuditors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-APPOINTMENT_DT' value='' class='form-control'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-EMAIL_ID_TX' type='email' value='' class='form-control'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-TELEPHONE_NO_TX' type='text' value='' class='form-control' maxlength='15' onkeypress='return isNumberKey(event)'></td>");
                    sbAuditors.Append("<td><input name='" + i + "-MEMBERSHIP_NO_TX' type='text' value='' class='form-control'></td>");
                    sbAuditors.Append("</tr>");
                }
            }

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TblAuditors", sbAuditors.ToString());
        }

        private void GetGeneralInfoChangeAuditor(FormCollection frm, Screen_T screen, int GeneralInfId)
        {
            StringBuilder sbAuditors = new StringBuilder();
            string TYPE_OF_AUDITOR = string.Empty;
            DataTable dtAuditors = new DataTable();

            if (GeneralInfId > 0)
            {
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("GENERAL_INFO_ID", GeneralInfId);
                conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

                JObject jdata = DBTable.GetData("fetch", conditions, "GENERAL_INFO_CHG_AUDITOR_T", 0, 10, applicationSchema);
                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "GENERAL_INFO_CHG_AUDITOR_T")
                            dtAuditors = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            int totalAuditorRows = 0;
            if (dtAuditors != null && dtAuditors.Rows != null && dtAuditors.Rows.Count > 0)
                totalAuditorRows = dtAuditors.Rows.Count;

            for (int i = 1; i <= 3; i++)
            {
                if (i == 1)
                    TYPE_OF_AUDITOR = "Secretarial Auditor";
                else if (i == 2)
                    TYPE_OF_AUDITOR = "Statutory Auditor";
                else if (i == 3)
                    TYPE_OF_AUDITOR = "Cost Auditor";

                if (totalAuditorRows > 0 && i <= totalAuditorRows)
                {
                    int j = i - 1;
                    string DATE_OF_CHG_CESS = string.Empty;
                    if (!string.IsNullOrEmpty(Convert.ToString(dtAuditors.Rows[j]["DATE_OF_CHG_CESS_DT"])))
                    {
                        DATE_OF_CHG_CESS = Convert.ToDateTime(dtAuditors.Rows[j]["DATE_OF_CHG_CESS_DT"]).Date.ToString("yyyy-MM-dd");
                        if (DATE_OF_CHG_CESS == "1900-01-01".Trim())
                            DATE_OF_CHG_CESS = string.Empty;
                    }

                    sbAuditors.Append("<tr>");
                    sbAuditors.Append("<td><input type='hidden' value='" + dtAuditors.Rows[j]["ID"] + "' name='" + i + "-CHG_AUDTIOR_ID'/><lable name='" + i + "-CHG_TYPE_OF_AUDITOR_TX'>" + TYPE_OF_AUDITOR + "</lable></td>");
                    sbAuditors.Append("<td><input name='" + i + "-CHG_AUDITOR_NAME_TX' type='text' value='" + dtAuditors.Rows[j]["AUDITOR_NAME_TX"] + "' class='form-control'></td>");
                    sbAuditors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_CHG_CESS_DT' value='" + DATE_OF_CHG_CESS + "' class='form-control'></td>");
                    sbAuditors.Append("<td><textarea name='" + i + "-REASON_OF_CHG_CESS_TX' class='form-control' maxlength='300'>" + dtAuditors.Rows[j]["REASON_OF_CHG_CESS_TX"] + "</textarea></td>");
                    sbAuditors.Append("</tr>");
                }
                else
                {
                    sbAuditors.Append("<tr>");
                    sbAuditors.Append("<td><input type='hidden' value='0' name='" + i + "-CHG_AUDTIOR_ID'/><lable name='" + i + "-CHG_TYPE_OF_AUDITOR_TX'>" + TYPE_OF_AUDITOR + "</lable></td>");
                    sbAuditors.Append("<td><input name='" + i + "-CHG_AUDITOR_NAME_TX' type='text' value='' class='form-control'></td>");
                    sbAuditors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_CHG_CESS_DT' value='' class='form-control'></td>");
                    sbAuditors.Append("<td><textarea name='" + i + "-REASON_OF_CHG_CESS_TX' value='' class='form-control' maxlength='300'></textarea></td>");
                    sbAuditors.Append("</tr>");
                }
            }

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TblChgAuditors", sbAuditors.ToString());
        }

        private void GetGeneralInfoDirector(FormCollection frm, Screen_T screen, int GeneralInfId)
        {
            StringBuilder sbDirectors = new StringBuilder();
            DataTable dtDirectors = new DataTable();
            int StaticRow = 5;

            if (GeneralInfId > 0)
            {
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("GENERAL_INFO_ID", GeneralInfId);
                conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

                JObject jdata = DBTable.GetData("fetch", conditions, "GENERAL_INFO_DIRECTOR_T", 0, 10, applicationSchema);
                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "GENERAL_INFO_DIRECTOR_T")
                            dtDirectors = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            int totalDirectorRows = 0;
            if (dtDirectors != null && dtDirectors.Rows != null && dtDirectors.Rows.Count > 0)
                totalDirectorRows = dtDirectors.Rows.Count;

            if (totalDirectorRows > StaticRow)
                StaticRow = totalDirectorRows;
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@LAST_ADDED_ROW", StaticRow.ToString());

            for (int i = 1; i <= StaticRow; i++)
            {
                if (totalDirectorRows > 0 && i <= totalDirectorRows)
                {
                    int j = i - 1;
                    string APPOINTMENT_DT = string.Empty;
                    if (!string.IsNullOrEmpty(Convert.ToString(dtDirectors.Rows[j]["APPOINTMENT_DT"])))
                    {
                        APPOINTMENT_DT = Convert.ToDateTime(dtDirectors.Rows[j]["APPOINTMENT_DT"]).Date.ToString("yyyy-MM-dd");
                        if (APPOINTMENT_DT == "1900-01-01".Trim())
                            APPOINTMENT_DT = string.Empty;
                    }

                    sbDirectors.Append("<tr>");
                    sbDirectors.Append("<td><input type='hidden' value='" + dtDirectors.Rows[j]["ID"] + "' name='" + i + "-DIR_ID'/><input name='" + i + "-DIRECTOR_NAME_TX' type='text' value='" + dtDirectors.Rows[j]["DIRECTOR_NAME_TX"] + "' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_APPOINTMENT_DT' value='" + APPOINTMENT_DT + "' class='form-control'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-ADDRESS_TX' type='text' value='" + dtDirectors.Rows[j]["ADDRESS_TX"] + "' class='form-control'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-DIR_TELEPHONE_NO_TX' type='text' value='" + dtDirectors.Rows[j]["TELEPHONE_NO_TX"] + "' class='form-control' maxlength='15' onkeypress='return isNumberKey(event)'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-DIR_EMAIL_ID_TX' type='email' value='" + dtDirectors.Rows[j]["EMAIL_ID_TX"] + "' class='form-control'></td>");
                    sbDirectors.Append("</tr>");
                }
                else
                {
                    sbDirectors.Append("<tr>");
                    sbDirectors.Append("<td><input type='hidden' value='0' name='" + i + "-DIR_ID'/><input name='" + i + "-DIRECTOR_NAME_TX' type='text' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_APPOINTMENT_DT' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-ADDRESS_TX' type='text' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-DIR_TELEPHONE_NO_TX' type='text' value='' class='form-control' maxlength='15' onkeypress='return isNumberKey(event)'></td>");
                    sbDirectors.Append("<td><input name='" + i + "-DIR_EMAIL_ID_TX' type='email' value='' class='form-control'></td>");
                    sbDirectors.Append("</tr>");
                }
            }
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TblDirectors", sbDirectors.ToString());
        }

        private void GetGeneralInfoChangeDirector(FormCollection frm, Screen_T screen, int GeneralInfId)
        {
            StringBuilder sbDirectors = new StringBuilder();
            DataTable dtDirectors = new DataTable();

            if (GeneralInfId > 0)
            {
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("GENERAL_INFO_ID", GeneralInfId);
                conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

                JObject jdata = DBTable.GetData("fetch", conditions, "GENERAL_INFO_CHG_DIRECTOR_T", 0, 10, applicationSchema);
                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "GENERAL_INFO_CHG_DIRECTOR_T")
                            dtDirectors = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            int totalDirectorRows = 0;
            if (dtDirectors != null && dtDirectors.Rows != null && dtDirectors.Rows.Count > 0)
                totalDirectorRows = dtDirectors.Rows.Count;

            for (int i = 1; i <= 5; i++)
            {
                if (totalDirectorRows > 0 && i <= totalDirectorRows)
                {
                    int j = i - 1;
                    string DATE_OF_JOINING = string.Empty;
                    if (!string.IsNullOrEmpty(Convert.ToString(dtDirectors.Rows[j]["DOJ_DT"])))
                    {
                        DATE_OF_JOINING = Convert.ToDateTime(dtDirectors.Rows[j]["DOJ_DT"]).Date.ToString("yyyy-MM-dd");
                        if (DATE_OF_JOINING == "1900-01-01".Trim())
                            DATE_OF_JOINING = string.Empty;
                    }

                    string DATE_OF_RESIGNING = string.Empty;
                    if (!string.IsNullOrEmpty(Convert.ToString(dtDirectors.Rows[j]["DOR_DT"])))
                    {
                        DATE_OF_RESIGNING = Convert.ToDateTime(dtDirectors.Rows[j]["DOR_DT"]).Date.ToString("yyyy-MM-dd");
                        if (DATE_OF_RESIGNING == "1900-01-01".Trim())
                            DATE_OF_RESIGNING = string.Empty;
                    }

                    sbDirectors.Append("<tr>");
                    sbDirectors.Append("<td><input type='hidden' value='" + dtDirectors.Rows[j]["ID"] + "' name='" + i + "-CHG_DIR_ID'/><input name='" + i + "-CHG_DIRECTOR_NAME_TX' type='text' value='" + dtDirectors.Rows[j]["DIRECTOR_NAME_TX"] + "' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_DOJ_DT' value='" + DATE_OF_JOINING + "' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_DOR_DT' value='" + DATE_OF_RESIGNING + "' class='form-control'></td>");
                    sbDirectors.Append("<td><textarea name='" + i + "-REASON_OF_CESS_TX' class='form-control' maxlength='300'>" + dtDirectors.Rows[j]["REASON_OF_CESS_TX"] + "</textarea></td>");
                    sbDirectors.Append("</tr>");
                }
                else
                {
                    sbDirectors.Append("<tr>");
                    sbDirectors.Append("<td><input type='hidden' value='0' name='" + i + "-CHG_DIR_ID'/><input name='" + i + "-CHG_DIRECTOR_NAME_TX' type='text' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_DOJ_DT' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DIR_DOR_DT' value='' class='form-control'></td>");
                    sbDirectors.Append("<td><textarea name='" + i + "-REASON_OF_CESS_TX' value='' class='form-control' maxlength='300'></textarea></td>");
                    sbDirectors.Append("</tr>");
                }
            }
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TblChgDirectors", sbDirectors.ToString());
        }

        public ActionClass beforeCompanyInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CGA_AWARDS_REG_ID = GetCGAAwardsRegId(Convert.ToString(frm["ui"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(CGA_AWARDS_REG_ID));
            GetScreenData(frm, screen, out int id);

            Get_Submitted_Data(frm, screen);
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCompanyInformation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))
                frm["s"] = "update";

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass beforeFinancialInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = null;
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", GetCGAAwardsRegId("").ToString());
            BindFinancialInformation(screen);

            act = Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterFinancialInformation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            Dictionary<string, object> Mul_tblData;
            string screenType = frm["s"];
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);

            #region For Insertion and updation
            foreach (var key in frm.AllKeys)
            {
                if (key.EndsWith("FIN_YEAR_1_TX"))
                {
                    Mul_tblData = new Dictionary<string, object>();

                    string[] split = frm[key].Split('_');
                    int FIN_HEAD_ID = Convert.ToInt32(split[0]);
                    int CGA_AWARDS_REG_ID = Convert.ToInt32(frm["CGA_AWARDS_REG_ID"]);
                    string CurrentFinYear = Convert.ToString(split[1]);
                    string PrevFinYear = frm[FIN_HEAD_ID + "-FIN_YEAR_2_TX"].Split('_')[1];
                    string PrevToPrevFinYear = frm[FIN_HEAD_ID + "-FIN_YEAR_3_TX"].Split('_')[1];
                    int ID = Convert.ToInt32(frm[FIN_HEAD_ID + "-ID"]);
                    decimal SFS_1_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-SFS_1_NM"]);
                    decimal SFS_2_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-SFS_2_NM"]);
                    decimal SFS_3_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-SFS_3_NM"]);
                    decimal CFS_1_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-CFS_1_NM"]);
                    decimal CFS_2_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-CFS_2_NM"]);
                    decimal CFS_3_NM = Convert.ToDecimal(frm[FIN_HEAD_ID + "-CFS_3_NM"]);
                    string Formula = Convert.ToString(frm[FIN_HEAD_ID + "-FORMULA_TX"]);
                    if (ID > 0)
                        screenType = "update";

                    Mul_tblData.Add("ID", ID);
                    Mul_tblData.Add("FIN_HEAD_ID", FIN_HEAD_ID);
                    Mul_tblData.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
                    Mul_tblData.Add("FIN_YEAR_1_TX", CurrentFinYear);
                    Mul_tblData.Add("SFS_1_NM", SFS_1_NM);
                    Mul_tblData.Add("CFS_1_NM", CFS_1_NM);
                    Mul_tblData.Add("FIN_YEAR_2_TX", PrevFinYear);
                    Mul_tblData.Add("SFS_2_NM", SFS_2_NM);
                    Mul_tblData.Add("CFS_2_NM", CFS_2_NM);
                    Mul_tblData.Add("FIN_YEAR_3_TX", PrevToPrevFinYear);
                    Mul_tblData.Add("SFS_3_NM", SFS_3_NM);
                    Mul_tblData.Add("CFS_3_NM", CFS_3_NM);
                    Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                    Mul_tblData.Add("FORMULA_TX", Formula);

                    lstData1.Add(Mul_tblData);
                }
            }

            AppUrl = AppUrl + "/AddUpdate";
            lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
            act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
            #endregion

            return act;
        }

        private void BindFinancialInformation(Screen_T screen, bool PDFPrint = false, int UniqueId = 0)
        {
            #region Bind Financial Head
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 102); //check records available or not 
            if (UniqueId == 0)
                conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId("").ToString());
            else
                conditions.Add("CGA_AWARDS_REG_ID", UniqueId);

            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));
            DataTable dt = new DataTable();
            DataTable dtavl = new DataTable();

            if (jdata == null || jdata.First.First.First == null)
            {
                conditions = new Dictionary<string, object>();
                conditions.Add("QID", 72);//when records are not available
                jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            else
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtavl = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                #region Bind Last three financial year
                string PrevToPrevFinYear = objCSRLayer.GetFinancialYear(DateTime.Now.AddYears(-3).Year, DateTime.Now.AddYears(-4).Year, DateTime.Now.AddYears(-2).Year);
                string PrevFinYear = objCSRLayer.GetFinancialYear(DateTime.Now.AddYears(-2).Year, DateTime.Now.AddYears(-2).Year, DateTime.Now.AddYears(-1).Year);
                string CurrentFinYear = objCSRLayer.GetFinancialYear(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-1).Year, DateTime.Now.Year);

                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_YEAR", "<th colspan='2'>" + PrevToPrevFinYear + "</th><th colspan='2'>" + PrevFinYear + "</th><th colspan='2'>" + CurrentFinYear + "</th>");
                #endregion

                StringBuilder sbFinancialHead = new StringBuilder();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sbFinancialHead.Append("<tr id='" + i + "-trHead'>");
                    sbFinancialHead.Append("<td style='text-align:left; font-weight:bold;'>" + dt.Rows[i]["FINANCIAL_HEAD_TX"] + " </td>");
                    if (Convert.ToBoolean(dt.Rows[i]["TEXTBOX_YN"]) == true)
                        sbFinancialHead.Append("<td><input id='" + i + "-FORMULA_TX' type='text' style='width:130px;' name='" + dt.Rows[i]["ID"] + "-FORMULA_TX' class='form-control' value=''></td>");
                    else
                        sbFinancialHead.Append("<td></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_1_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevToPrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-SFS_1_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input id='" + i + "-CFS_1_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-CFS_1_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_2_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-SFS_2_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input id='" + i + "-CFS_2_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-CFS_2_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_3_TX' value='" + dt.Rows[i]["ID"] + "_" + CurrentFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-SFS_3_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input id='" + i + "-CFS_3_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-CFS_3_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("</tr>");
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_HEAD", sbFinancialHead.ToString());
            }
            else if (dtavl != null && dtavl.Rows != null && dtavl.Rows.Count > 0)
            {
                #region Bind Last three financial year
                string PrevToPrevFinYear = Convert.ToString(dtavl.Rows[0]["FIN_YEAR_1_TX"]);
                string PrevFinYear = Convert.ToString(dtavl.Rows[0]["FIN_YEAR_2_TX"]);
                string CurrentFinYear = Convert.ToString(dtavl.Rows[0]["FIN_YEAR_3_TX"]);
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_YEAR", "<th colspan='2'>" + PrevToPrevFinYear + "</th><th colspan='2'>" + PrevFinYear + "</th><th colspan='2'>" + CurrentFinYear + "</th>");
                #endregion

                StringBuilder sbFinancialHead = new StringBuilder();
                for (int i = 0; i < dtavl.Rows.Count; i++)
                {
                    if (PDFPrint)
                    {
                        sbFinancialHead.Append("<tr id='" + i + "-trHead'>");
                        sbFinancialHead.Append("<td style='text-align:left; font-weight:bold;'>" + dtavl.Rows[i]["FINANCIAL_HEAD_TX"] + " </td>");
                        if (Convert.ToBoolean(dtavl.Rows[i]["TEXTBOX_YN"]) == true)
                            sbFinancialHead.Append("<td>" + dtavl.Rows[i]["FORMULA_TX"] + "</td>");
                        else
                            sbFinancialHead.Append("<td></td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_1_NM"])) + "</td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_1_NM"])) + "</td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_2_NM"])) + "</td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_2_NM"])) + "</td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_3_NM"])) + "</td>");
                        sbFinancialHead.Append("<td>" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_3_NM"])) + "</td>");
                        sbFinancialHead.Append("</tr>");
                    }
                    else
                    {
                        sbFinancialHead.Append("<tr id='" + i + "-trHead'>");
                        sbFinancialHead.Append("<td style='text-align:left; font-weight:bold;'>" + dtavl.Rows[i]["FINANCIAL_HEAD_TX"] + " </td>");
                        if (Convert.ToBoolean(dtavl.Rows[i]["TEXTBOX_YN"]) == true)
                            sbFinancialHead.Append("<td><input id='" + i + "-FORMULA_TX' type='text' style='width:130px;' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FORMULA_TX' class='form-control' value='" + dtavl.Rows[i]["FORMULA_TX"] + "'></td>");
                        else
                            sbFinancialHead.Append("<td></td>");
                        sbFinancialHead.Append("<td><input type='hidden' value='" + dtavl.Rows[i]["ID"] + "' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-ID'/><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_1_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + CurrentFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-SFS_1_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_1_NM"])) + "'></td>");
                        sbFinancialHead.Append("<td><input id='" + i + "-CFS_1_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-CFS_1_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_1_NM"])) + "'></td>");
                        sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_2_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + PrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-SFS_2_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_2_NM"])) + "'></td>");
                        sbFinancialHead.Append("<td><input id='" + i + "-CFS_2_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-CFS_2_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_2_NM"])) + "'></td>");
                        sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_3_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + PrevToPrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-SFS_3_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["SFS_3_NM"])) + "'></td>");
                        sbFinancialHead.Append("<td><input id='" + i + "-CFS_3_NM' type='text' onkeypress='return isFNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-CFS_3_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["CFS_3_NM"])) + "'></td>");
                        sbFinancialHead.Append("</tr>");
                    }
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_HEAD", sbFinancialHead.ToString());
            }
            #endregion
        }

        public ActionClass beforeCGARegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCGARegistration(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            DataTable dtReg = new DataTable();
            Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 108);
            conditions.Add("EMAIL_TX", frm["EMAIL_TX"]);
            conditions.Add("NAME_OF_COMPANY_TX", frm["NAME_OF_COMPANY_TX"]);

            JObject jdata = DBTable.GetData("qfetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtReg = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (Convert.ToString(dtReg.Rows[0]["Msg"]) == "1")
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            else
                act.StatMessage = Convert.ToString(dtReg.Rows[0]["Msg"]);

            return act;
        }

        public ActionClass beforeBoardStructure(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Structure), 110, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Structure));

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterBoardStructure(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeBoardProcesses(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes), 112, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes));

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterBoardProcesses(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        private void BindDynamicPage(Screen_T screen, int CATEGORY_TYPE_ID, int AnswereID, int SrNo, string QUESTION_REPLACE = "@Questions", int UniqueId = 0, bool isCleanPDF = false)
        {
            Dictionary<string, object> conditions;
            DataTable dtOP = new DataTable();
            DataTable dtANS = new DataTable();
            string TableName = screen.Table_Name_Tx;//GetTableName(screen.ID);
            int UploadedFiles = 0;
            int TotalFileUploader = 0;

            conditions = new Dictionary<string, object>();
            conditions.Add("QID", 75);
            conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
            JObject jdataOPS = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 200, Util.UtilService.getApplicationScheme(screen));

            if (jdataOPS != null && jdataOPS.First.First.First != null)
            {
                foreach (JProperty property in jdataOPS.Properties())
                {
                    if (property.Name == "qfetch")
                        dtOP = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            conditions = new Dictionary<string, object>();
            conditions.Add("QID", AnswereID);
            if (UniqueId != 0)
                conditions.Add("CGA_AWARDS_REG_ID", UniqueId);
            else
                conditions.Add("CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
            JObject jAnswers = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

            if (jAnswers != null && jAnswers.First.First.First != null)
            {
                foreach (JProperty property in jAnswers.Properties())
                {
                    if (property.Name == "qfetch")
                    {
                        dtANS = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        if (dtANS != null && dtANS.Rows != null && dtANS.Rows.Count > 0)
                            TotalFileUploader = dtANS.Select("Q_ENABLE_FILEUPLOADER_YN = " + true).Count();
                    }
                }
            }

            if (dtANS != null && dtANS.Rows != null && dtANS.Rows.Count > 0)
            {
                #region With Ans Bind
                int count = 0;
                int AdminMarks = 0;
                StringBuilder sbQuestions = new StringBuilder();
                for (int i = 0; i < dtANS.Rows.Count; i++)
                {
                    if (Convert.ToString(dtANS.Rows[i]["REF_ID"]) == "0")
                        count++;
                    sbQuestions.Append("<div><input type='hidden' value='" + dtANS.Rows[i]["ID"] + "' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-ID'/></div>");
                    if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                    {
                        sbQuestions.Append("<div class='col-xs-12'>");
                        sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtANS.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                        //sbQuestions.Append("<div class='text-right'>Marks: " + dtANS.Rows[i]["MARKS_NM"].ToString() + " </div>");
                        sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='4' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-ANSWERE_TX' value='' class='form-control' onkeydown='maxlength(this)'>" + Convert.ToString(dtANS.Rows[i]["ANSWER_TX"]) + " </textarea></div>");
                        sbQuestions.Append("</div>");
                        if (dtANS.Rows[i]["Q_ENABLE_FILEUPLOADER_YN"].ToString().ToUpper() == "TRUE")
                        {
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            if (!string.IsNullOrEmpty(Convert.ToString(dtANS.Rows[i]["QUES_FILENAME_TX"])))
                            {
                                UploadedFiles++;
                                string[] ArrFileName = Convert.ToString(dtANS.Rows[i]["QUES_FILENAME_TX"]).Split('\\');
                                string FileName = ArrFileName[ArrFileName.Length - 1];
                                sbQuestions.Append("&nbsp;&nbsp;<a target='_blank' style='color:blue;' title='Click to view' href='../CGA/DownloadFileByIDFromSpecificTable?id=" + dtANS.Rows[i]["ID"] + "&TableName=" + TableName + "&ColumnName=QUES_FILENAME_TX&schema=CGA'>" + FileName + "</a>");
                                if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15 && QUESTION_REPLACE == "@Questions")
                                {
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                    sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtANS.Rows[i]["QUESTION_ID"] + ")' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                    sbQuestions.Append("</div>");
                                }
                            }
                            else if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15)
                            {
                                sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtANS.Rows[i]["QUESTION_ID"] + ")' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                sbQuestions.Append("</div>");
                                sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtANS.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label><br/>");
                                sbQuestions.Append("<label id='" + dtANS.Rows[i]["QUESTION_ID"] + "_lblFile' style='font-weight:normal; font-size:12px;color:red;'>File is not attached</label>");
                            }
                            sbQuestions.Append("</div>");
                        }
                    }
                    else if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "1")
                    {
                        sbQuestions.Append("<div class='col-xs-12'>");
                        if (!string.IsNullOrEmpty(Convert.ToString(dtANS.Rows[i]["REF_ID"])) && Convert.ToString(dtANS.Rows[i]["REF_ID"]) != "0")
                            sbQuestions.Append("<div class='fontBold pt-20'>" + dtANS.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                        else
                            sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtANS.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");

                        sbQuestions.Append("<div class='col-md-9'>");
                        for (int j = 0; j < dtOP.Rows.Count; j++)
                        {
                            if (dtANS.Rows[i]["QUESTION_ID"].ToString() == dtOP.Rows[j]["QUESTION_ID"].ToString())
                            {
                                if (Convert.ToString(dtOP.Rows[j]["ID"]) == Convert.ToString(dtANS.Rows[i]["ANSWER_TX"]))
                                    sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' checked='checked' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                else
                                    sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                            }
                        }
                        sbQuestions.Append("</div>");

                        if (dtANS.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                        {
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label>" + dtANS.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");
                            //sbQuestions.Append("<input type='text' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_DESCRIPTION_TX' value='" + dtANS.Rows[i]["QUES_DESCRIPTION_TX"] + "' class='form-control'>");
                            sbQuestions.Append("<textarea type='textarea' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'>" + dtANS.Rows[i]["QUES_DESCRIPTION_TX"] + "</textarea>");
                            sbQuestions.Append("</div>");
                        }

                        if (dtANS.Rows[i]["Q_ENABLE_FILEUPLOADER_YN"].ToString().ToUpper() == "TRUE")
                        {
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            if (!string.IsNullOrEmpty(Convert.ToString(dtANS.Rows[i]["QUES_FILENAME_TX"])))
                            {
                                UploadedFiles++;
                                string[] ArrFileName = Convert.ToString(dtANS.Rows[i]["QUES_FILENAME_TX"]).Split('\\');
                                string FileName = ArrFileName[ArrFileName.Length - 1];
                                sbQuestions.Append("&nbsp;&nbsp;<a target='_blank' style='color:blue;' title='Click to view' href='../CGA/DownloadFileByIDFromSpecificTable?id=" + dtANS.Rows[i]["ID"] + "&TableName=" + TableName + "&ColumnName=QUES_FILENAME_TX&schema=CGA'>" + FileName + "</a>");
                                if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15 && QUESTION_REPLACE == "@Questions")
                                {
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                    sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtANS.Rows[i]["QUESTION_ID"] + ")' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                    sbQuestions.Append("</div>");
                                }
                            }
                            else if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15)
                            {
                                sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtANS.Rows[i]["QUESTION_ID"] + ")' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                sbQuestions.Append("</div>");
                                sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtANS.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label><br/>");
                                sbQuestions.Append("<label id='" + dtANS.Rows[i]["QUESTION_ID"] + "_lblFile' style='font-weight:normal; font-size:12px;color:red;'>File is not attached</label>");
                            }
                            sbQuestions.Append("</div>");
                        }
                        sbQuestions.Append("</div>");
                    }
                    else if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                    {
                        if (screen.ID == 274)
                        {
                            if (Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]) == 43)
                                sbQuestions.Append(BindStaticTableForBoardProcess1(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', SrNo + "." + count));
                            else if (Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]) == 48)
                                sbQuestions.Append(BindStaticTableForBoardProcess2(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', SrNo + "." + count));
                        }
                        else if (screen.ID == 295)
                        {
                            string ColHeader = Convert.ToString(dtANS.Rows[i]["Q_DESCRIPTION_TX"]);
                            sbQuestions.Append(BindStaticTableForStackholders(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', SrNo + "." + count, ColHeader));
                        }
                        if (dtANS.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                        {
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label>" + dtANS.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");
                            //sbQuestions.Append("<input type='text' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_DESCRIPTION_TX' value='" + dtANS.Rows[i]["QUES_DESCRIPTION_TX"] + "' class='form-control'>");
                            sbQuestions.Append("<textarea type='textarea' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'>" + dtANS.Rows[i]["QUES_DESCRIPTION_TX"] + "</textarea>");
                            sbQuestions.Append("</div>");
                        }
                    }
                    if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 15 && isCleanPDF == false)
                    {
                        AdminMarks++;
                        sbQuestions.Append("<div class='col-md-12 mt-10'>");
                        sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                        sbQuestions.Append("</div>");

                        sbQuestions.Append("<div class='col-md-12 mt-10'>");
                        sbQuestions.Append("<label>Remarks: </label><textarea id='" + AdminMarks + "-EVAL_REMARKS_TX' type='text' rows='5' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[i]["EVAL_REMARKS_TX"] + "</textarea>");
                        sbQuestions.Append("</div>");

                        //if (i == dtANS.Rows.Count - 1)
                        //{
                        //    AdminMarks++;
                        //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                        //    sbQuestions.Append("<label> Evaluation Remarks: </label><textarea id='" + AdminMarks + "-Marks' type='text' rows='5' name='EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[0]["EVAL_REMARKS_TX"] + "</textarea>");
                        //    sbQuestions.Append("</div>");
                        //}
                    }
                    if (CATEGORY_TYPE_ID == Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility_and_Sustainability) &&
                        Convert.ToInt32(dtANS.Rows[i]["STATIC_CONTENT"]) == 1)
                        sbQuestions.Append("<div class='col-md-12 mt-30'><p class='fontBold text-primary'>PART B - SUSTAINABILITY</p></div>");
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace(QUESTION_REPLACE, sbQuestions.ToString());
                #endregion
            }

            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 15)
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

        private ActionClass DMLOPsDyamicPage(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            Dictionary<string, object> Mul_tblData;
            string screenType = frm["s"];
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> FileUploader = new Dictionary<string, object>();
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            string QuestionIds = string.Empty;
            int CGA_AWARDS_REG_ID = 0;

            #region For Insertion and updation
            foreach (var key in frm.AllKeys)
            {
                if (key.EndsWith("ANSWERE_TX") && Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15)
                {
                    Mul_tblData = new Dictionary<string, object>();

                    string[] split = key.Split('-');
                    int QUESTION_ID = Convert.ToInt32(split[0]);
                    CGA_AWARDS_REG_ID = Convert.ToInt32(frm["CGA_AWARDS_REG_ID"]);
                    string ANSWERE_TX = Convert.ToString(frm[QUESTION_ID + "-ANSWERE_TX"]);
                    string QUES_DESCRIPTION_TX = string.Empty;
                    if (frm[QUESTION_ID + "-QUES_DESCRIPTION_TX"] != null)
                        QUES_DESCRIPTION_TX = Convert.ToString(frm[QUESTION_ID + "-QUES_DESCRIPTION_TX"]);
                    HttpPostedFile hFileName = HttpContext.Current.Request.Files[QUESTION_ID + "-QUES_FILEUPLOADER"] as HttpPostedFile;

                    int ID = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ID"])))
                        ID = Convert.ToInt32(frm[QUESTION_ID + "-ID"]);
                    if (ID > 0)
                        screenType = "update";

                    Mul_tblData.Add("ID", ID);
                    Mul_tblData.Add("QUESTION_ID", QUESTION_ID);
                    Mul_tblData.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
                    Mul_tblData.Add("ANSWER_TX", ANSWERE_TX);
                    Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                    if (!string.IsNullOrEmpty(QUES_DESCRIPTION_TX))
                        Mul_tblData.Add("QUES_DESCRIPTION_TX", QUES_DESCRIPTION_TX);

                    #region For File Uploading
                    if (hFileName != null && !string.IsNullOrEmpty(hFileName.FileName))
                    {
                        QuestionIds += QUESTION_ID + ",";
                        string FolderName = "CGA\\Documents\\" + CGA_AWARDS_REG_ID;
                        string _path = Util.UtilService.getDocumentPath(FolderName);
                        string _FullPath = _path + "\\" + QUESTION_ID + "_" + Convert.ToString(hFileName.FileName);
                        Mul_tblData.Add("QUES_FILENAME_TX", _FullPath);
                    }
                    #endregion                    

                    if (!string.IsNullOrEmpty(ANSWERE_TX.Trim()))
                        lstData1.Add(Mul_tblData);
                }
                else if (key.EndsWith("ADMIN_MARKS_NM"))
                {
                    #region For Admin Marks
                    Mul_tblData = new Dictionary<string, object>();
                    string[] split = key.Split('-');
                    int QUESTION_ID = Convert.ToInt32(split[0]);
                    string EVAL_REMARKS_TX = Convert.ToString(frm["EVAL_REMARKS_TX"]);
                    int ID = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ID"])))
                        ID = Convert.ToInt32(frm[QUESTION_ID + "-ID"]);

                    if (ID > 0)
                    {
                        screenType = "update";

                        Mul_tblData.Add("ID", ID);
                        decimal ADMIN_MARKS_NM = 0;

                        if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ADMIN_MARKS_NM"])))
                        {
                            ADMIN_MARKS_NM = Convert.ToDecimal(frm[QUESTION_ID + "-ADMIN_MARKS_NM"]);
                            EVAL_REMARKS_TX = Convert.ToString(frm[QUESTION_ID + "-EVAL_REMARKS_TX"]);
                            //if (ADMIN_MARKS_NM > 0)
                            //{
                            Mul_tblData.Add("ADMIN_MARKS_NM", ADMIN_MARKS_NM);
                            Mul_tblData.Add("ADMIN_MARKS_DT", DateTime.Now);
                            Mul_tblData.Add("EVAL_REMARKS_TX", EVAL_REMARKS_TX);
                            //}
                        }
                        #endregion

                        //if (ADMIN_MARKS_NM > 0)
                        lstData1.Add(Mul_tblData);
                    }
                }
            }

            if (lstData1.Count > 0)
            {
                AppUrl = AppUrl + "/AddUpdate";
                lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));

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
                            string FolderName = "CGA\\Documents\\" + CGA_AWARDS_REG_ID;
                            string _path = Util.UtilService.getDocumentPath(FolderName);
                            HttpPostedFile hFileName = HttpContext.Current.Request.Files[QuesId + "-QUES_FILEUPLOADER"] as HttpPostedFile;
                            string _FullPath = _path + "\\" + QuesId + "_" + Path.GetFileName(hFileName.FileName);

                            if (!(Directory.Exists(_path)))
                                Directory.CreateDirectory(_path);

                            if (File.Exists(_FullPath))
                            {
                                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                                string NewFileName = QuesId + "_" + Timestamp.ToString() + Path.GetExtension(hFileName.FileName);
                                File.Move(_FullPath, _path + "\\" + NewFileName);
                                //File.Delete(_FullPath);
                            }
                            HttpContext.Current.Request.Files[QuesId + "-QUES_FILEUPLOADER"].SaveAs(_FullPath);
                        }
                    }
                }
                #endregion

                #region insert and update for Board Process static table
                if (frm.AllKeys.Contains("BOARD_PROCESSES_STATIC_TABLE1_T"))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    int QuestionId = Convert.ToInt32(frm["BOARD_PROCESSES_STATIC_TABLE1_T"]);

                    for (int i = 0; i < 6; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_DIRECTOR_NM"])) &&
                            !string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_INDP_DIRECTOR_NM"])))
                        {
                            int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                            Mul_tblData = new Dictionary<string, object>();
                            if (ID > 0 && i == 0)
                                screenType = "update";
                            Mul_tblData.Add("ID", ID);
                            Mul_tblData.Add("QUESTION_ID", QuestionId);
                            Mul_tblData.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
                            Mul_tblData.Add("BOARD_METTING_DT", Convert.ToDateTime(frm["" + QuestionId + "_" + i + "-BOARD_METTING_DT"]));
                            Mul_tblData.Add("NO_OF_DIRECTOR_NM", Convert.ToString(frm["" + i + "-NO_OF_DIRECTOR_NM"]));
                            Mul_tblData.Add("NO_OF_INDP_DIRECTOR_NM", Convert.ToString(frm["" + i + "-NO_OF_INDP_DIRECTOR_NM"]));
                            Mul_tblData.Add("NO_OF_DIRECTOR_ATT_NM", Convert.ToString(frm["" + i + "-NO_OF_DIRECTOR_ATT_NM"]));
                            Mul_tblData.Add("NO_OF_INDP_DIRECTOR_ATT_NM", Convert.ToString(frm["" + i + "-NO_OF_INDP_DIRECTOR_ATT_NM"]));
                            Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                            lstData1.Add(Mul_tblData);
                        }
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "BOARD_PROCESSES_STATIC_TABLE1_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    }
                }

                if (frm.AllKeys.Contains("BOARD_PROCESSES_STATIC_TABLE2_T"))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    int QuestionId = Convert.ToInt32(frm["BOARD_PROCESSES_STATIC_TABLE2_T"]);

                    for (int i = 0; i < 6; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_NM"])) &&
                            !string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_ID_NM"])))
                        {
                            int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                            Mul_tblData = new Dictionary<string, object>();
                            if (ID > 0 && i == 0)
                                screenType = "update";
                            Mul_tblData.Add("ID", ID);
                            Mul_tblData.Add("QUESTION_ID", QuestionId);
                            Mul_tblData.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
                            Mul_tblData.Add("BOARD_METTING_DT", Convert.ToDateTime(frm["" + QuestionId + "_" + i + "-BOARD_METTING_DT"]));
                            Mul_tblData.Add("NO_OF_COMM_MEMBER_NM", Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_NM"]));
                            Mul_tblData.Add("NO_OF_ID_NM", Convert.ToString(frm["" + i + "-NO_OF_ID_NM"]));
                            Mul_tblData.Add("NO_OF_COMM_MEMBER_ATT_NM", Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_ATT_NM"]));
                            Mul_tblData.Add("NO_OF_ID_ATT_NM", Convert.ToString(frm["" + i + "-NO_OF_ID_ATT_NM"]));
                            Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                            lstData1.Add(Mul_tblData);
                        }
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "BOARD_PROCESSES_STATIC_TABLE2_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    }
                }
                #endregion

                #region insert and update for STAKEHOLDER VALUE ENHANCEMENT static table
                if (frm.AllKeys.Contains("STACKHOLDERS_VALUE_STATIC_TABLE_T"))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    string[] StaticQuestionIds = Convert.ToString(frm["STACKHOLDERS_VALUE_STATIC_TABLE_T"]).Split(',');

                    for (int j = 0; j < StaticQuestionIds.Length; j++)
                    {
                        int QuestionId = Convert.ToInt32(StaticQuestionIds[j]);
                        for (int i = 0; i < 20; i++)
                        {
                            string FINANCIAL_YEAR_TX = Convert.ToString(frm["" + QuestionId + "-" + i + "-FINANCIAL_YEAR_TX"]);
                            string VALUE_TX = Convert.ToString(frm["" + QuestionId + "-" + i + "-VALUE_TX"]);

                            if (!string.IsNullOrEmpty(FINANCIAL_YEAR_TX) && !string.IsNullOrEmpty(VALUE_TX))
                            {
                                int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                                Mul_tblData = new Dictionary<string, object>();
                                if (ID > 0 && i == 0)
                                    screenType = "update";
                                Mul_tblData.Add("ID", ID);
                                Mul_tblData.Add("QUESTION_ID", QuestionId);
                                Mul_tblData.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
                                Mul_tblData.Add("FINANCIAL_YEAR_TX", FINANCIAL_YEAR_TX);
                                Mul_tblData.Add("VALUE_TX", VALUE_TX);
                                Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                                lstData1.Add(Mul_tblData);
                            }
                        }
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "STACKHOLDERS_VALUE_STATIC_TABLE_T", 0, 0, lstData1, conditions));
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

        private StringBuilder BindStaticTableForBoardProcess1(int QuesID, string QuesName, char Type, string SrNo)
        {
            int rowCount = 6;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 118);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId(""));
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtStaticTable = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                StaticTableCount = dtStaticTable.Rows.Count;
            }
            #endregion

            #region Bind Static Table
            StringBuilder sbStaticTable = new StringBuilder();

            sbStaticTable.Append("<div class='col-xs-12'>");
            sbStaticTable.Append("<div class='fontBold pt-20'>" + SrNo + " " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='BOARD_PROCESSES_STATIC_TABLE1_T'/>");
            sbStaticTable.Append("<input type='hidden' value='BOARD_PROCESSES_STATIC_TABLE1_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>Date of Board Meeting</th>");
            sbStaticTable.Append("<th>Total No. of Directors on the Board (including Independent Directors on the date of meeting)</th>");
            sbStaticTable.Append("<th>Total No. of Independent Directors on the Board on the date of meeting</th>");
            sbStaticTable.Append("<th>No. of Directors who attended the meeting (including Independent Directors)</th>");
            sbStaticTable.Append("<th>No. of Independent Directors who attended the meeting</th>");
            sbStaticTable.Append("</tr></thead><tbody>");

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + QuesID + "_" + i + "-BOARD_METTING_DT' value='" + dtStaticTable.Rows[i]["BOARD_METTING_DT"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_DIRECTOR_NM' value='" + dtStaticTable.Rows[i]["NO_OF_DIRECTOR_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_INDP_DIRECTOR_NM' value='" + dtStaticTable.Rows[i]["NO_OF_INDP_DIRECTOR_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_DIRECTOR_ATT_NM' value='" + dtStaticTable.Rows[i]["NO_OF_DIRECTOR_ATT_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_INDP_DIRECTOR_ATT_NM' value='" + dtStaticTable.Rows[i]["NO_OF_INDP_DIRECTOR_ATT_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + QuesID + "_" + i + "-BOARD_METTING_DT' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_DIRECTOR_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_INDP_DIRECTOR_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_DIRECTOR_ATT_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_INDP_DIRECTOR_ATT_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        private StringBuilder BindStaticTableForBoardProcess2(int QuesID, string QuesName, char Type, string SrNo)
        {
            int rowCount = 6;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 119);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId(""));
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtStaticTable = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                StaticTableCount = dtStaticTable.Rows.Count;
            }
            #endregion

            #region Bind Static Table
            StringBuilder sbStaticTable = new StringBuilder();

            sbStaticTable.Append("<div class='col-xs-12'>");
            sbStaticTable.Append("<div class='fontBold pt-20'>" + SrNo + " " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='BOARD_PROCESSES_STATIC_TABLE2_T'/>");
            sbStaticTable.Append("<input type='hidden' value='BOARD_PROCESSES_STATIC_TABLE2_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>Date of Meeting</th>");
            sbStaticTable.Append("<th>Total no. of Committee members on the date of meeting</th>");
            sbStaticTable.Append("<th>Total no. of ID on the date of meeting</th>");
            sbStaticTable.Append("<th>No. of members who attended the meeting</th>");
            sbStaticTable.Append("<th>No of IDs who attended the meeting</th>");
            sbStaticTable.Append("</tr></thead><tbody>");

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + QuesID + "_" + i + "-BOARD_METTING_DT' value='" + dtStaticTable.Rows[i]["BOARD_METTING_DT"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_COMM_MEMBER_NM' value='" + dtStaticTable.Rows[i]["NO_OF_COMM_MEMBER_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_ID_NM' value='" + dtStaticTable.Rows[i]["NO_OF_ID_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_COMM_MEMBER_ATT_NM' value='" + dtStaticTable.Rows[i]["NO_OF_COMM_MEMBER_ATT_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_ID_ATT_NM' value='" + dtStaticTable.Rows[i]["NO_OF_ID_ATT_NM"] + "' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + QuesID + "_" + i + "-BOARD_METTING_DT' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_COMM_MEMBER_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_ID_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_COMM_MEMBER_ATT_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' name='" + i + "-NO_OF_ID_ATT_NM' value='' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        public ActionClass beforeTransparencyDisclosure(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure_Compliances), 117, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure_Compliances));

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterTransparencyDisclosure(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeStackholdersValueEnhancement(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement), 122, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement));

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterStackholdersValueEnhancement(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        private StringBuilder BindStaticTableForStackholders(int QuesID, string QuesName, char Type, string SrNo, string ColHeader)
        {
            int rowCount = 5;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 123);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CGA_AWARDS_REG_ID", GetCGAAwardsRegId(""));
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtStaticTable = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                StaticTableCount = dtStaticTable.Rows.Count;
            }
            #endregion

            #region Bind Static Table
            StringBuilder sbStaticTable = new StringBuilder();

            sbStaticTable.Append("<div class='col-xs-12'>");
            if (!string.IsNullOrEmpty(QuesName))
                sbStaticTable.Append("<div class='fontBold pt-20'>" + SrNo + " " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='STACKHOLDERS_VALUE_STATIC_TABLE_T'/>");
            sbStaticTable.Append("<input type='hidden' value='STACKHOLDERS_VALUE_STATIC_TABLE_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>Year</th>");
            sbStaticTable.Append("<th>" + ColHeader + "</th>");
            sbStaticTable.Append("</tr></thead><tbody>");

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["FINANCIAL_YEAR_TX"] + "' name='" + QuesID + "-" + i + "-FINANCIAL_YEAR_TX'/>" + dtStaticTable.Rows[i]["FINANCIAL_YEAR_TX"] + "</td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isDNumberKey(event)' name='" + QuesID + "-" + i + "-VALUE_TX' value='" + dtStaticTable.Rows[i]["VALUE_TX"] + "' class='form-control'></td>");

                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                int CurrentYear = 2020;
                int PrevYear = 2019;
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + PrevYear + "-" + CurrentYear + "' name='" + QuesID + "-" + i + "-FINANCIAL_YEAR_TX'/>" + (PrevYear + "-" + CurrentYear) + "</td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isDNumberKey(event)' name='" + QuesID + "-" + i + "-VALUE_TX' value='' class='form-control'></td>");

                    sbStaticTable.Append("</tr>");
                    PrevYear--;
                    CurrentYear--;
                }
            }
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        public ActionClass beforeCorporateSocialResponsibilitySustainability(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility_and_Sustainability), 126, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility_and_Sustainability));

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", HttpContext.Current.Session["CGA_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterCorporateSocialResponsibilitySustainability(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeQuestionMarksApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CGA_AWARDS_REG_ID = GetCGAAwardsRegId("");
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 129);
            conditions.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");
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
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", dt.Rows[0]["ID"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID.ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TOTAL_MARKS_NM", dt.Rows[0]["MARKS"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISQUALIFIED_YN", dt.Rows[0]["DISQUALIFIED_YN"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REMARKS_TX", dt.Rows[0]["REMARKS_TX"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EVAL_COUNT_NM", dt.Rows[0]["EVAL_COUNT_NM"].ToString());
            }

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterQuestionMarksApproval(int WEB_APP_ID, FormCollection frm)
        {
            frm["ADMIN_MARKS_DT"] = DateTime.Now.ToString();
            frm["s"] = "update";

            return Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        }

        #region Statutry auditor screen
        public ActionClass beforeStatutryAuditor(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);

            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15)
            {
                #region Check form submitted or not
                DataTable dtRDetails = new DataTable();

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 268);
                conditions.Add("CGA_AWARDS_REG_ID", frm["CGA_AWARDS_REG_ID"]);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtRDetails = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dtRDetails != null && dtRDetails.Rows != null && dtRDetails.Rows.Count > 0)
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "block").Replace("@Display", "none");
                else
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "none").Replace("@Display", "block");
                #endregion
            }
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "none").Replace("@Display", "block");

            return act;
        }

        public ActionClass afterStatutryAuditor(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            int Id = 0;
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(actionClass.DecryptData))
                {
                    JObject res = JObject.Parse(actionClass.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            Id = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                        }
                    }
                }
            }

            Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string[] CategoryID = frm["CategoryID"].Split(',');
            for (int i = 0; i < CategoryID.Length; i++)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["STATUTORY_AUDITOR_ID"] = Id;
                d["BASIC_CATEGORY_ID"] = CategoryID[i];
                d["CATEGORY_RATING_ID"] = Convert.ToInt32(frm["CATEGORY_RATING_" + i + "_ID"]);
                d["COMMENTS_TX"] = frm["COMMENTS_" + i + "_TX"];
                d["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                list.Add(d);
                UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "STATUTORY_AUDITOR_BASIC_CATEGORY_T", list);
            }
            return actionClass;
        }
        #endregion

        #region Secretarial auditor screen  
        public ActionClass beforeSecretarialAuditor(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);

            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 15)
            {
                #region Check form submitted or not
                DataTable dtRDetails = new DataTable();

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 269);
                conditions.Add("CGA_AWARDS_REG_ID", frm["CGA_AWARDS_REG_ID"]);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");

                if (jdata != null && jdata.First.First.First != null)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtRDetails = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dtRDetails != null && dtRDetails.Rows != null && dtRDetails.Rows.Count > 0)
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "block").Replace("@Display", "none");
                else
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "none").Replace("@Display", "block");
                #endregion
            }
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DisplayMsg", "none").Replace("@Display", "block");

            return act;
        }

        public ActionClass afterSecretarialAuditor(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            int Id = 0;
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(actionClass.DecryptData))
                {
                    JObject res = JObject.Parse(actionClass.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            Id = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                        }
                    }
                }
            }

            Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string[] CategoryID = frm["CategoryID"].Split(',');
            for (int i = 0; i < CategoryID.Length; i++)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["SECRETARIAL_AUDITOR_ID"] = Id;
                d["BASIC_CATEGORY_ID"] = CategoryID[i];
                d["CATEGORY_RATING_ID"] = Convert.ToInt32(frm["CATEGORY_RATING_" + i + "_ID"]);
                d["COMMENTS_TX"] = frm["COMMENTS_" + i + "_TX"];
                d["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                list.Add(d);
                UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "SECRETARIAL_AUDITOR_BASIC_CATEGORY_T", list);
            }
            return actionClass;
        }
        #endregion

        #region Independent director screen
        public ActionClass beforeIndependentDirector(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterIndependentDirector(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            int Id = 0;
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(actionClass.DecryptData))
                {
                    JObject res = JObject.Parse(actionClass.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            Id = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                        }
                    }
                }
            }

            Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string[] CategoryID = frm["CategoryID"].Split(',');
            for (int i = 0; i < CategoryID.Length; i++)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["INDEPENDENT_DIRECTOR_ID"] = Id;
                d["BASIC_CATEGORY_ID"] = CategoryID[i];
                d["CATEGORY_RATING_ID"] = Convert.ToInt32(frm["CATEGORY_RATING_" + i + "_ID"]);
                d["COMMENTS_TX"] = frm["COMMENTS_" + i + "_TX"];
                d["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                list.Add(d);
                UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "INDEPENDENT_DIRECTOR_BASIC_CATEGORY_T", list);
            }
            return actionClass;
        }
        #endregion

        public ActionClass beforeBoardProcesses2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes), 130, 1);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            Get_Submitted_Data(frm, screen);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterBoardProcesses2(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeTransparencyDisclosure2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure), 131, 2);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterTransparencyDisclosure2(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeRiskManagementFramework(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Risk_Management_Framework), 132, 3);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterRiskManagementFramework(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeStackholdersValueEnhancement2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement), 133, 4);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterStackholdersValueEnhancement2(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeHumanResources(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Human_Resources), 134, 5);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterHumanResources(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeSustainability(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Sustainability), 135, 6);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterSustainability(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeCorporateSocialResponsibility(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility), 136, 7);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return act;
        }

        public ActionClass afterCorporateSocialResponsibility(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeCompanyInformation2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CGA_AWARDS_REG_ID = GetCGAAwardsRegId(Convert.ToString(frm["ui"]));
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(CGA_AWARDS_REG_ID));
            //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(GetCGAAwardsRegId("")));
            GetScreenData(frm, screen, out int id);

            Get_Submitted_Data(frm, screen);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", HttpContext.Current.Session["CGA_APPROVE2_NM"].ToString());
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCompanyInformation2(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))
                frm["s"] = "update";

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            if (act.StatMessage.ToLower() == "success")
            {
                #region Insert and update for Directors
                List<Dictionary<string, object>> lstDirectors = new List<Dictionary<string, object>>();
                Dictionary<string, object> dDirectors;
                int CountDirector = Convert.ToInt32(frm["LAST_DIR_ROW"]);
                int CompanyInfoId = Convert.ToInt32(frm["ID"]);

                if (CountDirector > 0 && CompanyInfoId > 0)
                {
                    for (int i = 1; i <= CountDirector; i++)
                    {
                        if (!string.IsNullOrEmpty(frm["" + i + "_BOARD_DIRECTOR_NAME_TX"]))
                        {
                            dDirectors = new Dictionary<string, object>();
                            dDirectors["ID"] = frm["" + i + "_DIR_ID"];
                            dDirectors["COMPANY_INFO_ID"] = CompanyInfoId;
                            dDirectors["BOARD_DIRECTOR_NAME_TX"] = frm["" + i + "_BOARD_DIRECTOR_NAME_TX"];
                            dDirectors["CATEGORY_TX"] = frm["" + i + "_CATEGORY_TX"];
                            dDirectors["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                            lstDirectors.Add(dDirectors);
                        }
                    }

                    if (lstDirectors.Count > 0)
                        act = UtilService.insertOrUpdate("CGA", "BOARD_DIRECTORS_T", lstDirectors);
                }
                #endregion

                #region Insert and update for Board Committee
                List<Dictionary<string, object>> lstCommittee = new List<Dictionary<string, object>>();
                Dictionary<string, object> dCommittee;
                int CountCommittee = Convert.ToInt32(frm["LAST_COM_ROW"]);

                if (CountCommittee > 0 && CompanyInfoId > 0)
                {
                    for (int i = 1; i <= CountCommittee; i++)
                    {
                        if (!string.IsNullOrEmpty(frm["" + i + "_BOARD_COMMITTEE_NAME_TX"]))
                        {
                            dCommittee = new Dictionary<string, object>();
                            dCommittee["ID"] = frm["" + i + "_COM_ID"];
                            dCommittee["COMPANY_INFO_ID"] = CompanyInfoId;
                            dCommittee["BOARD_COMMITTEE_NAME_TX"] = frm["" + i + "_BOARD_COMMITTEE_NAME_TX"];
                            dCommittee["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                            lstCommittee.Add(dCommittee);
                        }
                    }

                    if (lstCommittee.Count > 0)
                        act = UtilService.insertOrUpdate("CGA", "BOARD_COMMITTEES_T", lstCommittee);
                }
                #endregion
            }

            return act;
        }

        public ActionClass beforeCGAPrint(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(frm["ui"]));
            bool isAdmin = false;
            int UniqueID = 0;
            bool isCleanPDF = true;
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 15)
            {
                UniqueID = Convert.ToInt32(frm["ui"]);
                isAdmin = true;
            }
            if (frm["aa"] == "1")
                isCleanPDF = false;

            #region General Information
            screen.Table_Name_Tx = "GENERAL_INFORMATION_T";
            GetScreenData(frm, screen, out int GeneralInfId, isAdmin);
            GetGeneralInfoAuditor(frm, screen, GeneralInfId);
            GetGeneralInfoChangeAuditor(frm, screen, GeneralInfId);
            GetGeneralInfoDirector(frm, screen, GeneralInfId);
            GetGeneralInfoChangeDirector(frm, screen, GeneralInfId);

            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Screen_Id == Convert.ToInt32(screen.ID)).FirstOrDefault();
            ActionClass act = UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, s.Id.ToString(), screen.ID.ToString());
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE_NM", "");
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_APPROVE2_NM", "");
            #endregion

            #region Company Information
            screen.Table_Name_Tx = "COMPANY_INFORMATION_T";
            GetScreenData(frm, screen, out int GeneralInfId1, isAdmin);
            #endregion

            #region Financial Information
            screen.Table_Name_Tx = "FINANCIAL_INFORMATION_T";
            BindFinancialInformation(screen, true, UniqueID);
            #endregion

            #region Quesstionnaire screens
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Structure), 110, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Structure), "@BOARD_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes), 112, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes), "@BOARD_PRO_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure_Compliances), 117, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure_Compliances), "@TRANS_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement), 122, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement), "@STACK_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility_and_Sustainability), 126, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility_and_Sustainability), "@CORPORATE_Questions", UniqueID, isCleanPDF);
            #endregion

            if (frm["aa"] == "1")
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:block;");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:none;");

            return act;
        }

        public ActionClass beforeCGALevel2Print(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", Convert.ToString(frm["ui"]));
            bool isAdmin = false;
            int UniqueID = 0;
            bool isCleanPDF = true;
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 15)
            {
                UniqueID = Convert.ToInt32(frm["ui"]);
                isAdmin = true;
            }
            if (frm["aa"] == "1")
                isCleanPDF = false;

            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Screen_Id == Convert.ToInt32(screen.ID)).FirstOrDefault();
            ActionClass act = UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, s.Id.ToString(), screen.ID.ToString());

            #region Quesstionnaire screens
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Board_Processes), 130, 1, "@BOARD_PRO_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Transparency_and_Disclosure), 131, 2, "@Trans_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Risk_Management_Framework), 132, 3, "@Risk_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stackholders_Value_Enhancement), 133, 4, "@Stackholders_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Human_Resources), 134, 5, "@Human_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Sustainability), 135, 6, "@Sustainability_Questions", UniqueID, isCleanPDF);
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Corporate_Social_Responsibility), 136, 7, "@Corporate_Questions", UniqueID, isCleanPDF);
            #endregion

            if (frm["aa"] == "1")
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:block;");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:none;");

            return act;
        }

        public ActionClass beforeQuestionMarksApprovalLevel2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CGA_AWARDS_REG_ID = GetCGAAwardsRegId("");
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 264);
            conditions.Add("CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CGA");
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
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ID", dt.Rows[0]["ID"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CGA_AWARDS_REG_ID", CGA_AWARDS_REG_ID.ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@MARKS_LEVEL1", dt.Rows[0]["MARKS_LEVEL1"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@MARKS_LEVEL2", dt.Rows[0]["MARKS_LEVEL2"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REMARKS_TX", dt.Rows[0]["REMARKS_TX"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REMARKS2_TX", dt.Rows[0]["REMARKS2_TX"].ToString());
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EVAL_COUNT2_NM", dt.Rows[0]["EVAL_COUNT2_NM"].ToString());
            }

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterQuestionMarksApprovalLevel2(int WEB_APP_ID, FormCollection frm)
        {
            frm["ADMIN_MARKS2_DT"] = DateTime.Now.ToString();
            frm["s"] = "update";

            return Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        }

        #region Send Mail 2th Level       
        public ActionClass searchSendMail2thLevel(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            ActionClass actionClass = null;
            string strUserID = Convert.ToString(frm["UserID"]);
            if (string.IsNullOrEmpty(strUserID))
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#Message", string.Empty);
                actionClass = Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
            }
            else
            {
                string[] UserID = frm["UserID"].Split(',');
                DataTable dtRDetails = new DataTable();
                Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 148); //check records available or not                                             
                conditions.Add("userID", strUserID);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(s));
                if (jdata.HasValues)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtRDetails = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dtRDetails != null && dtRDetails.Rows != null && dtRDetails.Rows.Count > 0)
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    for (int i = 0; i < UserID.Length; i++)
                    {
                        list.Clear();
                        d.Clear();
                        var row = dtRDetails.AsEnumerable().Where(r => (r["CGA_AWARDS_REG_ID"].ToString()) == Convert.ToString(UserID[i])).SingleOrDefault();
                        if (row != null)
                        {
                            int count = Convert.ToInt32(row["EMAIL_COUNT_NM"]);//
                            d["ID"] = Convert.ToInt32(row["ID"]);
                            d["EMAIL_COUNT_NM"] = count + 1;
                        }
                        else
                        {
                            d["CGA_AWARDS_REG_ID"] = UserID[i];
                            d["EMAIL_COUNT_NM"] = 1;
                        }
                        d["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                        list.Add(d);
                        actionClass = UtilService.insertOrUpdate("CGA", "CGA_SENDMAIL_T", list);
                    }

                }
                else
                {
                    for (int i = 0; i < UserID.Length; i++)
                    {
                        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        d["CGA_AWARDS_REG_ID"] = UserID[i];
                        d["EMAIL_COUNT_NM"] = 1;
                        d["FIN_YEAR_ID"] = frm["FIN_YEAR_ID"];
                        list.Add(d);
                        actionClass = UtilService.insertOrUpdate("CGA", "CGA_SENDMAIL_T", list);
                        list.Clear();
                    }
                }

                frm["s"] = "search";
                actionClass = Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));

            }
            return actionClass;

        }
        #endregion

        public ActionClass beforeCGASupportingAttachements(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCGASupportingAttachements(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            string CGA_AWARDS_REG_ID = HttpContext.Current.Session["CGA_AWARDS_REG_ID"].ToString();
            frm["CGA_AWARDS_REG_ID"] = CGA_AWARDS_REG_ID;

            HttpPostedFile hFileName = HttpContext.Current.Request.Files[0] as HttpPostedFile;
            if (!string.IsNullOrEmpty(hFileName.FileName))
            {
                string FolderName = "CGA\\Documents\\Support_Attachement\\";
                string _path = Util.UtilService.getDocumentPath(FolderName);
                string _FullPath = _path + "\\" + Convert.ToString(hFileName.FileName);
                frm["DOC_PATH_TX"] = _FullPath;
            }
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            return act;
        }

        public static DataTable RegisteredCompanies()
        {
            DataTable dtRegistered = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 263);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 500, "CGA");

            if (jdata != null || jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtRegistered = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            return dtRegistered;
        }

        public static DataTable GetAuditorsDirectors(int id)
        {
            DataTable dtAuditors = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            if (id == 2)
                conditions.Add("QID", 276);
            else if (id == 3)
                conditions.Add("QID", 277);
            else if (id == 4)
                conditions.Add("QID", 278);

            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 500, "CGA");

            if (jdata != null || jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtAuditors = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            return dtAuditors;
        }

        public static void GetActiveYear()
        {
            string TableName = "FINANCIAL_YEAR_T";
            JObject jdata = null;
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("SHOW_YEAR_YN", 1);

            jdata = DBTable.GetData("fetch", conditions, TableName, 0, 1000, "CGA");
            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                {
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
            {
                HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"] = Convert.ToInt32(dtData.Rows[0]["ID"]);
                HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR"] = (dtData.Rows[0]["FIN_YEAR_TX"]).ToString().Split('-')[0];
            }
        }

        public enum QUESTION_CATEGORY_TYPE
        {
            Board_Structure = 1,
            Board_Processes = 2,
            Transparency_and_Disclosure_Compliances = 3,
            Stackholders_Value_Enhancement = 4,
            Corporate_Social_Responsibility_and_Sustainability = 5,
            Transparency_and_Disclosure = 6,
            Risk_Management_Framework = 7,
            Human_Resources = 8,
            Sustainability = 9,
            Corporate_Social_Responsibility = 10
        }
    }
}