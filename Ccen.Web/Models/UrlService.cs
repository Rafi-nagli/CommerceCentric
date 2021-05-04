using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models;
using Amazon.Web.General.Models;

namespace Amazon.Web.Models
{
    public class UrlService : IUrlService
    {
        public string GetThumbnailUrl(string imageUrl,
            int width,
            int height,
            bool autoRotate,
            string defaultThumbnail = "",
            bool skipExternalUrlToThumbnail = false,
            bool convertToFullUrl = false,
            bool convertInDomainUrlToThumbnail = false)
        {
            return UrlHelper.GetThumbnailUrl(imageUrl,
                width,
                height,
                autoRotate,
                defaultThumbnail,
                skipExternalUrlToThumbnail,
                convertToFullUrl,
                convertInDomainUrlToThumbnail);
        }

        public string GetAbsolutePath(string relativePath)
        {
            return UrlHelper.GetAbsolutePath(relativePath);
        }

        public string GetProductUrl(string asin, MarketType market, string marketplaceId)
        {
            return UrlHelper.GetProductUrl(asin, market, marketplaceId);
        }

        public string GetMarketUrl(string asin, string sourceMarketId, MarketType market, string marketplaceId)
        {
            return UrlHelper.GetMarketUrl(asin, sourceMarketId, market, marketplaceId);
        }

        public string GetProductByStyleUrl(string styleString, MarketType market, string marketplaceId)
        {
            return UrlHelper.GetProductByStyleUrl(styleString, market, marketplaceId);
        }

        public string GetProductTemplateFilePath(string relativePath)
        {
            return UrlHelper.GetProductTemplateFilePath(relativePath);
        }

        public string GetProductTemplateUrl(string filename)
        {
            return UrlHelper.GetProductTemplateUrl(filename);
        }

        public string GetUploadImageUrl(string imageFileName)
        {
            return UrlHelper.GetUploadImageUrl(imageFileName);
        }
    }
}