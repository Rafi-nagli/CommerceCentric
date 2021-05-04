using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ImageResizer;

namespace Ccen.Common.Helpers
{
    public class ThumbnailHelper
    {
        public static void GenerateThumbnail(Image sourceImage,
            string thumbnailFileName,
            int width,
            int height,
            string rotate)
        {
            using (var outStream = System.IO.File.Open(thumbnailFileName, FileMode.Create))
            {
                var query = "";
                if (width > 0)
                    query = "width=" + width.ToString();
                if (height > 0)
                    query += (!String.IsNullOrEmpty(query) ? "&" : "") + "height=" + height.ToString();
                var settings = new ResizeSettings(query);

                //NOTE: if height more then height + 50%
                if (rotate == "auto") // && sourceImage.Width > sourceImage.Height * 1.5)
                    settings.Rotate = 90;
                ImageBuilder.Current.Build(sourceImage, outStream, settings);
            }
        }

        public static string GetThumbnailUrl(string imageUrl,
            string domainUrl,
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
            if (!String.IsNullOrEmpty(imageUrl))
            {
                var makeThumbnail = true;
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
                            if (imageUrl.Contains(domainUrl))
                            {
                                imageUrl = imageUrl.Replace(domainUrl, "~");

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
                                if (beginIndex >= 0 && endIndex >= 0)
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
                        thumbnail = domainUrl + thumbnail;
                }
            }

            if (!String.IsNullOrEmpty(thumbnail))
                return thumbnail;

            return imageUrl;
        }
    }
}
