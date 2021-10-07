using ClosedXML.Excel;
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
using System.Web;
using System.Web.Mvc;
using static ICSI_WebApp.Util.UtilService;

namespace ICSI_WebApp.BusinessLayer
{
    public class HQLayer
    {
        public ActionClass beforeTrainingStructure(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterTrainingStructure(int WEB_APP_ID, FormCollection frm)
        {
            string screenType = frm["s"];
            ActionClass act = null;
            if (screenType != null && screenType.Equals("insert"))
            {
                Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                Dictionary<string, string> conditionops = new Dictionary<string, string>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("EDN_DT", "null");
                //conditionops.Add("ACTIVE_YN", "=");
                conditionops.Add("EDN_DT", "is");

                DataTable dt = Util.UtilService.getData(applicationSchema, screen.Table_Name_Tx, conditions, conditionops, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    act = Util.UtilService.GetMessage(-201);
                }
                else
                {
                    conditions.Clear();
                    conditions.Add("START_DT", frm["START_DT"]);
                    conditionops.Clear();
                    conditionops.Add("START_DT", ">=");
                    dt = Util.UtilService.getData(applicationSchema, screen.Table_Name_Tx, conditions, conditionops, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        act = Util.UtilService.GetMessage(-202);
                    }
                    else
                    {
                        conditions.Clear();
                        conditions.Add("EDN_DT", frm["START_DT"]);
                        conditionops.Clear();
                        conditionops.Add("EDN_DT", ">=");
                        dt = Util.UtilService.getData(applicationSchema, screen.Table_Name_Tx, conditions, conditionops, 0, 1);
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            act = Util.UtilService.GetMessage(-203);
                        }
                        else
                        {
                            conditions.Clear();
                            conditions.Add("STRUCTURE_NAME_TX", frm["STRUCTURE_NAME_TX"]);
                            dt = Util.UtilService.getData(applicationSchema, screen.Table_Name_Tx, conditions, null, 0, 1);
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                act = Util.UtilService.GetMessage(-101);
                            }
                            else
                            {
                                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                            }
                        }
                    }
                }
            }
            else
            {
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }
            return act;
        }

