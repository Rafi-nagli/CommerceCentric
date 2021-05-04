using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface ISettingRepository : IRepository<Setting>
    {
        Setting GetByName(string name);
        void Set(string name, string value);
        void RefreshDate(string name);
    }
}
