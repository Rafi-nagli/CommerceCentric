using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class StyleItemActionHistoryRepository : Repository<StyleItemActionHistory>, IStyleItemActionHistoryRepository
    {
        public StyleItemActionHistoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
