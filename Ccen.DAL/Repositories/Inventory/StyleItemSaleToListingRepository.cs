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
    public class StyleItemSaleToListingRepository : Repository<StyleItemSaleToListing>, IStyleItemSaleToListingRepository
    {
        public StyleItemSaleToListingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<long> UpdateForSale(long saleId,
            IList<StyleItemSaleToListingDTO> saleToListings,
            DateTime when,
            long? by)
        {
            var changedListingIds = new List<long>();
            var dbExistItems = GetFiltered(l => l.SaleId == saleId).ToList();
            
            foreach (var dbItem in dbExistItems)
            {
                var existItem = saleToListings.FirstOrDefault(l => l.ListingId == dbItem.ListingId);
                if (existItem != null)
                {
                    existItem.Id = dbItem.Id;
                    if (dbItem.SaleToMarketId != existItem.SaleToMarketId
                        || dbItem.OverrideSalePrice != existItem.OverrideSalePrice) //TEMP: fixing issue with empty SaleToMarketId
                    {
                        dbItem.SaleToMarketId = existItem.SaleToMarketId;
                        dbItem.OverrideSalePrice = existItem.OverrideSalePrice;
                        changedListingIds.Add(dbItem.ListingId);
                    }
                }
                else
                {
                    Remove(dbItem);
                    changedListingIds.Add(dbItem.ListingId);
                }
            }

            var newItems = saleToListings.Where(l => l.Id == 0).ToList();
            foreach (var newItem in newItems)
            {
                Add(new StyleItemSaleToListing()
                {
                    SaleToMarketId = newItem.SaleToMarketId,
                    SaleId = saleId,

                    ListingId = newItem.ListingId,
                    OverrideSalePrice = newItem.OverrideSalePrice,

                    CreateDate = when,
                    CreatedBy = by
                });
                changedListingIds.Add(newItem.ListingId);
            }

            unitOfWork.Commit();

            return changedListingIds;
        }

        public IQueryable<ViewListingSaleDTO> GetAllListingSaleAsDTO()
        {
            return unitOfWork.Context.ViewListingSales.Select(l => new ViewListingSaleDTO()
            {
                Id = l.Id,
                ListingId = l.ListingId,
                SaleStartDate = l.SaleStartDate,
                SaleEndDate = l.SaleEndDate,
                MaxPiecesOnSale = l.MaxPiecesOnSale,
                PiecesSoldOnSale = l.PiecesSoldOnSale,
                MaxPiecesMode = l.MaxPiecesMode,
                SalePrice = l.SalePrice,
                SalePercent = l.SalePercent,
                ListingSaleCreateDate = l.ListingSaleCreateDate
            });
        }

        public IQueryable<StyleItemSaleToListingDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        
        private IQueryable<StyleItemSaleToListingDTO> AsDto(IQueryable<StyleItemSaleToListing> query)
        {
            return query.Select(s => new StyleItemSaleToListingDTO()
            {
                Id = s.Id,
                SaleId = s.SaleId,
                ListingId = s.ListingId,
                CreateDate = s.CreateDate,
                CreatedBy = s.CreatedBy,
            });
        }
    }
}
