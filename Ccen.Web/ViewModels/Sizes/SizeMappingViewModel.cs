using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.DAL;
using Amazon.Web.ViewModels.Inventory;
using DocumentFormat.OpenXml.Spreadsheet;
using SizeGroup = Amazon.Core.Entities.Sizes.SizeGroup;

namespace Amazon.Web.ViewModels
{
    public class SizeMappingViewModel
    {
        public int? Id { get; set; }
        //[Required]
        public string StyleSize { get; set; }
        [Required]
        public string ItemSize { get; set; }

        public int Priority { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public override string ToString()
        {
            return "Id=" + Id + ", StyleSize=" + StyleSize + ", ItemSize=" + ItemSize + ", Priority=" + Priority;
        }

        public static IEnumerable<SizeMappingViewModel> GetAll(IUnitOfWork db)
        {
            var sizeMappings = db.SizeMappings.GetAllAsDto()
                .Where(s => s.IsSystem == false)
                .Select(s =>
                new SizeMappingViewModel
                {
                    Id = s.Id,
                    StyleSize = s.StyleSize,
                    ItemSize = s.ItemSize,
                    Priority = s.Priority,
                    CreateDate = s.CreateDate,
                    UpdateDate = s.UpdateDate
                }).ToList();

            return sizeMappings;
        }

        public static ValidationResult[] Validate(IUnitOfWork db, SizeMappingViewModel item)
        {
            var exists = db.SizeMappings.GetExists(item.StyleSize, item.ItemSize);
            if (exists.Any(s => s.Id != item.Id)) //Exclude current
                return new[] { new ValidationResult("This combination already exist"), };
            return new ValidationResult[] {};
        }

        public static SizeMappingViewModel Add(IUnitOfWork db, SizeMappingViewModel item, DateTime when, long? by)
        {
            var sizeMapping = new SizeMapping
            {
                StyleSize = item.StyleSize,
                ItemSize = item.ItemSize,
                Priority = item.Priority,
                CreateDate = when,
                CreatedBy = by
            };
            db.SizeMappings.Add(sizeMapping);
            db.Commit();


            item.Id = sizeMapping.Id;
            return item;
        }

        public static SizeMappingViewModel Update(IUnitOfWork db, SizeMappingViewModel item, DateTime when, long? by)
        {
            if (item.Id.HasValue)
            {
                var sizeMapping = db.SizeMappings.Get(item.Id.Value);
                if (sizeMapping != null)
                {
                    sizeMapping.StyleSize = item.StyleSize;
                    sizeMapping.ItemSize = item.ItemSize;
                    sizeMapping.Priority = item.Priority;

                    sizeMapping.UpdateDate = when;
                    sizeMapping.UpdatedBy = by;
                    db.Commit();

                    return item;
                }
            }
            return null;
        }


        public static void Delete(IUnitOfWork db, int id)
        {
            var sizeMapping = db.SizeMappings.Get(id);
            if (sizeMapping != null)
            {
                db.SizeMappings.Remove(sizeMapping);
                db.Commit();
            }
        }
    }
}