using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Web.ViewModels.Graph
{
    public class ShippingByCarrierGraphViewModel
    {
        public IList<IList<int>> UnitSeries { get; set; }
        public IList<LabelInfo> Labels { get; set; }

        public class LabelInfo
        {
            public string Name { get; set; }
        }

        public enum PeriodType
        {
            Day,
            Week,
            Month,
            Year            
        }

        private class CarrierStatisticInfo
        {
            public string MethodName { get; set; }
            public int Quantity { get; set; }
        }


        public static ShippingByCarrierGraphViewModel Build(IUnitOfWork db,
            PeriodType period,
            string selectedCarrier)
        {
            return GetByCarrier(db, period, selectedCarrier);
        }

        private static ShippingByCarrierGraphViewModel GetByCarrier(IUnitOfWork db, PeriodType period, string selectedCarrier)
        {
            var result = new ShippingByCarrierGraphViewModel();

            var items = new List<CarrierStatisticInfo>();

            var fromDate = DateHelper.GetAppNowTime().Date.AddDays(-1);
            var toDate = DateHelper.SetEndOfDay(fromDate).Value;
            
            if (period == PeriodType.Week)
            {
                fromDate = fromDate.AddDays(-7);
            }
            if (period == PeriodType.Month)
            {
                fromDate = fromDate.AddMonths(-1);
            }
            if (period == PeriodType.Year)
            {
                fromDate = fromDate.AddYears(-1);
            }

            if (!String.IsNullOrEmpty(selectedCarrier))
            {
                items = (from o in db.Orders.GetAll()
                         join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                         join m in db.ShippingMethods.GetAll() on sh.ShippingMethodId equals m.Id
                         where o.OrderStatus != OrderStatusEnumEx.Canceled
                           && sh.IsActive
                           && o.OrderDate >= fromDate
                           && o.OrderDate <= toDate
                           && m.CarrierName == selectedCarrier
                         group new { o, sh, m } by m.Name into byCarrier
                         select new CarrierStatisticInfo
                         {
                             MethodName = byCarrier.Key,
                             Quantity = byCarrier.Count()
                         }).ToList();
            }
            else
            {
                items = (from o in db.Orders.GetAll()
                                   join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                   join m in db.ShippingMethods.GetAll() on sh.ShippingMethodId equals m.Id
                                   where o.OrderStatus != OrderStatusEnumEx.Canceled
                                     && sh.IsActive
                                     && o.OrderDate >= fromDate
                                     && o.OrderDate <= toDate
                                   group new { o, sh, m } by m.CarrierName into byCarrier
                                   select new CarrierStatisticInfo
                                   {
                                       MethodName = byCarrier.Key,
                                       Quantity = byCarrier.Count()
                                   }).ToList();
            }

            items.ForEach(i => i.MethodName = PrepareMethodName(i.MethodName));
            items = items
                .GroupBy(i => i.MethodName)
                .Select(i => new CarrierStatisticInfo()
                {
                    MethodName = i.Key,
                    Quantity = i.Sum(j => j.Quantity)
                })
                .ToList();

            var labels = new List<LabelInfo>();
            IList<int> unitSeries = new List<int>();
            IList<int> styleCountSeries = new List<int>();

            foreach (var item in items)
            {
                labels.Add(new LabelInfo()
                {
                    Name = item.MethodName,
                });
                unitSeries.Add(item.Quantity);
            }

            result.UnitSeries = new[] { unitSeries };
            result.Labels = labels;
            return result;
        }

        private static string PrepareMethodName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return "n/a";
            return name.Replace("FedEx", "").Replace("Fedex", "").Trim();
        }
    }
}