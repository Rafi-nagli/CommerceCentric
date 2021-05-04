using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Markets;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts
{
    public interface IAutoCreateListingService
    {
        //void CreateWalmartListings(IWalmartOpenApi api, IBarcodeService barcodeService);
        //void CreateWalmartCAListings();
        //void CreateEBayListings();
        void CreateListings();

        ParentItemDTO CreateFromStyle(IUnitOfWork db,
            long styleId,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages);

        ParentItemDTO CreateFromStyle(IUnitOfWork db,
            string styleString,
            decimal? customPrice,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages);

        ParentItemDTO CreateFromStyle(IUnitOfWork db,
            StyleEntireDto style,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages);

        ParentItemDTO CreateAndMergeWithExistFromStyle(IUnitOfWork db,
            StyleEntireDto style,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages);

        ParentItemDTO CreateFromParentASIN(IUnitOfWork db,
            string asin,
            int market,
            string marketplaceId,
            bool includeAllChild,
            bool includeWithZeroQty,
            int? minQty,
            out IList<MessageString> messages);

        void PrepareData(ParentItemDTO parentItemDto);

        CallResult<string> Save(ParentItemDTO parentItemDto,
            string newComment,
            IUnitOfWork db,
            DateTime when,
            long? by);
    }
}
