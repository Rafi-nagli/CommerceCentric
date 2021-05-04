using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Google.Geocoding.Api.Google;


namespace Amazon.Model.Implementation.Addresses
{
    public class GoogleGeocodeAddressCheckService : IAddressCheckService
    {
        private ILogService _log;
        private AddressProviderDTO _addressProviderInfo;

        public AddressProviderType Type
        {
            get { return (AddressProviderType)_addressProviderInfo.Type; }
        }

        public GoogleGeocodeAddressCheckService(ILogService log,
            AddressProviderDTO addressProviderInfo)
        {
            _log = log;
            _addressProviderInfo = addressProviderInfo;
        }

        public CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO inputAddress)
        {
            //var geocoder = new GoogleGeocoder() {ApiKey = _addressProviderInfo.Key1};

            ////Should be only if UI call
            //if (callSource == CallSource.UI)
            //{
            //    //Ignore address2
            //    inputAddress.Address2 = "";
            //}
            //var searchString = AddressHelper.ToString(inputAddress, " ");
            //IEnumerable<GoogleAddress> addresses = geocoder.Geocode(searchString);
            //var firstMatch = addresses.FirstOrDefault();

            //var isValid = firstMatch != null 
            //    && firstMatch.LocationType == GoogleLocationType.Rooftop
            //    && !firstMatch.IsPartialMatch;

            //var message = String.Empty;
            //if (firstMatch != null)
            //{
            //    message = "LocationType=" + firstMatch.LocationType 
            //        + ", IsPartialMatch=" + StringHelper.ToYesNo(firstMatch.IsPartialMatch);
            //}

            //return new CheckResult<AddressDTO>()
            //{
            //    Status = isValid ? (int)AddressValidationStatus.Valid : (int)AddressValidationStatus.Invalid,
            //    Message = message,
            //    AdditionalData = new List<string>() { OrderNotifyType.AddressCheckGoogleGeocode.ToString() }
            //};

            return new CheckResult<AddressDTO>()
            {
                Status = (int)AddressValidationStatus.Valid,
                Message = "[Service unavailable]",
                AdditionalData = new List<string>() { OrderNotifyType.AddressCheckGoogleGeocode.ToString() }
            };
        }
    }
}
