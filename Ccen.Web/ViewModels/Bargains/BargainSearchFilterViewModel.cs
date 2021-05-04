using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models.Bargains;

namespace Amazon.Web.ViewModels.Bargains
{
    public class BargainSearchFilterViewModel
    {
        public string Keywords { get; set; }
        public string CategoryId { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }

        public BargainSearchFilter GetModel()
        {
            return new BargainSearchFilter()
            {
                Keywords = Keywords,
                CategoryId = CategoryId,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                StartIndex = StartIndex,
                LimitCount = LimitCount,
            };
        }
    }
}