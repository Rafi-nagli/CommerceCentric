using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Entities;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Inventory;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class OrderToTrackController : BaseController
    {
        public virtual ActionResult Index()
        {
            LogI("OrdersToTrack.Index");
            return View();
        }

        public virtual ActionResult GetToTrack([DataSourceRequest] DataSourceRequest request)
        {
            LogI("GetToTrack");

            var items = OrderToTrackViewModel.GetAll(Db);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult AddOrderToTrack()
        {
            LogI("AddOrderToTrack");
            var model = new OrderToTrackViewModel();
            ViewBag.PartialViewName = "OrderToTrackViewModel";
            ViewBag.IsAdd = true;
            return View("EditNew", model);
        }


        //[HttpPost]
        public virtual ActionResult Submit(OrderToTrackViewModel model)
        {
            LogI("Submit, model=" + model);

            model.OrderNumber = StringHelper.TrimWhitespace(model.OrderNumber);
            model.TrackingNumber = StringHelper.TrimWhitespace(model.TrackingNumber);
            model.Carrier = "USPS"; //TODO: Dropdown on UI

            var messages = model.Validate(Db);
            if (!messages.Any())
            {
                model.Submit(Db, Time.GetAppNowTime(), AccessManager.UserId);
                return Json(new UpdateRowViewModel(model, "grid", null, true));
            }
            else
            {
                messages.Each(m => ModelState.AddModelError(m.Key, m.Message));
                return PartialView("OrderToTrackViewModel", model);
            }
        }

        public virtual ActionResult DeleteTracking(long id)
        {
            var tracking = Db.TrackingOrders.Get(id);
            if (tracking != null)
            {
                tracking.Deleted = true;
                Db.Commit();
            }
            return View("Index");
        }

        public virtual ActionResult UpdateComment([DataSourceRequest] DataSourceRequest request, OrderToTrackViewModel model)
        {
            var tracking = Db.TrackingOrders.Get(model.TrackingId);
            if (tracking != null)
            {
                tracking.Comment = model.Comment;
                Db.Commit();
            }
            return View("Index");
        }

    }
}
