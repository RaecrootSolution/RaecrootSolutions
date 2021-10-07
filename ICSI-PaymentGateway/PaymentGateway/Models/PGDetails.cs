using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentGateway.Models
{
    public class PGDetails
    {
        string strScreenId;
        string strType;
        string strBankMerchantId;
        string strTransId;
        string strUserId;
        string strMob;
        string strAmt;
        string strEmail;
        string strofid;
        string stritemid;
        private string strAddr1;
        private string strAddr2;
        private string strAddr3;
        private string strCity;
        private string strDistrict;
        private string strState;
        private string strAckId;
        private string strCustomerName;
        private string strSid;
        private string strRegNumber;
        private string strSessionKey;
        private string strPinCd;
        private string strPgmode;
        private string strOfficeName;
        private string strServiceTax;
        private string strShortCode;
        private string iReceiptIncSeed;
        private string iReceiptDecSeed;
        private string strReqId;
        private string returnURL;
        private string billing;
        private string shipping;
        private string gstNm;
        private string strReqData;
        private string strRemarks;
        private string strLocationIdentifier;
        private string strReceiptDbId;
        private int iRefId;
        private string strAddInfo1;
        private string strAddInfo2;
        private string strAddInfo3;
        private int recTmplId;
        private string strLoginId;
        private string strItemType;
        public string StrReturnURL { get => returnURL; set => returnURL = value; }
        public string StrBankMerchantId { get => strBankMerchantId; set => strBankMerchantId = value; }
        public string StrType { get => strType; set => strType = value; }
        public string StrScreenId { get => strScreenId; set => strScreenId = value; }
        public string StrTransId { get => strTransId; set => strTransId = value; }
        public string StrUserId { get => strUserId; set => strUserId = value; }
        public string StrMob { get => strMob; set => strMob = value; }
        public string StrAmt { get => strAmt; set => strAmt = value; }
        public string StrEmail { get => strEmail; set => strEmail = value; }
        public string Stritemid { get => stritemid; set => stritemid = value; }
        public string Strofid { get => strofid; set => strofid = value; }
        public string StrAckId { get => strAckId; set => strAckId = value; }
        public string StrCustomerName { get => strCustomerName; set => strCustomerName = value; }
        public string StrSid { get => strSid; set => strSid = value; }
        public string StrRegNumber { get => strRegNumber; set => strRegNumber = value; }
        public string StrSessionKey { get => strSessionKey; set => strSessionKey = value; }
        public string StrState { get => strState; set => strState = value; }
        public string StrDistrict { get => strDistrict; set => strDistrict = value; }
        public string StrCity { get => strCity; set => strCity = value; }
        public string StrAddr3 { get => strAddr3; set => strAddr3 = value; }
        public string StrAddr2 { get => strAddr2; set => strAddr2 = value; }
        public string StrAddr1 { get => strAddr1; set => strAddr1 = value; }
        public string StrPinCd { get => strPinCd; set => strPinCd = value; }
        public string StrPgmode { get => strPgmode; set => strPgmode = value; }
        public string StrShortCode { get => strShortCode; set => strShortCode = value; }
        public string StrServiceTax { get => strServiceTax; set => strServiceTax = value; }
        public string StrOfficeName { get => strOfficeName; set => strOfficeName = value; }
        public string ReceiptIncSeed { get => iReceiptIncSeed; set => iReceiptIncSeed = value; }
        public string ReceiptDecSeed { get => iReceiptDecSeed; set => iReceiptDecSeed = value; }
        public string StrReqId { get => strReqId; set => strReqId = value; }
        public string Billing { get => billing; set => billing = value; }
        public string Shipping { get => shipping; set => shipping = value; }
        public string GstNm { get => gstNm; set => gstNm = value; }
        public string StrReqData { get => strReqData; set => strReqData = value; }
        public string StrRemarks { get => strRemarks; set => strRemarks = value; }
        public string StrLocationIdentifier { get => strLocationIdentifier; set => strLocationIdentifier = value; }
        public string StrReceiptDbId { get => strReceiptDbId; set => strReceiptDbId = value; }
        public int IRefId { get => iRefId; set => iRefId = value; }
        public string StrAddInfo1 { get => strAddInfo1; set => strAddInfo1 = value; }
        public string StrLoginId { get => strLoginId; set => strLoginId = value; }
        public string StrAddInfo2 { get => strAddInfo2; set => strAddInfo2 = value; }
        public string StrAddInfo3 { get => strAddInfo3; set => strAddInfo3 = value; }
        public int RecTmplId { get => recTmplId; set => recTmplId = value; }
        public string StrItemType { get => strItemType; set => strItemType = value; }
    }

    public class BillDeskDtls
    {
        private string strBillDeskURL;
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
        private string strUserEmail;
        private string strUserMobile;
        private string strBillingAddr;
        private string strShippingAddr;
        private int strReceiptIncSeed;
        private int strReceiptDecSeed;

        public string MerchantID { get => merchantID; set => merchantID = value; }
        public int CustomerID { get => customerID; set => customerID = value; }
        public double TxnAmount { get => txnAmount; set => txnAmount = value; }
        public int CurrencyType { get => currencyType; set => currencyType = value; }
        public int TypeField1 { get => typeField1; set => typeField1 = value; }
        public string SecurityID { get => securityID; set => securityID = value; }
        public int TypeField { get => typeField2; set => typeField2 = value; }
        public string RU { get => rU; set => rU = value; }
        public string CheckSum { get => checkSum; set => checkSum = value; }
        public string Filler1 { get => filler1; set => filler1 = value; }
        public string AdditionalInfo3 { get => additionalInfo3; set => additionalInfo3 = value; }
        public string StrUserEmail { get => strUserEmail; set => strUserEmail = value; }
        public string StrUserMobile { get => strUserMobile; set => strUserMobile = value; }
        public string StrBillDeskURL { get => strBillDeskURL; set => strBillDeskURL = value; }
        public string StrShipplingAddr { get => strShippingAddr; set => strShippingAddr = value; }
        public string StrBillingAddr { get => strBillingAddr; set => strBillingAddr = value; }
        public int StrReceiptIncSeed { get => strReceiptIncSeed; set => strReceiptIncSeed = value; }
        public int StrReceiptDecSeed { get => strReceiptDecSeed; set => strReceiptDecSeed = value; }
    }

    public class PGTXDetails
    {
        int iDBTransId = 0;
        string strTransactionID = string.Empty;
        string strReceiptID = string.Empty;
        string strAcknowledgement = string.Empty;
        int iPaymentStatus = 0;
        DateTime dtPMInitiation = DateTime.Today;
        double dTransAmt = 0.00;
        string strUserId = string.Empty;
        string strUserTypeID = string.Empty;
        int iAppScrnId = 0;
        string strUserEmail = string.Empty;
        int strUserMobile = 0;
        string strUserBillingAddr = string.Empty;
        string strUserShippingAddr = string.Empty;
        bool bIsReconcile = false;
    }

    public class PGTXInDetail
    {
        int iDBTxDtlID = 0;
        string strTransactionID = string.Empty;
        int iItemsAmtDtlsId = 0;
        string strMerchantTx = string.Empty;
        int iOfficeId = 0;
        double dTransAmt = 0.00;
        string PGTransRefNo = string.Empty;
        string strBankRefNo = string.Empty;
        int iPaymentStatusId = 0;
        int iPayModeId = 0;
        int iAuthStatusId = 0;
        int iRefundStatusId = 0;
        double dTotalAmt = 0.00;
        double dRefundAmt = 0.00;
        string strSecurityType = string.Empty;
        int iSecurityId = 0;
        string strSecurityPwd = string.Empty;
        int iSettlementType = 0;
        string strErrorStatus = string.Empty;
        string strRequestData = string.Empty;
        string strResponseData = string.Empty;
        int iItemId = 0;
    }

    public class ItemCode
    {

    }

    public enum PGAuthStatus
    {
        SUCCESS = 0300,
        FAILURE = 0399,
        ERROR_CONDITION = 0,
        PENDING_ABANDONED = 0002,
        ERROR_AT_BILLDESK = 0001
    }

    public enum PGRefundStatus
    {
        REFUND_SUCCESS = 0699,
        REFUND_INITIATED = 0799,
        REFUND_CANCELLED = 0,
        REFUND_FAILED = 0
    }

    public enum PGTransStatus
    {
        DRAFT = 3,
        INITIATED = 4,
        SUCCESS = 5,
        FAIL = 6,
        DUPLICATE = 7,
        REFUNDED = 8,
        TO_BE_REFUNDED = 9
    }

    public class PGResponseDtls
    {
        public string strMerchantID = string.Empty;
        public string strTransId = string.Empty;
        public string strTxnReferenceNo;
        public string strBankReferenceNo;
        public string strTxnAmount;
        public string strBankID;
        public string strBankMerchantID;
        public string strTxnType;
        public string strCurrencyName;
        public string strItemCode;
        public string strSecurityType;
        public string strSecurityID;
        public string strSecurityPassword;
        public string strTxnDate;
        public string strAuthStatus;
        public string strSettlementType;
        public string strAdditionalInfo1;
        public string strAdditionalInfo2;
        public string strAdditionalInfo3;
        public string strAdditionalInfo4;
        public string strAdditionalInfo5;
        public string strAdditionalInfo6;
        public string strAdditionalInfo7;
        public string strErrorStatus;
        public string strErrorDescription;
        public string strCheckSum;
        public string strTotalAmt;
        public string strServiceTaxAmt;
        public string strProcessingFeeAmt;
    }

    public class SubItems
    {
        private int iItemId;
        private double iItemAmt;
        private string iItemDesc;
        private int iParentItem;

        public double ItemAmt { get => iItemAmt; set => iItemAmt = value; }
        public int ItemId { get => iItemId; set => iItemId = value; }
        public int IParentItem { get => iParentItem; set => iParentItem = value; }
        public string IItemDesc { get => iItemDesc; set => iItemDesc = value; }
    }

    public class PGReqDtls
    {
        public string pgUrl;
        public string strKeyId;
        public string order_id;
        public string amount;
        public string name;
        public string description;
        public string image;
        public string prefill_name;
        public string prefill_contact;
        public string prefill_email;
        public string shipping_address;
        public string transaction_id;
        public string callback_url;
        public string cancel_url;
        public string strmerchantid;
        public string strmandatoryfields;
        public string strOptionalFields;
        public string returnUrl;
        public string referenceNo;
        public string submerchantid;
        public string transactionamount;
        public string pgmode;
    }


}