//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICSI_UDIN.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblDocumentType
    {
        public int DocumentTypeID { get; set; }
        public string DocumentType { get; set; }
        public string TypeDesc { get; set; }
        public string IsValid { get; set; }
        public Nullable<int> IssueCerNo { get; set; }
        public Nullable<int> IssueCerPNo { get; set; }
        public Nullable<int> TypesOfDocument { get; set; }
        public Nullable<int> MaxNumber { get; set; }
        public Nullable<int> Pu_MaxNumber { get; set; }
    }
}
