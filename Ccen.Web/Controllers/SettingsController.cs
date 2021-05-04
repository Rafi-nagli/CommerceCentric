using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Web.Controllers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Settings;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Ccen.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllAdmins)]
    public partial class SettingsController : BaseController
    {
        public override string TAG
        {
            get { return "SettingsController."; }
        }

        //
        // GET: /Size/

        public virtual ActionResult ShippingCharges()
        {
            LogI("Index");
            var model = new ShippingChargeCollectionViewModel(DbFactory);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult SetShippingCharges(ShippingChargeCollectionViewModel model)
        {
            model.Save(DbFactory, Time.GetAppNowTime(), AccessManager.UserId);

            return JsonGet(new MessagesResult()
            {
                IsSuccess = true,
                Messages = new List<MessageString>()
                {
                    MessageString.Success("Succefully updated")
                }
            });
        }
    }
}