using ICSI.PG.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PaymentGateway.Util
{
    public class UtilService
    {
        public static SqlCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            return command;
        }

        public static int GetLastInsertedId(string tableName,IDbConnection con)
        {
            int returnValue = 0;
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
            return returnValue;
        }

        public static List<Dictionary<string, object>> GetData(string sql, IDbConnection con)
        {            
            SqlCommand command = null;
            SqlDataReader reader = null;
            List<Dictionary<string, object>> e = null;
            try
            {
                command = CreateCommand(sql, con);
                reader = command.ExecuteReader();
                e= Serialize(reader);
            }catch(Exception ex)
            {

            }
            finally
            {
                reader.Close();
                reader = null;              
                command = null;
                //con.Close();
                //con = null;
            }
            
            return e;
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
    }
}