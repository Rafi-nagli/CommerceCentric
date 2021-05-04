using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Core.Contracts.Db
{
    public interface IItemRepository : IRepository<Item>
    {
        void SetItemPublishingStatus(int itemId,
            int newStatus,
            string reason,
            DateTime? when);

        Item StoreItemIfNotExist(IItemHistoryService itemHistory,
            string calledFrom,
            ItemDTO item,
            MarketType market,
            string marketplaceId,
            long ownerId,
            DateTime? when);

        Item GetByASIN(string asin, MarketType market, string marketplaceId);
        Item GetBySKU(string sku, MarketType market, string marketplaceId);

        IQueryable<ItemDTO> GetAllViewAsDto(MarketType market, string marketplaceId);
        IQueryable<ItemDTO> GetAllViewAsDto();
        ItemDTO GetByIdAsDto(long id);

        bool CheckForExistenceBarcode(string barcode, MarketType market, string marketplaceId);

        IList<ItemDTO> GetBySKUAsDto(string sku);
        IList<ItemDTO> GetBySKUAsDto(IList<string> sku);

        IQueryable<ItemDTO> GetAllWithSizeMappingIssues();

        IQueryable<ViewItem> GetAllViewActual();
        IQueryable<ItemExDTO> GetAllActualExAsDto();
        IQueryable<ItemDTO> GetAllActualWithSold();

        IQueryable<ItemExDTO> GetAllActualForPrices();

        List<ItemDTO> GetByParentASINAsDto(MarketType market, string marketplaceId, string asin);
        List<Item> GetItemsByParentASIN(MarketType market, string marketplaceId, string asin);
        List<Listing> GetListingsByParentASIN(MarketType market, string marketplaceId, string parentAsin);

        IQueryable<PurchaseByDateDTO> GetSalesInfoBySKU();

        IQueryable<SoldSizeInfo> GetMarketsSoldQuantityByListing();
        IQueryable<SoldSizeInfo> GetMarketsSoldQuantityByStyleItem();
        IQueryable<SoldSizeInfo> GetMarketsSoldQuantityIncludePreOrderByStyleItem();
        IQueryable<SoldSizeInfo> GetMarketsSoldThisYearQuantityIncludePreOrderByStyleItem();

        IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndMarket();
        IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndItemStyle();
        IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndBrand();

        void DeleteAnyLinksToStyleId(long styleId);
        void UpdateItemsForParentItem(IItemHistoryService itemChangeHistory,
            string calledFrom,
            string parentAsin,
            int market,
            string marketplaceId,
            IList<ItemDTO> items,
            DateTime when,
            long? by);
    }
}
