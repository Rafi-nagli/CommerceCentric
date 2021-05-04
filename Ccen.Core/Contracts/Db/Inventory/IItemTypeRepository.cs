using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.Core.Contracts.Db
{
    public interface IItemTypeRepository : IRepository<ItemType>
    {
        IQueryable<ItemTypeDTO> GetAllAsDto();
    }
}
