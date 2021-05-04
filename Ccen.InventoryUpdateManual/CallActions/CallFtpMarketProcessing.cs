using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Bargains;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Walmart.Api;
using Walmart.Api.Core.Models.OpenApi;


namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallFtpMarketProcessing
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;
        private IQuantityManager _quantityManager;
        private IEmailService _emailService;

        public CallFtpMarketProcessing(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            ISystemActionService actionService,
            IQuantityManager quantityManager,
            IEmailService emailService)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _actionService = actionService;
            _quantityManager = quantityManager;
            _emailService = emailService;
        }

        public class FtpMarketProductItem
        {
            public long StyleId { get; set; }
            public long StyleItemId { get; set; }
            public string StyleString { get; set; }
            public string SKU { get; set; }
            public string Barcode { get; set; }
            public string VariationGroupId { get; set; }
            public string Size { get; set; }
            public string Color { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Name { get; set; }
            public string MainImage { get; set; }
            public string AdditionalImage1 { get; set; }
            public string AdditionalImage2 { get; set; }
            public string AdditionalImage3 { get; set; }
            public string AdditionalImage4 { get; set; }
            public string AdditionalImage5 { get; set; }

            public string BrandName { get; set; }
            public string Description { get; set; }
            public string BulletPoint1 { get; set; }
            public string BulletPoint2 { get; set; }
            public string BulletPoint3 { get; set; }
            public string BulletPoint4 { get; set; }
            public string BulletPoint5 { get; set; }
            public string SearchTerms { get; set; }

            public string Gender { get; set; }
            public string ItemStyle { get; set; }
            public string Accessories { get; set; }
            public string Sleeve { get; set; }
            public string Season { get; set; }
            public string Material { get; set; }
            public string MaterialComposition { get; set; }
            public string Color1 { get; set; }
            public string Color2 { get; set; }
            public string MainLicense { get; set; }
            public string SubLicense { get; set; }
            public string SecondarySubLicense { get; set; }
            public string Holiday { get; set; }
            public string Occasion { get; set; }
        }

        public void CreateListings()
        {
            var cacheService = new CacheService(_log, _time, _actionService, _quantityManager);
            var barcodeService = new BarcodeService(_log, _time, _dbFactory);
            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            var listingCreateService = new AutoCreateFtpMarketListingService(_log, _time, _dbFactory, cacheService, barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);

            listingCreateService.CreateListings();
        }

        public void GenerateProductFeed()
        {
            var filename = String.Format("product-feed-{0}.xls", _time.GetAppNowTime().ToString("yyyyMMddHHmmss"));
            var outputFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            IList<FtpMarketProductItem> items;
            IList<StyleFeatureValueDTO> allValueFeatures;
            IList<StyleFeatureValueDTO> allTextFeatures;
            IList<StyleImageDTO> allImages;
            using (var db = _dbFactory.GetRWDb())
            {
                items = (from st in db.Styles.GetAll()
                         join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                         join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                         join i in db.Items.GetAll() on si.Id equals i.StyleItemId
                         join l in db.Listings.GetAll() on i.Id equals l.ItemId
                         where i.Market == (int)MarketType.FtpMarket
                             && !l.IsRemoved
                             && sic.RemainingQuantity > 0
                             && i.StyleId.HasValue
                             && i.StyleItemId.HasValue                         
                         select new FtpMarketProductItem()
                         {
                             StyleId = i.StyleId.Value,
                             StyleItemId = i.StyleItemId.Value,
                             StyleString = st.StyleID,
                             SKU = l.SKU,
                             Barcode = i.Barcode,
                             VariationGroupId = i.ParentASIN,
                             Size = i.Size,
                             Color = i.Color,
                             Price = l.CurrentPrice,
                             Quantity = sic.RemainingQuantity,
                             Name = st.Name,
                             BrandName = st.Manufacturer,
                             Description = st.Description,
                             BulletPoint1 = st.BulletPoint1,
                             BulletPoint2 = st.BulletPoint2,
                             BulletPoint3 = st.BulletPoint3,
                             BulletPoint4 = st.BulletPoint4,
                             BulletPoint5 = st.BulletPoint5,
                             SearchTerms = st.SearchTerms
                         }).ToList();

                items = items.OrderBy(i => i.StyleString)
                    .ThenBy(i => i.StyleId)
                    .ThenBy(i => SizeHelper.GetSizeIndex(i.Size))
                    .ThenBy(i => i.Color)
                    .ToList();

                var styleIdList = items.Select(i => i.StyleId).Distinct().ToList();
                allValueFeatures = db.StyleFeatureValues.GetAllWithFeature().Where(f => styleIdList.Contains(f.StyleId)).ToList();
                allTextFeatures = db.StyleFeatureTextValues.GetAllWithFeature().Where(f => styleIdList.Contains(f.StyleId)).ToList();

                allImages = db.StyleImages.GetAllAsDto()
                    .Where(i => styleIdList.Contains(i.StyleId)
                        && !i.IsSystem)
                    .OrderBy(im => im.Id)
                    .ToList();
            }

            foreach (var item in items)
            {
                var styleId = item.StyleId;
                var images = allImages.Where(i => i.StyleId == item.StyleId).ToList();

                if (images.Count > 0)
                    item.MainImage = images[0].Image;

                if (images.Count > 1)
                    item.AdditionalImage1 = images[1].Image;
                if (images.Count > 2)
                    item.AdditionalImage2 = images[2].Image;
                if (images.Count > 3)
                    item.AdditionalImage3 = images[3].Image;
                if (images.Count > 4)
                    item.AdditionalImage4 = images[4].Image;
                if (images.Count > 5)
                    item.AdditionalImage5 = images[5].Image;

                var valueFeatures = allValueFeatures.Where(i => i.StyleId == item.StyleId).ToList();
                var textFeatures = allTextFeatures.Where(i => i.StyleId == item.StyleId).ToList();
                item.Gender = valueFeatures.FirstOrDefault(f => f.FeatureName == "Gender")?.Value;
                item.ItemStyle = valueFeatures.FirstOrDefault(f => f.FeatureName == "Item style")?.Value;
                item.Accessories = valueFeatures.FirstOrDefault(f => f.FeatureName == "Accessories")?.Value;
                item.Sleeve = valueFeatures.FirstOrDefault(f => f.FeatureName == "Sleeve")?.Value;
                item.Season = valueFeatures.FirstOrDefault(f => f.FeatureName == "Season")?.Value;
                item.Material = valueFeatures.FirstOrDefault(f => f.FeatureName == "Material")?.Value;
                item.MaterialComposition = textFeatures.FirstOrDefault(f => f.FeatureName == "Material Composition")?.Value;
                item.Color1 = valueFeatures.FirstOrDefault(f => f.FeatureName == "Color1")?.Value;
                item.Color2 = valueFeatures.FirstOrDefault(f => f.FeatureName == "Color2")?.Value;
                item.MainLicense = valueFeatures.FirstOrDefault(f => f.FeatureName == "Main License")?.Value;
                item.SubLicense = valueFeatures.FirstOrDefault(f => f.FeatureName == "Sub License")?.Value;
                item.SecondarySubLicense = valueFeatures.FirstOrDefault(f => f.FeatureName == "Secondary Sub License")?.Value;
                item.Holiday = valueFeatures.FirstOrDefault(f => f.FeatureName == "Holiday")?.Value;
                item.Occasion = valueFeatures.FirstOrDefault(f => f.FeatureName == "Occasion")?.Value;
            }


            var b = new ExportColumnBuilder<FtpMarketProductItem>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.SKU, "SKU", 15),
                b.Build(p => p.Barcode, "Barcode", 15),
                b.Build(p => p.VariationGroupId, "VariationGroupId", 15),
                b.Build(p => p.Size, "VariationSize", 15),
                b.Build(p => p.Color, "VariationColor", 15),
                b.Build(p => p.Price, "Price", 15),
                b.Build(p => p.Quantity, "Quantity", 15),
                b.Build(p => p.Name, "Name", 15),
                b.Build(p => p.MainImage, "MainImage", 15),
                b.Build(p => p.AdditionalImage1, "AdditionalImage1", 15),
                b.Build(p => p.AdditionalImage2, "AdditionalImage2", 15),
                b.Build(p => p.AdditionalImage3, "AdditionalImage3", 15),
                b.Build(p => p.AdditionalImage4, "AdditionalImage4", 15),
                b.Build(p => p.AdditionalImage5, "AdditionalImage5", 15),
                b.Build(p => p.Description, "Description", 15),
                b.Build(p => p.BulletPoint1, "BulletPoint1", 15),
                b.Build(p => p.BulletPoint2, "BulletPoint2", 15),
                b.Build(p => p.BulletPoint3, "BulletPoint3", 15),
                b.Build(p => p.BulletPoint4, "BulletPoint4", 15),
                b.Build(p => p.BulletPoint5, "BulletPoint5", 15),
                b.Build(p => p.SearchTerms, "SearchTerms", 15),
                b.Build(p => p.Gender, "Gender", 15),
                b.Build(p => p.ItemStyle, "ItemStyle", 15),
                b.Build(p => p.Accessories, "Accessories", 15),
                b.Build(p => p.Sleeve, "Sleeve", 15),
                b.Build(p => p.Season, "Season", 15),
                b.Build(p => p.Material, "Material", 15),
                b.Build(p => p.MaterialComposition, "MaterialComposition", 15),
                b.Build(p => p.Color1, "Color1", 15),
                b.Build(p => p.Color2, "Color2", 15),
                b.Build(p => p.MainLicense, "MainLicense", 15),
                b.Build(p => p.SubLicense, "SubLicense", 15),
                b.Build(p => p.SecondarySubLicense, "SecondarySubLicense", 15),
                b.Build(p => p.Holiday, "Holiday", 15),
                b.Build(p => p.Occasion, "Occasion", 15),
            };

            using (var stream = ExcelHelper.Export(items, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(outputFilepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
