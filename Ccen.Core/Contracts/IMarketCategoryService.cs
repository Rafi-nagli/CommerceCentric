using Amazon.Core.Models;
using Amazon.Core.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IMarketCategoryService
    {
        CategoryInfo GetCategory(MarketType market,
            string marketplaceId,
            string itemStyle,
            string gender,
            SizeTypes sizeType);
    }
}
