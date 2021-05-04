using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class SignConfirmationByOrderCostChecker
    {
        private ILogService _log;
        private IUnitOfWork _db;
        private ITime _time;

        public SignConfirmationByOrderCostChecker(ILogService log, 
            ITime time,
            IUnitOfWork db)
        {
            _log = log;
            _db = db;
            _time = time;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Enable SignConfirmation by CheckNeedSignConfirmationByOrderCost");
                dbOrder.IsSignConfirmation = true;

                _db.OrderComments.Add(new OrderComment()
                {
                    OrderId = dbOrder.Id,
                    Message = "Signature confirmation required, order value > $200",
                    Type = (int)CommentType.System,
                    CreateDate = _time.GetAppNowTime(),
                });
                _db.Commit();
            }
        }

        public CheckResult Check(DTOMarketOrder order, IList<ListingOrderDTO> items)
        {
            //NOTE: Temp disabled currenlty no need
            //var address = order.GetAddressDto();
            ////if ((order.ShipmentProviderType == (int)ShipmentProviderType.Stamps
            ////    || order.ShipmentProviderType == (int)ShipmentProviderType.Amazon)
            //    //|| order.ShipmentProviderType == (int)ShipmentProviderType.None)
            //    //&& !String.IsNullOrEmpty(address.FinalCountry)
            //    //&& !ShippingUtils.IsInternational(address.FinalCountry)
            //    //&& ShippingUtils.IsServiceSameDay(order.InitialServiceType))            
            //{
            //    var sum = items.Sum(i => i.ItemPrice - (i.PromotionDiscount ?? 0));
            //    var currency = PriceHelper.GetCurrencyAbbr((MarketType)order.Market, order.MarketplaceId);
            //    var sumInUSD = PriceHelper.ConvertToUSD(sum, currency);
            //    if (sumInUSD > 200)
            //        return new CheckResult() { IsSuccess = true };
            //}
            return new CheckResult() { IsSuccess = false };
        }
    }
}
