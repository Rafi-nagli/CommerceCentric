using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Web.ViewModels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.PurchaseOrders
{
    public class PurchaseOrderViewModel
    {
        public long Id { get; set; }
        public string StyleString { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public int BoxQuantity { get; set; }

        public IList<StyleItemViewModel> Sizes { get; set; }

        public PurchaseOrderViewModel()
        {

        }

        public PurchaseOrderViewModel(OpenBoxViewModel box)
        {
            Id = box.Id;
            StyleString = box.StyleString;
            ReceiveDate = box.CreateDateUtc;
            BoxQuantity = box.BoxQuantity;

            Sizes = box.StyleItems.Items;
        }

        public OpenBoxViewModel ToBox(IUnitOfWork db, ITime time)
        {
            var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == StyleString
                && !st.Deleted);

            var baseBoxId = dbStyle.StyleID + "-" + time.GetAppNowTime().ToString("MMddyyyy");
            var index = 0;
            var boxId = baseBoxId;
            while (db.OpenBoxes.GetAll().Count(b => b.BoxBarcode == boxId) > 0)
            {
                index++;
                boxId = baseBoxId + "-" + index;
            }

            return new OpenBoxViewModel()
            {
                Id = Id,
                BoxBarcode = boxId,
                StyleString = StyleString,
                StyleId = dbStyle?.Id ?? 0,
                CreateDateUtc = ReceiveDate,
                BoxQuantity = BoxQuantity,
                Type = (int)BoxTypes.Preorder,

                StyleItems = new StyleItemCollection()
                {
                    Items = Sizes
                }
            };
        }
    }
}