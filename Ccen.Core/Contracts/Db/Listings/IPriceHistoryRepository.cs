using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IPriceHistoryRepository : IRepository<PriceHistory>
    {
        IList<PriceHistoryDTO> GetByParentAsinOverPeriod(string asin, DateTime minDate, DateTime maxDate);
    }
}
