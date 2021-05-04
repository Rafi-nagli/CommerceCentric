using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Sizes;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class SizeRepository : Repository<Size>, ISizeRepository
    {
        public SizeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IList<SizeDTO> GetAllAsDto()
        {
            return unitOfWork.GetSet<Size>().Select(s => new SizeDTO
            {
                Id = s.Id,
                SizeGroupId = s.SizeGroupId,
                Name = s.Name,
                SortOrder = s.SortOrder,
                DefaultIsChecked = s.DefaultIsChecked,
            }).ToList();
        }

        public IQueryable<SizeDTO> GetAllWithGroupAsDto()
        {
            var query = from s in unitOfWork.GetSet<Size>()
                        join g in unitOfWork.GetSet<SizeGroup>() on s.SizeGroupId equals g.Id
                        select new SizeDTO
                        {
                            Id = s.Id,
                            SizeGroupId = s.SizeGroupId,
                            Name = s.Name,
                            SortOrder = s.SortOrder,
                            DefaultIsChecked = s.DefaultIsChecked,

                            SizeGroupName = g.Name,
                        };

            return query;
        }

        public IList<SizeDTO> GetAllWithGroupByItemTypeAsDto(int? itemTypeId)
        {
            var query = from s in unitOfWork.GetSet<Size>()
                        join g in unitOfWork.GetSet<SizeGroup>() on s.SizeGroupId equals g.Id
                        select new SizeDTO
                        {
                            Id = s.Id,
                            SizeGroupId = s.SizeGroupId,
                            Name = s.Name,
                            SortOrder = s.SortOrder,
                            DefaultIsChecked = s.DefaultIsChecked,

                            SizeGroupName = g.Name,
                            Departments = String.IsNullOrEmpty(s.Departments) ? g.Departments : s.Departments,
                        };

            var items = query.ToList();

            if (itemTypeId.HasValue)
            {
                var groupIdsByItemType = unitOfWork.GetSet<SizeGroupToItemType>()
                    .Where(g => g.ItemTypeId == itemTypeId.Value)
                    .Select(g => g.SizeGroupId)
                    .ToList();

                items = items.Where(i => i.SizeGroupId.HasValue && groupIdsByItemType.Contains(i.SizeGroupId.Value)).ToList();
            }

            return items;
        }

        public IList<SizeDTO> GetSizesForGroup(int groupId)
        {
            return GetFiltered(s => s.SizeGroupId == groupId).Select(s => new SizeDTO
            {
                Id = s.Id,
                SizeGroupId = s.SizeGroupId,
                Name = s.Name,
                SortOrder = s.SortOrder,
                DefaultIsChecked = s.DefaultIsChecked,
                Departments = s.Departments
            }).ToList();
        }

        public IList<SizeGroupToItemType> GetItemTypeBySizeGroups(IList<int> groupIdList)
        {
            return unitOfWork.GetSet<SizeGroupToItemType>().Where(i => groupIdList.Contains(i.SizeGroupId)).ToList();
        }

        public bool IsSize(string postfix)
        {
            return GetAll().Any(m => m.Name == postfix);
        }
    }
}
