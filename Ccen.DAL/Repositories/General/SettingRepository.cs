using System;
using System.Linq;

using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class SettingRepository : Repository<Setting>, ISettingRepository
    {
        public SettingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public Setting GetByName(string name)
        {
            return unitOfWork.GetSet<Setting>().FirstOrDefault(r => r.Name == name);
        }

        public void Set(string name, string value)
        {
            var existSetting = unitOfWork.GetSet<Setting>().FirstOrDefault(r => r.Name == name);
            if (existSetting == null)
            {
                unitOfWork.GetSet<Setting>().Add(new Setting {Name = name, Value = value, UpdateDate = DateTime.UtcNow});
            }
            else
            {
                existSetting.Value = value;
                existSetting.UpdateDate = DateTime.UtcNow;
            }
        }

        public void RefreshDate(string name)
        {
            var existSetting = unitOfWork.GetSet<Setting>().FirstOrDefault(r => r.Name == name);
            if (existSetting != null)
            {
                existSetting.UpdateDate = DateTime.UtcNow;
            }
        }
    }
}
