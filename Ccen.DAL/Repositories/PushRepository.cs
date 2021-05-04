using System;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class PushRepository : Repository<Push>, IPushRepository
    {
        public PushRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public Push GetByRegistrationId(string registrationId)
        {
            return this.GetFiltered(p => p.RegistrationId == registrationId).FirstOrDefault();
        }
    }
}
