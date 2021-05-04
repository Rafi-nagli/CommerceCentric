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
    public partial class SizeMappingController : BaseController
    {
        public override string TAG
        {
            get { return "SizeMappingController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAllSizeMappings([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAllSizeMappings");

            var items = SizeMappingViewModel.GetAll(Db);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public virtual ActionResult AddSizeMapping([DataSourceRequest]DataSourceRequest request, SizeMappingViewModel item)
        {
            var entry = LogService.Info("AddSizeMapping, item=" + item);
            if (ModelState.IsValid)
            {
                var validationResults = SizeMappingViewModel.Validate(Db, item);
                if (validationResults.Any())
                {
                    foreach (var vResult in validationResults)
                    {
                        ModelState.AddModelError("StyleSize", vResult.ErrorMessage);
                        LogService.Info("validation result: " + vResult.ErrorMessage, entry);
                    }
                }
                else
                {
                    SizeMappingViewModel.Add(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);
                    LogService.Info("Success added", entry);
                }
            }
            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult UpdateSizeMapping([DataSourceRequest]DataSourceRequest request, SizeMappingViewModel item)
        {
            var entry = LogService.Info("UpdateSizeMapping, item=" + item);

            if (ModelState.IsValid && item != null)
            {
                var validationResults = SizeMappingViewModel.Validate(Db, item);
                if (validationResults.Any())
                {
                    foreach (var vResult in validationResults)
                    {
                        ModelState.AddModelError("StyleSize", vResult.ErrorMessage);
                        LogService.Info("validation result: " + vResult.ErrorMessage, entry);
                    }
                }
                else
                {
                    SizeMappingViewModel.Update(Db, item, DateHelper.GetAppNowTime(), AccessManager.UserId);
                    LogService.Info("Success updated", entry);
                }
            }

            return Json((new[] { item }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult RemoveSizeMapping([DataSourceRequest]DataSourceRequest request, SizeMappingViewModel item)
        {
            LogI("SizeMappingViewModel, item=" + item);

            if (item != null && item.Id.HasValue)
                SizeMappingViewModel.Delete(Db, item.Id.Value);
            return Json((new SizeMappingViewModel[] { }).ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
    }
}
