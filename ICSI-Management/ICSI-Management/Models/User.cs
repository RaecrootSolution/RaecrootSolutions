using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICSI_Management.Models
{
    public class User
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long UserTypeId { get; set; }
        public string UserName { get; set; }
        public string LoginId { get; set; }
        public string LoginPwd { get; set; }
        public bool Active { get; set; }
        public string UserType { get; set; }
        public List<User> lst { get; set; }
        public List<SelectListItem> usertypelst { get; set; }
    }
}
