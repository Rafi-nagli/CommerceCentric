using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public partial class AmazonBarcodeController : BaseController
    {
        public override string TAG
        {
            get { return "AmazonBarcodeController."; }
        }
        
        public virtual ActionResult GetAmazonBarcodeStatus(string barcode)
        {
            LogI("GetAmazonBarcodeStatus, barcode=" + barcode);

            try
            {
                var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
                marketplaceManager.Init();

                IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                    .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

                var callResult = ItemEditViewModel.GetAmazonBarcodeStatus(api, barcode);
                if (callResult.IsSuccess)
                    return JsonGet(ValueResult<bool?>.Success("", callResult.Data));
                else
                    return JsonGet(ValueResult<bool?>.Error("No results"));
            }
            catch (Exception ex)
            {
                return JsonGet(ValueResult<bool?>.Error(ex.Message));
            }
        }
    }
}
