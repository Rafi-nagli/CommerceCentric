using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Usps.Api;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Orders;
using CanadaPost.Api;

namespace Amazon.Model.Implementation.Trackings
{
    public class CanadaPostTrackingProvider : ITrackingProvider
    {
        private CanadaPostApi _api;
        private ILogService _log;
        private ITime _time;

        public CarrierGroupType Carrier
        {
            get { return CarrierGroupType.USPS; }
        }

        public CanadaPostTrackingProvider(ILogService log, 
            ITime time,
            string canadaPostKeys)
        {
            _log = log;
            _time = time;
            _api = new CanadaPostApi(log, canadaPostKeys);
        }

        public IList<TrackingState> TrackShipments(IList<TrackingNumberToCheckDto> trackingNumbers)
        {
            var results = new List<TrackingState>();
            var trackingNumberList = trackingNumbers.Select(t => t.TrackingNumber).ToList();

            foreach (var trackingNumber in trackingNumberList)
            {
                _log.Info("Begin get CA track info=" + trackingNumber);
                var perOneTrackingResult = _api.GetTrackingField(new List<string>() {trackingNumber});
                if (perOneTrackingResult.Status == CallStatus.Fail)
                {
                    _log.Info(String.Format("Error, tracking number={0}: {1}", trackingNumber,
                        perOneTrackingResult.Message));
                    results.Add(new TrackingState()
                    {
                        TrackingNumber = trackingNumber,
                        Records = new List<TrackingRecord>()
                        {
                            new TrackingRecord()
                            {
                                Message = TrackingHelper.BuildUndefinedMessage(perOneTrackingResult.Message),
                                Date = _time.GetUtcTime(),
                            }
                        }
                    });
                }
                else
                {
                    foreach (var result in perOneTrackingResult.Data)
                    {
                        _log.Info("CA tracking result=" + result.TrackingNumber + ", last status=" +
                                  (result.Records != null && result.Records.Any()
                                      ? result.Records[0].Message + " at " + result.Records[0].Date
                                      : ""));
                    }
                    results.AddRange(perOneTrackingResult.Data);
                }
            }

            foreach (var info in results)
            {
                if (info.Records != null)
                    info.Records.ForEach(r => r.Source = TrackingStatusSources.CanadaPost);
            }

            return results;
        }
    }
}
