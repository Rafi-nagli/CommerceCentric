using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class QuantityOperationRepository : Repository<QuantityOperation>, IQuantityOperationRepository
    {
        public QuantityOperationRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SoldSizeInfo> GetSpecialCaseQuantities()
        {
            var items = unitOfWork.GetSet<ViewSpecialCaseQuantity>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.Id,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                Size = v.Size,
                StyleId = v.StyleId
            });
            return items;
        }        

        public IQueryable<QuantityOperationDTO> GetAllAsDtoWithChanges()
        {
            var operationsQuery = (from op in unitOfWork.GetSet<QuantityOperation>()
                                  join o in unitOfWork.GetSet<Order>().Select(x => new { x.AmazonIdentifier, x.MarketplaceId, x.Market }).Distinct() on op.OrderId equals o.AmazonIdentifier into withOrder
                                  from o in withOrder.DefaultIfEmpty()
                                  join u in unitOfWork.GetSet<User>() on op.CreatedBy equals u.Id into withUser
                                  from u in withUser.DefaultIfEmpty()
                                  select new
                                  {                                      
                                      op,
                                      o.MarketplaceId,
                                      o.Market,
                                      u
                                  });

            var changeQuery = (from ch in unitOfWork.GetSet<QuantityChange>()
                               join st in unitOfWork.GetSet<StyleItem>() on ch.StyleItemId equals st.Id into withStyleItem
                               from st in withStyleItem.DefaultIfEmpty()
                               join s in unitOfWork.GetSet<Style>() on ch.StyleId equals s.Id into withStyle
                               from s in withStyle.DefaultIfEmpty()
                               select new
                               {                                   
                                   ch,
                                   st,
                                   s
                               });

            var res = from o in operationsQuery
                      join c in changeQuery.GroupBy(y => y.ch.QuantityOperationId)
                      on o.op.Id equals c.Key
                      select new QuantityOperationDTO()
                      {
                          Id = o.op.Id,
                          OrderId = o.op.OrderId,
                          Market = o.Market,
                          MarketplaceId = o.MarketplaceId,
                          Comment = o.op.Comment,
                          Type = o.op.Type,
                          CreateDate = o.op.CreateDate,
                          CreatedBy = o.op.CreatedBy,
                          CreatedByName = o.u.Name,
                          QuantityChangesEnumerable = c.Select(x => new QuantityChangeDTO()
                          {
                              Id = x.ch.Id,
                              Quantity = x.ch.Quantity,
                              Size = x.st.Size,
                              StyleItemId = x.ch.StyleItemId,
                              StyleId = x.ch.StyleId,
                              StyleString = x.s.StyleID,
                              InActive = x.ch.InActive,
                              CreateDate = x.ch.CreateDate
                          })
                      };
            
            return res;
        }
        public IQueryable<QuantityOperationDTO> GetAllAsDto()
        {
            var operationsQuery = from op in unitOfWork.GetSet<QuantityOperation>()
                                  join o in unitOfWork.GetSet<Order>() on op.OrderId equals o.AmazonIdentifier into withOrder
                                  from o in withOrder.DefaultIfEmpty()
                                  join u in unitOfWork.GetSet<User>() on op.CreatedBy equals u.Id into withUser
                                  from u in withUser.DefaultIfEmpty()
                                  select new
                                  {                                      
                                      op,
                                      o,
                                      u
                                  };            

            var res = from o in operationsQuery                      
                      select new QuantityOperationDTO()
                      {
                          Id = o.op.Id,
                          OrderId = o.op.OrderId,
                          Market = o.o.Market,
                          MarketplaceId = o.o.MarketplaceId,
                          Comment = o.op.Comment,
                          Type = o.op.Type,
                          CreateDate = o.op.CreateDate,
                          CreatedBy = o.op.CreatedBy,
                          CreatedByName = o.u.Name                          
                      };
            return res;
        }

        private IQueryable<QuantityOperationDTO> AsDto(IQueryable<QuantityOperation> query)
        {
            return query.Select(q => new QuantityOperationDTO()
            {
                Id = q.Id,
                OrderId = q.OrderId,
                Comment = q.Comment,
                Type = q.Type,
                CreateDate = q.CreateDate
            });
        }
    }
}
