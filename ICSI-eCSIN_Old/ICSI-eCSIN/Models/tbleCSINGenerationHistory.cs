//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICSI_eCSIN.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbleCSINGenerationHistory
    {
        public int HistoryId { get; set; }
        public int eCSINGenerationId { get; set; }
        public string PrevDesignation { get; set; }
        public string CurrentDesignation { get; set; }
        public System.DateTime DateOfChangeDesignation { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual tbleCSINGeneration tbleCSINGeneration { get; set; }
    }
}
