using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleRepository : Repository<Style>, IStyleRepository
    {
        public StyleRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
     
        public IQueryable<ViewStyle> GetAllActual()
        {
            return unitOfWork.GetSet<ViewStyle>();
        }

        public IQueryable<Style> GetAllActive()
        {
            return unitOfWork.GetSet<Style>().Where(s => !s.Deleted);
        }

        public IQueryable<StyleEntireDto> GetAllActiveAsDto()
        {
            return AsDto(GetAllActive());
        }

        public IQueryable<StyleEntireDto> GetAllActiveAsDtoEx()
        {
            return AsDtoEx(GetAllActive());
        }

        public IQueryable<StyleEntireDto> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<Style>());
        }

        public IQueryable<StyleEntireDto> GetAllAsDtoLite()
        {
            return AsDtoLite(unitOfWork.GetSet<Style>());
        }

        public IQueryable<StyleEntireDto> GetAllAsDtoEx()
        {
            return AsDtoEx(unitOfWork.GetSet<Style>());
        }

        public void SetReSaveDate(long styleId, DateTime? when, long? by)
        {
            var style = Get(styleId);
            style.ReSaveDate = when;
            style.ReSaveBy = by;
            unitOfWork.Commit();
        }


        public StyleEntireDto GetActiveByStyleIdAsDto(long styleId)
        {
            return AsDtoEx(unitOfWork.GetSet<Style>()).FirstOrDefault(s => !s.Deleted && s.Id == styleId);
        }

        public StyleEntireDto GetActiveByStyleIdAsDto(string styleString)
        {
            return AsDtoEx(unitOfWork.GetSet<Style>()).FirstOrDefault(s => !s.Deleted && s.StyleID == styleString);
        }

        public StyleEntireDto GetByStyleIdAsDto(long styleId)
        {
            return AsDtoEx(unitOfWork.GetSet<Style>()).FirstOrDefault(s => s.Id == styleId);
        }


        public IQueryable<ViewStyle> GetAllActualFiltered(string barcode)
        {
            var baseSearch = from item in unitOfWork.GetSet<StyleItem>() select item;
            if (!String.IsNullOrEmpty(barcode))
            {
                baseSearch = (from vi in baseSearch
                    join itemBarcode in unitOfWork.GetSet<StyleItemBarcode>() on vi.Id equals itemBarcode.StyleItemId
                    where barcode.Contains(itemBarcode.Barcode)
                    select vi);
                //baseSearch = baseSearch.Where(b => b.Barcode.Contains(barcode));
            }

            var searchQuery = from item in baseSearch
                              group item by item.StyleId
                                  into byStyle
                                  select byStyle.Key;

            var query = from s in unitOfWork.GetSet<ViewStyle>()
                        join b in searchQuery on s.Id equals b
                        select s;

            return query;
        }

        public bool IsNewStyle(long? id, string styleId)
        {
            var exists = unitOfWork.GetSet<Style>().Where(s => s.StyleID == styleId && !s.Deleted).ToList();
            if (exists.Count > 1)
                return false;
            if (exists.Count == 1)
                return exists[0].Id == id;
            return true;
        }

        public List<StyleDTO> GetAllWithoutImage()
        {
            var styles = (from s in unitOfWork.GetSet<ViewStylesWithoutImage>() select s).ToList();
                
            return styles.Select(s => new StyleDTO
            {
                Id = s.Id,
                Name = s.StyleID.Length > 7 ? string.Format("{0}-{1}-{2}", s.StyleID.Substring(0, 2), s.StyleID.Substring(2, 5), s.StyleID.Substring(7)) : s.StyleID
            }).ToList();
        }

        public Style Store(string styleId, 
            int itemTypeId,
            string name, 
            string imageSource,
            DateTime when)
        {
            var dbStyle = new Style
            {
                StyleID = styleId,
                Name = name,
                Image = imageSource,
                ItemTypeId = itemTypeId,
                DropShipperId = DSHelper.DefaultPAId,

                UpdateDate = when,
                CreateDate = when
            };
            Add(dbStyle);
            unitOfWork.Commit();

            if (!String.IsNullOrEmpty(imageSource))
            {
                var dbStyleImage = new StyleImage()
                {
                    StyleId = dbStyle.Id,
                    Image = imageSource,
                    Type = (int)StyleImageType.None,
                    CreateDate = when,
                };
                unitOfWork.StyleImages.Add(dbStyleImage);
                unitOfWork.Commit();
            }

            return dbStyle;
        }

        private IQueryable<StyleEntireDto> AsDto(IQueryable<Style> query)
        {
            return query.Select(s => new StyleEntireDto()
            {
                Id = s.Id,
                StyleID = s.StyleID,
                OnHold = s.OnHold,
                DropShipperId = s.DropShipperId,
                AutoDSSelection = s.AutoDSSelection,
                Type = s.Type,
                Name = s.Name,
                Manufacturer = s.Manufacturer,
                Description = s.Description,
                MSRP = s.MSRP,

                Image = s.Image,
                AdditionalImages = s.AdditionalImages,
                Deleted = s.Deleted,
                ItemTypeId = s.ItemTypeId,
                ReSaveDate = s.ReSaveDate,
            });
        }

        private IQueryable<StyleEntireDto> AsDtoLite(IQueryable<Style> query)
        {
            return query.Select(s => new StyleEntireDto()
            {
                Id = s.Id,
                StyleID = s.StyleID,
                OnHold = s.OnHold,
                Type = s.Type,
                MSRP = s.MSRP,
                DropShipperId = s.DropShipperId,
                AutoDSSelection = s.AutoDSSelection,

                Deleted = s.Deleted,
                ItemTypeId = s.ItemTypeId,
                ReSaveDate = s.ReSaveDate,
            });
        }

        private IQueryable<StyleEntireDto> AsDtoEx(IQueryable<Style> query)
        {
            return query.Select(s => new StyleEntireDto()
            {
                Id = s.Id,
                StyleID = s.StyleID,
                DSStyleId = s.DSStyleID,
                OriginalStyleID = s.OriginalStyleID,                
                OnHold = s.OnHold,
                DropShipperId = s.DropShipperId,
                PreorderCountry = s.PreorderCountry,
                AutoDSSelection = s.AutoDSSelection,
                DSEffectiveDate = s.DSEffectiveDate,
                Type = s.Type,
                Name = s.Name,
                Manufacturer = s.Manufacturer,
                Description = s.Description,
                MSRP = s.MSRP,

                LongDescription = s.LongDescription,
                Price = s.Price,
                SearchTerms = s.SearchTerms,
                Tags = s.Tags,
                BulletPoint1 = s.BulletPoint1,
                BulletPoint2 = s.BulletPoint2,
                BulletPoint3 = s.BulletPoint3,
                BulletPoint4 = s.BulletPoint4,
                BulletPoint5 = s.BulletPoint5,

                Image = s.Image,
                AdditionalImages = s.AdditionalImages,
                Deleted = s.Deleted,
                ItemTypeId = s.ItemTypeId,
                DisplayMode = s.DisplayMode,
                RemovePriceTag = s.RemovePriceTag,

                FillingStatus = s.FillingStatus,
                
                PictureStatus = s.PictureStatus,
                PictureStatusUpdateDate = s.PictureStatusUpdateDate,

                Comment = s.Comment,
                CommentUpdateDate = s.CommentUpdateDate,

                ReSaveDate = s.ReSaveDate,

                CreateDate = s.CreateDate,
                UpdateDate = s.UpdateDate,

                CreatedBy = s.CreatedBy,
            });
        }
    }
}
