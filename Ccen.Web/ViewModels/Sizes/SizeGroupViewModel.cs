using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Sizes;
using Amazon.DAL;
using Amazon.Web.ViewModels.Inventory;
using DocumentFormat.OpenXml.Spreadsheet;
using SizeGroup = Amazon.Core.Entities.Sizes.SizeGroup;

namespace Amazon.Web.ViewModels
{
    public class SizeGroupViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }

        public List<CheckedItemViewModel> ItemTypes { get; set; }


        public string Departments { get; set; }

        public string TypeName { get; set; }
        public int SortOrder { get; set; }

        public override string ToString()
        {
            return "Id=" + Id 
                + ", SortOrder=" + SortOrder 
                + ", Name=" + Name 
                + ", Departments=" + Departments;
        }

        public static IEnumerable<SizeGroupViewModel> GetAll(IUnitOfWork db)
        {
            var groups = db.SizeGroups.GetAllAsDto().Select(g =>
                new SizeGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Departments = g.Departments,
                    SortOrder = g.SortOrder
                })
                .OrderBy(m => m.SortOrder)
                .ToList();


            #region Fill item types

            var itemTypes = db.ItemTypes.GetAllAsDto().ToList();
            var groupToItemTypes = db.SizeGroupToItemTypes.GetAll().ToList();

            foreach (var group in groups)
            {
                group.ItemTypes = itemTypes.Select(t => new CheckedItemViewModel()
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsChecked = groupToItemTypes.Any(g => g.SizeGroupId == group.Id && g.ItemTypeId == t.Id)
                }).ToList();
            }

            #endregion

            return groups;
        }

        public static SizeGroupViewModel Add(IUnitOfWork db, 
            SizeGroupViewModel item, 
            DateTime when, 
            long? by)
        {
            var group = new SizeGroup
            {
                Name = item.Name,
                Departments = item.Departments,
                SortOrder = item.SortOrder,
                CreateDate = when,
                CreatedBy = by
            };
            db.SizeGroups.Add(group);
            db.Commit();

            var itemTypes = item.ItemTypes != null ? item.ItemTypes.Where(i => i.IsChecked).ToList() : new List<CheckedItemViewModel>();
            MergeItemTypes(db, group.Id, itemTypes, when, by);

            item.Id = group.Id;
            return item;
        }
        
        public static SizeGroupViewModel Update(IUnitOfWork db, 
            SizeGroupViewModel item, 
            DateTime when, 
            long? by)
        {
            if (item.Id.HasValue)
            {
                var group = db.SizeGroups.Get(item.Id.Value);
                if (group != null)
                {
                    group.Name = item.Name;
                    group.Departments = item.Departments;
                    group.SortOrder = item.SortOrder;
                    group.UpdateDate = when;
                    group.UpdatedBy = by;
                    db.Commit();

                    var itemTypes = item.ItemTypes != null ? item.ItemTypes.Where(i => i.IsChecked).ToList() : new List<CheckedItemViewModel>();
                    MergeItemTypes(db, group.Id, itemTypes, when, by);

                    return item;
                }
            }
            return null;
        }

        public static void Delete(IUnitOfWork db, int id)
        {
            var group = db.SizeGroups.Get(id);
            if (group != null)
            {
                db.SizeGroups.Remove(group);
                db.Commit();
            }
        }


        private static void MergeItemTypes(IUnitOfWork db, 
            int groupId, 
            IList<CheckedItemViewModel> items, 
            DateTime when, 
            long? by)
        {
            var existItemTypes = db.SizeGroupToItemTypes.GetBySizeGroupId(groupId);
            var toRemove = existItemTypes.Where(exist => items.All(i => i.Id != exist.ItemTypeId)).ToList();
            var toAdd = items.Where(i => existItemTypes.All(exist => exist.ItemTypeId != i.Id)).ToList();
            if (toRemove.Any() || toAdd.Any())
            {
                foreach (var add in toAdd)
                {
                    db.SizeGroupToItemTypes.Add(new SizeGroupToItemType()
                    {
                        CreateDate = when,
                        CreatedBy = by,
                        ItemTypeId = add.Id,
                        SizeGroupId = groupId
                    });
                }
                foreach (var rem in toRemove)
                {
                    db.SizeGroupToItemTypes.Remove(rem);
                }
                db.Commit();
            }
        }
    }
}