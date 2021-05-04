using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleImageRepository : IRepository<StyleImage>
    {
        IList<EntityUpdateStatus<long>> UpdateImagesForStyle(long styleId,
            IList<StyleImageDTO> images,
            DateTime when,
            long? by);

        IQueryable<StyleImageDTO> GetAllAsDto();
    }
}
