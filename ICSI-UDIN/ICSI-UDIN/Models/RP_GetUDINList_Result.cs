using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_UDIN.Models
{
    public class RP_GetUDINList_Result
    {
        public string MembershipNumber { get; set; }
        public string MRN { get; set; }
        public string DocumentType { get; set; }
        public string DocumentDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FinancialYear { get; set; }
        public string ClientName { get; set; }
        public string CINNumber { get; set; }
        public string MemberDetails { get; set; }
        public string UDINRevokeReason { get; set; }
        public int ID { get; set; }
        public int UserId { get; set; }
        public string IsValid { get; set; }
        public DateTime DateOfSigningDoc { get; set; }
        public string PANNumber { get; set; }
        public string AadharNumber { get; set; }
        public string CertificateDesc { get; set; }
        public string CertificateType { get; set; }
        public int TypesOfDocument { get; set; }





    }
}