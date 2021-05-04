using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Notifications.NotificationParams;
using Amazon.Core.Entities.Messages;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Feeds;
using Amazon.DTO.Listings;
using Amazon.DTO.Messages;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Messages;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Amazon.Web.ViewModels.Feeds
{
    public class FeedMessageViewModel
    {
        public long Id { get; set; }

        public long? FeedId { get; set; }

        public string Message { get; set; }
        public string MessageCode { get; set; }
        public long MessageId { get; set; }
        public string ResultCode { get; set; }
        
        public MessageStatus Status { get; set; }
        public DateTime? CreateDate { get; set; }
        
        public FeedMessageViewModel()
        {
            
        }

        public FeedMessageViewModel(FeedMessageDTO feedMessage)
        {
            Id = feedMessage.Id;
            Message = feedMessage.Message;
            MessageCode = feedMessage.MessageCode;
            ResultCode = feedMessage.ResultCode;
            MessageId = feedMessage.MessageId;
        }
        
        public static IQueryable<FeedMessageViewModel> GetAll(IUnitOfWork db,
            long feedId)
        {
            var query = from m in db.FeedMessages.GetAllAsDto()
                        where m.FeedId == feedId
                        select new FeedMessageViewModel()
                        {
                            Id = m.Id,
                            Message = m.Message,
                            MessageCode = m.MessageCode,
                            MessageId = m.MessageId,
                            ResultCode = m.ResultCode,
                        };
            
            return query;
        }
    }
}