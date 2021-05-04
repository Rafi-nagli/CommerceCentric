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

namespace Amazon.Model.Implementation.Trackings
{
    public class UspsTrackingProvider : ITrackingProvider
    {
        private UspsApi _api;
        private ILogService _log;
        private ITime _time;

        public CarrierGroupType Carrier
        {
            get { return CarrierGroupType.USPS; }
        }

        public UspsTrackingProvider(ILogService log, 
            ITime time,
            string uspsUserId)
        {
            _log = log;
            _time = time;
            _api = new UspsApi(log, uspsUserId);
        }

        public IList<TrackingState> TrackShipments(IList<TrackingNumberToCheckDto> trackingNumbers)
        {
            var index = 0; 
            var stepSize = 10;
            var results = new List<TrackingState>();
            while (index < trackingNumbers.Count)
            {
                var requestTrackingList = trackingNumbers.Select(t => t.TrackingNumber).Skip(index).Take(stepSize).ToList();
                var trackingResult = _api.GetTrackingField(requestTrackingList); //If one tracking from batch request fail whole request fail
                if (trackingResult.Status == CallStatus.Success)
                {
                    results.AddRange(trackingResult.Data);
                }
                else
                {
                    //After batch request fail, make per item request
                    foreach (var trackingNumber in requestTrackingList)
                    {
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
                            results.AddRange(perOneTrackingResult.Data);
                        }
                    }

                    _log.Info("Error: " + trackingResult.Message);
                }

                index += stepSize;
            }

            foreach (var info in results)
            {
                if (info.Records != null)
                    info.Records.ForEach(r => r.Source = TrackingStatusSources.USPS);
            }

            return results;
        }
    }
}
