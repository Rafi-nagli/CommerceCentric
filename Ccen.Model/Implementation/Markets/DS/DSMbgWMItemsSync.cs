using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using DropShipper.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.DS
{
    public class DSCustomItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IBarcodeService _barcodeService;

        private long _dropShipperId;

        private MarketType _destMarket;
        private string _destMarketplaceId;

        private MarketType _sourceMarket;
        private string _sourceMarketplaceId;
                

        public DSCustomItemsSync(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IBarcodeService barcodeService,
            long dropShipperId,
            MarketType destMarket,
            string destMarketplaceId,
            MarketType sourceMarket,
            string sourceMarketplaceId)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _barcodeService = barcodeService;

            _dropShipperId = dropShipperId;
            _destMarket = destMarket;
            _destMarketplaceId = destMarketplaceId;
            _sourceMarket = sourceMarket;
            _sourceMarketplaceId = sourceMarketplaceId;            
        }

        private string PrepareManufacture(string manufacture)
        {
            if (StringHelper.ContainsNoCase(manufacture, "Mia Belle"))
                return "Mia Belle Girls";
            return manufacture;
        }

        public void SyncStyles(DropShipperApi api)
        {
            var styles = api.GetStyles(_log, _sourceMarket, _sourceMarketplaceId);

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var incomeStyle in styles)
                {
                    var existStyle = db.Styles.GetAll().FirstOrDefault(st => (st.StyleID == incomeStyle.StyleID
                        || st.OriginalStyleID.Contains(incomeStyle.StyleID + ";"))
                        && !st.Deleted);
                    if (existStyle == null)
                    {
                        _log.Info("Creating style, StyleId=" + incomeStyle.StyleID);

                        var newStyle = new Style()
                        {
                            StyleID = incomeStyle.StyleID,
                            OriginalStyleID = incomeStyle.OriginalStyleID,
                            Name = incomeStyle.Name,
                            Description = incomeStyle.Description,
                            BulletPoint1 = incomeStyle.BulletPoint1,
                            BulletPoint2 = incomeStyle.BulletPoint2,
                            BulletPoint3 = incomeStyle.BulletPoint3,
                            BulletPoint4 = incomeStyle.BulletPoint4,
                            BulletPoint5 = incomeStyle.BulletPoint5,
                            MSRP = incomeStyle.MSRP,
                            Manufacturer = PrepareManufacture(incomeStyle.Manufacturer),
                            DropShipperId = _dropShipperId,// DSHelper.MBGPAId,

                            Image = incomeStyle.Image,

                            CreateDate = _time.GetAppNowTime(),
                        };
                        db.Styles.Add(newStyle);
                        db.Commit();

                        foreach (var featureValue in incomeStyle.StyleFeatures)
                        {
                            var findFeature = db.Features.GetAll().FirstOrDefault(f => f.Name == featureValue.FeatureName);
                            if (findFeature == null)
                                continue;

                            if (featureValue.Type == (int)FeatureValuesType.TextBox
                                || featureValue.Type == (int)FeatureValuesType.CheckBox)
                            {
                                db.StyleFeatureTextValues.Add(new StyleFeatureTextValue()
                                {
                                    StyleId = newStyle.Id,
                                    FeatureId = findFeature.Id,
                                    Value = featureValue.Value,
                                    CreateDate = _time.GetAppNowTime(),
                                });
                            }
                            else
                            {
                                var findFeatureValue = db.FeatureValues.GetAll().FirstOrDefault(fv => fv.FeatureId == findFeature.Id
                                    && fv.Value == featureValue.Value);
                                if (findFeatureValue != null)
                                {
                                    db.StyleFeatureValues.Add(new StyleFeatureValue()
                                    {
                                        StyleId = newStyle.Id,
                                        FeatureId = findFeatureValue.FeatureId,
                                        FeatureValueId = findFeatureValue.Id,
                                        CreateDate = _time.GetAppNowTime(),
                                    });
                                }
                            }
                        }
                        db.Commit();


                        foreach (var si in incomeStyle.StyleItems)
                        {
                            var newStyleItem = new StyleItem()
                            {
                                StyleId = newStyle.Id,
                                Size = si.Size,
                                Color = si.Color,
                                Quantity = si.Quantity,
                                QuantitySetDate = si.QuantitySetDate,
                                Weight = si.Weight,

                                CreateDate = _time.GetAppNowTime()
                            };
                            db.StyleItems.Add(newStyleItem);
                            db.Commit();
                            if (si.Barcodes != null && si.Barcodes.Any())
                            {
                                foreach (var bi in si.Barcodes)
                                {
                                    db.StyleItemBarcodes.Add(new StyleItemBarcode()
                                    {
                                        StyleItemId = newStyleItem.Id,
                                        Barcode = bi.Barcode,

                                        CreateDate = _time.GetAppNowTime(),
                                    });
                                }
                            }
                            db.Commit();
                        }

                        foreach (var image in incomeStyle.Images)
                        {
                            var newStyleImage = new StyleImage()
                            {
                                StyleId = newStyle.Id,
                                Image = image.Image,
                                SourceImage = image.SourceImage,
                                SourceMarketId = image.SourceMarketId,
                                Type = image.Type,
                                Category = image.Category,
                                IsDefault = image.IsDefault,
                                IsSystem = image.IsSystem,
                                CreateDate = _time.GetAppNowTime()
                            };
                            db.StyleImages.Add(newStyleImage);
                        }
                        db.Commit();
                    }
                    else
                    {                        
                        _log.Info("Already exists: " + incomeStyle.StyleID);

                        _log.Info("Style Name changed: " + existStyle.Name + " => " + incomeStyle.Name);
                        existStyle.Name = incomeStyle.Name;
                        db.Commit();
                    }
                }
            }
        }

        public void SyncItems(DropShipperApi api)
        {
            var destMarket = _destMarket;// MarketType.Walmart;
            var destMarketplaceId = _destMarketplaceId; // "";
            var sourceMarketplaceId = _sourceMarketplaceId;// MarketplaceKeeper.DsPAWMCom;
            var parentItems = api.GetItems(_log, _sourceMarket /* MarketType.DropShipper */, sourceMarketplaceId);

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var parentItem in parentItems)
                {
                    var existParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.ASIN == parentItem.ASIN
                        && pi.Market == (int)destMarket
                        && (String.IsNullOrEmpty(destMarketplaceId) || pi.MarketplaceId == destMarketplaceId));

                    if (existParentItem == null)
                    {
                        _log.Info("Creating parentItem, ASIN=" + parentItem.ASIN);

                        var newParentItem = new ParentItem()
                        {
                            ASIN = parentItem.ASIN,
                            Market = (int)destMarket,
                            MarketplaceId = destMarketplaceId,

                            SKU = parentItem.SKU,
                            AmazonName = parentItem.AmazonName,
                            BrandName = parentItem.BrandName,
                            BulletPoint1 = parentItem.BulletPoint1,
                            BulletPoint2 = parentItem.BulletPoint2,
                            BulletPoint3 = parentItem.BulletPoint3,
                            BulletPoint4 = parentItem.BulletPoint4,
                            BulletPoint5 = parentItem.BulletPoint5,

                            CreateDate = _time.GetAppNowTime(),
                        };
                        db.ParentItems.Add(newParentItem);
                        db.Commit();

                        foreach (var v in parentItem.Variations)
                        {
                            var findStyle = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == v.StyleString
                                && !st.Deleted);
                            StyleItem findStyleItem = null;
                            if (findStyle != null)
                                findStyleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.StyleId == findStyle.Id
                                    && si.Size == v.StyleSize
                                    && (String.IsNullOrEmpty(v.StyleColor)
                                        || si.Color == v.StyleColor));

                            var barcode = v.Barcode;
                            if (string.IsNullOrEmpty(barcode) && !String.IsNullOrEmpty(v.SKU))
                            {
                                var result = _barcodeService.AssociateBarcodes(v.SKU, _time.GetAppNowTime(), null);
                                if (!String.IsNullOrEmpty(result.Barcode))
                                {
                                    barcode = result.Barcode;
                                }
                            }

                            var newItem = new Item()
                            {
                                ParentASIN = newParentItem.ASIN,
                                Market = (int)destMarket,
                                MarketplaceId = destMarketplaceId,

                                ASIN = v.ASIN,
                                Size = v.Size,
                                Color = v.Color,
                                StyleString = v.StyleString,
                                StyleId = findStyle?.Id,
                                StyleItemId = findStyleItem?.Id,

                                ItemPublishedStatus = (int)PublishedStatuses.New,

                                BrandName = v.BrandName,
                                Title = parentItem.AmazonName, //NOTE: v.Name is empty and always equal parent item Name
                                Barcode = barcode,

                                CreateDate = _time.GetAppNowTime()
                            };
                            db.Items.Add(newItem);
                            db.Commit();

                            var isPrime = sourceMarketplaceId == MarketplaceKeeper.DsPAWMCom
                                    && destMarket == MarketType.Walmart ? true : false;
                            var newListing = new Listing()
                            {
                                ASIN = v.ASIN,
                                Market = (int)destMarket,
                                MarketplaceId = destMarketplaceId,

                                IsPrime = isPrime,
                                SKU = v.SKU,
                                ListingId = v.ListingId,
                                ItemId = newItem.Id,
                                RealQuantity = 0,
                                CurrentPrice = v.CurrentPrice,
                            };
                            db.Listings.Add(newListing);
                            db.Commit();
                        }
                    }
                    else
                    {
                        _log.Info("ParentItem.AmazonName changed: " + existParentItem.AmazonName + " => " + parentItem.AmazonName);
                        existParentItem.AmazonName = parentItem.AmazonName;
                        db.Commit();

                        foreach (var v in parentItem.Variations)
                        {
                            var item = db.Items.GetAll().FirstOrDefault(i => i.ParentASIN == existParentItem.ASIN
                                && i.Market == (int)destMarket
                                && (String.IsNullOrEmpty(destMarketplaceId) || i.MarketplaceId == destMarketplaceId)
                                && i.Size == v.Size
                                && (i.Color == v.Color
                                    || String.IsNullOrEmpty(v.Color)));

                            if (item == null)
                            {
                                var itemsCandidats = db.Items.GetAll()
                                    .Where(i => i.ParentASIN == existParentItem.ASIN
                                        && i.Market == (int)destMarket
                                        && (String.IsNullOrEmpty(destMarketplaceId) || i.MarketplaceId == destMarketplaceId)
                                        && i.Size == v.Size)
                                    .ToList();
                                if (itemsCandidats.Count == 1)
                                    item = itemsCandidats[0];

                                if (item == null)
                                {
                                    continue;
                                    throw new Exception("Warning: missing item");
                                }
                            }
                            if (!v.StyleId.HasValue
                                || !v.StyleItemId.HasValue)
                            {
                                continue;
                                throw new Exception("Warning: missing StyleId");
                            }

                            if (String.IsNullOrEmpty(item.Barcode))
                            {
                                if (!String.IsNullOrEmpty(v.Barcode))
                                {
                                    item.Barcode = v.Barcode;
                                }
                                else
                                {
                                    var listing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == item.Id);
                                    var result = _barcodeService.AssociateBarcodes(listing?.SKU, _time.GetAppNowTime(), null);
                                    if (!String.IsNullOrEmpty(result.Barcode))
                                    {
                                        item.Barcode = result.Barcode;
                                    }
                                }
                            }

                            _log.Info("Item Name changed: " + item.Title + " => " + parentItem.AmazonName);
                            item.Title = parentItem.AmazonName;

                            db.Commit();
                        }

                        _log.Info("Already exists: " + parentItem.ASIN);
                    }
                }
            }
        }
    }
}
