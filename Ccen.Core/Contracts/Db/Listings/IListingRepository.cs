using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IListingRepository : IRepository<Listing>
    {
        IEnumerable<long> MarkNotExistingAsRemoved(IEnumerable<long> existListingIds, MarketType market, string marketplaceId);
        IEnumerable<string> MarkNotExistingAsRemoved(IEnumerable<string> existListingIds, MarketType market, string marketplaceId);
        void MarkAsRemoved(IList<long> listingIds);

        IList<ItemDTO> CheckForExistence(IList<ItemDTO> result, MarketType market, string marketplaceId);
        bool CheckForExistenceSKU(string sku, MarketType market, string marketplaceId);

        Listing StoreOrUpdate(ItemDTO dto, Item item, MarketType market, string marketplaceId, DateTime when);
        IList<Listing> GetByListingId(string listingId, MarketType market, string marketplaceId);
        Listing GetBySKU(string sku, MarketType market, string marketplaceId);

        IList<Listing> GetByListingIdIncludeRemoved(string listingId, MarketType market, string marketplaceId);
        bool FillOrderItemsBySKU(IList<ListingOrderDTO> orderItems, MarketType market, string marketplaceId);
        bool FillOrderItemsByListingId(IList<ListingOrderDTO> orderItems, MarketType market, string marketplaceId);
        bool FillOrderItemsBySKUOrBarcode(IList<ListingOrderDTO> orderItems,
            MarketType market,
            string marketplaceId);
        bool FillOrderItemsByStyleAndSize(IList<ListingOrderDTO> orderItems,
            MarketType market,
            string marketplaceId);
        bool FillOrderItemsBySourceMarketId(IList<ListingOrderDTO> orderItems, MarketType market,
            string marketplaceId);


        IList<ListingOrderDTO> GetOrderItems(long orderId);
        IList<ListingOrderDTO> GetOrderItems(string orderNumber);
        IQueryable<ListingOrderDTO> GetAllOrderItemsWithListingInfo();

        IList<ListingOrderDTO> GetOrderItemSources(long orderId);
        IList<ListingOrderDTO> GetOrderItemSources(string orderNumber);

        IList<Listing> GetPriceUpdateRequiredList(MarketType market, string marketplaceId);
        IList<Listing> GetQuantityUpdateRequiredList(MarketType market, string marketplaceId);
        IQueryable<ViewListing> GetViewListings();
        IQueryable<ViewUnmaskedListing> GetViewUnmaskedListings();
        IQueryable<ViewListingDTO> GetViewListingsAsDto(bool withUnmaskedStyles);
        IQueryable<ListingDTO> GetListingsAsListingDto();
        IQueryable<ListingQuantityDTO> GetAllAsQuantityDto();

        IList<StyleItemDTO> GetStyleItemIdListFromListingsBySKU(string sku);
    }
}
