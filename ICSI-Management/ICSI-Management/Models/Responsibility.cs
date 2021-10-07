using ICSI_Management.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Responsibility
    {
        [Required(ErrorMessage = "Responsibility Name must be Required")]
        public string Resp_Name { get; set; }

        [Required(ErrorMessage = "Responsibility Description must be Required")]
        public string Resp_Desc { get; set; }

        [Required(ErrorMessage = "Responsibility Type must be Required")]
        public int Resp_Type { get; set; }

        [Required(ErrorMessage = "Responsibility Symbolic Name must be Required")]
        public string Resp_Sym_Name{ get; set; }

        public bool Read_Write { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public DateTime ModifyDate { get; set; }

        public string RespTypeName { get; set; }

        public int RefId { get; set; }
        public string RefName { get; set; }
        public List<Responsibility> lstResponsibility { get; set; }
        public List<DropdownValue> lstDropDown { get; set; }
    }
}