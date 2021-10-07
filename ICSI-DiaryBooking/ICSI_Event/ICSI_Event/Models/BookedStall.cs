using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class BookedStall
    {
        public int Id { get; set; }
        public string  StallNumber { get; set; }
        public string CompanyName  { get; set; }
        public decimal Amount { get; set; }
        public decimal GST18Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public int status { get; set; }
    }
}