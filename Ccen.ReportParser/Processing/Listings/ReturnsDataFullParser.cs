using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.ReportParser.LineParser;
using Amazon.ReportParser.Models.Xml;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ReturnsDataFullParser : BaseParser
    {
        public ReturnsDataFullParser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            throw new NotSupportedException();
        }

        public override List<IReportItemDTO> GetReportItems(MarketType market, string marketplaceId)
        {
            AmazonEnvelope<ReturnReportMessageNode> parseResult = null;

            var text = _reader.ReadAll();
            
            using (var sr = new StringReader(text))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AmazonEnvelope<ReturnReportMessageNode>));
                parseResult = (AmazonEnvelope<ReturnReportMessageNode>)serializer.Deserialize(sr);                
            }

            var items = GetAllReportItems(parseResult?.Message?.ReturnDetails ?? new ReturnDetailsNode[] { });

            return items;
        }

        private List<IReportItemDTO> GetAllReportItems(IList<ReturnDetailsNode> items)
        {
            var results = new List<IReportItemDTO>();           
            
            foreach (ReturnDetailsNode returnDetail in items)
            {
                foreach (var returnItem in returnDetail.ItemDetails)
                {
                    results.Add(new ReturnRequestDTO()
                    {
                        MarketReturnId = returnDetail.AmazonRmaId,
                        OrderNumber = returnDetail.OrderId,
                        PrepaidLabelCost = returnDetail.LabelDetails?.LabelCost,
                        HasPrepaidLabel = returnDetail.LabelDetails?.LabelCost > 0, // labelPaidBy == "Customer"
                        PrepaidLabelBy = returnDetail.LabelToBePaidBy,
                        ReceiveDate = returnDetail.ReturnRequestDate,

                        ItemName = returnItem.ItemName,
                        SKU = returnItem.MerchantSku,
                        Reason = returnItem.ReturnReasonCode,
                        Quantity = returnItem.ReturnQuantity,
                        CustomerComments = returnDetail.ReturnType,
                        Details = returnItem.ReturnReasonCode + ", AtoZclaim: " + returnDetail.AtoZclaim + ", Resolution: " + returnItem.Resolution,
                    });
                }
            }

            return results;
        }

        public override void Process(IMarketApi api, 
            ITime time, 
            AmazonReportInfo reportInfo, 
            IList<IReportItemDTO> sourceItems)
        {
            using (var db = new UnitOfWork(Log))
            {
                var returnItems = sourceItems.Select(i => (ReturnRequestDTO)i).ToList();

               // var updatedList = new List<string>(returnItems.Count);
                foreach (var returnItem in returnItems)
                {
                    var existReturnRequest = db.ReturnRequests.GetAll()
                        .OrderByDescending(r => r.CreateDate)
                        .FirstOrDefault(r => r.OrderNumber == returnItem.OrderNumber
                            && (r.SKU == returnItem.SKU
                                || r.ItemName == returnItem.ItemName));

                    if (existReturnRequest != null)
                    {
                        //if (!updatedList.Contains(returnItem.OrderNumber))
                        //{
                            Log.Info("Return request has been updated, orderNumber=" + returnItem.OrderNumber);
                            existReturnRequest.HasPrepaidLabel = returnItem.HasPrepaidLabel;
                            existReturnRequest.PrepaidLabelCost = returnItem.PrepaidLabelCost;
                            existReturnRequest.PrepaidLabelBy = returnItem.PrepaidLabelBy;
                            existReturnRequest.MarketReturnId = returnItem.MarketReturnId;

                            //updatedList.Add(returnItem.OrderNumber);
                        //}
                        //else
                        //{
                        //    Log.Info("Order has multiple returns: " + returnItem.OrderNumber);
                        //}
                    }
                    else
                    {
                        //TODO: create request
                        Log.Info("Exist Return request == null, orderNumber=" + returnItem.OrderNumber);

                        var order = db.Orders.GetByOrderNumber(returnItem.OrderNumber);
                        var orderItems = db.Listings.GetOrderItems(order.Id);
                        var orderShippings = db.OrderShippingInfos.GetByOrderIdAsDto(order.Id).Where(sh => sh.IsActive).ToList();

                        var itemToCheck = orderItems.FirstOrDefault(i => i.SKU == returnItem.SKU);
                        //If not found check all items
                        if (itemToCheck == null && orderItems.Count == 1)
                        {
                            itemToCheck = orderItems.FirstOrDefault();
                        }
                        Log.Info("Item to check=" + (itemToCheck != null ? itemToCheck.SKU : "[null}"));

                        if (itemToCheck != null)
                        {
                            returnItem.StyleId = itemToCheck.StyleId;
                            returnItem.StyleItemId = itemToCheck.StyleItemId;
                            returnItem.SKU = itemToCheck.SKU;
                            returnItem.StyleString = itemToCheck.StyleID;
                        }

                        var requestDb = new ReturnRequest()
                        {
                            ItemName = StringHelper.Substring(returnItem.ItemName, 255),
                            MarketReturnId = returnItem.MarketReturnId,
                            OrderNumber = returnItem.OrderNumber,
                            Quantity = returnItem.Quantity,
                            Reason = returnItem.Reason,
                            CustomerComments = StringHelper.Substring(returnItem.CustomerComments, 255),
                            Details = StringHelper.Substring(returnItem.Details, 255),
                            ReceiveDate = returnItem.ReceiveDate,
                            CreateDate = time.GetAppNowTime(),

                            StyleId = returnItem.StyleId,
                            StyleItemId = returnItem.StyleItemId,
                            SKU = returnItem.SKU,

                            HasPrepaidLabel = returnItem.HasPrepaidLabel,
                            PrepaidLabelCost = returnItem.PrepaidLabelCost,
                            PrepaidLabelBy = returnItem.PrepaidLabelBy
                        };

                        db.ReturnRequests.Add(requestDb);
                        db.Commit();
                    }
                }
                db.Commit();
            }
        }
    }
}
