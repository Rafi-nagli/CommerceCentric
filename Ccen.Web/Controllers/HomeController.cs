using System;
using System.Linq;
using System.Web.Mvc;

using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Status;

namespace Amazon.Web.Controllers
{
    [Authorize]
    public partial class HomeController : BaseController
    {
        public override string TAG
        {
            get { return "HomeController."; }
        }

        public virtual ActionResult TestException()
        {
            throw new Exception("Test Error");
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            if (AccessManager.IsRestricted)
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Orders", "Order");
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult GetBalance()
        {
            LogI("GetBalance");

            var companyId = AccessManager.CompanyId;
            if (!companyId.HasValue)
                throw new ArgumentNullException("CompanyId");

            var shipmentProviders = ServiceFactory.GetShipmentProviders(LogService,
                Time,
                DbFactory,
                WeightService,
                AccessManager.Company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            CompanyHelper.UpdateBalance(Db, AccessManager.Company, shipmentProviders, true, Time.GetAppNowTime());

            var model = new AccountStatusViewModel(AccessManager.Company);
            
            return new JsonResult
            {
                Data = ValueResult<ShipmentProviderViewModel[]>.Success("", model.ShipmentProviderList.ToArray()), 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult Message(string message)
        {
            LogI("Message, message=" + message);

            if (String.IsNullOrEmpty(message))
                return View("Error");
            return View("Message", (object)message);
        }
    }
}
