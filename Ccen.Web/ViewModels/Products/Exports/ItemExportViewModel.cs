using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Items;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemExportViewModel
    {
        public enum PictureSourceTypes
        {
            FromListing = 0,
            FromStyle = 1,
        }

        public long Id { get; set; }
        public string ASIN { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string Name { get; set; }
        
        public bool CopyBulletPoints { get; set; }

        public PictureSourceTypes PictureSourceType { get; set; }

        public string ListingImage { get; set; }
        public string StyleImage { get; set; }

        public IList<ItemVariationExportViewModel> VariationList { get; set; }

        public DateTime CreateDate { get; set; }

        public ItemExportViewModel()
        {
            VariationList = new List<ItemVariationExportViewModel>();
        }
        
        public static ItemExportViewModel FromParentASIN(IUnitOfWork db, 
            string asin,
            int market,
            string marketplaceId,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            var parentItem = db.ParentItems.GetAllAsDto().FirstOrDefault(s => s.ASIN == asin
                && s.Market == market
                && s.MarketplaceId == marketplaceId);
            if (parentItem == null)
            {
                messages.Add(MessageString.Error("Parent ASIN was not found"));
                return null;
            }

            var items = db.Items.GetAllViewAsDto().Where(i => i.ParentASIN == parentItem.ASIN
                                                           && !i.IsFBA
                                                           && i.Market == parentItem.Market
                                                           && i.MarketplaceId == parentItem.MarketplaceId)
                                                           .ToList();

            foreach (var item in items)
            {
                item.BrandName = item.BrandName ?? parentItem.BrandName;
            }

            var model = new ItemExportViewModel();

            var mainItem = items.FirstOrDefault();
            StyleEntireDto style = mainItem != null && mainItem.StyleId.HasValue
                ? db.Styles.GetByStyleIdAsDto(mainItem.StyleId.Value)
                : null;

            var firstStyleString = items.Any() ? items.First().StyleString : "";

            model.ASIN = parentItem.ASIN;
            model.Id = parentItem.Id;

            model.Market = (int)market;
            model.MarketplaceId = marketplaceId;

            model.Name = parentItem.AmazonName;

            model.PictureSourceType = PictureSourceTypes.FromStyle; //NOTE: always generate excel with fresh images
            model.ListingImage = mainItem?.ImageUrl;
            model.StyleImage = style?.Image;

            model.VariationList = items
                .OrderBy(i => i.StyleId)
                .ThenBy(i => SizeHelper.GetSizeIndex(i.StyleSize))
                .Select(i => new ItemVariationExportViewModel(i)).ToList();

            return model;
        }

        public void PrepareData()
        {
            ASIN = StringHelper.TrimWhitespace(StringHelper.ToUpper(ASIN));
            Name = StringHelper.TrimWhitespace(Name);

            foreach (var item in VariationList)
            {
                item.Barcode = StringHelper.TrimWhitespace(item.Barcode);
                item.SKU = StringHelper.ToUpper(StringHelper.TrimWhitespace(item.SKU));

                item.Size = StringHelper.TrimWhitespace(item.Size);
                item.Color = StringHelper.TrimWhitespace(item.Color);

                if (String.IsNullOrEmpty(item.Size))
                    item.Size = item.StyleSize;
            }
        }

        public bool IsValid(IUnitOfWork db,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            if (Id == null || Id == 0)
            {
                //Check ASIN
                var existParent = db.ParentItems.GetByASIN(ASIN, (MarketType) Market, MarketplaceId);
                if (existParent != null)
                {
                    messages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = "Specified Parent ASIN already exists in the current marketplace"
                    });
                    return false;
                }
            }

            //Check SKU
            var skuList = VariationList
                .Where(v => !String.IsNullOrEmpty(v.SKU) && v.IsSelected)
                .Select(v => v.SKU)
                .ToList();

            var notSelectedItemIds = VariationList.Where(v => !v.IsSelected && v.Id.HasValue).Select(v => v.Id).ToList();

            var existSKUs = db.Listings.GetAll().Where(l => !l.IsRemoved
                                                            && skuList.Contains(l.SKU)
                                                            && !notSelectedItemIds.Contains(l.ItemId)
                                                            && l.Market == Market
                                                            && l.MarketplaceId == MarketplaceId).ToList();

            foreach (var item in VariationList.Where(v => v.IsSelected))
            {
                if (!String.IsNullOrEmpty(item.SKU))
                {
                    var existItemSKU = existSKUs.Where(s => s.ItemId != item.Id && s.SKU == item.SKU).Take(1).ToList();

                    var skuMessages = new List<MessageString>();
                    existItemSKU.ForEach(s => skuMessages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = String.Format("The variation SKU: \"{0}\" already exists in current marketplace", s.SKU)
                    }));
                    messages.AddRange(skuMessages);
                }
            }

            //Check Barcode
            CheckBarcodes(db, (MarketType)Market, MarketplaceId, VariationList, messages, false, false);
            CheckBarcodes(db, (MarketType)Market, MarketplaceId, VariationList, messages, true, false);
            CheckBarcodes(db, (MarketType)Market, MarketplaceId, VariationList, messages, false, true);

            return !messages.Any();
        }

        public static void CheckBarcodes(IUnitOfWork db,
            MarketType market,
            string marketplaceId,
            IList<ItemVariationExportViewModel> variations,
            IList<MessageString> messages,
            bool isPrime,
            bool isFBA)
        {
            var notSelectedIds = variations.Where(v => !v.IsSelected && v.Id.HasValue).Select(v => v.Id).ToList();
            variations = variations.Where(v => v.IsSelected).ToList();

            var checkingVariations = variations.Where(v => v.IsPrime == isPrime && v.IsFBA == isFBA).ToList();

            var barcodeListQuery = checkingVariations.Where(v => !String.IsNullOrEmpty(v.Barcode)
                    && !v.AutoGeneratedBarcode);

            var barcodeList = barcodeListQuery.Select(v => v.Barcode).ToList();

            var existBarcodes = db.Items.GetAllViewActual().Where(l => barcodeList.Contains(l.Barcode)
                                                            && l.Market == (int)market
                                                            && l.MarketplaceId == marketplaceId
                                                            && l.IsPrime == isPrime
                                                            && l.IsFBA == isFBA
                                                            && !notSelectedIds.Contains(l.Id)) //NOTE: Exclude not selected
                                                            .Select(b => new
                                                            {
                                                                Id = b.Id,
                                                                Barcode = b.Barcode,
                                                                SKU = b.SKU
                                                            })
                                                            .ToList();

            foreach (var item in checkingVariations)
            {
                if (!String.IsNullOrEmpty(item.Barcode))
                {
                    var existItemBarcode = existBarcodes.Where(s => s.Id != item.Id 
                        && s.Barcode == item.Barcode).Take(1).ToList();

                    var barcodeMessages = new List<MessageString>();
                    existItemBarcode.ForEach(s => barcodeMessages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = String.Format("SKU: \"{0}\": the variation Barcode: \"{1}\" already exists in SKU: \"{2}\"", item.SKU, item.Barcode, s.SKU)
                    }));
                    messages.AddRange(barcodeMessages);
                }
            }
        }

        public static IList<ItemVariationExportViewModel> CreateStyleVariations(IUnitOfWork db,
            string styleString,
            IList<ItemSizeMapping> existSizeMapping,
            MarketType market,
            string marketplaceId)
        {
            var results = new List<ItemVariationExportViewModel>();

            var style = db.Styles.GetActiveByStyleIdAsDto(styleString);

            if (style == null)
                return results;

            var styleMainLicenseDto = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(style.Id,
                StyleFeatureHelper.MAIN_LICENSE);
            var styleSubLicenseDto = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(style.Id,
                StyleFeatureHelper.SUB_LICENSE1);
            var styleMainLicense = styleMainLicenseDto != null ? styleMainLicenseDto.Value : null;
            var styleSubLicense = styleSubLicenseDto != null ? styleSubLicenseDto.Value : null;

            var styleItems = db.StyleItems
                .GetByStyleIdWithBarcodesAsDto(style.Id)
                .OrderBy(o => SizeHelper.GetSizeIndex(o.Size))
                .ToList();

            var forceReplace = styleItems.Any(s => (s.Size ?? "").Contains("/"));

            foreach (var styleItem in styleItems)
            {
                var newItem = new ItemVariationExportViewModel();

                var index = 0;
                var baseSKU = style.StyleID + "-" + SizeHelper.PrepareSizeForSKU(styleItem.Size, forceReplace);

                while (db.Listings.CheckForExistenceSKU(SkuHelper.SetSKUMiddleIndex(baseSKU, index),
                    (MarketType)market,
                    marketplaceId))
                    index++;

                newItem.IsSelected = true;
                newItem.SKU = SkuHelper.SetSKUMiddleIndex(baseSKU, index);
                newItem.StyleId = styleItem.StyleId;
                newItem.StyleString = style.StyleID;
                newItem.StyleItemId = styleItem.StyleItemId;
                newItem.StyleColor = styleItem.Color;
                newItem.StyleSize = styleItem.Size;

                newItem.BrandName = ItemExportHelper.GetBrandName(styleMainLicense, styleSubLicense);
                newItem.Size = SizeHelper.PrepareSizeForExport(db, newItem.StyleSize, existSizeMapping);

                if (styleItem.Barcodes != null)
                {
                    foreach (var barcode in styleItem.Barcodes)
                    {
                        if (!String.IsNullOrEmpty(barcode.Barcode)
                            && !db.Items.CheckForExistenceBarcode(barcode.Barcode, market, marketplaceId))
                        {
                            newItem.Barcode = barcode.Barcode;
                            break;
                        }
                    }
                }
                if (String.IsNullOrEmpty(newItem.Barcode))
                    newItem.AutoGeneratedBarcode = true;

                results.Add(newItem);
            }

            return results;
        }

        public static string GetUnusedBarcodeForStyleItem(IUnitOfWork db, 
            long styleItemId,
            MarketType market,
            string marketplaceId)
        {
            var barcodes = db.StyleItemBarcodes.GetByStyleItemId(styleItemId);
            if (barcodes != null)
            {
                foreach (var barcode in barcodes)
                {
                    if (!String.IsNullOrEmpty(barcode.Barcode)
                        && !db.Items.CheckForExistenceBarcode(barcode.Barcode, market, marketplaceId))
                    {
                        return barcode.Barcode;
                    }
                }
            }
            return null;
        }

        public string Export(IUnitOfWork db,
            ITime time,
            ILogService log,
            IBarcodeService barcodeService,
            IMarketCategoryService categoryService,
            IMarketplaceService marketplaceService,
            DateTime when, 
            long? by)
        {
            log.Info("Export, parentId=" + Id + ", market=" + Market + ", marketplaceId=" + MarketplaceId);

            string filename = null;
            var parent = db.ParentItems.GetAsDTO((int)Id);
            var parentImage = db.ParentItemImages.GetAllAsDto().FirstOrDefault(pi => pi.ItemId == parent.Id);
            parent.LargeImage = parentImage;

            var parentChildren = db.Items.GetAllActualExAsDto().Where(i => i.ParentASIN == parent.ASIN  //NOTE: use original ASIN
                && i.Market == (int)parent.Market
                && i.MarketplaceId == parent.MarketplaceId).ToList();
            //if (exportMode != ExportToExcelMode.FBA) //TASK: Generate excel only for listing that haven't FBA
            parentChildren = parentChildren.Where(i => !i.IsFBA).ToList();
            parentChildren = parentChildren
                .OrderBy(ch => ch.StyleString)
                .ThenBy(ch => SizeHelper.GetSizeIndex(ch.Size))
                .ToList();
            
            var children = VariationList
                .Where(v => v.IsSelected
                    && !String.IsNullOrEmpty(v.StyleString))
                .Select(v => new ItemExDTO()
            {
                StyleString = v.StyleString,
                StyleId = v.StyleId,
                StyleItemId = v.StyleItemId,
                Barcode = v.Barcode,
                SKU = v.SKU,
                Size = v.Size,
                Color = v.Color,
                CurrentPrice = v.Price,
            }).ToList();

            parent.ASIN = ASIN;
            parent.AmazonName = Name;

            var styleStringList = children.Select(ch => ch.StyleString).Distinct().ToList();
            var styles = db.Styles.GetAllActive().Where(s => styleStringList.Contains(s.StyleID)).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var style = styles.FirstOrDefault(s => s.StyleID == children[i].StyleString);
                if (style != null)
                    children[i].StyleId = style.Id;

                var existParentChild = parentChildren.FirstOrDefault(p => String.Compare(p.SKU, children[i].SKU, StringComparison.OrdinalIgnoreCase) == 0);
                if (existParentChild != null)
                {
                    children[i].Id = existParentChild.Id;
                    children[i].ASIN = existParentChild.ASIN;
                    
                    children[i].Name = existParentChild.Name;
                    children[i].ImageUrl = existParentChild.ImageUrl;
                    children[i].IsExistOnAmazon = existParentChild.IsExistOnAmazon;

                    children[i].ListPrice = existParentChild.ListPrice;
                    children[i].BrandName = existParentChild.BrandName;
                    children[i].SpecialSize = existParentChild.SpecialSize;
                    children[i].Features = existParentChild.Features;
                    children[i].Department = existParentChild.Department;

                    children[i].RealQuantity = existParentChild.RealQuantity;
                    children[i].Weight = existParentChild.Weight; //NOTE: only for FBA
                    children[i].IsPrime = existParentChild.IsPrime;
                }
            }

            var resultRecords = ExcelProductUSViewModel.GetItemsFor(db,
                categoryService,
                ExportToExcelMode.Normal, 
                null, 
                parent, 
                children,
                PictureSourceType == PictureSourceTypes.FromStyle ? UseStyleImageModes.StyleImage : UseStyleImageModes.ListingImage, 
                out filename);

            //NOTE: mark already exist listings as PartialUpdate
            foreach (var resultRecord in resultRecords)
            {
                if (resultRecord.Id > 0
                    && resultRecord.Parentage == ExcelHelper.ParentageChild
                    && resultRecord.IsExistOnAmazon)
                {
                    resultRecord.Update = "PartialUpdate";
                    resultRecord.Quantity = null;
                    resultRecord.StandardPrice = null;
                    //resultRecord.SuggestedPrice = null;
                    //resultRecord.MainImageURL = null;
                    //resultRecord.OtherImageUrl1 = null;
                    //resultRecord.OtherImageUrl2 = null;
                    //resultRecord.OtherImageUrl3 = null;
                }
            }

            var firstResult = resultRecords.FirstOrDefault(r => r.Parentage == ExcelHelper.ParentageChild);
            if (firstResult != null)
            {
                foreach (var resultRecord in resultRecords)
                {
                    if (CopyBulletPoints
                        && resultRecord.Id == 0
                        && resultRecord.Parentage == ExcelHelper.ParentageChild)
                    {
                        resultRecord.KeyProductFeatures1 = firstResult.KeyProductFeatures1;
                        resultRecord.KeyProductFeatures2 = firstResult.KeyProductFeatures2;
                        resultRecord.KeyProductFeatures3 = firstResult.KeyProductFeatures3;
                        resultRecord.KeyProductFeatures4 = firstResult.KeyProductFeatures4;
                        resultRecord.KeyProductFeatures5 = firstResult.KeyProductFeatures5;

                        resultRecord.SearchTerms1 = firstResult.SearchTerms1;
                    }
                }
            }

            var styleStringToGenerate = children.Where(ch => ch.Id == 0).Select(ch => ch.StyleString).Distinct().ToList();
            foreach (var styleString in styleStringToGenerate)
            {
                var styleDto = db.Styles.GetActiveByStyleIdAsDto(styleString);
                var styleModel = new StyleViewModel(db, marketplaceService, styleDto);
                //Update Barcode and AutoGenerateBarcode flag, other info already exists in resultRecords
                foreach (var si in styleModel.StyleItems.Items)
                {
                    var variationItem = VariationList.FirstOrDefault(v => v.StyleItemId == si.Id);
                    if (variationItem != null)
                    {
                        if (!String.IsNullOrEmpty(variationItem.Barcode))
                            si.Barcodes = new[] { new BarcodeDTO() { Barcode = variationItem.Barcode } };
                        si.AutoGeneratedBarcode = variationItem.AutoGeneratedBarcode;
                    }
                }

                string tempFilename = null;
                var styleResultRecords = ExcelProductUSViewModel.GenerateToExcelUS(db, barcodeService, categoryService, styleModel, when, out tempFilename);

                foreach (var styleResultRecord in styleResultRecords)
                {
                    if (!styleResultRecord.StyleItemId.HasValue)
                        continue;

                    var resultRecord = resultRecords.FirstOrDefault(s => s.StyleItemId == styleResultRecord.StyleItemId);
                    if (resultRecord != null)
                    {
                        resultRecord.ASIN = styleResultRecord.ASIN;
                        resultRecord.ProductId = styleResultRecord.ProductId;

                        resultRecord.Title = styleResultRecord.Title;
                        resultRecord.Description = styleResultRecord.Description;
                        resultRecord.BrandName = styleResultRecord.BrandName;
                        resultRecord.Type = styleResultRecord.Type;
                        resultRecord.MainImageURL = styleResultRecord.MainImageURL;
                        resultRecord.OtherImageUrl1 = styleResultRecord.OtherImageUrl1;
                        resultRecord.OtherImageUrl2 = styleResultRecord.OtherImageUrl2;
                        resultRecord.OtherImageUrl3 = styleResultRecord.OtherImageUrl3;
                        resultRecord.Department = styleResultRecord.Department;

                        resultRecord.SuggestedPrice = styleResultRecord.SuggestedPrice;
                        resultRecord.Quantity = styleResultRecord.Quantity;

                        if (!CopyBulletPoints)
                        {
                            resultRecord.KeyProductFeatures1 = styleResultRecord.KeyProductFeatures1;
                            resultRecord.KeyProductFeatures2 = styleResultRecord.KeyProductFeatures2;
                            resultRecord.KeyProductFeatures3 = styleResultRecord.KeyProductFeatures3;
                            resultRecord.KeyProductFeatures4 = styleResultRecord.KeyProductFeatures4;
                            resultRecord.KeyProductFeatures5 = styleResultRecord.KeyProductFeatures5;

                            resultRecord.SearchTerms1 = styleResultRecord.SearchTerms1;
                        }
                    }
                }
            }


            var stream = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelProductUSViewModel.USTemplatePath),
                "Template",
                resultRecords);

            var filePath = UrlHelper.GetProductTemplateFilePath(filename);
            using (var file = File.Open(filePath, FileMode.Create))
            {
                stream.WriteTo(file);
            }
            return UrlHelper.GetProductTemplateUrl(filename);
        }
        
    }
}