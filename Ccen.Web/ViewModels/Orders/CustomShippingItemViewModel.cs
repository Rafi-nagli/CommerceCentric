using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using Kendo.Mvc.UI;

namespace Amazon.Web.ViewModels.Orders
{
    public class CustomShippingItemViewModel
    {
        public long OrderItemId { get; set; }
        public long? ShippingInfoId { get; set; }
        
        public int? ShippingMethodId { get; set; }
        public string ShippingMethodName { get; set; }

        public int? PackageNumber { get; set; }

        public string PackageValue { get; set; }

        public string ASIN { get; set; }
        public string SKU { get; set; }
        public string StyleString { get; set; }
        public string StyleSize { get; set; }

        public double Weight { get; set; }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public string PictureUrl { get; set; }
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }


        public string ToString()
        {
            return "OrderItemId=" + OrderItemId
                   + ", ShippingInfoId=" + ShippingInfoId
                   + ", ShippingMethodId=" + ShippingInfoId
                   + ", ShippingMethodName=" + ShippingMethodName;
        }

        public CustomShippingItemViewModel()
        {
            
        }

        

        public static CallResult<IList<SelectListShippingOption>> Apply(IUnitOfWork db,
            ILogService log,
            ITime time,
            IWeightService weightService,
            long orderId,
            IList<IShipmentApi> ratePrividers,
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            IList<CustomShippingItemViewModel> customShippingItems,
            bool isFulfilmentUser)
        {
            var order = db.Orders.GetById(orderId);

            var correctedInitialShippingType = ShippingUtils.CorrectInitialShippingService(order.InitialServiceType, order.SourceShippingService, (OrderTypeEnum)order.OrderType);
            var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(correctedInitialShippingType, order.UpgradeLevel);

            var shippingProviderType = (ShipmentProviderType) db.Orders.GetById(orderId).ShipmentProviderType;
            var rateProvider = ratePrividers.FirstOrDefault(p => p.Type == shippingProviderType);

            var oldShippings = db.OrderShippingInfos.GetByOrderId(orderId).ToList();
            var previousNumberInBatch = oldShippings.FirstOrDefault(sh => sh.IsActive)?.NumberInBatch;

            var items = db.OrderItems.GetWithListingInfo()
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new ListingOrderDTO()
            {
                ItemOrderId = oi.ItemOrderId,
                OrderItemEntityId = oi.OrderItemEntityId,
                Weight = oi.Weight
            }).ToList();

            if (items.Any(i => !i.Weight.HasValue || i.Weight == 0))
                return CallResult<IList<SelectListShippingOption>>.Fail("No rates. Order has items w/o weight.", null);

            var packageDict = new Dictionary<string, PackageInfo>();
            foreach (var shippingItem in customShippingItems.OrderBy(sh => sh.PackageValue).ToList())
            {
                var packageValue = shippingItem.PackageValue;

                var orderItem = items.FirstOrDefault(oi => oi.OrderItemEntityId == shippingItem.OrderItemId);
                var package = packageDict.ContainsKey(packageValue) ? packageDict[packageValue] : null;
                if (package == null)
                {
                    var shippingMethodId = int.Parse(shippingItem.PackageValue.Split('-')[0]);
                    var dbShippingMethod = db.ShippingMethods.GetAll().FirstOrDefault(m => m.Id == shippingMethodId);

                    package = new PackageInfo
                    {
                        Items = new List<OrderItemRateInfo>(),
                        RequiredServiceIdentifier = dbShippingMethod.ServiceIdentifier,
                        ServiceTypeUniversal = ShippingUtils.GetShippingType(shippingMethodId),
                        PackageTypeUniversal = ShippingUtils.GetPackageType(shippingMethodId),
                        GroupId = RateHelper.CustomPartialGroupId,
                    };

                    packageDict[packageValue] = package;
                }

                package.Items.Add(new OrderItemRateInfo()
                {
                    Quantity = 1,
                    ItemOrderId = orderItem.ItemOrderId,
                    Weight = orderItem.Weight ?? 0,
                });
            }

            var packages = packageDict.Values;

            var addressTo = db.Orders.GetAddressInfo(order.Id);
            var shipDate = db.Dates.GetOrderShippingDate(null);
            
