using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Api.AmazonECommerceService;
using Amazon.Core.Entities.Features;
using Amazon.DAL;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Graph;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllRoles)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class DashboardController : BaseController
    {
        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }

        public virtual ActionResult GetSystemStatus()
        {
            var model = new SyncStatusViewModel(Settings, DbFactory);

            return Json(ValueResult<SyncStatusViewModel>.Success("", model), 
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSystemMessages()
        {
            var messages = SystemMessageViewModel.GetAll(DbFactory);

            return Json(ValueResult<IList<SystemMessageViewModel>>.Success("", messages),
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSalesByPeriod(int periodType, int valueType)
        {
            var model = SalesByDateGraphViewModel.Build(Db,
                (SalesByDateGraphViewModel.PeriodType)periodType,
                (SalesByDateGraphViewModel.ValueType)valueType);

            return new JsonResult
            {
                Data = ValueResult<SalesByDateGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetSalesByMarketplace(int periodType, int valueType)
        {
            var model = SalesByMarketplaceGraphViewModel.Build(Db,
                (SalesByMarketplaceGraphViewModel.PeriodType)periodType,
                (SalesByMarketplaceGraphViewModel.ValueType)valueType);

            return new JsonResult
            {
                Data = ValueResult<SalesByDateGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetSalesByProductType(int periodType, int valueType)
        {
            var model = SalesByProductTypeGraphViewModel.Build(Db,
                (SalesByProductTypeGraphViewModel.PeriodType)periodType,
                (SalesByProductTypeGraphViewModel.ValueType)valueType);

            return new JsonResult
            {
                Data = ValueResult<SalesByDateGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetShippingByCarrier(int periodType, string selectedCarrier)
        {
            var model = ShippingByCarrierGraphViewModel.Build(Db,
                (ShippingByCarrierGraphViewModel.PeriodType)periodType,
                selectedCarrier);

            return new JsonResult
            {
                Data = ValueResult<ShippingByCarrierGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetInventoryByFeature(int featureId, int valueType, int? selectedFeatureId)
        {
            var model = ItemsByFeatureTypeGraphViewModel.Build(Db,
                (ItemsByFeatureTypeGraphViewModel.ValueType)valueType,
                featureId,
                selectedFeatureId);

            return new JsonResult
            {
                Data = ValueResult<ItemsByFeatureTypeGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetListingErrorsByPeriod(int periodType)
        {
            var model = ListingErrorsByDateGraphViewModel.Build(Db,
                (ListingErrorsByDateGraphViewModel.PeriodType)periodType);

            return new JsonResult
            {
                Data = ValueResult<ListingErrorsByDateGraphViewModel>.Success("", model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}