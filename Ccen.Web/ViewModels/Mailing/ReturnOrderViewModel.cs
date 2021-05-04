using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Products;
using Amazon.Web.ViewModels.Results;
using Newtonsoft.Json;
using Amazon.DTO.Users;
using UrlHelper = Amazon.Web.Models.UrlHelper;
using Amazon.Model.General;

namespace Amazon.Web.ViewModels
{
    public class ReturnOrderViewModel
    {
        public long? OrderEntityId { get; set; }
        public string OrderID { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public DateTime? OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public DateTime? ExpDeliveryDate { get; set; }

        public ShippingMethodDTO ShippingMethod { get; set; }
        
        public AddressViewModel ToAddress { get; set; }

        public int? WeightLb { get; set; }

        public double? WeightOz { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal ActualShippingCost { get; set; }

        public string PriceCurrency { get; set; }


        public int? ShippingMethodSelected { get; set; }
        
        public string Notes { get; set; }

        public int? ReasonCode { get; set; }
        public int? RefundReasonCode { get; set; }

        public bool IsInsured { get; set; }

        public bool IsSignConfirmation { get; set; }
        public bool IsRefundLocked { get; set; }


        public bool IsPrinted { get; set; }
        public string PrintedLabelUrl { get; set; }
        public string PrintedTrackingNumber { get; set; }

        
        public IList<ReturnQuantityItemViewModel> Items { get; set; }

        public IList<LabelViewModel> Labels { get; set; }

        public IList<CommentViewModel> Comments { get; set; } 

        public List<MessageString> Messages { get; set; }

        public IList<ReturnRequestDTO> ReturnRequests { get; set; }
        public IList<RefundViewModel> Refunds { get; set; }

        public string OrderComment { get; set; }

        public bool BackItemsToInventory { get; set; }
        public bool SubtractReplacementFromInventory { get; set; }

        public bool DoRefund { get; set; }
        public bool IncludeShipping { get; set; }
        public bool DeductShipping { get; set; }

        public bool DeductPrepaidLabel { get; set; }
        public decimal? PrepaidLabelCost { get; set; }

        public bool SendNewTrackingNumberToClient { get; set; }
        public bool SendRefundCompletionToClient { get; set; }


        public bool ReturnShippingFee { get; set; }
        public bool RestockingFee { get; set; }
        public string ChangingFeeReason { get; set; }
        public long? ReturnRequestId { get; set; }

        public decimal? RefundAmount { get;set; }


        public string MarketReturnUrl
        {
            get { return UrlHelper.GetSellarCentralReturnUrl(Market, OrderID); }
        }

        public string MarketOrderUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl(Market, MarketplaceId, OrderID); }
        }

        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderID); }
        }

        public override string ToString()
        {
            var text = ToStringHelper.ToString(this);
            
            if (Items != null)
                foreach (var item in Items)
                    text += item.ToString();
            return text;
        }

        public ReturnOrderViewModel()
        {
            Messages = new List<MessageString>();
        }

        public IList<MessageString> Validate()
        {
            var results = new List<MessageString>();
            foreach (var item in Items)
                if (!item.StyleItemId.HasValue)
                    results.Add(MessageString.Error("Undefined item size"));

            return results;
        }

        public IList<MessageString> ValidateRefundRequest(IUnitOfWork db,
            ILogService log)
        {
            var results = new List<MessageString>();
            if (!DoRefund)
                return results;
            
            var existRefunds = db.SystemActions.GetAllAsDto()
                .Where(a => a.Tag == OrderID
                    && a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                    && (a.Status == (int)SystemActionStatus.Done
                    || a.Status == (int)SystemActionStatus.InProgress
                    || a.Status == (int)SystemActionStatus.None))
                .ToList();

            var requestedAmount = Items.Sum(i => i.RefundItemPrice 
            + (IncludeShipping ? i.RefundShippingPrice : 0) 
            - (DeductShipping ? i.DeductShippingPrice : 0)
            - (DeductPrepaidLabel ? i.DeductPrepaidLabelCost : 0));
            var requestedCount = Items.Sum(i => i.InputQuantity);

            decimal refundedAmount = 0;
            int refundedCount = 0;
            var existRefundItems = new List<ReturnOrderItemInput>();
            for (int i = 0; i < existRefunds.Count; i++)
            {
                var action = existRefunds[i];
                var input = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                refundedAmount += input.Items.Sum(it => it.RefundItemPrice 
                    + (input.IncludeShipping ? it.RefundShippingPrice : 0) 
                    - (input.DeductShipping ? it.DeductShippingPrice : 0)
                    - (input.IsDeductPrepaidLabelCost ? it.DeductPrepaidLabelCost : 0));
                refundedCount += input.Items.Sum(it => it.Quantity);
                existRefundItems.AddRange(input.Items);
            }

            if (refundedAmount + requestedAmount > TotalPrice)
            {
                results.Add(MessageString.Error("Total refund amount exceeded total price"));
            }

            if (refundedCount + requestedCount > Items.Sum(i => i.Quantity))
            {
                results.Add(MessageString.Error("Total refund items exceeded total ordered items"));
            }

            if ((RestockingFee || ReturnShippingFee)
                && String.IsNullOrEmpty(ChangingFeeReason)
                && ReturnRequestId.HasValue
                && Market == MarketType.Walmart)
            {
                results.Add(MessageString.Error("Reason for Charging a Fee field can’t be empty. Please provide the reason why the fee was deducted. That information will be sent to Marketplace"));
            }

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var existRefundQty = existRefundItems.Where(r => r.ItemOrderId == item.ItemOrderId).Sum(r => r.Quantity);
                if (item.Quantity < item.InputQuantity + existRefundQty)
                    results.Add(MessageString.Error("The quantity of items has been specified more than the unreturned remains (ASIN: " + item.ASIN + ")"));
            }

            return results;
        }

        public static CallResult MarkRefundAsProcessed(IUnitOfWork db,
            ILogService log,
            ITime time,
            ISystemActionService actionService,
            long id,
            DateTime when,
            long? by)
        {
            log.Info("MarkRefundAsProcessed");

            var model = new RefundViewModel();

            actionService.SetResult(db, id, SystemActionStatus.Done, null);
            db.Commit();

            return CallResult.Success();
        }

        public IList<MessageString> ExchangeAction(IUnitOfWork db,
            ILogService log,
            ITime time,
            IQuantityManager quantityManager,
            ILabelService labelService,
            IWeightService weightService,
            IShippingService shippingService,
            CompanyDTO company,
            DateTime when,
            long? by)
        {
            var results = new List<MessageString>();
            
            log.Info("Exchange action");

            var orderItems = Items.Select(i => new MailItemViewModel()
            {
                ASIN = i.ASIN,
                ItemOrderId = i.ItemOrderId,
                ItemPriceCurrency = i.PriceCurrency,
                ItemPrice = i.ItemPrice,
                Weight = i.Weight ?? 0,
                Quantity = i.InputQuantity,
                StyleItemId = i.ExchangeStyleItemId,
                StyleString = i.ExchangeStyleString,
                StyleId = i.ExchangeStyleId,
            })
            .Where(i => i.Quantity > 0)
            .ToList();

            //Fill with additional data
            MailViewModel.FillItemsWithAdditionalInfo(db, weightService, OrderID, orderItems.Select(i => i.GetItemDto()).ToList());

            var companyAddress = new CompanyAddressService(company);

            var mailModel = new MailViewModel()
            {
                FromAddress = MailViewModel.GetFromAddress(companyAddress.GetReturnAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon),
                ToAddress = ToAddress,
                IsAddressSwitched = false,
                CancelCurrentOrderLabel = false,
                IsInsured = IsInsured,
                IsSignConfirmation = IsSignConfirmation,
                Notes = Notes,
                OrderID = OrderID,
                WeightLb = WeightLb,
                WeightOz = WeightOz,
                ShippingMethodSelected = ShippingMethodSelected,
                ReasonCode = ReasonCode,
                UpdateAmazon = false,
                TotalPrice = TotalPrice,

                Items = orderItems
            };

            var result = MailViewModel.GenerateLabel(db,
                labelService,
                weightService,
                shippingService,
                mailModel,
                time.GetAppNowTime(),
                by);

            log.Info("Label generated");

            MailViewModel.AddToOrderTracking(log,
                db, 
                mailModel.OrderID, 
                result.Carrier,
                result.TrackingNumber,
                time.GetAppNowTime(),
                by);

            log.Info("Tracking number added to orderTracking");
            results.Add(MessageString.Success("Tracking number was added to Track Orders list"));

            if (OrderEntityId.HasValue && !String.IsNullOrEmpty(OrderComment))
            {
                //var exchangeList = new List<string>();
                //foreach (var item in Items)
                //{
                //    if (item.InputQuantity > 0)
                //        exchangeList.Add(item.ExchangeStyleString + "[" + item.Size + "]");
                //}
            
                //db.OrderComments.Add(new OrderComment()
                //{
                //    OrderId = OrderEntityId.Value,
                //    Message = "[System] Was exchanged to " + String.Join(", ", exchangeList),
                //    CreateDate = when,
                //    CreatedBy = by
                //});

                db.OrderComments.Add(new OrderComment()
                {
                    OrderId = OrderEntityId.Value,
                    Message = OrderComment,
                    Type = (int)CommentType.ReturnExchange,
                    CreateDate = when,
                    CreatedBy = by
                });
                db.Commit();

                results.Add(MessageString.Success("Comment was added"));
            }
            
            if (BackItemsToInventory || SubtractReplacementFromInventory)
            {
                if (Items.Any(i => i.InputQuantity > 0 || i.InputDamagedQuantity > 0))
                {
                    var returnOperation = MakeQuantityOperation(QuantityOperationType.Exchange,
                        OrderID,
                        Items,
                        BackItemsToInventory,
                        SubtractReplacementFromInventory);

                    quantityManager.AddQuantityOperation(db,
                        returnOperation,
                        when,
                        by);

                    log.Info("Added return special case");
                }
                else
                {
                    log.Info("No items. All items damaged");
                }

                if (BackItemsToInventory)
                {
                    if (Items.Any(i => i.InputDamagedQuantity > 0))
                    {
                        var damageOperation = MakeQuantityOperation(QuantityOperationType.Damaged,
                            OrderID,
                            Items,
                            BackItemsToInventory,
                            SubtractReplacementFromInventory);

                        quantityManager.AddQuantityOperation(db,
                            damageOperation,
                            when,
                            by);

                        log.Info("Added damaged special case");
                    }
                    else
                    {
                        log.Info("Not damaged items");
                    }
                }

                results.Add(MessageString.Success("Inventory quantity was adjusted"));
            }


            Messages.AddRange(result.Messages.Select(m => MessageString.Error(m.Text)).ToList());
            IsPrinted = result.Success;
            PrintedLabelUrl = UrlHelper.GetPrintLabelPathById(result.PrintPackId);
            PrintedTrackingNumber = result.TrackingNumber;

            return results;
        }

        public IList<MessageString> ReturnAction(IUnitOfWork db, 
            IQuantityManager quantityManager,
            ISystemActionService actionService,
            ILogService log, 
            ITime time,
            DateTime when,
            long? by)
        {
            var results = new List<MessageString>();

            log.Info("Return action");

            if (BackItemsToInventory)
            {
                if (Items.Any(i => i.InputQuantity > 0))
                {
                    var returnOperation = MakeQuantityOperation(QuantityOperationType.Return,
                        OrderID,
                        Items,
                        true,
                        true);

                    quantityManager.AddQuantityOperation(db,
                        returnOperation,
                        when,
                        by);

                    log.Info("Added return special case");
                }
                else
                {
                    log.Info("No items. All items damaged");
                }

                if (Items.Any(i => i.InputDamagedQuantity > 0))
                {
                    var damageOperation = MakeQuantityOperation(QuantityOperationType.Damaged,
                        OrderID,
                        Items,
                        true,
                        true);

                    quantityManager.AddQuantityOperation(db,
                        damageOperation,
                        when,
                        by);

                    log.Info("Added damaged special case");
                }
                else
                {
                    log.Info("Not damaged items");
                }

                results.Add(MessageString.Success("Inventory quantity was adjusted"));
            }

            if (OrderEntityId.HasValue && !String.IsNullOrEmpty(OrderComment))
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

            if (DoRefund)
            {
                var items = new List<ReturnOrderItemInput>();
                string orderRefundId = "";
                string orderNumber = "";
                string customerOrderNumber = "";
                var tag = "";
                if (ReturnRequestId.HasValue && Market == MarketType.Walmart)
                {                    
                    var refundRequest = db.ReturnRequests.Get(ReturnRequestId.Value);
                    var refundRequestItems = db.ReturnRequestItems.GetAll().Where(r => r.ReturnRequestId == ReturnRequestId.Value).ToList();
                    var order = db.Orders.GetAll().FirstOrDefault(o => o.AmazonIdentifier == refundRequest.OrderNumber);
                    orderRefundId = refundRequest.MarketReturnId;
                    orderNumber = refundRequest.OrderNumber;
                    customerOrderNumber = order.CustomerOrderId;
                    tag = refundRequest.OrderNumber;

                    items = refundRequestItems.Select(i => new ReturnOrderItemInput()
                    {
                        ItemOrderId = i.LineNumber.ToString(),
                        Quantity = i.Quantity ?? 0,
                        RefundItemPrice = i.RefundTotalAmount ?? 0,
                        RefundShippingPrice = 0,
                        DeductShippingPrice = 0,
                        DeductPrepaidLabelCost = 0,
                    }).ToList();
                }
                else
                {
                    orderNumber = OrderID;
                    tag = OrderID;
                    items = Items.Select(i => new ReturnOrderItemInput()
                    {
                        ItemOrderId = i.ItemOrderId,
                        Quantity = i.InputQuantity,
                        RefundItemPrice = i.RefundItemPrice,
                        RefundShippingPrice = i.RefundShippingPrice,
                        DeductShippingPrice = i.DeductShippingPrice,
                        DeductPrepaidLabelCost = i.DeductPrepaidLabelCost,
                    }).ToList();
                }


                actionService.AddAction(db,
                    SystemActionType.UpdateOnMarketReturnOrder,
                    tag,
                    new ReturnOrderInput()
                    {
                        OrderRefundId = orderRefundId,
                        OrderNumber = orderNumber,
                        CustomerOrderNumber = customerOrderNumber,
                        IncludeShipping = IncludeShipping,
                        DeductShipping = DeductShipping,
                        IsDeductPrepaidLabelCost = DeductPrepaidLabel,
                        ChangingFeeReason = ChangingFeeReason,
                        ShippingFee = ReturnShippingFee,
                        RestockingFee = RestockingFee,
                        Items = items,
                        RefundAmount = PriceHelper.RoundToTwoPrecision(RefundAmount),
                    },
                    null,
                    by);

                results.Add(MessageString.Success("Refund request was submitted"));
            }

            //TODO: add action to email

            return results;
        }

        public static ValueMessageResult<RefundViewModel> AcceptReturn(IUnitOfWork db,
            ILogService log,
            IQuantityManager quantityManager,
            ISystemActionService actionService,
            long refundId,
            DateTime when,
            long? by)
        {
            var messages = new List<MessageString>();

            log.Info("Refund action");

            var refundRequest = db.ReturnRequests.Get(refundId);
            var refundRequestItems = db.ReturnRequestItems.GetAll().Where(r => r.ReturnRequestId == refundId).ToList();

            refundRequest.ProcessMode = (int)RefundProcessMode.Submitted;
            db.Commit();

            var actionId = actionService.AddAction(db,
                SystemActionType.UpdateOnMarketReturnOrder,
                refundRequest.OrderNumber,
                new ReturnOrderInput()
                {
                    OrderRefundId = refundRequest.MarketReturnId,
                    OrderNumber = refundRequest.OrderNumber,
                    IncludeShipping = true,
                    DeductShipping = false,
                    RefundReason = (int)RefundReasonCodes.Return,
                    Items = refundRequestItems.Select(i => new ReturnOrderItemInput()
                    {
                        ItemOrderId = i.LineNumber.ToString(),
                        Quantity = i.Quantity ?? 0,
                        RefundItemPrice = i.RefundTotalAmount ?? 0,// i.RefundItemPrice,
                        RefundShippingPrice = 0,// i.RefundShippingPrice,
                        DeductShippingPrice = 0,
                        DeductPrepaidLabelCost = 0,
                    }).ToList()
                },
                null,
                by);

            if(refundRequestItems.Any(i => i.Quantity > 0))
            {
                var returnOperation = MakeQuantityOperation(QuantityOperationType.Return,
                    refundRequest.OrderNumber,
                    refundRequestItems.Select(i => new ReturnQuantityItemViewModel()
                    {
                        InputQuantity = i.Quantity ?? 0,
                        StyleItemId = i.StyleItemId,
                        StyleId = i.StyleId
                    }).ToList(),
                    true,
                    true);

                quantityManager.AddQuantityOperation(db,
                    returnOperation,
                    when,
                    by);

                log.Info("Added return special case");
            }
            else
            {
                log.Info("No items. All items damaged");
            }

            messages.Add(MessageString.Success("Return request was accepted"));

            var actionDto = db.SystemActions.GetAllAsDto().FirstOrDefault(a => a.Id == actionId);
            var newRefund = new RefundViewModel(actionDto);

            return new ValueMessageResult<RefundViewModel>(true, messages, newRefund);
        }

        public IList<MessageString> RefundAction(IUnitOfWork db,
            IQuantityManager quantityManager,
            ISystemActionService actionService,
            ILogService log,
            ITime time,
            DateTime when,
            long? by)
        {
            var results = new List<MessageString>();

            log.Info("Refund action");
            
            if (OrderEntityId.HasValue && !String.IsNullOrEmpty(OrderComment))
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

            if (Market == MarketType.Walmart && ReturnRequestId.HasValue)
            {
                var refundRequest = db.ReturnRequests.Get(ReturnRequestId.Value);
                var refundRequestItems = db.ReturnRequestItems.GetAll().Where(r => r.ReturnRequestId == ReturnRequestId.Value).ToList();

                var actionId = actionService.AddAction(db,
                    SystemActionType.UpdateOnMarketReturnOrder,
                    refundRequest.OrderNumber,
                    new ReturnOrderInput()
                    {
                        OrderRefundId = refundRequest.MarketReturnId,
                        OrderNumber = refundRequest.OrderNumber,
                        ShippingFee = ReturnShippingFee,
                        RestockingFee = RestockingFee,
                        ChangingFeeReason = ChangingFeeReason,
                            
                        RefundReason = (int)RefundReasonCodes.Return,
                        Items = refundRequestItems.Select(i => new ReturnOrderItemInput()
                        {
                            ItemOrderId = i.LineNumber.ToString(),
                            Quantity = i.Quantity ?? 0,
                            RefundItemPrice = i.RefundTotalAmount ?? 0,// i.RefundItemPrice,
                            RefundShippingPrice = 0,// i.RefundShippingPrice,
                            DeductShippingPrice = 0,
                            DeductPrepaidLabelCost = 0,
                        }).ToList()
                    },
                    null,
                    by);
            }
            else
            {
                actionService.AddAction(db,
                    SystemActionType.UpdateOnMarketReturnOrder,
                    OrderID,
                    new ReturnOrderInput()
                    {
                        OrderNumber = OrderID,
                        IncludeShipping = IncludeShipping,
                        DeductShipping = DeductShipping,
                        RefundReason = RefundReasonCode,

                        Items = Items.Select(i => new ReturnOrderItemInput()
                        {
                            ItemOrderId = i.ItemOrderId,
                            Quantity = i.InputQuantity,
                            RefundItemPrice = i.RefundItemPrice,
                            RefundShippingPrice = i.RefundShippingPrice,
                            DeductShippingPrice = i.DeductShippingPrice,
                            DeductPrepaidLabelCost = i.DeductPrepaidLabelCost,
                        }).ToList()
                    },
                    null,
                    by);
            }

            results.Add(MessageString.Success("Refund request was submitted"));

            return results;
        }


        private static QuantityOperationDTO MakeQuantityOperation(QuantityOperationType type,
            string orderId,
            IList<ReturnQuantityItemViewModel> items,
            bool substractReturnedItems,
            bool substractReplacementItems)
        {
            var quantityOperationReturn = new QuantityOperationDTO()
            {
                Type = (int)type,
                OrderId = orderId,
                Comment = "From Return page",
                QuantityChanges = new List<QuantityChangeDTO>()
            };

            foreach (var item in items)
            {
                if (substractReturnedItems)
                {
                    if (type == QuantityOperationType.Exchange || type == QuantityOperationType.Return)
                    {
                        if (item.InputQuantity > 0)
                        {
                            quantityOperationReturn.QuantityChanges.Add(new QuantityChangeDTO()
                            {
                                StyleId = item.StyleId ?? 0,
                                StyleItemId = item.StyleItemId ?? 0,
                                Quantity = -item.InputQuantity
                            });
                        }
                    }
                }

                if (substractReplacementItems)
                {
                    if (type == QuantityOperationType.Exchange)
                    {
                        if (item.InputQuantity > 0)
                        {
                            quantityOperationReturn.QuantityChanges.Add(new QuantityChangeDTO()
                            {
                                StyleId = item.ExchangeStyleId ?? 0,
                                StyleItemId = item.ExchangeStyleItemId ?? 0,
                                Quantity = item.InputQuantity
                            });
                        }
                    }
                }

                if (substractReturnedItems)
                {
                    if (type == QuantityOperationType.Damaged)
                    {
                        if (item.InputDamagedQuantity > 0)
                        {
                            quantityOperationReturn.QuantityChanges.Add(new QuantityChangeDTO()
                            {
                                StyleId = item.StyleId ?? 0,
                                StyleItemId = item.StyleItemId ?? 0,
                                Quantity = item.InputDamagedQuantity
                            });
                        }
                    }
                }
            }

            return quantityOperationReturn;
        }


        public static IList<EmailViewModel> GetEmailsByOrderId(IUnitOfWork db,
            IAddressService addressService,
            string orderId)
        {
            var emails = db.Emails
                .GetAllWithOrder(new EmailSearchFilter() { OrderId = orderId })
                .OrderByDescending(e => e.ReceiveDate)
                .ToList();
            return emails.Select(e => new EmailViewModel()
            {
                Id = e.Id,
                Subject = e.Subject,
                Body = e.Message,
                FromEmail = e.From,
                ReceiveDate = e.ReceiveDate,
                FolderType = e.FolderType,

                OrderMarket = e.Market,
                FromName = e.FolderType == (int)EmailFolders.Sent ? addressService.DefaultName : e.BuyerName,
                OrderNumber = e.OrderIdString,
                OrderDate = e.OrderDate,
            }).ToList();
        }

        public static ReturnOrderViewModel GetByOrderId(IUnitOfWork db, 
            ILogService log,
            IWeightService weightService,
            string searchString)
        {
            DTOOrder order = null;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.RemoveWhitespaces();
                var orderNumber = OrderHelper.RemoveOrderNumberFormat(searchString);

                var byRma = db.ReturnRequests.GetAll().FirstOrDefault(r => r.MarketReturnId == searchString);
                if (byRma != null)
                    orderNumber = byRma.OrderNumber;


                var filter = new OrderSearchFilter()
                {
                    EqualOrderNumber = orderNumber,
                    IncludeMailInfos = true,
                    IncludeNotify = false,
                    
                    UnmaskReferenceStyles = false,
                };

                order = db.ItemOrderMappings
                    .GetFilteredOrdersWithItems(weightService, filter)
                    .FirstOrDefault();
            }
            
            if (order == null)
            {
                return new ReturnOrderViewModel
                {
                    Items = new List<ReturnQuantityItemViewModel>(),
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
                        ShipDate = null
                    },
                    Labels = new List<LabelViewModel>(),
                    Comments = new BindingList<CommentViewModel>(),
                    Refunds = new List<RefundViewModel>(),

                    Notes = String.Empty,
                    OrderID = String.Empty,
                    ShippingMethod = null,
                    WeightLb = null,
                    WeightOz = null,
                };
            }
            else
            {
                var comments = db.OrderComments.GetByOrderIdDto(order.Id)
                    .OrderByDescending(c => c.CreateDate)
                    .Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Type = c.Type,
                        Comment = c.Message,
                        CommentDate = c.CreateDate,
                        CommentByName = c.CreatedByName,
                    }).ToList();

                var returnRequests = db.ReturnRequests.GetAll()
                    .Where(r => r.OrderNumber == order.OrderId)
                    .ToList()
                    .Select(r => new ReturnRequestDTO()
                    {
                        Id = r.Id,
                        ReturnByDate = r.ReturnByDate,
                        MarketReturnId = r.MarketReturnId,
                        Type = r.Type,
                        ProcessMode = r.ProcessMode,
                        Name = DateHelper.ToDateString(r.ReceiveDate) + "-$" + PriceHelper.RoundToTwoPrecision(r.RequestedRefundAmount) + "-" + r.MarketReturnId,
                        OrderNumber = r.OrderNumber,
                        ReceiveDate = r.ReceiveDate,
                        ItemName = r.ItemName,
                        CustomerComments = r.CustomerComments,
                        Details = r.Details,
                        PrepaidLabelCost = r.PrepaidLabelCost ?? 0,
                        PrepaidLabelBy = r.PrepaidLabelBy,
                        HasPrepaidLabel = r.HasPrepaidLabel,
                        CreateDate = r.CreateDate,
                        Reason = r.Reason,
                        RequestedRefundAmount = r.RequestedRefundAmount,
                    })                    
                    .OrderBy(r => r.CreateDate)
                    .ToList();
                
                var refundRequestList = RefundViewModel.GetByOrderId(db, order.OrderId);

                var sourceRefunds = (from a in db.SystemActions.GetAllAsDtoWithUser()
                                        where a.Tag == order.OrderId
                                                && a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                                        select a).ToList();

                var labels = order.ShippingInfos
                    .Where(i => !String.IsNullOrEmpty(i.LabelPath) 
                        //|| i.LabelPurchaseResult != null //NOTE: in case when bag system shows user issue (keeped twice shippings with labels)
                    )
                    .Select(sh => new LabelViewModel()
                    {
                        Carrier = sh.ShippingMethod.CarrierName,
                        TrackingNumber = sh.TrackingNumber,
                        DeliveredStatus = sh.DeliveredStatus,
                        ActualDeliveryDate = sh.ActualDeliveryDate,
                        TrackingStateDate = sh.TrackingStateDate,
                        TrackingStateEvent = sh.TrackingStateEvent,

                        ShippingCountry = order.FinalShippingCountry,
                        EstDeliveryDate = order.LatestDeliveryDate,

                        NumberInBatch = sh.NumberInBatch,
                        Path = sh.LabelPath,
                        PurchaseMessage = sh.LabelPurchaseMessage,
                        PurchaseResult = sh.LabelPurchaseResult,
                        PurchaseDate = sh.LabelPurchaseDate,
                        FromType = LabelFromType.Batch,
                        IsCanceled = sh.LabelCanceled
                    })
                    .ToList();

                labels.AddRange(order.MailInfos.Select(mi => new LabelViewModel()
                {
                    Carrier = mi.ShippingMethod.CarrierName,
                    TrackingNumber = mi.TrackingNumber,
                    DeliveredStatus = mi.DeliveredStatus,
                    ActualDeliveryDate = mi.ActualDeliveryDate,
                    TrackingStateDate = mi.TrackingStateDate,
                    TrackingStateEvent = mi.TrackingStateEvent,
                    
                    ShippingCountry = order.FinalShippingCountry,
                    EstDeliveryDate = order.LatestDeliveryDate,

                    Path = mi.LabelPath,
                    PurchaseMessage = mi.LabelPurchaseMessage,
                    PurchaseResult = mi.LabelPurchaseResult,
                    PurchaseDate = mi.LabelPurchaseDate,
                    FromType = LabelFromType.Mail,
                    MailReasonId = mi.MailReasonId,
                    IsCanceled = mi.LabelCanceled
                }));

                labels = labels.OrderBy(l => l.PurchaseDate).ToList();

                var shippingPrice = 0M;
                
                var activeShippings = order.ShippingInfos.Where(i => i.IsActive).ToList();
                if (order.MailInfos.Any(m => !m.LabelCanceled)
                    && order.ShippingInfos.All(sh => String.IsNullOrEmpty(sh.LabelPath) || sh.LabelCanceled))
                {
                    var mailShipping = order.MailInfos.OrderBy(m => m.LabelPurchaseDate).FirstOrDefault(l => !l.LabelCanceled);
                    activeShippings = new List<OrderShippingInfoDTO>() {mailShipping};
                }
                shippingPrice = activeShippings.Sum(sh => sh.StampsShippingCost ?? 0);
                
                var address = order.GetAddressDto();

                AdjustItemPrice(order);

                var model = new ReturnOrderViewModel
                {
                    //Notes =  string.Format("{0} {1}", order.OrderId, itemsNotes),
                    OrderID = order.OrderId,
                    OrderEntityId = order.Id,
                    OrderDate = order.OrderDate,
                    Market = (MarketType)order.Market,
                    MarketplaceId = order.MarketplaceId,

                    DeliveryDate = activeShippings.Max(sh => sh.ActualDeliveryDate),
                    ExpDeliveryDate = order.EarliestDeliveryDate,

                    PrepaidLabelCost = returnRequests != null ? returnRequests.Max(r => r.PrepaidLabelCost) : null,
                    DeductPrepaidLabel = returnRequests != null ? returnRequests.Any(r => r.HasPrepaidLabel ?? false) : false,
                    ReturnRequests = returnRequests,
                    Comments = comments,
                    Refunds = refundRequestList,

                    WeightLb = (int)Math.Floor(order.WeightD / 16),
                    WeightOz = order.WeightD % 16,
                    TotalPrice = order.TotalPrice,
                    DiscountAmount = (order.DiscountAmount ?? 0) + order.Items.Sum(i => i.PromotionDiscount ?? 0),
                    TaxAmount = order.TaxAmount ?? 0 + order.Items.Sum(i => i.ItemTax ?? 0),
                    PriceCurrency = PriceHelper.FormatCurrency(order.TotalPriceCurrency),

                    ActualShippingCost = shippingPrice,

                    Items = order.Items.Select(i => new ReturnQuantityItemViewModel(i)).ToList(),
                    Labels = labels,

                    ToAddress = new AddressViewModel(address),

                    IsInsured = order.IsInsured,
                    IsRefundLocked = order.IsRefundLocked ?? false,
                };

                var refundInfoes = sourceRefunds.Select(r => JsonConvert.DeserializeObject<ReturnOrderInput>(r.InputData));
                foreach (var item in model.Items)
                {
                    foreach (var refundInfo in refundInfoes)
                    {
                        if (refundInfo.Items != null)
                        {
                            foreach (var refundItem in refundInfo.Items)
                            {
                                if (refundItem.ItemOrderId == item.ItemOrderId)
                                {
                                    item.ItemPrice -= refundItem.RefundItemPrice;
                                    item.ItemPriceInUSD -= refundItem.RefundItemPrice;
                                    if (refundInfo.IncludeShipping)
                                    {
                                        item.ShippingPrice -= refundItem.RefundShippingPrice;
                                    }                                    
                                    item.InputQuantity -= refundItem.Quantity;
                                }
                            }
                        }
                    }
                }                

                return model;
            }
        }

        private static void AdjustItemPrice(DTOOrder order)
        {
            if (order.Market == (int)MarketType.Amazon
                || order.Market == (int)MarketType.AmazonEU
                || order.Market == (int)MarketType.AmazonAU)
            {
                var totalSum = order.Items.Sum(i => i.ItemPaid);
                foreach (var item in order.Items)
                {
                    item.PromotionDiscount = item.PromotionDiscount ?? 0;
                    if (totalSum > 0)
                        item.PromotionDiscount += order.DiscountAmount * item.ItemPaid / (decimal)totalSum;
                    item.ItemPrice -= item.PromotionDiscount ?? 0;
                }
            }
        }

        public static IList<ReturnQuantityItemViewModel> GetExchangeInfo(IUnitOfWork db,
            IWeightService weightService,
            string orderId)
        {
            if (String.IsNullOrEmpty(orderId))
                return null;

            orderId = orderId.RemoveWhitespaces();
            var orderNumber = OrderHelper.RemoveOrderNumberFormat(orderId);
            var filter = new OrderSearchFilter()
            {
                EqualOrderNumber = orderNumber,
                IncludeMailInfos = true,
                IncludeNotify = false,

                UnmaskReferenceStyles = false,
            };

            DTOOrder order = null;
            order = db.ItemOrderMappings
                .GetFilteredOrdersWithItems(weightService, filter)
                .FirstOrDefault();

            if (order == null)
                return null;

            return order.Items.Select(i => new ReturnQuantityItemViewModel(i)).ToList();
        }


        private static Dictionary<int, string> _refundReasonCodeNames = new Dictionary<int, string>()
        {
            { (int)RefundReasonCodes.Oversold, RefundReasonCodeHelper.GetName(RefundReasonCodes.Oversold) },
            { (int)RefundReasonCodes.Late, RefundReasonCodeHelper.GetName(RefundReasonCodes.Late) },
            { (int)RefundReasonCodes.Lost, RefundReasonCodeHelper.GetName(RefundReasonCodes.Lost)},
            { (int)RefundReasonCodes.Return, RefundReasonCodeHelper.GetName(RefundReasonCodes.Return)},
            { (int)RefundReasonCodes.Other, RefundReasonCodeHelper.GetName(RefundReasonCodes.Other) },
        };

        public static string GetRefundReasonName(int reasonCode)
        {
            if (_refundReasonCodeNames.ContainsKey(reasonCode))
                return _refundReasonCodeNames[reasonCode];
            return "";
        }

        public static SelectList RefundReasons
        {
            get
            {
                return new SelectList(_refundReasonCodeNames.ToList(), "Key", "Value");
            }
        }
    }
}