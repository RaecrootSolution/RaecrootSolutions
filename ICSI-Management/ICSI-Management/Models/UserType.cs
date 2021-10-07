using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class UserType
    {
        [Required(ErrorMessage = "Responsibility Name must be Required")]
        public string User_Type { get; set; }

        [Required(ErrorMessage = "Responsibility Description must be Required")]
        public string User_Desc { get; set; }
        
        public bool Active { get; set; }
        public long ID { get; set; }
        public DateTime ModifyDate { get; set; }

        public List<UserType> lstUserType { get; set; }
    }
}