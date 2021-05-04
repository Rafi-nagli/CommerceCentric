using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.InventoryUpdateManual.CallActions;
using Amazon.InventoryUpdateManual.Models;
using Amazon.InventoryUpdateManual.TestCases;
using Amazon.Model.General;
using Amazon.Model.General.Markets.Amazon;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Emails;
using Amazon.Model.Implementation.Trackings;
using Amazon.Web.Models;
using DropShipper.Api;
using eBay.Api;
using Groupon.Api;
using Jet.Api;
using log4net.Config;
using Magento.Api.Wrapper;
using Shopify.Api;
using Supplieroasis.Api;
using Walmart.Api;
using WalmartCA.Api;
using WooCommerce.Api;

namespace Amazon.InventoryUpdateManual
{
    partial class Program
    {
        private static readonly ILogService _log = LogFactory.Console;
        //static private UserDTO _user;
        private static CompanyDTO _company;
        private static SettingsService _settings;
        private static IAddressService _addressService;
        private static MarketplaceKeeper _marketplace;
        private static AmazonApi _amazonApi;
        private static AmazonApi _amazonApiEU;
        private static AmazonApi _amazonApiDE;
        private static AmazonApi _amazonApiES;
        private static AmazonApi _amazonApiFR;
        private static AmazonApi _amazonApiIT;
        private static AmazonApi _amazonApiCA;
        private static AmazonApi _amazonApiMX;
        private static AmazonApi _amazonApiAU;
        private static AmazonApi _amazonApiIN;
        private static eBayApi _eBayAll4KidsApi;
        private static eBayApi _eBayPAApi;
        private static Magento20MarketApi _magentoApi;
        private static WalmartApi _walmartApi;
        private static WalmartCAApi _walmartCAApi;
        private static JetApi _jetApi;
        private static GrouponApi _grouponPA1Api;
        private static GrouponApi _grouponPA2Api;
        private static ShopifyApi _shopifyEveryChApi;
        private static WooCommerceApi _wooHdeaApi;
        private static ShopifyApi _shopifyBlondeApi;
        private static DropShipperApi _dsApi;
        private static SupplieroasisApi _overstock;

        private static IMarketCategoryService _amazonCategoryService;

        private static IShipmentApi _amazonLabelApi;

        private static ITime _time;
        private static IDbFactory _dbFactory;
        private static StyleManager _styleManager;
        private static IStyleHistoryService _styleHistoryService;
        private static IItemHistoryService _itemHistoryService;
        private static ISystemActionService _actionService;
        private static IQuantityManager _quantityManager;
        private static IPriceManager _priceManager;
        private static IPriceService _priceService;
        private static IEmailService _emailService;
        private static ICacheService _cacheService;

        static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            Console.WriteLine("Start...");
            Setup();

            var resultPath = Path.Combine(@"\\10.179.159.85\AmazonOutput", @"Labels\test.pdf");
            _log.Info(resultPath);

            _log.Info("/r/n/r/nStartup...............................");
            Console.WriteLine("Run...");
            
            //var result = StringHelper.KeepOnlyAlphanumeric(StringHelper.RemoveWhitespace("FASHIONMASK_2PK-BLACK"), new char[] { '-', '_' }, "-").Replace("--", "-").Replace("--", "-");
            //_log.Info(result);

            var matches = Regex.Matches("Order # 357 741-028 6925", @"([\d- ]{12,19})", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                _log.Info(match.Value.Replace("-", ""));
            }

            var result = StringHelper.RemoveAllNonASCII(@"joggers;lounging pajama party pants for girl;gift holiday;warm adult;fit for me;fun loungewear;home wear;pajamas for women;womens plus size pants;jogger;sleep;family;comfy;soft;printed lounge pink;purple;jogging;disney striped;snacks;victoria;donuts");
            _log.Info(result);

            Console.WriteLine(AppSettings.IsSampleLabels);

            var isWorkDay =_time.IsBusinessDay(DateTime.Now);
            _log.Info(isWorkDay.ToString());

            var autoPurchaseTime = TimeSpan.Parse(AppSettings.OverdueAutoPurchaseTime);
            _log.Info(autoPurchaseTime.ToString("hh\\:mm"));

            var service = new TrackingNumberService(null, null, null);
            var trackingNumber = service.AppendWithCheckDigit("940011690149549631782");
            //service.ApplyTrackingNumber(@"C:\Users\Ildar\Downloads\source-label-file-large-envelope.jpg", trackingNumber);
            _log.Info("#" + trackingNumber);

            using (var db = _dbFactory.GetRWDb())
            {
                //var lastPending = db.Orders
                //        .GetFiltered(o => o.OrderStatus == OrderStatusEnumEx.Pending
                //            && o.Market == (int)_amazonApi.Market
                //            && o.MarketplaceId == _amazonApi.MarketplaceId)
                //        .OrderBy(o => o.Id).FirstOrDefault();
                //_log.Info(lastPending?.OrderDate.ToString());
                //var result = db.SizeMappings.IsSize("1012");
                //_log.Info(result.ToString());
                //var styleId = SkuHelper.RetrieveStyleIdFromSKU(db, "K650TG-960-4", "Test 1");
                //_log.Info("StyleString: " + styleId);

            }
            //    var email = db.Emails.Get(43444);
            //    RegexOptions options = RegexOptions.None;
            //    Regex regex = new Regex("(<br/>|<br>|<br />){2,}", options);
            //    var ebayBody = regex.Replace(email.Message, " ");
            //    var ids = EmailParserHelper.GetEBayAssociatedOrderIds(ebayBody);
            //    _log.Info(ids.ToString());
            //}

            //var text = "Test sentence 1. Test sentence 2. Test sentence 3.";
            //var index = StringHelper.FindLastIndexOfTheSentenenceEnd(text, 35);
            //_log.Info(text.Substring(index.Value));
            //_log.Info(text.Substring(0, index.Value));

            var demoUpdater = new CallDemoProcessing(_company);
            //demoUpdater.CallUpdateTimeStamps();

            //_log.Info(SizeHelper.ExcludeSizeInfo("Peppa Pig Girls Toddler Plush Pink Bathrobe Robe Pajamas (2t/3t)"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Komar Kids Big Girls' Santa's Workshop Pink Pajamas L/10-12"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Angry Birds Boys' \"Born to be Angry\" 2 Piece Long Pajama Set L(10/12)"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("DC Comics Big Boys' Batman Vs Superman Sleepwear Coat Set, Black, Large"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("DC Comics Big Boys' Batman Vs Superman Sleepwear Coat Set, Black, Xs"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Widgeon Little Girls' 3 Bow Faux Fur Coat with Hat, color, talla 3"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Komar Kids Big Boys' Justice League BMJ Coat Set, Black, Large(10/12)"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Avengers Little Boys Multi Color Character Printed 2pc Pajama Short Set (4)"));
            //_log.Info(SizeHelper.ExcludeSizeInfo("Disney Frozen Dress - Up Fancy Girls Gown"));


            //_log.Info(StringHelper.ReplaceMultipleWhitespaceWithOne("ddd  ddddd dd dd "));

