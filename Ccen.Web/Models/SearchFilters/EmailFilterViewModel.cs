using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Web.ViewModels.Html;

namespace Amazon.Web.Models.SearchFilters
{
    public class EmailFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }

        public bool OnlyIncoming { get; set; }
        public bool OnlyWithoutAnswer { get; set; }
        public bool IncludeSystem { get; set; }
        public int? ResponseStatus { get; set; }

        public int? Market { get; set; }

        public static EmailFilterViewModel Empty
        {
            get
            {
                return new EmailFilterViewModel()
                {
                    OnlyIncoming = false,
                    OnlyWithoutAnswer = false,
                    IncludeSystem = false,
                };
            }
        }

        public IList<SelectListItemTag> MarketList
        {
            get
            {
                var list = new List<SelectListItemTag>();
                list.Add(new SelectListItemTag()
                {
                    Text = "Amazon",
                    Value = ((int)MarketType.Amazon).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "Amazon EU",
                    Value = ((int)MarketType.AmazonEU).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "Amazon AU",
                    Value = ((int)MarketType.AmazonAU).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "Walmart",
                    Value = ((int)MarketType.Walmart).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "eBay",
                    Value = ((int)MarketType.eBay).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = EmailResponseStatusFilterEnum.Escalated.ToString(),
                    Value = ((int)EmailResponseStatusFilterEnum.Escalated).ToString()
                });

                return list;
            }
        }

        public IList<SelectListItemTag> ResponseStatusList
        {
            get
            {
                var list = new List<SelectListItemTag>();
                list.Add(new SelectListItemTag()
                {
                    Text = "All",
                    Value = ((int)EmailResponseStatusFilterEnum.None).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "Response Needed",
                    Value = ((int)EmailResponseStatusFilterEnum.ResponseNeeded).ToString()
                });
                list.Add(new SelectListItemTag()
                {
                    Text = "Recently Dismissed",
                    Value = ((int)EmailResponseStatusFilterEnum.ResponseNeededDismissed).ToString()
                });
                return list;
            }
        }

        public EmailSearchFilter GetModel()
        {
            return new EmailSearchFilter()
            {
                OrderId = OrderNumber,
                BuyerName = BuyerName,
                Market = Market,
                From = DateFrom,
                To = DateTo,
                OnlyIncoming = OnlyIncoming,
                OnlyWithoutAnswer = OnlyWithoutAnswer,
                IncludeSystem = IncludeSystem,
                ResponseStatus = ResponseStatus ?? (int)EmailResponseStatusFilterEnum.None
            };
        }
    }
}