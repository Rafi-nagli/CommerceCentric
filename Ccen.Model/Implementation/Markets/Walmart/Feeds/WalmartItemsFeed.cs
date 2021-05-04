using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Feeds;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Utils;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    public class WalmartItemsFeed : BaseWalmartFeed, IFeed
    {
        private string _feedBaseDirectory;
        private string _swatchImageDirectory;
        private string _swatchImageBaseUrl;
        private string _walmartImageDirectory;
        private string _walmartImageBaseUrl;

        public override int FeedType
        {
            get { return (int) WalmartReportType.Items; }
        }

        public WalmartItemsFeed(ILogService log,
            ITime time,
            IWalmartApi api,
            IDbFactory dbFactory,
            string feedBaseDirectory,
            string swatchImageDirectory,
            string swatchImageBaseUrl,
            string walmartImageDirectory,
            string walmartImageBaseUrl) : base(log, dbFactory, api, time)
        {
            _feedBaseDirectory = feedBaseDirectory;

            _swatchImageDirectory = swatchImageDirectory;
            _swatchImageBaseUrl = swatchImageBaseUrl;

            _walmartImageDirectory = walmartImageDirectory;
            _walmartImageBaseUrl = walmartImageBaseUrl;
        }

        public void SubmitFeed()
        {
            SubmitFeed(null);
        }

        public void SubmitFeed(IList<string> asinList)
        {
            SubmitFeed(null, PublishedStatuses.None);
        }
        
        public void SubmitFeed(IList<string> asinList, PublishedStatuses overrideStatus)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toDate = _time.GetAppNowTime().AddHours(-2);
                IList<ItemDTO> itemsToSubmit;

                if (asinList == null || !asinList.Any())
                {
                    itemsToSubmit = (from i in db.Items.GetAll()
                                    join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                    where (i.ItemPublishedStatus == (int)PublishedStatuses.None
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.New
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithProductId
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithSKU
                                                 || ((i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                                      || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                                     && (i.ItemPublishedStatusDate < toDate
                                                        || !i.ItemPublishedStatusDate.HasValue)))
                                                //NOTE: Added in-progress statuses if items in it more then one day
                                                && i.Market == (int)_api.Market
                                                //&& !String.IsNullOrEmpty(i.Barcode)
                                                && i.StyleItemId.HasValue
                                                && !l.IsRemoved
                                                && !String.IsNullOrEmpty(i.Barcode)
                                    select new ItemDTO()
                                    {
                                        Id = i.Id,
                                        PuclishedStatusDate = i.ItemPublishedStatusDate,
                                        CreateDate = i.CreateDate,
                                        ParentASIN = i.ParentASIN,
                                    })
                        .ToList();
                }
                else
                {
                    itemsToSubmit = (from i in db.Items.GetAll()
                              join l in db.Listings.GetAll() on i.Id equals l.ItemId
                              where i.Market == (int)_api.Market
                                    && !l.IsRemoved
                                    //&& !String.IsNullOrEmpty(i.Barcode)
                                    && i.StyleItemId.HasValue
                                    && asinList.Contains(l.SKU)
                                select new ItemDTO()
                                {
                                    Id = i.Id,
                                    PuclishedStatusDate = i.ItemPublishedStatusDate,
                                    CreateDate = i.CreateDate,
                                    ParentASIN = i.ParentASIN,
                                })
                            .ToList();
                }

                //NOTE: Need to submit items with group, otherwise we have incorrect color variations calculation, and sometimes image calculation
                var parentInfoQuery = from i in itemsToSubmit
                                   group i by i.ParentASIN into byParent
                                   select new
                                   {
                                       ParentASIN = byParent.Key,
                                       ItemPublishedStatusDate = byParent.Min(i => i.PuclishedStatusDate ?? i.CreateDate)
                                   };

                var parentASINList = parentInfoQuery
                    .OrderBy(pi => pi.ItemPublishedStatusDate)
                    .Select(pi => pi.ParentASIN) //NOTE: Walmart has issue with large request ("(413) Request Entity Too Large.")
                    .ToList();

                var itemDtoList = db.Items.GetAllActualExAsDto()
                      .Where(i => parentASINList.Contains(i.ParentASIN)
                        && i.Market == (int)_api.Market
                        && !String.IsNullOrEmpty(i.Barcode)).ToList();

                _log.Info("Parent ASIN Count to submit=" + parentASINList.Count + ", item actually submitted=" + itemDtoList.Count + " (items in queue to submit: " + itemsToSubmit.Count + ")");
                _log.Info("SKUs to submit: " + String.Join(", ", itemDtoList.Select(i => i.SKU)));

                if (overrideStatus != PublishedStatuses.None)
                {
                    var toOverrideItems = itemDtoList.Where(i => asinList.Contains(i.SKU)).ToList();
                    toOverrideItems.ForEach(i => i.PublishedStatus = (int)overrideStatus);
                }

                //var parentASINList = dbItems.Select(i => i.ParentASIN).Distinct().ToList();
                var parentItemDtoList = db.ParentItems.GetAllAsDto().Where(p => parentASINList.Contains(p.ASIN)
                                                                         && p.Market == (int) _api.Market)
                    .ToList();

                var styleIdList = itemDtoList.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                var styleList = db.Styles.GetAllAsDtoEx().Where(s => styleIdList.Contains(s.Id)).ToList();
                var allStyleImageList = db.StyleImages.GetAllAsDto().Where(sim => styleIdList.Contains(sim.StyleId)
                    && sim.Category != (int)StyleImageCategories.Deleted).ToList();
                var allFeatures = db.FeatureValues.GetValuesByStyleIds(styleIdList);

                var allBrandMappings = db.WalmartBrandInfoes.GetAllAsDto().ToList();

                foreach (var item in itemDtoList)
                {
                    var parent = parentItemDtoList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                    if (parent != null && parent.OnHold)
                        item.OnHold = parent.OnHold;

                    var styleForItem = styleList.FirstOrDefault(s => s.Id == item.StyleId);

                    var features = allFeatures.Where(f => f.StyleId == item.StyleId).ToList();
                    var subLiscense =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.SUB_LICENSE1));
                    var mainLicense =
                        StyleFeatureHelper.PrepareMainLicense(
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MAIN_LICENSE)),
                            subLiscense);

                    var brand = StringHelper.GetFirstNotEmpty(mainLicense, styleForItem.Manufacturer);
                    var brandMapping = allBrandMappings.FirstOrDefault(b => b.GlobalBrandLicense == brand && StringHelper.ContainsNoCase(b.Character, subLiscense));
                    if (brandMapping == null)
                    {
                        var brandMappingCandidates = allBrandMappings.Where(b => b.GlobalBrandLicense == brand).ToList();
                        if (brandMappingCandidates.Count == 1)
                            brandMapping = brandMappingCandidates.FirstOrDefault();
                    }

                    var character = StringHelper.GetFirstNotEmpty(brandMapping?.Character,
                        StyleFeatureHelper.GetFeatureValueByName(features, StyleFeatureHelper.MAIN_CHARACTER_KEY));
                    StyleFeatureHelper.AddOrUpdateFeatureValueByName(allFeatures, styleForItem.Id, StyleFeatureHelper.MAIN_CHARACTER_KEY, character);

                    var globalLicense = StringHelper.GetFirstNotEmpty(brandMapping?.GlobalBrandLicense,
                        brand);
                    StyleFeatureHelper.AddOrUpdateFeatureValueByName(allFeatures, styleForItem.Id, StyleFeatureHelper.GLOBAL_LICENSE_KEY, globalLicense);

                    brand = StringHelper.GetFirstNotEmpty(brandMapping?.Brand, brand);
                    StyleFeatureHelper.AddOrUpdateFeatureValueByName(allFeatures, styleForItem.Id, StyleFeatureHelper.BRAND_KEY, brand);

                    if (String.IsNullOrEmpty(styleForItem.BulletPoint1))
                    {   
                        var material =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL));
                        var gender =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.GENDER));

                        var bulletPoints = ExportDataHelper.BuildKeyFeatures(mainLicense,
                            subLiscense,
                            material,
                            gender);

                        styleForItem.BulletPoint1 = bulletPoints.Count > 0 ? bulletPoints[0] : null;
                        styleForItem.BulletPoint2 = bulletPoints.Count > 1 ? bulletPoints[1] : null;
                        styleForItem.BulletPoint3 = bulletPoints.Count > 2 ? bulletPoints[2] : null;
                        styleForItem.BulletPoint4 = bulletPoints.Count > 3 ? bulletPoints[3] : null;
                        styleForItem.BulletPoint5 = bulletPoints.Count > 4 ? bulletPoints[4] : null;

                        item.Features = String.Join(";", bulletPoints);
                    }
                    else
                    {
                        //var itemStyleValue = WalmartUtils.GetFeatureValue(allFeatures.Where(f => f.StyleId == itemStyle.Id).ToList(), StyleFeatureHelper.ITEMSTYLE);

                        item.Features = String.Join(";", StyleFeatureHelper.PrepareBulletPoints(new string[]
                        {
                            styleForItem.BulletPoint1,
                            styleForItem.BulletPoint2,
                            styleForItem.BulletPoint3,
                            styleForItem.BulletPoint4,
                            styleForItem.BulletPoint5,
                        } ));
                    }
                }

                //Exclude OnHold (after ParentItem onHold was applied)
                //itemDtoList = itemDtoList.Where(i => !i.OnHold).ToList();

                foreach (var styleImage in allStyleImageList)
                {
                    try
                    {
                        var styleString = styleList.FirstOrDefault(s => s.Id == styleImage.StyleId)?.StyleID;

                        var filepath = ImageHelper.BuildWalmartImage(_walmartImageDirectory, 
                            styleImage.Image,
                            styleString + "_" + MD5Utils.GetMD5HashAsString(styleImage.Image));
                        var filename = Path.GetFileName(filepath);
                        styleImage.Image = UrlUtils.CombinePath(_walmartImageBaseUrl, filename);
                    }
                    catch (Exception ex)
                    {
                        _log.Info("BuildWalmartImage error, image=" + styleImage.Image, ex);
                    }
                }

                foreach (var style in styleList)
                {
                    //var styleImages = allStyleImageList.Where(sim => sim.StyleId == style.Id).ToList();

                    //var styleImage = styleImages.Where(si => si.Type == (int) StyleImageType.LoRes
                    //                                            || si.Type == (int) StyleImageType.HiRes)
                    //    .OrderByDescending(si => si.Type) //First Hi-res
                    //    .ThenBy(si => si.IsSystem) //First not system
                    //    .ThenByDescending(si => si.IsDefault) //First isDefault
                    //    .FirstOrDefault();

                    //if (styleImage != null)
                    //    style.Image = styleImage.Image; //Set to Hi-res

                    try
                    {
                        var swatchImage = style.Image;
                        var swatchImageObj = allStyleImageList.FirstOrDefault(st => st.Id == style.Id
                            && st.Category == (int)StyleImageCategories.Swatch);
                        if (swatchImageObj != null)
                            swatchImage = swatchImageObj.Image;

                        var filepath = ImageHelper.BuildSwatchImage(_swatchImageDirectory, swatchImage);
                        var filename = Path.GetFileName(filepath);
                        style.SwatchImage = UrlUtils.CombinePath(_swatchImageBaseUrl, filename);
                    }
                    catch (Exception ex)
                    {
                        _log.Info("BuildSwatchImage error, image=" + style.Image, ex);
                    }
                }



                if (itemDtoList.Any())
                {
                    _log.Info("Items to submit=" + String.Join(", ", itemDtoList.Select(i => i.SKU).ToList()));

                    var newFeed = new Feed()
                    {
                        Market = (int)Market,
                        MarketplaceId = MarketplaceId,
                        Type = (int)FeedType,
                        Status = (int)FeedStatus.Submitted,
                        MessageCount = itemDtoList.Count,
                        SubmitDate = _time.GetAppNowTime()
                    };
                    db.Feeds.Add(newFeed);
                    db.Commit();

                    _log.Info("Feed id=" + newFeed.Id);

                    IList<FeedMessageDTO> messages;

                    var submitResult = _api.SubmitItemsFeed(newFeed.Id.ToString(), 
                        itemDtoList,
                        parentItemDtoList,
                        styleList,
                        allStyleImageList,
                        allFeatures,
                        _feedBaseDirectory,
                        out messages);

                    #region Update item errors
                    var itemIds = itemDtoList.Select(i => i.Id).ToList();
                    //Remove all exist errors
                    var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
                                && (i.Field == ItemAdditionFields.PrePublishError)).ToList();
                    foreach (var dbExistError in dbExistErrors)
                    {
                        db.ItemAdditions.Remove(dbExistError);
                    }
                    foreach (var message in messages)
                    {
                        db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                        {
                            ItemId = (int)message.ItemId.Value,
                            Field = ItemAdditionFields.PrePublishError,
                            Value = message.Message,
                            Source = newFeed.Id.ToString(),
                            CreateDate = _time.GetAppNowTime(),
                        });
                    }
                    db.Commit();
                    #endregion

                    if (submitResult.IsSuccess)
                    {
                        _log.Info("Walmart feed id=" + submitResult.Data);

                        newFeed.AmazonIdentifier = submitResult.Data.AmazonIdentifier;
                        newFeed.RequestFilename = submitResult.Data.RequestFilename;
                        db.Commit();

                        var dbItems = db.Items.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();
                        foreach (var item in dbItems)
                        {
                            _log.Info("Mark item as in-progress, id=" + item.Id);

                            item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                            item.ItemPublishedStatus = (int) PublishedStatuses.ChangesSubmited;
                            item.ItemPublishedStatusDate = _time.GetAppNowTime();
                        }
                        db.Commit();

                        foreach (var item in itemDtoList)
                        {
                            db.FeedItems.Add(new Core.Entities.Feeds.FeedItem()
                            {
                                FeedId = newFeed.Id,
                                ItemId = item.Id,
                            });
                            db.Commit();
                        }

                        _log.Info("Feed submitted, feedId=" + newFeed.AmazonIdentifier);
                    }
                    else
                    {
                        _log.Fatal("Feed DIDN'T submitted, mark feed as deleted");

                        newFeed.Status = (int)FeedStatus.SubmissionFail;
                        db.Commit();

                        throw new Exception("Unable to submit feed, submission status failed. Details: " + submitResult.Message);
                    }
                }
            }
        }

        protected override void ProcessFeedItem(IUnitOfWork db, WalmartFeedItemDTO feedItem, DateTime feedSubmitDate)
        {
            var status = WalmartUtils.ConvertFromFeedItemPublishedStatusToStandard(feedItem.Status);
            var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == feedItem.ItemId
                && l.Market == (int)_api.Market
                && !l.IsRemoved);
            feedItem.Errors = feedItem.Errors ?? new List<ItemErrorDTO>();
            if (dbListing != null)
            {
                var dbItem = db.Items.GetAll().FirstOrDefault(i => i.Id == dbListing.ItemId);
                if (dbItem != null)
                {
                    _log.Info("Status has been updated");

                    var dbItemAdditions = db.ItemAdditions
                        .GetAll()
                        .Where(ia => ia.ItemId == dbItem.Id
                            && ia.Field == ItemAdditionFields.PublishError)
                        .ToList();

                    if (dbItemAdditions.Count > 0
                        && !feedItem.Errors.Any())
                    {
                        foreach (var dbItemAddition in dbItemAdditions)
                        {
                            db.ItemAdditions.Remove(dbItemAddition);
                        }
                    }
                    else
                    {
                        if (dbItemAdditions.Count == 0
                            && feedItem.Errors.Any())
                        {
                            foreach (var feedError in feedItem.Errors)
                            {
                                db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                                {
                                    CreateDate = _time.GetAmazonNowTime(),
                                    Field = ItemAdditionFields.PublishError,
                                    ItemId = dbItem.Id,
                                    Value = feedError.Message + ", Code: " + feedError.Code + ", Type: " + feedError.Type,
                                    Source = feedItem.FeedId.ToString()                                    
                                });
                            }
                        }
                        else
                        {
                            if (dbItemAdditions.Count == feedItem.Errors.Count)
                            {
                                for (var i = 0; i < dbItemAdditions.Count; i++)
                                {
                                    dbItemAdditions[i].CreateDate = _time.GetAmazonNowTime();
                                    dbItemAdditions[i].Value = feedItem.Errors[i].Message + ", Code: " + feedItem.Errors[i].Code + ", Type: " + feedItem.Errors[i].Type;
                                    dbItemAdditions[i].Source = feedItem.FeedId.ToString();
                                }
                            }
                            else
                            {
                                foreach (var dbItemError in dbItemAdditions)
                                {
                                    db.ItemAdditions.Remove(dbItemError);
                                }
                                foreach (var feedError in feedItem.Errors)
                                {
                                    db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                                    {
                                        CreateDate = _time.GetAppNowTime(),
                                        Field = ItemAdditionFields.PublishError,
                                        ItemId = dbItem.Id,
                                        Value = feedError.Message + ", Code: " + feedError.Code + ", Type: " + feedError.Type,
                                        Source = feedItem.FeedId.ToString()
                                    });
                                }
                            }
                        }
                    }

                    if (dbItem.ItemPublishedStatusDate < feedSubmitDate.AddMinutes(15)) //NOTE: update status when if they was updated before submittion
                    {
                        dbItem.ItemPublishedStatus = (int)status;
                        dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                    }
                }
            }
        }
    }
}
