using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Listings;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories.Listings
{
    public class ListingSizeIssueRepository : Repository<ListingSizeIssue>, IListingSizeIssueRepository
    {
        public ListingSizeIssueRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ListingSizeIssueDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }
        
        private IQueryable<ListingSizeIssueDTO> AsDto(IQueryable<ListingSizeIssue> query)
        {
            return query.Select(d => new ListingSizeIssueDTO()
            {
                Id = d.Id,
                ListingId = d.ListingId,

                AmazonSize = d.AmazonSize,
                StyleSize = d.StyleSize,

                IsActual = d.IsActual,

                ReCheckDate = d.ReCheckDate,
                CreateDate = d.CreateDate,
            });
        }
    }
}
