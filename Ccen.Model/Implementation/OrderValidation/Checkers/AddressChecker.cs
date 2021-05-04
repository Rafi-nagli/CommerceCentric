using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class AddressChecker
    {
        private ILogService _log;
        private IAddressService _addressService;
        private IOrderHistoryService _orderHistoryService;
        private ITime _time;
        private IDbFactory _dbFactory;

        public AddressChecker(
            ILogService log, 
            IDbFactory dbFactory,
            IAddressService addressService,
            IOrderHistoryService orderHistoryService,
            ITime time)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _addressService = addressService;
            _orderHistoryService = orderHistoryService;
        }

        public void RecheckAddressesWithException()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIdsToRecheck = (from o in db.Orders.GetAll()
                                         where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                            && (o.AddressValidationStatus == (int)AddressValidationStatus.ExceptionCommunication
                                                || o.AddressValidationStatus == (int)AddressValidationStatus.Exception)
                                         select o.Id).ToList();

                _log.Info("OrderIds to recheck: " + orderIdsToRecheck.Count);
                foreach (var orderId in orderIdsToRecheck)
                {
                    UpdateOrderAddressValidationStatus(db, orderId, null);
                }
            }
        }


        public void UpdateOrderAddressValidationStatus(IUnitOfWork db,            
            long orderId,
            long? by)
        {
            var dbOrder = db.Orders.GetById(orderId);

            AddressDTO correctedAddress = null;
            var sourceAddress = db.Orders.GetAddressInfo(orderId);// dbOrder.GetAddressDto();
            var checkResults = CheckAddress(CallSource.Service,
                    db,
                    sourceAddress,
                    orderId,
                    out correctedAddress);
            
            var checkStatus = (int)Core.Models.AddressValidationStatus.None;
            var stampsStatus = checkResults.FirstOrDefault(r => r.AdditionalData != null
                                                                && r.AdditionalData.Any()
                                                                && r.AdditionalData[0] == OrderNotifyType.AddressCheckStamps.ToString());

            if (stampsStatus != null) //Get main checker status (by default: stamps.com)
                checkStatus = stampsStatus.Status;

            if (dbOrder.AddressValidationStatus != checkStatus) //NOTE: If changed then reset dismiss status
            {
                _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.DismissAddressWarnKey, dbOrder.IsDismissAddressValidation, false, by);

                dbOrder.AddressValidationStatus = checkStatus;
                dbOrder.IsDismissAddressValidation = false;
                dbOrder.DismissAddressValidationBy = null;
                dbOrder.DismissAddressValidationDate = null;
            }
            db.Commit();
        }


        public IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            IUnitOfWork db,
            AddressDTO address,
            long? orderId,
            out AddressDTO addressWithCorrection)
        {
            if (orderId.HasValue && orderId.Value == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            addressWithCorrection = null;

            var checkResults = _addressService.CheckAddress(callSource,
                address);

            OrderNotify dbStampsOrderNotify = null;

            foreach (var subResult in checkResults)
            {
                //Stamps
                if (subResult.AdditionalData?[0] == OrderNotifyType.AddressCheckStamps.ToString())
                {
                    if (orderId.HasValue)
                    {
                        dbStampsOrderNotify = new OrderNotify()
                        {
                            OrderId = orderId.Value,
                            Type = (int)OrderNotifyType.AddressCheckStamps,
                            Message = StringHelper.Substring(subResult.Message, 512),
                            Status = (int)subResult.Status,
                            CreateDate = _time.GetAppNowTime()
                        };

                        db.OrderNotifies.Add(dbStampsOrderNotify);
                    }
                }

                //Previous correction address
                if (subResult.AdditionalData?[0] == OrderNotifyType.AddressCheckWithPerviousCorrection.ToString())
                {
                    addressWithCorrection = subResult.Data;

                    if (orderId.HasValue)
                    {
                        db.OrderNotifies.Add(new OrderNotify()
                        {
                            OrderId = orderId.Value,
                            Type = (int)OrderNotifyType.AddressCheckWithPerviousCorrection,
                            Message = StringHelper.Substring(subResult.Message, 512),
                            Status = subResult.Status,
                            CreateDate = _time.GetUtcTime()
                        });
                    }

                    if (subResult.Status < (int)AddressValidationStatus.Invalid
                        && subResult.Status != (int)AddressValidationStatus.None)
                    {
                        if (addressWithCorrection != null
                            && String.IsNullOrEmpty(addressWithCorrection.Address1)
                            && String.IsNullOrEmpty(addressWithCorrection.Address2)
                            && String.IsNullOrEmpty(addressWithCorrection.City)
                            && String.IsNullOrEmpty(addressWithCorrection.State)
                            && String.IsNullOrEmpty(addressWithCorrection.Country)
                            && String.IsNullOrEmpty(addressWithCorrection.Zip)
                            && String.IsNullOrEmpty(addressWithCorrection.ZipAddon))
                            addressWithCorrection = null;

                        if (addressWithCorrection != null)
                        {
                            /*
                             * Похоже идея автоматически менять адрес на предыдущий не совсем хорошая, есть клиент например который хотел послать посылку на новый адрес а мы послали на старый 110-4229580-1843404
                            Давай теперь вместо автоматического исправления адреса просто писать коммент, «previous delivery to %Old_Address% was successful”
                            */

                            if (orderId.HasValue)
                            {
                                var addressString = AddressHelper.ToString(addressWithCorrection, ", ");

                                db.OrderComments.Add(new OrderComment()
                                {
                                    OrderId = orderId.Value,
                                    Message = String.Format("[System] Previous delivery to \"{0}\" was successful", addressString),
                                    Type = (int)CommentType.Address,
                                    CreateDate = _time.GetUtcTime()
                                });
                            }

                            //NOTE: Previous correction address uses only to create order comment
                            addressWithCorrection = null;
                        }
                    }
                }

                if (subResult.AdditionalData?[0] == OrderNotifyType.AddressCheckMelissa.ToString())
                {
                    //Nothing
                }

                if (subResult.AdditionalData?[0] == OrderNotifyType.AddressCheckGoogleGeocode.ToString())
                {
                    if (orderId.HasValue)
                    {
                        db.OrderNotifies.Add(new OrderNotify()
                        {
                            OrderId = orderId.Value,
                            Type = (int)OrderNotifyType.AddressCheckGoogleGeocode,
                            Message = StringHelper.Substring(subResult.Message, 512),
                            Status = subResult.Status,
                            CreateDate = _time.GetUtcTime()
                        });
                    }
                }
                if (subResult.AdditionalData?[0] == OrderNotifyType.AddressCheckFedex.ToString())
                {
                    if (orderId.HasValue)
                    {
                        db.OrderNotifies.Add(new OrderNotify()
                        {
                            OrderId = orderId.Value,
                            Type = (int)OrderNotifyType.AddressCheckFedex,
                            Message = StringHelper.Substring(subResult.Message, 512),
                            Status = subResult.Status,
                            CreateDate = _time.GetUtcTime()
                        });
                    }

                    var stampsResult = checkResults.FirstOrDefault(r => r.AdditionalData[0] == OrderNotifyType.AddressCheckStamps.ToString());
                    if (stampsResult != null && stampsResult.Status >= (int)AddressValidationStatus.Invalid)
                    {
                        if (subResult.Status < (int)AddressValidationStatus.Invalid)
                        {
                            if (addressWithCorrection == null
                                && subResult.Data != null)
                            {
                                var correctionCandidate = subResult.Data;
                                correctionCandidate.FullName = address.FullName;
                                //TASK: If stamps verify it replace it, and don’t show errors.
                                var addressCandidateResults = _addressService.CheckAddress(callSource,
                                    correctionCandidate,
                                    new Core.Models.Settings.AddressProviderType[] { Core.Models.Settings.AddressProviderType.Stamps });

                                var stampsCandidateResult = addressCandidateResults.FirstOrDefault(r => r.AdditionalData[0] == OrderNotifyType.AddressCheckStamps.ToString());
                                if (stampsCandidateResult.Status < (int)AddressValidationStatus.Invalid)
                                {
                                    _log.Info("Replacing address to Fedex Effective address");

                                    stampsResult.Status = stampsCandidateResult.Status;
                                    if (dbStampsOrderNotify != null)
                                        dbStampsOrderNotify.Status = stampsCandidateResult.Status;

                                    if (orderId.HasValue)
                                    {
                                        db.OrderComments.Add(new OrderComment()
                                        {
                                            OrderId = orderId.Value,
                                            Message = String.Format("[System] Address was replaced by Fedex \"Effective address\""),
                                            Type = (int)CommentType.Address,
                                            CreateDate = _time.GetUtcTime()
                                        });
                                    }

                                    addressWithCorrection = subResult.Data;
                                }                                
                            }
                        }
                    }
                }
            }

            return checkResults;
        }
    }
}
