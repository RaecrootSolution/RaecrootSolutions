using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

namespace ICSI_Event.Config
{
    public class AppConfig
    {
        public static string PGType = ConfigurationManager.AppSettings["PGType"].ToString();
        public const string CGSTTaxRate= "CGST @9%";
        public const string SGSTTaxRate = "SGST @9%";
        public const string IGSTTaxRate = "IGST @18%";
        public const string GSTTaxRate = "GST @18%";

        public static string GetGST(string buyerstatecode,string ownerstatecode)
        {
            string taxname = "";
            if(buyerstatecode==ownerstatecode)
            {
                taxname = "CGSTSGST";
            }
            else

            {
                taxname = "IGST";
            }
            return taxname;
        }

        
        public static void WriteToErrorLogs(string Message, string UsedFor)
        {
            bool flag = true;
            if(flag)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + UsedFor + "" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                //string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine(Message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine(Message);
                    }
                }
            }
        }
    }
}