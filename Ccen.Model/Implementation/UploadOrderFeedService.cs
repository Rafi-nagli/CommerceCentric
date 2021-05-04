using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Categories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Categories;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Model.General.Services;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.Models;
using Amazon.Utils;
using Magento.Api.Wrapper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using static Amazon.Model.Implementation.UploadOrderFeedService;

namespace Amazon.Model.Implementation
{
    public class UploadOrderFeedService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _systemAction;
        private IStyleHistoryService _styleHistory;
        private IAutoCreateListingService _listingCreateService;

        public UploadOrderFeedService(IDbFactory dbFactory,
            ITime time,
            ILogService log,
            IAutoCreateListingService listingCreateService,
            ISystemActionService systemAction,
            IStyleHistoryService styleHistory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _listingCreateService = listingCreateService;
            _systemAction = systemAction;
            _styleHistory = styleHistory;
        }

        public void ProcessSystemAction(ISystemActionService actionService,
            string feedDirectory)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toProcessActions = actionService.GetUnprocessedByType(db, SystemActionType.ProcessUploadOrderFeed, null, null);

                foreach (var action in toProcessActions)
                {
                    _log.Info("Find action: " + action.Id);
                    var actionStatus = SystemActionStatus.None;
                    var output = new PublishFeedOutput();
                    try
                    {
                        var inputData = SystemActionHelper.FromStr<PublishFeedInput>(action.InputData);

                        var filepath = Path.Combine(feedDirectory, inputData.FileName);

                        var uploadResult = UploadFeed(filepath,
                            (MarketType)(inputData.Market ?? (int)MarketType.OfflineOrders),
                            inputData.MarketplaceId,
                            (PublishFeedModes)(inputData.Mode ?? (int)PublishFeedModes.Publish),
                            inputData.IsPrime);

                        output.ProgressPercent = 100;
                        output.IsSuccess = true;
                        if (uploadResult != null)
                        {
                            output.MatchedCount = uploadResult.MatchedCount;
                            output.ParsedCount = uploadResult.ParsedCount;
                            output.Processed1OperationCount = uploadResult.Processed1Count;
                            output.Processed2OperationCount = uploadResult.Processed2Count;
                        }

                        actionStatus = SystemActionStatus.Done;
                        _log.Info("Sale feed processed: " + filepath + ", actionId=" + action.Id);
                    }
                    catch (Exception ex)
                    {
                        actionStatus = SystemActionStatus.Fail;

                        _log.Error("Fail quantity distributed, actionId=" + action.Id + ", status=" + actionStatus, ex);
                    }

                    actionService.SetResult(db,
                        action.Id,
                        actionStatus,
                        output,
                        null);
                }

