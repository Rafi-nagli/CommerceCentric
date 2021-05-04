using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class FeedbackBlackListController : BaseController
    {
        public override string TAG
        {
            get { return "FeedbackBlackListController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAll(DataSourceRequest request)
        {
            LogI("GetAll");

            var items = FeedbackBlackListViewModel.GetAll(Db).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public virtual ActionResult Add([DataSourceRequest]DataSourceRequest request, FeedbackBlackListViewModel item)
        {
            LogI("AddSizeGroup, item=" + item);
            if (ModelState.IsValid)
            {
                IList<KeyValuePair<string, string>> errors = null;
                if (FeedbackBlackListViewModel.Validate(Db, item, out errors))
                {
                    item = FeedbackBlackListViewModel.Add(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);
                }
                else
                {
                    errors.Each(e => ModelState.AddModelError(e.Key, e.Value));
                }
            }
            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Update([DataSourceRequest]DataSourceRequest request, FeedbackBlackListViewModel item)
        {
            LogI("UpdateSizeGroup, item=" + item);

            if (ModelState.IsValid && item != null)
            {
                IList<KeyValuePair<string, string>> errors = null;
                if (FeedbackBlackListViewModel.Validate(Db, item, out errors))
                {
                    item = FeedbackBlackListViewModel.Update(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);
                }
                else
                {
                    errors.Each(e => ModelState.AddModelError(e.Key, e.Value));
                }
            }

            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Remove([DataSourceRequest]DataSourceRequest request, FeedbackBlackListViewModel item)
        {
            LogI("Remove, item=" + item);

            if (item != null && item.Id.HasValue)
                FeedbackBlackListViewModel.Delete(Db, item.Id.Value);
            return Json((new FeedbackBlackListViewModel[] { }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
    }
}
