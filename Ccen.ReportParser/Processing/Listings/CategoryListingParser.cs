using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class CategoryListingParser : BaseParser
    {
        public CategoryListingParser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            if (marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                return new CategoryListingUSLineParser(log);
            if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                return new CategoryListingCALineParser(log);
            return null;
        }

        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                var items = reportItems.Cast<ItemExDTO>().ToList();

                Log.Debug("Begin process items");
                ProcessItems(uow, items, api.Market, api.MarketplaceId);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db, 
            IList<ItemExDTO> items,
            MarketType market,
            string marketplaceId)
        {
            Log.Debug("ProcessItems begin");
            var dbParentList = db.ParentItems.GetAll()
                .Where(pi => pi.Market == (int)market && pi.MarketplaceId == marketplaceId)
                .ToList();
            var dtoItemList = db.Items.GetAllViewAsDto(market, marketplaceId).ToList();

            foreach (var item in items)
            {
                if (!String.IsNullOrEmpty(item.ParentSKU))
                {
                    ParentItem dbParentItem = null;
                    ItemDTO dtoItem = null;
                    if (!String.IsNullOrEmpty(item.ASIN))
                        dtoItem = dtoItemList.FirstOrDefault(i => i.ASIN == item.ASIN);
                    if (dtoItem == null && !String.IsNullOrEmpty(item.Barcode))
                        dtoItem = dtoItemList.FirstOrDefault(i => i.Barcode == item.Barcode);

                    if (dtoItem != null
                        && !String.IsNullOrEmpty(dtoItem.ParentASIN))
                    {
                        dbParentItem = dbParentList.FirstOrDefault(pi => pi.ASIN == dtoItem.ParentASIN);
                        if (dbParentItem != null
                            && String.IsNullOrEmpty(dbParentItem.SKU))
                        {
                            dbParentItem.SKU = item.ParentSKU;
                        }
                    }

                    if (dbParentItem == null && !String.IsNullOrEmpty(item.ASIN))
                        dbParentItem = dbParentList.FirstOrDefault(p => p.ASIN == item.ASIN);

                    if (dbParentItem != null)
                    {
                        if (String.IsNullOrEmpty(dbParentItem.SKU))
                            dbParentItem.SKU = item.SKU;
                        if (!String.IsNullOrEmpty(item.SearchKeywords))
                            dbParentItem.SearchKeywords = item.SearchKeywords;
                        if (!String.IsNullOrEmpty(item.Features))
                            dbParentItem.Features = item.Features;
                    }
                }
            }

            db.Commit();
            Log.Debug("ProcessItems end");
        }
    }
}
