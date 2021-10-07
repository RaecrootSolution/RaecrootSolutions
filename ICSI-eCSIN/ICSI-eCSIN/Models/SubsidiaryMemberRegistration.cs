using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ICSI_eCSIN.Models
{
    public class SubsidiaryMemberRegistration
    {
        [Required(ErrorMessage = "Please Enter Membership Number for Subsidiary eCSIN")]
        public string MembershipNumer { get; set; }
       

        [Required(ErrorMessage = "Approval Date must be required")]
        [Display(Name = "Approval Date")]        // Not User // [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public string ApprovalDate { get; set; }
    }
}