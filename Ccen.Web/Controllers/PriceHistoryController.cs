using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.SizeHistories;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class PriceHistoryController : BaseController
    {
        public override string TAG
        {
            get { return "PriceHistoryController."; }
        }

        public virtual ActionResult ViewPriceHistory(long styleItemId)
        {
            LogI("PriceHistoryPopup");

            var model = new StyleSizeHistoryViewModel()
            {
                StyleItemId = styleItemId,
            };

            return View("PriceHistoryPopupContent", model);
        }

        public virtual ActionResult GetAll(long styleItemId)
        {
            LogI("GetInventoryHistory, styleItemId=" + styleItemId);

            //request.Sorts = new List<SortDescriptor>()
            //{
            //    new SortDescriptor("UpdateDate", ListSortDirection.Descending)
            //};

            IList<IHistoryRecord> items;

            items = StyleSizeActionHistoryRecordViewModel.GetRecords(Db, styleItemId, new[] {
                "AddPermancentSale",
                "AddSale",
                "RemoveSale" }).ToList();

            items.AddRange(ListingPriceHistoryRecordViewModel.GetRecords(Db, styleItemId).ToList());

            items = items.OrderByDescending(i => i.When).ToList();

            foreach (var item in items)
                item.Prepare();

            //var dataSource = items.ToDataSourceResult(request);
            return JsonGet(ValueResult<IList<IHistoryRecord>>.Success("", items.ToList()));
        }
    }
}
