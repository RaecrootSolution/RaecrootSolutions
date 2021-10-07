using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Entity
{
    public enum Gateway
    {
        ICICI = 1,
        CHALLAN,
        BILLDESK,
        AXISBANK,
        KOTAKBANK
    }


    public enum ePaymentStatus
    {
        Draft = 1,
        Dup,
        Fail,
        Init,
        Refunded,
        Succ,
        TobeRefunded
    }

    public class PGMiddleURL
    {
        public string CheckoutUrl { get; set; }
    }


    public sealed class PaymentGatewayMaster
    {
        public string GateWayCode { get; set; }
        public string GatewayName { get; set; }
        /// <summary>
        /// represent Payment Geteway URL 
        /// </summary>
        public string GatewayURL { get; set; }
        /// <summary>
        /// represent Checksume key of Payment Geteway
        /// </summary>
        public string CheckSumKey { get; set; }
        /// <summary>
        /// represent Merchant Id of Payment Geteway
        /// </summary>
        public string MerchantId { get; set; }
        /// <summary>
        /// represent SubMerchant ID of Payment Geteway
        /// </summary>
        public string SubMerchantID { get; set; }
        /// <summary>
        /// represent Ref No of Payment Geteway
        /// </summary>
        public string RefNo { get; set; }
        /// <summary>
        /// represent Mobile Number of Payment Geteway
        /// </summary>
        public string MobNo { get; set; }
        /// <summary>
        /// represent Response URL where will go after comes to payment gateway
        /// </summary>
        public string ResponseURL { get; set; }

    }
    [Serializable]
    public class PaymentModeEntity
    {
        public int PaymentModeID { get; set; }
        public string ModeName { get; set; }
        public string ModeCode { get; set; }
        public bool IsOnline { get; set; }
    }
    [Serializable]
    public class PaymentTransactionEntity
    {
        public int RequestID { get; set; }
        public string ProcessRoute { get; set; }
        public int ProcessRequestID { get; set; }
        public double Amount { get; set; }
        public string RequestData { get; set; }
        public DateTime PaymentInitiationDate { get; set; }
        public string ResponseData { get; set; }
        public DateTime ResponseDate { get; set; }
        public string ReceiptSummary { get; set; }
        public string PGReturnTransactionID { get; set; }
        public DateTime ReconcillationDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public string PGCode { get; set; }
        public string PaymentMode { get; set; }
        public int PaymentStatusID { get; set; }
    }

    [Serializable]
    public class PaymentStatus
    {
        /// <summary>
        /// get payment Status Code Succ,Init,Fail,Draft
        /// </summary>
        public string PaymentStatusCode { get; set; }
        public int PaymentStatusID { get; set; }
        public string PaymentStatusName { get; set; }
    }
}
