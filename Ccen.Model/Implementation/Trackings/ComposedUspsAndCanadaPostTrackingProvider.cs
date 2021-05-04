using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Orders;

namespace Amazon.Model.Implementation.Trackings
{
    public class ComposedUspsAndCanadaPostTrackingProvider : ITrackingProvider
    {
        public CarrierGroupType Carrier
        {
            get { return CarrierGroupType.USPS; }
        }

        private UspsTrackingProvider _uspsProvider;
        private CanadaPostTrackingProvider _canadaPostProvider;
        private ILogService _log;

        public ComposedUspsAndCanadaPostTrackingProvider(ILogService log,
            ITime time,
            string uspsUserId,
            string canadaPostKeys)
        {
            _log = log;
            _uspsProvider = new UspsTrackingProvider(log, time, uspsUserId);
            _canadaPostProvider = new CanadaPostTrackingProvider(log, time, canadaPostKeys);
        }


        public IList<TrackingState> TrackShipments(IList<TrackingNumberToCheckDto> trackingNumbers)
        {
            var uspsResults = _uspsProvider.TrackShipments(trackingNumbers);
            var canadaPostResults = _canadaPostProvider.TrackShipments(trackingNumbers.Where(t => ShippingUtils.IsCanada(t.ToCountry)).ToList());

            //TODO: compose by date
            foreach (var trackingNumber in trackingNumbers)
            {
                var uspsResult = uspsResults.FirstOrDefault(r => r.TrackingNumber == trackingNumber.TrackingNumber);
                var canadaPostResult = canadaPostResults.FirstOrDefault(r => r.TrackingNumber == trackingNumber.TrackingNumber);

                if (uspsResult != null 
                    && uspsResult.Records != null
                    && uspsResult.Records.Any()
                    && canadaPostResult != null
                    && canadaPostResult.Records != null
                    && canadaPostResult.Records.Any())
                {
                    _log.Info("Has both CA ans USPS results");
                    var lastUspsRecordDate = uspsResult.Records.Max(r => r.Date);
                    if (lastUspsRecordDate < canadaPostResult.Records.Max(r => r.Date))
                    {
                        _log.Info("Add CA tracking results");
                        uspsResult.Records.AddRange(canadaPostResult.Records.Where(r => r.Date > lastUspsRecordDate).ToList());
                        uspsResult.Records = uspsResult.Records.OrderByDescending(r => r.Date).ToList();
                    }
                }

            }

            return uspsResults;
        }
    }
}
