using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_UDIN.Models
{
    public class UDINVerification
    {
        [Required(ErrorMessage ="Name must be required")]
        //[RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Please insert only characters")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string FName { get; set; }
        [Required(ErrorMessage = "Email Id must be required")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email Id is not valid.")]
        public string EmailId { get; set; }
        [Required(ErrorMessage = "Mobile Number must be required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Mobile Number is not valid.")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "UDIN Number must be required")]
        
        public string MembershipNumber { get; set; }
        [Required(ErrorMessage = "Captch Code must be required")]
        public string CaptchaCode { get; set; }
        
    }
}