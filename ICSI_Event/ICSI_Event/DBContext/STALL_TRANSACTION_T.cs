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
    using System.Collections.Generic;
    
    public partial class STALL_TRANSACTION_T
    {
        public int ID { get; set; }
        public Nullable<int> EVENTREGISTRATION_ID { get; set; }
        public string REQUEST_ID { get; set; }
        public string TRANSACTION_ID { get; set; }
        public Nullable<decimal> TOTALAMOUNT { get; set; }
        public Nullable<bool> STATUS { get; set; }
        public string INVOICE_NO { get; set; }
        public Nullable<System.DateTime> INVOICE_DT { get; set; }
        public string SAC_CODE { get; set; }
        public string RECEIPT_NO { get; set; }
        public Nullable<System.DateTime> RECEIPT_DT { get; set; }
        public bool ACTIVE_YN { get; set; }
        public System.DateTime CREATED_DT { get; set; }
        public int CREATED_BY { get; set; }
        public System.DateTime UPDATED_DT { get; set; }
        public int UPDATED_BY { get; set; }
    }
}