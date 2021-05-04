using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;

namespace Amazon.Web.Models
{
    public class OrderSearchFilterViewModel
    {
        public int StartIndex { get; set; }
        public int LimitCount { get; set; }

        public string SortField { get; set; }
        public int SortMode { get; set; }

        public string FulfillmentChannel { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? DropShipperId { get; set; }

        public string[] OrderStatus { get; set; }
        public string ShippingStatus { get; set; }
        public bool ExcludeWithLabels { get; set; }
        public bool IncludeForceVisible { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }
        public string EqualOrderNumber { get; set; }
        public long? BatchId { get; set; }

        public bool ExcludeOnHold { get; set; }

        public string StyleId { get; set; }
        public long? StyleItemId { get; set; }

        public long[] EqualOrderIds { get; set; }

        public static OrderSearchFilterViewModel Empty
        {
            get { return new OrderSearchFilterViewModel(); } 
        }

        public static OrderSearchFilterViewModel BuildFromOrderIds(long[] equalOrderIds)
        {
            return new OrderSearchFilterViewModel
            {
                EqualOrderIds = equalOrderIds,
            };
        }

        public static SelectList OrderStatusList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Select...", ""),
                    new KeyValuePair<string, string>("Unshipped", "Unshipped"),
                    new KeyValuePair<string, string>("Pending", "Pending"),
                }, "Value", "Key");
            }
        }

        //public static SelectList MarketList
        //{
        //    get
        //    {
        //        return new SelectList(new List<KeyValuePair<string, string>>
        //        {
        //            new KeyValuePair<string, string>("All", "0" + "_" + ""),
        //            new KeyValuePair<string, string>("Amazon US", ((int)MarketType.Amazon) + "_" + MarketplaceKeeper.AmazonComMarketplaceId),
        //            new KeyValuePair<string, string>("Amazon CA", ((int)MarketType.Amazon) + "_" + MarketplaceKeeper.AmazonCaMarketplaceId),
        //            new KeyValuePair<string, string>("Amazon MX", ((int)MarketType.Amazon) + "_" + MarketplaceKeeper.AmazonMxMarketplaceId),
        //            new KeyValuePair<string, string>("Amazon UK", ((int)MarketType.AmazonEU) + "_" + ""), //NOTE: disable MarketplaceId to include fr, de, e.t.c order, MarketplaceKeeper.AmazonUkMarketplaceId),
        //            new KeyValuePair<string, string>("eBay", ((int)MarketType.eBay) + "_" + ""),
        //            new KeyValuePair<string, string>("Magento", ((int)MarketType.Magento) + "_" + ""),
        //            new KeyValuePair<string, string>("Walmart", ((int)MarketType.Walmart) + "_" + ""),
        //            new KeyValuePair<string, string>("Walmart CA", ((int)MarketType.WalmartCA) + "_" + ""),
        //            new KeyValuePair<string, string>("Jet", ((int)MarketType.Jet) + "_" + "")
        //        }, "Value", "Key");
        //    }
        //}

        public static SelectList StatusList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Select...", ""),
                    new KeyValuePair<string, string>("No issues w/o Intl", "NoIssues"),
                    //new KeyValuePair<string, string>("No issues w/o IBC, Groupon", "NoIssuesIBCGroupon"),
                    new KeyValuePair<string, string>("No issues w/o Intl, only today", "NoIssuesIBCGrouponToday"),                                  
                    new KeyValuePair<string, string>("Dismissed issues", "DismissedIssues"),
                    //new KeyValuePair<string, string>("No weight", "NoWeight"),
                    new KeyValuePair<string, string>("Paid Priority", "PaidPriority"),
                    new KeyValuePair<string, string>("All Priority", "AllPriority"),
                    new KeyValuePair<string, string>("Non Priority", "AllStandard"),
                    new KeyValuePair<string, string>("Not on Hold", "NotOnHold"),
                    new KeyValuePair<string, string>("On Hold", "OnHold"),
                    new KeyValuePair<string, string>("Priorities not Hold", "PrioritiesNotHold"),// "ExpeditedNotOnHold"),
                    new KeyValuePair<string, string>("Upgraded", "Upgraded"),// "ExpeditedNotOnHold"),
                    new KeyValuePair<string, string>("Unshipped", "Unshipped"),
                    //new KeyValuePair<string, string>("No address issues", "NoAddressIssues"),
                    //new KeyValuePair<string, string>("With address issues", "WithAddressIssues"),
                    //new KeyValuePair<string, string>("Duplicate", "Duplicate"),
                    //new KeyValuePair<string, string>("W/o package", "WoPackage"),
                    //new KeyValuePair<string, string>("W/o Stamps price", "WoStampsPrice"),
                    //new KeyValuePair<string, string>("Overdue ship. date", "OverdueShipDate"),
                    //new KeyValuePair<string, string>("Same Day", "SameDay")
                }, "Value", "Key");
            }
        }

        public static SelectList BayList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Select...", ""),
                    new KeyValuePair<string, string>("1", "1"),
                    new KeyValuePair<string, string>("2", "2"),
                    new KeyValuePair<string, string>("3", "3"),
                    new KeyValuePair<string, string>("4", "4"),
                    new KeyValuePair<string, string>("7", "7"),
                    new KeyValuePair<string, string>("8", "8"),
                }, "Value", "Key");
            }
        }

        public static SelectList BatchStatusList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Select...", ""),
                    new KeyValuePair<string, string>("No weight", "NoWeight"),
                    new KeyValuePair<string, string>("All Priority", "AllPriority"),
                    new KeyValuePair<string, string>("Non Priority", "AllStandard"),
                    new KeyValuePair<string, string>("On Hold", "OnHold"),
                    new KeyValuePair<string, string>("No address issues", "NoAddressIssues"),
                    new KeyValuePair<string, string>("With address issues", "WithAddressIssues"),
                    new KeyValuePair<string, string>("Duplicate", "Duplicate"),
                    new KeyValuePair<string, string>("W/o package", "WoPackage"),
                    new KeyValuePair<string, string>("W/o Stamps price", "WoStampsPrice"),
                    new KeyValuePair<string, string>("Overdue ship. date", "OverdueShipDate"),
                    new KeyValuePair<string, string>("Same Day", "SameDay"),
                    new KeyValuePair<string, string>("Upgrade candidates", "UpgradeCandidates")
                }, "Value", "Key");
            }
        }

        public OrderSearchFilter GetModel()
        {
            return new OrderSearchFilter()
            {
                Market = Market,
                MarketplaceId = MarketplaceId,
                DropShipperId = DropShipperId,
                FulfillmentChannel = FulfillmentChannel,
                OrderStatus = OrderStatus,
                ExcludeWithLabels = ExcludeWithLabels,
                IncludeForceVisible = IncludeForceVisible,
                Status = ShippingStatus,
                ExcludeOnHold = ExcludeOnHold,
                From = DateFrom,
                To = DateTo,
                BuyerName = BuyerName,
                OrderNumber = OrderNumber,
                EqualOrderNumber = EqualOrderNumber,
                BatchId = BatchId,
                StyleId = StyleId,
                StyleItemId = StyleItemId,
                EqualOrderIds = EqualOrderIds,

                StartIndex = StartIndex,
                LimitCount = LimitCount,
                SortField = SortField,
                SortMode = SortMode,
            };
        }
    }
}