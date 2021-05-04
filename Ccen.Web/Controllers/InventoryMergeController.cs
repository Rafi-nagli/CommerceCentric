using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryMergeController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryMergeController."; }
        }

        private const string PopupContentView = "MergeStylePopupContent";
        
        public virtual ActionResult MergeStyle()
        {
            LogI("MergeStyle");
            var model = new MergeStyleViewModel();

            ViewBag.PartialViewName = PopupContentView;
            return View("EditNew", model);
        }


        [HttpPost]
        public virtual ActionResult Submit(MergeStyleViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var wasMerged = false;
                var errors = model.Validate();
                if (!errors.Any())
                {
                    IList<MessageString> messages;
                    wasMerged = model.Merge(LogService,
                        Db,
                        Cache,
                        DateHelper.GetAppNowTime(),
                        AccessManager.UserId,
                        out messages);

                    errors.AddRange(messages);
                }
                if (!wasMerged)
                {
                    errors.Each(e => ModelState.AddModelError(e.Key, e.Message));
                    
                    return PartialView(PopupContentView, model);
                }

                return Json(new UpdateRowViewModel(model, 
                    "Styles", 
                    null,
                    true));
            } 
            ViewBag.IsAdd = false;
            return PartialView(PopupContentView, model);
        }
    }
}
