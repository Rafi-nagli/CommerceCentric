using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db
{
    public interface IQuantityOperationRepository : IRepository<QuantityOperation>
    {
        IQueryable<SoldSizeInfo> GetSpecialCaseQuantities();
        IQueryable<QuantityOperationDTO> GetAllAsDtoWithChanges();
    }
}
