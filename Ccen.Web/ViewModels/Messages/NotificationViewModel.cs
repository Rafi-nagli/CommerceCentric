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
using Amazon.DTO.Messages;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels.Messages
{
    public class NotificationViewModel
    {
        public long Id { get; set; }

        public int Type { get; set; }
        
        public string RelatedEntityId { get; set; }
        public int RelatedEntityType { get; set; }

        public string Tag { get; set; }
        public string Message { get; set; }
        public string AdditionalParams { get; set; }


        private INotificationParams _paramObj;
        public INotificationParams ParamObj
        {
            get
            {
                if (_paramObj == null
                    && !String.IsNullOrEmpty(AdditionalParams))
                {
                    switch ((NotificationType) Type)
                    {
                        case NotificationType.LabelGotStuck:
                            _paramObj = NotificationHelper.FromStr<LabelGetStuckParams>(AdditionalParams);
                            break;
                        case NotificationType.LabelNeverShipped:
                            _paramObj = NotificationHelper.FromStr<LabelNeverShippedParams>(AdditionalParams);
                            break;
                        case NotificationType.AmazonProductImageChanged:
                            _paramObj = NotificationHelper.FromStr<ImageChangeParams>(AdditionalParams);
                            break;
                    }
                }
                return _paramObj;
            }
        }


        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public long? ReadBy { get; set; }

        public DateTime? EntityDate { get; set; }

        public DateTime? CreateDate { get; set; }

        //Additional
        public int? ShippingMethodId { get; set; }



        public string RelatedEntityUrl
        {
            get
            {
                if (Type == (int)NotificationType.AmazonProductImageChanged)
                {
                    if (ParamObj != null)
                    {
                        var param = (ImageChangeParams) ParamObj;
                        return UrlHelper.GetProductUrl(RelatedEntityId,
                            (MarketType)param.MarketType, param.MarketplaceId);
                    }
                }
                if (Type == (int) NotificationType.LabelGotStuck)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelGetStuckParams) ParamObj;
                        return MarketUrlHelper.GetTrackingUrl(RelatedEntityId, param.Carrier);
                    }
                }
                if (Type == (int)NotificationType.LabelNeverShipped)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelNeverShippedParams)ParamObj;
                        return MarketUrlHelper.GetTrackingUrl(RelatedEntityId, param.Carrier);
                    }
                }
                
                return "#";
            }
        }

        public string TypeAsString
        {
            get { return NotificationHelper.ToString((NotificationType) Type); }
        }

        public string RecordIdDetails
        {
            get
            {
                if (Type == (int) NotificationType.AmazonProductImageChanged)
                {
                    if (ParamObj != null)
                    {
                        var param = (ImageChangeParams) ParamObj;
                        var url = UrlHelper.GetSellarCentralInventoryUrl(RelatedEntityId,
                            (MarketType)param.MarketType, param.MarketplaceId);
                        var marketName = MarketHelper.GetMarketName(param.MarketType, param.MarketplaceId);;

                        return String.Format("on <a href='{0}' target='_blank' class='remark'>{1}</a>",
                            url, marketName);
                    }
                }
                if (Type == (int)NotificationType.LabelGotStuck)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelGetStuckParams)ParamObj;
                        return param.ReasonId == 0 ? "Batch" : MailViewModel.GetReasonName(param.ReasonId);
                    }
                }
                if (Type == (int)NotificationType.LabelNeverShipped)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelNeverShippedParams)ParamObj;
                        return param.ReasonId == 0 ? "Batch" : MailViewModel.GetReasonName(param.ReasonId);
                    }
                }
                return "";
            }
        }

        public string FormattedMessage
        {
            get
            {
                if (Type == (int)NotificationType.AmazonProductImageChanged)
                {
                    if (ParamObj != null)
                    {
                        var param = (ImageChangeParams)ParamObj;
                        return
                            String.Format("Image was changed (<span class='gray'>diff:</span> {0}%)</div><div><img src='{1}' height='50px' /> => <img src='{2}' height='50px' />",
                                param.ImageDiff.ToString("0.0"),
                                param.PreviousImage,
                                param.NewImage);
                    }
                }
                if (Type == (int)NotificationType.LabelGotStuck)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelGetStuckParams)ParamObj;
                        return String.Format("<span class='gray'>Pkg Status:</span> {0}, at: {1}, <span class='gray'>service:</span> {2}, <span class='gray'>order #:</span> <a href='{4}' target='_blank'>{3}</a>",
                            param.Status,
                            DateHelper.ToDateTimeString(param.StatusDate),
                            param.Carrier + " - " + param.ShippingName,
                            param.OrderNumber,
                            UrlHelper.GetOrderUrl(param.OrderNumber));
                    }
                }
                if (Type == (int)NotificationType.LabelNeverShipped)
                {
                    if (ParamObj != null)
                    {
                        var param = (LabelNeverShippedParams)ParamObj;
                        return String.Format("<span class='gray'>Label bought:</span> {0}, <span class='gray'>order #:</span> <a href='{2}' target='_blank'>{1}</a>",
                            param.BuyDate,
                            param.OrderNumber,
                            UrlHelper.GetOrderUrl(param.OrderNumber));
                    }
                }
                return "";
            }
        }

        public NotificationViewModel()
        {
            
        }

        public NotificationViewModel(NotificationDTO notification)
        {
            Id = notification.Id;
            Type = notification.Type;

            Message = notification.Message;
            Tag = notification.Tag;
            AdditionalParams = notification.AdditionalParams;
            
            RelatedEntityId = notification.RelatedEntityId;
            RelatedEntityType = notification.RelatedEntityType;
            
            CreateDate = notification.CreateDate;
        }

        public static void MarkAsRead(IUnitOfWork db, 
            ILogService log,
            MarkAsReadParams model,
            DateTime? when,
            long? by)
        {
            if (model == null
                || (model.IdList == null && !model.ForAll))
                return;

            var items = new List<Notification>();
            if (model.ForAll)
                items = db.Notifications.GetFiltered(n => !n.IsRead).ToList();
            else
                items = db.Notifications.GetFiltered(n => model.IdList.Contains(n.Id)).ToList();

            foreach (var item in items)
            {
                if (model.ReadStatus)
                {
                    if (!item.IsRead)
                    {
                        log.Info("Mark as read, id=" + item.Id);
                        item.IsRead = true;
                        item.ReadDate = when;
                        item.ReadBy = by;
                    }
                }
                else
                {
                    if (item.IsRead)
                    {
                        log.Info("Mark as unread, id=" + item.Id);
                        item.IsRead = false;
                        item.ReadDate = null;
                        item.ReadBy = null;
                    }
                }
            }

            db.Commit();
        }

        public static IQueryable<NotificationViewModel> GetAll(IUnitOfWork db,
            ITime time,
            NotificationFilterViewModel filter)
        {
            var fromDate = time.GetAppNowTime().AddYears(-1); 

            var query = from n in db.Notifications.GetAllAsDto()
                        join sh in db.OrderShippingInfos.GetAllAsDto() on n.RelatedEntityId equals sh.TrackingNumber into withSh
                        from sh in withSh.DefaultIfEmpty()
                        join o in db.Orders.GetAll() on sh.OrderId equals o.Id into withOr
                        from o in withOr.DefaultIfEmpty()
                        where !o.OrderDate.HasValue
                            || o.OrderDate > fromDate
                        select new NotificationViewModel()
                        {
                            Id = n.Id,
                            Type = n.Type,
                    
                            AdditionalParams = n.AdditionalParams,
                            Message = n.Message,
                            Tag = n.Tag,

                            RelatedEntityId = n.RelatedEntityId,
                            RelatedEntityType = n.RelatedEntityType,

                            IsRead = n.IsRead,
                            ReadDate = n.ReadDate,
                            ReadBy = n.ReadBy,

                            ShippingMethodId = sh.ShippingMethod.Id,

                            CreateDate = n.CreateDate,

                            EntityDate = o.OrderDate
                        };

            if (!String.IsNullOrEmpty(filter.OrderNumber))
                query = query.Where(n => n.RelatedEntityId == filter.OrderNumber
                    || n.Tag == filter.OrderNumber);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.CreateDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.CreateDate <= filter.DateTo.Value);

            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.OnlyPriority)
            {
                var excludeShippingMethods = new List<int>
                {
                    ShippingUtils.FirstClassShippingMethodId,
                    ShippingUtils.AmazonFirstClassShippingMethodId,
                    ShippingUtils.IBCCEPePocketMethodId,
                    ShippingUtils.IBCIPAMethodId,
                };
                query = query.Where(n => !n.ShippingMethodId.HasValue || !excludeShippingMethods.Contains(n.ShippingMethodId.Value));
            }

            if (!filter.InlcudeReaded 
                && String.IsNullOrEmpty(filter.OrderNumber))
                query = query.Where(n => !n.IsRead);
            
            return query;
        }
    }
}