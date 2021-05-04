using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Core.Helpers;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleItemRepository : Repository<StyleItem>, IStyleItemRepository
    {
        public StyleItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleItemDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<StyleItem>());
        }

        public IList<StyleItemDTO> GetByStyleIdAsDto(long styleId)
        {
            var query = from si in unitOfWork.GetSet<StyleItem>()
                        where si.StyleId == styleId
                        select si;

            return AsDto(query).ToList();
        }

        public IList<StyleItemDTO> GetByStyleIdAsDto(string styleId)
        {
            var query = from si in unitOfWork.GetSet<StyleItem>()
                        join s in unitOfWork.GetSet<Style>() on si.StyleId equals s.Id
                        where s.StyleID == styleId
                            && !s.Deleted
                        select si;

            return AsDto(query).ToList();
        }

        public IList<StyleItemDTO> GetByStyleIdWithRemainingAsDto(string styleId)
        {
            var query = from si in unitOfWork.GetSet<StyleItem>()
                        join sic in unitOfWork.GetSet<StyleItemCache>() on si.Id equals sic.Id
                        join s in unitOfWork.GetSet<Style>() on si.StyleId equals s.Id
                        where s.StyleID == styleId
                            && !s.Deleted
                        select new StyleItemDTO()
                        {
                            StyleItemId = si.Id,
                            StyleId = si.StyleId,
                            Size = si.Size,
                            Color = si.Color,
                            RemainingQuantity = sic.RemainingQuantity,
                            PackageHeight = si.PackageHeight,
                            PackageWidth = si.PackageWidth,
                            PackageLength = si.PackageLength
                        };

            return query.ToList();
        }

        public IList<StyleItemDTO> GetByStyleIdWithSizeGroupAsDto(long styleId)
        {
            return GetByStyleIdWithSizeGroupAsDto(new List<long>() { styleId });
        }

        public IList<StyleItemDTO> GetByStyleIdWithSizeGroupAsDto(IList<long> styleIds)
        {
            var query = from si in unitOfWork.GetSet<StyleItem>()
                        join s in unitOfWork.GetSet<Size>() on si.SizeId equals s.Id into withSize
                        from s in withSize.DefaultIfEmpty()
                        join sg in unitOfWork.GetSet<SizeGroup>() on s.SizeGroupId equals sg.Id into withSizeGroup
                        from sg in withSizeGroup.DefaultIfEmpty()
                        where styleIds.Contains(si.StyleId)
                        select new StyleItemDTO()
                        {
                            SizeGroupId = sg.Id,
                            SizeGroupName = sg.Name,

                            SizeGroupSortOrder = sg.SortOrder,
                            SizeSortOrder = s.SortOrder,

                            StyleId = si.StyleId,
                            StyleItemId = si.Id,

                            Size = si.Size,
                            Color = si.Color,
                            SizeId = si.SizeId,

                            Weight = si.Weight,
                            PackageHeight = si.PackageHeight,
                            PackageWidth = si.PackageWidth,
                            PackageLength = si.PackageLength,

                            MinPrice = si.MinPrice,
                            MaxPrice = si.MaxPrice,

                            Quantity = si.Quantity,
                            QuantitySetDate = si.QuantitySetDate,
                            QuantitySetBy = si.QuantitySetBy,

                            RestockDate = si.RestockDate,
                            FulfillDate = si.FulfillDate,
                            OnHold = si.OnHold,
                        };

            return query.ToList();
        }

        public IList<StyleItemDTO> GetByStyleIdWithBarcodesAsDto(long styleId)
        {
            var styleItems = GetByStyleIdWithSizeGroupAsDto(new List<long>() { styleId });

            var styleItemIdList = styleItems.Select(si => si.StyleItemId).ToArray();
            var barcodeList = unitOfWork.StyleItemBarcodes
                .GetAllAsDto()
                .Where(b => styleItemIdList.Contains(b.StyleItemId))
                .ToList();

            foreach (var item in styleItems)
            {
                item.Barcodes = barcodeList
                    .Where(b => b.StyleItemId == item.StyleItemId)
                    .ToList();
            }

            return styleItems;
        }

        public List<string> GetDuplicateList(IList<string> barcodes)
        {
            var selfDupl = barcodes.GroupBy(b => b).Where(b => b.Count() > 1).Select(b => b.Key).ToList();

            var styleItemQuery = from si in unitOfWork.GetSet<StyleItem>()
                                 join s in unitOfWork.GetSet<Style>() on si.StyleId equals s.Id
                                 join b in unitOfWork.GetSet<StyleItemBarcode>() on si.Id equals b.StyleItemId
                                 where s.Deleted == false
                                       && barcodes.Contains(b.Barcode)
                                 select b.Barcode;


            var duplicates = selfDupl;
            duplicates.AddRange(styleItemQuery.ToList());

            return duplicates;
        }

        public StyleItemDTO FindOrCreateForItem(long styleId,
            int itemTypeId,
            ItemDTO item,
            bool canCreate,
            SizeMode sizeMode,
            DateTime when)
        {
            var department = item.Department;
            
            //Step 1. Compose possible style sizes list
            var existStyleItems = GetStyleItemsByStyleIdWithBarcodes(styleId);
            
            var possibleSizeList = unitOfWork.SizeMappings
                .GetStyleSizesByItemSize(item.Size, itemTypeId)
                .ToList(); //NOTE: at first items with max priority

            var size = unitOfWork.Sizes.GetAllWithGroupByItemTypeAsDto(itemTypeId)
                .FirstOrDefault(s => s.Name == item.Size);
            if (size != null) //Keep style sizes sort order (size with same size name has max priority)
                possibleSizeList.Insert(0, size);

            //NOTE: For BLNKT sku
            if (!possibleSizeList.Any())
            {
                if (!String.IsNullOrEmpty(item.SKU)
                    && item.SKU.EndsWith("BLNKT", StringComparison.InvariantCultureIgnoreCase))
                {
                    var emptySize = unitOfWork.Sizes.GetAllAsDto().FirstOrDefault(s => s.Name == "OneSize");
                    if (emptySize != null)
                    {
                        possibleSizeList.Add(emptySize);
                    }
                }
            }

            //Step 2. Get/create suitable styleItem 

            //NOTE: get first suitable style item BY SIZE (TODO: add condition by color)
            var suitableStyleItems = new List<StyleItem>();
            //0. Get full size equivalent
            suitableStyleItems = existStyleItems.Where(i => i.Size == item.Size).ToList();
            //1. Get using possible mappings
            if (suitableStyleItems.Count == 0 
                && size == null) //NOTE: use possible size list only when system haven't equal size (6 always should map to 6, 8 to 8 and e.t.c)
                suitableStyleItems = existStyleItems
                    .Where(e => possibleSizeList.Any(sl => sl.Name == e.Size))
                    .ToList();

            //If not suitable styleItem
            if (canCreate && suitableStyleItems.Count == 0)
            {

                SizeDTO betterSize = null;
                //0. Trying get equal size (max priority => min value)
                betterSize = possibleSizeList.FirstOrDefault(i => i.Name == item.Size);

                //NOTE: detect which size is better
                //1. Find by department

                if (betterSize == null)
                {
                    if (!String.IsNullOrEmpty(department))
                    {
                        foreach (var possibleSize in possibleSizeList)
                        {
                            if (!String.IsNullOrEmpty(possibleSize.Departments))
                            {
                                var sizeDepartmentList = possibleSize.Departments.Split(";, \t\r\n".ToCharArray(),
                                    StringSplitOptions.RemoveEmptyEntries);
                                if (betterSize == null && //If not found yet, first match has max priority
                                    StringHelper.IsMatchWithAny(department, sizeDepartmentList))
                                {
                                    betterSize = possibleSize;
                                }
                            }
                        }
                    }
                }

                if (betterSize == null) //If can't found by department, get by max priority (i.e. first item from list)
                {
                    betterSize = possibleSizeList.FirstOrDefault();
                }

                if (sizeMode == SizeMode.DirectNaming)
                {
                    betterSize = new SizeDTO()
                    {
                        Name = item.Size,
                    };
                }

                if (betterSize != null)
                {
                    var newStyleItem = new StyleItem
                    {
                        StyleId = styleId,
                        Size = betterSize.Name,
                        //Color=TODO:
                        SizeId = betterSize.Id > 0 ? (int?)betterSize.Id : null,

                        Quantity = item.RealQuantity,
                        QuantitySetBy = null,
                        QuantitySetDate = when,

                        CreateDate = when,
                        StyleItemBarcodes = new Collection<StyleItemBarcode>()
                    };
                    Add(newStyleItem);
                    unitOfWork.Commit();

                    var styleItemHistory = new StyleItemQuantityHistory()
                    {
                        StyleItemId = newStyleItem.Id,
                        Quantity = item.RealQuantity,
                        Type = (int)QuantityChangeSourceType.Initial,
                        CreateDate = when,
                    };
                    unitOfWork.StyleItemQuantityHistories.Add(styleItemHistory);
                    unitOfWork.Commit();

                    suitableStyleItems.Add(newStyleItem);
                }
            }

            var styleItemForItem = suitableStyleItems.FirstOrDefault();

            if (styleItemForItem == null
                && existStyleItems.Count == 1
                && ((String.IsNullOrEmpty(item.Size)
                    && String.IsNullOrEmpty(item.Color))
                    || (String.IsNullOrEmpty(existStyleItems[0].Size)
                        && String.IsNullOrEmpty(existStyleItems[0].Color))))
            {
                styleItemForItem = existStyleItems[0];
            }

            //Step 3. Add barcode
            if (!String.IsNullOrEmpty(item.Barcode))
            {
                var isExistBarcode = suitableStyleItems.Any(si =>
                    si.StyleItemBarcodes != null
                    && si.StyleItemBarcodes.Any(b => b.Barcode == item.Barcode));

                if (!isExistBarcode)
                {
                    if (styleItemForItem != null)
                    {
                        unitOfWork.StyleItemBarcodes.Add(new StyleItemBarcode
                        {
                            StyleItemId = styleItemForItem.Id,

                            Barcode = item.Barcode,
                            CreateDate = when
                        });
                        unitOfWork.Commit();
                    }
                }
            }

            if (styleItemForItem != null)
            {
                return new StyleItemDTO()
                {
                    StyleItemId = styleItemForItem.Id,
                    StyleId = styleItemForItem.StyleId,
                };
            }
            return null;
        }


        public IList<EntityUpdateStatus<long>> UpdateStyleItemsForStyle(long styleId,
            IList<StyleItemDTO> styleItems,
            DateTime when,
            long? by)
        {
            var updateResults = new List<EntityUpdateStatus<long>>();
            var items = styleItems;
            var dbExistItems = GetFiltered(l => l.StyleId == styleId).ToList();
            var newItems = styleItems.Where(l => l.StyleItemId == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = items.FirstOrDefault(l => l.StyleItemId == dbItem.Id);

                if (existItem != null)
                {
                    var hasQtyChanges = false;
                    var hasChanges = dbItem.Weight != existItem.Weight
                                     || dbItem.SizeId != existItem.SizeId
                                     || dbItem.Size != existItem.Size
                                     || dbItem.PackageHeight != existItem.PackageHeight
                                     || dbItem.PackageWidth != existItem.PackageWidth
                                     ||dbItem.PackageLength != existItem.PackageLength
                    ;

                    if (hasChanges)
                    {
                        dbItem.Weight = existItem.Weight;
                        dbItem.SizeId = existItem.SizeId;
                        dbItem.Size = existItem.Size;
                        dbItem.Color = existItem.Color;
                        dbItem.PackageHeight = existItem.PackageHeight;
                        dbItem.PackageWidth = existItem.PackageWidth;
                        dbItem.PackageLength = existItem.PackageLength;

                        dbItem.UpdateDate = when;
                        dbItem.UpdatedBy = by;
                    }

                    if (dbItem.Quantity == null
                        && existItem.Quantity > 0)
                    {
                        hasQtyChanges = true;
                        dbItem.Quantity = existItem.Quantity;
                        dbItem.QuantitySetBy = existItem.QuantitySetBy;
                        dbItem.QuantitySetDate = existItem.QuantitySetDate;
                    }

                    if (hasChanges || hasQtyChanges)
                    {
                        updateResults.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Update));
                    }
                }
                else
                {
                    updateResults.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Removed));
                    Remove(dbItem);
                }
            }

            unitOfWork.Commit();

            foreach (var newItem in newItems)
            {
                var dbItem = new StyleItem()
                {
                    StyleId = styleId,
                    Size = newItem.Size,
                    SizeId = newItem.SizeId,
                    Color = newItem.Color,
                    Weight = newItem.Weight,

                    Quantity = newItem.Quantity,
                    QuantitySetBy = newItem.QuantitySetBy,
                    QuantitySetDate = newItem.QuantitySetDate,
                    PackageHeight = newItem.PackageHeight,
                    PackageWidth = newItem.PackageWidth,
                    PackageLength = newItem.PackageLength,

                CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);

                unitOfWork.Commit();

                updateResults.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Insert));

                newItem.StyleItemId = dbItem.Id;
            }

            return updateResults;
        }

        public BarcodeDTO GetFullBarcodeInfo(string barcode)
        {
            var query = from b in unitOfWork.GetSet<StyleItemBarcode>()
                join si in unitOfWork.GetSet<StyleItem>() on b.StyleItemId equals si.Id
                join sic in unitOfWork.GetSet<StyleItemCache>() on b.StyleItemId equals sic.Id
                join s in unitOfWork.GetSet<Style>() on si.StyleId equals s.Id
                where b.Barcode == barcode
                select new BarcodeDTO()
                {
                    Id = b.Id,
                    Barcode = b.Barcode,
                    Size = si.Size,
                    Color = si.Color,
                    
                    Picture = s.Image,
                    StyleId = s.StyleID,

                    RemainingQuantity = sic.RemainingQuantity,
                };
            return query.FirstOrDefault();
        }

        private IList<StyleItem> GetStyleItemsByStyleIdWithBarcodes(long styleId)
        {
            var items = unitOfWork.GetSet<StyleItem>()
                .Include(i => i.StyleItemBarcodes)
                .Where(i => i.StyleId == styleId);

            return items.ToList();
        }

        public IQueryable<SoldSizeInfo> GetInventoryQuantities()
        {
            return unitOfWork.GetSet<ViewInventoryQuantity>().Select(s => new SoldSizeInfo()
            {
                StyleItemId = s.Id,
                StyleId = s.StyleId,
                Size = s.Size,
                TotalQuantity = s.Quantity,
                QuantitySetDate = s.QuantitySetDate,

                ItemPrice = s.ItemPrice,

                BoxQuantity = s.BoxQuantity,
                BoxQuantitySetDate = s.BoxQuantitySetDate,
                DirectQuantity = s.DirectQuantity,
                DirectQuantitySetDate = s.DirectQuantitySetDate
            });
        }

        public IQueryable<ViewInventoryThisYearQuantity> GetInventoryThisYearQuantities()
        {
            return unitOfWork.GetSet<ViewInventoryThisYearQuantity>();
        }


        public IQueryable<SoldSizeInfo> GetInventoryOnHandQuantities()
        {
            return unitOfWork.GetSet<ViewInventoryOnHandQuantity>().Select(s => new SoldSizeInfo()
            {
                StyleItemId = s.Id,
                StyleId = s.StyleId,
                Size = s.Size,
                TotalQuantity = s.Quantity,
                QuantitySetDate = s.QuantitySetDate,

                BoxQuantity = s.BoxQuantity,
                BoxQuantitySetDate = s.BoxQuantitySetDate,
                DirectQuantity = s.DirectQuantity,
                DirectQuantitySetDate = s.DirectQuantitySetDate
            });
        }

        private IQueryable<StyleItemDTO> AsDto(IQueryable<StyleItem> query)
        {
            return query.Select(s => new StyleItemDTO()
            {
                StyleItemId = s.Id,
                StyleId = s.StyleId,
                Size = s.Size,
                SizeId = s.SizeId,
                Color = s.Color,
                Weight = s.Weight,
                PackageHeight = s.PackageHeight,
                PackageWidth = s.PackageWidth,
                PackageLength = s.PackageLength,

                MinPrice = s.MinPrice,
                MaxPrice = s.MaxPrice,

                Quantity = s.Quantity,
                QuantitySetDate = s.QuantitySetDate,
                QuantitySetBy = s.QuantitySetBy,

                RestockDate = s.RestockDate,
                FulfillDate = s.FulfillDate,
                OnHold = s.OnHold,
            });
        }
    }
}
