using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Menu
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Menu Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string MenuName { get; set; }
        [Required(ErrorMessage = "Menu Description must be required")]
        public int OrderNumber { get; set; }
        [Required(ErrorMessage = "Menu Label must be required")]
        public string MenuLabelName { get; set; }
        public int ScreenId { get; set; }
        public List<Screen> screens { get; set; }
        public string ScreenName { get; set; }
        public bool Active { get; set; }        
        public int ParentMenuId { get; set; }
        public string ParentMenuName { get; set; }
        public List<Menu> ParentMenues { get; set; }
        public List<Menu> AllMenues { get; set; }
        [Required(ErrorMessage = "Web Application Name must be required")]
        public int WebAppId { get; set; }        
        public string WebAppName { get; set; }
        public List<WebApplication> WebApps { get; set; }
    }
}