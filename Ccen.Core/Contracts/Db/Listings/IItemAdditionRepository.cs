using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Graphs;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IItemAdditionRepository : IRepository<ItemAddition>
    {
        IQueryable<ItemAdditionDTO> GetAllAsDTO();
    }
}
