using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation
{
    public class ShippingService : IShippingService
    {
        private IDbFactory _dbFactory;
        private bool _isFulfillmentUser;
        private IList<ShippingChargeDTO> _shippingCharges;

        public ShippingService(IDbFactory dbFactory,
            bool isFulfillmentUser)
        {
            _dbFactory = dbFactory;
            _isFulfillmentUser = isFulfillmentUser;
        }

        public void ApplyCharges(IList<OrderShippingInfoDTO> shippings)
        {
            if (_isFulfillmentUser)
                return; //NOTE: Skip, only for Client Role

            if (_shippingCharges == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    _shippingCharges = db.ShippingCharges.GetAllAsDto().ToList();
                }
            }

            foreach (var shipping in shippings)
            {
                var shippingCharge = _shippingCharges.FirstOrDefault(ch => ch.ShippingMethodId == shipping.ShippingMethodId);
                if (shippingCharge != null)
                {
                    shipping.StampsShippingCost = PriceHelper.RoundToTwoPrecision(shipping.StampsShippingCost * (1 + shippingCharge.ChargePercent / 100M));
                }
            }
        }

        public decimal? ApplyCharges(int shippingMethodId, decimal? rateAmount)
        {
            if (_isFulfillmentUser) //NOTE: Skip, only for Client Role
                return rateAmount; 

            if (!rateAmount.HasValue)
                return rateAmount;

            if (_shippingCharges == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    _shippingCharges = db.ShippingCharges.GetAllAsDto().ToList();
                }
            }

            var shippingCharge = _shippingCharges.FirstOrDefault(ch => ch.ShippingMethodId == shippingMethodId);
            if (shippingCharge != null)
            {
                return PriceHelper.RoundToTwoPrecision(rateAmount * (1 + shippingCharge.ChargePercent / 100M));
            }

            return rateAmount;
        }

        public decimal GetUpCharge(int shippingMethodId, decimal? rateAmount)
        {
            if (!rateAmount.HasValue)
                return 0;

            if (_shippingCharges == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    _shippingCharges = db.ShippingCharges.GetAllAsDto().ToList();
                }
            }

            var shippingCharge = _shippingCharges.FirstOrDefault(ch => ch.ShippingMethodId == shippingMethodId);
            if (shippingCharge != null)
            {
                return PriceHelper.RoundToTwoPrecision(rateAmount.Value * shippingCharge.ChargePercent / 100M);
            }

            return 0;
        }
    }
}
