using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace ICSI_EmailSmsService.Utility
{
    public class SMSService
    {
        DataAccessService objDataAccessService = new DataAccessService();

        public void SendSMSData()
        {
            try
            {
                DataTable dtSMSdata = new DataTable();
                Dictionary<string, SqlParameter> cmdParameters = new Dictionary<string, SqlParameter>();
                cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "SMS_DATA");

                dtSMSdata = objDataAccessService.ExecuteQuery(Convert.ToString(CommonService.enmConnectionName.training),
                    Convert.ToString(CommonService.enmStoreProcedures.PRD_GET_EMAILSMS), cmdParameters);
                if (dtSMSdata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSMSdata.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dtSMSdata.Rows[i]["ID"]);
                        string MobileNo = Convert.ToString(dtSMSdata.Rows[i]["SMS_MOB_NO_TX"]);                        
                        string Body = Convert.ToString(dtSMSdata.Rows[i]["SMS_BODY_TX"]);
                        //int mailStatus = sendMail(MailTo, Subject, Body, MailBCC, MailCC);
                        int SMSStatus = 0;
                        if (SMSStatus == 1)
                        {
                            int updateStatus = 0;
                            cmdParameters = new Dictionary<string, SqlParameter>();
                            cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "SMS_UPDATE_STATUS");
                            cmdParameters["ID"] = new SqlParameter("ID", ID);
                            updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.training),
                                Convert.ToString(CommonService.enmStoreProcedures.PRD_UPDATE_EMAILSMS_STATUS), cmdParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.SMS));
            }
        }
    }
}
