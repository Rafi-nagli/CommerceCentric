using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Sizes;
using Amazon.DTO.TrackingNumbers;
using System;
using Amazon.Core.Entities.Inventory;
using System.Data.Entity.Core.Objects;

namespace Amazon.DAL.Repositories
{
    public class TrackingNumberStatusRepository : Repository<TrackingNumberStatus>, ITrackingNumberStatusRepository
    {
        public TrackingNumberStatusRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public IQueryable<TrackingNumberStatusDTO> GetUnDeliveredInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> boxIds)
        {
            var now = when;
            var toDate = when.AddHours(-24);
            var toWhenExpDelDate = when.AddHours(-8);
            var deliveredReCheckPeriod = when.AddDays(-3);
            var oneYear = when.AddYears(-1); //NOTE: Tracking number was resused by USPS
            var hasBoxIdFilters = boxIds != null && boxIds.Any();

            var query = from btr in unitOfWork.GetSet<OpenBoxTracking>()
                        join tr in unitOfWork.GetSet<TrackingNumberStatus>() on btr.TrackingNumber equals tr.TrackingNumber
                        //where !hasBoxIdFilters || boxIds.Contains(btr.BoxId)
                        select new TrackingNumberStatusDTO()
                        {
                            Id = tr.Id,

                            Carrier = tr.Carrier,
                            TrackingNumber = tr.TrackingNumber,
                            
                            ActualDeliveryDate = tr.ActualDeliveryDate,
                            DeliveredStatus = tr.DeliveredStatus,
                            IsDelivered = tr.IsDelivered,

                            LastTrackingRequestDate = tr.LastTrackingRequestDate,
                            TrackingRequestAttempts = tr.TrackingRequestAttempts,
                            TrackingStateDate = tr.TrackingStateDate,
                            TrackingStateEvent = tr.TrackingStateEvent,
                            TrackingLocation = tr.TrackingLocation,

                            CreateDate = tr.CreateDate,
                        };

            if (excludeRecentlyProcessed)
            {
                query = query.Where(tr => (tr.TrackingRequestAttempts <= 200)
                                            && (!tr.LastTrackingRequestDate.HasValue ||
                                                tr.LastTrackingRequestDate < toDate));
                                            
            }

            return query;
        }

        public IQueryable<TrackingNumberStatusDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<TrackingNumberStatusDTO> AsDto(IQueryable<TrackingNumberStatus> query)
        {
            return query.Select(s => new TrackingNumberStatusDTO()
            {
                Id = s.Id,
                Carrier = s.Carrier,
                TrackingNumber = s.TrackingNumber,
                LastTrackingRequestDate = s.LastTrackingRequestDate,
                TrackingRequestAttempts = s.TrackingRequestAttempts,
                IsDelivered = s.IsDelivered,
                DeliveredStatus = s.DeliveredStatus,
                TrackingStateSource = s.TrackingStateSource,
                TrackingStateDate = s.TrackingStateDate,
                TrackingStateType = s.TrackingStateType,
                TrackingStateEvent = s.TrackingStateEvent,
                TrackingLocation = s.TrackingLocation,
                ScanDate = s.ScanDate,
                ActualDeliveryDate = s.ActualDeliveryDate,
                CreateDate = s.CreateDate,
            });
        }
    }
}
