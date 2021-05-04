using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;

namespace Amazon.Web.ViewModels.ScanOrders
{
    public class ScanOrderViewModel
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public bool IsFBA { get; set; }

        public long TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }

        public DateTime? OrderDate { get; set; }

        public IList<ScanOrderItemViewModel> ScanOrderItems { get; set; }


        public DateTime? ConvertedOrderDate
        {
            get { return DateHelper.ConvertUtcToApp(OrderDate); }
        }

        public ScanOrderViewModel()
        {
            ScanOrderItems = new List<ScanOrderItemViewModel>();
        }
        
        public static IList<ScanOrderViewModel> GetAll(IUnitOfWork db)
        {
            var quantityByOrder = from i in db.Scanned.GetScanItemAsDto()
                                 group i by i.OrderId into byOrder
                                 select new
                                 {
                                     OrderId = byOrder.Key,
                                     TotalQuantity = byOrder.Sum(i => i.Quantity > 100000 ? 0 : i.Quantity)
                                 };

            var scanOrders = (from o in db.Scanned.GetScanOrdersAsDto()
                             join qty in quantityByOrder on o.Id equals qty.OrderId into withQty
                             from qty in withQty.DefaultIfEmpty()
                             orderby o.OrderDate
                            select new ScanOrderViewModel()
                            {
                                Id = o.Id,
                                Name = o.Description,
                                OrderDate = o.OrderDate,
                                TotalQuantity = (long?)qty.TotalQuantity ?? 0,
                                IsFBA = o.IsFBA
                            }).ToList();


            return scanOrders;
        }
    }
}