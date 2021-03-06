using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IBuyerRepository : IRepository<Buyer>
    {
        BuyerDTO GetByEmailAsDto(string email);
        bool CreateIfNotExistFromOrderDto(DTOMarketOrder order, DateTime when);
    }
}
