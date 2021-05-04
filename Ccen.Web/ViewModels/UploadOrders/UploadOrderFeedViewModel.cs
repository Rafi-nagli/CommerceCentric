using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Categories;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Feeds;
using Newtonsoft.Json;

namespace Amazon.Web.ViewModels.UploadOrders
{
    public class UploadOrderFeedViewModel
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public long? FieldMappingsId { get; set; }

        public int Status { get; set; }

        public string FormattedStatus
        {
            get
            {
                var status = (SystemActionStatus)Status;
                switch (status)
                {
                    case SystemActionStatus.Fail:
                        return "Fail";
                    case SystemActionStatus.Done:
                        return "Done";
                    case SystemActionStatus.InProgress:
                        return "In progress";
                    case SystemActionStatus.None:
                        return "Pending";
                }
                return "Pending";
            }
        }

        public int ProgressPercent { get; set; }
        public DateTime CreateDate { get; set; }


        public int? ParsedCount { get; set; }
        public int? MatchedCount { get; set; }
        public int? Valid1OperationCount { get; set; }
        public int? Valid2OperationCount { get; set; }
        public int? Processed1OperationCount { get; set; }
        public int? Processed2OperationCount { get; set; }


        public DateTime FormattedCreateDate
        {
            get { return DateHelper.ConvertUtcToApp(CreateDate); }
        }

        public UploadOrderFeedViewModel()
        {

        }

        public UploadOrderFeedViewModel(SystemActionDTO action)
        {
            Id = action.Id;
            Status = action.Status;
            var inputModel = SystemActionHelper.FromStr<PublishFeedInput>(action.InputData);
            var outputModel = SystemActionHelper.FromStr<PublishFeedOutput>(action.OutputData);

            FileName = inputModel.FileName;
            FieldMappingsId = inputModel.FieldMappingsId;

            ProgressPercent = outputModel?.ProgressPercent ?? 0;

            CreateDate = action.CreateDate;

            ParsedCount = outputModel?.ParsedCount;
            MatchedCount = outputModel?.MatchedCount;
            Valid1OperationCount = outputModel?.Valid1OperationCount;
            Valid2OperationCount = outputModel?.Valid2OperationCount;
            Processed1OperationCount = outputModel?.Processed1OperationCount;
            Processed2OperationCount = outputModel?.Processed2OperationCount;
        }

        public IList<MessageString> Validate()
        {
            List<MessageString> messages = new List<MessageString>();

            if (String.IsNullOrEmpty(FileName))
            {
                messages.Add(new MessageString()
                {
                    Message = "File name is empty",
                    Status = MessageStatus.Error
                });
            }

            if (!String.IsNullOrEmpty(FileName))
            {
                var dir = Models.UrlHelper.GetUploadOrderFeedPath();
                var destinationPath = Path.Combine(dir, FileName);
                messages.AddRange(UploadOrderFeedService.ValidateFeed(destinationPath));
            }

            return messages;
        }

        public static CallMessagesResultVoid Add(IUnitOfWork db,
            ISystemActionService actionService,
            UploadOrderFeedViewModel model,
            long? by)
        {
            actionService.AddAction(db,
                SystemActionType.ProcessUploadOrderFeed,
                "",
                new PublishFeedInput()
                {
                    FileName = model.FileName,
                    FieldMappingsId = model.FieldMappingsId,
                },
                null,
                by);

            db.Commit();

            return CallMessagesResultVoid.Success();
        }

        public static IEnumerable<UploadOrderFeedViewModel> GetAll(IUnitOfWork db,
            ITime time,
            UploadOrderFeedFilterViewModel filter)
        {
            var query = db.SystemActions.GetAllAsDto()
                .Where(a => a.Type == (int)SystemActionType.ProcessUploadOrderFeed);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.CreateDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.CreateDate <= filter.DateTo.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            return query.ToList().Select(s => new UploadOrderFeedViewModel(s)).ToList();
        }
    }
}