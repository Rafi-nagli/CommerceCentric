using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ILabelRepository : IRepository<ViewLabel>
    {
        IList<OrderShippingInfoDTO> GetByOrderIdAsDto(long orderId);
    }
}
