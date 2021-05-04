using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Inventory;
using Amazon.DTO.ScanOrders;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Services.Models;
using Ccen.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Amazon.Web.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class InventoryAppService : IInventoryAppService
    {
        public BarcodeInfo GetInfoByBarcode(string barcode)
        {
            using (var db = new UnitOfWork(null))
            {
                var info = db.StyleItems.GetFullBarcodeInfo(barcode);
                if (info != null)
                    return new BarcodeInfo()
                    {
                        Barcode = info.Barcode,
                        Image = UrlHelper.GetThumbnailUrl(info.Picture, 75, 75, false, ImageHelper.NO_IMAGE_URL, false, true, convertInDomainUrlToThumbnail: true),
                        Size = info.Size,
                        StyleId = info.StyleId,
                        Quantity = info.RemainingQuantity ?? 0,
                    };
            }
            return null;
        }

        public BarcodeInfo[] GetAllBarcodes()
        {
            using (var db = new Amazon.DAL.Inventory.InventoryUnitOfWork())
            {
                return db.Context.ViewBarcodes
                    .Select(b => new BarcodeInfo()
                    {
                        Barcode = b.Barcode
                    }).ToArray();
            }
        }

        [Obsolete]
        public BarcodeInfo[] GetFBAPickList(string type)
        {
            using (var db = new UnitOfWork(null))
            {
                var results = new List<BarcodeInfo>();
                var pickList = db.FBAPickLists.GetAllAsDto(GetShipmentsTypeEnum(type))
                    .OrderByDescending(f => f.CreateDate)
                    .FirstOrDefault();

                if (pickList != null)
                {
                    var pickListItems = from pi in db.FBAPickListEntries.GetAllAsDto()
                                        join l in db.Listings.GetAll() on pi.ListingId equals l.Id
                                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                                        join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                        join si in db.StyleItems.GetAll() on i.StyleItemId equals si.Id
                                        where pi.FBAPickListId == pickList.Id
                                        select new BarcodeInfo()
                                        {
                                            Barcode = i.Barcode,
                                            Quantity = pi.Quantity,
                                            StyleId = st.StyleID,
                                            Size = si.Size,
                                            Image = st.Image,
                                        };

                    results = pickListItems.ToList()
                        .OrderBy(pi => pi.StyleId)
                        .ThenBy(pi => SizeHelper.GetSizeIndex(pi.Size))
                        .ToList();
                }

                return results.ToArray();
            }
        }

        public BarcodeInfo[] GetLastFBAPickList(string type)
        {
            using (var db = new UnitOfWork(null))
            {
                var results = new List<BarcodeInfo>();
                var pickList = db.FBAPickLists.GetAllAsDto(GetShipmentsTypeEnum(type))
                    .OrderByDescending(f => f.CreateDate)
                    .FirstOrDefault();

                if (pickList != null)
                {
                    var pickListItems = from pi in db.FBAPickListEntries.GetAllAsDto()
                                        join l in db.Listings.GetAll() on pi.ListingId equals l.Id
                                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                                        join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                        join si in db.StyleItems.GetAll() on i.StyleItemId equals si.Id
                                        where pi.FBAPickListId == pickList.Id
                                        select new BarcodeInfo()
                                        {
                                            Barcode = i.Barcode,
                                            Quantity = pi.Quantity,
                                            StyleId = st.StyleID,
                                            Size = si.Size,
                                            Image = st.Image,
                                        };

                    results = pickListItems.ToList()
                        .OrderBy(pi => pi.StyleId)
                        .ThenBy(pi => SizeHelper.GetSizeIndex(pi.Size))
                        .ToList();
                }

                return results.ToArray();
            }
        }

        private ShipmentsTypeEnum GetShipmentsTypeEnum(string type)
        {
            switch (type)
            {
                case "FBA": return ShipmentsTypeEnum.FBA;
                case "WFS": return ShipmentsTypeEnum.WFS;
                default:
                    return ShipmentsTypeEnum.FBA;
            }
        }

        public void StoreOrderInfo(OrderInfo order)
        {
            var dbFactory = new DbFactory();
            var log = LogFactory.Default;
            var time = new TimeService(dbFactory);

            var quantityManager = new QuantityManager(log, time);

            using (var invDb = new Amazon.DAL.Inventory.InventoryUnitOfWork())
            {
                if (order.Type == InventoryOrderType.InventoryOrder)
                {
                    var orderDto = new InventoryDTO()
                    {
                        Description = order.Name,
                        FileName = order.FileName,
                        InventoryDate = DateHelper.ConvertUtcToApp(order.OrderDate)
                    };

                    var itemsDto = order.Barcodes.Select(b => new ScanItemDTO()
                    {
                        Barcode = b.Barcode,
                        Quantity = b.Quantity
                    }).ToList();

                    invDb.ItemInventoryMappings.AddNewInventory(orderDto, itemsDto);
                }

                if (order.Type == InventoryOrderType.ShopOrder
                    || order.Type == InventoryOrderType.FBAOrder)
                {
                    var orderDto = new ScanOrderDTO()
                    {
                        Description = order.Name,
                        FileName = order.FileName,
                        OrderDate = DateHelper.ConvertUtcToApp(order.OrderDate),
                        IsFBA = order.Type == InventoryOrderType.FBAOrder
                    };

                    var itemsDto = order.Barcodes.Select(b => new ScanItemDTO()
                    {
                        Barcode = b.Barcode,
                        Quantity = b.Quantity
                    }).ToList();

                    invDb.ItemOrderMappings.AddNewOrder(orderDto, itemsDto);

                    try
                    {
                        using (var db = dbFactory.GetRWDb())
                        {
                            foreach (var item in itemsDto)
                            {
                                var barcodeDto =
                                    db.StyleItemBarcodes.GetAllAsDto().FirstOrDefault(b => b.Barcode == item.Barcode);
                                if (barcodeDto != null)
                                {
                                    quantityManager.LogStyleItemQuantity(db,
                                        barcodeDto.StyleItemId,
                                        item.Quantity,
                                        null,
                                        order.Type == InventoryOrderType.ShopOrder
                                            ? QuantityChangeSourceType.SentToStore
                                            : QuantityChangeSourceType.SentToFBA,
                                        orderDto.Id.ToString(),
                                        item.Id,
                                        StringHelper.Substring(orderDto.Description, 50),
                                        time.GetAppNowTime(),
                                        null);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("When write style item log", ex);
                    }
                }
            }
        }
    }
}
