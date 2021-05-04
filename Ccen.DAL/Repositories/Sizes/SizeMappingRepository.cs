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
    public class SizeMappingRepository : Repository<SizeMapping>, ISizeMappingRepository
    {
        public SizeMappingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SizeMappingDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IList<SizeMappingDTO> GetExists(string styleSize, string itemSize)
        {
            return AsDto(GetAll().Where(s => s.StyleSize == styleSize && s.ItemSize == itemSize)).ToList();
        }

        public IList<SizeDTO> GetStyleSizesByItemSize(string itemSize, int itemTypeId)
        {
            var query = from m in GetAll()
                join s in unitOfWork.GetSet<Size>() on m.StyleSize equals s.Name 
                //from s in withSize.DefaultIfEmpty()
                join g in unitOfWork.GetSet<SizeGroup>() on s.SizeGroupId equals g.Id into withGroup
                from g in withGroup.DefaultIfEmpty()
                where m.ItemSize == itemSize
                orderby m.Priority ascending,
                    s.SortOrder ascending 
                select new SizeDTO()
                {
                    Id = s.Id,
                    Name = s.Name,
                    SortOrder = m.Priority,
                    SizeGroupId = s.SizeGroupId,

                    SizeGroupName = g.Name,
                    Departments = String.IsNullOrEmpty(s.Departments) ?  g.Departments : s.Departments,
                };
            
            var items = query.ToList();

            var groupIdListByItemType =
                unitOfWork.GetSet<SizeGroupToItemType>()
                .Where(g => g.ItemTypeId == itemTypeId)
                .Select(g => g.SizeGroupId)
                .ToList();

            return items.Where(i => !i.SizeGroupId.HasValue || groupIdListByItemType.Contains(i.SizeGroupId.Value)).ToList();
        }

        public bool IsSize(string postfix)
        {
            return GetAll().Any(m => m.StyleSize == postfix || m.ItemSize == postfix || m.StyleSize.Replace("-", "").Replace("/", "").Replace("\\", "") == postfix || m.StyleSize.Replace("-", "").Replace("/", "").Replace("\\", "") == postfix);
        }

        private IQueryable<SizeMappingDTO> AsDto(IQueryable<SizeMapping> query)
        {
            return query.Select(s => new SizeMappingDTO()
            {
                Id = s.Id,
                ItemSize = s.ItemSize,
                StyleSize = s.StyleSize,
                Priority = s.Priority,
                IsSystem = s.IsSystem,
                UpdateDate = s.UpdateDate,
                CreateDate = s.CreateDate
            });
        }
    }
}
