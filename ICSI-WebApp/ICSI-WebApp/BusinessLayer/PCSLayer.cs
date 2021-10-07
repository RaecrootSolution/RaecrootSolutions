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
    public class PCSLayer
    {
        public ActionClass beforePCSRegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && !frm["UNIQUE_REG_ID"].Trim().Equals("") && isUqNumberExists(frm["UNIQUE_REG_ID"], screen))
            {
                string uniqueId = frm["UNIQUE_REG_ID"].ToString();
                uniqueId = (uniqueId != null && uniqueId.Length < 10) ? uniqueId.PadLeft(10, '0') : uniqueId;
                frm["s"] = "edit";
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", uniqueId);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
                frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                SessionHandling("PCS_COMPANY_MASTER_DETAIL_T", frm["UNIQUE_REG_ID"]);
            }
            if (frm["ui"] != null && !frm["ui"].Trim().Equals(""))
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", frm["ui"]);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
                if (dt.Rows[0]["ID"] != null)
                {
                    frm["UNIQUE_REG_ID"] = dt.Rows[0]["UNIQUE_REG_ID"].ToString();
                    frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "edit";
                    SessionHandling("PCS_COMPANY_MASTER_DETAIL_T", frm["UNIQUE_REG_ID"]);
                }
            }
            frm["DID"] = "1";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterPCSRegistration(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            // Validations Start

            // Validations end
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string uqNumber = GetUqNumber(screen, frm);
            if (uqNumber == string.Empty)
            {
                actionClass = Util.UtilService.GetMessage(-202);
            }
            else
            {
                frm["UNIQUE_REG_ID"] = uqNumber;
                SessionHandling(screen.Table_Name_Tx, uqNumber);
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
                    storeEmailData(actionClass, screen, frm["s"], AppUrl, Session_Key, UserName, ID);
                }

                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                //frm["s"] = "new";
                frm["DID"] = "1";
            }
            return actionClass;
        }

        public ActionClass beforePCSRegistrationPage2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass action = new ActionClass();
            string uqNumber = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            if (uqNumber != null && uqNumber != string.Empty)
            {
                frm["UNIQUE_REG_ID"] = uqNumber;
            }
            if (frm["UNIQUE_REG_ID"] != null && !frm["UNIQUE_REG_ID"].Trim().Equals("") && (frm["ID"] != null && frm["ID"].ToString() != "" || (frm["ui"] != null && frm["ui"].ToString() != "")))
            {
                string unqID = (frm["ID"] != null) ? frm["ID"].ToString() : frm["ui"].ToString();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                conditions.Add("ID", unqID);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["ID"] != null)
                {
                    frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "edit";
                    action = UtilService.beforeLoad(WEB_APP_ID, frm);
                }
            }

            frm["DID"] = "1";
            return action;
        }

        public ActionClass afterPCSRegistrationPage2(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string uqNumber = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            // Validations Start
            if (frm["btnNext"] != null)
            {
                //actionClass.StatCode = "0";
                //actionClass.StatMessage = "success";
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["ID"] != null)
                {
                    frm["ui"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "update";
                    actionClass = Update(WEB_APP_ID, frm);
                }

                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                frm["UNIQUE_REG_ID"] = uqNumber;
                return actionClass;
            }
            // Validations end

            if (uqNumber == string.Empty)
            {
                actionClass = Util.UtilService.GetMessage(-202);
            }
            else
            {
                string unqID = (frm["ID"] != null && frm["ID"].ToString() != "") ? frm["ID"].ToString() : "0";
                frm["UNIQUE_REG_ID"] = uqNumber;
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                conditions.Add("ID", unqID);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["ID"] != null)
                {
                    frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "update";
                }
                if (frm["btnAddAddress"] != null)
                {
                    actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                    frm.Remove("nextscreen");
                    //frm["nextscreen"] = Convert.ToString(screen.ID);
                    //frm["ID"] = null;
                    //frm["s"] = "new";
                    frm["UNIQUE_REG_ID"] = uqNumber;
                }
                else if (frm["btnNext"] == null && frm["s"].ToString() != "edit")
                {
                    actionClass.StatCode = "-1";
                    actionClass.StatMessage = "";
                    //frm["nextscreen"] = null;
                    //frm["s"] = "edit";
                    frm["UNIQUE_REG_ID"] = uqNumber;
                    return beforePCSRegistrationPage2(WEB_APP_ID, frm, screen);
                }
                else
                {
                    frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                    frm["UNIQUE_REG_ID"] = uqNumber;
                }

                frm["DID"] = "1";
            }
            return actionClass;
        }

        public ActionClass beforePCSOnlyRegistrationPage2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && !frm["UNIQUE_REG_ID"].Trim().Equals(""))
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["ID"] != null)
                {
                    frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "edit";
                }
            }
            frm["DID"] = "1";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterPCSOnlyRegistrationPage2(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            // Validations Start

            // Validations end
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string uqNumber = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            if (uqNumber == string.Empty)
            {
                actionClass = Util.UtilService.GetMessage(-202);
            }
            else
            {
                frm["UNIQUE_REG_ID"] = uqNumber;
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["ID"] != null)
                {
                    frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                    frm["s"] = "update";
                }
                else
                {
                    frm.Remove("ID");
                }
                if (frm["btnNextPCS"] == null)
                {
                    actionClass.StatCode = "-1";
                    actionClass.StatMessage = "";
                    //frm["nextscreen"] = null;
                    //frm["s"] = "edit";
                    frm["UNIQUE_REG_ID"] = uqNumber;
                    return beforePCSOnlyRegistrationPage2(WEB_APP_ID, frm, screen);
                }
                actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                frm["DID"] = "1";
            }
            return actionClass;
        }

        public ActionClass beforePCSRegistrationPage3(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string uqNumber = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            if (uqNumber == string.Empty)
            {

            }
            else
            {
                frm["UNIQUE_REG_ID"] = uqNumber;
            }
            if (frm["UNIQUE_REG_ID"] != null && !frm["UNIQUE_REG_ID"].Trim().Equals("")
            //&& isUqNumberExists(frm["UNIQUE_REG_ID"],screen)
            )
            {
                frm["s"] = "edit";
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", frm["UNIQUE_REG_ID"]);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
                frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                frm["ui"] = Convert.ToString(dt.Rows[0]["ID"]);
            }
            frm["DID"] = "1";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterPCSRegistrationPage3(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            // Validations Start

            // Validations end
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string uqNumber = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            if (uqNumber == string.Empty)
            {
                actionClass = Util.UtilService.GetMessage(-202);
            }
            else
            {
                frm["UNIQUE_REG_ID"] = uqNumber;
                actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                if (UserName == "pcscomp")
                {
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
                    if (ID > 0 && screen.is_Email_yn)
                        storeEmailData(actionClass, screen, "insert", AppUrl, Session_Key, UserName, ID);
                }

                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                frm["DID"] = "1";
            }
            return actionClass;
        }

        private static bool isUqNumberExists(string uqNumber, Screen_T screen, bool isFinalTable = false)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("UNIQUE_REG_ID", uqNumber);
            DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), !isFinalTable ? screen.Table_Name_Tx : "COMPANY_PCS_T", conditions, null, 0, 1);
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else
            {
                return isFinalTable ? false : isUqNumberExists(uqNumber, screen, true);
            }
        }

        private static string GetUqNumber(Screen_T screen, FormCollection frm)
        {
            string uqNumber = string.Empty;
            bool isPassed = false;
            if (frm["UNIQUE_REG_ID"] != null && !Convert.ToString(frm["UNIQUE_REG_ID"]).Trim().Equals(""))
            {
                isPassed = true;
                uqNumber = frm["UNIQUE_REG_ID"];
            }
            while (!isPassed)
            {
                string uq = UtilService.RandomNumber();
                if (isUqNumberExists(uqNumber, screen))
                {
                    isPassed = true;
                    uqNumber = uq;
                }
            }
            return uqNumber;
        }

        public static void SessionHandling(string TableName, string uniqueregId, string ID = null)
        {
            Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
            uniqueregId = uniqueregId.PadLeft(10, '0');
            HttpContext.Current.Session["UNIQUE_REG_ID"] = uniqueregId.ToString();
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
                        sessionvalues[0] = uniqueregId.ToString();
                        sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                        sessionObjs.Remove(TableName);
                        sessionObjs.Add(TableName, sessionvalues);
                        HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                    }
                }
                else
                {
                    sessionvalues[0] = uniqueregId.ToString();
                    sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                    sessionObjs.Add(TableName, sessionvalues);
                    HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                }
            }
            else
            {
                sessionvalues[0] = uniqueregId.ToString();
                sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                sessionObjs.Add(TableName, sessionvalues);
                HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
            }
        }

        public static string GetSessionData(string TableName, string ID = null)
        {
            Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
            string uniqueRegID = string.Empty;
            object sessionobjvalues;
            if (sessionObjs.TryGetValue(TableName, out sessionobjvalues))
            {
                object[] arr = sessionobjvalues as object[];
                uniqueRegID = (ID != null) ? ((arr[1] != null) ? arr[1].ToString() : null) : arr[0].ToString();
            }
            return uniqueRegID;
        }

        public static string RandomNumber()
        {
            Random random = new Random();
            return random.Next(1, 1000000000).ToString("D10");
        }

        public static void UploadFile(HttpPostedFileBase file, FormCollection frm)
        {
            string FolderName = string.Empty;
            string uniqueregID = string.Empty;

            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                uniqueregID = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            }

            FolderName = Convert.ToString(FilePath.PCSCOMPDOCS.GetEnumDisplayName());
            string _FileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
            string fileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
            string _PathExt = Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);
            _FileName = uniqueregID + "_" + _FileName;// + _PathExt;
                                                      //string _path = HttpContext.Current.Server.MapPath("..//" + Convert.ToString(frm["FPath"]));
            string _path = UtilService.getDocumentPath(FolderName);
            string _FullPath = _path + _FileName;

            if (!(Directory.Exists(_path)))
                Directory.CreateDirectory(_path);

            if (File.Exists(_FullPath))
            {
                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                string NewFileName = uniqueregID + "_" + Timestamp.ToString() + _PathExt;
                File.Move(_FullPath, _path + "//" + NewFileName);
                //File.Delete(_FullPath);
            }
            HttpContext.Current.Request.Files[0].SaveAs(_FullPath);

            ActionClass actionClass = new ActionClass();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            JObject jdata = DBTable.GetData("mfetch", null, "PCS_REGISTRATION_DOCUMENT_T", 0, 100, "Training");
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> tableData = new Dictionary<string, object>();
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();
            if (jdata.HasValues)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "META_DATA")
                    {
                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        #region single operation
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY"
                                && Convert.ToString(dt.Rows[i][0]) != "STATUS_YN" && Convert.ToString(dt.Rows[i][0]) != "REG_TYPE_ID"
                                && Convert.ToString(dt.Rows[i][0]) != "UNIQUE_REG_ID" && Convert.ToString(dt.Rows[i][0]) != "PATH_TX"
                                 && Convert.ToString(dt.Rows[i][0]) != "FILE_NAME_TX")
                            {
                                tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                            }

                            if (Convert.ToString(dt.Rows[i][0]) == "FILE_NAME_TX")
                            {
                                tableData.Add(dt.Rows[i][0].ToString(), fileName);
                            }
                            if (Convert.ToString(dt.Rows[i][0]) == "PATH_TX")
                            {
                                tableData.Add(dt.Rows[i][0].ToString(), _FullPath);
                            }
                            if (Convert.ToString(dt.Rows[i][0]) == "UNIQUE_REG_ID" && HttpContext.Current.Session["SESSION_OBJECTS"] != null)
                            {
                                tableData.Add(dt.Rows[i][0].ToString(), uniqueregID);
                            }
                        }

                        lstData1.Add(tableData);
                        #endregion

                        AppUrl = AppUrl + "/AddUpdate";
                        lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_REGISTRATION_DOCUMENT_T", 0, 0, lstData1, conditions));
                        actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                        actionClass.columnMetadata = jdata;
                    }
                }
            }
        }

        public static ActionClass Update(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string screenType = frm["s"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            // screentype = insert / update / fetch
            try
            {
                if (screenType != null)
                {
                    //else if (screenType.Equals("update"))
                    //{
                    //    Screen_T screen = screenObject(frm);
                    //    if (screen != null && userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals("") && uniqueId != null && !uniqueId.Trim().Equals(""))
                    //    {
                    //        string applicationSchema = getApplicationScheme(screen);

                    //    }
                    //}
                    if (screenType.Equals("insert") || screenType.Equals("update"))
                    {
                        Screen_T screen = screenObject(WEB_APP_ID, frm);
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, "Training");

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
                                    lstData.Add(Util.UtilService.addSubParameter("Training", screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
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

        public ActionClass beforePCSCompStatus(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string uniqueId = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            frm["UNIQUE_REG_ID"] = uniqueId;
            frm["s"] = "edit";
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("UNIQUE_REG_ID", uniqueId);
            DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
            frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
            frm["ui"] = Convert.ToString(dt.Rows[0]["ID"]);
            frm["DID"] = "1";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass beforePCSCompAdmin(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && !frm["UNIQUE_REG_ID"].Trim().Equals(""))
            {
                string uniqueId = frm["UNIQUE_REG_ID"].ToString();
                uniqueId = (uniqueId != null && uniqueId.Length < 10) ? uniqueId.PadLeft(10, '0') : uniqueId;
                frm["s"] = "edit";
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("UNIQUE_REG_ID", uniqueId);
                DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
                frm["ID"] = Convert.ToString(dt.Rows[0]["ID"]);
                frm["ui"] = Convert.ToString(dt.Rows[0]["ID"]);
                SessionHandling("PCS_COMPANY_MASTER_DETAIL_T", frm["UNIQUE_REG_ID"], frm["ID"]);
            }
            frm["DID"] = "1";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterPCSCompAdmin(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            string path = string.Empty;
            string reg_type = string.Empty;
            string query_id = string.Empty;
            AppUrl = AppUrl + "/AddUpdate";
            // Validations Start

            // Validations end
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string uqNumber = GetSessionData(screen.Table_Name_Tx);
            string ID = GetSessionData(screen.Table_Name_Tx, "ID");
            frm["UNIQUE_REG_ID"] = uqNumber;
            SessionHandling(screen.Table_Name_Tx, uqNumber);
            frm["ui"] = ID.ToString();
            frm["s"] = "update";
            frm["APPROVED_DT"] = DateTime.Now.ToString();

            if (frm["STATUS_TX"] == "approved")
                frm["APPROVE_NM"] = "1";
            else if (frm["STATUS_TX"] == "rejected")
                frm["APPROVE_NM"] = "3";
            else if (frm["STATUS_TX"] == "call for")
                frm["APPROVE_NM"] = "2";

            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            int uID = 0;
            string status = string.Empty;
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
                            uID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            status = dtb.Rows[0]["STATUS_TX"].ToString();
                            reg_type = dtb.Rows[0]["REG_COM_TYPE_ID"].ToString();
                            if (reg_type != null && reg_type != string.Empty)
                            {
                                reg_type = (reg_type == "1") ? "PCS" : "Company";
                                query_id = (reg_type == "PCS") ? "16" : "17";
                            }
                        }
                    }
                }
            }

            if (uID > 0 && status != null && status != string.Empty && status == "approved")
            {
                //storeEmailData(actionClass, screen, frm["s"], AppUrl, Session_Key, UserName, uID);
                DataTable dtht = new DataTable();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("TEMPLATE_DESC", reg_type);
                conditions.Add("ACTIVE_YN", 1); ;
                dtht = UtilService.getData(UtilService.getApplicationScheme(screen), "PCS_COMPANY_REGISTRATION_LETTER_TEMPLATE_T", conditions, null, 0, 1);
                if (dtht != null && dtht.Rows.Count > 0)
                {
                    string htmltemplate = dtht.Rows[0]["TEMPLATE_HTML_TX"].ToString();
                    string MethodName = "qfetch";
                    DataTable dtq = new DataTable();
                    conditions.Clear();
                    conditions.Add("ID", uID);
                    conditions.Add("QID", Convert.ToInt32(query_id));
                    JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, "Training");

                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                        {
                            dtq = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }
                    if (dtq != null && dtq.Rows.Count > 0)
                    {
                        foreach (DataRow dtRow in dtq.Rows)
                        {
                            // On all tables' columns
                            foreach (DataColumn dc in dtq.Columns)
                            {
                                htmltemplate = htmltemplate.Replace("@" + dc.ToString(), dtRow[dc].ToString());
                            }
                        }
                    }

                    byte[] pdfbytes = PdfSharpConvert(htmltemplate);
                    path = SaveRegistrationLetter(pdfbytes);
                    if (path != null && path != string.Empty)
                    {
                        List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                        Dictionary<string, object> tableData = new Dictionary<string, object>();

                        tableData.Add("PATH_TX", path);
                        tableData.Add("FILE_NAME_TX", "Registration Approval Letter");
                        tableData.Add("UNIQUE_REG_ID", uqNumber);

                        //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                        //if (value)
                        //{
                        lstData1.Add(tableData);
                        lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_REGISTRATION_APPROVAL_T", 0, 0, lstData1, conditions));
                        actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    }


                }
            }
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            frm["s"] = "edit";
            //frm["s"] = "new";
            frm["DID"] = "1";

            return actionClass;
        }
        public static Byte[] PdfSharpConvert(String html)
        {
            Byte[] res = null;
            using (MemoryStream ms = new MemoryStream())
            {
                var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
                pdf.Save(ms);
                res = ms.ToArray();

            }
            return res;
        }

        public static string SaveRegistrationLetter(byte[] pdfbytes)
        {
            string FolderName = string.Empty;
            string uniqueregID = string.Empty;

            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                uniqueregID = GetSessionData("PCS_COMPANY_MASTER_DETAIL_T");
            }

            FolderName = Convert.ToString(FilePath.PCSCOMPDOCS.GetEnumDisplayName());
            string _FileName = "Registration Letter";
            //string fileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
            string _PathExt = ".pdf";
            _FileName = uniqueregID + "_" + _FileName + _PathExt;// + _PathExt;
                                                                 //string _path = HttpContext.Current.Server.MapPath("..//" + Convert.ToString(frm["FPath"]));
            string _path = UtilService.getDocumentPath(FolderName); 
            string _FullPath = _path + _FileName;

            if (!(Directory.Exists(_path)))
                Directory.CreateDirectory(_path);

            if (File.Exists(_FullPath))
            {
                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                string NewFileName = uniqueregID + "_" + Timestamp.ToString() + _PathExt;
                File.Move(_FullPath, _path + "//" + NewFileName);
                //File.Delete(_FullPath);
            }
            //MemoryStream memoryStream = new MemoryStream(pdfbytes);
            //memoryStream.Position = 0;
            //System.IO.File.WriteAllBytes(_FullPath, memoryStream.ToArray());
            MemoryStream ms = new MemoryStream(pdfbytes);
            //write to file
            FileStream file = new FileStream(_FullPath, FileMode.Create, FileAccess.Write);
            ms.WriteTo(file);
            file.Close();
            ms.Close();
            return _FullPath;

        }
    }
}