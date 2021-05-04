using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;


namespace Amazon.Model.Implementation.Addresses
{
    public class PreviousCorrectionAddressCheckService : IAddressCheckService
    {
        private IDbFactory _dbFactory;

        public AddressProviderType Type
        {
            get { return AddressProviderType.SelfCorrection; }
        }

        public PreviousCorrectionAddressCheckService(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO inputAddress)
        {
            if (String.IsNullOrEmpty(inputAddress.BuyerEmail))
            {
                return new CheckResult<AddressDTO>()
                {
                    Status = (int) AddressValidationStatus.Valid,
                    AdditionalData = null,
                };
            }

            var additionalData = new List<string>() {OrderNotifyType.AddressCheckWithPerviousCorrection.ToString()};

            Order lastCorrection = null;
            

            using (var db = _dbFactory.GetRDb())
            {
                lastCorrection = db.Orders.GetAddressCorrectionForBuyer(inputAddress.BuyerEmail,
                    inputAddress.Address1,
                    inputAddress.Address2,
                    inputAddress.City,
                    inputAddress.State,
                    inputAddress.Country,
                    inputAddress.Zip,
                    inputAddress.ZipAddon);
            }
            if (lastCorrection != null)
            {
                var outputAddress = new AddressDTO()
                {
                    FullName = inputAddress.FullName,
                    Phone = inputAddress.Phone,

                    Country = lastCorrection.ManuallyShippingCountry,
                    City = lastCorrection.ManuallyShippingCity,
                    State = lastCorrection.ManuallyShippingState,
                    Address1 = lastCorrection.ManuallyShippingAddress1,
                    Address2 = lastCorrection.ManuallyShippingAddress2,
                    Zip = lastCorrection.ManuallyShippingZip,
                    ZipAddon = lastCorrection.ManuallyShippingZipAddon,
                };

                return new CheckResult<AddressDTO>()
                {
                    Status = (int)AddressValidationStatus.Valid,
                    AdditionalData = additionalData,
                    Data = outputAddress
                };
            }
            return new CheckResult<AddressDTO>()
            {
                Status = (int)AddressValidationStatus.Invalid,
                AdditionalData = additionalData,
            };
        }
    }
}
