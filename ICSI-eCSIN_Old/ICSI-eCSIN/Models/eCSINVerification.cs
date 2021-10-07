using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class eCSINVerification
    {
        [Required(ErrorMessage = "Please Enter eCSIN")]
        public string eCSINNumber { get; set; }
        public bool status { get; set; }
    }
}