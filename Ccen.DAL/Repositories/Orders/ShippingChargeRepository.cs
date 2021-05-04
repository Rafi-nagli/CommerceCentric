using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class ShippingChargeRepository : Repository<ShippingCharge>, IShippingChargeRepository
    {
        public ShippingChargeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public IQueryable<ShippingChargeDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }


        private IQueryable<ShippingChargeDTO> AsDto(IQueryable<ShippingCharge> query)
        {
            return query.Select(i => new ShippingChargeDTO()
            {
                Id = i.Id,
                
                ShippingMethodId = i.ShippingMethodId,
                ChargePercent = i.ChargePercent,

                CreateDate = i.CreateDate,
                CreatedBy = i.CreatedBy,
            });
        }
    }
}
