using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class SearcheCSIN
    {
        [Required(ErrorMessage = "Please Enter From Date")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter To Date")]
        public string ToDate { get; set; }
        public int UserId { get; set; }
    }
}