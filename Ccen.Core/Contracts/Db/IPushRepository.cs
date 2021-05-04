using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface IPushRepository : IRepository<Push>
    {
        Push GetByRegistrationId(string registrationId);
    }
}
