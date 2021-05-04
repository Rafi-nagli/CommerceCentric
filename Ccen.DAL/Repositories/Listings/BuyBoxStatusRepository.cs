using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class BuyBoxStatusRepository : Repository<BuyBoxStatus>, IBuyBoxStatusRepository
    {
        public BuyBoxStatusRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<BuyBoxStatusDTO> GetAllWithItems()
        {
            var query = from bb in unitOfWork.GetSet<BuyBoxStatus>()
                join i in unitOfWork.GetSet<ViewItem>() on
                new { bb.ASIN, bb.Market, bb.MarketplaceId } equals new { i.ASIN, i.Market, i.MarketplaceId }
                join st in unitOfWork.GetSet<StyleItemCache>() on i.StyleItemId equals st.Id into withStyleItem
                from st in withStyleItem.DefaultIfEmpty()
                select new BuyBoxStatusDTO()
                {
                    Id = bb.Id,

                    ASIN = bb.ASIN,
                    Market = bb.Market,
                    MarketplaceId = bb.MarketplaceId,

                    CheckedDate = bb.CheckedDate,
                    WinnerMerchantName = bb.WinnerMerchantName,
                    WinnerPrice = bb.WinnerPrice,
                    WinnerSalePrice = bb.WinnerSalePrice,
                    WinnerAmountSaved = bb.WinnerAmountSaved,

                    LostWinnerDate = bb.LostWinnerDate,
                    Status = bb.Status,
                    IsIgnored = bb.IsIgnored,

                    Price = i.CurrentPrice,
                    Quantity = st.InventoryQuantity 
                    - st.MarketsSoldQuantityFromDate 
                    - st.ScannedSoldQuantityFromDate 
                    - st.SpecialCaseQuantityFromDate
                    - st.SentToFBAQuantityFromDate,

                    ParentASIN = i.ParentASIN,
                    Size = i.Size,
                    Images = i.ItemPicture,
                    SKU = i.SKU,

                    StyleString = i.StyleString,
                    StyleId = i.StyleId,
                };

            return query;
        }

        public void UpdateBulkByBarcode(IList<BuyBoxStatusDTO> buyBoxList, MarketType market, string marketplaceId, DateTime when)
        {
            throw new NotImplementedException();
        }

        public void Update(ILogService log,
            ITime time,
            BuyBoxStatusDTO buyBoxInfo,
            MarketType market,
            string marketplaceId)
        {
            var dbExist = GetAll().FirstOrDefault(b => b.ASIN == buyBoxInfo.ASIN
                && b.Market == (int)market
                && b.MarketplaceId == marketplaceId);

            UpdateItem(log, market, marketplaceId, dbExist, buyBoxInfo, time.GetAppNowTime());

            unitOfWork.Commit();
        }

        public void UpdateBulk(ILogService log,
            ITime time,
            IList<BuyBoxStatusDTO> buyBoxList, 
            MarketType market, 
            string marketplaceId)
        {
            var dbBuyBoxList = GetAll().Where(i => i.Market == (int)market
                    && i.MarketplaceId == marketplaceId).ToList();

            foreach (var buyBoxInfo in buyBoxList)
            {
                var dbExist = dbBuyBoxList.FirstOrDefault(b => b.ASIN == buyBoxInfo.ASIN);
                var newDbExist = UpdateItem(log, market, marketplaceId, dbExist, buyBoxInfo, time.GetAppNowTime());
                if (newDbExist != null)
                    dbBuyBoxList.Add(newDbExist);
            }

            unitOfWork.Commit();
        }

        private BuyBoxStatus UpdateItem(ILogService log, 
            MarketType market,
            string marketplaceId,
            BuyBoxStatus dbExist, 
            BuyBoxStatusDTO buyBoxInfo,
            DateTime when)
        {
            if (dbExist != null)
            {
                if (dbExist.Status == BuyBoxStatusCode.Win && buyBoxInfo.Status == BuyBoxStatusCode.NotWin)
                    dbExist.LostWinnerDate = buyBoxInfo.CheckedDate;

                dbExist.WinnerMerchantName = buyBoxInfo.WinnerMerchantName;

                if (dbExist.WinnerPrice != buyBoxInfo.WinnerPrice)
                {
                    log.Info("Price changed, ASIN=" + buyBoxInfo.ASIN + ", MarketplaceId=" + marketplaceId + ": " + dbExist.WinnerPrice + "->" + buyBoxInfo.WinnerPrice + " (win=" + buyBoxInfo.Status);

                    dbExist.WinnerPriceLastChangeValue = buyBoxInfo.WinnerPrice ?? 0 - dbExist.WinnerPrice ?? 0;
                    dbExist.WinnerPrice = buyBoxInfo.WinnerPrice;
                    dbExist.WinnerPriceLastChangeDate = when;
                }

                dbExist.Status = buyBoxInfo.Status;
                dbExist.CheckedDate = buyBoxInfo.CheckedDate;
            }
            else
            {
                dbExist = new BuyBoxStatus();
                dbExist.ASIN = buyBoxInfo.ASIN;

                dbExist.Market = (int)market;
                dbExist.MarketplaceId = marketplaceId;

                if (dbExist.Status == BuyBoxStatusCode.Win && buyBoxInfo.Status == BuyBoxStatusCode.NotWin)
                    dbExist.LostWinnerDate = buyBoxInfo.CheckedDate;

                dbExist.WinnerMerchantName = buyBoxInfo.WinnerMerchantName;

                dbExist.WinnerPrice = buyBoxInfo.WinnerPrice;
                dbExist.WinnerPriceLastChangeDate = when;

                dbExist.Status = buyBoxInfo.Status;
                dbExist.CheckedDate = buyBoxInfo.CheckedDate;

                Add(dbExist);

                return dbExist;
            }

            return null;
        }
    }
}
