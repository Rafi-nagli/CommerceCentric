using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleToGroupRepository : IRepository<StyleToGroup>
    {
        IQueryable<StyleToGroupDTO> GetAllAsDTO();
    }
}