            var ourAmazonReports = new OurAmazonReports(_dbFactory, _log, _time, _amazonApi);
            //ourAmazonReports.BuildReportWithUnassignedToAmazonBarcodes();
            //ourAmazonReports.BuildReportMissingSizesOnAmazon();
            //ourAmazonReports.BuildFullInventoryReport();

            var chartProcessing = new CallChartProcessing(_dbFactory, _time, _log);
            //chartProcessing.AddListingErrorsPoints();
            
            var ftpMarketplaceProcessing = new CallFtpMarketProcessing(_dbFactory, _log, _time, _actionService, _quantityManager, _emailService);
            //ftpMarketplaceProcessing.CreateListings();
            //ftpMarketplaceProcessing.GenerateProductFeed();

            var grouponProcessing = new CallGrouponProcessing(_dbFactory, _cacheService, _emailService, _log, _time);
            //grouponProcessing.ImportGroupId(@"C:\Users\Ildar\Documents\Copy of Groupon Offer May_June.xlsx");
            //grouponProcessing.CreateGrouponListings(@"C:\Users\Ildar\Documents\PA_Groupon_Upload_2019_09_27.xlsx");
            //grouponProcessing.GenerateFeed(@"C:\Users\Ildar\Documents\PA_Groupon_Upload_2019_09_27.xlsx");
            //grouponProcessing.MergeKidsStyleInfoes(@"C:\Users\Ildar\Downloads\PA_Kids_For_Groupon_2.csv", @"C:\Users\Ildar\Downloads\Copy of Groupon Kids.csv");
            //grouponProcessing.UpdateOrders(_grouponApi);

            var bargains = new CallBargainsSearch(_dbFactory, _log, _time, _amazonApi);
            //var items = bargains.GetWalmartProducts(_amazonApi);
            //bargains.ExportBargains(items);
            //bargains.FindListingsPositionInSearch();            

            var supplieroasis = new CallSupplieroasisProcessing(_log, _time, _cacheService, _emailService, _dbFactory, _company);
            //supplieroasis.SyncItems(_overstock);
            //supplieroasis.SyncOrders(_overstock);
            //supplieroasis.SyncItemsFromSample(_overstock);
            //supplieroasis.SendInventoryUpdates(_overstock, new List<string>() { "669PN_672PM-FBP-S" });
            //supplieroasis.SendOrdersUpdate(_overstock);
                        
            var shopify = new CallShopifyProcessing(_log, _time, _cacheService, _emailService, _dbFactory, _styleHistoryService);
            //shopify.GetOrders(_shopifyBlondeApi, null);
            //shopify.ImportShopifyListings(_shopifyBlondeApi);
            //shopify.CreateShopifyListings();
            //shopify.SendItemUpdates(_shopifyBlondeApi);

            var wooCommerce = new CallWooCommerceProcessing(_log, _time, _cacheService, _emailService, _dbFactory);
            //wooCommerce.GetOrders(_wooHdeaApi, "196217");
            //wooCommerce.GetItems(_wooHdeaApi, null);
            //wooCommerce.ImportListings(_wooHdeaApi);
            //wooCommerce.SendItemUpdates(_wooHdeaApi);
            //wooCommerce.CallUpdateFulfillmentData(_wooHdeaApi, null);


            var magento = new CallMagentoProcessing(_log, _time, _emailService, _cacheService, _dbFactory);
            //magento.CreateMagentoListings();
            //magento.SubmitInventory(_magentoApi, new List<string>() { "FMM0001RB-2-S" });
            //magento.SyncAttributeOptions(_magentoApi);
            //magento.SubmitItems(_magentoApi, null);// new List<string>() { "N00257BR02-00;1548" });// new List<string>() { "DP314" });
            //magento.SubmitParentItems(_magentoApi, null);// new List<string>() { "DP314" });// "FMM0001RB" });//, "FMM0004RB" });
            //magento.LoadCustomCategories(@"C:\Users\Ildar\Downloads\Item Mapping from CCEN to PA Website .csv");

            var fedex = new CallFedexProcessing(_log, _time, _dbFactory, _company);
            //fedex.TestGetOneRateRates("111-4110885-3664232");
            //fedex.TestGetSmartPostRates("111-4110885-3664232");
            //fedex.ResetAndCreateBatch(@"C:\AmazonOutput\CustomBatches\PA-All.txt");
            //fedex.UpdateRateReport();

            var fims = new CallFIMSProcessing(_log, _time, _dbFactory, _company);
            //fims.TestGetLabel("702-8649569-2409866");
            
            var dsApiProcessing = new CallDsApiProcessing(_log, _time, _cacheService, _quantityManager, _actionService, _emailService, _dbFactory);
            //dsApiProcessing.GetOrders(_dsApi);
            //dsApiProcessing.CallUpdateFulfillmentData(_dsApi, new List<string>() { "180575",
//"181888" });
            //dsApiProcessing.CallUpdateQty(_dsApi);
            //dsApiProcessing.TestCloseIBCManifest(_dsApi);
            //dsApiProcessing.SyncStyles(_dsApi);
            //dsApiProcessing.SyncItems(_dsApi);
            //dsApiProcessing.SyncQty(_dsApi);
            //dsApiProcessing.CreatePAonMBGListings(@"C:\Users\Ildar\Documents\PAtoMBGSKUList_11_18_2019.xlsx");

            var walmartCA = new CallWalmartCAProcessing(_log, _time, _cacheService, _dbFactory, _emailService, _walmartCAApi, _company);
            //walmartCA.CreateWalmartListings();
            //walmartCA.UpdateListingInfo(_walmartCAApi, @"C:\Users\Ildar\Downloads\item_ca_report (10).zip");
            //walmartCA.GetAllItems(_walmartCAApi);
            //walmartCA.GetOrders(_walmartCAApi, "Y16320660");
            //walmartCA.CallOrderAcknowledgement();
            //walmartCA.CallUpdateFulfillmentData("Y17691105");
            //walmartCA.CallProcessCancellations();
            //walmartCA.CallProcessRefunds("Y12807246");
            //walmartCA.SubmitItemsFeed(_walmartCAApi, null, PublishedStatuses.None);
            //walmartCA.SubmitItemsFeed(_walmartCAApi, 
            //    new List<string>() {
            //        "3619MCLC-6X",
            //        "3620CLP-6X",
            //        "MS18056RP-SMALL/MEDIUM",
            //        "3652FWG-6X",
            //        "3640-QLB-6X",
            //        "FB-GOTWOODBB-M",
            //        "FB-TURNMEONBB-M",
            //        "FB-GOTWOODBB-L",
            //        "FB-TURNMEONBB-XL",
            //        "FB-TURNMEONBB-L",
            //        "FB-GOTWOODBB-XL",
            //        "FB-GOTWOODBB-S"},
            //        PublishedStatuses.HasChangesWithSKU);/// new List<string>() { "ML073TLL-2T" });
            //walmartCA.SubmitInventoryFeed(_walmartCAApi);
            //walmartCA.SubmitPriceFeed(_walmartCAApi, new List<string>() { "PK067-S" });
            //walmartCA.RetireUnpublishedItems(_walmartCAApi);
            //walmartCA.ResetNotExistListingQty(_walmartCAApi, @"C:\Users\Ildar\Downloads\item_ca_report (10).zip");

