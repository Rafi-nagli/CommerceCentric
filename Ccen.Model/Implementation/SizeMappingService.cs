using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Listings;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.Model.Implementation
{
    public class SizeMappingService : ISizeMappingService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public SizeMappingService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _time = time;
            _log = log;
            _dbFactory = dbFactory;
        }

        public IList<ItemDTO> CheckItemsSizeMappingIssue()
        {
            IList<ItemDTO> newSizeIssues = new List<ItemDTO>();

            using (var db = _dbFactory.GetRWDb())
            {
                var existListingWithIssues = db.ListingSizeIssues.GetAll().ToList();
                var allListingWithIssues = db.Items.GetAllWithSizeMappingIssues().ToList();
                var existListingIds = existListingWithIssues.Select(i => i.ListingId).ToList();

                newSizeIssues = allListingWithIssues
                    .Where(l => l.ListingEntityId.HasValue && !existListingIds.Contains(l.ListingEntityId.Value))
                    .ToList();

                foreach (var sizeIssue in newSizeIssues)
                {
                    db.ListingSizeIssues.Add(new ListingSizeIssue()
                    {
                        ListingId = sizeIssue.ListingEntityId.Value,
                        AmazonSize = sizeIssue.Size,
                        StyleSize = sizeIssue.StyleSize,

                        IsActual = true,
                        ReCheckDate = _time.GetUtcTime(),
                        CreateDate = _time.GetUtcTime(),
                    });
                }
                db.Commit();

                foreach (var existIssue in existListingWithIssues)
                {
                    var listingIssue = allListingWithIssues.FirstOrDefault(l => l.ListingEntityId == existIssue.ListingId);
                    if (listingIssue == null)
                    {
                        existIssue.IsActual = false;
                    }
                    else
                    {
                        existIssue.IsActual = true;
                    }
                    existIssue.ReCheckDate = _time.GetUtcTime();
                }
            }
            return newSizeIssues.OrderByDescending(l => l.OpenDate).ToList();   
        }

        public IList<ItemDTO> GetItemsSummaryWithSizeMappingIssue()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var allListingWithIssues = db.Items.GetAllWithSizeMappingIssues()
                    .Where(i => i.ListingEntityId.HasValue)
                    .ToList();
                return allListingWithIssues.OrderByDescending(l => l.OpenDate).ToList();
            }
        }
    }
}
