using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Models;

namespace Amazon.Web.General.Models
{
    public interface IUrlService
    {
        string GetThumbnailUrl(string imageUrl,
            int width,
            int height,
            bool autoRotate,
            string defaultThumbnail = "",
            bool skipExternalUrlToThumbnail = false,
            bool convertToFullUrl = false,
            bool convertInDomainUrlToThumbnail = false);

        string GetAbsolutePath(string relativePath);

        string GetProductUrl(string asin, MarketType market, string marketplaceId);

        string GetMarketUrl(string asin, string sourceMarketId, MarketType market, string marketplaceId);

        string GetProductByStyleUrl(string styleString, MarketType market, string marketplaceId);

        string GetProductTemplateFilePath(string relativePath);

        string GetProductTemplateUrl(string filename);

        string GetUploadImageUrl(string imageFileName);
    }
}
