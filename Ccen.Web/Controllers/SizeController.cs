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
    [Authorize(Roles = AccessManager.AllAdmins)]
    public partial class SizeController : BaseController
    {
        public override string TAG
        {
            get { return "SizeController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAllGroups(DataSourceRequest request)
        {
            LogI("GetAllGroups");

            var items = SizeGroupViewModel.GetAll(Db).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public virtual ActionResult AddSizeGroup([DataSourceRequest]DataSourceRequest request, SizeGroupViewModel item)
        {
            LogI("AddSizeGroup, item=" + item);
            if (ModelState.IsValid)
            {
                SizeGroupViewModel.Add(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);
            }
            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult UpdateSizeGroup([DataSourceRequest]DataSourceRequest request, SizeGroupViewModel item)
        {
            LogI("UpdateSizeGroup, item=" + item);

            if (ModelState.IsValid && item != null)
                SizeGroupViewModel.Update(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);

            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult RemoveSizeGroup([DataSourceRequest]DataSourceRequest request, SizeGroupViewModel item)
        {
            LogI("RemoveSizeGroup, item=" + item);

            if (item != null && item.Id.HasValue)
                SizeGroupViewModel.Delete(Db, item.Id.Value);
            return Json((new SizeGroupViewModel[] { }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }


        public virtual ActionResult GetSizes([DataSourceRequest]DataSourceRequest request, int groupId)
        {
            LogI("GetSizes, groupId=" + groupId);

            var items = SizeViewModel.GetSizesForGroup(Db, groupId).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult AddSize([DataSourceRequest]DataSourceRequest request, SizeViewModel item)
        {
            LogI("AddSize, item=" + item);

            item = SizeViewModel.Add(Db, item);
            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult UpdateSize([DataSourceRequest]DataSourceRequest request, SizeViewModel item)
        {
            LogI("UpdateChild, item=" + item);

            if (ModelState.IsValid && item != null)
                item = SizeViewModel.Update(Db, item);

            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult RemoveSize([DataSourceRequest]DataSourceRequest request, SizeViewModel item)
        {
            LogI("RemoveSize, item=" + item);

            if (item != null)
                SizeViewModel.Delete(Db, item.Id);
            return Json((new SizeViewModel[] { }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

    }
}
