using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Web.General.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Inventory
{
    public class ImageViewModel
    {
        public long Id { get; set; }

        public string UploadedImageUrl { get; set; }
        public string DirectImageUrl { get; set; }

        public string Label { get; set; }
        public int Category { get; set; }
        public bool IsDefault { get; set; }

        public string UploadedFileName
        {
            get
            {
                if (!String.IsNullOrEmpty(UploadedImageUrl))
                    return Path.GetFileName(UploadedImageUrl);
                return "";
            }
        }

        public string DirectThumbnailUrl
        {
            get { return UrlManager.UrlService.GetThumbnailUrl(DirectImageUrl, 0, 75, false, convertInDomainUrlToThumbnail:true); }
        }

        public string ImageUrl
        {
            get { return String.IsNullOrEmpty(UploadedImageUrl) ? DirectImageUrl : UploadedImageUrl; }
        }

        public ImageViewModel()
        {
            
        }
    }
}