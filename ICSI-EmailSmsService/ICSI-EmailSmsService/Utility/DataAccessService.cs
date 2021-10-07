using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ICSI_EmailSmsService.Utility
{
    public class DataAccessService
    {
        public SqlConnection GetConnection(string connectionName)
        {
            string cnstr = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            SqlConnection cn = new SqlConnection(cnstr);
            cn.Open();
            return cn;
        }

        public DataTable ExecuteQuery(string connectionName, string storedProcName, Dictionary<string, SqlParameter> procParameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = GetConnection(connectionName))
            {
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcName;
                    // assign parameters passed in to the command
                    foreach (var procParameter in procParameters)
                    {
                        cmd.Parameters.Add(procParameter.Value);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public int ExecuteCommand(string connectionName, string storedProcName, Dictionary<string, SqlParameter> procParameters)
        {
            int rc;
            using (SqlConnection cn = GetConnection(connectionName))
            {
                // create a SQL command to execute the stored procedure
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcName;
                    // assign parameters passed in to the command
                    foreach (var procParameter in procParameters)
                    {
                        cmd.Parameters.Add(procParameter.Value);
                    }
                    rc = cmd.ExecuteNonQuery();
                }
            }
            return rc;
        }

        public DataTable ExecuteInlineQuery(string connectionName, string queryString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = GetConnection(connectionName))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                //connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                        dt.Load(reader);
                }
                finally
                {
                    // Always call Close when done reading.
                    //reader.Close();
                }
            }
            return dt;
        }

        public int ExecuteDMLInlineQuery(string connectionName, string queryString)
        {
            int msg = 0;
            using (SqlConnection connection = GetConnection(connectionName))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    //connection.Open();
                    msg = command.ExecuteNonQuery();
                }
            }
            return msg;
        }
    }
}