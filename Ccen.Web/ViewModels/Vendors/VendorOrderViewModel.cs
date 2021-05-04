using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.VendorOrders;
using Amazon.Core.Models.Calls;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.UI;

namespace Amazon.Web.ViewModels.Vendors
{
    public class VendorOrderViewModel
    {
        public long? Id { get; set; }
        public string VendorName { get; set; }

        public int StatusCode { get; set; }

        public string Description { get; set; }

        public int? TotalQuantity
        {
            get
            {
                if (VendorOrderItems != null)
                    return VendorOrderItems.Sum(v => v.Quantity);
                return null;
            }
        }

        public decimal? LineTotal
        {
            get
            {
                if (VendorOrderItems != null)
                    return VendorOrderItems.Sum(v => v.Quantity * v.Price);
                return null;
            }
        }


        public string StatusString
        {
            get
            {
                var status = VendorOrderStatusList.FirstOrDefault(s => s.Value == StatusCode.ToString());
                if (status != null)
                    return status.Text;
                return "-";
            }
        }
        public DateTime? CreateDate { get; set; }

        public IList<VendorOrderItemViewModel> VendorOrderItems { get; set; }
        public IList<VendorOrderAttachmentViewModel> VendorOrderAttachments { get; set; }

        public VendorOrderViewModel()
        {
            
        }


        
        public static IList<VendorOrderViewModel> GetAll(IUnitOfWork db)
        {
            var vendorOrders = db.VendorOrders.GetAllAsDto()
                .Where(v => !v.IsDeleted)
                .OrderByDescending(o => o.CreateDate)
                .Select(o => new VendorOrderViewModel()
                {
                    Id = o.Id,
                    VendorName = o.VendorName,
                    StatusCode = o.Status,
                    Description = o.Description,
                    CreateDate = o.CreateDate
                })
                .ToList();
            var vendorOrderItems = db.VendorOrderItems.GetAllAsDto().ToList();
            var vendorAttachments = db.VendorOrderAttachments.GetAllAsDto().ToList();

            foreach (var order in vendorOrders)
            {
                order.VendorOrderItems = vendorOrderItems
                    .Where(i => i.VendorOrderId == order.Id)
                    .OrderByDescending(i => i.CreateDate)
                    .Select(i => new VendorOrderItemViewModel(i))
                    .ToList();

                order.VendorOrderAttachments = vendorAttachments
                    .Where(a => a.VendorOrderId == order.Id)
                    .OrderByDescending(i => i.CreateDate)
                    .Select(a => new VendorOrderAttachmentViewModel(a))
                    .ToList();
            }

            return vendorOrders;
        }

        public static string GenerateExcel(IUnitOfWork db, 
            ITime time,
            long vendorOrderId)
        {
            string fileName;
            var stream = VendorExport.ExportToExcelVendorOrder(db, 
                time, 
                vendorOrderId, 
                out fileName);
            var filePath = UrlHelper.GetProductTemplateFilePath(fileName);
            using (var file = File.Open(filePath, FileMode.Create))
            {
                stream.WriteTo(file);
            }
            return UrlHelper.GetProductTemplateUrl(fileName);
        }

        public long Save(IUnitOfWork db, DateTime when, long? by)
        {
            VendorOrder order = null; 
            if (Id.HasValue)
            {
                order = db.VendorOrders.Get(Id.Value);
            }
            if (order == null)
            {
                order = new VendorOrder();
                db.VendorOrders.Add(order);

                order.CreateDate = when;
                order.CreatedBy = by;
            }

            order.VendorName = VendorName;
            order.Status = StatusCode;
            order.Description = Description;

            order.UpdateDate = when;
            order.UpdatedBy = by;

            db.Commit();

            return order.Id;
        }

        public IList<MessageString> Validate(IUnitOfWork db)
        {
            return new List<MessageString>();
        }

        public static VendorOrderViewModel GetById(IUnitOfWork db, long id)
        {
            var vendorOrder = db.VendorOrders.Get(id);

            return new VendorOrderViewModel()
            {
                Id = vendorOrder.Id,
                VendorName = vendorOrder.VendorName,
                StatusCode = vendorOrder.Status,
                Description = vendorOrder.Description,
            };
        }

        public static void Delete(IUnitOfWork db, long id)
        {
            var record = db.VendorOrders.Get(id);
            record.IsDeleted = true;
            db.Commit();
        }

        public static VendorOrderViewModel Create(IUnitOfWork db)
        {
            return new VendorOrderViewModel();
        }

        public const int OrderInProgressCode = 1;
        public const int OrderSubmittedCode = 2;
        public const int OrderConfirmedCode = 3;
        public const int OrderPaidCode = 4;
        public const int OrderDeliveredCode = 5;


        public static List<DropDownListItem> VendorOrderStatusList
        {
            get { return new List<DropDownListItem>()
            {
                new DropDownListItem() { Text = "In-progress", Value = OrderInProgressCode.ToString() },
                new DropDownListItem() { Text = "Submitted", Value = OrderSubmittedCode.ToString() },
                new DropDownListItem() { Text = "Confirmed", Value = OrderConfirmedCode.ToString() },
                new DropDownListItem() { Text = "Paid", Value = OrderPaidCode.ToString() },
                new DropDownListItem() { Text = "Delivered", Value = OrderDeliveredCode.ToString() },
            };}
        }
    }
}