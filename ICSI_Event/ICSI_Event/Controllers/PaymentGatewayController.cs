using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaymentGateway.Entity;
using PaymentGateway.BAL;
using ICSI_Event.Models;
using ICSI_Event.Config;
using System.Configuration;

namespace ICSI_Event.Controllers
{
    public class PaymentGatewayController : Controller
    {
        [HttpGet]
        public ActionResult GoToPG()
        {
            string RequestMsg = string.Empty;            
            string ReqID = Convert.ToString(TempData["Id"]);
            //string TotAmount = Convert.ToString(TempData["Amount"]);
            string TotAmount = Convert.ToString(Session["Amount"]);
            string PayMode = Convert.ToString(TempData["PayMode"]);
            PayMode = "9";            
            string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
            PaymentGatewayData objPGData = new PaymentGatewayData();            
            //TotAmount = "122.00";
            string MobileNo = Convert.ToString(Session["MobileNo"]);
            string Name = Convert.ToString(Session["Name"]);
            string connectionStringPG1 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG1"].ConnectionString;
            string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG2"].ConnectionString;
            PaymentTransactionEntity objTxnEntity = objPGData.GetPaymentRequestData(connectionStringPG1, connectionStringPG2,"9999999999",TotAmount, ReqID, PayMode, ProcessRoute, AppConfig.PGType, ref RequestMsg, MobileNo, Name,Convert.ToInt32(Session["RequetID"]));
            Session["RequetID"] = objTxnEntity.RequestID;
            return View("PGMiddleView", new PGMiddleURL { CheckoutUrl = RequestMsg });
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
            if (HttpContext.Request.Form[HttpContext.Request.Form.Keys[0].ToString()] == "E000")
            {
                // Success
                paymentstatus = Convert.ToInt32(ePaymentStatus.Succ);
                txncode = HttpContext.Request.Form[HttpContext.Request.Form.Keys[0].ToString()];
                txnid = HttpContext.Request.Form[HttpContext.Request.Form.Keys[1].ToString()];
                txnDt = Convert.ToDateTime(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
            }
            else
            {
                // Error
                paymentstatus = Convert.ToInt32(ePaymentStatus.Fail);
                txncode = HttpContext.Request.Form[HttpContext.Request.Form.Keys[0].ToString()];
                txnid = HttpContext.Request.Form[HttpContext.Request.Form.Keys[1].ToString()];
                txnDt = Convert.ToDateTime(HttpContext.Request.Form[HttpContext.Request.Form.Keys[6]]);
                txnResponse = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[13]]);
                paymode = Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[9]]);
                amt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[5]]);
                taxamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[2]]);
                processingfee = HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[3]]);
                totamt = HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]] == null ? 0 : Convert.ToInt32(HttpContext.Request.Form[HttpContext.Request.Form.Keys[4]]);
            }
            PayMentBal objPmt = new PayMentBal();
            string connectionStringPG2 = ConfigurationManager.ConnectionStrings["ICSI_Event_PG2"].ConnectionString;
            int id = objPmt.UpdatePaymentBal(connectionStringPG2,txnid, txnDt, txncode, txnResponse, Convert.ToInt32(Convert.ToString(HttpContext.Request.Form[HttpContext.Request.Form.Keys[11]])), paymentstatus, paymode, taxamt, processingfee, totamt,0);
            ViewBag.TxnID = txnid;
            ViewBag.Status = paymentstatus;
            return View();
        }
    }
}