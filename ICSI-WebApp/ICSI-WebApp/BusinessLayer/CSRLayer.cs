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
using System.Globalization;

namespace ICSI_WebApp.BusinessLayer
{
    public class CSRLayer
    {
        public ActionClass beforeQuestionMaster(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            return act;
        }

        public ActionClass afterQuestionMaster(int WEB_APP_ID, FormCollection frm)
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
            string qopts = frm["Q_OPTIONS"];
            if (Id > 0 && qopts != null && !qopts.Trim().Equals(""))
            {
                Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
                int num = Convert.ToInt32(qopts);
                for (int i = 0; i < num; i++)
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["QUESTION_ID"] = Id;
                    d["Q_OPTION_NAME_TX"] = frm["SUB_QST_TX_" + i];
                    d["MARKS_NM"] = Convert.ToInt32(frm["SUB_QST_MARKS_NM_" + i]);
                    d["ORDER_NM"] = i + 1;
                    list.Add(d);
                    UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "QUESTION_OPTIONS_T", list);
                }
                string subOpts = frm["Q_SUBQST_OPTIONS"];
                if (subOpts != null && !subOpts.Trim().Equals("") && subOpts.Trim() != "0")
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["REF_ID"] = Id;
                    d["Q_CATEGORY_TYPE_ID"] = frm["Q_CATEGORY_TYPE_ID"];
                    d["QUESTION_TYPE"] = frm["QUESTION_TYPE"];
                    d["QST_SERIAL_NM"] = frm["QST_SERIAL_NM"];
                    d["SUB_QST_NO_YN"] = frm["SUB_QST_NO_YN"];
                    d["QST_SUB_SN_NM"] = frm["QST_SUB_SN_NM"];
                    d["PRIM_SN_DISPLAY_NO_YN"] = frm["PRIM_SN_DISPLAY_NO_YN"];
                    d["SQ_PARANTHESIS"] = frm["SQ_PARANTHESIS"];
                    d["Q_DESCRIPTION_TX"] = frm["Q_DESCRIPTION_TX"];
                    d["QST_DSC_DISPLAY_NO_YN"] = frm["QST_DSC_DISPLAY_NO_YN"];
                    d["QUESTION_NAME_TX"] = frm["SUBQUESTION_NAME_TX"];
                    d["MARKS_NM"] = frm["MARKS_NM"];
                    d["Q_OPTIONS"] = frm["Q_SUBQST_OPTIONS"];
                    if (frm["SUBQST_ENABLE_TEXTAREA_YN"].ToString().ToLower() == "on")
                    {
                        d["Q_ENABLE_TEXTAREA_YN"] = 1;
                        d["Q_TEXTAREA_NAME_TX"] = frm["SUBQST_TEXTAREA_NAME_TX"];
                    }
                    list.Add(d);
                    UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "MST_QUESTIONS_T", list);
                    Id = Id + 1;
                    int numOps = Convert.ToInt32(subOpts);
                    for (int i = 0; i < numOps; i++)
                    {
                        List<Dictionary<string, object>> sublist = new List<Dictionary<string, object>>();
                        Dictionary<string, object> sub_d = new Dictionary<string, object>();
                        sub_d["QUESTION_ID"] = Id;
                        sub_d["Q_OPTION_NAME_TX"] = frm["SUBQST_OPTION_NAME_TX_" + i];
                        sub_d["MARKS_NM"] = Convert.ToInt32(frm["SUBQST_OPTION_MARKS_NM_" + i]);
                        sub_d["ORDER_NM"] = i + 1;
                        sublist.Add(sub_d);
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(s), "QUESTION_OPTIONS_T", sublist);
                    }
                }
            }
            return actionClass;
        }

        public ActionClass beforeGeneralInstructions(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            #region Get CSR Awards Reg Id
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("USER_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));

            JObject jdata = DBTable.GetData("fetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
            DataTable dt = null;
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name.Equals(screen.Table_Name_Tx)) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                HttpContext.Current.Session["CSR_AWARDS_REG_ID"] = dt.Rows[0]["USER_ID"];
            #endregion

            #region Proceed button hide and show
            if (HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"] == null)
                GetActiveYear();

            conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("CSR_AWARDS_REG_ID", Convert.ToInt32(HttpContext.Current.Session["USER_ID"]));
            conditions.Add("FIN_YEAR_ID", Convert.ToInt32(HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"]));

            JObject jdata1 = DBTable.GetData("fetch", conditions, "CSR_AWARDS_APPROVAL_T", 0, 10, applicationSchema);
            dt = null;
            foreach (JProperty property in jdata1.Properties())
            {
                if (property.Name.Equals("CSR_AWARDS_APPROVAL_T")) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "style='display:none;'");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@btn_display", "");
            #endregion

            return null;
        }

        public ActionClass beforeGeneralInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CSR_AWARDS_REG_ID = GetCSRAwardsRegId(Convert.ToString(frm["ui"]));
            if (HttpContext.Current.Session["ACTIVE_ARCHIVE_YEAR_ID"] == null)
                GetActiveYear();

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(CSR_AWARDS_REG_ID));
            GetScreenData(frm, screen);

            Get_Submitted_Data(frm, screen);
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterGeneralInformation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (!string.IsNullOrEmpty(frm["CATEGORY_NM"]))
            {
                int CATEGORY_NM = Convert.ToInt32(frm["CATEGORY_NM"]);
                frm["AVG_NET_PROFIT_TX"] = Convert.ToString(frm["AVG_NET_PROFIT_TX_" + CATEGORY_NM + ""]);
            }
            if (!string.IsNullOrEmpty(Convert.ToString(frm["ID"])))
                frm["s"] = "update";

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }

        private void GetScreenData(FormCollection frm, Screen_T screen)
        {
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("CSR_AWARDS_REG_ID", GetCSRAwardsRegId(""));
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
                    if (screen.Screen_Content_Tx != null)
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                }
            }
        }

        public ActionClass beforeFinancialInformation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass act = null;

            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", GetCSRAwardsRegId("").ToString());
            BindFinancialInformation(screen);

            act = Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
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
                    int CSR_AWARDS_REG_ID = Convert.ToInt32(frm["CSR_AWARDS_REG_ID"]);
                    string CurrentFinYear = Convert.ToString(split[1]);
                    string PrevFinYear = frm[FIN_HEAD_ID + "-FIN_YEAR_2_TX"].Split('_')[1];
                    string PrevToPrevFinYear = frm[FIN_HEAD_ID + "-FIN_YEAR_3_TX"].Split('_')[1];
                    int ID = Convert.ToInt32(frm[FIN_HEAD_ID + "-ID"]);
                    decimal FinYearIstVal = Convert.ToDecimal(frm[FIN_HEAD_ID + "-FIN_YEAR_VALUE_1_NM"]);
                    decimal FinYear2ndVal = Convert.ToDecimal(frm[FIN_HEAD_ID + "-FIN_YEAR_VALUE_2_NM"]);
                    decimal FinYear3rdVal = Convert.ToDecimal(frm[FIN_HEAD_ID + "-FIN_YEAR_VALUE_3_NM"]);
                    if (ID > 0)
                        screenType = "update";

                    Mul_tblData.Add("ID", ID);
                    Mul_tblData.Add("FIN_HEAD_ID", FIN_HEAD_ID);
                    Mul_tblData.Add("CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID);
                    Mul_tblData.Add("FIN_YEAR_1_TX", CurrentFinYear);
                    Mul_tblData.Add("FIN_YEAR_VALUE_1_NM", FinYearIstVal);
                    Mul_tblData.Add("FIN_YEAR_2_TX", PrevFinYear);
                    Mul_tblData.Add("FIN_YEAR_VALUE_2_NM", FinYear2ndVal);
                    Mul_tblData.Add("FIN_YEAR_3_TX", PrevToPrevFinYear);
                    Mul_tblData.Add("FIN_YEAR_VALUE_3_NM", FinYear3rdVal);

                    lstData1.Add(Mul_tblData);
                }
            }

            AppUrl = AppUrl + "/AddUpdate";
            lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
            act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
            #endregion

            return act;
        }

        private void BindFinancialInformation(Screen_T screen)
        {
            #region Bind Financial Head
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 73); //check records available or not     
            conditions.Add("CSR_AWARDS_REG_ID", GetCSRAwardsRegId("").ToString());
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
                //string CurrentFinYear = GetFinancialYear(DateTime.Now.Year, DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(1).Year);
                //string PrevFinYear = GetFinancialYear(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-2).Year, DateTime.Now.Year);
                //string PrevToPrevFinYear = GetFinancialYear(DateTime.Now.AddYears(-2).Year, DateTime.Now.AddYears(-3).Year, DateTime.Now.AddYears(-1).Year);

                string PrevToPrevFinYear = GetFinancialYear(DateTime.Now.AddYears(-3).Year, DateTime.Now.AddYears(-4).Year, DateTime.Now.AddYears(-2).Year);
                string PrevFinYear = GetFinancialYear(DateTime.Now.AddYears(-2).Year, DateTime.Now.AddYears(-2).Year, DateTime.Now.AddYears(-1).Year);
                string CurrentFinYear = GetFinancialYear(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-1).Year, DateTime.Now.Year);

                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_YEAR", "<th>" + PrevToPrevFinYear + "</th><th>" + PrevFinYear + "</th><th>" + CurrentFinYear + "</th>");
                #endregion

                StringBuilder sbFinancialHead = new StringBuilder();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sbFinancialHead.Append("<tr>");
                    sbFinancialHead.Append("<td style='text-align:left; font-weight:bold;'>" + dt.Rows[i]["FINANCIAL_HEAD_TX"] + " *</td>");
                    //sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_1_TX' value='" + dt.Rows[i]["ID"] + "_" + CurrentFinYear + "' /><input type='number' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_1_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='0.00'></td>");
                    //sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_2_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevFinYear + "' /><input type='number' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_2_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='0.00'></td>");
                    //sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_3_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevToPrevFinYear + "' /><input type='number' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_3_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='0.00'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_1_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevToPrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_1_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_2_TX' value='" + dt.Rows[i]["ID"] + "_" + PrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_2_NM' class='form-control' value='00.00'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_3_TX' value='" + dt.Rows[i]["ID"] + "_" + CurrentFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dt.Rows[i]["ID"] + "-FIN_YEAR_VALUE_3_NM' class='form-control' value='00.00'></td>");
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
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_YEAR", "<th>" + PrevToPrevFinYear + "</th><th>" + PrevFinYear + "</th><th>" + CurrentFinYear + "</th>");
                #endregion

                StringBuilder sbFinancialHead = new StringBuilder();
                for (int i = 0; i < dtavl.Rows.Count; i++)
                {
                    sbFinancialHead.Append("<tr>");
                    sbFinancialHead.Append("<td style='text-align:left; font-weight:bold;'>" + dtavl.Rows[i]["FINANCIAL_HEAD_TX"] + " *</td>");
                    //sbFinancialHead.Append("<td><input type='hidden' value='" + dtavl.Rows[i]["ID"] + "' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-ID'/><input type='hidden' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_1_TX' value='" + dtavl.Rows[i]["ID"] + "_" + CurrentFinYear + "' /><input type='number' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_VALUE_1_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_1_NM"])) + "'></td>");
                    //sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_2_TX' value='" + dtavl.Rows[i]["ID"] + "_" + PrevFinYear + "' /><input type='number' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_VALUE_2_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_2_NM"])) + "'></td>");
                    //sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_3_TX' value='" + dtavl.Rows[i]["ID"] + "_" + PrevToPrevFinYear + "' /><input type='number' name='" + dtavl.Rows[i]["ID"] + "-FIN_YEAR_VALUE_3_NM' class='form-control' onchange='setTwoNumberDecimal' min='0' max='200' step='0.25' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_3_NM"])) + "'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' value='" + dtavl.Rows[i]["ID"] + "' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-ID'/><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_1_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + CurrentFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_VALUE_1_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_1_NM"])) + "'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_2_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + PrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_VALUE_2_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_2_NM"])) + "'></td>");
                    sbFinancialHead.Append("<td><input type='hidden' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_3_TX' value='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "_" + PrevToPrevFinYear + "' /><input type='text' onkeypress='return isNumberKey(event)' name='" + dtavl.Rows[i]["FIN_HEAD_ID"] + "-FIN_YEAR_VALUE_3_NM' class='form-control' value='" + string.Format("{0:N2}", Convert.ToDecimal(dtavl.Rows[i]["FIN_YEAR_VALUE_3_NM"])) + "'></td>");
                    sbFinancialHead.Append("</tr>");
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@FINANCIAL_HEAD", sbFinancialHead.ToString());
            }
            #endregion
        }

        public string GetFinancialYear(int CurYear, int PrevYear, int NextYear)
        {
            string FinYear = null;
            if (DateTime.Today.Month > 3)
                FinYear = CurYear + "-" + NextYear.ToString().Substring(2, 2);
            else
                FinYear = PrevYear + "-" + CurYear.ToString().Substring(2, 2);
            return FinYear.Trim();
        }

        public ActionClass beforeCSRPolicies(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(GetCSRAwardsRegId("")));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.CSR_Policies), 76, 1);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterCSRPolicies(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        private void BindDynamicPage(Screen_T screen, int CATEGORY_TYPE_ID, int AnswereID, int SrNo)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 74);
            conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));
            DataTable dtQ = new DataTable();
            DataTable dtOP = new DataTable();
            DataTable dtANS = new DataTable();
            string TableName = string.Empty;
            int UploadedFiles = 0;
            int TotalFileUploader = 0;

            if (screen.ID == 230)
                TableName = "CSR_POLICIES_T";
            else if (screen.ID == 231)
                TableName = "STAKEHOLDER_ENGAGEMENT_T";
            else if (screen.ID == 232)
                TableName = "STRATEGY_EXECUTION_AND_IMPACT_T";
            else if (screen.ID == 233)
                TableName = "CSR_MONITORING_AND_REPORTING_T";

            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
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
                conditions.Add("CSR_AWARDS_REG_ID", Convert.ToString(GetCSRAwardsRegId("")));
                conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
                JObject jAnswers = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

                if (jAnswers != null && jAnswers.First.First.First != null)
                {
                    foreach (JProperty property in jAnswers.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtANS = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
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
                        if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                            count++;
                        sbQuestions.Append("<div><input type='hidden' value='" + dtANS.Rows[i]["ID"] + "' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-ID'/></div>");
                        if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                        {
                            sbQuestions.Append("<div class='col-xs-12'>");
                            sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtANS.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                            //sbQuestions.Append("<div class='text-right'>Marks: " + dtANS.Rows[i]["MARKS_NM"].ToString() + " </div>");
                            sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value='' class='form-control' onkeydown='maxlength(this)'>" + Convert.ToString(dtANS.Rows[i]["ANSWER_TX"]) + " </textarea></div>");
                            sbQuestions.Append("</div>");
                            //if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            //    sbQuestions.Append("</div>");
                            //}
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
                                sbQuestions.Append("<textarea type='textarea' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control' onkeydown='maxlength(this)'>" + dtANS.Rows[i]["QUES_DESCRIPTION_TX"] + "</textarea>");
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
                                    sbQuestions.Append("&nbsp;&nbsp;<a target='_blank' style='color:blue;' title='Click to view' href='../CSR/DownloadFileByIDFromSpecificTable?id=" + dtANS.Rows[i]["ID"] + "&TableName=" + TableName + "&ColumnName=QUES_FILENAME_TX&schema=CSR'>" + FileName + "</a>");

                                    if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 14)
                                    {
                                        sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                        sbQuestions.Append("<input type='file' onchange='ValidateSize(this," + dtANS.Rows[i]["QUESTION_ID"] + ")' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                        sbQuestions.Append("</div>");
                                    }
                                }
                                else if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 14)
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

                            //if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            //    sbQuestions.Append("</div>");
                            //}
                        }
                        else if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                        {
                            if (screen.ID == 230)
                                sbQuestions.Append(BindStaticTableForCSRPolicy(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U'));
                            else if (screen.ID == 231)
                                sbQuestions.Append(BindStaticTableForStakeholder(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', screen));
                        }
                        if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                        {
                            AdminMarks++;
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            sbQuestions.Append("</div>");

                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label> Remarks: </label><textarea id='" + AdminMarks + "-EVAL_REMARKS_TX' type='text' rows='5' name='" + dtQ.Rows[i]["ID"] + "-EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[i]["EVAL_REMARKS_TX"] + "</textarea>");
                            sbQuestions.Append("</div>");

                            //if (i == dtANS.Rows.Count - 1)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Evaluation Remarks: </label><textarea id='" + AdminMarks + "-Marks' type='text' rows='5' name='EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[0]["EVAL_REMARKS_TX"] + "</textarea>");
                            //    sbQuestions.Append("</div>");
                            //}
                        }
                    }
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Questions", sbQuestions.ToString());
                    #endregion
                }
                else
                {
                    if (dtQ != null && dtQ.Rows != null && dtQ.Rows.Count > 0)
                    {
                        #region Without Ans bind
                        int count = 0;
                        StringBuilder sbQuestions = new StringBuilder();
                        for (int i = 0; i < dtQ.Rows.Count; i++)
                        {
                            if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                                count++;

                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                            {
                                sbQuestions.Append("<div class='col-xs-12'>");
                                sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                                //sbQuestions.Append("<div class='text-right'>Marks: " + dtQ.Rows[i]["MARKS_NM"].ToString() + " </div>");
                                sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value='' class='form-control' onkeydown='maxlength(this)'> </textarea></div>");
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
                                        sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                }
                                sbQuestions.Append("</div>");

                                if (dtQ.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                                {
                                    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                    sbQuestions.Append("<label>" + dtQ.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");
                                    //sbQuestions.Append("<input type='text' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'>");
                                    sbQuestions.Append("<textarea type='textarea' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control' onkeydown='maxlength(this)'></textarea>");
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
                                if (screen.ID == 230)
                                    sbQuestions.Append(BindStaticTableForCSRPolicy(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'I'));
                                else if (screen.ID == 231)
                                    sbQuestions.Append(BindStaticTableForStakeholder(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'I', screen));
                            }
                        }
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Questions", sbQuestions.ToString());
                        #endregion
                    }
                }

                if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
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
            int CSR_AWARDS_REG_ID = 0;

            #region For Insertion and updation
            foreach (var key in frm.AllKeys)
            {
                if (key.EndsWith("ANSWERE_TX") && Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 14)
                {
                    Mul_tblData = new Dictionary<string, object>();

                    string[] split = key.Split('-');
                    int QUESTION_ID = Convert.ToInt32(split[0]);
                    CSR_AWARDS_REG_ID = Convert.ToInt32(frm["CSR_AWARDS_REG_ID"]);
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
                    Mul_tblData.Add("CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID);
                    Mul_tblData.Add("ANSWER_TX", ANSWERE_TX);
                    Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                    if (!string.IsNullOrEmpty(QUES_DESCRIPTION_TX))
                        Mul_tblData.Add("QUES_DESCRIPTION_TX", QUES_DESCRIPTION_TX);

                    #region For File Uploading
                    if (hFileName != null && !string.IsNullOrEmpty(hFileName.FileName))
                    {
                        QuestionIds += QUESTION_ID + ",";
                        string FolderName = "CSR\\Documents\\" + CSR_AWARDS_REG_ID;
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
                    string EVAL_REMARKS_TX = string.Empty;
                    int ID = 0;
                    if (!string.IsNullOrEmpty(Convert.ToString(frm[QUESTION_ID + "-ID"])))
                        ID = Convert.ToInt32(frm[QUESTION_ID + "-ID"]);
                    if (ID > 0)
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
                    if (ID > 0)
                        lstData1.Add(Mul_tblData);
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
                            string FolderName = "CSR\\Documents\\" + CSR_AWARDS_REG_ID;
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

                #region insert and update for CSR policy static table
                if (frm.AllKeys.Contains("CSR_POLICY_STATIC_TABLE_T"))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    int QuestionId = Convert.ToInt32(frm["CSR_POLICY_STATIC_TABLE_T"]);

                    for (int i = 0; i < 4; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-DATE_OF_METTING_DT"])) &&
                            !string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_TX"])))
                        {
                            int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                            Mul_tblData = new Dictionary<string, object>();
                            if (ID > 0 && i == 0)
                                screenType = "update";
                            Mul_tblData.Add("ID", ID);
                            Mul_tblData.Add("QUESTION_ID", QuestionId);
                            Mul_tblData.Add("CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID);
                            Mul_tblData.Add("DATE_OF_METTING_DT", Convert.ToDateTime(frm["" + i + "-DATE_OF_METTING_DT"]));
                            Mul_tblData.Add("NO_OF_COMM_MEMBER_TX", Convert.ToString(frm["" + i + "-NO_OF_COMM_MEMBER_TX"]));
                            Mul_tblData.Add("NO_OF_ATT_MEMBER_TX", Convert.ToString(frm["" + i + "-NO_OF_ATT_MEMBER_TX"]));
                            Mul_tblData.Add("PER_OF_ATTENDANCE_TX", Convert.ToString(frm["" + i + "-PER_OF_ATTENDANCE_TX"]));
                            Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                            lstData1.Add(Mul_tblData);
                        }
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "CSR_POLICY_STATIC_TABLE_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    }
                }
                #endregion

                #region insert and update for STAKEHOLDER ENGAGEMENT static table
                if (frm.AllKeys.Contains("STAKEHOLDER_STATIC_TABLE_T"))
                {
                    int UploadedRows = Convert.ToInt32(frm["AvailableRows"]);
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    int QuestionId = Convert.ToInt32(frm["STAKEHOLDER_STATIC_TABLE_T"]);

                    for (int i = 0; i < UploadedRows; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-FOCUS_AREA_TX"])) &&
                            !string.IsNullOrEmpty(Convert.ToString(frm["" + i + "-GEOGRAPHICAL_LOCATION_TX"])))
                        {
                            int ID = Convert.ToInt32(frm["" + QuestionId + "-" + i + "-ID"]);
                            Mul_tblData = new Dictionary<string, object>();
                            if (ID > 0 && i == 0)
                                screenType = "update";
                            Mul_tblData.Add("ID", ID);
                            Mul_tblData.Add("QUESTION_ID", QuestionId);
                            Mul_tblData.Add("CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID);
                            Mul_tblData.Add("FOCUS_AREA_TX", Convert.ToString(frm["" + i + "-FOCUS_AREA_TX"]));
                            Mul_tblData.Add("GEOGRAPHICAL_LOCATION_TX", Convert.ToString(frm["" + i + "-GEOGRAPHICAL_LOCATION_TX"]));
                            Mul_tblData.Add("BROAD_HEAD_TX", Convert.ToString(frm["" + i + "-BROAD_HEAD_TX"]));
                            Mul_tblData.Add("EXPENDITURE_FOCUS_AREA_TX", Convert.ToString(frm["" + i + "-EXPENDITURE_FOCUS_AREA_TX"]));
                            Mul_tblData.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);
                            lstData1.Add(Mul_tblData);
                        }
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "STAKEHOLDER_STATIC_TABLE_T", 0, 0, lstData1, conditions));
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

        public ActionClass beforeStakeholderEngagement(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stakeholder_Engagement), 77, 2);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterStakeholderEngagement(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeStrategyExecution(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Strategy_Execution_and_Impact), 78, 3);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterStrategyExecution(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeCSRMonitoring(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.CSR_Monitoring_and_Reporting), 79, 4);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterCSRMonitoring(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public ActionClass beforeSustainability(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            BindDynamicPage(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Sustainability), 283, 5);

            ActionClass act = UtilService.beforeLoad(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", HttpContext.Current.Session["CSR_APPROVE_NM"].ToString());
            return act;
        }

        public ActionClass afterSustainability(int WEB_APP_ID, FormCollection frm)
        {
            return DMLOPsDyamicPage(WEB_APP_ID, frm);
        }

        public enum QUESTION_CATEGORY_TYPE
        {
            CSR_Policies = 1,
            Stakeholder_Engagement = 2,
            Strategy_Execution_and_Impact = 3,
            CSR_Monitoring_and_Reporting = 4,
            Sustainability = 5
        }

        private StringBuilder BindStaticTableForCSRPolicy(int QuesID, string QuesName, char Type)
        {
            int rowCount = 4;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 83);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CSR_AWARDS_REG_ID", GetCSRAwardsRegId(""));
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");

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
            sbStaticTable.Append("<div class='fontBold pt-20'>1.10 " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='CSR_POLICY_STATIC_TABLE_T'/>");
            sbStaticTable.Append("<input type='hidden' value='CSR_POLICY_STATIC_TABLE_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>Date of Meeting</th>");
            sbStaticTable.Append("<th>Total No. of Committee Members on the date of Meeting</ th>");
            sbStaticTable.Append("<th>No. of members who attended the meeting</th>");
            sbStaticTable.Append("<th>% of Attendance</th>");
            sbStaticTable.Append("</tr></thead><tbody>");
            double SumOfAttandance = 0;

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"])))
                        SumOfAttandance += Convert.ToDouble(dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"]);
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_METTING_DT' value='" + dtStaticTable.Rows[i]["DATE_OF_METTING_DT"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' onkeyup='return GetAttandance(" + i + ")' name='" + i + "-NO_OF_COMM_MEMBER_TX' value='" + dtStaticTable.Rows[i]["NO_OF_COMM_MEMBER_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' onkeyup='return GetAttandance(" + i + ")' name='" + i + "-NO_OF_ATT_MEMBER_TX' value='" + dtStaticTable.Rows[i]["NO_OF_ATT_MEMBER_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-PER_OF_ATTENDANCE_TX' value='" + dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"] + "' class='form-control' readonly></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_METTING_DT' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><input type='text' onkeypress='return isNumberKey(event)' onkeyup='return GetAttandance(" + i + ")' name='" + i + "-NO_OF_COMM_MEMBER_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' onkeypress='return isNumberKey(event)' onkeyup='return GetAttandance(" + i + ")' name='" + i + "-NO_OF_ATT_MEMBER_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-PER_OF_ATTENDANCE_TX' value='' class='form-control' readonly></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("<tr>");
            //sbStaticTable.Append("<td colspan='3'>Average of column <lable id='lblAvgofCol4'>" + StaticTableCount + "</label></ td>");
            sbStaticTable.Append("<td colspan='3'>Average of column <lable id='lblAvgofCol4'>4</label></ td>");
            if (SumOfAttandance == 0)
                sbStaticTable.Append("<td><label id='lblAvgcol'>0 %</label></button></td>");
            else
                sbStaticTable.Append("<td><label id='lblAvgcol'>" + (SumOfAttandance / StaticTableCount) + " %</label></button></td>");
            sbStaticTable.Append("</tr>");
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

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
            //actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string _email = frm["EMAIL_TX"];
            int UID = 0;

            if (_email != null && !_email.Trim().Equals(""))
            {
                DataTable dtRDetails = new DataTable();
                Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 80); //check records available or not     
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
                        actionClass = UtilService.insertOrUpdate("CSR", "CSR_AWARDS_REGISTRATION_T", list);

                        // Update password in USER_T
                        if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
                        {
                            List<Dictionary<string, object>> list_Screen_T = new List<Dictionary<string, object>>();
                            Dictionary<string, object> d1 = new Dictionary<string, object>();
                            d1["ID"] = dtRDetails.Rows[i]["USER_ID"].ToString(); //Convert.ToString(HttpContext.Current.Session["USER_ID"]);
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

        private void Get_Submitted_Data(FormCollection frm, Screen_T screen)
        {
            DataTable dtAD = new DataTable();
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 81);
            conditions.Add("CSR_AWARDS_REG_ID", GetCSRAwardsRegId(""));

            JObject jdata = DBTable.GetData("qfetch", conditions, "CSR_AWARDS_APPROVAL_T", 0, 10, applicationSchema);
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
                    HttpContext.Current.Session["CSR_APPROVE_NM"] = dtAD.Rows[0]["APPROVE_NM"].ToString();
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", dtAD.Rows[0]["APPROVE_NM"].ToString());
                }
                else
                {
                    HttpContext.Current.Session["CSR_APPROVE_NM"] = "";
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", "");
                }
            }
            else
            {
                HttpContext.Current.Session["CSR_APPROVE_NM"] = "";
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", "");
            }
        }

        private int GetCSRAwardsRegId(string UniqueId)
        {
            int CSR_AWARDS_REG_ID = 0;
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14 && !string.IsNullOrEmpty(UniqueId) && UniqueId != "undefined")//For Admin
            {
                CSR_AWARDS_REG_ID = Convert.ToInt32(UniqueId);
                HttpContext.Current.Session["CSR_AWARDS_REG_ID"] = CSR_AWARDS_REG_ID;
            }
            else if (HttpContext.Current.Session["CSR_AWARDS_REG_ID"] != null)
                CSR_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["CSR_AWARDS_REG_ID"]);
            else
                CSR_AWARDS_REG_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);

            return CSR_AWARDS_REG_ID;
        }

        public ActionClass beforeQuestionMarksApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int CSR_AWARDS_REG_ID = GetCSRAwardsRegId("");
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 86);
            conditions.Add("CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");
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
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", CSR_AWARDS_REG_ID.ToString());
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

        public ActionClass beforeCSRRegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCSRRegistration(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            DataTable dtReg = new DataTable();
            Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 104);
            conditions.Add("EMAIL_TX", frm["EMAIL_TX"]);
            conditions.Add("NAME_OF_COMPANY_TX", frm["NAME_OF_COMPANY_TX"]);
            conditions.Add("FIN_YEAR_ID", frm["FIN_YEAR_ID"]);

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

        private void BindDynamicPagePrint(Screen_T screen, int CATEGORY_TYPE_ID, int AnswereID, int SrNo, FormCollection frm)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 74);
            conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));
            DataTable dtQ = new DataTable();
            DataTable dtOP = new DataTable();
            DataTable dtANS = new DataTable();
            string TableName = string.Empty;

            if (SrNo == 1)
                TableName = "CSR_POLICIES_T";
            else if (SrNo == 2)
                TableName = "STAKEHOLDER_ENGAGEMENT_T";
            else if (SrNo == 3)
                TableName = "STRATEGY_EXECUTION_AND_IMPACT_T";
            else if (SrNo == 4)
                TableName = "CSR_MONITORING_AND_REPORTING_T";
            else if (SrNo == 5)
                TableName = "SUSTAINABILITY_T";

            if (jdata != null && jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtQ = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
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
                conditions.Add("CSR_AWARDS_REG_ID", Convert.ToString(frm["ui"]));
                conditions.Add("Q_CATEGORY_TYPE_ID", CATEGORY_TYPE_ID);
                JObject jAnswers = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));

                if (jAnswers != null && jAnswers.First.First.First != null)
                {
                    foreach (JProperty property in jAnswers.Properties())
                    {
                        if (property.Name == "qfetch")
                        {
                            dtANS = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
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
                        if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                            count++;
                        sbQuestions.Append("<div><input type='hidden' value='" + dtANS.Rows[i]["ID"] + "' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-ID'/></div>");
                        if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                        {
                            sbQuestions.Append("<div class='col-xs-12'>");
                            sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtANS.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                            //sbQuestions.Append("<div class='text-right'>Marks: " + dtANS.Rows[i]["MARKS_NM"].ToString() + " </div>");
                            sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value='' class='form-control' onkeydown='maxlength(this)'>" + Convert.ToString(dtANS.Rows[i]["ANSWER_TX"]) + " </textarea></div>");
                            sbQuestions.Append("</div>");
                            //if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            //    sbQuestions.Append("</div>");
                            //}
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
                                    string[] ArrFileName = Convert.ToString(dtANS.Rows[i]["QUES_FILENAME_TX"]).Split('\\');
                                    string FileName = ArrFileName[ArrFileName.Length - 1];
                                    sbQuestions.Append("&nbsp;&nbsp;<a target='_blank' style='color:blue;' title='Click to view' href='../CSR/DownloadFileByIDFromSpecificTable?id=" + dtANS.Rows[i]["ID"] + "&TableName=" + TableName + "&ColumnName=QUES_FILENAME_TX&schema=CSR'>" + FileName + "</a>");
                                }
                                else if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) != 14)
                                {
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'>");
                                    sbQuestions.Append("<input type='file' onchange='ValidateSize(this)' name='" + dtANS.Rows[i]["QUESTION_ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'>");
                                    sbQuestions.Append("</div>");
                                    sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtANS.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label>");
                                }
                                sbQuestions.Append("</div>");
                            }
                            sbQuestions.Append("</div>");

                            //if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            //    sbQuestions.Append("</div>");
                            //}
                        }
                        else if (dtANS.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                        {
                            if (SrNo == 1)
                                sbQuestions.Append(BindStaticTableForCSRPolicyPrint(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', frm));
                            else if (SrNo == 2)
                                sbQuestions.Append(BindStaticTableForStakeholderPrint(Convert.ToInt32(dtANS.Rows[i]["QUESTION_ID"]), Convert.ToString(dtANS.Rows[i]["QUESTION_NAME_TX"]), 'U', frm));
                        }
                        if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14 && frm["aa"] == "1")
                        {
                            AdminMarks++;
                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label> Approved Marks: </label> <input type='text' id='" + AdminMarks + "-Marks' name='" + dtQ.Rows[i]["ID"] + "-ADMIN_MARKS_NM' value=" + dtANS.Rows[i]["ADMIN_MARKS"].ToString() + " class='form-control' onkeypress='return isDNumberKey(event)'>");
                            sbQuestions.Append("</div>");

                            sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            sbQuestions.Append("<label> Remarks: </label><textarea id='" + AdminMarks + "-EVAL_REMARKS_TX' type='text' rows='5' name='EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[i]["EVAL_REMARKS_TX"] + "</textarea>");
                            sbQuestions.Append("</div>");

                            //if (i == dtANS.Rows.Count - 1)
                            //{
                            //    AdminMarks++;
                            //    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                            //    sbQuestions.Append("<label> Evaluation Remarks: </label><textarea id='" + AdminMarks + "-Marks' type='text' rows='5' name='EVAL_REMARKS_TX' class='form-control'>" + dtANS.Rows[0]["EVAL_REMARKS_TX"] + "</textarea>");
                            //    sbQuestions.Append("</div>");
                            //}
                        }
                    }
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Questions" + SrNo.ToString(), sbQuestions.ToString());
                    #endregion
                }
                else
                {
                    if (dtQ != null && dtQ.Rows != null && dtQ.Rows.Count > 0)
                    {
                        #region Without Ans bind
                        int count = 0;
                        StringBuilder sbQuestions = new StringBuilder();
                        for (int i = 0; i < dtQ.Rows.Count; i++)
                        {
                            if (Convert.ToString(dtQ.Rows[i]["REF_ID"]) == "0")
                                count++;

                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "2")
                            {
                                sbQuestions.Append("<div class='col-xs-12'>");
                                sbQuestions.Append("<div class='fontBold pt-20'>" + SrNo + "." + count + " " + dtQ.Rows[i]["QUESTION_NAME_TX"].ToString() + "</div>");
                                //sbQuestions.Append("<div class='text-right'>Marks: " + dtQ.Rows[i]["MARKS_NM"].ToString() + " </div>");
                                sbQuestions.Append("<div class='col-md-12 pt-10'><textarea type='textarea' rows='2' name='" + dtQ.Rows[i]["ID"] + "-ANSWERE_TX' value='' class='form-control' onkeydown='maxlength(this)'> </textarea></div>");
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
                                        sbQuestions.Append("<div class='radio radio-question'><label> <input type='radio' value='" + dtOP.Rows[j]["ID"] + "' name='" + dtOP.Rows[j]["QUESTION_ID"] + "-ANSWERE_TX'>&nbsp;" + dtOP.Rows[j]["Q_OPTION_NAME_TX"].ToString() + " </label></div>");
                                }
                                sbQuestions.Append("</div>");

                                if (dtQ.Rows[i]["Q_ENABLE_TEXTAREA_YN"].ToString().ToUpper() == "TRUE")
                                {
                                    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                    sbQuestions.Append("<label>" + dtQ.Rows[i]["Q_TEXTAREA_NAME_TX"].ToString() + "</label>");
                                    //sbQuestions.Append("<input type='text' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'>");
                                    sbQuestions.Append("<textarea type='textarea' name='" + dtQ.Rows[i]["ID"] + "-QUES_DESCRIPTION_TX' value='' class='form-control'></textarea>");
                                    sbQuestions.Append("</div>");
                                }

                                if (dtQ.Rows[i]["Q_ENABLE_FILEUPLOADER_YN"].ToString().ToUpper() == "TRUE")
                                {
                                    sbQuestions.Append("<div class='col-md-12 mt-10'>");
                                    sbQuestions.Append("<div class='col-md-12' style='width:100%'><input type='file' onchange='ValidateSize(this)' name='" + dtQ.Rows[i]["ID"] + "-QUES_FILEUPLOADER' style='width:40%;margin-left: -15px;' class='fl form-control IpadInput mobilInputAddT'></div>");
                                    sbQuestions.Append("<label style='font-weight:normal; font-size:12px;'>(" + dtQ.Rows[i]["Q_FILEUPLOADER_TEXT_TX"].ToString() + ")</label>");
                                    sbQuestions.Append("</div>");
                                }
                                sbQuestions.Append("</div>");
                            }
                            if (dtQ.Rows[i]["QUESTION_TYPE"].ToString() == "3")//For Static Tables
                            {
                                if (SrNo == 1)
                                    sbQuestions.Append(BindStaticTableForCSRPolicyPrint(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'I', frm));
                                else if (SrNo == 2)
                                    sbQuestions.Append(BindStaticTableForStakeholderPrint(Convert.ToInt32(dtQ.Rows[i]["ID"]), Convert.ToString(dtQ.Rows[i]["QUESTION_NAME_TX"]), 'I', frm));
                            }
                        }
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@Questions" + SrNo.ToString(), sbQuestions.ToString());
                        #endregion
                    }
                }
            }
        }

        public ActionClass beforeCSRPrint(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_AWARDS_REG_ID", Convert.ToString(frm["ui"]));
            BindDynamicPagePrint(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.CSR_Policies), 76, 1, frm);
            BindDynamicPagePrint(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Stakeholder_Engagement), 77, 2, frm);
            BindDynamicPagePrint(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Strategy_Execution_and_Impact), 78, 3, frm);
            BindDynamicPagePrint(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.CSR_Monitoring_and_Reporting), 79, 4, frm);
            BindDynamicPagePrint(screen, Convert.ToInt32(QUESTION_CATEGORY_TYPE.Sustainability), 283, 5, frm);

            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Screen_Id == Convert.ToInt32(screen.ID)).FirstOrDefault();
            ActionClass act = UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, s.Id.ToString(), screen.ID.ToString());
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CSR_APPROVE_NM", "");

            if (frm["aa"] == "1")
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:block;");
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@DISPLAY_STATUS", "display:none;");

            return act;
        }

        private StringBuilder BindStaticTableForStakeholder(int QuesID, string QuesName, char Type, Screen_T screen)
        {
            int rowCount = 6;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 105);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CSR_AWARDS_REG_ID", GetCSRAwardsRegId(""));
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");

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

            if (StaticTableCount > 6)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@AvailableRows", StaticTableCount.ToString());
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@AvailableRows", rowCount.ToString());

            #region Bind Static Table
            StringBuilder sbStaticTable = new StringBuilder();

            sbStaticTable.Append("<div class='col-xs-12'>");
            sbStaticTable.Append("<div class='fontBold pt-20'>2.3 " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='STAKEHOLDER_STATIC_TABLE_T'/>");
            sbStaticTable.Append("<input type='hidden' value='STAKEHOLDER_STATIC_TABLE_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table id='tblStack' class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>S. No.</th>");
            sbStaticTable.Append("<th style='width:40%'>Focus Area</th>");
            sbStaticTable.Append("<th style='width:20%'>Geographical Location</ th>");
            sbStaticTable.Append("<th style='width:20%'>Broad Head as per Schedule VII</th>");
            sbStaticTable.Append("<th style='width:20%'>Expenditure on this focus area as % of total CSR expenditure</th>");
            sbStaticTable.Append("</tr></thead><tbody>");

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td>" + (i + 1) + "</td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-FOCUS_AREA_TX' value='" + dtStaticTable.Rows[i]["FOCUS_AREA_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><input type='text' name='" + i + "-GEOGRAPHICAL_LOCATION_TX' value='" + dtStaticTable.Rows[i]["GEOGRAPHICAL_LOCATION_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-BROAD_HEAD_TX' value='" + dtStaticTable.Rows[i]["BROAD_HEAD_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-EXPENDITURE_FOCUS_AREA_TX' value='" + dtStaticTable.Rows[i]["EXPENDITURE_FOCUS_AREA_TX"] + "' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td>" + (i + 1) + "</td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-FOCUS_AREA_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><input type='text' name='" + i + "-GEOGRAPHICAL_LOCATION_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-BROAD_HEAD_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='text' name='" + i + "-EXPENDITURE_FOCUS_AREA_TX' value='' class='form-control'></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            if (Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 14)
                sbStaticTable.Append("</tbody></table></div></div>");
            else
                sbStaticTable.Append("</tbody></table></div><div style='float:right;'><button class='btn btn-primary fontBold' title='Click to add more rows' onclick='return AddRows(" + QuesID + ")'>Add more..</button></div></div>");
            #endregion

            return sbStaticTable;
        }

        private StringBuilder BindStaticTableForCSRPolicyPrint(int QuesID, string QuesName, char Type, FormCollection frm)
        {
            int rowCount = 4;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 83);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CSR_AWARDS_REG_ID", frm["ui"]);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");

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
            sbStaticTable.Append("<div class='fontBold pt-20'>1.10 " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='CSR_POLICY_STATIC_TABLE_T'/>");
            sbStaticTable.Append("<input type='hidden' value='CSR_POLICY_STATIC_TABLE_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>Date of Meeting</th>");
            sbStaticTable.Append("<th>Total No. of Committee Members on the date of Meeting</ th>");
            sbStaticTable.Append("<th>No. of members who attended the meeting</th>");
            sbStaticTable.Append("<th>% of Attendance</th>");
            sbStaticTable.Append("</tr></thead><tbody>");
            double SumOfAttandance = 0;

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"])))
                        SumOfAttandance += Convert.ToDouble(dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"]);
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_METTING_DT' value='" + dtStaticTable.Rows[i]["DATE_OF_METTING_DT"] + "' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><label name='" + i + "-NO_OF_COMM_MEMBER_TX'> " + dtStaticTable.Rows[i]["NO_OF_COMM_MEMBER_TX"] + "</label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-NO_OF_ATT_MEMBER_TX'> " + dtStaticTable.Rows[i]["NO_OF_ATT_MEMBER_TX"] + "</lable></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-PER_OF_ATTENDANCE_TX'> " + dtStaticTable.Rows[i]["PER_OF_ATTENDANCE_TX"] + "</lable></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td><input type='date' placeholder='dd/MM/yyyy' style='padding:4px 0; width:130px;' name='" + i + "-DATE_OF_METTING_DT' value='' class='form-control'></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><label name='" + i + "-NO_OF_COMM_MEMBER_TX'> </label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-NO_OF_ATT_MEMBER_TX'> </label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-PER_OF_ATTENDANCE_TX'> </label></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("<tr>");
            //sbStaticTable.Append("<td colspan='3'>Average of column <lable id='lblAvgofCol4'>" + StaticTableCount + "</label></ td>");
            sbStaticTable.Append("<td colspan='3'>Average of column <lable id='lblAvgofCol4'>4</label></ td>");
            if (SumOfAttandance == 0)
                sbStaticTable.Append("<td><label id='lblAvgcol'>0 %</label></button></td>");
            else
                sbStaticTable.Append("<td><label id='lblAvgcol'>" + (SumOfAttandance / StaticTableCount) + " %</label></button></td>");
            sbStaticTable.Append("</tr>");
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        private StringBuilder BindStaticTableForStakeholderPrint(int QuesID, string QuesName, char Type, FormCollection frm)
        {
            int rowCount = 6;
            int StaticTableCount = 0;
            #region Get Data of Static Table
            DataTable dtStaticTable = new DataTable();
            if (Type == 'U')
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 105);
                conditions.Add("QUESTION_ID", QuesID);
                conditions.Add("CSR_AWARDS_REG_ID", frm["ui"]);
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");

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
            sbStaticTable.Append("<div class='fontBold pt-20'>2.3 " + QuesName + "</div>");
            sbStaticTable.Append("<div class='clearfix'></div>");
            sbStaticTable.Append("<div><input type='hidden' value='" + QuesID + "' name='STAKEHOLDER_STATIC_TABLE_T'/>");
            sbStaticTable.Append("<input type='hidden' value='STAKEHOLDER_STATIC_TABLE_T' name='" + QuesID + "-ANSWERE_TX'/></div>");
            sbStaticTable.Append("<div class='table-responsible pt-20'>");
            sbStaticTable.Append("<table class='table table-bordered'>");
            sbStaticTable.Append("<thead>");
            sbStaticTable.Append("<tr class='active'>");
            sbStaticTable.Append("<th>S. No.</th>");
            sbStaticTable.Append("<th style='width:40%'>Focus Area</th>");
            sbStaticTable.Append("<th style='width:20%'>Geographical Location</ th>");
            sbStaticTable.Append("<th style='width:20%'>Broad Head as per Schedule VII</th>");
            sbStaticTable.Append("<th style='width:20%'>Expenditure on this focus area as % of total CSR expenditure</th>");
            sbStaticTable.Append("</tr></thead><tbody>");

            if (dtStaticTable != null && dtStaticTable.Rows != null && dtStaticTable.Rows.Count > 0)
            {
                for (int i = 0; i < dtStaticTable.Rows.Count; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td>" + (i + 1) + "</td>");
                    sbStaticTable.Append("<td><label name='" + i + "-FOCUS_AREA_TX'> " + dtStaticTable.Rows[i]["FOCUS_AREA_TX"] + "</label></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='" + dtStaticTable.Rows[i]["ID"] + "' name='" + QuesID + "-" + i + "-ID'/><label name='" + i + "-GEOGRAPHICAL_LOCATION_TX'> " + dtStaticTable.Rows[i]["GEOGRAPHICAL_LOCATION_TX"] + "</lable></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-BROAD_HEAD_TX'> " + dtStaticTable.Rows[i]["BROAD_HEAD_TX"] + "</label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-EXPENDITURE_FOCUS_AREA_TX'> " + dtStaticTable.Rows[i]["EXPENDITURE_FOCUS_AREA_TX"] + "</label></td>");
                    sbStaticTable.Append("</tr>");
                }
            }

            if (rowCount != StaticTableCount)
            {
                for (int i = StaticTableCount; i < rowCount; i++)
                {
                    sbStaticTable.Append("<tr>");
                    sbStaticTable.Append("<td>" + (i + 1) + "</td>");
                    sbStaticTable.Append("<td><label name='" + i + "-FOCUS_AREA_TX'> </label></td>");
                    sbStaticTable.Append("<td><input type='hidden' value='0' name='" + QuesID + "-" + i + "-ID'/><label name='" + i + "-GEOGRAPHICAL_LOCATION_TX'> </label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-BROAD_HEAD_TX'> </label></td>");
                    sbStaticTable.Append("<td><label name='" + i + "-EXPENDITURE_FOCUS_AREA_TX'> </label></td>");
                    sbStaticTable.Append("</tr>");
                }
            }
            sbStaticTable.Append("</tbody></table></div></div>");
            #endregion

            return sbStaticTable;
        }

        public static DataTable EvaluationReport(int Category)
        {
            DataTable dtEval = new DataTable();
            DataRow myDataRow;
            string ScreenNameHeading = string.Empty;
            string TotalOfSecHeading = string.Empty;
            string PerOfSecHeading = string.Empty;
            float PerOfSection = 0;
            int AnsQueryId = 0;
            float SumOfSection = 0;
            int QuestionId = 0;
            DataRow[] drFilter;
            int CSR_AWARDS_REG_ID = 0;
            DataTable dtFilter;

            try
            {
                #region Get Top 3 Headings             
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QID", 151);
                conditions.Add("CATEGORY_NM", Category);
                JObject jdataTop3 = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "CSR");
                DataTable dtTop3Heading = new DataTable();

                if (jdataTop3 != null || jdataTop3.First.First.First != null)
                {
                    foreach (JProperty property in jdataTop3.Properties())
                    {
                        if (property.Name == "qfetch")
                            dtTop3Heading = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dtTop3Heading != null && dtTop3Heading.Rows != null && dtTop3Heading.Rows.Count > 0)
                {
                    #region Bind First row
                    dtEval.Columns.Add("CO. NO.", typeof(string));
                    dtEval.Columns.Add(" ", typeof(string));
                    for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                        dtEval.Columns.Add(Convert.ToString(dtTop3Heading.Rows[i]["APPLICATION_REF_ID"]), typeof(string));
                    #endregion

                    #region Bind Second Row
                    myDataRow = dtEval.NewRow();
                    myDataRow[0] = "Question";
                    myDataRow[1] = "Max. Marks";
                    for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                        myDataRow[i + 2] = dtTop3Heading.Rows[i]["NAME_OF_COMPANY_TX"];
                    dtEval.Rows.Add(myDataRow);
                    #endregion

                    #region Bind Third Row
                    myDataRow = dtEval.NewRow();
                    myDataRow[0] = "Category";
                    myDataRow[1] = "";
                    for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                        myDataRow[i + 2] = dtTop3Heading.Rows[i]["CATEGORY_TX"];
                    dtEval.Rows.Add(myDataRow);
                    #endregion
                }
                #endregion

                for (int Cat = 1; Cat <= 4; Cat++)
                {
                    #region Define Categories and Heading
                    if (Cat == 1)
                    {
                        ScreenNameHeading = "1 CSR POLICY";
                        TotalOfSecHeading = "Total of Section I [CSR Policies]";
                        PerOfSecHeading = "% of Section I(15%)";
                        PerOfSection = 15;
                        AnsQueryId = 153;
                    }
                    else if (Cat == 2)
                    {
                        ScreenNameHeading = "2 STAKEHOLDER ENGAGEMENT";
                        TotalOfSecHeading = "Total of Section II [Stakeholder Engagement]";
                        PerOfSecHeading = "% of Section II(25%)";
                        PerOfSection = 25;
                        AnsQueryId = 154;
                    }
                    else if (Cat == 3)
                    {
                        ScreenNameHeading = "3 STRATEGY EXECUTION AND IMPACT";
                        TotalOfSecHeading = "Total of Section III [Strategy Execution and Impact]";
                        PerOfSecHeading = "% of Section III(30%)";
                        PerOfSection = 30;
                        AnsQueryId = 155;
                    }
                    else if (Cat == 4)
                    {
                        ScreenNameHeading = "4 CSR MONITORING AND REPORTING";
                        TotalOfSecHeading = "Total of Section IV [CSR Monitoring and Reporting]";
                        PerOfSecHeading = "% of Section IV(30%)";
                        PerOfSection = 30;
                        AnsQueryId = 156;
                    }
                    #endregion

                    #region Get Questions
                    conditions = new Dictionary<string, object>();
                    conditions.Add("QID", 152);
                    conditions.Add("Q_CATEGORY_TYPE_ID", Cat);

                    JObject jdataGetQues = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 1000, "CSR");
                    DataTable dtGetQues = new DataTable();

                    if (jdataGetQues != null || jdataGetQues.First.First.First != null)
                    {
                        foreach (JProperty property in jdataGetQues.Properties())
                        {
                            if (property.Name == "qfetch")
                                dtGetQues = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }
                    #endregion

                    #region Get Companies Ans
                    conditions = new Dictionary<string, object>();
                    conditions.Add("QID", AnsQueryId);
                    conditions.Add("CATEGORY_NM", Category);

                    JObject jdataGetAns = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 1000, "CSR");
                    DataTable dtGetAns = new DataTable();

                    if (jdataGetAns != null || jdataGetAns.First.First.First != null)
                    {
                        foreach (JProperty property in jdataGetAns.Properties())
                        {
                            if (property.Name == "qfetch")
                                dtGetAns = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }
                    #endregion

                    #region Bind Questions
                    if (dtGetQues != null && dtGetQues.Rows != null && dtGetQues.Rows.Count > 0)
                    {
                        SumOfSection = 0;
                        myDataRow = dtEval.NewRow();
                        myDataRow[0] = ScreenNameHeading;
                        for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                            myDataRow[i + 1] = "";
                        dtEval.Rows.Add(myDataRow);

                        for (int iQues = 0; iQues < dtGetQues.Rows.Count; iQues++)
                        {
                            QuestionId = Convert.ToInt32(dtGetQues.Rows[iQues]["ID"]);

                            myDataRow = dtEval.NewRow();
                            myDataRow[0] = "Que " + (iQues + 1);
                            myDataRow[1] = Convert.ToString(dtGetQues.Rows[iQues]["TOTAL_MARKS_NM"]);
                            if (!string.IsNullOrEmpty(Convert.ToString(dtGetQues.Rows[iQues]["TOTAL_MARKS_NM"])))
                                SumOfSection += float.Parse(Convert.ToString(dtGetQues.Rows[iQues]["TOTAL_MARKS_NM"]), CultureInfo.InvariantCulture.NumberFormat);
                            for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                            {
                                CSR_AWARDS_REG_ID = Convert.ToInt32(dtTop3Heading.Rows[i]["CSR_AWARDS_REG_ID"]);
                                drFilter = dtGetAns.Select("ID = " + QuestionId + "AND CSR_AWARDS_REG_ID=" + CSR_AWARDS_REG_ID);

                                if (drFilter != null && drFilter.Count() > 0)
                                    myDataRow[i + 2] = Convert.ToString(drFilter[0]["ADMIN_MARKS_NM"]);
                                else
                                    myDataRow[i + 2] = "0";
                            }
                            dtEval.Rows.Add(myDataRow);
                        }
                    }
                    #endregion

                    #region Sum of All questions
                    if (dtGetAns != null && dtGetAns.Rows != null && dtGetAns.Rows.Count > 0)
                    {
                        myDataRow = dtEval.NewRow();
                        myDataRow[0] = TotalOfSecHeading;
                        SumOfSection = float.Parse(dtGetQues.AsEnumerable().Sum(x => x.Field<Int64>("TOTAL_MARKS_NM")).ToString(), CultureInfo.InvariantCulture.NumberFormat);
                        myDataRow[1] = SumOfSection;
                        for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                        {
                            dtFilter = new DataTable();
                            CSR_AWARDS_REG_ID = Convert.ToInt32(dtTop3Heading.Rows[i]["CSR_AWARDS_REG_ID"]);
                            if (dtGetAns.Select("CSR_AWARDS_REG_ID = " + CSR_AWARDS_REG_ID).Count() > 0)
                            {
                                dtFilter = dtGetAns.Select("CSR_AWARDS_REG_ID = " + CSR_AWARDS_REG_ID).CopyToDataTable();
                                if (dtFilter != null && dtFilter.Rows != null)
                                    myDataRow[i + 2] = dtFilter.AsEnumerable().Sum(x => x.Field<double>("ADMIN_MARKS_NM")).ToString("F");
                            }
                            else
                                myDataRow[i + 2] = 0;
                        }
                        dtEval.Rows.Add(myDataRow);
                    }
                    #endregion

                    #region Average of All questions
                    if (dtGetAns != null && dtGetAns.Rows != null && dtGetAns.Rows.Count > 0)
                    {
                        float TotalMarks = 0;
                        myDataRow = dtEval.NewRow();
                        myDataRow[0] = PerOfSecHeading;
                        myDataRow[1] = PerOfSection * SumOfSection / SumOfSection;
                        for (int i = 0; i < dtTop3Heading.Rows.Count; i++)
                        {
                            dtFilter = new DataTable();
                            CSR_AWARDS_REG_ID = Convert.ToInt32(dtTop3Heading.Rows[i]["CSR_AWARDS_REG_ID"]);
                            if (dtGetAns.Select("CSR_AWARDS_REG_ID = " + CSR_AWARDS_REG_ID).Count() > 0)
                            {
                                dtFilter = dtGetAns.Select("CSR_AWARDS_REG_ID = " + CSR_AWARDS_REG_ID).CopyToDataTable();
                                if (dtFilter != null && dtFilter.Rows != null)
                                {
                                    TotalMarks = float.Parse(dtFilter.AsEnumerable().Sum(x => x.Field<double>("ADMIN_MARKS_NM")).ToString(), CultureInfo.InvariantCulture.NumberFormat);
                                    myDataRow[i + 2] = (PerOfSection * TotalMarks / SumOfSection).ToString("F");
                                }
                            }
                            else
                                myDataRow[i + 2] = 0;
                        }
                        dtEval.Rows.Add(myDataRow);
                    }
                    #endregion
                }

                #region Grand Total of all Sections
                DataRow BlankRow = dtEval.NewRow();
                float SumGrandTotal = 0;
                DataTable dtGrandTotal = new DataTable();
                dtGrandTotal = dtEval.AsEnumerable().Where(x => x.Field<string>("CO. NO.").Contains("% of Section")).CopyToDataTable();

                myDataRow = dtEval.NewRow();
                myDataRow[0] = "Grand Total";
                BlankRow[0] = "";
                for (int i = 0; i <= dtTop3Heading.Rows.Count; i++)
                {
                    SumGrandTotal = 0;
                    for (int k = 0; k < dtGrandTotal.Rows.Count; k++)
                    {
                        SumGrandTotal += float.Parse(Convert.ToString(dtGrandTotal.Rows[k][i + 1]), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    BlankRow[i + 1] = "";
                    myDataRow[i + 1] = SumGrandTotal.ToString("F");
                }
                dtEval.Rows.Add(BlankRow);
                dtEval.Rows.Add(myDataRow);
                #endregion
            }
            catch (Exception ex)
            {
            }

            return dtEval;
        }

        public static DataTable NominationsReport()
        {
            DataTable dtNominations = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 158);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 300, "CSR");

            if (jdata != null || jdata.First.First.First != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtNominations = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            return dtNominations;
        }

        public static void GetActiveYear()
        {
            string TableName = "FINANCIAL_YEAR_T";
            JObject jdata = null;
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("SHOW_YEAR_YN", 1);

            jdata = DBTable.GetData("fetch", conditions, TableName, 0, 1000, "CSR");
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

        public static DataTable RegisteredCompanies()
        {
            DataTable dtRegistered = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 255);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 500, "CSR");

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

        public ActionClass beforeCSRSupportingAttachements(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCSRSupportingAttachements(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            string CSR_AWARDS_REG_ID = HttpContext.Current.Session["CSR_AWARDS_REG_ID"].ToString();
            frm["CSR_AWARDS_REG_ID"] = CSR_AWARDS_REG_ID;

            HttpPostedFile hFileName = HttpContext.Current.Request.Files[0] as HttpPostedFile;
            if (!string.IsNullOrEmpty(hFileName.FileName))
            {
                string FolderName = "CSR\\Documents\\Support_Attachement\\";
                string _path = Util.UtilService.getDocumentPath(FolderName);
                string _FullPath = _path + "\\" + Convert.ToString(hFileName.FileName);
                frm["DOC_PATH_TX"] = _FullPath;
            }
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            return act;
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

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
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
    }
}