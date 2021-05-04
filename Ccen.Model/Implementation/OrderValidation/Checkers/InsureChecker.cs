using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Validation
{
    public class InsureChecker
    {
        private ILogService _log;
        private IOrderHistoryService _orderHistory;

        public InsureChecker(ILogService log, IOrderHistoryService orderHistory)
        {
            _log = log;
            _orderHistory = orderHistory;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Info("Insure enabled");

                _orderHistory.AddRecord(dbOrder.Id, OrderHistoryHelper.IsInsuredKey, dbOrder.IsInsured, true, null);
                
                dbOrder.IsInsured = true;
                dbOrder.InsuredValue = dbOrder.TotalPrice;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order)
        {
            //NOTE: not standard it is DHL (should be w/o insurance)
            if (order.InitialServiceType != ShippingUtils.StandardServiceName) 
            {
                return new CheckResult()
                {
                    IsSuccess = false
                };
            }

            var countryName = order.ShippingCountry;
            var country = db.Countries.GetFiltered(c => c.CountryCode2 == countryName)
                .FirstOrDefault();
            var isInusred = country != null && country.IsInsure;
            if (isInusred)
                _log.Info("Insured by Country Code");
            return new CheckResult()
            {
                IsSuccess = isInusred
            };
        }
    }
}
