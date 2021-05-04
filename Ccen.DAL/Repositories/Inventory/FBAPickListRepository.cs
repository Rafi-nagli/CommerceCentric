using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;
using Ccen.Core.Models.Enums;
using System.Linq;

namespace Amazon.DAL.Repositories
{
    public class FBAPickListRepository : Repository<FBAPickList>, IFBAPickListRepository
    {
        public FBAPickListRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<FBAPickListDTO> GetAllAsDto(ShipmentsTypeEnum FBAPickListType)
        {   
            var query = unitOfWork.GetSet<FBAPickList>()
                .Select(f => new FBAPickListDTO()
                {
                    Id = f.Id,
                    Status = f.Status,
                    FBAPickListType = f.FBAPickListType,
                    Archived = f.Archived,
                    CreateDate = f.CreateDate,
                    CreatedBy = f.CreatedBy,
                });

            if (FBAPickListType != ShipmentsTypeEnum.None)
            {
                query = query.Where(x => x.FBAPickListType == FBAPickListType.ToString());
            }
            return query;
        }
    }
}
