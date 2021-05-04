using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Api.AmazonECommerceService;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;

namespace Amazon.Model.Implementation
{
    public class ImageRequestingService
    {
        private ILogService _log;
        private IHtmlScraperService _htmlScraper;
        
        public ImageRequestingService(ILogService log,
            IHtmlScraperService htmlScraperService)
        {
            _log = log;
            _htmlScraper = htmlScraperService;
        }



        public CallResult<IList<ImageInfo>> GetMainImageFromUrl(string url, IWebPageParser pageParser, out long downloadedSize)
        {
            downloadedSize = 0;
            var html = String.Empty;
            try
            {
                /*
                 <div id="imgTagWrapperId" class="imgTagWrapper" style="height: 801.299px;">
                          
                            <img alt="Sara's Prints Little Girls' Short Sleeve Nightie, Red/Pink Chevron, 2" 
                 src="http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY879_.jpg" 
                 data-old-hires="http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UL1500_.jpg" 
                 class="a-dynamic-image  a-stretch-vertical" id="landingImage" 
                 data-a-dynamic-image="{&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY606_.jpg&quot;:[405,606],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY879_.jpg&quot;:[587,879],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY741_.jpg&quot;:[495,741],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY445_.jpg&quot;:[297,445],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY500_.jpg&quot;:[334,500],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY679_.jpg&quot;:[453,679],&quot;http://ecx.images-amazon.com/images/I/81YDdI2Rk0L._UY550_.jpg&quot;:[367,550]}" style="max-height: 801px; max-width: 587px;">
                          
                        </div>
                */

                var htmlResult = _htmlScraper.GetHtml(url, ProxyUseTypes.Amazon, (proxy, status, content) =>
                {
                    return status == HttpStatusCode.OK
                        && pageParser.ValidateHtml(content);
                });
                if (htmlResult.IsSuccess)
                {
                    html = htmlResult.Data;
                    downloadedSize = html.Length;

                    return pageParser.GetLargeImages(html);
                }
                else
                {
                    CallHelper.ThrowIfFail(htmlResult);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Parsing html page issue, url=" + url, ex);
                _log.Info("HTML: " + html);
                return new CallResult<IList<ImageInfo>>()
                {
                    Status = CallStatus.Fail,
                    Exception = ex,
                };
            }
            return new CallResult<IList<ImageInfo>>()
            {
                Status = CallStatus.Fail
            };
        }
    }
}
