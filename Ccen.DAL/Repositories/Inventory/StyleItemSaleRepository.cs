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
    public class StyleItemSaleRepository : Repository<StyleItemSale>, IStyleItemSaleRepository
    {
        public StyleItemSaleRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleItemSaleDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        
        private IQueryable<StyleItemSaleDTO> AsDto(IQueryable<StyleItemSale> query)
        {
            return query.Select(s => new StyleItemSaleDTO()
            {
                Id = s.Id,
                StyleItemId = s.StyleItemId,
                
                SaleStartDate = s.SaleStartDate,
                SaleEndDate = s.SaleEndDate,
                MaxPiecesOnSale = s.MaxPiecesOnSale,
                MaxPiecesMode = s.MaxPiecesMode,
                PiecesSoldOnSale = s.PiecesSoldOnSale,

                CloseDate = s.CloseDate,
                IsDeleted = s.IsDeleted,

                CreateDate = s.CreateDate,
                CreatedBy = s.CreatedBy,
            });
        }
    }
}
