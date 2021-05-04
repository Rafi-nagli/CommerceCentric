using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Amazon.Core.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class NotDeliveredController : BaseController
    {
        public override string TAG
        {
            get { return "FeedbackController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View(NotDeliveredFilterViewModel.Empty);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo,
            string buyerName,
            string orderNumber,
            string status)
        {
            LogI("GetAll");

            var filter = new NotDeliveredFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                BuyerName = buyerName,
                OrderNumber = orderNumber,
                Status = status
            };

            request.Sorts = new List<SortDescriptor>()
            {
                new SortDescriptor("OrderDate", ListSortDirection.Descending)
            };
            var items = NotDeliveredOrderViewModel.GetAll(Db, filter);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult SetDismiss(long shippingId, int shippingType)
        {
            LogI("SetDismiss, shippingId=" + shippingId);

            NotDeliveredOrderViewModel.Dismiss(Db, shippingId, (ShippingInfoTypes)shippingType);
            var result = MessageResult.Success();

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult SetSubmitClaim(long shippingId, int shippingType)
        {
            LogI("SetSubmitClaim, shippingId=" + shippingId);

            NotDeliveredOrderViewModel.SubmitClaim(Db, shippingId, (ShippingInfoTypes)shippingType);
            var result = MessageResult.Success();

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult SetHighlight(long shippingId, int shippingType, bool isHighlight)
        {
            LogI("SetHighlight, shippingId=" + shippingId + ", isHighlight=" + isHighlight);

            NotDeliveredOrderViewModel.Highlight(Db, shippingId, (ShippingInfoTypes)shippingType, isHighlight);
            var result = MessageResult.Success();

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
