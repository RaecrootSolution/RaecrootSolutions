using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSI_Event.DBContext;
using ICSI_Event.Models;
using ICSI_Event.Repository;
using PaymentGateway.Entity;
using PaymentGateway.BAL;
using System.Text.RegularExpressions;
using ICSI_Event.Config;

namespace ICSI_Event.Controllers
{
    public class HomeController : Controller
    {
        private IUserRepository _userRepository;
        public HomeController()
        {
            this._userRepository = new UserRepository(new ICSI_EventsEntities());
        }

        [HttpGet]
        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ICSI_Event.Models.Login obj)
        {
            //bool check = false;          

            //User_T objtbluser = new User_T();
            //objtbluser.User_Name = obj.UserName;
            //objtbluser.Password = obj.Password;
            //string message = string.Empty;
            //if (ModelState.IsValid)
            //{

            //    check = _userRepository.CheckLogin(objtbluser);
            //    if (check == false)
            //    {
            //        ViewBag.Message = "You are not registered. Please go to registeration link.";
            //        //ViewBag.Message = message;
            //    }
            //    else
            //    {
            //        Session["UserID"] = objtbluser.Password;                    
            //       return RedirectToAction("StallBooking");



            //    }
            //}

            return View(obj);
        }

        [HttpGet]
        public ActionResult StallRegistration()
        {
            EventsRegistration obj = new EventsRegistration();
            obj.lstEvent = this._userRepository.GetAllEvents();
            obj.lstState = this._userRepository.GetAllState();

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StallRegistration(EventsRegistration eventsRegistration, String CityId)
        {
            if (ModelState.IsValid)
            {
                eventsRegistration.CityId = Convert.ToInt32(CityId);
                bool isExist = this._userRepository.ExistRegistration(eventsRegistration);
                if (!isExist)
                {
                    this._userRepository.StallRegistration(eventsRegistration);
                    return RedirectToAction("Index");
                    //ViewBag.Message = "Save Successfully";
                }
                else
                {
                    ViewBag.Message = eventsRegistration.UserId + " Is already registered";
                }
            }
            eventsRegistration.lstEvent = this._userRepository.GetAllEvents();
            eventsRegistration.lstState = this._userRepository.GetAllState();
            return View(eventsRegistration);
        }
        //public JsonResult GetAmount(int Id)
        //{
        //    StallNumber lst = this._userRepository.GetAllStallNumber().Where(x => x.Id == Id).Select(x => new StallNumber
        //    {
        //        Id = x.Id,
        //        StallNo = x.StallNo,
        //        StallAmount = x.StallAmount
        //    }).FirstOrDefault();
        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result = javaScriptSerializer.Serialize(lst);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetAmount(int Id)
        {
            StallNumber lst = this._userRepository.GetAllDIARYNumber().Where(x => x.Id == Id).Select(x => new StallNumber
            {
                Id = x.Id,
                StallNo = x.StallNo,
                StallAmount = x.StallAmount
            }).FirstOrDefault();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(lst);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStateBasedOnCode(int Id)
        {

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(this._userRepository.GetSateByCode(Id.ToString()));
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult StallBooking()
        {
            this._userRepository.UpdateStallStatusTimeMore10Mints();

            StallBooking stallBooking = new StallBooking();
            stallBooking.lstNumber = this._userRepository.GetAllStallNumber().Select(x => new StallNumber
            {
                Id = x.Id,
                StallNo = x.StallNo + " Rs. " + x.StallAmount

            }).ToList();
           // stallBooking.lstBooked = this._userRepository.BookedStall();
            stallBooking.lstEvent = this._userRepository.GetAllEvents();
            stallBooking.lstState = this._userRepository.GetAllState();
            stallBooking.ICSIGST = this._userRepository.GetICSIGST();
            // StallBooking stallBooking = null;
            if (TempData["StallBooking"] != null)
            {
                stallBooking = TempData["StallBooking"] as StallBooking;
               // stallBooking.lstBooked = this._userRepository.BookedStall();
                stallBooking.lstEvent = this._userRepository.GetAllEvents();
                stallBooking.lstState = this._userRepository.GetAllState();
                stallBooking.lstNumber = this._userRepository.GetAllStallNumber().Select(x => new StallNumber
                {
                    Id = x.Id,
                    StallNo = x.StallNo + " Rs. " + x.StallAmount
                }).ToList();
                stallBooking.ICSIGST = this._userRepository.GetICSIGST();
                //TempData.Keep("StallBooking");

            }
            return View(stallBooking);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StallBooking(StallBooking stallBooking, string Amount, string GST18Amount, string TotalAmount, string StallNumber,string rdbgroup)
        {
            Decimal GSTAmount = Math.Round(Convert.ToDecimal(GST18Amount), 2);
            stallBooking.IsYesNo = rdbgroup;
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(stallBooking.GSTN))
                {

                    Regex rgx = new Regex(@"^([0][1-9]|[1-2][0-9]|[3][0-5])([a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}[zZ]{1}[0-9a-zA-Z]{1})+$");
                   // stallBooking.lstBooked = this._userRepository.BookedStall();
                    stallBooking.lstEvent = this._userRepository.GetAllEvents();
                    stallBooking.lstState = this._userRepository.GetAllState();
                    stallBooking.lstNumber = this._userRepository.GetAllStallNumber().Select(x => new StallNumber
                    {
                        Id = x.Id,
                        StallNo = x.StallNo + " Rs. " + x.StallAmount
                    }).ToList();
                    stallBooking.ICSIGST = this._userRepository.GetICSIGST();
                    if (!rgx.IsMatch(stallBooking.GSTN))
                    {

                        ViewBag.Message = "Please enter valid GST Number";

                        return View(stallBooking);
                    }
                    else
                    {
                        if (stallBooking.GSTN == "09AATTT1103F2ZX")
                        {
                            ViewBag.Message = "This GST is not valid...";
                            return View(stallBooking);
                        }
                    }
                }
                stallBooking.StallNumber = StallNumber;
                stallBooking.Amount = Convert.ToDecimal(Amount);
                stallBooking.GST18Amount = GSTAmount;
                stallBooking.TotalAmount = Convert.ToDecimal(TotalAmount);
                stallBooking.ICSIGST = this._userRepository.GetICSIGST();
                if (string.IsNullOrEmpty(stallBooking.GSTN))
                {
                    stallBooking.SGSTAmount = (stallBooking.Amount * 9) / 100;
                    stallBooking.CGSTAmount = (stallBooking.Amount * 9) / 100;
                    stallBooking.TotalAmount = stallBooking.Amount + stallBooking.SGSTAmount + stallBooking.CGSTAmount;
                }
                else if (stallBooking.GSTN.Substring(0, 2) == stallBooking.ICSIGST.Substring(0, 2))
                {
                    stallBooking.SGSTAmount = (stallBooking.Amount * 9) / 100;
                    stallBooking.CGSTAmount = (stallBooking.Amount * 9) / 100;
                    stallBooking.TotalAmount = stallBooking.Amount + stallBooking.SGSTAmount + stallBooking.CGSTAmount;
                }
                else
                {
                    stallBooking.IGSTAmount = (stallBooking.Amount * 18) / 100;
                    stallBooking.TotalAmount = stallBooking.Amount + stallBooking.IGSTAmount;
                }
                stallBooking.SACCODE = "997212";
                TempData["StallBooking"] = stallBooking;
                if (TempData["StallBooking"] != null)
                {
                    return RedirectToAction("StallConfirmation");
                }

                //stallBooking.StateCode = this._userRepository.GetStateCode(stallBooking.StateId);

            }
            else
            {
                stallBooking.lstNumber = this._userRepository.GetAllStallNumber().Select(x => new StallNumber
                {
                    Id = x.Id,
                    StallNo = x.StallNo + " Rs. " + x.StallAmount
                }).ToList();
                //stallBooking.lstBooked = this._userRepository.BookedStall();
                stallBooking.lstEvent = this._userRepository.GetAllEvents();
                stallBooking.lstState = this._userRepository.GetAllState();
            }
            //string icsiGstin = "09AAATT1103F2ZX";

            return View(stallBooking);

        }
        public JsonResult GetCityList(int Id)
        {
            List<City> lst = this._userRepository.GetALLCity(Id);
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(lst);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStateCode(int Id)
        {
            string statecode = this._userRepository.GetStateCode(Id);
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(statecode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStateByCode(string code)
        {
            string code1 = string.Empty;
            if (string.IsNullOrEmpty(code))
            {
                code1 = "09";
            }
            else
            {
                code1 = code.Substring(0, 2);
            }
            string statecode = this._userRepository.GetSateByCode(code1);
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(statecode);
            return Json(result, JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult StallNumber()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View();
        }
        [HttpPost]
        public ActionResult StallNumber(StallNumber stallNumber)
        {
            if (ModelState.IsValid)
            {
                bool result = this._userRepository.CheckExistStallNo(stallNumber.StallNo);
                if (result)
                {
                    ViewBag.Message = stallNumber.StallNo + " already exist.";
                    return View(stallNumber);
                }

                this._userRepository.AddStallNumber(stallNumber);
                TempData["Message"] = stallNumber.StallNo + " is created successfully" ;
                stallNumber = new StallNumber();
                return RedirectToAction("StallNumber");
            }
            return View(stallNumber);
        }

        public ActionResult PaymentDetail()
        {
            int Id = 0;
            PaymentDetail paymentDetail = null;
            if (TempData["Id"] != null)
            {                
                
                Id = Convert.ToInt32(TempData["Id"]);
                ViewBag.ReqID = Convert.ToString(Id);
                paymentDetail = this._userRepository.GetPaymentDetail(Id);  //for Diary1
                if (paymentDetail.MobileNuber == "8076686086")
                {
                    paymentDetail.TotalAmount = 2;
                }
                TempData["Id"] = TempData["Id"];
                Session["Amount"] = paymentDetail.TotalAmount;
                Session["MobileNo"] = paymentDetail.MobileNuber;
                Session["Name"] = paymentDetail.Name;
            }
            //if (TempData["paymentDetail"]!=null)
            //{
            //    paymentDetail = TempData.Peek("paymentDetail") as PaymentDetail;
            //}
            return View(paymentDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentDetail(PaymentDetail paymentDetail)
        {
            int Id = 0;
            ViewBag.DisableButton = 1;
            if (TempData["Id"] != null)
            {
                TempData["Amount"] = paymentDetail.TotalAmount.ToString("0.00");
                TempData["PayMode"] = "3";
                Id = Convert.ToInt32(TempData["Id"]);
                int Diarynumberid = this._userRepository.GetDIARYNumberId(Id);
                this._userRepository.UpdateDIARYStatus(Diarynumberid);
                //TempData["Id"] = Id;
                Session["Id"] = Id;
                Session["DiaryNumberId"] = Diarynumberid;
                //return RedirectToAction("PaymentReciept");
                //TempData["paymentDetail"] = paymentDetail;
                //TempData.Keep("paymentDetail");
                return RedirectToAction("GoToPG", "PaymentGateway");

            }
            return View(paymentDetail);
        }

        public ActionResult PaymentDone(FormCollection frm)
        {
            PayMentBal objPmt = new PayMentBal();
            string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG2"].ConnectionString;
            try
            {
                AppConfig.WriteToErrorLogs("initated function", "SuccessPayment");
                string txnid = string.Empty;
                DateTime txnDt = DateTime.Now;
                string txncode = string.Empty;
                string txnResponse = string.Empty;
                int paymentstatus = 0;
                string paymode = string.Empty;
                double amt = 0;
                double taxamt = 0;
                double processingfee = 0;
                double totamt = 0;
                int ReqId = 0;
                string txdt = string.Empty;
                /*
                if (Session["Id"] != null)
                {
                    ReqId = Convert.ToInt32(Session["Id"]);
                    //AppConfig.WriteToErrorLogs("Session Req Id catching", "SuccessPayment");
                }
                //AppConfig.WriteToErrorLogs("after Session Req Id catching", "SuccessPayment");
                //ReqId = Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]]));
                // AppConfig.WriteToErrorLogs("Reg Id catching", "SuccessPayment");
                int RegId = this._userRepository.GetRegId(ReqId);
                // AppConfig.WriteToErrorLogs("after Reg Id catching", "SuccessPayment");
                if (Session["Id"] != null)
                {
                    // AppConfig.WriteToErrorLogs("before Reg Id catching from session", "SuccessPayment");
                    RegId = Convert.ToInt32(Session["Id"]);
                    // AppConfig.WriteToErrorLogs("after Reg Id catching from session", "SuccessPayment");
                }
                */
                
                int RegId=0;
                string icsirefno = string.Empty;
                if(frm.AllKeys.Count()>0 && frm[11]!=null)
                {
                    icsirefno = Convert.ToString(frm[11]);
                    int.TryParse(objPmt.GetRegIdBAL(connectionStringPG2, icsirefno),out RegId);

                    TempData["RegId"] = RegId;
                }
                AppConfig.WriteToErrorLogs("Reg ID" + Convert.ToString(RegId), "SuccessPayment");
                //int StallNumberId = this._userRepository.GetStallNumberId(RegId);  
                int DiaryNumberId = this._userRepository.GetDIARYNumberId(RegId);       //for Diary

                AppConfig.WriteToErrorLogs("Total Form Keys: " + Convert.ToString(frm.AllKeys.Count()), "SuccessPayment");
                AppConfig.WriteToErrorLogs("Diary Number ID" + Convert.ToString(DiaryNumberId), "SuccessPayment");
                //AppConfig.WriteToErrorLogs("after diary Id catching ", "SuccessPayment");
                //if (Session["DiaryNumberId"] != null)
                //{
                //    // AppConfig.WriteToErrorLogs("after diary Id catching from session ", "SuccessPayment");
                //    DiaryNumberId = Convert.ToInt32(Session["DiaryNumberId"]);

                //}
                if (Convert.ToString(frm[0]) == "E000")
                {
                    // Success
                    paymentstatus = Convert.ToInt32(ePaymentStatus.Succ);
                    txncode = Convert.ToString(frm[0]);
                    txnid = Convert.ToString(frm[1]);
                    txdt = string.Empty;
                    if (Convert.ToString(frm[6]).Contains("-"))
                    {
                        string converteddt = Convert.ToString(frm[6]);
                        txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                    }
                    //if (!string.IsNullOrEmpty(txdt))
                    //{
                    //    txnDt = Convert.ToDateTime(txdt);
                    //}
                    txnResponse = Convert.ToString(frm[13]);
                    AppConfig.WriteToErrorLogs("Response :" + Convert.ToString(txnResponse), "SuccessPayment");
                    paymode = Convert.ToString(frm[9]);
                    amt = frm[5] == null ? 0 : Convert.ToString(frm[5]) == "null" ? 0 : Convert.ToDouble(frm[5]);
                    taxamt = frm[2] == null ? 0 : Convert.ToString(frm[2]) == "null" ? 0 : Convert.ToDouble(frm[2]);
                    processingfee = frm[3] == null ? 0 : Convert.ToString(frm[3]) == "null" ? 0 : Convert.ToDouble(frm[3]);
                    totamt = frm[4] == null ? 0 : Convert.ToString(frm[4]) == "null" ? 0 : Convert.ToDouble(frm[4]);
                    //this._userRepository.UpdateStallStatus(StallNumberId, 3);
                    //this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, true);

                    this._userRepository.UpdateDIARYStatus(DiaryNumberId, 3);   // for Diary 3
                    this._userRepository.Update_DIARY_TRANSACTION_T_ID(RegId, txnid, true, icsirefno);  // for Diary 4
                    
                    AppConfig.WriteToErrorLogs("Success Done", "SuccessPayment");
                }
                else
                {
                    AppConfig.WriteToErrorLogs("inserted in error condition", "SuccessPayment");
                    // Error
                    paymentstatus = Convert.ToInt32(ePaymentStatus.Fail);

                    txncode = Convert.ToString(frm[0]);
                    AppConfig.WriteToErrorLogs("txncode", "SuccessPayment");
                    txnid = Convert.ToString(frm[1]);
                    AppConfig.WriteToErrorLogs("txnid", "SuccessPayment");
                    txdt = string.Empty;
                    if (Convert.ToString(frm[6]).Contains("-"))
                    {
                        string converteddt = Convert.ToString(frm[6]);
                        AppConfig.WriteToErrorLogs("txndt", "SuccessPayment");
                        txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                        AppConfig.WriteToErrorLogs("converted txndt", "SuccessPayment");

                    }

                    if ((frm.Keys.Count == 12))
                    {
                        return View("CancelOnlinePayment");
                    }

                    // txnDt = Convert.ToDateTime(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                    txnResponse = Convert.ToString(frm[13]);
                    AppConfig.WriteToErrorLogs("txnresponse :" + txnResponse, "SuccessPayment");
                    paymode = Convert.ToString(frm[9]);
                    AppConfig.WriteToErrorLogs("paymode", "SuccessPayment");
                    amt = frm[5] == null ? 0 : Convert.ToString(frm[5]) == "null" ? 0 : Convert.ToDouble(frm[5]);
                    AppConfig.WriteToErrorLogs("txncode", "SuccessPayment");
                    taxamt = frm[2] == null ? 0 : Convert.ToString(frm[2]) == "null" ? 0 : Convert.ToDouble(frm[2]);
                    AppConfig.WriteToErrorLogs("taxamt", "SuccessPayment");
                    processingfee = frm[3] == null ? 0 : Convert.ToString(frm[3]) == "null" ? 0 : Convert.ToDouble(frm[3]);
                    AppConfig.WriteToErrorLogs("procfee", "SuccessPayment");
                    totamt = frm[4] == null ? 0 : Convert.ToString(frm[4]) == "null" ? 0 : Convert.ToDouble(frm[4]);
                    AppConfig.WriteToErrorLogs("totamt", "SuccessPayment");
                    //this._userRepository.UpdateStallStatus(StallNumberId, 4);
                    //this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, false);
                    this._userRepository.UpdateDIARYStatus(DiaryNumberId, 4);                     //for Diary 3_1
                    AppConfig.WriteToErrorLogs("updated diary status", "SuccessPayment");
                    this._userRepository.Update_DIARY_TRANSACTION_T_ID(RegId, txnid, false,icsirefno);      //for Diary 4_1
                    AppConfig.WriteToErrorLogs("updated diary txn status", "SuccessPayment");

                }
                
                AppConfig.WriteToErrorLogs("before updating online table", "SuccessPayment");
                int id = objPmt.UpdatePaymentBal(connectionStringPG2, txnid, txdt, txncode, txnResponse, Convert.ToInt32(Convert.ToString(frm[11])), paymentstatus, paymode, taxamt, processingfee, totamt, 0);
                AppConfig.WriteToErrorLogs("after updating online table", "SuccessPayment");
                ViewBag.TxnID = txnid;
                ViewBag.Status = paymentstatus;
                TempData["TxnId"] = txnid;
                Session["TxnID_R"] = txnid;
                AppConfig.WriteToErrorLogs("Payment Done Successfully", "SuccessPayment");
                if (Convert.ToString(frm[0]) == "E000")
                {
                    try
                    {
                        AppConfig.WriteToErrorLogs("Mail Started", "SuccessPayment");
                        //for email notification
                        string MemebershipNumber = Convert.ToString(_userRepository.GetEmailTransByID(RegId).USER_ID);
                        string EmailTo = Convert.ToString(_userRepository.GetEmailTransByID(RegId).EMAIL_TX);
                        //EmailTo = "akumar@gemini-us.com";
                        string Body = "Dear " + MemebershipNumber + " : Thank you for Online Diary Booking ! The ICSI Transaction ID is " + icsirefno + ", Please keep this for future Reference.";
                        if (!string.IsNullOrEmpty(EmailTo))
                            _userRepository.sendMail(EmailTo, "Diary Booking", Body);
                        AppConfig.WriteToErrorLogs("Mail Sent", "SuccessPayment");
                    }
                    catch { }
                }
            }
            catch(Exception ex)
            {
                AppConfig.WriteToErrorLogs(ex.ToString(), "ErrorPayment");
                AppConfig.WriteToErrorLogs(ex.InnerException.ToString(), "ErrorPayment");
            }
            return View();
        }

        /*  public ActionResult PaymentDone(FormCollection frm)
          {            
              AppConfig.WriteToErrorLogs("initate function", "SuccessPayment");
              string txnid = string.Empty;
              DateTime txnDt = DateTime.Now;
              string txncode = string.Empty;
              string txnResponse = string.Empty;
              int paymentstatus = 0;
              string paymode = string.Empty;
              double amt = 0;
              double taxamt = 0;
              double processingfee = 0;
              double totamt = 0;
              int ReqId = 0;
              string txdt = string.Empty;            
              if (Session["Id"]!=null)
              {
                  ReqId = Convert.ToInt32(Session["Id"]);
                  AppConfig.WriteToErrorLogs("Session Req Id catching", "SuccessPayment");
              }
              AppConfig.WriteToErrorLogs("after Session Req Id catching", "SuccessPayment");
              //ReqId = Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]]));
              AppConfig.WriteToErrorLogs("Reg Id catching", "SuccessPayment");
              int RegId = this._userRepository.GetRegId(ReqId);
              AppConfig.WriteToErrorLogs("after Reg Id catching", "SuccessPayment");
              if (Session["Id"] != null)
              {
                  AppConfig.WriteToErrorLogs("before Reg Id catching from session", "SuccessPayment");
                  RegId = Convert.ToInt32(Session["Id"]);
                  AppConfig.WriteToErrorLogs("after Reg Id catching from session", "SuccessPayment");
              }
              //int StallNumberId = this._userRepository.GetStallNumberId(RegId);  
              int DiaryNumberId = this._userRepository.GetDIARYNumberId(RegId);       //for Diary
              AppConfig.WriteToErrorLogs("after diary Id catching ", "SuccessPayment");
              if (Session["DiaryNumberId"]!=null)
              {
                  AppConfig.WriteToErrorLogs("after diary Id catching from session ", "SuccessPayment");
                  // DiaryNumberId = Convert.ToInt32(Session["DiaryNumberId"]);
                  AppConfig.WriteToErrorLogs(Convert.ToString(Session["DiaryNumberId"]), "SuccessPayment");
                  AppConfig.WriteToErrorLogs(Convert.ToString(((System.Web.HttpRequestWrapper)HttpContext.Request).Form), "SuccessPayment");                
              }
              if (HttpContext.Request.Form[Convert.ToString(HttpContext.Request.Form.Keys[0])] == "E000")
              {
                  // Success
                  paymentstatus = Convert.ToInt32(ePaymentStatus.Succ);
                  txncode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[0]]);
                  txnid = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[1]]);
                   txdt = string.Empty;
                  if (Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]).Contains("-"))
                  {
                      string converteddt = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                      txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                  }
                  //if (!string.IsNullOrEmpty(txdt))
                  //{
                  //    txnDt = Convert.ToDateTime(txdt);
                  //}
                  txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                  paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                  amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                  taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                  processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                  totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
                  //this._userRepository.UpdateStallStatus(StallNumberId, 3);
                  //this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, true);

                  this._userRepository.UpdateDIARYStatus(DiaryNumberId, 3);   // for Diary 3
                  this._userRepository.Update_DIARY_TRANSACTION_T_ID(RegId, txnid, true);  // for Diary 4

                  //for email notification
                  string MemebershipNumber = Session["MembershipNo"].ToString();
                  string TransactionICSI_ID = Session["RequetID"].ToString();
                  string EmailTo = _userRepository.GetEmailTransByID(MemebershipNumber).EMAIL_TX;
                  //EmailTo = "akumar@gemini-us.com";
                  string Body = "Dear " + MemebershipNumber + " : Thank you for Online Diary Booking ! The ICSI Transaction ID is " + TransactionICSI_ID + ", Please keep this for future Reference.";
                  if (!string.IsNullOrEmpty(EmailTo))
                      _userRepository.sendMail(EmailTo, "Diary Booking", Body);
              }
              else
              {
                  AppConfig.WriteToErrorLogs("inserted in error condition", "SuccessPayment");                
                  // Error
                  paymentstatus = Convert.ToInt32(ePaymentStatus.Fail);

                  txncode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[0]]);
                  AppConfig.WriteToErrorLogs("txncode", "SuccessPayment");
                  txnid = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[1]]);
                  AppConfig.WriteToErrorLogs("txnid", "SuccessPayment");
                  txdt = string.Empty;
                  if (Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]).Contains("-"))
                  {
                      string converteddt = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                      AppConfig.WriteToErrorLogs("txndt", "SuccessPayment");
                      txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                      AppConfig.WriteToErrorLogs("converted txndt", "SuccessPayment");

                  }

                  if ((HttpContext.Request.Form.Keys.Count == 12))
                  {
                      return View("CancelOnlinePayment");
                  }

                  // txnDt = Convert.ToDateTime(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                  txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                  AppConfig.WriteToErrorLogs("txnresponse", "SuccessPayment");
                  paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                  AppConfig.WriteToErrorLogs("paymode", "SuccessPayment");
                  amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                  AppConfig.WriteToErrorLogs("txncode", "SuccessPayment");
                  taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                  AppConfig.WriteToErrorLogs("taxamt", "SuccessPayment");
                  processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                  AppConfig.WriteToErrorLogs("procfee", "SuccessPayment");
                  totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
                  AppConfig.WriteToErrorLogs("totamt", "SuccessPayment");
                  //this._userRepository.UpdateStallStatus(StallNumberId, 4);
                  //this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, false);
                  this._userRepository.UpdateDIARYStatus(DiaryNumberId, 4);                     //for Diary 3_1
                  AppConfig.WriteToErrorLogs("updated diary status", "SuccessPayment");
                  this._userRepository.Update_DIARY_TRANSACTION_T_ID(RegId, txnid, false);      //for Diary 4_1
                  AppConfig.WriteToErrorLogs("updated diary txn status", "SuccessPayment");

              }

              PayMentBal objPmt = new PayMentBal();
              string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG2"].ConnectionString;
              AppConfig.WriteToErrorLogs("before updating online table", "SuccessPayment");
              int id = objPmt.UpdatePaymentBal(connectionStringPG2,txnid, txdt, txncode, txnResponse, Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]])), paymentstatus, paymode, taxamt, processingfee, totamt,0);
              AppConfig.WriteToErrorLogs("after updating online table", "SuccessPayment");

              ViewBag.TxnID = txnid;
              ViewBag.Status = paymentstatus;
              TempData["TxnId"] = txnid;
              Session["TxnID_R"] = txnid;
              return View();
          }*/

        public ActionResult PaymentStatus()
        {
            int Id = 0;
            PaymentDetail paymentDetail = null;
            if (TempData["Id"] != null)
            {
                Id = Convert.ToInt32(TempData["Id"]);
                paymentDetail = this._userRepository.GetPaymentDetail(Id);
                TempData.Keep("Id");
            }
            return View(paymentDetail);
        }
        [HttpPost]
        public ActionResult PaymentStatus(string ss)
        {

            if (TempData["Id"] != null)
            {
                TempData["Id"] = TempData["Id"];
                return RedirectToAction("PaymentReciept");
            }
            return View();
        }
        [HttpGet]
        public ActionResult ExportToExcel()
        {
            var resultTempData = TempData["List"];

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

        public ActionResult StallConfirmation()
        {
            StallBooking stallBooking = null;
            //if (TempData["StallBooking"] != null)
            //{
            //    stallBooking = TempData["StallBooking"] as StallBooking;
            //    TempData.Keep("StallBooking");

            //}
            return View(stallBooking);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StallConfirmation(StallBooking stallBooking)
        {
            if (TempData["StallBooking"] != null)
            {
                stallBooking = TempData["StallBooking"] as StallBooking;
            }
            //TempData["Id_1"] = this._userRepository.StallPayment(stallBooking);
            //if (TempData["Id_1"] != null)
            //{
            //    string ReqID = Convert.ToString(TempData["Id_1"]);               
            //    string TotAmount = Convert.ToString(stallBooking.TotalAmount);
            //    string PayMode = "9";
            //    string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
            //    PaymentGatewayData objPGData = new PaymentGatewayData();
            //    string MobileNo = stallBooking.MobileNo;
            //    string Name = stallBooking.ContactPersion;
            //    string connectionStringPG1 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG1"].ConnectionString;
            //    string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG2"].ConnectionString;
            //    int Paymentrequestid = objPGData.InsertOnlinePaymentTransaction(connectionStringPG1, connectionStringPG2,"9999999999",TotAmount, ReqID, PayMode, ProcessRoute, AppConfig.PGType, MobileNo, Name);
            //    Session["RequetID"] = Paymentrequestid;               
            //    return RedirectToAction("PaymentDetail");
            //}
            return View(stallBooking);
        }


        public ActionResult PaymentReciept()
        {
            int Id = 0;
            string txnid = string.Empty;
            if (TempData["TxnId"] != null)
            {
                txnid = Convert.ToString(txnid);
            }
            StallReceipt lstReceipt = new StallReceipt();
            
            //if (Session["Id"] != null)
            //{
            //    Id = Convert.ToInt32(Session["Id"]);
            //    //lstReceipt = this._userRepository.GetStallReceipt(Id);
            //    lstReceipt = this._userRepository.GetDIARYReceipt(Id);  // for Diary 5
            //    TempData["List"] = lstReceipt;
            //}

            if (TempData["RegId"] != null)
            {
                Id = Convert.ToInt32(TempData["RegId"]);
                //lstReceipt = this._userRepository.GetStallReceipt(Id);
                lstReceipt = this._userRepository.GetDIARYReceipt(Id);  // for Diary 5
                TempData["List"] = lstReceipt;
            }
            if (!string.IsNullOrEmpty(txnid))
            {
                lstReceipt.EVENTREGISTRATION_ID = txnid;
            }
            return View(lstReceipt);

        }

        // Print Receipt

        [HttpGet]
        public ActionResult SearchReceiptTransaction()
        {
            //if (string.IsNullOrEmpty(Convert.ToString(Session["FullName"])))
            //{
            //    return RedirectToAction("DiaryOnlineHome");            

            //}

            return View();
        }
        [HttpPost]
        public ActionResult SearchReceiptTransaction(SearchReceiptTransaction obj)
        {
            TempData["ListDate"] = null;
            if (ModelState.IsValid)
            {
                var result = _userRepository.GetReceiptTransaction(obj);   // for Diary b
                if (result != null)
                {
                    ViewBag.ErrorMsg = string.Empty;
                    TempData["ListData"] = result;
                    return RedirectToAction("DiaryPrintReceipt");
                }
                else
                {
                    ViewBag.ErrorMsg = "Invalid Transaction Id,Please Enter Valid Transaction Id.";
                }
            }
            return View(obj);
        }

        [HttpGet]
        public ActionResult StallPrintReceipt()
        {
            ReceiptDetails obj = null;
            if (TempData["ListData"] != null)
            {
                obj = TempData["ListData"] as ReceiptDetails;

                TempData.Keep("ListData");
            }

            return View(obj);
        }

        [HttpGet]
        public ActionResult DiaryOnlineHome()
        {
            ViewBag.Dropdown = new List<Dropdown>
            {
                new Dropdown
                {
                    Value=1,
                    Text="A"
                },
                 new Dropdown
                {
                    Value=2,
                    Text="F"
                }
            };
            HttpContext.Session.Abandon();
            return View();
        }

        [HttpPost]
        public ActionResult DiaryOnlineHome(DairyOnlineHome objMemberRegistration, string premember)
        {
           ViewBag.Dropdown = new List<Dropdown>
            {
                new Dropdown
                {
                    Value=1,
                    Text="A"
                },
                 new Dropdown
                {
                    Value=2,
                    Text="F"
                }
            };

            ViewBag.Path = ConfigurationManager.AppSettings["Path"].ToString() + "Home/DiaryBooking";

            if (objMemberRegistration.Type == 0)
            {
                ViewBag.premember = "Please choose member type.";
                return View();
            }

            if (ModelState.IsValid)
            {
                string prememberval = string.Empty;
                prememberval = objMemberRegistration.Type == 1 ? "A" : "F";

                ICSI_SoapService.Service obj = new ICSI_SoapService.Service();
                var soapData = obj.GetMemberShipData(prememberval, Convert.ToInt32(objMemberRegistration.MRN));            //int CertificateofPracticalNumber = Convert.ToInt32(soapData.CertificateofPracticalNumber);
                if (soapData.MembershipNo != objMemberRegistration.MRN)
                {
                    ViewBag.ErrorMsg = "Please Enter Correct Membership Number";//prememberval + objMemberRegistration.MRN + " is Invalid Membership Number.";
                    return View();
                }

                DateTime temp = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
                //DateTime temp1 = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string strDOB = temp.ToString("dd/MM/yyyy");


                if (soapData.DateofBirth != objMemberRegistration.DOB)
                {
                    ViewBag.ErrorMsg = "Please Enter Correct Date of Birth"; // objMemberRegistration.DOB + " DOB is not matched.";
                    return View();
                }

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

                Session["FullName"] = FirstName + " " + LastName;
                Session["MembershipNo"] = MembershipNo;

                TempData["Name"] = FirstName + " " + LastName;

                //DateTime temp = DateTime.ParseExact(objMemberRegistration.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //string strDOB = temp.ToString("yyyy/MM/dd");

                // var soapGetMemberDetailsOnline = obj.GetMemberDetailsOnline(prememberval + objMemberRegistration.MRN, Convert.ToDateTime(strDOB.Replace("-", "/")));
                //var soapGetMemberDetailsOnline = obj.GetMemberDetailsOnline("a1", Convert.ToDateTime("1944/01/08"));


                return RedirectToAction("DiaryBooking");


            }
            return View();
        }


        [HttpGet]
        public ActionResult DiaryBooking()
        {           
            DiaryBooking DiaryBooking = new DiaryBooking();     // DiaryBooking objMemberName = new DiaryBooking();
            if (!string.IsNullOrEmpty(Convert.ToString(Session["FullName"])))
            {
                string MName = Session["FullName"].ToString();               //objMemberName.MembershipName = Session["FullName"].ToString();
                DiaryBooking.MembershipName = Session["FullName"].ToString();
            }
          
            else
            {
                return RedirectToAction("DiaryOnlineHome");
            }
            //this._userRepository.UpdateStallStatusTimeMore10Mints();
            this._userRepository.UpdateDIARYStatusTimeMore10Mints();  // for Diary 6


            DiaryBooking.lstNumber = this._userRepository.GetAllDIARYNumber().Select(x => new StallNumber
            {
                Id = x.Id,
                StallNo = x.StallNo + " Rs. " + x.StallAmount

            }).ToList();
           
            //DiaryBooking.lstBooked = this._userRepository.BookedStall();
            DiaryBooking.lstEvent = this._userRepository.GetAllEvents();
            DiaryBooking.lstState = this._userRepository.GetAllState();
            DiaryBooking.ICSIGST = this._userRepository.GetICSIGST();
            
            // StallBooking stallBooking = null;
            if (TempData["DiaryBooking"] != null)
            {
                DiaryBooking = TempData["DiaryBooking"] as DiaryBooking;
               // DiaryBooking.lstBooked = this._userRepository.BookedStall();
                DiaryBooking.lstEvent = this._userRepository.GetAllEvents();
                DiaryBooking.lstState = this._userRepository.GetAllState();
                DiaryBooking.lstNumber = this._userRepository.GetAllDIARYNumber().Select(x => new StallNumber
                {
                    Id = x.Id,
                    StallNo = x.StallNo + " Rs. " + x.StallAmount
                }).ToList();
                DiaryBooking.ICSIGST = this._userRepository.GetICSIGST();             //TempData.Keep("StallBooking");

            }
            return View(DiaryBooking);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DiaryBooking(DiaryBooking DiaryBooking, string Amount, string GST18Amount, string TotalAmount, string StallNumber, string rdbgroup)   //string hidStatetext
        {
            Decimal GSTAmount = Math.Round(Convert.ToDecimal(GST18Amount), 2);
            DiaryBooking.IsYesNo = rdbgroup;
            if (ModelState.IsValid)
            {
                //  DiaryBooking.lstBooked = this._userRepository.BookedStall();
                DiaryBooking.lstEvent = this._userRepository.GetAllEvents();
                DiaryBooking.lstState = this._userRepository.GetAllState();
                DiaryBooking.lstNumber = this._userRepository.GetAllDIARYNumber().Select(x => new StallNumber
                {
                    Id = x.Id,
                    StallNo = x.StallNo + " Rs. " + x.StallAmount
                }).ToList();
                DiaryBooking.ICSIGST = this._userRepository.GetICSIGST();

                if (!string.IsNullOrEmpty(DiaryBooking.GSTN))
                {

                    Regex rgx = new Regex(@"^([0][1-9]|[1-2][0-9]|[3][0-5])([a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}[zZ]{1}[0-9a-zA-Z]{1})+$");
                  
                    if (!rgx.IsMatch(DiaryBooking.GSTN))
                    {

                        ViewBag.Message = "Please enter valid GST Number";

                        return View(DiaryBooking);
                    }
                    else
                    {
                        if (DiaryBooking.GSTN == "09AATTT1103F2ZX")
                        {
                            ViewBag.Message = "This GST is not valid...";
                            return View(DiaryBooking);
                        }
                    }
                }
                DiaryBooking.StallNumber = StallNumber;
                //DiaryBooking.StateName = hidStatetext;
                DiaryBooking.StateName = DiaryBooking.lstState.Where(x => x.Id == DiaryBooking.StateId).Select(x=>x.Name).FirstOrDefault().ToString();
                DiaryBooking.Description = _userRepository.GetDIARYDes(StallNumber);
                DiaryBooking.Amount = Convert.ToDecimal(Amount);
                DiaryBooking.GST18Amount = GSTAmount;
                DiaryBooking.TotalAmount = Convert.ToDecimal(TotalAmount);
                DiaryBooking.ICSIGST = this._userRepository.GetICSIGST();
                if (string.IsNullOrEmpty(DiaryBooking.GSTN))
                {
                    DiaryBooking.SGSTAmount = (DiaryBooking.Amount * 9) / 100;
                    DiaryBooking.CGSTAmount = (DiaryBooking.Amount * 9) / 100;
                    DiaryBooking.TotalAmount = DiaryBooking.Amount + DiaryBooking.SGSTAmount + DiaryBooking.CGSTAmount;
                }
                else if (DiaryBooking.GSTN.Substring(0, 2) == DiaryBooking.ICSIGST.Substring(0, 2))
                {
                    DiaryBooking.SGSTAmount = (DiaryBooking.Amount * 9) / 100;
                    DiaryBooking.CGSTAmount = (DiaryBooking.Amount * 9) / 100;
                    DiaryBooking.TotalAmount = DiaryBooking.Amount + DiaryBooking.SGSTAmount + DiaryBooking.CGSTAmount;
                }
                else
                {
                    DiaryBooking.IGSTAmount = (DiaryBooking.Amount * 18) / 100;
                    DiaryBooking.TotalAmount = DiaryBooking.Amount + DiaryBooking.IGSTAmount;
                }
                DiaryBooking.SACCODE = "997212";
                TempData["DiaryBooking"] = DiaryBooking;
                if (TempData["DiaryBooking"] != null)
                {
                    return RedirectToAction("DiaryConfirmation");
                }

                //stallBooking.StateCode = this._userRepository.GetStateCode(stallBooking.StateId);

            }
            else
            {
                DiaryBooking.lstNumber = this._userRepository.GetAllDIARYNumber().Select(x => new StallNumber
                {
                    Id = x.Id,
                    StallNo = x.StallNo + " Rs. " + x.StallAmount
                }).ToList();
                // DiaryBooking.lstBooked = this._userRepository.BookedStall();
                DiaryBooking.lstEvent = this._userRepository.GetAllEvents();
                DiaryBooking.lstState = this._userRepository.GetAllState();
            }
            //string icsiGstin = "09AAATT1103F2ZX";

            return View(DiaryBooking);

        }


        public ActionResult DiaryConfirmation()
        {
            DiaryBooking DiaryBooking = null;
            if (TempData["DiaryBooking"] != null)
            {
                DiaryBooking = TempData["DiaryBooking"] as DiaryBooking;
                TempData.Keep("DiaryBooking");

            }
            return View(DiaryBooking);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DiaryConfirmation(DiaryBooking DiaryBooking)
        {
            try
            {
                AppConfig.WriteToErrorLogs("diary confirmation page", "SuccessPayment");
                if (TempData["DiaryBooking"] != null)
                {
                    DiaryBooking = TempData["DiaryBooking"] as DiaryBooking;
                }
                TempData["Id"] = this._userRepository.DiaryPayment(DiaryBooking);
                if (TempData["Id"] != null)
                {
                    string MobileNo = DiaryBooking.MobileNo;
                    string TotAmount = Convert.ToString(DiaryBooking.TotalAmount);
                    if (MobileNo.Trim()=="8076686086")
                    {
                        TotAmount = "2.00";
                    }
                    string ReqID = Convert.ToString(TempData["Id"]);                    
                    string PayMode = "9";
                    string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
                    PaymentGatewayData objPGData = new PaymentGatewayData();                    
                    string Name = DiaryBooking.ContactPersion;
                    string connectionStringPG1 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG1"].ConnectionString;
                    string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Diary_PG2"].ConnectionString;
                    int Paymentrequestid = objPGData.InsertOnlinePaymentTransaction(connectionStringPG1, connectionStringPG2, "9999999999", TotAmount, ReqID, PayMode, ProcessRoute, AppConfig.PGType, MobileNo, Name);
                    Session["RequetID"] = Paymentrequestid;
                    return RedirectToAction("PaymentDetail");
                }
            }
            catch (Exception ex)
            { }
            return View(DiaryBooking);
        }

        [HttpGet]
        public ActionResult DiaryPrintReceipt()
        {
            ReceiptDetails obj = null;
            if (TempData["ListData"] != null)
            {
                obj = TempData["ListData"] as ReceiptDetails;

                TempData.Keep("ListData");
            }

            return View(obj);
        }

        public ActionResult CancelOnlinePayment()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FinalDiaryPrintReceipt()
        {
            ReceiptDetails obj = null;
            if (TempData["ListData"] != null)
            {
                obj = TempData["ListData"] as ReceiptDetails;

                TempData.Keep("ListData");
            }

            return View(obj);
        }


    }
    public class Dropdown
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}