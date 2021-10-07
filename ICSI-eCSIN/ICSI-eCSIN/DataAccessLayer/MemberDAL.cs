using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Xml.Serialization;
//using System.Data.OracleClient;

namespace PwC.ICSI.DataAccessLayer
{
    public class MemberDAL
    {
        // New Method for HTML letters --Anil-Filix

        public DataSet GetMemberDetails(String RegNo, String reportname,string Pre)
        {
            string procName = "";
            try
            {
                if (reportname == "IssueCertificatePractice.rpt")
                {
                    procName = "getCopDetails";
                }
                else if (reportname == "LetterCPRenewalOnline.rpt")
                {
                    procName = "CopRenewDetails";
                }
                else if (reportname == "ACSLetter.rpt")
                {
                    procName = "GetAcsDetails";
                }
                else{
                    procName = "GetFcsDetails";
                }
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@memNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@Pre", Pre, SqlDbType.VarChar);
                //***********UNCOMMENT BY TBIPL***********//
                //sparams[1] = SqlHelper.BuildSqlParameter("@PREMEMBNO", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember(procName, sparams);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
      
        public bool InsertPaymentDetails(DataTable StudentPayment)
        {
            DataSet objdsPayment;
            string xmlPayment = string.Empty;
            bool blnSuccess = true;
            //int returnCode = 0;
            SqlCommand objCmd;
            SqlConnection objConn =SqlHelper.GetDataBaseConnection();

            try
            {
                objCmd = new SqlCommand("uspInsertStudentPaymentDetails", objConn);

                //Opening connection
                objConn.Open();

                objdsPayment = new DataSet("StudentPayment");
                objdsPayment.Tables.Add(StudentPayment);
                xmlPayment = objdsPayment.GetXml();
                
                //Adding oledb parameters
                objCmd.Parameters.Add(new SqlParameter("@StudentPayment", OleDbType.LongVarChar));
                objCmd.Parameters["@StudentPayment"].Value = xmlPayment;

                //Executing Stored Procedure
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //ex.Message
                blnSuccess = false;
            }
            finally
            {
                try
                {
                    if (objConn != null)
                    {
                        //Closing connection
                        objConn.Close();
                    }
                }
                catch (Exception exp)
                {
                }
            }
            return blnSuccess;                
        }

        public DataSet GetAddressDetails(string RegNo, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetAddressDetail", sparams);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataSet UpdateAddress(string[] addressdetails)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[14];
                sparams[0] = SqlHelper.BuildSqlParameter("@Address1", addressdetails[0], SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@Address2", addressdetails[1], SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@Address3", addressdetails[2], SqlDbType.VarChar);
                sparams[3] = SqlHelper.BuildSqlParameter("@City", addressdetails[3], SqlDbType.VarChar);
                sparams[4] = SqlHelper.BuildSqlParameter("@State", addressdetails[4], SqlDbType.VarChar);
                sparams[5] = SqlHelper.BuildSqlParameter("@Pin", Convert.ToInt64(addressdetails[5]), SqlDbType.Float);
                sparams[6] = SqlHelper.BuildSqlParameter("@Email", addressdetails[6], SqlDbType.VarChar);
                sparams[7] = SqlHelper.BuildSqlParameter("@RegNo", addressdetails[7], SqlDbType.VarChar);
                sparams[8] = SqlHelper.BuildSqlParameter("@Request", addressdetails[8], SqlDbType.VarChar);
                sparams[9] = SqlHelper.BuildSqlParameter("@AddressType", addressdetails[9], SqlDbType.VarChar);
                sparams[10] = SqlHelper.BuildSqlParameter("@CorrAddr", addressdetails[10], SqlDbType.VarChar);
                sparams[11] = SqlHelper.BuildSqlParameter("@PreMembNo", addressdetails[11], SqlDbType.VarChar);
                sparams[12] = SqlHelper.BuildSqlParameter("@Mobile", addressdetails[12], SqlDbType.VarChar);
                sparams[13] = SqlHelper.BuildSqlParameter("@Phone", addressdetails[13], SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("UpdateAddress", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet QueryRequest(string RegNo, string PreMembNo, string request, string User)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[4];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@Request", request, SqlDbType.VarChar);
                sparams[3] = SqlHelper.BuildSqlParameter("@CreatedBy", User, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("QueryRequest", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetCourseDetails(string RegNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[1];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetCourseDetails", sparams);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataSet CheckOnlineMembAddRegion(string chapcd, string regionnm)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@chapcd", chapcd, SqlDbType.VarChar);

                sparams[1] = SqlHelper.BuildSqlParameter("@regionnm", regionnm, SqlDbType.VarChar);

                dsUserInfo = SqlHelper.ExecuteDataSetMember("CheckOnlineMembAddRegion", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }


        public DataSet GetMonthYear()
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[1];
                sparams[0] = SqlHelper.BuildSqlParameter("@Dummy", 1, SqlDbType.Int);
                return SqlHelper.ExecuteDataSetMember("GetMonthYear", sparams);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataSet GetRequestNo(string RegNo, string request)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@Request", request, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetRequestNo", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetMemberInfo(string RegNo, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetMemberInfo", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetData(string RegNo, string type)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", type, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetMemberData", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        public string OracleConnection()
        {
            return String.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.2.49)" + "(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=VIS)));User Id=icsirx;Password=icsirx123;");
        }

        //public string InsertOracle(string[] OracleValues)
        //{
        //    try
        //    {
        //        string AckNo = "";
        //        string connectionString = OracleConnection();
        //        //using (OracleConnection connection = new OracleConnection())
        //        //{
        //        //    connection.ConnectionString = connectionString;
        //        //    connection.Open();

        //        //    OracleCommand ora = new OracleCommand("ICSI_PG_TRANSACTION_ENTRY", connection);
        //        //    ora.CommandType = CommandType.StoredProcedure;

        //        //    OracleParameter[] param = new OracleParameter[8];

        //        //    param[0] = new OracleParameter("P_FEE_ID", OracleType.Number);
        //        //    param[0].Direction = ParameterDirection.Input;
        //        //    param[0].Value = OracleValues[1];
        //        //    ora.Parameters.Add(param[0]);

        //        //    param[1] = new OracleParameter("P_PAYER_MST_ID", OracleType.Number);
        //        //    param[1].Direction = ParameterDirection.Input;
        //        //    param[1].Value = OracleValues[2];
        //        //    ora.Parameters.Add(param[1]);

        //        //    param[2] = new OracleParameter("P_PAYER_CATEGORY", OracleType.VarChar);
        //        //    param[2].Direction = ParameterDirection.Input;
        //        //    param[2].Value = "GENERAL";
        //        //    ora.Parameters.Add(param[2]);

        //        //    param[3] = new OracleParameter("P_PAYER_ID", OracleType.VarChar);
        //        //    param[3].Direction = ParameterDirection.Input;
        //        //    param[3].Value = OracleValues[3];
        //        //    ora.Parameters.Add(param[3]);

        //        //    param[4] = new OracleParameter("P_PAYER_NAME", OracleType.VarChar);
        //        //    param[4].Direction = ParameterDirection.Input;
        //        //    param[4].Value = OracleValues[4];
        //        //    ora.Parameters.Add(param[4]);

        //        //    param[5] = new OracleParameter("P_TOTAL_FEES", OracleType.Number);
        //        //    param[5].Direction = ParameterDirection.Input;
        //        //    param[5].Value = OracleValues[5];
        //        //    ora.Parameters.Add(param[5]);

        //        //    param[6] = new OracleParameter("P_COMMENTS", OracleType.VarChar);
        //        //    param[6].Direction = ParameterDirection.Input;
        //        //    param[6].Value = OracleValues[8];
        //        //    ora.Parameters.Add(param[6]);

        //        //    param[7] = new OracleParameter("X_ACK_NO", OracleType.Number);
        //        //    param[7].Direction = ParameterDirection.Output;
        //        //    //param[7].Value = AckNo;
        //        //    ora.Parameters.Add(param[7]);

        //        //    ora.ExecuteNonQuery();
        //        //    AckNo = ora.Parameters["X_ACK_NO"].Value.ToString();
        //        //    connection.Close();

        //        //    connection.Dispose();
        //        //    ora.Dispose();

        //        //    return AckNo;
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";
        //    }
        //}

        public DataSet ChangePassword(string[] paramvalues)
        {
            try
            {
                string OldPass = EncryptPassword(paramvalues[2]);
                string EncPass = EncryptPassword(paramvalues[1]);

                SqlParameter[] sparams = new SqlParameter[4];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", paramvalues[0], SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@Password", EncPass, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@OldPassword", OldPass, SqlDbType.VarChar);
                sparams[3] = SqlHelper.BuildSqlParameter("@PreMembNo", paramvalues[3], SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("ChangePassword", sparams);
            }
            catch (Exception ex)
            { return null; }
        }

        private string EncryptPassword(string Password)
        {
            string EncPass = "";

            for (int i = 0; i < Password.Length; i++)
            {
                char chr = Convert.ToChar(Password.Substring(i, 1));
                chr++;
                EncPass += chr.ToString();
            }

            return EncPass;
        }

        private string DecryptPassword(string Password)
        {
            string DecPass = "";

            for (int i = 0; i < Password.Length; i++)
            {
                char chr = Convert.ToChar(Password.Substring(i, 1));
                chr--;
                DecPass += chr.ToString();
            }

            return DecPass;
        }

        public void InsertAuditRecord(string RegNo, string AckNo, string request, string User, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[5];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@Request", request, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@AckNo", AckNo, SqlDbType.VarChar);
                sparams[3] = SqlHelper.BuildSqlParameter("@CreatedBy", User, SqlDbType.VarChar);
                sparams[4] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                DataSet ds = SqlHelper.ExecuteDataSetMember("InsertAuditRecord", sparams);
            }
            catch (Exception ex)
            { }
        }

        public DataSet GetMemberLoginData(string RegNo, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetMemberLoginData", sparams);
            }
            catch (Exception ex)
            { return null; }
        }


        public DataSet GetMemberOnlineAddDetails(string UserID, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);

                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                dsUserInfo = SqlHelper.ExecuteDataSetMember("GetMemberOnlineAddDetails", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

        
        public DataSet GetCreditCertNo(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetCertificateNo", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetFirstBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFirstBlockYear", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetSecondBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetSecondBlockYear", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetThirdBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                //else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetThirdBlockYeartest", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetThirdBlockYeartest_sudhakar", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }
        public DataSet GetMemberInfo1()
        {
            try
            {

                return SqlHelper.ExecuteDataSetMember("GetMemberInfo1");
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet GetFourBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                //else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFourBlockYeartest", sparams);
                 //else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFourBlockYearTEST_Update", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFourBlockYearTEST_sudhakar", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }
        public DataSet GetFifthBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                //else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFifthBlockYear", sparams);
               // else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetFifthBlockYear_sudhakar", sparams);
		 else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetBLOCKYEAR20142017", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

	public DataSet GetsixBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            try
            {
                DataSet dsUserInfo = new DataSet();
                SqlParameter[] sparams = new SqlParameter[3];
                sparams[0] = SqlHelper.BuildSqlParameter("@UserID", UserID, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@MemberType", MemberType, SqlDbType.VarChar);
                sparams[2] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);

                if (MemberType == "S")
                    dsUserInfo = SqlHelper.ExecuteDataSet("uspUserValidate", sparams);
                //else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetsixBlockYear", sparams);
                else dsUserInfo = SqlHelper.ExecuteDataSetMember("GetsixthBlockYear_sudhakar", sparams);

                return dsUserInfo;
            }
            catch (Exception ex)
            { return null; }
        }

        public DataSet AddMemberInfo_NC(string regno, string premembno, int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, string checkin, string checkout, int addp, int chil)
        {
            SqlParameter[] param = new SqlParameter[35];
            try
            {

                param[0] = SqlHelper.BuildSqlParameter("@Regno", regno, SqlDbType.VarChar);
                param[1] = SqlHelper.BuildSqlParameter("@PreMembNo", premembno, SqlDbType.VarChar);
                param[2] = SqlHelper.BuildSqlParameter("@fees", fees, SqlDbType.Int);
                param[3] = SqlHelper.BuildSqlParameter("@hotelname", hotelname, SqlDbType.VarChar);
                param[4] = SqlHelper.BuildSqlParameter("@hotelamt", hotelamt, SqlDbType.Int);
                param[5] = SqlHelper.BuildSqlParameter("@total", total, SqlDbType.BigInt);
                param[6] = SqlHelper.BuildSqlParameter("@OthHotel", other, SqlDbType.VarChar);
                param[7] = SqlHelper.BuildSqlParameter("@occup", occup, SqlDbType.VarChar);
                param[8] = SqlHelper.BuildSqlParameter("@spouse", spouse, SqlDbType.VarChar);
                param[9] = SqlHelper.BuildSqlParameter("@spouseveg", sveg, SqlDbType.Int);
                param[10] = SqlHelper.BuildSqlParameter("@secdel", secdel, SqlDbType.VarChar);
                param[11] = SqlHelper.BuildSqlParameter("@secveg", secveg, SqlDbType.Int);
                param[12] = SqlHelper.BuildSqlParameter("@thirdel", thirdel, SqlDbType.VarChar);
                param[13] = SqlHelper.BuildSqlParameter("@thirveg", thirveg, SqlDbType.Int);
                param[14] = SqlHelper.BuildSqlParameter("@mobile", mobile, SqlDbType.VarChar);
                param[15] = SqlHelper.BuildSqlParameter("@add1", add1, SqlDbType.VarChar);
                param[16] = SqlHelper.BuildSqlParameter("@add2", add2, SqlDbType.VarChar);
                param[17] = SqlHelper.BuildSqlParameter("@add3", add3, SqlDbType.VarChar);
                param[18] = SqlHelper.BuildSqlParameter("@city", city, SqlDbType.VarChar);
                param[19] = SqlHelper.BuildSqlParameter("@state", state, SqlDbType.VarChar);
                param[20] = SqlHelper.BuildSqlParameter("@pin", pin, SqlDbType.VarChar);
                param[21] = SqlHelper.BuildSqlParameter("@phone", phone, SqlDbType.VarChar);
                param[22] = SqlHelper.BuildSqlParameter("@email", email, SqlDbType.VarChar);
                param[23] = SqlHelper.BuildSqlParameter("@fax", fax, SqlDbType.VarChar);
                param[24] = SqlHelper.BuildSqlParameter("@mb", mem, SqlDbType.Int);
                param[25] = SqlHelper.BuildSqlParameter("@cp", cp, SqlDbType.Int);
                param[26] = SqlHelper.BuildSqlParameter("@sc", sc, SqlDbType.Int);
                param[27] = SqlHelper.BuildSqlParameter("@spou", spou, SqlDbType.Int);
                param[28] = SqlHelper.BuildSqlParameter("@spouamt", spouamt, SqlDbType.Int);
                param[29] = SqlHelper.BuildSqlParameter("@hcode", hcode, SqlDbType.Int);
                param[30] = SqlHelper.BuildSqlParameter("@checkin", checkin, SqlDbType.VarChar);
                param[31] = SqlHelper.BuildSqlParameter("@checkout", checkout, SqlDbType.VarChar);
                param[32] = SqlHelper.BuildSqlParameter("@addp", addp, SqlDbType.Int);
                param[33] = SqlHelper.BuildSqlParameter("@child", chil, SqlDbType.Int);
                param[34] = SqlHelper.BuildSqlParameter("@statusmsg", "", SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetPayment("MemberDetail_NC_I", param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;

        }




        public DataSet AddOtherMemberInfo_NC(string regno, int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string fname, string lname, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, string checkin, string checkout)
        {
            SqlParameter[] param = new SqlParameter[34];
            try
            {

                param[0] = SqlHelper.BuildSqlParameter("@RegNo", regno, SqlDbType.VarChar);
                param[1] = SqlHelper.BuildSqlParameter("@fees", fees, SqlDbType.Int);
                param[2] = SqlHelper.BuildSqlParameter("@hotelname", hotelname, SqlDbType.VarChar);
                param[3] = SqlHelper.BuildSqlParameter("@hotelamt", hotelamt, SqlDbType.Int);
                param[4] = SqlHelper.BuildSqlParameter("@total", total, SqlDbType.Int);
                param[5] = SqlHelper.BuildSqlParameter("@OthHotel", other, SqlDbType.VarChar);
                param[6] = SqlHelper.BuildSqlParameter("@occup", occup, SqlDbType.VarChar);
                param[7] = SqlHelper.BuildSqlParameter("@spouse", spouse, SqlDbType.VarChar);
                param[8] = SqlHelper.BuildSqlParameter("@spouseveg", sveg, SqlDbType.Int);
                param[9] = SqlHelper.BuildSqlParameter("@secdel", secdel, SqlDbType.VarChar);
                param[10] = SqlHelper.BuildSqlParameter("@secveg", secveg, SqlDbType.Int);
                param[11] = SqlHelper.BuildSqlParameter("@thirdel", thirdel, SqlDbType.VarChar);
                param[12] = SqlHelper.BuildSqlParameter("@thirveg", thirveg, SqlDbType.Int);
                param[13] = SqlHelper.BuildSqlParameter("@fname", fname, SqlDbType.VarChar);
                param[14] = SqlHelper.BuildSqlParameter("@lname", lname, SqlDbType.VarChar);
                param[15] = SqlHelper.BuildSqlParameter("@mobile", mobile, SqlDbType.VarChar);
                param[16] = SqlHelper.BuildSqlParameter("@add1", add1, SqlDbType.VarChar);
                param[17] = SqlHelper.BuildSqlParameter("@add2", add2, SqlDbType.VarChar);
                param[18] = SqlHelper.BuildSqlParameter("@add3", add3, SqlDbType.VarChar);
                param[19] = SqlHelper.BuildSqlParameter("@city", city, SqlDbType.VarChar);
                param[20] = SqlHelper.BuildSqlParameter("@state", state, SqlDbType.VarChar);
                param[21] = SqlHelper.BuildSqlParameter("@pin", pin, SqlDbType.VarChar);
                param[22] = SqlHelper.BuildSqlParameter("@phone", phone, SqlDbType.VarChar);
                param[23] = SqlHelper.BuildSqlParameter("@email", email, SqlDbType.VarChar);
                param[24] = SqlHelper.BuildSqlParameter("@fax", fax, SqlDbType.VarChar);
                param[25] = SqlHelper.BuildSqlParameter("@mb", mem, SqlDbType.Int);
                param[26] = SqlHelper.BuildSqlParameter("@cp", cp, SqlDbType.Int);
                param[27] = SqlHelper.BuildSqlParameter("@sc", sc, SqlDbType.Int);
                param[28] = SqlHelper.BuildSqlParameter("@spou", spou, SqlDbType.Int);
                param[29] = SqlHelper.BuildSqlParameter("@spouamt", spouamt, SqlDbType.Int);
                param[30] = SqlHelper.BuildSqlParameter("@hcode", hcode, SqlDbType.Int);
                param[31] = SqlHelper.BuildSqlParameter("@checkin", checkin, SqlDbType.VarChar);
                param[32] = SqlHelper.BuildSqlParameter("@checkout", checkout, SqlDbType.VarChar);
                param[33] = SqlHelper.BuildSqlParameter("@statusmsg", "", SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("OtherMemberDetail_NC_I", param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public DataSet AddNonMemberInfo_NC(int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string fname, string lname, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, int nomember, string checkin, string checkout)
        {
            SqlParameter[] param = new SqlParameter[34];
            try
            {

                param[0] = SqlHelper.BuildSqlParameter("@fees", fees, SqlDbType.Int);
                param[1] = SqlHelper.BuildSqlParameter("@hotelname", hotelname, SqlDbType.VarChar);
                param[2] = SqlHelper.BuildSqlParameter("@hotelamt", hotelamt, SqlDbType.Int);
                param[3] = SqlHelper.BuildSqlParameter("@total", total, SqlDbType.BigInt);
                param[4] = SqlHelper.BuildSqlParameter("@OthHotel", other, SqlDbType.VarChar);
                param[5] = SqlHelper.BuildSqlParameter("@occup", occup, SqlDbType.VarChar);
                param[6] = SqlHelper.BuildSqlParameter("@spouse", spouse, SqlDbType.VarChar);
                param[7] = SqlHelper.BuildSqlParameter("@spouseveg", sveg, SqlDbType.Int);
                param[8] = SqlHelper.BuildSqlParameter("@secdel", secdel, SqlDbType.VarChar);
                param[9] = SqlHelper.BuildSqlParameter("@secveg", secveg, SqlDbType.Int);
                param[10] = SqlHelper.BuildSqlParameter("@thirdel", thirdel, SqlDbType.VarChar);
                param[11] = SqlHelper.BuildSqlParameter("@thirveg", thirveg, SqlDbType.Int);
                param[12] = SqlHelper.BuildSqlParameter("@fname", fname, SqlDbType.VarChar);
                param[13] = SqlHelper.BuildSqlParameter("@lname", lname, SqlDbType.VarChar);
                param[14] = SqlHelper.BuildSqlParameter("@mobile", mobile, SqlDbType.VarChar);
                param[15] = SqlHelper.BuildSqlParameter("@add1", add1, SqlDbType.VarChar);
                param[16] = SqlHelper.BuildSqlParameter("@add2", add2, SqlDbType.VarChar);
                param[17] = SqlHelper.BuildSqlParameter("@add3", add3, SqlDbType.VarChar);
                param[18] = SqlHelper.BuildSqlParameter("@city", city, SqlDbType.VarChar);
                param[19] = SqlHelper.BuildSqlParameter("@state", state, SqlDbType.VarChar);
                param[20] = SqlHelper.BuildSqlParameter("@pin", pin, SqlDbType.VarChar);
                param[21] = SqlHelper.BuildSqlParameter("@phone", phone, SqlDbType.VarChar);
                param[22] = SqlHelper.BuildSqlParameter("@email", email, SqlDbType.VarChar);
                param[23] = SqlHelper.BuildSqlParameter("@fax", fax, SqlDbType.VarChar);
                param[24] = SqlHelper.BuildSqlParameter("@mb", mem, SqlDbType.Int);
                param[25] = SqlHelper.BuildSqlParameter("@cp", cp, SqlDbType.Int);
                param[26] = SqlHelper.BuildSqlParameter("@sc", sc, SqlDbType.Int);
                param[27] = SqlHelper.BuildSqlParameter("@spou", spou, SqlDbType.Int);
                param[28] = SqlHelper.BuildSqlParameter("@spouamt", spouamt, SqlDbType.Int);
                param[29] = SqlHelper.BuildSqlParameter("@hcode", hcode, SqlDbType.Int);
                param[30] = SqlHelper.BuildSqlParameter("@nonmember", nomember, SqlDbType.Int);
                param[31] = SqlHelper.BuildSqlParameter("@checkin", checkin, SqlDbType.VarChar);
                param[32] = SqlHelper.BuildSqlParameter("@checkout", checkout, SqlDbType.VarChar);
                param[33] = SqlHelper.BuildSqlParameter("@statusmsg", "", SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("NonMemberDetail_NC_I", param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;

        }


        public DataSet GetDay(string date1, string date2)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@sdate", date1, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@edate", date2, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetDay_NC", sparams);
            }
            catch (Exception ex)
            { return null; }
        }


        public DataSet GetMemberName(string RegNo, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("GetMemberInfo_NC", sparams);
            }
            catch (Exception ex)
            { return null; }
        }


        public DataSet CheckMemberInfo(string RegNo, string PreMembNo)
        {
            try
            {
                SqlParameter[] sparams = new SqlParameter[2];
                sparams[0] = SqlHelper.BuildSqlParameter("@RegNo", RegNo, SqlDbType.VarChar);
                sparams[1] = SqlHelper.BuildSqlParameter("@PreMembNo", PreMembNo, SqlDbType.VarChar);
                return SqlHelper.ExecuteDataSetMember("CheckMemberInfo_NC", sparams);
            }
            catch (Exception ex)
            { return null; }
        }


        public int InsertRequestDetails(string MembNo, string Request, bool isTaskGenerated,int id )
        {
           
                
                SqlParameter []param = new SqlParameter[4];
                param[0] = SqlHelper.BuildSqlParameter("@membno",MembNo,SqlDbType.VarChar);
                param[1] = SqlHelper.BuildSqlParameter("@request", Request, SqlDbType.VarChar);
                param[2] = SqlHelper.BuildSqlParameter("@isTaskGenerated", isTaskGenerated, SqlDbType.Bit);
                param[3] = SqlHelper.BuildSqlParameter("@id", id, SqlDbType.Int);

               DataSet ds =  SqlHelper.ExecuteDataSet("spRequestAudit", param);

               return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

        }


        }
    }
