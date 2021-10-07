using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using ICSI_EmailSmsService.Utility;

namespace ICSI_EmailSmsService.Utility
{
    public class InterimSolution
    {
        DataAccessService objDataAccessService = new DataAccessService();

        public void GetRenewalPaymentData()
        {
            try
            {
                DataTable dtPCSdata = new DataTable();
                StringBuilder sbquery = new StringBuilder();
                StringBuilder sbUpdateQuery = new StringBuilder();
                Dictionary<string, SqlParameter> cmdParameters;
                int PAYMENT_ID = 0;

                sbquery.Append("SELECT MEM_PAYMENT.RECIEPT_NO AS ACKNUMB,MEM_PAYMENT.TRANSACTION_DT AS ACKDATE,MEMBERSHIP_NO_TX AS MEMBERSHIP_NO,COP_TX as CP_NO,MEM_PAYMENT.ID as PAYMENT_ID,");
                sbquery.Append("MEM_PAYMENT.TOTAL_FEE AS NETAMOUNT,ISNULL((SELECT TOP 1 FEE_HEAD_AMOUNT FROM MEM_PAYMENT_DETAILS_T WHERE PAYMENT_ID=MEM_PAYMENT.ID AND ACTIVE_YN=1 AND FEE_HEAD_ID IN(2,6,9)),0) AS AF_AMT,");
                sbquery.Append("ISNULL((SELECT TOP 1 FEE_HEAD_AMOUNT FROM MEM_PAYMENT_DETAILS_T WHERE PAYMENT_ID=MEM_PAYMENT.ID AND ACTIVE_YN=1 AND FEE_HEAD_ID IN(5,10)),0) AS CF_AMT,");
                sbquery.Append("PAN_NO_TX,AADHAR_NO_TX,GST_NO_TX,MEMBER_NAME_TX FROM MEM_PAYMENT_T AS MEM_PAYMENT JOIN MEMBERSHIP_DETAILS_T AS MEM_DETAILS ON MEM_DETAILS.ID=MEM_PAYMENT.REF_ID");               
                sbquery.Append(" WHERE PAYMENT_STATUS_ID=2 AND DATA_MOVE_YN=0 AND MEM_PAYMENT.ACTIVE_YN=1 AND MEM_DETAILS.ACTIVE_YN=1");                

                dtPCSdata = objDataAccessService.ExecuteInlineQuery(Convert.ToString(CommonService.enmConnectionName.InterimSol), sbquery.ToString());

                if (dtPCSdata != null && dtPCSdata.Rows != null && dtPCSdata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPCSdata.Rows.Count; i++)
                    {
                        PAYMENT_ID = Convert.ToInt32(dtPCSdata.Rows[i]["PAYMENT_ID"]);
                        string ACKDATE = dtPCSdata.Rows[i]["ACKDATE"].ToString().Split(' ')[0];
                        cmdParameters = new Dictionary<string, SqlParameter>();
                        cmdParameters["ACKNUMB"] = new SqlParameter("ACKNUMB", dtPCSdata.Rows[i]["ACKNUMB"]);
                        //cmdParameters["ACKDATE"] = new SqlParameter("ACKDATE", dtPCSdata.Rows[i]["ACKDATE"]);
                        cmdParameters["ACKDATE"] = new SqlParameter("ACKDATE", ACKDATE);
                        cmdParameters["MEMBERSHIP_NO"] = new SqlParameter("MEMBERSHIP_NO", dtPCSdata.Rows[i]["MEMBERSHIP_NO"]);
                        cmdParameters["CP_NO"] = new SqlParameter("CP_NO", dtPCSdata.Rows[i]["CP_NO"]);
                        cmdParameters["NETAMOUNT"] = new SqlParameter("NETAMOUNT", dtPCSdata.Rows[i]["NETAMOUNT"]);
                        cmdParameters["AF_AMT"] = new SqlParameter("AF_AMT", dtPCSdata.Rows[i]["AF_AMT"]);
                        cmdParameters["CF_AMT"] = new SqlParameter("CF_AMT", dtPCSdata.Rows[i]["CF_AMT"]);
                        cmdParameters["PAN_NO"] = new SqlParameter("PAN_NO", dtPCSdata.Rows[i]["PAN_NO_TX"]);
                        cmdParameters["AADHAR_NO"] = new SqlParameter("AADHAR_NO", dtPCSdata.Rows[i]["AADHAR_NO_TX"]);
                        cmdParameters["GST_NO"] = new SqlParameter("GST_NO", dtPCSdata.Rows[i]["GST_NO_TX"]);
                        cmdParameters["MEMBER_NAME"] = new SqlParameter("MEMBER_NAME", dtPCSdata.Rows[i]["MEMBER_NAME_TX"]);

                        int updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.PCS),
                            Convert.ToString(CommonService.enmStoreProcedures.SP_RENEWAL_PAYMENT_DATA_SYNC_INTERIM_SOLUTION), cmdParameters);

                        if (updateStatus > 0)
                        {
                            sbUpdateQuery.Append("UPDATE MEM_PAYMENT_T SET DATA_MOVE_YN=1, DATA_MOVE_DT=GETDATE() WHERE ID=" + PAYMENT_ID + "");
                            objDataAccessService.ExecuteDMLInlineQuery(Convert.ToString(CommonService.enmConnectionName.InterimSol),
                                sbUpdateQuery.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.InterimSolution));
            }
        }
    }
}
