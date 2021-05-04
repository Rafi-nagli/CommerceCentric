using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.General;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Core.Contracts.Db.Charts
{
    public interface IChartPointRepository : IRepository<ChartPoint>
    {
        
        IQueryable<ChartPointDTO> GetAllAsDto();
    }
}
