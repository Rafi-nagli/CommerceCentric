using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation.Listings.Rules
{
    public class FixBrandIssueRule : IItemAutoFixRule
    {
        private IDbFactory _dbFactory;
        private ILogService _log;

        public FixBrandIssueRule(IDbFactory dbFactory,
            ILogService log)
        {
            _dbFactory = dbFactory;
            _log = log;
        }

        public void Apply(ItemDTO item, IList<ItemAdditionDTO> additions)
        {
            additions = additions
                .Where(a => a.Value.Contains("The SKU data provided is different from what's already in the Amazon catalog."))
                .ToList();
            if (!additions.Any())
                return;

            var regex = new Regex("brand \\(Merchant: '(.*)' / Amazon: '(.*)'\\)");
            foreach (var addition in additions)
            {
                var result = regex.Match(addition.Value);
                if (result.Success)
                {
                    using (var db = _dbFactory.GetRWDb())
                    {
                        var dbParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Market == item.Market
                            && (item.MarketplaceId == item.MarketplaceId || String.IsNullOrEmpty(item.MarketplaceId))
                            && pi.ASIN == item.ParentASIN);
                        var dbItem = db.Items.Get(item.Id);
                                                
                        if (dbParentItem != null && dbItem != null && !String.IsNullOrEmpty(dbParentItem.ASIN))
                        {
                            if (dbParentItem.BrandName != result.Groups[2].Value)
                            {
                                _log.Info("Brand was changed for: " + dbParentItem.ASIN + ", change: " + dbParentItem.BrandName + " => " + result.Groups[2].Value);
                                dbParentItem.BrandName = result.Groups[2].Value;

                                if (dbItem.ItemPublishedStatus != (int)PublishedStatuses.Unpublished
                                    && dbItem.ItemPublishedStatus != (int)PublishedStatuses.HasUnpublishRequest)
                                {
                                    dbItem.ItemPublishedStatus = (int)PublishedStatuses.HasChanges;
                                }
                            }
                        }
                        db.Commit();
                    }
                }
            }
        }
    }
}
