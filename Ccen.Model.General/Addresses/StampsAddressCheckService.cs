using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Stamps.Api;

namespace Amazon.Model.Implementation.Addresses
{
    public class StampsAddressCheckService : IAddressCheckService
    {
        private ILogService _log;
        private AddressProviderDTO _addressProviderInfo;

        public AddressProviderType Type
        {
            get { return (AddressProviderType)_addressProviderInfo.Type; }
        }

        public StampsAddressCheckService(ILogService log,
            AddressProviderDTO addressProviderInfo)
        {
            _addressProviderInfo = addressProviderInfo;
            _log = log;
        }


        public CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO inputAddress)
        {
            _log.Info("StampsAddressCheckService, inputAddress=" + inputAddress);

            try
            {
                var status = RetryHelper.ActionWithRetries(() => StampComService.ValidateAddress(
                    _log,
                    _addressProviderInfo,
                    inputAddress),
                    _log,
                    throwException: true);

                return new CheckResult<AddressDTO>()
                {
                    Status = (int) status,
                    AdditionalData = new List<string>() {OrderNotifyType.AddressCheckStamps.ToString()},
                };
            }

            catch (FaultException ex)
            {
                if (ExceptionHelper.IsStampsConversationSyncEx(ex)
                    || ExceptionHelper.IsStampsCommunicationEx(ex))
                {
                    return new CheckResult<AddressDTO>()
                    {
                        Status = (int)AddressValidationStatus.ExceptionCommunication,
                        Message = ex.Message,
                        AdditionalData = new List<string>() { OrderNotifyType.AddressCheckStamps.ToString() }
                    };
                }
                return new CheckResult<AddressDTO>()
                {
                    Status = (int)AddressValidationStatus.Exception,
                    Message = ex.Message,
                    AdditionalData = new List<string>() { OrderNotifyType.AddressCheckStamps.ToString() }
                };
            }

            catch (Exception ex)
            {
                var message = ex.Message;
                if (message.Contains("First and Last Name, with two chars each or Full Name with two char First and Last Name or Company Name with two chars is required for both sender and recipient"))
                {
                    return new CheckResult<AddressDTO>()
                    {
                        Status = (int) AddressValidationStatus.InvalidRecipientName,
                        Message = ex.Message,
                        AdditionalData = new List<string>() { OrderNotifyType.AddressCheckStamps.ToString() }
                    };
                }
                return new CheckResult<AddressDTO>()
                {
                    Status = (int) AddressValidationStatus.Exception,
                    Message = ex.Message,
                    AdditionalData = new List<string>() { OrderNotifyType.AddressCheckStamps.ToString() }
                };
            }
        }
    }
}