        public ActionClass beforeLicentiateApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterLicentiateApproval(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            bool innerresult = true;
            if (frm["u"] != null && frm["u"] != "")
            {
                frm.Add("ACTION_BY", frm["U"]);
            }
            if (frm["ED_ID"] != null)
            {
                for (int i = 0; i < frm["ED_ID"].Split(',').Length; i++)
                {
                    FormCollection FRM1 = new FormCollection();
                    FRM1.Add("ID", Convert.ToString(frm["ED_ID"].Split(',')[i]));
                    FRM1.Add("APPROVE_NM", Convert.ToString(frm["DOC_APPROVE_NM"].Split(',')[i]));
                    FRM1.Add("s", "update");
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("u", frm["u"]);
                    }
                    if (frm["m"] != null && frm["m"] != "")
                    {
                        FRM1.Add("m", frm["m"]);
                    }
                    if (frm["ui"] != null && frm["ui"] != "")
                    {
                        FRM1.Add("ui", frm["ui"]);
                    }
                    if (frm["si"] != null && frm["si"] != "")
                    {
                        FRM1.Add("si", frm["si"]);
                    }
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("ACTION_BY", frm["U"]);
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_LICENTIATE_REG_EDUCATION_DTL_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                        break;
                    }
                }
            }
            if (innerresult)
            {
                FormCollection FRM1 = new FormCollection();
                FRM1.Add("s", "update");
                if (frm["u"] != null && frm["u"] != "")
                {
                    FRM1.Add("u", frm["u"]);
                }
                if (frm["m"] != null && frm["m"] != "")
                {
                    FRM1.Add("m", frm["m"]);
                }
                if (frm["ui"] != null && frm["ui"] != "")
                {
                    FRM1.Add("ui", frm["ui"]);
                }
                if (frm["si"] != null && frm["si"] != "")
                {
                    FRM1.Add("si", frm["si"]);
                }
                if (frm["REG_ID"] != null && frm["REG_ID"] != "")
                {
                    FRM1.Add("REG_ID", frm["REG_ID"]);
                }
                if (frm["VALID_DT"] != null && frm["VALID_DT"] != "")
                {
                    FRM1.Add("VALID_DT", frm["VALID_DT"]);
                }
                if (frm["DateFormat"] != null && frm["DateFormat"] != "")
                {
                    FRM1.Add("DateFormat", frm["DateFormat"]);
                }
                if (frm["EXPIRY_DT"] != null && frm["EXPIRY_DT"] != "")
                {
                    FRM1.Add("EXPIRY_DT", frm["EXPIRY_DT"]);
                }
                if (frm["APPROVED_DT"] != null && frm["APPROVED_DT"] != "")
                {
                    FRM1.Add("APPROVED_DT", frm["APPROVED_DT"]);
                }
                FRM1.Add("VALID_YN", "1");
                FRM1.Add("EXPIRED_YN", "0");
                if (frm["APPROVE_NM"] == "1")
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_LICENTIATE_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                    }
                    else
                    {
                        FRM1["s"] = "insert";
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_LICENTIATE_REGISTERED_STUDENT_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                    }
                }
                else
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_LICENTIATE_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                        FRM1.Add("ACTIVE_YN", "0");
                        ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_LICENTIATE_REGISTERED_STUDENT_T");
                        if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                        {
                            innerresult = true;
                        }
                        else
                        {
                            act = act1;
                            innerresult = false;
                        }
                    }
                }

                if (innerresult)
                {
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
            }
            return act;
        }

        public ActionClass beforeFCSApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterFCSApproval(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            bool innerresult = true;
            if (frm["u"] != null && frm["u"] != "")
            {
                frm.Add("ACTION_BY", frm["U"]);
            }
            if (frm["ED_ID"] != null)
            {
                for (int i = 0; i < frm["ED_ID"].Split(',').Length; i++)
                {
                    FormCollection FRM1 = new FormCollection();
                    FRM1.Add("ID", Convert.ToString(frm["ED_ID"].Split(',')[i]));
                    FRM1.Add("APPROVE_NM", Convert.ToString(frm["DOC_APPROVE_NM"].Split(',')[i]));
                    FRM1.Add("s", "update");
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("u", frm["u"]);
                    }
                    if (frm["m"] != null && frm["m"] != "")
                    {
                        FRM1.Add("m", frm["m"]);
                    }
                    if (frm["ui"] != null && frm["ui"] != "")
                    {
                        FRM1.Add("ui", frm["ui"]);
                    }
                    if (frm["si"] != null && frm["si"] != "")
                    {
                        FRM1.Add("si", frm["si"]);
                    }
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("ACTION_BY", frm["U"]);
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_FCS_REG_EDUCATION_DTL_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                        break;
                    }
                }
            }
            if (innerresult)
            {
                FormCollection FRM1 = new FormCollection();
                FRM1.Add("s", "update");
                if (frm["u"] != null && frm["u"] != "")
                {
                    FRM1.Add("u", frm["u"]);
                }
                if (frm["m"] != null && frm["m"] != "")
                {
                    FRM1.Add("m", frm["m"]);
                }
                if (frm["ui"] != null && frm["ui"] != "")
                {
                    FRM1.Add("ui", frm["ui"]);
                }
                if (frm["si"] != null && frm["si"] != "")
                {
                    FRM1.Add("si", frm["si"]);
                }
                if (frm["REG_ID"] != null && frm["REG_ID"] != "")
                {
                    FRM1.Add("REG_ID", frm["REG_ID"]);
                }
                if (frm["VALID_DT"] != null && frm["VALID_DT"] != "")
                {
                    FRM1.Add("VALID_DT", frm["VALID_DT"]);
                }
                if (frm["DateFormat"] != null && frm["DateFormat"] != "")
                {
                    FRM1.Add("DateFormat", frm["DateFormat"]);
                }
                if (frm["EXPIRY_DT"] != null && frm["EXPIRY_DT"] != "")
                {
                    FRM1.Add("EXPIRY_DT", frm["EXPIRY_DT"]);
                }
                if (frm["APPROVED_DT"] != null && frm["APPROVED_DT"] != "")
                {
                    FRM1.Add("APPROVED_DT", frm["APPROVED_DT"]);
                }
                FRM1.Add("VALID_YN", "1");
                FRM1.Add("EXPIRED_YN", "0");
                if (frm["APPROVE_NM"] == "1")
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_FCS_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                    }
                    else
                    {
                        FRM1["s"] = "insert";
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_FCS_REGISTERED_STUDENT_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                    }
                }
                else
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_FCS_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                        FRM1.Add("ACTIVE_YN", "0");
                        ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_FCS_REGISTERED_STUDENT_T");
                        if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                        {
                            innerresult = true;
                        }
                        else
                        {
                            act = act1;

                            innerresult = false;
                        }
                    }
                }

                if (innerresult)
                {
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
            }
            return act;
        }

        public ActionClass searchFCSApproval(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            long forwardto = 0;
            int userid = 0;

            if (HttpContext.Current.Session["USER_ID"] != null)
            {
                int.TryParse(Convert.ToString(HttpContext.Current.Session["USER_ID"]), out userid);
            }
            UserData uSER_DATA = new UserData();
            uSER_DATA = ((ICSI_WebApp.Util.UserData)HttpContext.Current.Session["USER_DATA"]);
            var ROLE_ID = uSER_DATA.USER_ROLE_T.AsEnumerable().Where(myRow => myRow.Field<long>("USER_ID") == userid).Select(x => x.Field<long>("ROLE_ID")).FirstOrDefault();

            if (frm["SCRH_APPROVAL_FILTER"] != null)
            {
                if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) != "")
                {
                    if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Pending" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Approved" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Rejected")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    }
                    else if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Recommended")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Remove("SCRH_APPROVAL_FILTER");
                        frm.Remove("COND_APPROVAL_FILTER");
                        frm.Add("COND_APPROVAL_FILTER", "AND =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Approval");
                        frm.Add("COND_APPROVAL_FILTER", "OR =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Rejection");
                    }
                    else
                    {
                        forwardto = ROLE_ID;
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                        frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                    }
                }
                else
                {
                    forwardto = ROLE_ID;
                    frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                    frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                    frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                }
            }
            else
            {
                forwardto = ROLE_ID;
                frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                frm.Remove("COND_FORWARD_TO_ROLE_ID");
                frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
            }

            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }

        public static ActionClass afterSubmitforBL(int WEB_APP_ID, FormCollection frm, string tablename)
        {
            ActionClass actionClass = new ActionClass();
            string Message = string.Empty;
            string userid = frm["u"];
            string menuid = frm["m"];
            string screenType = frm["s"];
            string uniqueId = frm["ui"];
            string screenId = frm["si"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            // screentype = insert / update / fetch
            try
            {
                if (screenType != null)
                {
                    if (screenType.Equals("fetch"))
                    {
                        int st = 0;
                        int ed = 0;
                        if (frm["st"] != null) st = Convert.ToInt32(frm["st"]);
                        if (frm["ed"] != null) ed = Convert.ToInt32(frm["ed"]);
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        foreach (var key in frm.AllKeys)
                        {
                            // FTCH_TABLE_NAME___COLUMN_NAME
                            if (key.StartsWith("FTCH_"))
                            {
                                conditions.Add(key.Substring(5), frm[key]);
                            }
                        }

                    }

                    else if (screenType.Equals("insert") || screenType.Equals("update"))
                    {
                        Screen_T screen = screenObject(WEB_APP_ID, frm);
                        if (screen != null && userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            string applicationSchema = getApplicationScheme(screen);
                            Dictionary<string, object> conditions = new Dictionary<string, object>();
                            JObject jdata = DBTable.GetData("mfetch", null, tablename, 0, 100, applicationSchema);
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();

                            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                            Dictionary<string, object> tableData = new Dictionary<string, object>();
                            if (jdata.HasValues)
                            {
                                foreach (JProperty property in jdata.Properties())
                                {
                                    if (property.Name == "META_DATA")
                                    {
                                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                        Dictionary<string, object> Mul_tblData;
                                        int recCount = 0;
                                        if (frm["Ops_Multiple"] == "yes")
                                        {
                                            foreach (var key in frm.AllKeys)
                                            {
                                                if (key.StartsWith("MUL-"))
                                                {
                                                    string[] splitArr = frm[key].Split(',');
                                                    recCount = splitArr.Length;
                                                    break;
                                                }
                                            }
                                            for (int i = 0; i < recCount; i++)
                                            {
                                                Mul_tblData = new Dictionary<string, object>();
                                                foreach (var key in frm.AllKeys)
                                                {
                                                    if (key.StartsWith("MUL-"))
                                                    {
                                                        string[] splitArr = frm[key].Split(',');
                                                        if (key.Substring(4) != "ID")
                                                            Mul_tblData.Add(key.Substring(4), splitArr[i]);
                                                        else
                                                            Mul_tblData.Add(key.Substring(4), Convert.ToInt64(splitArr[i]));
                                                    }
                                                }
                                                lstData1.Add(Mul_tblData);
                                            }
                                        }
                                        else
                                        {
                                            #region single operation
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                if (screenType.Equals("insert"))
                                                {
                                                    if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                                        && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                        && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY"
                                                        && Convert.ToString(dt.Rows[i][0]) != "STATUS_YN")
                                                    {
                                                        tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                                    }
                                                }
                                                else if (screenType.Equals("update"))
                                                {
                                                    //if (Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                                    //    && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                    //    && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                                    if (Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                       && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                                    {
                                                        if (frm[dt.Rows[i][0].ToString()] != null)
                                                        {
                                                            tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                                        }
                                                    }
                                                }
                                            }

                                            lstData1.Add(tableData);
                                            #endregion
                                        }
                                        AppUrl = AppUrl + "/AddUpdate";
                                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, tablename, 0, 0, lstData1, conditions));
                                        actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                                        actionClass.columnMetadata = jdata;
                                        string ref_string_id = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)(JObject.Parse(actionClass.DecryptData).First.First.First.First)).Value);
                                        int ref_id = 0;
                                        int.TryParse(ref_string_id, out ref_id);

                                        uploadDocuments(ref_id, screen);

                                        #region upload image on server
                                        string TblName = string.Empty;
                                        if (actionClass.StatMessage.ToLower() == "success" && HttpContext.Current.Request.Files.Count > 0
                                            && actionClass.DecryptData != null)
                                        {
                                            int Id = 0;
                                            if (!string.IsNullOrEmpty(actionClass.DecryptData))
                                            {
                                                JObject res = JObject.Parse(actionClass.DecryptData);
                                                foreach (JProperty jproperty in res.Properties())
                                                {
                                                    if (jproperty.Name != null)
                                                    {
                                                        TblName = jproperty.Name;
                                                        DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                                                        Id = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                                                    }
                                                }
                                            }

                                            string FolderName = string.Empty;
                                            if (TblName == Convert.ToString(TableName.FACULTY_T))
                                                FolderName = Convert.ToString(FilePath.FacultyMaster.GetEnumDisplayName());
                                            else if (TblName == Convert.ToString(TableName.TRAINING_DOC_T))
                                                FolderName = Convert.ToString(FilePath.TrainingDoc.GetEnumDisplayName());
                                            else if (TblName == Convert.ToString(TableName.STUDENT_REGISTER_TRAINING_CERTIFICATE))
                                                FolderName = Convert.ToString(FilePath.Certificate.GetEnumDisplayName());
                                            else if (TblName == Convert.ToString(TableName.TRAINING_SESS_ATTANDACE_T))
                                                FolderName = Convert.ToString(FilePath.ATTENDANCE.GetEnumDisplayName());
                                            else if (TblName == Convert.ToString(TableName.COMPLETION_CERTIFICATE_T))
                                                FolderName = Convert.ToString(FilePath.CompletionCertificates.GetEnumDisplayName());

                                            if (!string.IsNullOrEmpty(FolderName))
                                            {
                                                string _FileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                                                string _PathExt = Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);
                                                _FileName = Id + "_" + _FileName;// + _PathExt;
                                                                                 //string _path = HttpContext.Current.Server.MapPath("..//" + Convert.ToString(frm["FPath"]));
                                                string _path = getDocumentPath(FolderName);
                                                string _FullPath = _path + _FileName;

                                                if (!(Directory.Exists(_path)))
                                                    Directory.CreateDirectory(_path);

                                                if (File.Exists(_FullPath))
                                                {
                                                    var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                                                    string NewFileName = Id + "_" + Timestamp.ToString() + _PathExt;
                                                    File.Move(_FullPath, _path + "//" + NewFileName);
                                                    //File.Delete(_FullPath);
                                                }
                                                HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                                            }
                                        }
                                        #endregion

                                        int UniqueId = 0;
                                        UniqueId = Convert.ToInt32(frm["UniqueId"]);
                                        if (UniqueId == 0)
                                        {
                                            UniqueId = ref_id;
                                        }
                                        #region store email data                                        
                                        if (screen.is_Email_yn)
                                            storeEmailData(actionClass, screen, screenType, AppUrl, Session_Key, UserName, UniqueId);
                                        #endregion

                                        #region store SMS data
                                        if (screen.is_SMS_yn)
                                            storeSMSData(actionClass, screen, screenType, AppUrl, Session_Key, UserName, UniqueId);
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                actionClass.StatMessage = ex.Message;
                actionClass.StatCode = "-1";
            }
            return actionClass;
        }

        public ActionClass searchLicentateApproval(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            long forwardto = 0;
            int userid = 0;

            if (HttpContext.Current.Session["USER_ID"] != null)
            {
                int.TryParse(Convert.ToString(HttpContext.Current.Session["USER_ID"]), out userid);
            }
            UserData uSER_DATA = new UserData();
            uSER_DATA = ((ICSI_WebApp.Util.UserData)HttpContext.Current.Session["USER_DATA"]);
            var ROLE_ID = uSER_DATA.USER_ROLE_T.AsEnumerable().Where(myRow => myRow.Field<long>("USER_ID") == userid).Select(x => x.Field<long>("ROLE_ID")).FirstOrDefault();

            if (frm["SCRH_APPROVAL_FILTER"] != null)
            {
                if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) != "")
                {
                    if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Pending" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Approved" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Rejected")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    }
                    else if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Recommended")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Remove("SCRH_APPROVAL_FILTER");
                        frm.Remove("COND_APPROVAL_FILTER");
                        frm.Add("COND_APPROVAL_FILTER", "AND =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Approval");
                        frm.Add("COND_APPROVAL_FILTER", "OR =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Rejection");
                    }
                    else
                    {
                        forwardto = ROLE_ID;
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                        frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                    }
                }
                else
                {
                    forwardto = ROLE_ID;
                    frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                    frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                    frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                }
            }
            else
            {
                forwardto = ROLE_ID;
                frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                frm.Remove("COND_FORWARD_TO_ROLE_ID");
                frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
            }

            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }


        public ActionClass beforeTCCApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterTCCApproval(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            bool innerresult = true;
            if (frm["ED_ID"] != null)
            {
                for (int i = 0; i < frm["ED_ID"].Split(',').Length; i++)
                {
                    FormCollection FRM1 = new FormCollection();
                    FRM1.Add("ID", Convert.ToString(frm["ED_ID"].Split(',')[i]));
                    FRM1.Add("APPROVE_NM", Convert.ToString(frm["DOC_APPROVE_NM"].Split(',')[i]));
                    FRM1.Add("REMARKS_TX", Convert.ToString(frm["DOC_REMARKS_TX"].Split(',')[i]));
                    FRM1.Add("s", "update");
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("u", frm["u"]);
                    }
                    if (frm["m"] != null && frm["m"] != "")
                    {
                        FRM1.Add("m", frm["m"]);
                    }
                    if (frm["ui"] != null && frm["ui"] != "")
                    {
                        FRM1.Add("ui", frm["ui"]);
                    }
                    if (frm["si"] != null && frm["si"] != "")
                    {
                        FRM1.Add("si", frm["si"]);
                    }
                    if (frm["u"] != null && frm["u"] != "")
                    {
                        FRM1.Add("ACTION_BY", frm["U"]);
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "STUDENT_CLEARANCE_CERTIFICATE_DOCUMENT_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                        break;
                    }
                }
            }
            if (innerresult)
            {
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }

            return act;
        }

        public ActionClass beforeExemptionDocumentEdit(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterExemptionDocumentEdit(int WEB_APP_ID, FormCollection frm)
        {

            if (frm["REMOVE_NM"] == "1")
            {
                frm["s"] = "update";
                frm.Add("ID", frm["REMOVE_ID"]);
                frm.Add("ACTIVE_YN", "0");
                frm.Remove("APPROVE_NM");
                frm.Remove("DOC_TYPE_ID");
                string filePath = frm["PATH_TX"];
                System.IO.File.Delete(filePath);
                frm.Remove("FILE_PATH_TX");
                frm.Remove("FILE_NAME_TX");
            }
            else if (frm["APPROVE_YN"] == "1")
            {
                frm["s"] = "update";
                frm.Add("ID", frm["APPROVE_ID"]);
                frm.Remove("APPROVE_NM");
                frm.Add("APPROVE_NM", frm["APPROVE_NMM"]);
                frm.Remove("DOC_TYPE_ID");
                frm.Remove("FILE_PATH_TX");
                frm.Remove("FILE_NAME_TX");
            }
            else
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;
                FolderName = "Uploads\\Exemption_GEMINI_STAGING\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\Exemption_Id_" + Convert.ToString(frm["EXEMPTION_ID"]) + "\\Document_Type_ID_" + Convert.ToString(frm["DOC_TYPE_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["DOCUMENT_ROOT"]) + FolderName;
                }
                else
                {
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;
                }

                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = System.IO.Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = System.IO.Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);

                    string _path = FolderName;
                    string _FullPath = System.IO.Path.Combine(_path, _FileName);

                    if (!(System.IO.Directory.Exists(_path)))
                        System.IO.Directory.CreateDirectory(_path);

                    chechagain:

                    if (System.IO.File.Exists(_FullPath))
                    {
                        _FileName = "1_" + _FileName;
                        _FullPath = System.IO.Path.Combine(_path, _FileName);
                        goto chechagain;
                    }
                    else
                    {
                        File_name_tx = _FileName;
                        file_path_tx = _FullPath;
                        frm.Add("FILE_NAME_TX", File_name_tx);
                        frm.Add("FILE_PATH_TX", file_path_tx);
                        HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                    }
                }
            }
            ActionClass act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }


        public ActionClass beforeProjectReportAdminApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"].ToString() != string.Empty)
            {
                //HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                frm["STUDENT_REG_ID"] = frm["UNIQUE_REG_ID"];
                int id = Convert.ToInt32(frm["UNIQUE_REG_ID"]);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", id);
                conditions.Add("QID", 55); // for fetching the drop down data                    
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
                DataTable dtt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                    {
                        dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REQUEST_TYPE_ID", Convert.ToString(dtt.Rows[0]["REQUEST_TYPE_ID"]))
                        .Replace("@COMPANY_TYPE_ID", Convert.ToString(dtt.Rows[0]["COMPANY_TYPE_ID"]))
                        .Replace("@COMP_PCS_NAME_TX", Convert.ToString(dtt.Rows[0]["COMP_PCS_NAME_TX"]))
                        .Replace("@CITY_NAME_TX", Convert.ToString(dtt.Rows[0]["CITY_NAME_TX"]))
                        .Replace("@STUDENT_NAME_TX", Convert.ToString(dtt.Rows[0]["STUDENT_NAME_TX"]))
                        .Replace("@TRAINING_DURATION", Convert.ToString(dtt.Rows[0]["TRAINING_DURATION"]))
                        .Replace("@MOBILE_TX", Convert.ToString(dtt.Rows[0]["MOBILE_TX"]))
                        .Replace("@EMAIL_ID", Convert.ToString(dtt.Rows[0]["EMAIL_ID"]))
                        .Replace("@REG_NUMBER_TX", Convert.ToString(dtt.Rows[0]["REG_NUMBER_TX"]));
                    if (dtt.Rows[0]["TRAINING_COMMENCE_DT"] != null)
                    {
                        //DateTime dtime = Convert.ToDateTime(dtt.Rows[0]["TRAINING_COMMENCE_DT"]);
                        //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", dtime.ToString("dd/MM/yyyy"));
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]));
                    }
                }
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                conditions.Clear();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_PROJECT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    //frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    //frm["REMARKS_TX"]
                    frm["s"] = "edit";
                }
                else
                {
                    frm["s"] = "insert";
                    frm["STATUS_TX"] = "";
                }
                //frm["ui"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["ID"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["s"] = "edit";
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterProjectReportAdminApproval(int WEB_APP_ID, FormCollection frm)
        {
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string qids = frm["qids"];
            string[] qidArr = null;
            if (qids != null && qids.Trim() != "")
            {
                qids = qids.Substring(1);
                qidArr = qids.Split(',');
            }
            ActionClass act = null;
            frm["APPROVAL_NM"] = frm["STATUS_TX"];
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
                if (frm["STUDENT_REG_ID"] == null || frm["STUDENT_REG_ID"] == "@STUDENT_REG_ID" || frm["STUDENT_REG_ID"] == "") frm["STUDENT_REG_ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();

            List<Int32> ids = new List<Int32>();
            Dictionary<Int32, Int32> idStatuses = new Dictionary<Int32, Int32>();
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("REF_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                conditionss.Add("ADMIN_STATUS_NM", 1);
                Dictionary<string, string> conditionopss = new Dictionary<string, string>();
                conditionopss.Add("ADMIN_STATUS_NM", "!=");
                DataTable beforeData = UtilService.getData(UtilService.getApplicationScheme(screen), "PROJECT_REPORT_APPROVAL_T", conditionss, conditionopss, 0, 100);
                if (beforeData != null && beforeData.Rows != null && beforeData.Rows.Count > 0)
                {
                    for (int i = 0; i < beforeData.Rows.Count; i++)
                    {
                        ids.Add(Convert.ToInt32(beforeData.Rows[i]["ID"]));
                        idStatuses.Add(Convert.ToInt32(beforeData.Rows[i]["ID"]), Convert.ToInt32(beforeData.Rows[i]["ADMIN_STATUS_NM"]));
                    }
                }
            }

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string approvalStatuses = frm["DOCUMENT_STATUS"];
            string[] apprvArr = null;
            if (approvalStatuses != null && approvalStatuses.Trim().Length > 0)
            {
                apprvArr = approvalStatuses.Split(',');
                foreach (string conds in apprvArr)
                {
                    if (!conds.Trim().Equals(""))
                        UpdateStatusTrainingAdmin(idStatuses, conds, 2, screen);
                }
            }
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("REF_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                DataTable dtt = UtilService.getData(UtilService.getApplicationScheme(screen), "PROJECT_REPORT_APPROVAL_T", conditionss, null, 0, 100);
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    List<Dictionary<string, object>> ldict = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    for (int i = 0; i < dtt.Rows.Count; i++)
                    {
                        Int32 i32 = Convert.ToInt32(dtt.Rows[i]["ID"]);
                        if (ids.Contains(i32))
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["QRTR_ID"] = Convert.ToInt32(dtt.Rows[i]["ID"]);
                            dict["STATUS_TX"] = Convert.ToString(dtt.Rows[i]["STATUS_TX"] == null ? "" : dtt.Rows[i]["STATUS_TX"]);
                            dict["TYPE_ID"] = 2;
                            dict["REMARKS_TX"] = frm["REMARKS_TX"] == null ? "" : frm["REMARKS_TX"];
                            dict["ACTIVE_YN"] = 1;
                            ldict.Add(dict);
                            if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1")
                            {
                                dtt.Rows[i]["ADMIN_STATUS_NM"] = 1;
                                list.Add(dtt.Rows[i].Table.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => dtt.Rows[i][column] // Value
                                ));
                            }
                        }
                    }
                    /*List<Dictionary<string, object>> list =
                                dtt.AsEnumerable().Select(
                                row => dtt.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => row[column] // Value
                                )
                            ).ToList();*/
                    if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1" && list.Count > 0)
                    {
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "PROJECT_REPORT_APPROVAL_T", list);
                    }
                    if (ldict.Count > 0)
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "QUARTERLY_REPORT_HISTORY_T", ldict);
                }
                int id = Convert.ToInt32(frm["STUDENT_REG_ID"]);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_PROJECT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    frm["s"] = "edit";
                }
                frm["nextscreen"] = "202";// Convert.ToString(screen.ID);
            }
            return act;
        }

        public ActionClass beforeCertificateReportAdminApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"].ToString() != string.Empty)
            {
                //HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                frm["STUDENT_REG_ID"] = frm["UNIQUE_REG_ID"];
                int id = Convert.ToInt32(frm["UNIQUE_REG_ID"]);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", id);
                conditions.Add("QID", 55); // for fetching the drop down data                    
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
                DataTable dtt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                    {
                        dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REQUEST_TYPE_ID", Convert.ToString(dtt.Rows[0]["REQUEST_TYPE_ID"]))
                        .Replace("@COMPANY_TYPE_ID", Convert.ToString(dtt.Rows[0]["COMPANY_TYPE_ID"]))
                        .Replace("@COMP_PCS_NAME_TX", Convert.ToString(dtt.Rows[0]["COMP_PCS_NAME_TX"]))
                        .Replace("@CITY_NAME_TX", Convert.ToString(dtt.Rows[0]["CITY_NAME_TX"]))
                        .Replace("@STUDENT_NAME_TX", Convert.ToString(dtt.Rows[0]["STUDENT_NAME_TX"]))
                        .Replace("@TRAINING_DURATION", Convert.ToString(dtt.Rows[0]["TRAINING_DURATION"]))
                        .Replace("@MOBILE_TX", Convert.ToString(dtt.Rows[0]["MOBILE_TX"]))
                        .Replace("@EMAIL_ID", Convert.ToString(dtt.Rows[0]["EMAIL_ID"]))
                        .Replace("@REG_NUMBER_TX", Convert.ToString(dtt.Rows[0]["REG_NUMBER_TX"]));
                    if (dtt.Rows[0]["TRAINING_COMMENCE_DT"] != null)
                    {
                        //DateTime dtime = Convert.ToDateTime(dtt.Rows[0]["TRAINING_COMMENCE_DT"]);
                        //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", dtime.ToString("dd/MM/yyyy"));
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]));
                    }
                }
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                conditions.Clear();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_CERT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    //frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    //frm["REMARKS_TX"]
                    frm["s"] = "edit";
                }
                else
                {
                    frm["s"] = "insert";
                    frm["STATUS_TX"] = "";
                }
                //frm["ui"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["ID"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["s"] = "edit";
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCertificateReportAdminApproval(int WEB_APP_ID, FormCollection frm)
        {
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string qids = frm["qids"];
            string[] qidArr = null;
            if (qids != null && qids.Trim() != "")
            {
                qids = qids.Substring(1);
                qidArr = qids.Split(',');
            }
            ActionClass act = null;
            frm["APPROVAL_NM"] = frm["STATUS_TX"];
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
                if (frm["STUDENT_REG_ID"] == null || frm["STUDENT_REG_ID"] == "@STUDENT_REG_ID" || frm["STUDENT_REG_ID"] == "") frm["STUDENT_REG_ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
            DataTable beforeData = null;
            List<Int32> ids = new List<Int32>();
            Dictionary<Int32, Int32> idStatuses = new Dictionary<Int32, Int32>();
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("REF_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                conditionss.Add("ADMIN_STATUS_NM", 1);
                Dictionary<string, string> conditionopss = new Dictionary<string, string>();
                conditionopss.Add("ADMIN_STATUS_NM", "!=");
                beforeData = UtilService.getData(UtilService.getApplicationScheme(screen), "TRAINNING_CERT_APPROVAL_T", conditionss, conditionopss, 0, 100);

                if (beforeData != null && beforeData.Rows != null && beforeData.Rows.Count > 0)
                {
                    for (int i = 0; i < beforeData.Rows.Count; i++)
                    {
                        ids.Add(Convert.ToInt32(beforeData.Rows[i]["ID"]));
                        idStatuses.Add(Convert.ToInt32(beforeData.Rows[i]["ID"]), Convert.ToInt32(beforeData.Rows[i]["ADMIN_STATUS_NM"]));
                    }
                }
            }
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string approvalStatuses = frm["DOCUMENT_STATUS"];
            string[] apprvArr = null;
            if (approvalStatuses != null && approvalStatuses.Trim().Length > 0)
            {
                apprvArr = approvalStatuses.Split(',');
                foreach (string conds in apprvArr)
                {
                    if (!conds.Trim().Equals(""))
                        UpdateStatusTrainingAdmin(idStatuses, conds, 3, screen);
                }
            }
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("REF_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                DataTable dtt = UtilService.getData(UtilService.getApplicationScheme(screen), "TRAINNING_CERT_APPROVAL_T", conditionss, null, 0, 100);
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    List<Dictionary<string, object>> ldict = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    for (int i = 0; i < dtt.Rows.Count; i++)
                    {
                        Int32 i32 = Convert.ToInt32(dtt.Rows[i]["ID"]);
                        if (ids.Contains(i32))
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["QRTR_ID"] = Convert.ToInt32(dtt.Rows[i]["ID"]);
                            dict["STATUS_TX"] = Convert.ToString(dtt.Rows[i]["STATUS_TX"] == null ? "" : dtt.Rows[i]["STATUS_TX"]);
                            dict["TYPE_ID"] = 3;
                            dict["REMARKS_TX"] = frm["REMARKS_TX"] == null ? "" : frm["REMARKS_TX"];
                            dict["ACTIVE_YN"] = 1;
                            ldict.Add(dict);
                            if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1")
                            {
                                dtt.Rows[i]["ADMIN_STATUS_NM"] = 1;
                                list.Add(dtt.Rows[i].Table.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => dtt.Rows[i][column] // Value
                                ));
                            }
                        }
                    }
                    /*List<Dictionary<string, object>> list =
                                dtt.AsEnumerable().Select(
                                row => dtt.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => row[column] // Value
                                )
                            ).ToList();*/
                    if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1" && list.Count > 0)
                    {
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "TRAINNING_CERT_APPROVAL_T", list);
                    }
                    if (ldict.Count > 0)
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "QUARTERLY_REPORT_HISTORY_T", ldict);
                }
                int id = Convert.ToInt32(frm["STUDENT_REG_ID"]);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_CERT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    frm["s"] = "edit";
                }
                frm["nextscreen"] = "203";// Convert.ToString(screen.ID);
            }
            return act;
        }

        public ActionClass beforeReportAdminApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"].ToString() != string.Empty)
            {
                //HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                frm["STUDENT_REG_ID"] = frm["UNIQUE_REG_ID"];
                int id = Convert.ToInt32(frm["UNIQUE_REG_ID"]);

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", id);
                conditions.Add("QID", 55); // for fetching the drop down data                    
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
                DataTable dtt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                    {
                        dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REQUEST_TYPE_ID", Convert.ToString(dtt.Rows[0]["REQUEST_TYPE_ID"]))
                        .Replace("@COMPANY_TYPE_ID", Convert.ToString(dtt.Rows[0]["COMPANY_TYPE_ID"]))
                        .Replace("@COMP_PCS_NAME_TX", Convert.ToString(dtt.Rows[0]["COMP_PCS_NAME_TX"]))
                        .Replace("@CITY_NAME_TX", Convert.ToString(dtt.Rows[0]["CITY_NAME_TX"]))
                        .Replace("@STUDENT_NAME_TX", Convert.ToString(dtt.Rows[0]["STUDENT_NAME_TX"]))
                        .Replace("@TRAINING_DURATION", Convert.ToString(dtt.Rows[0]["TRAINING_DURATION"]))
                        .Replace("@MOBILE_TX", Convert.ToString(dtt.Rows[0]["MOBILE_TX"]))
                        .Replace("@EMAIL_ID", Convert.ToString(dtt.Rows[0]["EMAIL_ID"]))
                        .Replace("@REG_NUMBER_TX", Convert.ToString(dtt.Rows[0]["REG_NUMBER_TX"]));
                    if (dtt.Rows[0]["TRAINING_COMMENCE_DT"] != null)
                    {
                        //DateTime dtime = Convert.ToDateTime(dtt.Rows[0]["TRAINING_COMMENCE_DT"]);
                        //screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", dtime.ToString("dd/MM/yyyy"));
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@COMMENCE_DT", Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]));
                    }
                }
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                conditions.Clear();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    //frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    //frm["REMARKS_TX"]
                    frm["s"] = "edit";
                }
                else
                {
                    frm["s"] = "insert";
                    frm["STATUS_TX"] = "";
                }
                //frm["ui"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["ID"] = frm["UNIQUE_REG_ID"].ToString();
                //frm["s"] = "edit";
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterReportAdminApproval(int WEB_APP_ID, FormCollection frm)
        {
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            //frm["s"] = "update";
            string qids = frm["qids"];
            string[] qidArr = null;
            if (qids != null && qids.Trim() != "")
            {
                qids = qids.Substring(1);
                qidArr = qids.Split(',');
            }

            ActionClass act = null;
            frm["APPROVAL_NM"] = frm["STATUS_TX"];
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
                if (frm["STUDENT_REG_ID"] == null || frm["STUDENT_REG_ID"] == "@STUDENT_REG_ID" || frm["STUDENT_REG_ID"] == "") frm["STUDENT_REG_ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
            DataTable beforeData = null;
            List<Int32> ids = new List<Int32>();
            Dictionary<Int32, Int32> idStatuses = new Dictionary<Int32, Int32>();
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("TRAINING_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                conditionss.Add("ADMIN_STATUS_NM", 1);
                Dictionary<string, string> conditionopss = new Dictionary<string, string>();
                conditionopss.Add("ADMIN_STATUS_NM", "!=");
                beforeData = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_QUATERLY_REPORT_T", conditionss, conditionopss, 0, 100);
                if (beforeData != null && beforeData.Rows != null && beforeData.Rows.Count > 0)
                {
                    for (int i = 0; i < beforeData.Rows.Count; i++)
                    {
                        Int32 i32 = Convert.ToInt32(beforeData.Rows[i]["ID"]);
                        if (!ids.Contains(i32))
                        {
                            ids.Add(i32);
                            idStatuses.Add(Convert.ToInt32(beforeData.Rows[i]["ID"]), Convert.ToInt32(beforeData.Rows[i]["ADMIN_STATUS_NM"]));
                        }
                    }
                }
            }
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string approvalStatuses = frm["DOCUMENT_STATUS"];
            string[] apprvArr = null;
            if (approvalStatuses != null && approvalStatuses.Trim().Length > 0)
            {
                apprvArr = approvalStatuses.Split(',');
                foreach (string conds in apprvArr)
                {
                    if (!conds.Trim().Equals(""))
                        UpdateStatusTrainingAdmin(idStatuses, conds, 1, screen);
                }
            }
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("TRAINING_ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                DataTable dtt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_QUATERLY_REPORT_T", conditionss, null, 0, 100);
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    List<Dictionary<string, object>> ldict = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    for (int i = 0; i < dtt.Rows.Count; i++)
                    {
                        Int32 i32 = Convert.ToInt32(dtt.Rows[i]["ID"]);
                        if (ids.Contains(i32))
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["QRTR_ID"] = i32;
                            dict["STATUS_TX"] = Convert.ToString(dtt.Rows[i]["STATUS_TX"] == null ? "" : dtt.Rows[i]["STATUS_TX"]);
                            dict["TYPE_ID"] = 1;
                            dict["REMARKS_TX"] = frm["REMARKS_TX"] == null ? "" : frm["REMARKS_TX"];
                            dict["ACTIVE_YN"] = 1;
                            ldict.Add(dict);
                            if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1")
                            {
                                dtt.Rows[i]["ADMIN_STATUS_NM"] = 1;
                                list.Add(dtt.Rows[i].Table.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => dtt.Rows[i][column] // Value
                                ));
                            }
                        }
                    }

                    /*List<Dictionary<string, object>> list =
                                dtt.AsEnumerable().Select(
                                row => dtt.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => row[column] // Value
                                )
                            ).ToList();*/
                    if (frm["APPROVAL_NM"] != null && frm["APPROVAL_NM"] == "1" && list.Count > 0)
                    {
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "STUDENT_QUATERLY_REPORT_T", list);
                    }
                    if (ldict.Count > 0)
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "QUARTERLY_REPORT_HISTORY_T", ldict);
                }
                int id = Convert.ToInt32(frm["STUDENT_REG_ID"]);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("STUDENT_REG_ID", id);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_ADMIN_APPROVAL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    frm["ui"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["ID"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["ID"]));
                    frm["STATUS_TX"] = Convert.ToString(Convert.ToInt32(dt.Rows[0]["APPROVAL_NM"]));
                    frm["s"] = "edit";
                }
                frm["nextscreen"] = "60";// Convert.ToString(screen.ID);
            }
            return act;
        }

        public ActionClass searchExemptionApproval(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            int level = 1;
            int forwardto = 0;
            int userid = 0;

            if (HttpContext.Current.Session["USER_ID"] != null)
            {
                int.TryParse(Convert.ToString(HttpContext.Current.Session["USER_ID"]), out userid);
            }

            if (Util.DBTable.USER_APPROVAL_CONFIG_T.Rows.Count > 0)
            {
                string strlevel = string.Empty;
                var uapproveconfig = Util.DBTable.USER_APPROVAL_CONFIG_T.AsEnumerable().Where(myRow => myRow.Field<long>("USER_ID") == userid && myRow.Field<long>("SCREEN_ID") == screen.ID).Select(x => x.Field<long>("LEVEL_NM")).FirstOrDefault();
                strlevel = Convert.ToString(uapproveconfig);
                int.TryParse(strlevel, out level);
            }
            //if (level == 1 || level == 0)
            //{
            //    forwardto = 0;
            //}
            //else
            {
                forwardto = userid;
            }
            frm.Remove("SCRH_FORWARD_TO");
            frm.Add("COND_FORWARD_TO", "AND =");
            frm.Add("SCRH_FORWARD_TO", Convert.ToString(forwardto));

            if (frm["SCRH_STATUS"] != null)
            {
                if (Convert.ToString(frm["SCRH_STATUS"]) != "")
                {
                    if (Convert.ToString(frm["SCRH_STATUS"]) == "AllForwarded")
                    {
                        frm.Remove("SCRH_FORWARD_TO");
                        frm.Remove("COND_FORWARD_TO");
                    }
                }
            }
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }
        public ActionClass searchClearanceUpperLevel(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            int level = 1;
            int forwardto = 0;
            int userid = 0;

            if (HttpContext.Current.Session["USER_ID"] != null)
            {
                int.TryParse(Convert.ToString(HttpContext.Current.Session["USER_ID"]), out userid);
            }
            if (Util.DBTable.USER_APPROVAL_CONFIG_T.Rows.Count > 0)
            {
                string strlevel = string.Empty;
                var uapproveconfig = Util.DBTable.USER_APPROVAL_CONFIG_T.AsEnumerable().Where(myRow => myRow.Field<long>("USER_ID") == userid && myRow.Field<long>("SCREEN_ID") == screen.ID).Select(x => x.Field<long>("LEVEL_NM")).FirstOrDefault();
                strlevel = Convert.ToString(uapproveconfig);
                int.TryParse(strlevel, out level);
            }
            frm.Remove("SCRH_LEVELS");
            frm.Remove("COND_LEVELS");
            frm.Add("COND_LEVELS", "AND =");
            frm.Add("SCRH_LEVELS", Convert.ToString(level));

            if (frm["SCRH_APPROVAL_FILTER"] != null)
            {
                if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) != "")
                {
                    if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Forwarded")
                    {
                        forwardto = userid;
                        frm.Remove("SCRH_FORWARD_TO");
                        frm.Remove("COND_FORWARD_TO");
                        frm.Add("COND_FORWARD_TO", "AND <>");
                        frm.Add("SCRH_FORWARD_TO", Convert.ToString(forwardto));
                    }
                    if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "All")
                    {
                        frm.Remove("SCRH_FORWARD_TO");
                        frm.Remove("COND_FORWARD_TO");
                        frm.Remove("SCRH_APPROVAL_FILTER");
                        frm.Remove("COND_APPROVAL_FILTER");
                    }
                }
                else
                {
                    forwardto = userid;
                    frm.Remove("SCRH_FORWARD_TO");
                    frm.Remove("COND_FORWARD_TO");
                    frm.Remove("COND_APPROVAL_FILTER");
                    frm.Remove("SCRH_APPROVAL_FILTER");
                    frm.Add("COND_FORWARD_TO", "AND =");
                    frm.Add("SCRH_FORWARD_TO", Convert.ToString(forwardto));
                    frm.Add("COND_APPROVAL_FILTER", "AND =");
                    frm.Add("SCRH_APPROVAL_FILTER", "Forwarded");
                }
            }
            else
            {
                forwardto = userid;
                frm.Remove("SCRH_FORWARD_TO");
                frm.Remove("COND_FORWARD_TO");
                frm.Remove("COND_APPROVAL_FILTER");
                frm.Remove("SCRH_APPROVAL_FILTER");
                frm.Add("COND_FORWARD_TO", "AND =");
                frm.Add("SCRH_FORWARD_TO", Convert.ToString(forwardto));
                frm.Add("COND_APPROVAL_FILTER", "AND =");
                frm.Add("SCRH_APPROVAL_FILTER", "Forwarded");
            }

            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }

        public ActionClass beforeApproveLTTrainingInnerScr(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.LOAD)).FirstOrDefault();
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, s.Id.ToString(), screen.ID.ToString());
        }

        public ActionClass afterApproveLTTrainingInnerScr(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            #region Old Email Code
            //string StudentID = string.Empty;
            //DataTable dt = new DataTable();
            //Dictionary<string, object> conditions = new Dictionary<string, object>();
            //conditions.Add("ACTIVE_YN", 1);
            //conditions.Add("ID", frm["STUDENT_REG_LONGTERM_ID"]);
            //dt = UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_LONGTERM_T", conditions, null, 0, 1);
            //if (dt.Rows.Count > 0)
            //{
            //    StudentID = dt.Rows[0]["STUDENT_ID"].ToString();
            //}
            //if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty)
            //{
            //    Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
            //}
            #endregion

            return actionClass;
        }

        public ActionClass beforeApproveLTExcem(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.LOAD)).FirstOrDefault();
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, s.Id.ToString(), screen.ID.ToString());
        }

        public ActionClass afterApproveLTExcem(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            #region Old Email Code
            //string StudentID = string.Empty;
            //DataTable dt = new DataTable();
            //Dictionary<string, object> conditions = new Dictionary<string, object>();
            //conditions.Add("ACTIVE_YN", 1);
            //conditions.Add("REG_NUMBER_TX", frm["REG_NUMBER_TX"]);
            //dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);
            //if (dt.Rows.Count > 0)
            //{
            //    StudentID = dt.Rows[0]["ID"].ToString();
            //}
            //if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty)
            //{
            //    Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
            //}
            #endregion

            return actionClass;
        }

        public void UpdateStatusTrainingAdmin(Dictionary<Int32, Int32> beforeCommitIdStatuses, string condition, int ptype, Screen_T screen)
        {
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();
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

            if ((ptype == 1 && tableName.Equals("STUDENT_QUATERLY_REPORT_T"))
                || (ptype == 2 && tableName.Equals("PROJECT_REPORT_APPROVAL_T"))
                || (ptype == 3 && tableName.Equals("TRAINNING_CERT_APPROVAL_T")))
            {
                int UID = Convert.ToInt32(condition.Split('_')[0].ToString());
                string uniqueRegID = tableName + "$" + Trn_ID;
                string status = condition.Split('_')[3].ToString().Trim(' ');
                ActionClass actionClass = new ActionClass();
                conditions.Add("ACTIVE_YN", 1);
                //conditions.Add("TRAINING_ID", 1);
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();

                data.Add("ID", condition.Split('_')[0].ToString());
                data.Add("STATUS_TX", status);
                int approveNm = 0;
                int previousStatus = beforeCommitIdStatuses[UID];
                if (status.Trim().Equals("Approve"))
                {
                    data.Add("ADMIN_STATUS_NM", 1);
                    data.Add("APPROVE_NM", 1);
                    approveNm = 1;
                }
                else if (status.Trim().Equals("Call"))
                {
                    if (previousStatus != 1) data.Add("ADMIN_STATUS_NM", 3);
                    data.Add("APPROVE_NM", 2);
                    approveNm = 3;
                }
                else if (status.Trim().Equals("Reject"))
                {
                    if (previousStatus != 1) data.Add("ADMIN_STATUS_NM", 2);
                    data.Add("APPROVE_NM", 3);
                    approveNm = 2;
                }

                data.Add("ADMIN_ACTION_DT", DateTime.Now.ToString("MM/dd/yyyy"));
                data.Add("UPDATED_BY", UserName);
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", tableName, 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                condition = condition.Split('_')[0].ToString();
                if (uniqueRegID != string.Empty)
                {
                    if (HttpContext.Current.Session["SESSION_OBJ"] != null)
                    {
                        Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJ"];
                        object sessionobjvalues;
                        if (sessionObjs.TryGetValue(uniqueRegID, out sessionobjvalues))
                        {
                            sessionobjvalues = (sessionobjvalues.ToString().Contains(condition.ToString())) ? sessionobjvalues.ToString().Replace(condition.ToString(), "") : sessionobjvalues;
                            condition += "," + sessionobjvalues.ToString();
                        }
                        sessionObjs.Remove(uniqueRegID);
                        sessionObjs.Add(uniqueRegID, condition);
                        HttpContext.Current.Session["SESSION_OBJ"] = sessionObjs;
                    }

                    else
                    {
                        Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                        sessionObjs.Add(uniqueRegID, condition.Split('_')[0].ToString());
                        HttpContext.Current.Session["SESSION_OBJ"] = sessionObjs;
                    }
                }

                if (previousStatus != approveNm)
                {
                    #region For Email
                    if (screen.is_Email_yn)
                        storeEmailData(actionClass, screen, "update", AppUrl, Session_Key, UserName, UID);
                    #endregion
                }
            }
        }

        public ActionClass beforeEditCompletionCertificate(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterEditCompletionCertificate(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();

            #region upload image on server
            string TblName = "COMPLETION_CERTIFICATE_T";
            if (HttpContext.Current.Request != null && HttpContext.Current.Request.Files != null
                && HttpContext.Current.Request.Files.Count > 0)
            {
                int Id = Convert.ToInt32(frm["REF_ID"]);
                frm["FILE_NAME_TX"] = HttpContext.Current.Request.Files[0].FileName;

                string FolderName = string.Empty;
                if (TblName == Convert.ToString(TableName.COMPLETION_CERTIFICATE_T))
                    FolderName = Convert.ToString(FilePath.CompletionCertificates.GetEnumDisplayName());

                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);
                    _FileName = Id + "_" + _FileName;
                    string _path = getDocumentPath(FolderName);
                    string _FullPath = _path + _FileName;

                    if (!(Directory.Exists(_path)))
                        Directory.CreateDirectory(_path);

                    if (File.Exists(_FullPath))
                    {
                        var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        string NewFileName = Id + "_" + Timestamp.ToString() + _PathExt;
                        File.Move(_FullPath, _path + "//" + NewFileName);
                        //File.Delete(_FullPath);

                    }
                    HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                }
            }
            #endregion

            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            return actionClass;
        }

        public ActionClass beforeSECApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterSECApproval(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            bool innerresult = true;
            if (frm["u"] != null && frm["u"] != "")
            {
                frm.Add("ACTION_BY", frm["U"]);
            }
            for (int i = 0; i < frm["ED_ID"].Split(',').Length; i++)
            {
                FormCollection FRM1 = new FormCollection();
                FRM1.Add("ID", Convert.ToString(frm["ED_ID"].Split(',')[i]));
                FRM1.Add("APPROVE_NM", Convert.ToString(frm["DOC_APPROVE_NM"].Split(',')[i]));
                FRM1.Add("s", "update");
                if (frm["u"] != null && frm["u"] != "")
                {
                    FRM1.Add("u", frm["u"]);
                }
                if (frm["m"] != null && frm["m"] != "")
                {
                    FRM1.Add("m", frm["m"]);
                }
                if (frm["ui"] != null && frm["ui"] != "")
                {
                    FRM1.Add("ui", frm["ui"]);
                }
                if (frm["si"] != null && frm["si"] != "")
                {
                    FRM1.Add("si", frm["si"]);
                }
                if (frm["u"] != null && frm["u"] != "")
                {
                    FRM1.Add("ACTION_BY", frm["U"]);
                }
                ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_SEC_REG_EDUCATION_DTL_T");
                if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                {
                    innerresult = true;
                }
                else
                {
                    act = act1;
                    innerresult = false;
                    break;
                }
            }
            if (innerresult)
            {
                FormCollection FRM1 = new FormCollection();
                FRM1.Add("s", "update");
                if (frm["u"] != null && frm["u"] != "")
                {
                    FRM1.Add("u", frm["u"]);
                }
                if (frm["m"] != null && frm["m"] != "")
                {
                    FRM1.Add("m", frm["m"]);
                }
                if (frm["ui"] != null && frm["ui"] != "")
                {
                    FRM1.Add("ui", frm["ui"]);
                }
                if (frm["si"] != null && frm["si"] != "")
                {
                    FRM1.Add("si", frm["si"]);
                }
                if (frm["REG_ID"] != null && frm["REG_ID"] != "")
                {
                    FRM1.Add("REG_ID", frm["REG_ID"]);
                }
                if (frm["VALID_DT"] != null && frm["VALID_DT"] != "")
                {
                    FRM1.Add("VALID_DT", frm["VALID_DT"]);
                }
                if (frm["DateFormat"] != null && frm["DateFormat"] != "")
                {
                    FRM1.Add("DateFormat", frm["DateFormat"]);
                }
                if (frm["EXPIRY_DT"] != null && frm["EXPIRY_DT"] != "")
                {
                    FRM1.Add("EXPIRY_DT", frm["EXPIRY_DT"]);
                }
                if (frm["APPROVED_DT"] != null && frm["APPROVED_DT"] != "")
                {
                    FRM1.Add("APPROVED_DT", frm["APPROVED_DT"]);
                }
                FRM1.Add("VALID_YN", "1");
                FRM1.Add("EXPIRED_YN", "0");
                if (frm["APPROVE_NM"] == "1")
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_SEC_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                    }
                    else
                    {
                        FRM1["s"] = "insert";
                    }
                    ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_SEC_REGISTERED_STUDENT_T");
                    if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                    {
                        innerresult = true;
                    }
                    else
                    {
                        act = act1;
                        innerresult = false;
                    }
                }
                else
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Clear();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("REG_ID", frm["REG_ID"]);
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_SEC_REGISTERED_STUDENT_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        FRM1["s"] = "update";
                        FRM1.Add("ACTIVE_YN", "0");
                        ActionClass act1 = afterSubmitforBL(WEB_APP_ID, FRM1, "MEM_SEC_REGISTERED_STUDENT_T");
                        if (act1.StatCode == "0" && act1.StatMessage.ToLower() == "success")
                        {
                            innerresult = true;
                        }
                        else
                        {
                            act = act1;
                            innerresult = false;
                        }
                    }
                }

                if (innerresult)
                {
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
            }
            return act;
        }

        public ActionClass beforeACSMemberApproval(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterACSMemberApproval(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            if (frm["u"] != null && frm["u"] != "")
            {
                frm.Add("ACTION_BY", frm["U"]);
                frm.Add("ADMIN_ACTION_BY", frm["U"]);
            }
            frm.Add("ACTION_DT", DateTime.Now.ToString());
            frm.Add("ADMIN_ACTION_DT", DateTime.Now.ToString());

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            if (act.StatCode == "0" && act.StatMessage.ToLower() == "success")
            {
                string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                string applicationSchema = Util.UtilService.getApplicationScheme(screen);

                Dictionary<string, object> Mul_tblData;
                List<Dictionary<string, object>> lstData;
                List<Dictionary<string, object>> lstData1;
                Dictionary<string, object> conditions = new Dictionary<string, object>();

                #region Update in MEM_ACS_REG_DOCUMENTS_T table
                if (!string.IsNullOrEmpty(frm["FIT_ID"]))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();

                    string[] splitsIDs = frm["FIT_ID"].Split(',');
                    string[] splitApproveNM = frm["FIT_APPROVE_NM"].Split(',');
                    for (int i = 0; i < splitsIDs.Length; i++)
                    {
                        Mul_tblData = new Dictionary<string, object>();
                        int ID = Convert.ToInt32(splitsIDs[i]);
                        int APPROVE_NM = Convert.ToInt32(splitApproveNM[i]);

                        Mul_tblData.Add("ID", ID);
                        Mul_tblData.Add("ADMIN_ACTION_BY", frm["ADMIN_ACTION_BY"]);
                        Mul_tblData.Add("ADMIN_ACTION_DT", frm["ADMIN_ACTION_DT"]);
                        Mul_tblData.Add("ADMIN_APPROVE_NM", APPROVE_NM);
                        lstData1.Add(Mul_tblData);
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "MEM_ACS_REG_DOCUMENTS_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    }
                }
                #endregion

                #region Update in MEM_ACS_REG_EDUCATION_DTL_T table
                if (!string.IsNullOrEmpty(frm["EDU_ID"]))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();

                    string[] splitsIDs = frm["EDU_ID"].Split(',');
                    string[] splitApproveNM = frm["EDU_APPROVE_NM"].Split(',');
                    for (int i = 0; i < splitsIDs.Length; i++)
                    {
                        Mul_tblData = new Dictionary<string, object>();
                        int ID = Convert.ToInt32(splitsIDs[i]);
                        int APPROVE_NM = Convert.ToInt32(splitApproveNM[i]);

                        Mul_tblData.Add("ID", ID);
                        Mul_tblData.Add("ACTION_BY", frm["ACTION_BY"]);
                        Mul_tblData.Add("ACTION_DT", frm["ACTION_DT"]);
                        Mul_tblData.Add("APPROVE_NM", APPROVE_NM);
                        lstData1.Add(Mul_tblData);
                    }

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "MEM_ACS_REG_EDUCATION_DTL_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    }
                }
                #endregion

                #region Insert in MEM_ACS_FORWARDING_HISTORY_T table
                if (!string.IsNullOrEmpty(frm["EDU_ID"]))
                {
                    lstData = new List<Dictionary<string, object>>();
                    lstData1 = new List<Dictionary<string, object>>();
                    Mul_tblData = new Dictionary<string, object>();

                    Mul_tblData.Add("REG_ID", frm["ID"]);
                    Mul_tblData.Add("FORWARD_TO", frm["FORWARD_TO_ROLE_ID"]);
                    Mul_tblData.Add("APPROVAL_REMARKS_TX", frm["APPROVAL_REMARKS_TX"]);
                    Mul_tblData.Add("INTERNAL_REMARKS_TX", frm["INTERNAL_REMARKS_TX"]);
                    Mul_tblData.Add("REQUEST_TYPE_TX", "ACS Admissionin");
                    Mul_tblData.Add("SCREEN_ID", frm["si"]);
                    Mul_tblData.Add("APPROVE_NM", frm["APPROVE_NM"]);
                    lstData1.Add(Mul_tblData);

                    if (lstData1.Count > 0)
                    {
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "MEM_ACS_FORWARDING_HISTORY_T", 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    }
                }
                #endregion
            }
            return act;
        }

        public ActionClass searchACSMemberApproval(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            long forwardto = 0;
            int userid = 0;

            if (HttpContext.Current.Session["USER_ID"] != null)
            {
                int.TryParse(Convert.ToString(HttpContext.Current.Session["USER_ID"]), out userid);
            }
            UserData uSER_DATA = new UserData();
            uSER_DATA = ((ICSI_WebApp.Util.UserData)HttpContext.Current.Session["USER_DATA"]);
            var ROLE_ID = uSER_DATA.USER_ROLE_T.AsEnumerable().Where(myRow => myRow.Field<long>("USER_ID") == userid).Select(x => x.Field<long>("ROLE_ID")).FirstOrDefault();

            if (frm["SCRH_APPROVAL_FILTER"] != null)
            {
                if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) != "")
                {
                    if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Pending" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Approved" || Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Rejected")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    }
                    else if (Convert.ToString(frm["SCRH_APPROVAL_FILTER"]) == "Recommended")
                    {
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Remove("SCRH_APPROVAL_FILTER");
                        frm.Remove("COND_APPROVAL_FILTER");
                        frm.Add("COND_APPROVAL_FILTER", "AND =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Approval");
                        frm.Add("COND_APPROVAL_FILTER", "OR =");
                        frm.Add("SCRH_APPROVAL_FILTER", "Recommended for Rejection");
                    }
                    else
                    {
                        forwardto = ROLE_ID;
                        frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                        frm.Remove("COND_FORWARD_TO_ROLE_ID");
                        frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                        frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                    }
                }
                else
                {
                    forwardto = ROLE_ID;
                    frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                    frm.Remove("COND_FORWARD_TO_ROLE_ID");
                    frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                    frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
                }
            }
            else
            {
                forwardto = ROLE_ID;
                frm.Remove("SCRH_FORWARD_TO_ROLE_ID");
                frm.Remove("COND_FORWARD_TO_ROLE_ID");
                frm.Add("COND_FORWARD_TO_ROLE_ID", "AND =");
                frm.Add("SCRH_FORWARD_TO_ROLE_ID", Convert.ToString(forwardto));
            }

            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }

        public ActionClass beforeUploadCertificateDispatch(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterUploadCertificateDispatch(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();

            if (HttpContext.Current.Request.Files[0].ContentLength > 0)
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;

                FolderName = "MEMBERSHIP\\ACS\\UPLOADS\\CERTIFICATE_DISPATCHED";
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                else
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = System.IO.Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = System.IO.Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);

                    string _path = FolderName;
                    string _FullPath = System.IO.Path.Combine(_path, _FileName);

                    if (!(System.IO.Directory.Exists(_path)))
                        System.IO.Directory.CreateDirectory(_path);

                    chechagain:

                    if (System.IO.File.Exists(_FullPath))
                    {
                        _FileName = "1_" + _FileName;
                        _FullPath = System.IO.Path.Combine(_path, _FileName);
                        goto chechagain;
                    }
                    else
                    {
                        File_name_tx = _FileName;
                        file_path_tx = _FullPath;
                        frm.Add("FILE_NAME_TX", File_name_tx);
                        frm.Add("FILE_PATH_TX", file_path_tx);
                        HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                    }

                    DataTable dt = Util.UtilService.ConvertXSLXtoDataTable();

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                        string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                        string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                        string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                        List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                        string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                        Dictionary<string, object> conditions;
                        
                        Dictionary<string, object> Mul_tblData;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            conditions = new Dictionary<string, object>();
                            conditions.Add("QID", 194);
                            conditions.Add("CERT_NO_TX", Convert.ToString(dt.Rows[i]["CERTIFICATE_NO"]));
                            conditions.Add("MEMBER_NO_TX", Convert.ToString(dt.Rows[i]["MEMBERSHIP_NO"]));
                            conditions.Add("COUR_NAME_TX", Convert.ToString(dt.Rows[i]["COURIER_NAME"]));
                            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
                            DataTable dtt = new DataTable();
                            foreach (JProperty property in jdata.Properties())
                            {
                                if (property.Name == "qfetch")
                                {
                                    dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                }
                            }

                            int MemberId = 0;
                            int CertId = 0;
                            int CourierId = 0;
                            int DelModeId = 0;
                            int DelStatusId = 0;

                            if (dtt != null && dtt.Rows != null)
                            {
                                MemberId = Convert.ToInt32(dtt.Rows[0]["MEMBERSHIP_NO"]);
                                CertId = Convert.ToInt32(dtt.Rows[0]["CERTIFICATE_NO"]);
                                CourierId = Convert.ToInt32(dtt.Rows[0]["COURIER_ID"]);

                                if (Convert.ToString(dt.Rows[i]["DELIVERY_MODE"]) == "Courier")
                                    DelModeId = 1;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_MODE"]) == "Convacation")
                                    DelModeId = 2;

                                if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "Delivered")
                                    DelStatusId = 1;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "Not Delivered")
                                    DelStatusId = 2;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "In Progress")
                                    DelStatusId = 3;

                                Mul_tblData = new Dictionary<string, object>();
                                Mul_tblData.Add("ID", 0);
                                Mul_tblData.Add("MEMBER_ID", MemberId);
                                Mul_tblData.Add("CERTIFICATE_ID", CertId);
                                Mul_tblData.Add("PURPOSE_TX", "ACS");
                                Mul_tblData.Add("DELIVERY_MODE_ID", DelModeId);
                                Mul_tblData.Add("DISPATCH_DT", Convert.ToDateTime(dt.Rows[i]["DISPATCH_DT"]));
                                Mul_tblData.Add("COURIER_ID", CourierId);
                                Mul_tblData.Add("DOCKE_NM_TX", Convert.ToString(dt.Rows[i]["DOCKER_UNIQUE_NO"]));
                                Mul_tblData.Add("DELIVERY_STATUS_NM", DelStatusId);
                                Mul_tblData.Add("DELIVERY_DT", Convert.ToDateTime(dt.Rows[i]["DELIVERY_DT"]));
                                lstData1.Add(Mul_tblData);
                            }
                        }

                        conditions = new Dictionary<string, object>();
                        if (lstData1.Count > 0)
                            AppUrl = AppUrl + "/AddUpdate";
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    }
                }
            }

            return act;
        }

        public ActionClass beforeUploadIDCardDispatch(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterUploadIDCardDispatch(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();

            if (HttpContext.Current.Request.Files[0].ContentLength > 0)
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;

                FolderName = "MEMBERSHIP\\ACS\\UPLOADS\\ID_CARD_DISPATCHED";
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                else
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = System.IO.Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = System.IO.Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);

                    string _path = FolderName;
                    string _FullPath = System.IO.Path.Combine(_path, _FileName);

                    if (!(System.IO.Directory.Exists(_path)))
                        System.IO.Directory.CreateDirectory(_path);

                    chechagain:

                    if (System.IO.File.Exists(_FullPath))
                    {
                        _FileName = "1_" + _FileName;
                        _FullPath = System.IO.Path.Combine(_path, _FileName);
                        goto chechagain;
                    }
                    else
                    {
                        File_name_tx = _FileName;
                        file_path_tx = _FullPath;
                        frm.Add("FILE_NAME_TX", File_name_tx);
                        frm.Add("FILE_PATH_TX", file_path_tx);
                        HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                    }

                    DataTable dt = Util.UtilService.ConvertXSLXtoDataTable();

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                        string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                        string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                        string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                        List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                        string applicationSchema = Util.UtilService.getApplicationScheme(screen);
                        Dictionary<string, object> conditions;

                        Dictionary<string, object> Mul_tblData;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            conditions = new Dictionary<string, object>();
                            conditions.Add("QID", 194);
                            conditions.Add("CERT_NO_TX", 0);
                            conditions.Add("MEMBER_NO_TX", Convert.ToString(dt.Rows[i]["MEMBERSHIP_NO"]));
                            conditions.Add("COUR_NAME_TX", Convert.ToString(dt.Rows[i]["COURIER_NAME"]));
                            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Training");
                            DataTable dtt = new DataTable();
                            foreach (JProperty property in jdata.Properties())
                            {
                                if (property.Name == "qfetch")
                                {
                                    dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                }
                            }

                            int MemberId = 0;
                            int CourierId = 0;
                            int DelModeId = 0;
                            int DelStatusId = 0;

                            if (dtt != null && dtt.Rows != null)
                            {
                                MemberId = Convert.ToInt32(dtt.Rows[0]["MEMBERSHIP_NO"]);
                                CourierId = Convert.ToInt32(dtt.Rows[0]["COURIER_ID"]);

                                if (Convert.ToString(dt.Rows[i]["DELIVERY_MODE"]) == "Courier")
                                    DelModeId = 1;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_MODE"]) == "Convacation")
                                    DelModeId = 2;

                                if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "Delivered")
                                    DelStatusId = 1;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "Not Delivered")
                                    DelStatusId = 2;
                                else if (Convert.ToString(dt.Rows[i]["DELIVERY_STATUS"]) == "In Progress")
                                    DelStatusId = 3;

                                Mul_tblData = new Dictionary<string, object>();
                                Mul_tblData.Add("ID", 0);
                                Mul_tblData.Add("MEMBER_ID", MemberId);
                                Mul_tblData.Add("DELIVERY_MODE_ID", DelModeId);
                                Mul_tblData.Add("DISPATCH_DT", Convert.ToDateTime(dt.Rows[i]["DISPATCH_DT"]));
                                Mul_tblData.Add("COURIER_ID", CourierId);
                                Mul_tblData.Add("DOCKE_NM_TX", Convert.ToString(dt.Rows[i]["DOCKER_UNIQUE_NO"]));
                                Mul_tblData.Add("DELIVERY_STATUS_NM", DelStatusId);
                                Mul_tblData.Add("DELIVERY_DT", Convert.ToDateTime(dt.Rows[i]["DELIVERY_DT"]));
                                lstData1.Add(Mul_tblData);
                            }
                        }

                        conditions = new Dictionary<string, object>();
                        if (lstData1.Count > 0)
                            AppUrl = AppUrl + "/AddUpdate";
                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                        act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    }
                }
            }

            return act;
        }

        public ActionClass beforeACSRenewalStep2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterACSRenewalStep2(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> d;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_1"])))
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_" + i + ""])))
                    {
                        d = new Dictionary<string, object>();
                        int Id = 0;
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["DEP-BEN_ID_" + i + ""])))
                        {
                            Id = Convert.ToInt32(frm["DEP-BEN_ID_" + i + ""]);
                            frm["s"] = "update";
                        }
                        else
                            frm["s"] = "insert";
                        string Name = Convert.ToString(frm["NAME_TX_" + i + ""]);
                        string Age = Convert.ToString(frm["AGE_TX_" + i + ""]);

                        string RELATION_TO_SUB = Convert.ToString(frm["RELATION_TO_SUB_TX_" + i + ""]);
                        string EMAIL_ID = Convert.ToString(frm["EMAIL_TX_" + i + ""]);
                        string PHONE_NUMBER = Convert.ToString(frm["PHONE_TX_" + i + ""]);
                        string ADDRESS = Convert.ToString(frm["ADDRESS_TX_" + i + ""]);
                        int ACS_RENEWAL_ID = Convert.ToInt32(frm["STEP1_ID"]);

                        if (Id != 0)
                            d["ID"] = Id;
                        d["ACS_REG_ID"] = ACS_RENEWAL_ID;
                        d["NAME_TX"] = Name;
                        d["AGE_TX"] = Age;
                        d["RELATION_TO_SUB_TX"] = RELATION_TO_SUB;
                        d["EMAIL_TX"] = EMAIL_ID;
                        d["PHONE_TX"] = PHONE_NUMBER;
                        d["ADDRESS_TX"] = ADDRESS;
                        d["PURPOSE_TX"] = "ACS Renewal";

                        list.Add(d);
                    }
                }

                if (list.Count > 0)
                    act = UtilService.insertOrUpdate("Training", "MEM_ACS_DEP_BENEIFICIARY_TX", list);
            }
            
            return act;
        }


        public ActionClass beforeUploadCOPDocument(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["PRE_REF_ID"] != null)
            {
                if (!string.IsNullOrEmpty(frm["PRE_REF_ID"]))
                {
                    frm["ui"] = frm["PRE_REF_ID"];
                }

            }
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));

            //return null;

        }

        public ActionClass afterUploadCOPDocument(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            if (frm["BUTTON_TYPE"] == "R")
            {
                if (frm["REMOVE_NM"] == "1")
                {
                    if (frm["ID"] != "")
                    {
                        frm["s"] = "update";
                        frm.Add("ID", frm["REMOVE_ID"]);
                        frm.Add("ACTIVE_YN", "0");
                        string filePath = frm["FILE_PATH"];
                        System.IO.File.Delete(filePath);
                    }
                    ActionClass act3 = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T");
                    act = act3;
                }
            }
            if (frm["BUTTON_TYPE"] == "U")
            {
                string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
                string screenId = frm["si"];
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;
                FolderName = ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"].ToString(); //"UploadImage" + "\\";
                //FolderName = "UploadImage" + "\\";
                //FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;
                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = System.IO.Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = System.IO.Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);

                    string _path = FolderName;
                    string _FullPath = System.IO.Path.Combine(_path, _FileName);

                    if (!(System.IO.Directory.Exists(_path)))
                        System.IO.Directory.CreateDirectory(_path);

                    chechagain:

                    if (System.IO.File.Exists(_FullPath))
                    {
                        _FileName = "1_" + _FileName;
                        _FullPath = System.IO.Path.Combine(_path, _FileName);
                        goto chechagain;
                    }
                    else
                    {
                        File_name_tx = _FileName;
                        file_path_tx = _FullPath;
                        frm.Add("FILE_NAME_TX", File_name_tx);
                        frm.Add("FILE_PATH", file_path_tx);
                        frm.Add("REMOVE_NM", "0");
                        frm.Add("ACTION_BY", "");
                        frm.Add("APPROVE_NM", "0");
                        frm.Add("ACTIVE_YN", "1");
                        HttpContext.Current.Request.Files[0].SaveAs(_FullPath);

                    }
                }

                string finalPath = frm["FILE_PATH"];
                frm.Remove("FILE_PATH");
                if (finalPath.Substring(0, 1).Contains(','))
                {
                    finalPath = finalPath.Remove(0, 1);
                    frm.Add("FILE_PATH", finalPath);
                }

                ActionClass act1 = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T");
                act = act1;
            }
            if (frm["BUTTON_TYPE"] == "S")
            {
                Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                DataTable dt1 = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_COP_ORIENTATION_DETAILS_T", conditions, null, 0, 1);
                DataTable dt2 = UtilService.getData(UtilService.getApplicationScheme(screen), "MEM_COP_LAST_EMPLOYEE_DETAILS_T", conditions, null, 0, 1);
                if (dt1 != null && dt1.Rows != null && dt1.Rows.Count > 0)
                {
                    if (dt2 != null && dt2.Rows != null && dt2.Rows.Count > 0)
                    {
                        frm.Add("CERTIFICATE_NO_TX", Convert.ToString(dt1.Rows[0]["CERTIFICATE_NO_TX"]));
                        frm.Add("DURATION_TX", Convert.ToString(dt1.Rows[0]["DURATION_TX"]));
                        frm.Add("FROM_DT", Convert.ToString(dt1.Rows[0]["FROM_DT"]));
                        frm.Add("TO_DT", Convert.ToString(dt1.Rows[0]["TO_DT"]));
                        frm.Add("ORGANIZED_BY", Convert.ToString(dt1.Rows[0]["ORGANIZED_BY"]));
                        frm.Add("COMPNAY_NAME_TX", Convert.ToString(dt2.Rows[0]["COMPNAY_NAME_TX"]));
                        frm.Add("DIR_TX", Convert.ToString(dt2.Rows[0]["DIR_TX"]));
                        frm.Add("ECSIN_NO_TX", Convert.ToString(dt2.Rows[0]["ECSIN_NO_TX"]));
                        frm.Add("DESIGNATION_TX", Convert.ToString(dt2.Rows[0]["DESIGNATION_TX"]));
                        frm.Add("JOINING_DT", Convert.ToString(dt2.Rows[0]["JOINING_DT"]));
                        frm.Add("LAST_END_DT", Convert.ToString(dt2.Rows[0]["LAST_END_DT"]));
                        frm.Add("DURATION_OF_EMPLOYEEMENT_TX", Convert.ToString(dt2.Rows[0]["DURATION_OF_EMPLOYEEMENT_TX"]));
                        frm.Add("REF_ID", HttpContext.Current.Session["PRE_REF_ID"].ToString());
                        frm["s"] = "insert";

                    }
                }
                //frm["nextscreen"] = "538";
                ActionClass act2 = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "MEM_COP_ORIENTATION_AND_LAST_EMPLOYEE_T");
                act = act2;
            }
            return act;
        }

    }
}
