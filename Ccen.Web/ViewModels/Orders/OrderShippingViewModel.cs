using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models.Calls;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderShippingViewModel
    {
        public long ShippingInfoId { get; set; }
        public string Carrier { get; set; }
        public string ShippingMethodName { get; set; }
        public string TrackingNumber { get; set; }
        public bool IsActive { get; set; }
        public int GroupId { get; set; }
        public bool RequirePackageSize { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public OrderShippingViewModel()
        {
            
        }

        public OrderShippingViewModel(OrderShippingInfoDTO shipping)
        {
            ShippingInfoId = shipping.Id;
            TrackingNumber = shipping.TrackingNumber;
            IsActive = shipping.IsActive;
            ShippingMethodName = shipping.ShippingMethod.ShortName ?? shipping.ShippingMethod.Name;
            RequirePackageSize = shipping.ShippingMethod.RequiredPackageSize;

            if (String.IsNullOrEmpty(shipping.CustomCarrier))
                Carrier = shipping.ShippingMethod.CarrierName;
            else
                Carrier = shipping.CustomCarrier;
            ShippingMethodName = shipping.ShippingMethod.Name;
            GroupId = shipping.ShippingGroupId;

            PackageLength = shipping.PackageLength;
            PackageWidth = shipping.PackageWidth;
            PackageHeight = shipping.PackageHeight;
        }
        
        public IList<MessageString> ValidateTrackingNumber()
        {
            TrackingNumber = StringHelper.TrimWhitespace(TrackingNumber);

            if (String.IsNullOrEmpty(TrackingNumber))
                return new List<MessageString>() { MessageString.Error("Tracking number is empty") };

            if (Carrier == ShippingServiceUtils.USPSCarrier)
            {
                if (TrackingNumber.Length != 22)
                    return new List<MessageString>() { MessageString.Error("Invalid tracking number format") };

                var checkDigitResult = TrackingHelper.GetCheckDigit(TrackingNumber);
                if (checkDigitResult.IsFail)
                    return new List<MessageString>() { MessageString.Error(checkDigitResult.Message) };

                if (checkDigitResult.Data.ToString()[0] != TrackingNumber.Last())
                    return new List<MessageString>() { MessageString.Error("Invalid check sum. Expected: " + checkDigitResult.Data) };                
            }

            return new List<MessageString>();
        }

        public void UpdateTrackingNumber(IUnitOfWork db,
            DateTime when,
            long? by)
        {
            var shipping = db.OrderShippingInfos.Get(ShippingInfoId);
            shipping.TrackingNumber = TrackingNumber;
            if (!String.IsNullOrEmpty(Carrier))
                shipping.CustomCarrier = Carrier;
            shipping.LabelPath = "#";
            shipping.LabelPurchaseDate = when;
            db.Commit();
        }
    }
}