//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICSI_Event.DBContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class CITY_T
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public int STATE_ID { get; set; }
        public bool ACTIVE_YN { get; set; }
        public System.DateTime CREATED_DT { get; set; }
        public int CREATED_BY { get; set; }
        public System.DateTime UPDATED_DT { get; set; }
        public int UPDATED_BY { get; set; }
    
        public virtual STATE_T STATE_T { get; set; }
    }
}
