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
    public class AmazonPageParser : IWebPageParser
    {
        private class AmazonColorImage
        {
            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("hiRes")]
            public string HiRes { get; set; }
        }

        public bool ValidateHtml(string content)
        {
            return (content ?? "").Contains("<div id=\"imgTagWrapperId\" class=\"imgTagWrapper\"");
        }

        public CallResult<IList<ImageInfo>> GetLargeImages(string html)
        {
            //Check multi-color
            var jsonKeyword = "data[\"colorImages\"] =";
            var beginJsonBlock = html.IndexOf(jsonKeyword);
            if (beginJsonBlock > 0)
            {
                beginJsonBlock += jsonKeyword.Length;
                var endJsonBlock = html.IndexOf("};", beginJsonBlock);
                if (endJsonBlock > 0)
                {
                    try
                    {
                        var json = html.Substring(beginJsonBlock, endJsonBlock - beginJsonBlock + 1);
                        var values = JsonConvert.DeserializeObject<Dictionary<string, List<AmazonColorImage>>>(json);

                        if (values.Any())
                        {
                            var images = values.Select(v => new ImageInfo()
                            {
                                Color = v.Key,
                                Image = v.Value.Select(i => i.HiRes).FirstOrDefault()
                            }).ToList();

                            return CallResult<IList<ImageInfo>>.Success(images);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }


            var beginImageBlock = html.IndexOf("<div id=\"imgTagWrapperId\" class=\"imgTagWrapper\"");
            if (beginImageBlock > 0)
            {
                var endImageBlock = html.IndexOf("</div>", beginImageBlock);
                if (endImageBlock > 0)
                {
                    var imageBlock = html.Substring(beginImageBlock, endImageBlock - beginImageBlock);
                    var matches = Regex.Match(imageBlock, "<img.*?data-old-hires=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
                    if (matches.Groups.Count > 1)
                    {
                        var image = matches.Groups[1].Value;
                        //if (image.Contains("_UL1500_") 
                        //    || image.Contains("_SL1500_")
                        //    || image.Contains("_SL1024_"))
                        if (image.EndsWith(".jpg")
                            || image.EndsWith(".jpeg")
                            || image.EndsWith(""))
                        {
                            return new CallResult<IList<ImageInfo>>()
                            {
                                Data = new List<ImageInfo>() { new ImageInfo { Color = "", Image = image } },
                                Status = CallStatus.Success
                            };
                        }
                    }

                    //matches = Regex.Match(imageBlock, "<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
                    //if (matches.Groups.Count > 1)
                    //    return matches.Groups[1].Value;
                }
            }
            return null;
        }

        public CallResult<string> GetSpecialSize(string html)
        {
            var specialSizeIndex = html.IndexOf("id=\"variation_special_size_type\"", StringComparison.OrdinalIgnoreCase);
            if (specialSizeIndex > 0)
            {
                var endSpecialSize = html.IndexOf("</div>", specialSizeIndex, StringComparison.Ordinal);
                if (endSpecialSize > 0)
                {
                    var specialSizeText = html.Substring(specialSizeIndex, endSpecialSize - specialSizeIndex);
                    if (specialSizeText.Contains("Special Size:"))
                    {
                        var token = "<span class=\"selection\">";
                        var beginValueIndex = specialSizeText.IndexOf(token);
                        if (beginValueIndex > 0)
                        {
                            var endValueIndex = specialSizeText.IndexOf("</span>", beginValueIndex);
                            if (endValueIndex > 0)
                            {
                                var specialCase = StringHelper.TrimWhitespace(specialSizeText.Substring(beginValueIndex + token.Length,
                                    endValueIndex - (beginValueIndex + token.Length)));
                                return new CallResult<string>()
                                {
                                    Data = specialCase,
                                    Status = CallStatus.Success
                                };
                            }
                        }
                    }
                }
            }

            return new CallResult<string>()
            {
                Data = null,
                Status = CallStatus.Success
            };
        }
    }
}
