using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class EditeCSIN
    {
        public string MembershipNo { get; set; }
        public string PrevEmpDesignation { get; set; }

        [Required(ErrorMessage = "New Designation must be required")]
        public string CurrentEmpDesignation { get; set; }

        public string EmployerCINNo { get; set; }
        public string EmployerName { get; set; }
        public string EmployerRegAddress { get; set; }
        public string DateOfOfferLetter { get; set; }
        public string DateOfConsentLetter { get; set; }
        public string DateOfAppointment { get; set; }
       

        [Required(ErrorMessage = "Date of Changed Designation must be required")]
        public string DateOfChangeDesignation { get; set; }
        public string eCSINNumber { get; set; }
        public string PANNumber { get; set; }

    }
}