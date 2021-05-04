using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.DTO;
using Amazon.DTO.TrackingNumbers;

namespace Amazon.Core.Contracts.Db
{
    public interface ITrackingNumberStatusRepository : IRepository<TrackingNumberStatus>
    {
        IQueryable<TrackingNumberStatusDTO> GetUnDeliveredInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> boxIds);

        IQueryable<TrackingNumberStatusDTO> GetAllAsDto();
    }
}
