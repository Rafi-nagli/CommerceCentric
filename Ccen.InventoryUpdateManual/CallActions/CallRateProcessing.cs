using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Rates;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Trackings;
using Amazon.Model.StampsCom;
using Ccen.Core.Entities.TrackingNumbers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Stamps.Api;

namespace Amazon.InventoryUpdateManual.TestCases
{
    public class CallRateProcessing
    {
        private CompanyDTO _company;
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IWeightService _weightService;
        private RateService _rateService;

        public CallRateProcessing(
            CompanyDTO company,
            ILogService log,
            IDbFactory dbFactory,
            ITime time)
        {
            _company = company;
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _weightService = new WeightService();

            var messageService = new SystemMessageService(log, time, dbFactory);
            var serviceFactory = new ServiceFactory();
            var actionService = new SystemActionService(log, time);
            var rateProviders = serviceFactory.GetShipmentProviders(log,
                time,
                dbFactory,
                _weightService,
                company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            _rateService = new RateService(dbFactory,
                log,
                time,
                _weightService,
                messageService,
                company,
                actionService,
                rateProviders);
        }

        public void FillWithCCENShippingCost(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);
                    
                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null)
                            continue;
                        var orderString = row.GetCell(32)?.ToString();

                        if (String.IsNullOrEmpty(orderString))
                            continue;

                        var parts = orderString.Split('-');
                        long? shippingId = null;
                        if (parts.Length > 2)
                        {
                            orderString = String.Join("-", parts.Skip(1).Take(parts.Length - 2));
                            shippingId = StringHelper.TryGetLong(parts.Last());
                        }

                        decimal? cost = null;
                        if (shippingId.HasValue)
                        {
                            cost = db.OrderShippingInfos.Get(shippingId.Value)?.StampsShippingCost;
                            if (cost == 1)
                                cost = null;
                        }

                        if (cost != null)
                        {
                            var cell = row.GetCell(31);
                            if (cell == null)
                                cell = row.CreateCell(31);
                            cell.SetCellValue(cost.ToString());
                        }
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void RefreshSuspiciousFedexRates()
        {
            _rateService.RefreshSuspiciousFedexRates();
        }

