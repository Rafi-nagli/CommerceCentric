using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class SizeGroupRepository : Repository<SizeGroup>, ISizeGroupRepository
    {
        public SizeGroupRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IList<SizeGroupDTO> GetAllAsDto()
        {
            return AsDto(GetAll()).ToList();
        }

        public IList<SizeGroupDTO> GetByItemType(int typeId)
        {
            var query = from sg in unitOfWork.GetSet<SizeGroup>()
                join m in unitOfWork.GetSet<SizeGroupToItemType>() on sg.Id equals m.SizeGroupId
                where m.ItemTypeId == typeId
                select sg;

            return AsDto(query).ToList();
        }

        private IQueryable<SizeGroupDTO> AsDto(IQueryable<SizeGroup> query)
        {
            return query.Select(s => new SizeGroupDTO()
            {
                Id = s.Id,
                Name = s.Name,
                Departments = s.Departments,
                SortOrder = s.SortOrder
            });
        }
    }
}
