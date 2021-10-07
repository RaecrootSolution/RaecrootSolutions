using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ICSI_UDIN.Models
{
    public class GenerateUDIN
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]

        [Required(ErrorMessage = "MRN Number must be required")]
        public string MRNNumber { get; set; }
        [Required(ErrorMessage = "Client Name must be required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        //[RegularExpression("/^[a-zA-Z]*$/", ErrorMessage = "Please insert only characters")]
        public string ClientName { get; set; }

        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special characters are not allowed.")]
        [Required(ErrorMessage = "Number must be required")]
        public string CINNumber { get; set; }

        [Required(ErrorMessage = "Financial Year must be required")]
        public string FinancialYear { get; set; }

        public string FinancialYear_Remark { get; set; }

        [Required(ErrorMessage = "Date Of Signing Document must be required")]
        public string DateOfSigningDoc { get; set; }
        //[Required(ErrorMessage = "Document Description must be required.")]
        [StringLength(200, ErrorMessage = "Maximum 200 characters is allowed for Document description! Please shorten the length and try again.")]
        public string DocDescription { get; set; }

        public string UDINNumber { get; set; }
        public int CertificateId { get; set; }
        public List<Certificate> lstCertificates { get; set; }
        //[Required(ErrorMessage = "PAN Number must be required")]
        //public string PANNumber { get; set; }
        //[Required(ErrorMessage = "Aadhar Number must be required")]
        //public string AdharNumber { get; set; }
        [Required(ErrorMessage = "Please Select CIN/PAN/Aadhar")]
        public string Number { get; set; }

    }

    public class Certificate
    {
        public int CertificateId { get; set; }
        public string CertificateName { get; set; }
        public int? MaxNumber { get; set; }
        public int? Pu_MaxNumber { get; set; }

    }
}