using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.DropShippers;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.DropShippers;
using Ccen.Web.ViewModels.CustomFeeds;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.CustomFeeds
{
    public class CustomIncomingFeedViewModel
    {
        public long Id { get; set; }

        public string FeedName { get; set; }
        public string FileType { get; set; }

        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }
        public int? OverrideDSFeedType { get; set; }
        public int? OverrideDSProductType { get; set; }

        public string FtpSite { get; set; }
        public string FtpFolder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsSFTP { get; set; }
        public bool IsPassiveMode { get; set; }

        public DateTime CreateDate { get; set; }

        public string OverrideDSFeedTypeName
        {
            get
            {
                if (!OverrideDSFeedType.HasValue)
                    return "";
                return DSFileTypeHelper.GetName((DSFileTypes)OverrideDSFeedType.Value);
            }
        }

        public string OverrideDSProductTypeName
        {
            get
            {
                if (!OverrideDSProductType.HasValue)
                    return "";
                return DSProductTypeHelper.GetName((DSProductType)OverrideDSProductType.Value);
            }
        }

        public DateTime FormattedCreateDate
        {
            get { return DateHelper.ConvertUtcToApp(CreateDate); }
        }

        public IList<CustomFeedFieldViewModel> Fields { get; set; }
        public IList<CustomFeedSourceFieldViewModel> AllSourceFields { get; set; }


        public IList<MessageString> Validate()
        {
            return new List<MessageString>();
        }

        public CustomIncomingFeedViewModel()
        {

        }

        public CustomIncomingFeedViewModel(IUnitOfWork db,
            ICustomFeedService feedService,
            long? id)
        {
            if (id.HasValue)
            {
                var feed = db.CustomFeeds.Get(id.Value);

                Id = feed.Id;

                FeedName = feed.FeedName;
                DropShipperId = feed.DropShipperId;
                OverrideDSFeedType = feed.OverrideDSFeedType;
                OverrideDSProductType = feed.OverrideDSProductType;

                FtpSite = feed.FtpSite;
                FtpFolder = feed.FtpFolder;
                UserName = feed.UserName;
                IsPassiveMode = feed.IsPassiveMode;
                IsSFTP = feed.IsSFTP;
                Password = feed.Password;
                CreateDate = feed.CreateDate;

                if (DropShipperId.HasValue)
                {
                    var dropShipper = db.DropShippers.GetAllAsDto()
                        .FirstOrDefault(ds => ds.Id == feed.DropShipperId);
                    DropShipperName = dropShipper?.Name;
                }

                var fields = db.CustomFeedFields.GetAllAsDto()
                    .Where(f => f.CustomFeedId == id)
                    .OrderBy(f => f.SortOrder)
                    .ToList();
                Fields = CustomFeedFieldViewModel.Build(fields);
            }
            else
            {
                Fields = GetFeedFields(feedService,
                    (DSFileTypes)(OverrideDSFeedType ?? (int)DSFileTypes.ItemsFull),
                    (DSProductType)(OverrideDSProductType ?? (int)DSProductType.Watches));

                OverrideDSFeedType = (int)DSFileTypes.ItemsFull;
                OverrideDSProductType = (int)DSProductType.Watches;
            }
        }

        public static IList<CustomFeedFieldViewModel> GetFeedFields(ICustomFeedService feedService,
            DSFileTypes dsFeedType,
            DSProductType dsProductType)
        {
            return CustomFeedFieldViewModel.Build(
                feedService.GetSourceFieldsListForIncomingFeed(dsFeedType, dsProductType).Select(i => new CustomFeedFieldDTO()
                {
                    SourceFieldName = i,
                }).ToList());
        }

        public static IList<CustomIncomingFeedViewModel> GetAll(IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var query = from a in db.CustomFeeds.GetAllAsDto()
                            join ds in db.DropShippers.GetAllAsDto() on a.DropShipperId equals ds.Id
                            where a.DropShipperId.HasValue
                                && (a.OverrideDSFeedType == (int)DSFileTypes.ItemsFull
                                    || a.OverrideDSFeedType == (int)DSFileTypes.ItemsLite)
                            select new CustomIncomingFeedViewModel()
                            {
                                Id = a.Id,
                                FeedName = a.FeedName,

                                DropShipperId = a.DropShipperId,
                                DropShipperName = ds.Name,
                                OverrideDSFeedType = a.OverrideDSFeedType,
                                OverrideDSProductType = a.OverrideDSProductType,

                                FtpSite = a.FtpSite,
                                FtpFolder = a.FtpFolder,
                                UserName = a.UserName,
                                IsPassiveMode = a.IsPassiveMode,
                                IsSFTP = a.IsSFTP,
                                Password = a.Password,
                                CreateDate = a.CreateDate
                            };
                return query.ToList();
            }
        }

        public static void Delete(IDbFactory dbFactory, long id)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var customFeed = db.CustomFeeds.Get(id);
                db.CustomFeeds.Remove(customFeed);
                db.Commit();
            }
        }

        public static CallMessagesResultVoid Apply(IUnitOfWork db, CustomIncomingFeedViewModel model, DateTime when, long? by)
        {
            CustomFeed feed = null;
            if (model.Id > 0)
            {
                feed = db.CustomFeeds.Get(model.Id);
            }
            else
            {
                feed = new CustomFeed();
                feed.CreateDate = when;
                feed.CreatedBy = by;
                db.CustomFeeds.Add(feed);
            }
            feed.FeedName = model.FeedName;

            feed.DropShipperId = model.DropShipperId;
            feed.OverrideDSFeedType = model.OverrideDSFeedType;
            feed.OverrideDSProductType = model.OverrideDSProductType;

            feed.FtpSite = model.FtpSite;
            feed.FtpFolder = model.FtpFolder;
            feed.UserName = model.UserName;
            feed.Password = model.Password;
            feed.IsPassiveMode = model.IsPassiveMode;
            feed.IsSFTP = model.IsSFTP;
            feed.UpdateDate = when;
            db.Commit();

            IList<CustomFeedFieldDTO> fields = new List<CustomFeedFieldDTO>();
            for (var i = 0; i < model.Fields.Count; i++)
            {
                fields.Add(new CustomFeedFieldDTO()
                {
                    CustomFeedId = feed.Id,
                    Id = model.Fields[i].Id,
                    SourceFieldName = model.Fields[i].SourceFieldName,
                    CustomFieldName = model.Fields[i].CustomFieldName,
                    CustomFieldValue = model.Fields[i].CustomFieldValue,
                    SortOrder = i + 1,
                });
            }
            db.CustomFeedFields.BulkUpdateForFeed(feed.Id, fields, when, by);

            db.Commit();

            return new CallMessagesResultVoid();
        }

        public CallMessagesResultVoid CheckConnection()
        {
            try
            {
                FtpClient client = new FtpClient(FtpSite);
                if (!String.IsNullOrEmpty(UserName))
                    client.Credentials = new NetworkCredential(UserName, Password);
                client.Connect();
                return new CallMessagesResultVoid()
                {
                    Status = CallStatus.Success,
                    Messages = new List<MessageString>()
                    {
                        MessageString.Success("Successfully connected")
                    }
                };
            }
            catch (Exception ex)
            {
                return CallMessagesResultVoid.Fail(ex.Message, null);
            }
        }
    }
}