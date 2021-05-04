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
    public interface IListingSizeIssueRepository : IRepository<ListingSizeIssue>
    {
        IQueryable<ListingSizeIssueDTO> GetAllAsDto();
    }
}