            var walmart = new CallWalmartProcessing(_log, _time, _cacheService, _dbFactory, _emailService, _itemHistoryService, _walmartApi, _company);
            //walmart.GetWalmartSizes(_walmartApi);
            //walmart.EnableSecondDayForListings();
            //walmart.ExportSetupByMatch();
            //walmart.ProcessCancellation(_walmartApi, "4922061994956");
            //walmart.TextExtractShortBody();
            //walmart.TestExtractWalmartItem();
            //walmart.LoadPrices();
            //walmart.GetOrders(_walmartApi);
            //walmart.GetOrder(_walmartCAApi, "Y13969940");
            //walmart.GetAllItems(_walmartApi);
            //walmart.GetItemBySKU(_walmartApi);
            //walmart.RetireToUnpublishItems(_walmartApi);
            //walmart.GetProductsInfo(new List<string>()
            //{
            //    "PR040-4",
            //    "PR040-6",
            //    "PR040-8",
            //    "PR040-10",
            //    "EF125-4",
            //    "EF125-6",
            //    "EF125-8",
            //    "EF125-10",
            //});
            //walmart.UpdateOrdersTaxInfo();
            //walmart.ReadInventoryInfo(_walmartApi);
            //walmart.ReadListingInfo(_walmartApi);
            //walmart.ResetNotExistListingQty(_walmartApi);
            //walmart.RetireNotExistListings(_walmartApi);
            //walmart.FindSecondDayFlagDisparity(_walmartApi, @"C:\Users\Ildar\Downloads\item_report.zip");
            //walmart.ResetNotExistListingQty(_walmartCAApi);
            //walmart.GetInventory(_walmartApi);
            //walmart.SubmitPriceFeed(_walmartApi, new List<string>() { "WF18271-XL" });
            //walmart.SendTestPromotion(_walmartCAApi);
            //walmart.SubmitInventoryFeed(_walmartApi, new List<string>() { "MBPENNY402560-12Y" });// new List<string>() { "4902CFF-PRT-3" });
            //walmart.SubmitTestInventoryFeed(_walmartApi);
            //walmart.SubmitPriceFeed(_walmartApi, new List<string>() { "886166911080-8" });
            //walmart.GetFeed(_walmartApi, "27CB339EC82545BF856ED35241916C8C@AQYBAQA");
            //walmart.SubmitSecondDayItems(_walmartApi);
            walmart.SubmitItemsFeed(_walmartApi,
                new List<string>()
                {
                    "K183489PP-2T",
                    "TE096-4T",
                    "1557HSH-2-2T",
                    "K183489PP-3T",
                    "1557NSS-8",
                    "K171727-4T",
                    "K171774-3T",
                    "K171787-2-XS",
                    "K171780-M",
                    "21SO108GDLBK-10",
                    "K171774-2T",
                    "K174891-2-3T",
                    "K157358SM-6/7",
                    "K171774-4T",
                    "K174891-2T",
                    "K157953EA-6/6X"
                },
                PublishedStatuses.HasChangesWithProductId);

            //walmart.SubmitItemsWithChildFeed(_walmartApi, new List<string>() { "GE223-5/6", "K157497PL-2T", "K183489PP-2T", "K203811WE-XL" });
            //walmart.SubmitItemsWithChildFeed(_walmartApi, new List<string>() { "WS17068US-S" }, PublishedStatuses.None);
            //walmart.SubmitItemsWithChildFeed(_walmartApi, new List<string>() { "K219449SC-2T" });
            //walmart.SubmitItemsWithChildFeed(_walmartApi, 
            //    new List<string>() { "WS17068US-S" });
            //walmart.ConvertPricesTo99();
            //walmart.CreateWalmartListings();
            //walmart.RepublishInactive();
            //walmart.RepublishWithImageIssue();
            //walmart.RepublishWithSKUIssue();
            //walmart.CallProcessOrders();
            //walmart.CallOrderAcknowledgement();
            //walmart.CallUpdatFulfillmentData(null);
            //walmart.CallProcessCancellations("3804333962080");
            //walmart.CallProcessRefunds(null);
            //walmart.ReadRefunds(_walmartApi);
            //walmart.ReducePrice(@"C:\Users\Ildar\Documents\reduce_WM.csv");

            var callTemplating  = new CallTemplating();
            //callTemplating.BuildRizorTemplate();

            var jet = new CallJetProcessing(_log, _time, _cacheService, _dbFactory, _emailService, _jetApi, _company);
            //jet.GetOrders(_jetApi);
            //jet.CallProcessOrders();
            //jet.CallOrderAcknowledgement();
            //jet.CallProcessReturns();
            //jet.CallUpdateFulfillmentData();
            //jet.SubmitItems(_jetApi);
            //jet.SubmitInventory(_jetApi);
            //jet.SubmitPrice(_jetApi);
            //jet.CreateJetListings();
            //jet.GetItemBySku(_jetApi);
            //jet.CallCompleteReturns(_jetApi);


            var eBay = new CallEBayProcessing(_log, _time, _cacheService, _dbFactory, _styleManager, _eBayAll4KidsApi, _emailService, _company);
            //eBay.CallGetItem("262889280839");
            //eBay.SubmitItemsFromFile(_eBayApi, @"C:\Users\Ildar\Downloads\eBayLinkIssues.xlsx");
            //eBay.SubmitItems(_eBayPAApi, new List<string>() { "M02030-S", "AP1852-DONUTS-HEARTS-2-S", "M02054-S", "88167RED-L",
            //    "AP-1852-ILOVEYOU-L",
            //    "LZ47907CO-XL",
            //"WF17378-PINK-XL",
            //"LZ47910SA-XS",
            //"M01825-XS"});// new List<string>() { "020186-BLACK-S" });// new List<string>() { "K172053-XS" });// new List<string>() { "EF011M-M", "K183945BM-XS", "S15G01BG-XS" }); // new List<string>() { "GE038-2-XS" });// new List<string>() { "262268096939-4" });
            //eBay.SubmitInventory(_eBayPAApi, new List<string>() { "WS18212PT-1X" });// new List<string>() { "K182645DJ-S" });
            //eBay.SubmitPrice(_eBayApi);
            //eBay.CallGetEBaySpecificOrders(new List<string>() { "262967057620-2280485017016" });
            //eBay.CreateEBayListings();
            //eBay.CallProcessEBayOrders("262895791908-2392734257016");
            //eBay.FindListingsSuitableForPromotion();
            //eBay.SyncItemsInfo(_eBayPAApi, null);
            //eBay.Republish(_eBayApi);

