//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICSI_Event.DBContext
{
    using System;
    
    public partial class USPUpdatePayment_Result
    {
        public int Id { get; set; }
        public Nullable<int> ProcessId { get; set; }
        public Nullable<int> ProcessRequestId { get; set; }
        public Nullable<int> PaymentStatusID { get; set; }
        public Nullable<int> PayModeID { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> TotalTax { get; set; }
        public Nullable<decimal> ConvenienceFee { get; set; }
        public Nullable<decimal> ProcessingFee { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public string ResponsePaymode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseTransactionID { get; set; }
        public Nullable<System.DateTime> ResponseTransactionDate { get; set; }
        public string ResponseData { get; set; }
        public string PGCode { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}
