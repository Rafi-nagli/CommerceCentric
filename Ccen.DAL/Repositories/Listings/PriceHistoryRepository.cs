using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class PriceHistoryRepository : Repository<PriceHistory>, IPriceHistoryRepository
    {
        public PriceHistoryRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IList<PriceHistoryDTO> GetByParentAsinOverPeriod(string asin, DateTime minDate, DateTime maxDate)
        {
            var query = (from l in unitOfWork.Listings.GetAll()
                join h in GetAll() on l.Id equals h.ListingId
                join i in unitOfWork.Items.GetAll() on l.ItemId equals i.Id
            
                where h.ChangeDate <= maxDate && i.ParentASIN == asin
                
                select new PriceHistoryDTO
                {
                    SKU = l.SKU,
                    ChangeDate = h.ChangeDate,
                    Price = h.Price
                }).OrderBy(q => q.ChangeDate).ToList();
            var skus = new List<string>();
            foreach (var dto in query)
            {
                if (!skus.Contains(dto.SKU))
                {
                    skus.Add(dto.SKU);
                }
            }

            var result = new List<PriceHistoryDTO>();
            foreach (var sku in skus)
            {
                var itemHistory = query.Where(q => q.SKU == sku).ToList();
                if (itemHistory.Count > 1)
                {
                    var earlyPrices = itemHistory.Where(q => q.ChangeDate <= minDate).ToList();
                    if (earlyPrices.Count() > 1)
                    {
                        itemHistory = itemHistory.SkipWhile(q => q.ChangeDate != earlyPrices.Max(p => p.ChangeDate)).ToList();
                    }
                    result.AddRange(itemHistory);
                }
            }
            return result;
        }
    }
}
