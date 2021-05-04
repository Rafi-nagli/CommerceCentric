using Amazon.Core;
using Ccen.Core.Entities.TrackingNumbers;
using Ccen.DTO.Trackings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Contracts.Db.Trackings
{
    public interface ICustomTrackingNumberRepository : IRepository<CustomTrackingNumber>
    {
        IQueryable<CustomTrackingNumberDTO> GetAllAsDto();
    }
}
