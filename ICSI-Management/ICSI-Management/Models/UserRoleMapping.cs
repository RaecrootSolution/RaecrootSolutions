using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class UserRoleMapping
    {
        [Required(ErrorMessage = "Please choose User Name")]
        public int User_Id { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public DateTime ModifyDate { get; set; }

        public List<User> lstUsers { get; set; }
        public List<Roles> lstRoles { get; set; }
        public List<UserRoleMapping> lstUserRoleMapping { get; set; }
        public List<UserRoleMapping> lstRoleByUserId { get; set; }
    }
}