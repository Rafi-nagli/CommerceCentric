using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Feeds;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.InventoryGroup;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryGroupController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryGroupController."; }
        }

        private const string PopupContentView = "InventoryGroupViewModel";

        public virtual ActionResult Index()
        {
            LogI("Index");

            var model = InventoryGroupFilterViewModel.Default;
            return View("Index", model);
        }


        public virtual ActionResult AddGroup(IList<long> styleIds)
        {
            LogI("AddGroup: " + string.Join(",", styleIds));

            var model = InventoryGroupViewModel.BuildFrom(Db, styleIds);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);

            //return View("EditEmpty", model);
        }

        public virtual ActionResult DeleteGroup(int id)
        {
            LogI("DeleteGroup, id=" + id);

            InventoryGroupViewModel.Remove(Db,
                LogService,
                id);


            return Json(MessageResult.Success(), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult EditGroup(long id)
        {
            LogI("EditGorup, groupId=" + id);

            var model = InventoryGroupViewModel.GetWithItems(Db, id);

            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }



        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            LogI("GetAll, "
                + ", dateFrom=" + dateFrom
                + ", dateTo=" + dateTo);

            var searchFilter = new InventoryGroupFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
            };
            var items = InventoryGroupViewModel.GetAll(Db, Time, searchFilter);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }



        public virtual ActionResult GetChildren([DataSourceRequest]DataSourceRequest request, int Id)
        {
            LogI("GetChildren, id=" + Id);

            var items = InventoryGroupViewModel.GetChildItems(Db, Id).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public virtual ActionResult DeleteChild([DataSourceRequest]DataSourceRequest request, InventoryGroupItemViewModel item)
        {
            LogI("DeleteChild, item=" + item);

            var styleToGroup = Db.StyleToGroups.Get(item.Id);
            Db.StyleToGroups.Remove(styleToGroup);
            Db.Commit();
            var items = new[] { item };
            return Json(items, JsonRequestBehavior.AllowGet); //.ToDataSourceResult(request, ModelState)
        }

        public virtual ActionResult UpdatePrice(InventoryGroupPriceViewModel model)
        {
            LogI("UpdatePrice");

            CallMessagesResultVoid result = new CallMessagesResultVoid();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = InventoryGroupPriceViewModel.Apply(Db, ActionService, model, AccessManager.UserId);
            }
            else
            {
                result = new CallMessagesResultVoid()
                {
                    Messages = messages,
                    Status = CallStatus.Fail
                };
            }

            return Json(result);
        }

        public virtual ActionResult AddFeed()
        {
            LogI("AddFeed");

            var model = new InventoryGroupViewModel();
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        public virtual ActionResult Submit(InventoryGroupViewModel model)
        {
            LogI("Submit");
            CallMessagesResultVoid result = new CallMessagesResultVoid();

            var messages = model.Validate();
            if (!messages.Any())
            {
                result = InventoryGroupViewModel.Apply(Db,
                    ActionService,
                    model,
                    Time.GetAmazonNowTime(),
                    AccessManager.UserId);
            }
            else
            {
                result = new CallMessagesResultVoid()
                {
                    Messages = messages,
                    Status = CallStatus.Fail
                };
            }

            return Json(new UpdateRowViewModel(model, "InventoryGroupList", null, false));
        }
    }
}
