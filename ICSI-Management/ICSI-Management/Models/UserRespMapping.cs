using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ICSI_Management.DBContext;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class UserRespMapping
    {
        [Required(ErrorMessage = "Please choose User Name")]
        public int User_Id { get; set; }
        public string UserName { get; set; }
        public int Resp_Id { get; set; }
        public string RespName { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public DateTime ModifyDate { get; set; }
        public int[] SelectedRes { get; set; }            
        

        public List<User> lstUsers { get; set; }
        public List<Responsibility> lstResponsibilities { get; set; }        
        public List<UserRespMapping> lstUserRespMapping { get; set; }
        public List<UserRespMapping> lstResponsibilityByUserId { get; set; }

    }
}