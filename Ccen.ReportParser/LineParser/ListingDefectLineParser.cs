using System;
using System.Globalization;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.ReportParser.LineParser
{
    public class ListingDefectLineParser : ILineParser
    {
        private const int SellerSKU = 0;
        private const int ProductName = 1;
        private const int ASIN = 2;
        private const int FieldName = 3;
        private const int AlertType = 4;
        private const int CurrentValue = 5;
        private const int LastUpdated = 6;
        private const int AlertName = 7;
        private const int Status = 8;
        private const int Explanation = 9;

        private const int Length = 9;

        private ILogService _log;

        /*
         * Index	Header	Description	Extra
0	sku		
1	product-name		
2	asin		
3	field-name		
4	alert-type		
5	current-value		
6	last-updated		
7	alert-name		
8	status		
9	explanation		
         * */

        public ListingDefectLineParser(ILogService log)
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
            var item = new ListingDefectDTO();

            for (var i = 0; i < fields.Length; i++)
            {
                try
                {
                    var val = fields[i];
                    switch (i)
                    {
                        case SellerSKU:
                            item.SKU = val;
                            break;
                        case ProductName:
                            item.ProductName = val;
                            break;
                        case ASIN:
                            item.ASIN = val;
                            break;
                        case FieldName:
                            item.FieldName = val;
                            break;
                        case AlertType:
                            item.AlertType = val;
                            break;
                        case CurrentValue:
                            item.CurrentValue = val;
                            break;
                        case LastUpdated:
                            item.LastUpdated = LineParserHelper.GetDateVal(_log, val, LastUpdated);
                            break;
                        case AlertName:
                            item.AlertName = val;
                            break;
                        case Status:
                            item.Status = val; 
                            break;
                        case Explanation:
                            item.Explanation = val;
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