            var amazonProcessing = new CallAmazonProcessing(_company, _log, _time, _cacheService, _emailService, _amazonCategoryService, _dbFactory, _priceManager, _actionService, _itemHistoryService);
            //amazonProcessing.CallDeleteAmazonCAFBAListings(_amazonApiCA);
            //amazonProcessing.CallDeleteListingData(_amazonApi, null);
            //amazonProcessing.CreateAmazonAUListings();
            //amazonProcessing.CreateSFPListings();
            //amazonProcessing.ProcessRequestedReport(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_,
            //    _amazonApi,            
            //    new List<string>()
            //    {
            //        "17VZ039MBSTG-FBA-S",
            //        "17VZ039MBSTG-FBA-M",
            //        "17VZ039MBSTG-FBA-L",
            //        "17VZ039MBSTG-FBA-XL"
            //    });
            //amazonProcessing.ProcessRequestedReport(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_, _amazonApiEU, null);
            //amazonProcessing.ProcessRequestedReport(AmazonReportType._GET_XML_RETURNS_DATA_BY_RETURN_DATE_, _amazonApi, null);
            //amazonProcessing.ProcessRequestedReport(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_, _amazonApi, new List<string>() { "FZF00739ST-Pink" });
            //amazonProcessing.ProcessInactiveReport(@"C:\Users\Ildar\Documents\Inactive+Listings+Report+05-23-2018.txt");
            //amazonProcessing.ProcessRequestedReport(AmazonReportType._GET_MERCHANT_LISTINGS_DEFECT_DATA_, _amazonApi);
            //amazonProcessing.UpdateBuyBoxPrices(_amazonApi);
            //amazonProcessing.FillMissingBarcodes(_amazonApi);
            //amazonProcessing.GetProductInfoByBarcode(_amazonApi);
            //amazonProcessing.CallUpdateForNotPublishedLocked(_amazonApi);
            //amazonProcessing.CallUpdateAllChildListingData(_amazonApi, new List<string>() { "NK0218-XL" });//"1530-CGN-12M" });// new List<string>() { "FMM0001RB-S" });// new List<string>() {"B075S1FT41"});// "B074MJHB88" });
            ////amazonProcessing.CallUpdateListingData(_amazonApi, new List<string>() { "WB0994-2-FBP-L" });// new List<string>() { "MF17048PTPL-3X" });// new List<string>() {"B075S1FT41"});// "B074MJHB88" });
            //amazonProcessing.CallDeleteListingData(_amazonApiCA, new List<string> { "K182104HK-3-FBA-M" });
            //amazonProcessing.CallUpdatePriceData(_amazonApi, new List<string>() { "M02030-FBP-XL" });
            //amazonProcessing.CallUpdatePriceRuleData(_amazonApi, null);
            //amazonProcessing.CallUpdateQuantityData(_amazonApi, new List<string>() { "GF17185SS-6-2X", "GF17185SS-6-FBP-2X" });

            //amazonProcessing.CallUpdateListingImageData(_amazonApi, null);// new List<string>() { "21TE165ELLZA-2T" }); // new List<string>() { "FMM0004RB-S" });//"DP306-2-2T" });//"K172370-2-XS" });
            //amazonProcessing.CallUpdateListingRelationshipData(_amazonApi, new List<string>() { "MS18096-FBP-S" });// "K203642MA-M" });// { "M01825-XS" });// new List<string>() { "1013CFF-KBUN-2T" });

            //amazonProcessing.CallUpdatePriceRuleData(_amazonApi, null);
            //amazonProcessing.CreateListingsFromFile(@"D:\Projects\Watches\Amazon\Amazon Initial Upload PRICING 041818.xls");
            //amazonProcessing.LoadBuyBoxMinMaxFromFile(@"C:\Users\Ildar\Downloads\amazon-rafi.xlsx");
            //amazonProcessing.CallReadListingData(_amazonApi);
            //amazonProcessing.PublishFeed(@"C:\Users\Ildar\Downloads\Amazon NEW Upload 051018.xls");
            //amazonProcessing.FixupParentASINsForAU();
            //amazonProcessing.ExportUnderwearException();
            //amazonProcessing.CallRequestFixRelationship();
            //amazonProcessing.CallRequestFixPublishingInProgress();
            //amazonProcessing.CallRequestUnpublishUPCIssueListings();
            //amazonProcessing.CallFixItemColor();
            //amazonProcessing.CallFixItemBrand();
            //amazonProcessing.CallFixImageDefects();

            var image = new CallImageProcessing(_dbFactory, _time, _log);
            //image.UpdateStyleImageTypes();
            //image.UpdateAllHiRes();
            //image.ReplaceStyleImageToHiRes();

            //image.GetLargeImage("191226775", MarketType.Walmart, "");
            //image.CheckGetLargeImage();
            //image.UpdateItemHiRes("B01FCIJX7K");
            //image.ConvertStyleToStyleImages();

            //image.UpdateStyleHiResImages();
            //image.CreateSwatchImage();
            //image.CreateWalmartImage();
            //image.UpdateAllAmazonImageDifferences();
            //image.UpdateAllWalmartImageDifferences();
            //image.ReplaceStyleImageToHiRes();

            var inventory = new ProcessInventorization(_log, _dbFactory, _time);
            //inventory.Process();
            

            //DateTime? date = DateTime.UtcNow;
            //date = _time.AddBusinessDays(date, 1);
            //date = _time.AddBusinessDays(date, 1);
            //date = _time.AddBusinessDays(date, -2);
            //Console.WriteLine(date);

            var callTestHttp = new CallTestHttpParsing(_log, _time, _dbFactory);
            //callTestHttp.CallMellissaPage();
            //callTestHttp.CallGetSpecialPrice();

            
            var orderUpgradeProcessing = new CallOrderUpgradeProcessing(_log, _time, _dbFactory, _company);
            //orderUpgradeProcessing.UpdateUnshippedShippingAvgDeliveryDays();
            //orderUpgradeProcessing.UpgradeOrderList(new List<long>() { 282042 });
            //orderUpgradeProcessing.UpgradeOrders();

            var orderProcessing = new CallOrderProcessing(_eBayAll4KidsApi, 
                _magentoApi,
                _walmartApi,
                _amazonApi, 
                _amazonApiCA,
                _log, 
                _dbFactory,
                _emailService,
                _time,
                _company);

            //orderProcessing.CallOrdersSyncThread(_grouponPA1Api);
            //orderProcessing.MoveToOrderPageUnprintedOrders();
            //orderProcessing.UpdateIsFulfilledFlag(_walmartCAApi);
            //orderProcessing.UpdateIsFulfilledFlag(_walmartApi);
            //orderProcessing.CallUpdateCounts();
            //orderProcessing.CallCheckOverdue();
            //orderProcessing.ChangeOrderProviderToStamps();
            //orderProcessing.ChangeOrderProviderToIBC();
            //orderProcessing.ChangeOrderProviderToSkyPostal();
            //orderProcessing.ChangeOrderProviderToAuto();
            //orderProcessing.ChangeShippingOption();
            //orderProcessing.CallValidationRules("026-2077098-2773161");
            //orderProcessing.CallAddressValidation();
            //orderProcessing.CallProcessIBCInvoice("C:\\Users\\Ildar\\Documents\\PremiumApparel.A9805.csv");
            //orderProcessing.CallProcessDhlInvoice("07-25-2016-15800649QG25.csv");
            //orderProcessing.CallProcessDhlInvoice("08-01-2016-15800649QH01.csv");
            //orderProcessing.CallProcessDhlInvoice("08-08-2016-15800649QH08.csv");
            //orderProcessing.CallProcessDhlInvoice("08-15-2016-15800649QH15.csv");
            //orderProcessing.CallProcessDhlInvoice("08-22-2016-15800649QH22.csv");
            //orderProcessing.CallProcessDhlRates("DHL_Rates_full.csv");
            //orderProcessing.CallUpdateRates("114-0767404-0881837");
            //orderProcessing.CallAddressValidation();
            //orderProcessing.CallSendNoWeightEmail("109-2149956-7787401");
            //orderProcessing.CallProcessEBaySpecificOrders(new List<string>() { "199975280017" });
            //orderProcessing.CallProcessMagentoOrders();
            //orderProcessing.CallProcessEBayOrders();
            //orderProcessing.CallTestViewListings();
            //orderProcessing.CallProcessSpecifiedOrder(_walmartCAApi, "Y11449890");
            //orderProcessing.CallProcessEBayOrders();

