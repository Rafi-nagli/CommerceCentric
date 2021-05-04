using System;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.DAL.Repositories
{
    public class VendorOrderRepository : Repository<VendorOrder>, IVendorOrderRepository
    {
        public VendorOrderRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<VendorOrderDTO> GetAllAsDto()
        {
            return GetAll().Select(o => new VendorOrderDTO()
            {
                Id = o.Id,
                Status = o.Status,
                VendorName = o.VendorName,
                Description = o.Description,
                PaidAmount = o.PaidAmount,
                CreateDate = o.CreateDate,
                IsDeleted = o.IsDeleted
            });
        }
    }
}
