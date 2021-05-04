using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Orders
{
    public class LabelViewModel
    {
        public long Id { get; set; }
        
        public string Path { get; set; }
        public int? PurchaseResult { get; set; }
        public string PurchaseMessage { get; set; }
        public DateTime? PurchaseDate { get; set; }

        public string Carrier { get; set; }
        public string CustomCarrier { get; set; }
        public string TrackingNumber { get; set; }

        public int? TrackingStatusSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public DateTime? LastTrackingStateUpdateDate { get; set; }

        public string ShippingCountry { get; set; }

        public DateTime? ShippingDate { get; set; }
        public DateTime? EstDeliveryDate { get; set; }
        public int? DeliveredStatus { get; set; }
        public string DeliveryStatusMessage { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public string ShippingProviderName { get; set; }
        public string ShippingMethodName { get; set; }

        public int? NumberInBatch { get; set; }

        public LabelFromType FromType { get; set; }

        public int MailReasonId { get; set; }

        public string MailReasonName
        {
            get { return MailViewModel.GetReasonName(MailReasonId); }
        }

        public bool IsInternational
        {
            get { return ShippingUtils.IsInternational(ShippingCountry); }
        }
        
        public bool IsPrinted
        {
            get { return !String.IsNullOrEmpty(LabelUrl); }
        }

        public bool IsCanceled { get; set; }

        public string LabelUrl
        {
            get
            {
                return !String.IsNullOrEmpty(Path)
                    ? HttpUtility.UrlEncode(Path)
                    : "";
            }
        }

        public string[] Files
        {
            get { return StringHelper.Split(Path, ";"); }
        }

        public string LabelViewUrl
        {
            get
            {
                return UrlHelper.GetLabelViewUrl(Path);
            }
        }

        public string TrackingUrl
        {
            get
            {
                var carrier = Carrier;
                if (!String.IsNullOrEmpty(CustomCarrier))
                    carrier = CustomCarrier;
                return MarketUrlHelper.GetTrackingUrl(TrackingNumber, carrier, TrackingStatusSource);
            }
        }

        public int TrackingNumberStatus
        {
            get
            {
                if (Carrier == ShippingServiceUtils.USPSCarrier
                    || Carrier == ShippingServiceUtils.IBCCarrier
                    || Carrier == ShippingServiceUtils.DHLCarrier
                    || Carrier == ShippingServiceUtils.DHLMXCarrier
                    || Carrier == ShippingServiceUtils.FedexCarrier)
                    return GetTrackingNumberStatus(
                        ActualDeliveryDate,
                        IsInternational,
                        TrackingStateDate,
                        EstDeliveryDate,
                        DeliveredStatus,
                        MailReasonId,
                        IsCanceled);
                return (int)TrackingNumberStatusEnum.Yellow;
            }
        }

        /// <summary>
        /// 1 - green, 0 - yellow, 2 - red
        /// </summary>
        /// <param name="actualDeliveryDate"></param>
        /// <param name="isInternational"></param>
        /// <param name="trackingStateDate"></param>
        /// <param name="estDeliveryDate"></param>
        /// <param name="deliveredStatus"></param>
        /// <param name="mailReasonId"></param>
        /// <returns></returns>
        public static int GetTrackingNumberStatus(
            DateTime? actualDeliveryDate,
            bool isInternational,
            DateTime? trackingStateDate,
            DateTime? estDeliveryDate,
            int? deliveredStatus,
            int mailReasonId,
            bool isCanceled)
        {
            /* kogda my pokazyvaem tracking number v sisteme, davay ego raskrashivat
               зеленый - delivered
               желтый - в пути
               красный если est problema...
               например estimated delivery date < today
               domestic tracking wasn't updated for a week, or international for 2 weeks*/

            if (isCanceled)
                return (int)TrackingNumberStatusEnum.Black;

            if (deliveredStatus == (int) DeliveredStatusEnum.InfoUnavailable)
            {
                return (int) TrackingNumberStatusEnum.Black;
            }

            //Delivered to sender
            if (deliveredStatus == (int)DeliveredStatusEnum.DeliveredToSender)
            {
                if (mailReasonId == (int)MailLabelReasonCodes.ReturnLabelReasonCode)
                    return (int)TrackingNumberStatusEnum.Green;
                return (int)TrackingNumberStatusEnum.Red;
            }

            if (actualDeliveryDate.HasValue)
                return (int)TrackingNumberStatusEnum.Green;

            if (isInternational)
            {
                if (trackingStateDate < DateTime.UtcNow.AddDays(-14))
                    return (int)TrackingNumberStatusEnum.Red;
            }
            else
            {
                if (trackingStateDate < DateTime.UtcNow.AddDays(-7))
                    return (int)TrackingNumberStatusEnum.Red;
            }
            if (estDeliveryDate.HasValue)
            {
                if (estDeliveryDate < DateTime.UtcNow)
                    return (int)TrackingNumberStatusEnum.Red;
            }

            return (int)TrackingNumberStatusEnum.Yellow;
        }


        public void UpdateTrackingStatus(TrackingManager trackingManager)
        {
            
        }
    }
}