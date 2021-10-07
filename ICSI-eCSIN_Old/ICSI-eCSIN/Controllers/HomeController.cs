using ICSI_eCSIN.Models;
using ICSI_eCSIN.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Text;

namespace ICSI_eCSIN.Controllers
{
    public class HomeController : Controller
    {
        private IUserRepository _userRepository;
        public HomeController()
        {
            this._userRepository = new UserRepository(new ICSI_eCSIN_DBModelEntities());
        }

        [HttpGet]
        public ActionResult Index()
        {
            GetTotalUsereCSIN_Result totalUsers = _userRepository.GetTotaleCSINUser();
            if (totalUsers != null)
            {
                Session["TotaleCSIN"] = totalUsers.TotaleCSIN;
                Session["ToTalUsers"] = totalUsers.TotalUsers;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ICSI_eCSIN.Models.Login obj)
        {
            bool check = false;
            bool checkUdn = false;

            tblUser objtbluser = new tblUser();
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                int userid = 0;
                tblUser tblUser = _userRepository.GetUserByUserName(obj.UserName);
                if (tblUser == null)
                {
                    ViewBag.Message = "You are not registered, Please go to Member Registration Link";
                    return View(obj);
                }
                else
                {
                    userid = tblUser.UserId;
                }
                objtbluser.UserName = obj.UserName;
                objtbluser.UserId = userid;
                Session["LogedUserID"] = objtbluser.UserName.ToString();
                Session["UserID"] = userid;
                objtbluser.Password = obj.Password;

                //for count user
                Session["UserName"] = objtbluser.UserName;
                Session["WelcomeFullName"] = Convert.ToString(tblUser.FirstName).PadRight(10) + Convert.ToString(tblUser.LastName);

                //for Subsidary Menu
                string UserName = Convert.ToString(Session["UserName"]);
                bool checkExistUserSub = _userRepository.checkExistUserSubsidiary(UserName);
                Session["checkExistUserSub_Menu"] = checkExistUserSub;

                check = _userRepository.CheckLogin(objtbluser);
                if (check == false)
                {
                    message = "Invalid login credentials";
                    ViewBag.Message = message;
                }
                else
                {
                    checkUdn = _userRepository.CheckUdn(objtbluser);
                    if (checkUdn == false)
                    {
                        return RedirectToAction("eCSINGeneration");
                    }
                    else
                    {
                        return RedirectToAction("eCSINListM");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Wrong User/Password.");
            }
            return View(obj);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult MembershipRegistation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MembershipRegistation(MemberRegistration objMemberRegistration, string premember)
        {
            ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/CreatePassword";
            if (premember == "0")
            {
                ViewBag.premember = "Please choose member type.";
                return View();
            }

            if (ModelState.IsValid)
            {
                string prememberval = string.Empty;
                prememberval = premember == "1" ? "A" : "F";

                DateTime DOB = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string strDOB = DOB.ToString("dd/MM/yyyy");

                ICSI_SoapService.Service obj = new ICSI_SoapService.Service();
                var soapData = obj.GetMemberDataeCSIN(prememberval + objMemberRegistration.MRN, strDOB.Replace("-", "/"));
                //var soapData = obj.GetMemberDataeCSIN("A1" + "08/01/1944");
                var soapData1 = obj.GetMemberShipData(prememberval, Convert.ToInt32(objMemberRegistration.MRN));

                if (soapData1.MembershipNo != objMemberRegistration.MRN)
                {
                    ViewBag.ErrorMsg = prememberval + objMemberRegistration.MRN + " is Invalid Membership Number.";
                    return View();
                }

                else if (soapData1.DateofBirth != objMemberRegistration.DOB)
                {
                    ViewBag.ErrorMsg = objMemberRegistration.DOB + " DOB is not matched.";
                    return View();
                }

                string MembershipNo = soapData.MembershipNo;
                string Name = soapData.Name;
                string EmailId = soapData.EmailID;
                int status = 0;
                objMemberRegistration.MRN = prememberval + objMemberRegistration.MRN;


                if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(strDOB))
                {
                    #region Insert in tblUser
                    tblUser objtblUser = new tblUser();
                    objtblUser.UserName = objMemberRegistration.MRN;
                    objtblUser.Password = null;
                    objtblUser.FirstName = Name;
                    objtblUser.MiddleName = null;
                    objtblUser.LastName = null;
                    objtblUser.Status = true;
                    objtblUser.DOB = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtblUser.MobileNumber = null;
                    objtblUser.EmailId = EmailId;
                    objtblUser.CreatedDate = DateTime.Now;
                    //_userRepository.InsertUser(objtblUser);

                    /*Check Member Exist or not */
                    int chkMember = _userRepository.checkMemberExist(objtblUser);
                    if(chkMember==1)
                    {
                        ViewBag.ErrorMsg = "Your Membership number is already registered. Please go to Login page";
                        return View();
                    }


                    /*Check Password Exist*/
                    int chkPassword = _userRepository.checkPasswordExist(objtblUser);
                    if (chkPassword == 1)
                        return RedirectToAction("ForgotPassword");

                    status = _userRepository.InsertTblUser(objtblUser);
                    #endregion

                    if (status > 0)
                    {
                        TempData["UserId"] = objtblUser.UserId;
                        ViewBag.ErrorMsg = "1";
                        //return RedirectToAction("CreatePassword");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Something went wrong! Please contact system Administraton";  // "Your Membership Number is already Registered. Please go to Login page";
                    }
                    //return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMsg = "You are not an authorized member to register with eCSIN Portal, Please check";
                    return View();
                }
            }
            return View();
        }

        public ActionResult CreatePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePassword(CreatePassword objCreatePassword)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/Index";
                int userId = Convert.ToInt32(TempData["UserId"]);
                //int userId = 1;
                tblUser objtblUser = new tblUser();
                objtblUser.UserId = userId;
                objtblUser.Password = objCreatePassword.ConfirmPassword;
                int status = _userRepository.updatetblUserById(objtblUser);
                if (status >= 0)
                {
                    ViewBag.ErrorMsg = 1;
                }
            }
            return View(objCreatePassword);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(Forgotpassword objpassword)
        {
            tblUser objuser = new tblUser();
            string msg = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    string MemmbershipNumber = Convert.ToString(objpassword.MemmbershipNumber);
                    DateTime DOB = DateTime.ParseExact(objpassword.DOB, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    string EmailId = _userRepository.FogotPassword(MemmbershipNumber, DOB);
                    if (string.IsNullOrEmpty(EmailId))
                    {
                        //msg = "Invalid MemmbershipNumber/DOB/YearOfEnrollment";
                        msg = "Invalid MemmbershipNumber/DOB.";
                        ViewBag.msg = msg;
                    }
                    else
                    {
                        int year = Convert.ToInt32(DOB.Year);
                        string EmailTo = EmailId;
                        TempData["mail"] = EmailTo;
                        TempData.Keep();
                        //string NewPassword = year + MemmbershipNumber;
                        string NewPassword = Generate8DigitRNDPassword();
                        objuser.EmailId = EmailTo;
                        objuser.UserName = objpassword.MemmbershipNumber;
                        objuser.Password = NewPassword;
                        string Body = "Please use this Password :" + NewPassword + ". Please keep this for future communications.";
                        if (!string.IsNullOrEmpty(EmailTo))
                            _userRepository.sendMail(EmailTo, "Forgot Password", Body);
                        _userRepository.UpdatePassword(objuser);
                        return RedirectToAction("NewPassword");
                    }
                }
                else
                {
                    ModelState.AddModelError("", " Wrong MemmbershipNumber/DOB/EmailId.");
                }
            }
            catch (Exception ex)
            {
                // ModelState.AddModelError("", " Please try again");
                msg = "You have must be registered email id or Please try again";
                ViewBag.msg = msg;

            }
            return View();
        }

