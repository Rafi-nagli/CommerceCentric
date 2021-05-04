using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Products;
using Ccen.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleQuantityViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public int Type { get; set; }

        public int ItemType { get; set; }

        public string Image { get; set; }
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Image, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
            }
        }


        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public IList<SizeQuantityViewModel> Sizes { get; set; }
        public IList<ListingQuantityViewModel> Listings { get; set; }
        public List<LocationViewModel> Locations { get; set; }


        public override string ToString()
        {
            var text = "Id=" + Id
                       + ", StyleId=" + StyleId
                       + ", Type=" + Type
                       + ", ItemType=" + ItemType
                       + ", UpdateDate=" + UpdateDate
                       + ", CreateDate=" + CreateDate;
            if (Sizes != null)
            {
                foreach (var size in Sizes)
                    text += "\r\n Size=" + size.Size
                            + ", NewManuallyQuantity=" + size.NewManuallyQuantity
                            + ", UseBoxQuantity=" + size.UseBoxQuantity
                            + ", NewRestockDate=" + size.NewRestockDate
                            + ", OnHold=" + size.OnHold
                            + ", IsRemoveRestockDate=" + size.IsRemoveRestockDate;
            }
            return text;
        }

        public StyleQuantityViewModel()
        {
            Sizes = new List<SizeQuantityViewModel>();
            Locations = new List<LocationViewModel>();
        }


        public StyleQuantityViewModel(IUnitOfWork db, long styleId, DateTime when)
        {
            var style = db.Styles.Get(styleId);
            var itemTypeId = style.ItemTypeId ?? StyleViewModel.DefaultItemType;

            StyleId = styleId;
            StyleString = style.StyleID;

            Type = style.Type;

            Image = style.Image;
            ItemType = itemTypeId;

            //Sizes
            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                .ToList();
            var styleItemCache = db.StyleItemCaches.GetForStyleId(styleId).ToList();

            Sizes = BuildSizes(styleSizes,
                styleItemCache);

            Listings = db.Listings.GetListingsAsListingDto()
                .Where(l => 
                    l.StyleId == styleId
                    && !l.IsFBA)
                .ToList()
                .Select(l => new ListingQuantityViewModel(l))
                .OrderBy(l => l.SizeIndex)
                .ThenBy(l => l.Market)
                .ThenBy(l => l.SKU)
                .ToList();

            Locations = StyleViewModel.GetLocations(db, style.Id);
        }

        public static void ValidateGenerateOpenBox(IUnitOfWork db, long styleId, out List<MessageString> messages)
        {
            messages = new List<MessageString>();
            var openBoxes = db.OpenBoxes.GetFiltered(b => b.StyleId == styleId && !b.Archived).Any();
            var sealedBoxes = db.SealedBoxes.GetFiltered(b => b.StyleId == styleId && !b.Archived).Any();

            if (openBoxes || sealedBoxes)
            {
                messages.Add(new MessageString()
                {
                    Message = "Generation only available for styles that haven't active boxes",
                    Status = MessageStatus.Error
                });
            }
        }

        public static void GenerateOpenBox(IUnitOfWork db,
            ICacheService cache,
            IQuantityManager quantityManager,
            long styleId,
            DateTime when,
            long? by)
        {
            var styleItemCaches = db.StyleItemCaches.GetFiltered(si => si.StyleId == styleId).ToList();
            var openBox = new OpenBox();
            openBox.BoxBarcode = "Generated_" + when.ToString("yyyyMMdd");
            openBox.StyleId = styleId;
            openBox.BoxQuantity = 1;
            openBox.CreateDate = when;
            openBox.CreatedBy = by;
            
            db.OpenBoxes.Add(openBox);
            db.Commit();

            foreach (var styleItem in styleItemCaches)
            {
                var openBoxItem = new OpenBoxItem()
                {
                    BoxId = openBox.Id,
                    StyleItemId = styleItem.Id,
                    Quantity = styleItem.RemainingQuantity,
                    CreateDate = when,
                    CreatedBy = by
                };
                db.OpenBoxItems.Add(openBoxItem);
            }
            db.Commit();

            var styleItems = db.StyleItems.GetFiltered(si => si.StyleId == styleId).ToList();
            foreach (var styleItem in styleItems)
            {
                var oldQuantity = styleItem.Quantity;
                var styleItemCache = styleItemCaches.FirstOrDefault(sic => sic.Id == styleItem.Id);
                int? newQuantity = styleItemCache != null ? styleItemCache.RemainingQuantity : (int?)null;

                styleItem.Quantity = null;
                styleItem.QuantitySetDate = null;
                styleItem.QuantitySetBy = null;
                styleItem.RestockDate = null;
                
                quantityManager.LogStyleItemQuantity(db,
                    styleItem.Id,
                    newQuantity,
                    oldQuantity,
                    QuantityChangeSourceType.UseBoxQuantity, 
                    null,
                    null,
                    null,
                    when,
                    by);

                db.Commit();
            }

            db.Styles.SetReSaveDate(styleId, when, by);

            if (AppSettings.IsDebug)
            {
                cache.UpdateStyleCacheForStyleId(db, new List<long>() { styleId });
                cache.UpdateStyleItemCacheForStyleId(db, new List<long>() { styleId });
            }
            else
            {
                cache.RequestStyleIdUpdates(db,
                    new List<long>() {styleId},
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);
            }
        }

        public List<MessageString> Validate(IUnitOfWork db,
           ILogService log,
           DateTime when)
        {
            var messages = new List<MessageString>();
            var fromDate = when.AddDays(-90);

            var message = "";
            foreach (var size in Sizes)
            {
                if (size.NewManuallyQuantity.HasValue && size.NewManuallyQuantity >= 0)
                {
                    var lastChange = db.StyleItemQuantityHistories.GetAll().FirstOrDefault(h => h.StyleItemId == size.StyleItemId
                                                                                   && h.Type == (int) QuantityChangeSourceType.EnterNewQuantity
                                                                                   && h.Tag == size.NewManuallyQuantity.ToString() 
                                                                                   && h.CreateDate >= fromDate);
                    if (lastChange != null)
                    {
                        message += "The same quantity change: +" + size.NewManuallyQuantity + " for size \"" + size.Size +
                                   "\" was entered earlier on " + DateHelper.ToDateString(lastChange.CreateDate) +
                                   ".<br/>";
                    }
                }
            }

            if (!String.IsNullOrEmpty(message))
            {
                messages.Add(new MessageString()
                {
                    Message = message + "Are you sure you would like to save these changes?",
                    Status = MessageStatus.Info,
                });
            }

            return messages;
        }

        public long Apply(IUnitOfWork db, 
            ICacheService cache,
            IQuantityManager quantityManager,
            IStyleHistoryService styleHistory,
            ISystemActionService actionService,
            DateTime when, 
            long? by)
        {
            var style = db.Styles.Get(StyleId);

            style.UpdateDate = when;
            style.UpdatedBy = by;

            style.ReSaveDate = when;
            style.ReSaveBy = by;

            StyleViewModel.UpdateLocations(db, styleHistory, StyleId, Locations, when, by);

            var wasAnyChanges = false;
            if (Sizes != null && Sizes.Any())
            {
                var styleItems = db.StyleItems.GetFiltered(si => si.StyleId == StyleId).ToList();
                var styleItemCaches = db.StyleItemCaches.GetFiltered(si => si.StyleId == StyleId).ToList(); 

                foreach (var size in Sizes)  //Update quantity (marking when/by)
                {
                    //int? oldQuantity = null;
                    //int? newQuantity = null;
                    string tag = null;
                    bool wasChanged = false;

                    var styleItem = styleItems.FirstOrDefault(si => si.Id == size.StyleItemId);
                    var styleItemCache = styleItemCaches.FirstOrDefault(sic => sic.Id == size.StyleItemId);
                    
                    if (styleItem != null)
                    {
                        if (size.UseBoxQuantity)
                        {
                            if (styleItem.Quantity != null)
                            {
                                var oldQuantity = styleItem.Quantity;
                                var newQuantity = size.BoxQuantity;
                                tag = size.BoxQuantitySetDate.ToString();

                                styleItem.Quantity = null;
                                styleItem.QuantitySetDate = null;
                                styleItem.QuantitySetBy = null;
                                //styleItem.RestockDate = null;
                                wasChanged = true;

                                quantityManager.LogStyleItemQuantity(db,
                                    styleItem.Id,
                                    newQuantity,
                                    oldQuantity,
                                    QuantityChangeSourceType.UseBoxQuantity,
                                    tag,
                                    null,
                                    null,
                                    when,
                                    by);
                            }
                        }

                        if (size.NewRestockDate.HasValue
                            && styleItem.RestockDate != size.NewRestockDate)
                        {
                            styleHistory.AddRecord(styleItem.StyleId,
                                StyleHistoryHelper.RestockDateKey,
                                DateHelper.ToDateTimeString(styleItem.RestockDate),
                                StringHelper.JoinTwo("-", styleItem.Size, styleItem.Color),
                                DateHelper.ToDateTimeString(size.NewRestockDate),
                                styleItem.Id.ToString(),
                                by);

                            styleItem.RestockDate = size.NewRestockDate;
                            wasChanged = true;
                        }
                        
                        if (size.NewManuallyQuantity.HasValue)
                        {
                            var operationType = size.NewManuallyQuantity.Value < 0 ? QuantityOperationType.Lost : QuantityOperationType.AddManually;

                            var quantityOperation = new QuantityOperationDTO()
                            {
                                Type = (int)operationType,
                                QuantityChanges = new List<QuantityChangeDTO>()
                                {
                                    new QuantityChangeDTO()
                                    {
                                        StyleId = style.Id,
                                        StyleItemId = styleItem.Id,
                                        Quantity = -1 * size.NewManuallyQuantity.Value, 
                                        //NOTE: we need to change sign to opposite because we substract quantity operataions from inventory                                        
                                    }
                                },
                                Comment = "From style quantity dialog",
                            };

                            quantityManager.AddQuantityOperation(db,
                                quantityOperation,
                                when,
                                by);

                            //NOTE: Hot updating the cache (only for first few seconds to display the updates before recalculation)
                            if (styleItemCache != null)
                            {
                                styleItemCache.SpecialCaseQuantityFromDate += -1 * size.NewManuallyQuantity.Value;
                                styleItemCache.TotalSpecialCaseQuantity += -1 * size.NewManuallyQuantity.Value;
                            }

                            wasChanged = true;
                        }

                        if (size.IsRemoveRestockDate == true)
                        {
                            styleItem.RestockDate = null;

                            wasChanged = true;
                        }

                        if (size.OnHold != styleItem.OnHold)
                        {
                            quantityManager.LogStyleItemQuantity(db,
                                styleItem.Id,
                                size.OnHold ? 0 : styleItem.Quantity,
                                size.OnHold ? styleItem.Quantity : 0,
                                QuantityChangeSourceType.OnHold,
                                size.OnHold.ToString(),
                                null,
                                null,
                                when,
                                by);
                            styleItem.OnHold = size.OnHold;

                            wasChanged = true;
                        }
                    }

                    if (wasChanged)
                    {
                        db.Commit();
                        wasAnyChanges = true;
                    }   
                }
            }

            //NOTE: always update cache
            cache.RequestStyleIdUpdates(db,
                new List<long>() { StyleId },
                UpdateCacheMode.IncludeChild,
                AccessManager.UserId);

            if (wasAnyChanges)
            {
                db.Commit();

                SystemActionHelper.RequestQuantityDistribution(db, actionService, StyleId, by);
            }

            return StyleId;
        }


        private IList<SizeQuantityViewModel> BuildSizes(
            IList<StyleItemDTO> styleItems,
            IList<StyleItemCacheDTO> styleItemCaches)
        {
            var resultSizes = new List<SizeQuantityViewModel>();

            foreach (var styleItem in styleItems)
            {
                var styleItemCache = styleItemCaches.FirstOrDefault(sic => sic.Id == styleItem.StyleItemId);
                
                var size = new SizeQuantityViewModel();
                size.StyleItemId = styleItem.StyleItemId;
                size.SizeGroupName = styleItem.SizeGroupName;
                size.Size = styleItem.Size;
                size.Color = styleItem.Color;
                
                size.ManuallyQuantitySetDate = styleItem.QuantitySetDate;
                size.ManuallyQuantity = styleItem.Quantity;

                size.RestockDate = styleItem.RestockDate;
                size.OnHold = styleItem.OnHold;

                if (styleItemCache != null)
                {
                    size.TotalMarketsSoldQuantity = styleItemCache.TotalMarketsSoldQuantity;
                    size.TotalScannedSoldQuantity = styleItemCache.TotalScannedSoldQuantity;
                    size.TotalSentToFBAQuantity = styleItemCache.TotalSentToFBAQuantity;
                    size.TotalSpecialCaseQuantity = styleItemCache.TotalSpecialCaseQuantity;
                    size.TotalSentToPhotoshootQuantity = styleItemCache.TotalSentToPhotoshootQuantity;

                    size.BoxQuantity = styleItemCache.BoxQuantity ?? 0;
                    size.BoxQuantitySetDate = styleItemCache.BoxQuantitySetDate;

                    //NOTE: no needed already have actual (not cached value)
                    //size.DirectQuantity = styleItemCache.DirectQuantity;
                    //size.DirectQuantitySetDate = styleItemCache.DirectQuantitySetDate;

                    if (size.HasManullyQuantity)
                    {
                        size.UseBoxQuantity = false;
                        size.ManuallySoldQuantity = styleItemCache.MarketsSoldQuantityFromDate;
                        size.ManuallySoldQuantity += styleItemCache.ScannedSoldQuantityFromDate;
                        size.ManuallySoldQuantity += styleItemCache.SentToFBAQuantityFromDate;
                        size.ManuallySoldQuantity += styleItemCache.SpecialCaseQuantityFromDate;
                        size.ManuallySoldQuantity += styleItemCache.SentToPhotoshootQuantityFromDate;
                    }
                    else
                    {
                        size.UseBoxQuantity = true;
                        size.BoxSoldQuantity = styleItemCache.MarketsSoldQuantityFromDate;
                        size.BoxSoldQuantity += styleItemCache.ScannedSoldQuantityFromDate;
                        size.BoxSoldQuantity += styleItemCache.SentToFBAQuantityFromDate;
                        size.BoxSoldQuantity += styleItemCache.SpecialCaseQuantityFromDate;
                        size.BoxSoldQuantity += styleItemCache.SentToPhotoshootQuantityFromDate;
                    }
                }
                else
                {
                    if (size.HasManullyQuantity)
                        size.UseBoxQuantity = false;
                    else
                        size.UseBoxQuantity = true;
                }

                resultSizes.Add(size);
            }

            return resultSizes
                .OrderBy(s => SizeHelper.GetSizeIndex(s.Size))
                .ThenBy(s => s.Color)
                .ToList();
        }
    }
}