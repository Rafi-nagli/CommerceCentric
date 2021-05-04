using System;
using System.Globalization;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class OrderSettlementV2LineParser : ILineParser
    {
        private const int SKU = 0;
        private const int ASIN = 1;
        private const int YourPrice = 7;
        private const int SalesPrice = 8;
        private const int Currency = 17;
        private const int EstimatedFee = 18;
        private const int EstimatedReferralFeePerUnit = 19;
        private const int EstimatedVariableClosingFee = 20;
        private const int EstimatedOrderHandlingFeePerOrder = 21;
        private const int EstimatedPickPackFeePerUnit = 22;
        private const int EstimatedWeightHandlingFeePerUnit = 23;
        
        private const int Length = 23;

        private ILogService _log;

        public OrderSettlementV2LineParser(ILogService log)
        {
            _log = log;
        }

        public IReportItemDTO Parse(string[] fields, string[] headers)
        {
            //string[] fields = line.Split('	');
            if (fields.Length < Length)
            {
                return null;
            }
            var item = new ListingFBAEstFeeDTO();

            for (var i = 0; i < fields.Length; i++)
            {
                try
                {
                    var val = fields[i];
                    switch (i)
                    {
                        case SKU:
                            item.SKU = val;
                            break;
                        case ASIN:
                            item.ASIN = val;
                            break;
                        case YourPrice:
                            item.YourPrice = LineParserHelper.GetPrice(val);
                            break;
                        case SalesPrice:
                            item.SalesPrice = LineParserHelper.GetPrice(val);
                            break;
                        case Currency:
                            item.Currency = val;
                            break;
                        case EstimatedFee:
                            item.EstimatedFee = LineParserHelper.GetPrice(val);
                            break;
                        case EstimatedReferralFeePerUnit:
                            item.EstimatedReferralFeePerUnit = LineParserHelper.GetPrice(val);
                            break;
                        case EstimatedVariableClosingFee:
                            item.EstimatedVariableClosingFee = LineParserHelper.GetPrice(val);
                            break;
                        case EstimatedOrderHandlingFeePerOrder:
                            item.EstimatedOrderHandlingFeePerOrder = LineParserHelper.GetPrice(val);
                            break;
                        case EstimatedPickPackFeePerUnit:
                            item.EstimatedPickPackFeePerUnit = LineParserHelper.GetPrice(val);
                            break;
                        case EstimatedWeightHandlingFeePerUnit:
                            item.EstimatedWeightHandlingFeePerUnit = LineParserHelper.GetPrice(val);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(string.Format("Unable to parse field: {0}", fields[i]), ex);
                }
            }
            return item;
        }
    }
}
