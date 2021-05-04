using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Inventory;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class BuyBoxController : BaseController
    {
        public override string TAG
        {
            get { return "BuyBoxController."; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult SetIgnored(int id, bool isIgnored)
        {
            LogI("SetIgnored, id=" + id + ", isIgnored=" + isIgnored);
            var buyBoxStatus = Db.BuyBoxStatus.Get(id);
            buyBoxStatus.IsIgnored = isIgnored;
            Db.Commit();

            return new JsonResult { Data = isIgnored, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetBuyBox([DataSourceRequest]DataSourceRequest request, 
            int? period, 
            bool? inStock, 
            bool? includeIgnored,
            int market,
            string marketplaceId)
        {
            LogI("GetBuyBox");

            var items = BuyBoxStatusViewModel.GetAll(Db, 
                    market,
                    marketplaceId,
                    (BuyBoxStatusViewModel.FilterPeriod) (period ?? 1),
                    inStock ?? true,
                    includeIgnored ?? false)
                .Where(bb => bb.Status != BuyBoxStatusCode.Undefined && bb.Status != BuyBoxStatusCode.None);
                
            var dataSource = items.ToDataSourceResult(request);
            return JsonGet(dataSource);
        }
    }
}