        private string Generate8DigitRNDPassword()
        {
            Random RNG = new Random();
            var builder = new StringBuilder();
            while (builder.Length < 8)
            {
                builder.Append(RNG.Next(10).ToString());
            }
            return builder.ToString();
        }

        [HttpGet]
        public ActionResult NewPassword(string maskedEmail)
        {
            maskedEmail = TempData.Peek("mail").ToString();
            string msg = "New Password has sent to your email please login with new password : " + maskedEmail;
            ViewBag.msg = msg;

            return View();
        }

        [HttpGet]
        public ActionResult eCSINGeneration()
        {
            ViewBag.Message = TempData["Message"];
            eCSINGeneration objeCSINGeneration = new eCSINGeneration();
            //int UserId = 1;
            if (!string.IsNullOrEmpty(Convert.ToString(Session["UserID"])))
            {
                int UserId = Convert.ToInt32(Session["UserID"]);
                tblUser objtblUser = _userRepository.GetUserByID(UserId);

                objeCSINGeneration.MembershipNo = objtblUser.UserName;
                objeCSINGeneration.eCSINGenerateNumber = _userRepository.UDINGeneration(objtblUser.UserName);
            }
            else
                return RedirectToAction("Index");
            return View(objeCSINGeneration);
        }

        [HttpPost]
        public ActionResult eCSINGeneration(eCSINGeneration objeCSINGeneration, string ddlCPA)
        {
            if (ModelState.IsValid)
            {

                int UserId = Convert.ToInt32(Session["UserID"]);

                //int UserId = 1;
                /*Check for multiple eCSIN generation*/
                bool checkExisteCSIN = _userRepository.checkExisteCSIN(UserId);

                Session["checkExisteCSIN_Menu"] = checkExisteCSIN;

                if (checkExisteCSIN == true)
                {
                    ViewBag.Message = "One eCSIN already active. Only after cessation the new eCSIN Number can be generated.";
                    return View(objeCSINGeneration);
                }
                /*End*/

                bool flag = _userRepository.CheckUdnExistance(objeCSINGeneration.eCSINGenerateNumber);

                if (flag == false)
                {
                    string strDOB = _userRepository.GetUserByUserName(objeCSINGeneration.MembershipNo).DOB.ToShortDateString();
                    //DateTime DOB = DateTime.ParseExact(strDOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //strDOB = DOB.ToString("dd/MM/yyyy");

                    ICSI_SoapService.Service obj = new ICSI_SoapService.Service();
                    var soapData = obj.GetMemberDataeCSIN("A" + objeCSINGeneration.MembershipNo.Replace("E", ""), strDOB.Replace("-", "/"));

                    DateTime? DateOfChangeMembershipNo = DateTime.Now;
                    if (!string.IsNullOrEmpty(soapData.ACS_ConversionDate))
                        DateOfChangeMembershipNo = Convert.ToDateTime(soapData.ACS_ConversionDate);
                    else
                        DateOfChangeMembershipNo = null;

                    #region Insert in tbleCSINGeneration
                    tbleCSINGeneration objtbleCSINGeneration = new tbleCSINGeneration();
                    objtbleCSINGeneration.UserId = UserId;
                    objtbleCSINGeneration.eCSINGeneratedNo = objeCSINGeneration.eCSINGenerateNumber;
                    objtbleCSINGeneration.DateOfChangeMembershipNo = DateOfChangeMembershipNo;
                    objtbleCSINGeneration.RestorationOfMembership = null;
                    objtbleCSINGeneration.EmployeeDesignation = objeCSINGeneration.EmployeeDesignation;
                    //objtbleCSINGeneration.EmployerCINNo = objeCSINGeneration.EmployerCINNo;
                    if (objeCSINGeneration.Number == "1")
                    {
                        objtbleCSINGeneration.EmployerCINNo = objeCSINGeneration.EmployerCINNo;
                        objtbleCSINGeneration.PANNumber = string.Empty;
                    }
                    if (objeCSINGeneration.Number == "2")
                    {
                        objtbleCSINGeneration.EmployerCINNo = string.Empty;
                        objtbleCSINGeneration.PANNumber = objeCSINGeneration.EmployerCINNo;
                    }


                    objtbleCSINGeneration.EmployerName = objeCSINGeneration.EmployerName;
                    objtbleCSINGeneration.EmployerRegAdd = objeCSINGeneration.EmployerRegAddress;
                    objtbleCSINGeneration.DateOfOfferLetter = DateTime.ParseExact(objeCSINGeneration.DateOfOfferLetter, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.DateOfConsentLetter = DateTime.ParseExact(objeCSINGeneration.DateOfConsentLetter, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.DateOfAppointment = DateTime.ParseExact(objeCSINGeneration.DateOfAppointment, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.WebsiteOfEmployer = objeCSINGeneration.WebsiteOfEmployer;
                    objtbleCSINGeneration.EmployerPhoneNo = objeCSINGeneration.PhoneNoOfEmployer;
                    objtbleCSINGeneration.EmployerEmailId = objeCSINGeneration.EmailIdOfEmployer;
                    objtbleCSINGeneration.Status = true;
                    objtbleCSINGeneration.CreatedDate = DateTime.Now;
                    objtbleCSINGeneration.FinancialYear = DateTime.Now.Year + "-" + DateTime.Now.AddYears(1).Year.ToString().Substring(2, 2);

                    int status = _userRepository.InserttbleCSINGeneration(objtbleCSINGeneration);

                    if (status > 0)
                    {
                        string EmailTo = _userRepository.GetUserByID(UserId).EmailId;
                        //string Body = "Thank you for registering UDIN. Your 16 digit UDIN number is " + objGenerateUDIN.UDINNumber + ". Please keep this for future communications.";
                        //EmailTo = "akumar@gemini-us.com";
                        string Body = _userRepository.eCSINGenerationEmailBody(objeCSINGeneration);

                        if (!string.IsNullOrEmpty(EmailTo))
                            _userRepository.sendMail(EmailTo, "eCSIN generation", Body);

                        _userRepository.InsertGenerateUDIN();
                        ViewBag.Message = "eCSIN has been generated successfully. eCSIN number is " + objeCSINGeneration.eCSINGenerateNumber + ".";
                        TempData["Message"] = ViewBag.Message;
                        return RedirectToAction("ShoweCSIN");
                    }
                    #endregion
                }
                else
                {
                    ViewBag.Message = "eCSIN number already generated.";
                }
            }

            return View(objeCSINGeneration);
        }

        public ActionResult ShoweCSIN()
        {
            ViewBag.Message = TempData.Peek("Message");
            return View();
        }

        // GET: UDIN
        [HttpGet]
        public ActionResult eCSINVerification()
        {

            return View();
        }
        [HttpPost]
        public ActionResult eCSINVerification(eCSINVerification obj)
        {
            TempData["ListDate"] = null;
            if (ModelState.IsValid)
            {
                var result = _userRepository.GetUDINVerification(obj);
                if (result != null)
                {
                    ViewBag.ErrorMsg = string.Empty;
                    TempData["ListData"] = result;
                    return RedirectToAction("eCSINDocumentDetails");
                }
                else
                {
                    ViewBag.ErrorMsg = "Record not Found";
                }
            }
            return View(obj);
        }
        [HttpGet]
        public ActionResult eCSINDocumentDetails()
        {
            eCSINDetails obj = null;
            if (TempData["ListData"] != null)
            {
                obj = TempData["ListData"] as eCSINDetails;

                TempData.Keep("ListData");
            }

            return View(obj);
        }
        public ActionResult SearcheCSIN()
        {
            return View();
        }
        public ActionResult SearcheCSINI()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SearcheCSINI(SearcheCSIN obj)
        {

            if (Session["UserID"] != null)
            {
                if (ModelState.IsValid)
                {
                    TempData["SearcheCSIN"] = 1;
                    obj.UserId = Convert.ToInt32(Session["UserID"]);
                    var result = _userRepository.GeteAllCSINList(obj);
                    if (result != null && result.Count > 0)
                    {
                        TempData["ListData"] = result;
                        return RedirectToAction("eCSINListM");

                    }
                    ViewBag.Mesaage = "No Data Found";
                    return View(obj);

                }
            }
            else
            {
                ViewBag.Session = "You are not Authorized User.";
            }


            return View(obj);


        }
        [HttpPost]
        public ActionResult SearcheCSIN(SearcheCSIN obj)
        {

            if (Session["UserID"] != null)
            {
                if (ModelState.IsValid)
                {
                    TempData["SearcheCSIN"] = 1;
                    obj.UserId = Convert.ToInt32(Session["UserID"]);
                    var result = _userRepository.GeteCSINList(obj);
                    if (result != null && result.Count > 0)
                    {
                        TempData["ListData"] = result;
                        return RedirectToAction("eCSINList");

                    }
                    ViewBag.Mesaage = "No Data Found";
                    return View(obj);

                }
            }
            else
            {
                ViewBag.Session = "You are not Authorized User.";
            }


            return View(obj);


        }
        [HttpGet]
        public ActionResult eCSINList()
        {
            ViewBag.Message = "";
            if (Convert.ToString(TempData["SearcheCSIN"]) == "1")
            {
                var result = TempData.Peek("ListData") as List<eCSINDetails>;
                if (result != null)
                {
                    return View(result);
                }
            }
            //else
            //{
            //    var result = _userRepository.GeteAllCSINList(obj);
            //    if (result != null && result.Count > 0)
            //    {
            //        TempData["ListData"] = result;
            //        return View(result);

            //    }

            //}
            ViewBag.Message = "No Data Found";
            return View();

        }
        [HttpGet]
        public ActionResult eCSINListM(SearcheCSIN obj)
        {
            ViewBag.Message = "";

            if (Convert.ToString(TempData["SearcheCSIN"]) == "1")
            {
                var result = TempData.Peek("ListData") as List<eCSINDetails>;
                if (result != null)
                {
                    return View(result);
                }
            }

            else
            {

                SearcheCSIN obj1 = new SearcheCSIN();
                obj1.UserId = Convert.ToInt32(Session["UserId"]);

                var result = _userRepository.GeteAllCSINList(obj1);
                if (result != null && result.Count > 0)
                {
                    TempData["ListData"] = result;
                    return View(result);

                }

            }


            ViewBag.Message = "No Data Found";
            return View();

        }
        [HttpGet]
        public ActionResult CanceleCSIN(eCSINDetails obj)
        {
            TempData["Revoke"] = obj;
            return View(obj);
        }
        [HttpPost]
        public ActionResult CanceleCSIN(string RevokeReason, string DateOfNoticeResig_NoticeOfTermination1, string DateOfCessationEmployment1, string radio, string CessationAcpReason)
        {
            eCSINDetails obj = TempData.Peek("Revoke") as eCSINDetails;
            DateTime dt = new DateTime(), dtTo = new DateTime();
            if (RevokeReason == "")
            {
                ViewBag.Message = "Please fill the reason";
                return View(obj);

            }
            if (DateOfNoticeResig_NoticeOfTermination1 == "")
            {
                ViewBag.Message = "Please enter date of termination";
                return View(obj);

            }
            if (DateOfCessationEmployment1 == "")
            {
                ViewBag.Message = "Please Enter Date of Cessation of Employment";
                return View(obj);
            }


            dt = DateTime.ParseExact(DateOfNoticeResig_NoticeOfTermination1, "dd/MM/yyyy",
                                         CultureInfo.InvariantCulture);
            dtTo = DateTime.ParseExact(DateOfCessationEmployment1, "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture);

            obj.RevokeReason = RevokeReason;
            obj.DateOfNoticeResig_NoticeOfTermination = dt;
            obj.DateOfCessationEmployment = dtTo;
            obj.IsAccepted = radio == "1" ? true : false;
            obj.CessationAcpReason = CessationAcpReason;
            //obj.eCSINGeneratedNo = obj.eCSINGeneratedNo.Replace("E", "R");
            string eCSINNo = obj.eCSINGeneratedNo.Remove(0, 1);
            string firstchar = (obj.eCSINGeneratedNo).Substring(0, 1);

            if(firstchar == "E")
            {
                obj.eCSINGeneratedNo = "R" + eCSINNo;
            }

            if (firstchar == "S")
            {
                obj.eCSINGeneratedNo = "C" + eCSINNo;
            }

            obj.Status = false;
           
                       
            bool result = _userRepository.RevokeeCSIN(obj);
            if (result)
            {
                ViewBag.Message = "Cessation of eCSIN Number " + obj.eCSINGeneratedNo + " is Successful";
                return View("ShowRevokedeCSIN");
            }
            ViewBag.Message = "eCSIN Cessation is not Success";
            return View(obj);

        }

        public ActionResult ShowRevokedeCSIN()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ExportToExcel()
        {
            var resultTempData = TempData.Peek("ListData") as List<eCSINDetails>;

            var gv = new GridView();
            gv.DataSource = resultTempData;
            gv.DataBind();
            //gv.HeaderRow.Cells[0].Text = "UDIN Number";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View("UDINList", resultTempData);
        }

        [HttpGet]
        public ActionResult editeCSINGeneration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult editeCSINGeneration(eCSINVerification obj)
        {
            bool status = _userRepository.CheckeCSINGeneration(obj.eCSINNumber);
            if (status == false)
                ViewBag.ErrorMsg = "It is an old eCSIN Number. Please input current eCSIN Unique Number.";
            else
            {
                TempData["eCSINNumber"] = obj.eCSINNumber;
                return RedirectToAction("editCurrenteCSIN");
            }
            return View();
        }

        [HttpGet]
        public ActionResult editCurrenteCSIN()
        {
            EditeCSIN objEditeCSIN;
            string eCSINNumber = Convert.ToString(TempData["eCSINNumber"]);
            if (string.IsNullOrEmpty(eCSINNumber))
                return RedirectToAction("editeCSINGeneration");
            else
                objEditeCSIN = _userRepository.GeteCSINDetails(eCSINNumber);
            objEditeCSIN.eCSINNumber = eCSINNumber;

            return View(objEditeCSIN);
        }

        [HttpPost]
        public ActionResult editCurrenteCSIN(EditeCSIN objEditeCSIN)
        {
            if (ModelState.IsValid)
            {
                int status = _userRepository.UpdateeCSINDetails(objEditeCSIN);
                if (status > 0)
                    ViewBag.Message = "Designation has been updated successfully.";
            }
            return View(objEditeCSIN);
        }

        public ActionResult ChangePassword()
        {
            if (Session["UserName"] != null)
            {
                ChangedNewPasswod changedNewPasswod = new ChangedNewPasswod();
                changedNewPasswod.UserName = Session["UserName"].ToString();
                return View(changedNewPasswod);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangedNewPasswod changedNewPasswod)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/Index";

                tblUser objtblUser = new tblUser();
                objtblUser.Password = changedNewPasswod.OldPassword;
                objtblUser.UserName = changedNewPasswod.UserName;
                if (!this._userRepository.CheckOldPassword(objtblUser))
                {
                    Session["OldPassword"] = changedNewPasswod.OldPassword;
                    objtblUser.Password = changedNewPasswod.ConfirmPassword;
                    int status = _userRepository.ChangePassword(objtblUser);
                    if (status == 1)
                    {
                        ViewBag.msg = "Your Password is successfully changed.";
                    }
                    else
                    {
                        ViewBag.msg = "New Password should not be same as old Password.";
                        return View(changedNewPasswod);
                    }
                }
                else
                {
                    ViewBag.msg = "Invalid old Password.";
                    return View(changedNewPasswod);
                }
                return View(changedNewPasswod);


            }
            return View(changedNewPasswod);
            //if (ModelState.IsValid)
            //{
            //    ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/Index";

            //    tblUser objtblUser = new tblUser();
            //    objtblUser.UserName = changedNewPasswod.UserName;
            //    objtblUser.Password = changedNewPasswod.ConfirmPassword;
            //    int status = _userRepository.ChangePassword(objtblUser);
            //    if (status ==1)
            //    {
            //        ViewBag.ErrorMsg = 1;
            //    }
            //    else
            //    {
            //        ViewBag.msg = "User Name Not Valid.";
            //        return View(changedNewPasswod);
            //    }


            //}
            //return View(changedNewPasswod);
        }


        [HttpGet]
        public ActionResult SearcheCSINUpdate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SearcheCSINUpdate(eCSINVerification obj)
        {
            bool status = _userRepository.CheckeCSINGeneration(obj.eCSINNumber);
            if (status == false)
                ViewBag.ErrorMsg = "It is an old eCSIN Number. Please input current eCSIN Unique Number.";
            else
            {
                TempData["eCSINNumber"] = obj.eCSINNumber;
                return RedirectToAction("UpdateDetaileCSIN");
            }
            return View();
        }

        [HttpGet]
        public ActionResult UpdateDetaileCSIN()
        {
            UpdateDetails objUpdateCSIN;
            string eCSINNumber = Convert.ToString(TempData["eCSINNumber"]);
            if (string.IsNullOrEmpty(eCSINNumber))
                return RedirectToAction("SearcheCSINUpdate");
            else
                objUpdateCSIN = _userRepository.GetEmployeeUpdateDetails(eCSINNumber);
            objUpdateCSIN.eCSINNumber = eCSINNumber;

            return View(objUpdateCSIN);
        }

        [HttpPost]
        public ActionResult UpdateDetaileCSIN(UpdateDetails objUpdateCSIN)
        {
            if (ModelState.IsValid)
            {
                //string eCSINNumber = Convert.ToString(TempData["eCSINNumber"]);
                int status = _userRepository.EmployeeUpdateDetailseCSIN(objUpdateCSIN);
                if (status > 0)
                    ViewBag.Message = "Update Details has been updated successfully.";
            }
            return View(objUpdateCSIN);
        }


        [HttpGet]
        public ActionResult SubeCSINGeneration()
        {
            //ViewBag.Message = TempData["Message"];
            eCSINGeneration objSubeCSINGeneration = new eCSINGeneration();
            //int UserId = 1;
            if (!string.IsNullOrEmpty(Convert.ToString(Session["UserID"])))
            {
                int UserId = Convert.ToInt32(Session["UserID"]);
                tblUser objtblUser = _userRepository.GetUserByID(UserId);

                objSubeCSINGeneration.MembershipNo = objtblUser.UserName;                //objeCSINGeneration.eCSINGenerateNumber = _userRepository.UDINGeneration(objtblUser.UserName);
                string SubeCSINNumber = _userRepository.GetExisteCSINForSub(UserId);
                if (!string.IsNullOrEmpty(SubeCSINNumber))
                {
                    objSubeCSINGeneration.eCSINGenerateNumber = SubeCSINNumber.Replace("E", "S");
                }
            }
            else
                return RedirectToAction("Index");
            return View(objSubeCSINGeneration);
        }

        [HttpPost]
        public ActionResult SubeCSINGeneration(eCSINGeneration objSubeCSINGeneration, string ddlCPA)
        {
            if (ModelState.IsValid)
            {

                int UserId = Convert.ToInt32(Session["UserID"]);              
                // bool checkExisteCSIN = _userRepository.checkExisteCSINforMSub(UserId);      /*Check for multiple Sub eCSIN generation*/
                //Session["checkExisteCSIN_Menu"] = checkExisteCSIN;
                //if (checkExisteCSIN == true)              
                //{
                //    ViewBag.Message = "One Subsidiary eCSIN already active. Only after cessation the new Subsidiary eCSIN Number can be generated.";
                //    return View(objSubeCSINGeneration);
                //}
                bool flag = _userRepository.CheckSubeCSINExistance(objSubeCSINGeneration.eCSINGenerateNumber);

                string firstchar = (objSubeCSINGeneration.eCSINGenerateNumber).Substring(0, 1);

                //if (!string.IsNullOrEmpty(objSubeCSINGeneration.eCSINGenerateNumber) && !string.IsNullOrEmpty(firstchar) && firstchar == "S")

                //{
                //    ViewBag.Message = "eCSIN number for subsidiary already generated.";

                //}

                //if (string.IsNullOrEmpty(firstchar) && firstchar == "C")

                //{
                //    ViewBag.Message = "eCSIN number for subsidiary already Revoked.";
                //    return View(objSubeCSINGeneration);
                //}


                    if (flag == false)                
                {
                    string strDOB = _userRepository.GetUserByUserName(objSubeCSINGeneration.MembershipNo).DOB.ToShortDateString();
                    //DateTime DOB = DateTime.ParseExact(strDOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //strDOB = DOB.ToString("dd/MM/yyyy");

                    ICSI_SoapService.Service obj = new ICSI_SoapService.Service();
                    var soapData = obj.GetMemberDataeCSIN("A" + objSubeCSINGeneration.MembershipNo.Replace("E", ""), strDOB.Replace("-", "/"));

                    DateTime? DateOfChangeMembershipNo = DateTime.Now;
                    if (!string.IsNullOrEmpty(soapData.ACS_ConversionDate))
                        DateOfChangeMembershipNo = Convert.ToDateTime(soapData.ACS_ConversionDate);
                    else
                        DateOfChangeMembershipNo = null;

                    #region Insert in tbleCSINGeneration
                    tbleCSINGeneration objtbleCSINGeneration = new tbleCSINGeneration();
                    objtbleCSINGeneration.UserId = UserId;
                    objtbleCSINGeneration.eCSINGeneratedNo = objSubeCSINGeneration.eCSINGenerateNumber;
                    objtbleCSINGeneration.DateOfChangeMembershipNo = DateOfChangeMembershipNo;
                    objtbleCSINGeneration.RestorationOfMembership = null;
                    objtbleCSINGeneration.EmployeeDesignation = objSubeCSINGeneration.EmployeeDesignation;       //objtbleCSINGeneration.EmployerCINNo = objeCSINGeneration.EmployerCINNo;
                    if (objSubeCSINGeneration.Number == "1")
                    {
                        objtbleCSINGeneration.EmployerCINNo = objSubeCSINGeneration.EmployerCINNo;
                        objtbleCSINGeneration.PANNumber = string.Empty;
                    }
                    if (objSubeCSINGeneration.Number == "2")
                    {
                        objtbleCSINGeneration.EmployerCINNo = string.Empty;
                        objtbleCSINGeneration.PANNumber = objSubeCSINGeneration.EmployerCINNo;
                    }
                    
                    objtbleCSINGeneration.EmployerName = objSubeCSINGeneration.EmployerName;
                    objtbleCSINGeneration.EmployerRegAdd = objSubeCSINGeneration.EmployerRegAddress;
                    objtbleCSINGeneration.DateOfOfferLetter = DateTime.ParseExact(objSubeCSINGeneration.DateOfOfferLetter, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.DateOfConsentLetter = DateTime.ParseExact(objSubeCSINGeneration.DateOfConsentLetter, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.DateOfAppointment = DateTime.ParseExact(objSubeCSINGeneration.DateOfAppointment, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    objtbleCSINGeneration.WebsiteOfEmployer = objSubeCSINGeneration.WebsiteOfEmployer;
                    objtbleCSINGeneration.EmployerPhoneNo = objSubeCSINGeneration.PhoneNoOfEmployer;
                    objtbleCSINGeneration.EmployerEmailId = objSubeCSINGeneration.EmailIdOfEmployer;
                    objtbleCSINGeneration.Status = true;
                    objtbleCSINGeneration.Subsidiarye_Status = true;
                    objtbleCSINGeneration.CreatedDate = DateTime.Now;
                    objtbleCSINGeneration.FinancialYear = DateTime.Now.Year + "-" + DateTime.Now.AddYears(1).Year.ToString().Substring(2, 2);

                    int status = _userRepository.InserttbleCSINGeneration(objtbleCSINGeneration);

                    if (status > 0)
                    {
                        string EmailTo = _userRepository.GetUserByID(UserId).EmailId;
                        //string Body = "Thank you for registering UDIN. Your 16 digit UDIN number is " + objGenerateUDIN.UDINNumber + ". Please keep this for future communications.";
                        //EmailTo = "akumar@gemini-us.com";
                        string Body = _userRepository.eCSINGenerationEmailBody(objSubeCSINGeneration);

                        if (!string.IsNullOrEmpty(EmailTo))
                            _userRepository.sendMail(EmailTo, "Subsidiary eCSIN generation", Body);

                        _userRepository.InsertGenerateUDIN();
                        ViewBag.Message = "eCSIN for Subsidiary has been generated successfully. eCSIN number is " + objSubeCSINGeneration.eCSINGenerateNumber + ".";
                        TempData["Message"] = ViewBag.Message;
                        return RedirectToAction("ShoweCSIN");
                    }
                    #endregion
                }
                else
                {
                    ViewBag.Message = "eCSIN number for subsidiary already generated.";
                }
            }

            return View(objSubeCSINGeneration);
        }
    }
}