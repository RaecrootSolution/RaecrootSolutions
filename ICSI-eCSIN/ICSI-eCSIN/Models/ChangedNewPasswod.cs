using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class ChangedNewPasswod
    {
        [Required(ErrorMessage = "User Name is required", AllowEmptyStrings = false)]
        public string UserName { get; set; }
        [Required(ErrorMessage = "New Password is required", AllowEmptyStrings = false)]
        [StringLength(25, ErrorMessage = "Password must be between 6 to 25 characters length", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Password should be alpha numeric")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password and confirm Password does not match")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Old Password is required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

    }
}