using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Listings;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IListingDefectRepository : IRepository<ListingDefect>
    {
        IQueryable<ListingDefectDTO> GetAllAsDto();
        void MarkExceptFromFeedAsRemoved(string reportId);
        void CreateOrUpdate(ListingDefectDTO defect, DateTime when);
    }
}
