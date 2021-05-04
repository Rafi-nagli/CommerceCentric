using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class MarketplaceRepository : Repository<Marketplace>, IMarketplaceRepository
    {
        public MarketplaceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<MarketplaceDTO> GetAllAsDto()
        {
            return GetAll().Select(m => new MarketplaceDTO()
            {
                Id = m.Id,
                CompanyId = m.CompanyId,

                Name = m.Name,
                Market = m.Market,
                MarketplaceId = m.MarketplaceId,

                Key1 = m.Key1,
                Key2 = m.Key2,
                Key3 = m.Key3,
                Key4 = m.Key4,
                Key5 = m.Key5,
                Token = m.Token,
                SellerId = m.SellerId,
                EndPointUrl = m.EndPointUrl,

                StoreLogo = m.StoreLogo,
                StoreUrl = m.StoreUrl,
                DisplayName = m.DisplayName,
                PackingSlipFooterTemplate = m.PackingSlipFooterTemplate,
                TemplateFolder = m.TemplateFolder,

                SortOrder = m.SortOrder,
                IsHidden = m.IsHidden,
                IsActive = m.IsActive,
            }).ToList();
        }
    }
}
