using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models.SearchFilters;
using Ccen.Common.Helpers;

namespace Amazon.Web.ViewModels.Inventory
{
    public class QuantityOperationViewModel
    {
        public long Id { get; set; }
        public QuantityOperationType Type { get; set; }
        public string Comment { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        //INPUT
        public string StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public int? Quantity { get; set; }

        public string OrderId { get; set; }
        public int? Market { get; set; }
        public string MarketplaceId { get; set; }
        public string OrderItemId { get; set; }
        public IList<DTOOrderItem> InputOrderItems { get; set; }



        public string StyleIdToRemove { get; set; }
        public long? StyleItemIdToRemove { get; set; }
        public int? QuantityToRemove { get; set; }


        public string SellerUrl
        {
            get
            {
                if (Market.HasValue)
                    return Amazon.Web.Models.UrlHelper.GetSellerCentralOrderUrl((MarketType)Market, MarketplaceId, OrderId);
                return null;
            }
        }



        public IEnumerable<QuantityChangeViewModel> QuantityChanges { get; set; }
        
        public string TypeAsString
        {
            get
            {
                return TypeToString(Type);
            }
        }



        private static string TypeToString(QuantityOperationType type)
        {

            return EnumHelper<QuantityOperationType>.GetEnumDescription(type.ToString());
        }

        public static SelectList TypeList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Exchange), ((int)QuantityOperationType.Exchange).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Return), ((int)QuantityOperationType.Return).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.ReturnOnHold), ((int)QuantityOperationType.ReturnOnHold).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.ExchangeOnHold), ((int)QuantityOperationType.ExchangeOnHold).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Replacement), ((int)QuantityOperationType.Replacement).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Lost), ((int)QuantityOperationType.Lost).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Damaged), ((int)QuantityOperationType.Damaged).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.InvalidBox), ((int)QuantityOperationType.InvalidBox).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.SoldOutside), ((int)QuantityOperationType.SoldOutside).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.StoreManual), ((int)QuantityOperationType.StoreManual).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.Wholesale), ((int)QuantityOperationType.Wholesale).ToString()),
                    new KeyValuePair<string, string>(TypeToString(QuantityOperationType.CompensationGift), ((int)QuantityOperationType.CompensationGift).ToString()),
                }, "Value", "Key");
            }
        }


        public QuantityOperationViewModel()
        {
            Type = QuantityOperationType.Exchange;
        }


        public List<ValidationResult> Validate(IUnitOfWork db)
        {
            var result = new List<ValidationResult>();

            if (Type == QuantityOperationType.Exchange
                || Type == QuantityOperationType.Return
                || Type == QuantityOperationType.ReturnOnHold
                || Type == QuantityOperationType.ExchangeOnHold
                || Type == QuantityOperationType.Replacement
                || Type == QuantityOperationType.CompensationGift)
            {
                var orderItems = db.Listings.GetOrderItems(OrderId);
                if (orderItems.Count == 0)
                {
                    result.Add(new ValidationResult("Specified OrderId is not found in the system"));
                }
            }

            if (Type == QuantityOperationType.Exchange
                || Type == QuantityOperationType.Lost
                || Type == QuantityOperationType.Damaged
                || Type == QuantityOperationType.InvalidBox
                || Type == QuantityOperationType.CompensationGift)
            {
                var style = db.Styles.GetActiveByStyleIdAsDto(StyleId);
                if (style == null)
                {
                    result.Add(new ValidationResult("Specified StyleId is not found in the system"));
                }

                if (!StyleItemId.HasValue)
                {
                    result.Add(new ValidationResult("Size is not specified"));
                }
                if (!Quantity.HasValue)
                {
                    result.Add(new ValidationResult("Quantity is not sepcified"));
                }
            }

            if (Type == QuantityOperationType.InvalidBox)
            {
                var style = db.Styles.GetActiveByStyleIdAsDto(StyleIdToRemove);
                if (style == null)
                {
                    result.Add(new ValidationResult("Specified StyleId (to remove) is not found in the system"));
                }
                if (!StyleItemIdToRemove.HasValue)
                {
                    result.Add(new ValidationResult("Size (to remove) is not specified"));
                }
                if (!QuantityToRemove.HasValue)
                {
                    result.Add(new ValidationResult("Quantity (to remove) is not sepcified"));
                }
            }

            return result;
        }

        public QuantityOperationViewModel(QuantityOperationDTO quantityOperationDto)
        {
            Id = quantityOperationDto.Id;
            Type = (QuantityOperationType)quantityOperationDto.Type;
            OrderId = quantityOperationDto.OrderId;
            Market = quantityOperationDto.Market;
            MarketplaceId = quantityOperationDto.MarketplaceId;
            Comment = quantityOperationDto.Comment;
            CreateDate = quantityOperationDto.CreateDate;
            CreatedBy = quantityOperationDto.CreatedBy;
            CreatedByName = quantityOperationDto.CreatedByName;

            QuantityChanges = quantityOperationDto.QuantityChanges
                .Select(q => new QuantityChangeViewModel()
                {
                    Id = q.Id,
                    Quantity = q.Quantity,
                    Size = q.Size,
                    StyleItemId = q.StyleItemId,
                    StyleId = q.StyleId,
                    StyleString = q.StyleString,
                    InActive = q.InActive
                })
                .OrderBy(q => q.Quantity)
                .ToList();
        }

        static public void Delete(IUnitOfWork db,
            IQuantityManager quantityManager,
            ICacheService cache,
            long operationId,
            DateTime when,
            long? by)
        {
            var operation = db.QuantityOperations.Get(operationId);
            var operationChanges = db.QuantityChanges
                .GetAll()
                .Where(ch => ch.QuantityOperationId == operationId)
                .ToList();
            db.QuantityOperations.Remove(operation);
            db.Commit();

            foreach (var change in operationChanges)
            {
                quantityManager.LogStyleItemQuantity(db,
                    change.StyleItemId,
                    -change.Quantity,
                    null,
                    QuantityChangeSourceType.RemoveSpecialCase, 
                    null,
                    change.Id,
                    StringHelper.Substring(StringHelper.GetFirstNotEmpty(operation.OrderId), 50),
                    when,
                    by);
                cache.RequestStyleItemIdUpdates(db, new List<long>() { change.StyleItemId }, by);
            }
        }

        public long Add(IUnitOfWork db,
            IQuantityManager quantityManager,
            ICacheService cache,
            DateTime when,
            long? by)
        {
            IList<ListingOrderDTO> orderItems;
            StyleEntireDto style;

            var operationDto = new QuantityOperationDTO();
            operationDto.Type = (int)Type;
            operationDto.OrderId = OrderId;
            operationDto.Comment = Comment;
            operationDto.QuantityChanges = new List<QuantityChangeDTO>();
            switch (Type)
            {
                case QuantityOperationType.Exchange:
                    orderItems = db.Listings.GetOrderItems(OrderId);
                    var current = orderItems.FirstOrDefault(o => o.ItemOrderId == OrderItemId);
                    if (current != null)
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = current.StyleId ?? 0,
                            StyleItemId = current.StyleItemId ?? 0,
                            Quantity = -(Quantity ?? 0),
                        });

                    style = db.Styles.GetActiveByStyleIdAsDto(StyleId);
                    if (style != null)
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = style.Id,
                            StyleItemId = StyleItemId ?? 0,
                            Quantity = Quantity ?? 0
                        });
                    break;
                case QuantityOperationType.Return:
                    orderItems = db.Listings.GetOrderItems(OrderId);
                    foreach (var item in orderItems)
                    {
                        if (InputOrderItems != null)
                        {
                            var inputItem = InputOrderItems.FirstOrDefault(i => i.ItemOrderId == item.ItemOrderId);
                            if (inputItem != null)
                                item.QuantityOrdered = inputItem.Quantity;
                        }

                        if (item.QuantityOrdered > 0)
                        {
                            operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                            {
                                StyleId = item.StyleId ?? 0,
                                StyleItemId = item.StyleItemId ?? 0,
                                Quantity = -item.QuantityOrdered,
                            });
                        }
                    }
                    break;
                case QuantityOperationType.ExchangeOnHold:
                case QuantityOperationType.ReturnOnHold:
                    orderItems = db.Listings.GetOrderItems(OrderId);
                    foreach (var item in orderItems)
                    {
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = item.StyleId ?? 0,
                            StyleItemId = item.StyleItemId ?? 0,
                            Quantity = -item.QuantityOrdered,
                            InActive = true
                        });
                    }
                    break;
                case QuantityOperationType.Replacement:
                    orderItems = db.Listings.GetOrderItems(OrderId);
                    foreach (var item in orderItems)
                    {
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = item.StyleId ?? 0,
                            StyleItemId = item.StyleItemId ?? 0,
                            Quantity = item.QuantityOrdered,
                        });
                    }
                    break;
                case QuantityOperationType.Lost:
                case QuantityOperationType.SoldOutside:
                case QuantityOperationType.StoreManual:
                case QuantityOperationType.Damaged:
                case QuantityOperationType.CompensationGift:
                    style = db.Styles.GetActiveByStyleIdAsDto(StyleId);
                    if (style != null)
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = style.Id,
                            StyleItemId = StyleItemId ?? 0,
                            Quantity = Quantity ?? 0,
                        });
                    break;
                case QuantityOperationType.InvalidBox:
                    style = db.Styles.GetActiveByStyleIdAsDto(StyleId);
                    if (style != null)
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = style.Id,
                            StyleItemId = StyleItemId ?? 0,
                            Quantity = Quantity ?? 0,
                        });
                    style = db.Styles.GetActiveByStyleIdAsDto(StyleIdToRemove);
                    if (style != null)
                        operationDto.QuantityChanges.Add(new QuantityChangeDTO()
                        {
                            StyleId = style.Id,
                            StyleItemId = StyleItemIdToRemove ?? 0,
                            Quantity = QuantityToRemove ?? 0,
                        });
                    break;
            }

            var operationId = quantityManager.AddQuantityOperation(db,
                operationDto,
                when,
                by);

            if (StyleItemId.HasValue)
                cache.RequestStyleItemIdUpdates(db, new List<long>() { StyleItemId.Value }, by);

            if (StyleItemIdToRemove.HasValue)
                cache.RequestStyleItemIdUpdates(db, new List<long>() { StyleItemIdToRemove.Value }, by);

            return operationId;
        }

        public static IEnumerable<QuantityOperationViewModel> GetAll(IUnitOfWork db,
            QuantityOperationFilterViewModel filter)
        {
            if (filter.LimitCount == 0)
                filter.LimitCount = 50;
            var query = db.QuantityOperations.GetAllAsDtoWithChanges();
            if (!String.IsNullOrEmpty(filter.OrderNumber))
                query = query.Where(o => o.OrderId == filter.OrderNumber);
            if (!String.IsNullOrEmpty(filter.StyleString))
                query = query.Where(o => o.QuantityChangesEnumerable.Any(ch => ch.StyleString == filter.StyleString));
            if (filter.StyleItemId.HasValue)
                query = query.Where(o => o.QuantityChangesEnumerable.Any(ch => ch.StyleItemId == filter.StyleItemId));
            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.CreateDate !=null && o.CreateDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                query = query.Where(o => o.CreateDate != null && o.CreateDate < filter.DateTo.Value);
            if (filter.Type.HasValue)
            {
                var typeId = (int)filter.Type;
                query = query.Where(o => o.Type == typeId);
            }
            if (filter.UserId.HasValue)
            {
                query = query.Where(x => x.CreatedBy != null && x.CreatedBy.Value == filter.UserId.Value);
            } 

           

            var items = query.Select(quantityOperationDto => new QuantityOperationViewModel() 
            {
                Id = quantityOperationDto.Id,
                Type = (QuantityOperationType)quantityOperationDto.Type,
                OrderId = quantityOperationDto.OrderId,
                Market = quantityOperationDto.Market,
                MarketplaceId = quantityOperationDto.MarketplaceId,
                Comment = quantityOperationDto.Comment,
                CreateDate = quantityOperationDto.CreateDate,
                CreatedBy = quantityOperationDto.CreatedBy,
                CreatedByName = quantityOperationDto.CreatedByName,
                QuantityChanges = quantityOperationDto.QuantityChangesEnumerable
                    .Select(q => new QuantityChangeViewModel()
                    {
                        Id = q.Id,
                        Quantity = q.Quantity,
                        Size = q.Size,
                        StyleItemId = q.StyleItemId,
                        StyleId = q.StyleId,
                        StyleString = q.StyleString,
                        InActive = q.InActive
                    })                    
            });

            items = items.OrderByDescending(i => i.CreateDate);

            return items;
        }
    }
}