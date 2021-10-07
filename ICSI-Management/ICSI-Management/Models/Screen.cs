using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class Screen
    {
        [Required(ErrorMessage = "Screen Name must be Required")]
        public string Screen_Name { get; set; }
        public string Screen_Sym_Name { get; set; }
        [Required(ErrorMessage = "Screen Title must be Required")]
        public string Screen_Title { get; set; }

        [Required(ErrorMessage = "Screen File Name must be Required")]
        public string Screen_File_Name { get; set; }

        [Required(ErrorMessage = "Screen Style must be Required")]
        public string Screen_Style { get; set; }

        [Required(ErrorMessage = "Screen Script must be Required")]
        public string Screen_Script { get; set; }

        [Required(ErrorMessage = "Screen Content must be Required")]
        public string Screen_Content { get; set; }

        [Required(ErrorMessage = "Screen Class Name must be Required")]
        public string Screen_Class_Name { get; set; }

        [Required(ErrorMessage = "Screen Reff Class Name must be Required")]
        public string Screen_Ref_Class_Name { get; set; }

        public bool Screen_Class_Static { get; set; }

        [Required(ErrorMessage = "Screen Method Name must be Required")]
        public string Screen_Method_Name { get; set; }

        public bool Screen_Method_Static { get; set; }
        [Required(ErrorMessage = "Schema Name must be Required")]
        public string Schema_Name { get; set; }
        [Required(ErrorMessage = "Table Name must be Required")]
        public string Table_Name { get; set; }

        [Required(ErrorMessage = "Action_TX must be Required")]
        public string Action_TX { get; set; }

        [Required(ErrorMessage = "App Module Name must be Required")]
        public int AppModuleId { get; set; }
        public String AppModuleName { get; set; }
        public List<AppModule> lstAppModule { get; set; }

        public bool Active { get; set; }
        public int ID { get; set; }
        public DateTime ModifyDate { get; set; }

        public List<Screen> lstScreen { get; set; }

        public int MandScreenId { get; set; }
        public string MandScreenName { get; set; }
        public List<Screen> mandSList { get; set; }
    }
}