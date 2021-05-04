using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class MailLabelInfoRepository : Repository<MailLabelInfo>, IMailLabelInfoRepository
    {
        public MailLabelInfoRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void StoreInfo(MailLabelDTO mailInfo,
            IList<DTOOrderItem> mailItems,
            DateTime when,
            long? by)
        {
            var orderId = mailItems.Where(i => i.Quantity > 0).Select(i => i.OrderId).FirstOrDefault();
            if (!orderId.HasValue)
                orderId = string.IsNullOrEmpty(mailInfo.OrderId)
                    ? null
                    : (long?) unitOfWork.Orders.GetFiltered(o => o.AmazonIdentifier == mailInfo.OrderId
                        || o.CustomerOrderId == mailInfo.OrderId).FirstOrDefault()?.Id;
            var sourceAddress = mailInfo.IsAddressSwitched
                ? mailInfo.FromAddress
                : mailInfo.ToAddress;

            var dbMailLabelInfo = new MailLabelInfo
            {
                Marketplace = mailInfo.MarketplaceCode,
                OrderId = orderId,
                TrackingNumber = mailInfo.TrackingNumber,
                IntegratorTxIdentifier = mailInfo.IntegratorTxIdentifier,

                StampsTxId = mailInfo.StampsTxId,
                StampsShippingCost = mailInfo.StampsShippingCost,
                ShipmentProviderType = mailInfo.ShipmentProviderType,

                ShippingMethodId = mailInfo.ShippingMethod.Id,
                ReasonId = mailInfo.Reason,

                Notes = mailInfo.Notes,
                Instructions = mailInfo.Instructions,

                WeightLb = mailInfo.WeightLb ?? 0,
                WeightOz = mailInfo.WeightOz ?? 0,

                IsAddressSwitched = mailInfo.IsAddressSwitched,

                Name = sourceAddress.FullName,
                Address1 = sourceAddress.Address1,
                Address2 = sourceAddress.Address2,
                City = sourceAddress.City,
                State = sourceAddress.State,
                Country = sourceAddress.Country,
                Zip = sourceAddress.Zip,
                ZipAddon = sourceAddress.ZipAddon,
                Phone = sourceAddress.Phone,

                IsInsured = mailInfo.IsInsured,
                InsuranceCost = mailInfo.InsuranceCost,
                InsuredValue = mailInfo.TotalPrice,

                IsSignConfirmation = mailInfo.IsSignConfirmation,
                SignConfirmationCost = mailInfo.SignConfirmationCost,
                UpChargeCost = mailInfo.UpChargeCost,

                ShippingDate = mailInfo.ShippingDate,
                EstimatedDeliveryDate = mailInfo.EstimatedDeliveryDate,

                BuyDate = when,
                LabelPath = mailInfo.LabelPath,
                LabelPrintPackId = mailInfo.LabelPrintPackId,

                IsUpdateRequired = mailInfo.IsUpdateRequired,
                IsCancelCurrentOrderRequested = mailInfo.IsCancelCurrentOrderLabel,

                CreateDate = when,
                CreatedBy = by
            };

            Add(dbMailLabelInfo);
            unitOfWork.Commit();

            foreach (var item in mailItems)
            {
                unitOfWork.MailLabelItems.Add(new MailLabelItem()
                {
                    MailLabelInfoId = dbMailLabelInfo.Id,
                    OrderId = orderId ?? 0,
                    ItemOrderIdentifier = item.ItemOrderId,

                    Quantity = item.Quantity,
                    StyleId = item.StyleEntityId ?? 0,
                    StyleItemId = item.StyleItemId ?? 0,
                    
                    CreateDate = when,
                    CreatedBy = by,
                });
            }
            unitOfWork.Commit();
        }

        public IQueryable<OrderShippingInfoDTO> GetAllAsDto()
        {
            var query = from s in GetAll()
                        join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id
                        join o in unitOfWork.GetSet<Order>() on s.OrderId equals o.Id
                        join u in unitOfWork.GetSet<User>() on s.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        select new OrderShippingInfoDTO
                        {
                            Id = s.Id,
                            LabelFromType = (int)LabelFromType.Mail,

                            LabelPath = s.LabelPath,
                            LabelPurchaseDate = s.BuyDate,
                            LabelPurchaseBy = s.CreatedBy,
                            LabelPurchaseByName = u.Name,
                            PrintLabelPackId = s.LabelPrintPackId,
                            MailReasonId = s.ReasonId,

                            ShippingDate = s.ShippingDate,
                            DeliveredStatus = s.DeliveredStatus,
                            ActualDeliveryDate = s.ActualDeliveryDate,
                            EstimatedDeliveryDate = s.EstimatedDeliveryDate,

                            TrackingNumber = s.TrackingNumber,
                            TrackingStateEvent = s.TrackingStateEvent,
                            TrackingStateDate = s.TrackingStateDate,
                            TrackingStateSource = s.TrackingStateSource,
                            LastTrackingRequestDate = s.LastTrackingRequestDate,

                            StampsShippingCost = s.StampsShippingCost,
                            InsuranceCost = s.InsuranceCost,
                            SignConfirmationCost = s.SignConfirmationCost,
                            UpChargeCost = s.UpChargeCost,

                            ShipmentProviderType = s.ShipmentProviderType,

                            CancelLabelRequested = s.CancelLabelRequested,
                            LabelCanceled = s.LabelCanceled,
                            

                            IsInsured = s.IsInsured,
                            IsSignConfirmation = s.IsSignConfirmation,
                            
                            IsActive = true,
                            IsVisible = true,

                            StampsTxId = s.StampsTxId,
                            ScanFormId = s.ScanFormId,

                            OrderAmazonId = o.AmazonIdentifier,
                            OrderId = s.OrderId ?? 0,

                            ToAddress = new AddressDTO()
                            {
                                Address1 = s.Address1,
                                Address2 = s.Address2,
                                City = s.City,
                                State = s.State,
                                Zip = s.Zip,
                                ZipAddon = s.ZipAddon,
                                Country = s.Country,

                                Phone = s.Phone,
                                FullName = s.Name,
                            },

                            ShippingMethod = new ShippingMethodDTO()
                            {
                                Id = sm.Id,
                                ShipmentProviderType = sm.ShipmentProviderType,
                                Name = sm.Name,
                                ShortName = sm.ShortName ?? sm.Name,
                                RequiredPackageSize = sm.RequiredPackageSize,
                                CarrierName = sm.CarrierName,
                                ServiceIdentifier = sm.ServiceIdentifier,
                                StampsServiceEnumCode = sm.StampsServiceEnumCode,
                                StampsPackageEnumCode = sm.StampsPackageEnumCode,
                                AllowOverweight = sm.AllowOverweight,
                                IsInternational = sm.IsInternational,

                                CroppedLayout = sm.CroppedLayout,
                                IsCroppedLabel = sm.IsCroppedLabel,
                                IsFullPagePrint = sm.IsFullPagePrint
                            },
                        };

            return query;
        }

        public IEnumerable<OrderShippingInfoDTO> GetByOrderIdsAsDto(IList<long> orderIds)
        {
            return GetAllAsDto().Where(o => orderIds.Contains(o.OrderId));
        }


        public IEnumerable<ShippingDTO> GetInfosToFulfillAsDTO(
            MarketType market,
            string marketplaceId)
        {
            var query = from sh in GetAll()
                join o in unitOfWork.Orders.GetAll() on sh.OrderId equals o.Id
                join method in unitOfWork.ShippingMethods.GetAll() on sh.ShippingMethodId equals method.Id
                where !string.IsNullOrEmpty(sh.TrackingNumber) 
                    && sh.IsUpdateRequired
                    && !sh.LabelCanceled
                    && sh.IsFulfilled == false
                    && sh.ShippingMethodId != ShippingUtils.DynamexPTPSameShippingMethodId //NOTE: Exclude Dynamex, Amazon return error when submit it!
                    && sh.ShippingMethodId != ShippingUtils.AmazonDhlExpressMXShippingMethodId //NOTE: Exclude DHLMX, Amazon return error when submit it!
                select new ShippingDTO
                {
                    Id = sh.Id,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    IsFromMailPage = true,
                    AmazonIdentifier = o.AmazonIdentifier,
                    MarketOrderId = o.MarketOrderId,
                    OrderId = o.Id,
                    TrackingNumber = sh.TrackingNumber,
                    ShippingMethodId = sh.ShippingMethodId,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    ShippingDate = sh.BuyDate ?? DateTime.Now,
                    ShippingService = string.Empty,

                    ShippingMethod = new ShippingMethodDTO()
                    {
                        Id = method.Id,
                        Name = method.Name,
                        ShortName = method.ShortName ?? method.Name,
                        RequiredPackageSize = method.RequiredPackageSize,
                        CarrierName = method.CarrierName,
                        IsInternational = method.IsInternational,
                        ShipmentProviderType = method.ShipmentProviderType,
                        ServiceIdentifier = method.ServiceIdentifier,
                    }
                };

            if (market != MarketType.None)
                query = query.Where(o => o.Market == (int)market);
            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(o => o.MarketplaceId == marketplaceId);

            return query;
        }
    }
}
