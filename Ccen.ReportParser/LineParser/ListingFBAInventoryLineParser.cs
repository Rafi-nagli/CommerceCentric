using System;
using System.Globalization;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class ListingFBAInventoryLineParser : ILineParser
    {
        private const int SellerSKU = 0;
        private const int FulfillmentChannelSKU = 1;
        private const int ASIN = 2;
        private const int ConditionType = 3; //Not uses
        private const int WarehouseConditionCode = 4;
        private const int QuantityAvailable = 5;

        private const int Length = 6;

        private ILogService _log;

        public ListingFBAInventoryLineParser(ILogService log)
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
            var item = new ListingFBAInvDTO();

            for (var i = 0; i < fields.Length; i++)
            {
                try
                {
                    var val = fields[i];
                    switch (i)
                    {
                        case SellerSKU:
                            item.SellerSKU = val;
                            break;
                        case ASIN:
                            item.ASIN = val;
                            break;
                        case FulfillmentChannelSKU:
                            item.FulfillmentChannelSKU = val;
                            break;
                        case WarehouseConditionCode:
                            item.WarehouseConditionCode = val;
                            break;
                        case QuantityAvailable:
                            item.QuantityAvailable = LineParserHelper.GetAmount(val);
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
