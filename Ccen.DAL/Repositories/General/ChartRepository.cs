using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO.Graphs;
using Amazon.Core.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db.Charts;

namespace Amazon.DAL.Repositories
{
    public class ChartRepository : Repository<Chart>, IChartRepository
    {
        public ChartRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        
        public IQueryable<ChartDTO> GetAllAsDto()
        {
            return GetAll().Select(p => new ChartDTO()
            {
                Id = p.Id,
                ChartName = p.ChartName,
                ChartSubGroup = p.ChartSubGroup,
                ChartTag = p.ChartTag,
                CreateDate = p.CreateDate,
            });
        }
    }
}
