using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class eCSINGeneration
    {
        [Required(ErrorMessage = "MRN Number must be required")]
        public string MembershipNo { get; set; }

        [Required(ErrorMessage = "Employee Designation must be required")]
        public string EmployeeDesignation { get; set; }

        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special characters are not allowed.")]
        [Required(ErrorMessage = "Employer CIN Number must be required")]
        public string EmployerCINNo { get; set; }

        [Required(ErrorMessage = "Please Select CIN/PAN")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Name of Company/Firm/Individual must be required")]
        public string EmployerName { get; set; }

        [Required(ErrorMessage = "Employer Registration Address must be required")]
        public string EmployerRegAddress { get; set; }

        [Required(ErrorMessage = "Date of Offer Letter must be required")]
        public string DateOfOfferLetter { get; set; }

        [Required(ErrorMessage = "Date of Consent Letter must be required")]
        public string DateOfConsentLetter { get; set; }

        [Required(ErrorMessage = "Date of Appointment must be required")]
        public string DateOfAppointment { get; set; }

        //[RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", ErrorMessage = "Website name is not valid.")]
        public string WebsiteOfEmployer { get; set; }

        //[DataType(DataType.PhoneNumber)]        
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Phone Number is not valid (*)")]
        [Required(ErrorMessage = "Phone Number of Employer must be required")]       
        [RegularExpression("^[0-9]{9,15}$", ErrorMessage = "Phone Number is not valid (*)")]
        public string PhoneNoOfEmployer { get; set;}

        [DataType(DataType.EmailAddress)]
        //[EmailAddress]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email Id is not valid (*)")]
        [Required(ErrorMessage = "Email Id of Employer must be required)")]
        public string EmailIdOfEmployer { get; set; }        

        public string eCSINGenerateNumber { get; set; }

        public string DateOfChangeDesignation { get; set; }
    }
}