using ICSI_Library.Util;
using ICSI_Library.Models;
using ICSI_WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Net.Mail;
using System.IO.Compression;
using ClosedXML.Excel;

namespace ICSI_WebApp.Util
{
    public static class UtilService
    {
        //public static int WEB_APP_ID = 1;
        public static List<SaltKeyEntry> ObjSaltKey = new List<SaltKeyEntry>();
        public static string createRequestObject(string URL, string sid, string key, Dictionary<string, object> data, out string StatMessage)
        {
            StatMessage = string.Empty;
            string sdata = string.Empty;
            try
            {
                JObject req = JObject.Parse(createRequest(URL, sid, key, data));
                if (req != null && req.HasValues && req.ContainsKey("StatCode") && req.ContainsKey("StatMessage"))
                {
                    string StatCode = req.GetValue("StatCode").ToString();
                    StatMessage = req.GetValue("StatMessage").ToString();
                    string edata = Convert.ToString(req.GetValue("data"));
                    if (!string.IsNullOrEmpty(edata))
                        sdata = CryptographyUtil.DecryptDataPKCS7(edata, key); // TODO - if there any exception, needs to handle here
                    else
                        sdata = StatMessage;
                }
            }
            catch (Exception ex) { }
            return sdata;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static ActionClass createRequestObject(string URL, string sid, string key, Dictionary<string, object> data)
        {
            ActionClass actionClass = new ActionClass();
            string sdata = string.Empty;
            try
            {
                JObject req = JObject.Parse(createRequest(URL, sid, key, data));
                if (req != null && req.HasValues && req.ContainsKey("StatCode") && req.ContainsKey("StatMessage"))
                {
                    actionClass.StatCode = req.GetValue("StatCode").ToString();
                    actionClass.StatMessage = req.GetValue("StatMessage").ToString();
                    actionClass.jObject = req;
                    string edata = Convert.ToString(req.GetValue("data"));
                    if (!string.IsNullOrEmpty(edata))
                    {
                        sdata = CryptographyUtil.DecryptDataPKCS7(edata, key); // TODO - if there any exception, needs to handle here
                        actionClass.DecryptData = sdata;
                    }
                }
            }
            catch (Exception ex) { actionClass.StatMessage = ex.Message; }
            return actionClass;
        }

        public static string createRequest(string URL, string sid, string key, Dictionary<string, object> data)
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
                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("sid", sid);
                string estr = JsonConvert.SerializeObject(data, Formatting.Indented);
                estr = CryptographyUtil.EncryptDataPKCS7(estr, key);
                d.Add("data", estr);
                string jsonData = JsonConvert.SerializeObject(d, Formatting.Indented);
                StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;

                using (System.IO.StreamReader readResponse = new System.IO.StreamReader(webResponse.GetResponseStream()))
                {
                    response = readResponse.ReadToEnd();
                }
            }
            catch (Exception ex) { }
            return response;
        }
        public static ActionClass GetRawData(Dictionary<string, object> conditions, Dictionary<string, string> conditionops, string TableName, int start, int end, string SchemaName)
        {
            string sdata = string.Empty;
            ActionClass act = new ActionClass();
            DataTable dt = new DataTable();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            try
            {
                AppUrl = AppUrl + "/fetch";
                List<Dictionary<string, object>> data_data = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                data_data.Add(Util.UtilService.addSubParameter(SchemaName, TableName, start, end, data, conditions, conditionops));

                string Message = string.Empty;
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "fetch", data_data));
            }
            catch (Exception ex) { }
            return act;
        }

        public static Dictionary<string, object> createRequestForPaymentGateway(string URL, string methodName, Dictionary<string, object> data)
        {
            Dictionary<string, object> resp = null;
            string response = string.Empty;
            try
            {
                HttpWebRequest webRequest = WebRequest.Create(URL + methodName) as HttpWebRequest;
                webRequest.Method = "POST";
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.PreAuthenticate = true;
                webRequest.ContentType = "application/json";
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        response = streamReader.ReadToEnd();
                    }
                }
                //webRequest.ContentLength = 0;               
                /*string strJsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
                streamWriter.Write(strJsonData);
                streamWriter.Flush();
                streamWriter.Close();
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;

                using (System.IO.StreamReader readResponse = new System.IO.StreamReader(webResponse.GetResponseStream()))
                {
                    response = readResponse.ReadToEnd();
                }*/
                //resp = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                //response = Convert.ToString(resp["strInitiate"]);
                //}               
                resp = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            }
            catch (Exception ex)
            {
                resp = new Dictionary<string, object>();
                resp["strInitiate"] = "";
                string strErrMsg = ex.Message;
            }
            return resp;
        }

        public static Dictionary<string, object> createParameters(string application, string module, string screen, string role, string resp, string type, List<Dictionary<string, object>> data)
        {
            Dictionary<string, object> r = new Dictionary<string, object>();
            r.Add("application", application);
            r.Add("module", module);
            r.Add("role", role);
            r.Add("responsibility", resp);
            r.Add("type", type);
            r.Add("data", data);
            return r;
        }

        public static Dictionary<string, object> addSubParameter(string schema, string table, int start, int end, List<Dictionary<string, object>> data, Dictionary<string, object> conditions)
        {
            return addSubParameter(schema, table, start, end, 0, 0, data, conditions, null);
        }

        public static Dictionary<string, object> addSubParameter(string schema, string table, int start, int end, int pgn, int pgr, List<Dictionary<string, object>> data, Dictionary<string, object> conditions)
        {
            return addSubParameter(schema, table, start, end, pgn, pgr, data, conditions, null);
        }

        public static Dictionary<string, object> addSubParameter(string schema, string table, int start, int end, List<Dictionary<string, object>> data, Dictionary<string, object> conditions, Dictionary<string, string> conditionsops)
        {
            return addSubParameter(schema, table, start, end, 0, 0, data, conditions, conditionsops);
        }

        public static Dictionary<string, object> addSubParameter(string schema, string table, int start, int end, int pgn, int pgr, List<Dictionary<string, object>> data, Dictionary<string, object> conditions, Dictionary<string, string> conditionsops)
        {
            return addSubParameter(schema, table, start, end, false, pgn, pgr, data, conditions, conditionsops);
        }

        public static Dictionary<string, object> addSubParameter(string schema, string table, int start, int end, bool pgs, int pgn, int pgr, List<Dictionary<string, object>> data, Dictionary<string, object> conditions, Dictionary<string, string> conditionsops)
        {
            Dictionary<string, object> r = new Dictionary<string, object>();
            r.Add("schema", schema);
            r.Add("table", table);
            r.Add("start", start);
            r.Add("end", end);
            r.Add("pgs", pgs ? "1" : "0");
            r.Add("pgn", pgn);
            r.Add("pgr", pgr);
            r.Add("data", data);
            r.Add("conditions", conditions);
            r.Add("conditionops", conditionsops);
            return r;
        }

        public static void createManagement()
        {
            var tupleList = new List<Tuple<string, int, int>>
            {
                new Tuple<string,int, int>("APPLICATION_T",0,100),
                new Tuple<string,int, int>("APP_MODULE_T",0,100),
                new Tuple<string,int, int>("SCREEN_T",0,100),
                new Tuple<string,int, int>("ROLE_T",0,100),
                new Tuple<string,int, int>("RESPONSIBILITY_T",0,100)
            };
        }

        public static DataTable fetchResponsibilities(UserData userData)
        {
            DataTable responsibilities = new DataTable();
            DataTable userresponsibilites = new DataTable();
            if (userData.USER_ROLE_T != null && userData.USER_ROLE_T.Rows != null
                && DBTable.ROLE_T != null && DBTable.ROLE_T.Rows != null
                && DBTable.ROLE_RESP_T != null && DBTable.ROLE_RESP_T.Rows != null
                && DBTable.RESPONSIBILITY_T != null && DBTable.RESPONSIBILITY_T.Rows != null
                && userData.USER_ROLE_T.Columns.Count > 0 && DBTable.ROLE_T.Columns.Count > 0
                && DBTable.ROLE_RESP_T.Columns.Count > 0 && DBTable.RESPONSIBILITY_T.Columns.Count > 0
                )
            {
                //responsibilities = (from objUser_Resp in DBTable.ROLE_RESP_T.AsEnumerable()
                //                    join objResp in DBTable.RESPONSIBILITY_T.AsEnumerable()
                //                    on objUser_Resp.Field<int>("RESP_ID") equals objResp.Field<int>("ID")
                //                    join objUser_Role in userData.USER_ROLE_T.AsEnumerable()
                //                    on objUser_Resp.Field<int>("ROLE_ID") equals objUser_Role.Field<int>("ID")
                //                    join objRole in DBTable.ROLE_T.AsEnumerable()
                //                    on objUser_Role.Field<int>("ROLE_ID") equals objRole.Field<int>("ID")
                //                    select objResp).ToList();
                try
                {
                    responsibilities = (from userrole in userData.USER_ROLE_T.AsEnumerable()
                                        join role in DBTable.ROLE_T.AsEnumerable() on userrole.Field<long>("ROLE_ID") equals role.Field<long>("ID")
                                        join roleresp in DBTable.ROLE_RESP_T.AsEnumerable() on role.Field<long>("ID") equals roleresp.Field<long>("ROLE_ID")
                                        join resp in DBTable.RESPONSIBILITY_T.AsEnumerable() on roleresp.Field<long>("RESP_ID") equals resp.Field<long>("ID")
                                        select resp).CopyToDataTable();
                }
                catch (Exception ee1) { }
                try
                {
                    if (userData.USER_RESP_T != null && userData.USER_RESP_T.Rows != null && userData.USER_RESP_T.Rows.Count > 0)
                    {
                        userresponsibilites = (from userrole in userData.USER_ROLE_T.AsEnumerable()
                                               join role in DBTable.ROLE_T.AsEnumerable() on userrole.Field<long>("ROLE_ID") equals role.Field<long>("ID")
                                               join roleresp in userData.USER_RESP_T.AsEnumerable() on userrole.Field<long>("USER_ID") equals roleresp.Field<long>("USER_ID")
                                               join resp in DBTable.RESPONSIBILITY_T.AsEnumerable() on roleresp.Field<long>("RESP_ID") equals resp.Field<long>("ID")
                                               select resp).CopyToDataTable();
                        responsibilities.Merge(userresponsibilites);
                    }
                }
                catch (Exception ee2) { }

            }
            return responsibilities;
        }

        public static string fetchMenuString(List<object> menu, bool isFirstTime)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in menu)
            {
                if (!isFirstTime && i == 0)
                {
                    ++i;
                    continue;
                }
                if (item.GetType() == typeof(DataRow))
                {
                    sb.Append("<li class=''><a href='#' onclick='menuAction(");
                    DataRow row = (DataRow)item;
                    sb.Append(row["ID"].ToString()).Append(")'>").Append(row["MENU_LABEL_TX"].ToString()).Append("</a></li>");
                }
                else
                {
                    List<object> l = item as List<Object>;
                    DataRow row = (DataRow)l[0];
                    //l.Remove(row);
                    sb.Append("<li class='dropdown'><a class='dropdown-toggle' data-toggle='dropdown' href='#'>").Append(row["MENU_LABEL_TX"].ToString());
                    sb.Append("<span class='caret'></span></a><ul class='dropdown-menu'>");
                    sb.Append(fetchMenuString(l, false));
                    sb.Append("</ul></li>");
                }
                ++i;
            }
            return sb.ToString();
        }

        private static Screen_Comp_T getScreenComponent(DataRow row, string webAppSchema, string appSchema, string moduleSchema, string screenSchema)
        {
            Screen_Comp_T screen_Comp_T = new Screen_Comp_T();
            screen_Comp_T.Id = Convert.ToInt32(row["ID"]);
            screen_Comp_T.Screen_Id = Convert.ToInt32(row["SCREEN_ID"]);
            screen_Comp_T.Ref_Id = Convert.ToInt32(row["REF_ID"]);
            screen_Comp_T.Order_Nm = Convert.ToInt32(row["ORDER_NM"]);
            screen_Comp_T.Comp_Type_Nm = Convert.ToInt32(row["COMP_TYPE_NM"]);
            if (row["REPORT_ID"] != DBNull.Value) screen_Comp_T.reportId = Convert.ToInt32(row["REPORT_ID"]); else screen_Comp_T.reportId = 0;
            if (row["MAX_ROWS_NM"] != DBNull.Value) screen_Comp_T.maxRows = Convert.ToInt32(row["MAX_ROWS_NM"]); else screen_Comp_T.maxRows = 0;
            if (row["MIN_ROWS_NM"] != DBNull.Value) screen_Comp_T.minRows = Convert.ToInt32(row["MIN_ROWS_NM"]); else screen_Comp_T.minRows = 0;
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
            if (!string.IsNullOrEmpty(Convert.ToString(row["SCREEN_REF_METHOD_NAME_TX"]))) screen_Comp_T.screenReferenceMethodNameTx = Convert.ToString(row["SCREEN_REF_METHOD_NAME_TX"]); else screen_Comp_T.screenReferenceMethodNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_CLASS_NAME_TX"]))) screen_Comp_T.screenCompClassNameTx = Convert.ToString(row["COMP_CLASS_NAME_TX"]); else screen_Comp_T.screenCompClassNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["SQL_TX"]))) screen_Comp_T.sql = Convert.ToString(row["SQL_TX"]); else screen_Comp_T.sql = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["WHERE_TX"]))) screen_Comp_T.where = Convert.ToString(row["WHERE_TX"]); else screen_Comp_T.where = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_METHOD_NAME_TX"]))) screen_Comp_T.screenCompMethodNameTx = Convert.ToString(row["COMP_METHOD_NAME_TX"]); else screen_Comp_T.screenCompMethodNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["TABLE_NAME_TX"]))) screen_Comp_T.tableNameTx = Convert.ToString(row["TABLE_NAME_TX"]); else screen_Comp_T.tableNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COLUMN_NAME_TX"]))) screen_Comp_T.columnNameTx = Convert.ToString(row["COLUMN_NAME_TX"]); else screen_Comp_T.columnNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["MANDATORY_YN"]))) screen_Comp_T.isMandatoryYn = Convert.ToBoolean(row["MANDATORY_YN"]); else screen_Comp_T.isMandatoryYn = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_CLASS_STATIC_YN"]))) screen_Comp_T.isScreenCompClassStatic = Convert.ToBoolean(row["COMP_CLASS_STATIC_YN"]); else screen_Comp_T.isScreenCompClassStatic = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["READ_WRITE_YN"]))) screen_Comp_T.isReadWriteYn = Convert.ToBoolean(row["READ_WRITE_YN"]); else screen_Comp_T.isReadWriteYn = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["PAGINATION_YN"]))) screen_Comp_T.isPaginationYn = Convert.ToBoolean(row["PAGINATION_YN"]); else screen_Comp_T.isPaginationYn = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COMP_METHOD_STATIC_YN"]))) screen_Comp_T.isScreenCompMethodStatic = Convert.ToBoolean(row["COMP_METHOD_STATIC_YN"]); else screen_Comp_T.isScreenCompMethodStatic = false;
            //DataTable resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("REF_ID") == screen_Comp_T.Id && x.Field<long>("ID") != screen_Comp_T.Id).OrderBy(x => x.Field<long>("ORDER_NM")).CopyToDataTable();
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

        public static ActionClass GetMessage(int id, Screen_T screen = null)
        {
            ActionClass act = new ActionClass();
            act.StatCode = "" + id;
            if (screen != null)
            {
                act.StatMessage = DBTable.SCREEN_MESSAGE_T.AsEnumerable().Where(x => x.Field<long>("STAT_CODE_NM") == id && x.Field<long>("SCREEN_ID") == screen.ID).Select(x => x.Field<string>("STAT_MESSAGE_TX")).FirstOrDefault();
            }
            if (act.StatMessage == null) act.StatMessage = DBTable.SCREEN_MESSAGE_T.AsEnumerable().Where(x => x.Field<long>("STAT_CODE_NM") == id).Select(x => x.Field<string>("STAT_MESSAGE_TX")).FirstOrDefault();
            if (act.StatMessage == null) act.StatMessage = DBTable.SCREEN_MESSAGE_T.AsEnumerable().Where(x => x.Field<int>("STAT_CODE_NM") == -2).Select(x => x.Field<string>("STAT_MESSAGE_TX")).FirstOrDefault();
            return act;
        }

        public static ActionClass CheckUniqueness(Screen_T screen, FormCollection frm, string ScreenType = "insert")
        {
            ActionClass act = new ActionClass();
            act.StatCode = "0";
            act.StatMessage = "Success";
            if (screen.Unique_Field_Tx != null && !screen.Unique_Field_Tx.Trim().Equals(""))
            {
                string fieldValue = frm.AllKeys.Contains(screen.Unique_Field_Tx) ? Convert.ToString(frm[screen.Unique_Field_Tx]) : "";
                if (fieldValue.Trim().Equals(""))
                {
                    act.StatCode = "-4";
                    act.StatMessage = "The " + screen.Unique_Field_Label_Tx + " should not be empty";
                }
                else
                {
                    string applicationSchema = getApplicationScheme(screen);
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add(screen.Unique_Field_Tx, fieldValue);
                    DataTable dt = getData(applicationSchema, screen.Table_Name_Tx, conditions, null, 0, 1);
                    if (ScreenType == "update" && frm["ID"] != null)
                    {
                        var query = dt.AsEnumerable().Where(r => r.Field<Int64>("ID") == Convert.ToInt64(frm["ID"]));
                        if (query.FirstOrDefault() != null) dt.Rows.Remove(query.FirstOrDefault());
                    }
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        act.StatCode = "-3";
                        act.StatMessage = "The " + screen.Unique_Field_Label_Tx + " should be unique";
                    }
                }
            }
            return act;
        }

        public static Screen_T GetScreenData(int WEB_APP_ID, DataRow menu, string scid, FormCollection frm)
        {
            long ScreenId = Convert.ToInt32(scid != null ? scid : menu["SCREEN_ID"]);
            // TODO - needs to check the user has responsibility or not

            long App_Module_ID = DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == ScreenId).Select(x => x.Field<long>("APP_MODULE_ID")).FirstOrDefault();
            long App_ID = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == App_Module_ID).Select(X => X.Field<long>("APP_ID")).FirstOrDefault();
            //string moduleSchemaName = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("APP_ID") == WEB_APP_ID).Select(X => X.Field<string>("ID")).FirstOrDefault();
            DataRow resScreen = DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == ScreenId && x.Field<long>("APP_MODULE_ID") == App_Module_ID).FirstOrDefault();
            string webAppSchemaName = DBTable.WEB_APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == WEB_APP_ID).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
            string applicationSchemaName = DBTable.APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == App_ID).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
            string moduleSchemaName = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == App_Module_ID).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();

            if (resScreen != null)
            {
                Screen_T screen = new Screen_T();
                screen.Action_Tx = Convert.ToString(resScreen["ACTION_TX"]);
                screen.ID = Convert.ToInt32(resScreen["ID"]);
                screen.App_Module_Id = Convert.ToInt32(resScreen["APP_MODULE_ID"]);
                screen.Screen_Name_Tx = Convert.ToString(resScreen["SCREEN_NAME_TX"]);
                screen.Screen_Sym_Name_Tx = Convert.ToString(resScreen["SCREEN_SYM_NAME_TX"]);
                screen.Screen_Title_Tx = Convert.ToString(resScreen["SCREEN_TITLE_TX"]);
                //screen.Screen_Next_Id = Convert.ToInt32(resScreen["SCREEN_NEXT_ID"]);
                screen.Screen_Style_Tx = Convert.ToString(resScreen["SCREEN_STYLE_TX"]);
                screen.Screen_Script_Tx = Convert.ToString(resScreen["SCREEN_SCRIPT_TX"]);
                screen.Mandatory_Fields_Tx = Convert.ToString(resScreen["MANDATORY_FIELDS_TX"]);
                screen.Mandatory_Field_Labels_Tx = Convert.ToString(resScreen["MANDATORY_FIELD_LABELS_TX"]);
                screen.Screen_Content_Tx = Convert.ToString(resScreen["SCREEN_CONTENT_TX"]);
                screen.Screen_Ref_Class_Name_Tx = Convert.ToString(resScreen["SCREEN_REF_CLASS_NAME_TX"]);
                screen.schemaNameTx = Convert.ToString(resScreen["SCHEMA_NAME_TX"]);
                screen.Unique_Field_Tx = Convert.ToString(resScreen["UNIQUE_FIELD_NAME_TX"]);
                screen.Unique_Field_Label_Tx = Convert.ToString(resScreen["UNIQUE_FIELD_LABEL_TX"]);
                screen.WebAppSchemaNameTx = webAppSchemaName;
                screen.ApplicationSchemaNameTx = applicationSchemaName;
                screen.ModuleSchemaNameTx = moduleSchemaName;
                screen.Screen_Class_Name_Tx = Convert.ToString(resScreen["SCREEN_CLASS_NAME_TX"]);
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["SCREEN_NEXT_ID"])))
                    screen.Screen_Next_Id = Convert.ToInt32(resScreen["SCREEN_NEXT_ID"]);
                else
                    screen.Screen_Next_Id = 0;
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["SUCCESS_MSG_NM"])))
                    screen.Success_Message_Nm = Convert.ToInt32(resScreen["SUCCESS_MSG_NM"]);
                else
                    screen.Success_Message_Nm = 0;
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["FAILURE_MSG_NM"])))
                    screen.Failure_Message_Nm = Convert.ToInt32(resScreen["FAILURE_MSG_NM"]);
                else
                    screen.Failure_Message_Nm = -100;
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["SCREEN_CLASS_STATIC_YN"])))
                    screen.Screen_Class_Static_YN = Convert.ToBoolean(resScreen["SCREEN_CLASS_STATIC_YN"]);
                else
                    screen.Screen_Class_Static_YN = null;
                screen.Screen_Method_Name_Tx = Convert.ToString(resScreen["SCREEN_METHOD_NAME_TX"]);
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["SCREEN_METHOD_STATIC_YN"])))
                    screen.Screen_Method_Static_YN = Convert.ToBoolean(resScreen["SCREEN_METHOD_STATIC_YN"]);
                else
                    screen.Screen_Method_Static_YN = null;
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["MANDATORY_SCR_ID"])))
                    screen.Mandatory_Scr_Id = Convert.ToInt32(resScreen["MANDATORY_SCR_ID"]);
                else
                    screen.Mandatory_Scr_Id = null;
                if (!string.IsNullOrEmpty(Convert.ToString(resScreen["EDIT_SCREEN_ID"])))
                    screen.Edit_Scr_Id = Convert.ToInt32(resScreen["EDIT_SCREEN_ID"]);
                else
                    screen.Edit_Scr_Id = null;

                screen.is_Email_yn = false;
                if (Convert.ToBoolean(resScreen["IS_EMAIL_YN"]))
                    screen.is_Email_yn = true;
                screen.is_SMS_yn = false;
                if (Convert.ToBoolean(resScreen["IS_SMS_YN"]))
                    screen.is_SMS_yn = true;

                screen.Table_Name_Tx = Convert.ToString(resScreen["TABLE_NAME_TX"]);
                screen.Active_YN = Convert.ToBoolean(resScreen["ACTIVE_YN"]);
                List<Screen_Comp_T> screenComponents = new List<Screen_Comp_T>();
                if (DBTable.SCREEN_COMP_T.Rows.Count > 0)
                {
                    var resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("SCREEN_ID") == ScreenId && x.Field<long>("REF_ID") == 0).OrderBy(x => x.Field<long>("ORDER_NM")).ToList();
                    //DataTable resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("SCREEN_ID") == ScreenId).OrderBy(x => x.Field<long>("ORDER_NM")).CopyToDataTable();
                    if (resScreen_Comp != null && resScreen_Comp.Count > 0)
                    {
                        DataTable dtScreen_Comp = resScreen_Comp.CopyToDataTable();
                        screenComponents = new List<Screen_Comp_T>();
                        foreach (DataRow row in dtScreen_Comp.Rows)
                        {
                            screenComponents.Add(getScreenComponent(row, screen.WebAppSchemaNameTx, screen.ApplicationSchemaNameTx, screen.ModuleSchemaNameTx, screen.schemaNameTx));
                        }
                    }
                }
                screen.ScreenComponents = screenComponents;
                StringBuilder screenHTML = new StringBuilder();
                renderScreenComponent(screen, screenComponents, screenHTML, frm);
                //screen.Screen_Content_Tx = screen.Screen_Content_Tx + "<br/>" + screenHTML;
                screen.Screen_Content_Tx = screen.Screen_Content_Tx + screenHTML;
                return screen;
            }
            return null;
        }


        private static bool isResponsibilityExists(long screenId)
        {
            bool returnVal = false;
            //long screenId = Convert.ToInt32(row["SCREEN_ID"].ToString());
            long moduleId = DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == screenId).Select(x => x.Field<long>("APP_MODULE_ID")).FirstOrDefault();
            long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == moduleId).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();
            UserData userData = (UserData)HttpContext.Current.Session["USER_DATA"];
            DataTable resps = fetchResponsibilities(userData);
            if (resps != null && resps.Columns.Count > 0)
            {
                var resScreen = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 4 && x.Field<long>("REF_ID") == screenId).ToList();
                if (resScreen != null && resScreen.Count > 0)
                {
                    var r1 = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 3 && x.Field<long>("REF_ID") == moduleId).ToList();
                    if (r1 != null && r1.Count > 0)
                    {
                        var r2 = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 2 && x.Field<long>("REF_ID") == appid).ToList();
                        if (r2 != null && r2.Count > 0) returnVal = true;
                    }
                }
            }
            return returnVal;
        }


        public static DataRow fetchMenuItem(int WEB_APP_ID, int menuid)
        {
            return DBTable.MENU_T.AsEnumerable().Where(x => x.Field<long>("ID") == menuid && x.Field<long>("WEB_APP_ID") == WEB_APP_ID).OrderBy(x => x.Field<long>("ORDER_NM")).FirstOrDefault();
        }

        public static DataRow fetchMenuItemByScreenId(int WEB_APP_ID, int screenid)
        {
            return DBTable.MENU_T.AsEnumerable().Where(x => x.Field<long>("SCREEN_ID") == screenid && x.Field<long>("WEB_APP_ID") == WEB_APP_ID).OrderBy(x => x.Field<long>("ORDER_NM")).FirstOrDefault();
        }

        private static List<DataRow> fetchMenuItems(int WEB_APP_ID, int menuObj)
        {
            return DBTable.MENU_T.AsEnumerable().Where(x => x.Field<long>("REF_ID") == menuObj && x.Field<long>("WEB_APP_ID") == WEB_APP_ID).OrderBy(x => x.Field<long>("ORDER_NM")).ToList();
        }

        public static List<object> getMenu(int WEB_APP_ID, DataTable resps)
        {
            List<object> lmenu = new List<object>();
            List<DataRow> childRows = fetchMenuItems(WEB_APP_ID, 0);
            fillTheMenu(WEB_APP_ID, lmenu, childRows, resps);
            return lmenu;
        }

        public static DataTable getData(string schemaName, string tableName, Dictionary<string, object> conditions, Dictionary<string, string> conditionops, int start, int end)
        {
            JObject jdata = Util.DBTable.GetData("fetch", conditions, conditionops, tableName, 0, 100, schemaName);
            DataTable dt = null;
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name.Equals(tableName)) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            return dt;
        }

        private static void fillTheMenu(int WEB_APP_ID, List<object> menu, List<DataRow> rows, DataTable resps)
        {
            foreach (DataRow row in rows)
            {
                bool isResponsibilityExist = false;
                long screenId = Convert.ToInt32(row["SCREEN_ID"].ToString());
                long moduleId = DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == screenId).Select(x => x.Field<long>("APP_MODULE_ID")).FirstOrDefault();
                long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == moduleId).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();

                if (resps != null && resps.Columns.Count > 0)
                {
                    var resScreen = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 4 && x.Field<long>("REF_ID") == screenId).ToList();
                    if (resScreen != null && resScreen.Count > 0)
                    {
                        var r1 = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 3 && x.Field<long>("REF_ID") == moduleId).ToList();
                        if (r1 != null && r1.Count > 0)
                        {
                            var r2 = resps.AsEnumerable().Where(x => x.Field<Int64>("RESP_TYPE_NM") == 2 && x.Field<long>("REF_ID") == appid).ToList();
                            if (r2 != null && r2.Count > 0) isResponsibilityExist = true;
                        }
                    }
                }
                if (isResponsibilityExist)
                {
                    int menuId = Convert.ToInt32(row["ID"].ToString());
                    List<DataRow> childRows = fetchMenuItems(WEB_APP_ID, menuId);
                    if (childRows != null && childRows.Count > 0)
                    {
                        List<object> submenu = new List<object>();
                        submenu.Add(row);
                        menu.Add(submenu);
                        fillTheMenu(WEB_APP_ID, submenu, childRows, resps);
                    }
                    else
                    {
                        menu.Add(row);
                    }
                }
            }
        }

        public static object FillFormValue(string dataType, FormCollection frm, string ColumnName)
        {
            object value = 0;
            try
            {
                if (dataType == "int")
                    value = frm[ColumnName] != null && frm[ColumnName].Length > 0 ? Convert.ToInt64(frm[ColumnName]) : 0;
                else if (dataType == "varchar" || dataType == "nvarchar" || dataType == "text")

                    value = frm[ColumnName] == null ? string.Empty : frm[ColumnName].Length > 0 ? Convert.ToString(frm[ColumnName]) : string.Empty;
                else if (dataType == "bit")
                {
                    if (frm[ColumnName] != null)
                    {
                        if (frm[ColumnName] == "1")
                            value = true;
                        else if (frm[ColumnName] == "0")
                            value = false;
                        else
                            value = frm[ColumnName].Length > 0 ? Convert.ToBoolean(frm[ColumnName]) : true;
                    }
                    else value = false;
                }
                else if (dataType == "datetime" || dataType == "date")
                {
                    if (frm[ColumnName] == null || frm[ColumnName].ToString().Trim().Equals(""))
                    {
                        //value = "01/01/1900";
                        value = null;
                    }
                    else
                    {
                        if (frm[ColumnName].Length == 10)
                        {
                            string dateformat = "MM/dd/yyyy";
                            if (frm["DateFormat"] != null)
                            {
                                if (frm["DateFormat"] != "")
                                {
                                    dateformat = frm["DateFormat"];
                                }
                                else
                                {
                                    dateformat = "MM/dd/yyyy";
                                }
                            }
                            else
                            {
                                dateformat = "MM/dd/yyyy";
                            }
                            try { value = DateTime.ParseExact(frm[ColumnName], dateformat, System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat); }
                            catch { value = DateTime.ParseExact(frm[ColumnName], dateformat, System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat); }
                        }
                        else if (frm[ColumnName].Length < 10)
                            value = frm[ColumnName];
                        else if (frm[ColumnName].Length > 12)
                            value = frm[ColumnName];
                        else
                            value = "01/01/1900";
                    }
                }
                else if (dataType == "char")

                    value = frm[ColumnName] == null ? ' ' : frm[ColumnName].Length > 0 ? Convert.ToChar(frm[ColumnName]) : ' ';
                else if (dataType == "long" || dataType == "bigint")
                    value = frm[ColumnName].Length > 0 ? Convert.ToInt64(frm[ColumnName]) : 0;
                else if (dataType == "money" || dataType == "decimal")
                    value = frm[ColumnName].Length > 0 ? Convert.ToDecimal(frm[ColumnName]) : 0;
                else if (dataType == "float")
                    value = frm[ColumnName].Length > 0 ? Convert.ToDouble(frm[ColumnName]) : 0;
            }
            catch (Exception ex) { }

            return value;
        }

        public static string getSchemaName(Screen_Comp_T s)
        {
            if (s.schemaNameTx != null && !s.schemaNameTx.Trim().Equals("")) return s.schemaNameTx;
            else if (s.ScreenSchemaNameTx != null && !s.ScreenSchemaNameTx.Trim().Equals("")) return s.ScreenSchemaNameTx;
            else if (s.ModuleSchemaNameTx != null && !s.ModuleSchemaNameTx.Trim().Equals("")) return s.ModuleSchemaNameTx;
            else if (s.ApplicationSchemaNameTx != null && !s.ApplicationSchemaNameTx.Trim().Equals("")) return s.ApplicationSchemaNameTx;
            else return s.WebAppSchemaNameTx;
        }

        public static string getSchemaNameById(long App_Module_ID)
        {
            return DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == App_Module_ID).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
        }

        public static void buildTable(Screen_Comp_T s, StringBuilder sb, FormCollection frm)
        {
            string MethodName = string.Empty;
            if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_QUERY)
                MethodName = "query";
            else
                MethodName = "report";

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", s.Id);
            if (s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
            {
                bool useuniqueid = false;
                if (s.dynWhere.Contains(';'))
                {
                    useuniqueid = true;
                }

                if (!useuniqueid)
                {
                    string[] cons = s.dynWhere.Split(',');
                    foreach (string con in cons)
                    {
                        string[] convals = con.Split('=');
                        if (!conditions.ContainsKey(convals[0].Trim()))
                        {
                            if (convals[1].Trim().StartsWith("@"))
                            {
                                if (frm != null)
                                {
                                    if (frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                                    {
                                        conditions.Add(convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                                    }
                                    else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                    {
                                        //conditions.Add(convals[0].Trim(), HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                        string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                        bool proceed = false;
                                        try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                        if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                                    }
                                }
                                else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                {                                    
                                    string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                    bool proceed = false;
                                    try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                    if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                                }
                            }
                            else conditions.Add(convals[0].Trim(), convals[1].Trim());
                        }
                    }
                }
                else
                {
                    string dynWhere = s.dynWhere;
                    dynWhere = dynWhere.Substring(0, dynWhere.IndexOf(';'));
                    string[] convalsunique = dynWhere.Split('=');
                    if (!conditions.ContainsKey(convalsunique[0].Trim()))
                    {
                        if (convalsunique[1].Trim().StartsWith("@"))
                        {
                            if (frm.AllKeys.Contains("ui"))
                            {
                                conditions.Add(convalsunique[0].Trim(), frm["ui"]);
                            }
                        }
                    }
                    dynWhere = s.dynWhere;
                    dynWhere = dynWhere.Replace(dynWhere.Substring(0, dynWhere.IndexOf(';') + 1), "");
                    if (dynWhere != null && !dynWhere.Trim().Equals(""))
                    {
                        string[] cons = dynWhere.Split(',');
                        foreach (string con in cons)
                        {
                            string[] convals = con.Split('=');
                            if (!conditions.ContainsKey(convals[0].Trim()))
                            {
                                if (convals[1].Trim().StartsWith("@"))
                                {
                                    if (frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                                    {
                                        conditions.Add(convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                                    }
                                    else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                    {
                                        //conditions.Add(convals[0].Trim(), HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                        string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                        bool proceed = false;
                                        try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                        if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                                    }
                                }
                                else conditions.Add(convals[0].Trim(), convals[1].Trim());
                            }
                        }
                    }
                }
            }
            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "query" || property.Name == "report")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
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

                string Checkbox = string.Empty;
                bool isCHECKBOX = cols.IndexOf("CHECKBOX") > -1;

                if (isCHECKBOX)
                    Checkbox = "<th class='searchResultsHeading'>Select</th>";
                string Radio = string.Empty;
                bool isRADIO = cols.IndexOf("RADIO") > -1;
                if (isRADIO)
                    Radio = "<th class='searchResultsHeading'>Select</th>";
                bool isDropDown = cols.IndexOf("DROPDOWN") > -1;

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
                        if (!string.IsNullOrEmpty(Checkbox))
                        {
                            sb.Append(Checkbox);
                            Checkbox = string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(Radio))
                        {
                            sb.Append(Radio);
                            Radio = string.Empty;
                        }
                        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                        if (!isColExists) isColExists = true;
                    }
                }
                bool isEdit = cols.IndexOf("EDIT") > -1;
                bool isPreview = cols.IndexOf("VIEW") > -1;

                if (isTableStart)
                {
                    if (isEdit == true || isDropDown == true) sb.Append("<th class='searchResultsHeading'>Action</th>");
                    if (isPreview) sb.Append("<th></th>");
                    sb.Append("</thead></tr><tbody>");

                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                    {
                        sb.Append("<tr>");
                        if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name='chkList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                        if (isRADIO) sb.Append("<td><input type=\"radio\" name='rdoList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"rad_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string cn = dt.Columns[j].ColumnName;
                            string value = Convert.ToString(dt.Rows[i][cn]).Replace(" 00:00:00", "");
                            int pos = cols.IndexOf(cn);
                            if (pos > -1)
                                sb.Append("<td>" + value + "</td>");
                        }
                        if (isEdit)
                        {
                            sb.Append("<td><a href='#'");
                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(s.Screen_Id).Append(")' ");
                            string editcaption = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                            if (string.IsNullOrEmpty(editcaption))
                            {
                                editcaption = "Edit";
                            }
                            sb.Append(">" + editcaption + "</a></td>");
                        }
                        if (isPreview)
                        {
                            string Id = dt.Rows[i]["ID"].ToString();
                            //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                            var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                            Id = value + "_" + Id;
                            if (isIdExists) sb.Append("<td><a href='../Home/DownloadFile/" + Id + "'");
                            //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                            sb.Append(">Click to View</a></td>");
                        }
                        if (isDropDown)
                        {
                            if (s.Screen_Id == 53)
                            {
                                string Id = dt.Rows[i]["TRAINING_CALENDER_ID"].ToString() + "_" + dt.Rows[i]["ID"].ToString();
                                sb.Append("<td><div class=\"dropdown\">"
                                    + "<button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\">View Feedback"
                                     + "<span class=\"caret\"></span></button>"
                                    + "<ul class=\"dropdown-menu\">"
                                        + "<li onclick=\"MoveFeedbackForm(42,'" + Id + "')\">Session Feedback</li>"
                                        + "<li onclick=\"MoveFeedbackForm(44,'" + Id + "')\">Faculty Feedback</li>"
                                        + "<li onclick=\"MoveFeedbackForm(43,'" + Id + "')\">Final Feedback</a></li>"
                                    + "</ul>"
                                    + "</div></td>");
                            }
                        }
                        sb.Append("</tr>");
                    }

                    sb.Append("</tbody></table></div></div></div></div>");
                }
            }
        }



        private static void buildTableWithID(Screen_Comp_T s, StringBuilder sb, FormCollection frm)
        {
            string MethodName = string.Empty;
            MethodName = "query";

            string uniqueID = string.Empty;
            string unqueID = string.Empty;
            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
                object sessionobjvalues;
                if (sessionObjs.TryGetValue("PCS_COMPANY_MASTER_DETAIL_T", out sessionobjvalues))
                {
                    object[] arr = sessionobjvalues as object[];
                    uniqueID = arr[0].ToString();
                    if (frm != null && frm["UNIQUE_REG_ID"] == null)
                    {
                        frm["UNIQUE_REG_ID"] = uniqueID;
                    }
                }

                if (unqueID != null && uniqueID != string.Empty)
                {
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("UNIQUE_REG_ID", unqueID);
                    DataTable dtb = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", cond, null, 0, 1);

                    if (dtb != null && dtb.Rows.Count > 0 && dtb.Rows[0]["ID"] != null)
                    {
                        frm["COMP_TYPE_ID"] = Convert.ToString(dtb.Rows[0]["COMPANY_TYPE_ID"]);
                    }
                }
            }
            /*if(frm["UNIQUE_REG_ID"]!=null && frm["UNIQUE_REG_ID"] != string.Empty)
            {
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                unqueID = frm["UNIQUE_REG_ID"].ToString();
            }
            else*/
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                frm["UNIQUE_REG_ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
                unqueID = frm["UNIQUE_REG_ID"].ToString();
            }



            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", s.Id);
            if (s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
            {
                string[] cons = s.dynWhere.Split(',');
                foreach (string con in cons)
                {
                    string[] convals = con.Split('=');
                    if (!conditions.ContainsKey(convals[0].Trim()))
                    {
                        if (convals[1].Trim().StartsWith("@"))
                        {
                            if (frm != null && frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                            {
                                conditions.Add(convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                            }
                            else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                            {
                                //conditions.Add(convals[0].Trim(), HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                bool proceed = false;
                                try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                            }
                        }
                        else conditions.Add(convals[0].Trim(), convals[1].Trim());
                    }
                }
            }
            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
            DataTable dt = null;
            if (jdata != null)
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "query" || property.Name == "report")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
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

                string Checkbox = string.Empty;
                bool isCHECKBOX = cols.IndexOf("CHECKBOX") > -1;
                if (isCHECKBOX)
                    Checkbox = "<th class='searchResultsHeading'>Select</th>";
                bool isDropDown = cols.IndexOf("DROPDOWN") > -1;

                List<string> hiddens = new List<string>();
                StringBuilder hidden = new StringBuilder();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (cols.IndexOf("HIDDEN_" + dt.Columns[i].ColumnName) != -1)
                    {
                        hiddens.Add(dt.Columns[i].ColumnName);
                    }
                    string cn = dt.Columns[i].ColumnName;
                    if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                    int pos = cols.IndexOf(cn);
                    if (pos > -1)
                    {
                        if (!isTableStart)
                        {
                            isTableStart = true;
                            sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(s.compTextTx).Append("</span></h4><div style='overflow-x:auto;'><table id='").Append(s.compTextTx.Replace(" ", "")).Append("'").Append("class='table table-bordered tableSearchResults'");
                            if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                            if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                            sb.Append("><thead>");
                            sb.Append("<tr>");
                        }
                        cn = colvals[pos];
                        if (!string.IsNullOrEmpty(Checkbox))
                        {
                            sb.Append(Checkbox);
                            Checkbox = string.Empty;
                        }
                        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                        if (!isColExists) isColExists = true;
                    }
                }
                bool isEdit = cols.IndexOf("EDIT") > -1;
                bool isEditSpl = cols.IndexOf("EDIT_SPL") > -1;
                bool isPreview = cols.IndexOf("VIEW") > -1;
                bool isRemove = cols.IndexOf("REMOVE") > -1;
                bool isDropDownSpl = cols.IndexOf("DROPDOWN_SPL") > -1;
                bool isDropDownSplMul = cols.IndexOf("DROPDOWN_SPL_MUL") > -1;
                bool isPreviewSpl = cols.IndexOf("PREVIEW_SPL") > -1;
                bool isDownload = cols.IndexOf("DOWNLOAD") > -1;
                bool isDownloadSpl = cols.IndexOf("DOWNLOAD_SPL") > -1;
                bool isApprovalLetter = cols.IndexOf("APPROVAL") > -1;
                bool isQRTR = false;// cols.IndexOf("QRTR") > -1;


                if (isTableStart)
                {
                    if (isEdit == true || isDropDown == true) sb.Append("<th class='searchResultsHeading'>Action</th>");
                    if (isEditSpl == true || isDropDown == true) sb.Append("<th class='searchResultsHeading'>Action</th>");
                    if (isPreview) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                    if (isRemove) sb.Append("<th class='searchResultsHeading'>Delete</th>");
                    if (isDropDownSpl) sb.Append("<th class='searchResultsHeading'>Status</th>");
                    if (isPreviewSpl) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                    if (isDownload) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                    if (isDownloadSpl) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                    if (isDropDownSplMul) sb.Append("<th class='searchResultsHeading'>Status</th>");
                    if (isApprovalLetter) sb.Append("<th class='searchResultsHeading'>Download</th>");

                    sb.Append("</thead></tr><tbody>");

                    string EditScreenId = string.Empty;
                    EditScreenId = Convert.ToString((DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == Convert.ToInt32(s.Screen_Id)).FirstOrDefault())["EDIT_SCREEN_ID"]);

                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                    {
                        foreach (string hdstr in hiddens)
                        {
                            hidden.Append("<input type='hidden' name='").Append(hdstr).Append("' value='").Append(dt.Rows[i][hdstr].ToString()).Append("_").Append(dt.Rows[i]["ID"].ToString()).Append("'>");
                        }
                        sb.Append("<tr>");
                        if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name='chkList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string cn = dt.Columns[j].ColumnName;
                            string value = Convert.ToString(dt.Rows[i][cn]).Replace(" 00:00:00", "");
                            int pos = cols.IndexOf(cn);
                            if (pos > -1)
                                sb.Append("<td>" + value + "</td>");
                        }
                        if (isEdit)
                        {
                            sb.Append("<td><a href='#'");
                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(EditScreenId != null && EditScreenId != string.Empty ? Convert.ToInt32(EditScreenId) : s.Screen_Id).Append(")' ");
                            string editcaption = colvals.Contains("EDIT") ? null : colvals[cols.IndexOf("EDIT")];
                            if (string.IsNullOrEmpty(editcaption))
                            {
                                editcaption = "Edit";
                            }
                            sb.Append(">" + editcaption + "</a></td>");
                        }
                        if (isQRTR)
                        {
                            sb.Append("<td><a href='#'");
                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append("0,").Append(s.Screen_Id).Append(",").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                            string editcaption = colvals.Contains("QRTR") ? colvals[cols.IndexOf("EDIT")] : "Quarterly";
                            if (string.IsNullOrEmpty(editcaption))
                            {
                                editcaption = "Edit";
                            }
                            sb.Append(">" + editcaption + "</a></td>");
                        }

                        if (isEditSpl)
                        {
                            string scrId = s.Screen_Id.ToString();

                            //ANONYMOUS PCS / Company Pending or in progress application Application
                            if (HttpContext.Current.Session["LOGIN_ID"].ToString() == "pcscomp")
                            {
                                if (dt.Columns.Contains("STATUS_TX") && (dt.Rows[i]["STATUS_TX"].ToString() == "in-progress" || dt.Rows[i]["STATUS_TX"].ToString() == "pending") && dt.Columns.Contains("REG_COM_TYPE_ID"))
                                {
                                    scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1") ? "107" : "106";
                                }
                                sb.Append("<td><a");
                                if (isIdExists) sb.Append(" onclick=").Append("'screenAction(").Append(scrId).Append(",").Append(dt.Rows[i]["ID"].ToString()).Append(")'");
                                string editcaptionS = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                                if (string.IsNullOrEmpty(editcaptionS))
                                {
                                    editcaptionS = "Edit";
                                }
                                sb.Append(">" + editcaptionS + "</a></td>");
                            }
                            //NON-ANONYMOUS PCS / Company Pending or in progress application Application
                            if (HttpContext.Current.Session["LOGIN_ID"].ToString() != "pcscomp")
                            {
                                if (dt.Columns.Contains("STATUS_TX") && (dt.Rows[i]["STATUS_TX"].ToString() == "in-progress" || dt.Rows[i]["STATUS_TX"].ToString() == "pending" || dt.Rows[i]["STATUS_TX"].ToString() == "call for") && dt.Columns.Contains("REG_COM_TYPE_ID"))
                                {
                                    scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1") ? "107" : "106";
                                }
                                sb.Append("<td><a");
                                string unique_reg_id = dt.Rows[i]["UNIQUE_REG_ID"].ToString();// Int64.Parse(dt.Rows[i]["UNIQUE_REG_ID"].ToString());
                                //if (isIdExists) sb.Append(" onclick=").Append("'loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(scrId).Append(",").Append(unique_reg_id).Append(")'");
                                string LoadRecord = "loadRecord(" + dt.Rows[i]["ID"].ToString() + "," + scrId + ",'" + unique_reg_id + "')";
                                if (isIdExists) sb.Append(" onclick=").Append("\"").Append(LoadRecord).Append("\"");
                                string editcaptionS = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                                if (string.IsNullOrEmpty(editcaptionS))
                                {
                                    editcaptionS = "Edit";
                                }
                                sb.Append(">" + editcaptionS + "</a></td>");
                            }

                        }
                        if (isPreview)
                        {
                            string Id = dt.Rows[i]["ID"].ToString();
                            //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                            var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                            Id = value + "_" + Id;
                            if (isIdExists) sb.Append("<td><a href='../PCS/DownloadFilePCS/" + Id + "'");
                            //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                            sb.Append("class='button'>Preview</a></td>");
                        }
                        if (isApprovalLetter)
                        {
                            string Id = dt.Rows[i]["ID"].ToString();
                            //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                            //var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                            //Id = Id;
                            if (isIdExists) sb.Append("<td><a href='../PCS/DownloadApprovalLetterPCS/" + Id + "'");
                            //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                            sb.Append("class='button'>Preview</a></td>");
                        }
                        if (isDownload)
                        {
                            string Id = (dt.Columns.Contains("DOC_ID")) ? dt.Rows[i]["DOC_ID"].ToString() : dt.Rows[i]["ID"].ToString();
                            if (isIdExists) sb.Append("<td><a href='../Home/DownloadFileByID/" + Id + "'");
                            sb.Append("class='button'>Preview</a></td>");
                        }

                        if (isDownloadSpl)
                        {
                            string Id = (dt.Columns.Contains("DOC_ID")) ? dt.Rows[i]["DOC_ID"].ToString() : dt.Rows[i]["ID"].ToString();
                            if (isIdExists) sb.Append("<td><a ").Append(" data-value='").Append(dt.Rows[i]["ID"].ToString()).Append("' href ='../Home/DownloadFileByID/" + Id + "'");
                            sb.Append("class='button'>Preview</a></td>");
                        }

                        if (isPreviewSpl)
                        {
                            string Id = dt.Rows[i]["ID"].ToString();
                            //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                            //var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                            //Id = value + "_" + Id;
                            if (isIdExists) sb.Append("<td><a ");
                            if (isIdExists) sb.Append(" data-value='").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append("151").Append("' ");
                            sb.Append("class='button' name='poprpt'>Preview</a></td>");
                        }
                        if (isRemove)
                        {
                            string Id = dt.Rows[i]["ID"].ToString();
                            if (isIdExists) sb.Append("<td> <button type ='submit' class='btn btn-danger btn-xs' name='deleteDoc' value='" + Id + "'");
                            //< button type = "submit"  class="btn btn-primary SubmitBtn"> Submit</button>
                            sb.Append(">Remove</button></td>");
                        }
                        if (isDropDown)
                        {
                            if (s.Screen_Id == 53)
                            {
                                string Id = dt.Rows[i]["TRAINING_CALENDER_ID"].ToString() + "_" + dt.Rows[i]["ID"].ToString();
                                sb.Append("<td><div class=\"dropdown\">"
                                    + "<button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\">View Feedback"
                                     + "<span class=\"caret\"></span></button>"
                                    + "<ul class=\"dropdown-menu\">"
                                        + "<li onclick=\"MoveFeedbackForm(42,'" + Id + "')\">Session Feedback</li>"
                                        + "<li onclick=\"MoveFeedbackForm(44,'" + Id + "')\">Faculty Feedback</li>"
                                        + "<li onclick=\"MoveFeedbackForm(43,'" + Id + "')\">Final Feedback</a></li>"
                                    + "</ul>"
                                    + "</div></td>");
                            }
                        }
                        if (isDropDownSpl)
                        {
                            sb.Append("<td>");
                            string Id = dt.Rows[i]["ID"].ToString();
                            sb.Append("<select class='form-control panelInputs' name='").Append("DOCUMENT_STATUS").Append("'");
                            sb.Append(">");
                            sb.Append("<option value=''>").Append("-- Select --</option>");
                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Approve")
                            {
                                sb.Append("<option selected=" + "selected" + " value=" + Id + "_Approve>").Append("Approve</option>");
                            }
                            else
                            {
                                sb.Append("<option value=" + Id + "_Approve>").Append("Approve</option>");
                            }

                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Reject")
                            {
                                sb.Append("<option selected=" + "selected" + " value=" + Id + "_Reject>").Append("Reject</option>");
                            }
                            else
                            {
                                sb.Append("<option value=" + Id + "_Reject>").Append("Reject</option>");
                            }

                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Call")
                            {
                                sb.Append("<option selected=" + "selected" + " value=" + Id + "_Call>").Append("Call For</option>");
                            }
                            else
                            {
                                sb.Append("<option value=" + Id + "_Call>").Append("Call For</option>");
                            }
                            sb.Append(" </select>");
                            sb.Append("</td>");
                        }
                        if (isDropDownSplMul)
                        {
                            sb.Append("<td>");
                            string Id = dt.Rows[i]["ID"].ToString();
                            sb.Append("<select class='form-control panelInputs' name='").Append("DOCUMENT_STATUS").Append("'");
                            sb.Append(">");
                            sb.Append("<option value=''>").Append("-- Select --</option>");
                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Approve")
                            {
                                sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Approve'>").Append("Approve</option>");
                            }
                            else
                            {
                                sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Approve'>").Append("Approve</option>");
                            }

                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Reject")
                            {
                                sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Reject'>").Append("Reject</option>");
                            }
                            else
                            {
                                sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Reject'>").Append("Reject</option>");
                            }

                            if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Call")
                            {
                                sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Call'>").Append("Call For</option>");
                            }
                            else
                            {
                                sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Call'>").Append("Call For</option>");
                            }
                            sb.Append(" </select>");
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }

                    sb.Append("</tbody></table></div></div></div></div>");
                    sb.Append(hidden.ToString());
                }
            }
        }

        private static void buildDropDown(Screen_Comp_T s, StringBuilder sb, FormCollection frm)
        {
            string uniqueID = string.Empty;

            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
                object sessionobjvalues;
                if (sessionObjs.TryGetValue("PCS_COMPANY_MASTER_DETAIL_T", out sessionobjvalues))
                {
                    object[] arr = sessionobjvalues as object[];
                    uniqueID = arr[0].ToString();

                }

                if (uniqueID != null && uniqueID != string.Empty)
                {
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("UNIQUE_REG_ID", uniqueID);
                    DataTable dtb = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", cond, null, 0, 1);

                    if (dtb != null && dtb.Rows.Count > 0 && dtb.Rows[0]["ID"] != null)
                    {
                        frm["COMP_TYPE_ID"] = Convert.ToString(dtb.Rows[0]["COMPANY_TYPE_ID"]);
                    }
                }
            }
            string selected_value = string.Empty;

            if (frm != null && frm[s.compNameTx] != null)
            {
                selected_value = frm[s.compNameTx];
            }
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", s.Id);
            if (s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
            {
                string[] cons = s.dynWhere.Split(',');
                foreach (string con in cons)
                {
                    string[] convals = con.Split('=');
                    if (!conditions.ContainsKey(convals[0].Trim()))
                    {
                        if (convals[1].Trim().StartsWith("@") && Convert.ToString(convals[1].Trim().Substring(1)) != "0")
                        {
                            if (frm != null && frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                            {
                                conditions.Add(convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                            }
                            else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                            {
                                //conditions.Add(convals[0].Trim(), HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                bool proceed = false;
                                try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                            }
                        }
                        else conditions.Add(convals[0].Trim(), convals[1].Trim());
                    }
                }
            }
            JObject jdata = DBTable.GetData("query", conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
            DataTable dt = new DataTable();
            if (jdata != null)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "query" || property.Name == "report")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
            }

            if (s.Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY)
                sb.Append("<select class='form-control panelInputs' name='").Append(s.compNameTx).Append("'");
            else
                sb.Append("<select class='form-control panelInputs' multiple='multiple' size=" + (dt != null ? (dt.Rows.Count + 1) : 0) + " name='").Append(s.compNameTx).Append("'");

            if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
            if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
            sb.Append(">");
            //sb.Append("<option value=0>").Append("-- Select --</option>");
            sb.Append("<option value=''>").Append("-- Select --</option>");
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //int key = Convert.ToInt32(dt.Rows[i][0]);
                        string key = Convert.ToString(dt.Rows[i][0]);// Convert.ToInt32(dt.Rows[i][0]);
                        string value = Convert.ToString(dt.Rows[i][1]);

                        if (!string.IsNullOrEmpty(selected_value) && selected_value == key)
                        {
                            sb.Append("<option selected value='" + key + "'>").Append("" + value + "</option>");
                        }
                        else
                        {
                            sb.Append("<option value='" + key + "'>").Append("" + value + "</option>");

                        }
                    }
                }
                //sb.Append(" </select>");
            }/*else if (s.Comp_Type_Nm == (Int32)HTMLTag.LIST)
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", s.Id);

                JObject jdata = DBTable.GetData("query", null, "SCREEN_COMP_T", 0, 100, string.Empty);
                DataTable dt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "query" || property.Name == "report")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    sb.Append("<select size='5' name='").Append(s.compNameTx).Append("'");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int key = Convert.ToInt32(dt.Rows[i][0]);
                        string value = Convert.ToString(dt.Rows[i][1]);
                        sb.Append("<option value=" + key + ">").Append("'" + value + "'</option>");
                    }
                    sb.Append(" </select>");
                }
            }*/
            sb.Append(" </select>");
        }


        public static void renderScreenComponent(Screen_T screen, List<Screen_Comp_T> screenComponents, StringBuilder sb, FormCollection frm)
        {
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

                if (s.Comp_Type_Nm < (Int32)HTMLTag.TEXTAREA)
                {
                    sb.Append("<input type='").Append(s.Comp_Type_Nm == (Int32)HTMLTag.HIDDEN ?
                        "hidden" : (s.Comp_Type_Nm == (Int32)HTMLTag.TEXT ? "text" :
                        (s.Comp_Type_Nm == (Int32)HTMLTag.PASSWORD ? "password" :
                        (s.Comp_Type_Nm == (Int32)HTMLTag.RADIO_GROUP ||
                        s.Comp_Type_Nm == (Int32)HTMLTag.RADIO_BUTTON ? "radio" : "checkbox")))).Append("' name='").Append(s.compNameTx).Append("' value='");
                    if (s.tableNameTx != null && s.compNameTx.Equals(s.tableNameTx)) sb.Append("@").Append(s.compNameTx);
                    else sb.Append(s.compValueTx);
                    sb.Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(" />");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TEXTAREA)
                {
                    sb.Append("<textarea cols='100' rows='10' name='").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    if (s.compNameTx.Equals(s.tableNameTx)) sb.Append("@").Append(s.compNameTx);
                    else sb.Append(s.compValueTx);
                    sb.Append(s.compNameTx).Append(" </textarea>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_QUERY_AND_ID)
                {
                    buildTableWithID(s, sb, frm);
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY || s.Comp_Type_Nm == (Int32)HTMLTag.LIST)
                {
                    buildDropDown(s, sb, frm);
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.SUBMIT || s.Comp_Type_Nm == (Int32)HTMLTag.BUTTON)
                {
                    string ButtonType = string.Empty;
                    if (s.Comp_Type_Nm == (Int32)HTMLTag.SUBMIT)
                        ButtonType = "submit";
                    else
                        ButtonType = "button";

                    sb.Append("<input type='" + ButtonType + "' value='").Append(s.compValueTx).Append("' name='").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    //if (s.compNameTx.Equals(s.tableNameTx)) sb.Append("@").Append(s.compNameTx);
                    //else sb.Append(s.compValueTx);
                    sb.Append(s.compNameTx).Append(" </input>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_QUERY || s.Comp_Type_Nm == (Int32)HTMLTag.REPORT)
                {
                    buildTable(s, sb, frm);
                }

                else if (s.Comp_Type_Nm == (Int32)HTMLTag.DIV)
                {
                    sb.Append("<Div id='").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    sb.Append(s.compTextTx).Append(" </Div>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.SPAN)
                {
                    sb.Append("<span id='").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    sb.Append(s.compTextTx).Append(" </span>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.Tr || s.Comp_Type_Nm == (Int32)HTMLTag.Td || s.Comp_Type_Nm == (Int32)HTMLTag.Th)
                {
                    string TagName = string.Empty;
                    if (s.Comp_Type_Nm == (Int32)HTMLTag.Tr)
                        TagName = "Tr";
                    else if (s.Comp_Type_Nm == (Int32)HTMLTag.Td)
                        TagName = "Td";
                    else
                        TagName = "Th";

                    sb.Append("<" + TagName + " name='").Append(s.compNameTx).Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(">");
                    sb.Append(s.compNameTx).Append(" </" + TagName + ">");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.IMAGE)
                {
                    string imgsrc = string.Empty;
                    if (!string.IsNullOrEmpty(s.compContentTx))
                        imgsrc = "data:image/png;base64, " + s.compContentTx;
                    else
                        imgsrc = "#";
                    sb.Append("<image name='").Append(s.compNameTx).Append("' ");
                    sb.Append("src='" + imgsrc + "'");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    sb.Append(">");
                    sb.Append(s.compNameTx).Append(" </image>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.ANCHOR)
                {
                    string anchref = string.Empty;
                    if (!string.IsNullOrEmpty(s.compContentTx))
                        anchref = s.compContentTx;
                    else
                        anchref = "#";
                    sb.Append("<a name='").Append(s.compNameTx).Append("' ");
                    sb.Append("href='" + anchref + "'");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append("onclick='").Append(s.compScriptTx).Append("' ");
                    sb.Append(">");
                    sb.Append(s.compNameTx).Append(" </a>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.FORM)
                {
                    sb.Append("<form name='").Append(s.compNameTx).Append("' ");
                    sb.Append("action='" + s.compContentTx + "'");
                    sb.Append(">");
                    sb.Append(s.compNameTx).Append(" </form>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.CUSTOM)
                {
                    sb.Append("<input type='hidden' name='sc_comp_id' value='").Append(s.Id).Append("'>");
                    sb.Append(s.compContentTx);
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.DATE || s.Comp_Type_Nm == (Int32)HTMLTag.DATETIME)
                {
                    sb.Append("<input type='date' name='").Append(s.compNameTx).Append("' id='").Append(s.compNameTx).Append("' value='");
                    if (s.compNameTx.Equals(s.columnNameTx)) sb.Append("@").Append(s.compNameTx); // TODO - nneds to check
                    else sb.Append(s.compValueTx);
                    sb.Append("' ");
                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                    sb.Append(" />");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.SEARCH)
                {
                    if (screen.Action_Tx.ToString() == "reports")
                    {
                        sb.Append("<div class=\"panelSpacing\" id=\"SearchCriteriaId\"><div class='panel panel-primary searchCtr'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>Report Criteria</span></h4><div class='row'>");
                        //if(s.compContentTx!=null)sb.Append(s.compContentTx);
                        sb.Append("<input type='hidden' name='scmpid' value='").Append(s.Id).Append("'>");
                        if (s.ScreenCompList != null)
                        {
                            for (int i = 0; i < s.ScreenCompList.Count; i++)
                            {
                                List<Screen_Comp_T> l = new List<Screen_Comp_T>();
                                l.Add(s.ScreenCompList[i]);
                                sb.Append("<div class='col-md-6' style='margin - top: 7px;'><div class='form-inline'><label class='col-md-5 control-label TrainingSchdlLabelTxt FormInputsLabel FacltyNamTxt FacltyNamTxt'>").Append(s.ScreenCompList[i].compTextTx).Append("</label><div class='col-md-7  '><div class='input-group col-md-7 col-xs-11 col-xs-11 IpadInput mobilInput3 panelInputsSub");
                                if (s.ScreenCompList[i].Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY)
                                    sb.Append(" selectoptionBg");
                                if (s.ScreenCompList[i].compNameTx != null && !s.ScreenCompList[i].compNameTx.StartsWith("MANSCR_"))
                                {
                                    sb.Append(" '><input type='hidden' name='COND_").Append(s.ScreenCompList[i].compNameTx).Append("' value='").Append(s.ScreenCompList[i].where).Append("' >");
                                    s.ScreenCompList[i].compNameTx = "SCRH_" + s.ScreenCompList[i].compNameTx;
                                }
                                else sb.Append(" '>");
                                renderScreenComponent(screen, l, sb, frm);
                                sb.Append("</div></div></div></div>");
                            }
                        }
                        sb.Append("</div><div class=''><div class='form-group'><button type='button' onclick=\"searchPage(").Append(screen.ID).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Get Report</button></div></div></div></div></div>");
                        sb.Append("<div id='divSearchResult' style='display: none'></div>");
                    }
                    else
                    {
                        sb.Append("<div class=\"panelSpacing\" id=\"SearchCriteriaId\"><div class='panel panel-primary searchCtr'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>Search Criteria</span></h4><div class='row'>");
                        //if(s.compContentTx!=null)sb.Append(s.compContentTx);
                        sb.Append("<input type='hidden' name='scmpid' value='").Append(s.Id).Append("'>");
                        if (s.ScreenCompList != null)
                        {
                            for (int i = 0; i < s.ScreenCompList.Count; i++)
                            {
                                List<Screen_Comp_T> l = new List<Screen_Comp_T>();
                                l.Add(s.ScreenCompList[i]);
                                sb.Append("<div class='col-md-6' style='margin - top: 7px;'><div class='form-inline'><label class='col-md-5 control-label TrainingSchdlLabelTxt FormInputsLabel FacltyNamTxt FacltyNamTxt'>").Append(s.ScreenCompList[i].compTextTx).Append("</label><div class='col-md-7 '><div class='input-group col-md-7 col-xs-11 col-xs-11 IpadInput mobilInput3 panelInputsSub");
                                if (s.ScreenCompList[i].Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY)
                                    sb.Append(" selectoptionBg");
                                if (s.ScreenCompList[i].compNameTx != null && !s.ScreenCompList[i].compNameTx.StartsWith("MANSCR_"))
                                {
                                    sb.Append(" '><input type='hidden' name='COND_").Append(s.ScreenCompList[i].compNameTx).Append("' value='").Append(s.ScreenCompList[i].where).Append("' >");
                                    s.ScreenCompList[i].compNameTx = "SCRH_" + s.ScreenCompList[i].compNameTx;
                                }
                                else sb.Append(" '>");
                                renderScreenComponent(screen, l, sb, frm);
                                sb.Append("</div></div></div></div>");
                            }
                        }
                        sb.Append("</div><div class=''><div class='form-group'><button type='button' onclick=\"searchPage(").Append(screen.ID).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Search</button></div></div></div></div></div>");
                        sb.Append("<div id='divSearchResult' style='display: none'></div>");
                    }
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.ADD_MORE)
                {
                    sb.Append("<div class=\"panelSpacing\" id=\"SearchCriteriaId\"><div class='panel panel-primary searchCtr'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(s.compTextTx == null ? "" : s.compTextTx).Append("</span></h4><div class='row'>");
                    //if(s.compContentTx!=null)sb.Append(s.compContentTx);
                    sb.Append("<input type='hidden' name='scmpid' value='").Append(s.Id).Append("'>");
                    if (s.ScreenCompList != null)
                    {
                        for (int i = 0; i < s.ScreenCompList.Count; i++)
                        {
                            List<Screen_Comp_T> l = new List<Screen_Comp_T>();
                            l.Add(s.ScreenCompList[i]);
                            sb.Append("<div class='col-md-6'><div class='form-inline'><label class='col-md-5 control-label TrainingSchdlLabelTxt FormInputsLabel FacltyNamTxt FacltyNamTxt'>").Append(s.ScreenCompList[i].compTextTx).Append("</label><div class='col-md-7  '><div class='input-group col-md-7 col-xs-11 col-xs-11 IpadInput mobilInput3 panelInputsSub");
                            if (s.ScreenCompList[i].Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY)
                                sb.Append(" selectoptionBg");
                            sb.Append(" '><input type='hidden' name='COND_").Append(s.ScreenCompList[i].compNameTx).Append("' value='").Append(s.ScreenCompList[i].where).Append("' >");
                            s.ScreenCompList[i].compNameTx = "SCRH_" + s.ScreenCompList[i].compNameTx;
                            renderScreenComponent(screen, l, sb, frm);
                            sb.Append("</div></div></div></div>");
                        }
                    }
                    sb.Append("</div><div class=''><div class='form-group'><button type='button' onclick=\"searchPage(").Append(screen.ID).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Search</button></div></div></div></div></div>");
                    sb.Append("<div id='divSearchResult' style='display: none'></div>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.UPLOAD_DOCUMENTS)
                {
                    // COMP_VALUE_TX has DOCUMENT_MODULE_ID
                    // GET ALL THE DOCUMENT MODULE TRANSACTIONS
                    //frm["UPD_ID"] = s.compValueTx;
                    //s.Comp_Type_Nm = (Int32)HTMLTag.DROPDOWN_WITH_QUERY;
                    int docModuleId = 0;
                    if (frm != null && frm["DID"] != null)
                    {
                        docModuleId = Convert.ToInt32(frm["DID"]);
                    }
                    else
                    {
                        docModuleId = Convert.ToInt32(s.reportId);
                    }
                    string MethodName = "qfetch";

                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ID", docModuleId);
                    conditions.Add("QID", 1); // for fetching the drop down data                    
                    JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    DataTable dt = new DataTable();
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "qfetch")
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }

                    if (s.compValueTx != null && s.compValueTx.Trim().Equals("1")) sb.Append("<div class='panel panel-primary searchCtr'>");
                    sb.Append("<div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>");
                    if (s.compTextTx != null && !s.compTextTx.Trim().Equals("")) sb.Append(s.compTextTx);
                    else sb.Append("Upload Documents");
                    sb.Append("</span></h4><div class='row'><div><label class='col-md-2 control-label'>Document Type</label><div class='col-md-4'><div class='input-group col-md-12 col-xs-12 col-sm-11 selectoptionBg1'>");
                    sb.Append("<select class='form-control panelInputs' name='updtype").Append(s.compNameTx).Append("'");

                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append(" style='").Append(s.compStyleTx).Append("' ");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);

                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    sb1.Append("><option value=''>").Append("-- Select --</option>");
                                    sb.Append(" onchange= selectSTDocumentType(" + s.Id.ToString() + ",'").Append(s.compNameTx).Append("',["); // corrected by Shashank
                                    //sb.Append(" onchange=\"selectSTDocumentType('").Append(s.compNameTx).Append("','"); // Commented by Shashank
                                }

                                sb2.Append("\"").Append(Convert.ToString(dt.Rows[i][2])).Append("\","); // Corrected by Shashank    
                                string key = Convert.ToString(dt.Rows[i][0]);// Convert.ToInt32(dt.Rows[i][0]);
                                string value = Convert.ToString(dt.Rows[i][1]);
                                sb1.Append("<option value='").Append(key).Append("'>").Append(value).Append("</option>");

                            }

                            // sb.Append("') "); // Commented by Shashank
                        }
                        else sb1.Append("><option value=''>").Append("-- Select --</option>");
                    }
                    else sb1.Append("><option value=''>").Append("-- Select --</option>");
                    if (sb2.Length > 0)
                    {
                        sb2.Remove(sb2.Length - 1, 1);
                        sb.Append(sb2);
                    }
                    sb.Append("])").Append(sb1.ToString()).Append(" </select></div></div><div class='col-md-6' id='fl").Append(s.compNameTx).Append("div' style='display:none'><div class='form-inline'><input id='commonuploadfile' name='file").Append(s.compNameTx).Append("' placeholder=' ' class='form-control' type='file'>");
                    sb.Append("<button type='button'");
                    sb.Append("onclick =uploadSTDocument('").Append(s.compNameTx).Append("'");
                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(",").Append(s.compScriptTx);
                    sb.Append(")"); // Commented by Shashank
                    sb.Append(" class='btn btn-primary'>Upload</button> </div></div></div></div></div>");
                    sb.Append("<div class='row mt-15'></div>");
                    sb.Append("<div id='divUploaded'>");
                    List<DocumentReference> docs = null;// 
                    if (HttpContext.Current.Session["UPLOAD_DOCUMENTS"] != null)
                    {
                        docs = (List<DocumentReference>)HttpContext.Current.Session["UPLOAD_DOCUMENTS"];
                    }
                    else
                    {
                        docs = new List<DocumentReference>();
                        HttpContext.Current.Session["UPLOAD_DOCUMENTS"] = docs;
                    }

                    List<DocumentReference> Loaddocs = null;// 
                    if (HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"] != null)
                    {
                        Loaddocs = (List<DocumentReference>)HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"];
                    }
                    else
                    {
                        Loaddocs = new List<DocumentReference>();
                        HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"] = Loaddocs;
                    }

                    // sb.Append("<div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'>");

                    MethodName = "query";
                    conditions.Clear();
                    //Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ID", s.Id);
                    int refid = 0;
                    int.TryParse(frm["ui"], out refid);
                    conditions.Add("REF_ID", refid);
                    jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    dt = new DataTable();
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "query")
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }
                    List<string> cols = new List<string>();
                    if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                    List<string> colvals = new List<string>();
                    if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();
                    bool isDownload = cols.IndexOf("DOWN_YN") > -1;
                    bool isPreview = cols.IndexOf("DELETE_YN") > -1;
                    bool isDelete = cols.IndexOf("VIEW_YN") > -1;

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DocumentReference doc = new DocumentReference();
                            doc.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                            doc.SCREEN_ID = Convert.ToInt32(dt.Rows[i]["SCREEN_ID"]); //screen.ID;
                            doc.REF_ID = Convert.ToInt32(dt.Rows[i]["REF_ID"]); //s.Id;
                            doc.DOC_MOD_TRANS_ID = Convert.ToInt32(dt.Rows[i]["DOC_MOD_TRANS_ID"]);
                            doc.FILE_NAME_TX = Convert.ToString(dt.Rows[i]["FILE_NAME_TX"]);
                            byte[] fl_blb = File.ReadAllBytes(Convert.ToString(dt.Rows[i]["FILE_BLB"]));
                            // byte[] fl_blb = (byte[])dt.Rows[i]["FILE_BLB"];
                            //Stream blb = new MemoryStream(fl_blb);
                            // doc.FILE_BLB = Convert.ToString(dt.Rows[i]["FILE_BLB"]); //fl_blb;// blb;
                            doc.FILE_BLB = fl_blb;
                            doc.FILE_TYPES_TX = Convert.ToString(dt.Rows[i]["FILE_TYPES_TX"]);
                            doc.DOC_TYPES_TX = Convert.ToString(dt.Rows[i]["DOC_TYPES_TX"]);
                            doc.UPLOADED_ON = Convert.ToString(dt.Rows[i]["UPLOADED_ON"]);
                            doc.DELETE_YN = isDelete;
                            doc.VIEW_YN = isPreview;
                            doc.DOWNLOAD_YN = isDownload;
                            doc.isRemoved = false;
                            doc.isNew = false;
                            docs.Add(doc);
                            Loaddocs.Add(doc);
                        }
                        HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"] = Loaddocs;
                        HttpContext.Current.Session["UPLOAD_DOCUMENTS"] = docs;
                        string tblid = "tableuploaded" + s.Id.ToString();
                        string htmlstring = string.Empty;
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
                            htmlstring += "</tbody></table> ";
                        }
                        sb.Append(htmlstring);
                        //bool isIdExists = false;
                        //bool isTableStart = false;
                        //bool isColExists = false;
                        //int colsCount = 0;
                        //for (int i = 0; i < dt.Columns.Count; i++)
                        //{

                        //    string cn = dt.Columns[i].ColumnName;
                        //    if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                        //    int pos = cols.IndexOf(cn);
                        //    if (pos > -1)
                        //    {

                        //        if (!isTableStart)
                        //        {
                        //            sb.Append("<table class='table table - bordered tableSearchResults'> <thead><tr class='bg'><th class='searchResultsHeading'>SL.No.</th>");
                        //            isTableStart = true;
                        //        }

                        //        if (cn.Trim().ToUpper().Equals("DOWN_YN")) cn = "Download";
                        //        if (cn.Trim().ToUpper().Equals("DELETE_YN")) cn = "Remove";
                        //        else if (cn.Trim().ToUpper().Equals("VIEW_YN")) cn = "Preview";
                        //        else if (!isColExists) { cn = colvals[pos]; ++colsCount; isColExists = true; }
                        //        else if (isColExists) continue;
                        //        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                        //    }

                        //}

                        //if (isTableStart)
                        //{
                        //    sb.Append("</thead><tbody>");
                        //    for (int i = 0; i < dt.Rows.Count; i++)
                        //    {  
                        //        DocumentReference doc = new DocumentReference();
                        //        doc.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                        //        doc.SCREEN_ID = Convert.ToInt32(dt.Rows[i]["SCREEN_ID"]); //screen.ID;
                        //        doc.REF_ID = Convert.ToInt32(dt.Rows[i]["REF_ID"]); //s.Id;
                        //        doc.DOC_MOD_TRANS_ID = Convert.ToInt32(dt.Rows[i]["DOC_MOD_TRANS_ID"]);
                        //        doc.FILE_NAME_TX = Convert.ToString(dt.Rows[i]["FILE_NAME_TX"]);
                        //        byte[] fl_blb = (byte[])dt.Rows[i]["FILE_BLB"];
                        //        Stream blb = new MemoryStream(fl_blb);
                        //        doc.FILE_BLB = blb;
                        //        //doc.FILE_BLB = dt.Rows[i]["FILE_BLB"];
                        //        doc.FILE_TYPES_TX = Convert.ToString(dt.Rows[i]["FILE_TYPES_TX"]);
                        //        doc.DELETE_YN = isDelete;
                        //        doc.VIEW_YN = isPreview;
                        //        doc.DOWNLOAD_YN = isDownload;
                        //        doc.isRemoved = false;
                        //        doc.isNew = false;
                        //        docs.Add(doc);
                        //        sb.Append("<tr><td>").Append(i + 1).Append("<input type='hidden' name='ID").Append(i).Append("' value='").Append(dt.Rows[i]["ID"]).Append("'></td>");
                        //        for (int j = 0; j < dt.Columns.Count; j++)
                        //        {
                        //            string cn = dt.Columns[j].ColumnName;
                        //            int pos = cols.IndexOf(cn);
                        //            if (pos > -1)
                        //                sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                        //        }
                        //        if (isPreview)
                        //        {
                        //            sb.Append("<td><input type='button' value='Preview' onclick=\"openDocPreview(").Append(dt.Rows[i]["ID"]).Append(")'></td>");
                        //        }
                        //        if (isDownload)
                        //        {
                        //            sb.Append("<td><input type='button' value='Download' onclick=\"openDocDownload(").Append(dt.Rows[i]["ID"]).Append(")'></td>");
                        //        }
                        //        if (isDelete)
                        //        {
                        //            sb.Append("<td><input type='button' value='Remove' onclick=\"openDocRemove(").Append(dt.Rows[i]["ID"]).Append(")'></td>");
                        //        }
                        //        sb.Append("</tr>");
                        //    }

                        //    sb.Append("</tbody></table>");
                        //}


                    }
                    sb.Append("</div>");
                    sb.Append("</div>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_QUERY_FILTER)
                {
                    string MethodName = string.Empty;
                    if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_QUERY_FILTER)
                        MethodName = "query";
                    else
                        MethodName = "report";

                    string uniqueId = string.Empty;
                    if (frm != null && !string.IsNullOrEmpty(frm["TRAINING_CALENDER_ID"]) && s.Screen_Id == 24)
                    {
                        uniqueId = frm["TRAINING_CALENDER_ID"];
                    }
                    if (frm != null && !string.IsNullOrEmpty(frm["UNIQUE_REG_ID"]) && s.Screen_Id == 156)
                    {
                        uniqueId = frm["UNIQUE_REG_ID"];
                    }
                    else if (frm != null && !string.IsNullOrEmpty(frm["ui"]) && frm["s"] != "edit")
                    {
                        uniqueId = frm["ui"];
                        if (uniqueId.Contains(","))
                            uniqueId = uniqueId.Split(',')[0];
                        //sb.Append("<input type='hidden' id='uniqueId' value='").Append(uniqueId).Append("'>");
                    }
                    else if (frm != null && !string.IsNullOrEmpty(frm["uniqueId"]))
                        uniqueId = frm["uniqueId"];

                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ID", s.Id);
                    if (!string.IsNullOrEmpty(uniqueId))
                        conditions.Add("UID", uniqueId);

                    JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    DataTable dt = new DataTable();
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == "query" || property.Name == "report")
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                        }
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        int unqueID = 0;
                        if (s.Screen_Id == 156)
                        {
                            unqueID = Convert.ToInt32(dt.Rows[0]["ID"]);
                        }
                        List<string> cols = new List<string>();
                        if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                        List<string> colvals = new List<string>();
                        if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();
                        bool isTableStart = false;
                        bool isDropDown = cols.IndexOf("DROPDOWN") > -1;
                        bool isColExists = false;
                        bool isIdExists = false;

                        string Checkbox = string.Empty;
                        bool isCHECKBOX = cols.IndexOf("CHECKBOX") > -1;
                        if (isCHECKBOX)
                            Checkbox = "<th class='searchResultsHeading'>Select</th>";

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
                                    sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(s.compTextTx).Append("</span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");
                                    if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                                    if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                                    sb.Append("><thead>");
                                    sb.Append("<tr>");
                                }
                                cn = colvals[pos];
                                if (!string.IsNullOrEmpty(Checkbox))
                                {
                                    sb.Append(Checkbox);
                                    Checkbox = string.Empty;
                                }
                                if (!string.IsNullOrEmpty(Radio))
                                {
                                    sb.Append(Radio);
                                    Radio = string.Empty;
                                }
                                sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                                if (!isColExists) isColExists = true;
                            }
                        }
                        bool isEdit = cols.IndexOf("EDIT") > -1;
                        bool isPreview = cols.IndexOf("VIEW") > -1;
                        bool isEditSpl = cols.IndexOf("EDIT_SPL") > -1;
                        bool isRemove = cols.IndexOf("REMOVE") > -1;
                        bool isDropDownSpl = cols.IndexOf("DROPDOWN_SPL") > -1;
                        bool isDropDownSplMul = cols.IndexOf("DROPDOWN_SPL_MUL") > -1;
                        bool isPreviewSpl = cols.IndexOf("PREVIEW_SPL") > -1;
                        bool isDownload = cols.IndexOf("DOWNLOAD") > -1;
                        bool isApprovalLetter = cols.IndexOf("APPROVAL") > -1;
                        if (isTableStart)
                        {
                            if (isEdit) sb.Append("<th class='searchResultsHeading'>Action</th>");
                            if (isPreview) sb.Append("<th></th>");
                            if (isEditSpl == true || isDropDown == true) sb.Append("<th class='searchResultsHeading'>Action</th>");
                            if (isRemove) sb.Append("<th class='searchResultsHeading'>Delete</th>");
                            if (isDropDownSpl) sb.Append("<th class='searchResultsHeading'>Status</th>");
                            if (isPreviewSpl) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                            if (isDownload) sb.Append("<th class='searchResultsHeading'>Preview</th>");
                            if (isDropDownSplMul) sb.Append("<th class='searchResultsHeading'>Status</th>");
                            if (isApprovalLetter) sb.Append("<th class='searchResultsHeading'>Download</th>");
                            sb.Append("</thead></tr><tbody>");

                            for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                            {
                                sb.Append("<tr>");
                                if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name='chkList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                if (isRADIO) sb.Append("<td><input type=\"radio\" name='rdoList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");

                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    string cn = dt.Columns[j].ColumnName;
                                    int pos = cols.IndexOf(cn);
                                    if (pos > -1)
                                        sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                                }
                                if (isEdit)
                                {
                                    sb.Append("<td><a href='#'");
                                    if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(screen.Edit_Scr_Id).Append(")' ");
                                    string editcaption = colvals[cols.IndexOf("EDIT")];
                                    if (string.IsNullOrEmpty(editcaption))
                                    {
                                        editcaption = "Edit";
                                    }
                                    sb.Append(">" + editcaption + "</a></td>");
                                }
                                if (isPreview)
                                {
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                                    var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                                    Id = value + "_" + Id;
                                    if (isIdExists) sb.Append("<td><a href='../Home/DownloadFile/" + Id + "'");
                                    //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                    sb.Append(">Click to View</a></td>");
                                }
                                if (isEditSpl)
                                {
                                    string scrId = s.Screen_Id.ToString();

                                    //ANONYMOUS PCS / Company Pending or in progress application Application
                                    if (HttpContext.Current.Session["LOGIN_ID"].ToString() == "pcscomp")
                                    {
                                        if (dt.Columns.Contains("STATUS_TX") && (dt.Rows[i]["STATUS_TX"].ToString() == "in-progress" || dt.Rows[i]["STATUS_TX"].ToString() == "pending") && dt.Columns.Contains("REG_COM_TYPE_ID"))
                                        {
                                            scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1") ? "107" : "106";
                                        }
                                        sb.Append("<td><a");
                                        if (isIdExists) sb.Append(" onclick=").Append("'screenAction(").Append(scrId).Append(",").Append(dt.Rows[i]["ID"].ToString()).Append(")'");
                                        string editcaptionS = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                                        if (string.IsNullOrEmpty(editcaptionS))
                                        {
                                            editcaptionS = "Edit";
                                        }
                                        sb.Append(">" + editcaptionS + "</a></td>");
                                    }
                                    //NON-ANONYMOUS PCS / Company Pending or in progress application Application
                                    if (HttpContext.Current.Session["LOGIN_ID"].ToString() != "pcscomp")
                                    {
                                        if (dt.Columns.Contains("STATUS_TX") && (dt.Rows[i]["STATUS_TX"].ToString() == "in-progress" || dt.Rows[i]["STATUS_TX"].ToString() == "pending" || dt.Rows[i]["STATUS_TX"].ToString() == "call for") && dt.Columns.Contains("REG_COM_TYPE_ID"))
                                        {
                                            scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1") ? "107" : "106";
                                        }
                                        sb.Append("<td><a");
                                        long unique_reg_id = Int64.Parse(dt.Rows[i]["UNIQUE_REG_ID"].ToString());
                                        if (isIdExists) sb.Append(" onclick=").Append("'loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(scrId).Append(",").Append(unique_reg_id).Append(")'");
                                        string editcaptionS = colvals.Contains("EDIT") ? colvals[cols.IndexOf("EDIT")] : null;
                                        if (string.IsNullOrEmpty(editcaptionS))
                                        {
                                            editcaptionS = "Edit";
                                        }
                                        sb.Append(">" + editcaptionS + "</a></td>");
                                    }

                                }

                                if (isApprovalLetter)
                                {
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                                    //var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                                    //Id = Id;
                                    if (isIdExists) sb.Append("<td><a href='../PCS/DownloadApprovalLetterPCS/" + Id + "'");
                                    //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                    sb.Append("class='button'>Preview</a></td>");
                                }
                                if (isDownload)
                                {
                                    string Id = (dt.Columns.Contains("DOC_ID")) ? dt.Rows[i]["DOC_ID"].ToString() : dt.Rows[i]["ID"].ToString();
                                    if (isIdExists) sb.Append("<td><a href='../Home/DownloadFileByID/" + Id + "'");
                                    sb.Append("class='button'>Preview</a></td>");
                                }

                                if (isPreviewSpl)
                                {
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                                    //var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                                    //Id = value + "_" + Id;
                                    if (isIdExists) sb.Append("<td><a ");
                                    if (isIdExists) sb.Append(" data-value='").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append("151").Append("' ");
                                    sb.Append("class='button' name='poprpt'>Preview</a></td>");
                                }
                                if (isRemove)
                                {
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    if (isIdExists) sb.Append("<td> <button type ='submit' class='btn btn-danger btn-xs' name='deleteDoc' value='" + Id + "'");
                                    //< button type = "submit"  class="btn btn-primary SubmitBtn"> Submit</button>
                                    sb.Append(">Remove</button></td>");
                                }
                                if (isDropDown)
                                {
                                    if (s.Screen_Id == 53)
                                    {
                                        string Id = dt.Rows[i]["TRAINING_CALENDER_ID"].ToString() + "_" + dt.Rows[i]["ID"].ToString();
                                        sb.Append("<td><div class=\"dropdown\">"
                                            + "<button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\">View Feedback"
                                             + "<span class=\"caret\"></span></button>"
                                            + "<ul class=\"dropdown-menu\">"
                                                + "<li onclick=\"MoveFeedbackForm(42,'" + Id + "')\">Session Feedback</li>"
                                                + "<li onclick=\"MoveFeedbackForm(44,'" + Id + "')\">Faculty Feedback</li>"
                                                + "<li onclick=\"MoveFeedbackForm(43,'" + Id + "')\">Final Feedback</a></li>"
                                            + "</ul>"
                                            + "</div></td>");
                                    }
                                }
                                if (isDropDownSpl)
                                {
                                    sb.Append("<td>");
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    sb.Append("<select class='form-control panelInputs' name='").Append("DOCUMENT_STATUS").Append("'");
                                    sb.Append(">");
                                    sb.Append("<option value=''>").Append("-- Select --</option>");
                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Approve")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value=" + Id + "_Approve>").Append("Approve</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value=" + Id + "_Approve>").Append("Approve</option>");
                                    }

                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Reject")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value=" + Id + "_Reject>").Append("Reject</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value=" + Id + "_Reject>").Append("Reject</option>");
                                    }

                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Call")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value=" + Id + "_Call>").Append("Call For</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value=" + Id + "_Call>").Append("Call For</option>");
                                    }
                                    sb.Append(" </select>");
                                    sb.Append("</td>");
                                }
                                if (isDropDownSplMul)
                                {
                                    sb.Append("<td>");
                                    string Id = dt.Rows[i]["ID"].ToString();
                                    sb.Append("<select class='form-control panelInputs' name='").Append("DOCUMENT_STATUS").Append("'");
                                    sb.Append(">");
                                    sb.Append("<option value=''>").Append("-- Select --</option>");
                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Approve")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Approve'>").Append("Approve</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Approve'>").Append("Approve</option>");
                                    }

                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Reject")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Reject'>").Append("Reject</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Reject'>").Append("Reject</option>");
                                    }

                                    if (dt.Columns.Contains("STATUS_TX") && dt.Rows[i]["STATUS_TX"].ToString() != string.Empty && dt.Rows[i]["STATUS_TX"].ToString() == "Call")
                                    {
                                        sb.Append("<option selected=" + "selected" + " value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Call'>").Append("Call For</option>");
                                    }
                                    else
                                    {
                                        sb.Append("<option value='" + Id + "_" + s.compTextTx.Trim(' ') + "_" + unqueID + "_Call'>").Append("Call For</option>");
                                    }
                                    sb.Append(" </select>");
                                    sb.Append("</td>");
                                }

                                sb.Append("</tr>");
                            }

                            sb.Append("</tbody></table></div></div></div></div>");
                        }
                    }
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.ADD_NEW_ROW)
                {
                    sb.Append("<div class='col-md-12 FacltyNamTxt' style='clear: both; width: 100 %;'>")
                      .Append("<div style='overflow - x:auto;'><table class='table table-bordered tableSearchResults' name='")
                      .Append(s.compNameTx)
                       .Append("'")
                      .Append(" id=").Append("'").Append(s.compNameTx.ToString().Trim())
                      .Append("'")
                      .Append("><thead><tr>");
                    string[] strCompVal = s.compValueTx.Split(',');
                    string[] strColTxtBxName = s.compTextTx.Split(',');
                    for (int i = 0; i < strCompVal.Count(); i++)
                    {
                        sb.Append("<th class='searchResultsHeading'>").Append(strCompVal[i]).Append("</th>");
                    }
                    // sb.Append("</tr></thead><tbody ><tr>");
                    sb.Append("</tr></thead>");
                    sb.Append("<tbody><tr>");
                    for (int i = 0; i < strCompVal.Count(); i++)
                    {
                        sb.Append("<td><input class='form-control textCenter' type='text' placeholder=' ' value=''");
                        sb.Append(" name=").Append("'").Append(strColTxtBxName[i].ToString().Trim()).Append("'");
                        sb.Append(" id=").Append("'").Append(strColTxtBxName[i].ToString().Trim()).Append("'");
                        sb.Append("></td>");
                    }
                    sb.Append("</tr></tbody></table></div>");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_TEXTBOXES)
                {
                    sb.Append("<div class='form-inline'><div style = 'overflow-x:auto;'>")
                    .Append("<table class='table table-bordered tableSearchResults'>")
                    .Append("<thead><tr class=''>");
                    List<string> cols = new List<string>();
                    if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                    List<string> colvals = new List<string>();
                    if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();
                    bool isTableStart = true;
                    bool isColExists = false;
                    bool isIdExists = false;
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("ID", s.Id);
                    JObject jdata = DBTable.GetData("query", cond, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    DataTable dt = new DataTable();
                    if (jdata != null && jdata.HasValues)
                    {
                        foreach (JProperty property in jdata.Properties())
                        {
                            if (property.Name == "query")
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string cn = dt.Columns[i].ColumnName;
                                if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                                int pos = cols.IndexOf(cn);
                                if (pos > -1)
                                {
                                    cn = colvals[pos];
                                    sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                                    if (!isColExists) isColExists = true;
                                }
                            }
                            if (s.Screen_Id == 190 || s.Screen_Id == 200)
                            {
                                sb.Append("<th class='searchResultsHeading'>")
                                   .Append(DateTime.Now.AddYears(-2).Year).Append("-")
                                   .Append(DateTime.Now.AddYears(-1).Year).Append("</th>")
                                   .Append("<th class='searchResultsHeading'>")
                                   .Append(DateTime.Now.AddYears(-1).Year).Append("-")
                                   .Append(DateTime.Now.Year).Append("</th>");
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
                                        int pos = cols.IndexOf(cn);
                                        if (pos > -1)
                                            sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                                    }
                                    sb.Append("<td><input name = 'FY1_").Append(dt.Rows[i]["NAME_TX"]).Append("' placeholder = '' class='form-control text-center' type='text' id='FY1_")
                                        .Append(dt.Rows[i]["NAME_TX"])
                                        .Append("' value='@FY1_").Append(dt.Rows[i]["NAME_TX"]).Append("' style ='width:100%'></td>")
                                        .Append("<td><input name ='FY2_").Append(dt.Rows[i]["NAME_TX"]).Append("' placeholder='' class='form-control text-center' type='text' id='FY2_")
                                        .Append(dt.Rows[i]["NAME_TX"])
                                        .Append("' value='@FY2_").Append(dt.Rows[i]["NAME_TX"]).Append("' style='width:100%'></td>")
                                        .Append("</tr>");
                                }
                                sb.Append("</tbody></table>");
                                if (s.Screen_Id == 190)
                                {
                                    sb.Append("<input type='hidden' name='FY1_TX' value='")
                                      .Append(DateTime.Now.AddYears(-2).Year).Append("-")
                                      .Append(DateTime.Now.AddYears(-1).Year).Append("'</input>")
                                      .Append("<input type='hidden' name='FY2_TX' value='")
                                      .Append(DateTime.Now.AddYears(-1).Year).Append("-")
                                      .Append(DateTime.Now.Year).Append("'</input>");
                                }
                                sb.Append("</div></div>");
                            }
                        }
                    }
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.QUES_ANS_CHK_BOX)
                {
                    sb.Append("<label class='col-xs-12'>")
                      .Append("<h5 class='text-primary'><strong>")
                      .Append(s.compTextTx).Append("</h5></strong></label><div class='pt-10'></div>");                    
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("ID", s.Id);
                    JObject jdata = DBTable.GetData("query", cond, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    DataTable dt = new DataTable();
                    if (jdata != null && jdata.HasValues)
                    {
                        foreach (JProperty property in jdata.Properties())
                        {
                            if (property.Name == "query")
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                sb.Append("<div class='form-inline'><label class='col-md-12 control-label AppliedTrainingnputsLabel FacltyNamTxt txtLabel boldText'>")
                                   .Append(i + 1).Append(".")
                                   .Append(dt.Rows[i]["QUESTION_NAME_TX"]).Append("?</label>")
                                   .Append("<label class='col-md-12 control-label AppliedTrainingnputsLabel FacltyNamTxt txtLabel boldText'>Ans: ")
                                   .Append("<input type='radio' value='1' class='radioText' id='").Append(dt.Rows[i]["NAME_TX"]).Append("_Y'  name='").Append(dt.Rows[i]["NAME_TX"]).Append("'>Yes")
                                   .Append("<input type='radio' value='0' class='radioText' id='").Append(dt.Rows[i]["NAME_TX"]).Append("_N' name='").Append(dt.Rows[i]["NAME_TX"]).Append("'>No</label></div>");
                            }
                        }
                    }
                    sb.Append("");
                }
                else if (s.Comp_Type_Nm == (Int32)HTMLTag.TABLE_WITH_CHKBOXES)
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ID", s.Id);
                    JObject jdata = DBTable.GetData("query", conditions, "SCREEN_COMP_T", 0, 100, getSchemaName(s));
                    DataTable dt = new DataTable();
                    if (jdata != null && jdata.HasValues)
                    {
                        foreach (JProperty property in jdata.Properties())
                        {
                            if (property.Name == "query")
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            sb.Append("<div class='col-md-10' style='clear:both; width: 100 %;'><div class='form-inline'><div style = 'overflow-x:auto;'>")
                                .Append("<table class='table table-bordered tableSearchResults'><tbody>");
                            foreach (DataRow dr in dt.Rows)
                            {
                                sb.Append("<tr>")
                                  .Append("<td width = '35%'>").Append(dr["PRACTICE_NAME_TX"]).Append("</td>");
                                if (Convert.ToString(dr["NAME_TX"]).Contains("_YN"))
                                {
                                    sb.Append("<td width = '35%'>")
                                      .Append("<label><input type='radio' id='").Append(dr["NAME_TX"]).Append("_Y' name='").Append(dr["NAME_TX"]).Append("' class='radioText' value='1'>Yes</label>")
                                      .Append("<label><input type='radio' id='").Append(dr["NAME_TX"]).Append("_N' name='").Append(dr["NAME_TX"]).Append("' class='radioText' value='0'>No</label>")
                                      .Append("</td></tr>");
                                }
                                else if (Convert.ToString(dr["NAME_TX"]).Contains("_TXT"))
                                {
                                    sb.Append("<td width = '35%'>")
                                      .Append("<input name = '").Append(dr["NAME_TX"]).Append("' placeholder = '' class='form-control' type='text' id='").Append(dr["NAME_TX"]).Append("' value='@").Append(dr["NAME_TX"]).Append("'></td></tr>");
                                }
                            }
                            sb.Append("</tbody></table></div></div></div>");
                        }
                    }
                }

                if (s.ScreenCompList != null && s.Comp_Type_Nm != (Int32)HTMLTag.SEARCH) renderScreenComponent(screen, s.ScreenCompList, sb, frm);
            }
        }

        public static string ConvertToWords(int num)
        {
            StringBuilder strNumInWords = new StringBuilder();
            strNumInWords.Append(numToWords((int)(num / 10000000), "crore "));
            strNumInWords.Append(numToWords((int)((num / 100000) % 100), "lakh "));
            strNumInWords.Append(numToWords((int)((num / 1000) % 100), "thousand "));
            strNumInWords.Append(numToWords((int)((num / 100) % 10), "hundred "));
            if (num > 100 && num % 100 > 0)
            {
                strNumInWords.Append("and ");
            }
            strNumInWords.Append(numToWords((int)(num % 100), ""));

            return strNumInWords.ToString();
        }

        private static string numToWords(int num, string strType)
        {
            string[] one = { "", "One ", "Two ", "Three ", "Four ",
                            "Five ", "Six ", "Seven ", "Eight ",
                            "Nine ", "Ten ", "Eleven ", "Twelve ",
                            "Thirteen ", "Fourteen ", "Fifteen ",
                            "Sixteen ", "Seventeen ", "Eighteen ",
                            "Nineteen " };
            string[] ten = { "", "", "Twenty ", "Thirty ", "Forty ",
                            "Fifty ", "Fixty ", "Seventy ", "Eighty ",
                            "Ninety " };
            StringBuilder str = new StringBuilder();


            if (num > 19) str.Append(ten[num / 10] + one[num % 10]);
            else str.Append(one[num]);

            if (num != 0) str.Append(strType);
            return str.ToString();
        }

        public enum HTMLTag
        {
            HIDDEN = 1,
            TEXT = 2,
            PASSWORD = 3,
            RADIO_GROUP = 4,
            RADIO_BUTTON = 5,
            CHECKBOX = 6,
            TEXTAREA = 7,
            DROPDOWN_WITH_QUERY = 8,
            SUBMIT = 9,
            BUTTON = 10,
            LIST = 11,
            IMAGE = 12,
            DIV = 13,
            SPAN = 14,
            ANCHOR = 15,
            FORM = 16,
            TABLE_WITH_QUERY = 17,
            TABLE = 18,
            Tr = 19,
            Td = 20,
            Th = 21,
            REPORT = 22,
            CUSTOM = 23,
            SEARCH = 24,
            DATE = 25,
            DATETIME = 26,
            LOAD = 27,
            TABLE_WITH_QUERY_FILTER = 28,
            UPLOAD_DOCUMENTS = 29,
            ADD_MORE = 30,
            TABLE_WITH_QUERY_AND_ID = 31,
            ADD_NEW_ROW = 32,
            TABLE_WITH_TEXTBOXES = 33,
            QUES_ANS_CHK_BOX = 34,
            TABLE_WITH_CHKBOXES = 35
        }

        public static byte[] GetBytes(Stream fs)
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] bytes = null;// br.ReadBytes((Int32)fs.Length);
                using (var streamReader = new MemoryStream())
                {
                    fs.CopyTo(streamReader);
                    bytes = streamReader.ToArray();
                    // bytes = Compress(bytes);
                }
                return bytes;
            }
        }

        public static ActionClass beforeLoad(int WEB_APP_ID, FormCollection frm)
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
            try
            {
                if (screenType != null)
                {
                    Screen_T screen = screenObject(WEB_APP_ID, frm);
                    if (screenType.Equals("edit") && uniqueId != null)
                    {
                        if (screen != null && userid != null && !userid.Trim().Equals("") && uniqueId != null && !uniqueId.Trim().Equals("")
                            && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            string applicationSchema = getApplicationScheme(screen);
                            Dictionary<string, object> conditions = new Dictionary<string, object>();
                            conditions.Add("ACTIVE_YN", 1);
                            conditions.Add("ID", uniqueId);
                            JObject jdata = DBTable.GetData("fetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
                            actionClass.jObject = jdata;
                        }
                    }
                    else if ((screenType.Equals("newwithdata") || screenType.Equals("newwithpayment")) && uniqueId != null)
                    {
                        string applicationSchema = getApplicationScheme(screen);
                        JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, applicationSchema);
                        actionClass.columnMetadata = jdata;
                    }
                    else
                    {
                        string applicationSchema = getApplicationScheme(screen);
                        JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, applicationSchema);
                        actionClass.columnMetadata = jdata;
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

        public static ActionClass afterSubmit(int WEB_APP_ID, FormCollection frm)
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
                    //else if (screenType.Equals("update"))
                    //{
                    //    Screen_T screen = screenObject(frm);
                    //    if (screen != null && userid != null && !userid.Trim().Equals("") && menuid != null && !menuid.Trim().Equals("") && uniqueId != null && !uniqueId.Trim().Equals(""))
                    //    {
                    //        string applicationSchema = getApplicationScheme(screen);

                    //    }
                    //}
                    else if (screenType.Equals("insert") || screenType.Equals("update"))
                    {
                        Screen_T screen = screenObject(WEB_APP_ID, frm);
                        if (screen != null && userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            string applicationSchema = getApplicationScheme(screen);
                            Dictionary<string, object> conditions = new Dictionary<string, object>();
                            JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, applicationSchema);
                            StringBuilder sb = new StringBuilder();

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
                                        lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                                        actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                                        actionClass.columnMetadata = jdata;

                                        string ref_string_id = "0";
                                        if (actionClass.DecryptData != null)
                                            ref_string_id = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)(JObject.Parse(actionClass.DecryptData).First.First.First.First)).Value);
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
                                            else if (TblName == Convert.ToString(TableName.CSR_AWARDS_SUPPORTING_DOCS_T))
                                                FolderName = Convert.ToString(FilePath.CSRAwardsSupportingDocs.GetEnumDisplayName());
                                            else if (TblName == Convert.ToString(TableName.CGA_AWARDS_SUPPORTING_DOCS_T))
                                                FolderName = Convert.ToString(FilePath.CGAAwardsSupportingDocs.GetEnumDisplayName());

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


        public static ActionClass searchLoad(int WEB_APP_ID, FormCollection frm, string scrtype, string scmptid, string screenId = "")
        {
            ActionClass actionClass = new ActionClass();
            string Message = string.Empty;
            string userid = frm["u"];
            string menuid = frm["m"];
            screenId = frm["si"];
            List<string> manSearchFilter = new List<string>();
            string screenType = frm["s"] == null ? scrtype : frm["s"] == "new" ? scrtype : frm["s"];
            if (!string.IsNullOrEmpty(scrtype) && (scrtype == "newwithdata" || scrtype == "newwithpayment" || scrtype == "reports"))
            {
                screenType = scrtype;
            }
            string uniqueId = frm["ui"] == null ? "" : frm["ui"];
            string scrrmpid = frm["scmpid"] == null ? scmptid : frm["scmpid"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            try
            {
                if (screenType != null)
                {
                    if (screenType.Equals("search") || screenType.Equals("reports") || screenType.Equals("newwithdata") || screenType.Equals("newwithpayment") || screenType.Equals("newwithstudent") || screenType.Equals("searchwithstudent") || screenType.Equals("searchwithupdate"))
                    {
                        int st = 0;
                        int ed = 0;
                        if (frm["st"] != null) st = Convert.ToInt32(frm["st"]);
                        if (frm["ed"] != null) ed = Convert.ToInt32(frm["ed"]);
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        foreach (var key in frm.AllKeys)
                        {
                            if (key.StartsWith("SCRH_") || key.StartsWith("COND_") || key.StartsWith("REPLACE_"))
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(frm[key])) && Convert.ToString(frm[key]) != "0")
                                {
                                    conditions.Add(key, Convert.ToString(frm[key]));
                                }
                            }
                            else if (key.StartsWith("MANSCR_"))
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(frm[key])))
                                {
                                    conditions.Add(key, Convert.ToString(frm[key]));
                                    manSearchFilter.Add(key);
                                }
                            }
                        }
                        conditions.Add("SCMPID", scrrmpid);

                        /*foreach (var key in frm.AllKeys)
                        {
                            conditions.Add(key, frm[key]);
                        }*/
                        Screen_T screen = screenObject(WEB_APP_ID, frm);
                        Screen_Comp_T s = getScreenComponent(screen.ScreenComponents, Convert.ToInt32(scrrmpid));
                        if (s != null && s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
                        {
                            string[] cons = s.dynWhere.Split(',');
                            foreach (string con in cons)
                            {
                                string[] convals = con.Split('=');
                                if (!conditions.ContainsKey(convals[0].Trim()))
                                {
                                    if (convals[1].Trim().StartsWith("@") && Convert.ToString(convals[1].Trim().Substring(1)) != "0")
                                    {
                                        if (frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                                        {
                                            conditions.Add("SCRH_" + convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                                        }
                                        else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                        {
                                            string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                            bool proceed = false;
                                            try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                            if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                                        }
                                    }
                                    else conditions.Add("SCRH_" + convals[0].Trim(), convals[1].Trim());
                                }
                            }
                        }

                        if (screen != null && userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            string applicationSchema = getApplicationScheme(screen);
                            bool pgs = frm["pgs"] != null && !frm["pgs"].Trim().Equals("") && frm["pgs"] == "1";
                            int pgn = frm["pgn"] != null && !frm["pgn"].Trim().Equals("") ? Convert.ToInt32(frm["pgn"]) : 0;
                            int pgr = frm["pgr"] != null && !frm["pgr"].Trim().Equals("") ? Convert.ToInt32(frm["pgr"]) : 0;
                            if (screenType.Equals("reports"))
                            {
                                actionClass = DBTable.GetSearchData(conditions, null, screen.Table_Name_Tx, pgs, pgn, pgr, st, ed, applicationSchema, "reports");
                            }
                            else
                            {
                                actionClass = DBTable.GetSearchData(conditions, null, screen.Table_Name_Tx, pgs, pgn, pgr, st, ed, applicationSchema);
                            }
                            actionClass.manSearchFilter = manSearchFilter;
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
        private static Screen_Comp_T getScreenComponent(List<Screen_Comp_T> scr, int id)
        {
            Screen_Comp_T s = null;
            for (int i = 0; s == null && scr != null && i < scr.Count; i++)
            {
                if (scr[i].Id == id) s = scr[i];
                else
                {
                    s = getScreenComponent(scr[i].ScreenCompList, id);
                }
            }
            return s;
        }
        public static string getApplicationScheme(Screen_T s)
        {
            //long appid = DBTable.APP_MODULE_T.AsEnumerable().Where(x => x.Field<long>("ID") == screen.App_Module_Id).Select(x => x.Field<long>("APP_ID")).FirstOrDefault();
            //string applicationSchema = DBTable.APPLICATION_T.AsEnumerable().Where(x => x.Field<long>("ID") == appid).Select(x => x.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
            //if (applicationSchema == null) applicationSchema = "";
            //return applicationSchema;
            //Screen_T s = screen;
            if (s.schemaNameTx != null && !s.schemaNameTx.Trim().Equals("")) return s.schemaNameTx;
            else if (s.ModuleSchemaNameTx != null && !s.ModuleSchemaNameTx.Trim().Equals("")) return s.ModuleSchemaNameTx;
            else if (s.ApplicationSchemaNameTx != null && !s.ApplicationSchemaNameTx.Trim().Equals("")) return s.ApplicationSchemaNameTx;
            else return s.WebAppSchemaNameTx;

        }
        public static Screen_T screenObject(int WEB_APP_ID, FormCollection frm)
        {
            string userid = frm["u"];
            string menuid = frm["m"];
            string screenId = frm["si"];
            if (screenId == null || screenId.Trim().Equals("")) screenId = null;
            if (screenId != null)
            {
                List<object> menu = (List<object>)HttpContext.Current.Session["USER_MENU"];
                DataRow row = null;
                if (!string.IsNullOrEmpty(menuid))
                {
                    row = UtilService.fetchMenuItem(WEB_APP_ID, Convert.ToInt32(menuid));
                }
                else if (!string.IsNullOrEmpty(screenId))
                {
                    row = UtilService.fetchMenuItemByScreenId(WEB_APP_ID, Convert.ToInt32(screenId));
                }
                Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                return screen;
            }
            else
            if (menuid != null && !menuid.Trim().Equals(""))
            {
                List<object> menu = (List<object>)HttpContext.Current.Session["USER_MENU"];
                DataRow row = UtilService.fetchMenuItem(WEB_APP_ID, Convert.ToInt32(menuid));
                if (row != null)
                {
                    Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                    menu = null;
                    return screen;
                }
                menu = null;
            }
            return null;
        }

        public static object DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(list);
        }

        /**
         * 
         */
        public static Screen_T homeAction(int WEB_APP_ID, FormCollection frm, string userid, string menuid, string screenId, string ScreenType, ActionClass act, List<object> menu)
        {
            if (ScreenType == "newwithdata" || ScreenType == "newwithpayment" || ScreenType == "newwithstudent" || ScreenType == "search" || ScreenType == "reports" || ScreenType == "searchwithstudent" || ScreenType == "searchwithupdate")
            {
                HttpContext.Current.Session.Remove("UPLOAD_DOCUMENTS");
                HttpContext.Current.Session.Remove("LOAD_UPLOAD_DOCUMENTS");
            }
            //if (menuid != null && menuid == "29")
            //{
            //    frm.Add("ui", "3");
            //}
            if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
            {
                DataRow row = null;
                if (menuid != null && !menuid.Trim().Equals("")) row = UtilService.fetchMenuItem(WEB_APP_ID, Convert.ToInt32(menuid));
                if (screenId == null || screenId.Trim().Equals(""))
                {
                    screenId = null;
                }
                else
                {
                    if (!isResponsibilityExists(Convert.ToInt32(screenId)))
                    {
                        screenId = null;
                        //if (ScreenType == "search") ScreenType = "new";
                    }
                }
                if (row != null || screenId != null)
                {
                    Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                    if (screen.Action_Tx == "report" || screen.Action_Tx == "search" || screen.Action_Tx == "reports" || screen.Action_Tx == "newwithdata" || screen.Action_Tx == "newwithpayment" || screen.Action_Tx == "searchwithstudent" || screen.Action_Tx == "searchwithupdate" || screen.Action_Tx == "newwithstudent")
                    {
                        if ((ScreenType == "insert" || ScreenType == "update") && (screen.Action_Tx == "newwithdata" || screen.Action_Tx == "newwithstudent" || screen.Action_Tx == "searchwithupdate")) { }
                        else
                        {
                            ScreenType = screen.Action_Tx;
                        }
                    }
                    if (screen != null)
                    {
                        if (ScreenType == "insert" || ScreenType == "update")
                        {
                            StringBuilder manSb = new StringBuilder();
                            if (screen.Mandatory_Fields_Tx != null && !screen.Mandatory_Fields_Tx.Trim().Equals(""))
                            {
                                string[] mns = screen.Mandatory_Fields_Tx.Split(',');
                                string[] mnsLables = null;
                                if (screen.Mandatory_Field_Labels_Tx != null && !screen.Mandatory_Field_Labels_Tx.Trim().Equals("")) mnsLables = screen.Mandatory_Field_Labels_Tx.Split(',');
                                for (int i = 0; mns != null && i < mns.Length; i++)
                                {
                                    if (!frm.AllKeys.Contains(mns[i]) || frm[mns[i]] == null || Convert.ToString(frm[mns[i]]).Trim().Equals(""))
                                    {
                                        manSb.Append(",");
                                        if (mnsLables != null && mnsLables.Length > i) manSb.Append(mnsLables[i]);
                                        else manSb.Append(mns[i]);
                                    }
                                }
                            }
                            if (manSb.Length > 0)
                            {
                                //act = new ActionClass();
                                act.StatCode = "-200";
                                act.StatMessage = "Mandatory Fields (" + manSb.ToString().Substring(1) + ") data missing!";

                            }
                            else
                            {

                                if (ScreenType.Equals("insert") || ScreenType == "update") act = CheckUniqueness(screen, frm, ScreenType);
                            }
                            if (!(act.StatCode != null && !act.StatCode.Equals("0")))
                            {
                                if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                                {
                                    act = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "after" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                                }
                                else act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);


                                /*if (ScreenType == "update" && string.IsNullOrEmpty(Convert.ToString(frm["nextscreen"]))) // screen.Screen_Next_Id==0 && 
                                {                                   
                                    ScreenType = "edit";
                                    frm["s"] = ScreenType;
                                    frm["ui"] = frm["ID"];
                                    return homeAction(WEB_APP_ID, frm, userid, menuid, screenId, ScreenType, act, menu);
                                
                                }else    if (screen.Screen_Next_Id > 0)
                                {
                                    ScreenType = Convert.ToString((DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == screen.Screen_Next_Id).FirstOrDefault())["ACTION_TX"]);
                                    if (ScreenType == "edit") { frm["ui"] = frm["ID"]; }
                                    frm["s"] = ScreenType;
                                    return homeAction(WEB_APP_ID, frm, userid, menuid, Convert.ToInt32(screen.Screen_Next_Id), ScreenType, act, menu);
                                }*/
                                screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                                if (act.columnMetadata == null)
                                {
                                    string applicationSchema = getApplicationScheme(screen);
                                    JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, applicationSchema);
                                    act.columnMetadata = jdata;
                                }
                                if (act.StatCode == "0" && act.StatMessage.ToLower() == "success")
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(frm["nextscreen"])))
                                    {

                                        screenId = Convert.ToString(frm["nextscreen"]);
                                        string scrtype = screen.Action_Tx;
                                        if (!string.IsNullOrEmpty(screenId))
                                        {
                                            scrtype = Convert.ToString((DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == Convert.ToInt32(screenId)).FirstOrDefault())["ACTION_TX"]);
                                        }
                                        string uid = string.Empty;
                                        if (((Newtonsoft.Json.Linq.JProperty)(JObject.Parse(act.DecryptData).First.First.First.First)).Name == "ID")
                                        {
                                            uid = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)(JObject.Parse(act.DecryptData).First.First.First.First)).Value);
                                            if (frm["ui"] != null)
                                            {
                                                frm["ui"] = uid;
                                            }
                                            else
                                            {
                                                frm.Add("ui", uid);
                                            }
                                            if (frm["s"] != null)
                                            {
                                                frm["s"] = scrtype;
                                            }
                                            else
                                            {
                                                frm.Add("s", scrtype);
                                            }
                                            if (frm["si"] != null)
                                            {
                                                frm["si"] = screenId;
                                            }
                                            else
                                            {
                                                frm.Add("si", screenId);
                                            }
                                            act = new ActionClass();
                                            Screen_T nextscreen = homeAction(WEB_APP_ID, frm, userid, null, screenId, scrtype, act, (List<object>)HttpContext.Current.Session["USER_MENU"]);
                                            //if (nextscreen != null && nextscreen.Screen_Content_Tx != null) nextscreen.Screen_Content_Tx = nextscreen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(HttpContext.Current.Session["REGION_ID"]));
                                            return nextscreen;
                                        }
                                    }
                                    else
                                    {
                                        string str = DBTable.SCREEN_MESSAGE_T.AsEnumerable().Where(x => x.Field<long>("STAT_CODE_NM") == screen.Success_Message_Nm && x.Field<long>("SCREEN_ID") == screen.ID).Select(x => x.Field<string>("STAT_MESSAGE_TX")).FirstOrDefault();
                                        if (str != null) act.StatMessage = str;
                                        else act.StatMessage = ScreenType.Equals("insert") ? "Record Inserted Successfully" : "Record Updated Successfully";
                                    }
                                }
                                else if (string.IsNullOrEmpty(act.StatCode))
                                {
                                    string str = DBTable.SCREEN_MESSAGE_T.AsEnumerable().Where(x => x.Field<long>("STAT_CODE_NM") == screen.Failure_Message_Nm && x.Field<long>("SCREEN_ID") == screen.ID).Select(x => x.Field<string>("STAT_MESSAGE_TX")).FirstOrDefault();
                                    if (str != null) act.StatMessage = str;
                                    else act.StatMessage = ScreenType.Equals("insert") ? "Record Inserted Successfully" : "Record Updated Successfully";
                                }
                                screen.resActionClass = act;
                                if (act.columnMetadata.HasValues)
                                {
                                    foreach (JProperty property in act.columnMetadata.Properties())
                                    {
                                        if (property.Name == "META_DATA")
                                        {
                                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + Convert.ToString(dt.Rows[i][0]) + "", "");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string applicationSchema = getApplicationScheme(screen);
                                JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, applicationSchema);
                                act.columnMetadata = jdata;
                                screen.resActionClass = act;
                                screen.Action_Tx = ScreenType;
                                if (act.columnMetadata.HasValues)
                                {
                                    foreach (JProperty property in act.columnMetadata.Properties())
                                    {
                                        if (property.Name == "META_DATA")
                                        {
                                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                string columnName = Convert.ToString(dt.Rows[i][0]) + "";
                                                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + columnName, frm[columnName] == null ? "" : Convert.ToString(frm[columnName]));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (ScreenType == null || ScreenType == "new" || ScreenType == "edit")
                        {
                            //HttpContext.Current.Session["UPLOAD_DOCUMENTS"] = null;
                            HttpContext.Current.Session.Remove("UPLOAD_DOCUMENTS");
                            HttpContext.Current.Session.Remove("LOAD_UPLOAD_DOCUMENTS");
                            if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                            {
                                screen.resActionClass = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "before" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                            }
                            else screen.resActionClass = UtilService.beforeLoad(WEB_APP_ID, frm);

                            if ((ScreenType.Equals("edit") || (screen.Action_Tx != null && screen.Action_Tx.Equals("edit"))) && screen.resActionClass != null && screen.resActionClass.jObject != null)
                            {
                                if (screen.resActionClass.jObject.HasValues)
                                {
                                    string TokenName = string.Empty;
                                    Object TokenValue = string.Empty;
                                    if (screen.resActionClass.jObject.First != null)
                                    {
                                        var o = screen.resActionClass.jObject.First.First;
                                        if (o != null && o.First != null)
                                        {
                                            foreach (JToken token in screen.resActionClass.jObject.First.First.First)
                                            {
                                                TokenName = ((Newtonsoft.Json.Linq.JProperty)token).Name;
                                                TokenValue = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)token).Value);

                                                //if (TokenName == "START_DT" || TokenName == "EDN_DT")
                                                if (TokenName.ToString().Contains("_DT") && !string.IsNullOrEmpty(Convert.ToString(TokenValue))
                                                    && TokenValue.ToString().Length > 10)
                                                {
                                                    TokenValue = Convert.ToDateTime(TokenValue).ToString("dd/MM/yyyy").Replace("-", "/");
                                                    //TokenValue = DateTime.ParseExact(TokenValue.ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                                                }
                                                if (screen.Screen_Content_Tx != null)
                                                {
                                                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                                                }
                                            }
                                        }
                                    }
                                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#SESSION_STUDENT_ID#", Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));
                                    screen.Action_Tx = "update";
                                }
                            }
                            else
                            {
                                JObject jdata = null;
                                if (screen.resActionClass != null)
                                {
                                    jdata = screen.resActionClass.columnMetadata;
                                }
                                else
                                {
                                    // TODO needs mfetch is here
                                }
                                if (jdata != null)
                                {
                                    if (jdata.HasValues)
                                    {
                                        foreach (JProperty property in jdata.Properties())
                                        {
                                            if (property.Name == "META_DATA")
                                            {
                                                DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + Convert.ToString(dt.Rows[i][0]) + "", "");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (ScreenType == "newwithdata" || ScreenType == "newwithpayment" || ScreenType == "newwithstudent")
                        {
                            HttpContext.Current.Session.Remove("UPLOAD_DOCUMENTS");
                            HttpContext.Current.Session.Remove("LOAD_UPLOAD_DOCUMENTS");
                            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.LOAD)).FirstOrDefault();
                            if (ScreenType == "newwithstudent")
                            {
                                if (s.compNameTx.Contains(','))
                                {
                                    frm.Add("SCRH_" + s.compNameTx.Split(',')[0], frm["ui"] != null ? frm["ui"] : "");
                                    frm.Add("COND_" + s.compNameTx.Split(',')[0], s.where);
                                    frm.Add("REPLACE_" + s.compNameTx.Split(',')[1], Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));
                                }
                                else
                                {
                                    frm.Add("SCRH_" + s.compNameTx, frm["ui"] != null ? frm["ui"] : "");
                                    frm.Add("COND_" + s.compNameTx, s.where);
                                }
                            }
                            else
                            {
                                frm.Add("SCRH_" + s.compNameTx, frm["ui"] != null ? frm["ui"] : "");
                                frm.Add("COND_" + s.compNameTx, s.where);
                            }
                            if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                            {
                                screen.resActionClass = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "before" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                            }
                            else screen.resActionClass = UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id), Convert.ToString(screen.ID));
                            JObject jobjects = JObject.Parse(Convert.ToString(screen.resActionClass.DecryptData));
                            if (jobjects.HasValues)
                            {
                                string TokenName = string.Empty;
                                Object TokenValue = string.Empty;
                                if (jobjects.First != null)
                                {
                                    var o = jobjects.First.First;
                                    if (o != null && o.Count() > 0)
                                    {
                                        foreach (JToken token in jobjects.First.First.First)
                                        {
                                            TokenName = ((Newtonsoft.Json.Linq.JProperty)token).Name;
                                            TokenValue = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)token).Value);

                                            if (TokenName == "START_DT" || TokenName == "EDN_DT")
                                            {
                                                TokenValue = Convert.ToDateTime(TokenValue).ToString("MM/dd/yyyy").Replace("-", "/");
                                                //TokenValue = DateTime.ParseExact(TokenValue.ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                                            }
                                            if (screen.Screen_Content_Tx != null)
                                            {
                                                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + TokenName + "", TokenValue.ToString());
                                            }
                                        }
                                    }
                                }
                                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#SESSION_STUDENT_ID#", Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));

                            }
                            if (ScreenType == "newwithdata" || ScreenType == "newwithstudent")
                            {
                                if (frm["screen_type_tx"] != null && (frm["screen_type_tx"] == "insert" || frm["screen_type_tx"] == "update"))
                                {
                                    screen.Action_Tx = frm["screen_type_tx"];
                                    frm["s"] = frm["screen_type_tx"];
                                }
                                else
                                {
                                    screen.Action_Tx = "insert";
                                    frm["s"] = "insert";
                                }
                            }
                            else
                            {
                                screen.Action_Tx = "payment";
                                frm["s"] = "payment";
                            }
                        }
                        else if (ScreenType != null && (ScreenType == "search" || ScreenType == "reports" || ScreenType == "searchwithstudent" || ScreenType == "searchwithupdate"))
                        {
                            HttpContext.Current.Session.Remove("UPLOAD_DOCUMENTS");
                            HttpContext.Current.Session.Remove("LOAD_UPLOAD_DOCUMENTS");
                            int? edit_scr_id = 0;

                            if (screen.Edit_Scr_Id != null)
                            {
                                edit_scr_id = screen.Edit_Scr_Id;
                            }
                            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.SEARCH)).FirstOrDefault();

                            if (ScreenType == "searchwithstudent")
                            {
                                if (!string.IsNullOrEmpty(s.compNameTx))
                                {
                                    if (Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]) == "")
                                    {
                                        HttpContext.Current.Session["STUDENT_ID"] = frm["stid"].ToString();
                                    }
                                    frm.Add("REPLACE_" + s.compNameTx, Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));
                                }
                            }
                            if (s.isPaginationYn)
                            {
                                frm["pgs"] = "1";
                                frm["pgn"] = "1";
                                frm["pgr"] = Convert.ToString(s.maxRows);
                            }
                            if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                            {
                                screen.resActionClass = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "search" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id), "", screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                            }
                            else screen.resActionClass = UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id));

                            JObject jobjects = JObject.Parse(Convert.ToString(screen.resActionClass?.DecryptData));
                            DataTable dt = new DataTable();
                            if (jobjects.HasValues)
                            {
                                foreach (JProperty property in jobjects.Properties())
                                {
                                    if (property.Name == "search")
                                    {
                                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                    }
                                }
                                screen.Action_Tx = "update";
                            }

                            bool isSearchCountExists = false;
                            int rowsCount = 0;
                            StringBuilder sb = new StringBuilder();
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                isSearchCountExists = dt.Columns != null && dt.Columns.IndexOf("SRCHCOUNT") != -1;
                                if (isSearchCountExists)
                                {
                                    rowsCount = Convert.ToInt32(dt.Rows[0]["SRCHCOUNT"]);
                                    dt.Rows.Remove(dt.Rows[0]);
                                    dt.Columns.Remove("SRCHCOUNT");
                                }
                            }
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                List<string> cols = new List<string>();
                                if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                                List<string> colvals = new List<string>();
                                if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();
                                bool isTableStart = false;

                                bool isColExists = false;
                                bool isIdExists = false;

                                string Checkbox = string.Empty;
                                bool isCHECKBOX = cols.IndexOf("CHECKBOX") > -1;
                                if (isCHECKBOX)
                                    Checkbox = "<th class='searchResultsHeading'>Select</th>";

                                string Radio = string.Empty;
                                bool isRADIO = cols.IndexOf("RADIO") > -1;
                                if (isRADIO)
                                    Radio = "<th class='searchResultsHeading'>Select</th>";
                                StringBuilder pagination = new StringBuilder();
                                StringBuilder paginationDown = new StringBuilder();
                                List<string> hiddens = new List<string>();
                                StringBuilder hidden = new StringBuilder();
                                if (frm["pgs"] != null && frm["pgs"].Equals("1"))
                                {
                                    foreach (var key in frm.AllKeys)
                                    {
                                        if (key.StartsWith("SCRH_") || key.StartsWith("COND_") || key.StartsWith("REPLACE_") || key.StartsWith("MANSCR_"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(frm[key])) && Convert.ToString(frm[key]) != "0")
                                            {
                                                bool isReqd = false;

                                                string kky = string.Empty;
                                                string kkky = string.Empty;
                                                if (key.StartsWith("SCRH_")) { kkky = key.Replace("SCRH_", ""); kky = "SCRHPGN_" + kkky; }
                                                else if (key.StartsWith("COND_")) { kkky = key.Replace("COND_", ""); kky = "CONDGN_" + kkky; }
                                                else if (key.StartsWith("REPLACE_")) { kkky = key.Replace("REPLACE_", ""); kky = "REPLACEPGN_" + kkky; }
                                                else if (key.StartsWith("MANSCR_")) { kkky = key.Replace("MANSCR_", ""); kky = "MANSCRPGN_" + kkky; }
                                                if (kkky != string.Empty && kky != string.Empty)
                                                {
                                                    if (key.StartsWith("COND_"))
                                                    {
                                                        if (frm.AllKeys.Contains("SCRH_" + kkky))
                                                            isReqd = true;
                                                    }
                                                    else isReqd = true;
                                                }
                                                if (isReqd)
                                                {
                                                    pagination.Append("<input type='hidden' value='").Append(Convert.ToString(frm[key])).Append("' name='").Append(kky).Append("' />");
                                                }
                                            }
                                        }
                                    }

                                    if (s != null && s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
                                    {
                                        string[] cons = s.dynWhere.Split(',');
                                        foreach (string con in cons)
                                        {
                                            string[] convals = con.Split('=');
                                            if (convals[1].Trim().StartsWith("@"))
                                            {
                                                if (frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                                                {
                                                    pagination.Append("<input type='hidden' value='").Append(frm[convals[1].Trim().Substring(1)]).Append("' name='SCRHPGN_").Append(convals[0].Trim()).Append("' />");
                                                }
                                                else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                                {
                                                    string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                                    bool proceed = false;
                                                    try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                                    if (proceed) pagination.Append("<input type='hidden' value='").Append(val).Append("' name='SCRHPGN_").Append(convals[0].Trim()).Append("' />");
                                                }
                                                else
                                                {
                                                    pagination.Append("<input type='hidden' value='").Append(convals[1].Trim()).Append("' name='SCRHPGN_").Append(convals[0].Trim()).Append("' />");
                                                }
                                            }
                                        }
                                    }

                                    int pgr = Convert.ToInt32(frm["pgr"]);
                                    bool isExcess = rowsCount % pgr > 0;
                                    pgr = (rowsCount / pgr) + (isExcess ? 1 : 0);
                                    pagination.Append("<input type='hidden' name='pgs' value='1' /> <input type='hidden' name='pgn' value='1' /> <input type='hidden' name='pgr' value='").Append(s.maxRows).Append("'/>");
                                    pagination.Append("<div class='container' ><div class='row'><div class='col-xs-12'><ul class='pagination text-center' id='ulpgid'>");
                                    pagination.Append("<li> Page: <input type='text' style='width:25px' id='paginationid' value='1'><input type='button' value='Go' style='width=20px' onclick='loadPaginationOrigPage(1)'/><label>&nbsp;&nbsp;Total Pages: ").Append(pgr).Append(", Total Records:" + Convert.ToString(rowsCount) + "</label><input type='hidden' name='totpgs' value='").Append(pgr).Append("'>");
                                    pagination.Append("<input type='hidden' name='pren' value='1' ></li><li><a id='pgprevid' href='#' onclick='loadPaginationPrevPage();return false;'>« Previous</a></li>");
                                    pagination.Append("<li id='pgli1' class='active disabled'><a href='#' onclick='loadPaginationPage(1);return false;'>1</a></li>");

                                    paginationDown.Append("<div class='container' ><div class='row'><div class='col-xs-12'><ul class='pagination text-center' id='ulpgdnid'>");
                                    paginationDown.Append("<li> Page: <input type='text' style='width:25px' id='paginationdownid' value='1'><input type='button' value='Go' style='width=20px' onclick='loadPaginationOrigPage(2)'/><label>&nbsp;&nbsp;Total Pages: ").Append(pgr).Append("</label><input type='hidden' name='totdownpgs' value='").Append(pgr).Append("'>");
                                    paginationDown.Append("</li><li><a id='pgdnprevid' href='#' onclick='loadPaginationPrevPage();return false;'>« Previous</a></li>");
                                    paginationDown.Append("<li id='pgdnli1' class='active disabled'><a href='#' onclick='loadPaginationPage(1);return false;'>1</a></li>");

                                    for (int pp = 2; pp <= 5; pp++)
                                    {
                                        pagination.Append("<li ");
                                        paginationDown.Append("<li ");
                                        //(rowsCount > 5 ? 5 : rowsCount)
                                        if (pp > rowsCount)
                                        {
                                            pagination.Append(" style='display:none' ");
                                            paginationDown.Append(" style='display:none' ");
                                        }
                                        pagination.Append(" id='pgli").Append(pp).Append("' ><a href='#' onclick='loadPaginationPage(").Append(pp).Append(");return false;'>").Append(pp).Append("</a></li>");
                                        paginationDown.Append(" id='pgdnli").Append(pp).Append("' ><a href='#' onclick='loadPaginationPage(").Append(pp).Append(");return false;'>").Append(pp).Append("</a></li>");
                                    }
                                    pagination.Append("<li><a id = 'pgnextid' href = '#' onclick = 'loadPaginationNextPage();return false;' >Next » </a></li></ul></div></div></div> ");
                                    paginationDown.Append("<li><a id = 'pgdnnextid' href = '#' onclick = 'loadPaginationNextPage();return false;' >Next » </a></li></ul></div></div></div> ");
                                }

                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    string cn = dt.Columns[i].ColumnName;
                                    if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                                    int pos = cols.IndexOf(cn);
                                    if (cols.IndexOf("HIDDEN_" + dt.Columns[i].ColumnName) != -1)
                                    {
                                        hiddens.Add(dt.Columns[i].ColumnName);
                                    }
                                    if (pos > -1)
                                    {
                                        if (!isTableStart)
                                        {
                                            isTableStart = true;
                                            sb.Append(pagination.ToString());
                                            sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(s.compTextTx).Append("</span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");
                                            if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
                                            if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
                                            sb.Append("><thead>");
                                            sb.Append("<tr>");
                                        }
                                        cn = colvals[pos];
                                        if (!string.IsNullOrEmpty(Checkbox))
                                        {
                                            sb.Append(Checkbox);
                                            Checkbox = string.Empty;
                                        }
                                        if (!string.IsNullOrEmpty(Radio))
                                        {
                                            sb.Append(Radio);
                                            Radio = string.Empty;
                                        }
                                        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                                        if (!isColExists) isColExists = true;
                                    }
                                }
                                bool isEdit = cols.IndexOf("EDIT") > -1;
                                bool isPreview = cols.IndexOf("VIEW") > -1;
                                bool isCancel = cols.IndexOf("CANCEL") > -1;
                                bool isEditSpl = cols.IndexOf("EDIT_SPL") > -1;
                                bool isQrtr = cols.IndexOf("QRTR") > -1;
                                if (isTableStart)
                                {
                                    if (isEdit) sb.Append("<th class='searchResultsHeading'>Action</th>");
                                    if (isEditSpl) sb.Append("<th class='searchResultsHeading'>Action</th>");
                                    if (isPreview) sb.Append("<th></th>");
                                    if (isCancel) sb.Append("<th></th>");
                                    if (isQrtr) sb.Append("<th class='searchResultsHeading'>Report View</th>");
                                    sb.Append("</thead></tr><tbody id='pagesearchtbid'>");

                                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                                    {
                                        bool pass = true;
                                        /*if (screen.resActionClass.manSearchFilter != null && screen.resActionClass.manSearchFilter.Count > 0)
                                        {
                                            for (int yy = 0; pass && yy < screen.resActionClass.manSearchFilter.Count; yy++)
                                            {
                                                string kk = screen.resActionClass.manSearchFilter[yy];
                                                string kkk = kk.Replace("MANSCR_", "");
                                                if (dt.Columns.Contains(kkk) && frm[kk] != "")
                                                {
                                                    if (!frm[kk].Equals(Convert.ToString(dt.Rows[i][kkk])))
                                                        pass = false;
                                                }
                                            }
                                        }*/
                                        if (pass == true)
                                        {
                                            sb.Append("<tr>");
                                            // if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                            if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name='chkList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                            if (isRADIO) sb.Append("<td><input type=\"radio\" name='radioList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"radio_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                            for (int j = 0; j < dt.Columns.Count; j++)
                                            {
                                                string cn = dt.Columns[j].ColumnName;
                                                int pos = cols.IndexOf(cn);
                                                if (pos > -1)
                                                    sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                                            }
                                            foreach (string hdstr in hiddens)
                                            {
                                                hidden.Append("<input type='hidden' name='").Append(hdstr).Append("' value='").Append(dt.Rows[i][hdstr].ToString()).Append("_").Append(dt.Rows[i]["ID"].ToString()).Append("'>");
                                            }
                                            if (isQrtr)
                                            {
                                                if (dt.Columns.Contains("EDIT_SCR_ID") && !string.IsNullOrEmpty(dt.Rows[i]["EDIT_SCR_ID"].ToString()))
                                                {
                                                    int edscr_id = 0;
                                                    int.TryParse(dt.Rows[i]["EDIT_SCR_ID"].ToString(), out edscr_id);
                                                    edit_scr_id = edscr_id;
                                                }
                                                string editcaption = colvals[cols.IndexOf("QRTR")];
                                                if (string.IsNullOrEmpty(editcaption))
                                                {
                                                    editcaption = "View";
                                                }
                                                string colVal = String.Empty;
                                                bool proceed = false;
                                                if (dt.Columns.Contains(editcaption) && !string.IsNullOrEmpty(dt.Rows[i][editcaption].ToString()))
                                                {
                                                    colVal = dt.Rows[i][editcaption].ToString();
                                                    if (colVal.Equals(editcaption)) proceed = true;
                                                    editcaption = colVal;
                                                }
                                                else proceed = true;
                                                if (proceed)
                                                {
                                                    sb.Append("<td><a href='#'");
                                                    if (isIdExists) sb.Append(" onclick='screenAction(").Append(edit_scr_id).Append(",").Append("0,").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                                    sb.Append(">").Append(editcaption).Append("</a></td>");
                                                }
                                                else
                                                {
                                                    sb.Append("<td>").Append(editcaption).Append("</td>");
                                                }
                                            }
                                            if (isEdit)
                                            {
                                                if (dt.Columns.Contains("EDIT_SCR_ID") && !string.IsNullOrEmpty(dt.Rows[i]["EDIT_SCR_ID"].ToString()))
                                                {
                                                    int edscr_id = 0;
                                                    int.TryParse(dt.Rows[i]["EDIT_SCR_ID"].ToString(), out edscr_id);
                                                    edit_scr_id = edscr_id;
                                                }
                                                string editcaption = colvals[cols.IndexOf("EDIT")];
                                                if (string.IsNullOrEmpty(editcaption))
                                                {
                                                    editcaption = "Edit";
                                                }
                                                string colVal = String.Empty;
                                                bool proceed = false;
                                                if (dt.Columns.Contains(editcaption) && !string.IsNullOrEmpty(dt.Rows[i][editcaption].ToString()))
                                                {
                                                    colVal = dt.Rows[i][editcaption].ToString();
                                                    if (colVal.Equals(editcaption)) proceed = true;
                                                    editcaption = colVal;
                                                }
                                                else proceed = true;
                                                if (proceed)
                                                {

                                                    sb.Append("<td><a href='#'");
                                                    if (screen.Screen_Name_Tx == "Admin Track Financial Assistance Request")
                                                    {
                                                        if (dt.Rows[i]["REQUEST_TYPE_TX"].ToString() == "EDUCATION ALLOWANCE")
                                                        {
                                                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append("955").Append(")' ");
                                                        }
                                                        if (dt.Rows[i]["REQUEST_TYPE_TX"].ToString() == "MEDICAL REIMBURSEMENT")
                                                        {
                                                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(edit_scr_id).Append(")' ");
                                                        }
                                                        if (dt.Rows[i]["REQUEST_TYPE_TX"].ToString() == "FINANCIAL ASSISTANCE IN CASE OF MEMBER DEATH")
                                                        {
                                                            if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(edit_scr_id).Append(")' ");
                                                        }
                                                    }
                                                    else
                                                    {                                                        
                                                        if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(edit_scr_id).Append(")' ");
                                                        
                                                    } 
                                                    sb.Append(">").Append(editcaption).Append("</a></td>");
                                                }
                                                else
                                                {
                                                    sb.Append("<td>").Append(editcaption).Append("</td>");
                                                }
                                            }
                                            if (isEditSpl)
                                            {
                                                string scrId = "0";
                                                if (dt.Columns.Contains("REG_COM_TYPE_ID"))
                                                {
                                                    scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1" && dt.Rows[i]["REG_COM_TYPE_ID"].ToString() != "0") ? "134" : "133";
                                                }
                                                sb.Append("<td><a href='#'");
                                                long unique_reg_id = Int64.Parse(dt.Rows[i]["UNIQUE_REG_ID"].ToString());
                                                if (isIdExists) sb.Append(" onclick=").Append("'loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(scrId).Append(",").Append(unique_reg_id).Append(")'");
                                                string editcaption = colvals[cols.IndexOf("EDIT_SPL")];
                                                if (string.IsNullOrEmpty(editcaption))
                                                {
                                                    editcaption = "Edit";
                                                }
                                                sb.Append(">" + editcaption + "</a></td>");
                                            }
                                            if (isPreview)
                                            {
                                                string Id = string.Empty;
                                                if (!string.IsNullOrEmpty(s.columnNameTx))
                                                    Id = dt.Rows[i][s.columnNameTx].ToString();
                                                else
                                                    Id = dt.Rows[i]["ID"].ToString();
                                                //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                                                var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                                                Id = value + "_" + Id;
                                                if (isIdExists) sb.Append("<td><a target='_blank' href='../Home/DownloadFile/" + Id + "'");
                                                //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                                sb.Append(">Click to View</a></td>");
                                            }
                                            if (isCancel)
                                            {
                                                sb.Append("<td><u><a href=\"#\" data-toggle=\"modal\" data-target=\"#exampleModal\" onclick=\"Cancel(" + dt.Rows[i]["ID"].ToString() + ")\">Cancel</a></u></td>");
                                            }
                                            sb.Append("</tr>");
                                        }
                                    }

                                    sb.Append("</tbody></table></div></div></div></div>");
                                    sb.Append(paginationDown.ToString());
                                    sb.Append(hidden.ToString());
                                }
                            }

                            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("<div id='divSearchResult' style='display: none'></div>", sb.ToString());

                        }
                        else if (ScreenType != null && ScreenType == "report")
                        {
                            Dictionary<string, object> conds = new Dictionary<string, object>();
                            Dictionary<string, string> condOps = new Dictionary<string, string>();
                            Dictionary<string, object> dictExcelRpts = new Dictionary<string, object>();
                            conds.Add("SCREEN_ID", screen.ID);
                            string strRptSession = string.Empty;
                            string strRptType = string.Empty;
                            string strRptCols = string.Empty;
                            DataTable dtRep = getData("stimulate", "REPORT_T", conds, condOps, 0, 100);
                            if (dtRep != null && dtRep.Rows != null && dtRep.Rows.Count > 0)
                            {
                                int repId = Convert.ToInt32(dtRep.Rows[0]["ID"]);
                                strRptType = Convert.ToString(dtRep.Rows[0]["REPORT_TYPE"]);
                                strRptCols = Convert.ToString(dtRep.Rows[0]["COLUMNS_TX"]);
                                conds.Clear();
                                conds.Add("REPORT_ID", repId);
                                conds.Add("ACTIVE_YN", 1);

                                DataTable dtRepComps = getData("stimulate", "REPORT_COMP_T", conds, condOps, 0, 100);
                                if (dtRepComps != null && dtRepComps.Rows != null && dtRepComps.Rows.Count > 0)
                                {
                                    List<Report_Comp_T> lstRptComps = new List<Report_Comp_T>();
                                    foreach (DataRow Rptrow in dtRepComps.Rows)
                                    {
                                        lstRptComps.Add(getReportComponent(Rptrow, screen.WebAppSchemaNameTx, screen.ApplicationSchemaNameTx, screen.ModuleSchemaNameTx, screen.schemaNameTx));
                                    }
                                    StringBuilder screenHTML = new StringBuilder();
                                    lstRptComps = lstRptComps.OrderBy(x => x.Order_Nm).ToList<Report_Comp_T>();
                                    renderReportComponent(screen, lstRptComps, screenHTML, frm);
                                    screen.Screen_Content_Tx = screen.Screen_Content_Tx + screenHTML.ToString();
                                    List<Report_Comp_T> lstbodyComp = lstRptComps.Where(x => x.RepType == "B").ToList<Report_Comp_T>();
                                    StringBuilder rptBodyContent = new StringBuilder();
                                    //LOAD PARAMETERS
                                    List<Report_Comp_T> lstRptParams = lstRptComps.Where(x => x.RepType == "P").ToList<Report_Comp_T>();
                                    Dictionary<string, string> dictRptParams = new Dictionary<string, string>();
                                    if (frm.Keys.Count > 4)
                                    {
                                        foreach (Report_Comp_T RptComp in lstRptParams)
                                        {
                                            if (!string.IsNullOrEmpty(frm[RptComp.compNameTx]))
                                            {
                                                string rptVal = Convert.ToString(frm[RptComp.compNameTx]);
                                                string[] val = rptVal.Split(',');
                                                if (val.Length > 1)
                                                {
                                                    if (!dictRptParams.ContainsKey("Date") && RptComp.compTextTx.Contains("Date"))
                                                    {
                                                        if (!dictRptParams.ContainsValue(val[0] + " to " + val[1]))
                                                        {
                                                            dictRptParams.Add("Date", val[0].Replace("-", "/") + " to " + val[1].Replace("-", "/"));
                                                        }
                                                    }
                                                    else if (!dictRptParams.ContainsKey("Ack") && RptComp.compTextTx.Contains("Ack"))
                                                    {
                                                        if (!dictRptParams.ContainsValue(val[0] + " to " + val[1]))
                                                        {
                                                            dictRptParams.Add("Acknowledgement", val[0] + " to " + val[1]);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    conds.Clear();
                                                    if (RptComp.compTextTx.ToUpper().Equals("FEE HEAD"))
                                                    {
                                                        conds.Add("ITEM_CODE", frm[RptComp.compNameTx]);
                                                    }
                                                    else if (RptComp.compTextTx.ToUpper().Equals("TAX MODE"))
                                                    {
                                                        if (Convert.ToInt32(frm[RptComp.compNameTx]) == 0)
                                                        {
                                                            dictRptParams.Add("Tax Mode", "Non-Taxable");
                                                        }
                                                        else
                                                        {
                                                            dictRptParams.Add("Tax Mode", "Taxable");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        conds.Add("ID", frm[RptComp.compNameTx]);
                                                    }
                                                    conds.Add("ACTIVE_YN", 1);
                                                    DataTable dt = getData(RptComp.schemaNameTx, RptComp.tableNameTx, conds, condOps, 0, 100);
                                                    if (!dictRptParams.ContainsKey(RptComp.compTextTx))
                                                    {
                                                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                                        {
                                                            dictRptParams.Add(RptComp.compTextTx, Convert.ToString(dt.Rows[0][RptComp.columnNameTx]));
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                conds.Clear();
                                                conds.Add("ID", RptComp.Id);
                                                JObject jdata = DBTable.GetData("queryreport", conds, "Report_COMP_T", 0, 100, "stimulate");
                                                DataTable dt = new DataTable();
                                                foreach (JProperty property in jdata.Properties())
                                                {
                                                    if (property.Name == "query" || property.Name == "report" || property.Name == "queryreport")
                                                    {
                                                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                                    }
                                                }
                                                if (!dictRptParams.ContainsKey(RptComp.compTextTx))
                                                {
                                                    StringBuilder sbDataVal = new StringBuilder();
                                                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                                    {
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (i == 0) sbDataVal.Append(Convert.ToString(dt.Rows[i][RptComp.columnNameTx]));
                                                            else sbDataVal.Append("<br>").Append(",").Append(Convert.ToString(dt.Rows[i][RptComp.columnNameTx]));
                                                        }
                                                    }
                                                    dictRptParams.Add(RptComp.compTextTx, sbDataVal.ToString());
                                                }
                                            }
                                        }
                                    }
                                    //LOAD REPORTS

                                    if (frm.Keys.Count > 4)
                                    {
                                        foreach (Report_Comp_T rptComp in lstbodyComp)
                                        {
                                            JObject jobjects = UtilService.LoadReport(WEB_APP_ID, frm, ScreenType, Convert.ToString(repId), rptComp);
                                            DataTable dt = null;
                                            if (jobjects != null && jobjects.HasValues)
                                            {
                                                foreach (JProperty property in jobjects.Properties())
                                                {
                                                    if (property.Name == "report")
                                                    {
                                                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                                                    }
                                                }
                                                screen.Action_Tx = "update";
                                            }
                                            StringBuilder sb = new StringBuilder();
                                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                            {
                                                string dtSession = rptComp.compTextTx.ToUpper().Replace(" ", "_");
                                                HttpContext.Current.Session[dtSession] = dt;
                                                List<string> cols = new List<string>();
                                                if (rptComp.compContentTx != null && rptComp.compContentTx.IndexOf(",") > -1) cols = rptComp.compContentTx.Split(',').ToList<string>();
                                                List<string> colvals = new List<string>();
                                                if (rptComp.compValueTx != null && rptComp.compValueTx.IndexOf(",") > -1) colvals = rptComp.compValueTx.Split(',').ToList<string>();
                                                bool isTableStart = false;

                                                bool isColExists = false;
                                                bool isIdExists = false;

                                                DataTable dtExcelRpt = new DataTable();

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
                                                            sb.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'><h4 class='text-on-pannel'><span class='fontBold'>").Append(rptComp.compTextTx).Append("</span></h4><div style='overflow-x:auto;'><table class='table table-bordered tableSearchResults'");
                                                            if (rptComp.compStyleTx != null && !rptComp.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(rptComp.compStyleTx).Append("' ");
                                                            if (rptComp.compScriptTx != null && !rptComp.compScriptTx.Trim().Equals("")) sb.Append(rptComp.compStyleTx);
                                                            sb.Append("><thead>");
                                                            sb.Append("<tr>");
                                                        }
                                                        cn = colvals[pos];

                                                        sb.Append("<th class='searchResultsHeading'>").Append(cn).Append("</th>");
                                                        //ADDING HEADER COLUMNS TO DATATABLE FOR EXCEL
                                                        dtExcelRpt.Columns.Add(cn);
                                                        if (!isColExists) isColExists = true;
                                                    }
                                                }
                                                int totalAmount = 0;
                                                int amtPos = 0;
                                                int totalColCount = dt.Columns.Count;
                                                if (isTableStart)
                                                {
                                                    sb.Append("</thead></tr><tbody>");

                                                    for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                                                    {
                                                        DataRow drExcelRow = dtExcelRpt.NewRow();
                                                        sb.Append("<tr>");
                                                        for (int j = 0; j < dt.Columns.Count; j++)
                                                        {
                                                            string cn = dt.Columns[j].ColumnName;

                                                            if (cn.Equals("AMOUNT") || cn.Equals("TOTAL_AMT") || cn.Equals("AMT") || cn.Equals("FEE_AMT")
                                                                || cn.Equals("NON_TAXABLE_AMT") || cn.Equals("TAXABLE_AMT") || cn.Equals("TOTAL_TAX_AMT")
                                                                || cn.Equals("IGST_AMT") || cn.Equals("CGST_AMT") || cn.Equals("SGST_AMT") || cn.Equals("UTGST_AMT")
                                                                || cn.Equals("ROUND_OFF") || cn.Equals("TOTAL_AMOUNT"))
                                                            {
                                                                if (amtPos == 0) amtPos = j;
                                                                if (amtPos == j) totalAmount = totalAmount + Convert.ToInt32(dt.Rows[i][cn]);
                                                            }
                                                            int pos = cols.IndexOf(cn);
                                                            int isNum = 0;
                                                            if (pos > -1)
                                                            {
                                                                if (int.TryParse(Convert.ToString(dt.Rows[i][cn]), out isNum))
                                                                {
                                                                    sb.Append("<td><div align='right'>" + dt.Rows[i][cn] + "</div></td>");
                                                                }
                                                                else
                                                                {
                                                                    sb.Append("<td><div align='left'>" + dt.Rows[i][cn] + "</div></td>");
                                                                }
                                                            }
                                                            //ADDING ROWS TO DATATABLE FOR EXPORT TO EXCEL
                                                            drExcelRow[j] = dt.Rows[i][cn];
                                                        }
                                                        dtExcelRpt.Rows.Add(drExcelRow);
                                                        sb.Append("</tr>");
                                                    }
                                                    sb.Append("<tr>");
                                                    for (int i = 0; i < totalColCount; i++)
                                                    {
                                                        if (i == amtPos && amtPos > 0)
                                                        {
                                                            sb.Append("<td><div align='right'><b>").Append(totalAmount).Append("</b></div></td>");
                                                        }
                                                        else if (i == amtPos - 1)
                                                        {
                                                            sb.Append("<td><div align='left'><b>Total Amount(Rs).</div></b></td>");
                                                        }
                                                        else
                                                        {
                                                            sb.Append("<td></td>");
                                                        }
                                                    }
                                                    sb.Append("</tbody></table></div></div></div></div>");
                                                }
                                                dictExcelRpts.Add(rptComp.compTextTx, dtExcelRpt);
                                                dictExcelRpts.Add(rptComp.compTextTx + "_TOTAL", totalAmount);
                                            }
                                            rptBodyContent.Append(sb.ToString());
                                        }
                                    }

                                    //ADD REMAINING KEYS FOR GENERATING AN EXCEL
                                    if (dictExcelRpts.Count > 0)
                                    {
                                        dictExcelRpts.Add("REPORT_NAME", dtRep.Rows[0]["TITLE_TX"]);
                                        //if (dictRptParams.Count == 0)                                        
                                        //{
                                        //    dictRptParams.Add("Date", DateTime.Now.ToString("MM/dd/yyyy") + " to " + DateTime.Now.ToString("MM/dd/yyyy"));
                                        //}
                                        dictExcelRpts.Add("PARAMS", dictRptParams);
                                        strRptSession = Convert.ToString(dtRep.Rows[0]["TITLE_TX"]).Replace(" ", "_").ToUpper();
                                    }
                                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("<div id='divSearchResult' style='display: none'></div>", rptBodyContent.ToString());
                                }
                            }
                            string strValue = string.Empty;
                            if (strRptType == "MONTHLY" || strRptType == "DAILY")
                            {
                                if (frm.AllKeys.Contains(strRptCols))
                                {
                                    strValue = Convert.ToString(frm[strRptCols]);
                                }// : DateTime.Now.ToString("dd/MM/yyyy");
                            }
                            //if (!string.IsNullOrEmpty(strValue))
                            //{
                            screen.Screen_Content_Tx = screen.Screen_Content_Tx + "<input type='hidden' value='" + strValue + "' name='DATE_TX' />";
                            //}
                            if (dictExcelRpts.Count > 0)
                            {
                                HttpContext.Current.Session[strRptSession] = JsonConvert.SerializeObject(dictExcelRpts);
                                screen.Screen_Content_Tx = screen.Screen_Content_Tx + "<input type='hidden' value='" + strRptSession + "' name='REPORT_SESSION' />";
                            }
                        }

                        if (!screen.Screen_Content_Tx.Contains("name='si'"))
                        {
                            screen.Screen_Content_Tx = screen.Screen_Content_Tx + "<input type='hidden' value='" + frm["si"] + "' name='si' />";
                        }
                        return screen;
                    }
                }
            }
            return null;
        }

        public static string AjaxSearch(int WEB_APP_ID, FormCollection frm)
        {
            StringBuilder sb = new StringBuilder();
            string userid = frm["u"];
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            if (ScreenType != null && ScreenType == "search")
            {
                DataRow row = null;
                if (menuid != null && !menuid.Trim().Equals("")) row = UtilService.fetchMenuItem(WEB_APP_ID, Convert.ToInt32(menuid));
                if (screenId == null || screenId.Trim().Equals(""))
                {
                    screenId = null;
                }
                else
                {
                    if (!UtilService.isResponsibilityExists(Convert.ToInt32(screenId)))
                    {
                        screenId = null;
                    }
                }
                if (row != null || screenId != null)
                {
                    Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                    Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.SEARCH)).FirstOrDefault();
                    if (ScreenType == "searchwithstudent")
                    {
                        if (!string.IsNullOrEmpty(s.compNameTx))
                        {
                            frm.Add("REPLACE_" + s.compNameTx, Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));
                        }
                    }
                    int? edit_scr_id = 0;

                    if (screen.Edit_Scr_Id != null)
                    {
                        edit_scr_id = screen.Edit_Scr_Id;
                    }
                    if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                    {
                        screen.resActionClass = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "search" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id), "", screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                    }
                    else screen.resActionClass = UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id));
                    JObject jobjects = JObject.Parse(Convert.ToString(screen.resActionClass.DecryptData));
                    DataTable dt = new DataTable();
                    if (jobjects.HasValues)
                    {
                        foreach (JProperty property in jobjects.Properties())
                        {
                            if (property.Name == "search")
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }
                    }
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        List<string> cols = new List<string>();
                        if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                        List<string> colvals = new List<string>();
                        if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();

                        bool isColExists = false;
                        bool isIdExists = false;

                        string Checkbox = string.Empty;
                        bool isCHECKBOX = cols.IndexOf("CHECKBOX") > -1;

                        string Radio = string.Empty;
                        bool isRADIO = cols.IndexOf("RADIO") > -1;
                        bool isEdit = cols.IndexOf("EDIT") > -1;
                        bool isPreview = cols.IndexOf("VIEW") > -1;
                        bool isCancel = cols.IndexOf("CANCEL") > -1;
                        bool isEditSpl = cols.IndexOf("EDIT_SPL") > -1;
                        bool isQrtr = cols.IndexOf("QRTR") > -1;
                        for (int i = 0; (!isColExists || !isIdExists) && i < dt.Columns.Count; i++)
                        {

                            string cn = dt.Columns[i].ColumnName;
                            if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                            int pos = cols.IndexOf(cn);
                            if (pos > -1)
                            {
                                if (!isColExists) isColExists = true;
                            }
                        }
                        for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                        {
                            bool pass = true;
                            if (screen.resActionClass.manSearchFilter != null && screen.resActionClass.manSearchFilter.Count > 0)
                            {
                                for (int yy = 0; pass && yy < screen.resActionClass.manSearchFilter.Count; yy++)
                                {
                                    string kk = screen.resActionClass.manSearchFilter[yy];
                                    string kkk = kk.Replace("MANSCR_", "");
                                    if (dt.Columns.Contains(kkk) && frm[kk] != "")
                                    {
                                        if (!frm[kk].Equals(Convert.ToString(dt.Rows[i][kkk])))
                                            pass = false;
                                    }
                                }
                            }
                            if (pass == true)
                            {
                                sb.Append("<tr>");
                                if (isCHECKBOX) sb.Append("<td><input type=\"checkbox\" name='chkList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"chk_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                if (isRADIO) sb.Append("<td><input type=\"radio\" name='radioList' value=" + dt.Rows[i]["ID"].ToString() + " id=\"radio_" + dt.Rows[i]["ID"].ToString() + "\" /></td>");
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    string cn = dt.Columns[j].ColumnName;
                                    int pos = cols.IndexOf(cn);
                                    if (pos > -1)
                                        sb.Append("<td>" + dt.Rows[i][cn] + "</td>");
                                }
                                if (isQrtr)
                                {
                                    if (dt.Columns.Contains("EDIT_SCR_ID") && !string.IsNullOrEmpty(dt.Rows[i]["EDIT_SCR_ID"].ToString()))
                                    {
                                        int edscr_id = 0;
                                        int.TryParse(dt.Rows[i]["EDIT_SCR_ID"].ToString(), out edscr_id);
                                        edit_scr_id = edscr_id;
                                    }
                                    string editcaption = colvals[cols.IndexOf("QRTR")];
                                    if (string.IsNullOrEmpty(editcaption))
                                    {
                                        editcaption = "View";
                                    }
                                    string colVal = String.Empty;
                                    bool proceed = false;
                                    if (dt.Columns.Contains(editcaption) && !string.IsNullOrEmpty(dt.Rows[i][editcaption].ToString()))
                                    {
                                        colVal = dt.Rows[i][editcaption].ToString();
                                        if (colVal.Equals(editcaption)) proceed = true;
                                        editcaption = colVal;
                                    }
                                    else proceed = true;
                                    if (proceed)
                                    {
                                        sb.Append("<td><a href='#'");
                                        if (isIdExists) sb.Append(" onclick='screenAction(").Append(edit_scr_id).Append(",").Append("0,").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                        sb.Append(">").Append(editcaption).Append("</a></td>");
                                    }
                                    else
                                    {
                                        sb.Append("<td>").Append(editcaption).Append("</td>");
                                    }
                                }
                                if (isEdit)
                                {
                                    if (dt.Columns.Contains("EDIT_SCR_ID") && !string.IsNullOrEmpty(dt.Rows[i]["EDIT_SCR_ID"].ToString()))
                                    {
                                        int edscr_id = 0;
                                        int.TryParse(dt.Rows[i]["EDIT_SCR_ID"].ToString(), out edscr_id);
                                        edit_scr_id = edscr_id;
                                    }
                                    string editcaption = colvals[cols.IndexOf("EDIT")];
                                    if (string.IsNullOrEmpty(editcaption))
                                    {
                                        editcaption = "Edit";
                                    }
                                    string colVal = String.Empty;
                                    bool proceed = false;
                                    if (dt.Columns.Contains(editcaption) && !string.IsNullOrEmpty(dt.Rows[i][editcaption].ToString()))
                                    {
                                        colVal = dt.Rows[i][editcaption].ToString();
                                        if (colVal.Equals(editcaption)) proceed = true;
                                        editcaption = colVal;
                                    }
                                    else proceed = true;
                                    if (proceed)
                                    {
                                        sb.Append("<td><a href='#'");
                                        if (isIdExists) sb.Append(" onclick='loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(edit_scr_id).Append(")' ");
                                        sb.Append(">").Append(editcaption).Append("</a></td>");
                                    }
                                    else
                                    {
                                        sb.Append("<td>").Append(editcaption).Append("</td>");
                                    }
                                }
                                if (isEditSpl)
                                {
                                    string scrId = "0";
                                    if (dt.Columns.Contains("REG_COM_TYPE_ID"))
                                    {
                                        scrId = (dt.Rows[i]["REG_COM_TYPE_ID"].ToString() == "1" && dt.Rows[i]["REG_COM_TYPE_ID"].ToString() != "0") ? "134" : "133";
                                    }
                                    sb.Append("<td><a href='#'");
                                    long unique_reg_id = Int64.Parse(dt.Rows[i]["UNIQUE_REG_ID"].ToString());
                                    if (isIdExists) sb.Append(" onclick=").Append("'loadRecord(").Append(dt.Rows[i]["ID"].ToString()).Append(",").Append(scrId).Append(",").Append(unique_reg_id).Append(")'");
                                    string editcaption = colvals[cols.IndexOf("EDIT_SPL")];
                                    if (string.IsNullOrEmpty(editcaption))
                                    {
                                        editcaption = "Edit";
                                    }
                                    sb.Append(">" + editcaption + "</a></td>");
                                }
                                if (isPreview)
                                {
                                    string Id = string.Empty;
                                    if (!string.IsNullOrEmpty(s.columnNameTx))
                                        Id = dt.Rows[i][s.columnNameTx].ToString();
                                    else
                                        Id = dt.Rows[i]["ID"].ToString();
                                    //string m = Enum.GetName(typeof(TableName), s.tableNameTx);
                                    var value = (Int32)Enum.Parse(typeof(TableName), s.tableNameTx);
                                    Id = value + "_" + Id;
                                    if (isIdExists) sb.Append("<td><a target='_blank' href='../Home/DownloadFile/" + Id + "'");
                                    //if (isIdExists) sb.Append(" onclick='loadPreview(").Append(dt.Rows[i]["ID"].ToString()).Append(")' ");
                                    sb.Append(">Click to View</a></td>");
                                }
                                if (isCancel)
                                {
                                    sb.Append("<td><u><a href=\"#\" data-toggle=\"modal\" data-target=\"#exampleModal\" onclick=\"Cancel(" + dt.Rows[i]["ID"].ToString() + ")\">Cancel</a></u></td>");
                                }
                                sb.Append("</tr>");
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }


        public static void AjaxSearchData(int WEB_APP_ID, FormCollection frm)
        {
            StringBuilder sb = new StringBuilder();
            string userid = frm["u"];
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook worKbooK;
            Microsoft.Office.Interop.Excel.Worksheet worKsheeT;
            Microsoft.Office.Interop.Excel.Range celLrangE;
            if (ScreenType != null && ScreenType == "search")
            {
                DataRow row = null;
                if (menuid != null && !menuid.Trim().Equals("")) row = UtilService.fetchMenuItem(WEB_APP_ID, Convert.ToInt32(menuid));
                if (screenId == null || screenId.Trim().Equals(""))
                {
                    screenId = null;
                }
                else
                {
                    if (!UtilService.isResponsibilityExists(Convert.ToInt32(screenId)))
                    {
                        screenId = null;
                    }
                }
                if (row != null || screenId != null)
                {
                    Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, screenId, frm);
                    Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(HTMLTag.SEARCH)).FirstOrDefault();
                    if (ScreenType == "searchwithstudent")
                    {
                        if (!string.IsNullOrEmpty(s.compNameTx))
                        {
                            frm.Add("REPLACE_" + s.compNameTx, Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]));
                        }
                    }
                    int? edit_scr_id = 0;

                    if (screen.Edit_Scr_Id != null)
                    {
                        edit_scr_id = screen.Edit_Scr_Id;
                    }
                    if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                    {
                        screen.resActionClass = (ActionClass)executeMethod(screen.Screen_Class_Name_Tx, null, "search" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id), "", screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                    }
                    else screen.resActionClass = UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(s.Id));
                    JObject jobjects = JObject.Parse(Convert.ToString(screen.resActionClass.DecryptData));
                    DataTable dt = new DataTable();
                    if (jobjects.HasValues)
                    {
                        foreach (JProperty property in jobjects.Properties())
                        {
                            if (property.Name == "search")
                            {
                                dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            }
                        }
                    }
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        List<string> cols = new List<string>();
                        if (s.compContentTx != null && s.compContentTx.IndexOf(",") > -1) cols = s.compContentTx.Split(',').ToList<string>();
                        List<string> colvals = new List<string>();
                        if (s.compValueTx != null && s.compValueTx.IndexOf(",") > -1) colvals = s.compValueTx.Split(',').ToList<string>();

                        bool isColExists = false;
                        bool isIdExists = false;

                        for (int i = 0; (!isColExists || !isIdExists) && i < dt.Columns.Count; i++)
                        {

                            string cn = dt.Columns[i].ColumnName;
                            if (!isIdExists && cn.Equals("ID")) isIdExists = true;
                            int pos = cols.IndexOf(cn);
                            if (pos > -1)
                            {
                                if (!isColExists) isColExists = true;
                            }
                        }
                        try
                        {
                            excel = new Microsoft.Office.Interop.Excel.Application();
                            excel.Visible = false;
                            excel.DisplayAlerts = false;
                            worKbooK = excel.Workbooks.Add(Type.Missing);
                            worKsheeT = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;

                            for (int i = 0; isColExists && i < dt.Rows.Count; i++)
                            {
                                try
                                {

                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        catch (Exception exxx)
                        {

                        }
                    }
                }
            }

        }

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }


        public static object executeMethod(string className, object constructorParam, string methodName, object[] methodConstructorParam, bool isClassStatic, bool isMethodStatic)
        {
            Type objectClass = GetType(className);
            MethodInfo methodInfo = null;
            dynamic result = null;
            if (!isClassStatic)
            {
                object obj = createInistance(objectClass, constructorParam, isClassStatic);
                FieldInfo[] fields = objectClass.GetFields();
                MethodInfo[] methods = objectClass.GetMethods();
                MethodInfo[] staticMethods = objectClass.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                if (isMethodStatic)
                {
                    for (int i = 0; staticMethods != null && i < staticMethods.Length; i++)
                    {
                        MethodInfo m = staticMethods[i];
                        if (m.Name.Equals(methodName))
                        {
                            methodInfo = m;
                            i = staticMethods.Length;
                        }
                    }

                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        //object classInstance = Activator.CreateInstance(objectClass, null);
                        result = methodInfo.Invoke(null, parameters.Length == 0 ? null : methodConstructorParam);
                    }
                }
                else
                {
                    for (int i = 0; methods != null && i < methods.Length; i++)
                    {
                        MethodInfo m = methods[i];
                        if (m.Name.Equals(methodName))
                        {
                            methodInfo = m;
                            i = methods.Length;
                        }
                    }
                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        //object classInstance = Activator.CreateInstance(objectClass, null);
                        result = methodInfo.Invoke(obj, parameters.Length == 0 ? null : methodConstructorParam);
                    }
                }

            }
            else
            {
                FieldInfo[] fields = objectClass.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                MethodInfo[] methods = objectClass.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                for (int i = 0; methods != null && i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (m.Name.Equals(methodName))
                    {
                        methodInfo = m;
                        i = methods.Length;
                    }
                }
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    //object classInstance = Activator.CreateInstance(objectClass, null);
                    result = methodInfo.Invoke(null, parameters.Length == 0 ? null : parameters);
                }
            }
            return result;
        }
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static void uploadDocuments(int refid, Screen_T s)
        {
            // get the upload documents from session
            // check whether the UPLOAD DOCUMENT =S exists or NOT or empty
            // update the refid in the documentreference object
            // traverse each object from the list and insert into the table DOCUMENT_T - we can do single by single or multiple
            // when traverse all the new records should be inserted, make sure no ID present
            // removed cases handle ACTIVE_YN=0
            // isChanged cases make sure the ID present
            //string Schema = DBTable.SCREEN_T.AsEnumerable().Where(x => x.Field<long>("ID") == Convert.ToInt32(screen_id)).Select(y => y.Field<string>("SCHEMA_NAME_TX")).FirstOrDefault();
            List<DocumentReference> docs = null;
            List<DocumentReference> LOAD_docs = null;
            if (HttpContext.Current.Session["UPLOAD_DOCUMENTS"] != null)
            {
                docs = (List<DocumentReference>)HttpContext.Current.Session["UPLOAD_DOCUMENTS"];
            }
            if (HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"] != null)
            {
                LOAD_docs = (List<DocumentReference>)HttpContext.Current.Session["LOAD_UPLOAD_DOCUMENTS"];
            }
            if (docs != null && docs.Count > 0)
            {
                var updatelist = LOAD_docs.Except(docs).ToList();
                var insertlist = docs.Except(LOAD_docs).ToList();
                if (LOAD_docs.Count > 0 && updatelist.Count > 0)
                {
                    for (int i = 0; i < updatelist.Count; i++)
                    {
                        FormCollection frm = new FormCollection();
                        frm.Add("ID", Convert.ToString(updatelist[i].ID));
                        frm.Add("ACTIVE_YN", "0");
                        ActionClass act = ActionOnDB(frm, "update", "DOCUMENT_T", null, getApplicationScheme(s));
                    }
                }
                if (docs.Count > 0 && insertlist.Count > 0)
                {
                    for (int i = 0; i < insertlist.Count; i++)
                    {
                        string FolderName = "Documents\\" + s.ID + "\\" + refid + "\\";
                        //string _path = HttpContext.Current.Server.MapPath("..//" + FolderName);
                        //string FolderName = s.ID + "\\" + refid + "\\";
                        string _path = getDocumentPath(FolderName);
                        string _FullPath = _path + Convert.ToString(insertlist[i].FILE_NAME_TX);

                        FormCollection frm = new FormCollection();
                        frm.Add("SCREEN_ID", Convert.ToString(s.ID));
                        frm.Add("REF_ID", Convert.ToString(refid));
                        frm.Add("DOC_MOD_TRANS_ID", Convert.ToString(insertlist[i].DOC_MOD_TRANS_ID));
                        frm.Add("FILE_NAME_TX", Convert.ToString(insertlist[i].FILE_NAME_TX));
                        frm.Add("FILE_BLB", _FullPath);
                        /*byte[] fl_Content = null;
                        Stream InputStream = insertlist[i].FILE_BLB;
                        using (var streamReader = new MemoryStream())
                        {
                            InputStream.CopyTo(streamReader);
                            fl_Content = streamReader.ToArray();
                        }
                        fl_Content = Compress(fl_Content);*/
                        ActionClass act = ActionOnDB(frm, "insert", "DOCUMENT_T", insertlist[i].FILE_BLB, getApplicationScheme(s));


                        #region upload image on server
                        string TblName = string.Empty;
                        if (act.DecryptData != null)
                        {
                            int Id = 1;
                            if (!string.IsNullOrEmpty(act.DecryptData))
                            {
                                JObject res = JObject.Parse(act.DecryptData);
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

                            string _FileName = insertlist[i].FILE_NAME_TX;
                            string _PathExt = Path.GetExtension(insertlist[i].FILE_NAME_TX);
                            _FileName = Id + "_" + Path.GetFileNameWithoutExtension(_FileName) + _PathExt;
                            //string _path = HttpContext.Current.Server.MapPath("..//" + FolderName);
                            _FullPath = _path + _FileName;

                            if (!(Directory.Exists(_path)))
                                Directory.CreateDirectory(_path);

                            if (File.Exists(_FullPath))
                            {
                                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                                string NewFileName = Id + "_" + Timestamp.ToString() + _PathExt;
                                File.Move(_FullPath, _path + "//" + NewFileName);
                                //File.Delete(_FullPath);
                            }
                            File.WriteAllBytes(_FullPath, insertlist[i].FILE_BLB);
                            //HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                        }
                        #endregion
                    }
                }
            }
        }

        private static object createInistance(Type objectClass, object constructorParam, bool isStatic)
        {
            try
            {
                if (!isStatic)
                {
                    // Get it's constructor
                    //ObjectHandle classInstanceHandle = Activator.CreateInstance(assemblyName, type.FullName);
                    //object classInstance = classInstanceHandle.Unwrap(); 
                    //type = classInstace.GetType(); 
                    ConstructorInfo objectClassConstructor = null;
                    if (constructorParam != null)
                        objectClassConstructor = objectClass.GetConstructor(new Type[] { constructorParam.GetType() });
                    else
                        objectClassConstructor = objectClass.GetConstructor(new Type[] { });

                    // Invoke it's constructor, which returns an instance.
                    object createdObject = null;
                    if (constructorParam != null) createdObject = objectClassConstructor.Invoke(new object[] { constructorParam });
                    else createdObject = objectClassConstructor.Invoke(null);
                    return createdObject;
                }
                else return null;
            }
            catch (Exception excep)
            {
                throw new Exception("Error instantiating class " + objectClass.Name + ". " + excep.Message);
            }
        }

        public static void SendMail(string MailTo, string Subject, string Body)
        {
            try
            {
                string host = ConfigurationManager.AppSettings["host"].ToString();
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(host);
                mail.From = new MailAddress(username);
                mail.To.Add(MailTo);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpServer.Port = port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = false;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public enum TableName
        {
            FACULTY_T = 1,
            TRAINING_DOC_T = 2,
            STUDENT_REGISTER_TRAINING_CERTIFICATE = 3,
            TRAINING_SESS_ATTANDACE_T = 4,
            PCS_REGISTRATION_DOCUMENT_T = 5,
            COMPLETION_CERTIFICATE_T = 6,
            CSR_AWARDS_SUPPORTING_DOCS_T = 7,
            CGA_AWARDS_SUPPORTING_DOCS_T = 8
        }

        public enum FilePath
        {
            [Display(Name = "UploadedFiles\\Faculty Master\\")]
            FacultyMaster = 1,
            [Display(Name = "UploadedFiles\\Training Documents\\")]
            TrainingDoc = 2,
            [Display(Name = "UploadedFiles\\Certificate Documents\\")]
            Certificate = 3,
            [Display(Name = "UploadedFiles\\Attendance\\")]
            ATTENDANCE = 4,
            [Display(Name = "UploadedFiles\\PCSCOMP Registration Documents\\")]
            PCSCOMPDOCS = 5,
            [Display(Name = "UploadedFiles\\Completion Certificates\\")]
            CompletionCertificates = 6,
            [Display(Name = "CSR\\Documents\\Support_Attachement\\")]
            CSRAwardsSupportingDocs = 7,
            [Display(Name = "CGA\\Documents\\Support_Attachement\\")]
            CGAAwardsSupportingDocs = 8
        }

        public static string GetEnumDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .Name;
        }

        public static ActionClass ActionOnDB(FormCollection frm, string actiontype, string tablename, byte[] anyfile = null, string schema = null)
        {
            ActionClass actionClass = new ActionClass();
            string Message = string.Empty;
            string userid = frm["u"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            string applicationSchema = string.Empty;
            if (string.IsNullOrEmpty(schema))
            {
                applicationSchema = getSchemaNameById(1);
            }
            else
            {
                applicationSchema = schema;
            }
            // screentype = insert / update / fetch
            try
            {
                if (actiontype != null)
                {
                    if (actiontype.Equals("fetch"))
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
                    else if (actiontype.Equals("insert") || actiontype.Equals("update"))
                    {
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        JObject jdata = DBTable.GetData("mfetch", null, tablename, 0, 100, applicationSchema);
                        StringBuilder sb = new StringBuilder();
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

                                    #region single operation
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        if (actiontype.Equals("insert"))
                                        {
                                            if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                                && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY"
                                                && Convert.ToString(dt.Rows[i][0]) != "STATUS_YN")
                                            {
                                                if (dt.Rows[i][1].ToString() == "varbinary")
                                                {
                                                    tableData.Add(dt.Rows[i][0].ToString(), Encoding.UTF8.GetString(anyfile, 0, anyfile.Length));
                                                }
                                                else
                                                {
                                                    tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                                }
                                            }
                                        }
                                        else if (actiontype.Equals("update"))
                                        {
                                            if (Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                            {
                                                if (frm[dt.Rows[i][0].ToString()] != null)
                                                {
                                                    if (dt.Rows[i][1].ToString() == "varbinary")
                                                    {
                                                        tableData.Add(dt.Rows[i][0].ToString(), Encoding.UTF8.GetString(anyfile, 0, anyfile.Length));
                                                    }
                                                    else
                                                    {
                                                        tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    lstData1.Add(tableData);
                                    #endregion
                                    AppUrl = AppUrl + "/AddUpdate";
                                    lstData.Add(Util.UtilService.addSubParameter(applicationSchema, tablename, 0, 0, lstData1, conditions));
                                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", actiontype.Equals("insert") ? "insert" : "update", lstData));
                                    actionClass.columnMetadata = jdata;

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


                                        string _FileName = Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                                        string _PathExt = Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);
                                        _FileName = Id + _PathExt;
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
                                    #endregion

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

        public static string getDocumentPath(string FolderName)
        {
            //HttpContext.Current.Server.MapPath
            if (ConfigurationManager.AppSettings.AllKeys.Contains("DOCUMENT_ROOT"))
            {
                return Convert.ToString(ConfigurationManager.AppSettings["DOCUMENT_ROOT"]) + FolderName;
            }
            else return AppDomain.CurrentDomain.BaseDirectory + FolderName;
        }

        public static void storeEmailData(ActionClass actionClass, Screen_T screen, string screenType, string AppUrl, string Session_Key, string UserName, int UniqueId)
        {
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null)
            {
                string applicationSchema = getApplicationScheme(screen);
                string TableName = "EMAIL_TEMPLATE_T";
                DataTable dt = new DataTable();
                int APPROVE_NM = 0;
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                conditions.Add("SCREEN_ID", screen.ID);
                conditions.Add("ACTIVE_YN", true);
                JObject jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, applicationSchema);
                if (jdata != null && jdata.HasValues)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == TableName)
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            if (!string.IsNullOrEmpty(actionClass.DecryptData))
                            {
                                JObject res = JObject.Parse(actionClass.DecryptData);
                                foreach (JProperty jproperty in res.Properties())
                                {
                                    if (jproperty.Name != null)
                                    {
                                        DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                                        if (dtdata.Columns.Contains("APPROVE_NM"))
                                            APPROVE_NM = Convert.ToInt32(dtdata.Rows[0]["APPROVE_NM"]);
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                        bool INSERT_YN = Convert.ToBoolean(dt.Rows[i]["INSERT_YN"]);
                        bool UPDATE_YN = Convert.ToBoolean(dt.Rows[i]["UPDATE_YN"]);
                        int STATUS_TYPE_NM = Convert.ToInt32(dt.Rows[i]["STATUS_TYPE_NM"]);
                        int query_ID = Convert.ToInt32(dt.Rows[i]["QUERY_ID"]);
                        string EmailSubject = Convert.ToString(dt.Rows[i]["EMAIL_SUBJECT_TX"]);
                        string EmailBody = Convert.ToString(dt.Rows[i]["EMAIL_BODY_TX"]);
                        string Email_Fields_Tx = Convert.ToString(dt.Rows[i]["EMAIL_FIELDS_TX"]);

                        Dictionary<string, object> emailData = new Dictionary<string, object>();

                        if ((screenType.Equals("insert") && INSERT_YN) || (screenType.Equals("update") && UPDATE_YN))
                        {
                            if (Convert.ToInt32(ApprovalStatus.Approve) == STATUS_TYPE_NM && APPROVE_NM == 1)
                                emailData = ManipulateEmailData(screen, query_ID, EmailSubject, EmailBody, Email_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Reject) == STATUS_TYPE_NM && APPROVE_NM == 3)
                                emailData = ManipulateEmailData(screen, query_ID, EmailSubject, EmailBody, Email_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.CallFor) == STATUS_TYPE_NM && APPROVE_NM == 2)
                                emailData = ManipulateEmailData(screen, query_ID, EmailSubject, EmailBody, Email_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Forward_And_Recommend) == STATUS_TYPE_NM && APPROVE_NM == 4)
                                emailData = ManipulateEmailData(screen, query_ID, EmailSubject, EmailBody, Email_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Other) == STATUS_TYPE_NM)
                                emailData = ManipulateEmailData(screen, query_ID, EmailSubject, EmailBody, Email_Fields_Tx, UniqueId);

                            if (emailData.Count > 0)
                            {
                                emailData.Add("EMAIL_TEMPLATE_ID", ID);
                                emailData.Add("STATUS_YN", 0);
                                lstAddData.Add(emailData);
                            }
                        }
                    }

                    #region Insert in DB
                    //AppUrl = AppUrl + "/AddUpdate";
                    lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "EMAIL_TRANS_T", 0, 0, lstAddData, null));
                    UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    #endregion
                }
            }
        }

        public static Dictionary<string, object> ManipulateEmailData(Screen_T screen, int query_id, string Subject, string EmailBody, string Email_Fields_Tx, int UniqueId)
        {
            string MethodName = "qfetch";
            string EmailTo = string.Empty;
            string EmailBcc = string.Empty;
            string Emailcc = string.Empty;
            Dictionary<string, object> EmailData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", query_id); // 
            conditions.Add("ID", UniqueId);

            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getApplicationScheme(screen));
            DataTable dt = null;
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["EMAIL_TO_TX"])))
                {
                    string[] splitEmailField = Email_Fields_Tx.Split(',');
                    if (!string.IsNullOrEmpty(Email_Fields_Tx))
                    {
                        for (int i = 0; i < splitEmailField.Length; i++)
                        {
                            string ColName = splitEmailField[i].ToString();
                            string ColValue = dt.Rows[0][ColName].ToString();
                            Subject = Subject.Replace("@" + ColName + "", ColValue);
                            EmailBody = EmailBody.Replace("@" + ColName + "", ColValue);
                        }
                    }
                    EmailTo = Convert.ToString(dt.Rows[0]["EMAIL_TO_TX"]);
                    if (dt.Columns.Contains("EMAIL_BCC_TX"))
                        EmailBcc = Convert.ToString(dt.Rows[0]["EMAIL_BCC_TX"]);
                    if (dt.Columns.Contains("EMAIL_CC_TX"))
                        Emailcc = Convert.ToString(dt.Rows[0]["EMAIL_CC_TX"]);

                    EmailData.Add("EMAIL_BODY_TX", EmailBody);
                    EmailData.Add("EMAIL_SUBJECT_TX", Subject);
                    EmailData.Add("EMAIL_TO_TX", EmailTo);
                    EmailData.Add("EMAIL_BCC_TX", EmailBcc);
                    EmailData.Add("EMAIL_CC_TX", Emailcc);
                }
            }
            return EmailData;
        }

        public static void storeSMSData(ActionClass actionClass, Screen_T screen, string screenType, string AppUrl, string Session_Key, string UserName, int UniqueId)
        {
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null)
            {
                string applicationSchema = getApplicationScheme(screen);
                string TableName = "SMS_TEMPLATE_T";
                DataTable dt = new DataTable();
                int APPROVE_NM = 0;
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                List<Dictionary<string, object>> lstAddData = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                conditions.Add("SCREEN_ID", screen.ID);
                conditions.Add("ACTIVE_YN", true);
                JObject jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, applicationSchema);
                if (jdata != null && jdata.HasValues)
                {
                    foreach (JProperty property in jdata.Properties())
                    {
                        if (property.Name == TableName)
                        {
                            dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                            if (!string.IsNullOrEmpty(actionClass.DecryptData))
                            {
                                JObject res = JObject.Parse(actionClass.DecryptData);
                                foreach (JProperty jproperty in res.Properties())
                                {
                                    if (jproperty.Name != null)
                                    {
                                        DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                                        if (dtdata.Columns.Contains("APPROVE_NM"))
                                            APPROVE_NM = Convert.ToInt32(dtdata.Rows[0]["APPROVE_NM"]);
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                        bool INSERT_YN = Convert.ToBoolean(dt.Rows[i]["INSERT_YN"]);
                        bool UPDATE_YN = Convert.ToBoolean(dt.Rows[i]["UPDATE_YN"]);
                        int STATUS_TYPE_NM = Convert.ToInt32(dt.Rows[i]["STATUS_TYPE_NM"]);
                        int query_ID = Convert.ToInt32(dt.Rows[i]["QUERY_ID"]);
                        string SMSTemplate = Convert.ToString(dt.Rows[i]["TEMPLATE_TX"]);
                        string SMS_Fields_Tx = Convert.ToString(dt.Rows[i]["SMS_FIELDS_TX"]);

                        Dictionary<string, object> SMSData = new Dictionary<string, object>();

                        if ((screenType.Equals("insert") && INSERT_YN) || (screenType.Equals("update") && UPDATE_YN))
                        {
                            if (Convert.ToInt32(ApprovalStatus.Approve) == STATUS_TYPE_NM && APPROVE_NM == 1)
                                SMSData = ManipulateSMSData(screen, query_ID, SMSTemplate, SMS_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Reject) == STATUS_TYPE_NM && APPROVE_NM == 3)
                                SMSData = ManipulateSMSData(screen, query_ID, SMSTemplate, SMS_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.CallFor) == STATUS_TYPE_NM && APPROVE_NM == 2)
                                SMSData = ManipulateSMSData(screen, query_ID, SMSTemplate, SMS_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Forward_And_Recommend) == STATUS_TYPE_NM && APPROVE_NM == 4)
                                SMSData = ManipulateSMSData(screen, query_ID, SMSTemplate, SMS_Fields_Tx, UniqueId);
                            else if (Convert.ToInt32(ApprovalStatus.Other) == STATUS_TYPE_NM)
                                SMSData = ManipulateSMSData(screen, query_ID, SMSTemplate, SMS_Fields_Tx, UniqueId);

                            if (SMSData.Count > 0)
                            {
                                SMSData.Add("SMS_TEMPLATE_ID", ID);
                                SMSData.Add("STATUS_YN", 0);
                                lstAddData.Add(SMSData);
                            }
                        }
                    }

                    #region Insert in DB
                    //AppUrl = AppUrl + "/AddUpdate";
                    lstData.Add(Util.UtilService.addSubParameter(applicationSchema, "SMS_TRANS_T", 0, 0, lstAddData, null));
                    UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                    #endregion
                }
            }
        }

        public static Dictionary<string, object> ManipulateSMSData(Screen_T screen, int query_id, string SMSBody, string SMS_Fields_Tx, int UniqueId)
        {
            string MethodName = "qfetch";
            string MobileNo = string.Empty;
            Dictionary<string, object> SMSData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", query_id); // for fetching the drop down data
            conditions.Add("ID", UniqueId);

            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, getApplicationScheme(screen));
            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string[] splitSMSField = SMS_Fields_Tx.Split(',');
                if (!string.IsNullOrEmpty(SMS_Fields_Tx))
                {
                    for (int i = 0; i < splitSMSField.Length; i++)
                    {
                        string ColName = splitSMSField[i].ToString();
                        string ColValue = dt.Rows[0][ColName].ToString();
                        SMSBody = SMSBody.Replace("@" + ColName + "", ColValue);
                    }
                }
                MobileNo = Convert.ToString(dt.Rows[0]["SMS_MOBILE_NO_TX"]);
                SMSData.Add("SMS_BODY_TX", SMSBody);
                SMSData.Add("SMS_MOB_NO_TX", MobileNo);
            }
            return SMSData;
        }


        public enum ApprovalStatus
        {
            Approve = 1,
            CallFor = 2,
            Reject = 3,
            Forward_And_Recommend = 4,
            Other = 5
        }

        public static string RandomNumber()
        {
            Random random = new Random();
            return random.Next(1, 1000000000).ToString("D10");
        }

        public static DataTable GetExaminationDetails(int STUDENT_HDR_ID)
        {
            string MethodName = "qfetch";
            Dictionary<string, object> EmailData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 24);
            conditions.Add("ID", STUDENT_HDR_ID);

            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, "ICSI");
            DataTable dtexm = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dtexm = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }

            return dtexm;
        }

        public static string GenerateRandomComplaintNo()
        {
            Random random = new Random();
            return "CSJ" + random.Next(1, 100000).ToString("D10");

        }

        public static int GetCurrentCourse(int STUDENT_HDR_ID)
        {
            int currCourse = 0;
            if (STUDENT_HDR_ID == 0)
                STUDENT_HDR_ID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_HDR_ID"]);
            DataTable dtCurrCourse = new DataTable();
            dtCurrCourse = GetExaminationDetails(STUDENT_HDR_ID);
            if (dtCurrCourse.Rows.Count > 0)
                currCourse = Convert.ToInt32(dtCurrCourse.Rows[0]["PASSEDCOURSE"]);

            return currCourse;
        }

        public static void SendEmailAndSMS(string studentID, int screenID)
        {
            //string studentEmail = string.Empty;
            //string studentMobile = string.Empty;
            //string subject = string.Empty;
            //string emailBody = string.Empty;
            //Dictionary<string, object> conditions = new Dictionary<string, object>();
            //DataTable dt = new DataTable();
            //conditions.Add("ID", studentID);
            //conditions.Add("ACTIVE_YN", 1);
            //dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    studentEmail = dt.Rows[0]["EMAIL_ID"].ToString();
            //    studentMobile = dt.Rows[0]["MOBILE_TX"].ToString();

            //}

            //switch (screenID)
            //{

            //    case 65:
            //        subject = "New Training Registraion";
            //        emailBody = "Dear STUDENT,Your training documents for registration of training have been uploaded successfully.We will reply you soon.Thanking You Directorate of training,ICSI";
            //        break;
            //    case 68:
            //        subject = "Balance Training Registration";
            //        emailBody = "Dear STUDENT,Your training documents for registration of training have been uploaded successfully.We will reply you soon.Thanking You Directorate of training,ICSI";
            //        break;
            //    case 83:
            //        subject = "Apply Exemption";
            //        emailBody = "Dear STUDENT,Your application for claiming exemption has been successfully submitted.We will reply you soon.";
            //        break;
            //    case 159:
            //        subject = "Switch Over from Earlier to Modified Training";
            //        emailBody = "Dear STUDENT,Your training documents for registration of training have been uploaded successfully.We will reply you soon.Thanking You Directorate of training,ICSI";
            //        break;
            //    case 46:
            //        subject = "Approve Student Training";
            //        emailBody = "Dear STUDENT, Your application has been approved. Now you can generate letter at your end from your account. ";
            //        break;
            //    case 72:
            //        subject = "Approve Long Term Training Inner Screen";
            //        emailBody = "Dear STUDENT, Your application has been approved. Now you can generate letter at your end from your account. ";
            //        break;
            //    case 63:
            //        subject = "Approve Long Training Exemption Request";
            //        emailBody = "Dear STUDENT, Your application has been approved. Now you can generate letter at your end from your account. ";
            //        break;
            //    case 37:
            //        subject = "Payment Successfull";
            //        emailBody = "Dear STUDENT, Your application has been approved. Now you can generate letter at your end from your account. ";
            //        break;
            //    case 97:
            //        subject = "Payment Successfull";
            //        emailBody = "Dear STUDENT, Your payment has been Successfull.We will reply you soon.";
            //        break;
            //    case 183:
            //        subject = "Payment Successfull";
            //        emailBody = "Dear STUDENT, Your payment has been Successfull.We will reply you soon.";
            //        break;
            //    default:
            //        subject = "";
            //        emailBody = "";
            //        break;
            //}


            //if (subject != string.Empty && emailBody != string.Empty)
            //{
            //    Dictionary<string, object> cond = new Dictionary<string, object>();
            //    Dictionary<string, object> data = new Dictionary<string, object>();
            //    string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            //    string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();
            //    string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            //    AppUrl = AppUrl + "/AddUpdate";
            //    ActionClass actionClass = new ActionClass();
            //    //Email
            //    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            //    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            //    data.Add("EMAIL_BODY_TX", emailBody);
            //    data.Add("EMAIL_TO_TX", studentEmail);
            //    data.Add("EMAIL_SUBJECT_TX", subject);
            //    data.Add("STATUS_YN", 0);
            //    data.Add("EMAIL_TEMPLATE_ID", 0);
            //    lstData1.Add(data);
            //    lstData.Add(Util.UtilService.addSubParameter("Training", "EMAIL_TRANS_T", 0, 0, lstData1, cond));

            //    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
            //    //SMS
            //    data.Clear();
            //    lstData.Clear();
            //    lstData1.Clear();
            //    data.Add("SMS_BODY_TX", emailBody);
            //    data.Add("SMS_MOB_NO_TX", studentMobile);
            //    data.Add("STATUS_YN", 0);
            //    data.Add("SMS_TEMPLATE_ID", 0);
            //    lstData1.Add(data);
            //    lstData.Add(Util.UtilService.addSubParameter("Training", "SMS_TRANS_T", 0, 0, lstData1, cond));
            //    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));


            //}
        }

        public static ActionClass insertOrUpdate(string applicationSchema, string tableName, List<Dictionary<string, object>> lstData1)
        {
            ActionClass actionClass = null;
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            JObject jdata = DBTable.GetData("mfetch", null, tableName, 0, 100, applicationSchema);
            AppUrl = AppUrl + "/AddUpdate";
            lstData.Add(Util.UtilService.addSubParameter(applicationSchema, tableName, 0, 0, lstData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", !lstData1[0].ContainsKey("ID") ? "insert" : "update", lstData));
            actionClass.columnMetadata = jdata;
            return actionClass;
        }

        public static void updatePGTransaction(int trid)
        {
            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Add("ACTIVE_YN", 1);
            conds.Add("ID", trid);
            DataTable transtatusdt = Util.UtilService.getData("Stimulate", "PG_TRANSACTION_T", conds, null, 0, 1);
            if (transtatusdt != null && transtatusdt.Rows != null && transtatusdt.Rows.Count > 0)
            {
                if (Convert.ToInt32(transtatusdt.Rows[0]["PAYMENT_STATUS_ID"]) == 2 && Convert.ToInt32(transtatusdt.Rows[0]["SCREEN_ID"]) == 37)
                {
                    conds.Clear();
                    conds.Add("ACTIVE_YN", 1);
                    conds.Add("ID", Convert.ToInt32(transtatusdt.Rows[0]["REF_ID"]));
                    DataTable masterTbl = Util.UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_T", conds, null, 0, 1);

                    if (masterTbl != null && masterTbl.Rows != null && masterTbl.Rows.Count > 0)
                    {
                        masterTbl.Rows[0]["PAYMENT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                        masterTbl.Rows[0]["PAYMENT_STATUS_ID"] = transtatusdt.Rows[0]["PAYMENT_STATUS_ID"];

                        List<Dictionary<string, object>> list =
                            masterTbl.AsEnumerable().Select(
                            row => masterTbl.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName as string,    // Key
                                column => row[column] // Value
                            )
                        ).ToList();
                        insertOrUpdate("Training", "STUDENT_REGISTER_TRAINING_T", list);
                        conds.Clear();
                        conds.Add("ACTIVE_YN", 1);
                        conds.Add("REC_NO", Convert.ToInt32(transtatusdt.Rows[0]["RECEIPT_FK_ID"]));
                        conds.Add("STUDENT_REGISTER_TRAINING_ID", Convert.ToInt32(transtatusdt.Rows[0]["REF_ID"]));
                        conds.Add("TXN_ID", transtatusdt.Rows[0]["TRANSACTION_ID"]);
                        DataTable regTbl = Util.UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_FEE_DETAIL", conds, null, 0, 1);
                        List<Dictionary<string, object>> regList = null;
                        if (regTbl != null && regTbl.Rows != null && regTbl.Rows.Count > 0)
                        {
                            regList = regTbl.AsEnumerable().Select(
                                row => regTbl.Columns.Cast<DataColumn>().ToDictionary(
                                        column => column.ColumnName as string,    // Key
                                        column => row[column] // Value
                                    )
                                ).ToList();
                            Dictionary<string, object> d = regList[0];
                            d["AMOUNT_NM"] = transtatusdt.Rows[0]["AMOUNT"];
                            d["PAYMENT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                            d["ACTIVE_YN"] = 1;
                            d["CREATED_DT"] = DateTime.Now;
                            d["CREATED_BY"] = 1;
                            d["UPDATED_DT"] = DateTime.Now;
                            d["UPDATED_BY"] = 1;
                        }
                        else
                        {
                            regList = new List<Dictionary<string, object>>();
                            Dictionary<string, object> d = new Dictionary<string, object>();
                            d["REC_NO"] = Convert.ToInt32(transtatusdt.Rows[0]["RECEIPT_FK_ID"]);
                            d["STUDENT_REGISTER_TRAINING_ID"] = Convert.ToInt32(transtatusdt.Rows[0]["REF_ID"]);
                            d["TXN_ID"] = transtatusdt.Rows[0]["TRANSACTION_ID"];
                            d["AMOUNT_NM"] = transtatusdt.Rows[0]["AMOUNT"];
                            d["PAYMENT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                            d["ACTIVE_YN"] = 1;
                            d["CREATED_DT"] = DateTime.Now;
                            d["CREATED_BY"] = 1;
                            d["UPDATED_DT"] = DateTime.Now;
                            d["UPDATED_BY"] = 1;
                            regList.Add(d);
                        }
                        insertOrUpdate("Training", "STUDENT_REGISTER_TRAINING_FEE_DETAIL", regList);
                    }
                } // end of screen id 37
                else if ((Convert.ToInt32(transtatusdt.Rows[0]["PAYMENT_STATUS_ID"]) == 2 || Convert.ToInt32(transtatusdt.Rows[0]["PAYMENT_STATUS_ID"]) == 6) && Convert.ToInt32(transtatusdt.Rows[0]["SCREEN_ID"]) == 97)
                {
                    conds.Clear();
                    conds.Add("ACTIVE_YN", 1);
                    conds.Add("ID", Convert.ToInt32(transtatusdt.Rows[0]["REF_ID"]));
                    DataTable masterTbl = Util.UtilService.getData("Training", "STUDENT_REGISTER_TRAINING_LONGTERM_T", conds, null, 0, 1);

                    if (masterTbl != null && masterTbl.Rows != null && masterTbl.Rows.Count > 0)
                    {
                        List<Dictionary<string, object>> regList =
                            masterTbl.AsEnumerable().Select(
                            row => masterTbl.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName as string,    // Key
                                column => row[column] // Value
                            )
                        ).ToList();
                        Dictionary<string, object> d = regList[0];
                        d["PAYMENT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                        d["PAYMENT_STATUS_ID"] = transtatusdt.Rows[0]["PAYMENT_STATUS_ID"];
                        d["TXN_ID"] = transtatusdt.Rows[0]["TRANSACTION_ID"];
                        d["RECEIPT_NM"] = transtatusdt.Rows[0]["RECEIPT_FK_ID"];
                        insertOrUpdate("Training", "STUDENT_REGISTER_TRAINING_LONGTERM_T", regList);
                    }
                } // end of screen 97
                else if ((Convert.ToInt32(transtatusdt.Rows[0]["PAYMENT_STATUS_ID"]) == 2 || Convert.ToInt32(transtatusdt.Rows[0]["PAYMENT_STATUS_ID"]) == 6) && Convert.ToInt32(transtatusdt.Rows[0]["SCREEN_ID"]) == 183)
                {
                    conds.Clear();
                    conds.Add("ACTIVE_YN", 1);
                    conds.Add("ID", Convert.ToInt32(transtatusdt.Rows[0]["REF_ID"]));
                    DataTable masterTbl = Util.UtilService.getData("Training", "STUDENT_TRAINING_EXEMPTION_T", conds, null, 0, 1);

                    if (masterTbl != null && masterTbl.Rows != null && masterTbl.Rows.Count > 0)
                    {
                        List<Dictionary<string, object>> regList =
                            masterTbl.AsEnumerable().Select(
                            row => masterTbl.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName as string,    // Key
                                column => row[column] // Value
                            )
                        ).ToList();
                        Dictionary<string, object> d = regList[0];
                        d["PAYMENT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                        d["PAYMENT_STATUS_ID"] = transtatusdt.Rows[0]["PAYMENT_STATUS_ID"];
                        d["TRANSACTION_ID"] = transtatusdt.Rows[0]["TRANSACTION_ID"];
                        d["RECEIPT_DT"] = transtatusdt.Rows[0]["PAYMENT_INITIATION_DATE_TX"];
                        d["REC_NO_TX"] = transtatusdt.Rows[0]["RECEIPT_FK_ID"];
                        insertOrUpdate("Training", "STUDENT_TRAINING_EXEMPTION_T", regList);
                    }
                } // end of screen 183
            }
        }


        /// <summary>
        /// Build reconciliation table in rec reports screen
        /// </summary>
        /// <param name="dtFilterData"></param>
        /// <param name="strPanelName"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string BuildReconTable(DataTable dtFilterData, string strPanelName, int count)
        {
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<div class='panelSpacing'><div class='panel panel-primary searchResults'><div class='panel-body'>")
            .Append("<h4 class='text-on-pannel'><span class='fontBold'>").Append(strPanelName).Append("</span></h4>")
            .Append("<div style='overflow-x:auto;'><strong>").Append("Count: ").Append(count).Append("</strong>")
            .Append("<table class='table table-bordered tableSearchResults'><thead><tr class='bg'>")
            .Append("<th class='searchResultsHeading'>Chapter</th>")
            .Append("<th class='searchResultsHeading'>Transaction ID</th>")
            .Append("<th class='searchResultsHeading'>Receipt Number</th>")
            .Append("<th class='searchResultsHeading'>Reference Number</th>")
            .Append("<th class='searchResultsHeading'>Amount</th>")
            .Append("<th class='searchResultsHeading'>Payment Date</th>")
            .Append("<th class='searchResultsHeading'>Reconciliation Date</th>")
            .Append("<th class='searchResultsHeading'>Customer Name</th>")
            .Append("<th class='searchResultsHeading'>PG Status</th>")
            .Append("<th class='searchResultsHeading'>Is Reconcile Done</th>")
            .Append("<th class='searchResultsHeading'>Status</th></tr></thead><tbody>");

            if (dtFilterData != null && dtFilterData.Rows != null && dtFilterData.Rows.Count > 0)
            {
                foreach (DataRow row in dtFilterData.Rows)
                {
                    sbHtml.Append("<tr>")
                        .Append("<td>").Append(Convert.ToString(row["OFFICE_NAME_TX"])).Append("</td>")
                        .Append("<td>").Append(Convert.ToString(row["REFERENCE_NO_TX"])).Append("</td>");
                    if (row["RECEIPT_FK_ID"] != null) sbHtml.Append("<td>").Append(Convert.ToString(row["RECEIPT_FK_ID"])).Append("</td>");
                    else sbHtml.Append("<td></td>");
                    sbHtml.Append("<td>").Append(Convert.ToString(row["TRANID_TX"])).Append("</td>");
                    if (row["PGAMOUNT_TX"] != null) sbHtml.Append("<td>").Append(Convert.ToString(row["PGAMOUNT_TX"])).Append("</td>");
                    else sbHtml.Append("<td></td>");
                    if (row["TRANDATE_TX"] != null) sbHtml.Append("<td>").Append(Convert.ToString(row["TRANDATE_TX"])).Append("</td>");
                    else sbHtml.Append("<td></td>");
                    if (row["RECONCILLATION_DATE_DT"] != null)
                    {
                        sbHtml.Append("<td>").Append(Convert.ToString(row["RECONCILLATION_DATE_DT"])).Append("</td>");
                    }
                    else sbHtml.Append("<td></td>");
                    sbHtml.Append("<td>").Append(Convert.ToString(row["PAYER_NAME_TX"])).Append("</td>")
                        .Append("<td>").Append(Convert.ToString(row["STATUS_TX"])).Append("</td>");
                    if (Convert.ToInt32(row["IS_RECONCILE"]) == 1)
                    {
                        sbHtml.Append("<td>Yes</td>");
                    }
                    else
                    {
                        sbHtml.Append("<td>No</td>");
                    }
                    if (Convert.ToString(row["TYPE_TRANS"]).Equals("F"))
                    {
                        sbHtml.Append("<td>").Append("Un Match").Append("</td>");
                    }
                    else if (Convert.ToString(row["TYPE_TRANS"]).Equals("S"))
                    {
                        sbHtml.Append("<td>").Append("Match").Append("</td>");
                    }
                    else if (Convert.ToString(row["TYPE_TRANS"]).Equals("N"))
                    {
                        sbHtml.Append("<td>").Append("Un Known").Append("</td>");
                    }
                    else if (Convert.ToString(row["TYPE_TRANS"]).Equals("CF"))
                    {
                        sbHtml.Append("<td>").Append("Recon. Processed").Append("</td>");
                    }
                    else if (Convert.ToString(row["TYPE_TRANS"]).Equals("Z") && Convert.ToInt32(row["IS_RECONCILE"]) == 1)
                    {
                        sbHtml.Append("<td>").Append("Recon. already processed").Append("</td>");
                    }
                    else
                    {
                        sbHtml.Append("<td></td>");
                    }
                    sbHtml.Append("</tr>");

                }

            }
            else
            {
                sbHtml.Append("<tr>No records are present</tr>");
            }
            sbHtml.Append("</tbody></table></div></div></div></div></div>");

            return sbHtml.ToString();
        }

        /// <summary>
        /// Loads the report
        /// </summary>
        /// <param name="WEB_APP_ID"></param>
        /// <param name="frm"></param>
        /// <param name="scrtype"></param>
        /// <param name="scmptid"></param>
        /// <param name="rptComp"></param>
        /// <param name="screenId"></param>
        /// <returns></returns>
        public static JObject LoadReport(int WEB_APP_ID, FormCollection frm, string scrtype, string scmptid, Report_Comp_T rptComp, string screenId = "")
        {
            JObject jData = new JObject();
            string Message = string.Empty;
            string userid = frm["u"];
            string menuid = frm["m"];
            screenId = frm["si"];
            //string screenType = frm["s"] == null ? scrtype : frm["s"] == "new" ? scrtype : frm["s"];
            string screenType = scrtype;
            if (!string.IsNullOrEmpty(scrtype) && (scrtype == "newwithdata" || scrtype == "newwithpayment"))
            {
                screenType = scrtype;
            }
            string uniqueId = frm["ui"] == null ? "" : frm["ui"];
            string scrrmpid = frm["scmpid"] == null ? scmptid : frm["scmpid"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            try
            {
                if (screenType != null)
                {
                    if (screenType.Equals("report"))
                    {
                        int st = 0;
                        int ed = 0;
                        if (frm["st"] != null) st = Convert.ToInt32(frm["st"]);
                        if (frm["ed"] != null) ed = Convert.ToInt32(frm["ed"]);
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        foreach (var key in frm.AllKeys)
                        {
                            if (key.StartsWith("SCRH_") || key.StartsWith("COND_") || key.StartsWith("REPLACE_"))
                            {
                                //if (!string.IsNullOrEmpty(Convert.ToString(frm[key])) && Convert.ToString(frm[key]) != "0")
                                if (!string.IsNullOrEmpty(Convert.ToString(frm[key])))
                                {
                                    conditions.Add(key, Convert.ToString(frm[key]));
                                }
                            }
                        }
                        conditions.Add("SCMPID", rptComp.Id);

                        Screen_T screen = screenObject(WEB_APP_ID, frm);
                        if (rptComp != null && rptComp.dynWhere != null && !rptComp.dynWhere.Trim().Equals(""))
                        {

                            string[] cons = rptComp.dynWhere.Split(';');
                            foreach (string con in cons)
                            {
                                string[] convals = con.Split('=');
                                if (!conditions.ContainsKey(convals[0].Trim()))
                                {
                                    if (convals[1].Trim().StartsWith("@") && Convert.ToString(convals[1].Trim().Substring(1)) != "0")
                                    {
                                        string strKeyCond = string.Empty;
                                        if (conditions.ContainsKey(convals[1].Trim().Substring(1)))
                                        {
                                            conditions.Remove(convals[1].Trim().Substring(1));
                                        }
                                        if (convals[0].Trim().Contains("MONTH"))
                                        {
                                            strKeyCond = "MONTH(" + convals[0].Trim().Replace("MONTH_", "") + ")";
                                        }
                                        else if (convals[0].Trim().Contains("DAILY"))
                                        {
                                            strKeyCond = "CONVERT(DATE," + convals[0].Trim().Replace("DAILY_", "") + ",101)";
                                        }
                                        else
                                        {
                                            strKeyCond = convals[0].Trim();
                                        }
                                        if (frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                                        {
                                            string val = frm[convals[1].Trim().Substring(1)];
                                            if (strKeyCond.Equals("MONTH(" + convals[0].Trim().Replace("MONTH_", "") + ")"))
                                            {
                                                string[] vals = val.Split(',');
                                                string strMonthval = string.Empty;
                                                foreach (string strDate in vals)
                                                {
                                                    int month = DateTime.ParseExact(strDate, "MM/dd/yyyy", null).Month;
                                                    if (string.IsNullOrEmpty(strMonthval)) strMonthval = strMonthval + month;
                                                    else strMonthval = strMonthval + "," + month;
                                                }
                                                conditions.Add("SCRH_" + strKeyCond, strMonthval);
                                            }
                                            else
                                            {
                                                string dateVal = string.Empty;
                                                string[] vals = val.Split(',');
                                                DateTime date;
                                                if (vals.Length > 1)
                                                {
                                                    foreach (string strFrmVal in vals)
                                                    {
                                                        if (DateTime.TryParseExact(strFrmVal, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                                                        {
                                                            if (string.IsNullOrEmpty(dateVal)) dateVal = dateVal + date.ToString("yyyy-MM-dd");
                                                            else dateVal = dateVal + "," + date.ToString("yyyy-MM-dd"); ;
                                                        }
                                                    }
                                                    conditions.Add("SCRH_" + strKeyCond, dateVal);
                                                }
                                                else
                                                {
                                                    conditions.Add("SCRH_" + strKeyCond, frm[convals[1].Trim().Substring(1)]);
                                                }

                                            }
                                        }
                                        else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                                        {
                                            string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                            bool proceed = false;
                                            try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                            if (proceed) conditions.Add("SCRH_" + strKeyCond, val);
                                        }
                                        else if (convals[0].Trim().Contains("MONTH"))
                                        {
                                            conditions.Add("SCRH_" + strKeyCond, DateTime.Now.Month);
                                        }
                                        else if (convals[0].Trim().Contains("DAILY"))
                                        {
                                            conditions.Add("SCRH_" + strKeyCond, DateTime.Now.ToString("yyyy-MM-dd"));
                                        }
                                    }
                                    else conditions.Add("SCRH_" + convals[0].Trim(), convals[1].Trim());
                                }
                            }
                        }

                        if (screen != null && userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            string applicationSchema = getApplicationScheme(screen);
                            jData = DBTable.GetData("report", conditions, screen.Table_Name_Tx, 0, 100, applicationSchema);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return jData;
        }
        /// <summary>
        /// get all components related to report
        /// </summary>
        /// <param name="row"></param>
        /// <param name="webAppSchema"></param>
        /// <param name="appSchema"></param>
        /// <param name="moduleSchema"></param>
        /// <param name="screenSchema"></param>
        /// <returns></returns>
        public static Report_Comp_T getReportComponent(DataRow row, string webAppSchema, string appSchema, string moduleSchema, string screenSchema)
        {
            Report_Comp_T rptComp = new Report_Comp_T();
            rptComp.Id = Convert.ToInt32(row["ID"]);
            rptComp.RepType = Convert.ToString(row["COMP_TYPE"]);
            rptComp.Report_Id = Convert.ToInt32(row["REPORT_ID"]);
            rptComp.Ref_Id = Convert.ToInt32(row["REF_ID"]);
            rptComp.Order_Nm = Convert.ToInt32(row["ORDER_NM"]);
            rptComp.Comp_Type_Nm = Convert.ToInt32(row["REP_COMP_TYPE_NM"]);
            rptComp.ScreenSchemaNameTx = screenSchema;
            rptComp.ModuleSchemaNameTx = moduleSchema;
            rptComp.ApplicationSchemaNameTx = appSchema;
            rptComp.WebAppSchemaNameTx = webAppSchema;
            if (!string.IsNullOrEmpty(Convert.ToString(row["SCHEMA_NAME_TX"]))) rptComp.schemaNameTx = Convert.ToString(row["SCHEMA_NAME_TX"]); else rptComp.schemaNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["DYN_WHERE_TX"]))) rptComp.dynWhere = Convert.ToString(row["DYN_WHERE_TX"]); else rptComp.dynWhere = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_NAME_TX"]))) rptComp.compNameTx = Convert.ToString(row["REP_COMP_NAME_TX"]); else rptComp.compNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_CONTENT_TX"]))) rptComp.compContentTx = Convert.ToString(row["REP_COMP_CONTENT_TX"]); else rptComp.compContentTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_VALUE_TX"]))) rptComp.compValueTx = Convert.ToString(row["REP_COMP_VALUE_TX"]); else rptComp.compValueTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_TEXT_TX"]))) rptComp.compTextTx = Convert.ToString(row["REP_COMP_TEXT_TX"]); else rptComp.compTextTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["SCREEN_REF_METHOD_NAME_TX"]))) rptComp.screenReferenceMethodNameTx = Convert.ToString(row["SCREEN_REF_METHOD_NAME_TX"]); else rptComp.screenReferenceMethodNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_CLASS_NAME_TX"]))) rptComp.screenCompClassNameTx = Convert.ToString(row["REP_COMP_CLASS_NAME_TX"]); else rptComp.screenCompClassNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["SQL_TX"]))) rptComp.sql = Convert.ToString(row["SQL_TX"]); else rptComp.sql = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["WHERE_TX"]))) rptComp.where = Convert.ToString(row["WHERE_TX"]); else rptComp.where = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_METHOD_NAME_TX"]))) rptComp.screenCompMethodNameTx = Convert.ToString(row["REP_COMP_METHOD_NAME_TX"]); else rptComp.screenCompMethodNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["TABLE_NAME_TX"]))) rptComp.tableNameTx = Convert.ToString(row["TABLE_NAME_TX"]); else rptComp.tableNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["COLUMN_NAME_TX"]))) rptComp.columnNameTx = Convert.ToString(row["COLUMN_NAME_TX"]); else rptComp.columnNameTx = null;
            if (!string.IsNullOrEmpty(Convert.ToString(row["MANDATORY_YN"]))) rptComp.isMandatoryYn = Convert.ToBoolean(row["MANDATORY_YN"]); else rptComp.isMandatoryYn = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_CLASS_STATIC_YN"]))) rptComp.isScreenCompClassStatic = Convert.ToBoolean(row["REP_COMP_CLASS_STATIC_YN"]); else rptComp.isScreenCompClassStatic = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["READ_WRITE_YN"]))) rptComp.isReadWriteYn = Convert.ToBoolean(row["READ_WRITE_YN"]); else rptComp.isReadWriteYn = false;
            if (!string.IsNullOrEmpty(Convert.ToString(row["REP_COMP_METHOD_STATIC_YN"]))) rptComp.isScreenCompMethodStatic = Convert.ToBoolean(row["REP_COMP_METHOD_STATIC_YN"]); else rptComp.isScreenCompMethodStatic = false;
            //DataTable resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("REF_ID") == screen_Comp_T.Id && x.Field<long>("ID") != screen_Comp_T.Id).OrderBy(x => x.Field<long>("ORDER_NM")).CopyToDataTable();
            //var resScreen_Comp = DBTable.SCREEN_COMP_T.AsEnumerable().Where(x => x.Field<long>("REF_ID") == rptComp.Id && x.Field<long>("ID") != rptComp.Id).OrderBy(x => x.Field<long>("ORDER_NM")).ToList();
            //List<Report_Comp_T> rptComps = new List<Report_Comp_T>(); ;
            //if (resScreen_Comp != null && resScreen_Comp.Count > 0)
            //{
            //    DataTable dtScreen_Comp = resScreen_Comp.CopyToDataTable();
            //    foreach (DataRow row1 in dtScreen_Comp.Rows)
            //    {
            //        rptComps.Add(getReportComponent(row1, webAppSchema, appSchema, moduleSchema, screenSchema));
            //    }
            //}
            rptComp.ScreenCompList = null;
            return rptComp;
        }


        /// <summary>
        /// Render report components
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="lstRptComps"></param>
        /// <param name="sb"></param>
        /// <param name="frm"></param>
        private static void renderReportComponent(Screen_T screen, List<Report_Comp_T> lstRptComps, StringBuilder sb, FormCollection frm)
        {
            //throw new NotImplementedException();
            int counter = 0;
            foreach (Report_Comp_T rptComp in lstRptComps)
            {
                if (rptComp.RepType.Equals("P"))
                {
                    if (counter == 0)
                    {
                        sb.Append("<input type='hidden' id='altsi' value='").Append(screen.ID).Append("'>");
                        sb.Append("<div class=\"panelSpacing\" id=\"ReportParamsId\"><div class='panel panel-primary searchCtr'><div class='panel-body'>")
                          .Append("<h4 class='text-on-pannel'><span class='fontBold'>Report Parameters</span></h4>");
                        if (frm != null)
                        {
                            string uniqueId = frm["ui"];
                            if (!string.IsNullOrEmpty(uniqueId))
                                sb.Append("<input type='hidden' value='" + uniqueId + "' name='hidUI' id='hidUI' />");
                        }
                        counter++;
                        sb.Append("<div class='col-md-12 text-right fontBold'>Report Run Time : ")
                           .Append(DateTime.Now.ToShortDateString()).Append(" ").Append(DateTime.Now.ToShortTimeString()).Append("</div>");
                        sb.Append("<div class='row'>");
                    }

                    if (rptComp.Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY || rptComp.Comp_Type_Nm == (Int32)HTMLTag.LIST)
                    {
                        sb.Append("<input type='hidden' name='scmpid' value='").Append(rptComp.Id).Append("'>");
                        sb.Append("<div class='col-md-6' style='margin - top: 7px;'><div class='form-inline'><label class='col-md-5 control-label TrainingSchdlLabelTxt FormInputsLabel FacltyNamTxt FacltyNamTxt'>")
                          .Append(rptComp.compTextTx).Append("</label><div class='col-md-7'><div class='input-group col-md-7 col-xs-11 col-xs-11 IpadInput mobilInput3 panelInputsSub")
                          .Append(" selectoptionBg")
                          .Append(" '><input type='hidden' name='COND_").Append(rptComp.compNameTx).Append("' value='").Append(rptComp.where).Append("' >");
                        rptComp.compNameTx = "SCRH_" + rptComp.compNameTx;
                        buildRptDropDown(rptComp, sb, frm);
                        sb.Append("</div></div></div></div>");

                        //sb.Append("</div><div class=''><div class='form-group'><button type='button' onclick=\"searchPage(").Append(screen.ID).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Search</button></div></div></div></div></div>");
                        //sb.Append("<div id='divSearchResult' style='display: none'></div>");

                    }
                    else if (rptComp.Comp_Type_Nm == (Int32)HTMLTag.CUSTOM)
                    {
                        sb.Append("<input type='hidden' name='rept_id' value='").Append(rptComp.reportId).Append("'>");
                        sb.Append("<input type='hidden' name='scmpid' value='").Append(rptComp.Id).Append("'>");
                        sb.Append("<div class='col-md-6' style='margin - top: 7px;'><div class='form-inline'><label class='col-md-5 control-label TrainingSchdlLabelTxt FormInputsLabel FacltyNamTxt FacltyNamTxt'>")
                          .Append(rptComp.compTextTx).Append("</label><div class='col-md-7 pt-10'><div class='input-group col-md-7 col-xs-11 col-xs-11 IpadInput mobilInput3 panelInputsSub")
                          .Append(" '><input type='hidden' name='COND_").Append(rptComp.compNameTx).Append("' value='").Append(rptComp.where).Append("' >");
                        rptComp.compNameTx = "SCRH_" + rptComp.compNameTx;
                        sb.Append(rptComp.compContentTx);
                        sb.Append("</div></div></div></div>");
                    }
                    // if (rptComp.ScreenCompList != null && rptComp.Comp_Type_Nm != (Int32)HTMLTag.SEARCH) renderReportComponent(screen, rptComp.ScreenCompList, sb, frm);

                }
            }
            sb.Append("</div><div id = 'dvButtonsGrp' class=''><div class='form-group'>")
                .Append("<button type='button' onclick=\"GenerateRepts(").Append(lstRptComps[0].Report_Id).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Generate Reports</button>")
                //.Append("<button type='button' onclick=\"DownloadReport(").Append(lstRptComps[0].Report_Id).Append(")\" class='btn btn-primary SearchCrtSubmitBtn'>Download Report</button>")
                .Append("<button type='button' id = 'btnDownload' class='btn btn-primary SearchCrtSubmitBtn'>Download Report</button>")
                .Append("</div></div>");
            sb.Append("</div></div></div><div id='divSearchResult' style='display: none'></div>");

        }


        /// <summary>
        /// Build dropdowns for reports
        /// </summary>
        /// <param name="s"></param>
        /// <param name="sb"></param>
        /// <param name="frm"></param>
        private static void buildRptDropDown(Report_Comp_T s, StringBuilder sb, FormCollection frm)
        {
            string uniqueID = string.Empty;

            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                if (uniqueID != null && uniqueID != string.Empty)
                {
                    Dictionary<string, object> cond = new Dictionary<string, object>();
                    cond.Add("UNIQUE_REG_ID", uniqueID);
                    DataTable dtb = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", cond, null, 0, 1);

                    if (dtb != null && dtb.Rows.Count > 0 && dtb.Rows[0]["ID"] != null)
                    {
                        frm["COMP_TYPE_ID"] = Convert.ToString(dtb.Rows[0]["COMPANY_TYPE_ID"]);
                    }
                }
            }
            string selected_value = string.Empty;

            if (frm != null && frm[s.compNameTx] != null)
            {
                selected_value = frm[s.compNameTx];
            }
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", s.Id);
            if (s.dynWhere != null && !s.dynWhere.Trim().Equals(""))
            {
                string[] cons = s.dynWhere.Split(',');
                foreach (string con in cons)
                {
                    string[] convals = con.Split('=');
                    if (!conditions.ContainsKey(convals[0].Trim()))
                    {
                        if (convals[1].Trim().StartsWith("@") && Convert.ToString(convals[1].Trim().Substring(1)) != "0")
                        {
                            if (frm != null && frm.AllKeys.Contains(convals[1].Trim().Substring(1)))
                            {
                                conditions.Add(convals[0].Trim(), frm[convals[1].Trim().Substring(1)]);
                            }
                            else if (HttpContext.Current.Session[convals[1].Trim().Substring(1)] != null && !Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]).Trim().Equals(""))
                            {
                                //conditions.Add(convals[0].Trim(), HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                string val = Convert.ToString(HttpContext.Current.Session[convals[1].Trim().Substring(1)]);
                                bool proceed = false;
                                try { int v = Convert.ToInt32(val); if (v > 0) proceed = true; } catch { proceed = true; }
                                if (proceed) conditions.Add("SCRH_" + convals[0].Trim(), val);
                            }
                        }
                        else conditions.Add(convals[0].Trim(), convals[1].Trim());
                    }
                }
            }
            JObject jdata = DBTable.GetData("queryreport", conditions, "Report_COMP_T", 0, 100, "stimulate");
            DataTable dt = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "query" || property.Name == "report" || property.Name == "queryreport")
                {
                    dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            if (s.Comp_Type_Nm == (Int32)HTMLTag.DROPDOWN_WITH_QUERY)
                sb.Append("<select class='form-control panelInputs' name='").Append(s.compNameTx).Append("'");
            else
                sb.Append("<select class='form-control panelInputs' multiple='multiple' size=" + (dt != null ? (dt.Rows.Count + 1) : 0) + " name='").Append(s.compNameTx).Append("'");

            if (s.compStyleTx != null && !s.compStyleTx.Trim().Equals("")) sb.Append("style='").Append(s.compStyleTx).Append("' ");
            if (s.compScriptTx != null && !s.compScriptTx.Trim().Equals("")) sb.Append(s.compStyleTx);
            sb.Append(">");
            sb.Append("<option value=''>").Append("-- Select --</option>");
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //int key = Convert.ToInt32(dt.Rows[i][0]);
                        string key = Convert.ToString(dt.Rows[i][0]);// Convert.ToInt32(dt.Rows[i][0]);
                        string value = Convert.ToString(dt.Rows[i][1]);

                        if (!string.IsNullOrEmpty(selected_value) && selected_value == key)
                        {
                            sb.Append("<option selected value='" + key + "'>").Append("" + value + "</option>");
                        }
                        else
                        {
                            sb.Append("<option value='" + key + "'>").Append("" + value + "</option>");

                        }
                    }
                }
                sb.Append(" </select>");
            }
        }

        public static DataTable ConvertXSLXtoDataTable()
        {
            DataTable dt = new DataTable();
            using (XLWorkbook workBook = new XLWorkbook(HttpContext.Current.Request.Files[0].InputStream))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(1);

                //Loop through the Worksheet rows.
                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            i++;
                        }
                    }
                }
            }

            return dt;
        }
    }
}