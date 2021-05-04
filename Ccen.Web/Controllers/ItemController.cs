using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchResults;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Web.ViewModels.Feeds;
using Amazon.Web.ViewModels.Graph;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Products;
using Ccen.Web.ViewModels.Products.CheckBarcode;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Walmart.Api;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class ItemController : BaseController
    {
        public override string TAG
        {
            get { return "ItemController."; }
        }

        public virtual ActionResult Products(string parentAsin,
            MarketType? market,
            string marketplaceId,
            ListingsModeType? listingMode,
            string styleId,
            bool? showIssues)
        {
            LogI("Products, parentasin=" + parentAsin
                + ", market=" + market
                + ", marketplaceId=" + marketplaceId
                + ", listingMode=" + listingMode
                + ", styleId=" + styleId);

            marketplaceId = marketplaceId ?? "";

            if (market == MarketType.Magento
                || market == MarketType.Walmart
                || market == MarketType.WalmartCA
                || market == MarketType.Jet)
            {
                marketplaceId = "";
            }

            if (!market.HasValue)
            {
                market = MarketType.Amazon;
                marketplaceId = MarketplaceKeeper.AmazonComMarketplaceId;
            }

            var model = new ProductPageViewModel();
            model.ParentASIN = parentAsin;
            model.Market = market ?? MarketHelper.Default;
            model.MarketplaceId = marketplaceId ?? "";
            model.ListingsMode = listingMode ?? ListingsModeType.All;
            model.StyleId = styleId;
            if (showIssues == true)
            {
                model.PublishedStatus = (int)PublishedStatuses.PublishingErrors;
            }
            model.Init(Db, Settings);

            return View("Products", model);
        }

        [Compress]
        public virtual ActionResult GetAllParents(GridRequest request,
            string keywords,
            string styleName,
            string gender,
            string brand,
            long? styleId,
            long? dropShipperId,
            decimal? priceFrom,
            decimal? priceTo,
            int? publishedStatus,
            int? availability,
            int? listingsMode,
            int? market,
            string marketplaceId)
        {
            var pageSize = request.ItemsPerPage;

            var genderIds = (gender ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(g => Int32.Parse(g))
                .ToList();

            var filter = new ProductSearchFilterViewModel
            {
                Keywords = keywords,
                StyleName = styleName,
                StyleId = styleId,

                Brand = brand,
                Genders = genderIds,

                MinPrice = priceFrom,
                MaxPrice = priceTo,

                PublishedStatus = publishedStatus,

                DropShipperId = dropShipperId,
                Availability = availability.HasValue ? (ProductAvailability)availability : ProductAvailability.InStock,
                ListingsMode = (ListingsModeType?)listingsMode,

                Market = market.HasValue ? (MarketType)market.Value : MarketType.Amazon,
                MarketplaceId = marketplaceId,
                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
            };

            LogI("GetAllParents, filter=" + filter);
            var cacheService = new DbCacheService(Time);

            var data = ParentItemViewModel.GetAll(LogService,
                cacheService,
                DbFactory,
                request.ClearCache,
                filter);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //public virtual ActionResult GetIdListByFilters(string keywords,
        //    string styleName,
        //    int? availability,
        //    int? mainLicense,
        //    int? subLicense,
        //    string ageGroup,
        //    int? gender,
        //    int? noneSoldPeriod,
        //    decimal? priceFrom,
        //    decimal? priceTo,
        //    int? listingsMode,
        //    int? market,
        //    string marketplaceId,
        //    [Bind(Prefix = "sizes[]")] List<string> sizes)
        //{
        //    var filter = new ProductSearchFilterViewModel
        //    {
        //        MainLicense = mainLicense,
        //        SubLicense = subLicense,

        //        Gender = gender,
        //        NoneSoldPeriod = noneSoldPeriod,
        //        MinPrice = priceFrom,
        //        MaxPrice = priceTo,

        //        Market = market.HasValue ? (MarketType)market.Value : MarketType.Amazon,
        //        MarketplaceId = marketplaceId,
        //    };

        //    LogI("GetIdListByFilters, filter=" + filter);
        //    var result = ParentItemViewModel.GetIdListByFilters(Db, filter);
        //    return Json(ValueResult<ProductSearchResult>.Success("", result), JsonRequestBehavior.AllowGet);
        //}

        public virtual ActionResult OnUpdateParent(int id)
        {
            LogI("OnUpdateParent, id=" + id);

            var model = EditParentItemsViewModel.GetForEdit(Db, id);

            ViewBag.PartialViewName = "ParentItemViewModel";

            return View("EditNew", model);
        }

        public virtual ActionResult Submit(EditParentItemsViewModel model)
        {
            LogI("Submit, parent items, count=" + model.ParentItems.Count);

            if (model.ParentItems.Count > 0)
            {
                var selectedItem = model.SelectedItem;

                if (ModelState.IsValid)
                {   
                    foreach (var item in model.ParentItems)
                    {
                        if (item.Market.HasValue)
                        {
                            LogI("Sumbit parent item=" + item + ", market=" + item.Market + ", marketplaceId=" + item.MarketplaceId);

                            item.Update(Db,
                                LogService,
                                ActionService,
                                PriceManager,
                                (MarketType)item.Market.Value,
                                item.MarketplaceId,
                                Time.GetAppNowTime(),
                                AccessManager.UserId);
                        }
                        else
                        {
                            LogE("Can't update Parent Item. Market is empty");
                        }
                    }
                }

                return Json(new UpdateRowViewModel(selectedItem, "Products", new[]
                {
                    "StyleString",
                    "Comment",
                    "SalePrice",
                    "SaleStartDate",
                    "SaleEndDate",
                    "MaxPiecesOnSale",
                    "PiecesSoldOnSale",
                    "OnHold"
                }, false));
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetFeatureValues(int featureId, int? mainFeatureId)
        {
            LogI("GetFeatureValues, featureId=" + featureId + ", mainFeatureId=" + mainFeatureId);

            var allValues = Db.FeatureValues.GetValuesByFeatureId(featureId);

            var values = mainFeatureId.HasValue && mainFeatureId.Value > 0
                ? allValues.Where(f => f.ExtendedValue == mainFeatureId.ToString()).OrderBy(v => v.Value).ToList()
                : allValues.OrderBy(v => v.Value).ToList();

            return Json(new SelectList(values, "Id", "Value"), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetItemTypeList()
        {
            LogI("GetItemTypeList");

            return Json(OptionsHelper.ItemTypes, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult RequestUpdate(MarketType? market, string marketplaceId)
        {
            LogI("RequestUpdate, market=" + market + ", marketplaceId=" + marketplaceId);
            Settings.SetListingsManualSyncRequest(true, market ?? MarketType.Amazon, marketplaceId);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult PauseUpdate(MarketType? market, string marketplaceId)
        {
            LogI("PauseUpdate, market=" + market + ", marketplaceId=" + marketplaceId);
            var pauseState = Settings.GetListingsSyncPause(market ?? MarketType.Amazon, marketplaceId) ?? false;
            LogI("PauseUpdate, pause=" + pauseState);
            Settings.SetListingsSyncPause(!pauseState, market ?? MarketType.Amazon, marketplaceId);

            return JsonGet(ValueResult<bool>.Success("", !pauseState));
        }

        public virtual ActionResult CheckForUpdate(MarketType? market, string marketplaceId)
        {
            var date = Settings.GetListingsSendDate(market ?? MarketType.Amazon, marketplaceId);
            var updateRequested = Settings.GetListingsManualSyncRequest(market ?? MarketType.Amazon, marketplaceId) ?? false;

            return Json(MessageResult.Success(
                    date.HasValue ? DateHelper.ConvertUtcToApp(date.Value).ToString(DateHelper.DateTimeFormat) : "-",
                    updateRequested ? "1" : "0"),
                JsonRequestBehavior.AllowGet);
        }

        #region GetRank

        [AllowAnonymous]
        public virtual ActionResult CheckRank()
        {
            return View("CheckRank");
        }

        [AllowAnonymous]
        public virtual ActionResult GetRank(string asin)
        {
            LogI("GetRank, asin=" + asin);

            ItemRankViewModel model = null;
            string message = null;
            bool isSuccess = false;

            try
            {
                var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
                marketplaceManager.Init();

                IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                    .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

                model = ItemRankViewModel.GetRank(api, Db, LogService, Time, asin);

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogE("GetRank", ex);
                message = ex.Message;
                isSuccess = false;
            }

            return JsonGet(new ValueResult<ItemRankViewModel>(isSuccess, message, model));
        }

        #endregion

        #region GetBarcode

        [AllowAnonymous]
        public virtual ActionResult CheckBarcode()
        {
            return View("CheckBarcode");
        }

        [AllowAnonymous]
        public virtual ActionResult GetBarcode(string barcode)
        {
            LogI("GetBarcode, barcode=" + barcode);

            ItemBarcodeViewModel model = new ItemBarcodeViewModel();
            string message = null;
            bool isSuccess = false;

            if (string.IsNullOrEmpty(barcode))
            {
                return JsonGet(new ValueResult<ItemBarcodeViewModel>(isSuccess, "barcode is empty", model));
            }
            if (barcode.Length != 12 && barcode.Length != 13)
            {
                return JsonGet(new ValueResult<ItemBarcodeViewModel>(isSuccess, "incorect barcode format", model));
            }

            try
            {
                var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
                marketplaceManager.Init();

                IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                    .GetApi((AccessManager.Company ?? AccessManager.DefaultCompany).Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

                model.Items.AddRange(ItemBarcodeViewModel.GetUrlItemsByBarcodeFromAmazonApi(api, barcode)
                    .Select(x => new ItemBarcodeListingViewModel() { Marketplace = "Amazon", Url = x }));

                var openApi = new WalmartOpenApi(LogService, "trn9fdghvb8p9gjj9j6bvjwx");
                model.Items.AddRange(ItemBarcodeViewModel.GetUrlItemsByBarcodeFromWalmartApi(openApi, new ItemBarcodeSearchFilter
                {
                    Keywords = barcode,
                    CategoryId = "",
                    MinPrice = null,
                    MaxPrice = null,
                    StartIndex = 1,
                    LimitCount = 10,
                }).Select(x => new ItemBarcodeListingViewModel() { Marketplace = "Walmart", Url = x }));

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogE("GetRank", ex);
                message = ex.Message;
                isSuccess = false;
            }

            return JsonGet(new ValueResult<ItemBarcodeViewModel>(isSuccess, message, model));
        }

        #endregion

        #region Export to Excel


        public virtual ActionResult ExportReturnExemptions(string marketplaceId)
        {
            LogI("ExportReturnExemptions");

            string filename;
            var output = ReturnExemptionExportViewModel.Export(Db,
                Time,
                LogService,
                Time.GetAmazonNowTime(),
                AccessManager.UserId,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToExcelUS(string asin,
            MarketType market,
            string marketplaceId,
            bool useStyleImage)
        {
            LogI("ExportToExcelUS, asin=" + asin + ", market=" + market + ", marketplaceId=" + marketplaceId + ", useStyleImage=" + useStyleImage);

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi(AccessManager.Company.Id, market, marketplaceId);

            string filename;
            var output = ExcelProductUSViewModel.ExportToExcelUS(LogService,
                Time,
                AmazonCategoryService,
                HtmlScraper,
                api,
                Db,
                AccessManager.Company,
                asin,
                market,
                marketplaceId,
                useStyleImage,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToExcelCA(string asin,
            MarketType market,
            string marketplaceId,
            bool useStyleImage)
        {
            LogI("ExportToExcelCA, asin=" + asin + ", market=" + market + ", marketplaceId=" + marketplaceId + ", useStyleImage=" + useStyleImage);

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi(AccessManager.Company.Id, market, marketplaceId);

            string filename;
            var output = ExcelProductCAViewModel.ExportToExcelCA(LogService,
                Time,
                AmazonCategoryService,
                HtmlScraper,
                api,
                Db,
                AccessManager.Company,
                asin,
                market,
                marketplaceId,
                useStyleImage,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public virtual ActionResult ExportToExcelUK(string asin,
            MarketType market,
            string marketplaceId,
            bool useStyleImage)
        {
            LogI("ExportToExcelUK, asin=" + asin + ", market=" + market + ", marketplaceId=" + marketplaceId + ", useStyleImage=" + useStyleImage);

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), Time, LogService, DbFactory, null)
                .GetApi(AccessManager.Company.Id, market, marketplaceId);

            string filename;
            var output = ExcelProductUKViewModel.ExportToExcelUK(LogService,
                Time,
                AmazonCategoryService,
                HtmlScraper,
                api,
                Db,
                AccessManager.Company,
                asin,
                market,
                marketplaceId,
                useStyleImage,
                out filename);

            return File(output.ToArray(),   //The binary data of the XLS file
               "application/vnd.ms-excel", //MIME type of Excel files
                                           //string.Format("{0}s_{1}.xls", asin, DateTime.Now.ToString(DateHelper.DateTimeFormat))
               filename);     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        #endregion

        public virtual ActionResult GetItemErrors(long itemId)
        {
            LogI("GetItemErrors, itemId=" + itemId);

            var messages = ItemViewModel.GetItemErrors(Db, itemId);

            return JsonGet(new ValueResult<IList<FeedMessageViewModel>>()
            {
                Data = messages,
                IsSuccess = true
            });
        }

        public virtual ActionResult GetItemFeedInfo(long itemId)
        {
            LogI("GetItemFeedInfo, itemId=" + itemId);

            var feedInfo = ItemViewModel.GetItemFeedInfo(Db, itemId);

            return JsonGet(new ValueResult<FeedViewModel>(true, null, feedInfo));
        }


        #region Sales Graph
        public virtual ActionResult SalesPopupByASIN(MarketType market, string marketplaceId, string asin)
        {
            var model = SalesPageViewModel.ComposeByParentASIN(Db, market, marketplaceId, asin);

            return View("SalesPopupContent", model);
        }

        public virtual ActionResult SalesPopupByStyleItem(long styleItemId)
        {
            var model = SalesPageViewModel.ComposeByStyleItemId(Db, styleItemId);

            return View("SalesPopupContent", model);
        }

        public virtual ActionResult SalesPopupByStyleId(long styleId)
        {
            var model = SalesPageViewModel.ComposeByStyleId(Db, styleId);

            return View("SalesPopupContent", model);
        }

        public virtual ActionResult GetSalesByASIN(MarketType market, string marketplaceId, string asin, string[] skuList, int period)
        {
            var model = SalesGraphViewModel.ComposeByParams(Db, market, marketplaceId, skuList, null, period);
            return new JsonResult
            {
                Data = new ValueResult<SalesGraphViewModel>(true, null, model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetSalesByStyleItem(long[] styleItemIdList, int period)
        {
            var model = SalesGraphViewModel.ComposeByParams(Db, MarketType.None, null, null, styleItemIdList, period);
            return new JsonResult
            {
                Data = new ValueResult<SalesGraphViewModel>(true, null, model),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        #endregion
    }
}
