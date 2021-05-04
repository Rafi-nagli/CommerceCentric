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
    public class OrderNotifyRepository : Repository<OrderNotify>, IOrderNotifyRepository
    {
        public OrderNotifyRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public bool IsExist(long orderId, OrderNotifyType notifyType)
        {
            return GetAll().Any(n => n.OrderId == orderId && n.Type == (int) notifyType);
        }

        public void StoreGetRateMessage(long orderId, GetRateResultType resultType, string message, DateTime when)
        {
            unitOfWork.OrderNotifies.Add(new OrderNotify()
            {
                OrderId = orderId,
                Type = (int)OrderNotifyType.CalcRate,
                Status = (int)resultType,
                Message = StringHelper.Substring(message, 255),
                CreateDate = when,
            });
            unitOfWork.Commit();
        }

        public IQueryable<OrderNotifyDto> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IList<OrderNotifyDto> GetForOrders(IList<long> orderIds)
        {
            var query = from n in GetAll()
                where orderIds.Contains(n.OrderId)
                orderby n.CreateDate descending 
                select n;

            return AsDto(query).ToList();
        }

        public void UpdateForOrder(long orderId, 
            OrderNotifyType notifyType,
            IList<OrderNotifyDto> notifies,
            DateTime when)
        {
            notifies = notifies ?? new List<OrderNotifyDto>();

            var dbExistNotifies = GetAll()
                .Where(n => n.OrderId == orderId && n.Type == (int)notifyType)
                .ToList();

            foreach (var notify in notifies)
            {
                var dbExist = dbExistNotifies.FirstOrDefault(n => n.Message == notify.Message);
                if (dbExist == null)
                {
                    dbExist = new OrderNotify()
                    {
                        OrderId = orderId,
                        CreateDate = when,
                        Type = (int)notifyType,
                        Status = 1,
                        Message = notify.Message,
                        Params = notify.Params,
                    };
                    Add(dbExist);
                    unitOfWork.Commit();                    
                }

                notify.Id = dbExist.Id;
            }

            var toRemoveNotifies = dbExistNotifies.Where(n => !notifies.Any(newN => newN.Id == n.Id)).ToList();
            foreach (var toRemove in toRemoveNotifies)
            {
                Remove(toRemove);
            }

            unitOfWork.Commit();
        }

        private IQueryable<OrderNotifyDto> AsDto(IQueryable<OrderNotify> query)
        {
            return query.Select(n => new OrderNotifyDto()
            {
                Id = n.Id,
                Type = n.Type,
                Status = n.Status,
                OrderId = n.OrderId,
                Message = n.Message,
                Params = n.Params,
                CreateDate = n.CreateDate
            });
        }
    }
}
