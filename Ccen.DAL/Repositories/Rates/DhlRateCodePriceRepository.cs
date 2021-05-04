using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class DhlRateCodePriceRepository : Repository<DhlRateCodePrice>, IDhlRateCodePriceRepository
    {
        public DhlRateCodePriceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }


        public void Update(DhlRateCodePriceDTO rate)
        {
            var existEntry = Get(rate.Id);
            if (existEntry != null)
            {
                existEntry.Price = rate.Price;
            }
            unitOfWork.Commit();
        }

        public long Store(DhlRateCodePriceDTO rate)
        {
            var entity = new DhlRateCodePrice()
            {
                ProductCode = rate.ProductCode,
                Package = rate.Package,
                RateCode = rate.RateCode,
                Weight = rate.Weight,
                Price = rate.Price,
            };

            Add(entity);
            unitOfWork.Commit();

            return entity.Id;
        }

        public IQueryable<DhlRateCodePriceDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<DhlRateCodePriceDTO> AsDto(IQueryable<DhlRateCodePrice> query)
        {
            return query.Select(b => new DhlRateCodePriceDTO()
            {
                Id = b.Id,

                ProductCode = b.ProductCode,
                Package = b.Package,
                Weight = b.Weight,
                RateCode = b.RateCode,
                Price = b.Price,
            });
        }
    }
}
