using Amazon.DTO.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.General;

namespace Amazon.Core.Contracts.Db.Charts
{
    public interface IChartRepository : IRepository<Chart>
    {
        IQueryable<ChartDTO> GetAllAsDto();
    }
}
