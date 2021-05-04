using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomFeedScheduleRepository : IRepository<CustomFeedSchedule>
    {
        IQueryable<CustomFeedScheduleDTO> GetAllAsDto();
    }
}