        public void ImportCAZoneMappingRates()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Dhl/2017_CA_GB_Zone_Mappings.xlsx");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                using (var db = _dbFactory.GetRWDb())
                {
                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var facility = row.GetCell(0)?.ToString();
                        if (String.IsNullOrEmpty(facility))
                            continue;

                        if (facility != "ORD")
                            continue;

                        var zip = row.GetCell(1).ToString();
                        var zone = row.GetCell(3).ToString();
                        var intZone = Int32.Parse(zone.Replace("CA-", ""));

                        db.DhlCAZipCodeZones.Add(new DhlCAZipCodeZone()
                        {
                            Facility = facility,
                            FSA = zip,
                            Zone = intZone
                        });
                    }
                    db.Commit();
                }
            }
        }

        public void ImportCustomTrackings(string from, string to)
        {
            var trackingService = new TrackingNumberService(_log, _time, _dbFactory);
            using (var db = _dbFactory.GetRWDb())
            {
                var fromLong = UInt64.Parse(from.Substring(15));
                var toLong = UInt64.Parse(to.Substring(15));
                for (var i = fromLong; i <= toLong; i++)
                {
                    var tracking = from.Substring(0, 15) + i.ToString();
                    db.CustomTrackingNumbers.Add(new CustomTrackingNumber()
                    {
                        TrackingNumber = trackingService.AppendWithCheckDigit(tracking),
                        CreateDate = _time.GetAppNowTime(),
                    });
                    db.Commit();
                }
            }
        }


        public void ImportGBZoneMappingRates()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Dhl/2017_CA_GB_Zone_Mappings.xlsx");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(2);

                using (var db = _dbFactory.GetRWDb())
                {
                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var facility = row.GetCell(1)?.ToString();
                        if (String.IsNullOrEmpty(facility))
                            continue;

                        if (facility != "USORD1")
                            continue;

                        var zip = row.GetCell(2).ToString();
                        var zone = row.GetCell(4).ToString();
                        //var lookup = row.GetCell(3).ToString();
                        var intZone = Int32.Parse(zone.Replace("GB-", ""));

                        db.DhlGBZipCodeZones.Add(new DhlGBZipCodeZone()
                        {
                            Facility = facility,
                            Zip = zip,
                            //Lookup = lookup,
                            Zone = intZone
                        });
                    }
                    db.Commit();
                }
            }
        }

        public void ImportGlobalMailPlusRates()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Dhl/DHL GlobalMail Packet Plus.xlsx");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                using (var db = _dbFactory.GetRWDb())
                {
                    var countryCodes = new List<string>();
                    var row = sheet.GetRow(7);
                    var index = 1;
                    while (row.GetCell(index) != null)
                    {
                        countryCodes.Add(row.GetCell(index).ToString());
                        index++;
                    }
                    
                    for (var i = 8; i < 78; i++)
                    {
                        row = sheet.GetRow(i);
                        var weight = decimal.Parse(row.GetCell(0).ToString()) * 16;
                        for (var j = 1; j <= countryCodes.Count; j++)
                        {
                            var rate = decimal.Parse(row.GetCell(j).ToString().Replace("$", "").Replace(" ", ""));
                            db.DhlECommerceRates.Add(new DhlECommerceRate()
                            {
                                ServiceType = 29,
                                Rate = rate,
                                CountryCode = countryCodes[j - 1],
                                Zone = null,
                                Weight = weight,
                            });
                        }
                    }

                    db.Commit();
                }
            }
        }

        public void ImportIntlDirectRates()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Dhl/DHL Parcel International Direct.xlsx");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                    var countryCodes = new List<string>();
                    var row = sheet.GetRow(7);
                    var index = 1;
                    while (row.GetCell(index) != null)
                    {
                        countryCodes.Add(row.GetCell(index).ToString());
                        index++;
                    }

                for (var i = 8; i < 141; i++)
                {
                    using (var db = _dbFactory.GetRWDb())
                    {
                        _log.Info("Processing line " + i);
                        row = sheet.GetRow(i);
                        var weight = decimal.Parse(row.GetCell(0).ToString())*16;
                        for (var j = 1; j <= countryCodes.Count; j++)
                        {
                            if (row.GetCell(j).ToString() == "0")
                                continue;

                            int? zone = null;
                            var countryCode = countryCodes[j - 1];
                            if (countryCode.StartsWith("CA"))
                            {
                                zone = Int32.Parse(countryCode.Substring(2));
                                countryCode = "CA";
                            }

                            if (countryCode.StartsWith("GB"))
                            {
                                zone = Int32.Parse(countryCode.Substring(2));
                                countryCode = "GB";
                            }

                            if (countryCode.StartsWith("AU"))
                            {
                                zone = null;
                                countryCode = "AU";
                            }

                            if (countryCode.StartsWith("CN"))
                            {
                                zone = null;
                                countryCode = "CN";
                            }

                            var rate = decimal.Parse(row.GetCell(j).ToString().Replace("$", "").Replace(" ", ""));
                            db.DhlECommerceRates.Add(new DhlECommerceRate()
                            {
                                ServiceType = 60,
                                Rate = rate,
                                CountryCode = countryCode,
                                Zone = zone,
                                Weight = weight,
                            });
                        }

                        db.Commit();
                    }
                }
            }
        }

        public void ImportGroundPlusRates()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Dhl/GroundPlusRates.xlsx");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                using (var db = _dbFactory.GetRWDb())
                {
                    for (var i = 7; i < 32; i++)
                    {
                        var row = sheet.GetRow(i);
                        var weight = decimal.Parse(row.GetCell(0).ToString());
                        for (var j = 1; j <= 8; j++)
                        {
                            var rate = decimal.Parse(row.GetCell(j).ToString().Replace("$", "").Replace(" ", ""));
                            if (j == 1)
                            {
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = 1,
                                    Weight = weight,
                                });
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = 2,
                                    Weight = weight,
                                });
                            }
                            if (j > 1 && j < 8)
                            {
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = j + 1,
                                    Weight = weight,
                                });
                            }
                            if (j == 8)
                            {
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = 11,
                                    Weight = weight,
                                });
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = 12,
                                    Weight = weight,
                                });
                                db.DhlECommerceRates.Add(new DhlECommerceRate()
                                {
                                    ServiceType = 3,
                                    Rate = rate,
                                    Zone = 13,
                                    Weight = weight,
                                });
                            }
                        }
                    }

                    db.Commit();
                }
            }
        }

        public void FillStampsRateTable()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var newRateTable = new List<RateByCountryDTO>();
                var addressList = new List<AddressDTO>()
                {
                    RateHelper.GetSampleUSAddress(),
                };

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);

                var companyAddress = new CompanyAddressService(_company);

                var shippingSizes = new string[] { "S", "XL" };
                var internationalPackages = new string[] { "", "" };

                for (var oz = 1; oz < 50; oz++)
                {
                    //International Package Type: Regular, Flat
                    //Shipping Size: S, XL

                    foreach (var address in addressList)
                    {
                        foreach (var shippingSize in shippingSizes)
                        {
                            var packageType = shippingSize == "XL" ?
                                PackageTypeCode.Regular :
                                (ShippingUtils.IsInternational(address.Country) ? PackageTypeCode.LargeEnvelopeOrFlat : PackageTypeCode.Flat);

                            var shippintType = ShippingUtils.IsInternational(address.Country)
                                ? ShippingTypeCode.IStandard
                                : ShippingTypeCode.Standard;

                            var rate = RateHelper.GetRougeChipestRate(_log,
                                stampsRateProvider,
                                companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                                address,
                                oz,
                                DateTime.Today,
                                shippintType,
                                packageType);

                            if (rate != null && rate.Amount.HasValue)
                            {
                                _log.Info("Add rate: " + address.Country + ", " + oz + "oz, " + shippingSize + ", " + rate.Amount.Value + ", package=" + ((PackageTypeCode)rate.PackageTypeUniversal).ToString() + ", shippingType=" + ((ShippingTypeCode)rate.ServiceTypeUniversal).ToString());
                                newRateTable.Add(new RateByCountryDTO()
                                {
                                    Cost = PriceHelper.RoundToTwoPrecision(rate.Amount.Value),
                                    Country = address.Country,
                                    ShipmentProvider = stampsRateProvider.Type.ToString(),
                                    Weight = oz,
                                    PackageType = packageType.ToString(), //NOTE: need to use source package type, no matter what actual package is ((PackageTypeCode) rate.PackageTypeUniversal).ToString(),
                                    UpdateDate = _time.GetAppNowTime()
                                });
                            }
                            else
                            {
                                _log.Info("No rates: " + oz + "oz, " + shippingSize);
                            }
                        }
                    }
                }

                var existRates = db.RateByCountries.GetAll();
                foreach (var rate in newRateTable)
                {
                    var exist = existRates.FirstOrDefault(r => r.Country == rate.Country
                                                               && r.PackageType == rate.PackageType
                                                               && r.ShipmentProvider == rate.ShipmentProvider
                                                               && r.Weight == rate.Weight);
                    if (exist == null)
                    {
                        _log.Info("New rate: " + rate.ShipmentProvider + ", " + rate.PackageType + ", " + rate.Country + ", weight: " + rate.Weight + ", cost: " + rate.Cost);
                        db.RateByCountries.Add(new RateByCountry()
                        {
                            Country = rate.Country,
                            PackageType = rate.PackageType,
                            ShipmentProvider = rate.ShipmentProvider,
                            Cost = rate.Cost,
                            Weight = rate.Weight,
                            UpdateDate = rate.UpdateDate,
                        });
                    }
                    else
                    {
                        if (exist.Cost != PriceHelper.RoundToTwoPrecision(rate.Cost))
                        {
                            _log.Info("Update rate: " + rate.ShipmentProvider + ", " + rate.PackageType + ", " + rate.Country + ", weight: " + rate.Weight + ", cost: " + exist.Cost + " => " + rate.Cost);
                            exist.Cost = PriceHelper.RoundToTwoPrecision(rate.Cost);
                        }
                    }
                }
                db.Commit();
            }
        }

        public void FillRateTable()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var newRateTable = new List<RateByCountryDTO>();
                var addressList = new List<AddressDTO>()
                {
                    RateHelper.GetSampleUSAddress(),
                    RateHelper.GetSampleCAAddress(),
                    RateHelper.GetSampleUKAddress(),
                    RateHelper.GetSampleAUAddress()
                };

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.IBC);

                var companyAddress = new CompanyAddressService(_company);

                var shippingSizes = new string[] {"S", "XL"};
                var internationalPackages = new string[] {"", ""};

                for (var oz = 1; oz < 50; oz++)
                {
                    //International Package Type: Regular, Flat
                    //Shipping Size: S, XL
                    
                    foreach (var address in addressList)
                    {
                        foreach (var shippingSize in shippingSizes)
                        {
                            var packageType = shippingSize == "XL" ? 
                                PackageTypeCode.Regular :
                                (ShippingUtils.IsInternational(address.Country) ? PackageTypeCode.LargeEnvelopeOrFlat : PackageTypeCode.Flat);

                            var shippintType = ShippingUtils.IsInternational(address.Country)
                                ? ShippingTypeCode.IStandard
                                : ShippingTypeCode.Standard;

                            var rate = RateHelper.GetRougeChipestRate(_log,
                                stampsRateProvider,
                                companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                                address,
                                oz,
                                DateTime.Today,
                                shippintType,
                                packageType);

                            if (rate != null && rate.Amount.HasValue)
                            {
                                _log.Info("Add rate: " + address.Country + ", " + oz + "oz, " + shippingSize + ", " + rate.Amount.Value + ", package=" + ((PackageTypeCode)rate.PackageTypeUniversal).ToString() + ", shippingType=" + ((ShippingTypeCode) rate.ServiceTypeUniversal).ToString());
                                newRateTable.Add(new RateByCountryDTO()
                                {
                                    Cost = PriceHelper.RoundToTwoPrecision(rate.Amount.Value),
                                    Country = address.Country,
                                    ShipmentProvider = stampsRateProvider.Type.ToString(),
                                    Weight = oz,
                                    PackageType = packageType.ToString(), //NOTE: need to use source package type, no matter what actual package is ((PackageTypeCode) rate.PackageTypeUniversal).ToString(),
                                    UpdateDate = _time.GetAppNowTime()
                                });
                            }
                            else
                            {
                                _log.Info("No rates: " + oz + "oz, " + shippingSize);
                            }
                        }
                    }
                }

                var existRates = db.RateByCountries.GetAll();
                foreach (var rate in newRateTable)
                {
                    var exist = existRates.FirstOrDefault(r => r.Country == rate.Country
                                                               && r.PackageType == rate.PackageType
                                                               && r.ShipmentProvider == rate.ShipmentProvider
                                                               && r.Weight == rate.Weight);
                    if (exist == null)
                    {
                        _log.Info("New rate: " + rate.ShipmentProvider + ", " + rate.PackageType + ", " + rate.Country + ", weight: " + rate.Weight + ", cost: " + rate.Cost);
                        db.RateByCountries.Add(new RateByCountry()
                        {
                            Country = rate.Country,
                            PackageType = rate.PackageType,
                            ShipmentProvider = rate.ShipmentProvider,
                            Cost = rate.Cost,
                            Weight = rate.Weight,
                            UpdateDate = rate.UpdateDate,
                        });
                    }
                    else
                    {
                        if (exist.Cost != PriceHelper.RoundToTwoPrecision(rate.Cost))
                        {
                            _log.Info("Update rate: " + rate.ShipmentProvider + ", " + rate.PackageType + ", " + rate.Country + ", weight: " + rate.Weight + ", cost: " + exist.Cost + " => " + rate.Cost);
                            exist.Cost = PriceHelper.RoundToTwoPrecision(rate.Cost);
                        }
                    }
                }
                db.Commit();
            }
        }

        public void FillIBCRateTable()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var newRateTable = new List<RateByCountryDTO>();
                var addressList = new List<AddressDTO>()
                {
                    //RateHelper.GetSampleUSAddress(),
                    RateHelper.GetSampleCAAddress(),
                    RateHelper.GetSampleUKAddress()
                };

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.IBC);
                var companyAddress = new CompanyAddressService(_company);

                var shippingSizes = new string[] { //"S",
                    "XL" };
                var internationalPackages = new string[] { "", "" };

                for (var oz = 1; oz < 50; oz++)
                {
                    //International Package Type: Regular, Flat
                    //Shipping Size: S, XL

                    foreach (var address in addressList)
                    {
                        foreach (var shippingSize in shippingSizes)
                        {
                            var packageType = shippingSize == "XL" ?
                                PackageTypeCode.Regular :
                                (ShippingUtils.IsInternational(address.Country) ? PackageTypeCode.LargeEnvelopeOrFlat : PackageTypeCode.Flat);

                            var shippintType = ShippingUtils.IsInternational(address.Country)
                                ? ShippingTypeCode.IStandard
                                : ShippingTypeCode.Standard;

                            var rate = RateHelper.GetRougeChipestRate(_log,
                                stampsRateProvider,
                                companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                                address,
                                oz,
                                DateTime.Today,
                                shippintType,
                                packageType);

                            if (rate != null && rate.Amount.HasValue)
                            {
                                _log.Info("Add rate: " + address.Country + ", " + oz + "oz, " + shippingSize + ", " + rate.Amount.Value + ", package=" + ((PackageTypeCode)rate.PackageTypeUniversal).ToString() + ", shippingType=" + ((ShippingTypeCode)rate.ServiceTypeUniversal).ToString());
                                newRateTable.Add(new RateByCountryDTO()
                                {
                                    Cost = rate.Amount.Value,
                                    Country = address.Country,
                                    Weight = oz,
                                    PackageType = packageType.ToString(), //NOTE: need to use source package type, no matter what actual package is ((PackageTypeCode) rate.PackageTypeUniversal).ToString(),
                                    ShipmentProvider = stampsRateProvider.Type.ToString(),
                                    UpdateDate = _time.GetAppNowTime()
                                });
                            }
                            else
                            {
                                _log.Info("No rates: " + oz + "oz, " + shippingSize);
                            }
                        }
                    }
                }

                var existRates = db.RateByCountries.GetAll();
                foreach (var rate in newRateTable)
                {
                    var exist = existRates.FirstOrDefault(r => r.Country == rate.Country
                                                               && r.PackageType == rate.PackageType
                                                               && r.Weight == rate.Weight);
                    if (exist == null)
                    {
                        db.RateByCountries.Add(new RateByCountry()
                        {
                            Country = rate.Country,
                            PackageType = rate.PackageType,
                            Cost = rate.Cost,
                            Weight = rate.Weight,
                            ShipmentProvider = rate.ShipmentProvider,
                            UpdateDate = rate.UpdateDate,
                        });
                    }
                    else
                    {
                        exist.Cost = rate.Cost;
                        exist.ShipmentProvider = rate.ShipmentProvider;
                    }
                }
                db.Commit();
            }
        }

        public void GetIntlRatesTest(string orderId, ShipmentProviderType type)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(_weightService, orderId, unmaskReferenceStyle: false, includeSourceItems: true);

                var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(order.InitialServiceType, order.UpgradeLevel); //order.ShippingService
                var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.Items);
                var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.SourceItems);

                var providers = GetShipmentProviders(_company);
                var provider = providers.FirstOrDefault(p => p.Type == type);

                var companyAddress = new CompanyAddressService(_company);

                if (ShippingUtils.IsInternational(order.ShippingCountry))
                {
                    var rates = provider.GetInternationalRates(
                        companyAddress.GetReturnAddress(order.GetMarketId()),
                        companyAddress.GetPickupAddress(order.GetMarketId()),
                        order.GetAddressDto(),
                        _time.GetAppNowTime(),
                        order.WeightD,
                        null,
                        order.IsInsured ? order.TotalPrice : 0,
                        order.IsSignConfirmation,
                        new OrderRateInfo()
                        {
                            ShippingService = shippingService,
                            OrderNumber = order.OrderId,
                            Items = orderItemInfoes,
                            SourceItems = sourceOrderItemInfoes,
                            TotalPrice = order.TotalPrice,
                            Currency = order.TotalPriceCurrency,
                        },
                        RetryModeType.Normal);

                    Console.WriteLine(rates.Rates.Count);
                }
                else
                {
                    var rates = provider.GetLocalRate(
                        companyAddress.GetReturnAddress(order.GetMarketId()),
                        companyAddress.GetPickupAddress(order.GetMarketId()),
                        order.GetAddressDto(),
                        _time.GetAppNowTime(),
                        order.WeightD,
                        null,
                        order.IsInsured ? order.TotalPrice : 0,
                        order.IsSignConfirmation,
                        new OrderRateInfo()
                        {
                            ShippingService = shippingService,
                            InitialServiceType = order.InitialServiceType,
                            OrderNumber = order.OrderId,
                            Items = orderItemInfoes,
                            SourceItems = sourceOrderItemInfoes,
                            TotalPrice = order.TotalPrice,
                            Currency = order.TotalPriceCurrency,
                        },
                        RetryModeType.Normal);

                    Console.WriteLine(rates.Rates.Count);
                }
            }
        }

        //public void GetRatesTest(string orderId)
        //{
        //    using (var db = new UnitOfWork())
        //    {//18028 18037
        //        var order = db.Orders.GetFiltered(o => o.AmazonIdentifier == orderId).FirstOrDefault();
        //        var orderInfo = db.ItemOrderMappings.GetSelectedOrdersWithItems(new[] { order.Id }).First();//db.OrderShippingInfos.GetOrderInfoWithItems(16263).First();
        //        var addressTo = db.Orders.GetAddressInfo(orderInfo.OrderId);
        //        Func<DateTime, DateTime, int> getBizDaysCount = (start, end) => db.Dates.GetBizDaysCount(start, end);
        //        var orderItems = new List<OrderItemRateInfo>();

        //        if (ShippingUtils.IsInternational(addressTo.Country))
        //        {
        //            var rate = StampComService.GetInternationalRates(
        //                _company.ShipmentProviderInfoList.ToList<IStampsAuthInfo>(),
        //                _log,
        //                 DateTime.Now,
        //                _company.Zip,
        //                addressTo,
        //                orderInfo.ShippingService,
        //                PackageTypeCode.None,
        //                orderInfo.WeightD,
        //                orderInfo.IsInsured ? orderInfo.TotalPrice : 0,
        //                orderItems);
        //            Console.WriteLine(rate.Count);
        //        }
        //        else
        //        {
        //            var rate = RateHelper.GetLocalRates(
        //                _company.ShipmentProviderInfoList.ToList<IStampsAuthInfo>(),
        //                _log,
        //                getBizDaysCount,
        //                _company.Zip,
        //                addressTo,
        //                DateTime.Now,
        //                orderInfo.ShippingService,
        //                orderInfo.WeightD,
        //                orderInfo.IsInsured ? orderInfo.TotalPrice : 0,
        //                orderItems);
        //            Console.WriteLine(rate.Count);
        //        }

        //    }
        //}

        private IList<IShipmentApi> GetShipmentProviders(CompanyDTO company)
        {
            var serviceFactory = new ServiceFactory();
            return serviceFactory.GetShipmentProviders(_log,
                _time,
                _dbFactory,
                _weightService,
                company.ShipmentProviderInfoList,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory);
        }

        public void ImportZipZones(string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                using (var db = _dbFactory.GetRWDb())
                {
                    for (var i = 0; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var range = row.GetCell(0).ToString();
                        var zone = row.GetCell(1).ToString();
                        var rangeParts = range.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var startRange = rangeParts.First() + "00";
                        var endRange = rangeParts.Last() + "99";

                        if (String.IsNullOrEmpty(range))
                            continue;

                        db.ZipCodeZones.Add(new ZipCodeZone()
                        {
                            RangeStart = Int32.Parse(startRange),
                            RangeEnd = Int32.Parse(endRange),
                            Zone = Int32.Parse(zone.Replace("*", "").Replace("+", "")),
                            Description = range + " : " + zone
                        });
                    }

                    db.Commit();
                }
            }
        }
    }
}
