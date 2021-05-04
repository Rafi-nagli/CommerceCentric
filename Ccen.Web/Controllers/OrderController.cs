using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Implementation.Validation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Pages;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class OrderController : BaseController
    {
        public override string TAG
        {
            get { return "OrderController."; }
        }

        public virtual ActionResult Stats()
        {
            LogI("Stats");

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            var model = OrdersStatViewModel.Get(Db, marketplaceManager);
            return View(model);
        }

        public virtual ActionResult Orders(string orderId)
        {
            LogI("Orders");

            var model = new OrderPageViewModel
            {
                DefaultMarket = MarketHelper.DefaultUIMarket,
                DefaultMarketplaceId = MarketHelper.DefaultUIMarketplaceId,
                SearchOrderId = orderId,
            };

            if (AccessManager.Company.ShortName == "PA")
            {
                model.DefaultDropShipperId = DSHelper.DefaultPAId;
            }

            if (AccessManager.Company.ShortName == "MBG")
            {
                model.DefaultDropShipperId = DSHelper.DefaultMBGId;
            }

            return View(model);
        }

        public virtual ActionResult GetSearchHistory()
        {
            return Json(OrderPageViewModel.GetLastOrderSearchList(Db,
                AccessManager.UserId), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult FBAOrders(string market)
        {
            LogI("FBAOrders");
            ViewBag.Title = String.IsNullOrEmpty(market) || market == ((int)MarketType.Amazon).ToString() ? "FBA Orders" : "WFS Orders";
            var model = new FBAOrderPageViewModel();
            model.Market = String.IsNullOrEmpty(market) || market == ((int)MarketType.Amazon).ToString() ? MarketType.Amazon : MarketType.Walmart;
            model.Init(Db);
            return View(model);
        }

        public virtual ActionResult PendingOrders()
        {
            LogI("PendingOrders");

            return View(new OrderPageViewModel
            {
                DefaultMarket = MarketHelper.DefaultUIMarket,
                DefaultMarketplaceId = MarketHelper.DefaultUIMarketplaceId,
            });
        }

        public virtual ActionResult EBayOrders()
        {
            LogI("EBayOrders");

            return View("Orders", new OrderPageViewModel
            {
                DefaultMarket = MarketType.eBay,
                DefaultMarketplaceId = "",
            });
        }

        public virtual ActionResult MagentoOrders()
        {
            LogI("MagentoOrders");

            return View("Orders", new OrderPageViewModel
            {
                DefaultMarket = MarketType.Magento,
                DefaultMarketplaceId = "",
            });
        }

        public virtual ActionResult WalmartOrders()
        {
            LogI("WalmartOrders");

            return View("Orders", new OrderPageViewModel
            {
                DefaultMarket = MarketType.Walmart,
                DefaultMarketplaceId = "",
            });
        }

        public virtual ActionResult WalmartCAOrders()
        {
            LogI("WalmartCAOrders");

            return View("Orders", new OrderPageViewModel
            {
                DefaultMarket = MarketType.WalmartCA,
                DefaultMarketplaceId = "",
            });
        }

        public virtual ActionResult JetOrders()
        {
            LogI("JetOrders");

            return View("Orders", new OrderPageViewModel
            {
                DefaultMarket = MarketType.Jet,
                DefaultMarketplaceId = "",
            });
        }

        public virtual ActionResult SecondDayOrders()
        {
            LogI("SecondDayOrders");
            return View();
        }

        public virtual ActionResult GetFBAOrders([DataSourceRequest] DataSourceRequest request,
            string dateFrom,
            string dateTo, string market)
        {
            LogI("GetFBAOrders, DateFrom=" + dateFrom + ", DateTo=" + dateTo);
            var m = String.IsNullOrEmpty(market) || market==((int)MarketType.Amazon).ToString() ? MarketType.Amazon : MarketType.Walmart;
            //1. Let user to select timeframe and page should show orders between selected dates. 
            //By default it should show orders from 10am. So at 9pm it will sow orders from 10am-9pm, at 9am from 10 am previous day till 9am today.
            var model = new OrderSearchFilterViewModel
            {
                FulfillmentChannel = FulfillmentChannelTypeEx.AFN,
                OrderStatus = OrderStatusEnumEx.AllUnshippedWithShipped,
                //Set time to 10AM
                DateFrom = dateFrom.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateFrom, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                        .AddHours(10)
                    : null,
                //Set time to 10AM
                DateTo = dateTo.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateTo, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                        .AddHours(10)
                    : null,
                Market = m
            };

            var items = OrderViewModel.GetFilteredForDisplay(Db,
                LogService,
                WeightService,
                model,
                AccessManager.IsFulfilment,
                resetTime: false);

            var dataSource = items.Items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetWFSOrders([DataSourceRequest] DataSourceRequest request,
            string dateFrom,
            string dateTo)
        {
            LogI("GetFBAOrders, DateFrom=" + dateFrom + ", DateTo=" + dateTo);

            //1. Let user to select timeframe and page should show orders between selected dates. 
            //By default it should show orders from 10am. So at 9pm it will sow orders from 10am-9pm, at 9am from 10 am previous day till 9am today.
            var model = new OrderSearchFilterViewModel
            {
                FulfillmentChannel = FulfillmentChannelTypeEx.AFN,
                OrderStatus = OrderStatusEnumEx.AllUnshippedWithShipped,
                //Set time to 10AM
                DateFrom = dateFrom.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateFrom, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                        .AddHours(10)
                    : null,
                //Set time to 10AM
                DateTo = dateTo.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateTo, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                        .AddHours(10)
                    : null,
                Market = MarketType.Walmart
            };

            var items = OrderViewModel.GetFilteredForDisplay(Db,
                LogService,
                WeightService,
                model,
                AccessManager.IsFulfilment,
                resetTime: false);

            var dataSource = items.Items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        [Compress]
        public virtual ActionResult GetAllUnshippedInfo()
        {
            //NOTE: Not USED, for future refactoring!

            LogI("GetAllUnshippedInfo");

            var items = OrderInfoViewModel.GetAll(Db, LogService, WeightService, null);

            return Json(ValueResult<IList<OrderInfoViewModel>>.Success("", items), 
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetOrderById(long orderId)
        {
            var rowOrderDto = Db.ItemOrderMappings.GetOrderWithItems(WeightService, orderId, false, true, unmaskReferenceStyles: true); //NOTE: Unmask for display
            var rowModel = new OrderViewModel(rowOrderDto, AccessManager.IsFulfilment);
            rowModel.Items = rowOrderDto.Items.Select(i =>
                    new OrderItemViewModel(i,
                        rowOrderDto.OnHold,
                        ShippingUtils.IsOrderPartial(rowOrderDto.OrderStatus))).ToList();

            return JsonGet(ValueResult<OrderViewModel>.Success("", rowModel));
        }

        [Compress]
        public virtual ActionResult GetOrders(GridRequest request,
            int? market,
            string marketplaceId,
            string orderStatus,
            string shippingStatus,
            string dateFrom,
            string dateTo,
            string buyerName,
            string orderNumber,
            long? batchId,
            long? dropShipperId,
            string styleId,
            long? styleItemId)
        {
            LogI("Begin GetOrders, market=" + market
                + ", marketplaceId=" + marketplaceId
                + ", orderStatus=" + orderStatus
                + ", shipppingStatus=" + shippingStatus
                + ", DateFrom=" + dateFrom
                + ", DateTo=" + dateTo
                + ", BuyerName=" + buyerName
                + ", OrderNumber=" + orderNumber
                + ", BatchId=" + batchId
                + ", dropShipperId=" + dropShipperId
                + ", StyleId=" + styleId
                + ", StyleItemId=" + styleItemId);

            bool excludeWithLabels = false;
            bool includeForceVisible = false;
            string[] orderStatusList = null;
            if (!String.IsNullOrEmpty(orderStatus))
                orderStatusList = new[] { orderStatus };
            //Exclude canceled from batch on UI


            if (batchId == 0)
                batchId = null;

            if (orderStatus == null)
            {
                if (batchId != null)
                {
                    orderStatusList = OrderStatusEnumEx.AllUnshippedWithShipped;
                }
                else
                {
                    orderStatusList = OrderStatusEnumEx.AllUnshipped;
                    excludeWithLabels = true;
                    includeForceVisible = true;
                }
            }

            buyerName = StringHelper.TrimWhitespace(buyerName);
            orderNumber = StringHelper.TrimWhitespace(orderNumber);
            styleId = StringHelper.TrimWhitespace(styleId);

            var model = new OrderSearchFilterViewModel
            {
                //FulfillmentChannel = FulfillmentChannelTypeEx.MFN,
                Market = (MarketType)(market ?? (int)MarketHelper.DefaultUIMarket),
                MarketplaceId = marketplaceId ?? MarketHelper.DefaultUIMarketplaceId,
                DropShipperId = dropShipperId,
                OrderStatus = orderStatusList,
                ShippingStatus = shippingStatus,
                ExcludeWithLabels = excludeWithLabels,
                IncludeForceVisible = includeForceVisible,
                DateFrom = dateFrom.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateFrom, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null,
                DateTo = dateTo.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateTo, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null,
                BuyerName = string.IsNullOrEmpty(buyerName) ? null : buyerName.Trim(),
                BatchId = batchId,
                StyleId = string.IsNullOrEmpty(styleId) ? null : styleId.Trim(),
                StyleItemId = styleItemId,
            };

            if (!string.IsNullOrEmpty(orderNumber))
            {
                //model.EqualOrderNumber = orderNumber;
                if (orderNumber.Contains("-"))
                    model.OrderNumber = orderNumber.Trim();
                else
                    model.OrderNumber = OrderHelper.FormatDisplayOrderNumber(orderNumber, MarketType.None);
            }

            var searchResult = OrderViewModel.GetFilteredForDisplay(ReadDb,
                LogService,
                WeightService,
                model,
                AccessManager.IsFulfilment,
                SortMode.ByShippingMethodThenLocation);

            for (int i = 0; i < searchResult.Items.Count(); i++)
            {
                searchResult.Items[i].NumberByLocation = i;
            }

            if (!String.IsNullOrEmpty(model.OrderNumber))
            {
                OrderPageViewModel.AddSearchHistory(Db,
                    model.OrderNumber,
                    Time.GetUtcTime(),
                    AccessManager.UserId);
            }

            LogI("End GetOrders");
            var data = new GridResponse<OrderViewModel>(searchResult.Items, searchResult.Items.Count);
            data.RequestTimeStamp = request.TimeStamp;
            return JsonGet(data);
        }

        [Compress]
        public virtual ActionResult GetPendingOrders([DataSourceRequest] DataSourceRequest request,
            int? market,
            string marketplaceId)
        {
            LogI("GetOrders, market=" + market
                + ", marketplaceId=" + marketplaceId);

            var orderStatusList = new string[] { OrderStatusEnumEx.Pending };

            var model = new OrderSearchFilterViewModel
            {
                Market = (MarketType)(market ?? (int)MarketHelper.DefaultUIMarket),
                MarketplaceId = marketplaceId ?? MarketHelper.DefaultUIMarketplaceId,
                OrderStatus = orderStatusList,
            };

            var items = OrderViewModel.GetFilteredForDisplay(ReadDb,
                LogService,
                WeightService,
                model,
                AccessManager.IsFulfilment,
                SortMode.ByShippingMethodThenLocation);

            var dataSource = items.Items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SetOnHold(int id, bool onHold)
        {
            LogI("SetOnHold, id=" + id + ", onHold=" + onHold);
            var when = Time.GetAppNowTime();
            OrderEditViewModel.SetOnHold(Db, OrderHistoryService, id, onHold, when, AccessManager.UserId);

            return JsonGet(new {
                    OnHold = onHold,
                    OnHoldUpdateDate = when
                });
        }

        public virtual ActionResult SetIsOversold(int id, bool isOversold)
        {
            LogI("SetIsOversold, id=" + id + ", isOversold=" + isOversold);
            var when = Time.GetAppNowTime();
            OrderEditViewModel.SetIsOversold(Db, OrderHistoryService, id, isOversold, when, AccessManager.UserId);

            return JsonGet(new {
                    IsOversold = isOversold,
                    IsOversoldUpdateDate = when
                });
        }

        public virtual ActionResult SetRefundLocked(int id, bool isRefundLocked)
        {
            LogI("SetRefundLocked, id=" + id + ", isRefundLocked=" + isRefundLocked);
            var when = Time.GetAppNowTime();
            OrderEditViewModel.SetRefundLocked(Db, OrderHistoryService, id, isRefundLocked, when, AccessManager.UserId);

            return new JsonResult
            {
                Data = new
                {
                    IsRefundLocked = isRefundLocked,
                    IsRefundLockedDate = when
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult SetShippingOptions(int id, string orderId, int groupId)
        {
            LogI("SetShippingOptions, id=" + id + ", orderId=" + orderId + ", groupId=" + groupId);

            var model = OrderViewModel.UpdateShippingInfo(Db, OrderHistoryService, WeightService, id, orderId, groupId, AccessManager.UserId);

            return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetFile(string fileName)
        {
            LogI("GetFile, fileName=" + fileName);

            var path = UrlHelper.GetLabelPath(fileName);
            return File(path, "image/jpeg", Path.GetFileName(path));
        }

        public virtual ActionResult OrderHistory(string orderId)
        {
            LogI("OrderHistory, orderId=" + orderId);

            var model = new OrderHistoryControlViewModel()
            {
                OrderNumber = orderId,
                IsCollapsed = false
            };
            
            return View("OrderHistory", model);
        }

        public virtual ActionResult GetOrderHistory(string orderId)
        {
            LogI("GetOrderHistory, orderId=" + orderId);

            var model = OrderHistoryViewModel.GetByOrderId(Db, LogService, WeightService, orderId);

            return JsonGet(new ValueResult<OrderHistoryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult GetOrderQuickSummary(string orderId)
        {
            LogI("GetOrderQuickSummary, orderId=" + orderId);

            var model = OrderQuickSummaryViewModel.GetByOrderId(Db, LogService, WeightService, orderId);

            return JsonGet(new ValueResult<OrderQuickSummaryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult GetOrderMessagesSummary(string orderNumber)
        {
            LogI("GetOrderMessagesSummary, orderNumber=" + orderNumber);

            var model = OrderMessagesSummaryViewModel.GetByOrderId(Db, LogService, orderNumber);

            return JsonGet(new ValueResult<OrderMessagesSummaryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult OnEditOrder(long id, long? batchId)
        {
            LogI("OnEditOrder, Id=" + id);

            var item = Db.ItemOrderMappings.GetOrderWithItems(WeightService, id, true, true, unmaskReferenceStyles: false); //Show only original orderItems
            item.BatchId = batchId;
            var model = new OrderEditViewModel(Db, LogService, item, AccessManager.ShipmentProviderInfoList, AccessManager.IsFulfilment);//{ BatchId = batchId }
            ViewBag.PartialViewName = "OrderEdit";
            return View("EditEmpty", model);
        }

        public virtual ActionResult OnViewComments(long orderId)
        {
            LogI("OnViewComments, orderId=" + orderId);

            var model = new ViewCommentsViewModel(Db, orderId);
            
            ViewBag.PartialViewName = "ViewComments";
            return View("EditEmpty", model);
        }

        public virtual ActionResult SetDismissAddressValidationError(long id)
        {
            LogI("SetDismissAddressValidationError, id=" + id);
            var userId = AccessManager.UserId;
            OrderViewModel.DismissAddressWarn(Db,
                OrderHistoryService,
                id,
                Time.GetAppNowTime(),
                userId);

            return new JsonResult
            {
                Data = MessageResult.Success("", id.ToString()),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult SetCustomShipping(long orderId, 
            IList<CustomShippingItemViewModel> items)
        {
            LogI("SetCustomShipping, orderId=" + orderId + ", items=" + String.Join(", ", items.Select(i => i.ToString()).ToList()));

            var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                Time,
                DbFactory,
                WeightService,
                AccessManager.Company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            var result = CustomShippingItemViewModel.Apply(Db,
                LogService,
                Time,
                WeightService,
                orderId,
                rateProviders,
                CompanyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                CompanyAddress.GetPickupAddress(MarketIdentifier.Empty()),
                items,
                AccessManager.IsFulfilment);

            if (result.IsSuccess)
            {
                return JsonGet(ValueResult<IList<SelectListShippingOption>>.Success("", result.Data));
            }
            return JsonGet(ValueResult<IList<SelectListShippingOption>>.Error(result.Message));
        }

        public virtual ActionResult GetCustomShipping(long orderId, long? defaultShippingMethodId)
        {
            LogI("GetCustomShipping, orderId=" + orderId + ", defaultShippingMethodId=" + defaultShippingMethodId);

            var model = CustomShippingViewModel.Get(Db, orderId, defaultShippingMethodId);

            return JsonGet(ValueResult<CustomShippingViewModel>.Success("", model));
        }

        public virtual ActionResult SetTrackingNumber(IList<OrderShippingViewModel> shippings)
        {
            LogI("SetTrackingNumber");
            var userId = AccessManager.UserId;
            var result = new CallMessagesResultVoid();
            var messages = new List<MessageString>();
            foreach (var shipping in shippings)
            {
                messages.AddRange(shipping.ValidateTrackingNumber());
            }

            if (!messages.Any())
            {
                foreach (var shipping in shippings)
                {
                    LogI("Update, shippingInfoId=" + shipping.ShippingInfoId + ", trackingNumber=" + shipping.TrackingNumber);
                    shipping.UpdateTrackingNumber(Db,
                        Time.GetAppNowTime(),
                        userId);
                }
            }

            return JsonGet(new CallMessagesResultVoid() {
                Messages = messages,
                Status = messages.Any() ? CallStatus.Fail : CallStatus.Success
            });
        }

        public virtual ActionResult AttachOrderTo(long orderId, string toOrderString)
        {
            LogI("AttachOrderTo, orderId=" + orderId + " toOrderString=" + toOrderString);

            var result = OrderViewModel.AttachOrderTo(Db, orderId, toOrderString, Time.GetAppNowTime(), AccessManager.UserId);

            return JsonGet(result);
        }

        public virtual ActionResult CancelOrder(long id)
        {
            LogI("CancelOrder, id=" + id);
            var userId = AccessManager.UserId;
            OrderEditViewModel.CancelOrder(Db, LogService, Time, ActionService, OrderHistoryService, id, userId);
            return new JsonResult
            {
                Data = MessageResult.Success("", id.ToString()),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public virtual ActionResult CheckAddress(OrderEditViewModel model)
        {
            var serviceFactory = new ServiceFactory();
            var addressProviders = AccessManager.Company.AddressProviderInfoList
                .Where(a => a.Type != (int)AddressProviderType.SelfCorrection)
                .ToList(); //NOTE: exclude self correction
            var addressCheckService = serviceFactory.GetAddressCheckServices(LogService,
                Time,
                DbFactory,
                addressProviders);

            var addressService = new AddressService(addressCheckService,
                null,
                null);

            var validatorService = new OrderValidatorService(LogService,
                DbFactory,
                EmailService,
                Settings,
                OrderHistoryService,
                ActionService,
                PriceService,
                HtmlScraper,
                addressService,
                null,
                null,
                Time,
                AccessManager.Company);

            var sourceAddress = model.ComposeAddressDto();
            AddressDTO correctedAddress = null;
            var checkResults = validatorService.CheckAddress(CallSource.UI, Db, sourceAddress, null, out correctedAddress);
            foreach (var checkResult in checkResults)
            {
                checkResult.Message = AddressHelper.GeocodeMessageToDisplay(checkResult.Message, true);
            }

            var isSuccess = checkResults.Any(r => r.Status < (int)AddressValidationStatus.Invalid
                && r.Status != (int)AddressValidationStatus.None);

            AddressViewModel correctedModel = correctedAddress != null ?
                new AddressViewModel(correctedAddress) : null;

            var result = new AddressValidationResultViewModel()
            {
                IsSuccess = isSuccess,
                CheckResults = checkResults,
                CorrectedAddress = correctedModel
            };

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public virtual ActionResult Submit(OrderEditViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var company = AccessManager.Company;
                var serviceFactory = new ServiceFactory();
                var addressProviders = AccessManager.Company.AddressProviderInfoList
                    .Where(a => a.Type != (int)AddressProviderType.SelfCorrection)
                    .ToList(); //NOTE: exclude self correction
                var addressCheckService = serviceFactory.GetAddressCheckServices(LogService,
                    Time,
                    DbFactory,
                    addressProviders);

                var companyAddress = new CompanyAddressService(company);
                var addressService = new AddressService(addressCheckService, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                var addressChecker = new AddressChecker(LogService, DbFactory, addressService, OrderHistoryService, Time);

                //var validatorService = new OrderValidatorService(LogService,
                //    DbFactory,
                //    EmailService,
                //    OrderHistoryService,
                //    ActionService,
                //    HtmlScraper,
                //    addressService,
                //    null,
                //    null,
                //    Time,
                //    AccessManager.Company);
                var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);
                var syncInfo = new EmptySyncInformer(LogService, SyncType.Orders);
                var synchronizer = new AmazonOrdersSynchronizer(LogService,
                    AccessManager.Company,
                    syncInfo,
                    rateProviders,
                    CompanyAddress,
                    Time,
                    WeightService,
                    MessageService);

                var applyResult = model.Apply(LogService, Db, OrderHistoryService, QuantityManager, Time.GetAppNowTime(), AccessManager.UserId);

                var resultErrors = model.ProcessApplyResult(applyResult,
                    Db,
                    LogService,
                    Time,
                    synchronizer,
                    addressChecker,
                    OrderHistoryService,
                    WeightService,
                    AccessManager.UserId);

                if (resultErrors.Any())
                {
                    resultErrors.ForEach(r => ModelState.AddModelError(r.Key, r.Message));
                    return PartialView("OrderEdit", model);
                }

                var rowOrderDto = Db.ItemOrderMappings.GetOrderWithItems(WeightService, model.EntityId, false, true, unmaskReferenceStyles:true); //NOTE: Unmask for display
                var rowModel = new OrderViewModel(rowOrderDto, AccessManager.IsFulfilment);
                rowModel.Items = rowOrderDto.Items.Select(i =>
                        new OrderItemViewModel(i,
                            rowOrderDto.OnHold,
                            ShippingUtils.IsOrderPartial(rowOrderDto.OrderStatus))).ToList();

                return Json(new UpdateRowViewModel(rowModel,
                    model.BatchId.HasValue ? "grid_" + model.BatchId.Value : "grid",
                    null,
                    false));
            }
            return PartialView("OrderEdit", model);
        }

        public virtual JsonResult GetListingsToReplace(string styleString,
            int market,
            string marketplaceId,
            long listingId)
        {
            var items = OrderEditViewModel.GetListingsToReplace(Db,
                styleString,
                market,
                marketplaceId,
                listingId);

            return Json(new ValueResult<IList<ListingOrderDTO>>(true, "", items), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSecondDay([DataSourceRequest] DataSourceRequest request,
            string dateFrom,
            string dateTo)
        {
            LogI("GetSecondDay");

            var model = new OrderSearchFilterViewModel
            {
                DateFrom = dateFrom.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateFrom, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null,
                DateTo = dateTo.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateTo, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null
            };
            var items = SecondDayViewModel.GetAll(Db, model);
            var dataSource = items.ToDataSourceResult(request);
            return Json(dataSource, JsonRequestBehavior.AllowGet);
        }
    }
}
