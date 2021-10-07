using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class SearchReceiptTransaction
    {
        [Required(ErrorMessage = "Please Enter Transaction ID")]
        public string TransactionID { get; set; }
       // public bool status { get; set; }
    }
}