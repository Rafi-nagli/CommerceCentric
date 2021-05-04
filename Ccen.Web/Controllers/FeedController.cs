using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Feeds;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.ScanOrders;
using Amazon.Web.ViewModels.Vendors;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class FeedController : BaseController
    {
        public override string TAG
        {
            get { return "FeedController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            var model = FeedFilterViewModel.Default;
            return View("Index", model);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            MarketType market,
            string marketplaceId,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? type)
        {
            LogI("GetAll, market=" + market 
                + ", marketplaceId=" + marketplaceId
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo);

            var searchFilter = new FeedFilterViewModel()
            {
                Market = market,
                MarketplaceId = marketplaceId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Type = type,
            };
            var items = FeedViewModel.GetAll(Db, Time, searchFilter);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetAllMessage([DataSourceRequest]DataSourceRequest request,
            long feedId)
        {
            LogI("GetAllMessage, feedId=" + feedId);
            
            var items = FeedMessageViewModel.GetAll(Db, feedId);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetFeed(long feedId)
        {
            var file = FeedViewModel.GetFilepath(Db, feedId);
            var filename = Path.GetFileName(file);
            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

    }
}
