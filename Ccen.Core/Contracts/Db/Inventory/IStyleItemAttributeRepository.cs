using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;
using Ccen.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemAttributeRepository : IRepository<StyleItemAttribute>
    {
        IQueryable<StyleItemAttributeDTO> GetAllAsDto();
    }
}
