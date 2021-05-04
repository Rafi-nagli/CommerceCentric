using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models.Calls;
using Amazon.DTO.ScanOrders;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Results;

namespace Amazon.Web.ViewModels.ScanOrders
{
    public class ScanOrderItemViewModel
    {
        public long? Id { get; set; }
        public long? ScanOrderId { get; set; }

        public string Barcode { get; set; }
        public int Quantity { get; set; }

        public long? StyleId { get;set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }
        public string Size { get; set; }


        public string StyleName { get; set; }
        public string SubLicense { get; set; }

        public string InputStyleString { get;set; }
        public long? InputStyleItemId { get; set; }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public ScanOrderItemViewModel()
        {

        }

        public ScanOrderItemViewModel(ScanItemDTO item)
        {
            Id = item.Id;
            ScanOrderId = item.OrderId;
            Barcode = item.Barcode;

            StyleId = item.StyleId;
            StyleString = item.StyleString;
            StyleItemId = item.StyleItemId;

            StyleName = item.StyleName;
            SubLicense = item.SubLicense;

            Size = item.Size;
            
            Quantity = item.Quantity;
        }

        public static ScanOrderItemViewModel GetById(IUnitOfWork db, long scanItemId, long scanOrderId)
        {
            var item = db.Scanned.GetScanItemAsDto().FirstOrDefault(i => i.Id == scanItemId
                && i.OrderId == scanOrderId);
            return new ScanOrderItemViewModel(item);
        }

        public static IList<ScanOrderItemViewModel> GetAll(IUnitOfWork db,
            long scanOrderId)
        {
            var items = db.Scanned.GetScanItemAsDto()
                .Where(i => i.OrderId == scanOrderId)
                .OrderBy(i => i.Barcode)
                .ToList()
                .Select(i => new ScanOrderItemViewModel(i))
                .ToList();
            
            return items;
        }

        public IList<MessageString> Validate(IUnitOfWork db)
        {
            return new List<MessageString>();
        }

        public long Save(IUnitOfWork db, DateTime when, long? by)
        {
            if (!InputStyleItemId.HasValue)
                return 0;

            var barcode = new StyleItemBarcode()
            {
                Barcode = Barcode,
                StyleItemId = InputStyleItemId.Value,
                CreateDate = when,
                CreatedBy = by
            };

            db.StyleItemBarcodes.Add(barcode);

            db.Commit();
            
            var styleItem = db.StyleItems.GetAllAsDto().FirstOrDefault(si => si.StyleItemId == InputStyleItemId);
            if (styleItem != null)
            {
                StyleId = styleItem.StyleId;
                StyleString = InputStyleString;
                StyleItemId = styleItem.StyleItemId;
                Size = styleItem.Size;
            }

            return barcode.Id;
        }
    }
}