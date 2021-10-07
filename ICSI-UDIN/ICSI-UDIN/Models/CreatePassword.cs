using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace ICSI_UDIN.Models
{
    public class CreatePassword
    {

        [Required(ErrorMessage = "New password is required", AllowEmptyStrings = false)]
        [StringLength(25, ErrorMessage = "Password Must be between 6 to 25 characters length", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Password should be alpha numeric")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password does not match")]
        public string ConfirmPassword { get; set; }



    }
}