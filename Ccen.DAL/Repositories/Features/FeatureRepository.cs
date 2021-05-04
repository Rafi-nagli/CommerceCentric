using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories
{
    public class FeatureRepository : Repository<Feature>, IFeatureRepository
    {
        public FeatureRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<FeatureDTO> GetByItemType(int itemTypeId)
        {
            return AsDto(unitOfWork.GetSet<Feature>()
                        .Where(f => (f.ItemTypeId == itemTypeId
                            || !f.ItemTypeId.HasValue)
                            && f.IsActive)
                        .OrderBy(f => f.Order)
                        .ThenBy(f => f.Name))
                    .ToList();
        }

        public IQueryable<FeatureDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<FeatureDTO> AsDto(IQueryable<Feature> query)
        {
            return query.Select(f => new FeatureDTO()
            {
                Id = f.Id,
                ItemTypeId = f.ItemTypeId,
                Name = f.Name,
                ExtendedValue = f.ExtendedValue,
                Notes = f.Notes,
                Title = f.Title,
                Order = f.Order,
                ValuesType = f.ValuesType
            });
        }
    }
}
