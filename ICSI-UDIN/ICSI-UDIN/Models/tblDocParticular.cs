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
    
    public partial class tblDocParticular
    {
        public int ID { get; set; }
        public string ParticularType { get; set; }
        public string ParticularDesc { get; set; }
        public Nullable<int> DocumentTypeID { get; set; }
        public string IsValid { get; set; }
    }
}