using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;

namespace Amazon.Core.Contracts
{
    public interface IWebPageParser
    {
        bool ValidateHtml(string content);
        CallResult<IList<ImageInfo>> GetLargeImages(string html);
    }
}
