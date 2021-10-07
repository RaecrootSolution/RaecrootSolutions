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
    public class EmailService
    {
        DataAccessService objDataAccessService = new DataAccessService();
        bool EnableDebug = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDebug"]);

        public void SendEmailDataTraining()
        {
            try
            {
                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is enter in SendEmailData" + DateTime.Now, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                DataTable dtEmaildata = new DataTable();
                Dictionary<string, SqlParameter> cmdParameters = new Dictionary<string, SqlParameter>();
                cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_DATA");

                dtEmaildata = objDataAccessService.ExecuteQuery(Convert.ToString(CommonService.enmConnectionName.training),
                    Convert.ToString(CommonService.enmStoreProcedures.PRD_GET_EMAILSMS), cmdParameters);

                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is count dtEmaildata:- " + dtEmaildata.Rows.Count, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                if (dtEmaildata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmaildata.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dtEmaildata.Rows[i]["ID"]);
                        string MailTo = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_TO_TX"]);
                        string MailBCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BCC_TX"]);
                        string MailCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_CC_TX"]);
                        string Subject = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_SUBJECT_TX"]);
                        string Body = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BODY_TX"]);
                        int mailStatus = sendMail(MailTo, Subject, Body, MailBCC, MailCC);
                        if (mailStatus == 1)
                        {
                            int updateStatus = 0;
                            cmdParameters = new Dictionary<string, SqlParameter>();
                            cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_UPDATE_STATUS");
                            cmdParameters["ID"] = new SqlParameter("ID", ID);
                            updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.training),
                                Convert.ToString(CommonService.enmStoreProcedures.PRD_UPDATE_EMAILSMS_STATUS), cmdParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.Email));
            }
        }

        /// <summary>
        /// Sends email notifications for SAR module
        /// </summary>
        internal void SendEmailDataSAR()
        {
            //throw new NotImplementedException();
            try
            {
                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is enter in SendEmailData" + DateTime.Now, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                DataTable dtEmaildata = new DataTable();
                Dictionary<string, SqlParameter> cmdParameters = new Dictionary<string, SqlParameter>();
                cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_DATA");

                dtEmaildata = objDataAccessService.ExecuteQuery(Convert.ToString(CommonService.enmConnectionName.SAR),
                    Convert.ToString(CommonService.enmStoreProcedures.PRD_GET_EMAILSMS), cmdParameters);

                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is count dtEmaildata:- " + dtEmaildata.Rows.Count, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                if (dtEmaildata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmaildata.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dtEmaildata.Rows[i]["ID"]);
                        string MailTo = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_TO_TX"]);
                        string MailBCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BCC_TX"]);
                        string MailCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_CC_TX"]);
                        string Subject = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_SUBJECT_TX"]);
                        string Body = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BODY_TX"]);
                        int mailStatus = sendMail(MailTo, Subject, Body, MailBCC, MailCC);
                        if (mailStatus == 1)
                        {
                            int updateStatus = 0;
                            cmdParameters = new Dictionary<string, SqlParameter>();
                            cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_UPDATE_STATUS");
                            cmdParameters["ID"] = new SqlParameter("ID", ID);
                            updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.SAR),
                                Convert.ToString(CommonService.enmStoreProcedures.PRD_UPDATE_EMAILSMS_STATUS), cmdParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.Email));
            }
        }

        public void SendEmailDataCSR()
        {
            try
            {
                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is enter in SendEmailData" + DateTime.Now, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                DataTable dtEmaildata = new DataTable();
                Dictionary<string, SqlParameter> cmdParameters = new Dictionary<string, SqlParameter>();
                cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_DATA");

                dtEmaildata = objDataAccessService.ExecuteQuery(Convert.ToString(CommonService.enmConnectionName.CSR),
                    Convert.ToString(CommonService.enmStoreProcedures.PRD_GET_EMAILSMS), cmdParameters);

                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is count dtEmaildata:- " + dtEmaildata.Rows.Count, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                if (dtEmaildata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmaildata.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dtEmaildata.Rows[i]["ID"]);
                        string MailTo = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_TO_TX"]);
                        string MailBCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BCC_TX"]);
                        string MailCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_CC_TX"]);
                        string Subject = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_SUBJECT_TX"]);
                        string Body = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BODY_TX"]);
                        int mailStatus = sendMail(MailTo, Subject, Body, MailBCC, MailCC);
                        if (mailStatus == 1)
                        {
                            int updateStatus = 0;
                            cmdParameters = new Dictionary<string, SqlParameter>();
                            cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_UPDATE_STATUS");
                            cmdParameters["ID"] = new SqlParameter("ID", ID);
                            updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.CSR),
                                Convert.ToString(CommonService.enmStoreProcedures.PRD_UPDATE_EMAILSMS_STATUS), cmdParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.Email));
            }
        }

        public void SendEmailDataCGA()
        {
            try
            {
                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is enter in SendEmailData" + DateTime.Now, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                DataTable dtEmaildata = new DataTable();
                Dictionary<string, SqlParameter> cmdParameters = new Dictionary<string, SqlParameter>();
                cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_DATA");

                dtEmaildata = objDataAccessService.ExecuteQuery(Convert.ToString(CommonService.enmConnectionName.CGA),
                    Convert.ToString(CommonService.enmStoreProcedures.PRD_GET_EMAILSMS), cmdParameters);

                if (EnableDebug)
                    CommonService.WriteToErrorLogs("Service is count dtEmaildata:- " + dtEmaildata.Rows.Count, Convert.ToString(CommonService.enmErrorLogs.ServiceLog));

                if (dtEmaildata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmaildata.Rows.Count; i++)
                    {
                        int ID = Convert.ToInt32(dtEmaildata.Rows[i]["ID"]);
                        string MailTo = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_TO_TX"]);
                        string MailBCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BCC_TX"]);
                        string MailCC = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_CC_TX"]);
                        string Subject = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_SUBJECT_TX"]);
                        string Body = Convert.ToString(dtEmaildata.Rows[i]["EMAIL_BODY_TX"]);
                        int mailStatus = sendMail(MailTo, Subject, Body, MailBCC, MailCC);
                        if (mailStatus == 1)
                        {
                            int updateStatus = 0;
                            cmdParameters = new Dictionary<string, SqlParameter>();
                            cmdParameters["TYPE_REQ"] = new SqlParameter("TYPE_REQ", "EMAIL_UPDATE_STATUS");
                            cmdParameters["ID"] = new SqlParameter("ID", ID);
                            updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.CGA),
                                Convert.ToString(CommonService.enmStoreProcedures.PRD_UPDATE_EMAILSMS_STATUS), cmdParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.Email));
            }
        }

        private int sendMail(string MailTo, string Subject, string Body, string MailBCC, string MailCC)
        {
            int mailStatus = 0;
            try
            {
                //MailTo = "akumar@gemini-us.com";                
                string host = ConfigurationManager.AppSettings["host"].ToString();
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(host);

                mail.From = new MailAddress(username);
                mail.To.Add(MailTo);
                if (!string.IsNullOrEmpty(MailBCC))
                    mail.Bcc.Add(MailBCC);
                if (!string.IsNullOrEmpty(MailCC))
                    mail.CC.Add(MailCC);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpServer.Port = port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                //SmtpServer.EnableSsl = false;
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                mailStatus = 1;
            }
            catch (Exception ex)
            {
                mailStatus = 0;
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.Email));
            }
            return mailStatus;
        }
    }
}
