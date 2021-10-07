using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSI_UDIN.Models;
using ICSI_UDIN.Repository;

namespace ICSI_UDIN.Controllers
{
    public class HomeController : Controller
    {

        private IUserRepository _userRepository;
        public HomeController()
        {
            this._userRepository = new UserRepository(new ICSI_DBModelEntities());
        }

        [HttpGet]
        public ActionResult Index()
        {
            GetTotalUserUDIN_Result totalUsers = _userRepository.GetTotalUDINUser();
            if (totalUsers != null)
            {
                Session["TotalUDINs"] = totalUsers.TotalUDINs;
                Session["ToTalUsers"] = totalUsers.TotalUsers;
            }
            Session.Abandon();
            return View();
        }

        //[HttpPost]
        //public ActionResult Index(ICSI_UDIN.Models.Login obj)
        //{
        //    bool check = false;
        //    //tblUser objUser = new tblUser();
        //    string message = string.Empty;
        //    if (ModelState.IsValid)
        //    {
        //        tblUser objtbluser = new tblUser();
        //        objtbluser.UserName = obj.UserName;
        //        objtbluser.Password = obj.Password;

        //        check = _userRepository.CheckLogin(objtbluser);
        //        if (check == false)
        //        {
        //            message = "Invalid login credentials";
        //            ViewBag.Message = message;
        //        }
        //        else
        //        {
        //            return RedirectToAction("MembershipRegistation");
        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("", " Wrong User/Password.");
        //    }
        //    return View(obj);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ICSI_UDIN.Models.Login obj)
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
                    ViewBag.Message = "You are not registered. Please go to Member registeration link.";
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
                        return RedirectToAction("GenerateUDIN");
                    }
                    else
                    {
                        return RedirectToAction("UDINList");
                    }


                }
            }
            else
            {
                ModelState.AddModelError("", " Wrong User/Password.");
            }
            return View(obj);
        }

        public ActionResult Details(int UserId)

        {
            var objuser = _userRepository.GetUserByID(UserId);
            var User = new tblUser();
            User.UserName = objuser.UserName;
            User.Password = objuser.Password;
            return View(User);
        }
        [HttpPost]

        public ActionResult Create(FormCollection collection, tblUser objuser)

        {
            try
            {
                var User = new tblUser();
                User.UserId = 0;
                User.UserName = objuser.UserName;
                User.Password = objuser.Password;
                _userRepository.InsertUser(User); // Passing data to InsertEmployee of UserRepository

                return RedirectToAction("Index");

            }
            catch
            {
                return View();

            }

        }

        [HttpPost]

        public ActionResult Edit(FormCollection collection, tblUser objuser)

        {

            try

            {

                var Employee = new tblUser();

                _userRepository.UpdateUser(objuser); // calling UpdateUser method of UserRepository

                return RedirectToAction("Index");

            }

            catch

            {

                return View();

            }
        }

        [HttpPost]
        public ActionResult GenerateAlphaNumericOTP()
        {
            string numbers = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random objrandom = new Random();
            string passwordString = "";
            string strrandom = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                int temp = objrandom.Next(0, numbers.Length);
                passwordString = numbers.ToCharArray()[temp].ToString();
                strrandom += passwordString;
            }
            ViewBag.anotp = strrandom;
            return View("Index");
        }

        public ViewResult Login()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        public ActionResult ChangePassword()
        {
            if (Session["UserName"]!=null)
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
                if(!this._userRepository.CheckOldPassword(objtblUser))
                {
                    Session["OldPassword"]= changedNewPasswod.OldPassword;
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
                    ViewBag.msg = "Invalid old Password";
                    return View(changedNewPasswod);
                }
                return View(changedNewPasswod);


            }
            return View(changedNewPasswod);
        }


        //srinivas
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

                    //DateTime DOB = Convert.ToDateTime(objpassword.DOB);
                    //int YearOfEnrollment = Convert.ToInt32(objpassword.YearOfEnrolment);                    
                    //List<Forgotpassword> EmailId = _userRepository.FogotPassword(MemmbershipNumber, DOB, YearOfEnrollment);
                    List<Forgotpassword> EmailId = _userRepository.FogotPassword(MemmbershipNumber, DOB, 0);
                    if (EmailId.Count == 0)
                    {
                        //msg = "Invalid Memmbership Number/DOB/YearOfEnrollment";
                        msg = "Invalid Memmbership Number/DOB.";
                        ViewBag.msg = msg;
                    }
                    else
                    {
                        int year = Convert.ToInt32(DOB.Year);
                        string EmailTo = EmailId[0].EmailId.ToString();
                        //string maskedEmail = objpassword.MaskEmail(EmailTo);
                        //TempData["mail"] = maskedEmail;
                        TempData["mail"] = EmailTo;
                        TempData.Keep();

                        string NewPassword = Generate8DigitRNDPassword();
                        objuser.EmailId = EmailTo;
                        objuser.UserName = objpassword.MemmbershipNumber;
                        objuser.Password = NewPassword;
                        // _userRepository.UpdatePassword(objuser);
                        string Body = "Please use this Password :" + NewPassword + " Please keep this for future communications.";
                      int result=  _userRepository.UpdatePassword(objuser);
                        if(result!=-1)
                        {
                            if (!string.IsNullOrEmpty(EmailTo))
                                _userRepository.sendMail(EmailTo, "Forgot Password", Body);

                        }
                        else
                        {
                            ViewBag.msg = "Error...";
                            return View(objpassword);
                        }
                       
                      
                        return RedirectToAction("NewPassword");

                    }

                }
                else
                {
                    ModelState.AddModelError("", " Wrong MemmbershipNumber/DOB/EmailId.");

                }
            }
            catch (Exception)
            {

                // ModelState.AddModelError("", " Please try again");
                msg = "Your email Id is not registered with ICSI. Please update your email id through ICSI Member's portal.";
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
        ////srinivas
        //[HttpPost]
        //public ActionResult ForgotPassword(Forgotpassword objpassword)
        //{
        //    tblUser objuser = new tblUser();
        //    string msg = string.Empty;
        //    try
        //    {

        //        if (ModelState.IsValid)
        //        {
        //            string MemmbershipNumber = Convert.ToString(objpassword.MemmbershipNumber);

        //            DateTime DOB = DateTime.ParseExact(objpassword.DOB, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);

        //            //DateTime DOB = Convert.ToDateTime(objpassword.DOB);
        //            //int YearOfEnrollment = Convert.ToInt32(objpassword.YearOfEnrolment);                    
        //            //List<Forgotpassword> EmailId = _userRepository.FogotPassword(MemmbershipNumber, DOB, YearOfEnrollment);
        //            List<Forgotpassword> EmailId = _userRepository.FogotPassword(MemmbershipNumber, DOB, 0);
        //            if (EmailId.Count == 0)
        //            {
        //                //msg = "Invalid MemmbershipNumber/DOB/YearOfEnrollment";
        //                msg = "Invalid MemmbershipNumber/DOB.";
        //                ViewBag.msg = msg;
        //            }
        //            else
        //            {
        //                int year = Convert.ToInt32(DOB.Year);
        //                string EmailTo = EmailId[0].EmailId.ToString();
        //                //string maskedEmail = objpassword.MaskEmail(EmailTo);
        //                //TempData["mail"] = maskedEmail;
        //                TempData["mail"] = EmailTo;
        //                TempData.Keep();
        //                string NewPassword = year + MemmbershipNumber;
        //                objuser.EmailId = EmailTo;
        //                objuser.Password = NewPassword;
        //                // _userRepository.UpdatePassword(objuser);
        //                string Body = "Please use this Password :" + NewPassword + " Please keep this for future communications.";
        //                if (!string.IsNullOrEmpty(EmailTo))
        //                    _userRepository.sendMail(EmailTo, "Forgot Password", Body);
        //                _userRepository.UpdatePassword(objuser);
        //                return RedirectToAction("NewPassword");

        //            }

        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", " Wrong MemmbershipNumber/DOB/EmailId.");

        //        }
        //    }
        //    catch (Exception)
        //    {

        //        // ModelState.AddModelError("", " Please try again");
        //        msg = "Please try again";
        //        ViewBag.msg = msg;
        //    }


        //    return View();
        //}
        [HttpGet]
        public ActionResult NewPassword(string maskedEmail)
        {
            maskedEmail = TempData.Peek("mail").ToString();
            string msg = "New Password is sent your registerd email. Please login with new password : " + maskedEmail;
            ViewBag.msg = msg;

            return View();
        }

        //

        // GET: UDIN
        [HttpGet]
        public ActionResult UDINVerification()
        {

            return View();
        }

        public ActionResult GenerateCaptcha(int? New)
        {
            string str = Convert.ToString(Session["ID"]);

            UDIN_Captcha UdinCph = new UDIN_Captcha();
            byte[] imageByteData = UdinCph.CreateCaptchaImage(New);
            return File(imageByteData, "~/images/ImgCaptcha");

        }


        [HttpPost]
        public ActionResult UDINVerification(UDINVerification obj)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["UDINCaptchaCode"])) && obj.CaptchaCode == Convert.ToString(Session["UdinCaptchaCode"]))
                {
                    ViewBag.CaptchMessage = "";
                    var result = _userRepository.GetUDINVerification(obj);
                    if (result != null)
                    {
                        ViewBag.VNF = "";
                        TempData["Data"] = result;
                        return RedirectToAction("UDINDocumentDetails", obj);
                    }
                    else
                    {
                        ViewBag.VNF = "UDIN does not exist.";
                    }


                }
                else
                {
                    ViewBag.CaptchMessage = "Captcha is not Match";
                }
            }



            return View(obj);


        }
        [HttpGet]
        public ActionResult UDINDocumentDetails()
        {
            RP_UDINVerification_Result obj = null;
            if (TempData["Data"] != null)
            {
                obj = TempData["Data"] as RP_UDINVerification_Result;
                TempData.Keep("Data");

            }

            return View(obj);
        }

        public ActionResult MembershipRegistation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MembershipRegistation(MemberRegistration objMemberRegistration, string premember)
        {
            ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/CreatePassword";

            if (ModelState.IsValid)
            {
                string prememberval = string.Empty;
                prememberval = premember == "1" ? "A" : "F";

                ICSI_SoapService.Service obj = new ICSI_SoapService.Service();
                var soapData = obj.GetMemberShipData(prememberval, Convert.ToInt32(objMemberRegistration.MRN));
              
                if (soapData.MembershipNo != objMemberRegistration.MRN)
                {
                    ViewBag.ErrorMsg = prememberval + objMemberRegistration.MRN + " is Invalid Membership Number.";
                    return View();
                }

                if (soapData.DateofBirth != objMemberRegistration.DOB)
                {
                    ViewBag.ErrorMsg = objMemberRegistration.DOB + " DOB is not matched.";
                    return View();
                }

                if (soapData.CertificateofPracticalNumber == null || string.IsNullOrEmpty(soapData.CertificateofPracticalNumber))
                {
                    ViewBag.ErrorMsg = "Only members with a valid CoP can register on UDIN Portal";
                    return View();
                }

                int CertificateofPracticalNumber = Convert.ToInt32(soapData.CertificateofPracticalNumber);
                    
                if (CertificateofPracticalNumber > 0)
                {
                    string MembershipNo = prememberval + soapData.MembershipNo;
                    string DateOfBirth = soapData.DateofBirth;
                    string FirstName = soapData.FirstName;
                    string MiddleName = soapData.MiddleName;
                    string LastName = soapData.LastName;
                    string PreMembNo = soapData.premembno;
                    string BarredMember = soapData.BarredMember;
                    string BarredDate = soapData.BarredDate;
                    string EnrollDate = soapData.EnrollDate;
                    string CPDate = soapData.CPDate;
                    string Name = soapData.Name;
                    string EmailId = soapData.EmailID;
                    string MemberStatus = soapData.MemberStatus;
                    string Msg = soapData.MSg;
                    string MobileNo = string.Empty;

                    DateTime DOB = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    string strDOB = DOB.ToString("dd/MM/yyyy");  //string strDOB = temp.ToString("yyyy/MM/dd");
                    //var soapGetMemberDetailsOnline = obj.GetMemberDetailsOnline(prememberval + objMemberRegistration.MRN, Convert.ToDateTime(strDOB.Replace("-", "/")));
                    var soapGetMemberDetailsOnline = obj.GetMemberDetailsOnline(prememberval + objMemberRegistration.MRN, strDOB.Replace("-", "/"));
                    //var soapGetMemberDetailsOnline1 = obj.GetMemberDetailsOnline("a1", Convert.ToDateTime("1944/01/08"));

                    if (soapGetMemberDetailsOnline != null)
                    {
                        if (string.IsNullOrEmpty(EmailId))
                            EmailId = soapGetMemberDetailsOnline.Email;
                        MobileNo = soapGetMemberDetailsOnline.Mobile;
                    }

                    //if(EmailId==null)
                    //{
                    //    ViewBag.ErrorMsg = "Your Professional Details are not available ! Please check and update online through www.icsi.in portal.";
                    //    return View();
                    //}

                    if (MobileNo != null)
                    {
                        bool Mobstatus = Regex.IsMatch(MobileNo, "\\A[0-9]{10}\\z");
                        if (Mobstatus == false)
                        {
                            ViewBag.ErrorMsg = "Your Mobile number seems Invalid! Please check and update online through wwww.icsi.in portal.";
                            return View();
                        }
                    }

                    #region Insert in tblUser
                    tblUser objtblUser = new tblUser();
                    objtblUser.UserName = MembershipNo;
                    objtblUser.Password = null;
                    objtblUser.FirstName = FirstName;
                    objtblUser.MiddleName = MiddleName;
                    objtblUser.LastName = LastName;

                    objtblUser.DOB = DateTime.ParseExact(DateOfBirth, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    //objtblUser.DOB = DateOfBirth;
                    objtblUser.MobileNumber = MobileNo;
                    objtblUser.EmailId = EmailId;
                    objtblUser.CreatedDate = DateTime.Now;
                    //_userRepository.InsertUser(objtblUser);                    

                    /*Check Password Exist*/
                    int chkPassword = _userRepository.checkPasswordExist(objtblUser);
                    if (chkPassword == 1)
                        return RedirectToAction("ForgotPassword");

                    /*Check Member Exist or not */
                    int chkMember = _userRepository.checkMemberExist(objtblUser);
                    if (chkMember == 1)
                    {
                        ViewBag.ErrorMsg = "Your Membership number is already registered, Please go to Login page.";
                        return View();
                    }

                    int status = _userRepository.InsertTblUser(objtblUser);
                    #endregion

                    if (status > 0)
                    {

                        TempData["UserId"] = objtblUser.UserId;
                        ViewBag.ErrorMsg = "1";             //return RedirectToAction("CreatePassword");
                    }
                    else
                    {

                        ViewBag.ErrorMsg = "Something went wrong! Please contact system Administraton";  // "Your Membership Number is already Registered. Please go to Login page";
                        // ViewBag.ErrorMsg = "User already exists.";
                        //return RedirectToAction("Index");
                    }
                }
                else
                    ViewBag.ErrorMsg = "Only members with a valid CoP can register on UDIN Portal";  // "CP Holder Can Only Register";
            }
            return View();
        }

        public ActionResult SearchUDIN()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult SearchUDIN(UDINSearch obj)
        //{

        //    if (Session["UserId"] != null)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            obj.UserId = Convert.ToInt32(Session["UserId"]);
        //            var result = _userRepository.GetUDINList(obj);
        //            if (result != null && result.Count > 0)
        //            {
        //                TempData["UDINList"] = result;
        //                return RedirectToAction("UDINList");

        //            }
        //            ViewBag.Mesaage = "No Data Found";
        //            return View(obj);

        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Session = "You are not Authorized User.";
        //    }


        //    return View(obj);


        //}

        //[HttpGet]
        //public ActionResult UDINList()
        //{
        //    var result = TempData.Peek("UDINList") as List<RP_GetUDINList_Result>;
        //    if (result != null)
        //    {
        //        return View(result);
        //    }
        //    return View("SearchUDIN");

        //}

        [HttpPost]
        public ActionResult SearchUDIN(UDINSearch obj)
        {

            if (Session["UserId"] != null)
            {
                if (ModelState.IsValid)
                {
                    TempData["SearchUDIN"] = 1;
                    obj.UserId = Convert.ToInt32(Session["UserId"]);
                    DateTime fromdate = Convert.ToDateTime(obj.FromDate);
                    DateTime todate = Convert.ToDateTime(obj.ToDate);
                    if(fromdate>todate)
                    {

                        ViewBag.Mesaage1 = "To Date must be greater than From Date";
                        return View(obj);

                    }
                    var result = _userRepository.GetUDINList(obj);
                    if (result != null && result.Count > 0)
                    {
                        TempData["UDINList"] = result;
                        return RedirectToAction("UDINList");

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
        public ActionResult UDINList()
        {
            ViewBag.Message = "";
            if (Convert.ToString(TempData["SearchUDIN"]) == "1")
            {
                var result = TempData.Peek("UDINList") as List<RP_GetUDINList_Result>;
                if (result != null)
                {
                    return View(result);
                }
            }
            else
            {
                UDINSearch obj = new UDINSearch();
                obj.UserId = Convert.ToInt32(Session["UserId"]);
                var result = _userRepository.GetUDINList(obj);
                if (result != null && result.Count > 0)
                {
                    TempData["UDINList"] = result;
                    return View(result);

                }
            }
            ViewBag.Message = "No Data Found";

            return View();

        }

        [HttpGet]
        public ActionResult CancelUDIN(RP_GetUDINList_Result obj)
        {
            TempData["Revoke"] = obj;
            return View(obj);
        }

        [HttpPost]
        public ActionResult CancelUDIN(string UDINRevokeReason)
        {
            ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/CancelUDIN";
            RP_GetUDINList_Result obj = TempData.Peek("Revoke") as RP_GetUDINList_Result;
            obj.UDINRevokeReason = UDINRevokeReason;
            if (obj.UDINRevokeReason == null || obj.UDINRevokeReason == "")
            {
                ViewBag.Message = "Please fill the reason";
                return View(obj);
            }
            if (obj != null)
            {
                DateTime dtSignDate = obj.DateOfSigningDoc.AddDays(7);
                int TotalDays = Convert.ToInt32((DateTime.Now - dtSignDate).TotalDays);
                if (TotalDays > 7)
                {
                    ViewBag.Message = "Kindly note that Unused UDIN can only be revoked within 7 Days.";
                    return View(obj);
                }

                //Session["Datediff"] = DateTime.Now - dtSignDate;
            }

            int result = _userRepository.RevokeUDIN(obj);
            if (result == -1)
            {
                ViewBag.Msg = "1";
                ViewBag.Message = "UDIN Revoked Successfully.";
                TempData["Message"] = "UDIN Number " + obj.MembershipNumber + " is Revoked Successfully";
                return RedirectToAction("ShowRevokeUDIN");
            }
            else
            {
                ViewBag.Msg = "0";
                ViewBag.Message = "UDIN Revoked Failed.";
                return View(obj);
            }

        }
        public ActionResult ShowRevokeUDIN()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }



        [HttpGet]
        public ActionResult ExportToExcel()
        {
            var resultTempData = TempData.Peek("UDINList") as List<RP_GetUDINList_Result>;

            var gv = new GridView();
            gv.DataSource = resultTempData;
            gv.DataBind();
            gv.HeaderRow.Cells[0].Text = "UDIN Number";
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

        public ActionResult HelpDeskFacilty()
        {
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
                int userId = Convert.ToInt32(TempData["UserId"]);      //userId = 4;
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

        public ActionResult GenerateUDIN()
        {
            //ViewBag.Message = TempData["Message"];
            string FinYear = (DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year).ToString() + "-" + (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString().Substring(2, 2); 
            int UserId = Convert.ToInt32(Session["UserID"]);
            tblUser objtblUser = _userRepository.GetUserByID(UserId);
            GenerateUDIN objGenerateUDIN = new GenerateUDIN();
            objGenerateUDIN.MRNNumber = objtblUser.UserName;
            objGenerateUDIN.UDINNumber = _userRepository.UDINGeneration(objGenerateUDIN.MRNNumber);
            objGenerateUDIN.FinancialYear = FinYear;
            // objGenerateUDIN.FinancialYear = DateTime.Now.Year + "-" + DateTime.Now.AddYears(1).Year.ToString().Substring(2, 2);
            //objGenerateUDIN.DateOfSigningDoc = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");

            objGenerateUDIN.lstCertificates = _userRepository.CertificateList(1);

            return View(objGenerateUDIN);
        }

        [HttpPost]
        public ActionResult GenerateUDIN(GenerateUDIN objGenerateUDIN, string rdbgroup, string rdbUDINgroup, string ddlCPA)
        {
            //for Peer & Review user
            string UserName = Convert.ToString(Session["UserName"]);
            bool checkExistUserPu = _userRepository.checkExistPuMembershipNumber(UserName);  //Session["checkExistUserPu"] = checkExistUserPu;

            if (Convert.ToInt32(rdbgroup) == 1 || Convert.ToInt32(rdbgroup) == 2)
                objGenerateUDIN.lstCertificates = _userRepository.CertificateList(Convert.ToInt32(rdbgroup));
            else if (Convert.ToInt32(rdbgroup) == 3)
                objGenerateUDIN.lstCertificates = _userRepository.CertificateList(1);

            if (rdbgroup == "3" && string.IsNullOrEmpty(objGenerateUDIN.DocDescription))
            {
                ViewBag.Message = "Document description must be required";
                return View(objGenerateUDIN);
            }
            else if ((rdbgroup == "1" || rdbgroup == "2") && objGenerateUDIN.CertificateId == 0)
            {
                if (rdbgroup == "1")
                    ViewBag.Message = "Please choose Certificate";
                else
                    ViewBag.Message = "Please choose Report";

                return View(objGenerateUDIN);
            }
          
            if (Convert.ToInt32(rdbgroup) == 1)
            {
                Certificate certificate = objGenerateUDIN.lstCertificates.Where(x => x.CertificateId == objGenerateUDIN.CertificateId).FirstOrDefault();
                int count = this._userRepository.checkDocumentType(Convert.ToInt32(Session["UserID"]),1, objGenerateUDIN.CertificateId);
                count = count + 1;

                if (checkExistUserPu == true)
                {
                    if (certificate.MaxNumber != null && certificate.MaxNumber != -1 && count > certificate.MaxNumber && objGenerateUDIN.CertificateId == 1)
                    {
                        ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Certificate Type";
                        return View(objGenerateUDIN);
                    }

                }

                else
                {
                    if (certificate.MaxNumber != null && certificate.MaxNumber != -1 && count > certificate.MaxNumber && objGenerateUDIN.CertificateId == 1)
                    {
                        ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Certificate Type";
                        return View(objGenerateUDIN);
                    }

                }
               
            }

            if (Convert.ToInt32(rdbgroup) == 2)
            {
                Certificate certificate = objGenerateUDIN.lstCertificates.Where(x => x.CertificateId == objGenerateUDIN.CertificateId).FirstOrDefault();
                int count = this._userRepository.checkDocumentType(Convert.ToInt32(Session["UserID"]), 2, objGenerateUDIN.CertificateId);
                count = count + 1;

                //if (Session["checkExistUserPu"] != null && Convert.ToBoolean(Session["checkExistUserPu"]) == true)
                if (checkExistUserPu == true)
                {
                    if (certificate.Pu_MaxNumber != null && certificate.Pu_MaxNumber != -1 && count > certificate.Pu_MaxNumber && objGenerateUDIN.CertificateId == 2)
                    {

                        ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Report Type";
                        return View(objGenerateUDIN);
                    }

                    if (certificate.Pu_MaxNumber != null && certificate.Pu_MaxNumber != -1 && count > certificate.Pu_MaxNumber && objGenerateUDIN.CertificateId == 6)
                    {

                        ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Report Type";
                        return View(objGenerateUDIN);
                    }
                }

                else
                {

                //if (count > 10 && objGenerateUDIN.CertificateId == 2)
                if (certificate.MaxNumber != null && certificate.MaxNumber != -1 && count > certificate.MaxNumber && objGenerateUDIN.CertificateId == 2)
                {
                    
                    ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Report Type";
                    return View(objGenerateUDIN);
                }

                ////if (count > 5 && objGenerateUDIN.CertificateId == 6)
                ////{
                ////    ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Report Type";
                ////    return View(objGenerateUDIN);
                ////}    
                if (certificate.MaxNumber != null && certificate.MaxNumber != -1 && count > certificate.MaxNumber && objGenerateUDIN.CertificateId == 6)
                {

                    ViewBag.Message = "You have reached your maximum UDIN Generation Limit for this Report Type";
                    return View(objGenerateUDIN);
                }

                }

            }

          
            if (ModelState.IsValid)
            {
                int UserId = Convert.ToInt32(Session["UserID"]);
                bool flag = _userRepository.CheckUdnExistance(objGenerateUDIN.UDINNumber);

                if (flag == false)
                {
                    #region Insert in tblUDIN
                    tblUDIN objtblUDIN = new tblUDIN();
                    objtblUDIN.MembershipNumber = objGenerateUDIN.MRNNumber;
                    objtblUDIN.YearOfEnrollment = null;
                    objtblUDIN.CreatedDate = DateTime.Now;
                    objtblUDIN.CreatedBy = null;
                    objtblUDIN.ModifyDate = null;
                    objtblUDIN.UDINUniqueCode = objGenerateUDIN.UDINNumber;
                    objtblUDIN.IsValid = "Y";
                    if (string.IsNullOrEmpty(objGenerateUDIN.DocDescription))
                        objtblUDIN.DocumentTypeId = objGenerateUDIN.CertificateId;
                    else
                    {
                        objtblUDIN.DocumentDescription = objGenerateUDIN.DocDescription;
                        objtblUDIN.DocumentTypeId = 13;
                    }
                    objtblUDIN.CertificateTypeId = Convert.ToInt32(rdbgroup);
                    objtblUDIN.StatusId = 0;
                    objtblUDIN.UserId = UserId;
                    objtblUDIN.FinancialYear = objGenerateUDIN.FinancialYear;
                    objtblUDIN.FinancialYear_Remark = objGenerateUDIN.FinancialYear_Remark;
                    objtblUDIN.ClientName = objGenerateUDIN.ClientName;
                    // objtblUDIN.CINNumber = objGenerateUDIN.CINNumber;
                    objtblUDIN.UDINInitiative = Convert.ToInt32(rdbUDINgroup);
                    //objtblUDIN.DateOfSigningDoc = Convert.ToDateTime(objGenerateUDIN.DateOfSigningDoc);
                    objtblUDIN.DateOfSigningDoc = DateTime.ParseExact(objGenerateUDIN.DateOfSigningDoc, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                    if (objGenerateUDIN.Number == "1")
                    {
                        objtblUDIN.CINNumber = objGenerateUDIN.CINNumber;
                    }
                    if (objGenerateUDIN.Number == "2")
                    {
                        objtblUDIN.PANNumber = objGenerateUDIN.CINNumber;
                    }
                    if (objGenerateUDIN.Number == "3")
                    {
                        objtblUDIN.AadharNumber = objGenerateUDIN.CINNumber;
                    }
                    int status = _userRepository.InserttblUDINUser(objtblUDIN);

                    if (status > 0)
                    {
                        string EmailTo = _userRepository.GetUserByID(UserId).EmailId;
                        //string Body = "Thank you for registering UDIN. Your 16 digit UDIN number is " + objGenerateUDIN.UDINNumber + ". Please keep this for future communications.";
                        // EmailTo = "akumar@gemini-us.com";
                        string Body = _userRepository.UDINGenerationEmailBody(objGenerateUDIN.MRNNumber, objGenerateUDIN.UDINNumber, objtblUDIN.CINNumber, objGenerateUDIN.FinancialYear, objtblUDIN.ID, objGenerateUDIN.DateOfSigningDoc, objtblUDIN.PANNumber, objtblUDIN.AadharNumber,objtblUDIN.ClientName);

                        if (!string.IsNullOrEmpty(EmailTo))
                            _userRepository.sendMail(EmailTo, "UDIN generation", Body);

                        _userRepository.InsertGenerateUDIN();
                        ViewBag.Message = "UDIN number " + objGenerateUDIN.UDINNumber + " has been generated successfully. <br/>Kindly note that Unused UDIN can only be revoked within 7 Days.";
                        TempData["Message"] = ViewBag.Message;
                        return RedirectToAction("ShowUDIN");
                    }
                    #endregion
                }
                else
                {
                    ViewBag.Message = "Please refresh the page for multiple UDIN generation.";
                }
            }
            return View(objGenerateUDIN);
        }
        public ActionResult ShowUDIN()
        {
            ViewBag.Message = TempData.Peek("Message");
            //ViewBag.Message = "UDIN number F005922A000000499 has been generated successfully.<br/> Kindly note that Unused UDIN can only be revoked within 7 Days.";
            return View();
        }
        public JsonResult CertificateList(int TypeOfDocument)
        {
            List<Certificate> lstcertificates = new List<Certificate>();
            lstcertificates = _userRepository.CertificateList(Convert.ToInt32(TypeOfDocument));

            return Json(lstcertificates);
        }


        public FileResult download(string Parameter)
        {



            string filename = Server.MapPath("~/PDF/UdinGuidelines.pdf");

            string contentType = "application/pdf";
            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser
            return File(filename, contentType, "UdinGuidelines.pdf");
        }

        public ActionResult DowntimeUDINSite()
        {
            return View();
        }





    }

}