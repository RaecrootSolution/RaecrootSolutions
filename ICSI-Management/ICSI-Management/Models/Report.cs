using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Report
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Report Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string ReportName { get; set; }
        [Required(ErrorMessage = "SQl Query must be required")]
        public string SQL { get; set; }
        [Required(ErrorMessage = "Column must be required")]
        public string Column { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
        public List<Report> AllReport { get; set; }

    }
}