using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ICSI_Event.Models
{
    public class EventsRegistration
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Event Name  Required")]
        public int EventId { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        [Required(ErrorMessage = "Company Name  Required")]
        public string CompanyName    { get; set; }
        public string UserName { get; set; }
        
        public string GSTN { get; set; }
        [Required(ErrorMessage = "Addrewss 1  Required")]
        [StringLength(100, ErrorMessage = "Length should be less than 100")]
        public string BillingAdd     { get; set; }
        //[Required(ErrorMessage = "Address 2  Required")]
        [StringLength(100, ErrorMessage = "Length should be less than 100")]
        public string ShipingAdd { get; set; }
        [Required(ErrorMessage = "Email Id  Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mobile Number  Required")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "Sate Name  Required")]
        public int StateCode { get; set; }
        [Required(ErrorMessage = "User Id  Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Password   Required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Re Password  Required")]
        public string RePassword { get; set; }
        public string City { get; set; }
        public bool Approved { get; set; }
        public bool Active { get; set; }
        public string PinCode { get; set; }
        public List<Country> lstCountry { get; set; }
        public List<State> lstState { get; set; }        
        public List<Event> lstEvent { get; set; }
    }
}