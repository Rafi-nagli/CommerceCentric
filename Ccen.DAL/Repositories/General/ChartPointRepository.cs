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
    public class ChartPointRepository : Repository<ChartPoint>, IChartPointRepository
    {
        public ChartPointRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        
        public IQueryable<ChartPointDTO> GetAllAsDto()
        {
            return GetAll().Select(p => new ChartPointDTO()
            {
                Id = p.Id,
                ChartId = p.ChartId,
                Value = p.Value,
                Date = p.Date,
            });
        }
    }
}
