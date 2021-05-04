using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallTestHttpParsing
    {
        private IHtmlScraperService _htmlScraper;
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public CallTestHttpParsing(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
        }


        public bool CallMellissaPage()
        {
            var url = "http://www.melissadata.com/Lookups/AddressVerify.asp?name=linda%20airhart&Company=&Address=300%20garber%20st&city=plains&state=MT&zip=59859-0178";
            var response = _htmlScraper.GetHtml(url, ProxyUseTypes.Mellisa, ((dto, code, arg3) => { return true; } ));
            var result = false;
            if (response.IsSuccess && response.Data.Contains("Address is served by FedEx, UPS and NOT the USPS"))
                result = true;

            url = "http://www.melissadata.com/Lookups/AddressVerify.asp?name=EVELYN+JOSE&Company=&Address=7137+35+AVENUE+NW&city=CALGARY&state=AB&zip=T3B+1T1";
            response = _htmlScraper.GetHtml(url);
            result = false;
            if (response.IsSuccess && response.Data.Contains("Address is served by FedEx, UPS and NOT the USPS"))
                result = true;

            return result;
        }

        public string CallGetSpecialPrice()
        {
            var url = "https://www.amazon.com/dp/B00XEUMHRW";
            var response = _htmlScraper.GetHtml(url);
            if (response.IsSuccess)
            {
                var result = new AmazonPageParser().GetSpecialSize(response.Data);
                if (result != null)
                    return result.Data;
            }
            return null;
        }
    }
}
