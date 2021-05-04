using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Bargains;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Amazon.Web.ViewModels.Statuses;
using DropShipper.Api;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class FedexCloseoutController : BaseController
    {
        public override string TAG
        {
            get { return "FedexCloseoutController."; }
        }

        //
        // GET: /Size/

        [OutputCache(CacheProfile = "MiddleTimeProfile")]
        public virtual ActionResult GetInfo()
        {
            LogI("GetInfo");

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.DropShipper, MarketplaceKeeper.DsToMBG);
            
            var infoes = FedexCloseoutInfoViewModel.GetInfo(Db, Time, (DropShipperApi)api);
            return JsonGet(CallResult<IList<FedexCloseoutInfoViewModel>>.Success(infoes));
        }
    }
}
