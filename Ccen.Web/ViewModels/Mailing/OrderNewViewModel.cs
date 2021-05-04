using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Results;
using Amazon.DTO.Users;
using UrlHelper = Amazon.Web.Models.UrlHelper;
using Amazon.Model.Implementation.Markets.User;
using Amazon.Model.General;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Implementation.Errors;

namespace Amazon.Web.ViewModels
{
    public class OrderNewViewModel
    {
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string ShippingService { get; set; }


        public AddressViewModel ToAddress { get; set; }

        public IList<MailItemViewModel> Items { get; set; }

        public List<MessageString> Messages { get; set; }

        public OrderNewViewModel()
        {
            Items = new List<MailItemViewModel>();
            ToAddress = new AddressViewModel();
            Messages = new List<MessageString>();
        }

        public bool IsValid(IUnitOfWork db, out IList<MessageString> messages)
        {
            messages = new List<MessageString>();
            return true;
        }


        public void Create(ILogService log,
            ITime time,
            IQuantityManager quantityManager,
            IDbFactory dbFactory,
            IWeightService weightService,
            IShippingService shippingService,
            IAutoCreateListingService createListingService,
            ISettingsService settingService,
            IEmailService emailService,
            ISystemActionService actionService,
            IHtmlScraperService htmlScraper,
            IOrderHistoryService orderHistory,
            IPriceService priceService,            
            CompanyDTO company,
            DateTime when,
            long? by)
        {
            var syncInfo = new EmptySyncInformer(log, SyncType.Orders);
            var market = (int)MarketType.OfflineOrders;
            var marketplaceId = MarketplaceKeeper.ManuallyCreated;

            var orderItems = new List<ListingOrderDTO>();

            using (var db = dbFactory.GetRWDb())
            {
                var index = 1;
                foreach (var item in Items)
                {
                    var dbItem = db.Items.GetAll().FirstOrDefault(i => i.Market == market
                        && i.MarketplaceId == marketplaceId
                        && i.StyleItemId == item.StyleItemId);

                    if (dbItem == null)
                    {
                        var itemPrice = item.ItemPrice;// db.Items.GetAllViewActual()
                                                       //.FirstOrDefault(i => i.Market == (int)MarketType.Amazon
                                                       //    && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)?.CurrentPrice;

                        log.Info("Request create listing, market=" + market
                            + ", marketplaceId=" + marketplaceId);

                        IList<MessageString> messages = new List<MessageString>();
                        //Create New
                        var model = createListingService.CreateFromStyle(db,
                            item.StyleId.Value,
                            (MarketType)market,
                            marketplaceId,
                            out messages);

                        model.Variations.ForEach(v => v.CurrentPrice = itemPrice);

                        createListingService.Save(model,
                            "",
                            db,
                            when,
                            by);

                        dbItem = db.Items.GetAll().FirstOrDefault(i => i.Market == market
                            && i.MarketplaceId == marketplaceId
                            && i.StyleItemId == item.StyleItemId);
                    }

                    var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.Market == market
                        && l.MarketplaceId == marketplaceId
                        && l.ItemId == dbItem.Id);

                    orderItems.Add(new ListingOrderDTO()
                    {
                        ASIN = dbItem.ASIN,
                        SKU = dbListing.SKU,
                        ItemPaid = item.ItemPrice,
                        ItemPrice = item.ItemPrice,
                        ItemGrandPrice = item.ItemPrice,
                        StyleId = dbItem.StyleId,
                        StyleID = dbItem.StyleString,
                        StyleItemId = dbItem.StyleItemId,
                        Market = dbItem.Market,
                        MarketplaceId = dbItem.MarketplaceId,
                        QuantityOrdered = item.Quantity,
                        ItemOrderId = index.ToString(),
                        SourceListingId = dbListing.Id,
                    });

                    index++;
                }

                OrderNumber = db.Orders.GetAll()
                    .Where(o => o.Market == (int)market
                        && o.MarketplaceId == marketplaceId)
                    .OrderByDescending(o => o.Id).FirstOrDefault()?.AmazonIdentifier;

                if (String.IsNullOrEmpty(OrderNumber))
                    OrderNumber = "1000";
                else
                    OrderNumber = ((StringHelper.TryGetInt(OrderNumber) ?? 1000) + 1).ToString();
            }

            var dtoOrder = new DTOOrder()
            {
                Market = market,
                MarketplaceId = marketplaceId,
                OrderDate = OrderDate,
                OrderStatus = "Unshipped",
                SourceOrderStatus = "Unshipped",
                OrderId = OrderNumber,
                CustomerOrderId = OrderNumber,
                MarketOrderId = OrderNumber,

                AmazonEmail = ToAddress.Email,
                BuyerEmail = ToAddress.Email,
                PersonName = ToAddress.FullName,
                BuyerName = ToAddress.FullName,
                ShippingAddress1 = ToAddress.Address1,
                ShippingAddress2 = ToAddress.Address2,
                ShippingCity = ToAddress.City,
                ShippingCountry = ToAddress.Country,
                ShippingZip = ToAddress.Zip,
                ShippingZipAddon = ToAddress.ZipAddon,
                ShippingPhone = ToAddress.Phone,
                ShippingState = StringHelper.GetFirstNotEmpty(ToAddress.USAState, ToAddress.NonUSAState),

                ShippingPaid = 0,
                ShippingPrice = 0,
                TotalPaid = Items.Sum(i => i.ItemPrice),
                TotalPrice = Items.Sum(i => i.ItemPrice),

                Quantity = Items.Sum(i => i.Quantity),

                InitialServiceType = ShippingService,
                ShippingService = ShippingService,
                SourceShippingService = ShippingService,

                Items = orderItems,
            };

            var userOrderApi = new UserOrderApi(new List<DTOOrder>() { dtoOrder });

            var serviceFactory = new ServiceFactory();
            var addressCheckServices = serviceFactory.GetAddressCheckServices(log,
                time,
                dbFactory,
                company.AddressProviderInfoList);
            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(addressCheckServices, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));

            var rateProviders = serviceFactory.GetShipmentProviders(log,
                time,
                dbFactory,
                weightService,
                company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);

            var validatorService = new OrderValidatorService(log, dbFactory, emailService, settingService, orderHistory, actionService, 
                priceService, htmlScraper, addressService, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), stampsRateProvider, time, company);
            var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
            var cacheService = new CacheService(log, time, actionService, quantityManager);

            using (var db = dbFactory.GetRWDb())
            {
                try
                {
                    var orderSyncFactory = new OrderSyncFactory();
                    var synchronizer = orderSyncFactory.GetForMarket(userOrderApi,
                        log,
                        company,
                        settingService,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        actionService,
                        companyAddress,
                        time,
                        weightService,
                        null);

                    if (!String.IsNullOrEmpty(OrderNumber))
                    {
                        synchronizer.ProcessSpecifiedOrder(db, OrderNumber);
                        Messages.Add(MessageString.Success("The order has been successfully created, order #: " + OrderNumber));
                    }
                }
                catch (Exception ex)
                {
                    Messages.Add(MessageString.Error(ex.Message));
                }
            }
        }
    }
}