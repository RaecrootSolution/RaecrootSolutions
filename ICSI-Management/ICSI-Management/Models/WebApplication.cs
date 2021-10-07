using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class WebApplication
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Web Application Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string WebAppName { get; set; }
        public string WebURL { get; set; }
        public string WebAppDesc { get; set; }
        public string SchemaName { get; set; }
        public bool Active { get; set; }

        public List<WebApplication> allapplications { get; set; }

    }
}