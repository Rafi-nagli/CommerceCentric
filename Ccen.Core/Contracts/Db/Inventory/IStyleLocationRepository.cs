using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleLocationRepository : IRepository<StyleLocation>
    {
        IList<StyleLocationDTO> GetByStyleId(long styleId);
        IList<StyleLocationDTO> GetByStyleIdsAsDto(IList<long> styleIds);
        IQueryable<StyleLocationDTO> GetAllAsDTO();

        void UpdateLocationsForStyle(IStyleHistoryService styleHistory,
            long styleId,
            IList<StyleLocationDTO> locations,
            DateTime when,
            long? by);
    }
}
