using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;
using Amazon.Core.Entities;

namespace Amazon.Core.Models.Extensions
{
    public static class OrderExtensions
    {
        public static AddressDTO GetAddressDto(this Order order)
        {
            var address = new AddressDTO();
            
            address.FullName = order.PersonName;
            address.BuyerEmail = order.BuyerEmail;
            address.IsManuallyUpdated = order.IsManuallyUpdated;

            address.ManuallyFullName = order.ManuallyPersonName;
            address.ManuallyAddress1 = order.ManuallyShippingAddress1;
            address.ManuallyAddress2 = order.ManuallyShippingAddress2;
            address.ManuallyCity = order.ManuallyShippingCity;
            address.ManuallyState = order.ManuallyShippingState;
            address.ManuallyCountry = order.ManuallyShippingCountry;
            address.ManuallyZip = order.ManuallyShippingZip;
            address.ManuallyZipAddon = order.ManuallyShippingZipAddon;
            address.ManuallyPhone = order.ManuallyShippingPhone;

            address.Address1 = order.ShippingAddress1;
            address.Address2 = order.ShippingAddress2;
            address.City = order.ShippingCity;
            address.State = order.ShippingState;
            address.Country = order.ShippingCountry;
            address.Zip = order.ShippingZip;
            address.ZipAddon = order.ShippingZipAddon;
            address.Phone = order.ShippingPhone;

            return address;
        }
    }
}
