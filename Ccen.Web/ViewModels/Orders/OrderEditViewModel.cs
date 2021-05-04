using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.UI;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Extensions;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Inventory;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Controllers;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Results;
using Amazon.Model.Implementation.Validation;
using Amazon.Core.Contracts.Factories;

namespace Amazon.Web.ViewModels
{
    public class OrderEditViewModel
    {
        public long EntityId { get; set; }
        public long? BatchId { get; set; }
        public string OrderId { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public MarketType Market { get;set; }
        public string MarketplaceId { get; set; }

        public bool IsPrime { get; set; }

        public string OrderStatus { get; set; }

        public string PersonName { get; set; }
        public string BuyerEmail { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; }

        public string SourceShippingName { get; set; }

        
        public bool IsManuallyUpdated { get; set; }
        public string ManuallyPersonName { get; set; }
        public string ManuallyShippingAddress1 { get; set; }
        public string ManuallyShippingAddress2 { get; set; }
        public string ManuallyShippingCity { get; set; }
        public string ManuallyShippingState { get; set; }
        public string ManuallyShippingUSState { get; set; }
        public string ManuallyShippingCountry { get; set; }
        public string ManuallyShippingZip { get; set; }
        public string ManuallyShippingZipAddon { get; set; }
        public string ManuallyShippingPhone { get; set; }
        public bool RequiredPackageSize { get; set; }

        public int? ManuallyShipmentProviderType { get; set; }
        public string ManuallyShipmentProviderName
        {
            get { return ShipmentProviderHelper.GetName((ShipmentProviderType)ManuallyShipmentProviderType); }
        }
        public List<DropDownListItem> ShippingProviders { get; set; } 


        public string ManuallyShippingGroupId { get; set; }
        public List<SelectListShippingOption> ShippingOptions { get; set; }

        public List<PackageViewModel> Packages { get; set; }

        public bool OnHold { get; set; }
        public DateTime? OnHoldUpdateDate { get; set; }

        public bool IsRefundLocked { get; set; }


        public bool IsDisabled
        {
            get
            {
                return false;// IsPrime;
            }
        }

        public bool HasBatchLabel { get; set; }

        public bool HasCancelationRequest { get; set; }
        public bool IsOversold { get; set; }

        public int AddressValidationStatus { get; set; }
        public bool IsDismissAddressValidation { get; set; }
        public DateTime? AddressVerifyRequestDate { get; set; }

        public string AddressGoogleValidationMessage { get; set; }

        public string AttachedToOrderId { get; set; }


        [Display(Name = "Insured Value")]
        public decimal? InsuredValue { get; set; }
        public bool IsInsured { get; set; }
        
        public string Currency
        {
            get { return PriceHelper.GetCurrencySymbol(Market, MarketplaceId); }
        }

        public string FormattedOrderId
        {
            get { return OrderHelper.FormatDisplayOrderNumber(OrderId, Market); }
        }
        
        public bool IsSignConfirmation { get; set; }

        public string AddressFullString
        {
            get
            {
                return string.Join(", ",
                    new[]
                    {
                        string.Format("{0} {1}", ManuallyShippingAddress1, ManuallyShippingAddress2), ManuallyShippingCity, ManuallyShippingState,
                        string.Format("{0} {1}", ManuallyShippingZip, ManuallyShippingZipAddon), ManuallyShippingCountry
                    });
            }
        }

        public string AddressSearchString
        {
            get
            {
                return
                    HttpUtility.UrlEncode(AddressFullString);
            }
        }

        public string AddressString
        {
            get
            {
                return HttpUtility.UrlEncode(ShippingAddress1) +
                       (!String.IsNullOrEmpty(ShippingAddress2) ? " " + HttpUtility.UrlEncode(ShippingAddress2) : "");
            }
        }

        public string ZipString
        {
            get
            {
                return HttpUtility.UrlEncode(ShippingUtils.CombineZip(ShippingZip, ShippingZipAddon));
            }
        }

        public string MarketUrl
        {
            get { return MarketUrlHelper.GetSellarCentralOrderUrl(Market, MarketplaceId, OrderId, null); }
        }

        public IList<OrderItemEditViewModel> Items { get; set; }

        public IList<OrderShippingViewModel> Shippings { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public OrderEditViewModel()
        {
            Comments = new List<CommentViewModel>();
        }

        public OrderEditViewModel(IUnitOfWork db, 
            ILogService log,
            DTOOrder order, 
            IList<ShipmentProviderDTO> shipmentProviders,
            bool isFulfilmentUser)
        {
            EntityId = order.Id;
            Market = (MarketType)order.Market;
            MarketplaceId = order.MarketplaceId;
            BatchId = order.BatchId;
            OrderStatus = order.OrderStatus;

            OrderId = order.OrderId;
            ExpectedShipDate = order.EarliestShipDate;
            OnHold = order.OnHold;
            OnHoldUpdateDate = order.OnHoldUpdateDate;
            IsRefundLocked = order.IsRefundLocked ?? false;

            AddressValidationStatus = order.AddressValidationStatus;
            IsDismissAddressValidation = order.IsDismissAddressValidation;
            AddressVerifyRequestDate = order.AddressVerifyRequestDate;

            AttachedToOrderId = order.AttachedToOrderString;

            PersonName = order.PersonName;
            BuyerEmail = order.BuyerEmail;
            ShippingAddress1 = order.ShippingAddress1;
            ShippingAddress2 = order.ShippingAddress2;
            ShippingCity = order.ShippingCity;
            ShippingState = order.ShippingState;
            ShippingCountry = order.ShippingCountry;
            ShippingZip = order.ShippingZip;
            ShippingZipAddon = order.ShippingZipAddon;
            ShippingPhone = order.ShippingPhone;
            SourceShippingName = order.InitialServiceType;

            log.Info("OnEditOrder, Id=" + order.Id + " Before GetByOrderIdDto");
            Comments = db.OrderComments.GetByOrderIdDto(order.Id).OrderBy(c => c.CreateDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Comment = c.Message,
                    Type = c.Type,
                    LinkedEmailId = c.LinkedEmailId,
                    CommentDate = c.CreateDate,
                    CommentByName = c.CreatedByName
                }).ToList();
            Comments.ForEach(c =>
            {
                c.OrderNumber = OrderId; //NOTE: for View Email Url (reply button)
            });

            log.Info("OnEditOrder, Id=" + order.Id + " Before GetByOrderIdDto");
            var addressGoogleValidationMessage = db.OrderNotifies.GetAll()
                .OrderByDescending(n => n.CreateDate)
                .FirstOrDefault(n => n.Type == (int)OrderNotifyType.AddressCheckGoogleGeocode
                    && n.OrderId == order.Id);
            if (addressGoogleValidationMessage != null)
            {
                AddressGoogleValidationMessage = AddressHelper.GeocodeMessageToDisplay(addressGoogleValidationMessage.Message, false);
            }

            log.Info("OnEditOrder, Id=" + order.Id + " Before GetShippingOptions");
            ShippingOptions = OrderViewModel.GetShippingOptions(order.ShippingInfos,
                (MarketType)order.Market,
                order.IsSignConfirmation,
                order.IsInsured,
                isFulfilmentUser,
                showOptionsPrices: false,
                showProviderName: false);
            ShippingProviders = OrderViewModel.GetShippingProviders(shipmentProviders,
                (MarketType)order.Market,
                order.MarketplaceId,
                order.ShippingCountry,
                order.SourceShippingService,
                order.OrderType);

            Shippings = order.ShippingInfos.Select(sh => new OrderShippingViewModel(sh)).ToList();

            Packages = Shippings
                .Where(sh => sh.IsActive)
                .OrderBy(sh => sh.ShippingInfoId)
                .Select(p => new PackageViewModel()
                {
                    ShippingId = p.ShippingInfoId,
                    PackageLength = p.PackageLength,
                    PackageWidth = p.PackageWidth,
                    PackageHeight = p.PackageHeight,
                })
                .ToList();

            ManuallyShipmentProviderType = order.ShipmentProviderType;

            var shippingGroup = ShippingOptions.FirstOrDefault(x => x.Selected);
            ManuallyShippingGroupId = shippingGroup == null ? null : shippingGroup.Value;
            RequiredPackageSize = shippingGroup != null && shippingGroup.RequiredPackageSize;

            if (order.IsManuallyUpdated)
            {
                ManuallyPersonName = order.ManuallyPersonName;
                ManuallyShippingAddress1 = order.ManuallyShippingAddress1;
                ManuallyShippingAddress2 = order.ManuallyShippingAddress2;
                ManuallyShippingCity = order.ManuallyShippingCity;
                ManuallyShippingState = order.ManuallyShippingState;
                ManuallyShippingUSState = order.ManuallyShippingState;
                ManuallyShippingCountry = order.ManuallyShippingCountry;
                ManuallyShippingZip = order.ManuallyShippingZip;
                ManuallyShippingZipAddon = order.ManuallyShippingZipAddon;
                ManuallyShippingPhone = order.ManuallyShippingPhone;
            }
            else
            {
                ManuallyPersonName = order.PersonName;
                ManuallyShippingAddress1 = order.ShippingAddress1;
                ManuallyShippingAddress2 = order.ShippingAddress2;
                ManuallyShippingCity = order.ShippingCity;
                ManuallyShippingState = order.ShippingState;
                ManuallyShippingUSState = order.ShippingState;
                ManuallyShippingCountry = order.ShippingCountry;
                ManuallyShippingZip = order.ShippingZip;
                ManuallyShippingZipAddon = order.ShippingZipAddon;
                ManuallyShippingPhone = order.ShippingPhone;
            }

            IsManuallyUpdated = order.IsManuallyUpdated;

            IsPrime = order.OrderType == (int) OrderTypeEnum.Prime;

            //InsuredValue = order.TotalPrice;
            InsuredValue = order.Items.Sum(i => i.ItemPrice);
            IsInsured = order.IsInsured;
            IsSignConfirmation = order.IsSignConfirmation;

            HasBatchLabel = order.ShippingInfos.Any(sh => !String.IsNullOrEmpty(sh.TrackingNumber));
            Items = SplitItems(order.Items).ToList();

            if (order.Notifies != null)
            {
                var cancelationRequest =
                    order.Notifies.FirstOrDefault(n => n.Type == (int) OrderNotifyType.CancellationRequest);
                if (cancelationRequest != null)
                {
                    HasCancelationRequest = true;
                }

                var oversoldNotify = order.Notifies.FirstOrDefault(n => n.Type == (int)OrderNotifyType.OversoldItem
                    || n.Type == (int)OrderNotifyType.OversoldOnHoldItem);
                if (oversoldNotify != null)
                {
                    IsOversold = true;
                }
            }
            log.Info("OnEditOrder, Id=" + order.Id + " End OrderEditViewModel");
        }

        private IList<OrderItemEditViewModel> SplitItems(IList<ListingOrderDTO> items)
        {
            var results = new List<OrderItemEditViewModel>();
            foreach (var item in items)
            {
                var quantity = item.QuantityOrdered;
                for (int i = 0; i < quantity; i++)
                {
                    var itemVM = new OrderItemEditViewModel(item);
                    itemVM.Quantity = 1;
                    results.Add(itemVM);
                }
            }
            return results;
        }

        private IList<OrderItemEditViewModel> JoinItems(IList<OrderItemEditViewModel> items)
        {
            var results = new List<OrderItemEditViewModel>();
            foreach (var item in items)
            {
                var existItem = results.FirstOrDefault(i => i.SourceItemOrderId 
                        == item.SourceItemOrderId
                    && i.NewListingId == item.NewListingId);
                if (existItem == null)
                {
                    results.Add(item.Clone());
                }
                else
                {
                    existItem.Quantity += item.Quantity;
                }
            }

            //Restore itemOrderId if possible
            foreach (var item in results)
            {
                var count = results.Count(r => r.SourceItemOrderId == item.SourceItemOrderId);
                if (count == 1)
                {
                    //If the source itemId presents only once, remove postfix
                    item.ItemOrderId = item.SourceItemOrderId;
                }
                if (count > 1)
                {
                    //If more then ones actualize postfix
                    item.ItemOrderId = item.SourceItemOrderId + "_" + item.NewListingId;
                }
            }

            return results;
        }

        public AddressDTO ComposeAddressDto()
        {
            var address = new AddressDTO();

            //NOTE: Original address dosn't came after submit

            address.FullName = ManuallyPersonName;
            address.ManuallyAddress1 = ManuallyShippingAddress1;
            address.ManuallyAddress2 = ManuallyShippingAddress2;
            address.ManuallyCity = ManuallyShippingCity;
            address.ManuallyCountry = ManuallyShippingCountry;

            address.ManuallyState = !ShippingUtils.IsInternational(address.ManuallyCountry) // == "US" 
                ? ManuallyShippingUSState
                : ManuallyShippingState;

            address.ManuallyZip = ManuallyShippingZip;
            address.ManuallyZipAddon = ManuallyShippingZipAddon;
            address.ManuallyPhone = ManuallyShippingPhone;

            address.IsManuallyUpdated = true;

            return address;
        }


        public static IList<ListingOrderDTO> GetListingsToReplace(IUnitOfWork db,
            string styleString,
            int market,
            string marketplaceId,
            long currentListingId)
        {
            var listingList = (from l in db.Listings.GetViewListingsAsDto(withUnmaskedStyles:false)
                join stCache in db.StyleItemCaches.GetAll() on l.StyleItemId equals stCache.Id
                where l.StyleString == styleString
                      && l.Market == market
                      && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                orderby l.Quantity descending 
                select new ListingOrderDTO()
                {
                    Id = l.Id,
                    ASIN = l.ASIN,
                    Market = l.Market,
                    MarketplaceId = l.MarketplaceId,
                    StyleSize = l.StyleSize,
                    StyleColor = l.StyleColor,
                    StyleItemId = l.StyleItemId,
                    StyleId = l.StyleId,
                    StyleID = l.StyleString,
                    RealQuantity = l.Quantity,
                    AvailableQuantity = stCache.RemainingQuantity,
                }).ToList();

            var results = new List<ListingOrderDTO>();
            //NOTE: also require to sold possible issue with remap to another listings with same size/color, when style has multiple listings for same size/color,
            var currentListing = listingList.FirstOrDefault(l => l.Id == currentListingId);
            if (currentListing != null)
                results.Add(currentListing);
            
            foreach (var listing in listingList)
            {
                if (listing.AvailableQuantity <= 0)
                    continue;

                if (!results.Any(r => r.StyleSize == listing.StyleSize
                                     && r.StyleColor == listing.StyleColor))
                {
                    results.Add(listing);
                }
            }

            return results
                .OrderBy(r => SizeHelper.GetSizeIndex(r.StyleSize))
                .ThenBy(r => r.StyleColor)
                .ToList();
        }

        public class ApplyOrderResult
        {
            public bool AddressValidationRequested { get; set; }
            public bool RateRecalcRequested { get; set; }
            public bool ShipmentProviderChanged { get; set; }
        }

        public ApplyOrderResult Apply(ILogService log, 
            IUnitOfWork db,
            IOrderHistoryService orderHistoryService,
            IQuantityManager quantityManager,
            DateTime when, 
            long? by)
        {
            var dbOrder = db.Orders.GetById(EntityId);
            var addressChanged = false;
            var shipmentProviderChanged = false;
            var shouldRecalcRates = dbOrder.IsInsured != IsInsured
                || dbOrder.IsSignConfirmation != IsSignConfirmation;

            var manuallyAddress = ComposeAddressDto();
            
            //NOTE: empty when fields was readonly
            if (!AddressHelper.IsEmptyManually(manuallyAddress))
            {
                var originalAddress = dbOrder.GetAddressDto();

                addressChanged = AddressHelper.CompareWithManuallyAllFields(originalAddress, manuallyAddress);
                shouldRecalcRates = shouldRecalcRates || AddressHelper.CompareWithManuallyBigChanges(originalAddress, manuallyAddress);
                
                if (addressChanged)
                {
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyPersonNameKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyPersonName : dbOrder.PersonName, ManuallyPersonName, by);
                    dbOrder.ManuallyPersonName = ManuallyPersonName;
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingAddress1Key, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingAddress1 : dbOrder.ShippingAddress1, ManuallyShippingAddress1, by);
                    dbOrder.ManuallyShippingAddress1 = ManuallyShippingAddress1;
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingAddress2Key, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingAddress2 : dbOrder.ShippingAddress2, ManuallyShippingAddress2, by);
                    dbOrder.ManuallyShippingAddress2 = ManuallyShippingAddress2;
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingCityKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingCity : dbOrder.ShippingCity, ManuallyShippingCity, by);
                    dbOrder.ManuallyShippingCity = ManuallyShippingCity;
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingCountryKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingCountry : dbOrder.ShippingCountry, ManuallyShippingCountry, by);
                    dbOrder.ManuallyShippingCountry = ManuallyShippingCountry;

