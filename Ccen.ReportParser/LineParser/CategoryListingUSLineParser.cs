using System;
using System.Globalization;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class CategoryListingUSLineParser : ILineParser
    {
        private const int SKU = 0;
        private const int ProductName = 1;
        private const int ProductId = 2;
        private const int ProductIdType = 3; //upc/asin

        private const int KeyProductFeatures1 = 37; //=bullet_point1
        private const int KeyProductFeatures2 = 38; //=bullet_point2
        private const int KeyProductFeatures3 = 39; //=bullet_point3
        private const int KeyProductFeatures4 = 40; //=bullet_point4
        private const int KeyProductFeatures5 = 41; //=bullet_point5

        private const int SearchTerm1 = 42; //=generic_keywords1
        private const int SearchTerm2 = 43; //=generic_keywords2
        private const int SearchTerm3 = 44; //=generic_keywords3
        private const int SearchTerm4 = 45; //=generic_keywords4
        private const int SearchTerm5 = 46; //=generic_keywords5

        private const int Parentage = 69; //child,parent
        private const int ParentSKU = 70; 
        
        private const int Length = 139;

        private ILogService _log;

        public CategoryListingUSLineParser(ILogService log)
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
            var item = new ItemExDTO();

            for (var i = 0; i < fields.Length; i++)
            {
                try
                {
                    var val = UnQuote(fields[i]);
                    switch (i)
                    {
                        case SKU:
                            item.SKU = val;
                            break;
                        case ProductId:
                            item.ASIN = val;
                            break;
                        case ProductIdType:
                            if (val == "upc")
                            {
                                item.Barcode = item.ASIN;
                                item.ASIN = "";
                            }
                            break;
                        case ParentSKU:
                            item.ParentSKU = val;
                            break;
                        case Parentage:
                            if (val == "child")
                                item.IsChild = true;
                            if (val == "parent")
                                item.IsParent = true;
                            break;
                        case KeyProductFeatures1:
                            item.Features += val + ";";
                            break;
                        case KeyProductFeatures2:
                            item.Features += val + ";";
                            break;
                        case KeyProductFeatures3:
                            item.Features += val + ";";
                            break;
                        case KeyProductFeatures4:
                            item.Features += val + ";";
                            break;
                        case KeyProductFeatures5:
                            item.Features += val + ";";
                            break;
                        case SearchTerm1:
                            item.SearchKeywords += val + ";";
                            break;
                        case SearchTerm2:
                            item.SearchKeywords += val + ";";
                            break;
                        case SearchTerm3:
                            item.SearchKeywords += val + ";";
                            break;
                        case SearchTerm4:
                            item.SearchKeywords += val + ";";
                            break;
                        case SearchTerm5:
                            item.SearchKeywords += val + ";";
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

        private string UnQuote(string val)
        {
            if (String.IsNullOrEmpty(val))
                return val;

            if (val.Length > 2)
            {
                if (val[0] == '\"' && val.Last() == '\"')
                    val = val.Substring(1, val.Length - 2);
            }
            return val;
        }
    }
}