            var rates = new List<RateDTO>();
            foreach (var package in packages)
            {
                package.Weight = weightService.AdjustWeight(package.Items.Sum(i => i.Weight * i.Quantity),
                    package.Items.Sum(i => i.Quantity));

                GetRateResult rateResult = null;
                log.Info("GetSpecificLocalRate, orderId=" + orderId);
                rateResult = rateProvider.GetAllRate(returnAddress,
                    pickupAddress,
                    addressTo,
                    shipDate,
                    package.Weight ?? 1,
                    package.GetDimension(),
                    order.IsInsured ? order.TotalPrice : 0,
                    order.IsSignConfirmation,
                    new OrderRateInfo()
                    {
                        OrderNumber = order.AmazonIdentifier,

                        Items = package.Items,
                        SourceItems = package.Items,

                        EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(order.LatestShipDate, (MarketType) order.Market),
                        ShippingService = shippingService,
                        TotalPrice = order.TotalPrice,
                        Currency = order.TotalPriceCurrency,
                    },
                    RetryModeType.Random);

                if (rateResult.Result != GetRateResultType.Success)
                    return CallResult<IList<SelectListShippingOption>>.Fail("Error when get rates for package, serviceType=" + package.ServiceTypeUniversal.ToString() + ", packageType=" + package.PackageTypeUniversal.ToString(), null);

                var rate = rateResult.Rates.FirstOrDefault(r => r.ServiceIdentifier == package.RequiredServiceIdentifier);
                if (rate == null)
                    return CallResult<IList<SelectListShippingOption>>.Fail("Not rates for package, serviceType=" + package.ServiceTypeUniversal.ToString() + ", packageType=" + package.PackageTypeUniversal.ToString(), null);

                rate.GroupId = RateHelper.CustomPartialGroupId;

                RateHelper.GroupPackageItems(package);
                rate.ItemOrderIds = package.Items.Select(i => new RateItemDTO()
                {
                    OrderItemId = i.ItemOrderId,
                    Quantity = i.Quantity
                }).ToList();

                rates.AddRange(new List<RateDTO>() { rate });
            }

            foreach (var rate in rates)
            {
                rate.IsDefault = true;
                rate.IsVisible = true;
                if (previousNumberInBatch.HasValue)
                {
                    rate.NumberInBatch = previousNumberInBatch;
                    previousNumberInBatch = null;
                }
            }

            //save
            var newShippings = new List<OrderShippingInfo>();
            var shippingMethodList = db.ShippingMethods.GetAllAsDto().ToList();
            var lastShippingNumber = oldShippings.Any() ? oldShippings.Max(sh => sh.ShippingNumber) ?? 0 : 0;
            var shippingNumber = lastShippingNumber;
            foreach (var rate in rates)
            {
                log.Debug("store rate, service" + rate.ServiceTypeUniversal
                          + ", package=" + rate.PackageTypeUniversal
                          + ", cost=" + rate.Amount
                          + ", defualt=" + rate.IsDefault
                          + ", visible=" + rate.IsVisible
                          + ", groupId=" + rate.GroupId
                          + ", shipDate=" + rate.ShipDate
                          + ", deliveryDate=" + rate.DeliveryDate
                          + ", daysInfo=" + rate.DeliveryDaysInfo
                          + ", items=" + (rate.ItemOrderIds != null ? String.Join(", ", rate.ItemOrderIds.Select(i => (i.OrderItemId.ToString() + "-" + i.Quantity.ToString())).ToList()) : ""));
                var currentRate = rate;

                shippingNumber++;
                var method = shippingMethodList.FirstOrDefault(m => m.ServiceIdentifier == currentRate.ServiceIdentifier);
                if (method != null)
                {
                    currentRate.DeliveryDays = time.GetBizDaysCount(currentRate.ShipDate,
                        currentRate.DeliveryDate);

                    var shippingInfo = db.OrderShippingInfos.CreateShippingInfo(currentRate,
                        orderId,
                        shippingNumber,
                        method.Id);
                    newShippings.Add(shippingInfo);

                    if (currentRate.ItemOrderIds != null && currentRate.ItemOrderIds.Any())
                    {
                        log.Debug("store partial, items="
                                  +
                                  String.Join(", ",
                                      currentRate.ItemOrderIds.Select(
                                          i => (i.OrderItemId.ToString() + "-" + i.Quantity.ToString())).ToList()));
                        db.ItemOrderMappings.StorePartialShippingItemMappings(shippingInfo.Id,
                            currentRate.ItemOrderIds,
                            items,
                            time.GetAppNowTime());
                    }
                    else
                    {
                        db.ItemOrderMappings.StoreShippingItemMappings(shippingInfo.Id,
                            items,
                            time.GetAppNowTime());
                    }
                }
            }

            foreach (var oldShipping in oldShippings)
            {
                if (oldShipping.ShippingGroupId == RateHelper.CustomPartialGroupId)
                    db.OrderShippingInfos.Remove(oldShipping);
                else  
                    oldShipping.IsActive = false;
            }

            order.ShippingCalculationStatus = (int) ShippingCalculationStatusEnum.FullCalculation;

            db.Commit();

            //Return actual values
            var allShippings = db.OrderShippingInfos.GetByOrderIdAsDto(orderId);
            var results = OrderViewModel.GetShippingOptions(allShippings,
                (MarketType)order.Market,
                order.IsSignConfirmation,
                order.IsInsured,
                isFulfilmentUser,
                showOptionsPrices: true,
                showProviderName: false);

            return CallResult<IList<SelectListShippingOption>>.Success(results);
        }
    }
}