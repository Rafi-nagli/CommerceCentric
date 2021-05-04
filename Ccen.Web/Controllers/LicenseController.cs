using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Core.Entities.Features;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class LicenseController : BaseController
    {
        public override string TAG
        {
            get { return "LicenseController."; }
        }

        //
        // GET: /License/

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View();
        }


        public virtual ActionResult GetAllParents([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAllParents");

            var items = LicenseViewModel.GetLicenses(Db).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetChildren([DataSourceRequest]DataSourceRequest request, int Id)
        {
            LogI("GetChildren, id=" + Id);

            var items = LicenseViewModel.GetSubLicenses(Db, Id).ToList();
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult AddParent([DataSourceRequest]DataSourceRequest request, LicenseViewModel item)
        {
            LogI("AddParent, item=" + item);

            item.AddAsParent(Db);
            
            var items = new[] { item };
            return Json(items.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult UpdateParent([DataSourceRequest]DataSourceRequest request, LicenseViewModel item)
        {
            LogI("UpdateParent, item=" + item);

            if (item != null && ModelState.IsValid)
            {
                item.UpdateAsParent(Db);
            }

            var items = new[] { item };
            return Json(items, JsonRequestBehavior.AllowGet); //.ToDataSourceResult(request, ModelState)
        }

        public virtual ActionResult AddChild([DataSourceRequest]DataSourceRequest request, LicenseViewModel item, int parentId)
        {
            LogI("AddChild, item=" + item + ", parentId=" + parentId);

            item.AddAsChild(Db);

            var items = new[] { item };
            return Json(items.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult UpdateChild([DataSourceRequest]DataSourceRequest request, LicenseViewModel item)
        {
            LogI("UpdateChild, item=" + item);

            item.UpdateAsChild(Db);

            var items = new[] {item};
            return Json(items, JsonRequestBehavior.AllowGet); //.ToDataSourceResult(request, ModelState)
        }
    }
}
