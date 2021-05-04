using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Model.Implementation
{
    public class StyleManager : IStyleManager
    {
        private ILogService _log;
        private ITime _time;
        private IStyleHistoryService _styleHistoryService;

        public StyleManager(ILogService log, 
            ITime time,
            IStyleHistoryService styleHistoryService)
        {
            _log = log;
            _time = time;
            _styleHistoryService = styleHistoryService;
        }

        public bool StoreOrUpdateBarcode(IUnitOfWork db,
            long styleItemId, 
            string barcode)
        {
            var existBarcode = db.StyleItemBarcodes.GetAllAsDto().FirstOrDefault(b => b.Barcode == barcode);
            if (existBarcode == null)
            {
                db.StyleItemBarcodes.Add(new StyleItemBarcode()
                {
                    StyleItemId = styleItemId,
                    Barcode = barcode,
                    CreateDate = _time.GetAppNowTime(),
                });
                _log.Info("Barcode=" + barcode + " was added, to styleItemId=" + styleItemId);

                return true;
            }
            else
            {
                if (existBarcode.StyleItemId != styleItemId)
                {
                    _log.Info("Barcode=" + barcode + " was moved, from styleItemId: " + existBarcode.StyleItemId + " => " + styleItemId);
                    existBarcode.StyleItemId = styleItemId;
                }

                return false;
            }
        }

        public StyleItemDTO TryGetStyleItemIdFromOtherMarkets(IUnitOfWork db, ItemDTO dtoItem)
        {
            var styleItemIdList = db.Listings.GetStyleItemIdListFromListingsBySKU(dtoItem.SKU);
            if (styleItemIdList.Any())
            {
                return styleItemIdList[0];
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dtoItem"></param>
        /// <param name="canCreate"></param>
        /// <returns></returns>
        public StyleItemDTO FindOrCreateStyleAndStyleItemForItem(IUnitOfWork db,
            int itemTypeId,
            ItemDTO dtoItem,
            bool canCreate,
            bool noVariation)
        {
            var styleString = dtoItem.StyleString;
            var styleId = dtoItem.StyleId;
            Style style = null;

            if (styleId.HasValue)
            {
                style = db.Styles.GetAllActive().FirstOrDefault(s => s.Id == styleId.Value);
            }
            if (style == null && !String.IsNullOrEmpty(styleString))
            {
                style = db.Styles.GetAllActive().FirstOrDefault(s => s.StyleID == styleString);
            }

            if (style == null 
                && !String.IsNullOrEmpty(styleString) 
                && canCreate)
            {
                _log.Info("Style create, from ItemId=" + dtoItem.Id + ", ASIN=" + dtoItem.ASIN + ", MarketplaceId=" +dtoItem.MarketplaceId);
                _log.Info("New style info, styleId=" + styleString + ", itemTypeId=" + itemTypeId + ", name=" + dtoItem.Name + ", imageUrl=" + dtoItem.ImageUrl);
                style = db.Styles.Store(styleString,
                    itemTypeId,
                    SizeHelper.ExcludeSizeInfo(dtoItem.Name),
                    dtoItem.ImageUrl,
                   _time.GetAppNowTime());
            }

            if (style != null)
            {
                var foundStyleItem = db.StyleItems.FindOrCreateForItem(style.Id,
                    itemTypeId,
                    dtoItem,
                    canCreate,
                    SizeMode.MappingNaming,
                    _time.GetAppNowTime());

                //NOTE: Can be null, in this case take only styleId
                if (foundStyleItem == null)
                {
                    _log.Info("FindOrCreateForItem, Size=" + dtoItem.Size + " is empty");
                    return new StyleItemDTO()
                    {
                        StyleId = style.Id,
                    };
                }
                else
                {
                    return foundStyleItem;
                }
            }

            return null;
        }

        public bool UpdateStyleImageIfEmpty(IUnitOfWork db,
            long styleId,
            string imageUrl)
        {
            var style = db.Styles.GetAllActive().FirstOrDefault(s => s.Id == styleId);
            if (style != null)
            {
                if (String.IsNullOrEmpty(style.Image))
                {
                    _log.Info("Style image was updated, styleId=" + styleId + ", image=" + imageUrl);
                    _styleHistoryService.AddRecord(styleId,
                        StyleHistoryHelper.PictureKey,
                        style.Image,
                        imageUrl,
                        null);

                    style.Image = imageUrl;
                    
                    db.StyleImages.Add(new StyleImage()
                    {
                        StyleId = style.Id,
                        Image = imageUrl,
                        Type = (int)StyleImageType.None,
                        CreateDate = _time.GetAppNowTime(),
                    });
                    db.Commit();

                    return true;
                }
            }
            return false;
        }
    }
}
