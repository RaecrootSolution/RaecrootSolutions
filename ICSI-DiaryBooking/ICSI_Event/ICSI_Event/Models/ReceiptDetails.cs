using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class ReceiptDetails
    {
        public int ID { get; set; }
        public string BILLING_ADDRESS_TX { get; set; }
        public string SHIPPING_ADDRESS_TX { get; set; }
        public string GSTIN { get; set; }
        public string CONTACTPERSON { get; set; }
        public string STALLDESCRIPTION { get; set; }
        public string CustomerName { get; set; }
        public string SAC_CODE { get; set; }

        public decimal AMOUNT { get; set; }

        public decimal GSTAMOUNT { get; set; }

        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }

        public decimal TOTALAMOUNT { get; set; }

        public string INVOICE_NO { get; set; }

        public DateTime INVOICE_DT { get; set; }

        public string STATE_CODE { get; set; }

        public string Stata_Name { get; set; }

        public string EVENTREGISTRATION_ID { get; set; }

        public string StallNumber { get; set; }

        public string RECEIPT_NO { get; set; }

        public DateTime RECEIPT_DT { get; set; }
        public string TransactionId { get; set; }

        public string MembershipNo { get; set; }

        public string ICSITransactionID_R { get; set; }

        public int QtyDiaryNumber { get; set; }

        public string PinCode { get; set; }

        public string DCIty { get; set; }

    }
}