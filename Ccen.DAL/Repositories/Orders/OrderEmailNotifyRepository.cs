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
    public class OrderEmailNotifyRepository : Repository<OrderEmailNotify>, IOrderEmailNotifyRepository
    {
        public OrderEmailNotifyRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public bool IsExist(string orderNumber, OrderEmailNotifyType type)
        {
            if (String.IsNullOrEmpty(orderNumber))
                return false;
            return GetAll().Any(n => n.OrderNumber == orderNumber && n.Type == (int) type);
        }

        public IList<OrderEmailNotifyDto> GetForOrders(IList<string> orderNumbers)
        {
            var query = from n in GetAll()
                where orderNumbers.Contains(n.OrderNumber)
                orderby n.CreateDate descending 
                select n;

            return AsDto(query).ToList();
        }

        private IQueryable<OrderEmailNotifyDto> AsDto(IQueryable<OrderEmailNotify> query)
        {
            return query.Select(n => new OrderEmailNotifyDto()
            {
                Id = n.Id,
                Type = n.Type,
                OrderNumber = n.OrderNumber,
                Reason = n.Reason,
                CreateDate = n.CreateDate,
                CreatedBy = n.CreatedBy,
            });
        }
    }
}
