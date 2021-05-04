using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Print
{
    public class PriceInfo
    {
        public decimal Price { get; set; }
        public string Currency { get; set; }

        public float CurrencyIndex
        {
            get { return PriceHelper.GetCurrencyIndex(Currency); }
        }
    }

    public class PickListSizeInfo
    {
        public string StyleSize { get; set; }

        public IList<string> Sizes { get; set; }
        public long? StyleItemId { get; set; }

        public string Color { get; set; }
        public int Quantity { get; set; }
        public double? Weight { get; set; }

        public IList<PriceInfo> Prices { get; set; }
        public int PendingQuantity { get; set; }
        public int OrderedQuantity { get; set; }
        public bool NoVariation { get; set; }

        public float SizeIndex
        {
            get { return SizeHelper.GetSizeIndex(StyleSize); }
        }
    }
}