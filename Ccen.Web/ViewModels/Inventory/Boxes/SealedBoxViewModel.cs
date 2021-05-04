using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory
{
    public class SealedBoxViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public string BoxBarcode { get; set; }
        public string Breakdown { get; set; }
        public int BoxQuantity { get; set; }
        public int BoxSealedQuantity { get; set; }
        public int BoxItemsQuantity { get; set; }
        public bool Printed { get; set; }
        public bool PolyBags { get; set; }

        public int TotalBoxesQuantity
        {
            get { return BoxItemsQuantity*BoxQuantity; }
        }
        public bool Owned { get; set; }

        public bool Archived { get; set; }

        public decimal Price { get; set; }

        public DateTime? OriginCreateDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? CreateDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CreateDate); }
            set { CreateDate = DateHelper.ConvertUtcToApp(value); }
        }

        public string CreateDateFormatted
        {
            get { return CreateDate.HasValue ? CreateDate.Value.ToString(DateHelper.DateFormat) : "-"; }
        }

        public string OriginCreateDateFormatted
        {
            get { return OriginCreateDate.HasValue ? OriginCreateDate.Value.ToString(DateHelper.DateFormat) : "-"; }
        }

        public StyleItemCollection StyleItems { get; set; }


        public override string ToString()
        {
            return "Id=" + Id 
                + ", StyleId=" + StyleId 
                + ", StyleString=" + StyleString
                + ", BoxBarcode=" + BoxBarcode 
                + ", Breakdown=" + Breakdown
                + ", BoxQuantity=" + BoxQuantity 
                + ", BoxSealedQuantity=" + BoxSealedQuantity 
                + ", BoxItemsQuantity=" + BoxItemsQuantity
                + ", Printed=" + Printed 
                + ", PolyBags=" + PolyBags 
                + ", Owned=" + Owned 
                + ", Price=" + Price 
                + ", Archived=" + Archived;
        } 

        public SealedBoxViewModel()
        {
            Owned = true;

            StyleItems = new StyleItemCollection(StyleItemDisplayMode.BoxBreakdown);
        }

        public static SealedBoxViewModel BuildFromBoxId(IUnitOfWork db, long sealedBoxId)
        {
            var model = new SealedBoxViewModel();

            var item = db.SealedBoxes.GetAllAsDto().FirstOrDefault(b => b.Id == sealedBoxId);
            var style = db.Styles.GetByStyleIdAsDto(item.StyleId);
            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(item.StyleId).ToList();
            var boxSizeItems = db.SealedBoxItems.GetByBoxIdAsDto(sealedBoxId).ToList();

            model.ConstructFrom(item, style, styleItems, boxSizeItems);

            return model;
        }
        
        public static SealedBoxViewModel BuildFromStyleId(IUnitOfWork db, long styleId)
        {
            var model = new SealedBoxViewModel();

            var style = db.Styles.GetByStyleIdAsDto(styleId);
            model.StyleString = style.StyleID;

            model.StyleId = styleId;
            model.Owned = true;

            model.Price = db.OpenBoxes.GetByStyleId(styleId).OrderBy(b => b.Id).Select(b => b.Price).FirstOrDefault();

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .ToList();

            model.StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxBreakdown,
                Items = styleItems.Select(si => new StyleItemViewModel(si)).ToList()
            };
            SetDefaultBreakdowns(model.StyleItems.Items.ToList());

            model.CreateDate = DateHelper.GetAppNowTime().Date;

            model.BoxBarcode = db.SealedBoxes.GetDefaultBoxName(styleId, model.CreateDate.Value);

            return model;
        }

        public SealedBoxViewModel(SealedBoxDto item,
            StyleEntireDto style,
            List<StyleItemDTO> styleItems,
            List<SealedBoxItemDto> boxItems)
        {
            ConstructFrom(item, style, styleItems, boxItems);
        }

        private void ConstructFrom(SealedBoxDto item,
            StyleEntireDto style,
            List<StyleItemDTO> styleItems,
            List<SealedBoxItemDto> boxItems)
        {
            Id = item.Id;
            StyleId = item.StyleId;
            if (style != null)
                StyleString = style.StyleID;

            BoxBarcode = item.BoxBarcode;
            Printed = item.Printed;
            PolyBags = item.PolyBags;
            BoxQuantity = item.BoxQuantity;
            Owned = item.Owned;
            Price = item.PajamaPrice;
            Archived = item.Archived;
            UpdateDate = item.UpdateDate;
            CreateDate = item.CreateDate;
            OriginCreateDate = item.OriginCreateDate;

            UpdatedByName = item.UpdatedByName;

            BoxItemsQuantity = boxItems.Sum(b => b.BreakDown);

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxBreakdown,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si)).ToList()
            };

            //Set boxes values
            foreach (var boxItem in boxItems)
            {
                var styleItem = StyleItems.Items.FirstOrDefault(si => si.Id == boxItem.StyleItemId);
                if (styleItem != null)
                {
                    styleItem.Breakdown = boxItem.BreakDown;
                    styleItem.BoxItemId = boxItem.Id;
                }
            }

            Breakdown = string.Join("-", StyleItems.Items
                            .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                            .ThenBy(si => si.Color)
                            .Where(si => si.Breakdown.HasValue)
                    .Select(si => si.Breakdown).ToList());
        }

        public static IList<SealedBoxViewModel> GetAll(IUnitOfWork db, long styleId, bool includeArchive)
        {
            var boxQuery = db.SealedBoxes.GetAllAsDto().Where(b => b.StyleId == styleId);
            if (!includeArchive)
            {
                boxQuery = boxQuery.Where(b => !b.Archived);
            }
            var boxList = boxQuery.ToList();
            
            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                .ThenBy(si => si.Color)
                .ToList();
            
            return boxList.Select(box => new SealedBoxViewModel(box, 
                    null,
                    styleSizes, 
                    db.SealedBoxItems.GetByBoxIdAsDto(box.Id)
                        .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                        .ThenBy(si => si.Color)
                        .ToList()))
                .ToList();
        }

        public long Apply(IUnitOfWork db,
            IQuantityManager quantityManager,
            DateTime when, 
            long? by)
        {
            var existBox = db.SealedBoxes.GetFiltered(b => b.Id == Id && !b.Deleted).FirstOrDefault();
            long? boxId = null;

            if (existBox == null)
            {
                boxId = AddNewBox(db, when, by);
            }
            else
            {
                UpdateExistingBox(db, existBox, when, by);
                boxId = existBox.Id;
            }

            var boxItems = StyleItems.Items
                .Where(si => si.Breakdown.HasValue)
                .Select(si => new SealedBoxItemDto()
                {
                    Id = si.BoxItemId ?? 0,
                    StyleItemId = si.Id,
                    BreakDown = si.Breakdown ?? 0,
                }).ToList();


            var updateResults = db.SealedBoxItems.UpdateBoxItemsForBox(boxId.Value, boxItems, when, by);

            foreach (var updateResult in updateResults)
            {
                var boxItem = boxItems.FirstOrDefault(bi => bi.Id == updateResult.Id);
                if (boxItem != null)
                {
                    var changeType = QuantityChangeSourceType.None;
                    if (updateResult.Status == UpdateType.Update)
                        changeType = QuantityChangeSourceType.ChangeBox;
                    if (updateResult.Status == UpdateType.Insert)
                        changeType = QuantityChangeSourceType.AddNewBox;
                    if (updateResult.Status == UpdateType.Removed)
                        changeType = QuantityChangeSourceType.RemoveBox;

                    quantityManager.LogStyleItemQuantity(db,
                        boxItem.StyleItemId,
                        changeType != QuantityChangeSourceType.RemoveBox ? boxItem.BreakDown * BoxQuantity : (int?)null,
                        changeType != QuantityChangeSourceType.AddNewBox ? boxItem.BreakDown * BoxQuantity : (int?)null,
                        changeType,
                        null,
                        boxItem.Id,
                        existBox?.BoxBarcode,
                        when,
                        by);
                }
            }

            var id = db.Styles.GetFiltered(s => s.Id == StyleId).Select(s => s.Id).FirstOrDefault();
            return id;
        }

        public static void Remove(IUnitOfWork db,
                long id,
                IQuantityManager quantityManager,
                ICacheService cache,
                DateTime when,
                long? by)
        {
            var box = db.SealedBoxes.Get(id);
            box.Deleted = true;
            box.UpdateDate = when;
            box.UpdatedBy = by;
            db.Commit();

            var openBoxItems = db.SealedBoxItems.GetAll().Where(bi => bi.BoxId == id).ToList();
            foreach (var boxItem in openBoxItems)
            {
                quantityManager.LogStyleItemQuantity(db,
                    boxItem.StyleItemId ?? 0,
                    null,
                    boxItem.BreakDown * box.BoxQuantity,
                    QuantityChangeSourceType.RemoveBox,
                    null,
                    boxItem.Id,
                    box?.BoxBarcode,
                    when,
                    by);
            }

            cache.RequestStyleIdUpdates(db,
                    new List<long>() { box.StyleId },
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);
        }

        private void UpdateExistingBox(IUnitOfWork db, 
            SealedBox dbBox, 
            DateTime when,
            long? by)
        {
            dbBox.StyleId = StyleId;
            dbBox.BoxBarcode = BoxBarcode;
            dbBox.BoxQuantity = BoxQuantity;
            dbBox.PajamaPrice = Price;
            dbBox.Printed = Printed;
            dbBox.PolyBags = PolyBags;
            dbBox.Owned = Owned;
            dbBox.Archived = Archived;
            dbBox.CreateDate = DateHelper.ConvertAppToUtc(CreateDate) ?? when;
            dbBox.UpdateDate = when;
            dbBox.UpdatedBy = by;

            db.Commit();
        }

        public static void SetDefaultBreakdowns(List<StyleItemViewModel> items)
        {
            if (items.Count < 1)
                return;

            var breakdowns = SizeHelper.GetBreakdowns(items.Select(i => i.Size).ToList());
            if (breakdowns != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Breakdown = breakdowns[i];
                }
            }
        }

        private long AddNewBox(IUnitOfWork db, 
            DateTime when,
            long? by)
        {
            var sealedBox = new SealedBox
            {
                StyleId = StyleId,
                BoxBarcode = BoxBarcode,
                BoxQuantity = BoxQuantity,
                //Breakdown = Breakdown,
                Printed = Printed,
                PolyBags = PolyBags,
                Owned = Owned,
                PajamaPrice = Price,
                CreateDate = DateHelper.ConvertAppToUtc(CreateDate) ?? when,
                CreatedBy = by,

                UpdateDate = DateHelper.ConvertAppToUtc(CreateDate) ?? when,
                UpdatedBy = by,

                OriginCreateDate = when,
                OriginType = (int)BoxOriginTypes.ByUser
            };
            db.SealedBoxes.Add(sealedBox);
            db.Commit();

            return sealedBox.Id;
        }

        public static SealedBox CopyBox(IUnitOfWork db, 
            long toCopyBoxId, 
            DateTime when,
            long? by)
        {
            var toCopySealedBox = db.SealedBoxes.Get(toCopyBoxId);

            var sealedBox = new SealedBox
            {
                StyleId = toCopySealedBox.StyleId,
                BoxBarcode = toCopySealedBox.BoxBarcode,
                BoxQuantity = toCopySealedBox.BoxQuantity,
                //Breakdown = Breakdown,
                Printed = toCopySealedBox.Printed,
                PolyBags = toCopySealedBox.PolyBags,
                Owned = toCopySealedBox.Owned,
                PajamaPrice = toCopySealedBox.PajamaPrice,
                CreateDate = when,
                CreatedBy = by
            };
            db.SealedBoxes.Add(sealedBox);
            db.Commit();

            var toCopyItems = db.SealedBoxItems.GetByBoxIdAsDto(toCopyBoxId);
            foreach (var boxItem in toCopyItems)
            {
                db.SealedBoxItems.Add(new SealedBoxItem
                {
                    StyleItemId = boxItem.StyleItemId,
                    BreakDown = boxItem.BreakDown,
                    BoxId = sealedBox.Id,
                    
                    CreateDate = when,
                    CreatedBy = by,
                });
            }
            db.Commit();

            return sealedBox;
        }
    }
}