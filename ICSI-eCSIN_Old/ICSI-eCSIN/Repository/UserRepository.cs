using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using ICSI_eCSIN.Models;
using System.Text;
using System.Globalization;

namespace ICSI_eCSIN.Repository
{
    public class UserRepository : IUserRepository
    {
        private ICSI_eCSIN_DBModelEntities DBcontext;
        public UserRepository(ICSI_eCSIN_DBModelEntities objusercontext)
        {
            this.DBcontext = objusercontext;
        }

        public tblUser GetUserByUserName(string UserName)
        {
            return DBcontext.tblUsers.Where(x => x.UserName == UserName).FirstOrDefault();
        }

        public bool CheckLogin(tblUser objuser) //This method check the user existence
        {
            bool flag = false;
            tblUser loginresult = DBcontext.tblUsers.Where(s => s.UserName == objuser.UserName && s.Password == objuser.Password).FirstOrDefault();
            if (loginresult == null)
            {
                return flag;
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public bool CheckUdn(tblUser objuser) //This method check the Udin existence
        {
            bool flag = false;
            var eCSINId = (from cust in DBcontext.tblUsers
                         join ord in DBcontext.tbleCSINGenerations
                         on cust.UserId equals ord.UserId
                         where (cust.UserId == objuser.UserId)
                         select new
                         {
                             ord.eCSINGeneratedNo
                         }).ToList();

            if (eCSINId.Count == 0)
            {
                flag = false;
            }
            else
            {
                flag = true;
            }

            return flag;
        }

        public int checkPasswordExist(tblUser User)
        {
            int status = 0;
            try
            {
                var resTblUser = DBcontext.tblUsers.Where(x => x.DOB == User.DOB && x.UserName == User.UserName).FirstOrDefault();

                if (resTblUser.Password == null)
                {
                    status = 1;
                }
            }
            catch (Exception ex)
            {
                status = 0;
            }
            return status;
        }

        public int checkMemberExist(tblUser UserMember)
        {
            int status = 0;
            try
            {
                var MemTblUser = DBcontext.tblUsers.Where(x => x.UserName == UserMember.UserName).FirstOrDefault();
                if(MemTblUser.UserName!=null)
                {
                    status = 1;
                }
            }
            catch (Exception ex)
            {
                status = 0;
            }
            return status;

        }

        public int InsertTblUser(tblUser User)
        {
            int status = 0;
            try
            {
                var resTblUser = DBcontext.tblUsers.Where(x => x.DOB == User.DOB && x.UserName == User.UserName).FirstOrDefault();

                if (resTblUser == null)
                {
                    DBcontext.tblUsers.Add(User);
                    status = DBcontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                status = 0;
            }
            return status;
        }

        public int updatetblUserById(tblUser objtblUser)
        {
            int status = 0;
            tblUser restblUser = DBcontext.tblUsers.Where(x => x.UserId == objtblUser.UserId).FirstOrDefault();
            restblUser.Password = objtblUser.Password;
            status = DBcontext.SaveChanges();
            //restblUser.EmailId = "akumar@gemini-us.com";
            string body = "Dear " + restblUser.FirstName + ",<br/><br/> Your eCSIN Application is Registered Successfully. Your Login Credentials are  as follows :-  LoginID :- " + restblUser.UserName + " and Password:- " + restblUser.Password + ".";

            if (status >= 0 && restblUser.EmailId != null)
                sendMail(restblUser.EmailId, "Create Password", body);

            return status;
        }

        public void sendMail(string MailTo, string Subject, string Body)
        {
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
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpServer.Port = port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = false;

                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string FogotPassword(string MemmbershipNumber, DateTime DOB)
        {
            var useremail = (from user in DBcontext.tblUsers
                             where (user.UserName == MemmbershipNumber && user.DOB == DOB)
                             select new Forgotpassword
                             {
                                 EmailId = user.EmailId
                             }).SingleOrDefault();

            return useremail.EmailId;
        }

        public void UpdatePassword(tblUser objuser)
        {
            try
            {
                tblUser c = this.DBcontext.tblUsers.Where(x => x.UserName == objuser.UserName).FirstOrDefault();
                //= (from x in DBcontext.tblUsers
                //             where x.EmailId == objuser.EmailId
                //             select x).First();
                c.Password = objuser.Password;
                DBcontext.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            //try
            //{
            //    tblUser c = (from x in DBcontext.tblUsers
            //                 where x.EmailId == objuser.EmailId
            //                 select x).First();
            //    c.Password = objuser.Password;
            //    DBcontext.SaveChanges();
            //}
            //catch (Exception ex)
            //{

            //}
        }

        public tblUser GetUserByID(int UserId)
        {
            return DBcontext.tblUsers.Find(UserId);
        }

        public string UDINGeneration(string MembershipNo)
        {
            char FirstChar = Convert.ToChar(MembershipNo.Substring(0, 1));
            int TotalLength = MembershipNo.Length;
            string First7Digit = string.Empty;
            string UDINNumber = string.Empty;
            for (int i = 0; i < 7 - TotalLength; i++)
            {
                First7Digit += "0";
            }
            First7Digit = FirstChar + First7Digit + MembershipNo.Substring(1, TotalLength - 1);
            foreach (FinancialYear year in Enum.GetValues(typeof(FinancialYear)))
            {
                if (Convert.ToInt32(year) == DateTime.Now.Year)
                    First7Digit = First7Digit + year;
            }


            string Last8Digit = string.Empty;
            int lastValue = 0;
            string FinancialYear = DateTime.Now.Year + "-" + DateTime.Now.AddYears(1).Year.ToString().Substring(2, 2);
            var resGenerateUDIN = DBcontext.tblGenerateeCSINs.Where(x => x.FinancialYear == FinancialYear).SingleOrDefault();
            if (resGenerateUDIN == null)
            {
                Last8Digit = "0000000" + 1;
                lastValue = 1;
            }
            else
            {
                lastValue = resGenerateUDIN.TotalCount + 1;
                for (int i = 0; i < 8 - lastValue.ToString().Length; i++)
                {
                    Last8Digit += "0";
                }
                Last8Digit = Last8Digit + lastValue;
            }
            UDINNumber = First7Digit + Last8Digit;

            string MembershipNowithoutChar = MembershipNo.Substring(1, MembershipNo.Length - 1);
            lastValue = Convert.ToInt32(MembershipNowithoutChar) + lastValue;
            //int last17Value = Convert.ToInt32((lastValue / 11).ToString().Substring(0, 1));
            int last17Value = Convert.ToInt32((lastValue % 11).ToString().Substring(0, 1));

            UDINNumber = UDINNumber + last17Value;
            return "E" + UDINNumber;
        }

        public enum FinancialYear
        {
            A = 2019, B, C, D, E, F, G, H
        }

        public bool CheckUdnExistance(string UDNNumber)
        {
            bool flag = false;
            var udnId = (from UDINs in DBcontext.tbleCSINGenerations
                         where UDINs.eCSINGeneratedNo == UDNNumber
                         select new
                         {
                             UDINs.eCSINGeneratedNo
                         }).ToList();

            if (udnId.Count == 0)
                flag = false;
            else
                flag = true;

            return flag;
        }

        public int InserttbleCSINGeneration(tbleCSINGeneration User)
        {
            int status = 0;
            try
            {
                DBcontext.tbleCSINGenerations.Add(User);
                status = DBcontext.SaveChanges();
            }
            catch (Exception ex) { }
            return status;
        }

        public void InsertGenerateUDIN()
        {
            tblGenerateeCSIN objGenerateUDIN = new tblGenerateeCSIN();
            objGenerateUDIN.FinancialYear = DateTime.Now.Year + "-" + DateTime.Now.AddYears(1).Year.ToString().Substring(2, 2);
            objGenerateUDIN.TotalCount = 1;

            var resGenerateUDIN = DBcontext.tblGenerateeCSINs.Where(x => x.FinancialYear == objGenerateUDIN.FinancialYear).FirstOrDefault();
            if (resGenerateUDIN == null)
                DBcontext.tblGenerateeCSINs.Add(objGenerateUDIN);
            else
                resGenerateUDIN.TotalCount = resGenerateUDIN.TotalCount + 1;
            DBcontext.SaveChanges();
        }

        public string eCSINGenerationEmailBody(eCSINGeneration objeCSINGeneration)
        {
            string CINTitle = string.Empty;
            string CINValue = string.Empty;
            if (objeCSINGeneration.Number == "1")
            {
                CINTitle = "CIN Number";
                CINValue = objeCSINGeneration.EmployerCINNo;
            }
            else
            {
                CINTitle = "Pan Number";
                CINValue = objeCSINGeneration.EmployerCINNo;
            }
            StringBuilder sbString = new StringBuilder();
            sbString.Append("<!DOCTYPE html>"
                + "<html>"
                + "<head>"
                + "</head>"
                + "<body>"
                    + "<h2>eCSIN GENERATED SUCCESSFULLY</h2>"
                    + "<table style=\"font-family:arial,sans-serif;border-collapse:collapse;width:100%;font-size:13px;\">"
                        + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Membership Number</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.MembershipNo + "</td>"
                        + "</tr>"
                        + "<tr style=\"background-color:#dddddd\">"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">eCSIN Number</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.eCSINGenerateNumber + "</td>"
                        + "</tr>"
                            //+ "<tr>"
                            //    + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Employer CIN Number</th>"
                            //    + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.EmployerCINNo + "</td>"
                            //+ "</tr>"
                            + "<tr>"
                                + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">" + CINTitle + "</th>"
                                + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + CINValue + "</td>"
                            + "</tr>"

                        + "<tr style=\"background-color:#dddddd\">"
                           + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Name of Company/Firm/Individual</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.EmployerName + "</td>"
                        + "</tr>"
                        + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Employee Designation</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.EmployeeDesignation + "</td>"
                        + "</tr>"
                        + "<tr style=\"background-color:#dddddd\">"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Employer Regd. Address</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.EmployerRegAddress + "</td>"
                        + "</tr>"
                        + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Date Of Offer Letter</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.DateOfOfferLetter + "</td>"
                        + "</tr>"
                         // + "<tr style=\"background-color:#dddddd\">"
                         //    + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Date Of Offer Letter</th>"
                         //    + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.DateOfOfferLetter + "</td>"
                         //+ "</tr>"
                         + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Date Of Consent Letter</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.DateOfConsentLetter + "</td>"
                        + "</tr>"
                         + "<tr style=\"background-color:#dddddd\">"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Date Of Appointment</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.DateOfAppointment + "</td>"
                        + "</tr>"
                         + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">Website of Employer</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.WebsiteOfEmployer + "</td>"
                        + "</tr>"
                        + "<tr style=\"background-color:#dddddd\">"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">PhoneNo of Employer</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.PhoneNoOfEmployer + "</td>"
                        + "</tr>"
                        + "<tr>"
                            + "<th style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #2D4383\">EmailId of Employer</th>"
                            + "<td style=\"border: 1px solid #dddddd;text-align: left;padding: 8px;color: #666\">" + objeCSINGeneration.EmailIdOfEmployer + "</td>"
                        + "</tr>"
                    + "</table>"
                + "</body>"
                + "</html>");

            return sbString.ToString();
        }

        public eCSINDetails GetUDINVerification(eCSINVerification obj)
        {

            var result = (from eCSIN in DBcontext.tbleCSINGenerations
                          join user in DBcontext.tblUsers
                          on eCSIN.UserId equals user.UserId
                          where eCSIN.eCSINGeneratedNo == obj.eCSINNumber
                          select new eCSINDetails
                          {
                              eCSINGeneratedNo = eCSIN.eCSINGeneratedNo,
                              eCSINGenerationId = eCSIN.eCSINGenerationId,
                              CreatedDate = eCSIN.CreatedDate,
                              MemberName = user.FirstName + " " + user.MiddleName + " " + user.LastName,
                              //UserId = user.UserId,
                              MemberNumber = user.UserName,
                              EmployerCINNo = eCSIN.EmployerCINNo,
                              EmployerName = eCSIN.EmployerName,
                              EmployerRegAdd = eCSIN.EmployerRegAdd,
                              DateOfOfferLetter = eCSIN.DateOfOfferLetter,
                              DateOfConsentLetter = eCSIN.DateOfConsentLetter,
                              DateOfAppointment = eCSIN.DateOfAppointment,
                              DateOfNoticeResig_NoticeOfTermination = eCSIN.DateOfNoticeResig_NoticeOfTermination,
                              DateOfCessationEmployment = eCSIN.DateOfCessationEmployment
                          }).ToList<eCSINDetails>();

            if (result != null && result.Count > 0)
                return result[0];
            else
                return null;
        }

        public List<eCSINDetails> GeteCSINList(SearcheCSIN obj)
        {
            DateTime dtFrom = DateTime.ParseExact(obj.FromDate, "dd/MM/yyyy",
                                           CultureInfo.InvariantCulture);
            DateTime dtTo = DateTime.ParseExact(obj.ToDate, "dd/MM/yyyy",
                                           CultureInfo.InvariantCulture);
            var result = (from eCSIN in DBcontext.tbleCSINGenerations
                          join user in DBcontext.tblUsers
                          on eCSIN.UserId equals user.UserId
                          where
                          //user.UserId == obj.UserId &&
                          (eCSIN.CreatedDate >= dtFrom && eCSIN.CreatedDate <= dtTo)
                          select new eCSINDetails
                          {
                              eCSINGeneratedNo = eCSIN.eCSINGeneratedNo,
                              eCSINGenerationId = eCSIN.eCSINGenerationId,
                              CreatedDate = eCSIN.CreatedDate,
                              MemberName = user.FirstName + " " + user.MiddleName + " " + user.LastName,
                              UserId = user.UserId,
                              EmployerCINNo = eCSIN.EmployerCINNo,
                              EmployerName = eCSIN.EmployerName,
                              EmployerRegAdd = eCSIN.EmployerRegAdd,
                              DateOfOfferLetter = eCSIN.DateOfOfferLetter,
                              DateOfConsentLetter = eCSIN.DateOfConsentLetter,
                              DateOfAppointment = eCSIN.DateOfAppointment,
                              DateOfNoticeResig_NoticeOfTermination = (eCSIN.DateOfNoticeResig_NoticeOfTermination),
                              DateOfCessationEmployment = (eCSIN.DateOfCessationEmployment),
                              Status = eCSIN.Status,
                              MemberNumber = user.UserName
                          }).ToList<eCSINDetails>();

            if (result != null && result.Count > 0)
                return result;
            else
                return null;
        }
        public List<eCSINDetails> GeteAllCSINList(SearcheCSIN obj)
        {
            if (obj.FromDate != null)
            {
                DateTime dtFrom = DateTime.ParseExact(obj.FromDate, "dd/MM/yyyy",
                                          System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                DateTime dtTo = DateTime.ParseExact(obj.ToDate, "dd/MM/yyyy",
                                               System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat).AddDays(1);
                var result = (from eCSIN in DBcontext.tbleCSINGenerations
                              join user in DBcontext.tblUsers
                              on eCSIN.UserId equals user.UserId
                              join eCSINHistory in DBcontext.tbleCSINGenerationHistories
                              on eCSIN.eCSINGenerationId equals eCSINHistory.eCSINGenerationId into ps
                              from eCSINHistory in ps.DefaultIfEmpty()
                              where user.UserId == obj.UserId &&
                                (eCSIN.CreatedDate >= dtFrom && eCSIN.CreatedDate <= dtTo)
                              select new eCSINDetails
                              {
                                  eCSINGeneratedNo = eCSIN.eCSINGeneratedNo,
                                  eCSINGenerationId = eCSIN.eCSINGenerationId,
                                  CreatedDate = eCSIN.CreatedDate,
                                  MemberName = user.FirstName + " " + user.MiddleName + " " + user.LastName,
                                  UserId = user.UserId,

                                  EmployerCINNo = eCSIN.EmployerCINNo == string.Empty ? eCSIN.PANNumber : eCSIN.EmployerCINNo,

                                  EmployerName = eCSIN.EmployerName,
                                  EmployerRegAdd = eCSIN.EmployerRegAdd,
                                  DateOfOfferLetter = eCSIN.DateOfOfferLetter,
                                  DateOfConsentLetter = eCSIN.DateOfConsentLetter,
                                  DateOfAppointment = eCSIN.DateOfAppointment,
                                  DateOfNoticeResig_NoticeOfTermination = (eCSIN.DateOfNoticeResig_NoticeOfTermination),
                                  DateOfCessationEmployment = (eCSIN.DateOfCessationEmployment),
                                  Status = eCSIN.Status,
                                  DateOfUpdateDesignation = eCSINHistory.DateOfChangeDesignation,
                                  MemberNumber = user.UserName
                              }).ToList<eCSINDetails>();
                if (result != null && result.Count > 0)
                    return result;
            }

            else
            {

                var result = (from eCSIN in DBcontext.tbleCSINGenerations
                              join user in DBcontext.tblUsers
                              on eCSIN.UserId equals user.UserId
                              join eCSINHistory in DBcontext.tbleCSINGenerationHistories
                              on eCSIN.eCSINGenerationId equals eCSINHistory.eCSINGenerationId into ps
                              from eCSINHistory in ps.DefaultIfEmpty()
                              where user.UserId == obj.UserId
                              select new eCSINDetails
                              {
                                  eCSINGeneratedNo = eCSIN.eCSINGeneratedNo,
                                  eCSINGenerationId = eCSIN.eCSINGenerationId,
                                  CreatedDate = eCSIN.CreatedDate,
                                  MemberName = user.FirstName + " " + user.MiddleName + " " + user.LastName,
                                  UserId = user.UserId,

                                  EmployerCINNo = eCSIN.EmployerCINNo == string.Empty ? eCSIN.PANNumber : eCSIN.EmployerCINNo,

                                  EmployerName = eCSIN.EmployerName,
                                  EmployerRegAdd = eCSIN.EmployerRegAdd,
                                  DateOfOfferLetter = eCSIN.DateOfOfferLetter,
                                  DateOfConsentLetter = eCSIN.DateOfConsentLetter,
                                  DateOfAppointment = eCSIN.DateOfAppointment,
                                  DateOfNoticeResig_NoticeOfTermination = (eCSIN.DateOfNoticeResig_NoticeOfTermination),
                                  DateOfCessationEmployment = (eCSIN.DateOfCessationEmployment),
                                  Status = eCSIN.Status,
                                  DateOfUpdateDesignation = eCSINHistory.DateOfChangeDesignation,
                                  MemberNumber = user.UserName
                              }).ToList<eCSINDetails>();
                if (result != null && result.Count > 0)
                    return result;
            }


            return null;
        }

        public bool RevokeeCSIN(eCSINDetails obj)
        {
            tbleCSINGeneration tblObj = DBcontext.tbleCSINGenerations.Where(x => x.eCSINGenerationId == obj.eCSINGenerationId).FirstOrDefault();
            if (tblObj != null)
            {
                tblObj.RevokeDate = DateTime.Now;
                tblObj.RevokeReason = obj.RevokeReason;
                tblObj.CessationAcpReason = obj.CessationAcpReason;

                tblObj.Status = false;
                //string firstchar = (tblObj.eCSINGeneratedNo).Substring(0, 1);

                //if (firstchar == "E")
                //{
                //    tblObj.Status = false;
                //}

                //if (firstchar == "R")
                //{
                //    tblObj.Subsidiarye_Status = false;
                //}


                tblObj.DateOfCessationEmployment = obj.DateOfCessationEmployment;
                tblObj.DateOfNoticeResig_NoticeOfTermination = obj.DateOfNoticeResig_NoticeOfTermination;
                tblObj.IsAccepted = obj.IsAccepted;
                tblObj.eCSINGeneratedNo = obj.eCSINGeneratedNo;

                DBcontext.SaveChanges();
                return true;
            }
            return false;

        }

        public bool CheckeCSINGeneration(string eCSINNumber)
        {
            tbleCSINGeneration tblObj = DBcontext.tbleCSINGenerations.Where(x => x.eCSINGeneratedNo == eCSINNumber && x.Status == true).FirstOrDefault();
            if (tblObj != null)
            {
                return true;
            }
            return false;
        }

        public EditeCSIN GeteCSINDetails(string eCSINNumber)
        {
            var reseCSINDetails = (from objuser in DBcontext.tblUsers.AsEnumerable()
                                   join objeCSINDetails in DBcontext.tbleCSINGenerations.AsEnumerable()
                                   on objuser.UserId equals objeCSINDetails.UserId
                                   where objeCSINDetails.eCSINGeneratedNo == eCSINNumber
                                   && objeCSINDetails.Status == true
                                   select new EditeCSIN
                                   {
                                       MembershipNo = objuser.UserName,
                                       PrevEmpDesignation = objeCSINDetails.EmployeeDesignation,
                                       //EmployerCINNo = objeCSINDetails.EmployerCINNo,
                                       EmployerCINNo = objeCSINDetails.EmployerCINNo == string.Empty ? objeCSINDetails.PANNumber : objeCSINDetails.EmployerCINNo,
                                       EmployerName = objeCSINDetails.EmployerName,
                                       EmployerRegAddress = objeCSINDetails.EmployerRegAdd,
                                       DateOfOfferLetter = objeCSINDetails.DateOfOfferLetter.ToString("dd/MM/yyyy"),
                                       DateOfConsentLetter = objeCSINDetails.DateOfConsentLetter.ToString("dd/MM/yyyy"),
                                       DateOfAppointment = objeCSINDetails.DateOfAppointment.ToString("dd/MM/yyyy")
                                   }).SingleOrDefault();
            return reseCSINDetails;
        }

        public int UpdateeCSINDetails(EditeCSIN objEditeCSIN)
        {
            int status = 0;
            try
            {
                var reseCSIN = DBcontext.tbleCSINGenerations.Where(x => x.eCSINGeneratedNo == objEditeCSIN.eCSINNumber).FirstOrDefault();
                if (reseCSIN != null)
                {
                    reseCSIN.EmployeeDesignation = objEditeCSIN.CurrentEmpDesignation;
                    status = DBcontext.SaveChanges();
                    if (status > 0)
                    {
                        tbleCSINGenerationHistory objtbleCSINGenerationHistory = new tbleCSINGenerationHistory();
                        objtbleCSINGenerationHistory.PrevDesignation = objEditeCSIN.PrevEmpDesignation;
                        objtbleCSINGenerationHistory.CurrentDesignation = objEditeCSIN.CurrentEmpDesignation;
                        objtbleCSINGenerationHistory.CreatedDate = DateTime.Now;
                        objtbleCSINGenerationHistory.eCSINGenerationId = reseCSIN.eCSINGenerationId;
                        objtbleCSINGenerationHistory.DateOfChangeDesignation = DateTime.ParseExact(objEditeCSIN.DateOfChangeDesignation, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DBcontext.tbleCSINGenerationHistories.Add(objtbleCSINGenerationHistory);
                        status = 0;
                        status = DBcontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex) { }

            return status;
        }

        public bool checkExisteCSIN(int UserId)
        {
            bool status = false;

            var reseCSIN = DBcontext.tbleCSINGenerations.Where(x => x.UserId == UserId && x.Status == true).FirstOrDefault();
            if (reseCSIN != null)
                status = true;

            return status;
        }

        public GetTotalUsereCSIN_Result GetTotaleCSINUser()
        {
            try
            {
                return DBcontext.GetTotalUsereCSIN().SingleOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public int ChangePassword(tblUser objtblUser)
        {

            try
            {
                int status = 0;
                tblUser restblUser = DBcontext.tblUsers.AsEnumerable().Where(x => x.UserName == objtblUser.UserName && x.Password == HttpContext.Current.Session["OldPassword"].ToString()).FirstOrDefault();
                restblUser.Password = objtblUser.Password;
                status = DBcontext.SaveChanges();

                //restblUser.EmailId = "akumar@gemini-us.com";
                //string body = "Dear " + restblUser.FirstName + ",<br/><br/> Your UDIN Application is Registered Successfully. Your Login Credentials are  as follows :-  LoginID :- " + restblUser.UserId + " and Password:- " + restblUser.Password + ".";

                //if (status >= 0 && restblUser.EmailId != null)
                //    sendMail(restblUser.EmailId, "Create Password", body);

                return status;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public bool CheckOldPassword(tblUser tblUser)
        {
            tblUser restblUser = DBcontext.tblUsers.Where(x => x.UserName == tblUser.UserName && x.Password == tblUser.Password).FirstOrDefault();
            if (restblUser == null)
            {
                return true;
            }
            return false;

        }

        public UpdateDetails GetEmployeeUpdateDetails(string eCSINNumber)
        {
            var reseCSINDetails = (from objuser in DBcontext.tblUsers.AsEnumerable()
                                   join objeCSINDetails in DBcontext.tbleCSINGenerations.AsEnumerable()
                                   on objuser.UserId equals objeCSINDetails.UserId
                                   where objeCSINDetails.eCSINGeneratedNo == eCSINNumber
                                   && objeCSINDetails.Status == true
                                   select new UpdateDetails
                                   {
                                       MembershipNo = objuser.UserName,
                                       EmployerRegAddress = objeCSINDetails.EmployerRegAdd,
                                       DateOfOfferLetter = objeCSINDetails.DateOfOfferLetter.ToString("dd/MM/yyyy"),
                                       DateOfConsentLetter = objeCSINDetails.DateOfConsentLetter.ToString("dd/MM/yyyy"),
                                       DateOfAppointment = objeCSINDetails.DateOfAppointment.ToString("dd/MM/yyyy"),
                                       PhoneNoOfEmployer = objeCSINDetails.EmployerPhoneNo,
                                       EmployerEmailId = objeCSINDetails.EmployerEmailId,
                                       WebsiteOfEmployer = objeCSINDetails.WebsiteOfEmployer
                                   }).SingleOrDefault();
            return reseCSINDetails;
        }

        public int EmployeeUpdateDetailseCSIN(UpdateDetails objUpdateCSIN)
        {
            int status = 0;
            try
            {

                var reseCSIN = DBcontext.tbleCSINGenerations.Where(x => x.eCSINGeneratedNo == objUpdateCSIN.eCSINNumber).FirstOrDefault();
                if (reseCSIN != null)
                {
                    //reseCSIN.eCSINGeneratedNo = objUpdateCSIN.eCSINNumber;

                    if (reseCSIN.eCSINGeneratedNo != null)
                    {
                        //tbleCSINGeneration objtbleCSINGenerationUpdate = new tbleCSINGeneration();
                        reseCSIN.EmployerRegAdd = objUpdateCSIN.EmployerRegAddress;
                        reseCSIN.CreatedDate = DateTime.Now;
                        reseCSIN.eCSINGenerationId = reseCSIN.eCSINGenerationId;
                        if (objUpdateCSIN.DateOfOfferLetter.Contains("-"))
                        {
                            objUpdateCSIN.DateOfOfferLetter = objUpdateCSIN.DateOfOfferLetter.Replace("-", "/");
                        }
                        if (objUpdateCSIN.DateOfConsentLetter.Contains("-"))
                        {
                            objUpdateCSIN.DateOfConsentLetter = objUpdateCSIN.DateOfConsentLetter.Replace("-", "/");
                        }
                        if (objUpdateCSIN.DateOfAppointment.Contains("-"))
                        {
                            objUpdateCSIN.DateOfAppointment = objUpdateCSIN.DateOfAppointment.Replace("-", "/");
                        }
                        reseCSIN.DateOfOfferLetter = DateTime.ParseExact(objUpdateCSIN.DateOfOfferLetter, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        reseCSIN.DateOfConsentLetter = DateTime.ParseExact(objUpdateCSIN.DateOfConsentLetter, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        reseCSIN.DateOfAppointment = DateTime.ParseExact(objUpdateCSIN.DateOfAppointment, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        reseCSIN.EmployerPhoneNo = objUpdateCSIN.PhoneNoOfEmployer;
                        reseCSIN.EmployerEmailId = objUpdateCSIN.EmployerEmailId;
                        reseCSIN.WebsiteOfEmployer = objUpdateCSIN.WebsiteOfEmployer; //DBcontext.tbleCSINGenerations.Add(reseCSIN);
                        status = DBcontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return status;
        }


        public string GetExisteCSINForSub(int UserId)
        {
            // bool status = false;

            var reseCSIN = DBcontext.tbleCSINGenerations.Where(x => x.UserId == UserId && x.Status == true && x.Subsidiarye_Status==null).FirstOrDefault();
            if (reseCSIN != null)
                return reseCSIN.eCSINGeneratedNo;

            return null;
        }

        public bool checkExisteCSINforMSub(int UserId)   /*Check for multiple eCSIN generation : currently not use*/
        {
            bool status = false;

            var reseCSIN = DBcontext.tbleCSINGenerations.Where(x => x.UserId == UserId && x.Subsidiarye_Status == true).FirstOrDefault();
            if (reseCSIN != null)
                status = true;

            return status;
        }

        public bool CheckSubeCSINExistance(string SubeCSINNumber)
        {
            bool flag = false;
            var eCSINId = (from UDINs in DBcontext.tbleCSINGenerations
                         where UDINs.eCSINGeneratedNo == SubeCSINNumber 
                         select new
                         {
                             UDINs.eCSINGeneratedNo
                         }).ToList();

            if (eCSINId.Count == 0)
                flag = false;
            else
                flag = true;

            return flag;
        }

        public bool checkExistUserSubsidiary(string UserName)
        {
            bool status = false;

            var SubeCSINUser = DBcontext.tblSubsidiaryUsers.Where(x => x.Membership_Number == UserName && x.Status == true).FirstOrDefault();
            if (SubeCSINUser!=null)
                status = true;
            else
                status = false;

            return status;
        }

        public bool checkExistMembershipNumber(string UserName)
        {
            bool status = false;

            var UserMembership = DBcontext.tblSubsidiaryUsers.Where(x => x.Membership_Number == UserName && x.Status == true).FirstOrDefault();
            if (UserMembership != null)
                status = true;
            else
                status = false;

            return status;
        }
    }
}