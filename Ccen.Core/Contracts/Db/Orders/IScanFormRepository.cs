using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.Core.Contracts.Db
{
    public interface IScanFormRepository : IRepository<ScanForm>
    {
        IQueryable<ScanFormDTO> GetAllAsDto();
    }
}
