using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.ViewModels.ExcelToAmazon;

namespace Amazon.Web.Models.Exports
{
    public class VendorExport
    {
        public static string VendorOrderTemplatePath = "~/App_Data/VendorOrderTemplate.xls";

        public static MemoryStream ExportToExcelVendorOrder(IUnitOfWork db,
            ITime time,
            long vendorOrderId,
            out string filename)
        {
            var models = new List<ExcelOrderVendorViewModel>();

            var vendorOrder = db.VendorOrders.Get(vendorOrderId);

            var items = db.VendorOrderItems.GetAllAsDto()
                .Where(i => i.VendorOrderId == vendorOrderId)
                .ToList();
            var itemIds = items.Select(i => i.Id).ToList();
            var sizes = db.VendorOrderItemSizes.GetAllAsDto()
                .Where(s => itemIds.Contains(s.VendorOrderItemId))
                .OrderBy(s => s.Order)
                .ToList();

            foreach (var item in items)
            {
                var sizeList = sizes.Where(s => s.VendorOrderItemId == item.Id).ToList();
                
                models.Add(new ExcelOrderVendorViewModel()
                {
                    Style = item.StyleString,
                    Name = item.Name,
                    Sizes = String.Join(", ", sizeList.Select(s => s.Size).ToList()),
                    Breakdown = String.Join("-", sizeList.Select(s => s.Breakdown).ToList()),
                    Price = (double)item.Price,//.ToString("0" + priceSeparator + "00"),
                    Quantity = item.Quantity, //.ToString(),
                    QuantityDate1 = item.QuantityDate1 ?? 0, //.ToString(),
                    
                    TargetSaleDate = item.TargetSaleDate.HasValue ? item.TargetSaleDate.Value.ToString("MM.dd.yyyy") : "",
                    Comment = item.Comment
                });   
            }
            
            filename = vendorOrder.VendorName + "_" + vendorOrder.Id + ".xls";

            return ExcelHelper.ExportIntoFile(
                HttpContext.Current.Server.MapPath(VendorOrderTemplatePath), 
                "Template",
                models,
                customData: null,
                headerRowOffset: 1,
                createRow: false,
                useOriginalTypes: true);
        }
    }
}