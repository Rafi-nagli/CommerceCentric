using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Newtonsoft.Json;

namespace Amazon.Common.Helpers
{
    public class WalmartPageParser : IWebPageParser
    {
        public bool ValidateHtml(string content)
        {
            return (content ?? "").Contains("<div class=\"prod-HeroImage-container\"");
        }

        public CallResult<IList<ImageInfo>> GetLargeImages(string html)
        {
            var beginNameBlock = html.IndexOf("<h2 class=\"prod-ProductTitle");
            var name = String.Empty;
            if (beginNameBlock > 0)
            {
                var endNameBlock = html.IndexOf("</h2>", beginNameBlock);
                if (endNameBlock > 0)
                {
                    var nameBlock = html.Substring(beginNameBlock, endNameBlock - beginNameBlock);
                    var matches = Regex.Match(nameBlock, "<div.*?>(.+?)</div>", RegexOptions.IgnoreCase);
                    if (matches.Groups.Count > 1)
                    {
                        name = matches.Groups[1].Value;
                    }
                }
            }

            var beginImageBlock = html.IndexOf("<div class=\"prod-HeroImage-container\"");
            if (beginImageBlock > 0)
            {
                var endImageBlock = html.IndexOf("</div></div>", beginImageBlock);
                if (endImageBlock > 0)
                {
                    var imageBlock = html.Substring(beginImageBlock, endImageBlock - beginImageBlock);
                    var matches = Regex.Match(imageBlock, "<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
                    if (matches.Groups.Count > 1)
                    {
                        var image = matches.Groups[1].Value;
                        if (image.EndsWith(".jpg")
                            || image.EndsWith(".jpeg")
                            || image.EndsWith(""))
                        {
                            return new CallResult<IList<ImageInfo>>()
                            {
                                Data = new List<ImageInfo>() { new ImageInfo
                                {
                                    Color = "",
                                    Image = image,
                                    Name = name
                                }},
                                Status = CallStatus.Success
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
