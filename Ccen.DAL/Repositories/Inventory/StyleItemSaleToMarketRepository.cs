using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class StyleItemSaleToMarketRepository : Repository<StyleItemSaleToMarket>, IStyleItemSaleToMarketRepository
    {
        public StyleItemSaleToMarketRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public void UpdateForSale(long saleId,
            IList<StyleItemSaleToMarketDTO> saleToMarkets,
            DateTime when,
            long? by)
        {
            var dbExistItems = GetFiltered(l => l.SaleId == saleId).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = saleToMarkets.FirstOrDefault(l => l.Market == dbItem.Market
                    && (l.MarketplaceId == dbItem.MarketplaceId || String.IsNullOrEmpty(dbItem.MarketplaceId)));

                if (existItem != null)
                {
                    var hasChanged = existItem.SalePrice != dbItem.SalePrice
                                     || existItem.SFPSalePrice != dbItem.SFPSalePrice
                                     || existItem.SalePercent != dbItem.SalePercent
                                     || existItem.ApplyToNewListings != dbItem.ApplyToNewListings;

                    if (hasChanged)
                    {
                        dbItem.SalePrice = existItem.SalePrice;
                        dbItem.SFPSalePrice = existItem.SFPSalePrice;
                        dbItem.SalePercent = existItem.SalePercent;
                        dbItem.ApplyToNewListings = existItem.ApplyToNewListings;
                    }

                    existItem.Id = dbItem.Id;
                }
                else
                {
                    Remove(dbItem);
                }
            }

            var newItems = saleToMarkets.Where(l => l.Id == 0).ToList();
            foreach (var newItem in newItems)
            {
                var dbItem = new StyleItemSaleToMarket()
                {
                    SaleId = saleId,

                    Market = newItem.Market,
                    MarketplaceId = String.IsNullOrEmpty(newItem.MarketplaceId) ? null : newItem.MarketplaceId,

                    SalePrice = newItem.SalePrice,
                    SFPSalePrice = newItem.SFPSalePrice,
                    SalePercent = newItem.SalePercent,
                    ApplyToNewListings = newItem.ApplyToNewListings,

                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);
                unitOfWork.Commit();
                newItem.Id = dbItem.Id;
            }
        }

        public IQueryable<StyleItemSaleToMarketDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }
        
        private IQueryable<StyleItemSaleToMarketDTO> AsDto(IQueryable<StyleItemSaleToMarket> query)
        {
            return query.Select(s => new StyleItemSaleToMarketDTO()
            {
                Id = s.Id,
                SaleId = s.SaleId,
                Market = s.Market,
                MarketplaceId = s.MarketplaceId,

                SalePrice = s.SalePrice,
                SFPSalePrice = s.SFPSalePrice,
                SalePercent = s.SalePercent,
                ApplyToNewListings = s.ApplyToNewListings,

                CreateDate = s.CreateDate,
                CreatedBy = s.CreatedBy,
            });
        }
    }
}
