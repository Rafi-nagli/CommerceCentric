using System;
using System.Globalization;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class ListingLiteLineParser : ILineParser
    {
        private const int SellerSKU = 0;
        private const int ASIN = 2;
        private const int Quantity = 3;
        private const int Price = 3;

        private const int Length = 4;

        private ILogService _log;

        public ListingLiteLineParser(ILogService log)
        {
            _log = log;
        }

        public IReportItemDTO Parse(string[] fields, string[] headers)
        {
            //string[] fields = line.Split('	');
            //if (fields.Length < Length)
            //{
            //    return null;
            //}
            //var item = new ItemDTO();

            //for (var i = 0; i < fields.Length; i++)
            //{
            //    try
            //    {
            //        var val = fields[i];
            //        switch (i)
            //        {
            //            case SellerSKU:
            //                item.SKU = val;
            //                break;
            //            case ASIN:
            //                item.ASIN = val;
            //                break;
                        
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _log.Error(string.Format("Unable to parse line: {0}", line), ex);
            //    }
            //}
            //return item;
            return new ItemDTO();
        }
    }
}
