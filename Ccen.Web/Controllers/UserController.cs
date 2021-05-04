using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Companies;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllAdmins)]
    public partial class UserController : BaseController
    {
        public override string TAG
        {
            get { return "UserController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult Index()
        {
            LogI("Index");
            return View();
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request)
        {
            LogI("GetAll");

            var items = UserViewModel.GetAll(Db);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public virtual ActionResult Add([DataSourceRequest]DataSourceRequest request, 
            UserViewModel item)
        {
            LogI("Add, item=" + item);
            if (ModelState.IsValid)
            {
                var validationResults = item.Validate(Db);
                if (validationResults.Any())
                {
                    validationResults.Each(v => ModelState.AddModelError("", v.Message));
                }
                else
                {
                    if (AccessManager.CompanyId.HasValue)
                    {
                        item.Apply(Db,
                            AccessManager.CompanyId.Value,
                            Time.GetAppNowTime(),
                            AccessManager.UserId);
                    }
                }
            }
            return Json((new[] { item }).ToDataSourceResult(request, ModelState), 
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Update([DataSourceRequest]DataSourceRequest request, 
            UserViewModel item)
        {
            LogI("Update, item=" + item);

            if (ModelState.IsValid && item != null)
            {
                var validationResults = item.Validate(Db);
                if (validationResults.Any())
                {
                    validationResults.Each(v => ModelState.AddModelError("", v.Message));
                }
                else
                {
                    if (AccessManager.CompanyId.HasValue)
                    {
                        item.Apply(Db,
                            AccessManager.CompanyId.Value,
                            Time.GetAppNowTime(),
                            AccessManager.UserId);
                    }
                }
            }

            return Json((new[] { item }).ToDataSourceResult(request, ModelState), 
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult Remove([DataSourceRequest]DataSourceRequest request, 
            UserViewModel item)
        {
            LogI("Remove, item=" + item);

            if (item != null && item.Id.HasValue)
                item.Delete(Db,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

            return Json((new UserViewModel[] { }).ToDataSourceResult(request, ModelState), 
                JsonRequestBehavior.AllowGet);
        }
    }
}
