using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentGateway.Models
{
    public class Receipt
    {
        public string REQUEST_ID = string.Empty;
        public string RECEIPT_ID = string.Empty;
        public string TRANSACTION_ID = string.Empty;
        public string AMOUNT = string.Empty;
        public string CUSTOMER_NAME = string.Empty;
        public string MOBILE = string.Empty;
        public string EMAIL = string.Empty;
        public string RECEIPT_DATE_DT = string.Empty;
        public string SHORT_CODE_TX = string.Empty;
        public string SERVICE_TAX_TX = string.Empty;
        public string REG_NUMBER_TX = string.Empty;
        public string ADDR_LINE_1 = string.Empty;
        public string ADDR_LINE_2 = string.Empty;
        public string ADDR_LINE_3 = string.Empty;
        public string CITY_NAME_TX = string.Empty;
        public string DISTRICT_NAME_TX = string.Empty;
        public string STATE_NAME_TX = string.Empty;
        public string PIN_CODE_TX = string.Empty;
        public string BILLING_ADDR = string.Empty;
        public string SHIPPING_ADDR = string.Empty;
        public string url = string.Empty;
        public string sskey = string.Empty;
        public string ofid = string.Empty;
        public string userid = string.Empty;
        public string html = string.Empty;
        public string scid = string.Empty;
        public string strRespMsg = string.Empty;
        public bool isTxSuccess;
        public string strPgTranId = string.Empty;
        public string strEncryptedHTML = string.Empty;
        public int iRefId = 0;
        public string strAddInfo1 = string.Empty;
        public string strAddInfo2 = string.Empty;
        public string strAddInfo3 = string.Empty;
        public int recTmplId = 0;
        public string strNxtScnID;
        public string strLoginId = string.Empty;
    }
}