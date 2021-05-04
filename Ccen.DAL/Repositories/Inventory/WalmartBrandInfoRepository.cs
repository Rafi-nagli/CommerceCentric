using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.DAL.Repositories.Inventory
{
    public class WalmartBrandInfoRepository : Repository<WalmartBrandInfo>, IWalmartBrandInfoRepository
    {
        public WalmartBrandInfoRepository(IQueryableUnitOfWork db) : base(db)
        {

        }

        public IQueryable<WalmartBrandInfoDto> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IQueryable<WalmartBrandInfoDto> AsDto(IQueryable<WalmartBrandInfo> query)
        {
            return query.Select(q => new WalmartBrandInfoDto()
            {
                Id = q.Id,
                Brand = q.Brand,
                Character = q.Character,
                GlobalBrandLicense = q.GlobalBrandLicense
            });
        }
    }
}
