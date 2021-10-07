using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ICSI_Event.Models
{
    public class Login
    {
        [Required(ErrorMessage = "UserName Required")]

        [DisplayName("UserName")]

        // [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",

        //          ErrorMessage = "Email Format is wrong")]

        // [StringLength(50, ErrorMessage = "Less than 50 characters")]

        public string UserName

        {

            get; set;

        }


        [DataType(DataType.Password)]

        [Required(ErrorMessage = "Password Required")]

        [DisplayName("Password")]

        [StringLength(30, ErrorMessage = ":Less than 30 characters")]

        public string Password

        {

            get; set;

        }
        public bool IsExistUser(string userName, string password)
        {
            return true;
        }

    }
}