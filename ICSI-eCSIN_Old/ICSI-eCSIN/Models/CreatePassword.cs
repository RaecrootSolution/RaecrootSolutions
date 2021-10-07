using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ICSI_eCSIN.Models
{
    public class CreatePassword
    {
        [Required(ErrorMessage = "New password must be required", AllowEmptyStrings = false)]
        [StringLength(15, ErrorMessage = "New Password should be alphanumeric and minimum 6 characters of length", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "New Password should be alphanumeric")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password must be required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password does not match")]
        public string ConfirmPassword { get; set; }
    }
}