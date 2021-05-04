using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation
{
    public class SaleManager
    {
        private ILogService _log;
        private ITime _time;

        public SaleManager(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }


        public static TimeSpan DefaultSalesPeriod = new TimeSpan(720, 0, 0, 0);

        public void CheckSaleEndForAll(IUnitOfWork db)
        {
            var saleList = db.StyleItemSales.GetAll().Where(s => !s.IsDeleted).ToList();

            foreach (var sale in saleList)
            {
                UpdateSoldPieces(db, sale, _time.GetAppNowTime());
            }

            var styleItemIds = saleList.Select(s => s.StyleItemId).ToList();
            var styleItems = db.StyleItems
                .GetAllAsDto()
                .Where(si => styleItemIds.Contains(si.StyleItemId))
                .ToList();

            foreach (var sale in saleList)
            {
                var isSaleEnd = false;
                //End time (Amazon should automaticaly stop Sale)
                if (_time.GetAppNowTime() > sale.SaleEndDate)
                {
                    _log.Info("Sale end by EndDate, saleId=" + sale.Id);
                    isSaleEnd = true;
                }
                if (sale.MaxPiecesOnSale.HasValue)
                {
                    if (sale.MaxPiecesMode == (int) MaxPiecesOnSaleMode.BySize)
                    {
                        if (sale.MaxPiecesOnSale <= sale.PiecesSoldOnSale)
                        {
                            _log.Info("Sale end by MaxPiecesOnSale, saleId=" + sale.Id);
                            isSaleEnd = true;
                        }
                    }
                    if (sale.MaxPiecesMode == (int) MaxPiecesOnSaleMode.ByStyle)
                    {
                        var saleStyleItem = styleItems.FirstOrDefault(si => si.StyleItemId == sale.StyleItemId);
                        if (saleStyleItem == null)
                        {
                            _log.Info("Sale end by not style item info for them");
                            isSaleEnd = true;
                        }
                        else
                        {
                            var styleStyleItemIds = styleItems.Where(si => si.StyleId == saleStyleItem.StyleId).Select(si => si.StyleItemId).ToList();
                            var styleSales = saleList.Where(s => styleStyleItemIds.Contains(s.StyleItemId)).ToList();
                            var styleSoldPieces = styleSales.Sum(si => si.PiecesSoldOnSale);
                            if (sale.MaxPiecesOnSale <= styleSoldPieces)
                            {
                                _log.Info("Sale end by MaxPiecesOnSale, saleId=" + sale.Id);
                                isSaleEnd = true;
                            }
                        }
                    }
                }
                
                UpdateSale(db, sale, isSaleEnd, _time.GetAppNowTime());
            }
        }
        
        public void UpdateSoldPieces(IUnitOfWork db, StyleItemSale sale, DateTime when)
        {
            _log.Info("Update sold pices, Id=" + sale.Id);
            var saleListingIds = db.StyleItemSaleToListings.GetAllAsDto().Where(s => s.SaleId == sale.Id).Select(s => s.ListingId).ToList();

            //NOTE: no needed to check with SaleEndDate
            var saledPieces = db.QuantityHistories
                .GetFiltered(h =>
                    (h.Type == (int)QuantityChangeSourceType.NewOrder || h.Type == (int)QuantityChangeSourceType.OrderCancelled)
                        && saleListingIds.Contains(h.ListingId)
                        && h.CreateDate >= sale.SaleStartDate
                        && (!sale.SaleEndDate.HasValue || h.CreateDate <= sale.SaleEndDate))
                .Sum(s => (int?)s.QuantityChanged) ?? 0;

            _log.Info("Update PiecesSoldOnSale, from=" + sale.PiecesSoldOnSale + ", to=" + saledPieces);
            sale.PiecesSoldOnSale = saledPieces;
            sale.PiecesSoldOnSaleUpdateDate = when;
            db.Commit();
        }

        public void UpdateSale(IUnitOfWork db, 
            StyleItemSale sale, 
            bool isSaleEnd,
            DateTime when)
        {
            _log.Info("Checking SaleEnd, saleId=" + sale.Id + ", isSaleEnd=" + isSaleEnd);
            
            var saleListingIds = db.StyleItemSaleToListings.GetAllAsDto()
                        .Where(s => s.SaleId == sale.Id)
                        .Select(s => s.ListingId)
                        .ToList();
            var saleListings = db.Listings.GetAll().Where(l => saleListingIds.Contains(l.Id)).ToList();

            if (isSaleEnd)
            {
                foreach (var listing in saleListings)
                {
                    listing.PriceUpdateRequested = true;
                }

                sale.CloseDate = when;
                sale.IsDeleted = true;
            }

            if (!isSaleEnd) { 
                //NOTE: Forse request price updated ones per day, in case of relist listing
                foreach (var listing in saleListings)
                {
                    if (!listing.LastPriceUpdatedOnMarket.HasValue
                        || listing.LastPriceUpdatedOnMarket < when.AddDays(-1))
                    {
                        _log.Info("Request price update for=" + listing.ListingId +
                                  ", has sale, no updates more then one days");
                        listing.PriceUpdateRequested = true;
                    }
                }

                db.Commit();
            }
        }
    }
}
