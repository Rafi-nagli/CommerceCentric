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
using Amazon.Model.General;
using Amazon.Web.ViewModels.Html;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.General.Markets;
using Amazon.DTO.Listings;
using Ccen.Web;
using Amazon.Core.Models.Search;

namespace Amazon.Web.ViewModels
{
    public class MailViewModel
    {
        public int MarketplaceCode { get; set; }
        public string OrderID { get; set; }
        public string CustomerOrderId { get; set; }
        public long? OrderEntityId { get; set; }
        public string OrderStatus { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public bool IsPrime { get; set; }

        public bool RequireAmazonProvider { get; set; }

        public int ShipmentProviderId { get; set; }

        public bool HasBatchLabels { get; set; }
        public bool HasMailLabels { get; set; }

        public AddressViewModel ToAddress { get; set; }
        public AddressViewModel FromAddress { get; set; }

        [Required]
        public int? WeightLb { get; set; }
        [Required]
        public double? WeightOz { get; set; }

        public decimal? PackageHeight { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageLength { get; set; }

        public decimal TotalPrice { get; set; }
        public string TotalPriceCurrency { get; set; }

        [Required]
        [Display(Name = "Shipping Method")]
        public int? ShippingMethodSelected { get; set; }
        
        public string Notes { get; set; }
        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Reason")]
        public int? ReasonCode { get; set; }
        
        public bool UpdateAmazon { get; set; }
        public bool CancelCurrentOrderLabel { get; set; }
        public bool ReduceInventory { get; set; }

        public string OrderComment { get; set; }

        public bool IsInsured { get; set; }

        public bool IsSignConfirmation { get; set; }

        public bool IsAddressSwitched { get; set; }
        
        public bool IsPrinted { get; set; }
        public string PrintedLabelUrl { get; set; }
        public string PrintedLabelPath { get; set; }
        public string PrintedTrackingNumber { get; set; }

        public IList<MailItemViewModel> Items { get; set; }

        public List<MessageString> Messages { get; set; }
        
        public override string ToString()
        {
            var itemsString = "";
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    itemsString += "\r\n" + item.ToString();
                }
            }
            
            return "MarketplaceCode=" + MarketplaceCode
                   + ", OrderID=" + OrderID
                   + ", OrderEntityId=" + OrderEntityId
                   + ", WeightLb=" + WeightLb
                   + ", WeightOz=" + WeightOz
                   + ", PackageHeight=" + PackageHeight
                   + ", PackageWidth=" + PackageWidth
                   + ", PackageLength=" + PackageLength
                   + ", TotalPrice=" + TotalPrice
                   + ", ShipmentProviderId=" + ShipmentProviderId
                   + ", ShippingMethodSelected=" + ShippingMethodSelected
                   + ", HasMailLabels=" + HasMailLabels
                   + ", HasBatchLabels=" + HasBatchLabels
                   + ", Notes=" + Notes
                   + ", Instruction=" + Instructions
                   + ", ReasonCode=" + ReasonCode
                   + ", UpdateAmazon=" + UpdateAmazon
                   + ", CancelCurrentOrderLabel=" + CancelCurrentOrderLabel
                   + ", IsInsured=" + IsInsured
                   + ", IsSignConfirmation=" + IsSignConfirmation
                   + ", IsAddressSwitched=" + IsAddressSwitched
                   + ", OrderComment=" + OrderComment
                   + ", Message=" + MessageString.ToString(Messages)
                   + ", IsPrinted=" + IsPrinted
                   + ", PrintedLabelUrl=" + PrintedLabelUrl
                   + ", PrintedTrackingNumber=" + PrintedTrackingNumber
                   + itemsString;
        }

        public MailViewModel()
        {
            MarketplaceCode = (int)MarketplaceType.Amazon;
            ToAddress = new AddressViewModel();
            FromAddress = new AddressViewModel();
            Messages = new List<MessageString>();
        }

