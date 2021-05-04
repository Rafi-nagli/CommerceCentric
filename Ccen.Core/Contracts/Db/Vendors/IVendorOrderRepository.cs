using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.Core.Contracts.Db
{
    public interface IVendorOrderRepository : IRepository<VendorOrder>
    {
        IQueryable<VendorOrderDTO> GetAllAsDto();
    }
}
