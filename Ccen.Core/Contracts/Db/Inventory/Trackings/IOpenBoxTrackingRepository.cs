using System;
using System.Collections.Generic;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IOpenBoxTrackingRepository : IRepository<OpenBoxTracking>
    {
        IList<OpenBoxTrackingDTO> GetByBoxIdAsDto(long openBoxId);

        IList<EntityUpdateStatus<long>> UpdateBoxTrackingsForBox(long boxId,
            IList<OpenBoxTrackingDTO> boxTrackings,
            DateTime when,
            long? by);
    }
}
