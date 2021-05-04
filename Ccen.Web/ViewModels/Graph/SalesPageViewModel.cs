using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class SalesPageViewModel
    {
        public enum SalesGraphMode
        {
            SKU = 0,
            StyleItem
        }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ParentASIN { get; set; }
        public long[] StyleItemIdList { get; set; }
        public long? StyleId { get; set; }
        public IList<SalesChildViewModel> Items { get; set; }
        public int Period { get; set; }
        public string ImageSource { get; set; }

        public SalesGraphMode Mode { get; set; }

        public SalesPageViewModel()
        {
            
        }

        static public SalesPageViewModel ComposeByStyleItemId(IUnitOfWork db, long styleItemId)
        {
            var salesChild = db.StyleItems.GetAllAsDto()
                .Select(i => new SalesChildViewModel
                {
                    Size = i.Size,
                    Color = i.Color,
                    StyleItemId = i.StyleItemId,
                    StyleId = i.StyleId
                })
                .FirstOrDefault(si => si.StyleItemId == styleItemId);

            if (salesChild == null)
            {
                throw new ArgumentNullException("styleItemId");
            }

            var style = db.Styles.GetAllAsDto().FirstOrDefault(st => st.Id == salesChild.StyleId);
            var picture = style != null ? style.Image : String.Empty;

            return new SalesPageViewModel
            {
                Items = new List<SalesChildViewModel>() { salesChild },
                Period = (int)SalesPeriod.TwoMonth,
                StyleItemIdList = new long[] { styleItemId },
                ImageSource = picture,
                Mode = SalesGraphMode.StyleItem
            };
        }

        static public SalesPageViewModel ComposeByStyleId(IUnitOfWork db, long styleId)
        {
            var salesChilds = db.StyleItems.GetAllAsDto()
                .Select(i => new SalesChildViewModel
                {
                    Size = i.Size,
                    Color = i.Color,
                    StyleItemId = i.StyleItemId,
                    StyleId = i.StyleId
                })
                .Where(si => si.StyleId == styleId)
                .ToList();

            var style = db.Styles.GetAllAsDto().FirstOrDefault(st => st.Id == styleId);
            var picture = style != null ? style.Image : String.Empty;

            return new SalesPageViewModel
            {
                Items = salesChilds,
                Period = (int)SalesPeriod.TwoMonth,
                StyleId = styleId,
                StyleItemIdList = salesChilds.Where(ch => ch.StyleItemId.HasValue).Select(ch => ch.StyleItemId.Value).ToArray(),
                ImageSource = picture,
                Mode = SalesGraphMode.StyleItem
            };
        }

        static public SalesPageViewModel ComposeByParentASIN(IUnitOfWork db, MarketType market, string marketplaceId, string parentASIN)
        {
            var children = db.Items.GetByParentASINAsDto(market, marketplaceId, parentASIN)
                .Select(i => new SalesChildViewModel
                {
                    ASIN = i.ASIN,
                    SKU = i.SKU,
                    Size = i.Size,
                    Color = i.Color,
                    StyleString = i.StyleString,
                    StyleId = i.StyleId
                })
                .OrderBy(i => i.SizeIndex)
                .ThenBy(i => i.Size)
                .ThenBy(i => i.Color)
                .ThenBy(i => i.SKU);

            var parentItem = db.ParentItems.GetAsDTO(parentASIN, market, marketplaceId);
            var picture = parentItem != null ? parentItem.ImageSource : String.Empty;

            return new SalesPageViewModel
            {
                Market = market,
                MarketplaceId = marketplaceId,
                ParentASIN = parentASIN,
                Items = children.ToList(),
                Period = (int)SalesPeriod.Overall,
                ImageSource = picture,
                Mode = SalesGraphMode.SKU
            };
        }
    }

    public class SalesChildViewModel
    {
        public string ASIN { get; set; }
        public string SKU { get; set; }
        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public float SizeIndex
        {
            get { return SizeHelper.GetSizeIndex(Size); }
        }
    }
}