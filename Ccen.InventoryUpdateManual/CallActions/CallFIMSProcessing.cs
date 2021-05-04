using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.General.Markets;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Utils;
using Fedex.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallFIMSProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;

        public CallFIMSProcessing(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _company = company;
        }
        
        public void TestGetLabel(string orderId)
        {
            var weightService = new WeightService();
            var companyAddress = new CompanyAddressService(_company);

            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(weightService, orderId, unmaskReferenceStyle: false, includeSourceItems: true);
                var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(order.InitialServiceType, order.UpgradeLevel); //order.ShippingService
                var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.Items);
                var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.SourceItems);

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    _company.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory);

                var fimsProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FIMS);

                //var rates = fedexRateProvider.GetLocalRate(
                //        companyAddress.GetReturnAddress(order.GetMarketId()),
                //        companyAddress.GetPickupAddress(order.GetMarketId()),
                //        order.GetAddressDto(),
                //        _time.GetAppNowTime(),
                //        order.WeightD,
                //        order.IsInsured ? order.TotalPrice : 0,
                //        order.IsSignConfirmation,
                //        new OrderRateInfo()
                //        {
                //            ShippingService = shippingService,
                //            InitialServiceType = order.InitialServiceType,
                //            OrderNumber = order.OrderId,
                //            Items = orderItemInfoes,
                //            SourceItems = sourceOrderItemInfoes,
                //            TotalPrice = order.TotalPrice,
                //            Currency = order.TotalPriceCurrency,
                //        },
                //        RetryModeType.Normal);

                var shipmentInfo = new OrderShippingInfoDTO()
                {
                    OrderAmazonId = order.OrderId,
                    WeightD = order.WeightD, 
                    IsInsured = order.IsInsured,
                    TotalPrice = order.TotalPrice,
                    TotalPriceCurrency = order.TotalPriceCurrency,

                    IsSignConfirmation = order.IsSignConfirmation,

                    Items = orderItemInfoes.Select(oi => new DTOOrderItem()
                    {
                        ItemOrderId = oi.ItemOrderId,
                        ItemPrice = oi.ItemPrice,
                        Weight = oi.Weight,
                        Quantity = oi.Quantity,
                    }).ToList(),
                    ShippingMethod = new ShippingMethodDTO()
                    {
                        
                    },
                };

                var shipDate = db.Dates.GetOrderShippingDate(null);

                var boughtInTheCountry = MarketBaseHelper.GetMarketCountry((MarketType)order.Market, order.MarketplaceId);
                //var callResult = labelProvider.CreateShipment(shipmentInfo,
                //    returnAddress,
                //    pickupAddress,
                //    toAddress,
                //    shipDate.Date,
                //    model.Notes,
                //    !model.ShippingMethod.IsSupportReturnToPOBox,
                //    sampleMode,
                //    fromUI: true);


                var labels = fimsProvider.CreateShipment(
                       shipmentInfo,
                       companyAddress.GetReturnAddress(order.GetMarketId()),
                       companyAddress.GetPickupAddress(order.GetMarketId()),
                       order.GetAddressDto(),
                       boughtInTheCountry,
                       shipDate,
                       "",
                       false,
                       false,
                       false);


                _log.Info("Labels: " + labels.Data.LabelFileList.Count);
            }
        }
    }
}
