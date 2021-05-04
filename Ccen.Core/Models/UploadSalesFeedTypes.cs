using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum UploadSaleFeedTypes
    {
        None = 0,
        AddToCategory = 1,
        CreateSales = 2,
        CreateSalesAndAddToCategory = 3
    }

    public class UploadSaleFeedTypeHelper
    {
        public static string GetName(UploadSaleFeedTypes feedType)
        {
            if (feedType == UploadSaleFeedTypes.AddToCategory)
                return "SKUs to category";
            if (feedType == UploadSaleFeedTypes.CreateSales)
                return "Create sales w/o add to category";
            if (feedType == UploadSaleFeedTypes.CreateSalesAndAddToCategory)
                return "Create sales & add to category";
            return "";
        }
    }
}
