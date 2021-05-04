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
using Amazon.Core.Entities.Events;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO;
using Amazon.DTO.Events;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class SaleEventEntryRepository : Repository<SaleEventEntry>, ISaleEventEntryRepository
    {
        public SaleEventEntryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SaleEventEntryDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<SaleEventEntry>()
                join st in unitOfWork.GetSet<Style>() on se.StyleId equals st.Id
                select new SaleEventEntryDTO()
                {
                    Id = se.Id,
                    SaleEventId = se.SaleEventId,
                    StyleId = se.StyleId,
                    StyleString = st.StyleID,
                    QuantityPercent = se.QuantityPercent,
                    Price = se.Price,

                    CreateDate = se.CreateDate,
                    CreatedBy = se.CreatedBy
                };

            return query;
        }
    }
}
