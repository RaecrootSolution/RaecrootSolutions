using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSI_Library.Models
{
    public class Report_Comp_T
    {
        public int Id { get; set; }
        public string RepType { get; set; }
        public int Report_Id { get; set; }
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
        public List<Report_Comp_T> ScreenCompList { get; set; }
        public string WebAppSchemaNameTx { get; set; }
        public string ApplicationSchemaNameTx { get; set; }
        public string ModuleSchemaNameTx { get; set; }
        public string ScreenSchemaNameTx { get; set; }
    }
}
