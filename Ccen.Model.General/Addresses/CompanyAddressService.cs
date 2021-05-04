using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Model.General
{
    public class CompanyAddressService : ICompanyAddressService
    {
        private string _fullName { get; set; }
        private string _companyName { get; set; }
        private string _contactEmail { get; set; }
        
        private IList<AddressDTO> _addressList { get; set; }
        private IList<MarketplaceDTO> _markets { get; set; }

        public bool IsVerified { get; set; }

        public CompanyAddressService(AddressDTO address)
        {
            if (address == null)
                throw new ArgumentNullException("Input Address DTO is empty");

            _fullName = address.FullName;
            _companyName = address.CompanyName;
            _contactEmail = address.BuyerEmail;

            _addressList = new List<AddressDTO>() { address };
        }

        public CompanyAddressService(CompanyDTO company, IList<MarketplaceDTO> markets = null)
        {
            if (company.AddressList == null)
                throw new ArgumentNullException("Company Address List is empty");

            _fullName = company.FullName;
            _companyName = company.CompanyName;
            _contactEmail = company.SellerEmail;

            _addressList = company.AddressList.Select(a => a.GetAddressDto()).ToList();
            _markets = markets;
        }

        public AddressDTO GetPickupAddress(MarketIdentifier market)
        {
            var pickupAddress = _addressList.FirstOrDefault(p => p.Type == (int)CompanyAddressTypes.Physical);
            if (pickupAddress == null)
                pickupAddress = _addressList.FirstOrDefault();

            return new AddressDTO()
            {
                Type = pickupAddress.Type,
                FullName = _fullName,

                Address1 = pickupAddress.Address1,
                Address2 = pickupAddress.Address2,
                City = pickupAddress.City,
                State = pickupAddress.State,
                Zip = pickupAddress.Zip,
                ZipAddon = pickupAddress.ZipAddon,
                Country = pickupAddress.Country,
                Phone = pickupAddress.Phone,

                BuyerEmail = _contactEmail,
                CompanyName = _companyName,
                ContactName = _fullName,
            };
        }

        public AddressDTO GetReturnAddress(MarketIdentifier market)
        {
            var returnAddress = _addressList.FirstOrDefault(p => p.Type == (int)CompanyAddressTypes.Default);
            if (returnAddress == null)
                returnAddress = _addressList.FirstOrDefault();

            return new AddressDTO()
            {
                Type = returnAddress.Type,
                FullName = _fullName,

                Address1 = returnAddress.Address1,
                Address2 = returnAddress.Address2,
                City = returnAddress.City,
                State = returnAddress.State,
                Zip = returnAddress.Zip,
                ZipAddon = returnAddress.ZipAddon,
                Country = returnAddress.Country,
                Phone = returnAddress.Phone,

                BuyerEmail = _contactEmail,
                CompanyName = _companyName,
                ContactName = _fullName,
            };
        }
    }
}
