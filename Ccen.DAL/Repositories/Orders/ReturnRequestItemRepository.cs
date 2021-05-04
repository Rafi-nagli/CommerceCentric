using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.Core;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories
{
    public class ReturnRequestItemRepository : Repository<ReturnRequestItem>, IReturnRequestItemRepository
    {
        public ReturnRequestItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
