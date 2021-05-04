using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.Core.Contracts.Db
{
    public interface IVendorOrderAttachmentRepository : IRepository<VendorOrderAttachment>
    {
        IQueryable<VendorOrderAttachmentDTO> GetAllAsDto();
    }
}