                db.Commit();
            }
        }


        public static IList<MessageString> ValidateFeed(string filepath)
        {
            var results = new List<MessageString>();
            try
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    var headerRow = sheet.GetRow(0);

                    //var modelColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                    //                                                           &&
                    //                                                           (StringHelper.IsEqualNoCase(
                    //                                                               StringHelper.TrimWhitespace(
                    //                                                                   c.StringCellValue), "SKU")
                    //                                                            ||
                    //                                                            StringHelper.IsEqualNoCase(
                    //                                                                StringHelper.TrimWhitespace(
                    //                                                                    c.StringCellValue), "Model")
                    //                                                            ||
                    //                                                            StringHelper.IsEqualNoCase(
                    //                                                                StringHelper.TrimWhitespace(
                    //                                                                    c.StringCellValue), "Product Id")))?
                    //    .ColumnIndex;

                    //if (!modelColumnIndex.HasValue)
                    //    results.Add(MessageString.Error("Unable to find the SKU/Model column"));

                    //if (market == MarketType.Amazon
                    //    || market == MarketType.AmazonPrime)
                    //{
                    //    var minPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                    //        && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min Price")
                    //        || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min")
                    //        || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min/Floor")))?.ColumnIndex;

                    //    var maxPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                    //        && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max Price")
                    //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max")))?.ColumnIndex;

                    //    if (!minPriceColumnIndex.HasValue)
                    //    {
                    //        results.Add(MessageString.Error("Unable to find the \"Min Price\" column"));
                    //    }
                    //    if (!maxPriceColumnIndex.HasValue)
                    //    {
                    //        results.Add(MessageString.Error("Unable to find the \"Max Price\" column"));
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                results.Add(MessageString.Error("Invalid file format. Details: " + ex.Message));
            }
            return results;
        }

        public IList<MessageString> PreviewFeed(string filepath, PublishFeedModes mode)
        {
            var ordersCount = 0;

            var results = new List<MessageString>();

            _log.Info("Preview Feed file: " + filepath);

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                int? orderDateIndex = 1;
                //var modelColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                //                                                               &&
                //                                                               (StringHelper.IsEqualNoCase(
                //                                                                   StringHelper.TrimWhitespace(
                //                                                                       c.StringCellValue), "SKU")
                //                                                                ||
                //                                                                StringHelper.IsEqualNoCase(
                //                                                                    StringHelper.TrimWhitespace(
                //                                                                        c.StringCellValue), "Model")
                //                                                                ||
                //                                                                StringHelper.IsEqualNoCase(
                //                                                                    StringHelper.TrimWhitespace(
                //                                                                        c.StringCellValue), "Product Id")))?
                //        .ColumnIndex;

                //var salePriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                //    && StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Price"))?.ColumnIndex;

                //var minPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                //    && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min Price")
                //        || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min")))?.ColumnIndex;

                //var maxPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                //    && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max Price")
                //        || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max")))?.ColumnIndex;

                //if (!modelColumnIndex.HasValue)
                //{
                //    results.Add(MessageString.Error("Unable to find the SKU/Model column"));
                //    return results;
                //}

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(0) != null)
                    {
                        if (String.IsNullOrEmpty(row.GetCell(orderDateIndex.Value).ToString()))
                            ordersCount++;
                        //            skuDataIssue++;
                        //        else
                        //            skuCount++;

                        //        try
                        //        {
                        //            var currentPrice = (salePriceColumnIndex.HasValue &&
                        //                                row.GetCell(salePriceColumnIndex.Value) != null)
                        //                ? PriceHelper.RoundToTwoPrecision(
                        //                    ExcelHelper.TryGetCellDecimal(row.GetCell(salePriceColumnIndex.Value)))
                        //                : 0;

                        //            var minPrice = (minPriceColumnIndex.HasValue &&
                        //                                row.GetCell(minPriceColumnIndex.Value) != null)
                        //                ? PriceHelper.RoundToTwoPrecision(
                        //                    ExcelHelper.TryGetCellDecimal(row.GetCell(minPriceColumnIndex.Value)))
                        //                : 0;

                        //            var maxPrice = (maxPriceColumnIndex.HasValue &&
                        //                                row.GetCell(maxPriceColumnIndex.Value) != null)
                        //                ? PriceHelper.RoundToTwoPrecision(
                        //                    ExcelHelper.TryGetCellDecimal(row.GetCell(maxPriceColumnIndex.Value)))
                        //                : 0;

                        //            if (currentPrice == 0 && (minPrice == 0 || maxPrice == 0))
                        //                priceDataIssue++;
                        //            else
                        //                priceCount++;
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _log.Info("Issue with processing: " + ex.Message);
                        //            priceDataIssue++;
                        //        }
                    }
                }
            }

            _log.Info("Parsed count: " + results.Count);

            results.Add(MessageString.Info(String.Format("{0} Orders will be created", ordersCount)));

            //if (mode == PublishFeedModes.Publish)
            //{
            //    results.Add(MessageString.Info(String.Format("{0} SKU(s) will be published or updated", skuCount)));
            //    results.Add(MessageString.Info(String.Format("for {0} SKU(s) the price will be updated", priceCount)));
            //}
            //if (mode == PublishFeedModes.Hold)
            //{
            //    results.Add(MessageString.Info(String.Format("for {0} SKU(s) the status will be changed to \"on hold\"", skuCount)));
            //}
            //if (mode == PublishFeedModes.UnHold)
            //{
            //    results.Add(MessageString.Info(String.Format("for {0} SKU(s) the status will be changed to \"un hold\"", skuCount)));
            //}
            //if (mode == PublishFeedModes.HoldStyle)
            //{
            //    results.Add(MessageString.Info(String.Format("for {0} style(s) the status will be changed to \"on hold\"", skuCount)));
            //}
            //if (mode == PublishFeedModes.UnHoldStyle)
            //{
            //    results.Add(MessageString.Info(String.Format("for {0} style(s) the status will be changed to \"un hold\"", skuCount)));
            //}

            return results;
        }

        public class ProcessFeedResult
        {
            public int? ParsedCount { get; set; }
            public int? MatchedCount { get; set; }
            public int? Processed1Count { get; set; }
            public int? Processed2Count { get; set; }
            public int? Processed3Count { get; set; }
        }

        public IList<ItemDTO> ParseFeed(string filepath)
        {
            var results = new List<ItemDTO>();

            //using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            //{
            //    IWorkbook workbook = null;
            //    if (filepath.EndsWith(".xlsx"))
            //        workbook = new XSSFWorkbook(stream);
            //    else
            //        workbook = new HSSFWorkbook(stream);

            //    var sheet = workbook.GetSheetAt(0);

            //    var headerRow = sheet.GetRow(0);

            //    var modelColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
            //                                                                   &&
            //                                                                   (StringHelper.IsEqualNoCase(
            //                                                                       StringHelper.TrimWhitespace(
            //                                                                           c.StringCellValue), "SKU")
            //                                                                    ||
            //                                                                    StringHelper.IsEqualNoCase(
            //                                                                        StringHelper.TrimWhitespace(
            //                                                                            c.StringCellValue), "Model")
            //                                                                    ||
            //                                                                    StringHelper.IsEqualNoCase(
            //                                                                        StringHelper.TrimWhitespace(
            //                                                                            c.StringCellValue), "Product Id")))?
            //            .ColumnIndex;

            //    var salePriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
            //        && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Price")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Sale_Price")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Regular Price")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Groupon Cost")))?.ColumnIndex;

            //    var minPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
            //        && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min Price")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Min/Floor")))?.ColumnIndex;


            //    var maxPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
            //        && (StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max Price")
            //            || StringHelper.IsEqualNoCase(StringHelper.TrimWhitespace(c.StringCellValue), "Max")))?.ColumnIndex;

            //    if (!salePriceColumnIndex.HasValue && maxPriceColumnIndex.HasValue)
            //        salePriceColumnIndex = maxPriceColumnIndex;

            //    if (!modelColumnIndex.HasValue)
            //        return null;

            //    for (var i = 1; i <= sheet.LastRowNum; i++)
            //    {
            //        var row = sheet.GetRow(i);
            //        if (row != null && row.GetCell(0) != null)
            //        {
            //            try
            //            {
            //                results.Add(new ItemDTO()
            //                {
            //                    SKU = row.GetCell(modelColumnIndex.Value).ToString(),
            //                    CurrentPrice = (salePriceColumnIndex.HasValue && row.GetCell(salePriceColumnIndex.Value) != null) ?
            //                            (PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(salePriceColumnIndex.Value))) ?? 0)
            //                            : 0,
            //                    MinPrice = (minPriceColumnIndex.HasValue && row.GetCell(minPriceColumnIndex.Value) != null) ?
            //                            (PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(minPriceColumnIndex.Value))) ?? 0)
            //                            : 0,
            //                    MaxPrice = (maxPriceColumnIndex.HasValue && row.GetCell(maxPriceColumnIndex.Value) != null) ?
            //                            (PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(maxPriceColumnIndex.Value))) ?? 0)
            //                            : 0,
            //                });
            //            }
            //            catch (Exception ex)
            //            {
            //                _log.Info("Issue with processing: " + ex.Message);
            //            }
            //        }
            //    }
            //}

            return results;
        }

        public ProcessFeedResult UploadFeed(string filepath,
            MarketType market,
            string marketplaceId,
            PublishFeedModes mode,
            bool isPrime)
        {
            var result = new ProcessFeedResult();

            _log.Info("Feed file: " + filepath);

            var results = ParseFeed(filepath);
            if (results == null)
                return result;

            _log.Info("Parsed count: " + results.Count);

            result.ParsedCount = results.Count;
            return result;
        }
    }
}
