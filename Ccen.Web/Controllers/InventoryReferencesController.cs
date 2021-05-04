using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryReferencesController : BaseController
    {
        public override string TAG
        {
            get { return "StyleReferencesController."; }
        }

        private const string PopupContentView = "StyleReferencesPopupContent";

        public virtual ActionResult EditStyleReferences(long? id)
        {
            LogI("EditStyleReference, id=" + id);
            SessionHelper.ClearUploadedImages();

            var model = new StyleReferencesViewModel(Db, id, Time.GetAppNowTime());
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        public virtual ActionResult CreateStyleReferences()
        {
            LogI("CreateStyleReferences");
            SessionHelper.ClearUploadedImages();

            var model = new StyleReferencesViewModel(Db, null, Time.GetAppNowTime());
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }


        [HttpPost]
        public virtual ActionResult Submit(StyleReferencesViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var errors = model.Validate(Db);
                StyleViewModel returnModel = null;

                if (!errors.Any())
                {
                    model.SetUploadedImages(SessionHelper.GetUploadedImages());

                    var id = model.Apply(Db,
                        Cache,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    returnModel = StyleViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                    {
                        StyleId = id
                    }).Items?.FirstOrDefault();

                    SessionHelper.ClearUploadedImages();
                }
                else
                {
                    foreach (var error in errors)
                        ModelState.AddModelError("", error.ErrorMessage);

                    return PartialView(PopupContentView, model);
                }

                return Json(new UpdateRowViewModel(returnModel,
                    "Styles",
                    new[]
                    {
                        "HasImage", 
                        "Image", 
                        "Thumbnail", 
                        "StyleId", 
                        "Name", 
                        "StyleItemCaches",
                        "StyleItems",
                        "Locations",
                        "MainLocation",
                        "HasLocation",
                        "IsOnline",
                        "OnHold",
                        "CreateDate"
                    },
                    false));
            }

            return View(PopupContentView, model);
        }
    }
}