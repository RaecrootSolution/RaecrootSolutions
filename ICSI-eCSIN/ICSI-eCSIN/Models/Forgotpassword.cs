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
        // [RegularExpression(@"^(?![0-9]+$)[A-Za-z0-9_-]{10,30}$", ErrorMessage = "Please put correct membership number (ex : A1234/F1234)")]
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Please put correct membership number (ex : A1234/F1234)")]
        [Required(ErrorMessage = "Membership Number is required")]
        public string MemmbershipNumber { get; set; }

        //[Required]   ^[a-z][a-z0-9]+$
        //[Display(Name = "Date Of Birth")]
        //[DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "DOB is required")]
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