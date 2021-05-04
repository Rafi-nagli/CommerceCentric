using System;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.DAL.Repositories
{
    public class VendorOrderAttachmentRepository : Repository<VendorOrderAttachment>, IVendorOrderAttachmentRepository
    {
        public VendorOrderAttachmentRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<VendorOrderAttachmentDTO> GetAllAsDto()
        {
            return GetAll().Select(a => new VendorOrderAttachmentDTO()
            {
                Id = a.Id,
                VendorOrderId = a.VendorOrderId,
                FileName = a.FileName,
                CreateDate = a.CreateDate
            });
        }
    }
}
