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
using Fedex.Api;
using Google.Geocoding.Api.Google;


namespace Amazon.Model.Implementation.Addresses
{
    public class FedexAddressCheckService : IAddressCheckService
    {
        private ILogService _log;
        private ITime _time;
        private AddressProviderDTO _addressProviderInfo;
        private string _portalName;

        public AddressProviderType Type
        {
            get { return (AddressProviderType)_addressProviderInfo.Type; }
        }

        public FedexAddressCheckService(ILogService log,
            ITime time,
            AddressProviderDTO addressProviderInfo,
            string portalName)
        {
            _log = log;
            _time = time;
            _addressProviderInfo = addressProviderInfo;
            _portalName = portalName;
        }

        public CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO inputAddress)
        {
            //outputAddress = null;

            var fedexApi = new FedexAddressApi(_log, 
                _time, 
                _addressProviderInfo.EndPoint, 
                _addressProviderInfo.UserName,
                _addressProviderInfo.Password,
                _addressProviderInfo.Key1,
                _addressProviderInfo.Key2,
                _addressProviderInfo.Key3,
                _portalName);

            //Should be only if UI call
            if (callSource == CallSource.UI)
            {
                //Ignore address2
                inputAddress.Address2 = "";
            }
            var searchString = AddressHelper.ToString(inputAddress, " ");
            var result = fedexApi.ValidateAddress(inputAddress);
            
            var isValid = result != null 
                && result.Data != null
                && result.Data.ValidationStatus < (int)AddressValidationStatus.Invalid;

            return new CheckResult<AddressDTO>()
            {
                Status = isValid ? (int)AddressValidationStatus.Valid : (int)AddressValidationStatus.Invalid,
                Message = "Effective Address: " + StringHelper.Join(", ", result.Data.Address1, result.Data.Address2, result.Data.City, result.Data.State, result.Data.Zip, result.Data.Country),
                AdditionalData = new List<string>() { OrderNotifyType.AddressCheckFedex.ToString(), result.Data?.IsResidential.ToString(), result.Data?.State },
                Data = result.Data
            };
        }
    }
}
