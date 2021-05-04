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
using Amazon.DTO.DropShippers;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomReportFieldRepository : IRepository<CustomReportField>
    {
        IQueryable<CustomReportFieldDTO> GetAllAsDto();

        IList<EntityUpdateStatus<long>> BulkUpdateForFeed(long reportId,
            IList<CustomReportFieldDTO> fields,
            DateTime when,
            long? by);
    }
}