            //orderProcessing.CallUpdateFulfillmentData(_amazonApi, null);// new List<string>() { "113-6243873-0250616", "114-0234173-5284278" });
            //orderProcessing.CallUpdateFulfillmentData(_amazonApiCA, new List<string>() { "702-1560109-8732264" });
            //orderProcessing.CallUpdateFulfillmentData(_amazonApiEU, null);
            //orderProcessing.CallUpdateFulfillmentData(_amazonApiMX, null);

            //orderProcessing.CallGetEuropeOrders(_amazonApiEU);
            //orderProcessing.CallTestDbInclude();
            //orderProcessing.CallUpdateEBayTrackingNumbers();
            //orderProcessing.CheckDupl("105-2999981-1913060");
            //orderProcessing.CallProcessSpecifiedOrder(_eBayPAApi, "163689735589-1868688139006");

            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "3806723437605");// "5571880034771");
            //            orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "3806813262611");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803122457611");
            //            orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803122389175");
            //            orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803122299137");
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "113-7767895-6481802");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "7803280771818");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803313678996");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803313673013");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "7803313643657");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "4803313680118");
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "111-0608481-7449828");
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "1807004230843");
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "4803313653915");

            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "113-7258205-0718642");
            //orderProcessing.CallProcessSpecifiedOrder(_wooHdeaApi, null);

            //orderProcessing.CallProcessSpecifiedOrder(_amazonApiEU, "405-2190584-1325106");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "1805750432087");
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApiCA, "702-2667316-0795426");// "5571880034771");
            //orderProcessing.CallProcessSpecifiedOrder(_grouponPA1Api, null);
            //orderProcessing.CallProcessSpecifiedOrder(_grouponApi, "GG-2CS2-XYBL-29V4-H6N9");

            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "111-7093751-3453031");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "1806736743472");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "1806733545341");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "3806723402614");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "1806722861008");

            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "2802426239765");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "3805429063850");
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, "1805429155827");  
            //orderProcessing.CallProcessSpecifiedOrder(_shopifyBlondeApi, null);
            //orderProcessing.CallProcessSpecifiedOrder(_walmartApi, null);

            //orderProcessing.CallProcessSpecifiedOrder(_dsApi, null);
            //orderProcessing.CallProcessSpecifiedOrder(_amazonApi, "113-6549686-6581821"); //, "102 -4145610-9208216"

            //orderProcessing.CallGetSpecificOrders(_walmartCAApi, new List<string>() { "Y12807246" });
            //orderProcessing.CallGetAllEBayItem();
            //orderProcessing.CallProcessEBayOrders("262120867095-1807120869016");
            //orderProcessing.CallUpdateAcknowledgementData(_amazonApi);
            //orderProcessing.CallUpdateAdjustmentData(_amazonApi, "114-5732084-0569051");


            var actionProcessing = new CallSystemActionProcessing(_log, _dbFactory, _time, _actionService);
            //actionProcessing.ProcessAddCommentSystemActions();
            //actionProcessing.ProcessRefundsAddComments();

            var printProcessing = new CallPrintProcessing(_log, _time, _dbFactory, _emailService, _company, 
                AppSettings.LabelDirectory,
                AppSettings.TemplateDirectory,
                AppSettings.ReserveDirectory);
            //printProcessing.AutoBuySameDay();
            //printProcessing.AutoBuyAmazonNextDay();
            //printProcessing.GetDhlInvoice();
            //printProcessing.PrintPdfFromOrders();
            //printProcessing.GetAmazonShipment();
            //printProcessing.RePrintBatch(3703);
            //printProcessing.PrintPdfFromBatch(3703);
            //printProcessing.PrintPdfFromOrder("113-1809801-6175412");
            //printProcessing.PrintActionProcessing();
            //printProcessing.GetCloseOutManifest(2151);
            //printProcessing.AutoBuyOverdueOrders();
            //printProcessing.CancelBatch(14387);
            //printProcessing.CallCanelLabelsRun1();
            //printProcessing.CallCancelLabels(new[] {
         
            //}, ShipmentProviderType.Stamps);
            //printProcessing.CallCancelLabels(new[] {"LZ594142825US", "LZ594143242US", "LZ594140405US"});// "9470111899223074826759" }); //For that tracking number refund was requested
            //printProcessing.CallCancelLabels(new[]
            //{
            //    "9400116901495496404024",
            //}, ShipmentProviderType.Stamps);
            //printProcessing.GetScanForm(1454, new DateTime(2017, 2, 23));
            //printProcessing.Validate(1397);            

            var notificationProcessing = new CallNotificationProcessing(_log, _addressService, _emailService, _dbFactory, _company, _time);
            //notificationProcessing.CheckEmailStatusNotification();
            //notificationProcessing.CheckQtyChangeNotification();
            //notificationProcessing.CheckListingIssuesNotification();
            //notificationProcessing.CheckQtyDisparityNotification();
            //notificationProcessing.CheckPriceDisparityNotification();

            var emailProcessing = new CallEmailProcessing(_log, _addressService, _dbFactory, _company, _time);
            //emailProcessing.TestBodyParse3();
            //emailProcessing.TestBodyParse2();
            //emailProcessing.TestCutBody2();
            //emailProcessing.SendEmails();
            //emailProcessing.ReadInboxEmails();
            //emailProcessing.ReadInboxEmails();
            //emailProcessing.UpdateAnsweredId();
            //emailProcessing.UpdateMatchOrderId(new List<long>() { 87263 });
            //emailProcessing.TestProcessSampleRemoveSignConfirmationEmail();
            //emailProcessing.TestProcessOrderCancellationEmail(275247);
            //emailProcessing.RecheckSetSystemTypeRule(162859);
            //emailProcessing.CheckEmailStatusNotification();
            //emailProcessing.TestProcessSampleReturnRequestEmail();
            //emailProcessing.RecheckAllReturnEmails();
            //emailProcessing.RecheckAllRefundEmails();
            //emailProcessing.CheckAutoResponse();            

            var addressProcessing = new CallAddressProcessing(_log, _time, _dbFactory, _company);
            //addressProcessing.CheckAddressByGoogleAPI("102-8788585-9750654");
            //addressProcessing.CheckAddressByFedexAPI("102-8788585-9750654");
            //addressProcessing.CallCheckAddress("108-2559720-8553047");
            //addressProcessing.RecheckAllUnshippedOrderAddress();
            //addressProcessing.CallCheckCorrectableAddress();
            //addressProcessing.CallCheckCorrectableAddress2();
            //addressProcessing.CallCheckCompleteInvalidAddress1();
            //addressProcessing.CallCheckAddressByMelissaScrapper("271239419339");

            var rePricingProcessing = new CallRePricingProcessing(_company, _styleManager, _priceService, _actionService, _settings, _itemHistoryService, _dbFactory, _log, _time);
            //rePricingProcessing.ProcessAmazonSQS();
            //rePricingProcessing.RequestQuantityByAdv(_amazonApi);
            //rePricingProcessing.RequestQuantityForAllMarketUnprocessedAsins(_amazonApi);
            //rePricingProcessing.CallUpdateRanks(_amazonApi, _dbFactory);
            //rePricingProcessing.CallUpdateLowestPrices(_amazonApi, _dbFactory);
            //rePricingProcessing.CallUpdateMyPrices(_amazonApi, _dbFactory);
            //rePricingProcessing.CallUpdateBuyBoxPrices(_amazonApi);
            //rePricingProcessing.CallUpdateBuyBoxPriceForSKU(_amazonApi, "F15b59LW-M");
            //rePricingProcessing.MoveSales();
            //rePricingProcessing.CallSalesEndChecker();
            //rePricingProcessing.MigrateUSSaleTo(MarketType.Jet);
            //rePricingProcessing.MigrateUSSaleTo(MarketType.eBay);
            //rePricingProcessing.UpdateIntlPrices(MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);
            //rePricingProcessing.FixupWalmartPrices();
            //rePricingProcessing.SyncWmSalesWithAmzSales();
            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.Amazon,
            //    MarketplaceKeeper.AmazonCaMarketplaceId,
            //    new List<string>()
            //    {
            //    });
            //            rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.WalmartCA, null,
            //                new List<string>()
            //            {
            //            });
            //, new List<string>() { "FW000-4" });
            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId, null);// new List<string>() { "FW000-4" });
            //rePricingProcessing.UpdateIntlSales(MarketType.WalmartCA, "");

            //rePricingProcessing.UpdateEBayPricesAddedShippingCost();
            //rePricingProcessing.UpdateWalmartPricesBasedOnUS();
            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.Walmart, null, null, true);// new List<string>() { "DF19304PC-L/XL" });// new List<string>() { "SH0010RSG-L" });

            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.WalmartCA, null, new List<string>() {
            //    "TP218-4",
            //    "TP218-6",
            //    "TP218-8",
            //});
            //    "17VZ039MBSTG-FPB-XL" });// new List<string>() { "FW000-4" });
            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId, null, false);
            //rePricingProcessing.UpdateIntlPricesBasedOnUS(MarketType.WalmartCA, null, null, false); //new List<string>() { "21MK086ELLDZ-1-2T" }
            //rePricingProcessing.UpdateIntlSalesBasedOnUS(MarketType.WalmartCA, "");

            //rePricingProcessing.UpdateIntlSalesBasedOnUS(MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);

            //rePricingProcessing.UpdateIntlSales(MarketType.AmazonAU, MarketplaceKeeper.AmazonAuMarketplaceId);
            //rePricingProcessing.UpdateIntlSalesBasedOnUS(MarketType.AmazonAU, MarketplaceKeeper.AmazonAuMarketplaceId);
            //rePricingProcessing.UpdatePrices(@"C:\Users\Ildar\Downloads\2nd Day Shipping.xlsx");

            var quantityProcessing = new CallQuantityProcessing(_dbFactory, _actionService, _log, _time);
            //quantityProcessing.CallRedistributeQuantity(null);
            //quantityProcessing.CallRedistributeQuantity(new List<string>() {
            //    "K157386PN"
            //}); 

            //quantityProcessing.CallRedistributeQuantity(new List<string>() {
            //    "MF19300SS_Xmas_hat",
            //    "MF19300SS",
            //});
            //With CA listings (not in US)
            //quantityProcessing.CallCheckSaleEnd(3922);
            //quantityProcessing.CallCheckPrice();   
            //quantityProcessing.CallCheckFbaFbpPrice();
            //quantityProcessing.CallCheckQty();

            var reportProcessing = new CallReportProcessing(_log, _dbFactory, _emailService, _amazonApi, _amazonApiCA, _eBayAll4KidsApi, _company, _time);
            //reportProcessing.FillWithQtyInfo(@"C:\Users\Ildar\Documents\FBA-WFS.xlsx", 0, 8);
            //reportProcessing.FillWithStyleInfo(@"C:\Users\Ildar\Documents\Clearance.xlsx");

            //reportProcessing.FillWithStyleInfo(@"C:\Users\Ildar\Documents\6-4-20 info need to pull from ccen - updated-v4.xlsx");
            //reportProcessing.FillWithStyleInfo(@"C:\Users\Ildar\Downloads\labels for wfs.xlsx");
            //reportProcessing.FillWithListingInfo(@"C:\Users\Ildar\Documents\FBA list links and qtys.xls");
            //reportProcessing.FillWithApiASIN(_amazonApi, @"C:\Users\Ildar\Documents\FBA list links and qtys and barcodes.xls");
            //reportProcessing.GenerateSKUInfoesForStyles(@"C:\Users\Ildar\Documents\MGB to WM to fill.xlsx");
            //reportProcessing.ProcessRequestedReport(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_, _amazonApi, new List<string>() { "B07JVQT6SN", "B07JD782LH", "B07JD75WDZ" });
            //reportProcessing.FillWithBarcodes();
            //reportProcessing.FillWithRateInfo(@"C:\Users\Ildar\Documents\expected rates.xlsx_updated.xlsx");
            //reportProcessing.AssingCustomBarcodesToFileSKUs(@"C:\Users\Ildar\Documents\sam_barcodes.xlsx");
            //reportProcessing.GetLargeImage("B015VJDFLK", MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);
            //reportProcessing.UpdateAllLargeImages();
            //reportProcessing.ReadPriceInfo(_amazonApi, "TD021-10");
            //reportProcessing.ReadAllPriceInfo(_amazonApi);
            //reportProcessing.GetRankByProductApi(_amazonApi);
            //reportProcessing.ComposeRefundReport(DateTime.Now.AddDays(-18));
            //reportProcessing.ComposeCompareRefundReport(DateTime.Now.AddDays(-45));
            //reportProcessing.ParseRefundReport(@"C:\Users\Ildar\Downloads\report-607828017766.xml");
            //_user.StampUsername = "rnagliPR";
            //reportProcessing.ReadFeatureExAttributes(@"C:\Users\Ildar\Downloads\PASubLicenseReport.csv");

            var rateProcessing = new CallRateProcessing(_company, _log, _dbFactory, _time);
            //rateProcessing.GetIntlRatesTest("114-7435247-9254660", ShipmentProviderType.Dhl);
            //rateProcessing.GetIntlRatesTest("116-8656965-8872229", ShipmentProviderType.Stamps);//"701-4330202-6193040");//"702-3194436-3925063");
            //rateProcessing.GetIntlRatesTest("114-9982299-5081011", ShipmentProviderType.Amazon);// "105-7129203-9779451");
            //rateProcessing.FillRateTable();
            //rateProcessing.FillIBCRateTable();
            //rateProcessing.FillWithCCENShippingCost(@"C:\Users\Ildar\Downloads\premium-apparel_7457_2020-06-01 - updated.xlsx");
            //var zipZonefilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/US_Zones.xlsx");
            //rateProcessing.ImportZipZones(zipZonefilePath);
            //rateProcessing.ImportGroundPlusRates();
            //rateProcessing.ImportGlobalMailPlusRates();
            //rateProcessing.ImportIntlDirectRates();
            //rateProcessing.ImportCAZoneMappingRates();
            //rateProcessing.ImportGBZoneMappingRates();
            //rateProcessing.ImportCustomTrackings("911490230722493425302", "911490230722493425499");
            //rateProcessing.RefreshSuspiciousFedexRates();

            var trackingProcessing = new CallTrackingProcessing(_log, _dbFactory, _emailService, _company, _time);
            //trackingProcessing.UpdateUSPSTrackOrders();
            //trackingProcessing.GetFedexTracking("123");
            //trackingProcessing.UpdateDHLTrackOrders();

            //trackingProcessing.UpdateUSPSTrackFromFile(@"D:\Temp\USPS invoice-Nov-First Class.csv");

            //trackingProcessing.GetDHLTracking("2437956743");
            //trackingProcessing.ReProcessTrackInfoFull(_dbFactory, "393418292108");
            //http://www.amazon.ca/dp/B015VJDFLK

            var inventoryProcessing = new CallInventoryProcessing(_company, _styleManager, _emailService, _cacheService, _amazonCategoryService, _dbFactory, _log, _time);
            //inventoryProcessing.ImportDescription(@"C:\Users\Ildar\Downloads\Copy of Copy of ccen bulk upload mens union suit.xlsx");
            //inventoryProcessing.ImportProperties(@"C:\Users\Ildar\Downloads\FB_Properties.xlsx");
            //inventoryProcessing.CallKioskBarcodeWoStyleNotification();
            //inventoryProcessing.TestParseSQSMessage();
            //inventoryProcessing.TestCallAdvCartCreate();
            //inventoryProcessing.CallStyleManagerCreateItem("1505-SZS", "6", "boys");
            //inventoryProcessing.CreateShopifyListings();
            //inventoryProcessing.CallUpdateMagentoItems(_magentoApi);
            //inventoryProcessing.CallSentQtyToMagento(_magentoApi);
            //inventoryProcessing.CallUpdatePriceData(_amazonApi, null);
            //inventoryProcessing.CallSentPriceToMagento(_magentoApi);
            //inventoryProcessing.CallUpdateAllParentItemsStyleIdEBay(_eBayApi);
            //inventoryProcessing.CallProcessCategoryReport(_amazonApi, "C:\\Projects.Vionix\\Marketplaces\\MarketsSellerCentral\\Amazon.InventoryUpdateManual\\Files\\Reports\\ParentSKU1_US.txt");
            //inventoryProcessing.CallProcessCategoryReport(_amazonApiCA, "C:\\Projects.Vionix\\Marketplaces\\MarketsSellerCentral\\Amazon.InventoryUpdateManual\\Files\\Reports\\ParentSKU1_CA.txt");
            //inventoryProcessing.CallUpdateParentItem(_amazonApi, "B01B4P27ZU");// "B00TSR5CS6");
            //inventoryProcessing.CallUpdateParentItemEBay(_eBayApi, "252153173865");
            //inventoryProcessing.CallUpdateItem(_amazonApi, "B013GGSND0");// "B00RA01KG6");
            //inventoryProcessing.CallGetItemsBySKU(_amazonApi, "GE223-5/6");// "HP0202_HP0203 -M");
            //inventoryProcessing.CallGetItemsByASIN(_amazonApi, "B00K4WM1T2");
            //inventoryProcessing.CallGetItemsBySKUAndCheckStyle(_amazonApi, "HP0075-S");
            //inventoryProcessing.CallGetItemsBySKUAndCheckStyle(_amazonApi, "HP0075-M");

            //inventoryProcessing.CallUpdateEBayItems(_eBayApi);
            //inventoryProcessing.CallSentQtyToEBay(_eBayApi);
            //inventoryProcessing.CallProcessTestListings(_amazonApi);
            //inventoryProcessing.CallGetProductByBarcode(_amazonApi);
            //inventoryProcessing.CallUpdateItem(_amazonApi);
            //inventoryProcessing.CallUpdateListingData(_amazonApi, new List<string>() {"K172370-2-XS"});//"DP306-2-2T" });//"K172370-2-XS" });
            //inventoryProcessing.CallUpdatePriceData(_amazonApi, new List<string>() { "K172370-2-XS" });
            //inventoryProcessing.CallUpdateQuantityData(_amazonApi, new List<string>() { "K172370-2-XS" });
            //inventoryProcessing.CallUpdateListingImageData(_amazonApi, new List<string>() { "K172370-2-XS" });//"DP306-2-2T" });//"K172370-2-XS" });
            //inventoryProcessing.CallUpdatePriceRuleData(_amazonApi, new List<string>() { "MF009M-XL" });//"DP306-2-2T" });//"K172370-2-XS" });
            //inventoryProcessing.CheckSizeMapping();
            //inventoryProcessing.UpdateBoxPrices();
            //inventoryProcessing.UpdateFileWithStyleInfo(@"C:\Users\Ildar\Downloads\Copy of Amazon - Suspected Intellectual Property Violations - ASIN List.csv");
            //inventoryProcessing.UpdateFileWithQty(@"C:\Users\Ildar\Documents\Copy of Premium Apparel Content Quality 2.21.xlsx");
            //inventoryProcessing.UpdateFileWithShippingPrice(@"C:\Users\Ildar\Documents\AppendWeight.xlsx");

            var cacheProcessing = new CallCacheProcessing(_log, _time, _quantityManager);
            //cacheProcessing.CallUpdateCache();
            //cacheProcessing.CallUpdateStyleIdCache(12911);
            //cacheProcessing.CallUpdateCache(1302);
            //cacheProcessing.CallUpdateStyleItemIdCache(12911);
            //cacheProcessing.CallUpdateRefundAmounts();

            var holidayProcessing = new CallHolidaysProcessing(_dbFactory, _log, _time);
            //holidayProcessing.AddDates();

            var buildAmazonListingsFeed = new BuildAmazonMultiListingExcel(_dbFactory,
                _log,
                _time,
                _amazonCategoryService,
                _company);

            //buildAmazonListingsFeed.BuildMultilistingCAExcel(new List<string>()
            //{
            //});

            //buildAmazonListingsFeed.BuildMultilistingUSPrimeExcelByChildASIN(
            //    new List<string>()
            //{
            //});

            //var fbaFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/FBA/Fbadone.csv");
            //var fbaInfoList = buildAmazonListingsFeed.ReadFBAInfo(fbaFilePath);
            //buildAmazonListingsFeed.BuildMultilistingUSFBAExcel(fbaInfoList);


            //var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/NewPrices/SP_updatedPrices.XLSX");
            //var priceImporter = new ImportNewPrices(_log, _dbFactory);
            //priceImporter.ProcessFile(filePath);

            var relist = new ImportRelist(_log, _time, _dbFactory, _cacheService, _emailService, _actionService);
            //var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/CheckRelist/item_export_1.csv");
            //relist.ImportWalmartItemsCsv(filePath);
            //relist.RelistByDbStatus();

            var inputFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Temp/LastPackingSlip.xlsx");
            var outputFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Temp/LastPackingSlip_changed.xlsx");
            //var model = new TempFileChangeFormat();
            //model.ChangeFormat(inputFilepath, outputFilepath);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/KomarInvoices/Foot 03.06.xlsx");
            //var invoiceImporter = new ImportKomarInvoices(_log, _time, _dbFactory, _quantityManager);
            //invoiceImporter.ProcessFile(filePath);

            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Upload_location_30_06_2015.csv");
            //var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            //    String.Format("Files/LocationNotFoundStyle_{0}.xls", DateTime.Now.ToString("yyyyMMdd_HHmmss")));
            //var importer = new ImportLocations(_log);  
            //importer.Import(importPath, outputPath, false, new int[] { 24, 25, 26 });

            //GetScanFormTest();
            //RePrintLastPack(620); 
            //CallPrintSampleLabel("111-2610062-1565823");// 30287);//"107-7844493-6852249");//46600

            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Isaak_order_02_08_2016.csv");
            //var importer = new ImportWholesale(_log);
            //importer.Import(importPath);

            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/ImportBarcodes-summary.csv");
            //var importer = new ImportBarcodes(_log);
            //importer.Import(importPath);

            //var styleBarcodeJob = new SyncListingAndStyleBarcodesJob(_log, _dbFactory, _time);
            //styleBarcodeJob.MoveBarcodesFromListingsToStyles();
            //styleBarcodeJob.RelinkListingsBarcodesToActualStyles();

            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/UPC-12 Barcodes List.txt");
            //var customBarcodeImporter = new ImportCustomBarcodes(_dbFactory, _log, _time);
            //customBarcodeImporter.Import(importPath);
      

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
        
        private static void Setup()
        {
            Database.SetInitializer<AmazonContext>(null);
            XmlConfigurator.Configure(new FileInfo(AppSettings.log4net_Config));
                        
            _dbFactory = new DbFactory();
            _time = new TimeService(_dbFactory);
            _settings = new SettingsService(_dbFactory);
            _addressService = AddressService.Default;

            _styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            _itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _styleManager = new StyleManager(_log, _time, _styleHistoryService);
            _actionService = new SystemActionService(_log, _time);
            _quantityManager = new QuantityManager(_log, _time);
            _priceManager = new PriceManager(_log, _time, _dbFactory, _actionService, _settings);
            _priceService = new PriceService(_dbFactory);
            _cacheService = new CacheService(_log, _time, _actionService, _quantityManager);
            _amazonCategoryService = new AmazonCategoryService(_log, _time, _dbFactory);

            var barcodeService = new BarcodeService(_log, _time, _dbFactory);

            IEmailSmtpSettings smtpSettings = new EmailSmtpSettings();

            using (var db = _dbFactory.GetRWDb())
            {
                _company = db.Companies.GetFirstWithSettingsAsDto();

                if (AppSettings.IsDebug)
                    smtpSettings = Models.SettingsBuilder.GetSmtpSettingsFromAppSettings();
                else
                    smtpSettings = Models.SettingsBuilder.GetSmtpSettingsFromCompany(_company);
                

                _emailService = new EmailService(_log, smtpSettings, _addressService);

                var marketplaces = new MarketplaceKeeper(_dbFactory, false);
                marketplaces.Init();

                var shipmentPrividers = db.ShipmentProviders.GetByCompanyId(_company.Id);

                var apiFactory = new MarketFactory(marketplaces.GetAll(), _time, _log, _dbFactory, AppSettings.JavaPath);

                var weightService = new WeightService();

                var serviceFactory = new ServiceFactory();
                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    shipmentPrividers,
                    null,
                    null,
                    null,
                    null);

                _amazonApi = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);
                _amazonApiCA = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);
                _amazonApiMX = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonMxMarketplaceId);
                _amazonApiEU = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonUkMarketplaceId);
                _amazonApiDE = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonDeMarketplaceId);
                _amazonApiES = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonEsMarketplaceId);
                _amazonApiFR = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonFrMarketplaceId);
                _amazonApiIT = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonItMarketplaceId);
                _amazonApiAU = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonAU, MarketplaceKeeper.AmazonAuMarketplaceId);
                _amazonApiIN = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.AmazonIN, MarketplaceKeeper.AmazonInMarketplaceId);
                _eBayAll4KidsApi = (eBayApi)apiFactory.GetApi(_company.Id, MarketType.eBay, MarketplaceKeeper.eBayAll4Kids);
                _eBayPAApi = (eBayApi)apiFactory.GetApi(_company.Id, MarketType.eBay, MarketplaceKeeper.eBayPA);
                _magentoApi = (Magento20MarketApi)apiFactory.GetApi(_company.Id, MarketType.Magento, "");
                _walmartApi = (WalmartApi) apiFactory.GetApi(_company.Id, MarketType.Walmart, "");
                _walmartCAApi = (WalmartCAApi) apiFactory.GetApi(_company.Id, MarketType.WalmartCA, "");
                _jetApi = (JetApi)apiFactory.GetApi(_company.Id, MarketType.Jet, "");
                _grouponPA1Api = (GrouponApi)apiFactory.GetApi(_company.Id, MarketType.Groupon, MarketplaceKeeper.GrouponPA1);
                _grouponPA2Api = (GrouponApi)apiFactory.GetApi(_company.Id, MarketType.Groupon, MarketplaceKeeper.GrouponPA2);
                _shopifyEveryChApi = (ShopifyApi)apiFactory.GetApi(_company.Id, MarketType.Shopify, MarketplaceKeeper.ShopifyEveryCh);
                _wooHdeaApi = (WooCommerceApi)apiFactory.GetApi(_company.Id, MarketType.WooCommerce, MarketplaceKeeper.WooHdea);
                _shopifyBlondeApi = (ShopifyApi)apiFactory.GetApi(_company.Id, MarketType.Shopify, MarketplaceKeeper.ShopifyBlonde);

                _dsApi = (DropShipperApi)apiFactory.GetApi(_company.Id, MarketType.DropShipper, MarketplaceKeeper.DsToMBG);
                _overstock = (SupplieroasisApi)apiFactory.GetApi(_company.Id, MarketType.OverStock, "");

                _amazonLabelApi = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Amazon);
            }
        }
    }
}
