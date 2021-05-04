using Amazon.Core.Models.Calls;
using Amazon.Utils;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllClients)]
    public partial class NewOrderController : BaseController
    {
        public override string TAG
        {
            get { return "NewOrderController."; }
        }

        private const string PopupContentView = "OrderNewViewModel";

        public virtual ActionResult Index(string orderId)
        {
            LogI("Orders");

            return View();
        }

        [HttpPost]
        public virtual ActionResult Create(OrderNewViewModel model)
        {
            LogI("Submit, model=" + model);

            //model.PrepareData();

            //Save
            if (ModelState.IsValid)
            {
                IList<MessageString> messages;

                if (model.IsValid(Db, out messages))
                {
                    model.Create(LogService,
                        Time,
                        QuantityManager,
                        DbFactory,
                        WeightService,
                        ShippingService,
                        AutoCreateListingService,
                        Settings,
                        EmailService,
                        ActionService,
                        HtmlScraper,
                        OrderHistoryService,
                        PriceService,
                        AccessManager.Company,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    return Json(model);
                }
                else
                {
                    messages.ForEach(m => ModelState.AddModelError("model", m.Message));
                }
                //var rand = new Random();                
                //model.OrderNumber = (20000 + rand.Next(1, 100)).ToString();
                model.Messages.Add(MessageString.Success("Order has been successfully created. Order #: " + model.OrderNumber));
                return Json(model);
            }

            return PartialView(PopupContentView, model);
        }
    }
}