using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Settings;
using DropShipper.Api;

namespace Amazon.Web.ViewModels.Statuses
{
    public class FedexCloseoutInfoViewModel
    {
        public string Name { get; set; }
        public bool IsSuccess { get; set; }

        public int FedexOrdersCount { get; set; }
        public int FedexLabelsCount { get; set; }
        public decimal FedexOrdersWeight { get; set; }

        public static IList<FedexCloseoutInfoViewModel> GetInfo(IUnitOfWork db,
            ITime time,
            DropShipperApi mbgApi)
        {
            var fromDate = time.GetAppNowTime().Date;

            var allFedexShipments = (from sh in db.OrderShippingInfos.GetAll()
                                     join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                                     join oi in db.OrderItems.GetWithListingInfo() on o.Id equals oi.OrderId
                                     join m in db.MailLabelInfos.GetAll() on o.Id equals m.OrderId into withMail
                                     from m in withMail.DefaultIfEmpty()
                                     where (sh.ShipmentProviderType == (int)ShipmentProviderType.FedexOneRate
                                                  //|| sh.ShipmentProviderType == (int)ShipmentProviderType.FedexSmartPost
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedExPriorityOvernight
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedExStandardOvernight
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedExExpressSaverShippingMethodId
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayShppingMethodId
                                                  || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayAMShppingMethodId

                                                  || (sh.ShipmentProviderType == (int)ShipmentProviderType.FedexGeneral
                                                      && sh.ShippingMethodId != ShippingUtils.FedexGroundShippingMethodId
                                                      && sh.ShippingMethodId != ShippingUtils.FedexHomeDeliveryShippingMethodId))
                                          && sh.IsActive
                                          && (((OrderStatusEnumEx.Shipped == o.OrderStatus
                                              || OrderStatusEnumEx.PartiallyShipped == o.OrderStatus)
                                              && sh.ShippingDate >= fromDate)
                                              || OrderStatusEnumEx.Unshipped == o.OrderStatus)
                                          && (m == null || m.LabelCanceled)
                                     group new { sh, oi } by sh.Id into byShipment
                                     select new {
                                         Id = byShipment.Key,
                                         Weight = byShipment.Sum(i => i.oi.Weight),
                                         HasLabel = byShipment.Any(i => !String.IsNullOrEmpty(i.sh.LabelPath))
                                     })
                                    .ToList();

            var allFedexMailings = (from sh in db.MailLabelInfos.GetAll()
                                    join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                                    where (sh.ShipmentProviderType == (int)ShipmentProviderType.FedexOneRate
                                                   //|| sh.ShipmentProviderType == (int)ShipmentProviderType.FedexSmartPost
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedExPriorityOvernight
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedExStandardOvernight
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedExExpressSaverShippingMethodId
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayShppingMethodId
                                                   || sh.ShippingMethodId == ShippingUtils.AmazonFedEx2DayAMShppingMethodId

                                                   || (sh.ShipmentProviderType == (int)ShipmentProviderType.FedexGeneral
                                                       && sh.ShippingMethodId != ShippingUtils.FedexGroundShippingMethodId
                                                       && sh.ShippingMethodId != ShippingUtils.FedexHomeDeliveryShippingMethodId))
                                       && (((OrderStatusEnumEx.Shipped == o.OrderStatus
                                               || OrderStatusEnumEx.PartiallyShipped == o.OrderStatus)
                                               && sh.ShippingDate >= fromDate)
                                               || OrderStatusEnumEx.Unshipped == o.OrderStatus)
                                    select new {
                                        Id = sh.Id,
                                        Weight = sh.WeightLb * 16 + sh.WeightOz,
                                        HasLabel = !String.IsNullOrEmpty(sh.LabelPath)
                                    }).ToList();

            var currentFedex = new FedexCloseoutInfoViewModel()
            {
                Name = "PA",
                IsSuccess = true,
                FedexOrdersCount = allFedexShipments.Count(sh => !sh.HasLabel) + allFedexMailings.Count(sh => !sh.HasLabel),
                FedexLabelsCount = allFedexShipments.Count(sh => sh.HasLabel) + allFedexMailings.Count(sh => sh.HasLabel),
                //FedexOrdersWeight = allFedexShipments.Sum(sh => (decimal)sh.Weight) + allFedexMailings.Sum(sh => (decimal)sh.Weight),
            };

            var results = new List<FedexCloseoutInfoViewModel>() { currentFedex };

            if (mbgApi != null)
            {
                var mbgFedexInfo = mbgApi.GetFedexOrdersInfo();
                results.Add(new FedexCloseoutInfoViewModel()
                {
                    Name = "MBG",
                    IsSuccess = mbgFedexInfo.IsSuccess,
                    FedexOrdersCount = mbgFedexInfo.Data?.OrdersCount ?? 0,
                    FedexLabelsCount = mbgFedexInfo.Data?.ToCloseoutCount ?? 0,
                    //FedexOrdersWeight = mbgFedexInfo.Data?.OrdersWeight ?? 0,
                });
            }
            return results;
        }
    }
}