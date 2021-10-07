using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_WebApp.Models
{
    public class Screen_Comp_T
    {
        public int Id { get; set; }
        public int Screen_Id { get; set; }
        public int Ref_Id { get; set; }
        public int Order_Nm { get; set; }
        public int Comp_Type_Nm { get; set; }
        public int reportId { get; set; }
        public string compNameTx { get; set; }
        public string compContentTx { get; set; }
        public string compValueTx { get; set; }
        public string compTextTx { get; set; }
        public string compStyleTx { get; set; }
        public string compScriptTx { get; set; }
        public string screenReferenceMethodNameTx { get; set; }
        public string screenCompClassNameTx { get; set; }
        public bool isScreenCompClassStatic { get; set; }
        public string screenCompMethodNameTx { get; set; }
        public bool isScreenCompMethodStatic { get; set; }
        public string schemaNameTx { get; set; }
        public string tableNameTx { get; set; }
        public string columnNameTx { get; set; }
        public bool isMandatoryYn { get; set; }
        public bool isReadWriteYn { get; set; }
        public string sql { get; set; }
        public string where { get; set; }
        public string dynWhere { get; set; }
        public List<Screen_Comp_T> ScreenCompList { get; set; }
        public string WebAppSchemaNameTx { get; set; }
        public string ApplicationSchemaNameTx { get; set; }
        public string ModuleSchemaNameTx { get; set; }
        public string ScreenSchemaNameTx { get; set; }
        public int minRows { get; set; }
        public int maxRows { get; set; }
        public bool isPaginationYn { get; set; }


        //FOR SAR AWARDS
        public int categoryId { get; set; }
        public int finYrId { get; set; }
    }
}