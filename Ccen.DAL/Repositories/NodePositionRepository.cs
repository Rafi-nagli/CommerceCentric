using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class NodePositionRepository : Repository<NodePosition>, INodePositionRepository
    {
        public NodePositionRepository(IQueryableUnitOfWork unitOfWork) 
            : base(unitOfWork)
        {
        }
    }
}
