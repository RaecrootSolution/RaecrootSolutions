using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace PwC.ICSI.DataAccessLayer 
{
    public class SqlHelper
    {
        #region Methods
        public static SqlConnection GetDataBaseConnection()
        {
            SqlConnection objConn = new SqlConnection();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();

            return objConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetDataBaseConnectionView()
        {
            SqlConnection objConn = new SqlConnection();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();

            return objConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetDataBaseConnectionForRegistration()
        {
            SqlConnection objConn = new SqlConnection();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();

            return objConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetMemberDB()
        {
            SqlConnection objConn = new SqlConnection();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();

            return objConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetPaymentDB()
        {
            SqlConnection objConn = new SqlConnection();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();

            return objConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool ExecuteResultPubSP1(string strSpName, params SqlParameter[] parameters)
        {
            //int returnCode = -1;

            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            SqlConnection objConnection = null;
            //SqlParameter paramReturnValue = new SqlParameter("@returnValue", SqlDbType.Int);
            //paramReturnValue.Direction = ParameterDirection.ReturnValue;

            try
            {
                objConnection = GetResultPubDB();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                //command.Parameters.Add(paramReturnValue);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                int i = command.ExecuteNonQuery();

                //returnCode = Convert.ToInt32(paramReturnValue.Value);

                //return ((returnCode == 0) ? true : false);
                return ((i == 1) ? true : false);
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetResultPubDB()
        {
            SqlConnection objConn = new SqlConnection();
            //objConn.ConnectionString = ConfigurationManager.ConnectionStrings["ResultPubDB"].ToString();
            objConn.ConnectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ToString();
            return objConn;
        }
        /// <summary>
        /// GetMemberDB
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetDataBaseConnection();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);
                command.CommandTimeout = 60000;
                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                dsList.Dispose();
            }
            catch (SqlException ex)
            {
               throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetView(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetDataBaseConnectionView();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);
                command.CommandTimeout = 60000;
                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                dsList.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetForRegistration(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetDataBaseConnectionForRegistration();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);
                command.CommandTimeout = 60000;
                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                dsList.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetMember(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetMemberDB();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                command.CommandTimeout = 10000;
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                dsList.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetResultPub(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetResultPubDB();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                //dsList.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetPayment(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            DataSet dsList = null;
            SqlConnection objConnection = null;

            try
            {
                objConnection = GetPaymentDB();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);
                objDataAdapter.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }

            return dsList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="objConnection"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static SqlCommand BuildSqlCommand(string strSpName, SqlConnection objConnection, params SqlParameter[] parameters)
        {
            SqlCommand command = new SqlCommand();
            command.Connection = objConnection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = strSpName;

            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput) && (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }

                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="blnSuccess"></param>
        /// <param name="parameters"></param>
        public static void ExecuteSP(string strSpName, out bool blnSuccess, params SqlParameter[] parameters)
        {
            int returnCode = -1;
            blnSuccess = false;

            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            SqlConnection objConnection = null;
            SqlParameter paramReturnValue = new SqlParameter("@returnValue", SqlDbType.Int);
            paramReturnValue.Direction = ParameterDirection.ReturnValue;

            try
            {
                objConnection = GetDataBaseConnection();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                command.Parameters.Add(paramReturnValue);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                command.ExecuteNonQuery();

                returnCode = Convert.ToInt32(paramReturnValue.Value);

                blnSuccess = ((returnCode == 0) ? true : false);
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool ExecuteResultPubSP(string strSpName, params SqlParameter[] parameters)
        {
            int returnCode = -1;
            
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");

            SqlConnection objConnection = null;
            SqlParameter paramReturnValue = new SqlParameter("@returnValue", SqlDbType.Int);
            paramReturnValue.Direction = ParameterDirection.ReturnValue;

            try
            {
                objConnection = GetResultPubDB();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                command.Parameters.Add(paramReturnValue);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                command.ExecuteNonQuery();

                returnCode = Convert.ToInt32(paramReturnValue.Value);

                return ((returnCode == 0) ? true : false);
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strParamName"></param>
        /// <param name="paramValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlParameter BuildSqlParameter<T>(string strParamName, T paramValue, SqlDbType type)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.SqlDbType = type;
            parameter.Value = paramValue;
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = strParamName;

            return parameter;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strParamName"></param>
        /// <param name="paramValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlParameter BuildOutputSqlParameter<T>(string strParamName, T paramValue, SqlDbType type)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.SqlDbType = type;
            parameter.Value = paramValue;
            parameter.Direction = ParameterDirection.Output;
            parameter.ParameterName = strParamName;

            return parameter;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSpName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetStudent1(string strSpName, params SqlParameter[] parameters)
        {
            if (String.IsNullOrEmpty(strSpName))
                throw new ArgumentNullException("Please pass the Stored procedure name");
            DataSet dsList = null;
            SqlConnection objConnection = null;
            try
            {
                objConnection = GetDataBaseConnection();
                SqlCommand command = BuildSqlCommand(strSpName, objConnection, parameters);
                SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);

                if (objConnection.State == ConnectionState.Closed)
                    objConnection.Open();

                dsList = new DataSet();
                objDataAdapter.Fill(dsList);

                command.Dispose();
                objDataAdapter.Dispose();
                dsList.Dispose();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (objConnection != null && objConnection.State == ConnectionState.Open)
                    objConnection.Dispose();
            }
            return dsList;
        }
        #endregion
    }
}
