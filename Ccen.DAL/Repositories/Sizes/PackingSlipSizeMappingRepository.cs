using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class PackingSlipSizeMappingRepository : Repository<PackingSlipSizeMapping>, IPackingSlipSizeMappingRepository
    {
        public PackingSlipSizeMappingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<PackingSlipSizeMappingDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }
        
        private IQueryable<PackingSlipSizeMappingDTO> AsDto(IQueryable<PackingSlipSizeMapping> query)
        {
            return query.Select(s => new PackingSlipSizeMappingDTO()
            {
                Id = s.Id,
                SourceSize = s.SourceSize,
                DisplaySize = s.DisplaySize,
            });
        }
    }
}
