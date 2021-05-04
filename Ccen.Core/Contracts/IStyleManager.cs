using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IStyleManager
    {
        bool StoreOrUpdateBarcode(IUnitOfWork db,
            long styleItemId,
            string barcode);

        StyleItemDTO TryGetStyleItemIdFromOtherMarkets(IUnitOfWork db, 
            ItemDTO dtoItem);

        StyleItemDTO FindOrCreateStyleAndStyleItemForItem(IUnitOfWork db,
            int itemTypeId,
            ItemDTO dtoItem,
            bool canCreate,
            bool noVariation);

        bool UpdateStyleImageIfEmpty(IUnitOfWork db,
            long styleId,
            string imageUrl);
    }
}
