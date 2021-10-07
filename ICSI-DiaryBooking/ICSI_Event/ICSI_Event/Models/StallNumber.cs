using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class StallNumber
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Stall Number  Required")]
        [StringLength(50,ErrorMessage ="Length should be less than 50")]
        public string StallNo { get; set; }
        [Required(ErrorMessage = "Stall Description  Required")]
        public string StallDescription { get; set; }
        [Required(ErrorMessage = "Stall Amount  Required")]
        [Range(1, 100000)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(12, 2)")]
        [RegularExpression("^-?(([1-9]\\d*)|0)(.0*[1-9](0*[1-9])*)?$", ErrorMessage = "Only decimal is allowed.")]
        public decimal StallAmount { get; set; }
        public int Status { get; set; }
        
    }
}