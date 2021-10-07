using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_eCSIN.Models
{
    public class eCSINDetails
    {
        public int eCSINGenerationId { get; set; }
        public int UserId { get; set; }
        public string MemberName { get; set; }
        public string eCSINGeneratedNo { get; set; }
        public DateTime? DateOfChangeMembershipNo { get; set; }
        public string RestorationOfMembership { get; set; }
        public string EmployeeDesignation { get; set; }
        public string EmployerCINNo { get; set; }
        public string EmployerName { get; set; }
        public string EmployerRegAdd { get; set; }
        public DateTime DateOfOfferLetter { get; set; }
        public DateTime DateOfConsentLetter { get; set; }
        public DateTime DateOfAppointment { get; set; }
        public DateTime? DateOfNoticeResig_NoticeOfTermination { get; set; }
        public DateTime? DateOfCessationEmployment { get; set; }
        public string WebsiteOfEmployer { get; set; }
        public string EmployerEmailId { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string RevokeReason { get; set; }
        public bool IsAccepted { get; set; }
        public string DateOfNoticeResig_NoticeOfTermination1 { get; set; }
        public string DateOfCessationEmployment1 { get; set; }
        public DateTime? DateOfUpdateDesignation { get; set; }
        public string MemberNumber {get;set;}
        public string PANNumber { get; set; }
        public string CessationAcpReason { get; set; }
        public decimal Amount { get; set; }
        

    }
}