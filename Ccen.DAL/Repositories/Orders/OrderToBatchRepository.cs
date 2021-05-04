using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class OrderToBatchRepository : Repository<OrderToBatch>, IOrderToBatchRepository
    {
        public OrderToBatchRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<OrderToBatchDTO> GetViewAllAsDto()
        {
            var query = from ob in GetAll()
                select new OrderToBatchDTO()
                {
                    BatchId = ob.BatchId,
                    ShippingInfoId = ob.ShippingInfoId,
                    OrderId = ob.OrderId,
                    SortIndex1 = ob.SortIndex1,
                    SortIndex2 = ob.SortIndex2,
                    SortIndex3 = ob.SortIndex3,
                    SortIndex4 = ob.SortIndex4,
                    SortIndex5 = ob.SortIndex5,
                    //SortIndex6 = ob.SortIndex6,
                };
            return query;
        }

        public IQueryable<OrderToBatchDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<OrderToBatchDTO> AsDto(IQueryable<OrderToBatch> query)
        {
            return query.Select(b => new OrderToBatchDTO()
            {
                Id = b.Id,
                ShippingInfoId = b.ShippingInfoId,
                BatchId = b.BatchId,
                OrderId = b.OrderId,
                SortIndex1 = b.SortIndex1,
                SortIndex2 = b.SortIndex2,
                SortIndex3 = b.SortIndex3,
                SortIndex4 = b.SortIndex4,
                SortIndex5 = b.SortIndex5,
                //SortIndex6 = b.SortIndex6,
            });
        }
    }
}
