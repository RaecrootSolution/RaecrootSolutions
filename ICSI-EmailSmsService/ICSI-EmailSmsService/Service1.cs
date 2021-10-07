using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ICSI_EmailSmsService.Utility;
using System.Configuration;

namespace ICSI_EmailSmsService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            bool EnableDebug = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDebug"]);
            EmailService objEmailUtilService = new EmailService();
            PCSService objPCSService = new PCSService();
            InterimSolution objInterimSolution = new InterimSolution();

            while (true)
            {
                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is started at " + DateTime.Now, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                objEmailUtilService.SendEmailDataTraining();
                objEmailUtilService.SendEmailDataCSR();
                objEmailUtilService.SendEmailDataSAR();
                //objEmailUtilService.SendEmailDataCGA();

                var runAt = DateTime.Today + TimeSpan.FromHours(1);//Run Service at 1 a.m.                
                if (runAt >= DateTime.Now)
                    objPCSService.PCSUpdateData();

                var runAt3 = DateTime.Today + TimeSpan.FromHours(3);//Run Service at 3 a.m.
                var runAt3Prev = DateTime.Today + TimeSpan.FromMinutes(195);//Run Service at 3:15 a.m.
                var runAtLunch = DateTime.Today + TimeSpan.FromMinutes(825);//Run Service at 1:45 p.m.
                var runAt3PrevLunch = DateTime.Today + TimeSpan.FromHours(14);//Run Service at 2 p.m.
                if ((runAt3 >= DateTime.Now && DateTime.Now < runAt3Prev) || (runAtLunch >= DateTime.Now && DateTime.Now < runAt3PrevLunch))
                    objInterimSolution.GetRenewalPaymentData();
            }
        }
    }
}
