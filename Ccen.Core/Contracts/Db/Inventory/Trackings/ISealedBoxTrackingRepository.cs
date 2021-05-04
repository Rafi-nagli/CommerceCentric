using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface ISealedBoxTrackingRepository : IRepository<SealedBoxTracking>
    {
        IList<SealedBoxTrackingDTO> GetByBoxIdAsDto(long sealedBoxId);

        IList<EntityUpdateStatus<long>> UpdateBoxTrackingsForBox(long boxId,
            IList<SealedBoxTrackingDTO> boxTrackings,
            DateTime when,
            long? by);
    }
}
