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
    
    public partial class tbleCSINGeneration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbleCSINGeneration()
        {
            this.tbleCSINGenerationHistories = new HashSet<tbleCSINGenerationHistory>();
        }
    
        public int eCSINGenerationId { get; set; }
        public int UserId { get; set; }
        public string eCSINGeneratedNo { get; set; }
        public Nullable<System.DateTime> DateOfChangeMembershipNo { get; set; }
        public string RestorationOfMembership { get; set; }
        public string EmployeeDesignation { get; set; }
        public string EmployerCINNo { get; set; }
        public string EmployerName { get; set; }
        public string EmployerRegAdd { get; set; }
        public System.DateTime DateOfOfferLetter { get; set; }
        public System.DateTime DateOfConsentLetter { get; set; }
        public System.DateTime DateOfAppointment { get; set; }
        public Nullable<System.DateTime> DateOfNoticeResig_NoticeOfTermination { get; set; }
        public Nullable<System.DateTime> DateOfCessationEmployment { get; set; }
        public string WebsiteOfEmployer { get; set; }
        public string EmployerPhoneNo { get; set; }
        public string EmployerEmailId { get; set; }
        public bool Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> RevokeDate { get; set; }
        public string RevokeReason { get; set; }
        public string FinancialYear { get; set; }
        public bool IsAccepted { get; set; }
        public string PANNumber { get; set; }
        public string CessationAcpReason { get; set; }
        public Nullable<bool> Subsidiarye_Status { get; set; }
        public string Remarks { get; set; }
        public string Sub_Remarks { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string Remarks2 { get; set; }
        public string Remarks3 { get; set; }
        public string Remarks4 { get; set; }
        public string Remarks5 { get; set; }
        public string EmployerMobileNo { get; set; }
    
        public virtual tblUser tblUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbleCSINGenerationHistory> tbleCSINGenerationHistories { get; set; }
    }
}
