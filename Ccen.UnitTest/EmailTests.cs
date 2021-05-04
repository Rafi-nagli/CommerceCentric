using System;
using System.IO;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Emails;
using Ccen.UnitTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ccen.UnitTest
{
    [TestClass]
    public class EmailTests : BaseTest
    {
        [TestMethod]
        public void ConfirmationEmailTest()
        {
            var path = Path.Combine(AppSettings.TemplateDirectory, @"OrderConfirmation\OrderConfirmation.cshtml");
            var domainUrl = "http://mbg.commercentric.com";
           

            /*var orderConfirmationService = new OrderConfirmationService(
                _context._log,
                _context._time,
                _context._dbFactory,
                _context._weightService,
                _context._emailService,
                domainUrl,
                path);

            orderConfirmationService.SendOrderConfirmationEmail("272445", MarketType.Shopify, MarketplaceKeeper.ShopifyMBB);*/
        }
    }
}
