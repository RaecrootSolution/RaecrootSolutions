using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;
using System.Data;
using System.Data.SqlClient;

namespace PaymentGateway.DAL
{
    class PaymentDAL
    {
        public int ExecutePaymentDal(string connectionStringPg2, PaymentTransactionEntity objPmtEntity)
        {
            int ReqID = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionStringPg2))
                {
                    con.Open();
                    string query = "Insert into ONLINE_PAYMENT_DETAIL_T(PROCESS_ID,PROCESS_REQUEST_ID,PAYMENT_STATUS_ID,PAYMODE_ID,AMOUNT_NM,TOTALTAX_NM,PGCode_TX,CREATED_BY,CREATED_DT,ACTIVE_YN) Select 3, " + objPmtEntity.ProcessRequestID+", "+objPmtEntity.PaymentStatusID+", "+objPmtEntity.PaymentMode+", "+objPmtEntity.Amount+",0, '"+objPmtEntity.PGCode+"',1,getdate(),1";
                    //using (SqlCommand cmd = new SqlCommand("EXEC USPInsertPayment '" + objPmtEntity.ProcessRoute + "'," + objPmtEntity.ProcessRequestID + "," + objPmtEntity.PaymentStatusID + "," + objPmtEntity.PaymentMode + "," + objPmtEntity.Amount + ",0,'" + objPmtEntity.PGCode + "'", con))
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        string str = Convert.ToString(cmd.ExecuteNonQuery());                      
                    }
                    string Selectquery = "SELECT IDENT_CURRENT('ONLINE_PAYMENT_DETAIL_T') AS LAST_INSERTED_ID";
                    //using (SqlCommand cmd = new SqlCommand("EXEC USPInsertPayment '" + objPmtEntity.ProcessRoute + "'," + objPmtEntity.ProcessRequestID + "," + objPmtEntity.PaymentStatusID + "," + objPmtEntity.PaymentMode + "," + objPmtEntity.Amount + ",0,'" + objPmtEntity.PGCode + "'", con))
                    using (SqlCommand cmd = new SqlCommand(Selectquery, con))
                    {
                        string str = Convert.ToString(cmd.ExecuteScalar());
                         int.TryParse(str, out ReqID);
                    }
                }
            }
            catch
            {

            }

            return ReqID;
        }

        public int UpdatePaymentDal(string connectionStringPg2,string TxnID, DateTime TxnDate, string TxnCode, string TxnResponse, int RequestID, int paymentstatus,string paymode,double ST,double ProcFee,double TotAmt,double convfee)
        {
            int UpdID = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionStringPg2))
                {
                    con.Open();
                    string query = "UPDATE ONLINE_PAYMENT_DETAIL_T SET PAYMENT_STATUS_ID="+ paymentstatus + ",RESPONSE_CODE_TX='"+ TxnCode + "',RESPONSE_TRANSACTION_ID_TX='"+ TxnID + "',RESPONSE_DATA_TX='"+ TxnResponse + "',TOTAL_TAX_NM="+ST+",UPDATED_DT=GETDATE(),UPDATED_BY=1,RESPONSE_TRANSACTION_DT='"+ TxnDate.ToString("yyyy-MM-dd HH:mm:ss") + "',RESPONSE_PAYMODE_TX='"+paymode+"',TOTAL_AMOUNT_NM="+TotAmt+",PROC_FEE_NM="+ProcFee+",CONV_FEE_NM="+convfee+" WHERE ID="+ RequestID + "";
                    //using (SqlCommand cmd = new SqlCommand("EXEC USPUpdatePayment " + RequestID + ",'" + TxnCode + "','" + TxnID + "','" + TxnDate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + TxnResponse + "'," + paymentstatus + ",'"+paymode+"',"+ST+","+ProcFee+","+TotAmt+"", con))
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        string str = Convert.ToString(cmd.ExecuteNonQuery());
                        int.TryParse(str, out UpdID);
                    }
                }
            }
            catch
            {

            }

            return UpdID;
        }

        public int UpdatePaymentStatusDal(string connectionStringPg2, int paymentstatus, int RequestID)
        {
            int UpdID = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionStringPg2))
                {
                    con.Open();
                    string query = "UPDATE ONLINE_PAYMENT_DETAIL_T SET PAYMENT_STATUS_ID=" + paymentstatus + " WHERE ID=" + RequestID + "";
                    //using (SqlCommand cmd = new SqlCommand("EXEC USPUpdatePayment " + RequestID + ",'" + TxnCode + "','" + TxnID + "','" + TxnDate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + TxnResponse + "'," + paymentstatus + ",'"+paymode+"',"+ST+","+ProcFee+","+TotAmt+"", con))
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        string str = Convert.ToString(cmd.ExecuteNonQuery());
                        int.TryParse(str, out UpdID);
                    }
                }
            }
            catch
            {

            }

            return UpdID;
        }
    }
}
