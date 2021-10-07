using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Application
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Application Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string ApplicationName { get; set; }
        [Required(ErrorMessage = "Application Description must be required")]
        public string ApplicationDescription { get; set; }        
        public bool Active { get; set; }
        public List<Application> AllApplications { get; set; }
        public List<Application> mandApps { get; set; }
        public int MandAppId { get; set; }
        public string MandAppName { get; set; }
        public int WebAppId { get; set; }
        public string SchemaName { get; set; }
        public string WebAppName { get; set; }
        public List<WebApplication> WebApps { get; set; }
    }
}