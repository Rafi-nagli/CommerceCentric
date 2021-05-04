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
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory
{
    public class OpenBoxViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }

        public string StyleString { get; set; }

        public long? DropShipperId { get; set; }

        public int Type { get; set; }
        public string TypeName
        {
            get { return BoxTypesHelper.TypeToString((BoxTypes)Type); }
        }

        public string BoxBarcode { get; set; }
        public int BoxQuantity { get; set; }
        public int SizesQuantity { get; set; }
        public decimal Price { get; set; }
        public bool Printed { get; set; }
        public bool PolyBags { get; set; }
        public bool Owned { get; set; }

        public bool Archived { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public DateTime? OriginCreateDate { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? CreateDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CreateDate); }
            set { CreateDate = DateHelper.ConvertUtcToApp(value); }
        }

        public string CreateDateFormatted
        {
            get
            {
                return CreateDate.HasValue ? CreateDate.Value.ToString(DateHelper.DateFormat) : "-";
            }
        }


        public string OriginCreateDateFormatted
        {
            get
            {
                return OriginCreateDate.HasValue ? OriginCreateDate.Value.ToString(DateHelper.DateFormat) : "-";
            }
        }

        public StyleItemCollection StyleItems { get; set; }

        public int TotalBoxesQuantity
        {
            get { return SizesQuantity * BoxQuantity; }
        }

        public override string ToString()
        {
            return "Id=" + Id
                   + ", StyleId=" + StyleId
                   + ", StyleString=" + StyleString
                   + ", BoxBarcode=" + BoxBarcode
                   + ", BoxQuantity=" + BoxQuantity
                   + ", SizesQuantity=" + SizesQuantity
                   + ", Price=" + Price
                   + ", Printed=" + Printed
                   + ", PolyBags=" + PolyBags
                   + ", Owned=" + Owned
                   + ", Archived=" + Archived;
        }

        public OpenBoxViewModel()
        {
            BoxQuantity = 1;
            Owned = true;

            StyleItems = new StyleItemCollection(StyleItemDisplayMode.BoxQty);
        }


        public List<MessageString> Validate(IUnitOfWork db)
        {
            var messages = new List<MessageString>();

            var total = StyleItems.Items.Sum(i => i.Quantity ?? 0)*BoxQuantity;
            var existBoxes = db.OpenBoxes.GetAllAsDto()
                .Where(b => b.StyleId == StyleId
                            && b.BoxQuantity == BoxQuantity
                            && !b.Deleted)
                .ToList();

            var boxIds = existBoxes.Select(b => b.Id).ToList();
            var allBoxItems = db.OpenBoxItems.GetAll()
                .Where(b => boxIds.Contains(b.BoxId))
                .ToList();

            foreach (var box in existBoxes)
            {
                var boxItems = allBoxItems.Where(bi => bi.BoxId == box.Id).ToList();
                if (boxItems.Sum(b => b.Quantity)*BoxQuantity == total)
                {
                    messages.Add(new MessageString()
                    {
                        Message = "",
                        Status = MessageStatus.Info,
                    });
                }
            }

            return messages;
        }

        public static OpenBoxViewModel BuildFromStyleId(IUnitOfWork db, long styleId)
        {
            var model = new OpenBoxViewModel();

            var style = db.Styles.GetByStyleIdAsDto(styleId);
            model.StyleString = style.StyleID;

            model.StyleId = styleId;

            model.Price = db.OpenBoxes.GetByStyleId(styleId).OrderBy(b => b.Id).Select(b => b.Price).FirstOrDefault();
            model.BoxQuantity = 1;
            model.Owned = true;

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId).ToList();
            model.StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxQty,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si))
                                .ToList()
            };

            model.CreateDate = DateHelper.GetAppNowTime().Date;

            model.BoxBarcode = db.OpenBoxes.GetDefaultBoxName(styleId, model.CreateDate.Value);

            return model;
        }

        public static OpenBoxViewModel BuildFromBoxId(IUnitOfWork db,
            long openBoxId)
        {
            var model = new OpenBoxViewModel();

            var item = db.OpenBoxes.GetAllAsDto().FirstOrDefault(o => o.Id == openBoxId);

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(item.StyleId).ToList();
            var boxSizeItems = db.OpenBoxItems.GetByBoxIdAsDto(openBoxId).ToList();
            var style = db.Styles.GetByStyleIdAsDto(item.StyleId);

            model.ConstructFrom(item, style, styleItems, boxSizeItems);

            return model;
        }

        public OpenBoxViewModel(OpenBoxDto item,
            StyleEntireDto style,
            IList<StyleItemDTO> styleItems,
            List<OpenBoxItemDto> boxItems)
        {
            ConstructFrom(item, style, styleItems, boxItems);
        }


        private void ConstructFrom(OpenBoxDto item,
            StyleEntireDto style,
            IList<StyleItemDTO> styleItems,
            List<OpenBoxItemDto> boxItems)
        {
            Id = item.Id;
            StyleId = item.StyleId;

            if (style != null)
                StyleString = style.StyleID;

            BoxBarcode = item.BoxBarcode;
            Printed = item.Printed;
            PolyBags = item.PolyBags;
            BoxQuantity = item.BoxQuantity;
            Price = item.Price;
            Owned = item.Owned;
            Archived = item.Archived;

            CreateDate = item.CreateDate;
            UpdateDate = item.UpdateDate;
            OriginCreateDate = item.OriginCreateDate;

            UpdatedByName = item.UpdatedByName;

            SizesQuantity = boxItems.Sum(s => s.Quantity);

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxQty,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si))
                                .ToList()
            };

            //Set boxes values
            foreach (var boxItem in boxItems)
            {
                var styleItem = StyleItems.Items.FirstOrDefault(si => si.Id == boxItem.StyleItemId);
                if (styleItem != null)
                {
                    styleItem.Quantity = boxItem.Quantity;
                    styleItem.BoxItemId = boxItem.Id;
                }
            }
        }


        public OpenBoxViewModel(IUnitOfWork db,
            OpenBoxDto item,
            StyleEntireDto style,
            IList<StyleItemDTO> styleItems,
            List<OpenBoxItemDto> boxItems,
            List<OpenBoxTrackingDTO> boxTrackings)
        {
            ConstructFrom(db, item, style, styleItems, boxItems, boxTrackings);
        }


        private void ConstructFrom(IUnitOfWork db,
            OpenBoxDto item,
            StyleEntireDto style,
            IList<StyleItemDTO> styleItems,
            List<OpenBoxItemDto> boxItems,
            List<OpenBoxTrackingDTO> boxTrackings)
        {
            Id = item.Id;
            StyleId = item.StyleId;

            if (style != null)
                StyleString = style.StyleID;

            Type = item.Type;

            BoxBarcode = item.BoxBarcode;
            Printed = item.Printed;
            PolyBags = item.PolyBags;
            BoxQuantity = item.BoxQuantity;
            Price = item.Price;
            Owned = item.Owned;
            Archived = item.Archived;

            CreateDate = item.CreateDate;
            UpdateDate = item.UpdateDate;

            UpdatedByName = item.UpdatedByName;

            SizesQuantity = boxItems.Sum(s => s.Quantity);

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxQty,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si))
                                .ToList()
            };

            //Set boxes values
            foreach (var boxItem in boxItems)
            {
                var styleItem = StyleItems.Items.FirstOrDefault(si => si.Id == boxItem.StyleItemId);
                if (styleItem != null)
                {
                    styleItem.Quantity = boxItem.Quantity;
                    styleItem.BoxItemId = boxItem.Id;
                }
            }
        }



        public static IList<OpenBoxViewModel> GetAll(IUnitOfWork db, 
            long? styleId,
            bool includeArchive)
        {
            var boxQuery = db.OpenBoxes.GetAllAsDto().Where(b => !b.Deleted);
            if (styleId.HasValue)
            {
                boxQuery = boxQuery.Where(b => b.StyleId == styleId);
            }
            if (!includeArchive)
            {
                boxQuery = boxQuery.Where(b => !b.Archived);
            }
            var boxList = boxQuery.ToList();
            var styleIds = boxList.Select(b => b.StyleId).Distinct().ToList();

            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleIds)
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .ToList();

            var styles = db.Styles.GetAllAsDtoLite().Where(st => styleIds.Contains(st.Id)).ToList();

            return boxList
                .Select(box => new OpenBoxViewModel(db,
                    box,
                    styles.FirstOrDefault(st => st.Id == box.StyleId),
                    styleSizes.Where(si => si.StyleId == box.StyleId).ToList(),
                    db.OpenBoxItems.GetByBoxIdAsDto(box.Id).ToList(),
                    db.OpenBoxTrackings.GetByBoxIdAsDto(box.Id).ToList()))
                .ToList();
        }

        public long Apply(IUnitOfWork db, IQuantityManager quantityManager, DateTime when, long? by)
        {
            var existBox = db.OpenBoxes.GetFiltered(b => b.Id == Id && !b.Deleted).FirstOrDefault();
            long? boxId = null;
            var isNewBox = false;
            var archiveChanged = false;
            var boxQuantityChanged = false;

            if (existBox == null)
            {
                boxId = AddNewBox(db, when, by);
                isNewBox = true;
            }
            else
            {
                archiveChanged = existBox.Archived != Archived;
                boxQuantityChanged = existBox.BoxQuantity != BoxQuantity;
                UpdateExistingBox(db, existBox, when, by);
                boxId = existBox.Id;
            }

            var boxItems = StyleItems.Items
                .Where(si => si.Quantity.HasValue)
                .Select(si => new OpenBoxItemDto()
                {
                    Id = si.BoxItemId ?? 0,
                    StyleItemId = si.Id,
                    Quantity = si.Quantity ?? 0,
                }).ToList();

            var updateResults = db.OpenBoxItems.UpdateBoxItemsForBox(boxId.Value, boxItems, when, by);

            foreach (var updateResult in updateResults)
            {
                var boxItem = boxItems.FirstOrDefault(bi => bi.Id == updateResult.Id);
                if (boxItem != null)
                {
                    var changeType = QuantityChangeSourceType.None;
                    if (updateResult.Status == UpdateType.Update || boxQuantityChanged)
                        changeType = QuantityChangeSourceType.ChangeBox;
                    if (updateResult.Status == UpdateType.Insert)
                        changeType = QuantityChangeSourceType.AddNewBox;
                    if (updateResult.Status == UpdateType.Removed)
                        changeType = QuantityChangeSourceType.RemoveBox;

                    quantityManager.LogStyleItemQuantity(db,
                        boxItem.StyleItemId,
                        changeType != QuantityChangeSourceType.RemoveBox ? boxItem.Quantity * BoxQuantity : (int?)null,
                        changeType == QuantityChangeSourceType.RemoveBox ? boxItem.Quantity * BoxQuantity : (int?)null,
                        changeType,
                        null,
                        boxItem.Id,
                        existBox?.BoxBarcode,
                        when,
                        by);


                    var archiveChangeType = QuantityChangeSourceType.None;
                    if (isNewBox)
                    {
                        if (Archived)
                        {
                            archiveChangeType = QuantityChangeSourceType.ArchiveBox;
                        }
                    }
                    else
                    {
                        if (archiveChanged)
                        {
                            archiveChangeType = Archived ? QuantityChangeSourceType.ArchiveBox : QuantityChangeSourceType.UnArchiveBox;
                        }
                    }
                    if (archiveChangeType != QuantityChangeSourceType.None)
                    {
                        quantityManager.LogStyleItemQuantity(db,
                            boxItem.StyleItemId,
                            changeType == QuantityChangeSourceType.UnArchiveBox
                                ? boxItem.Quantity*BoxQuantity
                                : (int?) null,
                            changeType == QuantityChangeSourceType.ArchiveBox
                                ? boxItem.Quantity*BoxQuantity
                                : (int?) null,
                            changeType,
                            null,
                            boxItem.Id,
                            existBox?.BoxBarcode,
                            when,
                            by);
                    }
                }
            }
            
            var id = db.Styles.GetFiltered(s => s.Id == StyleId).Select(s => s.Id).FirstOrDefault();
            return id;
        }

        private void UpdateExistingBox(IUnitOfWork db, 
            OpenBox dbBox, 
            DateTime when,
            long? by)
        {
            dbBox.StyleId = StyleId;
            dbBox.BoxBarcode = BoxBarcode;
            dbBox.Printed = Printed;
            dbBox.PolyBags = PolyBags;
            dbBox.BoxQuantity = BoxQuantity;
            dbBox.Price = Price;
            dbBox.Owned = Owned;
            dbBox.Archived = Archived;
            dbBox.CreateDate = DateHelper.ConvertAppToUtc(CreateDate) ?? when;
            dbBox.UpdateDate = when;
            dbBox.UpdatedBy = by;

            db.Commit();
        }
        
        private long AddNewBox(IUnitOfWork db, 
            DateTime when, 
            long? by)
        {
            var openBox = new OpenBox
            {
                StyleId = StyleId,
                BoxBarcode = BoxBarcode,
                BoxQuantity = BoxQuantity,
                Price = Price,
                Printed = Printed,
                PolyBags = PolyBags,
                Owned = Owned,
                Archived = Archived,
                CreateDate =  DateHelper.ConvertAppToUtc(CreateDate) ?? when,
                CreatedBy = by,

                UpdateDate = DateHelper.ConvertAppToUtc(CreateDate) ?? when,
                UpdatedBy = by,

                OriginCreateDate = when,
                OriginType = (int)BoxOriginTypes.ByUser
            };
            db.OpenBoxes.Add(openBox);
            db.Commit();

            return openBox.Id;
        }

        public static void Remove(IUnitOfWork db, 
            long id, 
            IQuantityManager quantityManager,
            ICacheService cache,
            DateTime when,
            long? by)
        {
            var box = db.OpenBoxes.Get(id);
            box.Deleted = true;
            box.UpdateDate = when;
            box.UpdatedBy = by;
            db.Commit();

            var openBoxItems = db.OpenBoxItems.GetAll().Where(bi => bi.BoxId == id).ToList();
            foreach (var boxItem in openBoxItems)
            {
                quantityManager.LogStyleItemQuantity(db,
                    boxItem.StyleItemId ?? 0,
                    null,
                    boxItem.Quantity * box.BoxQuantity,
                    QuantityChangeSourceType.RemoveBox, 
                    null,
                    box.Id,
                    box.BoxBarcode,
                    when,
                    by);
            }

            cache.RequestStyleIdUpdates(db,
                    new List<long>() { box.StyleId },
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);
        }
    }
}