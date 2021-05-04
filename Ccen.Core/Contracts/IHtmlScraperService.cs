using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IHtmlScraperService
    {
        CallResult<string> GetHtml(string url);
        CallResult<string> GetHtml(string url, ProxyUseTypes proxyType, Func<ProxyInfoDto, HttpStatusCode, string, bool> checkResult);
    }
}
