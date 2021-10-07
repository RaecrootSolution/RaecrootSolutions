using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ICSI_eCSIN.Models
{
    public class Forgotpassword
    {        
        [Display(Name = "Membership Number :")]
        [Required(ErrorMessage = "Membership Number must be required")]
        public string MemmbershipNumber { get; set; }

        //[Required]
        //[Display(Name = "Date Of Birth")]
        //[DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "DOB must be required")]
        public string DOB { get; set; }
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        // public string DOB { get; set; }



        //[Required]
        //[Display(Name = "Year Of Enrolment :")]
        //[RegularExpression(@"^([0-9]{4})$", ErrorMessage = " Year Of Enrolment  must be 4 digits")]
        //public int YearOfEnrolment { get; set; }

        public string MaskEmail(string s)
        {
            string pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{4}@)";
            string result = Regex.Replace(s, pattern, m => new string('*', m.Length));
            return result;
        }
        public string EmailId { set; get; }

    }
}