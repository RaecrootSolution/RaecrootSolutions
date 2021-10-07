using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using ICSI.PG.DB;
using System.Runtime.CompilerServices;

namespace PaymentGateway
{
    public class Utils
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static DataTable GetDataFromProc(string ProcName,Hashtable htconditions,IDbConnection sqlConnection)
        {
            DataTable dt = new DataTable();
            dt = ExecuteSP(sqlConnection, ProcName, htconditions);
            return dt;
        }

        private static void GetParameters(SqlCommand SqlCmd, Hashtable SPParamList)
        {
            IDictionaryEnumerator myEnum = SPParamList.GetEnumerator();
            while (myEnum.MoveNext())
                SqlCmd.Parameters.AddWithValue("@" + myEnum.Key, myEnum.Value.ToString().Trim());
        }

        public static SqlCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            return command;
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
            catch
            {
                throw;
            }
            finally
            {
                //if (con.State == ConnectionState.Open) con.Close();
            }
            return oDt;
        }

    }
}