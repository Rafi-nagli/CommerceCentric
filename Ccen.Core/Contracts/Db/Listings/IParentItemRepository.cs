using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Cache;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IParentItemRepository : IRepository<ParentItem>
    {
        IQueryable<ParentItemDTO> GetAllAsDto();
        IQueryable<ParentItemDTO> GetAllAsDto(MarketType market, string marketplaceId);
        ParentItem CreateOrUpdateParent(ParentItemDTO dto, MarketType market, string marketplaceId, DateTime? when);
        void UpdateParent(string currentASIN,
            ParentItemDTO dto,
            MarketType market,
            string marketplaceId,
            DateTime? when);

        ParentItem FindOrCreateForItem(Item item, DateTime? when);
        
        bool AnyWithASIN(string asin);
        ParentItemDTO GetAsDTO(int id);
        ParentItemDTO GetAsDTO(string asin, MarketType market, string marketplaceId);
        List<ItemDTO> CheckForExistence(List<ItemDTO> newListings, MarketType market, string marketplaceId);
        GridResponse<ParentItemDTO> GetWithChildSizesWithPaging(ILogService log,
            IDbCacheService cacheService,
            bool clearCache,
            bool useStyleImage,
            ItemSearchFiltersDTO filters);
        ParentItem GetByASIN(string parentASIN, MarketType market, string marketplaceId);

        IList<ParentItemDTO> GetSimilarByChildSkuAsDto(int id);
    }
}
