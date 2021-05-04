using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ICustomBarcodeRepository : IRepository<CustomBarcode>
    {
        IQueryable<CustomBarcodeDTO> GetAllAsDto();
    }
}
