//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICSI_Management.DBContext
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ICSI_DBModelEntities : DbContext
    {
        public ICSI_DBModelEntities()
            : base("name=ICSI_DBModelEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<APP_MODULE_T> APP_MODULE_T { get; set; }
        public virtual DbSet<APPLICATION_T> APPLICATION_T { get; set; }
        public virtual DbSet<MENU_T> MENU_T { get; set; }
        public virtual DbSet<REPORT_T> REPORT_T { get; set; }
        public virtual DbSet<RESP_SCREEN_T> RESP_SCREEN_T { get; set; }
        public virtual DbSet<RESPONSIBILITY_T> RESPONSIBILITY_T { get; set; }
        public virtual DbSet<ROLE_RESP_T> ROLE_RESP_T { get; set; }
        public virtual DbSet<ROLE_T> ROLE_T { get; set; }
        public virtual DbSet<SCREEN_COMP_T> SCREEN_COMP_T { get; set; }
        public virtual DbSet<SCREEN_T> SCREEN_T { get; set; }
        public virtual DbSet<SUPER_USER_T> SUPER_USER_T { get; set; }
        public virtual DbSet<USER_RESP_T> USER_RESP_T { get; set; }
        public virtual DbSet<USER_ROLE_T> USER_ROLE_T { get; set; }
        public virtual DbSet<USER_SCREEN_COMP_T> USER_SCREEN_COMP_T { get; set; }
        public virtual DbSet<USER_T> USER_T { get; set; }
        public virtual DbSet<USER_TYPE_T> USER_TYPE_T { get; set; }
        public virtual DbSet<WEB_APPLICATION_T> WEB_APPLICATION_T { get; set; }
    }
}
