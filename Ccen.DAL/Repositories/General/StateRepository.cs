using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class StateRepository : Repository<State>, IStateRepository
    {
        public StateRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public string GetCodeByName(string name)
        {
            var state = GetAll().FirstOrDefault(s => s.StateName == name);
            if (state != null)
            {
                return state.StateCode;
            }
            return name;
        }
    }
}
