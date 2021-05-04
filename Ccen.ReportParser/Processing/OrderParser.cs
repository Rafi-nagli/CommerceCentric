using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing
{
    //public class OrderParser : Parser
    //{
    //    public OrderParser(ILineParser parser, ILineProcessing lineProcessor, ISyncInformer syncInfo, long userId) : base(parser, lineProcessor, userId)
    //    {
    //    }

    //    public override void Process(AmazonApi api, long userId)
    //    {
    //        if (Reader.EndOfFile)
    //        {
    //            return;
    //        }
    //        Log.Debug("Begin parse");
    //        var result = GetOrders(GetReportItems());
    //        Log.Debug("End parse");
    //        Log.Debug("Begin process items");
    //        ProcessOrders(result);
    //        Log.Debug("End process items");
    //    }

    //    private void ProcessOrders(List<OrderBuyerDTO> result)
    //    {
    //        using (var db = new UnitOfWork())
    //        {
                
    //            var ordersToUpdate = db.Orders.GetWithoutEmail().ToList();
    //            if (ordersToUpdate.Any())
    //            {
    //                var emailsUpdated = 0;
    //                foreach (var order in ordersToUpdate)
    //                {
    //                    var reportOrder = result.FirstOrDefault(r => r.OrderId == order.AmazonIdentifier);
    //                    if (reportOrder != null)
    //                    {
    //                        order.AmazonEmail = reportOrder.OrderEmail;
    //                        emailsUpdated++;
    //                        order.UpdateDate = DateHelper.GetAppNowTime();
    //                    }
    //                }
    //                db.Commit();
    //                Log.Debug(string.Format("Orders without emails: {0}; orders updated: {1}", ordersToUpdate.Count, emailsUpdated));
    //            }
    //            var ordersWithoutAddress = db.Orders.GetWithoutAddress().ToList();
    //            if (ordersWithoutAddress.Any())
    //            {
    //                var addressUpdated = 0;
    //                foreach (var order in ordersWithoutAddress)
    //                {
    //                    var reportOrder = result.FirstOrDefault(r => r.OrderId == order.AmazonIdentifier);
    //                    if (reportOrder != null)
    //                    {
    //                        var state = order.ShippingState;
    //                        reportOrder.ShippingCountry = ShippingUtils.CorrectingCountryCode(order.ShippingCountry);
    //                        if (!string.IsNullOrEmpty(reportOrder.ShippingState) && reportOrder.ShippingState.Length > 2 
    //                            && !ShippingUtils.IsInternational(reportOrder.ShippingCountry))
    //                        {
    //                            reportOrder.ShippingState = db.States.GetCodeByName(state);
    //                        }
    //                        order.UpdateFromDTO(reportOrder);
    //                        order.UpdateDate = DateHelper.GetAppNowTime();
    //                        addressUpdated++;
    //                    }
    //                }
    //                db.Commit();
    //                Log.Debug(string.Format("Orders without address: {0}; orders updated: {1}", ordersWithoutAddress.Count, addressUpdated));
    //            }
    //        }
    //    }

    //    private List<OrderBuyerDTO> GetOrders(List<IReportItemDTO> parseResult)
    //    {
    //        return parseResult.Cast<OrderBuyerDTO>().ToList();
    //    }
    //}
}
