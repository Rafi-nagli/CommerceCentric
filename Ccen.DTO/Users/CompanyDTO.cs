using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Users
{
    public class CompanyDTO
    {
        public long Id { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }

        public string FullName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Phone { get; set; }

        
        public string AmazonFeedMerchantIdentifier { get; set; }
        public string MelissaCustomerId { get; set; }
        public string USPSUserId { get; set; }
        public string CanadaPostKeys { get; set; }

        public string SellerName { get; set; }
        public string SellerEmail { get; set; }

        public string SellerAlertName { get; set; }
        public string SellerAlertEmail { get; set; }

        public string SellerWarehouseEmailName { get; set; }
        public string SellerWarehouseEmailAddress { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }


        //Navigation
        public IList<ShipmentProviderDTO> ShipmentProviderInfoList { get; set; }
        public IList<EmailAccountDTO> EmailAccounts { get; set; }
        public IList<SQSAccountDTO> SQSAccounts { get; set; }
        public IList<AddressProviderDTO> AddressProviderInfoList { get; set; }
        public IList<CompanyAddressDTO> AddressList { get; set; }


        //public AddressDTO GetReturnAddressDto()
        //{
        //    return new AddressDTO()
        //    {
        //        FullName = FullName,
        //        Address1 = Address1,
        //        Address2 = Address2,
        //        City = City,
        //        State = State,
        //        Country = Country,
        //        Zip = Zip,
        //        ZipAddon = ZipAddon,
        //        Phone = Phone,

        //        BuyerEmail = SellerEmail,
        //        CompanyName = CompanyName,
        //        ContactName = FullName,
        //    };
        //}

        //public AddressDTO GetPickupAddressDto()
        //{
        //    return new AddressDTO()
        //    {
        //        FullName = FullName,
        //        Address1 = Address1,
        //        Address2 = Address2,
        //        City = City,
        //        State = State,
        //        Country = Country,
        //        Zip = Zip,
        //        ZipAddon = ZipAddon,
        //        Phone = Phone,

        //        BuyerEmail = SellerEmail,
        //        CompanyName = CompanyName,
        //        ContactName = FullName,
        //    };
        //}
    }
}
