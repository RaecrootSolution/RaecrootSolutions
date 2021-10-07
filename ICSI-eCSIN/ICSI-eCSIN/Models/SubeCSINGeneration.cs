using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
   
    public class SubeCSINGeneration
    {
        //[Key]
        //[DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
         
        [Required(ErrorMessage = "MRN Number must be required")]
        public string MembershipNo { get; set; }

        [Required(ErrorMessage = "Employee Designation must be required")]
        [StringLength(100, ErrorMessage = "Please shorten the length of Designation")]
        public string EmployeeDesignation { get; set; }

        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special characters are not allowed.")]
        [Required(ErrorMessage = "Employer CIN Number must be required")]
        public string EmployerCINNo { get; set; }

        [Required(ErrorMessage = "Please Select CIN/PAN")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Name of Company/Firm/Individual must be required")]       
        public string EmployerName { get; set; }

        [Required(ErrorMessage = "Name of Subsidiary Company must be required")]
        [StringLength(100, ErrorMessage = "Please shorten the length of Subsidiary Company")]
        public string SubEmployerName { get; set; }

        [Required(ErrorMessage = "Employer Registration Address must be required")]
        [StringLength(500, ErrorMessage = "Please shorten the length of Address")]
        public string EmployerRegAddress { get; set; }

        //Not In Used // [Required(ErrorMessage = "Date of Offer Letter must be required")]

        public string DateOfOfferLetter { get; set; }

        [Required(ErrorMessage = "Date of Consent Letter must be required")]
        public string DateOfConsentLetter { get; set; }

        [Required(ErrorMessage = "Date of Appointment must be required")]
        public string DateOfAppointment { get; set; }

        //not in used//[RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", ErrorMessage = "Website name is not valid.")]

        [StringLength(100, ErrorMessage = "Please shorten the length of website ")]
        public string WebsiteOfEmployer { get; set; }

        //Not in Used // [DataType(DataType.PhoneNumber)] //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Phone Number is not valid (*)")]

        [Required(ErrorMessage = "Phone Number of Employer must be required")]
        [RegularExpression("^[0-9]{9,15}$", ErrorMessage = "Phone Number is not valid (*)")]
        public string PhoneNoOfEmployer { get; set; }

        public string MobileNoOfEmployer { get; set; }

        [DataType(DataType.EmailAddress)]   // [EmailAddress]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email Id is not valid (*)")]
        [Required(ErrorMessage = "Email Id of Employer must be required")]
        [StringLength(100, ErrorMessage = "Please shorten the length of email")]
        public string EmailIdOfEmployer { get; set; }

        public string eCSINGenerateNumber { get; set; }

        public string DateOfChangeDesignation { get; set; }

        public string Remarks { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Number is allowed.")]
        [StringLength(9, ErrorMessage = "Please enter valid amount")]
        public string Amount { get; set; }

    }
}