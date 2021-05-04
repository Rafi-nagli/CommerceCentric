using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts
{
    public interface IQuantityManager
    {
        void Process(IUnitOfWork db,
            long listingId,
            int quantity,
            QuantityChangeSourceType changeType,
            string orderId,
            long? styleId,
            string styleString,
            long? styleItemId,
            string sku,
            string size);


        void LogStyleItemQuantity(IUnitOfWork db,
            long styleItemId,
            int? newQuantity,
            int? oldQuantity,
            QuantityChangeSourceType type,
            string tag,
            long? sourceEntityTag,
            string sourceEntityName,
            DateTime when,
            long? by);

        long AddQuantityOperation(IUnitOfWork db,
            QuantityOperationDTO quantityOperation,
            DateTime when,
            long? by);


        void UpdateListingQuantity(IUnitOfWork db,
            QuantityChangeSourceType type,
            Listing listing,
            int? newQuantity,
            string size,
            string styleString,
            long? styleId,
            long? styleItemId,
            DateTime when,
            long? by);
    }
}
