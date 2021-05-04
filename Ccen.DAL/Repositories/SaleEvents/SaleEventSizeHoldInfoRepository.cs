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
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Events;
using Amazon.DTO.Inventory;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class SaleEventSizeHoldInfoRepository : Repository<SaleEventSizeHoldInfo>, ISaleEventSizeHoldInfoRepository
    {
        public SaleEventSizeHoldInfoRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IQueryable<SoldSizeInfo> GetSoldQuantitiesByStyleItem()
        {
            var items = from se in unitOfWork.GetSet<ViewSaleEventSoldByStyleItem>()
                        join si in unitOfWork.GetSet<StyleItem>() on se.Id equals si.Id
                        select new SoldSizeInfo
                        {
                            StyleItemId = se.Id,
                            StyleId = si.StyleId,
                            SoldQuantity = se.SoldQuantity,
                            TotalSoldQuantity = se.TotalSoldQuantity,
                        };
            return items;
        }

        public IQueryable<SoldSizeInfo> GetHoldedQuantitiesByStyleItem()
        {
            var items = from se in unitOfWork.GetSet<ViewSaleEventHoldByStyleItem>()
                join si in unitOfWork.GetSet<StyleItem>() on se.Id equals si.Id
                select new SoldSizeInfo
                {
                    StyleItemId = se.Id,
                    StyleId = si.StyleId,
                    SoldQuantity = se.SoldQuantity,
                    TotalSoldQuantity = se.TotalSoldQuantity,
                };
            return items;
        }

        public IQueryable<SaleEventSizeHoldInfoDTO> GetAllAsDto()
        {
            var query = from se in unitOfWork.GetSet<SaleEventSizeHoldInfo>()
                select new SaleEventSizeHoldInfoDTO()
                {
                    Id = se.Id,
                    EventEntryId = se.EventEntryId,
                    StyleItemId = se.StyleItemId,
                    HoldedCount = se.HoldedCount,
                    PurchasedCount = se.PurchasedCount,
                    FeedIndex = se.FeedIndex,
                };

            return query;
        }
    }
}
