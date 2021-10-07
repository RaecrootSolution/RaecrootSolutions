using ICSI_Management.DBContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class RoleResponsibilityMapping
    {
        [Required(ErrorMessage = "Please choose Role Name")]
        public int Role_Id { get; set; }
        public string RoleName { get; set; }
        public int Resp_Id { get; set; }
        public string RespName { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public DateTime ModifyDate { get; set; }
        public int[] SelectedRes { get; set; }

        public List<Roles> lstRolls { get; set; }
        public List<Responsibility> lstResponsibilities { get; set; }
       // public List<Responsibility> lstExistingResponsibilities { get; set; }
        public List<RoleResponsibilityMapping> lstRoleRespMapping { get; set; }
        public List<RoleResponsibilityMapping> lstResponsibilityByRoleId { get; set; }
    }
}