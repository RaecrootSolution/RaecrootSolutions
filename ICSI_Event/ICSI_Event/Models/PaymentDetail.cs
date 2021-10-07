using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class PaymentDetail
    {
        public int Id { get; set; }
        public string RequestId { get; set; }
        public string TransactonId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentType { get; set; }
        public string MobileNuber { get; set; }
        public string PaymentMode { get; set; }
        public string InvoiceNo { get; set; }


    }
}