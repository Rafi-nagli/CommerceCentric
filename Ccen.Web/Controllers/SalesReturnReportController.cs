using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Reports;
using Ccen.Web.ViewModels.Reports;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class SalesReturnReportController : BaseController
    {
        public override string TAG
        {
            get { return "SalesReturnReportController."; }
        }
        private const string OrdersPopupContentView = "ReturnOrdersPopupContent";

        //
        // GET: /Reports/

        public virtual ActionResult Index()
        {
            LogI("Index");

            var model = new SalesExtReportPageViewModel();
            model.Init(Db);
            return View("Index", model);
        }

        public virtual ActionResult GetAll(GridRequest request,
            DateTime? fromDate,
            DateTime? toDate,
            string keywords,
            string gender,
            [Bind(Prefix = "itemStyles[]")]List<int> itemStyles,
            int? mainLicense,
            int? subLicense,
            int? market,
            string marketplaceId)
        {
            var genderIds = (gender ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => Int32.Parse(g))
                    .ToList();

            var pageSize = request.ItemsPerPage;

            var filters = new SalesExtReportFiltersViewModel()
            {
                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
                SortField = request.SortField,
                SortMode = request.SortMode == "asc" ? 0 : 1,

                DateFrom = fromDate,
                DateTo = toDate,

                Keywords = keywords,
                Genders = genderIds,
                ItemStyles = itemStyles,
                MainLicense = mainLicense,
                SubLicense = subLicense,

                Market = market,
                MarketplaceId = marketplaceId,
            };
            var gridResponse = new SalesReturnReportViewModel().GetAll(Db, filters);

            var data = new GridResponse<SalesReturnReportViewModel>(gridResponse.Items, gridResponse.TotalCount);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ReturnedOrders(long styleId)
        {
            //ViewBag.PartialViewName = OrdersPopupContentView;
            var model = new ReturnedOrderStyleViewModel() { StyleId = styleId };
            return View(OrdersPopupContentView, model);            
        }

        public virtual ActionResult GetAllOrders([DataSourceRequest] DataSourceRequest request, long styleId)
        {
            var items = SalesReturnReportViewModel.GetAllOrders(Db, styleId);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }
    }
}
