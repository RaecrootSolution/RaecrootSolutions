using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

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
    }
}