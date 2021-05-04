using Amazon.Core;
using Amazon.DAL;
using Ccen.Core.Contracts.Db.Trackings;
using Ccen.Core.Entities.TrackingNumbers;
using Ccen.DTO.Trackings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DAL.Repositories.Trackings
{
    public class CustomTrackingNumberRepository : Repository<CustomTrackingNumber>, ICustomTrackingNumberRepository
    {
        public CustomTrackingNumberRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<CustomTrackingNumberDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomTrackingNumberDTO> AsDto(IQueryable<CustomTrackingNumber> query)
        {
            return query.Select(s => new CustomTrackingNumberDTO()
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,

                AttachedDate = s.AttachedDate,
                AttachedToShippingInfoId = s.AttachedToShippingInfoId,
                AttachedToTrackingNumber = s.AttachedToTrackingNumber,

                CreateDate = s.CreateDate,
            });
        }
    }
}
