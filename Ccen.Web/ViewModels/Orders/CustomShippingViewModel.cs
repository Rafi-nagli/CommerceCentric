using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;

namespace Amazon.Web.ViewModels.Orders
{
    public class CustomShippingViewModel
    {
        public long? OrderId { get; set; }
        public IList<CustomShippingItemViewModel> Items { get; set; }
        public IList<SelectListItemTag> PackageList { get; set; }

        public CustomShippingViewModel()
        {
            
        }

        public static CustomShippingViewModel Get(IUnitOfWork db, long orderId, long? defaultShippingMethodId)
        {
            var result = new CustomShippingViewModel();

            var order = db.Orders.GetById(orderId);
            var isIntl = ShippingUtils.IsInternational(order.ShippingCountry);
            var shippingMethods = db.ShippingMethods.GetAllAsDto()
                    .Where(m => m.ShipmentProviderType == order.ShipmentProviderType
                        && m.IsInternational == isIntl
                        && m.IsActive)
                    .ToList();

            result.PackageList = new List<SelectListItemTag>();
            foreach (var shippingMethod in shippingMethods)
            {
                for (int i = 0; i <= 6; i++)
                {
                    result.PackageList.Add(new SelectListItemTag()
                    {
                        Text = shippingMethod.Name + " - #" + (i + 1),
                        Value = shippingMethod.Id.ToString() + "-" + (i + 1),
                    });
                }
            }

            var customShippings = db.OrderShippingInfos.GetAllAsDto()
                .Where(sh => sh.OrderId == orderId
                             && sh.ShippingGroupId == RateHelper.CustomPartialGroupId)
                .OrderBy(sh => sh.ShippingNumber)
                .ToList();

            var orderItems = db.Listings.GetOrderItems(orderId)
                .OrderBy(oi => oi.ItemOrderId)
                .ToList();

            //var orderItems = db.OrderItems.GetWithListingInfo()
            //    .Where(oi => oi.OrderId == orderId)
            //    .OrderBy(oi => oi.ItemOrderId)
            //    .ToList();

            var itemToShipping = new List<OrderShippingInfoDTO>();
            if (customShippings.Any())
            {
                var shippingIds = customShippings.Select(sh => sh.Id).ToList();

                itemToShipping = (from m in db.ItemOrderMappings.GetAll()
                    join sh in db.OrderShippingInfos.GetAll() on m.ShippingInfoId equals sh.Id
                    join sm in db.ShippingMethods.GetAll() on sh.ShippingMethodId equals sm.Id
                    where shippingIds.Contains(m.ShippingInfoId)
                    select new OrderShippingInfoDTO()
                    {
                        Id = m.ShippingInfoId,
                        Items = new List<DTOOrderItem>()
                        {
                            new DTOOrderItem()
                            {
                                OrderItemEntityId = m.OrderItemId,
                                Quantity = m.MappedQuantity
                            }
                        },
                        ShippingMethod = new ShippingMethodDTO()
                        {
                            Id = sm.Id,
                            Name = sm.Name,
                            ShortName = sm.ShortName,
                            RequiredPackageSize = sm.RequiredPackageSize
                        }
                    }).ToList();
            }

            var shippingNumbers = new Dictionary<long, int>();
            var shippings = itemToShipping.GroupBy(sh => sh.Id).Select(sh => sh.First()).ToList();
            var processedShippings = new List<OrderShippingInfoDTO>();
            foreach (var shipping in shippings)
            {
                var count = processedShippings.Count(sh => sh.ShippingMethod.Id == shipping.ShippingMethod.Id);
                shippingNumbers.Add(shipping.Id, count + 1);
                processedShippings.Add(shipping);
            }

            ShippingMethodDTO defaultShippingMethod = null;
            if (defaultShippingMethodId.HasValue)
                defaultShippingMethod = shippingMethods.FirstOrDefault(m => m.Id == defaultShippingMethodId.Value);
            if (defaultShippingMethod == null)
                defaultShippingMethod = shippingMethods.FirstOrDefault(m => m.Id == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                    || m.Id == ShippingUtils.FedexOneRate2DayPak);

            var defaultPackageNumber = defaultShippingMethod != null ? 1 : (int?)null;

            var items = new List<CustomShippingItemViewModel>();
            foreach (var orderItem in orderItems)
            {
                for (int q = 0; q < orderItem.QuantityOrdered; q++)
                {
                    var existCustomMapping = itemToShipping.FirstOrDefault(i => i.Items.First().OrderItemEntityId == orderItem.OrderItemEntityId
                        && i.Items.First().Quantity > 0);

                    if (existCustomMapping != null)
                        existCustomMapping.Items.First().Quantity--;

                    var image = orderItem.Picture;
                    if (String.IsNullOrEmpty(image))
                        image = orderItem.StyleImage;
                    if (orderItem.ReplaceType == (int)ItemReplaceTypes.Combined)
                        image = orderItem.StyleImage;
                    var pictureUrl = ImageHelper.GetFirstOrDefaultPicture(image);

                    items.Add(new CustomShippingItemViewModel()
                    {
                        ASIN = orderItem.ASIN,
                        StyleString = orderItem.StyleID,
                        StyleSize = orderItem.StyleSize,

                        Weight = orderItem.Weight ?? 0,

                        PictureUrl = pictureUrl,

                        OrderItemId = orderItem.OrderItemEntityId,
                        ShippingInfoId = existCustomMapping != null ? existCustomMapping.Id : (long?)null,
                        ShippingMethodName = existCustomMapping != null ? existCustomMapping.ShippingMethod.Name : defaultShippingMethod?.Name,
                        ShippingMethodId = existCustomMapping != null ? existCustomMapping.ShippingMethod.Id : defaultShippingMethod?.Id,

                        PackageNumber = existCustomMapping != null ? shippingNumbers[existCustomMapping.Id] : defaultPackageNumber,
                    });
                }
            }

            items.ForEach(i =>
            {
                if (i.PackageNumber.HasValue && i.ShippingMethodId.HasValue)
                    i.PackageValue = i.ShippingMethodId.Value + "-" + i.PackageNumber;
            });

            result.Items = items.OrderBy(i => i.ShippingMethodId)
                .ThenBy(i => i.PackageNumber)
                .ThenBy(i => i.StyleString) //TODO: by location
                .ThenBy(i => SizeHelper.GetSizeIndex(i.StyleSize))
                .ToList();

            return result;
        }
    }
}