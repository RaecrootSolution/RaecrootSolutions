using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSI_EmailSmsService.Utility
{
    public class CommonService
    {
        public static void WriteToErrorLogs(string Message, string UsedFor)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + UsedFor + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            //string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public enum enmErrorLogs
        {
            Email = 1,
            SMS = 2,
            ServiceLog = 3,
            PCS = 4,
            InterimSolution = 5
        }

        public enum enmStoreProcedures
        {
            PRD_GET_EMAILSMS = 1,//Get the Email and Sms records
            PRD_UPDATE_EMAILSMS_STATUS = 2, //Update the Email and SMS status
            SP_UPDATE_PCS_DETAILS = 3, //Update PCS details from Membership
            SP_RENEWAL_PAYMENT_DATA_SYNC_INTERIM_SOLUTION = 4//Get details in interim solution db
        }

        public enum enmConnectionName
        {
            training = 1,
            PCS = 2,
            CSR = 3,
            CGA = 4,
            InterimSol = 5,
            SAR = 6
        }
    }
}
