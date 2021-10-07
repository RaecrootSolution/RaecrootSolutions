using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using PaymentGateway.Models;
using System.Collections;
using ICSI.PG.DB;
using System.Text;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace PaymentGateway.Controllers
{
    public class HomeController : Controller
    {
        private Database _database;
        private Dictionary<string, ICSI.PG.DB.Database> _databases;
        private StringBuilder sbResp = new StringBuilder();
        private SqlCommand sqlCmd = new SqlCommand();
        public HomeController()
        {
            DatabaseFactory.setConfiguration();
            _databases = DatabaseFactory.GetDatabases();
            _database = _databases["DEFAULT"];
        }

        string strPaymentData = string.Empty;
        string strMessage = string.Empty;
        string strKey = string.Empty;
        string strUserID = string.Empty;
        string strType = string.Empty;
        DataTable dtData = new DataTable();

        public ActionResult Index()
        {
            return View();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaymentRequest(FormCollection frm)
        {
            IDbConnection con = _database.CreateOpenConnection();
            Dictionary<string, string> dictParams = new Dictionary<string, string>();
            PGDetails pgDtl = new PGDetails();
            try
            {
                string epgdtl = frm["pgDetails"];
                string sskey = frm["sskey"];
                string pgdtlstr = Util.CryptographyUtil.DecryptDataPKCS7(epgdtl, sskey);
                pgDtl = JsonConvert.DeserializeObject<PGDetails>(pgdtlstr);
                SqlCommand cmd = Util.UtilService.CreateCommand("UPDATE PG_TRANSACTION_T SET PG_DETAILS_TX=@PG_DETAILS WHERE TRANSACTION_ID=@ID", con);
                cmd.Parameters.AddWithValue("@PG_DETAILS", pgdtlstr);
                cmd.Parameters.AddWithValue("@ID", pgDtl.StrTransId);
                cmd.ExecuteNonQuery();
                Session["PGOBJECT"] = pgDtl;

                string strRequestData = PGLogic.BuildReqData(con, pgDtl, ref dictParams);
                string url = string.Empty;
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                if (pgDtl != null && pgDtl.StrTransId != null)
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PaymentRequest exception: TransactionId - " + pgDtl.StrTransId + " Exception - " + ex.Message);
                }
                else
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PaymentRequest exception: TransactionId - Null, Exception - " + ex.Message);
                }
                sqlCmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }

            if (dictParams.ContainsKey("ID_MAX_DIGITS_NM")) dictParams.Remove("ID_MAX_DIGITS_NM");
            if (dictParams.ContainsKey("RETURN_INT_YN")) dictParams.Remove("RETURN_INT_YN");

            TempData["dictParams"] = dictParams;
            ViewBag.DictParams = TempData["dictParams"];
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PgResponse(string msg)
        {
            IDbConnection con = _database.CreateOpenConnection();
            Receipt receipt = new Receipt();
            string strHtml = string.Empty;
            string strTxStatus = string.Empty;
            int iTxStatusId = 0;
            PGDetails pgDtls = null;
            try
            {

                bool bRespReceived = false;
                if (!string.IsNullOrEmpty(msg))
                {
                    string[] arrResponse = msg.Split('|');
                    if (arrResponse.Length > 0)
                    {
                        bRespReceived = true;
                    }
                    else
                    {
                        bRespReceived = false;
                    }

                    string strCheckSum = arrResponse[arrResponse.Length - 1];
                    PGResponseDtls pgRespDtls = new PGResponseDtls();
                    pgRespDtls = PGLogic.GenerateResponseDtls(arrResponse);


                    List<Dictionary<string, object>> pgData = Util.UtilService.GetData("SELECT PG_DETAILS_TX FROM PG_TRANSACTION_T WHERE TRANSACTION_ID='" + pgRespDtls.strTransId + "'", con);

                    if (pgData != null && pgData.Count > 0)
                    {
                        pgDtls = JsonConvert.DeserializeObject<PGDetails>(Convert.ToString(pgData[0]["PG_DETAILS_TX"]));
                        Session["PGOBJECT"] = pgDtls;
                    }

                    bool isCheckSumValid = PGLogic.ValidateCheckSum(con, msg, strCheckSum, pgDtls);


                    if (pgRespDtls.strErrorStatus.Equals("NA"))
                    {
                        if (isCheckSumValid)
                        {
                            Dictionary<string, object> status = GetAuthStatus(con, pgRespDtls.strAuthStatus, Convert.ToInt32(pgDtls.StrBankMerchantId));
                            strTxStatus = Convert.ToString(status["Status"]);
                            iTxStatusId = Convert.ToInt32(status["StatusId"]);
                            if (iTxStatusId == 2)
                            {
                                receipt.isTxSuccess = true;
                            }
                            else
                            {
                                receipt.isTxSuccess = false;
                            }
                        }
                        else
                        {
                            receipt.strRespMsg = "Invalid CheckSum";
                            receipt.isTxSuccess = false;
                            iTxStatusId = 5;
                        }
                    }
                    else
                    {
                        receipt.strRespMsg = pgRespDtls.strErrorDescription;
                        receipt.isTxSuccess = false;
                        iTxStatusId = 6;
                    }

                    bool gst = false;
                    if (!string.IsNullOrEmpty(pgDtls.GstNm))
                    {
                        if (Convert.ToInt32(pgDtls.GstNm) > 0)
                        {
                            gst = true;
                        }
                    }
                    strHtml = PGLogic.ProcessResponseTransactions(con, receipt, pgDtls, pgRespDtls
                        , strTxStatus, iTxStatusId, isCheckSumValid, bRespReceived, msg, gst);
                }
                else
                {
                    strHtml = "<strong><p style='color:red'>Receipt is not generated for unknown reasons</strong>";
                }
            }
            catch (Exception ex)
            {
                strHtml = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                if (pgDtls != null && pgDtls.StrTransId != null)
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PgResponse exception: TransactionId - " + pgDtls.StrTransId + " Exception - " + ex.Message);
                }
                else
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PgResponse exception: TransactionId - Null, Exception - " + ex.Message);
                }
                sqlCmd.ExecuteNonQuery();
            }
            con.Close();
            receipt.html = strHtml;
            receipt.strEncryptedHTML = Util.CryptographyUtil.EncryptDataPKCS7(receipt.html, receipt.sskey);
            var jsonobj = new
            {
                strInitiate = strHtml,
                receipt = receipt
            };

            return View(receipt);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaymentDone(FormCollection frm)
        {
            IDbConnection con = _database.CreateOpenConnection();
            Receipt receipt = new Receipt();
            string strHtml = string.Empty;
            string strTxStatus = string.Empty;
            string msg = string.Empty;
            int iTxStatusId = 0;
            bool isCheckSumValid = false;
            PGDetails pgDtls = new PGDetails();
            Dictionary<string, string> dictFormKeys = new Dictionary<string, string>();
            try
            {
                StringBuilder sb = new StringBuilder();
                bool bRespReceived = false;
                StringBuilder sbResp = new StringBuilder();
                SqlCommand sqlCmd = new SqlCommand();
                if (frm != null && frm.HasKeys() && HttpContext.Request.Form != null && HttpContext.Request.Form.HasKeys() && HttpContext.Request.Form.Keys.Count > 0)
                {
                    bRespReceived = true;
                    string key = string.Empty;
                    for (int index = 0; index < frm.Keys.Count; index++)
                    {
                        key = frm.Keys[index];
                        dictFormKeys.Add(key, frm[key]);
                        sb.Append(key).Append("=").Append(frm[key]).Append("|");
                    }
                    sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                        .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                    sqlCmd = PaymentGateway.Util.UtilService.CreateCommand(sbResp.ToString(), con);
                    sqlCmd.Parameters.AddWithValue("@PG_TYPE", 2);
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "Response:" + sb.ToString());
                    sqlCmd.ExecuteNonQuery();

                }
                else
                {
                    bRespReceived = false;
                }

                //BUILD ICICI RESPONSE
                if (dictFormKeys.ContainsKey("Response Code") && dictFormKeys.ContainsKey("ReferenceNo"))
                {
                    PGResponseDtls pgRespDtls = new PGResponseDtls();
                    string strRespCode = Convert.ToString(dictFormKeys["Response Code"]);
                    pgRespDtls.strTransId = Convert.ToString(dictFormKeys["ReferenceNo"]);

                    msg = sb.ToString();
                    List<Dictionary<string, object>> pgData = Util.UtilService.GetData("SELECT PG_DETAILS_TX FROM PG_TRANSACTION_T WHERE TRANSACTION_ID='" + pgRespDtls.strTransId + "'", con);

                    if (pgData != null && pgData.Count > 0)
                    {
                        pgDtls = JsonConvert.DeserializeObject<PGDetails>(Convert.ToString(pgData[0]["PG_DETAILS_TX"]));
                        Session["PGOBJECT"] = pgDtls;
                    }

                    Dictionary<string, object> status = GetAuthStatus(con, strRespCode, Convert.ToInt32(pgDtls.StrBankMerchantId));

                    if (status != null)
                    {
                        strTxStatus = Convert.ToString(status["Status"]);
                        iTxStatusId = Convert.ToInt32(status["StatusId"]);
                        if (iTxStatusId == 2)
                        {
                            receipt.isTxSuccess = true;
                        }
                        else
                        {
                            receipt.isTxSuccess = false;
                        }
                    }
                    else
                    {
                        if (strRespCode.Equals("E000"))
                        {
                            strTxStatus = "Success";
                            iTxStatusId = 2;
                            receipt.isTxSuccess = true;
                        }
                        else
                        {
                            strTxStatus = "Fail";
                            iTxStatusId = 6;
                            receipt.isTxSuccess = false;
                        }
                    }



                    //BUILD PGResponseDetails OBJECT
                    pgRespDtls.strErrorStatus = strTxStatus;
                    if (dictFormKeys.ContainsKey("Unique Ref Number")) { pgRespDtls.strTxnReferenceNo = Convert.ToString(dictFormKeys["Unique Ref Number"]); } else { pgRespDtls.strTxnReferenceNo = string.Empty; };
                    if (dictFormKeys.ContainsKey("Transaction Amount")) { pgRespDtls.strTxnAmount = Convert.ToString(dictFormKeys["Transaction Amount"]); } else { pgRespDtls.strTxnAmount = "0"; };
                    if (dictFormKeys.ContainsKey("Transaction Date")) { pgRespDtls.strTxnDate = Convert.ToString(dictFormKeys["Transaction Date"]); } else { pgRespDtls.strTxnDate = string.Empty; };
                    if (dictFormKeys.ContainsKey("Payment Mode")) { pgRespDtls.strTxnType = Convert.ToString(dictFormKeys["Payment Mode"]); } else { pgRespDtls.strTxnType = string.Empty; };
                    if (dictFormKeys.ContainsKey("Total Amount")) { pgRespDtls.strTotalAmt = Convert.ToString(dictFormKeys["Total Amount"]); } else { pgRespDtls.strTotalAmt = string.Empty; };
                    if (dictFormKeys.ContainsKey("Processing Fee Amount")) { pgRespDtls.strProcessingFeeAmt = Convert.ToString(dictFormKeys["Processing Fee Amount"]); } else { pgRespDtls.strProcessingFeeAmt = "0"; };
                    if (dictFormKeys.ContainsKey("Service Tax Amount")) { pgRespDtls.strServiceTaxAmt = Convert.ToString(dictFormKeys["Service Tax Amount"]); } else { pgRespDtls.strServiceTaxAmt = string.Empty; };
                    if (dictFormKeys.ContainsKey("ID")) { pgRespDtls.strBankMerchantID = Convert.ToString(dictFormKeys["ID"]); } else { pgRespDtls.strBankMerchantID = string.Empty; };

                    string strRS = string.Empty;
                    string strRSVal = string.Empty;

                    if (dictFormKeys.ContainsKey("RS")) { strRS = Convert.ToString(dictFormKeys["RS"]); } else { strRS = string.Empty; };
                    if (dictFormKeys.ContainsKey("RSV")) { strRSVal = Convert.ToString(dictFormKeys["RSV"]); } else { strRSVal = string.Empty; };

                    string strTPS = string.Empty;
                    if (pgRespDtls.strTxnType.Equals("NET_BANKING") || pgRespDtls.strTxnType.Equals("CREDIT_CARD") || pgRespDtls.strTxnType.Equals("DEBIT_CARD"))
                    {
                        strTPS = "0";
                    }
                    else
                    {
                        strTPS = null;
                    }

                    //CHECK WHETHER TRANSACTION IS VALID BY COMPARING AMOUNT AND MERCHANT TRANSACTION ID
                    sb.Clear();
                    sb.Append(pgRespDtls.strBankMerchantID).Append("|")
                        .Append(strRespCode).Append("|")
                        .Append(pgRespDtls.strTxnReferenceNo).Append("|")
                        .Append(pgRespDtls.strServiceTaxAmt).Append("|")
                        .Append(pgRespDtls.strProcessingFeeAmt).Append("|")
                        .Append(pgRespDtls.strTotalAmt).Append("|")
                        .Append(pgRespDtls.strTxnAmount).Append("|")
                        .Append(pgRespDtls.strTxnDate).Append("|")
                        .Append(dictFormKeys["Interchange Value"].ToString()).Append("|")
                        .Append(strTPS).Append("|")
                        .Append(dictFormKeys["Payment Mode"].ToString()).Append("|")
                        .Append(dictFormKeys["SubMerchantId"].ToString()).Append("|")
                        .Append(pgRespDtls.strTransId);

                    isCheckSumValid = PGLogic.ValidateCheckSum(con, sb.ToString(), strRS, pgDtls);


                    bool gst = false;
                    if (!string.IsNullOrEmpty(pgDtls.GstNm))
                    {
                        if (Convert.ToInt32(pgDtls.GstNm) > 0)
                        {
                            gst = true;
                        }
                    }

                    strHtml = PGLogic.ProcessResponseTransactions(con, receipt, pgDtls, pgRespDtls
                        , strTxStatus, iTxStatusId, isCheckSumValid, bRespReceived, msg, gst);
                }

                else if (dictFormKeys.ContainsKey("razorpay_order_id") || dictFormKeys.ContainsKey("error[code]"))
                {

                    bRespReceived = true;
                    string strOrderId = string.Empty;
                    string strPaymentId = string.Empty;
                    string strSignature = string.Empty;
                    string strErrorDesc = string.Empty;
                    string strErrorCd = string.Empty;
                    string strTransId = string.Empty;
                    msg = sb.ToString();
                    PGResponseDtls pgRespDtls = new PGResponseDtls();


                    if (dictFormKeys.ContainsKey("razorpay_order_id"))
                    {
                        strOrderId = Convert.ToString(dictFormKeys["razorpay_order_id"]);
                    }
                    if (dictFormKeys.ContainsKey("razorpay_payment_id"))
                    {
                        strPaymentId = Convert.ToString(dictFormKeys["razorpay_payment_id"]);
                    }
                    if (dictFormKeys.ContainsKey("error[description]"))
                    {
                        strErrorDesc = Convert.ToString(dictFormKeys["error[description]"]);
                    }
                    if (dictFormKeys.ContainsKey("error[code]"))
                    {
                        strErrorCd = Convert.ToString(dictFormKeys["error[code]"]);
                    }
                    if (dictFormKeys.ContainsKey("razorpay_signature"))
                    {
                        strSignature = Convert.ToString(dictFormKeys["razorpay_signature"]);
                    }


                    //GET PGOBJECT FROM TRANSACTION
                    if (!string.IsNullOrEmpty(strOrderId))
                    {
                        List<Dictionary<string, object>> pgData = Util.UtilService.GetData("SELECT PG_DETAILS_TX,TRANSACTION_ID FROM PG_TRANSACTION_T WHERE ORDER_ID_TX='" + strOrderId + "'", con);

                        if (pgData != null && pgData.Count > 0)
                        {
                            pgDtls = JsonConvert.DeserializeObject<PGDetails>(Convert.ToString(pgData[0]["PG_DETAILS_TX"]));
                            strTransId = Convert.ToString(pgData[0]["TRANSACTION_ID"]);
                            Session["PGOBJECT"] = pgDtls;
                        }

                    }

                    if (!string.IsNullOrEmpty(strErrorCd) && !string.IsNullOrEmpty(strErrorDesc))
                    {
                        //TRANSACTION FAILED SET THE VALUES
                        pgRespDtls.strErrorDescription = strTxStatus = strErrorDesc;
                        iTxStatusId = 6;
                        receipt.isTxSuccess = false;
                        pgRespDtls.strErrorStatus = "6";
                    }
                    else
                    {
                        //TRANSACTION IS SUCCESS, GET PAYMENT RELATED INFORMATION

                        string strPGUrl = string.Empty, strUserName = string.Empty, strPassword = string.Empty;

                        //GET PAYMENT DETAILS
                        DataTable dtPGDtls = PGLogic.getMerchantIdDetails(con, Convert.ToInt32(pgDtls.StrBankMerchantId));

                        if (dtPGDtls != null && dtPGDtls.Rows != null && dtPGDtls.Rows.Count > 0)
                        {
                            DataRow row = dtPGDtls.Rows[0];
                            strPGUrl = Convert.ToString(row["GATEWAY_URL_TX"]);

                            if (!string.IsNullOrEmpty(Convert.ToString(row["PG_USER_NAME_TX"])))
                            {
                                strUserName = Convert.ToString(row["PG_USER_NAME_TX"]);
                            }
                            else
                            {
                                strUserName = string.Empty;
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(row["PG_PASSWORD_TX"])))
                            {
                                strPassword = Convert.ToString(row["PG_PASSWORD_TX"]);
                            }
                            else
                            {
                                strPassword = string.Empty;
                            }
                        }
                        Dictionary<string, object> paymentData = new Dictionary<string, object>();
                        paymentData = JsonConvert.DeserializeObject<Dictionary<string, object>>(PGLogic.CreateHDFCRequest(con, null, strPGUrl, "GET", "/payments/" + strPaymentId, strUserName, strPassword));

                        if (paymentData["error_code"] != null || paymentData["error_description"] != null)
                        {
                            pgRespDtls.strErrorDescription = strTxStatus = paymentData["error_description"].ToString();
                            iTxStatusId = 6;
                            pgRespDtls.strErrorStatus = "6";
                            receipt.isTxSuccess = false;
                        }
                        else
                        {
                            strTxStatus = "Success";
                            iTxStatusId = 2;
                            pgRespDtls.strErrorStatus = string.Empty;
                            receipt.isTxSuccess = true;
                        }



                        Dictionary<string, string> notesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentData["notes"].ToString());

                        pgRespDtls.strTxnReferenceNo = strPaymentId;
                        if (paymentData["amount"] == null)
                        {
                            pgRespDtls.strTxnAmount = string.Empty;
                        }
                        else
                        {
                            pgRespDtls.strTxnAmount = (Convert.ToInt32(paymentData["amount"]) / 100).ToString();
                        }

                        if (paymentData["created_at"] == null) pgRespDtls.strTxnDate = string.Empty;
                        else pgRespDtls.strTxnDate = paymentData["created_at"].ToString();

                        msg = strOrderId + "|" + strPaymentId + "|" + strSignature;

                        //if(paymentData["bank"] == null) pgRespDtls.strBankID = string.Empty;
                        //else pgRespDtls.strBankID = paymentData["bank"].ToString();

                        if (paymentData["tax"] == null) pgRespDtls.strServiceTaxAmt = string.Empty;
                        else pgRespDtls.strServiceTaxAmt = paymentData["tax"].ToString();

                        if (paymentData["fee"] == null) pgRespDtls.strProcessingFeeAmt = string.Empty;
                        else pgRespDtls.strProcessingFeeAmt = paymentData["fee"].ToString();

                        if (paymentData["amount"] == null) pgRespDtls.strTotalAmt = string.Empty;
                        else pgRespDtls.strTotalAmt = (Convert.ToInt32(paymentData["amount"]) / 100).ToString();

                        if (paymentData["method"] == null) pgRespDtls.strTxnType = string.Empty;
                        else pgRespDtls.strTxnType = paymentData["method"].ToString();

                        if (notesDict["transaction id"] == null) strTransId = string.Empty;
                        else strTransId = notesDict["transaction id"];

                        pgRespDtls.strCheckSum = strSignature;
                        pgRespDtls.strTransId = strTransId;

                        //CHECK WHETHER TRANSACTION IS VALID BY COMPARING AMOUNT AND MERCHANT TRANSACTION ID
                        if (strTransId == pgDtls.StrTransId)
                        {
                            isCheckSumValid = PGLogic.ValidateCheckSum(con, strOrderId + "|" + strPaymentId, pgRespDtls.strCheckSum, pgDtls);
                        }
                        else
                        {
                            isCheckSumValid = false;
                        }

                        if (!isCheckSumValid)
                        {
                            receipt.strRespMsg = "Invalid CheckSum";
                            receipt.isTxSuccess = false;
                            iTxStatusId = 5;
                        }


                        bool gst = false;
                        if (!string.IsNullOrEmpty(pgDtls.GstNm))
                        {
                            if (Convert.ToInt32(pgDtls.GstNm) > 0)
                            {
                                gst = true;
                            }
                        }
                        strHtml = PGLogic.ProcessResponseTransactions(con, receipt, pgDtls, pgRespDtls
                          , strTxStatus, iTxStatusId, isCheckSumValid, true, msg, gst);
                    }
                }
            }
            catch (Exception ex)
            {
                strHtml = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                if (pgDtls != null && pgDtls.StrTransId != null)
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PaymentDone exception: TransactionId - " + pgDtls.StrTransId + " Exception - " + ex.Message);
                }
                else
                {
                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "PaymentDone exception: TransactionId - Null, Exception - " + ex.Message);
                }
                sqlCmd.ExecuteNonQuery();
            }
            con.Close();
            receipt.html = strHtml;
            receipt.strEncryptedHTML = Util.CryptographyUtil.EncryptDataPKCS7(receipt.html, receipt.sskey);
            var jsonobj = new
            {
                strInitiate = strHtml,
                receipt = receipt
            };

            return View("PgResponse", receipt);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strAuthStatus"></param>
        /// <param name="pg_id"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetAuthStatus(IDbConnection con, string strAuthStatus, int pg_id)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<Dictionary<string, object>> statusData = Util.UtilService.GetData("SELECT ERROR_DESC_TX,STATUS_ID FROM PG_ERROR_CODES WHERE ERROR_CODE_TX = '" + strAuthStatus + "' AND PG_ID = " + pg_id, con);
                if (statusData != null && statusData.Count > 0)
                {
                    dict["Status"] = Convert.ToString(statusData[0]["ERROR_DESC_TX"]);
                    dict["StatusId"] = Convert.ToString(statusData[0]["STATUS_ID"]);
                }
                return dict;
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetAuthStatus exception: Exception - " + ex.Message);

                sqlCmd.ExecuteNonQuery();
                return null;
            }
        }

    }
}
