using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class StallBooking
    {
        public int Id { get; set; }        
        public int EventId { get; set; }
        public int CountryId { get; set; }
        [Required(ErrorMessage = "Sate  Required")]
        public int StateId { get; set; }
        public int CityId { get; set; }
        [Required(ErrorMessage = "Company Name  Required")]
        public string CompanyName { get; set; }
        public string UserName { get; set; }
        //[Required(ErrorMessage = "GST Number  Required")]
        //[RegularExpression(@"^([0][1-9]|[1-2][0-9]|[3][0-5])([a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}[zZ]{1}[0-9a-zA-Z]{1})+$", ErrorMessage = "GST Number is not valid.")]
        public string GSTN { get; set; }
        [Required(ErrorMessage = "Address is Required")]
        [StringLength(100, ErrorMessage = "Length should be less than 100")]
        public string Add1 { get; set; }
        //[Required(ErrorMessage = "Address 2  Required")]
        [StringLength(100, ErrorMessage = "Length should be less than 100")]
        public string Add2 { get; set; }
        [Required(ErrorMessage = "Email Id  Required")]        
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email Id is not valid.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mobile Number  required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Mobile Number is not valid.")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "Sate Name  Required")]
        public int StateCode { get; set; }
        
        public string UserId { get; set; }
       
        public string Password { get; set; }
        
        public string RePassword { get; set; }
        [Required(ErrorMessage = "City  Required")]
        public string City { get; set; }
        public bool Approved { get; set; }
        public bool Active { get; set; }
        [Required(ErrorMessage = "Pin Code  Required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Number is allowed.")]
        public string PinCode { get; set; }
        public List<Country> lstCountry { get; set; }
        public List<State> lstState { get; set; }
        public List<Event> lstEvent { get; set; }
        public decimal Amount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal GST18Amount { get; set; }
        public decimal TotalAmount { get; set; }
        [Required(ErrorMessage = "Stall Description  Required")]
        public string StatllDescription { get; set; }
        [Required(ErrorMessage = "Contact Person  Required")]
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special characters are not allowed.")]
        public string ContactPersion { get; set; }
        [Required(ErrorMessage = "Stall Number  Required")]
        public int StallNumberId { get; set; }
        public List<StallNumber> lstNumber { get; set; }
        public List<BookedStall> lstBooked { get; set; }
        public string StallNumber { get; set; }
        public string Description { get; set; }
        public string SACCODE { get; set; }
        public string ICSIGST { get; set; }

        public string IsYesNo { get; set; }
    }

}
