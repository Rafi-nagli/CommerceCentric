using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleItemSaleShowViewModel
    {
        public string MarketName { get; set; }
        public string SalePrice { get; set; }
        public int ListingCount { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }

        public string FormattedInfo
        {
            get
            {
                return MarketName + ": " + SalePrice
                       + (MaxPiecesOnSale > 0 ? " (" + (PiecesSoldOnSale ?? 0) + "/" + (MaxPiecesOnSale ?? 0) + ")" : "");
            }
        }

        public StyleItemSaleShowViewModel()
        {
        }

        public StyleItemSaleShowViewModel(string infoString)
        {
            var parts = infoString.Split(":".ToCharArray());
            MarketName = parts[0];
            SalePrice = parts[1];
            ListingCount = StringHelper.TryGetInt(parts[2]) ?? 0;
            MaxPiecesOnSale = StringHelper.TryGetInt(parts[3]);
            PiecesSoldOnSale = StringHelper.TryGetInt(parts[4]);
        }
    }
}