        public IList<MessageString> Generate(ILogService log,
            ITime time,
            ILabelService labelService,
            IQuantityManager quantityManager,
            IUnitOfWork db,
            IWeightService weightService,
            IShippingService shippingService,
            bool sampleMode,
            DateTime when,
            long? by)
        {
            var results = new List<MessageString>();

            //Cancel privious
            if (CancelCurrentOrderLabel)
            {
                var cancelResults = CancelCurrentOrderLabels(log,
                    db,
                    labelService,
                    time,
                    OrderEntityId.Value,
                    sampleMode);

                results.AddRange(cancelResults);
            }

            //Print new
            var printResult = GenerateLabel(db,
                labelService,
                weightService,
                shippingService,
                this,
                time.GetAppNowTime(),
                by);

            if (printResult.Success)
            {
                //Remove from batch if unshipped
                var wasRemoved = RemoveFromBatchIfUnshipped(log,
                    db,
                    OrderEntityId.Value);
                if (wasRemoved)
                {
                    results.Add(MessageString.Success("Order was removed from batch"));
                }
            }

            //Processing print result
            Messages.AddRange(printResult.Messages.Select(m => MessageString.Error(m.Text)).ToList());
            IsPrinted = printResult.Success;
            PrintedLabelPath = printResult.Url;
            PrintedLabelUrl = UrlHelper.GetPrintLabelPathById(printResult.PrintPackId);
            PrintedTrackingNumber = printResult.TrackingNumber;
            
            if (ReduceInventory)
            {
                AddQuantityOperation(log,
                    db,
                    quantityManager,
                    OrderID,
                    Items.Select(i => i.GetItemDto()).ToList(),
                    time,
                    by);

                results.Add(MessageString.Success("Inventory quantity was adjusted"));
            }

            //Add to track table
            if (ReasonCode == (int)MailLabelReasonCodes.ReturnLabelReasonCode)
            {
                AddToOrderTracking(log,
                    db,
                    OrderID,
                    printResult.TrackingNumber,
                    printResult.Carrier,
                    when,
                    by);

                results.Add(MessageString.Success("Tracking number was added to Track Orders list"));
            }

            if (OrderEntityId.HasValue 
                && !String.IsNullOrEmpty(OrderComment))
            {
                db.OrderComments.Add(new OrderComment()
                {
                    OrderId = OrderEntityId.Value,
                    Message = OrderComment, // "[System] Returned",
                    Type = (int)CommentType.ReturnExchange,
                    CreateDate = when,
                    CreatedBy = by
                });
                db.Commit();

                results.Add(MessageString.Success("Comment was added"));
            }

            if (printResult.Success && !string.IsNullOrEmpty(printResult.Url))
            {
                log.Info(string.Format("LabelPath for {0} order: {1}",
                    string.IsNullOrEmpty(OrderID) ? "Ebay" : OrderID, printResult.Url));
            }


            if (printResult.Success && UpdateAmazon)
            {
                results.Add(MessageString.Success("Send the order update to the market was scheduled"));
            }

            return results;
        }

        
        public static void AddQuantityOperation(ILogService log,
            IUnitOfWork db,
            IQuantityManager quantityManager,
            string orderId,
            IList<DTOOrderItem> items,
            ITime time,
            long? by)
        {
            var operationDto = new QuantityOperationDTO();
            operationDto.Type = (int)QuantityOperationType.FromMailPage;
            operationDto.OrderId = orderId;
            operationDto.Comment = String.Empty;
            operationDto.QuantityChanges = new List<QuantityChangeDTO>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                    {
                        StyleId = item.StyleEntityId ?? 0,
                        StyleItemId = item.StyleItemId ?? 0,
                        Quantity = item.Quantity,
                    });
                }
            }

            quantityManager.AddQuantityOperation(db,
                operationDto,
                time.GetAppNowTime(),
                by);
        }

        public static void FillItemsWithAdditionalInfo(IUnitOfWork db,
            IWeightService weightService,
            string orderNumber,
            IList<DTOOrderItem> orderItems)
        {
            var orderInfo = db.ItemOrderMappings.GetOrderWithItems(weightService, orderNumber, true, includeSourceItems:false);
            var sourceOrderItems = orderInfo.Items;

            foreach (var orderItem in orderItems)
            {
                var sourceOrderItem = sourceOrderItems.FirstOrDefault(i => i.ItemOrderId == orderItem.ItemOrderId);
                if (sourceOrderItem != null)
                {
                    orderItem.SourceItemOrderId = sourceOrderItem.SourceItemOrderIdentifier;

                    if (orderItem.StyleItemId == sourceOrderItem.StyleItemId
                        || orderItem.StyleEntityId == sourceOrderItem.StyleId)
                    {
                        orderItem.Title = sourceOrderItem.Title;
                        orderItem.StyleSize = sourceOrderItem.StyleSize;
                        orderItem.ItemPicture = sourceOrderItem.ItemPicture;
                        if (String.IsNullOrEmpty(orderItem.ItemPicture))
                            orderItem.ItemPicture = sourceOrderItem.StyleImage;
                        orderItem.Locations = sourceOrderItem.Locations;
                    }
                    if (orderItem.StyleItemId != sourceOrderItem.StyleItemId)
                    {
                        if (orderItem.StyleItemId.HasValue)
                        {
                            var styleItem = db.StyleItems.Get(orderItem.StyleItemId.Value);
                            if (styleItem != null)
                                orderItem.StyleSize = styleItem.Size;
                        }
                    }
                    if (orderItem.StyleEntityId != sourceOrderItem.StyleId)
                    {
                        if (orderItem.StyleEntityId.HasValue)
                        {
                            var style = db.Styles.Get(orderItem.StyleEntityId.Value);
                            var locations =
                                db.StyleLocations.GetByStyleIdsAsDto(new List<long>() {orderItem.StyleEntityId.Value});
                            if (style != null)
                            {
                                orderItem.Title = style.Name;
                                orderItem.ItemPicture = style.Image;
                            }
                            orderItem.Locations = locations;
                        }
                    }
                    orderItem.ItemPicture = UrlHelper.GetThumbnailUrl(orderItem.ItemPicture,
                        0,
                        75,
                        false,
                        ImageHelper.NO_IMAGE_URL,
                        convertToFullUrl: true);
                }
            }
        }

        public static void AddToOrderTracking(
            ILogService log,
            IUnitOfWork db,
            string orderId,
            string trackingNumber,
            string carrier,
            DateTime when,
            long? by)
        {
            var order = db.Orders.GetByOrderIdAsDto(orderId);

            if (order != null || !String.IsNullOrEmpty(trackingNumber))
            db.TrackingOrders.Add(new TrackingOrder()
            {
                TrackingNumber = trackingNumber,
                Carrier = carrier,
                Comment = "Return label",
                CreateDate = when,
                CreatedBy = by
            });
        }

        public static bool ValidateReasonCode(IUnitOfWork db, 
            long orderId,
            int reasonCode)
        {
            var mailLabelQuery = from m in db.MailLabelInfos.GetAll()
                join o in db.Orders.GetAll() on m.OrderId equals o.Id
                where o.Id == orderId
                select m;
            
            var mailLabel = mailLabelQuery
                .OrderByDescending(m => m.CreateDate)
                .FirstOrDefault();
            if (mailLabel != null)
            {
                return mailLabel.ReasonId != reasonCode;
            }
            return true;
        }

        public static bool ValidateCanceled(IUnitOfWork db, long orderId, DateTime now, out DateTime? minLabelToCancelDate)
        {
            var fromDate = db.Dates.GetMinBizDayByOffset(now, -1);
            fromDate = fromDate.AddHours(now.Hour);
            fromDate = fromDate.AddMinutes(now.Minute);
            fromDate = fromDate.AddSeconds(now.Second);
            
            var shippings = GetOrdersToCancel(db, orderId);
            minLabelToCancelDate = shippings.Min(sh => sh.LabelPurchaseDate);

            return !minLabelToCancelDate.HasValue || minLabelToCancelDate >= fromDate;
        }

        private static IList<OrderShippingInfoDTO> GetOrdersToCancel(IUnitOfWork db, long orderId)
        {
            var cancelTrackings = new List<OrderShippingInfoDTO>();
            var order = db.Orders.GetByOrderIdAsDto(orderId);

            List<OrderShippingInfoDTO> labels = db.OrderShippingInfos
                .GetByOrderIdAsDto(order.Id)
                .Where(sh => !String.IsNullOrEmpty(sh.TrackingNumber))
                .ToList();
                
            labels.AddRange(db.MailLabelInfos.GetByOrderIdsAsDto(new[] {order.Id}).ToList());

            var lastNotCancelledLabel = labels.OrderByDescending(l => l.LabelPurchaseDate)
                .Where(l => !l.CancelLabelRequested && !l.LabelCanceled)
                .ToList();

            if (lastNotCancelledLabel.Any())
            {
                IList<OrderShippingInfoDTO> shippingsToCancel = new List<OrderShippingInfoDTO>() { lastNotCancelledLabel[0] };
                if (RateHelper.IsMultiPackageGroup(lastNotCancelledLabel[0].ShippingGroupId))
                {
                    shippingsToCancel = lastNotCancelledLabel
                        .Where(sh => sh.ShippingGroupId == lastNotCancelledLabel[0].ShippingGroupId)
                        .ToList();
                }

                foreach (var toCancel in shippingsToCancel)
                {
                    cancelTrackings.Add(new OrderShippingInfoDTO()
                    {
                        Id = toCancel.Id,
                        LabelFromType = toCancel.LabelFromType,
                        ShipmentProviderType = toCancel.ShipmentProviderType,
                        TrackingNumber = toCancel.TrackingNumber,
                        StampsTxId = toCancel.StampsTxId,
                        LabelPurchaseDate = toCancel.LabelPurchaseDate,
                    });
                }
            }

            return cancelTrackings;
        }

        public static bool RemoveFromBatchIfUnshipped(
            ILogService log,
            IUnitOfWork db,
            long orderId)
        {
            var order = db.Orders.GetById(orderId);
            if (order.BatchId != null)
            {
                var shippings = db.OrderShippingInfos.GetByOrderId(order.Id);
                if (shippings.All(sh => String.IsNullOrEmpty(sh.LabelPath)))
                {
                    order.BatchId = null;
                    db.Commit();

                    return true;
                }
            }

            return false;
        }

        public static IList<MessageString> CancelCurrentOrderLabels(
            ILogService log,
            IUnitOfWork db, 
            ILabelService labelService,
            ITime time,
            long orderId,
            bool sampleMode)
        {
            var cancelTrackings = GetOrdersToCancel(db, orderId);

            var results = new List<MessageString>();
            
            foreach (var tracking in cancelTrackings)
            {
                MailLabelInfo mailInfo = null;
                OrderShippingInfo shippingInfo = null;
                if (tracking.LabelFromType == (int)LabelFromType.Mail)
                {
                    mailInfo = db.MailLabelInfos.Get(tracking.Id);
                    if (mailInfo != null)
                    {
                        mailInfo.CancelLabelRequested = true;
                    }
                }
                else
                {
                    shippingInfo = db.OrderShippingInfos.Get(tracking.Id);
                    if (shippingInfo != null)
                    {
                        shippingInfo.CancelLabelRequested = true;
                    }
                }

                try
                {
                    string shipmentIdentifier = tracking.StampsTxId;
                    if (tracking.ShipmentProviderType == (int) ShipmentProviderType.Stamps
                        || tracking.ShipmentProviderType == (int) ShipmentProviderType.StampsPriority)
                        shipmentIdentifier = tracking.TrackingNumber;

                    var result = labelService.CancelLabel((ShipmentProviderType)tracking.ShipmentProviderType,
                        shipmentIdentifier, 
                        sampleMode);

                    if (result.IsSuccess)
                    {
                        results.Add(MessageString.Success(tracking.TrackingNumber + " (cancelled)" + ", OrderId=" + orderId));
                        if (tracking.LabelFromType == (int)LabelFromType.Mail)
                        {
                            if (mailInfo != null)
                            {
                                mailInfo.LabelCanceled = true;
                                mailInfo.LabelCanceledDate = time.GetAppNowTime();
                            }
                        }
                        else
                        {
                            if (shippingInfo != null)
                            {
                                shippingInfo.LabelCanceled = true;
                                shippingInfo.LabelCanceledDate = time.GetAppNowTime();
                            }
                        }
                    }
                    else
                    {
                        results.Add(MessageString.Error(tracking.TrackingNumber + " (cancellation was failed with error: " + result.Message + ")"));
                    }
                }
                catch (Exception ex)
                {
                    results.Add(MessageString.Error(tracking.TrackingNumber + " (cancellation was failed with error: " + ExceptionHelper.GetMostDeeperException(ex) + ")"));
                }
            }
            db.Commit();

            if (!cancelTrackings.Any())
                results.Add(MessageString.Success("No tracking numbers to cancel"));

            return results;
        }



        #region Get Rates

        public CallResult<List<ShippingMethodViewModel>> GetShippingOptionsModel(IUnitOfWork db,
            ITime time,
            ILogService log,
            IShipmentApi rateProvider,
            IShippingService shippingService,
            IWeightService weightService)
        {
            var result = new CallResult<List<ShippingMethodViewModel>>();

            if (String.IsNullOrEmpty(OrderID))
            {
                result.Status = CallStatus.Fail;
                result.Message = "Empty Order Id";
                return result;
            }

            var fromAddress = FromAddress.GetAddressDto();
            var toAddress = ToAddress.GetAddressDto();

            if (AddressHelper.IsEmpty(fromAddress)
                || AddressHelper.IsEmpty(toAddress))
            {
                result.Status = CallStatus.Fail;
                result.Message = "Empty from/to address";
            }

            var orders = db.ItemOrderMappings.GetFilteredOrdersWithItems(weightService,
                new OrderSearchFilter()
                {
                    Market = MarketType.None,
                    EqualOrderNumber = OrderID,
                    IgnoreBatchFilter = true,
                    IncludeNotify = false,
                    UnmaskReferenceStyles = false,
                    IncludeSourceItems = true,
                });

            if (!orders.Any())
            {
                result.Status = CallStatus.Fail;
                result.Message = "Cannot find order Id";
                return result;
            }

            //NOTE: no need to use Items from model, we always buy label for all items, only needs a custom weight
            var orderItems = OrderHelper.BuildAndGroupOrderItems(orders.SelectMany(o => o.Items).ToList());
            orderItems = OrderHelper.GroupBySourceItemOrderId(orderItems);

            var sourceOrderItems = OrderHelper.BuildAndGroupOrderItems(orders.SelectMany(o => o.SourceItems).ToList());
            sourceOrderItems = OrderHelper.GroupBySourceItemOrderId(sourceOrderItems);

            var shipDate = db.Dates.GetOrderShippingDate(null);
            var mainOrder = orders.FirstOrDefault();

            result = MailViewModel.GetShippingOptionsWithRate(db,
                log,
                time,
                rateProvider,
                shippingService,
                fromAddress,
                toAddress,
                shipDate,
                WeightLb ?? 0,
                (decimal)(WeightOz ?? 0),
                new ItemPackageDTO()
                {
                    PackageLength = PackageLength,
                    PackageWidth = PackageWidth,
                    PackageHeight = PackageHeight,
                },
                0,
                new OrderRateInfo()
                {
                    OrderNumber = mainOrder.OrderId,

                    Items = orderItems,
                    SourceItems = sourceOrderItems,

                    EstimatedShipDate = ShippingUtils.AlignMarketDateByEstDayEnd(mainOrder.LatestShipDate, (MarketType)mainOrder.Market),
                    TotalPrice = orders.Sum(o => o.TotalPrice),
                    Currency = mainOrder.TotalPriceCurrency,
                });

            return result;
        }

        public static IList<ShippingMethodViewModel> GetShippingOptions(IUnitOfWork db,
            string countryTo,
            string countryFrom,
            int weightLb,
            decimal weightOz,
            ShipmentProviderType providerType)
        {
            var methodList = GetShippingMethods(db,
                countryFrom,
                countryTo,
                weightLb,
                weightOz,
                providerType);

            return methodList.Select(m => new ShippingMethodViewModel()
            {
                Id = m.Id,
                Name = m.Name
            }).ToList();
        } 
        
        public static CallResult<List<ShippingMethodViewModel>> GetShippingOptionsWithRate(IUnitOfWork db, 
            ILogService log,
            ITime time,
            IShipmentApi rateProvider,
            IShippingService shippingService,
            AddressDTO fromAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            int weightLb,
            decimal weightOz,
            ItemPackageDTO overridePackageSize,
            decimal insuredValue,
            OrderRateInfo orderInfo)
        {
            var result = new CallResult<List<ShippingMethodViewModel>>();
            var pickupAddress = fromAddress;

            var rateResult = rateProvider.GetAllRate(
                 fromAddress,
                 pickupAddress,
                 toAddress,
                 shipDate,
                 (double)(weightLb * 16 + weightOz),
                 overridePackageSize,
                 insuredValue,
                 false,
                 orderInfo,
                 RetryModeType.Fast);

            if (rateResult.Result != GetRateResultType.Success)
            {
                result.Status = CallStatus.Fail;
                result.Message = rateResult.Message;
                return result;
            }

            var methodList = GetShippingMethods(db,
                fromAddress.FinalCountry,
                toAddress.FinalCountry,
                weightLb,
                weightOz,
                rateProvider.Type);


            result.Data = new List<ShippingMethodViewModel>();
            result.Status = CallStatus.Success;

            foreach (var method in methodList)
            {
                var rate = rateResult.Rates.FirstOrDefault(r => r.ServiceIdentifier == method.ServiceIdentifier);

                if (rate != null)
                {
                    //var deliveryDays = time.GetBizDaysCount(rate.ShipDate, rate.DeliveryDate);
                    var deliveryDaysInfo = rate.DeliveryDaysInfo;
                    string providerPrefix = "";
                    switch ((ShipmentProviderType)method.ShipmentProviderType)
                    {
                        case ShipmentProviderType.Amazon:
                            providerPrefix = "AMZ ";
                            break;
                        case ShipmentProviderType.Stamps:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.Dhl:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.DhlECom:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.IBC:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.SkyPostal:
                            providerPrefix = "";
                            break;
                    }

                    var adjustedAmount = shippingService.ApplyCharges(method.Id, rate.Amount);

                    result.Data.Add(new ShippingMethodViewModel()
                    {
                        Id = method.Id,
                        ProviderPrefix = providerPrefix,
                        Carrier = method.CarrierName,
                        Name = ShippingUtils.PrepareMethodNameToDisplay(method.Name, deliveryDaysInfo),
                        Rate = adjustedAmount,
                    });
                }
            }

            if (result.Data != null)
            {
                result.Data = result.Data.OrderBy(r => r.Rate).ToList();
            }

            return result;
        }

        private static IList<ShippingMethodDTO> GetShippingMethods(IUnitOfWork db,
            string countryTo,
            string countryFrom,
            int? weightLb,
            decimal? weightOz,
            ShipmentProviderType providerType)
        {
            var isInternational = countryTo != countryFrom;

            var oz = (double)(weightOz + weightLb * 16);

            var providerList = new List<int>() {(int) providerType};
            if (providerType == ShipmentProviderType.Stamps)
                providerList.Add((int)ShipmentProviderType.StampsPriority);

            return db.ShippingMethods
                .GetAllAsDto()
                .Where(sh => sh.IsActive
                             && providerList.Contains(sh.ShipmentProviderType)
                             && sh.IsInternational == isInternational
                             && (!sh.MaxWeight.HasValue || oz < sh.MaxWeight || sh.MaxWeight == 0))
                .ToList();

            //if (!ShippingUtils.IsInternational(countryTo)
            //    && !ShippingUtils.IsInternational(countryFrom) 
            //    && weightLb == 0
            //    && weightOz == 0)
            //{
            //    return db.ShippingMethods.GetAllAsDto().ToList();
            //}
            //if (!ShippingUtils.IsInternational(countryTo)
            //    && !ShippingUtils.IsInternational(countryFrom)) //(string.IsNullOrEmpty(countryTo) || countryTo == "US") && (string.IsNullOrEmpty(countryFrom) || countryFrom == "US"))
            //{
            //    return db.ShippingMethods
            //        .GetAllAsDto()
            //        .Where(m => (!(weightLb > 0) && weightOz <= 16) || m.AllowOverweight || m.IsInternational)
            //        .ToList();
            //}

            ////TODO: m.b. add country correction / isInternational logic
            //return db.ShippingMethods.GetAllAsDto().Where(m => m.IsInternational == ((countryFrom != countryTo) || string.IsNullOrEmpty(countryFrom) || string.IsNullOrEmpty(countryTo))
            //    && ((!(weightLb > 0) && weightOz <= 16) || m.AllowOverweight || ((countryFrom != countryTo) || string.IsNullOrEmpty(countryFrom) || string.IsNullOrEmpty(countryTo))))
            //    .ToList();
        }
        #endregion



        public static MailViewModel GetByOrderId(IUnitOfWork db,
            IWeightService weightService,
            string id)
        {
            var order = db.Orders.GetMailDTOByOrderId(weightService, id);
            if (order == null)
            {
                return new MailViewModel
                {
                    ToAddress = new AddressViewModel
                    {
                        Address1 = String.Empty,
                        Address2 = String.Empty,
                        FullName = String.Empty,
                        City = String.Empty,
                        USAState = String.Empty,
                        NonUSAState = String.Empty,
                        Country = String.Empty,
                        Zip = String.Empty,
                        Phone = String.Empty,
                        Email = String.Empty,
                        ShipDate = null
                    },
                    Items = new List<MailItemViewModel>(),
                    MarketplaceCode = 1,
                    Notes = String.Empty,
                    OrderStatus = String.Empty,
                    OrderID = String.Empty,
                    OrderEntityId = null,
                    IsPrime = false,
                    RequireAmazonProvider = false,

                    ShipmentProviderId = (int)ShipmentProviderType.Stamps,
                    HasBatchLabels = false,
                    HasMailLabels = false,

                    WeightLb = null,
                    WeightOz = null,
                };
            }
            else
            {
                return new MailViewModel
                {
                    ToAddress = new AddressViewModel
                    {
                        Address1 = order.ToAddress.FinalAddress1,
                        Address2 = order.ToAddress.FinalAddress2,
                        FullName = order.ToAddress.FinalFullName,
                        City = order.ToAddress.FinalCity,
                        USAState = StringHelper.ToUpper(order.ToAddress.FinalState),
                        NonUSAState = StringHelper.ToUpper(order.ToAddress.FinalState),
                        Country = order.ToAddress.FinalCountry,
                        Zip = order.ToAddress.FinalZip,
                        ZipAddon = order.ToAddress.FinalZipAddon,
                        Phone = order.ToAddress.FinalPhone,
                        Email = order.ToAddress.BuyerEmail,
                        ShipDate = order.ToAddress.ShipDate
                    },
                    MarketplaceCode = 1,
                    Notes = "",

                    Market = order.Market,
                    MarketplaceId = order.MarketplaceId,

                    OrderStatus = order.OrderStatus,
                    OrderEntityId = order.OrderEntityId,
                    OrderID = order.OrderId,
                    CustomerOrderId = order.CustomerOrderId,
                    IsPrime = order.OrderType == (int)OrderTypeEnum.Prime,
                    RequireAmazonProvider = ShippingUtils.RequireAmazonProvider(order.OrderType,
                        order.Market,
                        order.ToAddress.FinalCountry,
                        order.SourceShippingService),
                
                    ShipmentProviderId = order.ShipmentProviderType,
                    HasBatchLabels = order.Labels.Any(l => l.LabelFromType == (int)LabelFromType.Batch),
                    HasMailLabels = order.Labels.Any(l => l.LabelFromType == (int)LabelFromType.Mail),
                    
                    WeightLb = order.WeightLb,
                    WeightOz = order.WeightOz,

                    PackageLength = order.PackageLength,
                    PackageWidth = order.PackageWidth,
                    PackageHeight = order.PackageHeight,

                    TotalPrice = order.TotalPrice,
                    TotalPriceCurrency = order.TotalPriceCurrency,

                    Items = order.Items.Select(i => new MailItemViewModel(i)).ToList(),

                    IsInsured = order.IsInsured
                };
            }
        }

        public static IList<MessageString> ValidateQuickReturnLabel(IUnitOfWork db,
            string orderNumber)
        {
            var messages = new List<MessageString>();
            var order = db.Orders.GetAll().FirstOrDefault(o => o.AmazonIdentifier == orderNumber || o.CustomerOrderId == orderNumber);
            if (ShippingUtils.IsInternational(order.ShippingCountry))
            {
                messages.Add(MessageString.Warning("The International return label cannot be generated automatically"));
            }
            var existReturnLabel = db.MailLabelInfos.GetAllAsDto()
                .Where(m => m.OrderId == order.Id
                    && m.MailReasonId == (int)MailLabelReasonCodes.ReturnLabelReasonCode).ToList();
            if (existReturnLabel.Any())
            {
                messages.Add(MessageString.Warning("Order already has return label"));
            }
            return messages;
        }

        public static ShippingMethodViewModel GetQuickPrintLabelRate(IUnitOfWork db,
            IDbFactory dbFactory,
            IServiceFactory serviceFactory,
            IShippingService shippingService,
            CompanyDTO company,
            ILogService log,
            ITime time,
            IWeightService weightService,
            string orderNumber)
        {
            var companyAddress = new CompanyAddressService(company);

            var model = MailViewModel.GetByOrderId(db, weightService, orderNumber);
            model.IsAddressSwitched = true;
            model.FromAddress = model.ToAddress;
            model.ToAddress = MailViewModel.GetFromAddress(companyAddress.GetReturnAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon);
            model.ShipmentProviderId = (int)ShipmentProviderType.Stamps;
            model.ReasonCode = (int)MailLabelReasonCodes.ReturnLabelReasonCode;

            var rateProvider = serviceFactory.GetShipmentProviderByType((ShipmentProviderType)model.ShipmentProviderId,
                 log,
                 time,
                 dbFactory,
                 weightService,
                 company.ShipmentProviderInfoList,
                 null,
                 null,
                 null,
                 null);

            var shippingOptionsResult = model.GetShippingOptionsModel(db, time, log, rateProvider, shippingService, weightService);
            ShippingMethodViewModel chipestRate = null;
            if (shippingOptionsResult.IsSuccess)
            {
                chipestRate = shippingOptionsResult.Data.OrderBy(o => o.Rate).FirstOrDefault();
            }
            return chipestRate;
        }

        public static PrintLabelResult GenerateLabel(
            IUnitOfWork db, 
            ILabelService labelService,
            IWeightService weightService,
            IShippingService shippingService,
            MailViewModel model,
            DateTime when,
            long? by)
        {
            var shippingMethod = db.ShippingMethods.GetByIdAsDto(model.ShippingMethodSelected.Value);

            var orderItems = model.Items.Select(i => i.GetItemDto()).ToList() ?? new List<DTOOrderItem>();
            //Fill with additional data
            MailViewModel.FillItemsWithAdditionalInfo(db, weightService, model.OrderID, orderItems);

            var mailInfo = new MailLabelDTO
            {
                FromAddress = model.FromAddress.GetAddressDto(),
                ToAddress = model.ToAddress.GetAddressDto(),

                Notes = model.Notes,
                Instructions = model.Instructions,
                OrderId = model.OrderID,
                WeightLb = model.WeightLb,
                WeightOz = model.WeightOz,

                PackageHeight = model.PackageHeight,
                PackageLength = model.PackageLength,
                PackageWidth = model.PackageWidth,

                IsAddressSwitched = model.IsAddressSwitched,
                IsUpdateRequired = model.UpdateAmazon,
                IsCancelCurrentOrderLabel = model.CancelCurrentOrderLabel,
                
                IsInsured = model.IsInsured,
                IsSignConfirmation = model.IsSignConfirmation,
                TotalPrice = model.TotalPrice,
                TotalPriceCurrency = model.TotalPriceCurrency,

                ShippingMethod = shippingMethod,
                ShipmentProviderType = shippingMethod.ShipmentProviderType,

                Items = orderItems,

                MarketplaceCode = model.MarketplaceCode,
                Reason = model.ReasonCode ?? 0,

                BoughtInTheCountry = MarketBaseHelper.GetMarketCountry((MarketType)model.Market, model.MarketplaceId),
            };

            return labelService.PrintMailLabel(db,
                shippingService,
                mailInfo, 
                when,
                by,
                AppSettings.LabelDirectory, 
                AppSettings.TemplateDirectory,
                AppSettings.IsSampleLabels);
        }

        private static Dictionary<int, string> _reasonCodeNames = new Dictionary<int, string>()
        {
            { (int)MailLabelReasonCodes.ReplacementLabelCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReplacementLabelCode) },
            { (int)MailLabelReasonCodes.ReplacingLostDamagedReasonCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReplacingLostDamagedReasonCode) },
            { (int)MailLabelReasonCodes.ResendingOrderCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ResendingOrderCode)},
            { (int)MailLabelReasonCodes.ExchangeCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ExchangeCode) },
            { (int)MailLabelReasonCodes.ReturnLabelReasonCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReturnLabelReasonCode) },
            { (int)MailLabelReasonCodes.ManualLabelCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ManualLabelCode)},
            { (int)MailLabelReasonCodes.OtherCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.OtherCode)}
        };

        public static string GetReasonName(int reasonCode)
        {
            if (_reasonCodeNames.ContainsKey(reasonCode))
                return _reasonCodeNames[reasonCode];
            return "";
        }

        public static SelectList Reasons
        {
            get
            {
                return new SelectList(_reasonCodeNames.ToList(), "Key", "Value");
            }
        }

        public static SelectList ShippingProviderList
        {
            get
            {
                return new SelectList(ShipmentProviderHelper.GetMainProviderList(), "Key", "Value");
            }
        }

        public static IList<SelectListItemEx> ShippingServiceList
        {
            get
            {
                return ShipmentProviderHelper.GetAllShippingServiceList().Select(m => new SelectListItemEx()
                {
                    Text = m.Name,
                    Value = m.Id.ToString(),
                    ParentValue = m.ShipmentProviderType.ToString()
                }).ToList();
            }
        }

        public static AddressViewModel GetFromAddress(AddressDTO address, MarketplaceType type)
        {
            var model = new AddressViewModel
            {
                FullName = address.FullName,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = address.City,
                USAState = address.State,
                NonUSAState = address.State,
                Country = address.Country,// ?? Constants.DefaultCountryCode,
                Zip = address.Zip,
                ZipAddon = address.ZipAddon,
                Phone = address.Phone,
                Email = address.BuyerEmail,
                IsVerified = true
            };
            return model;
        }
    }
}