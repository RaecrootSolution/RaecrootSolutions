using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class SearchMember
    {
        [Required(ErrorMessage = "Please Enter Membership Number")]
        public string MembershipNumer { get; set; }
    
        public int UserId { get; set; }
    }
}