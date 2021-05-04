using System.Collections.Generic;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UploadSaleInput : ISystemActionInput
    {
        public string CategoryName { get; set; }
        public long? ExistMagentoCategoryId { get; set; }
        public bool CreateCategoryRequested { get; set; }

        public string FileName { get; set; }
        public int? Mode { get; set; }
        public int? Market { get; set; }

        public bool IsAddSkuToCategory { get; set; }
        public bool IsAddSale { get; set; }
        public bool IsUpdateRegPrice { get; set; }

        public IList<MarketplaceName> SaleToMarkets { get; set; }

        //public UploadSaleFeedTypes FeedType { get; set; }
    }
}
