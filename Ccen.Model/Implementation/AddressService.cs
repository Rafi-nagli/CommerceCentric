using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;


namespace Amazon.Model.Implementation
{
    public class AddressService : IAddressService
    {
        private IList<IAddressCheckService> _addressCheckers;
        private AddressDTO _returnAddress;
        private AddressDTO _pickupAddress;

        public AddressService(IList<IAddressCheckService> addressCheckers,
            AddressDTO returnAddress,
            AddressDTO pickupAddress)
        {
            _addressCheckers = addressCheckers;
            _returnAddress = returnAddress;
            _pickupAddress = pickupAddress;
        }

        public static AddressService Default
        {
            get { return new AddressService(null, null, null); }
        }

        public bool IsMine(AddressDTO address)
        {
            if (address == null)
                return false;

            if ((address.Zip == _returnAddress.Zip
                                    || address.Zip == _pickupAddress.Zip)
                                    && address.State == _returnAddress.State
                                    && (String.Compare(address.City, _returnAddress.City, StringComparison.OrdinalIgnoreCase) == 0 //NOTE: has different spelling
                                        || String.Compare(address.City, _pickupAddress.City, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }
            return false;
        }

        public IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            AddressDTO inputAddress)
        {
            return CheckAddress(callSource, inputAddress, null);
        }


        public IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            AddressDTO inputAddress,
            AddressProviderType[] applyAddressProviderTypes)
        {
            var checkResults = new List<CheckResult<AddressDTO>>();

            var actualAddressCheckers = _addressCheckers;
            if (applyAddressProviderTypes != null)
                actualAddressCheckers = _addressCheckers.Where(a => applyAddressProviderTypes.Contains(a.Type)).ToList();

            foreach (var checker in actualAddressCheckers)
            {
                var checkResult = checker.CheckAddress(callSource, inputAddress);
                checkResults.Add(checkResult);

                ////NOTE: break after first validation success result
                //if (checkResult.Status < (int)AddressValidationStatus.Invalid
                //    && checkResult.Status != (int)AddressValidationStatus.None)
                //    break;
            }
            return checkResults;
        }

        public string DefaultName
        {
            get { return _returnAddress.CompanyName; }
        }

        public AddressDTO ReturnAddress => _returnAddress;
        public AddressDTO PickupAddress => _pickupAddress;

        //public const string AmazonName = "Premium Apparel";
        //public const string EBayName = "Premium Apparel"; //"All4Kidz"; NOTE: stamps.com require 2 words name
        //public const string MagentoName = "Premium Apparel";
        //public const string WalmartName = "Premium Apparel";
        //public const string WalmartCAName = "Premium Apparel";
        //public const string JetName = "Premium Apparel";
        //public const string ShopifyName = "Premium Apparel";


        public string GetEmailSignature(MarketType market)
        {
            if (market == MarketType.eBay)
                return "All4Kidz";
            if (market == MarketType.WalmartCA)
                return "Happy Trends";
            if (market == MarketType.DropShipper)
                //&& MarketplaceId == MarketplaceKeeper.DsMBG) //TEMP MarketplaceId is empty
                return "Mia Belle Baby";
            return _returnAddress.CompanyName;
        }

        public string GetFullname(MarketType market)
        {
            return _returnAddress.CompanyName;
        }

        public AddressDTO GetGrouponReturnAddress()
        {
            return new AddressDTO()
            {
                FullName = "Groupon Goods",
                CompanyName = "Groupon Goods",
                Address1 = "1081 Aviation Blvd",
                City = "Hebron",
                State = "KY",
                Zip = "41048",
                Country = "US",
                Phone = "460326934"
            };
        }

        public AddressDTO GetReturnAddressByType(CompanyAddressTypes addressType)
        {
            if (addressType == CompanyAddressTypes.Canada)
                return new AddressDTO()
                {
                    FullName = "Premium Apparel",
                    CompanyName = "Premium Apparel",
                    Address1 = "3711 Cedarille Drive SW",
                    City = "Calgary",
                    State = "Alberta",
                    Zip = "T2W 3J5",
                    Country = "CA",
                    Phone = ""
                };

            if (addressType == CompanyAddressTypes.Groupon)
                return GetGrouponReturnAddress();

            if (addressType == CompanyAddressTypes.Default)
                return ReturnAddress;

            if (addressType == CompanyAddressTypes.Physical)
                return PickupAddress;

            return PickupAddress;
        }
    }
}
