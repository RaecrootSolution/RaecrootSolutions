using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_Management.Models
{
    public class ScreenComponent
    {
        public int Id { get; set; }
        public int ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string ScreenSymName { get; set; }
        public int RefId { get; set; }
        public int OrderNumber { get; set; }
        public int ComponentType { get; set; }
        public string ComponentTypeName { get; set; }
        public string ComponentContent { get; set; }
        public string ComponentName { get; set; }
        public string ComponentValue { get; set; }
        public string ComponentText { get; set; }
        public string ComponentStyle { get; set; }
        public string ComponentScript { get; set; }
        public string ScreenRefMethod { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public bool IsMand { get; set; }
        public string Where { get; set; }
        public string ComponentClassName { get; set; }
        public bool ComponentClassStatic { get; set; }
        public string ComponentMethodName { get; set; }
        public bool ComponentMethodStatic { get; set; }
        public string ColumnName { get; set; }
        public bool ReadWrite { get; set; }
        public string SQL { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public bool Active { get; set; }
        public int ManComId { get; set; }
        public string ManComName { get; set; }
        public List<ScreenComponent> lstMndCom { get; set; }
        public List<ScreenComponent> lst { get; set; }
        public List<Screen> lstScreen { get; set; }
        public List<Report> lstReport { get; set; }
    }
}