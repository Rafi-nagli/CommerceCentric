using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon.Readers;
using Amazon.Model.Implementation.Trackings;
using Amazon.Model.Implementation.Trackings.Rules;
using Amazon.Model.SyncService.Threads.Simple;
using Amazon.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using Dhl.Api;
using eBay.Api;
using Fedex.Api;
using Stamps.Api;

namespace Amazon.InventoryUpdateManual.TestCases
{
    public class CallTrackingProcessing
    {
        private ILogService _log;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private CompanyDTO _company;
        private ITime _time;

        public CallTrackingProcessing(ILogService log,
            IDbFactory dbFactory,
            IEmailService emailService,
            CompanyDTO company,
            ITime time)
        {
            _log = log;
            _dbFactory = dbFactory;
            _emailService = emailService;
            _company = company;
            _time = time;
        }

        public void UpdateUSPSTrackFromFile(string filePath)
        {
            var uspsTrackingProvider = new UspsTrackingProvider(_log, _time, _company.USPSUserId);            
            var pageIndex = 0;
            var index = 0;
            var allLines = new List<string[]>(400000);
            var headers = new string[] { };
            var minLine = 400001;
            var maxLine = 600000;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                using (CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                }))
                {
                    var lines = new List<string[]>();
                    
                    while (reader.Read())
                    {
                        index++;

                        if (headers.Length == 0)
                            headers = reader.FieldHeaders;

                        if (index < minLine)
                            continue;

                        if (index > maxLine)
                            break;

                        lines.Add(reader.CurrentRecord);
                        if (lines.Count == 10)
                        {
                            //Process
                            var results = ProcessFileRecords(uspsTrackingProvider, lines.Select(l => (l[5]).Trim('\'')).ToList());
                            foreach (var result in results)
                            {
                                var line = lines.FirstOrDefault(l => l[5] == "'" + result.TrackingNumber + "'");
                                line[12] = DateHelper.ToDateTimeString(result.FirstTrackDate);
                                line[13] = result.FirstZipCode;
                                line[14] = result.SecondZipCode;
                            }
                            allLines.AddRange(lines);
                            lines = new List<string[]>();

                            _log.Info(allLines.Count.ToString());
                        }
                                                
                    }

                    if (lines.Count > 0)
                    {
                        //Process
                        var results = ProcessFileRecords(uspsTrackingProvider, lines.Select(l => (l[5]).Trim('\'')).ToList());
                        foreach (var result in results)
                        {
                            var line = lines.FirstOrDefault(l => l[5] == "'" + result.TrackingNumber + "'");
                            line[12] = DateHelper.ToDateTimeString(result.FirstTrackDate);
                            line[13] = result.FirstZipCode;
                            line[14] = result.SecondZipCode;
                        }
                        allLines.AddRange(lines);
                        lines = new List<string[]>();
                    }
                }
            }

            using (TextWriter writer = new StreamWriter(Path.Combine(filePath.Replace(".csv", "_" + minLine + "_" + maxLine + "_updated.csv"))))
            {
                using (var csvWriter = new CsvWriter(writer, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                }))
                {
                    csvWriter.Configuration.Encoding = Encoding.UTF8;
                                        
                    foreach (var field in headers)
                        csvWriter.WriteField(field);
                    csvWriter.NextRecord();

                    foreach (var row in allLines)
                    {
                        foreach (var field in row)
                        {
                            csvWriter.WriteField(field);
                        }
                        csvWriter.NextRecord();
                    }
                }
            }
        }

        private class TrackingResult
        {
            public string TrackingNumber { get; set; }
            public DateTime? FirstTrackDate { get; set; }
            public string FirstZipCode { get; set; }
            public string SecondZipCode { get; set; }
        }

        private IList<TrackingResult> ProcessFileRecords(ITrackingProvider trackingProvider, IList<string> trackings)
        {
            var trackingList = trackings.Select(tr => new TrackingNumberToCheckDto()
            {
                TrackingNumber = tr,
                ToCountry = "US"
            }).ToList();

            var trackResults = trackingProvider.TrackShipments(trackingList);
            if (trackResults.Count == 0)
            {
                _log.Info("No results. Wait.");
                Thread.Sleep(60000);
            }

            var results = new List<TrackingResult>();
            foreach (var track in trackResults)
            {
                var records = track.Records.OrderBy(r => r.Date).ToList();
                results.Add(new TrackingResult()
                {
                    TrackingNumber = track.TrackingNumber,
                    FirstTrackDate = records.FirstOrDefault()?.Date,
                    FirstZipCode = records.FirstOrDefault()?.Zip,
                    SecondZipCode = records.Skip(1).FirstOrDefault()?.Zip,
                });
            }

            return results;
        }

        public void GetFedexTracking(string trackingNumber)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var fedexInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.FedexOneRate);

                var fedexTrackingProvider =  new FedexTrackingApi(_log,
                    _time,
                    fedexInfo.EndPointUrl,
                    fedexInfo.UserName,
                    fedexInfo.Password,
                    fedexInfo.Key1,
                    fedexInfo.Key2,
                    fedexInfo.Key3,
                    _company.ShortName);


                var trackList = new List<TrackingNumberToCheckDto>()
                {
                    new TrackingNumberToCheckDto()
                    {
                        TrackingNumber = trackingNumber
                    }
                };

                var stateList = fedexTrackingProvider.TrackShipments(trackList);

                Console.WriteLine(stateList);
            }

        }


        public void GetUSPSTrackingByStamps(string trackingNumber)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var shipmentProviderInfo = db.ShipmentProviders.GetByCompanyId(_company.Id)
                    .FirstOrDefault(p => p.Type == (int) ShipmentProviderType.Stamps);

                var trackList = new List<string>() {trackingNumber};

                var stateList = RetryHelper.ActionWithRetries(
                    () => StampComService.TrackShipment(trackList, shipmentProviderInfo),
                    _log,
                    1,
                    1000,
                    RetryModeType.Normal,
                    true);

                Console.WriteLine(stateList);
            }
        }

        public void GetUSPSTrackingByUSPS(string trackingNumber)
        {
            var uspsTrackingProvider = new ComposedUspsAndCanadaPostTrackingProvider(_log, _time, _company.USPSUserId, _company.CanadaPostKeys);
            var results = uspsTrackingProvider.TrackShipments(new List<TrackingNumberToCheckDto>()
            {
                new TrackingNumberToCheckDto()
                {
                    TrackingNumber = trackingNumber
                }
            });

            _log.Info("Result lines: " + results.Count);
        }

        public void GetDHLTracking(string trackingNumber)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var dhlInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.Dhl);

                var dhlTrackingProvider = new DhlTrackingProvider(_log,
                    _time,
                    dhlInfo.EndPointUrl,
                    dhlInfo.UserName,
                    dhlInfo.Password,
                    dhlInfo.Key1);


                var trackList = new List<TrackingNumberToCheckDto>()
                {
                    new TrackingNumberToCheckDto()
                    {
                        TrackingNumber = trackingNumber
                    }
                };

                var stateList = dhlTrackingProvider.TrackShipments(trackList);

                Console.WriteLine(stateList);
            }
        }

        public void UpdateUSPSTrackOrders()
        {
            while (true)
            {
                using (var db = new UnitOfWork(_log))
                {
                    var service = new UpdateOrderTrackingStatus(_company.Id, null, TimeSpan.FromSeconds(10));
                    var actionService = new SystemActionService(_log, _time);
                    var dbFactory = new DbFactory();
                    var notificationService = new NotificationService(_log, _time, dbFactory);
                    var companyAddress = new CompanyAddressService(_company);
                    var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                    var ruleList = new List<ITrackingRule>()
                    {
                    };
                    
                    var trackingService = new TrackingManager(_log, actionService, addressService, _emailService, _time, ruleList);

                    var uspsTrackingProvider = new ComposedUspsAndCanadaPostTrackingProvider(_log, _time, _company.USPSUserId, _company.CanadaPostKeys);

                    service.UpdateAllShippedOrderStatus(trackingService, 
                        _time, 
                        db,
                        uspsTrackingProvider,
                        _company);
                }
                Thread.Sleep(TimeSpan.FromMinutes(0));
            }
        }

        public void UpdateFedexTrackOrders()
        {
            while (true)
            {
                using (var db = new UnitOfWork(_log))
                {
                    var service = new UpdateOrderTrackingStatus(_company.Id, null, TimeSpan.FromSeconds(10));
                    var actionService = new SystemActionService(_log, _time);
                    var dbFactory = new DbFactory();
                    var notificationService = new NotificationService(_log, _time, dbFactory);
                    var companyAddress = new CompanyAddressService(_company);
                    var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                    var ruleList = new List<ITrackingRule>()
                    {
                    };

                    var trackingService = new TrackingManager(_log, actionService, addressService, _emailService, _time, ruleList);
                    var fedexInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.FedexOneRate);

                    var fedexTrackingProvider = new FedexTrackingApi(_log,
                        _time,
                        fedexInfo.EndPointUrl,
                        fedexInfo.UserName,
                        fedexInfo.Password,
                        fedexInfo.Key1,
                        fedexInfo.Key2,
                        fedexInfo.Key3,
                        _company.ShortName);

                    service.UpdateAllShippedOrderStatus(trackingService,
                        _time,
                        db,
                        fedexTrackingProvider,
                        _company);
                }
                Thread.Sleep(TimeSpan.FromMinutes(0));
            }
        }

        public void UpdateDHLTrackOrders()
        {
            while (true)
            {
                using (var db = new UnitOfWork(_log))
                {
                    var service = new UpdateOrderTrackingStatus(_company.Id, null, TimeSpan.FromSeconds(10));
                    var actionService = new SystemActionService(_log, _time);
                    var dbFactory = new DbFactory();
                    var notificationService = new NotificationService(_log, _time, dbFactory);
                    var companyAddress = new CompanyAddressService(_company);
                    var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                    var ruleList = new List<ITrackingRule>()
                    {
                    };

                    var dhlInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.Dhl);

                    var trackingService = new TrackingManager(_log, actionService, addressService, _emailService, _time, ruleList);

                    var dhlTrackingProvider = new DhlTrackingProvider(_log,
                        _time,
                        dhlInfo.EndPointUrl,
                        dhlInfo.UserName,
                        dhlInfo.Password,
                        dhlInfo.Key1);

                    service.UpdateAllShippedOrderStatus(trackingService,
                        _time,
                        db,
                        dhlTrackingProvider,
                        _company);
                }
                Thread.Sleep(TimeSpan.FromMinutes(0));
            }
        }

        public void ReProcessTrackNotifications(IDbFactory dbFactory)
        {
            var from = _time.GetAppNowTime().AddDays(-42); //NOTE: Possible/not sure: After 42 days USPS not update/keep info
            var orderFrom = _time.GetAppNowTime().AddDays(-90);
            using (var db = dbFactory.GetRWDb())
            {
                var shippings = db.Orders.GetUnDeliveredShippingInfoes(_time.GetUtcTime(), false, null)
                    .Where(o => (!o.TrackingStateDate.HasValue || o.TrackingStateDate.Value > from)
                        && o.OrderDate > orderFrom)
                    .OrderBy(o => o.OrderDate)
                    .ToList();

                shippings.AddRange(db.Orders.GetUnDeliveredMailInfoes(_time.GetUtcTime(), false, null)
                    .Where(o => (!o.TrackingStateDate.HasValue || o.TrackingStateDate.Value > from)
                        && o.OrderDate > orderFrom)
                    .OrderBy(o => o.OrderDate)
                    .ToList());

                var actionService = new SystemActionService(_log, _time);
                var companyAddress = new CompanyAddressService(_company);
                var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                var notificationService = new NotificationService(_log, _time, dbFactory);

                var ruleList = new List<ITrackingRule>()
                {
                    //new NeverShippedTrackingRule(_log, notificationService, _time),
                    //new GetStuckTrackingRule(_log, notificationService, _time),
                    //new NoticeLeftTrackingRule(actionService, _log)
                };

                var trackingService = new TrackingManager(_log, 
                    actionService,
                    addressService,
                    _emailService,
                    _time,
                    ruleList);


                foreach (var shipping in shippings)
                {
                    trackingService.CheckRules(db,
                        shipping,
                        shipping.TrackingStateEvent,
                        shipping.TrackingStateDate,
                        new List<TrackingRecord>()
                        {
                            new TrackingRecord()
                            {
                                Date = shipping.TrackingStateDate,
                                Message = shipping.TrackingStateEvent,
                            }
                        }, 
                        ruleList);
                }
            }

        }

        public void ReProcessTrackInfoFull(IDbFactory dbFactory, string trackingNumber)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var shippings = db.Orders.GetUnDeliveredShippingInfoes(_time.GetUtcTime(), false, null)
                    .Where(o => o.TrackingNumber == trackingNumber)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                shippings.AddRange(db.Orders.GetUnDeliveredMailInfoes(_time.GetUtcTime(), false, null)
                    .Where(o => o.TrackingNumber == trackingNumber)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList());

                var actionService = new SystemActionService(_log, _time);
                var companyAddress = new CompanyAddressService(_company);
                var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                var notificationService = new NotificationService(_log, _time, dbFactory);

                var ruleList = new List<ITrackingRule>()
                {
                    new NeverShippedTrackingRule(_log, notificationService, _time),
                    new GetStuckTrackingRule(_log, notificationService, _time),
                    new NoticeLeftTrackingRule(_log, actionService, addressService, _time),
                    //new UndeliverableAsAddressedTrackingRule(_log, actionService, addressService, _emailService, _time)
                };

                var trackingService = new TrackingManager(_log,
                    actionService,
                    addressService,
                    _emailService,
                    _time,
                    ruleList);

                var trackingProvider = new ComposedUspsAndCanadaPostTrackingProvider(_log, _time, _company.USPSUserId, _company.CanadaPostKeys);

                var fedexInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.FedexGeneral);
                var fedexTrackingProvider = new FedexTrackingApi(_log,
                    _time,
                    fedexInfo.EndPointUrl,
                    fedexInfo.UserName,
                    fedexInfo.Password,
                    fedexInfo.Key1,
                    fedexInfo.Key2,
                    fedexInfo.Key3,
                    _company.ShortName);

                //var dhlInfo = _company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)ShipmentProviderType.Dhl);
                //var trackingProvider = new DhlTrackingProvider(_log,
                //    _time,
                //    dhlInfo.EndPointUrl,
                //    dhlInfo.UserName,
                //    dhlInfo.Password,
                //    dhlInfo.Key1);

                trackingService.UpdateOrderTracking(db, _company, shippings, fedexTrackingProvider);
            }
        }
    }
}
