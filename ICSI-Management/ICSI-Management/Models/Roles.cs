using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Roles
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Role Name must be required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Special Charecter not allowed")]
        public string RoleName { get; set; }
        public string RoleSysName { get; set; }
        [Required(ErrorMessage = "Role Description must be required")]
        public string RoleDescription { get; set; }
        [Required(ErrorMessage = "Role Order must be required")]
        public int RoleOrder { get; set; }
        public bool Active { get; set; }
        public List<Roles> AllRoles { get; set; }
    }
}