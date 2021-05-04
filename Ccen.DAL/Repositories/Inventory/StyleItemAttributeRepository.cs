using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Core.Helpers;
using Ccen.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleItemAttributeRepository : Repository<StyleItemAttribute>, IStyleItemAttributeRepository
    {
        public StyleItemAttributeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<StyleItemAttributeDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<StyleItemAttribute>());
        }

        private IQueryable<StyleItemAttributeDTO> AsDto(IQueryable<StyleItemAttribute> query)
        {
            return query.Select(s => new StyleItemAttributeDTO()
            {
                Id = s.Id,
                StyleItemId = s.StyleItemId,

                Name = s.Name,
                Value = s.Value,

                CreateDate = s.CreateDate,
                CreatedBy = s.CreatedBy,

                UpdateDate = s.UpdateDate,
                UpdatedBy = s.UpdatedBy
            });
        }
    }
}
