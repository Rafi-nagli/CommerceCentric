using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleLocationViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public string Image { get; set; }
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Image, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }


        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public List<LocationViewModel> Locations { get; set; }


        public override string ToString()
        {
            return "Id=" + Id
                   + ", StyleId=" + StyleId
                   + ", UpdateDate=" + UpdateDate
                   + ", CreateDate=" + CreateDate;
        }

        public StyleLocationViewModel()
        {
            Locations = new List<LocationViewModel>();
        }


        public StyleLocationViewModel(IUnitOfWork db, long styleId, DateTime when)
        {
            var style = db.Styles.Get(styleId);
            
            StyleId = styleId;
            StyleString = style.StyleID;


            Image = style.Image;

            Locations = StyleViewModel.GetLocations(db, style.Id);
        }

        public long Apply(IUnitOfWork db, 
            IStyleHistoryService styleHistory,
            DateTime when, 
            long? by)
        {
            var style = db.Styles.Get(StyleId);

            style.UpdateDate = when;
            style.UpdatedBy = by;

            style.ReSaveDate = when;
            style.ReSaveBy = by;

            StyleViewModel.UpdateLocations(db, styleHistory, StyleId, Locations, when, by);
            
            return StyleId;
        }
    }
}