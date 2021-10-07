using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class UpdateDetails
    {
        public string MembershipNo { get; set; }      
        public string EmployerRegAddress { get; set; }
        public string DateOfOfferLetter { get; set; }
        public string DateOfConsentLetter { get; set; }
        public string DateOfAppointment { get; set; }
        public string eCSINNumber { get; set; }

        public string WebsiteOfEmployer { get; set; }
        public string EmployerEmailId { get; set; }
        public string PhoneNoOfEmployer { get; set; }

        public string Remarks1 { get; set; }
        public string Remarks2 { get; set; }
        public string Remarks3 { get; set; }
        public string Remarks4 { get; set; }
        public string Remarks5 { get; set; }
        
    }
}