                    dbOrder.ManuallyShippingState = !ShippingUtils.IsInternational(dbOrder.ManuallyShippingCountry)
                        // == "US" 
                        ? ManuallyShippingUSState
                        : ManuallyShippingState;

                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingZipKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingZip : dbOrder.ShippingZip, ManuallyShippingZip, by);
                    dbOrder.ManuallyShippingZip = StringHelper.TrimWhitespace(ManuallyShippingZip);
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingZipAddonKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingZipAddon : dbOrder.ShippingZipAddon, ManuallyShippingZipAddon, by);
                    dbOrder.ManuallyShippingZipAddon = StringHelper.TrimWhitespace(ManuallyShippingZipAddon);
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ManuallyShippingPhoneKey, dbOrder.IsManuallyUpdated ? dbOrder.ManuallyShippingPhone : dbOrder.ShippingPhone, ManuallyShippingPhone, by);
                    dbOrder.ManuallyShippingPhone = ManuallyShippingPhone;

                    dbOrder.IsManuallyUpdated = true;
                }
                else
                {
                    dbOrder.ManuallyPersonName = String.Empty;
                    dbOrder.ManuallyShippingAddress1 = String.Empty;
                    dbOrder.ManuallyShippingAddress2 = String.Empty;
                    dbOrder.ManuallyShippingCity = String.Empty;
                    dbOrder.ManuallyShippingCountry = String.Empty;

                    dbOrder.ManuallyShippingState = String.Empty;
                    dbOrder.ManuallyShippingZip = String.Empty;
                    dbOrder.ManuallyShippingZipAddon = String.Empty;
                    dbOrder.ManuallyShippingPhone = String.Empty;

                    dbOrder.IsManuallyUpdated = false;
                }
            }

            dbOrder.InsuredValue = InsuredValue;
            orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.IsInsuredKey, dbOrder.IsInsured, IsInsured, by);
            dbOrder.IsInsured = IsInsured;
            orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.IsSignConfirmationKey, dbOrder.IsSignConfirmation, IsSignConfirmation, by);
            dbOrder.IsSignConfirmation = IsSignConfirmation;
            
            if (ManuallyShipmentProviderType.HasValue)
            {
                if (dbOrder.ShipmentProviderType != ManuallyShipmentProviderType.Value)
                {
                    shipmentProviderChanged = true;
                    shouldRecalcRates = true;
                }

                orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ShipmentProviderTypeKey, dbOrder.ShipmentProviderType, ManuallyShipmentProviderType, by);
                dbOrder.ShipmentProviderType = ManuallyShipmentProviderType.Value;
            }

            //dbOrder.OnHold = OnHold;

            dbOrder.UpdateDate = when;
            dbOrder.UpdatedBy = by;

            if (Items.All(i => i.NewListingId > 0)) //NOTE: only when enabled items edit
            {
                var changeNotes = "";
                var itemSizeWasChanged = false;
                var joinItems = JoinItems(Items);
                var dbOrderItems = db.OrderItems.GetAll().Where(i => i.OrderId == dbOrder.Id).ToList();
                var orderItemSources = db.OrderItemSources.GetAllAsDto().Where(i => i.OrderId == dbOrder.Id).ToList();

                foreach (var item in joinItems)
                {
                    var dbOrderItem = dbOrderItems.FirstOrDefault(im => im.ItemOrderIdentifier == item.ItemOrderId);

                    //NOTE: Get source info for set proprotionally ItemPrice etc.
                    var sourceItemOrderId = item.SourceItemOrderId;
                    var sourceItemMapping = orderItemSources.FirstOrDefault(i => i.ItemOrderIdentifier == sourceItemOrderId);

                    if (dbOrderItem != null)
                    {
                        log.Info("Updated orderItemId=" + item.ItemOrderId + ", qty=" + item.Quantity);
                    }
                    else
                    {
                        log.Info("Added orderItemId=" + item.ItemOrderId + ", qty=" + item.Quantity);
                        dbOrderItem = db.OrderItemSources.CreateItemFromSourceDto(sourceItemMapping);
                        dbOrderItem.CreateDate = when;

                        db.OrderItems.Add(dbOrderItem);
                    }

                    dbOrderItem.ItemOrderIdentifier = item.ItemOrderId;
                    dbOrderItem.QuantityOrdered = item.Quantity;
                    dbOrderItem.ListingId = item.NewListingId;

                    dbOrderItem.SourceListingId = sourceItemMapping.ListingId;
                    dbOrderItem.SourceItemOrderIdentifier = sourceItemMapping.ItemOrderIdentifier;

                    var newListing = db.Listings.GetViewListingsAsDto(withUnmaskedStyles: false)
                        .FirstOrDefault(l => l.Id == item.NewListingId);

                    var keepListingUpdateOnlyStyle = newListing.StyleItemId != dbOrderItem.StyleItemId;
                    if (dbOrderItem.Id == 0 
                        || item.NewListingId != item.ListingId
                        || keepListingUpdateOnlyStyle)
                    {
                        var oldListing = db.Listings.GetViewListingsAsDto(withUnmaskedStyles: false)
                            .FirstOrDefault(l => l.Id == sourceItemMapping.ListingId);

                        if (newListing != null && oldListing != null)
                        {
                            itemSizeWasChanged = newListing.StyleItemId != oldListing.StyleItemId;
                            if (itemSizeWasChanged)
                            {
                                var isStyleChanged = newListing.StyleString != oldListing.StyleString;
                                changeNotes += (isStyleChanged ? "Order item" : "Size") + " was changed from: " +
                                               (isStyleChanged ? oldListing.StyleString + " - " : " ") +
                                               SizeHelper.ToVariation(oldListing.StyleSize, oldListing.StyleColor)
                                               + " to: " + (isStyleChanged ? newListing.StyleString + " - " : "") +
                                               SizeHelper.ToVariation(newListing.StyleSize, newListing.StyleColor);

                                dbOrderItem.ReplaceType = (int)ItemReplaceTypes.Change;
                                dbOrderItem.ReplaceDate = when;
                                orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ReplaceItemKey, dbOrderItem.StyleItemId, dbOrderItem.Id.ToString(), newListing.StyleItemId, null, by);

                                var quantityOperation = new QuantityOperationDTO()
                                {
                                    Type = (int)QuantityOperationType.Lost,
                                    QuantityChanges = new List<QuantityChangeDTO>()
                                    {
                                        new QuantityChangeDTO()
                                        {
                                            StyleId = oldListing.StyleId.Value,
                                            StyleItemId = oldListing.StyleItemId.Value,
                                            Quantity = dbOrderItem.QuantityOrdered,
                                            //NOTE: W/o sign means that the qty will be substracted
                                        }
                                    },
                                    Comment = "Order edit, change style/size",
                                };

                                quantityManager.AddQuantityOperation(db,
                                    quantityOperation,
                                    when,
                                    by);
                            }

                            //NOTE: Actualize current style info
                            dbOrderItem.StyleId = newListing.StyleId;
                            dbOrderItem.StyleItemId = newListing.StyleItemId;
                            dbOrderItem.StyleString = newListing.StyleString;
                            
                            dbOrderItem.SourceStyleString = oldListing.StyleString;
                            dbOrderItem.SourceStyleItemId = oldListing.StyleItemId;
                            dbOrderItem.SourceStyleSize = oldListing.StyleSize;
                            dbOrderItem.SourceStyleColor = oldListing.StyleColor;
                        }
                    }

                    if (dbOrderItem.ItemOrderIdentifier != dbOrderItem.SourceItemOrderIdentifier)
                    {
                        var portionCoef = dbOrderItem.QuantityOrdered/(decimal) sourceItemMapping.QuantityOrdered;

                        dbOrderItem.ItemPrice = sourceItemMapping.ItemPrice*portionCoef;
                        dbOrderItem.ItemPriceInUSD = sourceItemMapping.ItemPriceInUSD*portionCoef;
                        dbOrderItem.ShippingPrice = sourceItemMapping.ShippingPrice*portionCoef;
                        dbOrderItem.ShippingPriceInUSD = sourceItemMapping.ShippingPriceInUSD*portionCoef;
                        dbOrderItem.ShippingDiscount = sourceItemMapping.ShippingDiscount*portionCoef;
                        dbOrderItem.ShippingDiscountInUSD = sourceItemMapping.ShippingDiscountInUSD*portionCoef;
                    }
                    else //NOTE: m.b. no needed, for now no cases, but can be found in future
                    {
                        dbOrderItem.ItemPrice = sourceItemMapping.ItemPrice;
                        dbOrderItem.ItemPriceInUSD = sourceItemMapping.ItemPriceInUSD;
                        dbOrderItem.ShippingPrice = sourceItemMapping.ShippingPrice;
                        dbOrderItem.ShippingPriceInUSD = sourceItemMapping.ShippingPriceInUSD;
                        dbOrderItem.ShippingDiscount = sourceItemMapping.ShippingDiscount;
                        dbOrderItem.ShippingDiscountInUSD = sourceItemMapping.ShippingDiscountInUSD;
                    }
                }
                db.Commit();

                var toRemoveOrderItems = dbOrderItems.Where(oi => joinItems.All(i => i.ItemOrderId != oi.ItemOrderIdentifier)
                    && oi.QuantityOrdered > 0).ToList(); //Keeping cancelled items with qty = 0
                foreach (var toRemove in toRemoveOrderItems)
                {
                    log.Info("Remove orderItem, ordrItemId=" + toRemove.ItemOrderIdentifier 
                        + ", qty=" + toRemove.QuantityOrdered);
                    db.OrderItems.Remove(toRemove);

                    itemSizeWasChanged = true;
                }
                db.Commit();

                if (itemSizeWasChanged)
                {
                    shouldRecalcRates = true;

                    Comments.Add(new CommentViewModel()
                    {
                        Comment = changeNotes,
                        Type = (int) CommentType.ReturnExchange
                    });
                }
            }


            if (!string.IsNullOrEmpty(ManuallyShippingGroupId))
            {
                var groupId = int.Parse(ManuallyShippingGroupId);
                var shippings = db.OrderShippingInfos.GetByOrderId(EntityId).ToList();
                var previousIsActiveMethodIds = String.Join(";", shippings.Where(sh => sh.IsActive).Select(sh => sh.ShippingMethodId).ToList());
                var hasDropdown = shippings.Where(sh => sh.IsVisible).GroupBy(sh => sh.ShippingMethodId).Count() > 1;
                if (shippings.Any(sh => sh.ShippingGroupId == groupId))
                {
                    foreach (var shipping in shippings)
                    {
                        shipping.IsActive = shipping.ShippingGroupId == groupId;
                        if (hasDropdown) //Keep is visible
                            shipping.IsVisible = shipping.IsVisible || shipping.ShippingGroupId == groupId;
                        else
                            shipping.IsVisible = shipping.ShippingGroupId == groupId;
                    }
                    var newIsActiveMethodIds = String.Join(";", shippings.Where(sh => sh.IsActive).Select(sh => sh.ShippingMethodId).ToList());
                    orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.ShippingMethodKey, previousIsActiveMethodIds, newIsActiveMethodIds, by);
                }
                else
                {
                    //Can't change active shipping to not exists
                }
            }
            db.Commit();

            //Update Package Sizes
            var activeShippings = db.OrderShippingInfos.GetByOrderId(EntityId)
                .Where(sh => sh.IsActive)
                .OrderBy(sh => sh.Id)
                .ToList();

            for (var i = 0; i < activeShippings.Count; i++)
            {
                if (Packages != null && i < Packages.Count)
                {
                    if (activeShippings[i].PackageLength != Packages[i].PackageLength)
                    {
                        log.Info("Changed length: " + activeShippings[i].PackageLength + "=>" + Packages[i].PackageLength);
                        activeShippings[i].PackageLength = Packages[i].PackageLength;

                        shouldRecalcRates = true;
                    }
                    if (activeShippings[i].PackageWidth != Packages[i].PackageWidth)
                    {
                        log.Info("Changed width: " + activeShippings[i].PackageWidth + "=>" + Packages[i].PackageWidth);
                        activeShippings[i].PackageWidth = Packages[i].PackageWidth;

                        shouldRecalcRates = true;
                    }
                    if (activeShippings[i].PackageHeight != Packages[i].PackageHeight)
                    {
                        log.Info("Changed height: " + activeShippings[i].PackageHeight + "=>" + Packages[i].PackageHeight);
                        activeShippings[i].PackageHeight = Packages[i].PackageHeight;

                        shouldRecalcRates = true;
                    }
                }
            }
            db.Commit();
            



            foreach (var comment in Comments)
            {
                if (comment.Id == 0)
                    log.Info("New comment: " + comment.Comment);
            }

            db.OrderComments.AddComments(
                    Comments.Select(c => new CommentDTO()
                    {
                        Id = c.Id,
                        Message = c.Comment,
                        Type = c.Type,
                    }).ToList(),
                    EntityId,
                    when,
                    by);
            
            return new ApplyOrderResult()
            {
                RateRecalcRequested = shouldRecalcRates,
                AddressValidationRequested = addressChanged 
                    || dbOrder.AddressValidationStatus == (int)Core.Models.AddressValidationStatus.ExceptionCommunication
                    || dbOrder.AddressValidationStatus == (int)Core.Models.AddressValidationStatus.Exception,
                ShipmentProviderChanged = shipmentProviderChanged
            };
        }

        public List<MessageString> ProcessApplyResult(ApplyOrderResult applyResult, 
            IUnitOfWork db,
            ILogService log,
            ITime time,
            IOrderSynchronizer synchronizer,
            AddressChecker addressChecker,
            IOrderHistoryService orderHistoryService,
            IWeightService weightService,
            long? by)
        {
            var results = new List<MessageString>();

            if (applyResult.RateRecalcRequested)
            {
                var dtoOrder = db.ItemOrderMappings.GetOrderWithItems(weightService, OrderId, unmaskReferenceStyle: false, includeSourceItems: true);

                try
                {                    
                    RetryHelper.ActionWithRetries(() => synchronizer.UIUpdate(db, 
                            dtoOrder, 
                            isForceOverride: false, 
                            keepActiveShipping: true, 
                            keepCustomShipping: applyResult.ShipmentProviderChanged ? false : true,
                            switchToMethodId: null),
                        log,
                        2,
                        300,
                        RetryModeType.Normal,
                        true);

                    orderHistoryService.AddRecord(dtoOrder.Id, OrderHistoryHelper.RecalculateRatesKey, null, true, by);
                }
                catch (Exception ex)
                {
                    results.Add(MessageString.Error("", "An unexpected error has occurred. Please try again. Detail: " + ex.Message));
                }
            }

            if (applyResult.AddressValidationRequested)
            {
                addressChecker.UpdateOrderAddressValidationStatus(db, EntityId, null);
            }

            return results;
        }

        public static void CancelOrder(IUnitOfWork db, 
            ILogService log,
            ITime time,
            ISystemActionService actionService,
            IOrderHistoryService orderHistoryService,
            long orderId,
            long? by)
        {
            var dbOrder = db.Orders.Get(orderId);
            if (dbOrder != null)
            {
                var orderItems = db.OrderItemSources.GetWithListingInfo().Where(oi => oi.OrderId == orderId).ToList();
                var itemIdList = orderItems.Where(oi => !String.IsNullOrEmpty(oi.SourceMarketId))
                        .Select(oi => oi.SourceMarketId)
                        .ToList();

                log.Info("Items to cancel: " + String.Join(";", itemIdList));

                db.OrderNotifies.Add(new OrderNotify()
                {
                   OrderId = dbOrder.Id,
                   Type = (int)OrderNotifyType.CancellationRequest,
                   Status = 1,
                   Message = "Email cancelation from web page",
                   CreateDate = time.GetAppNowTime(),
                });
                db.Commit();

                orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.CancellationRequestKey, null, true, by);

                SystemActionHelper.AddCancelationActionSequences(db,
                    actionService,
                    dbOrder.Id,
                    dbOrder.AmazonIdentifier,
                    String.Join(";", itemIdList),
                    null,
                    null,
                    null,
                    null,
                    by,
                    CancelReasonType.Manually);
            }
        }

        public static void SetOnHold(IUnitOfWork db,
            IOrderHistoryService orderHistorySerivce,
            long id,
            bool onHoldStatus,
            DateTime when,
            long? by)
        {
            var order = db.Orders.GetById(id);

            orderHistorySerivce.AddRecord(id, OrderHistoryHelper.OnHoldKey, order.OnHold, onHoldStatus, by);

            order.OnHold = onHoldStatus;
            order.OnHoldUpdateDate = when;
            db.Commit();
        }

        public static void SetIsOversold(IUnitOfWork db,
            IOrderHistoryService orderHistorySerivce,
            long id,
            bool isOversold,
            DateTime when,
            long? by)
        {
            var oversoldNotify = db.OrderNotifies.GetAll().FirstOrDefault(n => (n.Type == (int)OrderNotifyType.OversoldItem
                || n.Type == (int)OrderNotifyType.OversoldOnHoldItem)
                && n.OrderId == id);
            if (oversoldNotify != null)
            {
                db.OrderNotifies.Remove(oversoldNotify);
            }
            else
            {
                db.OrderNotifies.Add(new OrderNotify()
                {
                    OrderId = id,
                    Type = (int)OrderNotifyType.OversoldItem,
                    Message = "Manually from order edit",
                    Status = 1,
                    CreateDate = when
                });
            }
            db.Commit();
        }


        public static void SetRefundLocked(IUnitOfWork db,
            IOrderHistoryService orderHistorySerivce,
            long id,
            bool onRefundLockedStatus,
            DateTime when,
            long? by)
        {
            var order = db.Orders.GetById(id);

            orderHistorySerivce.AddRecord(id, OrderHistoryHelper.OnRefundLockedKey, order.IsRefundLocked, onRefundLockedStatus, by);

            order.IsRefundLocked = onRefundLockedStatus;
            //order.IsRefundLockedDate = when;
            db.Commit();
        }
        public override string ToString()
        {
            return "EntityId=" + EntityId +
                   ", BatchId=" + BatchId +
                   ", OrderId=" + OrderId +
                   ", PersonName=" + PersonName +
                   ", ManuallyShippingAddress1=" + ManuallyShippingAddress1 +
                   ", ManuallyShippingAddress2=" + ManuallyShippingAddress2 +
                   ", ManuallyShippingCity=" + ManuallyShippingCity +
                   ", ManuallyShippingState=" + ManuallyShippingState +
                   ", ManuallyShippingCountry=" + ManuallyShippingCountry +
                   ", ManuallyShippingZip=" + ManuallyShippingZip +
                   ", ManuallyShippingZipAddon=" + ManuallyShippingZipAddon +
                   ", IsManuallyUpdated=" + IsManuallyUpdated +
                   ", InsuredValue=" + InsuredValue +
                   ", IsInsured=" + IsInsured +
                   ", IsSignConfirmation=" + IsSignConfirmation;
        }
    }
}