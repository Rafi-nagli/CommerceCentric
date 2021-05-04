using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Utils;
using Amazon.Web.General.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleReferencesViewModel : IImagesContainer
    {
        public long Id { get; set; }
        public string StyleId { get; set; }
        public int Type { get { return (int)StyleTypes.References; } }
        public string Name { get; set; }

        public int AutoPriceIndex { get; set; }

        public IList<StyleReferenceDTO> LinkedStyles { get; set; }
        public IList<StyleItemDTO> StyleItems { get; set; }

        public List<ImageViewModel> Images
        {
            get { return ImageSet != null ? ImageSet.Images : null; }
            set
            {
                if (ImageSet != null)
                    ImageSet.Images = value;
            }
        }

        public ImageCollectionViewModel ImageSet { get; set; }


        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }


        public override string ToString()
        {
            return "Id=" + Id
                   + ", StyleId=" + StyleId
                   + ", Name=" + Name
                   + ", UpdateDate=" + UpdateDate
                   + ", CreateDate=" + CreateDate;
        }

        public StyleReferencesViewModel()
        {
            LinkedStyles = new List<StyleReferenceDTO>();
            StyleItems = new List<StyleItemDTO>();
            ImageSet = new ImageCollectionViewModel();
        }


        public StyleReferencesViewModel(IUnitOfWork db, long? styleId, DateTime when)
        {
            if (styleId.HasValue)
            {
                var style = db.Styles.Get(styleId.Value);
                var styleImages = db.StyleImages.GetAllAsDto()
                    .Where(im => im.StyleId == style.Id && !im.IsSystem)
                    .OrderBy(im => im.Id)
                    .ToList();

                Id = style.Id;
                StyleId = style.StyleID;
                Name = style.Name;

                LinkedStyles = db.StyleReferences.GetByStyleId(styleId.Value);
                StyleItems = db.StyleItems.GetByStyleIdAsDto(styleId.Value);

                AutoPriceIndex = Array.FindIndex(LinkedStyles.ToArray(), l => !l.Price.HasValue);

                ImageSet = new ImageCollectionViewModel(1);
                ImageSet.SetImages(styleImages);
            }
            else
            {
                LinkedStyles = new List<StyleReferenceDTO>();
                StyleItems = new List<StyleItemDTO>();
                ImageSet = new ImageCollectionViewModel(1);
            }
        }

        public void SetUploadedImages(IList<SessionHelper.UploadedFileInfo> images)
        {
            if (images == null)
                return;

            //Set absolute pathes
            foreach (var img in images)
            {
                if (!(img.FileName ?? "").StartsWith("http"))
                {
                    img.FileName = UrlManager.UrlService.GetAbsolutePath(UrlManager.UrlService.GetUploadImageUrl(img.FileName));
                }
            }
                
            ImageSet.SetUploadedImageUrls(images);
        }

        public long Apply(IUnitOfWork db,
            ICacheService cache,
            DateTime when,
            long? by)
        {
            var style = db.Styles.Get(Id);

            if (style == null)
                Id = AddStyle(db, when, by);
            else
                UpdateStyle(db, style, when, by);

            return Id;
        }

        public List<ValidationResult> Validate(IUnitOfWork db)
        {
            var result = new List<ValidationResult>();

            if (String.IsNullOrEmpty(StyleId))
            {
                result.Add(new ValidationResult("Style Id is empty"));
            }
            else
            {
                var existStyles = db.Styles.GetAllAsDto().Where(s => s.StyleID == StyleId).ToList();
                if (existStyles.Count > 1 || (existStyles.Count == 1 && existStyles[0].Id != Id))
                    result.Add(new ValidationResult("Specified StyleId already exist"));
            }

            var notEmptyLinkedStyles = LinkedStyles.Where(s => !String.IsNullOrEmpty(s.LinkedStyleString)).ToList();

            if (!notEmptyLinkedStyles.Any())
            {
                result.Add(new ValidationResult("At least the one linked style should be specified"));
            }
            else
            {
                var linkedStyleStringList = notEmptyLinkedStyles.Select(s => s.LinkedStyleString).ToList();
                var dbStyles = db.Styles.GetAllAsDto()
                    .Where(s => linkedStyleStringList.Contains(s.StyleID)
                        && !s.Deleted)
                    .ToList();

                foreach (var linkedStyle in notEmptyLinkedStyles)
                {
                    var existDbStyle = dbStyles.FirstOrDefault(s => s.StyleID == linkedStyle.LinkedStyleString);
                    if (existDbStyle == null)
                    {
                        result.Add(new ValidationResult(String.Format("Specified StyleId: \"{0}\" is not found in the system", linkedStyle.LinkedStyleString)));
                    }
                    else
                    {
                        linkedStyle.LinkedStyleId = existDbStyle.Id;
                    }
                }
            }

            return result;
        }

        private long AddStyle(IUnitOfWork db, DateTime when, long? by)
        {
            var notEmptyLinkedStyles = LinkedStyles
                .Where(s => !String.IsNullOrEmpty(s.LinkedStyleString))
                .ToList();

            StyleViewModel.SetDefaultImage(ImageSet.Images);

            var dbStyle = new Style
            {
                StyleID = StyleId,
                Type = (int)StyleTypes.References,
                Name = Name,
                DropShipperId = DSHelper.DefaultPAId,
                Image = ImageSet.GetMainImageUrl(),
                //AdditionalImages = ImageSet.GetAdditionalImagesUrl(),

                ItemTypeId = (int)ItemType.Pajama,

                CreateDate = when,
                CreatedBy = by
            };
            
            db.Styles.Add(dbStyle);
            db.Commit();

            StyleViewModel.UpdateImages(db, dbStyle.Id, ImageSet.Images, when, by);

            db.StyleReferences.UpdateStyleReferencesForStyle(dbStyle.Id,
                notEmptyLinkedStyles,
                when,
                by);

            var styleItemToLinkedItems = RebuildStyleItems(db, dbStyle, notEmptyLinkedStyles);

            db.StyleItems.UpdateStyleItemsForStyle(dbStyle.Id,
                styleItemToLinkedItems.Select(si => si.StyleItem).ToList(),
                when,
                by);

            var styleItemReferences = BuildStyleItemReferences(db, dbStyle.Id, styleItemToLinkedItems);
            db.StyleItemReferences.UpdateStyleItemReferencesForStyle(dbStyle.Id,
                styleItemReferences,
                when,
                by);

            UpdateStyleFeatures(db, dbStyle.Id, LinkedStyles.Select(s => s.LinkedStyleId).ToList(), when, by);

            return dbStyle.Id;
        }

        private void UpdateStyle(IUnitOfWork db, Style style, DateTime when, long? by)
        {
            var notEmptyLinkedStyles = LinkedStyles
                .Where(s => !String.IsNullOrEmpty(s.LinkedStyleString))
                .ToList();

            StyleViewModel.SetDefaultImage(ImageSet.Images);

            style.StyleID = StyleId;
            style.Name = Name;

            style.Image = ImageSet.GetMainImageUrl();
            //style.AdditionalImages = ImageSet.GetAdditionalImagesUrl();

            style.UpdateDate = when;
            style.UpdatedBy = by;

            db.Commit();
            
            StyleViewModel.UpdateImages(db, style.Id, ImageSet.Images, when, by);

            db.StyleReferences.UpdateStyleReferencesForStyle(style.Id,
                notEmptyLinkedStyles,
                when,
                by);

            var styleItemToLinkedItems = RebuildStyleItems(db, style, notEmptyLinkedStyles);

            db.StyleItems.UpdateStyleItemsForStyle(style.Id,
                styleItemToLinkedItems.Select(si => si.StyleItem).ToList(),
                when,
                by);

            var styleItemReferences = BuildStyleItemReferences(db, style.Id, styleItemToLinkedItems);
            db.StyleItemReferences.UpdateStyleItemReferencesForStyle(style.Id,
                styleItemReferences,
                when,
                by);
        }

        private class StyleItemToLinkedItem
        {
            public StyleItemDTO StyleItem;
            public IList<StyleItemDTO> LinkedStyleItems;
        }

        private void UpdateStyleFeatures(IUnitOfWork db, long styleId, IList<long> linkedStyleIdList, DateTime when, long? by)
        {
            var allStyleFeatures = db.StyleFeatureValues.GetAllWithFeature().Where(f => linkedStyleIdList.Contains(f.StyleId)).ToList();
            allStyleFeatures.AddRange(db.StyleFeatureTextValues.GetAllWithFeature().Where(f => linkedStyleIdList.Contains(f.StyleId)).ToList());

            var resultFeatures = new List<StyleFeatureValueDTO>();

            var valueFeatureIds = allStyleFeatures.Select(f => f.FeatureId).Distinct();
            foreach (var featureId in valueFeatureIds)
            {
                var stylesFeaturesForFeature = allStyleFeatures.Where(sf => sf.FeatureId == featureId).ToList();
                if (linkedStyleIdList.All(st => stylesFeaturesForFeature.Any(f => f.StyleId == st))
                    && stylesFeaturesForFeature.Select(f => f.Value).Distinct().Count() == 1)
                    resultFeatures.Add(stylesFeaturesForFeature.FirstOrDefault());
            }
            
            resultFeatures.ForEach(f => f.StyleId = styleId);

            db.StyleFeatureValues.UpdateFeatureValues(styleId, resultFeatures, when, by);
        }

        private IList<StyleItemReferenceDTO> BuildStyleItemReferences(IUnitOfWork db, long styleId, IList<StyleItemToLinkedItem> linkedItems)
        {
            var results = new List<StyleItemReferenceDTO>();
            var dbExistLinkedItems = db.StyleItemReferences.GetAllAsDto().Where(si => si.StyleId == styleId).ToList();
            
            foreach (var linkedItem in linkedItems)
            {
                foreach (var linkedStyleItem in linkedItem.LinkedStyleItems)
                {
                    var dbExistItem = dbExistLinkedItems.FirstOrDefault(si => si.StyleItemId == linkedItem.StyleItem.StyleItemId
                                                                && si.LinkedStyleItemId == linkedStyleItem.StyleItemId);

                    //NOTE: for case when one mapping should be added twice
                    if (dbExistItem != null)
                        dbExistLinkedItems.Remove(dbExistItem);

                    results.Add(new StyleItemReferenceDTO()
                    {
                        Id = dbExistItem != null ? dbExistItem.Id : 0,
                        StyleId = styleId,
                        StyleItemId = linkedItem.StyleItem.StyleItemId,
                        LinkedStyleItemId = linkedStyleItem.StyleItemId,
                    });
                }
            }

            return results;
        }

        private IList<StyleItemToLinkedItem> RebuildStyleItems(IUnitOfWork db, Style style, IList<StyleReferenceDTO> linkedStyles)
        {
            var linkedStyleIds = linkedStyles.Select(l => l.LinkedStyleId).ToList();
            var allStyleItems = db.StyleItems.GetAllAsDto()
                .Where(s => linkedStyleIds.Contains(s.StyleId))
                .ToList();
            var dbExistStyleItems = db.StyleItems.GetAllAsDto().Where(s => s.StyleId == style.Id).ToList();

            var firstLinkedStyleId = linkedStyles.First().LinkedStyleId;
            var firstLinkedStyleItems = allStyleItems.Where(si => si.StyleId == firstLinkedStyleId).ToList();

            var sizeMappings = db.SizeMappings.GetAllAsDto().ToList();

            var results = new List<StyleItemToLinkedItem>();
            //var sizeMappings = 
            foreach (var styleItem in firstLinkedStyleItems)
            {
                var sameSizeStyleItems = new List<StyleItemDTO>();
                foreach (var styleId in linkedStyleIds)
                {
                    var styleItemForSize = allStyleItems.FirstOrDefault(s => s.StyleId == styleId
                        && (s.Size == styleItem.Size
                            || s.Size == "[empty]"
                            || s.Size == "OneSize")); //NOTE: add empty size as link to all

                    if (styleItemForSize == null)
                    {
                        var mappingsForSize = sizeMappings.Where(s => s.StyleSize == styleItem.Size).Select(s => s.ItemSize);
                        styleItemForSize = allStyleItems.FirstOrDefault(s => s.StyleId == styleId
                            && mappingsForSize.Contains(s.Size));
                    }

                    if (styleItemForSize != null)
                        sameSizeStyleItems.Add(styleItemForSize);
                }
                
                //NOTE: lets add size only if all linked styles have that size
                if (linkedStyleIds.All(st => sameSizeStyleItems.Any(si => si.StyleId == st)))
                {
                    var existStyleItem = dbExistStyleItems.FirstOrDefault(si => si.Size == styleItem.Size);
                    if (existStyleItem == null)
                        existStyleItem = new StyleItemDTO();

                    existStyleItem.StyleId = style.Id;
                    existStyleItem.Size = styleItem.Size;
                    existStyleItem.SizeId = styleItem.SizeId;
                    //NOTE: Only if all items has weight
                    existStyleItem.Weight = sameSizeStyleItems.Any(s => !s.Weight.HasValue)
                        ? null
                        : sameSizeStyleItems.Sum(si => si.Weight);

                    var item = new StyleItemToLinkedItem()
                    {
                        StyleItem = existStyleItem,
                        LinkedStyleItems = linkedStyleIds.Select(s => new StyleItemDTO()
                        {
                            StyleId = s,
                            StyleItemId = sameSizeStyleItems.FirstOrDefault(si => si.StyleId == s)?.StyleItemId ?? 0
                        }).ToList()
                    };

                    results.Add(item);
                }
            }

            return results;
        }
    }
}