using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Histories;
using Amazon.Core.Models.SystemActions;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Utils;
using Amazon.Web.General.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.Models.Exports.Types;
using Style = Amazon.Core.Entities.Inventory.Style;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Core.Entities.Caches;
using Amazon.Web.General.ViewModels.Inventory;
using System.Threading.Tasks;
using System.Net.Http;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleMarketplaceInfo
    {
        public string StyleString { get; set; }
        public int Count { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ShortName
        {
            get { return MarketHelper.GetShortName(Market, MarketplaceId); }
        }
        public string ASIN { get; set; }
        public string SourceMarketId { get; set; }

        public bool IsPublished { get; set; }
        public bool HasPublishErrors { get; set; }
        public bool HasPublishInProgress { get; set; }

        public string MarketUrl
        {
            get
            {
                return UrlManager.UrlService.GetMarketUrl(ASIN,
                      SourceMarketId,
                      (MarketType)Market,
                      MarketplaceId);
            }
        }

        public string ProductStyleUrl
        {
            get
            {
                return UrlManager.UrlService.GetProductByStyleUrl(StyleString,
                  (MarketType)Market,
                  MarketplaceId);
            }
        }

        public StyleMarketplaceInfo()
        {

        }

        //public StyleMarketplaceInfo(MarketplaceCacheInfo info)
        //{
        //    //ShortName = info.MarketName;
        //    Count = info.ListingsCount;
        //    Market = (int)MarketHelper.GetMarketTypeByName(ShortName);
        //    MarketplaceId = MarketHelper.GetMarketplaceIdTypeByName(ShortName);
        //    ASIN = info.ASIN;
        //    SourceMarketId = info.SourceMarketId;
        //}

        //public StyleMarketplaceInfo(string infoString)
        //{
        //    var parts = infoString.Split(":".ToCharArray());
        //    ShortName = parts[0];
        //    Count = Int32.Parse(parts[1]);
        //    Market = (int)MarketHelper.GetMarketTypeByName(ShortName);
        //    MarketplaceId = MarketHelper.GetMarketplaceIdTypeByName(ShortName);
        //}
    }


    public class StyleViewModel : IImagesContainer
    {
        public const int DefaultItemType = ItemType.Pajama;

        public long Id { get; set; }
        [Required]
        public string StyleId { get; set; }
        public string OriginalStyleId { get; set; }
        public int Type { get; set; }

        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }

        public bool OnHold { get; set; }

        public string Name { get; set; }


        public int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }

        public int DisplayMode { get; set; }

        public bool RemovePriceTag { get; set; }
        
        public string Manufacturer { get; set; }

        public string Description { get; set; }

        public int FillingStatus { get; set; }

        public int PictureStatus { get; set; }
        public DateTime? PictureStatusUpdateDate { get; set; }
        public string Comment { get; set; }
        public DateTime? CommentUpdateDate { get; set; }


        #region Excel Fields

        public decimal Price { get; set; }
        public decimal MSRP { get; set; }

        public string SearchTerms { get; set; }
        public string WMKeywords { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }

        public IList<string> GetBulletPoints()
        {
            return new List<string>()
            {
                BulletPoint1,
                BulletPoint2,
                BulletPoint3,
                BulletPoint4,
                BulletPoint5,
            };
        }

        #endregion

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? ReSaveDate { get; set; }

        public string Thumbnail
        {
            get
            {
                return UrlManager.UrlService.GetThumbnailUrl(Image,
                    75,
                    75,
                    false,
                    ImageHelper.NO_IMAGE_URL,
                    false,
                    false,
                    convertInDomainUrlToThumbnail: true);
            }
        }

        public bool IsHidden
        {
            get { return DisplayMode == (int) DisplayModes.Hidden; }
            set { DisplayMode = value ? (int)DisplayModes.Hidden : (int)DisplayModes.Visible; }
        }

        public int BoxQuantity { get; set; }

        public int UsedBoxQuantity
        {
            get
            {
                return Math.Max(0, (InventoryQuantity ?? 0) - (ManuallyQuantity ?? 0));
            }
        }

        //Grid Fields
        public string Image { get; set; }

        public bool IsOnline { get { return AssociatedMarket != null && AssociatedMarket != (int)MarketType.None; } }

        public string Status
        {
            get { return IsOnline ? "Online" : "Offline"; }
        }

        public bool HasManuallyQuantity { get; set; }
        public int? ManuallyQuantity { get; set; }

        public int TotalQuantity
        {
            get { return BoxQuantity + (ManuallyQuantity ?? 0); }
        }

        public int? RemainingQuantity { get; set; }

        public int? InventoryQuantity { get; set; }

        public int? MarketsSoldQuantity { get; set; }
        public int? ScannedSoldQuantity { get; set; }
        public int? SentToFBAQuantity { get; set; }
        public int? SpecialCaseQuantity { get; set; }
        public int? SentToPhotoshootQuantity { get; set; }

        public int? TotalMarketsSoldQuantity { get; set; }
        public int? TotalScannedSoldQuantity { get; set; }
        public int? TotalSentToFBAQuantity { get; set; }
        public int? TotalSpecialCaseQuantity { get; set; }
        public int? TotalSentToPhotoshootQuantity { get; set; }

        public string QuantityMode
        {
            get
            {
                if (HasManuallyQuantity && BoxQuantity > 0)
                    return "Mixed";
                if (HasManuallyQuantity && BoxQuantity == 0)
                    return "Manually";
                if (!HasManuallyQuantity && BoxQuantity > 0)
                    return "From boxes";
                return "-";
            }
        }


        public decimal BoxTotalPrice { get; set; }
        public decimal BoxItemMinPrice { get; set; }
        public decimal BoxItemMaxPrice { get; set; }

        public int? AssociatedMarket { get; set; }
        public string AssociatedMarketplaceId { get; set; }

        public string AssociatedASIN { get; set; }
        public string AssociatedSourceMarketId { get; set; }
        //

        //public string MarketplacesInfo { get; set; }

        public bool HasAssociatedASIN
        {
            get
            {
                return !String.IsNullOrEmpty(AssociatedASIN);
            }
        }

        public string ProductStyleUrl
        {
            get
            {
                return UrlManager.UrlService.GetProductByStyleUrl(StyleId,
                  AssociatedMarket.HasValue ? (MarketType)AssociatedMarket : MarketType.Amazon,
                  AssociatedMarketplaceId);
            }
        }

        public string ProductUrl
        {
            get
            {
                return UrlManager.UrlService.GetProductUrl(AssociatedASIN,
                  AssociatedMarket.HasValue ? (MarketType)AssociatedMarket : MarketType.Amazon,
                  AssociatedMarketplaceId);
            }
        }

        public string MarketUrl
        {
            get
            {
                return UrlManager.UrlService.GetMarketUrl(AssociatedASIN,
                      AssociatedSourceMarketId,
                      AssociatedMarket.HasValue ? (MarketType)AssociatedMarket : MarketType.Amazon,
                      AssociatedMarketplaceId);
            }
        }

        public bool HasImage { get { return !string.IsNullOrEmpty(Image); } }

        public bool HasSale
        {
            get { return StyleItemCaches != null && StyleItemCaches.Any(si => !String.IsNullOrEmpty(si.SalesInfo)); }
        }

        public List<FeatureViewModel> Features { get; set; }

        public List<LocationViewModel> Locations { get; set; }

        public StyleItemCollection StyleItems { get; set; }

        public IList<StyleItemShowViewModel> StyleItemCaches { get; set; }

        public List<ImageViewModel> Images
        {
            get { return ImageSet != null ? ImageSet.Images : null; }
            set
            {
                if (ImageSet != null)
                    ImageSet.Images = value;
            }
        }

        public IList<StyleItemPublishViewModel> Publishes { get; set; }

        public ImageCollectionViewModel ImageSet { get; set; }

        public IList<StyleMarketplaceInfo> Marketplaces { get; set; }
        
        public override string ToString()
        {
            return "Id=" + Id
                + ", StyleId=" + StyleId
                + ", Name=" + Name
                + ", QuantityMode=" + QuantityMode
                + ", OnHold=" + OnHold;
        }

        public StyleViewModel()
        {
            Features = new List<FeatureViewModel>();
            Locations = new List<LocationViewModel>();
            ImageSet = new ImageCollectionViewModel();
            Publishes = new List<StyleItemPublishViewModel>();
            StyleItems = new StyleItemCollection()
            {
                DisplayMode = Type == (int)StyleTypes.References ? StyleItemDisplayMode.StandardNoActions : StyleItemDisplayMode.Standard,
                Items = new List<StyleItemViewModel>()
            };
        }

        public StyleViewModel(IUnitOfWork db, int itemTypeId)
        {
            ItemTypeId = itemTypeId;
            DropShipperId = DSHelper.DefaultPAId;

            InitFeatures(db, itemTypeId, null);
            Locations = new List<LocationViewModel>();
            ImageSet = new ImageCollectionViewModel(1);
            Publishes = new List<StyleItemPublishViewModel>();
            StyleItems = new StyleItemCollection()
            {
                DisplayMode = Type == (int)StyleTypes.References ? StyleItemDisplayMode.StandardNoActions : StyleItemDisplayMode.Standard,
                Items = new List<StyleItemViewModel>()
            };
        }

        public StyleViewModel(IUnitOfWork db, 
            IMarketplaceService marketplaceService,
            StyleEntireDto style)
        {
            var itemTypeId = style.ItemTypeId ?? DefaultItemType;

            Id = style.Id;

            StyleId = style.StyleID;
            OriginalStyleId = style.OriginalStyleID;
            Type = style.Type;

            DropShipperId = style.DropShipperId;

            DisplayMode = style.DisplayMode;
            OnHold = style.OnHold;
            RemovePriceTag = style.RemovePriceTag;

            FillingStatus = style.FillingStatus;

            PictureStatus = style.PictureStatus;
            PictureStatusUpdateDate = style.PictureStatusUpdateDate;

            Name = style.Name;

            ItemTypeId = itemTypeId;
            Manufacturer = style.Manufacturer;
            MSRP = style.MSRP ?? 0;

            Price = style.Price ?? 0;
            SearchTerms = style.SearchTerms;
            BulletPoint1 = style.BulletPoint1;
            BulletPoint2 = style.BulletPoint2;
            BulletPoint3 = style.BulletPoint3;
            BulletPoint4 = style.BulletPoint4;
            BulletPoint5 = style.BulletPoint5;

            Description = style.Description;

            Comment = style.Comment;
            CommentUpdateDate = style.CommentUpdateDate;

            Image = style.Image;
            var images = db.StyleImages.GetAllAsDto()
                .Where(im => im.StyleId == style.Id && !im.IsSystem)
                .OrderBy(im => im.Id)
                .ToList();

            var wrongImages = GetWrongImages(images.Where(x=>x.Image.Contains(".jpg")).ToList());

            ImageSet = new ImageCollectionViewModel(1);
            ImageSet.SetImages(images);           
            InitFeatures(db, itemTypeId, style.Id);
            InitStyleItems(db, style.Id);
            InitDS(db, StyleItems.Items.FirstOrDefault()?.Id, style.DropShipperId);

            InitPublishes(db, marketplaceService, style.Id);

            Locations = GetLocations(db, style.Id);
        }

        private IList<StyleImageDTO> GetWrongImages(IList<StyleImageDTO> allImages)
        {            
            var res1 = allImages.AsParallel<StyleImageDTO>().Where(x => {
                using (var s = ImageHelper.DownloadRemoteImageFileAsStream(x.Image))
                {
                    if (s != null)
                    {
                        var i = ImageHelper.GetImageFromStream(s);
                        return !ImageHelper.IsImageSrgbJpeg(i);
                    }
                    else
                    {
                        return true;
                    }                    
                }; 
            }).ToList();
            return res1;
        }

        private static string GetImage(string path)
        {
            return !string.IsNullOrEmpty(path)
                ? path.Contains("http")
                    ? path
                    : UrlManager.UrlService.GetAbsolutePath(path)
                : string.Empty;
        }

        public void Apply(IUnitOfWork db,
            ILogService log,
            ICacheService cache,
            IQuantityManager quantityManager,
            IPriceManager priceManager,
            IBarcodeService barcodeService,
            ISystemActionService actionsService,
            IStyleHistoryService styleHistoryService,
            IAutoCreateListingService createListingService,
            bool applyItemQty,
            DateTime when,
            long? by)
        {
            CleanupData(StyleItems.Items);

            var exist = db.Styles.GetFiltered(s => s.Id == Id && !s.Deleted).FirstOrDefault();

            if (exist == null)
            {
                exist = AddStyle(db, 
                    log,
                    quantityManager, 
                    priceManager,
                    barcodeService,
                    styleHistoryService, 
                    createListingService,
                    applyItemQty, 
                    when, 
                    by);
            }
            else
            {
                UpdateStyle(db, 
                    log, 
                    quantityManager, 
                    priceManager,
                    barcodeService,
                    actionsService, 
                    styleHistoryService, 
                    createListingService,
                    exist, 
                    applyItemQty, 
                    when, 
                    by);
            }

            cache.RequestStyleIdUpdates(db,
                new List<long> { exist.Id },
                UpdateCacheMode.IncludeChild,
                AccessManager.UserId);
        }

        public static void SetOnHold(IUnitOfWork db,
            IStyleHistoryService styleHistorySerivce,
            ISystemActionService actionService,
            long id,
            bool onHoldStatus,
            DateTime when,
            long? by)
        {
            var style = db.Styles.Get(id);

            styleHistorySerivce.AddRecord(id, StyleHistoryHelper.OnHoldKey, style.OnHold, onHoldStatus, by);

            style.OnHold = onHoldStatus;
            //style.OnHoldUpdateDate = when;
            //style.OnHoldUpdatedBy = by;
            db.Commit();
            
            SystemActionHelper.RequestQuantityDistribution(db, actionService, id, by);
        }

        public static List<MessageString> Validate(IUnitOfWork db,
            string styleString,
            long? styleId,
            IList<StyleItemViewModel> styleItems)
        {
            var results = new List<MessageString>();

            CleanupData(styleItems);
            FixupBarcodes(styleItems);

            //Checking Duplicates
            var allBarcodes = GetAllBarcodes(styleItems);

            var selfDuplicates = allBarcodes.GroupBy(b => b).Where(b => b.Count() > 1).Select(b => b.Key).ToList()
                .Select(b => new BarcodeDTO()
                {
                    Barcode = b,
                    StyleId = styleString
                });

            var duplicates = db.StyleItemBarcodes.CheckOnDuplications(allBarcodes, styleId).ToList();
            duplicates.AddRange(selfDuplicates);

            if (duplicates.Any())
            {
                results.AddRange(duplicates
                    .Select(b => MessageString.Error("Barcode already exist: " + b.Barcode + " (used with style: " + b.StyleId + ")"))
                    .ToList());
            }

            foreach (var barcode in allBarcodes)
            {
                if (!String.IsNullOrEmpty(barcode) &&
                    !BarcodeHelper.IsValidBarcode(barcode))
                    results.Add(MessageString.Error("Invalid barcode format: " + barcode));
            }

            return results;
        }


        

        public string GenerateWalmart(IUnitOfWork db,
            IBarcodeService barcodeService,
            DateTime when, 
            out List<MessageString> messages)
        {
            string fileName;
            var items = ExcelProductWalmartViewModel.GenerateToExcelWalmart(db, barcodeService, this, when, out fileName);

            var isValid = ItemExportHelper.Validate(items.OfType<IExcelProductViewModel>().ToList(),
                out messages);
            if (isValid)
            {
                var stream = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelProductWalmartViewModel.TemplatePath),
                    "Dresses",
                    items);
                var filePath = UrlManager.UrlService.GetProductTemplateFilePath(fileName);
                using (var file = File.Open(filePath, FileMode.Create))
                {
                    stream.WriteTo(file);
                }
                return UrlManager.UrlService.GetProductTemplateUrl(fileName);
            }
            else
            {
                return null;
            }
        }

        public string GenerateUS(IUnitOfWork db, 
            IBarcodeService barcodeService,
            IMarketCategoryService categoryService,
            DateTime when, 
            out List<MessageString> messages)
        {
            string fileName;
            var items = ExcelProductUSViewModel.GenerateToExcelUS(db, 
                barcodeService,
                categoryService,
                this, 
                when, 
                out fileName);

            var isValid = ItemExportHelper.Validate(items.OfType<IExcelProductViewModel>().ToList(),
                out messages);
            if (isValid)
            {
                var stream = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelProductUSViewModel.USTemplatePath),
                    "Template",
                    items);
                var filePath = UrlManager.UrlService.GetProductTemplateFilePath(fileName);
                using (var file = File.Open(filePath, FileMode.Create))
                {
                    stream.WriteTo(file);
                }
                return UrlManager.UrlService.GetProductTemplateUrl(fileName);
            }
            else
            {
                return null;
            }
        }

        public string GenerateUK(IUnitOfWork db, 
            IBarcodeService barcodeService,
            IMarketCategoryService categoryService,
            DateTime when, 
            out List<MessageString> messages)
        {
            string fileName;
            var items = ExcelProductUKViewModel.GenerateToExcelUK(db, barcodeService, categoryService, this, when, out fileName);

            var isValid = ItemExportHelper.Validate(items.OfType<IExcelProductViewModel>().ToList(),
                out messages);
            if (isValid)
            {
                var stream = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelProductUKViewModel.UKTemplatePath),
                    "Template",
                    items);
                var filePath = UrlManager.UrlService.GetProductTemplateFilePath(fileName);
                using (var file = File.Open(filePath, FileMode.Create))
                {
                    stream.WriteTo(file);
                }
                return UrlManager.UrlService.GetProductTemplateUrl(fileName);
            }
            else
            {
                return null;
            }
        }

        private void UpdateStyle(IUnitOfWork db,
            ILogService log,
            IQuantityManager quantityManager,
            IPriceManager priceManager,
            IBarcodeService barcodeService,
            ISystemActionService actionService,
            IStyleHistoryService styleHistoryService,
            IAutoCreateListingService createListingService,
            Style dbStyle, 
            bool applyItemQty, 
            DateTime when, 
            long? by)
        {
            StyleViewModel.SetDefaultImage(ImageSet.Images);

            var itemTypeId = dbStyle.ItemTypeId ?? DefaultItemType;
            var dropShipperId = DropShipperId ?? DSHelper.DefaultPAId;
            var hasDescriptionChanges = false;

            dbStyle.StyleID = StyleId;
            dbStyle.OriginalStyleID = OriginalStyleId;

            if (dbStyle.DropShipperId != dropShipperId)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.DropShipperKey, dbStyle.DropShipperId, dropShipperId, by);
            }
            dbStyle.DropShipperId = dropShipperId;

            if (dbStyle.OnHold != OnHold)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.OnHoldKey, dbStyle.OnHold, OnHold, by);

                dbStyle.OnHold = OnHold;
                //dbStyle.OnHoldUpdateDate = when;
                //dbStyle.OnHoldUpdatedBy = by;

                SystemActionHelper.RequestQuantityDistribution(db, actionService, dbStyle.Id, by);
            }

            dbStyle.DisplayMode = IsHidden ? (int) DisplayModes.Hidden : (int) DisplayModes.Visible;
            if (dbStyle.PictureStatus != PictureStatus)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.PictureStatusKey, dbStyle.PictureStatus, PictureStatus, by);

                dbStyle.PictureStatusUpdateDate = when;
                dbStyle.PictureStatusUpdatedBy = by;
            }
            dbStyle.PictureStatus = PictureStatus;

            if (dbStyle.FillingStatus != FillingStatus)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.FillingStatusKey, dbStyle.FillingStatus, FillingStatus, by);
            }
            dbStyle.FillingStatus = FillingStatus;

            dbStyle.RemovePriceTag = RemovePriceTag;
            dbStyle.Name = Name;
            dbStyle.ItemTypeId = ItemTypeId;
            dbStyle.Manufacturer = Manufacturer;
            dbStyle.MSRP = MSRP;
            dbStyle.Price = Price;

            hasDescriptionChanges = dbStyle.SearchTerms != SearchTerms
                    || dbStyle.BulletPoint1 != BulletPoint1
                    || dbStyle.BulletPoint2 != BulletPoint2
                    || dbStyle.BulletPoint3 != BulletPoint3
                    || dbStyle.BulletPoint4 != BulletPoint4
                    || dbStyle.BulletPoint5 != BulletPoint5;

            dbStyle.SearchTerms = SearchTerms;
            dbStyle.BulletPoint1 = BulletPoint1;
            dbStyle.BulletPoint2 = BulletPoint2;
            dbStyle.BulletPoint3 = BulletPoint3;
            dbStyle.BulletPoint4 = BulletPoint4;
            dbStyle.BulletPoint5 = BulletPoint5;

            

            if (dbStyle.Comment != Comment)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.CommentKey, dbStyle.Comment, Comment, by);
                dbStyle.CommentUpdateDate = when;
                dbStyle.CommentUpdatedBy = by;
            }
            dbStyle.Comment = Comment;

            dbStyle.Description = Description;

            var newImage = ImageSet.GetMainImageUrl();
            if (dbStyle.Image != newImage)
            {
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.PictureKey, dbStyle.Image, newImage, by);
                dbStyle.Image = newImage;
            }

            dbStyle.UpdateDate = when;
            dbStyle.UpdatedBy = by;

            dbStyle.ReSaveDate = when;
            dbStyle.ReSaveBy = by;

            var hasImageChanges = UpdateImages(db, dbStyle.Id, Images, when, by);

            UpdateLocations(db, styleHistoryService, dbStyle.Id, Locations, when, by);

            var hasFeatureChanges = UpdateFeatures(db, dbStyle.Id, when, by);

            InitFeatures(db, itemTypeId, dbStyle.Id);

            UpdateStyleItems(db, quantityManager, barcodeService, dbStyle.Id, applyItemQty, when, by);

            UpdatePublishes(db, log, createListingService, priceManager, when, by);

            db.Commit();

            if (hasImageChanges || hasDescriptionChanges || hasFeatureChanges)
            {
                RequestListingsResubmitUpdates(db, log, actionService, dbStyle.Id, by);
            }

            CreateDate = dbStyle.CreateDate;
        }

        private Style AddStyle(IUnitOfWork db, 
            ILogService log,
            IQuantityManager quantityManager,
            IPriceManager priceManager,
            IBarcodeService barcodeService,
            IStyleHistoryService styleHistoryService,
            IAutoCreateListingService createListingService,
            bool applyItemQty, 
            DateTime when, 
            long? by)
        {
            StyleViewModel.SetDefaultImage(ImageSet.Images);

            var itemTypeId = ItemTypeId ?? DefaultItemType;
            var dropShipperId = DropShipperId ?? DSHelper.DefaultPAId;

            var dbStyle = new Style
            {
                StyleID = StyleId,
                OriginalStyleID = OriginalStyleId,
                Type = (int)StyleTypes.Default,

                DropShipperId = dropShipperId,

                OnHold = OnHold,
                DisplayMode = IsHidden ? (int)DisplayModes.Hidden : (int)DisplayModes.Visible,
                RemovePriceTag = RemovePriceTag,

                FillingStatus = FillingStatus,

                PictureStatus = PictureStatus,
                PictureStatusUpdateDate = when,
                PictureStatusUpdatedBy = by,

                ItemTypeId = itemTypeId,
                Name = Name,
                Manufacturer = Manufacturer,
                MSRP = MSRP,
                Price = Price,
                SearchTerms = SearchTerms,
                BulletPoint1 = BulletPoint1,
                BulletPoint2 = BulletPoint2,
                BulletPoint3 = BulletPoint3,
                BulletPoint4 = BulletPoint4,
                BulletPoint5 = BulletPoint5,

                Description = Description,

                Comment = Comment,
                CommentUpdateDate = when,
                CommentUpdatedBy = by,

                Image = ImageSet.GetMainImageUrl(),

                CreateDate = when,
                CreatedBy = by,

                ReSaveDate = when,
                ReSaveBy = by,
            };

            db.Styles.Add(dbStyle);
            db.Commit();
            //Get values to view
            Id = dbStyle.Id;
            CreateDate = dbStyle.CreateDate;

            if (dbStyle.PictureStatus != (int)StylePictureStatuses.None)
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.PictureStatusKey, StylePictureStatuses.None, dbStyle.PictureStatus, by);

            if (dbStyle.FillingStatus != (int)FillingStyleStatuses.None)
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.FillingStatusKey, FillingStyleStatuses.None, dbStyle.FillingStatus, by);


            if (!String.IsNullOrEmpty(dbStyle.Image))
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.PictureKey, "", dbStyle.Image, by);

            if (dbStyle.OnHold)
                styleHistoryService.AddRecord(dbStyle.Id, StyleHistoryHelper.OnHoldKey, false, dbStyle.OnHold, by);

            UpdateImages(db, dbStyle.Id, ImageSet.Images, when, by);

            UpdateLocations(db, styleHistoryService, dbStyle.Id, Locations, when, by);
            UpdateFeatures(db, dbStyle.Id, when, by);
            InitFeatures(db, itemTypeId, dbStyle.Id);

            AddStyleItems(db, quantityManager, barcodeService, dbStyle, applyItemQty, when, by);

            UpdatePublishes(db, log, createListingService, priceManager, when, by);

            return dbStyle;
        }

        #region Images

        private void RequestListingsResubmitUpdates(IUnitOfWork db,
            ILogService log, 
            ISystemActionService systemAction,
            long styleId,
            long? by)
        {
            var itemsToUpdate = db.Items.GetAll().Where(i => i.StyleId == styleId
                                                                && (i.Market == (int) MarketType.Walmart
                                                                    || i.Market == (int)MarketType.WalmartCA
                                                                    || i.Market == (int) MarketType.eBay
                                                                    || i.Market == (int) MarketType.Jet
                                                                    //|| i.Market == (int) MarketType.Amazon
                                                                    )
                                                                && (i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited
                                                                    || i.ItemPublishedStatus == (int)PublishedStatuses.Published)).ToList();

            log.Info("Update requested, for listings: " + itemsToUpdate.Count);

            itemsToUpdate.ForEach(l => l.ItemPublishedStatus = (int)PublishedStatuses.HasChanges);
            db.Commit();

            //var amazonItemIds = itemsToUpdate.Where(i => i.Market == (int)MarketType.Amazon)
            //    .Select(i => (long)i.Id)
            //    .ToList();

            //SystemActionHelper.RequestImageUpdates(db, systemAction, amazonItemIds, by);
        }

        public void SetUploadedImages(IList<SessionHelper.UploadedFileInfo> images)
        {
            if (images == null)
                return;

            //Set absolute pathes
            foreach (var img in images)
            {
                if (!(img.FileName ?? "").StartsWith("http"))
                {
                    img.FileName = UrlManager.UrlService.GetAbsolutePath(UrlManager.UrlService.GetUploadImageUrl(img.FileName));
                }
            }

            ImageSet.SetUploadedImageUrls(images);
        }

        public static void SetDefaultImage(IList<ImageViewModel> images)
        {
            //NOTE: Set Swatch image as default
            var hasSwatch = images.Any(i => i.Category == (int)StyleImageCategories.Swatch);
            if (hasSwatch)
            {
                images.ForEach(i => i.IsDefault = i.Category == (int)StyleImageCategories.Swatch);
            }
        }

        public static bool UpdateImages(IUnitOfWork db,
            long styleId,
            IList<ImageViewModel> images,
            DateTime when,
            long? by)
        {
            //Keep only one default element
            var existDef = false;
            var imageList = images.Where(im => !String.IsNullOrEmpty(im.ImageUrl))
                .Select(l => new StyleImageDTO()
                {
                    Id = l.Id,
                    IsDefault = l.IsDefault,
                    Category = l.Category,
                    Image = l.ImageUrl,
                }).ToList();

            //Check isDefault
            foreach (var img in imageList)
            {
                if (img.IsDefault)
                {
                    if (existDef)
                        img.IsDefault = false;
                    else
                        existDef = true;
                }
            }
            if (!existDef && imageList.Count > 0)
                imageList[0].IsDefault = true;

            var changes = db.StyleImages.UpdateImagesForStyle(styleId,
                imageList,
                when,
                by);

            return changes.Any(ch => ch.Status != UpdateType.None);
        }

        #endregion

        #region DS

        private void InitDS(IUnitOfWork db, long? styleItemId, long? dropShipperId)
        {
            if (dropShipperId.HasValue)
            {
                var dropShipper = db.DropShippers.GetAllAsDto().FirstOrDefault(ds => ds.Id == dropShipperId.Value);
                if (dropShipper != null)
                {
                    DropShipperName = dropShipper.Name;
                }
            }
        }

        #endregion

        #region Location

        public static List<LocationViewModel> GetLocations(IUnitOfWork db, long styleId)
        {
            if (styleId != 0)
            {
                var locations = db.StyleLocations.GetByStyleId(styleId).ToList();

                return locations.Select(l => new LocationViewModel(l))
                    .OrderByDescending(l => l.IsDefault)
                    .ThenBy(l => l.Isle)
                    .ThenBy(l => l.Section)
                    .ThenBy(l => l.Shelf)
                    .ToList();
            }
            return new List<LocationViewModel>();
        }



        public static void UpdateLocations(IUnitOfWork db,
            IStyleHistoryService styleHistory,
            long styleId,
            IList<LocationViewModel> locations,
            DateTime when,
            long? by)
        {
            //Keep only one default element
            var existDef = false;
            var locationList = locations.Where(l => !l.IsEmpty())
                .Select(l => new StyleLocationDTO()
                {
                    Id = l.Id,
                    IsDefault = l.IsDefault,
                    Isle = l.Isle,
                    Section = l.Section,
                    Shelf = l.Shelf,
                    SortIsle = StringHelper.GetFirstDigitSequences(l.Isle) ?? 0,
                    SortSection = StringHelper.GetFirstDigitSequences(l.Section) ?? 0,
                    SortShelf = StringHelper.GetFirstDigitSequences(l.Shelf) ?? 0
                }).ToList();

            foreach (var loc in locationList)
            {
                if (loc.IsDefault)
                {
                    if (existDef)
                        loc.IsDefault = false;
                    else
                        existDef = true;
                }
            }
            if (!existDef && locationList.Count > 0)
                locations[0].IsDefault = true;

            db.StyleLocations.UpdateLocationsForStyle(styleHistory,
                styleId,
                locationList,
                when,
                by);
        }

        #endregion

        #region Features

        private void InitFeatures(IUnitOfWork db, int itemTypeId, long? styleId)
        {
            //TODO: Temp override itemTypeId
            itemTypeId = DefaultItemType;

            Features = FeatureViewModel.BuildFrom(db.Features.GetByItemType(itemTypeId),
                db.FeatureValues.GetAllFeatureValueByItemType(itemTypeId),
                styleId.HasValue ? db.StyleFeatureValues.GetAllFeatureValuesByStyleIdAsDto(new List<long>() { styleId.Value }) : new List<StyleFeatureValueDTO>(),
                styleId.HasValue ? db.StyleFeatureTextValues.GetAllFeatureTextValuesByStyleIdAsDto(new List<long>() { styleId.Value }) : new List<StyleFeatureValueDTO>());

            WMKeywords = Features.FirstOrDefault(f => f.Name == StyleFeatureHelper.WMKeywordsName)?.Value;
            Features = Features.Where(f => f.Name != StyleFeatureHelper.WMKeywordsName).ToList();
        }

        public void ReInitFeatures(IUnitOfWork db)
        {
            //TODO: Temp override itemTypeId
            var itemTypeId = DefaultItemType;

            Features = FeatureViewModel.BuildFrom(
                db.Features.GetByItemType(itemTypeId),
                db.FeatureValues.GetAllFeatureValueByItemType(itemTypeId),
                Features.Where(f => f.ValueAsInt.HasValue && (f.Type == (int)FeatureValuesType.DropDown || f.Type == (int)FeatureValuesType.CacadeDropDown))
                    .Select(f => new StyleFeatureValueDTO()
                    {
                        FeatureId = f.FeatureId,
                        FeatureValueId = f.ValueAsInt.Value
                    }).ToList(),
                Features.Where(f => (f.Type == (int)FeatureValuesType.TextBox || f.Type == (int)FeatureValuesType.CheckBox))
                    .Select(f => new StyleFeatureValueDTO()
                    {
                        FeatureId = f.FeatureId,
                        Value = f.Value
                    }).ToList()
            );
        }

        private bool UpdateFeatures(IUnitOfWork db,
            long styleId,
            DateTime? when,
            long? by)
        {
            var featureValues = Features.Select(f => new StyleFeatureValueDTO()
            {
                FeatureId = f.FeatureId,
                FeatureValueId = f.ValueAsInt,
                Value = f.Value,
                StyleId = styleId,
                Type = f.Type,
            }).ToList();

            var wmKeywordsFeature = db.Features.GetAll().FirstOrDefault(f => f.Name == StyleFeatureHelper.WMKeywordsName);
            if (wmKeywordsFeature != null)
            {
                featureValues.Add(new StyleFeatureValueDTO()
                {
                    FeatureId = wmKeywordsFeature.Id,
                    Value = WMKeywords,
                    StyleId = styleId,
                    Type = wmKeywordsFeature.ValuesType
                });
            }

            return db.StyleFeatureValues.UpdateFeatureValues(styleId, featureValues, when, by);
        }

        #endregion

        #region Sizes

        private void InitStyleItems(IUnitOfWork db, long? styleId)
        {
            if (styleId.HasValue)
            {
                var styleItems = db.StyleItems.GetByStyleIdWithBarcodesAsDto(styleId.Value)
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .ToList();
                StyleItems = new StyleItemCollection()
                {
                    DisplayMode = Type == (int)StyleTypes.References ? StyleItemDisplayMode.StandardNoActions : StyleItemDisplayMode.Standard,
                    Items = styleItems.Select(si => new StyleItemViewModel(si)).ToList()
                };

                StyleItems.Items.ForEach(si => si.Quantity = 5); //Set for Generate Excel default 5
            }
        }

        private void PreGenerateBarcodes(IBarcodeService barcodeService, DateTime when)
        {
            foreach (var styleItem in StyleItems.Items)
            {
                if (styleItem.AutoGeneratedBarcode)
                {
                    var sku = StyleId + "-" + ItemExportHelper.ConvertSizeForStyleId(styleItem.Size, false);
                    var newBarcode = BarcodeHelper.GenerateBarcode(barcodeService, sku, when);
                    if (!String.IsNullOrEmpty(newBarcode))
                    {
                        if (styleItem.Barcodes == null)
                            styleItem.Barcodes = new List<BarcodeDTO>();
                        styleItem.Barcodes.Insert(0, new BarcodeDTO()
                        {
                            Barcode = newBarcode
                        });
                    }
                }
            }
        }

        private void InitPublishes(IUnitOfWork db, IMarketplaceService marketplaceService, long? styleId)
        {
            Publishes = new List<StyleItemPublishViewModel>();
            var markets = marketplaceService.GetAllWithVirtual();

            var styleListings = styleId.HasValue ? db.Items.GetAllViewAsDto().Where(i => i.StyleId == styleId).ToList()
                : new List<ItemDTO>();

            foreach (var market in markets)
            {
                var styleListing = styleListings.FirstOrDefault(l => l.Market == market.Market
                                                          && (l.MarketplaceId == market.MarketplaceId
                                                              || String.IsNullOrEmpty(market.MarketplaceId))
                                                        //NOTE: exclude eBay unpublished listings (we can't republish them)
                                                        && !(l.Market == (int)MarketType.eBay
                                                        && l.PublishedStatus == (int)PublishedStatuses.Unpublished));

                Publishes.Add(new StyleItemPublishViewModel()
                {
                    Market = market.Market,
                    MarketplaceId = market.MarketplaceId,
                    Price = styleListing != null ? styleListing.CurrentPrice : (decimal?)null,
                    IsPublished = styleListing != null
                    //&& 
                    //    (styleListing.PublishedStatus == (int)PublishedStatuses.Published
                    //    || styleListing.PublishedStatus == (int)PublishedStatuses.New
                    //    || styleListing.PublishedStatus == (int)PublishedStatuses.HasPublishRequest
                    //    || styleListing.PublishedStatus == (int)PublishedStatuses.),
                });
            }
        }

        private void UpdateStyleItems(IUnitOfWork db,
            IQuantityManager quantityManager,
            IBarcodeService barcodeService,
            long styleId,
            bool applyItemQty,
            DateTime when,
            long? by)
        {
            PreGenerateBarcodes(barcodeService, when);

            var styleItems = StyleItems.Items.Select(si => new StyleItemDTO()
            {
                StyleItemId = si.Id,
                Size = si.Size,
                SizeId = si.SizeId,
                Color = si.Color,
                Weight = si.Weight,
                Barcodes = si.Barcodes,

                PackageWidth = si.PackageWidth,
                PackageLength = si.PackageLength,
                PackageHeight = si.PackageHeight,

                Quantity = si.Quantity,
                QuantitySetDate = when,
                QuantitySetBy = by,
            }).ToList();

            //NOTE: if Style Item has BoxQuantity or InventoryQuantity
            //the entered qty from Generate Style page not used / ignored
            var styleItemCaches = db.StyleItemCaches.GetForStyleId(styleId);
            foreach (var styleItem in styleItems)
            {
                var styleItemCache = styleItem.StyleItemId > 0 ? styleItemCaches.FirstOrDefault(sic => sic.Id == styleItem.StyleItemId) : null;

                if (!applyItemQty 
                    || (styleItemCache != null
                        && (styleItemCache.InventoryQuantity > 0
                        || styleItemCache.BoxQuantity > 0
                        || styleItemCache.TotalMarketsSoldQuantity > 0))) //NOTE: exclude from set Qty StyleSizes that already had qty
                {
                    styleItem.Quantity = null;
                    styleItem.QuantitySetDate = null;
                    styleItem.QuantitySetBy = null;
                }
            }

            var updateResults = db.StyleItems.UpdateStyleItemsForStyle(styleId, styleItems, when, by);

            //NOTE: Log qty changes (previous always = null/0)
            foreach (var updateResult in updateResults)
            {
                var styleItem = styleItems.FirstOrDefault(si => si.StyleItemId == updateResult.Id);
                if (styleItem != null)
                {
                    var qtyStatus = QuantityChangeSourceType.None;
                    if (updateResult.Status == UpdateType.Update && styleItem.Quantity > 0)
                        qtyStatus = QuantityChangeSourceType.EnterNewQuantity;
                    if (updateResult.Status == UpdateType.Insert) //NOTE: can be NULL or box qty
                        qtyStatus = QuantityChangeSourceType.Initial;
                    if (updateResult.Status == UpdateType.Removed)
                        qtyStatus = QuantityChangeSourceType.Removed;

                    if (qtyStatus != QuantityChangeSourceType.None)
                    {
                        quantityManager.LogStyleItemQuantity(db,
                            styleItem.StyleItemId,
                            styleItem.Quantity,
                            null,
                            qtyStatus,
                            null,
                            null,
                            null,
                            when,
                            by);
                    }
                }
            }

            foreach (var styleItem in styleItems)
            {
                var barcodeList = styleItem.Barcodes.Where(b => !String.IsNullOrEmpty(b.Barcode)).ToList();
                db.StyleItemBarcodes.UpdateStyleItemBarcodeForStyleItem(styleItem.StyleItemId,
                            barcodeList,
                            when,
                            by);
            }
        }

        private void UpdatePublishes(IUnitOfWork db,
            ILogService log,
            IAutoCreateListingService listingCreateService,
            IPriceManager priceManager,
            DateTime when,
            long? by)
        {
            var existItems = db.Items.GetAll().Where(i => i.StyleId == Id).ToList();
            foreach (var publishInfo in Publishes)
            {
                publishInfo.MarketplaceId = publishInfo.MarketplaceId ?? "";

                var existMarketItems = existItems.Where(l => l.Market == publishInfo.Market
                                                                           && (l.MarketplaceId == publishInfo.MarketplaceId
                                                                            || String.IsNullOrEmpty(publishInfo.MarketplaceId))

                                                                           //NOTE: exclude eBay unpublished listings (we can't republish them)
                                                                           && !(l.Market == (int)MarketType.eBay
                                                                            && l.ItemPublishedStatus == (int)PublishedStatuses.Unpublished));

                if (!existMarketItems.Any())
                {
                    if (publishInfo.IsPublished && publishInfo.Price.HasValue)
                    {
                        log.Info("Request create listing, market=" + publishInfo.Market + ", marketplaceId=" +
                                 publishInfo.MarketplaceId);

                        IList<MessageString> messages = new List<MessageString>();
                        //Create New
                        var model = listingCreateService.CreateFromStyle(db,
                            StyleId,
                            publishInfo.Price.Value,
                            (MarketType)publishInfo.Market,
                            publishInfo.MarketplaceId,
                            out messages);

                        model.Variations.ForEach(v => v.CurrentPrice = publishInfo.Price.Value);

                        listingCreateService.Save(model,
                            "",
                            db,
                            when,
                            by);
                    }
                }
                else
                {
                    foreach (var existMarketItem in existMarketItems)
                    {
                        if (!publishInfo.IsPublished
                            && (existMarketItem.ItemPublishedStatus != (int)PublishedStatuses.Unpublished
                                && existMarketItem.ItemPublishedStatus != (int)PublishedStatuses.HasUnpublishRequest))
                        {
                            log.Info("Request listing unpublishing: ASIN=" + existMarketItem.ASIN);
                            existMarketItem.ItemPublishedStatus = (int)PublishedStatuses.HasUnpublishRequest;
                            db.Commit();
                        }
                        if (publishInfo.IsPublished
                            && (existMarketItem.ItemPublishedStatus == (int)PublishedStatuses.Unpublished
                                || existMarketItem.ItemPublishedStatus == (int)PublishedStatuses.HasUnpublishRequest))
                        {
                            log.Info("Request exist listing publishing: ASIN=" + existMarketItem.ASIN);
                            existMarketItem.ItemPublishedStatus = (int)PublishedStatuses.HasChanges;
                            db.Commit();

                            var existMarketListing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == existMarketItem.Id);
                            if (existMarketListing != null 
                                && !existMarketListing.IsFBA
                                && !existMarketListing.IsPrime
                                && publishInfo.Price.HasValue)
                            {
                                if (existMarketListing.CurrentPrice != publishInfo.Price.Value)
                                {
                                    log.Info("Price was changed: " + existMarketListing.CurrentPrice
                                            + " => " + publishInfo.Price.Value);

                                    priceManager.LogListingPrice(db, PriceChangeSourceType.EnterNewPrice,
                                        existMarketListing.Id,
                                        existMarketListing.SKU,
                                        publishInfo.Price.Value,
                                        existMarketListing.CurrentPrice,
                                        when,
                                        by);

                                    existMarketListing.CurrentPrice = publishInfo.Price.Value;
                                    existMarketListing.PriceUpdateRequested = true;
                                    db.Commit();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static List<string> GetAllBarcodes(IList<StyleItemViewModel> items)
        {
            var allBarcodes = new List<string>();
            foreach (var styleItem in items)
            {
                allBarcodes.AddRange(styleItem.Barcodes.Where(b => !string.IsNullOrEmpty(b.Barcode)).Select(b => b.Barcode));
            }
            return allBarcodes;
        }

        private static void CleanupData(IList<StyleItemViewModel> items)
        {
            foreach (var styleItem in items)
            {
                if (styleItem.Barcodes != null)
                {
                    foreach (var barcode in styleItem.Barcodes)
                    {
                        if (!String.IsNullOrEmpty(barcode.Barcode))
                            barcode.Barcode = barcode.Barcode.Trim();
                    }
                }
            }
        }

        private static void FixupBarcodes(IList<StyleItemViewModel> items)
        {
            foreach (var styleItem in items)
            {
                for (int i = 0; i < styleItem.Barcodes.Count; i++)
                {
                    styleItem.Barcodes[i].Barcode = BarcodeHelper.FixupBarcode(styleItem.Barcodes[i].Barcode);
                }
            }
        }

        private void AddStyleItems(IUnitOfWork db,
            IQuantityManager quantityManager,
            IBarcodeService barcodeService,
            Style dbStyle,
            bool applyItemQty,
            DateTime when,
            long? by)
        {
            PreGenerateBarcodes(barcodeService, when);

            var barcodes = GetAllBarcodes(StyleItems.Items);

            var duplicates = db.StyleItems.GetDuplicateList(barcodes);

            foreach (var styleItem in StyleItems.Items)
            {
                var item = new StyleItem
                {
                    StyleId = dbStyle.Id,
                    Weight = styleItem.Weight,

                    Size = styleItem.Size,
                    SizeId = styleItem.SizeId,
                    Color = styleItem.Color,

                    CreateDate = when,
                    CreatedBy = by
                };

                if (applyItemQty && styleItem.Quantity.HasValue && styleItem.Quantity > 0)
                {
                    item.Quantity = styleItem.Quantity;
                    item.QuantitySetBy = by;
                    item.QuantitySetDate = when;
                }

                db.StyleItems.Add(item);
                db.Commit();

                quantityManager.LogStyleItemQuantity(db,
                    item.Id,
                    item.Quantity,
                    null,
                    item.Quantity.HasValue ? QuantityChangeSourceType.Initial : QuantityChangeSourceType.UseBoxQuantity, 
                    null,
                    null,
                    null,
                    when,
                    by);

                foreach (var barcode in styleItem.Barcodes.Where(b => !string.IsNullOrEmpty(b.Barcode)))
                {
                    if (!duplicates.Contains(barcode.Barcode)) //Exclude duplicate barcode (in case of force save/without check duplication)
                    {
                        db.StyleItemBarcodes.Add(new StyleItemBarcode
                        {
                            StyleItemId = item.Id,
                            Barcode = barcode.Barcode,

                            CreateDate = when,
                            CreatedBy = by
                        });
                    }
                }
            }

            db.Commit();
        }
        #endregion

        public static IList<long> GetIdListByFilters(IUnitOfWork db,
            StyleSearchFilterViewModel filter)
        {
            if (!String.IsNullOrEmpty(filter.Barcode))
            {
                var query = from b in db.StyleItemBarcodes.GetAll()
                            join si in db.StyleItems.GetAll() on b.StyleItemId equals si.Id
                            where b.Barcode.Contains(filter.Barcode)
                            select si.StyleId;

                return query.ToList();
            }
            IList<long> styleIdList = null;
            if (filter.Gender.HasValue)
                styleIdList = db.StyleFeatureValues.GetAll()
                    .Where(f => f.FeatureValueId == filter.Gender.Value)
                    .Select(f => f.StyleId)
                    .ToList();

            if (filter.ItemStyles != null && filter.ItemStyles.Any())
            {
                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList,
                    db.StyleFeatureValues.GetAll()
                    .Where(f => filter.ItemStyles.Contains(f.FeatureValueId))
                    .Select(f => f.StyleId)
                    .ToList());
            }

            if (filter.Sleeves != null && filter.Sleeves.Any())
            {
                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList,
                    db.StyleFeatureValues.GetAll()
                    .Where(f => filter.Sleeves.Contains(f.FeatureValueId))
                    .Select(f => f.StyleId)
                    .ToList());
            }

            if (filter.HolidayId.HasValue)
            {
                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList,
                    db.StyleFeatureValues.GetAll()
                    .Where(f => filter.HolidayId.Value == f.FeatureValueId)
                    .Select(f => f.StyleId)
                    .ToList());
            }

            if (filter.MainLicense.HasValue)
            {
                IList<long> styleByMainLicenseList = null;
                if (filter.MainLicense.Value == 0)
                {
                    var withMainLicense = from sfv in db.StyleFeatureValues.GetAll()
                                          where sfv.FeatureId == StyleFeatureHelper.MAIN_LICENSE
                                          select new
                                          {
                                              StyleId = sfv.StyleId
                                          };

                    styleByMainLicenseList = (from st in db.Styles.GetAllAsDto()
                                              join sfv in withMainLicense on st.Id equals sfv.StyleId into withFeature
                                              from sfv in withFeature.DefaultIfEmpty()
                                              where sfv.StyleId == null
                                              select st.Id).ToList();
                }
                else
                {
                    styleByMainLicenseList = db.StyleFeatureValues.GetAll()
                        .Where(f => f.FeatureValueId == filter.MainLicense.Value)
                        .Select(f => f.StyleId)
                        .ToList();
                }

                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList,
                    styleByMainLicenseList);
            }

            if (filter.SubLicense.HasValue)
            {
                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList,
                    db.StyleFeatureValues.GetAll()
                    .Where(f => f.FeatureValueId == filter.SubLicense.Value)
                    .Select(f => f.StyleId)
                    .ToList());
            }

            if (filter.HasInitialQty)
            {
                var styleItemsWithInitialQty = db.StyleItems.GetAllAsDto()
                    .Where(si => !si.QuantitySetBy.HasValue 
                    && si.Quantity.HasValue //NOTE: not use box qty, otherwise = NULL
                    //&& si.Quantity > 0
                    )
                    .Select(si => si.StyleId)
                    .Distinct()
                    .ToList();

                var woUsingBoxes = (from sic in db.StyleItemCaches.GetAllAsDto()
                                      join si in db.StyleItems.GetAllAsDto() on sic.Id equals si.StyleItemId
                                      where sic.BoxQuantity.HasValue
                                            && sic.BoxQuantity > 0
                                            && si.Quantity.HasValue
                                      select si.StyleId).Distinct().ToList();
                var usingBoxes = (from si in db.StyleItems.GetAllAsDto()
                                  where !si.Quantity.HasValue
                                  select si.StyleId).Distinct().ToList();

                woUsingBoxes = GeneralUtils.RemoveIntersect(woUsingBoxes, usingBoxes).ToList(); //NOTE: remove style if it partially uses boxes

                var resultList = styleItemsWithInitialQty;
                resultList.AddRange(woUsingBoxes);
                resultList = resultList.Distinct().ToList();

                styleIdList = GeneralUtils.IntersectIfNotNull(styleIdList, resultList);
            }

            return styleIdList;
        }

        public static CallMessagesResult<StyleItemViewModel> ChangeSize(IUnitOfWork db,
            ILogService log,
            ICacheService cacheService,
            long styleItemId,
            int newSizeId,
            DateTime when,
            long? by)
        {
            log.Info("ChangeSize, styleItemId=" + styleItemId + ", newSizeId=" + newSizeId);

            var styleItem = db.StyleItems.Get(styleItemId);
            var size = db.Sizes.Get(newSizeId);

            var currentSizeId = styleItem.SizeId;
            var currentSize = styleItem.Size;
            styleItem.SizeId = newSizeId;
            styleItem.Size = size.Name;

            db.Commit();

            db.StyleItemActionHistories.Add(new StyleItemActionHistory()
            {
                ActionName = StyleItemHistoryTypes.ChangeSize,
                StyleItemId = styleItemId,
                Data = currentSizeId.ToString() + ";" + currentSize + ";" + size.Id + ";" + size.Name,

                CreateDate = when,
                CreatedBy = by
            });
            db.Commit();

            db.Styles.SetReSaveDate(styleItem.StyleId, when, by);

            cacheService.RequestStyleIdUpdates(db,
                new List<long>()
                {
                    styleItem.StyleId,
                },
                UpdateCacheMode.IncludeChild,
                by);

            return new CallMessagesResult<StyleItemViewModel>()
            {
                Status = CallStatus.Success,
                Messages = new List<MessageString>(),
                Data = new StyleItemViewModel()
                {
                    Id = styleItemId,
                    SizeId = size.Id,
                    Size = size.Name
                }
            };
        }

        public static CallMessagesResult<IList<long>> RemoveStyleItems(IUnitOfWork db,
            ILogService log,
            ICacheService cacheService,
            IList<long> styleItemIdList,
            DateTime when,
            long? by)
        {
            log.Info("RemoveStyleItems, idList=" + String.Join(",", styleItemIdList));

            var styleIdList = new List<long>();
            foreach (var styleItemId in styleItemIdList)
            {
                var styleItem = db.StyleItems.Get(styleItemId);
                db.StyleItems.Remove(styleItem);
                styleIdList.Add(styleItem.StyleId);

                db.StyleItemActionHistories.Add(new StyleItemActionHistory()
                {
                    ActionName = StyleItemHistoryTypes.Remove,
                    StyleItemId = styleItemId,
                    Data = styleItem.SizeId + ";" + styleItem.Size,

                    CreateDate = when,
                    CreatedBy = by
                });
            }

            db.Commit();

            foreach (var styleId in styleIdList.Distinct().ToList())
            {
                db.Styles.SetReSaveDate(styleId, when, by);
            }

            cacheService.RequestStyleIdUpdates(db,
                styleItemIdList.Distinct().ToList(),
                UpdateCacheMode.IncludeChild,
                by);

            return new CallMessagesResult<IList<long>>()
            {
                Status = CallStatus.Success,
                Messages = new List<MessageString>(),
                Data = styleItemIdList
            };
        }

        public static CallMessagesResultVoid MergeStyleItems(IUnitOfWork db,
            ILogService log,
            ICacheService cacheService,
            IQuantityManager quantityManager,
            long toStyleItemId,
            long fromStyleItemId,
            DateTime when,
            long? by)
        {
            log.Info("Merging styleItems, to=" + toStyleItemId + ", from=" + fromStyleItemId);

            var fromStyleItem = db.StyleItems.Get(fromStyleItemId);
            var toStyleItem = db.StyleItems.Get(toStyleItemId);
            var toStyleItemCache = db.StyleItemCaches.Get(toStyleItemId);

            var toMoveListings = db.Items.GetAll().Where(i => i.StyleItemId == fromStyleItem.Id).ToList();
            var toMoveBarcodes = db.StyleItemBarcodes.GetAll().Where(b => b.StyleItemId == fromStyleItem.Id).ToList();
            var toMoveSpecialCases = db.QuantityChanges.GetAll().Where(b => b.StyleItemId == fromStyleItem.Id).ToList();

            var toMoveOrderItems = db.OrderItems.GetAll().Where(b => b.StyleItemId == fromStyleItem.Id
                || b.SourceStyleItemId == fromStyleItem.Id).ToList();
            var toMoveSourceOrderItems =
                db.OrderItemSources.GetAll().Where(b => b.StyleItemId == fromStyleItem.Id).ToList();

            var toMoveOpenBoxItems = db.OpenBoxItems
                        .GetAll()
                        .Where(b => b.StyleItemId == fromStyleItem.Id)
                        .ToList();

            var toMoveSealedBoxItems = db.SealedBoxItems
                .GetAll()
                .Where(b => b.StyleItemId == fromStyleItem.Id)
                .ToList();

            toMoveListings.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved listing=" + i.Id + ", ASIN=" + i.ASIN);
            });
            toMoveBarcodes.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved barcode=" + i.Barcode);
            });
            toMoveSpecialCases.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved special case=" + i.Id);
            });
            toMoveOrderItems.ForEach(i =>
            {
                if (i.StyleItemId == fromStyleItemId)
                    i.StyleItemId = toStyleItemId;
                if (i.SourceStyleItemId == fromStyleItemId)
                    i.SourceStyleItemId = toStyleItemId;
                log.Info("Moved orderItem=" + i.Id);
            });
            toMoveSourceOrderItems.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved source orderItem=" + i.Id);
            });

            toMoveOpenBoxItems.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved open box=" + i.Id);
            });
            toMoveSealedBoxItems.ForEach(i =>
            {
                i.StyleItemId = toStyleItemId;
                log.Info("Moved sealed box=" + i.Id);
            });

            if (!toStyleItem.Weight.HasValue)
                toStyleItem.Weight = fromStyleItem.Weight;
            if (!toStyleItem.MinPrice.HasValue)
                toStyleItem.MinPrice = fromStyleItem.MinPrice;
            if (!toStyleItem.MaxPrice.HasValue)
                toStyleItem.MaxPrice = fromStyleItem.MaxPrice;

            bool quantityWasChanged = false;
            int? oldQuantity = null;
            bool hasEmptyBoxQuantity = toStyleItemCache != null
                                     && (toStyleItemCache.BoxQuantity == 0 || toStyleItemCache.BoxQuantity == null);
            if ((toStyleItem.Quantity == 0
                || (toStyleItem.Quantity == null && hasEmptyBoxQuantity))
                && fromStyleItem.Quantity > 0)
            {
                log.Info("Merge. Quantity was changed, styleItemId=" + toStyleItem.Id + ", from=" + toStyleItem.Quantity +
                         ", to=" + fromStyleItem.Quantity);

                oldQuantity = toStyleItem.Quantity;
                quantityWasChanged = true;

                toStyleItem.Quantity = fromStyleItem.Quantity;
                toStyleItem.QuantitySetBy = fromStyleItem.QuantitySetBy;
                toStyleItem.QuantitySetDate = fromStyleItem.QuantitySetDate;
            }

            db.Commit();

            db.StyleItems.Remove(fromStyleItem);
            db.Commit();

            if (quantityWasChanged)
            {
                quantityManager.LogStyleItemQuantity(db,
                    toStyleItem.Id,
                    toStyleItem.Quantity,
                    oldQuantity,
                    QuantityChangeSourceType.EnterNewQuantity, 
                    null,
                    null,
                    null,
                    when,
                    by);
            }

            quantityManager.LogStyleItemQuantity(db,
                fromStyleItem.Id,
                null,
                fromStyleItem.Quantity,
                QuantityChangeSourceType.Removed, 
                "Merge",
                toStyleItem.Id,
                StringHelper.JoinTwo(",", toStyleItem.Size, toStyleItem.Color),
                when,
                by);


            db.StyleItemActionHistories.Add(new StyleItemActionHistory()
            {
                ActionName = "Merge",
                StyleItemId = fromStyleItemId,
                Data = fromStyleItem.SizeId + ";" + fromStyleItem.Size + ";" + toStyleItemId.ToString(),

                CreateDate = when,
                CreatedBy = by
            });

            db.Commit();

            db.Styles.SetReSaveDate(fromStyleItem.StyleId, when, by);

            cacheService.RequestStyleIdUpdates(db,
                new List<long>()
                {
                    fromStyleItem.StyleId,
                },
                UpdateCacheMode.IncludeChild,
                by);

            return new CallMessagesResultVoid()
            {
                Status = CallStatus.Success,
                Messages = new List<MessageString>()
            };
        }


        public static GridResponse<StyleViewModel> GetAll(IUnitOfWork db,
            StyleSearchFilterViewModel filter)
        {
            if (filter.LimitCount == 0)
                filter.LimitCount = 50;

            IQueryable<ViewStyle> styleQuery = from st in db.Styles.GetAllActual()
                                               orderby st.Id descending
                                               select st;

            if (filter.StyleId.HasValue)
            {
                styleQuery = styleQuery.Where(s => s.Id == filter.StyleId.Value);
            }
            
            var styleItemQuantity = from sic in db.StyleItemCaches.GetAll()// on si.Id equals sic.Id
                                    group sic by sic.StyleId
                                    into byStyle
                                    select new
                                    {
                                        StyleId = byStyle.Key,
                                        Quantity = byStyle.Sum(s => s.RemainingQuantity > 0 ? s.RemainingQuantity : 0)
                                    };

            var styleWithCacheQuery = from st in styleQuery
                                      join s in db.Styles.GetAll() on st.Id equals s.Id
                                      join ds in db.DropShippers.GetAll() on s.DropShipperId equals ds.Id into withDs
                                      from ds in withDs.DefaultIfEmpty()
                                      join stQty in styleItemQuantity on st.Id equals stQty.StyleId into withQty
                                      from stQty in withQty.DefaultIfEmpty()
                                      join stCache in db.StyleCaches.GetAll() on st.Id equals stCache.Id into withCache
                                      from stCache in withCache.DefaultIfEmpty()
                                      select new
                                      {
                                          st,
                                          s,
                                          ds,
                                          stQty,
                                          stCache
                                      };

            if (filter.FromReSaveDate.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.ReSaveDate >= filter.FromReSaveDate.Value
                    || st.stCache.UpdateDate >= filter.FromReSaveDate.Value);
            }

            if (!String.IsNullOrEmpty(filter.StyleString))
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.StyleID.Contains(filter.StyleString));
            }

            if (filter.DropShipperId.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.s.DropShipperId == filter.DropShipperId);
            }

            if (!String.IsNullOrEmpty(filter.Barcode))
            {
                var barcodeQuery = from b in db.StyleItemBarcodes.GetAll()
                                   join si in db.StyleItems.GetAll() on b.StyleItemId equals si.Id
                                   where b.Barcode == filter.Barcode
                                   select si.StyleId;

                styleWithCacheQuery = from st in styleWithCacheQuery
                                      join b in barcodeQuery on st.st.Id equals b
                                      select st;
            }


            if (filter.ItemStyles != null && filter.ItemStyles.Any())
            {
                var itemStyleQuery = db.StyleFeatureValues.GetAll()
                    .Where(f => filter.ItemStyles.Contains(f.FeatureValueId))
                    .Select(f => f.StyleId)
                    .ToList();

                styleWithCacheQuery = from st in styleWithCacheQuery
                                      join i in itemStyleQuery on st.st.Id equals i
                                      select st;
            }

            if (filter.Sleeves != null && filter.Sleeves.Any())
            {
                var sleevesQuery = db.StyleFeatureValues.GetAll()
                    .Where(f => filter.Sleeves.Contains(f.FeatureValueId))
                    .Select(f => f.StyleId)
                    .ToList();

                styleWithCacheQuery = from st in styleWithCacheQuery
                                      join i in sleevesQuery on st.st.Id equals i
                                      select st;
            }

            if (filter.HolidayId.HasValue)
            {
                var sleevesQuery = db.StyleFeatureValues.GetAll()
                    .Where(f => filter.HolidayId == f.FeatureValueId)
                    .Select(f => f.StyleId)
                    .ToList();

                styleWithCacheQuery = from st in styleWithCacheQuery
                                      join i in sleevesQuery on st.st.Id equals i
                                      select st;
            }

            if (!String.IsNullOrEmpty(filter.Keywords))
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.StyleID.Contains(filter.Keywords)
                    || st.st.Name.Contains(filter.Keywords)
                    || st.stCache.HolidayValue.Contains(filter.Keywords));
            }

            if (filter.Genders != null && filter.Genders.Any())
            {
                var genderIds = filter.Genders.Select(g => g.ToString()).ToList();
                styleWithCacheQuery = styleWithCacheQuery.Where(st => genderIds.Contains(st.stCache.Gender));
            }

            if (filter.PictureStatus.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.PictureStatus == filter.PictureStatus.Value);
            }

            if (filter.FillingStatus.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.FillingStatus == filter.FillingStatus.Value);
            }
            
            if (filter.NoneSoldPeriod.HasValue && filter.NoneSoldPeriod > 0)
            {
                //NOTE: using styleId to hide parent item with that style but without sold
                //получаем список продающихся styleId
                //выводим список item у которых styleId not in Sold Style List
                var fromDate = DateTime.Today.AddDays(-filter.NoneSoldPeriod.Value);
                var soldStyleIds = db.StyleCaches.GetAll()
                    .Where(sc => sc.LastSoldDateOnMarket < fromDate)
                    .GroupBy(sc => sc.Id)
                    .Select(gsc => gsc.Key).ToList();

                styleWithCacheQuery = styleWithCacheQuery.Where(st => soldStyleIds.Contains(st.st.Id));
            }

            if (!String.IsNullOrEmpty(filter.OnlineStatus))
            {
                if (filter.OnlineStatus == "Online")
                {
                    styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stQty.Quantity > 0
                                                                          && st.stCache.AssociatedMarket.HasValue);
                }
                if (filter.OnlineStatus == "Offline")
                {
                    styleWithCacheQuery = styleWithCacheQuery.Where(st => !st.stCache.AssociatedMarket.HasValue);
                }
            }

            if (filter.MinQty.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stQty.Quantity > filter.MinQty.Value);
            }

            //if (filter.HasInitialQty)
            //{
            //    styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stQty.Quantity > filter.MinQty.Value);
            //}

            if (filter.MainLicense.HasValue)
            {
                if (filter.MainLicense == 0)
                    styleWithCacheQuery = styleWithCacheQuery.Where(st => String.IsNullOrEmpty(st.stCache.MainLicense));
                else
                    styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stCache.MainLicense == filter.MainLicense.Value.ToString());
            }

            if (filter.SubLicense.HasValue)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stCache.SubLicense == filter.SubLicense.Value.ToString());
            }

            if (!String.IsNullOrEmpty(filter.ExcludeMarketplaceId))
            {
                var parts = filter.ExcludeMarketplaceId.Split(';'); //m.Market + ";" + m.MarketplaceId
                var market = Int32.Parse(parts[0]);
                var marketplaceId = parts.Length > 1 ? parts[1] : "";
                var marketplaceName = ";" + MarketHelper.GetShortName(market, marketplaceId) + ":";

                styleWithCacheQuery = styleWithCacheQuery.Where(st => String.IsNullOrEmpty(st.stCache.MarketplacesInfo)
                    || !(";" + st.stCache.MarketplacesInfo + ";").Contains(marketplaceName));

                var itemQuery = db.Items.GetAll()
                    .Where(i => i.Market == market && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .Select(i => new { StyleId = i.StyleId })
                    .Distinct();

                styleWithCacheQuery = from sc in styleWithCacheQuery
                                      join i in itemQuery on sc.st.Id equals i.StyleId into withItem
                                      from i in withItem.DefaultIfEmpty()
                                      where !i.StyleId.HasValue
                                      select sc;
            }

            if (!String.IsNullOrEmpty(filter.IncludeMarketplaceId))
            {
                var parts = filter.IncludeMarketplaceId.Split(';'); //m.Market + ";" + m.MarketplaceId
                var market = Int32.Parse(parts[0]);
                var marketplaceId = parts.Length > 1 ? parts[1] : "";
                var marketplaceName = ";" + MarketHelper.GetShortName(market, marketplaceId) + ":";

                styleWithCacheQuery = styleWithCacheQuery.Where(st => String.IsNullOrEmpty(st.stCache.MarketplacesInfo)
                    || !(";" + st.stCache.MarketplacesInfo + ";").Contains(marketplaceName));

                var itemQuery = db.Items.GetAll()
                    .Where(i => i.Market == market && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .Select(i => new { StyleId = i.StyleId })
                    .Distinct();

                styleWithCacheQuery = from sc in styleWithCacheQuery
                                      join i in itemQuery on sc.st.Id equals i.StyleId
                                      select sc;
            }

            if (filter.OnlyInStock)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.stQty.Quantity > 0);
            }

            if (!filter.IncludeKiosk)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.DisplayMode != (int)DisplayModes.Hidden);
            }
            if (filter.OnlyOnHold)
            {
                styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.OnHold);
            }

            styleWithCacheQuery = styleWithCacheQuery.Where(st => st.st.DisplayMode != (int)DisplayModes.SystemHidden);

            var totalCount = styleWithCacheQuery.Count();

            if (!String.IsNullOrEmpty(filter.SortField))
            {
                switch (filter.SortField)
                {
                    //case "DropShipperName":
                    //    if (filter.SortMode == 0)
                    //        styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.ds.Name);
                    //    else
                    //        styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.ds.Name);
                    //    break;
                    case "CreateDate":
                        if (filter.SortMode == 0)
                            styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.st.CreateDate);
                        else
                            styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.st.CreateDate);
                        break;
                    case "StyleString":
                        if (filter.SortMode == 0)
                            styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.st.StyleID);
                        else
                            styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.st.StyleID);
                        break;
                    //case "BrandName":
                    //    if (filter.SortMode == 0)
                    //        styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.stCache.Brand);
                    //    else
                    //        styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.stCache.Brand);
                    //    break;
                    case "Quantity":
                        if (filter.SortMode == 0)
                            styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.stQty.Quantity);
                        else
                            styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.stQty.Quantity);
                        break;
                    case "LocationIndex":
                        if (filter.SortMode == 0)
                            styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.st.LocationIndex);
                        else
                            styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.st.LocationIndex);
                        break;

                    case "RemainingQuantity":
                        if (filter.SortMode == 0)
                            styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.stQty.Quantity);
                        else
                            styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.stQty.Quantity);
                        break;
                    default:
                        styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.st.CreateDate);
                        break;
                }
            }
            else
            {
                styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.st.CreateDate);
            }
            styleWithCacheQuery = styleWithCacheQuery
                .Skip(filter.StartIndex)
                .Take(filter.LimitCount);

            var styleList = styleWithCacheQuery.Select(s => new StyleViewModel
            {
                Id = s.st.Id,
                StyleId = s.st.StyleID,
                Type = s.st.Type,
                OnHold = s.st.OnHold,

                DropShipperId = s.s.DropShipperId,
                DropShipperName = s.ds.Name,

                Image = s.st.Image,

                Name = s.st.Name,

                BoxQuantity = s.st.BoxQuantity,
                BoxTotalPrice = s.st.BoxTotalPrice,
                BoxItemMinPrice = s.st.BoxItemMinPrice ?? 0,
                BoxItemMaxPrice = s.st.BoxItemMaxPrice ?? 0,

                ManuallyQuantity = s.st.ManuallyQuantity,
                HasManuallyQuantity = s.st.HasManuallyQuantity == 1,

                ItemTypeId = s.st.ItemTypeId,
                ItemTypeName = s.st.ItemTypeName,

                DisplayMode = s.st.DisplayMode,

                RemovePriceTag = s.st.RemovePriceTag,
                PictureStatus = s.st.PictureStatus,

                AssociatedASIN = s.stCache.AssociatedASIN,
                AssociatedSourceMarketId = s.stCache.AssociatedSourceMarketId,
                AssociatedMarket = s.stCache.AssociatedMarket,
                AssociatedMarketplaceId = s.stCache.AssociatedMarketplaceId,

                //MarketplacesInfo = s.stCache.MarketplacesInfo,
                
                CreateDate = s.st.CreateDate,
                UpdateDate = s.st.UpdateDate,
                ReSaveDate = s.st.ReSaveDate,
            }).ToList();

            //NOTE: fast exist for case with ReSaveDate, in most cases count will be = 0
            if (styleList.Count == 0)
            {
                return new GridResponse<StyleViewModel>(new List<StyleViewModel>(), 0);
            }

            var styleIds = styleList.Select(st => st.Id).ToList();

            var styleListingList = (from i in db.Items.GetAllViewActual()
                                    join pr in db.ParentItems.GetAll() on i.ParentASIN equals pr.ASIN
                                    where i.StyleId.HasValue && styleIds.Contains(i.StyleId.Value)
                                    select new ItemDTO()
                                    {
                                        StyleId = i.StyleId,
                                        StyleItemId = i.StyleItemId,
                                        Market = i.Market,
                                        MarketplaceId = i.MarketplaceId,
                                        ASIN = i.ASIN,
                                        ParentASIN = i.ParentASIN,
                                        SourceMarketId = i.SourceMarketId,
                                        Rank = pr.Rank ?? RankHelper.DefaultRank,
                                        PublishedStatus = i.ItemPublishedStatus,
                                    })
                .ToList();

            var styleListingByMarketList = styleListingList.GroupBy(i => new { i.StyleId.Value, i.Market, i.MarketplaceId })
                .Select(i => new
                {
                    Count = i.Count(),
                    HasPublished = i.Any(l => l.PublishedStatus == 50),
                    HasPublishErrors = i.Any(l => l.PublishedStatus > 50 && l.PublishedStatus == (int)PublishedStatuses.PublishingErrors),
                    HasPublishInProgress = i.Any(l => l.PublishedStatus < 50 && l.PublishedStatus != (int)PublishedStatuses.PublishingErrors),
                    MainItem = i.OrderByDescending(l => l.PublishedStatus <= 50 ? l.PublishedStatus : -1).OrderBy(l => l.Rank).FirstOrDefault()
                }).ToList();

            var styleItemList = (from si in db.StyleItems.GetAll()
                                 join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id into withCache
                                 from sic in withCache.DefaultIfEmpty()
                                 where styleIds.Contains(si.StyleId)
                                 select new StyleItemShowViewModel()
                                 {
                                     Size = si.Size,
                                     Color = si.Color,

                                     StyleId = si.StyleId,
                                     OnHold = si.OnHold,

                                     ScannedSoldQuantityFromDate = sic.ScannedSoldQuantityFromDate,
                                     SentToFBAQuantityFromDate = sic.SentToFBAQuantityFromDate,
                                     MarketsSoldQuantityFromDate = sic.MarketsSoldQuantityFromDate,
                                     SpecialCaseQuantityFromDate = sic.SpecialCaseQuantityFromDate,
                                     SentToPhotoshootQuantityFromDate = sic.SentToPhotoshootQuantityFromDate,

                                     TotalScannedSoldQuantity = sic.TotalScannedSoldQuantity,
                                     TotalSentToFBAQuantity = sic.TotalSentToFBAQuantity,
                                     TotalMarketsSoldQuantity = sic.TotalMarketsSoldQuantity,
                                     TotalSpecialCaseQuantity = sic.TotalSpecialCaseQuantity,
                                     TotalSentToPhotoshootQuantity = sic.TotalSentToPhotoshootQuantity,

                                     InventoryQuantity = sic.InventoryQuantity,
                                     RemainingQuantity = sic.RemainingQuantity,

                                     SalesInfo = sic.SalesInfo,
                                     IsInVirtual = sic.IsInVirtual,
                                 }).ToList();

            foreach (var style in styleList)
            {
                style.StyleItemCaches = styleItemList.Where(si => si.StyleId == style.Id)
                    .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                    .ToList();

                style.Marketplaces = styleListingByMarketList
                    .Where(i => i.MainItem.StyleId == style.Id)
                    .OrderBy(i => MarketHelper.GetMarketIndex((MarketType)i.MainItem.Market, i.MainItem.MarketplaceId))
                    .Select(i => new StyleMarketplaceInfo()
                    {
                        Count = styleListingList.Where(l => l.StyleId == style.Id && l.Market == i.MainItem.Market && l.MarketplaceId == i.MainItem.MarketplaceId).Select(l => l.ParentASIN).Distinct().Count(),
                        ASIN = i.MainItem.ASIN,
                        SourceMarketId = i.MainItem.SourceMarketId,
                        Market = i.MainItem.Market,
                        MarketplaceId = i.MainItem.MarketplaceId,
                        StyleString = style.StyleId,
                        IsPublished = i.MainItem.PublishedStatus == (int)PublishedStatuses.Published || i.MainItem.PublishedStatus == (int)PublishedStatuses.HasChanges,
                        HasPublishErrors = i.HasPublishErrors,
                        HasPublishInProgress = i.HasPublishInProgress,
                    }).ToList();

                style.ScannedSoldQuantity = style.StyleItemCaches.Sum(s => s.ScannedSoldQuantityFromDate);
                style.SentToFBAQuantity = style.StyleItemCaches.Sum(s => s.SentToFBAQuantityFromDate);
                style.MarketsSoldQuantity = style.StyleItemCaches.Sum(s => s.MarketsSoldQuantityFromDate);
                style.SpecialCaseQuantity = style.StyleItemCaches.Sum(s => s.SpecialCaseQuantityFromDate);
                style.SentToPhotoshootQuantity = style.StyleItemCaches.Sum(s => s.SentToPhotoshootQuantityFromDate);

                style.TotalScannedSoldQuantity = style.StyleItemCaches.Sum(s => s.TotalScannedSoldQuantity);
                style.TotalSentToFBAQuantity = style.StyleItemCaches.Sum(s => s.TotalSentToFBAQuantity);
                style.TotalMarketsSoldQuantity = style.StyleItemCaches.Sum(s => s.TotalMarketsSoldQuantity);
                style.TotalSpecialCaseQuantity = style.StyleItemCaches.Sum(s => s.TotalSpecialCaseQuantity);
                style.TotalSentToPhotoshootQuantity = style.StyleItemCaches.Sum(s => s.TotalSentToPhotoshootQuantity);

                style.InventoryQuantity = style.StyleItemCaches.Sum(s => s.InventoryQuantity);
                style.RemainingQuantity = style.StyleItemCaches.Sum(s => (!s.RemainingQuantity.HasValue || s.RemainingQuantity < 0) ? 0 : s.RemainingQuantity);
            }

            var styleLocations = (from l in db.StyleLocations.GetAllAsDTO()
                                  where styleIds.Contains(l.StyleId)
                                  orderby l.IsDefault descending
                                  select l).ToList();

            styleList.ForEach(i =>
            {
                i.Image = GetImage(i.Image);
                i.ItemTypeId = i.ItemTypeId ?? StyleViewModel.DefaultItemType;
                i.Locations = styleLocations.Where(s => s.StyleId == i.Id).Select(s => new LocationViewModel(s)).ToList();
            });

            return new GridResponse<StyleViewModel>(styleList, totalCount);
        }
    }
}