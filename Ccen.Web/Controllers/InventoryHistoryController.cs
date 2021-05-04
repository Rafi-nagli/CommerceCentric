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
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryHistoryController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryHistoryController."; }
        }

        public virtual ActionResult ViewInventoryHistory(long styleItemId)
        {
            LogI("InventoryHistoryPopup");

            var model = new StyleSizeHistoryViewModel()
            {
                StyleItemId = styleItemId,
            };

            return View("InventoryHistoryPopupContent", model);
        }

        public virtual ActionResult GetAll(long styleItemId, bool includeSnapshoot)
        {
            LogI("GetInventoryHistory, styleItemId=" + styleItemId + ", includeSnapshoot=" + includeSnapshoot);

            //request.Sorts = new List<SortDescriptor>()
            //{
            //    new SortDescriptor("UpdateDate", ListSortDirection.Descending)
            //};

            var items = StyleSizeInventoryHistoryRecordViewModel.GetRecords(Db, styleItemId, includeSnapshoot);
            //var dataSource = items.ToDataSourceResult(request);
            return JsonGet(ValueResult<IList<StyleSizeInventoryHistoryRecordViewModel>>.Success("", items.Take(50).ToList()));
        }
    }
}
