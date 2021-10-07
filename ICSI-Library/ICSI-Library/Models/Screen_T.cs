using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_WebApp.Models
{
    public class Screen_T
    {
        public int ID { get; set; }
        public int App_Module_Id { get; set; }
        public int Success_Message_Nm { get; set; }
        public int Failure_Message_Nm { get; set; }
        public string Unique_Field_Tx { get; set; }
        public string Unique_Field_Label_Tx { get; set; }
        public string Screen_Name_Tx { get; set; }
        public string Screen_Sym_Name_Tx { get; set; }
        public string Screen_Title_Tx { get; set; }
        public int Screen_Next_Id { get; set; }
        public string Screen_Style_Tx { get; set; }
        public string Screen_Script_Tx { get; set; }
        public string Mandatory_Fields_Tx { get; set; }
        public string Mandatory_Field_Labels_Tx { get; set; }
        public string Screen_Content_Tx { get; set; }
        public string Screen_Ref_Class_Name_Tx { get; set; }
        public string Screen_Class_Name_Tx { get; set; }
        public bool? Screen_Class_Static_YN { get; set; }
        public string Screen_Method_Name_Tx { get; set; }
        public string schemaNameTx { get; set; }
        public bool? Screen_Method_Static_YN { get; set; }
        public int? Mandatory_Scr_Id { get; set; }
        public int? Edit_Scr_Id { get; set; }
        public string Table_Name_Tx { get; set; }
        public string Action_Tx { get; set; }
        public bool Active_YN { get; set; }  
        public ActionClass resActionClass { get; set; }
        public string WebAppSchemaNameTx { get; set; }
        public string ApplicationSchemaNameTx { get; set; }
        public string ModuleSchemaNameTx { get; set; }
        public bool is_Email_yn { get; set; }
        public bool is_SMS_yn { get; set; }
        public List<Screen_Comp_T> ScreenComponents { get; set; }
    }

    public class ActionClass
    {
        public JObject jObject;
        public JObject columnMetadata;
        public string StatCode;
        public string StatMessage;
        public string UniqueId;
        public string ScreenType;
        public string DecryptData;
        public JObject QryData;
        public List<string> manSearchFilter;
    }
}