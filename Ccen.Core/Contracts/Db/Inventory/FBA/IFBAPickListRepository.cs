using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;
using Ccen.Core.Models.Enums;
using System.Linq;

namespace Amazon.Core.Contracts.Db
{
    public interface IFBAPickListRepository : IRepository<FBAPickList>
    {
        IQueryable<FBAPickListDTO> GetAllAsDto(ShipmentsTypeEnum FBAPickListType);
    }
}
