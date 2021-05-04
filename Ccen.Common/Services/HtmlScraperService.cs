using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;

namespace Amazon.Common.Helpers
{
    public class HtmlScraperService : IHtmlScraperService
    {
        private class ResponseInfo
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Html { get; set; }
        }

        //private static List<ProxyInfo> _proxyList = new List<ProxyInfo>()
        //{
        //    //http://hideme.ru/proxy-list/
        //    new ProxyInfo()
        //    {
        //        IPAddress = "54.164.192.252",
        //        Port = 3128
        //    },
        //    new ProxyInfo()
        //    {
        //        IPAddress = "67.205.129.243",
        //        Port = 8080
        //    },
        //    new ProxyInfo()
        //    {
        //        IPAddress = "46.37.193.74",
        //        Port = 3128
        //    },
        //    new ProxyInfo()
        //    {
        //        IPAddress = "111.23.6.152",
        //        Port = 80
        //    },
        //    new ProxyInfo()
        //    {
        //        IPAddress = "111.1.23.175",
        //        Port = 80
        //    }
        //};

        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;

        public HtmlScraperService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
        }

        public CallResult<string> GetHtml(string url)
        {
            return GetHtml(url, ProxyUseTypes.None, (info, code, html) => true);
        }

        public CallResult<string> GetHtml(string url, ProxyUseTypes proxyType, Func<ProxyInfoDto, HttpStatusCode, string, bool> checkResult)
        {
            var checkedCount = 0;
            var maxCheckedCount = 8;

            using (var db = _dbFactory.GetRWDb())
            {
                if (proxyType == ProxyUseTypes.None)
                {
                    var response = GetHtmlByProxy(url, null);
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                        return CallResult<string>.Success(response.Html);
                    return CallResult<string>.Fail("No repsonse, statusCode=" + (response != null ? response.StatusCode.ToString() : ""), null);
                }
                else
                {
                    var proxyList = db.ProxyInfos.GetAll().Where(p => p.UseType == (int) proxyType
                                                                      && p.IsActive
                                                                      && p.SucceedRequestCount * 5 > p.FailedRequestCount).ToList()
                                                                      .OrderBy(p => p.LastFailedRequestDate ?? DateTime.MinValue).ToList();

                    maxCheckedCount = Math.Min(maxCheckedCount, proxyList.Count);
                    var proxyIndex = 0;// new Random(DateTime.Now.Millisecond).Next(0, proxyList.Count);
                        //Random exclude Max value

                    while (checkedCount < maxCheckedCount)
                    {
                        var proxy = proxyList[proxyIndex];
                        var response = GetHtmlByProxy(url, proxy);
                        if (checkResult(BuildProxyDto(proxy), response.StatusCode, response.Html))
                        {
                            _log.Info("Check success");
                            proxy.SucceedRequestCount++;
                            proxy.LastSucceedRequestDate = _time.GetAppNowTime();

                            db.Commit();

                            return CallResult<string>.Success(response.Html);
                        }
                        else
                        {
                            _log.Info("Check failed, status=" + response.StatusCode);

                            proxy.FailedRequestCount++;
                            proxy.LastFailedRequestDate = _time.GetAppNowTime();

                            db.Commit();
                        }

                        proxyIndex++;
                        if (proxyIndex >= proxyList.Count)
                            proxyIndex = 0;

                        checkedCount++;
                    }
                }
            }

            return CallResult<string>.Fail("All proxies failes", null);
        }

        private ProxyInfoDto BuildProxyDto(ProxyInfo proxyInfo)
        {
            return new ProxyInfoDto()
            {
                Id = proxyInfo.Id,
                IPAddress = proxyInfo.IPAddress,
                Port = proxyInfo.Port
            };
        }

        private ResponseInfo GetHtmlByProxy(string url, ProxyInfo proxy)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
                request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                if (proxy != null && !String.IsNullOrEmpty(proxy.IPAddress))
                {
                    request.Proxy = new WebProxy(proxy.IPAddress + ":" + proxy.Port, false);
                }
                request.Timeout = 20000;
                request.ReadWriteTimeout = 30000;

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream receiveStream = response.GetResponseStream())
                        {
                            StreamReader readStream = null;

                            if (String.IsNullOrEmpty(response.CharacterSet))
                            {
                                readStream = new StreamReader(receiveStream);
                            }
                            else
                            {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            return new ResponseInfo()
                            {
                                Html = readStream.ReadToEnd(),
                                StatusCode = response.StatusCode
                            };
                        }
                    }
                    return new ResponseInfo()
                    {
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _log.Error("GetHtml", ex);

                return new ResponseInfo()
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            }
        }
    }
}
