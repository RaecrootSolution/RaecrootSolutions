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
        public JsonResult GetAmount(int Id)
        {
            StallNumber lst = this._userRepository.GetAllStallNumber().Where(x => x.Id == Id).Select(x => new StallNumber
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
            stallBooking.lstBooked = this._userRepository.BookedStall();
            stallBooking.lstEvent = this._userRepository.GetAllEvents();
            stallBooking.lstState = this._userRepository.GetAllState();
            stallBooking.ICSIGST = this._userRepository.GetICSIGST();
            // StallBooking stallBooking = null;
            if (TempData["StallBooking"] != null)
            {
                stallBooking = TempData["StallBooking"] as StallBooking;
                stallBooking.lstBooked = this._userRepository.BookedStall();
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
                    stallBooking.lstBooked = this._userRepository.BookedStall();
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
                stallBooking.lstBooked = this._userRepository.BookedStall();
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
                paymentDetail = this._userRepository.GetPaymentDetail(Id);
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

            if (TempData["Id"] != null)
            {
                TempData["Amount"] = paymentDetail.TotalAmount.ToString("0.00");
                TempData["PayMode"] = "3";
                Id = Convert.ToInt32(TempData["Id"]);
                int stallnumberid = this._userRepository.GetStallNumberId(Id);
                this._userRepository.UpdateStallStatus(stallnumberid);
                TempData["Id"] = Id;
                Session["Id"] = Id;
                Session["stallnumberid"] = stallnumberid;
                //return RedirectToAction("PaymentReciept");
                //TempData["paymentDetail"] = paymentDetail;
                //TempData.Keep("paymentDetail");
                return RedirectToAction("GoToPG", "PaymentGateway");

            }
            return View(paymentDetail);
        }

        public ActionResult PaymentDone()
        {
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
            if (Session["Id"]!=null)
            {
                ReqId = Convert.ToInt32(Session["Id"]);
            }
            //ReqId = Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]]));
            int RegId = this._userRepository.GetRegId(ReqId);
            if (Session["Id"] != null)
            {
                RegId = Convert.ToInt32(Session["Id"]);
            }
            int StallNumberId = this._userRepository.GetStallNumberId(RegId);
            if (Session["stallnumberid"]!=null)
            {
                StallNumberId= Convert.ToInt32(Session["stallnumberid"]);
            }
            if (HttpContext.Request.Form[HttpContext.Request.Form.Keys[0].ToString()] == "E000")
            {
                // Success
                paymentstatus = Convert.ToInt32(ePaymentStatus.Succ);
                txncode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[0]]);
                txnid = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[1]]);
                string txdt = string.Empty;
                if (Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]).Contains("-"))
                {
                    string converteddt = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                    txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                }
                if (!string.IsNullOrEmpty(txdt))
                {
                    txnDt = Convert.ToDateTime(txdt);
                }
                txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
                this._userRepository.UpdateStallStatus(StallNumberId, 3);
                this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, true);
            }
            else
            {
                // Error
                paymentstatus = Convert.ToInt32(ePaymentStatus.Fail);
                txncode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[0]]);
                txnid = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[1]]);

                string txdt = string.Empty;
                if (Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]).Contains("-"))
                {
                    string converteddt = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                    txdt = converteddt.Split('-')[2].Substring(0, 4) + "-" + converteddt.Split('-')[1] + "-" + converteddt.Split('-')[0] + " " + converteddt.Split('-')[2].Replace(converteddt.Split('-')[2].Substring(0, 4), "").Trim();
                }

               // txnDt = Convert.ToDateTime(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]) == "null" ? 0 : Convert.ToDouble(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
                this._userRepository.UpdateStallStatus(StallNumberId, 4);
                this._userRepository.Update_STALL_TRANSACTION_T_ID(RegId, txnid, false);
            }
            PayMentBal objPmt = new PayMentBal();
            string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG2"].ConnectionString;
            int id = objPmt.UpdatePaymentBal(connectionStringPG2,txnid, txnDt, txncode, txnResponse, Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]])), paymentstatus, paymode, taxamt, processingfee, totamt,0);
            ViewBag.TxnID = txnid;
            ViewBag.Status = paymentstatus;
            TempData["TxnId"] = txnid;
            Session["TxnID_R"] = txnid;
            return View();
        }

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
            if (TempData["StallBooking"] != null)
            {
                stallBooking = TempData["StallBooking"] as StallBooking;
                TempData.Keep("StallBooking");

            }
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
            TempData["Id"] = this._userRepository.StallPayment(stallBooking);
            if (TempData["Id"] != null)
            {
                string ReqID = Convert.ToString(TempData["Id"]);               
                string TotAmount = Convert.ToString(stallBooking.TotalAmount);
                string PayMode = "9";
                string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
                PaymentGatewayData objPGData = new PaymentGatewayData();
                string MobileNo = stallBooking.MobileNo;
                string Name = stallBooking.ContactPersion;
                string connectionStringPG1 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG1"].ConnectionString;
                string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG2"].ConnectionString;
                int Paymentrequestid = objPGData.InsertOnlinePaymentTransaction(connectionStringPG1, connectionStringPG2,"9999999999",TotAmount, ReqID, PayMode, ProcessRoute, AppConfig.PGType, MobileNo, Name);
                Session["RequetID"] = Paymentrequestid;               
                return RedirectToAction("PaymentDetail");
            }
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
            if (Session["Id"] != null)
            {
                Id = Convert.ToInt32(Session["Id"]);
                lstReceipt = this._userRepository.GetStallReceipt(Id);
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

            return View();
        }
        [HttpPost]
        public ActionResult SearchReceiptTransaction(SearchReceiptTransaction obj)
        {
            TempData["ListDate"] = null;
            if (ModelState.IsValid)
            {
                var result = _userRepository.GetReceiptTransaction(obj);
                if (result != null)
                {
                    ViewBag.ErrorMsg = string.Empty;
                    TempData["ListData"] = result;
                    return RedirectToAction("StallPrintReceipt");
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

    }
}