using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Results;
using Style = Amazon.Core.Entities.Inventory.Style;


namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleDescriptionViewModel
    {
        public long Id { get; set; }
        public string StyleString { get; set; }

        public string Name { get; set; }
        public string Image { get; set; }

        public string Description { get; set; }
        public string LongDescription { get; set; }

        public string SearchTerms { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }

        public int RemainingQuantity { get; set; }

        public string ItemStyle { get; set; }
        public string MainLicense { get; set; }
        public string SubLicense { get; set; }


        public DateTime? CountingDate { get; set; }

        public DateTime? ReSaveDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Image,
                    75,
                    75,
                    false,
                    ImageHelper.NO_IMAGE_URL,
                    false,
                    false,
                    convertInDomainUrlToThumbnail: true);
            }
        }

        public bool HasImage { get { return !string.IsNullOrEmpty(Image); } }

        public string ShortDescription
        {
            get { return StringHelper.Substring(Description, 0, 150, "..."); }
        }

        public override string ToString()
        {
            return "Id=" + Id
                   + ", StyleId=" + StyleString
                   + ", Name=" + Name
                   + ", Description=" + Description
                   + ", LongDescription=" + LongDescription
                   + ", SearchTerms=" + SearchTerms
                   + ", BulletPoint1=" + BulletPoint1
                   + ", BulletPoint2=" + BulletPoint2
                   + ", BulletPoint3=" + BulletPoint3
                   + ", BulletPoint4=" + BulletPoint4
                   + ", BulletPoint5=" + BulletPoint5;
        }

        public StyleDescriptionViewModel()
        {

        }

        public StyleDescriptionViewModel(IUnitOfWork db, long styleId)
        {
            var style = db.Styles.GetByStyleIdAsDto(styleId);

            Id = style.Id;

            StyleString = style.StyleID;
            Name = style.Name;
            Image = GetImage(style.Image);

            Description = style.Description;
            LongDescription = style.LongDescription;
            SearchTerms = style.SearchTerms;
            BulletPoint1 = style.BulletPoint1;
            BulletPoint2 = style.BulletPoint2;
            BulletPoint3 = style.BulletPoint3;
            BulletPoint4 = style.BulletPoint4;
            BulletPoint5 = style.BulletPoint5;
        }

        public StyleDescriptionViewModel(IUnitOfWork db, Style style)
        {
            Id = style.Id;

            StyleString = style.StyleID;
            Name = style.Name;
            Image = GetImage(style.Image);
        }

        private static string GetImage(string path)
        {
            return !string.IsNullOrEmpty(path)
                ? path.Contains("http")
                    ? path
                    : UrlHelper.GetAbsolutePath(path)
                : string.Empty;
        }

        public long Apply(IUnitOfWork db,
            DateTime when,
            long? by)
        {
            var style = db.Styles.Get(Id);

            style.Description = Description;
            style.LongDescription = LongDescription;
            style.SearchTerms = SearchTerms;
            style.BulletPoint1 = BulletPoint1;
            style.BulletPoint2 = BulletPoint2;
            style.BulletPoint3 = BulletPoint3;
            style.BulletPoint4 = BulletPoint4;
            style.BulletPoint5 = BulletPoint5;

            style.DescriptionUpdateDate = when;
            style.DescriptionUpdatedBy = by;

            style.UpdateDate = when;
            style.UpdatedBy = by;

            style.ReSaveDate = when;
            style.ReSaveBy = by;

            db.Commit();

            return Id;
        }


        public static IList<StyleDescriptionViewModel> GetAll(IUnitOfWork db,
            StyleSearchFilterViewModel filter)
        {

            IQueryable<StyleEntireDto> styleQuery = from st in db.Styles.GetAllActiveAsDtoEx()
                                               orderby st.Id descending
                                               select st;

            if (filter.StyleId.HasValue)
            {
                styleQuery = styleQuery.Where(s => s.Id == filter.StyleId.Value);
            }

            if (filter.FromReSaveDate.HasValue)
            {
                styleQuery = styleQuery.Where(s => s.ReSaveDate >= filter.FromReSaveDate.Value);
            }

            var styleList = styleQuery.Select(s => new StyleDescriptionViewModel
            {
                Id = s.Id,
                StyleString = s.StyleID,

                Image = s.Image,

                Name = s.Name,

                Description = s.Description,
                SearchTerms = s.SearchTerms,
                BulletPoint1 = s.BulletPoint1,
                BulletPoint2 = s.BulletPoint2,
                BulletPoint3 = s.BulletPoint3,
                BulletPoint4 = s.BulletPoint4,
                BulletPoint5 = s.BulletPoint5,

                CreateDate = s.CreateDate,
                UpdateDate = s.UpdateDate,
                ReSaveDate = s.ReSaveDate,
            }).ToList();

            //NOTE: fast exist for case with ReSaveDate, in most cases count will be = 0
            if (styleList.Count == 0)
            {
                return new List<StyleDescriptionViewModel>();
            }

            var allFeatures = db.StyleFeatureValues.GetFeatureValueForAllStyleByFeatureId(new int[]
            {
                StyleFeatureHelper.ITEMSTYLE,
                StyleFeatureHelper.MAIN_LICENSE,
                StyleFeatureHelper.SUB_LICENSE1
            });

            var styleItemList = (from si in db.StyleItems.GetAll()
                                 join st in styleQuery on si.StyleId equals st.Id into withStyle
                                 from st in withStyle.DefaultIfEmpty()
                                 join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id into withCache
                                 from sic in withCache.DefaultIfEmpty()
                                 select new StyleItemShowViewModel()
                                 {
                                     StyleId = si.StyleId,
                                     
                                     RemainingQuantity = sic.RemainingQuantity
                                 }).ToList();


            styleList.ForEach(i =>
            {
                i.Image = GetImage(i.Image);
                i.RemainingQuantity = styleItemList.Where(si => si.StyleId == i.Id).Sum(si => si.RemainingQuantity ?? 0);
                i.MainLicense = allFeatures.FirstOrDefault(f => f.StyleId == i.Id && f.FeatureId == StyleFeatureHelper.MAIN_LICENSE)?.Value;
                i.SubLicense = allFeatures.FirstOrDefault(f => f.StyleId == i.Id && f.FeatureId == StyleFeatureHelper.SUB_LICENSE1)?.Value;
                i.ItemStyle = allFeatures.FirstOrDefault(f => f.StyleId == i.Id && f.FeatureId == StyleFeatureHelper.ITEMSTYLE)?.Value;
            });
            return styleList;
        }
    }
}