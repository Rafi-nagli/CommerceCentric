using System;
using System.Collections.Generic;
using Amazon.Common.Services;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce.Api;

namespace WooCommerce.Test
{
    [TestClass]
    public class MainTests
    {
        private ILogService _log;
        private ITime _time;

        private string _apiKey;
        private string _apiSecret;
        private string _endPoint;

        [TestMethod]
        public void GetProducts()
        {
            var api = new WooCommerceApi(_log, _time, "", _apiKey, _apiSecret, _endPoint);

            List<string> asinWithErrors;
            var results = api.GetItems(_log, _time, null, ItemFillMode.Defualt, out asinWithErrors);
            _log.Info(results.ToString());
        }

        [TestMethod]
        public void GetOrders()
        {
            var api = new WooCommerceApi(_log, _time, "", _apiKey, _apiSecret, _endPoint);

            var results = api.GetOrders(_log, DateTime.Now.AddDays(-60), null);
            _log.Info(results.ToString());
        }

        [TestInitialize]
        public void Setup()
        {
            var dbFactory = new DbFactory();
            _time = new TimeService(dbFactory);
            _log = LogFactory.Console;

            _apiKey = "ck_075cade343b3655bf41c8f8c8f04ffb210b81016";
            _apiSecret = "cs_33fa9ebd809013bc5aa8a62b5d13534ec22ca0cb";
            _endPoint = "http://www.cibortv.com/wp-json/wc/v3/";
        }
    }
}
