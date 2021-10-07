using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public List<UserRole> userrole { get; set; }
    }
}