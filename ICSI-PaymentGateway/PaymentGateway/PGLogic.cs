using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using System.Data;
using System.Drawing;
using System.Collections;
using PaymentGateway.Models;
using System.Web.Mvc;
using System.Web;

namespace PaymentGateway
{
    public class PGLogic
    {
        private static StringBuilder sbResp = new StringBuilder();
        private static SqlCommand sqlCmd = new SqlCommand();

        private string merchantID;
        private int customerID;
        private double txnAmount;
        private int currencyType;
        private int typeField1;
        private string securityID;
        private int typeField2;
        private string rU;
        private string checkSum;
        private string filler1;
        private string additionalInfo3;

        public string MerchantID { get => merchantID; set => merchantID = value; }
        public int CustomerrID { get => customerID; set => customerID = value; }
        public double TxnAmount { get => txnAmount; set => txnAmount = value; }
        public int CurrencyType { get => currencyType; set => currencyType = value; }
        public int TypeField1 { get => typeField1; set => typeField1 = value; }
        public string SecurityID { get => securityID; set => securityID = value; }
        public int TypeField { get => typeField2; set => typeField2 = value; }
        public string RU { get => rU; set => rU = value; }
        public string CheckSum { get => checkSum; set => checkSum = value; }
        public string Filler1 { get => filler1; set => filler1 = value; }
        public string AdditionalInfo3 { get => additionalInfo3; set => additionalInfo3 = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetHMACSHA256(string text, string key)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(key);
            byte[] message = encoder.GetBytes(text);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <returns></returns>
        public static string GetPGURL(IDbConnection con, PGDetails pgDtls)
        {
            string rUrl = string.Empty;
            string strRequestUrl = string.Empty;
            string strReturnURL = string.Empty;
            string strUserName = string.Empty;
            string strPassword = string.Empty;
            string strRequest = string.Empty;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string strPGUrl = string.Empty;
            string strCheckSum = string.Empty;
            try
            {
                DataTable dtPGDtls = PGLogic.getMerchantIdDetails(con, Convert.ToInt32(pgDtls.StrBankMerchantId));

                if (dtPGDtls != null && dtPGDtls.Rows != null && dtPGDtls.Rows.Count > 0)
                {
                    DataRow row = dtPGDtls.Rows[0];
                    strPGUrl = Convert.ToString(row["GATEWAY_URL_TX"]);
                    strReturnURL = Convert.ToString(row["RETURN_URL_TX"]);
                    if (!string.IsNullOrEmpty(Convert.ToString(row["CHECKSUM_KEY_TX"])))
                    {
                        strCheckSum = Convert.ToString(row["CHECKSUM_KEY_TX"]);
                    }
                    else
                    {
                        strCheckSum = string.Empty;
                    }
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
                DataTable dtPGAddDtls = new DataTable();
                dtPGAddDtls = PGLogic.getPgAddDetails(con, Convert.ToInt32(pgDtls.StrBankMerchantId));

                switch (pgDtls.StrPgmode)
                {
                    case "Billdesk":
                        strRequest = "msg=" + PGLogic.BuildBillDeskRequestData(con, pgDtls, dtPGAddDtls);
                        strRequestUrl = strPGUrl + "?" + strRequest;
                        break;
                    default:
                        break;
                }

                string query = "UPDATE PG_TRANSACTION_T SET REQUEST_DATA_TX = '" + strRequest + "' WHERE ID = " + pgDtls.StrReqId;
                SqlCommand cmd = new SqlCommand();
                cmd = PaymentGateway.Util.UtilService.CreateCommand(query, con);
                cmd.ExecuteNonQuery();
                rUrl = PGLogic.callPGURL(strRequestUrl, con);


            }
            catch (Exception ex)
            {
                string strMsg = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetPGUrl exception: TransactionId - " + pgDtls.StrReqId + " Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return rUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="textToEncrypt"></param>
        /// <returns></returns>
        public static string encrypttext(string key, string textToEncrypt)
        {
            try
            {
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Mode = CipherMode.ECB;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                rijndaelCipher.KeySize = 0x80;
                rijndaelCipher.BlockSize = 0x80;
                byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
                byte[] keyBytes = new byte[0x10];
                int len = pwdBytes.Length;
                if (len > keyBytes.Length)
                {
                    len = keyBytes.Length;
                }
                Array.Copy(pwdBytes, keyBytes, len);
                rijndaelCipher.Key = keyBytes;
                rijndaelCipher.IV = keyBytes;
                ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
                return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
            }
            catch (Exception ex)
            {

                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static string GenerateSHA512String(string strMsg)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(strMsg);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i <= hash.Length - 1; i++)
            {
                stringBuilder.Append(hash[i].ToString("x2"));
            }
            return stringBuilder.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRequestURL"></param>
        /// <returns></returns>
        public static string callPGURL(string strRequestURL, IDbConnection con)
        {
            string responseUri = string.Empty;
            try
            {
                HttpWebRequest pgRequest = WebRequest.Create(strRequestURL) as HttpWebRequest;
                pgRequest.Method = "POST";
                pgRequest.ContentType = "text/html";

                try
                {
                    HttpWebResponse pgResponse = pgRequest.GetResponse() as HttpWebResponse;
                    if (pgResponse.StatusCode.ToString() == "OK")
                    {
                        responseUri = pgResponse.ResponseUri.AbsoluteUri;
                    }

                    using (var streamReader = new StreamReader(pgResponse.GetResponseStream()))
                    {
                        string response = streamReader.ReadToEnd();
                    }

                }
                catch
                {
                    pgRequest.Abort();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "Call PG URL Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return responseUri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="iBankMerId"></param>
        /// <returns></returns>
        public static DataTable getMerchantIdDetails(IDbConnection con, int iBankMerId)
        {
            Hashtable htConditions = new Hashtable();
            DataTable dtReturnData = new DataTable();
            try
            {
                htConditions.Add("BANK_MERCHANT_ID", iBankMerId);
                dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_PG_DETAILS]", htConditions, con);
                if (dtReturnData.Rows != null && dtReturnData != null && dtReturnData.Rows.Count > 0)
                {
                    return dtReturnData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "getMerchantIdDetails Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <param name="dtPGDtls"></param>
        /// <returns></returns>
        public static string BuildBillDeskRequestData(IDbConnection con, PaymentGateway.Models.PGDetails pgDtls, DataTable dtPGDtls)
        {
            //BillDeskDtls billDeskDtls = new BillDeskDtls();
            string strPGParam = string.Empty;
            StringBuilder strPGRequest = new StringBuilder();
            string strGeneratedCheckSum = string.Empty;
            try
            {
                if (pgDtls != null && dtPGDtls != null && dtPGDtls.Rows != null && dtPGDtls.Rows.Count > 0)
                {
                    foreach (DataRow row in dtPGDtls.Rows)
                    {
                        strPGParam = Convert.ToString(row["PARAM_TEXT_TX"]);
                        if (Convert.ToInt32(row["STATIC_YN"]) != 1)
                        {
                            switch (strPGParam)
                            {
                                case "CUSTOMER_ID":
                                    strPGRequest.Append(pgDtls.StrTransId)
                                    .Append(row["SEPERATOR_TX"]);
                                    break;
                                case "TXN_AMOUNT":
                                    strPGRequest.Append(pgDtls.StrAmt)
                                   .Append(row["SEPERATOR_TX"]);
                                    break;
                                case "ADDITIONAL_INFO_1":
                                    strPGRequest.Append(pgDtls.StrMob)
                                   .Append(row["SEPERATOR_TX"]);
                                    break;
                                case "ADDITIONAL_INFO_2":
                                    strPGRequest.Append(pgDtls.StrEmail)
                                   .Append(row["SEPERATOR_TX"]);
                                    break;
                                case "ADDITIONAL_INFO_3":
                                    strPGRequest.Append(pgDtls.StrLocationIdentifier)
                                   .Append(row["SEPERATOR_TX"]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if (Convert.ToString(row["PARAM_TEXT_TX"]).Equals("CHECK_SUM"))
                            {
                                string strReq = strPGRequest.ToString();
                                strGeneratedCheckSum = GetHMACSHA256(strReq, Convert.ToString(row["PARAM_VALUE_TX"])).ToUpper();
                                strPGRequest.Append(row["SEPERATOR_TX"])
                                  .Append(GetHMACSHA256(strReq, Convert.ToString(row["PARAM_VALUE_TX"])).ToUpper());
                            }
                            else if (Convert.ToString(row["PARAM_TEXT_TX"]).Equals("RU"))
                            {
                                strPGRequest.Append(row["PARAM_VALUE_TX"]);
                                //strPGRequest.Append("http://localhost:56327/home/PgResponse");
                            }
                            else
                            {
                                strPGRequest.Append(row["PARAM_VALUE_TX"])
                                        .Append(row["SEPERATOR_TX"]);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(strGeneratedCheckSum))
                {
                    StringBuilder sbQuery = new StringBuilder();
                    SqlCommand cmd = new SqlCommand();
                    sbQuery.Append("UPDATE PG_TRANSACTION_T SET CHECKSUM_TX = '").Append(strGeneratedCheckSum)
                           .Append("' WHERE TRANSACTION_ID = '").Append(pgDtls.StrTransId).Append("'");
                    cmd = PaymentGateway.Util.UtilService.CreateCommand(sbQuery.ToString(), con);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "BuildBillDeskRequestData: Transaction Id - " + pgDtls.StrTransId + ", Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return strPGRequest.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strRespMsg"></param>
        /// <param name="strCheckSum"></param>
        /// <param name="pgDtls"></param>
        /// <returns></returns>
        internal static bool ValidateCheckSum(IDbConnection con, string strRespMsg, string strCheckSum, PGDetails pgDtls)
        {
            bool isValid = false;
            string strKey = string.Empty;
            string strGenCheckSum = string.Empty;
            try
            {

                string sqlQuery = "SELECT PGNAME_TX, CHECKSUM_KEY_TX, PG_PASSWORD_TX FROM PAYMENT_GATEWAY_T WHERE ID = " + pgDtls.StrBankMerchantId;
                List<Dictionary<string, object>> data = PaymentGateway.Util.UtilService.GetData(sqlQuery, con);
                foreach (var row in data)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(row["CHECKSUM_KEY_TX"])) || !string.IsNullOrEmpty(Convert.ToString(row["PG_PASSWORD_TX"])))
                    {
                        switch (row["PGNAME_TX"].ToString())
                        {
                            case "Billdesk":
                                string strMsg = strRespMsg.Replace("|" + strCheckSum, "");
                                strGenCheckSum = GetHMACSHA256(strMsg, Convert.ToString(row["CHECKSUM_KEY_TX"])).ToUpper();
                                break;
                            case "ICICI":
                                strGenCheckSum = GenerateSHA512String(strRespMsg + "|" + Convert.ToString(row["CHECKSUM_KEY_TX"]));
                                break;
                            case "HDFC":
                                strGenCheckSum = GetHMACSHA256(strRespMsg, Convert.ToString(row["PG_PASSWORD_TX"]));
                                break;
                            default:
                                break;
                        }

                        if (strCheckSum.Equals(strGenCheckSum))
                        {
                            isValid = true;
                        }
                        else
                        {
                            isValid = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "ValidateCheckSum Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();

            }
            finally
            {

            }
            return isValid;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="iBankMerId"></param>
        /// <returns></returns>
        public static DataTable getPgAddDetails(IDbConnection con, int iBankMerId)
        {
            //throw new NotImplementedException();
            Hashtable htConditions = new Hashtable();
            DataTable dtReturnData = new DataTable();
            try
            {
                htConditions.Add("BANK_MERCHANT_ID", iBankMerId);
                dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_PG_ADDITIONAL_DETAILS]", htConditions, con);
                if (dtReturnData.Rows != null && dtReturnData != null && dtReturnData.Rows.Count > 0)
                {
                    return dtReturnData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "getPgAddDetails Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="type"></param>
        /// <param name="sid"></param>
        /// <param name="offid"></param>
        /// <param name="scrnid"></param>
        /// <param name="iItemId"></param>
        /// <returns></returns>
        public static DataTable GetDataInfo(IDbConnection con, string type, string sid, string offid, string scrnid, int iItemId)
        {
            Hashtable htConditions = new Hashtable();
            DataTable dtReturnData = new DataTable();
            try
            {
                switch (type)
                {
                    case "initiate":
                        htConditions.Add("OFFICE_ID", offid);
                        htConditions.Add("SCREEN_ID", scrnid);
                        dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_PG_BY_OFFICEID]", htConditions, con);
                        break;
                    case "user":
                        htConditions.Add("USER_ID", sid);
                        htConditions.Add("ITEM_ID", iItemId);
                        dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_USER_BY_USERID]", htConditions, con);
                        break;
                    case "chapter":
                        htConditions.Add("OFFICE_ID", offid);
                        dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_CHAPTER_BY_OFFICEID]", htConditions, con);
                        break;
                    case "item":
                        htConditions.Add("ITEM_ID", offid);
                        dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_CHAPTER_BY_OFFICEID]", htConditions, con);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "getDataInfo Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return dtReturnData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <param name="strCustName"></param>
        /// <returns></returns>
        public static string GetPGConfirmHTML(IDbConnection con, Models.PGDetails pgDtls, string strCustName)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                //sb.Append("<div class='contain-all'><div class='container form-field'><form class='well form-horizontal' action=' ' method='post' id='contact_form'><fieldset id='fieldset'>")
                sb.Append("<div class='addTrainingStructure paymentStructure'><h4 class=''>Payment Details</h4></div>")
                .Append("<div class=''><div class='row'><div class='col-md-9'>")
                .Append("<p class='PaymentPara bg-success'>Your Request Id is:")
                .Append("<label class='PaymntLabel' id='lblId'>")
                .Append(pgDtls.StrAckId)
                .Append("</label></p>")
                .Append("<p class='PaymentPara bg-success'>Your transaction Id is:")
                .Append("<label class='PaymntLabel' id='lblTransId'>")
                .Append(pgDtls.StrTransId)
                .Append("</label></p>")
                .Append("<p class='PaymentPara bg-success'>Your payment of Rs.")
                .Append(Convert.ToString(Math.Round(Convert.ToDecimal(pgDtls.StrAmt), 2)))
                .Append(" has been processing for location ")
                .Append(pgDtls.StrOfficeName);

                sb.Append("<pre class='PaymentPara bg-success'>Your Request has been received.Please note this is for all future communication.<br>")
                  .Append("The request id and Transaction id is generated only for control purpose and before actual payment transaction starts.<br>")
                  .Append("This does not confirm that payment has been received.Please print your challan and take to your nearest Canara Bank branch for making payment.<br>")
                  .Append("ICSI has no responsibility for delay in paymeny due to any technical / non technical issues whatsover.</pre></div></div>");
                sb.Append("<div class='row'><div class='col-md-9'><ul class='list-unstyled'>")
                .Append("<li>Payment Mode :-  <label class='PaymntLabel' id='lblpgMode'>")
                .Append(pgDtls.StrPgmode)
                .Append("</label></li>")
                .Append("<li>Payment Type:-  <label class='PaymntLabel'>")
                .Append(pgDtls.StrItemType)
                .Append("</label></li>")
                .Append("<li>Name:-  <label class='PaymntLabel' id='lblStudentName'>")
                .Append(strCustName)
                .Append("</label></li>")
                .Append("<li>Mobile Number:- <label class='PaymntLabel' id='lblStudentMobile'>")
                .Append(pgDtls.StrMob)
                .Append("</label> </li>")
                .Append("<li>Email Address:-  <label class='PaymntLabel' id='lblStudentEmail'>")
                .Append(pgDtls.StrEmail)
                .Append("</label> </li>")
                .Append("<li>Amount:-  <label class='PaymntLabel' id='lblPayAmt'>")
                .Append("Rs. ")
                .Append(pgDtls.StrAmt)
                .Append("</label> </li>")
                .Append("</ul></div></div></div><br>");

                sb.Append("<div id = 'dvItemInfo'>")
                    .Append(GetItemInfo(con, pgDtls.Strofid, pgDtls.StrReqId))
                    .Append("</div >");
                sb.Append("<div class='row'><div class='form-group'><div class='col-sm-12 col-md-12 text-right'>")
                    .Append("<button type = 'button' id = 'btnConfirm' onclick =\"processpayment()\" class='btn btn-primary AppliedSearchBtn'>Proceed Payment</button>")
                    .Append("</div></div></div>")
                    .Append("<input type='hidden' id='hdnPgDtls' name='pgDtls'")
                    .Append("value=")
                    .Append(JsonConvert.SerializeObject(pgDtls))
                    .Append(">")
                    .Append("</fieldset></form></div></div>");
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "getPGConfirmHTML Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="receipt"></param>
        /// <param name="isGst"></param>
        /// <param name="isTxSuccess"></param>
        /// <returns></returns>
        internal static string GetPgReceipthtml(IDbConnection con, Receipt receipt, bool isGst, bool isTxSuccess)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (isTxSuccess)
                {
                    sb.Append("<div class='contain-all'><div class='container form-field'><form class='well form-horizontal' action=' ' method='post' id='contact_form'>");

                    if (isGst)
                    {

                        sb.Append("<section id='Icsi_top_header'><div class='container-fluid'><div class='header'><div class='LogoSpacing'><div class='header-part'>")
                        .Append("<div class='text-center' id='LogoCenter'><img id='imgLogo' ").Append("src='' alt='logo'></div></div></div></div></div></section>")
                        .Append("<div class=' '><div class='row'><div class='col-md-10 col-md-offset-1 text-center'><div>ICSI HOUSE, C-36, INSTITUTIONAL AREA, SECTOR-62, NOIDA, GAUTAM BUDDHA NAGAR <br>")
                        .Append("UTTAR PRADESH.- 201309 INDIA</div><div>Phone : (0120) 4522000,Website - www.icsi.edu, www.icsi.in Grievance Redressal Portal :http://support.icsi.edu</div>")
                        .Append("<div> <strong>GST Number : 09AAATT1103F2ZX </strong> </div></div><div class='col-md-10 col-md-offset-1 text-center'><h3><strong>TAX INVOICE <br> ORIGINAL FOR RECIPIENT</strong></h3>")
                        .Append("</div><div class='col-md-5 col-md-offset-1 no-padding'><ul class='list-unstyled'><li>Invoice No. : <label class='PaymntLabel'>").Append(receipt.RECEIPT_ID).Append("</label>")
                        .Append("</li><li>Reverse Charge : <label class='PaymntLabel'>NO</label></li><li>Customer Name : <label class='PaymntLabel'>").Append(receipt.CUSTOMER_NAME).Append("</label> </li>")
                        .Append("<li>GST/UIN No.: <label class='PaymntLabel'>27AALCS3949Q1ZC</label> </li>")
                        .Append("<li> Billing Address: <label class='PaymntLabel'>").Append(receipt.BILLING_ADDR).Append("</label> </li>")
                        .Append("</ul></div><div class='col-md-5 no-padding'><ul class='list-unstyled'>")
                        .Append("<li>Invoice Date: <label class='PaymntLabel'>").Append(receipt.RECEIPT_DATE_DT).Append("</label> </li>")
                        .Append("<li>Memb./Regn./Ref.No. :<label class='PaymntLabel'>").Append(receipt.REG_NUMBER_TX).Append("</label></li>")
                        .Append("<li>Shipping Address : <label class='PaymntLabel'>").Append(receipt.SHIPPING_ADDR).Append("</label></li>")
                        .Append("</ul></div></div>");

                        sb.Append(GetItemInfoForReceipt(con, receipt.ofid, receipt.strPgTranId, isGst, receipt.strAddInfo1));

                        sb.Append("<div class='row'><div class='col-xs-10 col-xs-offset-1 no-padding'><div class='col-xs-6'><strong> Receipt No. : ")
                          .Append(receipt.RECEIPT_ID).Append("</strong></div><div class='col-xs-6 text-right'> <strong> Receipt Date : ").Append(receipt.RECEIPT_DATE_DT).Append("</strong></div></div>")
                          .Append("<div class='col-xs-10 col-xs-offset-1'><p></p><p> <strong>Dear Sir/ Madam, </strong></p><p> <strong>We acknowledge with thanks the receipt of Online Payment (Bill Desk Transaction ID : ")
                          .Append(receipt.TRANSACTION_ID).Append("of Rs.").Append(Convert.ToString(Math.Round(Convert.ToDecimal(receipt.AMOUNT), 2))).Append("towards the above.</strong> </p>")
                          .Append("<p class='text-right'>For The Insititute of Company Secretaries of India</p>").Append("<p>This is computer generated invoice and do not require any stamp or signature.</p>")
                          .Append("</div><div class='col-xs-10 col-xs-offset-1 text-right'><button type='button' class='btn btn-primary'><span class='glyphicon glyphicon-download-alt'></span> Download PDF</button>")
                          .Append("<button type='button' class='btn btn-primary' style='margin-left:15px;'><span class='glyphicon glyphicon-print'></span> Print</button></div></div>");
                    }
                    else
                    {

                        sb.Append("<section id='Icsi_top_header'><div class='container-fluid'>")
                          .Append("<div class='header'><div class='LogoSpacing'><div class='header-part'><div class='text-center'><a href=''><img id='imgLogo'").Append("alt='logo'></a></div>")
                          //.Append("<div class='col-xs-12 text-center'>ICSI HOUSE,C 36,INSTITUTIONAL AREA,SECTOR-62 , Noida , Gautam Budh Nagar , Uttar Pradesh - 201309, India.</div>")
                          //.Append("<div class='col-xs-12 text-center'>PH: (0120) 4522000 </div>")
                          .Append("<div class='col-xs-12 text-center'>Website: <a href='#' class='urlText'>www.icsi.edu</a>, <a href='#' class='urlText'> www.icsi.in</a></div>")
                          .Append("<div class='col-xs-12 text-center'><small>For timely resolution of the complaints/ queries ICSI is now on a Single Grievance Redressal Portal i.e at  <a href='#' class=''>http://support.icsi.edu</a></small></div>")
                          .Append("<div class='col-xs-12 text-center'><label><small>GST No.09AAATT1103F2ZX</small></label></div></div></div></div></div><div class='mt-10'></div>")
                          .Append("<div class='row'><div class='col-xs-10 col-xs-offset-1 no-padding' style='float:left'><div class='col-xs-7 no-padding'><ul class='list-unstyled'>")
                          .Append("<li>Receipt No : <label class='PaymntLabel'>").Append(receipt.RECEIPT_ID).Append("</label></li>")
                          .Append("<li>Request ID: <label class='PaymntLabel'>").Append(receipt.REQUEST_ID).Append("</label></li>")
                          .Append("<li>Mr/Ms/M/S : <label class='PaymntLabel'>").Append(receipt.CUSTOMER_NAME).Append("</label></li>")
                          .Append("<li><p class='PDFPara'>Address : <strong>").Append(receipt.BILLING_ADDR).Append(" </ strong ></ p > ")
                          .Append("<p class='PDFPara'>Mobile: <strong>").Append(receipt.MOBILE).Append("</strong></p>")
                          .Append("<p class='PDFPara'>Email: <strong>").Append(receipt.EMAIL).Append("</strong></p></li></ul></div>")
                          .Append("<div class='col-xs-5 no-padding'><ul class='list-unstyled'><li>Receipt Date: <label class='PaymntLabel'>").Append(receipt.RECEIPT_DATE_DT).Append("</label>")
                          .Append("</li><li>Transaction ID: <label class='PaymntLabel'>").Append(receipt.TRANSACTION_ID).Append("</label></li>")
                          .Append("<li>Memb./Regn./Ref.NO : <label class='PaymntLabel'>").Append(receipt.REG_NUMBER_TX).Append("</label> </li></ul></div></div></div>");

                        sb.Append("<div class='col-xs-12 text-center urlText'><h3> <strong> Bill of Supply </strong></h3></div><div class='row'><div class='col-xs-10 col-xs-offset-1 no-padding'>")
                         .Append("<ul class='list-unstyled'><li>Dear Sir/Madam, <br/><br/> We acknowledge with thanks the receipt of online payment (Axis Bank) of Rs.")
                         .Append(Convert.ToString(Math.Round(Convert.ToDecimal(receipt.AMOUNT), 2))).Append(" towards the following fee(s) :</li></ul></div></div><div class='row'><div class='col-xs-10 col-xs-offset-1 no-padding'>");

                        sb.Append(GetItemInfoForReceipt(con, receipt.ofid, receipt.strPgTranId, isGst, receipt.strAddInfo1));


                    }
                    sb.Append("<div></form></div></div>");
                }
                else
                {
                    sb.Append("<div class='contain-all'><div class='container form-field'><form class='well form-horizontal' action=' ' method='post' id='contact_form'>")
                      .Append("<div class='addTrainingStructure paymentStructure'><h4 class=''>Payment Status</h4></div><div class=''><div class='row'><div class='col-md-9'>")
                      .Append("<p class='PaymentPara bg-success'>Your request Id is: <label>").Append(receipt.REQUEST_ID).Append("</label></p>")
                      .Append("<p class='PaymentPara bg-success'>Your transaction Id is: <label>").Append(receipt.TRANSACTION_ID).Append("</label></p>")
                      .Append("<p class='PaymentPara bg-success'>Your Payment of Rs. <label>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(receipt.AMOUNT), 2))).Append("</label> has been failed.</p></div></div>")
                      .Append("<div class='row'><div class='col-md-9'><ul class='list-unstyled'><li>Payment Type:- <label class='PaymntLabel'>Training</label>")
                      .Append("</li><li>Name:- <label class='PaymntLabel'>").Append(receipt.CUSTOMER_NAME).Append("</label></li>")
                      .Append("<li>Mobile Number:- <label class='PaymntLabel'>").Append(receipt.MOBILE).Append("</label> </li>")
                      .Append("<li>Email Address:- <label class='PaymntLabel'>").Append(receipt.EMAIL).Append("</label> </li>")
                      .Append("</ul></div></div></div></div></form></div> <!-- /.container -->");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "getPgReceiptHtml Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <param name="dictParamData"></param>
        /// <returns></returns>
        public static string BuildReqData(IDbConnection con, PGDetails pgDtls, ref Dictionary<string, string> dictParamData)
        {
            string strReqDataUrl = string.Empty;
            string strReturnURL = string.Empty;
            string strUserName = string.Empty;
            string strPassword = string.Empty;
            string strRequest = string.Empty;
            string strPGUrl = string.Empty;
            string strCheckSum = string.Empty;
            Boolean bAmt_Chk;
            try
            {
                //GET MERCHANT ID DETAILS
                DataTable dtPGDtls = PGLogic.getMerchantIdDetails(con, Convert.ToInt32(pgDtls.StrBankMerchantId));

                if (dtPGDtls != null && dtPGDtls.Rows != null && dtPGDtls.Rows.Count > 0)
                {
                    DataRow row = dtPGDtls.Rows[0];
                    strPGUrl = Convert.ToString(row["GATEWAY_URL_TX"]);
                    bAmt_Chk = Convert.ToBoolean(row["AMT_CHK_YN"]);
                    if (bAmt_Chk)
                    {
                        pgDtls.StrAmt = "2.00";
                    }
                    dictParamData["URL"] = strPGUrl;
                    strReturnURL = Convert.ToString(row["RETURN_URL_TX"]);
                    dictParamData["POST_YN"] = Convert.ToBoolean(row["POST_YN"]) ? "POST" : "GET";
                    dictParamData["ID_MAX_DIGITS_NM"] = Convert.ToString(row["ID_MAX_DIGITS_NM"]);
                    dictParamData["RETURN_INT_YN"] = Convert.ToString(Convert.ToInt32(row["RETURN_INT_YN"]));
                    if (!string.IsNullOrEmpty(Convert.ToString(row["CHECKSUM_KEY_TX"])))
                    {
                        strCheckSum = Convert.ToString(row["CHECKSUM_KEY_TX"]);
                    }
                    else
                    {
                        strCheckSum = string.Empty;
                    }
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

                //GET ADDITIONAL INFO DETAILS TO BUILD A REQUEST
                DataTable dtPGAddDtls = new DataTable();
                dtPGAddDtls = PGLogic.getPgAddDetails(con, Convert.ToInt32(pgDtls.StrBankMerchantId));

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                switch (pgDtls.StrPgmode)
                {
                    case "Billdesk":
                        strRequest = PGLogic.BuildBillDeskRequestData(con, pgDtls, dtPGAddDtls);
                        dictParamData.Add("msg", strRequest);
                        strRequest = "msg=" + strRequest;
                        strReqDataUrl = strPGUrl + "?" + strRequest;
                        break;
                    case "ICICI":
                        strRequest = PGLogic.BuildICICIRequestData(strCheckSum, con, pgDtls, dtPGAddDtls, ref dictParamData);
                        strReqDataUrl = strPGUrl + "?" + strRequest;
                        break;
                    case "HDFC":
                        string strOrderId = PGLogic.CreateOrder(con, pgDtls, strPGUrl, strReturnURL, strUserName, strPassword);
                        StringBuilder sbReq = new StringBuilder();
                        if (!string.IsNullOrEmpty(strOrderId))
                        {
                            dictParamData["URL"] = "https://api.razorpay.com/v1/checkout/embedded";
                            dictParamData.Add("key_id", strUserName);
                            dictParamData.Add("order_id", strOrderId);
                            dictParamData.Add("amount", pgDtls.StrAmt);
                            dictParamData.Add("name", pgDtls.StrCustomerName);
                            dictParamData.Add("description", pgDtls.StrRemarks);
                            dictParamData.Add("image", "https://cdn.razorpay.com/logos/BUVwvgaqVByGp2_large.png");
                            dictParamData.Add("prefill[name]", pgDtls.StrCustomerName);
                            dictParamData.Add("prefill[contact]", pgDtls.StrMob);
                            dictParamData.Add("prefill[email]", pgDtls.StrEmail);
                            dictParamData.Add("notes[shipping address]", pgDtls.Shipping);
                            dictParamData.Add("notes[transaction id]", pgDtls.StrTransId);
                            dictParamData.Add("callback_url", strReturnURL);
                            dictParamData.Add("cancel_url", "https://stimulate.icsi.edu/");

                            foreach (var keyVal in dictParamData)
                            {
                                sbReq.Append(keyVal.Key).Append("=").Append(keyVal.Value).Append("|");
                            }
                            strRequest = sbReq.ToString();
                        }
                        else
                        {
                            dictParamData.Add("Error:", "Unable to process your payment. Please contact administrator");
                            strRequest = "Error occurred in generating the order id";
                        }
                        break;
                    default:
                        break;
                }

                string query = "UPDATE PG_TRANSACTION_T SET REQUEST_DATA_TX = '" + strRequest + "' WHERE ID = " + pgDtls.StrReqId;
                SqlCommand cmd = new SqlCommand();
                cmd = PaymentGateway.Util.UtilService.CreateCommand(query, con);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "BuildReqData Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                string strMsg = ex.Message;
            }
            return strReqDataUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <param name="strPGUrl"></param>
        /// <param name="strReturnURL"></param>
        /// <param name="strUserName"></param>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        internal static string CreateOrder(IDbConnection con, PGDetails pgDtls, string strPGUrl, string strReturnURL, string strUserName, string strPassword)
        {
            //Creating a new order for each transaction
            string orderId = string.Empty;
            string strResponse = string.Empty;
            Dictionary<string, object> orderResp = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                data.Add("amount", (int)(Decimal.Parse(pgDtls.StrAmt) * 100));
                data.Add("receipt", pgDtls.StrTransId);
                data.Add("currency", "INR");
                data.Add("payment_capture", "1");


                strResponse = CreateHDFCRequest(con, data, strPGUrl, "POST", "/orders", strUserName, strPassword);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    orderResp = JsonConvert.DeserializeObject<Dictionary<string, object>>(strResponse);
                }
                if (orderResp.Count > 0 && orderResp != null)
                {
                    orderId = Convert.ToString(orderResp["id"]);
                }
                else
                {
                    orderId = string.Empty;
                }

                //UPDATE ORDER ID IN PAYMENT_GATEWAY_T

                SqlCommand cmd = Util.UtilService.CreateCommand("UPDATE PG_TRANSACTION_T SET ORDER_ID_TX= @ORDER_ID_TX WHERE TRANSACTION_ID=@ID", con);
                cmd.Parameters.AddWithValue("@ORDER_ID_TX", orderId);
                cmd.Parameters.AddWithValue("@ID", pgDtls.StrTransId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "CreateOrder Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return orderId;
        }

        internal static string CreateHDFCRequest(IDbConnection con, Dictionary<string, object> data, string PGUrl, string strType, string methodName, string strUserName, string strPassword)
        {
            string response = string.Empty;

            try
            {
                HttpWebRequest webRequest = WebRequest.Create(PGUrl + methodName) as HttpWebRequest;
                webRequest.Method = strType;
                webRequest.PreAuthenticate = true;
                webRequest.ContentType = "application/json";
                webRequest.Credentials = new NetworkCredential(strUserName, strPassword);
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                if (strType.Equals("POST"))
                {
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                        var httpResponse = (HttpWebResponse)webRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            response = streamReader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        response = streamReader.ReadToEnd();
                    }
                }

            }
            catch (Exception ex)
            {
                //webRequest.Abort();
                string strErrMsg = ex.Message;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "CreateHDFCRequest Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCheckSum"></param>
        /// <param name="con"></param>
        /// <param name="pgDtls"></param>
        /// <param name="dtPGAddDtls"></param>
        /// <param name="reqData"></param>
        /// <returns></returns>
        private static string BuildICICIRequestData(string strCheckSum, IDbConnection con, PGDetails pgDtls, DataTable dtPGAddDtls, ref Dictionary<string, string> reqData)
        {
            string strPGParam = string.Empty;
            string strSeparator = string.Empty;
            StringBuilder strPGRequest = new StringBuilder();
            StringBuilder strPGRequest1 = new StringBuilder();
            string strGeneratedCheckSum = string.Empty;
            string strEncryptReq = string.Empty;
            int numberOfDigits = 17;
            int formatType = 0;
            try
            {
                if (reqData.ContainsKey("ID_MAX_DIGITS_NM")) numberOfDigits = Convert.ToInt32(reqData["ID_MAX_DIGITS_NM"]);
                if (reqData.ContainsKey("RETURN_INT_YN")) formatType = Convert.ToInt32(reqData["RETURN_INT_YN"]);
                if (pgDtls != null && dtPGAddDtls != null && dtPGAddDtls.Rows != null && dtPGAddDtls.Rows.Count > 0)
                {
                    foreach (DataRow row in dtPGAddDtls.Rows)
                    {
                        strSeparator = Convert.ToString(row["SEPERATOR_TX"]);
                        if (strSeparator == "&")
                        {
                            var values = dtPGAddDtls.AsEnumerable().Where(x => x.Field<int>("REF_ID") == Convert.ToInt32(row["ID"])).OrderBy(x => x.Field<int>("ORDER_NM")).ToList();

                            strPGRequest.Append(Convert.ToString(row["PARAM_NAME_TX"]));
                            strPGRequest1.Append(Convert.ToString(row["PARAM_NAME_TX"]));
                            strPGRequest.Append("=");
                            strPGRequest1.Append("=");
                            if (!string.IsNullOrEmpty(Convert.ToString(row["PARAM_VALUE_TX"])) && Convert.ToInt32(row["REF_ID"]) == 0)
                            {

                                strPGParam = Convert.ToString(row["PARAM_TEXT_TX"]);
                                if (Convert.ToInt32(row["STATIC_YN"]) != 1)
                                {
                                    switch (strPGParam)
                                    {
                                        case "CUSTOMER_ID":
                                            strPGRequest.Append(encrypttext(strCheckSum, pgDtls.StrTransId))
                                            .Append(row["SEPERATOR_TX"]);
                                            reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), encrypttext(strCheckSum, pgDtls.StrTransId));
                                            strPGRequest1.Append(pgDtls.StrTransId)
                                            .Append(row["SEPERATOR_TX"]);
                                            break;
                                        case "TXN_AMOUNT":
                                            strPGRequest.Append(encrypttext(strCheckSum, pgDtls.StrAmt))
                                           .Append(row["SEPERATOR_TX"]);
                                            reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), encrypttext(strCheckSum, pgDtls.StrAmt));
                                            strPGRequest1.Append(pgDtls.StrAmt)
                                                .Append(row["SEPERATOR_TX"]);
                                            break;
                                        case "ADDITIONAL_INFO_1":
                                            strPGRequest.Append(encrypttext(strCheckSum, pgDtls.StrMob))
                                           .Append(row["SEPERATOR_TX"]);
                                            strPGRequest1.Append(pgDtls.StrMob)
                                                .Append(row["SEPERATOR_TX"]);
                                            reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), encrypttext(strCheckSum, pgDtls.StrMob));
                                            break;
                                    }
                                }
                                else
                                {
                                    if (strPGParam.Equals("MERCHANT_ID"))
                                    {
                                        strPGRequest.Append(Convert.ToString(row["PARAM_VALUE_TX"]));
                                        strPGRequest1.Append(Convert.ToString(row["PARAM_VALUE_TX"]));
                                        reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), Convert.ToString(row["PARAM_VALUE_TX"]));
                                    }
                                    else
                                    {
                                        strPGRequest.Append(encrypttext(strCheckSum, Convert.ToString(row["PARAM_VALUE_TX"])));
                                        strPGRequest1.Append(Convert.ToString(row["PARAM_VALUE_TX"]));
                                        reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), encrypttext(strCheckSum, Convert.ToString(row["PARAM_VALUE_TX"])));
                                    }
                                    if (!strPGParam.Equals("PAY_MODE"))
                                    {
                                        strPGRequest.Append(row["SEPERATOR_TX"]);
                                        strPGRequest1.Append(row["SEPERATOR_TX"]);
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    StringBuilder sbDtlVal = new StringBuilder();
                                    StringBuilder sbDtlVal1 = new StringBuilder();
                                    if (values != null && values.Count > 0)
                                    {
                                        DataTable dtDtls = values.CopyToDataTable();
                                        foreach (DataRow drRow in dtDtls.Rows)
                                        {
                                            strPGParam = Convert.ToString(drRow["PARAM_TEXT_TX"]);
                                            if (Convert.ToInt32(drRow["STATIC_YN"]) != 1)
                                            {
                                                switch (strPGParam)
                                                {
                                                    case "CUSTOMER_ID":
                                                        sbDtlVal.Append(pgDtls.StrTransId)
                                                        .Append(drRow["SEPERATOR_TX"]);
                                                        sbDtlVal1.Append(pgDtls.StrTransId)
                                                        .Append(drRow["SEPERATOR_TX"]);
                                                        break;
                                                    case "TXN_AMOUNT":
                                                        sbDtlVal.Append(pgDtls.StrAmt)
                                                       .Append(drRow["SEPERATOR_TX"]);
                                                        sbDtlVal1.Append(pgDtls.StrAmt)
                                                       .Append(drRow["SEPERATOR_TX"]);
                                                        break;
                                                    case "ADDITIONAL_INFO_1":
                                                        sbDtlVal.Append(pgDtls.StrMob);
                                                        sbDtlVal1.Append(pgDtls.StrMob);
                                                        break;
                                                    case "CUSTOMER_NAME":
                                                        sbDtlVal.Append(pgDtls.StrCustomerName)
                                                      .Append(drRow["SEPERATOR_TX"]);
                                                        sbDtlVal1.Append(pgDtls.StrCustomerName)
                                                       .Append(drRow["SEPERATOR_TX"]);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                sbDtlVal.Append(drRow["PARAM_VALUE_TX"])
                                                           .Append(drRow["SEPERATOR_TX"]);
                                                sbDtlVal1.Append(drRow["PARAM_VALUE_TX"])
                                                          .Append(drRow["SEPERATOR_TX"]);
                                            }
                                        }
                                    }

                                    strPGRequest.Append(encrypttext(strCheckSum, sbDtlVal.ToString())).Append("&");
                                    strPGRequest1.Append(sbDtlVal1.ToString()).Append("&");
                                    reqData.Add(Convert.ToString(row["PARAM_NAME_TX"]), encrypttext(strCheckSum, sbDtlVal.ToString()));
                                }
                                catch (Exception ex)
                                {
                                    sbResp.Clear();
                                    sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                                     .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                                    sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                                    sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                                    sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "BuildICICIRequestData Exception1 - " + ex.Message);
                                    sqlCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "BuildICICIRequestData Exception2 - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return strPGRequest.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strType"></param>
        /// <param name="sid"></param>
        /// <param name="offid"></param>
        /// <param name="scrnId"></param>
        /// <param name="iItemId"></param>
        /// <param name="pgDtls"></param>
        /// <param name="bInfo"></param>
        /// <returns></returns>
        public static string GetPGModeHTML(IDbConnection con, string strType, string sid, string offid, string scrnId, int iItemId, Models.PGDetails pgDtls, bool bInfo)
        {

            StringBuilder sb = new StringBuilder();
            try
            {
                //sb.Append("<div class='contain-all'><div class='container form-field'><form class='well form-horizontal' action=' ' method='post' id='contact_form'><fieldset id='fieldset'>");
                if (!bInfo)
                {
                    //USER INFORMATION HTML CONSTRUCTOR

                    sb.Append("<div class='addTrainingStructure paymentStructure'><h4 class=''>User information</h4></div>")
                      .Append("<div class=''>")
                      .Append("<div class='row'><div class='col-xs-12 mb-10'><div id='dvSuccess'class='text-success'></div><div id='dvFail' class='text-danger'></div></div></div>")
                      .Append("<div class='row'><div class='form-group'><label class='col-md-3 AddTrainingStructrLabel control-label FormtxtLeft'>Email Address *</label>")
                      .Append("<div class='col-md-4'><div class='input-group col-md-12 col-xs-11 col-sm-11 IpadInput mobilInputAddT'><input name='USER_EMAIL_TX' id='txtEmail' placeholder='Enter email address ' class='form-control' type='text'>")
                      .Append("</div></div></div></div><div class='row'><div class='form-group'><label class='col-md-3 AddTrainingStructrLabel control-label FormtxtLeft'>Mobile *</label>")
                      .Append("<div class='col-md-4'><div class='input-group col-md-12 col-xs-11 col-sm-11 IpadInput mobilInputAddT'><input name='USER_MOBILE_NM' id='txtMobile' placeholder='Enter mobile number ' class='form-control' type='text'>")
                      .Append("</div></div></div></div><div class='row'><div class='form-group'><label class='col-md-3 AddTrainingStructrLabel control-label FormtxtLeft'>Billing Address *</label>")
                      .Append("<div class='col-md-4'><div class='input-group col-md-12 col-xs-11 col-sm-11 IpadInput mobilInputAddT'><textarea name='USER_BILLING_ADDR_TX' id='txtBillingAddr' placeholder='Enter billing address' class='form-control' rows='5' cols='100'></textarea>")
                      .Append("</div></div></div></div><div class='row'><div class='form-group'><label class='col-md-3 AddTrainingStructrLabel control-label FormtxtLeft'>Shipping Address *</label>")
                      .Append("<div class='col-md-4'><div class='input-group col-md-12 col-xs-11 col-sm-11 IpadInput mobilInputAddT'><textarea name='USER_SHIPPING_ADDR_TX' id='txtShippingAddr' placeholder='Enter shipping address ' class='form-control' rows='5' cols='100'></textarea>")
                      .Append("</div></div></div></div></div><br>");
                }

                //PAYMENT GATEWAY HTML CONSTRUCTOR   
                sb.Append("<div class='addTrainingStructure paymentStructure'><h4 class=''>Choose payment mode / gateway</h4></div>");
                sb.Append("<div class='row' id='dvPGModes'>");

                DataTable dtPgModes = PGLogic.GetDataInfo(con, strType, sid, offid, scrnId, 0);
                if (dtPgModes != null && dtPgModes.Rows != null && dtPgModes.Rows.Count > 0)
                {
                    //sb.Append("<div class='col-xs-12'>");
                    sb.Append("<div class='form-inline'><label class='col-md-3 PaymentModeLabel ROChapterLabl control-label TraingSchdlLeftLablTxt PaymentModeLabel FormInputsLabel'>")
                      .Append("Payment Mode/Gateway <span class='RedStar'>*</span></label><div class='col-md-4'><div class='input-group col-md-7 col-xs-11 selectCalndrForm selectoptionBgPayment panelInputsSub selectoptionBg'>")
                      .Append("<select class='form-control panelInputs' id='selMode' name='selMode'><option value='0'>Please Select</option>");
                    // int i = 0;
                    foreach (DataRow row in dtPgModes.Rows)
                    {
                        sb.Append("<option value='")
                            .Append(Convert.ToString(row["ID"]))
                            .Append(";")
                            .Append(Convert.ToString(row["PGNAME_TX"]))
                            .Append("'>")
                            .Append(Convert.ToString(row["PGNAME_TX"]));
                        sb.Append("</option>");
                    }
                    sb.Append("</select></div></div></div>");

                }
                else
                {
                    sb.Append("<br /><span><strong>No PaymentGateway Configured for this Screen! Please contact System Administrator</strong></span>");
                }

                sb.Append("</div><div class='row mt-30'><div class='col-sm-12 col-md-12 text-right'>") //LoadProceedScreen('confirm')//").Append(pgDtls.StrReturnURL).Append("
                .Append("<button type='submit' id='btnProceedNext' onclick=\"return LoadProceedScreen('confirm')\" class='btn btn-primary AppliedSearchBtn'>Next</button>")
                .Append("</div></div><div class='row'><div class='col-xs-12' style='margin-top:20px;'><p class='PaymentModePara'>* Bill Desk provides option to pay using Credit Card, Debit Card, Debit")
                .Append("Card + ATM PIN, Internet Banking, Wallet/Cash Cards.</p>")
                .Append("<p class='PaymentModePara'>* Axis Bank provdies option to pay using Debit.Credit Card only.</p>")
                .Append("<p class='PaymentModePara'>* Challan can be submitted to any Branch of Canara bank.</p>")
                .Append("</div></div>");//</fieldset></form></div></div>");
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetPGModeHTML Exception2 - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtUserInfo"></param>
        /// <returns></returns>
        public static Models.PGDetails UserInfo(DataTable dtUserInfo)
        {
            //throw new NotImplementedException();
            Models.PGDetails pgDtls = new Models.PGDetails();
            foreach (DataRow row in dtUserInfo.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(row["STUDENT_NAME_TX"])))
                    pgDtls.StrCustomerName = Convert.ToString(row["STUDENT_NAME_TX"]);
                else
                    pgDtls.StrCustomerName = "ADMIN";

                if (!string.IsNullOrEmpty(Convert.ToString(row["MOBILE_TX"])))
                    pgDtls.StrMob = Convert.ToString(row["MOBILE_TX"]);
                else
                    pgDtls.StrMob = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["EMAIL_ID"])))
                    pgDtls.StrEmail = Convert.ToString(row["EMAIL_ID"]);
                else
                    pgDtls.StrEmail = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["REG_NUMBER_TX"])))
                    pgDtls.StrRegNumber = Convert.ToString(row["REG_NUMBER_TX"]);
                else
                    pgDtls.StrRegNumber = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_1"])))
                    pgDtls.StrAddr1 = Convert.ToString(row["ADDR_LINE_1"]);
                else
                    pgDtls.StrAddr1 = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_2"])))
                    pgDtls.StrAddr2 = Convert.ToString(row["ADDR_LINE_2"]);
                else
                    pgDtls.StrAddr2 = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_3"])))
                    pgDtls.StrAddr3 = Convert.ToString(row["ADDR_LINE_3"]);
                else
                    pgDtls.StrAddr3 = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["CITY_NAME_TX"])))
                    pgDtls.StrCity = Convert.ToString(row["CITY_NAME_TX"]);
                else
                    pgDtls.StrCity = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["DISTRICT_NAME_TX"])))
                    pgDtls.StrDistrict = Convert.ToString(row["DISTRICT_NAME_TX"]);
                else
                    pgDtls.StrDistrict = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["STATE_NAME_TX"])))
                    pgDtls.StrState = Convert.ToString(row["STATE_NAME_TX"]);
                else
                    pgDtls.StrState = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["PIN_CODE_TX"])))
                    pgDtls.StrPinCd = Convert.ToString(row["PIN_CODE_TX"]);
                else
                    pgDtls.StrPinCd = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(row["SESSION_KEY"])))
                    pgDtls.StrSessionKey = Convert.ToString(row["SESSION_KEY"]);
                else
                    pgDtls.StrSessionKey = string.Empty;
            }
            return pgDtls;
        }

        public static void UserInfo(DataTable dtUserInfo, Models.PGDetails pgDtls)
        {
            //throw new NotImplementedException();            
            foreach (DataRow row in dtUserInfo.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(row["STUDENT_NAME_TX"])))
                    pgDtls.StrCustomerName = Convert.ToString(row["STUDENT_NAME_TX"]);
                else
                    pgDtls.StrCustomerName = "ADMIN";

                if (!string.IsNullOrEmpty(Convert.ToString(row["MOBILE_TX"])))
                    pgDtls.StrMob = Convert.ToString(row["MOBILE_TX"]);
                else
                    pgDtls.StrMob = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["EMAIL_ID"])))
                    pgDtls.StrEmail = Convert.ToString(row["EMAIL_ID"]);
                else
                    pgDtls.StrEmail = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["REG_NUMBER_TX"])))
                    pgDtls.StrRegNumber = Convert.ToString(row["REG_NUMBER_TX"]);
                else
                    pgDtls.StrRegNumber = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_1"])))
                    pgDtls.StrAddr1 = Convert.ToString(row["ADDR_LINE_1"]);
                else
                    pgDtls.StrAddr1 = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_2"])))
                    pgDtls.StrAddr2 = Convert.ToString(row["ADDR_LINE_2"]);
                else
                    pgDtls.StrAddr2 = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["ADDR_LINE_3"])))
                    pgDtls.StrAddr3 = Convert.ToString(row["ADDR_LINE_3"]);
                else
                    pgDtls.StrAddr3 = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["CITY_NAME_TX"])))
                    pgDtls.StrCity = Convert.ToString(row["CITY_NAME_TX"]);
                else
                    pgDtls.StrCity = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["DISTRICT_NAME_TX"])))
                    pgDtls.StrDistrict = Convert.ToString(row["DISTRICT_NAME_TX"]);
                else
                    pgDtls.StrDistrict = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["STATE_NAME_TX"])))
                    pgDtls.StrState = Convert.ToString(row["STATE_NAME_TX"]);
                else
                    pgDtls.StrState = string.Empty;

                if (!string.IsNullOrEmpty(Convert.ToString(row["PIN_CODE_TX"])))
                    pgDtls.StrPinCd = Convert.ToString(row["PIN_CODE_TX"]);
                else
                    pgDtls.StrPinCd = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(row["SESSION_KEY"])))
                    pgDtls.StrSessionKey = Convert.ToString(row["SESSION_KEY"]);
                else
                    pgDtls.StrSessionKey = string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strUserId"></param>
        /// <param name="iItemId"></param>
        /// <returns></returns>
        public static DataTable GetUserDataInfo(IDbConnection con, string strUserId, string iItemId)
        {
            //throw new NotImplementedException();
            Hashtable htConditions = new Hashtable();
            DataTable dtReturnData = new DataTable();
            try
            {
                htConditions.Add("USER_ID", strUserId);
                htConditions.Add("ITEM_ID", iItemId);
                dtReturnData = Utils.GetDataFromProc("[dbo].[PMT_GET_USER_BY_USERID]", htConditions, con);
                if (dtReturnData.Rows != null && dtReturnData != null && dtReturnData.Rows.Count > 0)
                {
                    return dtReturnData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetUserDAtaInfo Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrResponse"></param>
        /// <returns></returns>
        public static Models.PGResponseDtls GenerateResponseDtls(string[] arrResponse)
        {
            //throw new NotImplementedException();
            Models.PGResponseDtls pgRespDtls = new Models.PGResponseDtls();
            pgRespDtls.strMerchantID = arrResponse[0];
            pgRespDtls.strTransId = arrResponse[1];
            pgRespDtls.strTxnReferenceNo = arrResponse[2];
            pgRespDtls.strBankReferenceNo = arrResponse[3];
            pgRespDtls.strTxnAmount = arrResponse[4];
            pgRespDtls.strBankID = arrResponse[5];
            pgRespDtls.strBankMerchantID = arrResponse[6];
            pgRespDtls.strTxnType = arrResponse[7];
            pgRespDtls.strCurrencyName = arrResponse[8];
            pgRespDtls.strItemCode = arrResponse[9];
            pgRespDtls.strSecurityType = arrResponse[10];
            pgRespDtls.strSecurityID = arrResponse[11];
            pgRespDtls.strSecurityPassword = arrResponse[12];
            pgRespDtls.strTxnDate = arrResponse[13];
            pgRespDtls.strAuthStatus = arrResponse[14];
            pgRespDtls.strSettlementType = arrResponse[15];
            pgRespDtls.strAdditionalInfo1 = arrResponse[16];
            pgRespDtls.strAdditionalInfo2 = arrResponse[17];
            pgRespDtls.strAdditionalInfo3 = arrResponse[18];
            pgRespDtls.strAdditionalInfo4 = arrResponse[19];
            pgRespDtls.strAdditionalInfo5 = arrResponse[20];
            pgRespDtls.strAdditionalInfo6 = arrResponse[21];
            pgRespDtls.strAdditionalInfo7 = arrResponse[22];
            pgRespDtls.strErrorStatus = arrResponse[23];
            pgRespDtls.strErrorDescription = arrResponse[24];
            pgRespDtls.strCheckSum = arrResponse[25];
            pgRespDtls.strTotalAmt = arrResponse[4];
            pgRespDtls.strServiceTaxAmt = "0.00";
            pgRespDtls.strProcessingFeeAmt = "0.00";
            return pgRespDtls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        private static string GetB64Image(string Path)
        {
            using (Image image = Image.FromFile(Path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strofid"></param>
        /// <param name="strPGTranId"></param>
        /// <returns></returns>
        private static string GetItemInfo(IDbConnection con, string strofid, string strPGTranId)
        {
            string strOfficeID = strofid;
            try
            {
                int pgTranId = Convert.ToInt32(strPGTranId);


                StringBuilder sbHtml = new StringBuilder();
                sbHtml.Append("<div class='row'><div class='col-xs-12'><div class='table-responsive'><div class='table-responsive'>");
                sbHtml.Append("<table class='table table-bordered'><thead><tr class='bg-primary'><th>S.No.</th><th>Item Description</th>");
                sbHtml.Append("<th>SAC Code</th><th>Description</th><th>Price</th><th>GST</th><th>Total (Rs.)</th></tr></thead><tbody>");

                StringBuilder sbRow = new StringBuilder();

                DataTable dtData = new DataTable();
                Hashtable htTransData = new Hashtable();
                htTransData.Add("PG_TRAN_ID", pgTranId);
                dtData = Utils.GetDataFromProc("[dbo].[PMT_GET_ITEM_INFO]", htTransData, con);
                int i = 0;
                decimal total_tax = 0,item_tax=0;
                if (dtData.Rows.Count > 0 && dtData != null)
                {
                    foreach (DataRow row in dtData.Rows)
                    {
                        item_tax = 0;
                        i = i + 1;
                        sbRow.Append("<tr>");
                        sbRow.Append("<th>").Append(i).Append("</th>");
                        sbRow.Append("<th>").Append(row["SUB_ITEM_DESC_TX"]).Append("</th>");
                        sbRow.Append("<th>").Append(row["SAC_CODE"]).Append("</th>");
                        sbRow.Append("<th></th>");
                        sbRow.Append("<th>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</th>");

                        if (row["IGST_AMT"] != DBNull.Value && Convert.ToDecimal(row["IGST_AMT"]) != 0) item_tax = item_tax + Convert.ToDecimal(row["IGST_AMT"]);
                        if (row["CGST_AMT"] != DBNull.Value && Convert.ToDecimal(row["CGST_AMT"]) != 0) item_tax = item_tax + Convert.ToDecimal(row["CGST_AMT"]);
                        if (row["SGST_AMT"] != DBNull.Value && Convert.ToDecimal(row["SGST_AMT"]) != 0) item_tax = item_tax + Convert.ToDecimal(row["SGST_AMT"]);
                        if (item_tax > 0) sbRow.Append("<th>").Append(Convert.ToString(Math.Round(item_tax, 2))).Append("</th>");
                        else sbRow.Append("<th>0.00</th>");

                        //sbRow.Append("<th>0.00</th>");
                        sbRow.Append("<th>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["TOTAL_AMT"]), 2))).Append("</th>");
                        sbRow.Append("</tr>");
                        total_tax = total_tax + item_tax;
                    }

                    //TOTAL AMOUNT ROW
                    sbRow.Append("<tr>");
                    sbRow.Append("<td style = 'text-align:right;' colspan = '6' > Total </ td >");
                    sbRow.Append("<th>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</th>");
                    sbRow.Append("</tr>");
                    sbHtml.Append(sbRow);
                    sbHtml.Append("</tbody></table>");
                    return sbHtml.ToString();

                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetItemInfo Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strofid"></param>
        /// <param name="strPGTranId"></param>
        /// <param name="isGst"></param>
        /// <returns></returns>
        private static string GetItemInfoForReceipt(IDbConnection con, string strofid, string strPGTranId, bool isGst, string strAddInfo1)
        {
            string strOfficeID = strofid;
            int pgTranId = Convert.ToInt32(strPGTranId);
            StringBuilder sbRow = new StringBuilder();
            DataTable dtData = new DataTable();
            Hashtable htTransData = new Hashtable();
            htTransData.Add("PG_TRAN_ID", pgTranId);
            try
            {
                dtData = Utils.GetDataFromProc("[dbo].[PMT_GET_ITEM_INFO]", htTransData, con);
                if (isGst)
                {
                    StringBuilder sbHtml = new StringBuilder();
                    sbHtml.Append("<div class='table-responsive'><table class='table table-bordered'><thead><tr class='bg-primary'>")
                          .Append("<th>S.No.</th><th>Item Description</th><th>SAC Code</th><th>Taxable Amt</th><th>IGST @</th><th>IGST Amt Rs.</th></tr>")
                          .Append("</thead><tbody>");

                    int i = 0;
                    if (dtData.Rows.Count > 0 && dtData != null)
                    {
                        if (!string.IsNullOrEmpty(strAddInfo1))
                        {
                            sbRow.Append("<tr>");
                            sbRow.Append("<td class='receiptTable'>").Append(1).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(strAddInfo1).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(dtData.Rows[0]["SAC_CODE"]).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["SUB_AMT_NM"]), 2))).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["TAX_RATE_NM"]), 2))).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["SUB_AMT_NM"]), 2))).Append("</td>");
                            sbRow.Append("</tr>");
                        }
                        else
                        {

                            foreach (DataRow row in dtData.Rows)
                            {
                                i = i + 1;
                                sbRow.Append("<tr>");
                                sbRow.Append("<td class='receiptTable'>").Append(i).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(row["SUB_ITEM_DESC_TX"]).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(row["SAC_CODE"]).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["TAX_RATE_NM"]), 2))).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                sbRow.Append("</tr>");
                            }
                        }

                        sbHtml.Append(sbRow.ToString())
                        .Append("<tr class='fontBold'><td colspan = '3' style = 'text-align:right;'> Total </td>")
                        .Append("<td>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td><td></td>")
                        .Append("<td>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td></tr>");
                        sbHtml.Append("<tr class='fontBold'><td colspan = '3'></td><td colspan='2' style='text-align:left;'>Taxable Amt</td>")
                              .Append("<td colspan = '3' style= 'text-align:left;'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td></tr>")
                              .Append("<tr class='fontBold'><td colspan = '3'></td><td colspan='2' style='text-align:left;'>IGST @ 18% </td>")
                              .Append("<td colspan = '3' style='text-align:left;'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td></tr>")
                              .Append("<tr class='fontBold'><td colspan = '3'> </td><td colspan='2' style='text-align:left;'>Invoice Total(In Figure) Rs. </td>")
                              .Append("<td colspan = '3' style='text-align:left;'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td></tr>")
                              .Append("</tbody></table></div>");
                        return sbHtml.ToString();

                    }
                    else
                    {
                        return "<strong><p>No entries found</p></strong>";
                    }

                }
                else
                {
                    StringBuilder sbHtml = new StringBuilder();
                    sbHtml.Append("<table class='table table-bordered tableSearchResults'>")
                          .Append("<thead><tr><th class='receiptTable'>SI. No</th><th class='receiptTable'>Description</th><th class='receiptText'>Amount</th></tr></thead><tbody>");

                    int i = 0;
                    if (dtData.Rows.Count > 0 && dtData != null)
                    {
                        if (!string.IsNullOrEmpty(strAddInfo1))
                        {
                            sbRow.Append("<tr>");
                            sbRow.Append("<td class='receiptTable'>").Append(1).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>").Append(strAddInfo1).Append("</td>");
                            sbRow.Append("<td class='receiptTable'>")
                                .Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</td>");
                            sbRow.Append("</tr>");
                        }
                        else
                        {
                            foreach (DataRow row in dtData.Rows)
                            {

                                i = i + 1;

                                sbRow.Append("<tr>");
                                sbRow.Append("<td class='receiptTable'>").Append(i).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(row["SUB_ITEM_DESC_TX"]).Append("</td>");
                                sbRow.Append("<td class='receiptTable'>").Append(Convert.ToString(Math.Round(Convert.ToDecimal(row["SUB_AMT_NM"]), 2))).Append("</td>");
                                sbRow.Append("</tr>");
                            }
                        }
                        sbHtml.Append(sbRow.ToString()).Append("</tbody></table>");
                        sbHtml.Append("</div><div class='row'><div class='col-xs-10 col-xs-offset-1 text-right'><b>Total Amount: ").Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</b></div>")
                            .Append("</div><div class='row mt-10'><div class='col-xs-10 col-xs-offset-1' style='border:#666 1px solid; padding:5px;'><ul class='list-unstyled'><li><h6>Accepted Fees <b>")
                            .Append(Convert.ToString(Math.Round(Convert.ToDecimal(dtData.Rows[0]["AMT_NM"]), 2))).Append("</h6></b></li></ul></div></div>")
                            .Append("<div class='row mt-30'><div class='col-xs-10 col-xs-offset-1 text-right no-padding'>FOR THE INSTITUTE OF COMPANY SECRETARIES OF INDIA</div>")
                            .Append("</div><div class='row  mt-30'><div class='col-xs-10 col-xs-offset-1 text-right no-padding'>Authorised Signatory</div></div> ")
                            .Append("<div class='row  mt-30'><div class='col-xs-10 col-xs-offset-1 no-padding'>This is computer generated invoice and do not require any stamp or signature")
                            .Append("</div></div></section>");
                        return sbHtml.ToString();
                    }
                    else
                    {
                        return "<strong><p>No entries found</p></strong>";
                    }
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "GetItemInfoGorReceipt: Transaction Id - " + strPGTranId + ", Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return "Issue Occurs at GetItemInfo method: " + ex.Message;
            }
        }

        public void LogError(Exception ex)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = HttpContext.Current.Server.MapPath("D:\\errorlog.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="receipt"></param>
        /// <param name="pgDtls"></param>
        /// <param name="pgRespDtls"></param>
        /// <param name="strTxStatus"></param>
        /// <param name="iTxStatusId"></param>
        /// <param name="isCheckSumValid"></param>
        /// <param name="bRespReceived"></param>
        /// <param name="msg"></param>
        /// <param name="gst"></param>
        /// <returns></returns>
        public static string ProcessResponseTransactions(IDbConnection con, Receipt receipt, PGDetails pgDtls
                        , PGResponseDtls pgRespDtls, string strTxStatus, int iTxStatusId, bool isCheckSumValid
                        , bool bRespReceived, string msg, bool gst)
        {
            try
            {
                string strHtml = string.Empty;
                StringBuilder sb = new StringBuilder();
                if (bRespReceived)
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    sb.Append("INSERT INTO PG_TRANSACTION_STATUS_T(PG_ID,PG_STATUS_TX,PG_STATUS_ID,ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY)VALUES")
                                        .Append("(@PG_ID,@PG_STATUS_TX,@PG_STATUS_ID,1,CURRENT_TIMESTAMP,@USER_ID,CURRENT_TIMESTAMP,@USER_ID)");
                    SqlCommand trCmd = new SqlCommand();

                    trCmd = PaymentGateway.Util.UtilService.CreateCommand(sb.ToString(), con);
                    trCmd.Parameters.AddWithValue("@PG_ID", pgDtls.StrReqId);
                    trCmd.Parameters.AddWithValue("@PG_STATUS_TX", strTxStatus);
                    trCmd.Parameters.AddWithValue("@PG_STATUS_ID", iTxStatusId);
                    trCmd.Parameters.AddWithValue("@USER_ID", pgDtls.StrUserId);

                    trCmd.ExecuteNonQuery();


                    string sql = "SELECT c.APP_NXT_SCREEN_ID, c.RETURN_URL_TX, a.SCREEN_ID,a.OFFICE_ID,a.SESSION_TX,a.CREATED_BY as USER_ID FROM PG_TRANSACTION_T a, PAYMENT_GATEWAY_T b, PG_SCREEN_MAP_T c WHERE a.PG_ID=b.ID AND c.PG_OFFICE_ID=a.OFFICE_ID AND c.APP_SCREEN_ID=a.SCREEN_ID AND c.PG_ID=b.ID AND a.ACTIVE_YN=1 AND b.ACTIVE_YN=1 AND c.ACTIVE_YN=1 AND a.ID=" + pgDtls.StrReqId;
                    try
                    {
                        List<Dictionary<string, object>> data = PaymentGateway.Util.UtilService.GetData(sql, con);
                        foreach (var row in data)
                        {
                            receipt.url = Convert.ToString(row["RETURN_URL_TX"]);
                            receipt.scid = Convert.ToString(row["SCREEN_ID"]);
                            receipt.ofid = Convert.ToString(row["OFFICE_ID"]);
                            receipt.sskey = Convert.ToString(row["SESSION_TX"]);
                            receipt.strNxtScnID = Convert.ToString(row["APP_NXT_SCREEN_ID"]);
                            string url = receipt.url;
                            if (url.IndexOf("{") != -1)
                            {
                                url = url.Substring(0, url.IndexOf("{") - 1);
                                receipt.url = url;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exMsg = ex.Message;
                        sbResp.Clear();
                        sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                         .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                        sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                        sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                        sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "ProcessResponseTransaction: TransactionId - " + pgDtls.StrTransId + ", Exception1 - " + ex.Message);
                        sqlCmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        //con.Close();
                    }

                    if (receipt.isTxSuccess)
                    {
                        Hashtable htCond = new Hashtable();
                        htCond.Add("TRANSACTION_ID", pgRespDtls.strTransId);
                        htCond.Add("IS_CHECK_SUM_INVALID", !isCheckSumValid);
                        htCond.Add("RESPONSE_RECEIVED_YN", bRespReceived);
                        htCond.Add("PG_RETURN_TRANSACTION_ID", pgRespDtls.strTxnReferenceNo);
                        htCond.Add("TRANSACTION_STATUS", iTxStatusId);// GetAuthStatus(pgRespDtls.strAuthStatus));
                        htCond.Add("REQUEST_DATA_TX", string.Empty);
                        htCond.Add("RESPONSE_DATA_TX", msg);
                        htCond.Add("PAYMENT_STATUS", iTxStatusId);// GetAuthStatus(pgRespDtls.strAuthStatus));
                        htCond.Add("MERCHANT_ID", pgRespDtls.strMerchantID);
                        htCond.Add("CUSTOMER_NAME", pgDtls.StrCustomerName);
                        htCond.Add("EMAIL", pgDtls.StrEmail);
                        htCond.Add("MOBILE", pgDtls.StrMob);
                        htCond.Add("PG_CODE", pgDtls.StrPgmode);
                        htCond.Add("OFFICE_ID", Convert.ToInt32(pgDtls.Strofid));
                        htCond.Add("AMOUNT", Convert.ToDecimal(pgDtls.StrAmt));
                        htCond.Add("RECEIPT_START_SEED", pgDtls.ReceiptIncSeed);
                        htCond.Add("USER_BILLING_ADDR", pgDtls.Billing);
                        htCond.Add("USER_SHIPPING_ADDR", pgDtls.Shipping);
                        htCond.Add("IS_GST", gst);
                        htCond.Add("TOTAL_AMT", pgRespDtls.strTotalAmt);
                        htCond.Add("SERVICE_TAX_AMT", pgRespDtls.strServiceTaxAmt);
                        htCond.Add("PROCESSING_FEE_AMT", pgRespDtls.strProcessingFeeAmt);
                        htCond.Add("RESPONSE_DATE_DT", pgRespDtls.strTxnDate);
                        htCond.Add("USER_ID", Convert.ToInt32(pgDtls.StrUserId));
                        DataTable dtReceiptData = new DataTable();

                        dtReceiptData = Utils.GetDataFromProc("[dbo].[PMT_UPDATE_TRANSACTION]", htCond, con);
                        if (dtReceiptData != null && dtReceiptData.Rows != null && dtReceiptData.Rows.Count > 0)
                        {
                            receipt.REQUEST_ID = Convert.ToString(dtReceiptData.Rows[0]["ACKNOWLEDGEMENT_ID"]); // Convert.ToInt32(pgDtls.StrReqId);
                            receipt.RECEIPT_ID = pgDtls.StrShortCode + "/" + Convert.ToString(dtReceiptData.Rows[0]["ST_RECEIPT_ID_TX"]);
                            receipt.TRANSACTION_ID = Convert.ToString(dtReceiptData.Rows[0]["TRANSACTION_ID"]);
                            receipt.AMOUNT = Convert.ToString(Math.Round(Convert.ToDecimal(dtReceiptData.Rows[0]["AMOUNT"]), 2));
                            receipt.CUSTOMER_NAME = pgDtls.StrCustomerName;
                            receipt.MOBILE = pgDtls.StrMob;
                            receipt.EMAIL = pgDtls.StrEmail;
                            receipt.ADDR_LINE_1 = pgDtls.StrAddr1;
                            receipt.ADDR_LINE_2 = pgDtls.StrAddr2;
                            receipt.ADDR_LINE_3 = pgDtls.StrAddr3;
                            receipt.STATE_NAME_TX = pgDtls.StrState;
                            receipt.DISTRICT_NAME_TX = pgDtls.StrDistrict;
                            receipt.PIN_CODE_TX = pgDtls.StrPinCd;
                            receipt.RECEIPT_DATE_DT = Convert.ToDateTime(dtReceiptData.Rows[0]["RECEIPT_DATE_DT"]).ToString("dd/MM/yyyy").Replace("-", "/");
                            receipt.SERVICE_TAX_TX = pgDtls.StrServiceTax;
                            receipt.REG_NUMBER_TX = pgDtls.StrRegNumber;
                            receipt.BILLING_ADDR = pgDtls.Billing;
                            receipt.SHIPPING_ADDR = pgDtls.Shipping;
                            receipt.userid = pgDtls.StrUserId;
                            receipt.strPgTranId = pgDtls.StrReqId;
                            receipt.iRefId = pgDtls.IRefId;
                            receipt.strAddInfo1 = pgDtls.StrAddInfo1;
                            receipt.strAddInfo2 = pgDtls.StrAddInfo2;
                            receipt.strAddInfo3 = pgDtls.StrAddInfo3;
                            receipt.recTmplId = pgDtls.RecTmplId;
                            receipt.strLoginId = pgDtls.StrLoginId;
                        }
                        else
                        {
                            strHtml = "<strong>Not able to generate receipt.</strong>";
                        }
                    }
                    else
                    {
                        receipt.REQUEST_ID = pgDtls.StrAckId; // Convert.ToInt32(pgDtls.StrReqId);                      
                        receipt.TRANSACTION_ID = Convert.ToString(pgRespDtls.strTransId);
                        receipt.AMOUNT = Convert.ToString(Math.Round(Convert.ToDecimal(pgRespDtls.strTxnAmount), 2));
                        receipt.CUSTOMER_NAME = pgDtls.StrCustomerName;
                        receipt.MOBILE = pgDtls.StrMob;
                        receipt.EMAIL = pgDtls.StrEmail;
                        receipt.strPgTranId = pgDtls.StrReqId;
                        receipt.userid = pgDtls.StrUserId;
                        receipt.iRefId = pgDtls.IRefId;
                        receipt.strAddInfo1 = pgDtls.StrAddInfo1;
                        receipt.strLoginId = pgDtls.StrLoginId;

                        List<Dictionary<string, object>> lstDictData = PaymentGateway.Util.UtilService.GetData("select dbo.GetNewReceiptID('" + pgDtls.ReceiptIncSeed + "') as NEW_RECEIPTID", con);
                        long receiptId = Convert.ToInt64(lstDictData[0]["NEW_RECEIPTID"]);

                        StringBuilder sbQuery = new StringBuilder();
                        SqlCommand cmd = new SqlCommand();
                        int bresprec = 0;
                        if (bRespReceived) bresprec = 1;
                        int chkSum = 0;
                        if (isCheckSumValid) chkSum = 1;

                        sbQuery.Append("UPDATE PG_TRANSACTION_T SET PG_CODE = '").Append(pgDtls.StrPgmode).Append("', MERCHANT_ID = '").Append(pgRespDtls.strMerchantID)
                               .Append("', RESPONSE_DATA_TX = '").Append(msg).Append("', RESPONSE_DATE_DT = '").Append(pgRespDtls.strTxnDate).Append("', PAYMENT_STATUS_ID =").Append(iTxStatusId)
                               .Append(", PG_RETURN_TRANSACTION_ID ='").Append(pgRespDtls.strTxnReferenceNo)
                               .Append("', RESPONSE_RECEIVED_YN = ").Append(bresprec)
                               .Append(", IS_CHECK_SUM_INVALID =").Append(chkSum)
                               .Append(", IS_RECONCILE=").Append("0")
                               .Append(" WHERE TRANSACTION_ID = '").Append(pgRespDtls.strTransId).Append("'");

                        cmd = PaymentGateway.Util.UtilService.CreateCommand(sbQuery.ToString(), con);
                        cmd.ExecuteNonQuery();
                        sbQuery.Clear();
                        sbQuery.Append("UPDATE PG_PAYMENT_RECEIPT_HDR_T SET RECEIPT_ID =").Append(receiptId).Append(", RECEIPT_DATE_DT = getdate(), RECEIPT_SUMMARY_TX = 'Transaction failed', PAYMENT_MODE_ID =").Append(pgDtls.StrBankMerchantId).Append(",")
                            .Append(" PAYMENT_STATUS_ID =").Append(iTxStatusId).Append(" WHERE TRANSACTION_ID = '").Append(pgRespDtls.strTransId).Append("'");
                        cmd = PaymentGateway.Util.UtilService.CreateCommand(sbQuery.ToString(), con);
                        cmd.ExecuteNonQuery();
                    }
                    //if (!receipt.isTxSuccess || pgDtls.RecTmplId == 0)
                    //{
                    //   strHtml = PGLogic.GetPgReceipthtml(con, receipt, gst, receipt.isTxSuccess);
                    //}
                    return strHtml;
                }
                else
                {
                    return "No response received from payment gateway";
                }
            }
            catch (Exception ex)
            {
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "Process Response Transaction Exception1 - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return ex.Message;
            }

        }
    }
}