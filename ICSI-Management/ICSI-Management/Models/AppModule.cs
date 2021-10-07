using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class AppModule
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Role Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string ModuleName { get; set; }        
        [Required(ErrorMessage = "Module Description must be required")]
        public string ModuleDescription { get; set; }        
        public int AppId { get; set; }
        public List<Application> apps { get; set; }
        public string AppName { get; set; }
        public string SchemaName { get; set; }
        public string MandModuleName { get; set; }
        public int MandModuleId { get; set; }
        public List<AppModule> lstMandMoudule { get; set; }
        public bool Active { get; set; }
        public List<AppModule> AllAppModules { get; set; }
    }
}