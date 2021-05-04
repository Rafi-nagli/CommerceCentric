using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleGroupRepository : Repository<StyleGroup>, IStyleGroupRepository
    {
        public StyleGroupRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleGroupDTO> GetAllAsDTO()
        {
            var countByGroup = from sToG in unitOfWork.GetSet<StyleToGroup>()
                               group sToG by sToG.StyleGroupId into byGroup
                               select new
                               {
                                   GroupId = byGroup.Key,
                                   Count = byGroup.Count()
                               };

            var query = from sg in unitOfWork.GetSet<StyleGroup>()
                        join sToG in countByGroup on sg.Id equals sToG.GroupId
                        select new StyleGroupDTO()
                        {
                            Id = sg.Id,
                            Name = sg.Name,
                            StyleCount = sToG.Count,
                            CreateDate = sg.CreateDate,
                            CreatedBy = sg.CreatedBy,
                        };

            return query;
        }
    }
}
