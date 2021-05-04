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
    public class ListingDefectRepository : Repository<ListingDefect>, IListingDefectRepository
    {
        public ListingDefectRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ListingDefectDTO> GetAllAsDto()
        {
            return AsDto(GetAll().Where(d => !d.IsDeleted));
        }

        public void MarkExceptFromFeedAsRemoved(string reportId)
        {
            var results = new List<string>();
            var items = GetFiltered(d => !d.IsDeleted
                                         && d.ReportId != reportId);

            foreach (var item in items)
            {
                item.IsDeleted = true;
            }
            unitOfWork.Commit();
        }

        public void CreateOrUpdate(ListingDefectDTO defect, DateTime when)
        {
            var dbDefect = GetFiltered(l => !l.IsDeleted
                && l.SKU == defect.SKU
                && l.MarketType == defect.MarketType
                && l.MarketplaceId == defect.MarketplaceId
                && l.FieldName == defect.FieldName
                && l.AlertType == defect.AlertType).FirstOrDefault();
            if (dbDefect != null)
            {
                dbDefect.FeedId = defect.FeedId ?? 0;

                dbDefect.ReportId = defect.ReportId;
                dbDefect.MarketType = defect.MarketType;
                dbDefect.MarketplaceId = defect.MarketplaceId;

                dbDefect.SKU = defect.SKU;
                dbDefect.ASIN = defect.ASIN;

                dbDefect.FieldName = defect.FieldName;
                dbDefect.CurrentValue = defect.CurrentValue;
                dbDefect.LastUpdated = defect.LastUpdated;

                dbDefect.AlertType = defect.AlertType;
                dbDefect.AlertName = defect.AlertName;
                dbDefect.Status = defect.Status;
                dbDefect.Explanation = defect.Explanation;

                dbDefect.UpdateDate = when;
            }
            else
            {
                dbDefect = new ListingDefect();

                dbDefect.FeedId = defect.FeedId ?? 0;

                dbDefect.ReportId = defect.ReportId;
                dbDefect.MarketType = defect.MarketType;
                dbDefect.MarketplaceId = defect.MarketplaceId;

                dbDefect.SKU = defect.SKU;
                dbDefect.ASIN = defect.ASIN;

                dbDefect.FieldName = defect.FieldName;
                dbDefect.CurrentValue = defect.CurrentValue;
                dbDefect.LastUpdated = defect.LastUpdated;

                dbDefect.AlertType = defect.AlertType;
                dbDefect.AlertName = defect.AlertName;
                dbDefect.Status = defect.Status;
                dbDefect.Explanation = defect.Explanation;

                dbDefect.UpdateDate = when;
                dbDefect.CreateDate = when;

                unitOfWork.ListingDefects.Add(dbDefect);
            }

            unitOfWork.Commit();
        }

        private IQueryable<ListingDefectDTO> AsDto(IQueryable<ListingDefect> query)
        {
            return query.Select(d => new ListingDefectDTO()
            {
                Id = d.Id,
                FeedId = d.FeedId,

                ReportId = d.ReportId,
                MarketType = d.MarketType,
                MarketplaceId = d.MarketplaceId,

                SKU = d.SKU,
                ASIN = d.ASIN,
                
                FieldName = d.FieldName,
                CurrentValue = d.CurrentValue,
                LastUpdated = d.LastUpdated,

                AlertType = d.AlertType,
                AlertName = d.AlertName,
                Status = d.Status,
                Explanation = d.Explanation,
            });
        }
    }
}
