using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class FBAPickListEntryRepository : Repository<FBAPickListEntry>, IFBAPickListEntryRepository
    {
        public FBAPickListEntryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<FBAPickListEntryDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<FBAPickListEntry>().Select(f => new FBAPickListEntryDTO()
            {
                Id = f.Id,
                
                FBAPickListId = f.FBAPickListId,
                StyleId = f.StyleId,
                StyleString = f.StyleString,
                StyleItemId = f.StyleItemId,
                Quantity = f.Quantity,
                ListingId = f.ListingId,
                ListingParentASIN = f.ListingParentASIN,
                ListingASIN = f.ListingASIN,
                ListingSKU = f.ListingSKU,

                CreateDate = f.CreateDate,
                CreatedBy = f.CreatedBy
            });
        }
    }
}
