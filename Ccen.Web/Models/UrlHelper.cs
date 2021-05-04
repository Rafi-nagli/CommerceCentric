using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.General.Models;
using Ccen.Web;

namespace Amazon.Web.Models
{
    public class UrlHelper
    {
        public static string DomainUrl = AppSettings.DomainUrl;
        public static string DomainHttpUrl
        {
            get { return DomainUrl.Replace("https://", "http://"); }
        }

        public static string GetLocalPath(string relativePath)
        {
            return HttpContext.Current.Server.MapPath(relativePath);
        }

        public static string GetLabelPath(string relativePath)
        {
            if (String.IsNullOrEmpty(relativePath))
                return String.Empty;
            return Path.Combine(AppSettings.LabelDirectory, relativePath.TrimStart("~/\\".ToCharArray()).Replace("\\", "/"));
        }

        public static string GetUploadSampleIncomingFeedPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "SampleIncomingFeed");
        }

        public static string GetUploadImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "UploadImages");
        }

        public static string GetUploadEmailAttachmentPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "UploadEmailAttachments");
        }

        public static string GetEmailAttachemntPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "EmailAttachments");
        }

        public static string GetSwatchImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "SwatchImages");
        }

        public static string GetWalmartImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "WalmartImages");
        }

        public static string GetWalmartFeedPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "WalmartFeeds");
        }

        public static string GetJetImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "RawImages");
        }

        public static string GetEBayImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "RawImages");
        }

        public static string GetGrouponImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "GrouponImages");
        }

        public static string GetRawImagePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "RawImages");
        }

        public static string GetUploadDhlInvoicePath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "DhlInvoices");
        }

        public static string GetUploadOrderFeedPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "UploadOrderFeed");
        }

        public static string GetUploadImageUrl(string imageFileName)
        {
            //UrlPathEncode doesn't encode anything after the first occurence of "?". It assumes the rest is the query string, and assumes the query string is already encoded
            //http://stackoverflow.com/questions/4145823/httpserverutility-urlpathencode-vs-httpserverutility-urlencode
            return "~/Image/Get/" + UrlHelper.UrlEncodeFilename(imageFileName);
        }

        public static string GetPathFromRelativeImageUrl(string relativePath)
        {
            if (relativePath.StartsWith("~/InventoryPics/"))
                return HttpContext.Current.Server.MapPath(relativePath);

            if (relativePath.StartsWith("~/Image/UploadImage/")
                || relativePath.StartsWith("~/Image/Get/")) //NOTE: Alias
                return GetUploadImagePath() + (relativePath ?? "").Replace("~/Image/UploadImage/", "/").Replace("~/Image/Get/", "/");

            return "";
        }

        public static string GetProductTemplateUrl(string filename)
        {
            return "/Inventory/GetGeneratedFile?fileName=" + HttpUtility.UrlEncode(filename);
        }

        public static string GetBargainExportUrl(string filename)
        {
            return "/Bargain/GetBargainExportFile?fileName=" + HttpUtility.UrlEncode(filename);
        }

        public static string GetImportCatalogFeedPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "ImportCatalogFeed");
        }

        public static string GetActualizeGrouponQtyFeedPath()
        {
            return Path.Combine(AppSettings.LabelDirectory, "ActualizeGrouponQtyFeed");
        }       

        public static string GetActualizeGrouponQtyUrl(string feedFileName)
        {
            return "~/Groupon/GetFeed/" + UrlHelper.UrlEncodeFilename(feedFileName);
        }

        public static string GetProductUrl(string asin, MarketType market, string marketplaceId)
        {
            return "/Item/Products?ParentASIN=" + HttpUtility.UrlEncode(asin) + "&market=" + (int)market + "&marketplaceId=" + marketplaceId;
        }

        public static string GetProductByStyleUrl(string styleString, MarketType market, string marketplaceId)
        {
            return "/Item/Products?StyleId=" + styleString + "&market=" + (int)market + "&marketplaceId=" + marketplaceId;
        }

        public static string GetFeedUrl(long feedId)
        {
            return "/Feed/GetFeed/" + feedId;
        }

        public static string GetSellarCentralInventoryUrl(string asin, MarketType market, string marketplaceId)
        {
            return MarketUrlHelper.GetSellarCentralInventoryUrl(asin, market, marketplaceId);
        }

        public static string GetSellerCentralOrderUrl(MarketType market, string marketplaceId, string orderId)
        {
            return MarketUrlHelper.GetSellarCentralOrderUrl(market, marketplaceId, orderId, null);
        }

        public static string GetSellarCentralReturnUrl(MarketType market, string orderId)
        {
            if (market == MarketType.Amazon)
                return "https://sellercentral.amazon.com/gp/returns/list?searchType=orderId&keywordSearch=" + orderId;
            if (market == MarketType.AmazonEU)
                return "https://sellercentral.amazon.co.uk/gp/returns/list?searchType=orderId&keywordSearch=" + orderId;
            if (market == MarketType.AmazonAU)
                return "https://sellercentral.amazon.com.au/gp/returns/list?searchType=orderId&keywordSearch=" + orderId;
            return "";
        }

        public static string GetUltraLargeImageUrl(string asin, string marketplaceId)
        {
            var marketCode = "";
            //http://stackoverflow.com/questions/9486900/how-to-get-a-full-res-image-from-an-amazon-zoom-window
            //https://en.wikipedia.org/wiki/Category_talk:Book_covers
            //1 The country or national edition; 1 or 01 = (US), 2 (UK), 3 (Germany), 8 (France), 9 (Japan).
            switch (marketplaceId)
            {
                case MarketplaceKeeper.AmazonComMarketplaceId:
                case MarketplaceKeeper.AmazonCaMarketplaceId:
                case MarketplaceKeeper.AmazonMxMarketplaceId:
                    marketCode = "01";
                    break;
                case MarketplaceKeeper.AmazonUkMarketplaceId:
                    marketCode = "02";
                    break;
            }
            if (!String.IsNullOrEmpty(marketCode))
                return String.Format("http://z-ecx.images-amazon.com/images/P/{0}.{1}.MAIN._SCRMZZZZZZ_.jpg",
                    asin,
                    marketCode);
            return "";
        }

        public static string GetNewEmailUrl(string orderId)
        {
            return "/Email/ComposeEmailFromTemplate?type=CustomEmail&orderId=" + orderId;
        }

        public static string GetOrderEmailsUrl(string orderId)
        {
            return "/Email?orderId=" + orderId;
        }

        public static string GetMarketUrl(string asin, string sourceMarketId, MarketType market, string marketplaceId)
        {
            var itemId = asin;
            if (market == MarketType.Magento
                || market == MarketType.Walmart
                || market == MarketType.WalmartCA
                || market == MarketType.Jet
                || market == MarketType.eBay)
            {
                itemId = sourceMarketId;
            }
            if (!String.IsNullOrEmpty(itemId))
                return MarketUrlHelper.GetMarketUrl(itemId, market, marketplaceId);
            return "";
        }

        public static string GetBatchUrl(long? batchId)
        {
            if (!batchId.HasValue)
                return "";
            return "/Batch/ActiveBatches?batchId=" + batchId;
        }

        public static string GetOrderUrl(string orderId)
        {
            return "/Order/Orders?orderId=" + orderId;
        }

        public static string GetOrderHistoryUrl(string orderId)
        {
            return "/Order/OrderHistory?orderId=" + orderId;
        }

        public static string GetViewEmailUrl(long id, string orderNumber)
        {
            return "/Email/ViewEmail?id=" + id + "&orderNumber=" + orderNumber;
        }

        public static string GetViewAttachmentUrl(long id)
        {
            return "/Email/GetAttachment?id=" + id;
        }

        public static string GetReplyToUrl(long emailId, string orderNumber)
        {
            return "/Email/ReplyTo?replyToId=" + emailId + "&orderNumber=" + orderNumber;
        }

        public static string GetStyleUrl(string styleString)
        {
            return !string.IsNullOrEmpty(styleString) ? "/Inventory/Styles?styleId=" + HttpUtility.UrlEncode(styleString) : string.Empty;
        }

        public static string GetStylePopoverInfoUrl(string styleString, long? listingId = null)
        {
            return !string.IsNullOrEmpty(styleString) ? 
                "/Inventory/GetStylePopoverInfo?styleId=" + HttpUtility.UrlEncode(styleString) + (listingId.HasValue ? "&listingId=" + listingId.Value : "") : string.Empty;
        }

        public static string GetProductTemplateFilePath(string relativePath)
        {
            if (String.IsNullOrEmpty(relativePath))
                return String.Empty;
            return Path.Combine(Path.Combine(AppSettings.LabelDirectory, "GeneratedProducts"), relativePath.TrimStart("~/\\".ToCharArray()).Replace("\\", "/"));
        }

        public static string GetBargainExportFilePath(string relativePath)
        {
            if (String.IsNullOrEmpty(relativePath))
                return String.Empty;
            return Path.Combine(Path.Combine(AppSettings.LabelDirectory, "BargainExports"), relativePath.TrimStart("~/\\".ToCharArray()).Replace("\\", "/"));
        }

        //public static string GetLabelPaths(IList<string> relativePaths)
        //{
        //    var paths = "";
        //    foreach (var path in relativePaths)
        //    {
        //        paths += GetLabelPath(path) + " ";
        //    }
        //    return paths;
        //}

        public static string GetAbsolutePath(string relativePath)
        {
            if (String.IsNullOrEmpty(relativePath))
                return String.Empty;
            return HttpContext.Current.Request.Url.Scheme
                           + "://"
                           + HttpContext.Current.Request.Url.Authority
                           + HttpContext.Current.Request.ApplicationPath
                           + relativePath.TrimStart("~/\\".ToCharArray()).Replace("\\", "/");
        }

        public static string GetLabelViewUrl(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                return String.Empty;
            return "/Order/GetFile?filename=" + UrlEncodeFilename(filename);
        }

        public static string GetPrintLabelPathById(long? printPackId)
        {
            if (!printPackId.HasValue || printPackId.Value == 0)
                return String.Empty;

            return HttpContext.Current.Request.Url.Scheme
               + "://"
               + HttpContext.Current.Request.Url.Authority
               + HttpContext.Current.Request.ApplicationPath
               + "/Print/GetLabelPrintFile" + "?id="
               + printPackId;
        }

        public static string GetPrintLabelPath(string label)
        {
            if (String.IsNullOrEmpty(label))
                return String.Empty;

            return HttpContext.Current.Request.Url.Scheme
               + "://"
               + HttpContext.Current.Request.Url.Authority
               + HttpContext.Current.Request.ApplicationPath
               + "/Print/GetFile" + "?filename="
               + label.TrimStart("~/\\".ToCharArray()).Replace("\\", "/");
        }

        //public static string GetAbsolutePaths(IList<string> relativePaths)
        //{
        //    var paths = "";
        //    foreach (var path in relativePaths)
        //    {
        //        paths += GetAbsolutePath(path) + " ";
        //    }
        //    return paths;
        //}

        public static string GetThumbnailUrl(string imageUrl, 
            int width, 
            int height, 
            bool autoRotate, 
            string defaultThumbnail = "",
            bool skipExternalUrlToThumbnail = false,
            bool convertToFullUrl = false,
            bool convertInDomainUrlToThumbnail = false)
        {
            if (String.IsNullOrEmpty(imageUrl))
                return defaultThumbnail;

            var thumbnail = "";
            //http => https for Amazon
            imageUrl = imageUrl.Replace("http://ecx.images-amazon.com", "https://images-na.ssl-images-amazon.com");

            if (!String.IsNullOrEmpty(imageUrl))
            {
                var makeThumbnail = false;
                if (imageUrl.Contains("http") || imageUrl.Contains("https"))
                {
                    if (skipExternalUrlToThumbnail)
                    {
                        //Do nothing (use original image)
                        makeThumbnail = false;
                    }
                    else
                    {
                        if (convertInDomainUrlToThumbnail)
                        {
                            if (imageUrl.Contains(DomainUrl))
                            {
                                imageUrl = imageUrl.Replace(DomainUrl, "~");
                                
                                makeThumbnail = true;
                            }

                            if (imageUrl.Contains(DomainHttpUrl))
                            {
                                imageUrl = imageUrl.Replace(DomainHttpUrl, "~");

                                makeThumbnail = true;
                            }

                            //NOTE: only for debug
                            if (imageUrl.Contains("http://localhost"))
                            {
                                imageUrl = imageUrl.Replace("http://localhost", "");
                                var index = imageUrl.IndexOf('/');
                                if (index > 0)
                                {
                                    imageUrl = imageUrl.Substring(index);
                                }
                                imageUrl = "~" + imageUrl;

                                makeThumbnail = true;
                            }
                        }

                        if (imageUrl.Contains("ebay"))
                        {
                            makeThumbnail = true;
                        }
                        
                        //ex.: http://ecx.images-amazon.com/images/I/81mNwH7ORsL._UL1500_.jpg
                        //https://images-na.ssl-images-amazon.com/images/I/81mNwH7ORsL._UL1500_.jpg
                        //http://z-ecx.images-amazon.com/images/P/B00FXJL30Y.03.MAIN.SL_SCRMZZZZZZ_.jpg
                        if (((imageUrl.StartsWith("http") && imageUrl.Contains("ecx.images-amazon.com"))
                            || (imageUrl.StartsWith("https") && imageUrl.Contains("images-amazon.com")))
                            && imageUrl.EndsWith(".jpg"))
                        {
                            if (imageUrl.Contains("._") && imageUrl.Contains("_."))
                            {
                                var beginIndex = imageUrl.LastIndexOf("._", StringComparison.Ordinal);
                                var endIndex = imageUrl.LastIndexOf("_.", StringComparison.Ordinal) + 1; //Keep "."
                                if (beginIndex >=0 && endIndex >= 0)
                                    imageUrl = imageUrl.Remove(beginIndex, (endIndex - beginIndex));
                            }
                            
                            //if (imageUrl.Contains("_SCRMZZZZZZ_"))
                            //    imageUrl = imageUrl.Replace("_SCRMZZZZZZ_", "_SL75_");

                            //if (imageUrl.Contains("_SL1500_"))
                            //    imageUrl = imageUrl.Replace("_SL1500_", "_SL75_");

                            //if (imageUrl.Contains("_SL1024_"))
                            //    imageUrl = imageUrl.Replace("_SL1024_", "_SL75_");

                            //if (imageUrl.Contains("_UL1500_"))
                            //    imageUrl = imageUrl.Replace("_UL1500_", "_SL75_");

                            if (!imageUrl.Contains("_.") && !imageUrl.Contains("._"))
                                imageUrl = imageUrl.Replace(".jpg", "._SL75_.jpg");
                        }
                    }
                }
                else
                {
                    makeThumbnail = true;
                }
                
                if (makeThumbnail)
                {
                    imageUrl = HttpUtility.UrlEncode(HttpUtility.UrlDecode(imageUrl)) ?? "";

                    //thumbnail = "/Image/Thumbnail?path=" + HttpUtility.UrlEncode(imageUrl) + "&height=" + height + "&width=" + width + "&rotate=auto";
                    thumbnail = String.Format("/Image/Thumbnail/{0}/{1}/{2}/{3}", 
                        width, 
                        height, 
                        autoRotate ? "auto" : "none", 
                        imageUrl.Replace("+", "%20"));

                    if (convertToFullUrl)
                        thumbnail = DomainUrl + thumbnail;
                }
            }

            if (!String.IsNullOrEmpty(thumbnail))
                return thumbnail;
            
            return imageUrl;
        }

        public static string UrlEncodeFilename(string filename)
        {
            return HttpUtility.UrlPathEncode(filename).Replace("#", "%23");
        }

        public static string RemoveUrlParams(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            return url.Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }
    }
}