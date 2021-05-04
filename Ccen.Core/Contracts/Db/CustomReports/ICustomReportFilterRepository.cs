using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.CustomReports;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.CustomReports;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomReportFilterRepository : IRepository<CustomReportFilter>
    {
        IList<EntityUpdateStatus<long>> BulkUpdateForFilter(long reportId,
            IList<CustomReportFilterDTO> filters,
            DateTime when,
            long? by);
        IQueryable<CustomReportFilterDTO> GetAllAsDto();       
    }
}
