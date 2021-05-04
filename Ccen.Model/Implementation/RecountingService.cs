using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Model.Implementation
{
    public class RecountingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public RecountingService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void ProcessStatuses()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var baseStyleIdListToCheck = (from si in db.StyleItems.GetAll()
                    join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                    where !String.IsNullOrEmpty(si.LiteCountingStatus)
                          && si.LiteCountingStatus != CountingStatusesEx.None
                          && !si.LiteCountingProcessed
                    select si.StyleId).ToList();

                var allStyleItems = (from si in db.StyleItems.GetAll()
                    join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                    where baseStyleIdListToCheck.Contains(si.StyleId)
                    select new StyleItemDTO()
                    {
                        StyleItemId = si.Id,
                        StyleId = si.StyleId,
                        RemainingQuantity = sic.RemainingQuantity,
                        LiteCountingProcessed = si.LiteCountingProcessed,
                        LiteCountingStatus = si.LiteCountingStatus,
                        ApproveStatus = si.ApproveStatus,
                    }).ToList();

                var allCountingOpenBoxes = (from b in db.OpenBoxCountings.GetAll()
                                                          join bi in db.OpenBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                                          select new 
                                                          {
                                                              StyleId = b.StyleId,
                                                              BoxQuantity = b.BoxQuantity,
                                                              StyleItemId = bi.StyleItemId,
                                                              Quantity = bi.Quantity,
                                                          }).ToList();

                var allCountingSealedBoxes = (from b in db.SealedBoxCountings.GetAll()
                                 join bi in db.SealedBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                 select new 
                                 {
                                     StyleId = b.StyleId,
                                     BoxQuantity = b.BoxQuantity,
                                     StyleItemId = bi.StyleItemId,
                                     Quantity = bi.BreakDown,
                                 }).ToList();

                var styleIdListToUpdate = new List<long>();

                foreach (var styleId in baseStyleIdListToCheck)
                {
                    var styleItems = allStyleItems.Where(si => si.StyleId == styleId).ToList();
                    
                    if (styleItems.Any()
                        && styleItems.All(si => ToUpdate(si,
                        allCountingSealedBoxes.Where(b => b.StyleItemId == si.StyleItemId).Sum(b => b.BoxQuantity * b.Quantity)
                        + allCountingOpenBoxes.Where(b => b.StyleItemId == si.StyleItemId).Sum(b => b.BoxQuantity * b.Quantity))))
                    {
                        styleIdListToUpdate.Add(styleId);
                    }
                }

                var updatedStyleIdList = ProcessStyles(styleIdListToUpdate.Distinct().ToList());

                foreach (var styleId in updatedStyleIdList)
                {
                    var styleItems = db.StyleItems.GetAll().Where(si => si.StyleId == styleId).ToList();
                    styleItems.ForEach(si =>
                    {
                        si.LiteCountingProcessed = true;
                        si.LiteCountingProcessedDate = _time.GetAppNowTime();
                    });
                }
                db.Commit();
            }
        }

        private bool ToUpdate(StyleItemDTO styleItem, int totalCounting)
        {
            if (!styleItem.LiteCountingProcessed)
            {
                if (styleItem.LiteCountingStatus == CountingStatusesEx.Counted
                    || styleItem.LiteCountingStatus == CountingStatusesEx.Verified)
                {
                    if (Math.Abs((styleItem.RemainingQuantity ?? 0) - totalCounting) <= 5
                        || styleItem.ApproveStatus == 1)
                        return true;
                }
            }
            return false;
        }

        private class StyleBoxItemInfo
        {
            public long StyleId { get; set; }
            public long StyleItemId { get; set; }
            public int BoxQuantity { get; set; }
            public int Quantity { get; set; }

            public DateTime CountingDate { get; set; }
            public int BatchTimeStatus { get; set; }
            public bool IsProcessed { get; set; }
        }

        private IList<long> ProcessStyles(IList<long> styleIdList)
        {
            var processedStyleIdList = new List<long>();

            using (var db = _dbFactory.GetRWDb())
            {
                IQueryable<ViewStyle> styleQuery = from st in db.Styles.GetAllActual()
                                                   where styleIdList.Contains(st.Id)
                                                   orderby st.Id descending
                                                   select st;
                var styleList = styleQuery.ToList();

                //var toDate = new DateTime(2016, 9, 23);
                var openBoxList = (from b in db.OpenBoxCountings.GetAll()
                                   join bi in db.OpenBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                   //where bi.CreateDate <= toDate
                                   select new StyleBoxItemInfo
                                   {
                                       StyleId = b.StyleId,
                                       BoxQuantity = b.BoxQuantity,
                                       StyleItemId = bi.StyleItemId,
                                       Quantity = bi.Quantity,
                                       CountingDate = b.CountingDate,
                                       BatchTimeStatus = b.BatchTimeStatus,
                                       IsProcessed = b.IsProcessed,
                                   }).ToList();

                var sealedBoxList = (from b in db.SealedBoxCountings.GetAll()
                                     join bi in db.SealedBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                     //where bi.CreateDate <= toDate
                                     select new StyleBoxItemInfo
                                     {
                                         StyleId = b.StyleId,
                                         BoxQuantity = b.BoxQuantity,
                                         StyleItemId = bi.StyleItemId,
                                         Quantity = bi.BreakDown,
                                         CountingDate = b.CountingDate,
                                         BatchTimeStatus = b.BatchTimeStatus,
                                         IsProcessed = b.IsProcessed,
                                     }).ToList();

                var checkingFromDate = new DateTime(2017, 10, 1);
                var batchList = db.OrderBatches.GetBatchesToDisplay(true)
                    .OrderBy(b => b.CreateDate)
                    .Where(b => b.CreateDate > checkingFromDate)
                    .ToList();

                foreach (var style in styleList)
                {
                    var styleOpenBoxes = openBoxList.Where(b => b.StyleId == style.Id && !b.IsProcessed).ToList();
                    var styleSealedBoxes = sealedBoxList.Where(b => b.StyleId == style.Id && !b.IsProcessed).ToList();

                    if (!styleSealedBoxes.Any()
                        && !styleOpenBoxes.Any())
                    {
                        //TODO: remove all
                        _log.Info("No boxes");
                        continue;
                    }

                    _log.Info("Processing style, StyleId=" + style.StyleID);

                    var styleBoxes = new List<StyleBoxItemInfo>();
                    styleBoxes.AddRange(styleOpenBoxes);
                    styleBoxes.AddRange(styleSealedBoxes);

                    var hasProcessed = openBoxList.Any(b => b.StyleId == style.Id && b.IsProcessed)
                                       || sealedBoxList.Any(b => b.StyleId == style.Id && b.IsProcessed);

                    _log.Info("hasProcessed=" + hasProcessed);

                    //STEP 1. Get min box date + batch mode
                    var firstBox = styleBoxes.OrderBy(sb => sb.CountingDate).FirstOrDefault();
                    var boxDate = firstBox.CountingDate;
                    _log.Info("Box date=" + boxDate);
                    var nextBoxDay = boxDate.Date.AddDays(1);
                    var boxDay = boxDate.Date;
                    var boxPrevious2Week = boxDay.AddDays(-14);

                    var styleItems = db.StyleItems.GetAll().Where(s => s.StyleId == style.Id).ToList();
                    var styleItemCaches = db.StyleItemCaches.GetForStyleId(style.Id);

                    #region STEP 2. Calculate panding orders when first boxes came
                    if (!hasProcessed)
                    {
                        //STEP 2.1 Get batch list
                        //Exclude batch with data > box date
                        //Exclude batch depend of "batch mode"
                        //NOTE: the "Main" batch can be created at 11 PM at previous day
                        var batchFromDate = boxDate.Date.AddHours(-3);
                        var batchToDate = boxDate.Date.AddHours(21);

                        var excludeBatchList = batchList.Where(b => b.CreateDate < batchFromDate).ToList();

                        //NOTE: always BEFORE FIRST BATCH
                        //var dayBatches = batchList.Where(b => b.CreateDate >= batchFromDate && b.CreateDate < batchToDate && b.OrdersCount > 0).ToList();
                        //if (dayBatches.Count > 0)
                        //{
                        //    if (batchTimeStatus == (int) BatchTimeStatus.BeforeFirst)
                        //    {
                        //        //Nothing
                        //    }
                        //    if (batchTimeStatus == (int) BatchTimeStatus.AfterFirstBeforeSecond)
                        //    {
                        //        excludeBatchList.Add(dayBatches[0]);
                        //    }
                        //    if (batchTimeStatus == (int) BatchTimeStatus.AfterSecond)
                        //    {
                        //        if (dayBatches.Count > 1)
                        //            excludeBatchList.Add(dayBatches[1]);
                        //    }
                        //}

                        //STEP 2.2. Get all orders with (box date - 1 week) <= order date <= box date
                        //Get orders outside of exclude batches
                        var excludeBatchIdList = excludeBatchList.Select(b => b.Id).ToList();
                        //NOTE: as result we get the list of orders came till end of day, that In-Pending
                        var pendingOrders =
                            db.Orders.GetAll()
                                .Where(
                                    o =>
                                        (o.OrderStatus == OrderStatusEnumEx.Shipped ||
                                         o.OrderStatus == OrderStatusEnumEx.Pending)
                                        && (!o.BatchId.HasValue || !excludeBatchIdList.Contains(o.BatchId.Value))
                                        && o.OrderDate > boxPrevious2Week && o.OrderDate < boxDay).ToList();

                        //Find styleId, styleItemId in these order items

                        _log.Info("Pending orders=" + pendingOrders.Count);
                        var pendingOrderIds = pendingOrders.Select(p => p.Id).ToArray();
                        var orderItems = db.OrderItems.GetWithListingInfo().Where(i => i.OrderId.HasValue && pendingOrderIds.Contains(i.OrderId.Value)).ToList();
                        //    db.ItemOrderMappings.GetFilteredOrdersWithItems(new OrderSearchFilter()
                        //    {
                        //        EqualOrderIds = pendingOrders.Select(p => p.Id).ToArray()
                        //    }).ToList();
                        //var orderItems =
                        //    pendingOrdersWithItems.Select(o => o.Items).ToList().SelectMany(i => i).ToList();

                        //If > 0, Compose InPendingWhenInventory record
                        if (orderItems.Any(i => i.StyleEntityId == style.Id))
                        {
                            foreach (var si in styleItems)
                            {
                                var sizeQuantity =
                                    orderItems.Where(i => i.StyleItemId == si.Id).Sum(i => i.Quantity);
                                if (sizeQuantity > 0)
                                {
                                    _log.Info(String.Format("Has pending orders items, size={0}({1}), qty={2}", si.Size, si.Id, sizeQuantity));
                                    var operation = new QuantityOperation()
                                    {
                                        Comment = "Pending orders",
                                        Type = (int)QuantityOperationType.InPendingWhenInventory,
                                        CreateDate = _time.GetAppNowTime(),
                                    };
                                    db.QuantityOperations.Add(operation);
                                    db.Commit();

                                    db.QuantityChanges.Add(new QuantityChange()
                                    {
                                        QuantityOperationId = operation.Id,
                                        StyleItemId = si.Id,
                                        StyleId = si.StyleId,
                                        CreateDate = _time.GetAppNowTime(),
                                        Quantity = sizeQuantity,
                                    });
                                    db.Commit();
                                }
                            }
                        }
                    }
                    #endregion

                    //Mark all exist boxes as archive, set box mode = true
                    var beginCounting = new DateTime(2017, 9, 1);
                    var existOpenBoxes = db.OpenBoxes.GetByStyleId(style.Id)
                        .Where(b => b.CreateDate < beginCounting && !b.ReInventory).ToList();
                    foreach (var openBox in existOpenBoxes)
                    {
                        _log.Info("Archive existing OpenBox, Id=" + openBox.Id);
                        openBox.Archived = true;
                    }

                    var existSealedBoxes = db.SealedBoxes.GetByStyleId(style.Id)
                        .Where(b => b.CreateDate < beginCounting && !b.ReInventory).ToList(); ;
                    foreach (var sealedBox in existSealedBoxes)
                    {
                        _log.Info("Archive existing SealedBox, Id=" + sealedBox.Id);
                        sealedBox.Archived = true;
                    }
                    db.Commit();

                    foreach (var si in styleItems)
                    {
                        if (si.Quantity != null)
                            _log.Info(String.Format("Reset Manually Qty, size={0} ({1}) from={2}, at={3}", si.Size, si.Id, si.Quantity, si.QuantitySetDate));
                        si.Quantity = null;
                        si.QuantitySetBy = null;
                        si.QuantitySetDate = null;
                    }
                    db.Commit();

                    //STEP 3.1. Sealed Box
                    var newSealedBoxList = db.SealedBoxCountings.GetByStyleId(style.Id).Where(b => !b.IsProcessed).ToList();
                    var index = 1;
                    foreach (var box in newSealedBoxList)
                    {
                        var newBoxItems = db.SealedBoxCountingItems.GetAll().Where(sb => sb.BoxId == box.Id).ToList();
                        var when = _time.GetAppNowTime();
                        var sizePart = "";
                        if (newBoxItems.Count == 1)
                        {
                            var styleItem = styleItems.FirstOrDefault(s => s.Id == newBoxItems[0].StyleItemId);
                            if (styleItem != null)
                                sizePart = "-" + styleItem.Size;
                        }

                        var newDbBox = new SealedBox()
                        {
                            StyleId = style.Id,
                            BoxBarcode = style.StyleID + sizePart + "-" + when.ToString("MMMMyyyy") + (index > 1 ? "-" + index : ""),
                            BoxQuantity = box.BoxQuantity,
                            CreateDate = boxDay, //nextBoxDay
                            CreatedBy = box.CreatedBy,
                            Owned = true,
                            ReInventory = true,
                        };
                        db.SealedBoxes.Add(newDbBox);
                        db.Commit();

                        foreach (var boxItem in newBoxItems)
                        {
                            db.SealedBoxItems.Add(new SealedBoxItem()
                            {
                                BoxId = newDbBox.Id,
                                StyleItemId = boxItem.StyleItemId,
                                BreakDown = boxItem.BreakDown,

                                CreateDate = boxItem.CreateDate,
                                CreatedBy = boxItem.CreatedBy
                            });
                        }

                        box.IsProcessed = true;
                        db.Commit();

                        _log.Info("Create SealedBox, name=" + newDbBox.BoxBarcode);

                        index++;
                    }

                    //STEP 3.2. Open Box
                    var newOpenBoxList = db.OpenBoxCountings.GetByStyleId(style.Id).Where(b => !b.IsProcessed).ToList();
                    index = 1;
                    foreach (var box in newOpenBoxList)
                    {
                        var when = _time.GetAppNowTime();
                        var sizePart = "";

                        var newBoxItems = db.OpenBoxCountingItems.GetAll().Where(sb => sb.BoxId == box.Id).ToList();
                        if (newBoxItems.Count == 1)
                        {
                            var styleItem = styleItems.FirstOrDefault(si => si.Id == newBoxItems[0].StyleItemId);
                            if (styleItem != null)
                                sizePart = "-" + styleItem.Size;
                        }

                        var newDbBox = new OpenBox()
                        {
                            StyleId = style.Id,
                            BoxBarcode = style.StyleID + sizePart + "-" + when.ToString("MMMMyyyy") + (index > 1 ? "-" + index : ""),
                            BoxQuantity = box.BoxQuantity,
                            CreateDate = boxDay, //nextBoxDay
                            CreatedBy = box.CreatedBy,
                            Owned = true,
                            ReInventory = true,
                        };
                        db.OpenBoxes.Add(newDbBox);
                        db.Commit();

                        foreach (var boxItem in newBoxItems)
                        {
                            db.OpenBoxItems.Add(new OpenBoxItem()
                            {
                                BoxId = newDbBox.Id,
                                StyleItemId = boxItem.StyleItemId,
                                Quantity = boxItem.Quantity,

                                CreateDate = boxItem.CreateDate,
                                CreatedBy = boxItem.CreatedBy
                            });
                        }

                        box.IsProcessed = true;
                        db.Commit();

                        _log.Info("Create OpenBox, name=" + newDbBox.BoxBarcode);

                        index++;
                    }

                    _log.Info("Changes:");
                    foreach (var si in styleItems)
                    {
                        var cache = styleItemCaches.FirstOrDefault(c => c.Id == si.Id);
                        var boxQty = styleBoxes.Where(b => b.StyleItemId == si.Id).Sum(b => b.BoxQuantity * b.Quantity);
                        _log.Info(si.Size + ": " + (cache != null ? cache.RemainingQuantity.ToString() : "[null]") + "->" + boxQty);
                    }

                    processedStyleIdList.Add(style.Id);
                }
            }

            return processedStyleIdList;
        }
    }
}
