using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Security.Cryptography;
using System.Reflection;

namespace ICSI.Api.Util
{
    public static class UtilService
    {
        private static string ANONYMOUS_PASSWORD = "FA6EC1E225B2420388203C6A67193A5E";
        
        public static string NewSessionKey(SqlConnection sqlConnection)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            string r = Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty);
            userData.Add("SESSION_KEY", r);
            List<Dictionary<string, object>> l = (List<Dictionary<string, object>>)Util.UtilService.GetData(sqlConnection, "USER_T", userData, 0, 0);
            if (l.Count > 0)
            {
                return NewSessionKey(sqlConnection);
            }
            else
                return r;
        }

        public static List<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));
            while (reader.Read())
                results.Add(SerializeRow(cols, reader));
            return results;
        }

        public static Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                        SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        public static string toJson(SqlDataReader reader)
        {
            var r = Serialize(reader);
            string json = JsonConvert.SerializeObject(r, Formatting.Indented);
                return json;
        }

        public static List<Dictionary<string, object>> fromJson(string json)
        {
            //List<Dictionary<string, object>>
            Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<Dictionary<string, object>> vals = new List<Dictionary<string, object>>();
            vals.Add(values);
            return vals;
        }

        public static SqlCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            return command;
        }

        public static List<string> GetTableList(IDbConnection con)
        {
            List<string> list = new List<string>();
            SqlCommand cmd = CreateCommand("SELECT name from sys.tables where name not in ('ROLE_T','MENU_T','APPLICATION_T','REPORT_T','RESP_SCREEN_T','APP_MODULE_T','ROLE_RESP_T','SUPER_USER_T','SCREEN_T','USER_ROLE_T','RESPONSIBILITY_T','USER_SCREEN_COMP_T','USER_TYPE_T','USER_RESP_T','SCREEN_COMP_T','USER_T') order by name", (IDbConnection)con);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((string)reader["name"]);
            }
            return list;
        }

        public static List<DataColumn> GetTableColumnMetadata(IDbConnection con, string tableName)
        {
            List<DataColumn> list = new List<DataColumn>();
            SqlCommand cmd = (SqlCommand)CreateCommand("select * from " + tableName, (IDbConnection)con);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
            DataTable schemaTable = reader.GetSchemaTable();
            foreach (DataRow field in schemaTable.Rows)
            {
                foreach (DataColumn property in schemaTable.Columns)
                {
                    list.Add(property);
                }
            }
            return list;
        }

        /* public static String ParameterValueForSQL(this SqlParameter sp)
         {
             String retval = "";

             switch (sp.SqlDbType)
             {
                 case SqlDbType.Char:
                 case SqlDbType.NChar:
                 case SqlDbType.NText:
                 case SqlDbType.NVarChar:
                 case SqlDbType.Text:
                 case SqlDbType.Time:
                 case SqlDbType.VarChar:
                 case SqlDbType.Xml:
                 case SqlDbType.Date:
                 case SqlDbType.DateTime:
                 case SqlDbType.DateTime2:
                 case SqlDbType.DateTimeOffset:
                     retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                     break;

                 case SqlDbType.Bit:
                     retval = (sp.Value.ToBooleanOrDefault(false)) ? "1" : "0";
                     break;

                 default:
                     retval = sp.Value.ToString().Replace("'", "''");
                     break;
             }
             return retval;
         }
         */
        public static string GetJsonData(IDbConnection con, string tableName, Dictionary<string, Object> p, int start, int end)
        {
            IEnumerable<Dictionary<string, object>> r = GetData(con, tableName, p, start, end);
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        public static List<Dictionary<string, object>> GetMetaData(IDbConnection con, string tableName)//string schemaName,
        {
            string sql = "SELECT COLUMN_NAME,DATA_TYPE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" + tableName + "'  ORDER BY ORDINAL_POSITION"; //and TABLE_CATALOG='" + schemaName + "'
            SqlCommand command = CreateCommand(sql, con);
            SqlDataReader reader = command.ExecuteReader();
            List<Dictionary<string, object>> e = Serialize(reader);
            reader.Close();
            reader = null;
            command = null;
            return e;
        }

        public static List<Dictionary<string, object>> GetData(IDbConnection con, string tableName, Dictionary<string, Object> p, int start, int end)
        {
            return GetData(con, tableName, p, null, start, end);
        }

        public static List<Dictionary<string, object>> GetData(IDbConnection con, string tableName, Dictionary<string, Object> p, Dictionary<string, string> q, int start, int end)
        {
            if (start == 0 && end == 0)
            {
                start = 0;
                end = 100;
            }
            StringBuilder sb = new StringBuilder();
            StringBuilder sbb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(tableName);
            sbb.Append("SELECT top 1 * FROM ").Append(tableName);
            SqlCommand command = CreateCommand(sbb.ToString(), con);
            //Dictionary<string, DataColumn> list = new Dictionary<string, DataColumn>();

            SqlDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo);
            List<string> list = reader.GetSchemaTable().Rows.Cast<DataRow>().Select(r => (string)r["ColumnName"]).ToList();
            /*DataTable schemaTable = reader.GetSchemaTable();
            //foreach (DataRow field in schemaTable.Rows)
            if (schemaTable != null && schemaTable.Columns.Count > 0)
            {
                foreach (DataColumn property in schemaTable.Columns)
                {
                    list.Add(property.ColumnName, property);
                }
            }*/
            reader.Close();
            reader = null;
            command = null;
            if (p != null && p.Count > 0)
            {
                int i = 0;
                foreach (var k in p)
                {
                    if (list.Contains(k.Key))
                    {
                        if (i == 0) sb.Append(" WHERE ");
                        else if (i > 0) sb.Append(" AND ");
                        sb.Append(k.Key);
                        if (q != null && q.ContainsKey(k.Key))
                        {
                            sb.Append(" ").Append(q[k.Key]);
                            if (q[k.Key].Contains("is")) sb.Append(" ").Append(k.Value);
                            else sb.Append("@").Append(k.Key);
                        }
                        else sb.Append("=").Append("@").Append(k.Key);
                        ++i;
                    }
                }
            }
            
            if (!tableName.Equals("INFORMATION_SCHEMA.TABLES") && !tableName.Equals("sys.all_objects"))
            {
                sb.Append(" ORDER BY ID").Append(" OFFSET ").Append(start).Append(" ROWS FETCH NEXT ").Append(end).Append(" ROWS ONLY");
            }
            
            //string q = sb.ToString();

            command = CreateCommand(sb.ToString(), con);
            if (p != null && p.Count > 0)
            {
                foreach (var k in p)
                {
                    if (list.Contains(k.Key))
                    {
                        if (q != null && q.ContainsKey(k.Key) && q[k.Key].Contains("is"))
                        {
                            continue;
                        }
                        SqlParameter param = new SqlParameter("@" + k.Key, k.Value);
                        command.Parameters.Add(param);
                    }
                }
            }
            reader = command.ExecuteReader();
            List<Dictionary<string, object>> e = Serialize(reader);
            //string json = toJson(reader);
            reader.Close();
            reader = null;
            command = null;
            return e;
        }

        public static bool Contains(this String str,
                                String substr,
                                StringComparison cmp)
        {
            if (substr == null)
                throw new ArgumentNullException("substring substring",
                                                " cannot be null.");

            else if (!Enum.IsDefined(typeof(StringComparison), cmp))
                throw new ArgumentException("comp is not a member of",
                                            "StringComparison, comp");

            return str.IndexOf(substr, cmp) >= 0;
        }

        public static void SearchData(IDbConnection con, string sessionKey, string schemaName, string tableName, string json)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELCT * from ").Append(tableName);
            List<Dictionary<string, object>> obj = fromJson(json);
            if (obj != null && obj.Count > 0)
            {
                Dictionary<string, object> d = obj[0];
                bool isWhere = false;
                if (d.Count > 0)
                {
                    foreach (var val in d)
                    {
                        if (!isWhere)
                        {
                            sb.Append(" WHERE ").Append(val.Key).Append("=@").Append(val.Key);
                            isWhere = true;
                        }
                        else
                        {
                            sb.Append(" OR ").Append(val.Key).Append("=@").Append(val.Key);
                        }
                    }
                    SqlCommand cmd = CreateCommand(sb.ToString(), con);
                    foreach (var val in d)
                    {
                        cmd.Parameters.AddWithValue("@" + val.Key, val.Value);
                    }

                }
            }
        }

        public static int InsertData(IDbConnection con, string sessionKey, string schemaName, string tableName, string json, int uid)
        {
            List<Dictionary<string, object>> obj = fromJson(json);
            return InsertData(con, sessionKey, schemaName, tableName, obj, uid);
        }
        public static int InsertData(IDbConnection con, string sessionKey, string schemaName, string tableName, List<Dictionary<string, object>> obj, int uid)
        {
            bool isInsert = false;
            int returnValue = -1;
            int updatedId = -1;
            try
            {
                if (obj != null && obj.Count > 0)
                {
                    Dictionary<string, object> d = obj[0];
                    StringBuilder query = new StringBuilder();
                    bool isActiveYn = false;
                    Int64 id = 0;
                    if (d.ContainsKey("ID") && d["ID"] != null)
                    {
                        id = Convert.ToInt64(d["ID"]);
                    }
                    if (id > 0)
                    {
                        query.Append("UPDATE ");
                        query.Append(tableName).Append(" SET ");
                        bool b = false;
                        foreach (var val in d)
                        {
                            if (val.Key.Equals("ID") || val.Key.Equals("UPDATED_DT") || val.Key.Equals("UPDATED_BY")) continue;
                            //else if (val.Key.Equals("ACTIVE_YN")) isActiveYn = true;
                            if (b == true)
                            {
                                query.Append(",");
                            }
                            else b = true;                            
                            query.Append(val.Key).Append("=@").Append(val.Key);
                            if (val.Key == "CAN_REASON_TX" && !string.IsNullOrEmpty(Convert.ToString(val.Value)))
                                query.Append(",CANCELLED_BY=@CANCELLED_BY,CANCELLED_DT=@CANCELLED_DT");
                        }
                        query.Append(", UPDATED_DT=@upddt, UPDATED_BY=@updby");
                        query.Append(" WHERE ID=").Append(d["ID"]);
                        string di = Convert.ToString(d["ID"]);
                        updatedId = Convert.ToInt32(di);
                    }
                    else
                    {
                        isInsert = true;
                        StringBuilder columns = new StringBuilder("(");
                        StringBuilder columnParams = new StringBuilder("(");
                        bool b = false;
                        foreach (var val in d)
                        {
                            if (val.Key.Equals("ID") || val.Key.Equals("CREATED_DT") || val.Key.Equals("CREATED_BY") || val.Key.Equals("UPDATED_DT") || val.Key.Equals("UPDATED_BY")) continue;
                            else if (val.Key.Equals("ACTIVE_YN")) isActiveYn = true;
                            if (b == true)
                            {
                                columns.Append(",");
                                columnParams.Append(",");
                            }
                            else b = true;
                            columns.Append(val.Key);
                            columnParams.Append("@").Append(val.Key);
                        }
                        if (!isActiveYn) columns.Append(",ACTIVE_YN");
                        columns.Append(",CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY");
                        if (!isActiveYn) columnParams.Append(",1");
                        columnParams.Append(",@crdt,@crby,@upddt,@updby");
                        query.Append("INSERT INTO ").Append(tableName).Append(columns.ToString()).Append(") VALUES").Append(columnParams.ToString()).Append(")");
                    }
                    SqlCommand cmd = CreateCommand(query.ToString(), con);
                    foreach (var val in d)
                    {                        
                        if (val.Value == null || Convert.ToString(val.Value).IndexOf("undefined") != -1)
                        {
                            if (query.ToString().IndexOf("@" + val.Key) != -1) cmd.Parameters.AddWithValue("@" + val.Key, DBNull.Value);
                            continue;
                        }
                        if (isInsert && val.Key.Equals("ID")) continue;
                        if (!isInsert && (val.Key.Equals("UPDATED_DT") || val.Key.Equals("UPDATED_BY"))) continue;
                        if (val.Key == "CAN_REASON_TX" && !string.IsNullOrEmpty(Convert.ToString(val.Value)))
                        {
                            cmd.Parameters.AddWithValue("@CANCELLED_BY", uid);
                            cmd.Parameters.AddWithValue("@CANCELLED_DT", DateTime.Now);
                        }
                        if (val.Value == null || Convert.ToString(val.Value) == "01/01/1900")
                            cmd.Parameters.AddWithValue("@" + val.Key, DBNull.Value);
                        else
                        {
                            if (val.Key.Equals("FILE_BLB"))
                            {
                                cmd.Parameters.AddWithValue("@" + val.Key, Encoding.UTF8.GetBytes((string)val.Value));
                            }else cmd.Parameters.AddWithValue("@" + val.Key, val.Value);
                        }
                    }
                    DateTime dnow = DateTime.Now;
                    cmd.Parameters.AddWithValue("@upddt", dnow);
                    cmd.Parameters.AddWithValue("@updby", uid);
                    if (isInsert)
                    {
                        cmd.Parameters.AddWithValue("@crdt", dnow);
                        cmd.Parameters.AddWithValue("@crby", uid);
                    }
                    cmd.ExecuteNonQuery();
                    returnValue = 0;
                }
            }
            catch (Exception ex)
            {
                //return GetResponse(sessionKey,"-100", ex.Message, json);                
                throw ex;
            }
            if (isInsert)
            {
                string qry = "SELECT IDENT_CURRENT('" + tableName + "') as LAST_INSERTED_ID";
                SqlCommand cmd1 = CreateCommand(qry, con);
                SqlDataReader reader = cmd1.ExecuteReader();
                List<Dictionary<string, object>> e = Serialize(reader);
                reader.Close();
                reader = null;
                cmd1 = null;
                if (e != null && e.Count == 1)
                {
                    var d = e[0];
                    if (d.ContainsKey("LAST_INSERTED_ID"))
                    {
                        string id = Convert.ToString(d["LAST_INSERTED_ID"]);
                        returnValue = Convert.ToInt32(id);
                    }
                }
            }
            else if (updatedId > 0) returnValue = updatedId;
            //return GetResponse(sessionKey,"0", (isInsert?"Insert":"Update") +" Successfully", json);
            return returnValue;// (isInsert ? "Insert" : "Update") + " Successfully";
        }

        public static int UpdateData(IDbConnection con, string sessionKey, string schemaName, string tableName, string json, int uid)
        {
            return InsertData(con, sessionKey, schemaName, tableName, json, uid);
        }

        public static string GetResponse(string sessionKey, string statCode, string statMessage, string jsonData)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("StatCode", statCode);
            d.Add("StatMessage", statMessage);
            d.Add("data", Util.CryptographyUtil.EncryptDataPKCS7(jsonData, sessionKey));
            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }

        public static string BuildResponse(string sessionKey, string statCode, string statMessage, List<Dictionary<string, object>> data)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("StatCode", statCode);
            d.Add("StatMessage", statMessage);
            Dictionary<string, object> d1 = new Dictionary<string, object>();
            d1.Add("data", data);
            if (sessionKey != null && !sessionKey.Trim().Equals("") && statCode.Equals("0"))
            {
                string encstr = JsonConvert.SerializeObject(d1, Formatting.Indented);
                d.Add("data", CryptographyUtil.EncryptDataPKCS7(encstr, sessionKey));
            }
            else d.Add("data", "");
            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }

        public static string BuildResponse(string statCode, string statMessage, string data)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("StatCode", statCode);
            d.Add("StatMessage", statMessage);
            Dictionary<string, object> d1 = new Dictionary<string, object>();
            d1.Add("data", data);
            if (statCode.Equals("0"))
            {
                d.Add("data", data);
            }
            else d.Add("data", "");
            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }

        public enum ICSICodes
        {
            Error = -100,
            [Display(Name = "Success")]
            Success = 0,
            [Display(Name = "Invalid Request Arguments")]
            Invalid_Request_Arguments = -1,
            [Display(Name = "Invalid JSON Request")]
            Invalid_JSON_Request = -2,
            [Display(Name = "Wrong JSON Format")]
            Wrong_JSON_Format = -3,
            [Display(Name = "Wrong username")]
            Wrong_username = -4,
            [Display(Name = "Wrong password")]
            Wrong_password = -5,
            [Display(Name = "Wrong Session")]
            Wrong_Session = -6,
            [Display(Name = "Invalid User Name/Password")]
            INVALID_USER = -7,
            [Display(Name = "The validity of your registration has expired. Please get your registration renewed through 'Continuation of Registration' process and then submit your training related request.")]
            PROFESSIONAL_INVALID = -8,
            [Display(Name = "The validity of your registration has expired. Please get your registration renewed through 'Registration Denovo' process and then submit your training related request.")]
            PROF_EXEC_ONGOING = -9,
            [Display(Name = "No Reconcilation Data")]
            RECON_DATA = -10
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }

        public static string doAction(Dictionary<string, DB.Database> _databases, object json)
        {
            string SessionKey = "";
            string StatMessage = "Un Expected Exception";
            int StatCode = -100;
            string StatData = "";
            SqlConnection sqlConnection = null;
            if (json != null)
            {
                JObject req = null;
                try
                {
                    req = JObject.Parse(json.ToString());
                    if (req != null && req.HasValues && req.ContainsKey("sid") && req.ContainsKey("data"))
                    {
                        string sid = req.GetValue("sid").ToString();
                        string edata = req.GetValue("data").ToString();
                        bool isAnonymous = sid.Equals("anonymous");
                        List<Dictionary<string, object>> l = null;
                        if (!isAnonymous) {
                            Dictionary<string, object> userData = new Dictionary<string, object>();
                            userData.Add("LOGIN_ID", sid);
                            sqlConnection = (SqlConnection)_databases["DEFAULT"].CreateOpenConnection();
                            l=(List<Dictionary<string, object>>)Util.UtilService.GetData(sqlConnection, "USER_T", userData, 0, 0);
                        }
                        if (isAnonymous || (l != null && l.Count >= 1))
                        {
                            Dictionary<string, object> user = l[0];
                            SessionKey = isAnonymous?ANONYMOUS_PASSWORD:(string)user["SESSION_KEY"];
                            long uid = isAnonymous?0:(long)user["ID"];
                            string data = Util.CryptographyUtil.DecryptDataPKCS7(edata, SessionKey);
                            if (data != null)
                            {
                                JObject jdata = JObject.Parse(data);
                                if (jdata.ContainsKey("type"))
                                {
                                    string type = jdata.GetValue("type").ToString();
                                    if (type.Equals("insert") || type.Equals("update"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        int returnValue = 0;
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            //connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                            //else connection = (SqlConnection)_databases["DEFAULT"].CreateOpenConnection(); 
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                //var conditions = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value;
                                                var tabledata = itemProperties.FirstOrDefault(x => x.Name == "data").Value;
                                                foreach (var titem in tabledata.Children())
                                                {
                                                    string str = titem.ToString();
                                                    returnValue = InsertData(isSchemaDifferent ? connection : sqlConnection, SessionKey, schemaName, tableName, str, Convert.ToInt32(uid));
                                                    if (returnValue > 0)
                                                    {
                                                        // last inserted id is the returnValue
                                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                                        int st = 0;
                                                        int ed = 1;
                                                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                                                        conditions.Add("ID", returnValue);
                                                        List<Dictionary<string, object>> tData = GetData(isSchemaDifferent ? connection : sqlConnection, tableName, conditions, st, ed);
                                                        dataOutput.Add(tableName, tData);
                                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                                    }
                                                }
                                            }
                                            catch (Exception exx)
                                            {
                                                // TODO needs to throw exception
                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        if (returnValue > 0)
                                        {
                                            StatCode = (int)ICSICodes.Success;
                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                        }
                                        else
                                        {
                                            StatCode = (int)ICSICodes.Invalid_Request_Arguments;
                                            //StatMessage = Convert.ToString(ICSICodes.Invalid_Request_Arguments);
                                            StatMessage = GetDisplayName(ICSICodes.Invalid_Request_Arguments);
                                        }
                                    }
                                    else if (type.Equals("qfetch"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string st = itemProperties.FirstOrDefault(x => x.Name == "start").Value.ToString();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string ed = itemProperties.FirstOrDefault(x => x.Name == "end").Value.ToString();
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            string condops = itemProperties.FirstOrDefault(x => x.Name == "conditionops").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            string pgn = itemProperties.FirstOrDefault(x => x.Name == "pgn").Value.ToString();
                                            string pgr = itemProperties.FirstOrDefault(x => x.Name == "pgr").Value.ToString();
                                            if (pgn != null && !pgn.Trim().Equals(""))
                                            {
                                                int pgnum = Convert.ToInt32(pgn);
                                                if (pgnum > 0)
                                                {
                                                    if (pgr != null && !pgr.Trim().Equals(""))
                                                    {
                                                        int pgrows = Convert.ToInt32(pgr);
                                                        if (pgrows > 0)
                                                        {
                                                            int start = (pgnum - 1) * pgrows;
                                                            int end = start + pgrows;
                                                            st = "" + start;
                                                            ed = "" + end;
                                                        }
                                                    }
                                                }
                                            }
                                            Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                            Dictionary<string, string> conditionops = JsonConvert.DeserializeObject<Dictionary<string, string>>(condops);
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                int b = Convert.ToInt32(conditions["QID"]);
                                                conditions.Remove("QID");
                                                List<Dictionary<string, object>> tData = GetQueryResult(sqlConnection, isSchemaDifferent ? connection : sqlConnection, b,conditions, Convert.ToInt32(st), Convert.ToInt32(ed));
                                                dataOutput.Add(type, tData);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { if(connection!=null)connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                        StatCode = (int)ICSICodes.Success;
                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                    }
                                    else if (type.Equals("fetch") || type.Equals("mfetch"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string st = itemProperties.FirstOrDefault(x => x.Name == "start").Value.ToString();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string ed = itemProperties.FirstOrDefault(x => x.Name == "end").Value.ToString();                                            
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            string condops = itemProperties.FirstOrDefault(x => x.Name == "conditionops").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            string pgn = itemProperties.FirstOrDefault(x => x.Name == "pgn").Value.ToString();
                                            string pgr = itemProperties.FirstOrDefault(x => x.Name == "pgr").Value.ToString();
                                            if (pgn!=null && !pgn.Trim().Equals(""))
                                            {
                                                int pgnum = Convert.ToInt32(pgn);
                                                if (pgnum > 0)
                                                {
                                                    if (pgr != null && !pgr.Trim().Equals(""))
                                                    {
                                                        int pgrows = Convert.ToInt32(pgr);
                                                        if (pgrows > 0)
                                                        {
                                                            int start = (pgnum - 1) * pgrows;
                                                            int end = start + pgrows;
                                                            st = "" + start;
                                                            ed = "" + end;
                                                        }
                                                    }
                                                }
                                            }
                                            Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                            Dictionary<string, string> conditionops = JsonConvert.DeserializeObject<Dictionary<string, string>>(condops);
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                List<Dictionary<string, object>> tData = null;
                                                if (type.Equals("fetch"))
                                                {
                                                    tData = GetData(isSchemaDifferent ? connection : sqlConnection, tableName, conditions, conditionops, Convert.ToInt32(st), Convert.ToInt32(ed));
                                                    dataOutput.Add(tableName, tData);
                                                }
                                                else
                                                {
                                                    List<Dictionary<string, object>> mData = GetMetaData(isSchemaDifferent ? connection : sqlConnection, tableName);
                                                    dataOutput.Add("META_DATA", mData);
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                        StatCode = (int)ICSICodes.Success;
                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                    }
                                    else if (type.Equals("query") || type.Equals("report") || type.Equals("queryreport"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string st = itemProperties.FirstOrDefault(x => x.Name == "start").Value.ToString();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string ed = itemProperties.FirstOrDefault(x => x.Name == "end").Value.ToString();
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                            string pgn = itemProperties.FirstOrDefault(x => x.Name == "pgn").Value.ToString();
                                            string pgr = itemProperties.FirstOrDefault(x => x.Name == "pgr").Value.ToString();
                                            if (pgn != null && !pgn.Trim().Equals(""))
                                            {
                                                int pgnum = Convert.ToInt32(pgn);
                                                if (pgnum > 0)
                                                {
                                                    if (pgr != null && !pgr.Trim().Equals(""))
                                                    {
                                                        int pgrows = Convert.ToInt32(pgr);
                                                        if (pgrows > 0)
                                                        {
                                                            int start = (pgnum - 1) * pgrows;
                                                            int end = start + pgrows;
                                                            st = "" + start;
                                                            ed = "" + end;
                                                        }
                                                    }
                                                }
                                            }
                                            int firstId = 0;
                                            int secondId = 0;
                                            string secondIdStr = string.Empty;
                                            string secondKey = string.Empty;
                                            if (conditions.Count > 0)
                                            {
                                                int counter = 0;
                                                if (type.Equals("query"))
                                                {
                                                    foreach (KeyValuePair<string, object> itemcon in conditions)
                                                    {
                                                        if (counter == 0)
                                                            firstId = Convert.ToInt32(itemcon.Value);
                                                        else
                                                        {
                                                            if(!(int.TryParse(Convert.ToString(itemcon.Value),out secondId)))secondIdStr = Convert.ToString(itemcon.Value);
                                                            else secondId = Convert.ToInt32(itemcon.Value);
                                                         
                                                            secondKey = itemcon.Key;
                                                            if (secondKey.Contains("SCRH_")) secondKey = secondKey.Replace("SCRH_", "");

                                                        }
                                                        counter++;
                                                    }
                                                }
                                                else
                                                {
                                                    if (conditions.ContainsKey("SCMPID"))
                                                    {
                                                        firstId = Convert.ToInt32(conditions["SCMPID"]);
                                                    }
                                                    else
                                                    {
                                                        firstId = Convert.ToInt32(conditions["ID"]);
                                                    }
                                                }
                                            }
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                //string sql = getQuery(sqlConnection, type.Equals("report"), Convert.ToInt32(conditions["ID"]));
                                                string sql = getQuery(sqlConnection, type.Contains("report"), firstId);
                                                string origsql = sql;
                                                if (!type.Equals("report"))
                                                {
                                                    if (!string.IsNullOrEmpty(secondIdStr))
                                                    {
                                                        sql = sql.Replace("@ID", secondIdStr);
                                                    }
                                                    else if (secondId != 0)
                                                    {
                                                        sql = sql.Replace("@ID", Convert.ToString(secondId));
                                                    }
                                                    else
                                                    {
                                                        sql = sql.Replace("@SQL", "AND " + secondKey + "='" + secondIdStr + "'");
                                                    }
                                                }
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                List<Dictionary<string, object>> tData = null;
                                                if (type.Equals("report"))
                                                {
                                                    if (conditions != null && conditions.ContainsKey("SCMPID"))
                                                    {
                                                        conditions.Remove("SCMPID");
                                                    }
                                                    tData = GetSearchResult(isSchemaDifferent ? connection : sqlConnection, sql, conditions, Convert.ToInt32(st), Convert.ToInt32(ed));
                                                }
                                                else
                                                {
                                                    tData = GetSQLData(isSchemaDifferent ? connection : sqlConnection, sql, origsql, secondIdStr);
                                                }
                                                dataOutput.Add(type, tData);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                        StatCode = (int)ICSICodes.Success;
                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                    }
                                    else if (type.Equals("ajax"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string st = itemProperties.FirstOrDefault(x => x.Name == "start").Value.ToString();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string ed = itemProperties.FirstOrDefault(x => x.Name == "end").Value.ToString();
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                            string pgn = itemProperties.FirstOrDefault(x => x.Name == "pgn").Value.ToString();
                                            string pgr = itemProperties.FirstOrDefault(x => x.Name == "pgr").Value.ToString();
                                            if (pgn != null && !pgn.Trim().Equals(""))
                                            {
                                                int pgnum = Convert.ToInt32(pgn);
                                                if (pgnum > 0)
                                                {
                                                    if (pgr != null && !pgr.Trim().Equals(""))
                                                    {
                                                        int pgrows = Convert.ToInt32(pgr);
                                                        if (pgrows > 0)
                                                        {
                                                            int start = (pgnum - 1) * pgrows;
                                                            int end = start + pgrows;
                                                            st = "" + start;
                                                            ed = "" + end;
                                                        }
                                                    }
                                                }
                                            }
                                            int firstId = 0;                                            
                                            if (conditions.Count > 0)
                                            {
                                                firstId = Convert.ToInt32(conditions.First().Value);                                               
                                            }
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                //string sql = getQuery(sqlConnection, type.Equals("report"), Convert.ToInt32(conditions["ID"]));
                                                string sql = getQueryfromQuery_T(sqlConnection, firstId);

                                                int counter = 0;
                                                foreach (KeyValuePair<string, object> itemcon in conditions)
                                                {
                                                    if (counter != 0)
                                                    {
                                                        sql = sql.Replace("@"+itemcon.Key.ToString(),Convert.ToString(itemcon.Value));
                                                    }
                                                    counter++;
                                                }
                                             
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                List<Dictionary<string, object>> tData = null;
                                                tData = GetSQLData(isSchemaDifferent ? connection : sqlConnection, sql);
                                                dataOutput.Add(type, tData);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                        StatCode = (int)ICSICodes.Success;
                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                    }
                                    else if (type.Equals("search") || type.Equals("reports"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        Dictionary<string, List<Dictionary<string, object>>> dataOutput = new Dictionary<string, List<Dictionary<string, object>>>();
                                        foreach (var item in fdata.Children())
                                        {                                            
                                            var itemProperties = item.Children<JProperty>();
                                            bool isPaginationExists = false;
                                            string pgs = itemProperties.FirstOrDefault(x => x.Name == "pgs").Value.ToString();
                                            if (pgs!=null && pgs=="1")
                                            {
                                                isPaginationExists = true;
                                            }
                                            string st = itemProperties.FirstOrDefault(x => x.Name == "start").Value.ToString();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string ed = itemProperties.FirstOrDefault(x => x.Name == "end").Value.ToString();
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            string pgn = itemProperties.FirstOrDefault(x => x.Name == "pgn").Value.ToString();
                                            string pgr = itemProperties.FirstOrDefault(x => x.Name == "pgr").Value.ToString();
                                            if (pgn != null && !pgn.Trim().Equals(""))
                                            {
                                                int pgnum = Convert.ToInt32(pgn);
                                                if (pgnum > 0)
                                                {
                                                    if (pgr != null && !pgr.Trim().Equals(""))
                                                    {
                                                        int pgrows = Convert.ToInt32(pgr);
                                                        if (pgrows > 0)
                                                        {
                                                            int start = (pgnum - 1) * pgrows;
                                                            int end = start + pgrows;
                                                            st = "" + start;
                                                            ed = "" + pgrows;
                                                        }
                                                    }
                                                }
                                            }
                                            Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                string sql = getQuery(sqlConnection, type.Equals("query"), Convert.ToInt32(conditions["SCMPID"]));
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                if (conditions != null && conditions.ContainsKey("SCMPID"))
                                                {
                                                    conditions.Remove("SCMPID");
                                                }
                                                if (type.Equals("reports"))
                                                {
                                                    DataTable dtData = new DataTable();
                                                    Hashtable ht = new Hashtable();
                                                    string Val = "";
                                                    if (conditions != null && conditions.Count > 0)
                                                    {
                                                        foreach (var v in conditions)
                                                        {
                                                            if (v.Key.StartsWith("SCRH_"))
                                                            {
                                                                Val = Val + " and " + v.Key.Replace("SCRH_", "") + "='" + v.Value.ToString().Replace("SCRH_", "") + "'";
                                                            }
                                                        }
                                                    }
                                                    ht.Add("USER_ID", uid);
                                                    ht.Add("SQL", Val);
                                                    dtData = ExecuteSP(connection, sql, ht);
                                                    //string pageurl = "WebForm2.aspx";
                                                    //UtilService.Response.Write("<script> window.open('" + pageurl + "','_blank'); </script>");
                                                    //Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('../Reports/ReportViewer.aspx','_newtab');", true);
                                                }
                                                else
                                                {
                                                    List<Dictionary<string, object>> tData = null;
                                                    tData = GetSearchResult(isSchemaDifferent ? connection : sqlConnection, sql, conditions, Convert.ToInt32(st), Convert.ToInt32(ed), isPaginationExists);
                                                    dataOutput.Add(type, tData);
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        StatData = JsonConvert.SerializeObject(dataOutput, Formatting.Indented);
                                        if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                        StatCode = (int)ICSICodes.Success;
                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                    }
                                    else if (type.Equals("bulkinsert"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        int returnValue = 0;
                                        DataTable dtData = new DataTable();
                                        Dictionary<string, object> dtDataOutPut = new Dictionary<string, object>();

                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                var tabledata = itemProperties.FirstOrDefault(x => x.Name == "data").Value;
                                                foreach (var titem in tabledata.Children())
                                                {
                                                    string str = titem.ToString();
                                                    List<Dictionary<string, object>> obj = fromJson(str);
                                                    Hashtable ht = new Hashtable();
                                                    BulkInsertUpdate(isSchemaDifferent ? connection : sqlConnection, SessionKey, schemaName, tableName, obj, Convert.ToInt32(uid));
                                                    obj = fromJson(str);
                                                    Dictionary<string, object> dictData = obj[0];
                                                    if (dictData.ContainsKey("IS_RECONCILE"))
                                                    {
                                                        string strProcName = Convert.ToString(dictData["SP_NAME"]);

                                                        ht.Add("REF_ID", Convert.ToInt32(dictData["REF_ID"]));

                                                        if (sqlConnection.State == ConnectionState.Closed)
                                                        {
                                                            sqlConnection.Open();
                                                        }                                                                                                                                           //DataTable dt = Util.UtilService.validateUser(icsiConn, sid);
                                                        dtData = Util.UtilService.ExecuteSP(sqlConnection, strProcName, ht);
                                                        if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
                                                        {
                                                            dtDataOutPut.Add("RECON_DATA", dtData);
                                                            StatCode = (int)ICSICodes.Success;
                                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                                            returnValue = 1;
                                                        }
                                                        else
                                                        {
                                                            StatCode = (int)ICSICodes.RECON_DATA;
                                                            StatMessage = Convert.ToString(ICSICodes.RECON_DATA);
                                                        }
                                                    }
                                                    else if (dictData.ContainsKey("PG_REC_ID"))
                                                    {
                                                        Dictionary<string, object> dtlData = new Dictionary<string, object>();
                                                        dtlData.Add("PG_REC_ID", dictData["PG_REC_ID"]);
                                                        if (sqlConnection.State == ConnectionState.Closed)
                                                        {
                                                            sqlConnection.Open();
                                                        }
                                                        List<Dictionary<string, object>> lst = Util.UtilService.GetData(sqlConnection, "PG_REC_TEMP_DTL_T", dtlData, 0, 100);
                                                        if (lst != null && lst.Count > 0)
                                                        {
                                                            dtDataOutPut.Add("DTL_TBL_DATA", lst);
                                                            StatCode = (int)ICSICodes.Success;
                                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                                            returnValue = 1;
                                                        }
                                                        else
                                                        {
                                                            StatCode = (int)ICSICodes.RECON_DATA;
                                                            StatMessage = Convert.ToString(ICSICodes.RECON_DATA);
                                                        }

                                                    }
                                                    StatData = JsonConvert.SerializeObject(dtDataOutPut, Formatting.Indented);
                                                    if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                StatMessage = ex.Message;
                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        if (returnValue > 0)
                                        {
                                            StatCode = (int)ICSICodes.Success;
                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                        }
                                        else
                                        {
                                            StatCode = (int)ICSICodes.Invalid_Request_Arguments;
                                            //StatMessage = Convert.ToString(ICSICodes.Invalid_Request_Arguments);
                                            StatMessage = GetDisplayName(ICSICodes.Invalid_Request_Arguments);
                                        }
                                    }
                                    else if (type.Equals("execsp"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        int returnVal = 0;
                                        DataTable dtData = new DataTable();
                                        Dictionary<string, object> dtDataOutPut = new Dictionary<string, object>();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            string conds = itemProperties.FirstOrDefault(x => x.Name == "conditions").Value.ToString();
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                var tabledata = itemProperties.FirstOrDefault(x => x.Name == "data").Value;

                                                foreach (var titem in tabledata.Children())
                                                {
                                                    string str = titem.ToString();
                                                    List<Dictionary<string, object>> obj = fromJson(str);
                                                    Dictionary<string, object> dictData = obj[0];
                                                    Hashtable ht = new Hashtable();
                                                    string strProcName = string.Empty;
                                                    Dictionary<string, object> conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(conds);
                                                    DataTable dtOutput = new DataTable();
                                                    if (conditions != null && conditions.Count > 0)
                                                    {
                                                        foreach (var v in conditions)
                                                        {
                                                            ht.Add(v.Key, v.Value);
                                                        }
                                                        if (dictData.ContainsKey("SP_NAME"))
                                                        {
                                                            dtOutput = ExecuteSP(sqlConnection, Convert.ToString(dictData["SP_NAME"]), ht);
                                                            if (dtOutput != null && dtOutput.Rows != null && dtOutput.Rows.Count > 0)
                                                            {
                                                                dtDataOutPut.Add("DATA", dtOutput);
                                                                StatCode = (int)ICSICodes.Success;
                                                                StatMessage = Convert.ToString(ICSICodes.Success);
                                                                returnVal = 1;
                                                                dtDataOutPut.Add("STATUS", "Success");
                                                            }
                                                            else
                                                            {
                                                                StatCode = (int)ICSICodes.Error;
                                                                StatMessage = Convert.ToString(ICSICodes.Error);
                                                                dtDataOutPut.Add("STATUS", "Error Occurred while pulling the reconcilation records");
                                                            }
                                                        }
                                                    }
                                                    if (dictData.ContainsKey("REF_ID"))
                                                    {
                                                        ht.Add("REF_ID", Convert.ToInt32(dictData["REF_ID"]));
                                                        strProcName = Convert.ToString(dictData["REC_SP_NAME"]);
                                                        if (sqlConnection.State == ConnectionState.Closed)
                                                        {
                                                            sqlConnection.Open();
                                                        }                                                                                                                                           //DataTable dt = Util.UtilService.validateUser(icsiConn, sid);
                                                        dtData = Util.UtilService.ExecuteSP(sqlConnection, strProcName, ht);
                                                        if (dtData != null && dtData.Rows != null && dtData.Rows.Count > 0)
                                                        {
                                                            if (dtData.Columns.Contains("RETURN_VAL"))
                                                            {
                                                                returnVal = Convert.ToInt32(dtData.Columns.Contains("RETURN_VAL"));
                                                            }
                                                            else
                                                            {
                                                                returnVal = 1;
                                                            }
                                                        }


                                                        strProcName = Convert.ToString(dictData["SP_NAME"]);
                                                        if (returnVal == 1)
                                                        {

                                                            dtOutput = Util.UtilService.ExecuteSP(sqlConnection, strProcName, ht);
                                                            if (dtOutput != null && dtOutput.Rows != null && dtOutput.Rows.Count > 0)
                                                            {
                                                                dtDataOutPut.Add("RECON_DATA", dtOutput);
                                                                StatCode = (int)ICSICodes.Success;
                                                                StatMessage = Convert.ToString(ICSICodes.Success);
                                                                returnVal = 1;
                                                                dtDataOutPut.Add("REC_STATUS", "Success");
                                                            }
                                                            else
                                                            {
                                                                StatCode = (int)ICSICodes.RECON_DATA;
                                                                StatMessage = Convert.ToString(ICSICodes.RECON_DATA);
                                                                dtDataOutPut.Add("REC_STATUS", "Error Occurred while pulling the reconcilation records");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            dtDataOutPut.Add("REC_STATUS", "Error Occurred while processing the reconcilation");
                                                        }
                                                    }
                                                    StatData = JsonConvert.SerializeObject(dtDataOutPut, Formatting.Indented);
                                                    if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                StatMessage = ex.Message;
                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        if (returnVal > 0)
                                        {
                                            StatCode = (int)ICSICodes.Success;
                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                        }
                                        else
                                        {
                                            StatCode = (int)ICSICodes.Invalid_Request_Arguments;
                                            //StatMessage = Convert.ToString(ICSICodes.Invalid_Request_Arguments);
                                            StatMessage = GetDisplayName(ICSICodes.Invalid_Request_Arguments);
                                        }
                                    }
                                    else if (type.Equals("createTbl"))
                                    {
                                        var fdata = jdata.Children<JProperty>().FirstOrDefault(x => x.Name == "data").Value;
                                        int returnVal = 0;
                                        DataTable dtData = new DataTable();
                                        foreach (var item in fdata.Children())
                                        {
                                            var itemProperties = item.Children<JProperty>();
                                            string schemaName = itemProperties.FirstOrDefault(x => x.Name == "schema").Value.ToString();
                                            string tableName = itemProperties.FirstOrDefault(x => x.Name == "table").Value.ToString();
                                            SqlConnection connection = null;
                                            bool isSchemaDifferent = false;
                                            if (schemaName != null && _databases.ContainsKey(schemaName)) isSchemaDifferent = true;
                                            try
                                            {
                                                if (isSchemaDifferent) connection = (SqlConnection)_databases[schemaName].CreateOpenConnection();
                                                var tabledata = itemProperties.FirstOrDefault(x => x.Name == "data").Value;
                                                foreach (var titem in tabledata.Children())
                                                {
                                                    string str = titem.ToString();
                                                    List<Dictionary<string, object>> obj = fromJson(str);
                                                    Dictionary<string, object> tblData = obj[0];
                                                    string tblQry = Convert.ToString(tblData["TBL_QUERY_STR"]);
                                                    SqlCommand cmd = Util.UtilService.CreateCommand(tblQry, connection);
                                                    cmd.ExecuteNonQuery();

                                                    Dictionary<string, object> dtDataOutPut = new Dictionary<string, object>();
                                                    Dictionary<string, object> dtlData = new Dictionary<string, object>();
                                                    dtlData.Add("TBL_NAME", Convert.ToString(tblData["TBL_NAME"]));
                                                    List<Dictionary<string, object>> lst = Util.UtilService.GetData(sqlConnection, "PG_REC_TEMP_DTL_T", dtlData, 0, 100);
                                                    if (lst != null && lst.Count > 0)
                                                    {
                                                        dtDataOutPut.Add("TBL_DATA", lst);
                                                        StatCode = (int)ICSICodes.Success;
                                                        StatMessage = Convert.ToString(ICSICodes.Success);
                                                        returnVal = 1;
                                                    }
                                                    else
                                                    {
                                                        StatCode = (int)ICSICodes.RECON_DATA;
                                                        StatMessage = Convert.ToString(ICSICodes.RECON_DATA);
                                                    }
                                                    StatData = JsonConvert.SerializeObject(dtDataOutPut, Formatting.Indented);
                                                    if (StatData != null && !StatData.Trim().Equals("")) StatData = CryptographyUtil.EncryptDataPKCS7(StatData, SessionKey);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                StatMessage = ex.Message;
                                            }
                                            if (isSchemaDifferent)
                                            {
                                                try { connection.Close(); } catch (Exception ecc) { }
                                                connection = null;
                                            }
                                        }
                                        if (returnVal > 0)
                                        {
                                            StatCode = (int)ICSICodes.Success;
                                            StatMessage = Convert.ToString(ICSICodes.Success);
                                        }
                                        else
                                        {
                                            StatCode = (int)ICSICodes.Invalid_Request_Arguments;
                                            //StatMessage = Convert.ToString(ICSICodes.Invalid_Request_Arguments);
                                            StatMessage = GetDisplayName(ICSICodes.Invalid_Request_Arguments);
                                        }
                                    }
                                }
                                else
                                {
                                    StatCode = (int)ICSICodes.Wrong_JSON_Format;
                                    StatMessage = Convert.ToString(ICSICodes.Wrong_JSON_Format);
                                }
                            }
                            else
                            {
                                StatCode = (int)ICSICodes.Wrong_Session;
                                StatMessage = Convert.ToString(ICSICodes.Wrong_Session);
                            }
                        }
                        else
                        {
                            // TODO - if the data not present with session key
                            StatCode = (int)ICSICodes.Wrong_Session;
                            StatMessage = Convert.ToString(ICSICodes.Wrong_Session);
                        }
                    }
                    else
                    {
                        // TODO - req does not have sid and data
                        StatCode = (int)ICSICodes.Invalid_Request_Arguments;
                        StatMessage = Convert.ToString(ICSICodes.Invalid_Request_Arguments);
                    }
                }
                catch (Exception ex)
                {
                    // TODO - if wrong json format or wring session key
                    StatCode = (int)ICSICodes.Invalid_JSON_Request;
                    StatMessage = Convert.ToString(ICSICodes.Invalid_JSON_Request);
                }
                if (sqlConnection != null)
                {
                    try { sqlConnection.Close(); } catch (Exception e) { }
                    sqlConnection = null;
                }
            }
            else
            {
                // TODO - req object is null
                StatCode = (int)ICSICodes.Invalid_JSON_Request;
                StatMessage = Convert.ToString(ICSICodes.Invalid_JSON_Request);
            }
            return BuildResponse(StatCode.ToString(), StatMessage, StatData);
        }

        public static string getQuery(IDbConnection con, bool isReport, int id)
        {
            string sql = "select * from " + (isReport ? "REPORT_COMP_T" : "SCREEN_COMP_T") + " where ID=" + id + " and ACTIVE_YN=1";
            SqlCommand command = CreateCommand(sql, con);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    sql = Convert.ToString(reader["SQL_TX"]);
                }
                else sql = "";
            }
            else sql = "";
            try { reader.Close(); } catch (Exception e) { }
            reader = null;
            command = null;
            return sql;
        }

        public static string getQueryfromQuery_T(IDbConnection con, int id)
        {
            string sql = "select * from QUERY_T where ID=" + id + " and ACTIVE_YN=1";
            SqlCommand command = CreateCommand(sql, con);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    sql = Convert.ToString(reader["QRY_TX"]);
                }
                else sql = "";
            }
            else sql = "";
            try { reader.Close(); } catch (Exception e) { }
            reader = null;
            command = null;
            return sql;
        }


        public static List<Dictionary<string, object>> GetSearchResult(IDbConnection con, string query, Dictionary<string, object> conditions, int start, int end, bool isCountReqd=false)
        {
            if (start == 0 && end == 0)
            {
                start = 0;
                end = 100;
            }
            Dictionary<string, object> manualSearchConds = new Dictionary<string, object>();
            StringBuilder sql = new StringBuilder(" ");
            if (conditions != null)
            {
                foreach (var sk in conditions)
                {
                    if (sk.Key.StartsWith("SCRH_") && sk.Value != null && !(Convert.ToString(sk.Value)).Trim().Equals(""))
                    {
                        string key = string.Empty;
                        if (sk.Key.Contains("MONTH") || sk.Key.Contains("DATE"))
                        {
                            if (sk.Key.Contains("CONVERT(DATE")) key = sk.Key.Replace("CONVERT(DATE,", "").Replace(",101)", "");
                            else if (sk.Key.Contains("MONTH")) key = sk.Key.Replace("MONTH(", "").Replace(")", "");
                            else key = sk.Key;
                            key = key.ToString().Replace("SCRH_", "");
                        }
                        else
                        {
                            key = sk.Key.ToString().Replace("SCRH_", "");
                        }
                        string cond = "AND =";
                        if (conditions.ContainsKey("COND_" + key))
                        {
                            cond = (string)conditions["COND_" + key];
                        }
                        if (cond.IndexOf(",") != -1)
                        {
                            string[] scrhs = Convert.ToString(sk.Value).Split(",");
                            String[] cnds = cond.Split(",");
                            for (int ii = 0; ii < cnds.Length; ii++)
                            {
                                if (scrhs != null && !scrhs[ii].Trim().Equals(""))
                                {
                                    String[] conds = cnds[ii].Split(" ");
                                    string keycond = key.Replace(".", "");
                                    string keyparam = string.Empty;
                                    if (sk.Key.Contains("MONTH") || sk.Key.Contains("DATE"))
                                    {
                                        keyparam = sk.Key.Replace("SCRH_", "");
                                    }
                                    else
                                    {
                                        keyparam = key.Replace("DT_TO", "DT");
                                    }
                                    if (conds[1].Length <= 2) sql.Append(" ").Append(conds[0].ToString()).Append(" ").Append(keyparam).Append(" ").Append(conds[1]).Append(" @").Append(keycond).Append(ii);
                                    else
                                    {
                                        sql.Append(" ").Append(conds[0].ToString()).Append(" ").Append(conds[1].ToString()).Append(" ");
                                    }
                                }
                            }
                        }
                        else
                        {
                            String[] conds = cond.Split(" ");
                            string keycond = string.Empty;
                            if (sk.Key.Contains("MONTH") || sk.Key.Contains("DATE"))
                            {
                                if (sk.Key.Contains("CONVERT(DATE"))
                                    keycond = key.Replace("CONVERT(DATE,", "").Replace(",101)", "").Replace(".", "");
                                else if (sk.Key.Contains("MONTH"))
                                    keycond = key.Replace("MONTH(", "").Replace(")", "").Replace(".", "");
                            }
                            else
                            {
                                keycond = key.Replace(".", "");
                            }

                            string keyparam = sk.Key.Replace("DT_TO", "DT").Replace("SCRH_", "");
                            if (conds[1].Length <= 2) sql.Append(" ").Append(conds[0].ToString()).Append(" ").Append(keyparam).Append(" ").Append(conds[1]).Append(" @").Append(keycond);
                            else
                            {
                                sql.Append(" ").Append(conds[0].ToString()).Append(" ").Append(conds[1].ToString()).Append(" ");
                            }
                        }
                    }
                    else if (sk.Key.StartsWith("REPLACE_") && sk.Value != null)
                    {
                        query = query.Replace(Convert.ToString(sk.Key.Replace("REPLACE_", "")), Convert.ToString(sk.Value));
                    }
                    else if (sk.Key.StartsWith("MANSCR_") && sk.Value != null)
                    {
                        manualSearchConds.Add(sk.Key, sk.Value);
                    }
                }
            }
            query = query.Replace("@SQL", sql.ToString());
            sql = null;
            sql = new StringBuilder(query);
            List<Dictionary<string, object>> b = null;
            if (isCountReqd)
            {
                try
                {
                    if (manualSearchConds.Count > 0)
                    {
                        string qry = "SELECT * FROM (" + sql.ToString() + ") as SRCHBBB WHERE 1=1 ";
                        foreach (var m in manualSearchConds)
                        {
                            qry = qry + " AND SRCHBBB." + m.Key.Replace("MANSCR_", "") + " = '" + m.Value.ToString() + "'";
                        }
                        b = executeSearchQuery(con, "SELECT COUNT(*) as SRCHCOUNT FROM (" + qry + ") as SRCHBB", conditions, true);
                    }
                    else
                        b = executeSearchQuery(con, "SELECT COUNT(*) as SRCHCOUNT FROM (" + sql.ToString() + ") as SRCHBB", conditions, true);
                }
                catch(Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            List<Dictionary<string, object>> e = null;
            try
            //if (!sql.ToString().Contains("GROUP BY"))
            {
                e = executeSearchQuery(con, sql.ToString() + " OFFSET " + start + " ROWS FETCH NEXT " + end + " ROWS ONLY", conditions);
                //sql.Append(" OFFSET ").Append(start).Append(" ROWS FETCH NEXT ").Append(end).Append(" ROWS ONLY");
            }
            catch (Exception ex)
            {
                e = executeSearchQuery(con, sql.ToString(), conditions);
            }
            if (b != null && b.Count > 0)
            {
                e.ForEach(item => b.Add(item));
                return b;
            }
            else return e;
        }

        private static string getQueryWithoutOrderBy(string query)
        {            
            if (query.ToUpper().IndexOf("ORDER BY") != -1)
            {
                string retQuery = string.Empty;
                int pos = query.ToUpper().IndexOf("ORDER BY ");
                retQuery = query.Substring(0, pos);
                query = query.Substring(pos + 9);
                char[] cs = query.ToCharArray();
                int counter = 0;
                bool isStarted = false;
                bool isExtraCheck = false;
                foreach (char c in cs)
                {
                    if (!isExtraCheck && !isStarted && c != ' ') isStarted = true;
                    else if (!isExtraCheck && isStarted && c == ' ') isExtraCheck = true;
                    else if (isStarted && c == ',')
                    {
                        isStarted = false;
                        isExtraCheck = false;
                    }
                    else if (isExtraCheck && isStarted && c != ',' && c != ' ') break;
                    ++counter;
                }
                query = query.Substring(counter);
                if (query.ToUpper().IndexOf("ASC") != -1)
                {
                    query = query.Trim().Substring(3);
                }
                else if (query.ToUpper().IndexOf("DESC") != -1)
                {
                    query = query.Trim().Substring(4);
                }
                retQuery = retQuery + " " + query;
                return retQuery;
            }
            else return query;            
        }

        private static List<Dictionary<string, object>> executeSearchQuery(IDbConnection con, string query, Dictionary<string, object> conditions,bool isCountReqd=false)
        {
            if (isCountReqd) query = getQueryWithoutOrderBy(query);
            SqlCommand command = CreateCommand(query, con);
            if (conditions != null)
            {
                foreach (var sk in conditions)
                {
                    if (sk.Key.StartsWith("SCRH_") && sk.Value != null)
                    {
                        string val = Convert.ToString(sk.Value);
                        if (val.IndexOf(",") != -1)
                        {
                            string[] vals = val.Split(",");
                            for (int ii = 0; vals != null && ii < vals.Length; ii++)
                            {
                                if (vals[ii] != null && !vals[ii].Trim().Equals(""))
                                {
                                    string keysqlparam =  string.Empty;
                                    if (sk.Key.Contains("MONTH") || sk.Key.Contains("DATE"))
                                    {
                                        if (sk.Key.Contains("CONVERT(DATE"))
                                            keysqlparam = sk.Key.Replace("CONVERT(DATE,", "").Replace(",101)", "").Replace(".", "");
                                        else if (sk.Key.Contains("MONTH"))
                                            keysqlparam = sk.Key.Replace("MONTH(", "").Replace(")", "").Replace(".", "");
                                        else keysqlparam = sk.Key.ToString().Replace(".", "");
                                    }
                                    else
                                    {
                                        keysqlparam = sk.Key.ToString().Replace(".", "");
                                    }
                                    keysqlparam = keysqlparam + ii;
                                    SqlParameter param = new SqlParameter("@" + keysqlparam.Replace("SCRH_", ""), vals[ii]);
                                    command.Parameters.Add(param);
                                }
                            }
                        }
                        else
                        {
                            string keysqlparam = string.Empty;
                            if (sk.Key.Contains("MONTH") || sk.Key.Contains("DATE"))
                            {
                                if (sk.Key.Contains("CONVERT(DATE"))
                                    keysqlparam = sk.Key.Replace("CONVERT(DATE,", "").Replace(",101)", "").Replace(".", "");
                                else if (sk.Key.Contains("MONTH"))
                                    keysqlparam = sk.Key.Replace("MONTH(", "").Replace(")", "").Replace(".", "");
                                else keysqlparam = sk.Key.ToString().Replace(".", "");
                            }
                            else
                            {
                                keysqlparam = sk.Key.ToString().Replace(".", "");
                            }
                            SqlParameter param = new SqlParameter("@" + keysqlparam.Replace("SCRH_", ""), sk.Value);
                            command.Parameters.Add(param);
                        }
                    }
                }
            }
            SqlDataReader reader = command.ExecuteReader();
            List<Dictionary<string, object>> e = Serialize(reader);
            reader.Close();
            reader = null;
            command = null;
            return e;
        }

        public static List<Dictionary<string, object>> GetSQLData(IDbConnection con, string sql,string origsql=null,string unqid=null)//string schemaName,
        {
            List<Dictionary<string, object>> e = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
               cmd = CreateCommand(sql, con);  
               reader = cmd.ExecuteReader();
                //cmd = CreateCommand(sql, con);
               e = Serialize(reader);
            }
            catch (Exception ex) {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (cmd != null) cmd = null;
                if (origsql != null && unqid != null) e = GetSQLDataWithUniqueId(con, origsql, unqid);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd = null;
                }
            }
            return e;
        }
        public static List<Dictionary<string, object>> GetSQLDataWithUniqueId(IDbConnection con, string sql, string unqid = null)//string schemaName,
        {
            List<Dictionary<string, object>> e = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = CreateCommand(sql, con);
                cmd.Parameters.Add(new SqlParameter("@ID", unqid));                
                reader = cmd.ExecuteReader();
                e = Serialize(reader);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd = null;
                }
            }
            return e;
        }

        public static List<Dictionary<string, object>> GetQueryResult(IDbConnection con, IDbConnection con2, int id, Dictionary<string, object> p,int start, int end)//string schemaName,
        {
            string query = null;
            try
            {
                SqlCommand cmd = CreateCommand("SELECT QRY_TX FROM QUERY_T WHERE ID="+id, con);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    query = Convert.ToString(reader["QRY_TX"]);
                }
                //cmd = CreateCommand(sql, con);
                //e = Serialize(reader);
                reader.Close();
                reader = null;
                cmd = null;
            }
            catch (Exception ex) { }
            if (query != null)
            {
                SqlDataReader reader = null;
                try
                {                    
                    StringBuilder sb = new StringBuilder(query);
                    if (!query.Contains("TOP"))
                    {
                        if (query.IndexOf("ORDER BY") == -1) sb.Append(" ORDER BY ID");
                        sb.Append(" OFFSET ").Append(start).Append(" ROWS FETCH NEXT ").Append(end).Append(" ROWS ONLY");
                    }
                    //string q = sb.ToString();

                    SqlCommand command = CreateCommand(sb.ToString(), con2);
                    if (p != null && p.Count > 0)
                    {
                        foreach (var k in p)
                        {
                            if (sb.ToString().IndexOf("@" + k.Key) != -1)
                            {
                                SqlParameter param = new SqlParameter("@" + k.Key, k.Value);
                                command.Parameters.Add(param);
                            }
                        }
                    }
                    reader = command.ExecuteReader();
                    List<Dictionary<string, object>> e = Serialize(reader);
                    reader.Close();
                    command = null;
                    return e;
                }
                catch(Exception ex)
                {
                    reader.Close();
                }
            }
            return null;
        }

        private static void GetParameters(SqlCommand SqlCmd, Hashtable SPParamList)
        {
            IDictionaryEnumerator myEnum = SPParamList.GetEnumerator();
            while (myEnum.MoveNext())
                SqlCmd.Parameters.AddWithValue("@" + myEnum.Key, myEnum.Value.ToString().Trim());
        }

        public static DataTable ExecuteSP(IDbConnection con, string spName, Hashtable ht)
        {
            SqlCommand cmd = CreateCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            GetParameters(cmd, ht);
            SqlDataAdapter oDa = new SqlDataAdapter();
            DataTable oDt = new DataTable();
            oDa.SelectCommand = cmd;
            try
            {
                //con.Open();
                oDa.Fill(oDt);
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                //if (con.State == ConnectionState.Open) con.Close();
            }
            return oDt;
        }

        public static string getMD5Hash(string input)
        {
            MD5CryptoServiceProvider oMD5 = new MD5CryptoServiceProvider();
            byte[] data = oMD5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string core_hmac_md5(string key, string data)
        {
            string KEYy = "";
            if (key.Length > data.Length)
            {
                int m = 0;
                for (int n = 0; n < key.Length; n++)
                {
                    KEYy += key[n];
                    if (m < data.Length) KEYy += data[m];
                    m++;
                }
            }
            else
            {
                int o = 0;
                for (int p = 0; p < data.Length; p++)
                {
                    if (o < key.Length) KEYy += key[o];
                    KEYy += data[p];
                    o++;
                }
            }
            return KEYy;
        }

        private static DataTable ToDataTable(List<Dictionary<string, object>> list)
        {
            if (list != null)
            {
                DataTable result = new DataTable();
                if (list.Count == 0)
                    return result;

                var columnNames = list.SelectMany(dict => dict.Keys).Distinct();
                result.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
                foreach (Dictionary<string, object> item in list)
                {
                    var row = result.NewRow();
                    foreach (var key in item.Keys)
                    {
                        row[key] = item[key];
                    }

                    result.Rows.Add(row);
                }

                return result;
            }
            else return null;
        }
        private static string MAIN_QRY = "Select UserPKID, UserType, RoleIsActive, IsActive , ResetPassword from [192.168.2.62].smash.dbo.PRD_USR_LOGINDETAIL  with (nolock) where UserID =";
        private static string PWD_QRY = "Select UserID,UserPassword from [192.168.2.62].smash.dbo.PRD_USR_LOGINDETAIL with (nolock) Where UserID =";
        private static string STUDENT_QRY = "select StudentHdrID from [192.168.2.62].smash.dbo.STU_STUDENT_HDR where UserFKID = ";
        private static string RESULT_SUDENT_QRY = ", UserPKID USER_PKID, UserID   LOGIN_ID, ResetPassword  IS_FIRST, RoleFKID   ROLE_FKID, RoleName   ROLE_NAME, UserTypeFKID  USER_TYPE_FKID,UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE,UserPassword USER_PWD, StudentUserId UserID,FirstName,MiddleName,LastName from [192.168.2.62].smash.dbo.ICSI_STD_LOGINVIEW where UserPKID =";
        private static string COMPANY_EXISTS_QRY = "select CompanyId from [192.168.2.62].smash.dbo.PRD_COMPANY_HDR where UserID = ";
        private static string COMPANY_QRY = ", UserPKID USER_PKID, UserID   LOGIN_ID, ResetPassword  IS_FIRST, RoleFKID   ROLE_FKID, RoleName   ROLE_NAME, UserTypeFKID  USER_TYPE_FKID,UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE, UserPassword USER_PWD, CompanyUserId UserID,CompanyName from [192.168.2.62].smash.dbo.ICSI_COMP_LOGINVIEW where UserPKID =";
        private static string APPLICATION_EXISTS_QRY = "select ApplicationReqID from [192.168.2.62].smash.[dbo].[CRC_APPLICATION_REQ] with (nolock) where UserFKID = ";
        private static string APPLICATION_QRY = ", UserPKID USER_PKID, UserID   LOGIN_ID, ResetPassword  IS_FIRST, RoleFKID   ROLE_FKID, RoleName   ROLE_NAME,UserTypeFKID  USER_TYPE_FKID, UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE, UserPassword USER_PWD, CounsellorId UserID,CounsellorName from [192.168.2.62].smash.dbo.ICSI_COUNSELLER_LOGINVIEW where UserPKID =";
        private static string EMPLOYEE_EXIST_QRY = "select EmployeeID from [192.168.2.62].smash.dbo.PRD_MST_EMPLOYEE where UserFKID= ";
        private static string EMP_CHECK1_QRY = "select OfficeFKID from [192.168.2.62].smash.dbo.PRD_USER_OFFICE_MAPPING with (nolock) where isactive=1 and userfkid=";
        private static string EMP_DATA_1_QRY = ",UserPKID   USER_PKID, UserID   LOGIN_ID, ResetPassword  IS_FIRST , RoleFKID   ROLE_FKID , RoleName   ROLE_NAME, UserTypeFKID  USER_TYPE_FKID , UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE, isnull(cast(EmpPKID as varchar),'') EMP_PKID, (EmployeeName + ' ' + '(' +isnull(DisplayRoleName,'')+ ')')    EMP_NAME, isnull(DesignationCode,'')   EMP_DESG_CODE, isnull(DesignationName,'')   EMP_DESG_NAME, isnull(cast(AgencyPKID as varchar),'')AGENCY_PKID, isnull(AgencyCode,'')     AGENCY_CODE , isnull(AgencyName,'')     AGENCY_NAME, isnull(AgencyTypeCode,'')    AGENCY_TYPE_CODE , isnull(AgencyTypeName,'')    AGENCY_TYPE_NAME , UserPassword       USER_PWD, Email as USER_EMAIL, City as USER_CITY, CONVERT(varchar(10),DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()), 0),103) as ROStartDate,OfficeFKID from [192.168.2.62].smash.dbo.ICSI_EMP_LOGINVIEW a with (nolock) join [192.168.2.62].smash.dbo.PRD_USER_OFFICE_MAPPING b with (nolock) on a.UserPKID=UserFKId and b.IsActive=1       where UserID =";
        private static string EMP_DATA_2_QRY = ",UserPKID   USER_PKID , UserID   LOGIN_ID, ResetPassword  IS_FIRST, RoleFKID   ROLE_FKID, RoleName   ROLE_NAME, UserTypeFKID  USER_TYPE_FKID, UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE, isnull(cast(EmpPKID as varchar),'') EMP_PKID, (EmployeeName + ' ' + '(' +isnull(RoleName,'') + ')')    EMP_NAME, isnull(DesignationCode,'')   EMP_DESG_CODE, isnull(DesignationName,'')   EMP_DESG_NAME, isnull(cast(AgencyPKID as varchar),'')AGENCY_PKID, isnull(AgencyCode,'')     AGENCY_CODE, isnull(AgencyName,'')     AGENCY_NAME, isnull(AgencyTypeCode,'')    AGENCY_TYPE_CODE, isnull(AgencyTypeName,'')    AGENCY_TYPE_NAME, UserPassword       USER_PWD, Email as USER_EMAIL, City as USER_CITY, CONVERT(varchar(10),DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()), 0),103) as ROStartDate from [192.168.2.62].smash.dbo.ICSI_EMP_LOGINVIEW  with (nolock) where UserID =";
        private static string EMP_DATA_3_QRY = ",UserPKID USER_PKID, UserID   LOGIN_ID, ResetPassword  IS_FIRST, RoleFKID   ROLE_FKID, RoleName   ROLE_NAME, UserTypeFKID  USER_TYPE_FKID,UserTypeCode  USER_TYPE_CODE, UserType   USER_TYPE,UserPassword USER_PWD from [192.168.2.62].smash.dbo.PRD_USR_LOGINDETAIL where UserPKID =";

        public static DataTable validateUser(IDbConnection con, string userId)
        {
            List<Dictionary<string, object>> finalData = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(MAIN_QRY).Append("'").Append(userId).Append("'");
            List<Dictionary<string, object>> l1 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
            if (l1 != null && l1.Count > 0)
            {
                Dictionary<string, object> d1 = l1[0];
                sql.Clear();
                sql.Append(PWD_QRY).Append("'").Append(userId).Append("'");
                l1 = null;
                l1 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                if (l1 != null && l1.Count > 0)
                {
                    int result = -1;
                    if (Convert.ToInt32(d1["RoleIsActive"]) == 1)
                    {
                        int isActive = Convert.ToInt32(d1["IsActive"]);
                        int isFirst = Convert.ToInt32("ResetPassword");
                        if (isActive == 1 && isFirst == 0)
                        {
                            result = 0;
                        }
                        else if (isActive == 1 && isFirst == 1)
                        {
                            result = 2;
                        }
                        else if (isActive == 0)
                        {
                            result = 3;
                        }
                        int userPkid = Convert.ToInt32(d1["UserPKID"]);
                        sql.Clear();
                        sql.Append(STUDENT_QRY).Append(userPkid).Append(" and ").Append(result).Append("=0");
                        List<Dictionary<string, object>> l2 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                        if (l2 != null && l2.Count > 0)
                        {
                            result = 8;
                            sql.Clear();
                            sql.Append("SELECT ").Append(result).Append(" as Result ").Append(RESULT_SUDENT_QRY).Append(userPkid);
                            finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                        }
                        else
                        {
                            sql.Clear();
                            sql.Append(COMPANY_EXISTS_QRY).Append(userPkid).Append(" and ").Append(result).Append("=0");
                            l2 = null;
                            l2 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                            if (l2 != null && l2.Count > 0)
                            {
                                result = 7;
                                sql.Clear();
                                sql.Append("SELECT ").Append(result).Append(" as Result ").Append(COMPANY_QRY).Append(userPkid);
                                finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                            }
                            else
                            {
                                sql.Clear();
                                sql.Append(APPLICATION_EXISTS_QRY).Append(userPkid).Append(" and ").Append(result).Append("=0");
                                l2 = null;
                                l2 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                if (l2 != null && l2.Count > 0)
                                {
                                    result = 9;
                                    sql.Clear();
                                    sql.Append("SELECT ").Append(result).Append(" as Result ").Append(APPLICATION_QRY).Append(userPkid);
                                    finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                }
                                else
                                {
                                    // main code
                                    sql.Clear();
                                    sql.Append(EMPLOYEE_EXIST_QRY).Append(userPkid).Append(" and ").Append(result).Append("=0");
                                    l2 = null;
                                    l2 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                    if (l2 != null && l2.Count > 0)
                                    {
                                        if (!userId.Equals("admin"))
                                        {
                                            sql.Clear();
                                            sql.Append(EMP_CHECK1_QRY).Append(userPkid);
                                            l2 = null;
                                            l2 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                            if (l2 != null && l2.Count > 0)
                                            {
                                                result = 5;
                                                finalData = new List<Dictionary<string, object>>();
                                                Dictionary<string, object> d2 = new Dictionary<string, object>();
                                                d2["Result"] = 5;
                                                finalData.Add(d2);
                                            }
                                            else
                                            {
                                                sql.Clear();
                                                sql.Append("SELECT ").Append(result).Append(" as Result ").Append(EMP_DATA_1_QRY).Append("'");
                                                sql.Append(userId).Append("'");
                                                finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                            }
                                        }
                                        if (finalData == null)
                                        {
                                            sql.Clear();
                                            sql.Append("SELECT ").Append(result).Append(" as Result ").Append(EMP_DATA_2_QRY).Append("'").Append(userId).Append("'");
                                            finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                        }
                                    }
                                    else
                                    {
                                        sql.Clear();
                                        sql.Append("SELECT ").Append(result).Append(" as Result ").Append(EMP_DATA_3_QRY).Append(userPkid);
                                        finalData = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(con, sql.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        finalData = new List<Dictionary<string, object>>();
                        Dictionary<string, object> d2 = new Dictionary<string, object>();
                        d2["Result"] = 6;
                        finalData.Add(d2);
                    }
                }
                else
                {
                    finalData = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d2 = new Dictionary<string, object>();
                    d2["Result"] = 1;
                    finalData.Add(d2);
                }
            }
            return ToDataTable(finalData);
        }

        public static void BulkInsertUpdate(IDbConnection con, string sessionKey, string schemaName, string tableName, List<Dictionary<string, object>> obj, int uid)
        {
            int iTmpDtlId = 0;
            try
            {
                if (obj != null && obj.Count > 0)
                {
                    Dictionary<string, object> dictData = obj[0];
                    List<Dictionary<string, object>> lstReconData = fromJson(dictData["0"].ToString());
                    Dictionary<string, object> dictReconData = lstReconData[0];
                    StringBuilder query = new StringBuilder();
                    bool isActiveYn = false;
                    bool b = true;
                    DataTable table = new DataTable();
                    if (tableName.Equals("PG_REC_TEMP_DTL_T"))
                    {
                        table.Columns.Add(new DataColumn("ID", typeof(int)));
                    }

                    foreach (var val in dictReconData)
                    {
                        if (val.Key.Equals("ID") || val.Key.Equals("REF_ID") || val.Key.Equals("CREATED_DT") || val.Key.Equals("CREATED_BY") || val.Key.Equals("UPDATED_DT") || val.Key.Equals("UPDATED_BY")) continue;
                        else if (val.Key.Equals("ACTIVE_YN")) isActiveYn = true;
                        if (b == true)
                        {
                            table.Columns.Add(new DataColumn(val.Key, typeof(string)));
                        }
                        else b = true;
                    }

                    if (!isActiveYn) table.Columns.Add(new DataColumn("ACTIVE_YN", typeof(bool)));
                    table.Columns.Add(new DataColumn("CREATED_DT", typeof(DateTime)));
                    table.Columns.Add(new DataColumn("CREATED_BY", typeof(int)));
                    table.Columns.Add(new DataColumn("UPDATED_DT", typeof(DateTime)));
                    table.Columns.Add(new DataColumn("UPDATED_BY", typeof(int)));
                    if (!tableName.Equals("PG_REC_TEMP_DTL_T"))
                    {
                        table.Columns.Add(new DataColumn("REF_ID", typeof(int)));
                    }
                    else
                    {
                        string qry = "SELECT IDENT_CURRENT('" + tableName + "') as LAST_INSERTED_ID";
                        SqlCommand cmd1 = CreateCommand(qry, con);
                        SqlDataReader reader = cmd1.ExecuteReader();
                        List<Dictionary<string, object>> e = Serialize(reader);
                        reader.Close();
                        reader = null;
                        cmd1 = null;
                        if (e != null && e.Count == 1)
                        {
                            var d = e[0];
                            if (d.ContainsKey("LAST_INSERTED_ID"))
                            {
                                string id = Convert.ToString(d["LAST_INSERTED_ID"]);
                                iTmpDtlId = Convert.ToInt32(id);
                            }
                        }
                    }
                    if (dictData.ContainsKey("IS_RECONCILE"))
                    {
                        dictData.Remove("IS_RECONCILE");
                    }
                    if (dictData.ContainsKey("SP_NAME"))
                    {
                        dictData.Remove("SP_NAME");
                    }
                    if (dictData.ContainsKey("REF_ID"))
                    {
                        dictData.Remove("REF_ID");
                    }
                    if (dictData.ContainsKey("PG_REC_ID"))
                    {
                        dictData.Remove("PG_REC_ID");
                    }


                    for (int i = 0; i < dictData.Count; i++)
                    {
                        lstReconData = fromJson(dictData[i.ToString()].ToString());

                        foreach (Dictionary<string, object> r in lstReconData)
                        {
                            DataRow row = table.NewRow();
                            for (int k = 0; k < table.Columns.Count; k++)
                            {
                                var t = table.Columns[k];

                                if (r.ContainsKey(t.ColumnName))
                                {
                                    row[t.ColumnName] = r[t.ColumnName];
                                }
                                else
                                {
                                    if (t.ColumnName.Equals("CREATED_BY") || t.ColumnName.Equals("UPDATED_BY"))
                                    {
                                        row[t.ColumnName] = 1;
                                    }
                                    else if (t.ColumnName.Equals("CREATED_DT") || t.ColumnName.Equals("UPDATED_DT"))
                                    {
                                        row[t.ColumnName] = DateTime.Now;
                                    }
                                    else if (t.ColumnName.Equals("ACTIVE_YN"))
                                    {
                                        row[t.ColumnName] = 1;
                                    }
                                    else if (t.ColumnName.Equals("ID"))
                                    {
                                        row[t.ColumnName] = iTmpDtlId + 1;
                                        iTmpDtlId = iTmpDtlId + 1;
                                    }
                                }
                            }
                            table.Rows.Add(row);
                        }
                    }

                    using (SqlBulkCopy copy = new SqlBulkCopy((SqlConnection)con))
                    {
                        copy.BulkCopyTimeout = 600;
                        copy.DestinationTableName = tableName;
                        copy.WriteToServerAsync(table);
                    }
                }
            }
            catch (Exception ex)
            {
                //return GetResponse(sessionKey,"-100", ex.Message, json);                
                throw ex;
            }
        }

    }
}
