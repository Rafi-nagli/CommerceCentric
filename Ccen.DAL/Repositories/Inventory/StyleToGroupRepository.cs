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
    public class StyleToGroupRepository : Repository<StyleToGroup>, IStyleToGroupRepository
    {
        public StyleToGroupRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleToGroupDTO> GetAllAsDTO()
        {
            var query = from sg in unitOfWork.GetSet<StyleToGroup>()
                        join st in unitOfWork.GetSet<Style>() on sg.StyleId equals st.Id
                        select new StyleToGroupDTO()
                        {
                            Id = sg.Id,

                            StyleGroupId = sg.StyleGroupId,
                            StyleId = sg.StyleId,

                            StyleString = st.StyleID,

                            CreateDate = sg.CreateDate,
                            CreatedBy = sg.CreatedBy,
                        };

            return query;
        }
    }
}
