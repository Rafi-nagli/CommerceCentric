using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemRepository : IRepository<StyleItem>
    {
        IQueryable<StyleItemDTO> GetAllAsDto();
        IList<StyleItemDTO> GetByStyleIdAsDto(long styleId);
        IList<StyleItemDTO> GetByStyleIdAsDto(string styleId);

        IList<StyleItemDTO> GetByStyleIdWithRemainingAsDto(string styleId);

        IList<StyleItemDTO> GetByStyleIdWithSizeGroupAsDto(long styleId);
        IList<StyleItemDTO> GetByStyleIdWithSizeGroupAsDto(IList<long> styleIds);
        IList<StyleItemDTO> GetByStyleIdWithBarcodesAsDto(long styleId);

        List<string> GetDuplicateList(IList<string> newSizes);

        BarcodeDTO GetFullBarcodeInfo(string barcode);

        StyleItemDTO FindOrCreateForItem(long styleId, 
            int itemTypeId,
            ItemDTO item, 
            bool canCreate,
            SizeMode sizeMode,
            DateTime when);

        IList<EntityUpdateStatus<long>> UpdateStyleItemsForStyle(long styleId,
            IList<StyleItemDTO> styleItems,
            DateTime when,
            long? by);
        
        IQueryable<SoldSizeInfo> GetInventoryQuantities();
        IQueryable<ViewInventoryThisYearQuantity> GetInventoryThisYearQuantities();
        IQueryable<SoldSizeInfo> GetInventoryOnHandQuantities();
    }
}
