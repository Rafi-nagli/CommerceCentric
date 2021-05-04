using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ReturnsDataParser : BaseParser
    {
        public ReturnsDataParser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            throw new NotSupportedException();
        }

        public override List<IReportItemDTO> GetReportItems(MarketType market, string marketplaceId)
        {
            var result = new List<IReportItemDTO>();

            var text = _reader.ReadAll();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(text);

            var items = GetAllReportItems(xmlDoc);

            return items;
        }

        private List<IReportItemDTO> GetAllReportItems(XmlDocument doc)
        {
            var results = new List<IReportItemDTO>();           
            
            var items = doc.SelectNodes(".//return_details");
            foreach (XmlNode item in items)
            {
                var orderId = item.SelectSingleNode("./order_id")?.InnerText;
                var labelPaidBy = item.SelectSingleNode("./label_to_be_paid_by")?.InnerText;
                var labelCost = item.SelectSingleNode("./label_details/label_cost")?.InnerText;
                var receiveDate = DateHelper.FromDateString(item.SelectSingleNode("./return_request_date")?.InnerText);
                var rma = item.SelectSingleNode("./amazon_rma_id")?.InnerText;

                var labelCostValue = StringHelper.TryGetDecimal(labelCost);
                results.Add(new ReturnRequestDTO()
                {
                    OrderNumber = orderId,
                    PrepaidLabelCost = labelCostValue,
                    HasPrepaidLabel = labelCostValue > 0, // labelPaidBy == "Customer"
                    PrepaidLabelBy = labelPaidBy,
                    ReceiveDate = receiveDate
                });
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

                var updatedList = new List<string>(returnItems.Count);
                foreach (var item in returnItems)
                {
                    var returnRequest = db.ReturnRequests.GetAll()
                        .OrderByDescending(r => r.CreateDate)
                        .FirstOrDefault(r => r.OrderNumber == item.OrderNumber);

                    if (returnRequest != null)
                    {
                        if (!updatedList.Contains(item.OrderNumber))
                        {
                            Log.Info("Return request has been updated, orderNumber=" + item.OrderNumber);
                            returnRequest.HasPrepaidLabel = item.HasPrepaidLabel;
                            returnRequest.PrepaidLabelCost = item.PrepaidLabelCost;
                            returnRequest.PrepaidLabelBy = item.PrepaidLabelBy;

                            updatedList.Add(item.OrderNumber);
                        }
                        else
                        {
                            Log.Info("Order has multiple returns: " + item.OrderNumber);
                        }
                    }
                    else
                    {
                        Log.Info("Return request == null, orderNumber=" + item.OrderNumber);
                    }
                }
                db.Commit();
            }
        }
    }
}
