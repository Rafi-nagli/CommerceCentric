using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class BuyerRepository : Repository<Buyer>, IBuyerRepository
    {
        public BuyerRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public BuyerDTO GetByEmailAsDto(string email)
        {
            return AsDto(GetAll().Where(b => b.Email == email)).FirstOrDefault();
        }

        public bool CreateIfNotExistFromOrderDto(DTOMarketOrder order, DateTime when)
        {
            var wasCreated = false;
            if (!String.IsNullOrEmpty(order.BuyerEmail))
            {
                var dbBuyer = GetAll().FirstOrDefault(b => b.Email == order.BuyerEmail);
                if (dbBuyer != null)
                {
                    dbBuyer.LastOrderDate = order.OrderDate;
                    dbBuyer.Name = order.BuyerName;
                }
                else
                {
                    dbBuyer = new Buyer()
                    {
                        Email = order.BuyerEmail,
                        Name = order.BuyerName,

                        LastOrderDate = order.OrderDate,
                        CreateDate = when
                    };
                    Add(dbBuyer);
                    wasCreated = true;
                }
                unitOfWork.Commit();
            }
            return wasCreated;
        }

        private IQueryable<BuyerDTO> AsDto(IQueryable<Buyer> query)
        {
            return query.Select(b => new BuyerDTO()
            {
                Email = b.Email,
                Name = b.Name,

                InBlackList = b.InBlackList,
                InBlackListReason = b.InBlackListReason,
                InBlackListOrderNumber = b.InBlackListOrderNumber,
                InBlackListDate = b.InBlackListDate,
                InBlackListBy = b.InBlackListBy,

                RemoveSignConfirmation = b.RemoveSignConfirmation,
                RemoveSignConfirmationDate = b.RemoveSignConfirmationDate,

                CreateDate = b.CreateDate,
            });
        }
    }
}
