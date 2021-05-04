using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels
{
    public class AddressViewModel
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string FullName { get; set; }
        public string City { get; set; }
        public string USAState { get; set; }
        public string NonUSAState { get; set; }

        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public DateTime? ShipDate { get; set; }

        public bool IsVerified { get; set; }

        [ToStringIgnore]
        public bool IsCountryUSA
        {
            get
            {
                return !ShippingUtils.IsInternational(Country);// Country == "US" || string.IsNullOrEmpty(Country);
            }
        }

        public AddressViewModel()
        {

        }

        public AddressViewModel(AddressDTO address)
        {
            FullName = address.FullName;

            Country = address.Country;
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            USAState = StringHelper.ToUpper(address.State);
            NonUSAState = StringHelper.ToUpper(address.State);
            Zip = address.Zip;
            ZipAddon = address.ZipAddon;

            Phone = address.Phone;
            ShipDate = address.ShipDate;
        }

        public AddressDTO GetAddressDto()
        {
            return new AddressDTO()
            {
                Country = Country ?? Constants.DefaultCountryCode,
                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = IsCountryUSA ? USAState : NonUSAState,
                Zip = Zip,
                ZipAddon = ZipAddon,

                FullName = FullName,
                Phone = Phone,
                BuyerEmail = Email,
                ShipDate = ShipDate,
                IsVerified = IsVerified,
            };
        }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}