using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using CrystalDecisions.Shared;
using System.Web.Configuration;

namespace ICSI_WebApp.Reports
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //System.Threading.Thread.Sleep(15000);
            string UserName = WebConfigurationManager.AppSettings["UserName"];
            string Pass = WebConfigurationManager.AppSettings["UserPass"];
            string Serv = WebConfigurationManager.AppSettings["ServerName"];
            string DBase = WebConfigurationManager.AppSettings["DataBaseName"];

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Reports"), "MISEdpReport.rpt"));

            rd.SetDatabaseLogon(UserName, Pass, Serv, DBase);

            rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
            rd.PrintOptions.ApplyPageMargins(new PageMargins(1, 1, 1, 1));
            rd.PrintOptions.PaperSize = PaperSize.PaperA4;
            
            CrystalReportViewer1.ReportSource = rd;
        }
    }
}