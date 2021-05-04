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
    public class QuantityHistoryRepository : Repository<QuantityHistory>, IQuantityHistoryRepository
    {
        public QuantityHistoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<QuantityHistoryDTO> GetByOrderId(string orderId)
        {
            var query = GetAll().Where(o => o.OrderId == orderId);
            return AsDto(query).ToList();
        }

        public SoldSizeInfo GetSoldQtyByStyleIdAndSizeFromDate(string styleString, string size, DateTime? fromDate)
        {
            var query = from h in GetAll()
                where h.StyleString == styleString
                      && h.Size == size
                      && h.CreateDate > fromDate.Value
                group h by new {h.StyleId, h.Size}
                into byStyleIdSize
                select new SoldSizeInfo()
                {
                    //StyleId = byStyleIdSize.Key.StyleId,
                    Size = byStyleIdSize.Key.Size,
                    SoldQuantity = byStyleIdSize.Sum(s => s.QuantityChanged)
                };

            return query.FirstOrDefault();
        }

        private IQueryable<QuantityHistoryDTO> AsDto(IQueryable<QuantityHistory> query)
        {
            return query.Select(s => new QuantityHistoryDTO()
            {
                Id = s.Id,
                ListingId = s.ListingId,
                OrderId = s.OrderId,
                QuantityChanged = s.QuantityChanged,
                Size = s.Size,
                SKU = s.SKU,
                StyleId = s.StyleId,
                StyleString = s.StyleString,
                StyleItemId = s.StyleItemId,
                Type = s.Type
            });
        }
    }
}
