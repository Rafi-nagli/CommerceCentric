using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemBarcodeRepository : IRepository<StyleItemBarcode>
    {
        IQueryable<BarcodeDTO> GetAllAsDto();
        List<BarcodeDTO> CheckOnDuplications(IList<string> barcodes, long? excludeStyleId);
        List<BarcodeDTO> GetByStyleItemId(long styleItemId);

        IList<EntityUpdateStatus<long>> UpdateStyleItemBarcodeForStyleItem(long styleItemId,
            IList<BarcodeDTO> barcodes,
            DateTime when,
            long? by);
    }
}
