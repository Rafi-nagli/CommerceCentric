﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
using Ccen.Web;
using DropShipper.Api;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class IbcCloseoutController : BaseController
    {
        public override string TAG
        {
            get { return "IbcCloseoutController."; }
        }

        //
        // GET: /Size/

        [OutputCache(CacheProfile = "MiddleTimeProfile")]
        public virtual ActionResult GetInfo()
        {
            LogI("GetInfo");

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi mbgApi = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.DropShipper, MarketplaceKeeper.DsToMBG);

            IMarketApi tmxApi = new MarketFactory(Db.Marketplaces.GetAllAsDto(), Time, LogService, DbFactory, null)
                .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.DropShipper, MarketplaceKeeper.DsToTMX);

            var externalApis = new List<DropShipperApi>();
            if (mbgApi != null)
                externalApis.Add((DropShipperApi)mbgApi);
            if (tmxApi != null)
                externalApis.Add((DropShipperApi)tmxApi);

            var infoes = IbcCloseoutInfoViewModel.GetInfo(Db,
                externalApis);
            return JsonGet(CallResult<IList<IbcCloseoutInfoViewModel>>.Success(infoes));
        }

        public virtual ActionResult GetCloseoutForm()
        {
            LogI("GetCloseoutForm");

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi mbgApi = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.DropShipper, MarketplaceKeeper.DsToMBG);

            //NOTE: Direct from Marketplaces because TMX is hidden (in active marketplace)
            IMarketApi tmxApi = new MarketFactory(Db.Marketplaces.GetAllAsDto(), Time, LogService, DbFactory, null)
                .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.DropShipper, MarketplaceKeeper.DsToTMX);


            var shipmentProvider = ServiceFactory.GetShipmentProviderByType(
                    ShipmentProviderType.IBC, 
                    LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory);

            var model = new IbcCloseoutFormViewModel(LogService, Time);
            var result = model.Closeout(Db, 
                shipmentProvider,
                new List<DropShipperApi>()
                {
                    (DropShipperApi)mbgApi,
                    (DropShipperApi)tmxApi
                },
                PdfMaker,
                AppSettings.LabelDirectory,
                AppSettings.IsSampleLabels,
                AccessManager.UserId);

            var cacheToRemove = Url.Action("GetInfo", "IbcCloseoutController");
            HttpResponse.RemoveOutputCacheItem(path: cacheToRemove);

            if (result.IsSuccess)
            {
                return JsonGet(ValueResult<string>.Success("", result.Data));
            }
            else
            {
                return JsonGet(ValueResult<string>.Error(result.Message));
            }
        }
    }
}
