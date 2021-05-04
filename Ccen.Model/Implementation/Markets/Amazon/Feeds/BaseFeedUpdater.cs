using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Amazon.Api;
using Amazon.Api.Feeds;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Feeds;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DAL;
using Amazon.DTO.Feeds;
using Amazon.Model.Models;
using Amazon.Utils;
using MarketplaceWebService;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public abstract class BaseFeedUpdater
    {
        protected class DocumentInfo
        {
            public XmlDocument XmlDocument { get; set; }
            public string TextDocument { get; set; }
            public IList<FeedItemDTO> FeedItems { get; set; }
            public IList<FeedMessageDTO> Messages { get; set; }

            public int NodesCount { get; set; }

            public bool HasInfo
            {
                get { return XmlDocument != null || !String.IsNullOrEmpty(TextDocument); }
            }
        }

        protected ILogService Log;
        protected ITime Time;
        protected IDbFactory DbFactory;

        protected abstract AmazonFeedType Type { get; }

        protected abstract string AmazonFeedName { get; }


        public BaseFeedUpdater(ILogService log, 
            ITime time,
            IDbFactory dbFactory)
        {
            Log = log;
            Time = time;
            DbFactory = dbFactory;
        }

        protected abstract DocumentInfo ComposeDocument(IUnitOfWork db,
            long companyId,
            MarketType market, 
            string marketplaceId,
            IList<string> asinList);

        protected abstract void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId);
        protected abstract void UpdateEntitiesAfterResponse(long feedId, 
            IList<FeedResultMessage> errorList);

        public string GetUnprocessedFeedId(string marketplaceId)
        {
            using (var context = new UnitOfWork(Log)) 
            {
                Log.Info("GetUnprocessedFeedId");
                return context.Feeds.GetUnprocessedFeedId((int) Type, MarketType.None, marketplaceId);
            }
        }

        public string GetUnprocessedFeedId(long feedId)
        {
            using (var context = new UnitOfWork(Log))
            {
                Log.Info("GetUnprocessedFeedId");
                return context.Feeds.Get(feedId)?.AmazonIdentifier;
            }
        }

        public long? SubmitFeed(AmazonApi api, 
            long companyId, 
            IList<string> skuList,
            string forRequestDirectory)
        {
            long? feedId = null;
            using (var db = DbFactory.GetRWDb())
            {
                var documentInfo = ComposeDocument(db, companyId, api.Market, api.MarketplaceId, skuList);
                if (documentInfo != null && documentInfo.HasInfo)
                {
                    Log.Info(api.MarketplaceId + ": Submit feed");

                    MemoryStream stream = null;
                    string feedRequestId = "";
                    try
                    {
                        if (documentInfo.XmlDocument != null)
                            stream = StreamHelper.GetStreamFromXml(documentInfo.XmlDocument);
                        if (documentInfo.TextDocument != null)
                            stream = StreamHelper.GetStreamFromString(documentInfo.TextDocument);

                        feedRequestId = api.SubmitFeed(Log,
                            stream,
                            AmazonFeedName,
                            Type == AmazonFeedType.OrderFulfillment);

                        if (!string.IsNullOrEmpty(feedRequestId))
                        {
                            Log.Info(string.Format("Insert new feed {0}, messages: {1}", feedRequestId,
                                documentInfo.NodesCount));
                            feedId = db.Feeds.InsertFeed(feedRequestId, 
                                documentInfo.NodesCount, 
                                (int)Type, 
                                (int)FeedStatus.Submitted,
                                api.Market,
                                api.MarketplaceId);

                            if (documentInfo.FeedItems != null)
                            {
                                documentInfo.FeedItems.ForEach(f => f.FeedId = feedId.Value);
                                db.FeedItems.Insert(documentInfo.FeedItems);
                            }

                            UpdateEntitiesBeforeSubmitFeed(db, feedId.Value);
                            Log.Info("Commit");
                            db.Commit();

                            Log.Info("Save submitted feed into file");
                            var toDirectory = FileHelper.ToDirectoryNameWithBackslash(forRequestDirectory) + Time.GetAppNowTime().ToString("yyyy_MM_dd");
                            if (!Directory.Exists(toDirectory))
                                Directory.CreateDirectory(toDirectory);

                            var saveToFilepath = toDirectory + "/" + string.Format("{0}_{1}_{2}.xml", api.MarketplaceId ?? "", Type, feedId);
                            using (var fileStream = File.Create(saveToFilepath))
                            {
                                stream.WriteTo(fileStream);
                            }
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Close();
                    }
                }
            }
            return feedId;
        }
        
        public void UpdateSubmittedFeed(AmazonApi api,
            string feedId,
            string forResponseDirectory)
        {
            var marketplaceId = api.MarketplaceId;
            Stream result = null;
            try
            {
                result = api.GetFeedSubmissionResult(feedId);
            }
            catch (MarketplaceWebServiceReportsException ex)
            {
                //Feed Submission Result not available. Feed Submission has been canceled for Feed: 67659017019
                if (ex.Message.Contains("Feed Submission has been canceled"))
                {
                    Log.Error("GetFeedSubmissionResult", ex);
                    using (var db = new UnitOfWork(Log))
                    {
                        var feed = db.Feeds.GetFiltered(f => f.AmazonIdentifier == feedId && f.MarketplaceId == marketplaceId)
                                .First();
                        feed.Status = (int)FeedStatus.Cancelled;
                        db.Commit();
                        Log.Info("Feed market as deleted, feedId=" + feedId);
                    }
                }
                else
                {
                    throw ex;
                }
            }
            if (result != null)
            {
                //var now = DateHelper.GetAppNowTime();
                Log.Info("Stream to document");
                XmlDocument document = null;
                try
                {
                    document = StreamHelper.GetXmlFromStream(result);
                }
                catch (Exception ex)
                {
                    Log.Error("GetDocumentXml", ex);
                }

                IList<FeedResultMessage> errorList = new List<FeedResultMessage>();
                long dbFeedId = 0;
                using (var db = DbFactory.GetRWDb())
                {
                    Log.Info("Get feed");
                    var feed = db.Feeds.GetFiltered(f => f.AmazonIdentifier == feedId && f.MarketplaceId == marketplaceId).First();
                    errorList = FeedHelper.GetErrorMessageList(document);
                    dbFeedId = feed.Id;

                    //Save file
                    var toDirectory = FileHelper.ToDirectoryNameWithBackslash(forResponseDirectory) + Time.GetAppNowTime().ToString("yyyy_MM_dd");
                    if (!Directory.Exists(toDirectory))
                        Directory.CreateDirectory(toDirectory);

                    using (var errorfileStream = File.Open(toDirectory + "/" + string.Format("{0}_{1}_{2}.xml", marketplaceId ?? "", Type, feedId),
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite))
                    {
                        result.Position = 0;
                        result.CopyTo(errorfileStream);
                    }

                    if (errorList.Any())
                    {
                        Log.Info("Errors found");
                    }
                    else
                    {
                        Log.Info("No errors");
                    }

                    if (errorList.Any())
                    {
                        feed.Status = (int)FeedStatus.ProcessedWithErrors;
                    }
                    else
                    {
                        feed.Status = (int)FeedStatus.Processed;
                    }

                    db.FeedMessages.Insert(errorList.Select(e => new FeedMessageDTO()
                    {
                        FeedId = feed.Id,
                        Message = e.Message,
                        MessageId = e.MessageId,
                        MessageCode = e.MessageCode,
                        ResultCode = e.ResultCode,
                        CreateDate = Time.GetAppNowTime(),
                    }).ToList());
                    
                    db.Commit();
                }

                try
                {
                    UpdateEntitiesAfterResponse(dbFeedId, errorList);
                }
                catch (Exception ex)
                {
                    Log.Fatal("Amazon: error when processing feed submission result", ex);
                }
            }
            else
            {
                Log.Info(string.Format("No result by api for feed type: {0}; feedId {1}!", Type, feedId));
            }
        }

        protected void DefaultActionUpdateEntitiesAfterResponse(long feedId,
            IList<FeedResultMessage> errorList,
            string itemAdditionalFieldName,
            int maxAttemptCount)
        {
            using (var db = DbFactory.GetRWDb())
            {
                var feedItems = db.FeedItems.GetAllAsDto().Where(fi => fi.FeedId == feedId).ToList();
                var systemActionIds = feedItems.Select(f => f.ItemId).ToList();
                var systemActions = db.SystemActions.GetAll().Where(i => systemActionIds.Contains(i.Id)).ToList();
                var itemIds = systemActions.Select(a => StringHelper.TryGetInt(a.Tag))
                    .Where(a => a.HasValue)
                    .Select(a => a.Value)
                    .ToList();

                //Remove all exist errors
                var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
                            && i.Field == itemAdditionalFieldName).ToList();
                foreach (var dbExistError in dbExistErrors)
                {
                    db.ItemAdditions.Remove(dbExistError);
                }

                foreach (var action in systemActions)
                {
                    var feedItem = feedItems.FirstOrDefault(fi => fi.ItemId == action.Id);
                    if (feedItem != null)
                    {
                        var itemErrors = errorList.Where(e => e.MessageId == feedItem.MessageId).ToList();
                        TextMessageOutput output = null;
                        SystemActionStatus status = SystemActionStatus.None;
                        if (itemErrors.Any())
                        {
                            action.AttemptNumber++;
                            action.AttemptDate = Time.GetAppNowTime();
                            if (action.AttemptNumber > maxAttemptCount)
                                action.Status = (int)SystemActionStatus.Fail;

                            var itemId = StringHelper.TryGetLong(action.Tag);

                            if (itemId.HasValue)
                            {
                                foreach (var itemError in itemErrors)
                                {
                                    db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                                    {
                                        ItemId = (int)itemId.Value,
                                        Field = itemAdditionalFieldName,
                                        Value = itemError.Message,
                                        CreateDate = Time.GetAppNowTime(),
                                    });
                                }
                            }

                            output = new TextMessageOutput()
                            {
                                IsSuccess = false,
                                Messages = itemErrors.Select(i => new MessageString()
                                {
                                    Status = MessageStatus.Error,
                                    Message = i.Message
                                }).ToList()
                            };
                            status = SystemActionStatus.Fail;
                        }
                        else
                        {
                            output = new TextMessageOutput()
                            {
                                IsSuccess = false
                            };
                            status = SystemActionStatus.Done;
                        }
                        action.OutputData = SystemActionHelper.ToStr(output);
                        action.Status = (int)status;

                        Log.Info("Update action status, actionId=" + action.Id + ", status=" + action.Status);
                    }
                }

                db.Commit();
            }
        }

        protected void UpdatePrePublishErrors(IUnitOfWork db,
            IList<int> itemIds,
            IList<FeedMessageDTO> messages,
            string itemAdditionalFieldName)
        {
            var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
                                        && i.Field == itemAdditionalFieldName).ToList();
            foreach (var dbExistError in dbExistErrors)
            {
                db.ItemAdditions.Remove(dbExistError);
            }

            foreach (var message in messages)
            {
                db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                {
                    ItemId = (int)message.Id,
                    Value = message.Message,
                    Field = ItemAdditionFields.PrePublishError,
                    CreateDate = Time.GetAppNowTime(),
                });
            }
            db.Commit();
        }
    }
}
