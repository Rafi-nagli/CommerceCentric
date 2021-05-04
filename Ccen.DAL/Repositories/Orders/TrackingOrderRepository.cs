using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class TrackingOrderRepository : Repository<TrackingOrder>, ITrackingOrderRepository
    {
        public TrackingOrderRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
