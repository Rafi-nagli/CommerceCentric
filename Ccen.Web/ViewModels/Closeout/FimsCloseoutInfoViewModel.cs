using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Settings;
using DropShipper.Api;

namespace Amazon.Web.ViewModels.Statuses
{
    public class FimsCloseoutInfoViewModel
    {
        public string Name { get; set; }
        public bool IsSuccess { get; set; }

        public int ToCloseoutCount { get; set; }
        public decimal ToCloseoutWeight { get; set; }

        public int IbcOrdersCount { get; set; }
        public decimal IbcOrdersWeight { get; set; }

        public static IList<FimsCloseoutInfoViewModel> GetInfo(IUnitOfWork db,
            DropShipperApi mbgApi)
        {
            var toCloseoutShipments = db.OrderShippingInfos.GetAll()
                    .Where(sh => sh.ShipmentProviderType == (int) ShipmentProviderType.FIMS
                                 && !sh.ScanFormId.HasValue
                                 && !sh.CancelLabelRequested
                                 && !sh.LabelCanceled
                                 && !String.IsNullOrEmpty(sh.StampsTxId))
                    .Select(sh => new {
                        Id = sh.Id,
                        Weight = sh.UsedWeight ?? 0
                    })
                    .ToList();
            

            var toCloseoutMailings = db.MailLabelInfos.GetAll()
                    .Where(sh => sh.ShipmentProviderType == (int)ShipmentProviderType.FIMS
                                 && !sh.ScanFormId.HasValue
                                 && !sh.CancelLabelRequested
                                 && !sh.LabelCanceled
                                 && !String.IsNullOrEmpty(sh.StampsTxId))
                    .Select(sh => new {
                        Id = sh.Id,
                        Weight = sh.WeightLb * 16 + sh.WeightOz
                    })
                    .ToList();

            var allFimsShipments = (from sh in db.OrderShippingInfos.GetAll()
                                   join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                                   join oi in db.OrderItems.GetWithListingInfo() on o.Id equals oi.OrderId
                                   join m in db.MailLabelInfos.GetAll() on o.Id equals m.OrderId into withMail
                                   from m in withMail.DefaultIfEmpty()
                                   where sh.ShipmentProviderType == (int)ShipmentProviderType.FIMS
                                        && !sh.ScanFormId.HasValue
                                        && sh.IsActive
                                        && OrderStatusEnumEx.AllUnshippedWithShipped.Contains(o.OrderStatus)
                                        && (m == null || m.LabelCanceled)
                                        && (o.OrderStatus != OrderStatusEnumEx.Canceled
                                            && o.OrderStatus != OrderStatusEnumEx.Pending)
                                        && sh.LabelPath != "#" //NOTE: External label
                                   group new { sh, oi } by sh.Id into byShipment
                                   select new {
                                        Id = byShipment.Key,
                                        Weight = byShipment.Sum(i => i.oi.Weight * i.oi.Quantity)
                                    })
                                    .ToList();
            
            var allFimsMailings = (from sh in db.MailLabelInfos.GetAll()
                                 join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                                 where sh.ShipmentProviderType == (int)ShipmentProviderType.FIMS
                                    && !sh.ScanFormId.HasValue
                                    && OrderStatusEnumEx.AllUnshippedWithShipped.Contains(o.OrderStatus)
                                 select new {
                                     Id = sh.Id,
                                     Weight = sh.WeightLb * 16 + sh.WeightOz
                                 }).ToList();


            var currentFims = new FimsCloseoutInfoViewModel()
            {
                Name = "PA",
                IsSuccess = true,
                ToCloseoutCount = toCloseoutShipments.Count() + toCloseoutMailings.Count(),
                ToCloseoutWeight = toCloseoutShipments.Sum(sh => (decimal)sh.Weight) + toCloseoutMailings.Sum(sh => (decimal)sh.Weight),
                IbcOrdersCount = allFimsShipments.Count() + allFimsMailings.Count(),
                IbcOrdersWeight = allFimsShipments.Sum(sh => (decimal)sh.Weight) + allFimsMailings.Sum(sh => (decimal)sh.Weight),
            };

            var results = new List<FimsCloseoutInfoViewModel>() { currentFims };
            //var mbgIBCInfo = mbgApi.GetIBCOrdersToClose();
            //results.Add(new IbcCloseoutInfoViewModel()
            //{
            //    Name = "MBG",
            //    IsSuccess = mbgIBCInfo.IsSuccess,
            //    IbcOrdersCount = mbgIBCInfo.Data?.OrdersCount ?? 0,
            //    IbcOrdersWeight = mbgIBCInfo.Data?.OrdersWeight ?? 0,
            //    ToCloseoutCount = mbgIBCInfo.Data?.ToCloseoutCount ?? 0,
            //    ToCloseoutWeight = mbgIBCInfo.Data?.ToCloseoutWeight ?? 0,
            //});

            return results;
        }
    }
}