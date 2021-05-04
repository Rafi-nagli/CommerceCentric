using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Common.ExcelExport;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;
using CsvHelper;
using CsvHelper.Configuration;

namespace Amazon.Model.Implementation
{
    public class DhlInvoiceService
    {
        private ITime _time;
        private ILogService _log;
        private IDbFactory _dbFactory;

        public DhlInvoiceService(ILogService log, ITime time, IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }


        public void ProcessRates(IList<DhlRateCodePriceDTO> rates)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var rate in rates)
                {
                    var existRate = db.DhlRateCodePrices.GetAllAsDto()
                        .FirstOrDefault(iv => iv.ProductCode == rate.ProductCode
                            && iv.Package == rate.Package
                            && iv.RateCode == rate.RateCode
                            && iv.Weight == rate.Weight);

                    if (existRate == null)
                    {
                        db.DhlRateCodePrices.Store(rate);
                    }
                    else
                    {
                        rate.Id = existRate.Id;
                        db.DhlRateCodePrices.Update(rate);
                    }

                    _log.Info("Invoice was processed, productCode: " + rate.ProductCode
                        + ", package: " + rate.Package
                        + ", weight: " + rate.Weight
                        + ", rateCode: " + rate.RateCode
                        + ", price: " + rate.Price);
                }
            }
        }

        public void ProcessRecords(IList<DhlInvoiceDTO> invoices, ShipmentProviderType type)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var invoice in invoices)
                {
                    var existInvoice = db.DhlInvoices.GetAllAsDto()
                        .FirstOrDefault(iv => iv.InvoiceNumber == invoice.InvoiceNumber
                                     && iv.BillNumber == invoice.BillNumber);

                    if (existInvoice == null)
                    {
                        var order = db.Orders.GetByOrderIdAsDto(invoice.OrderNumber);
                        decimal? chargeEstimated = null;
                        if (order != null)
                        {
                            var shippingInfoList = db.OrderShippingInfos.GetAllAsDto()
                                    .Where(sh => sh.OrderId == order.Id && sh.IsActive)
                                    .ToList();
                            chargeEstimated = shippingInfoList.Where(
                                sh => sh.ShipmentProviderType == (int) type)
                                .Sum(sh => sh.StampsShippingCost);

                            invoice.Estimated = chargeEstimated;
                            if (Math.Abs((chargeEstimated ?? 0) - invoice.ChargedSummary) < 1) //$1
                                invoice.Status = (int) DhlInvoiceStatusEnum.Matched;
                            else
                                invoice.Status = (int) DhlInvoiceStatusEnum.Incorrect;
                        }
                        else
                        {
                            invoice.Status = (int) DhlInvoiceStatusEnum.OrderNotFound;
                        }

                        db.DhlInvoices.Store(invoice);
                    }
                    else
                    {
                        invoice.Id = existInvoice.Id;
                        db.DhlInvoices.Update(invoice);
                    }

                    _log.Info("Invoice was processed, invoice #: " + invoice.InvoiceNumber 
                        + ", order #: " + invoice.OrderNumber 
                        + ", airbill: " + invoice.BillNumber
                        + ", status: " + invoice.Status);
                }
            }
        }

        public void ProcessIBCRecords(IList<DhlInvoiceDTO> invoices, ShipmentProviderType type)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var invoice in invoices)
                {
                    var existInvoice = db.DhlInvoices.GetAllAsDto()
                        .FirstOrDefault(iv => iv.InvoiceNumber == invoice.InvoiceNumber
                                     && iv.BillNumber == invoice.BillNumber);

                    if (existInvoice == null)
                    {
                        var shippings = db.OrderShippingInfos.GetAllAsDto().Where(sh => sh.TrackingNumber == invoice.BillNumber).ToList();
                        decimal? chargeEstimated = null;
                        if (shippings.Any())
                        {
                            chargeEstimated = shippings.Where(
                                sh => sh.ShipmentProviderType == (int)type)
                                .Sum(sh => sh.StampsShippingCost);

                            invoice.Estimated = chargeEstimated;
                            if (Math.Abs((chargeEstimated ?? 0) - invoice.ChargedSummary) < 1) //$1
                                invoice.Status = (int)DhlInvoiceStatusEnum.Matched;
                            else
                                invoice.Status = (int)DhlInvoiceStatusEnum.Incorrect;
                        }
                        else
                        {
                            invoice.Status = (int)DhlInvoiceStatusEnum.OrderNotFound;
                        }

                        db.DhlInvoices.Store(invoice);
                    }
                    else
                    {
                        invoice.Id = existInvoice.Id;
                        db.DhlInvoices.Update(invoice);
                    }

                    _log.Info("Invoice was processed, invoice #: " + invoice.InvoiceNumber
                        + ", order #: " + invoice.OrderNumber
                        + ", airbill: " + invoice.BillNumber
                        + ", status: " + invoice.Status);
                }
            }
        }

        public IList<DhlRateCodePriceDTO> GetRatesFromFile(string filePath)
        {
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });
            //Weight,A,B,C,D,E,F,G,H1,H2,I,L,M,J,K,N,P
            //Product, Package, Weight, A, B, ...
            var filename = Path.GetFileName(filePath);

            var results = new List<DhlRateCodePriceDTO>();
            while (reader.Read())
            {
                var productCode = reader.GetField<string>("Product");
                if (String.IsNullOrEmpty(productCode))
                    continue;

                var package = reader.GetField<string>("Package");
                var weight = double.Parse(reader.GetField<string>("Weight").Replace("lb", "").Trim());
                
                for (int i = 3; i < reader.CurrentRecord.Length; i++)
                {
                    var rate = new DhlRateCodePriceDTO();

                    rate.ProductCode = productCode;
                    rate.Package = package;
                    rate.Price = decimal.Parse(reader.GetField<string>(i).Replace("$", "").Trim());
                    rate.RateCode = reader.FieldHeaders[i];
                    rate.Weight = weight;

                    results.Add(rate);
                }
            }

            _log.Info("File was processed, file: " + filename + ", records: " + results.Count);

            return results;
        } 

        public IList<DhlInvoiceDTO> GetRecordsFromFile(string filePath)
        {
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            var filename = Path.GetFileName(filePath);

            var results = new List<DhlInvoiceDTO>();
            while (reader.Read())
            {
                var invoice = new DhlInvoiceDTO();

                invoice.InvoiceNumber = reader.GetField<string>("INVOICE #");
                invoice.InvoiceDate = reader.GetField<DateTime>("INVOICE DATE");
                invoice.BillNumber = reader.GetField<string>("AIRBILL #");
                invoice.Dimensions = reader.GetField<string>("DIMENSIONS");
                invoice.SourceFile = filename;

                invoice.OrderNumber = reader.GetField<string>("SHIPPER REFERENCE");
                invoice.ChargedBase = reader.GetField<decimal>("BASE CHARGE AMOUNT");
                invoice.ChargedSummary = reader.GetField<decimal>("BASE CHARGE AMOUNT")
                                         + (reader.GetField<decimal?>("CHARGE 1 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 2 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 3 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 4 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 5 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 6 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 7 AMT") ?? 0)
                                         + (reader.GetField<decimal?>("CHARGE 8 AMT") ?? 0);
                invoice.ChargedCredit = (reader.GetField<decimal?>("CREDIT 1 AMT") ?? 0)
                                        + (reader.GetField<decimal?>("CREDIT 2 AMT") ?? 0)
                                        + (reader.GetField<decimal?>("CREDIT 3 AMT") ?? 0);

                invoice.CreateDate = _time.GetAppNowTime();

                results.Add(invoice);
            }

            _log.Info("File was processed, file: " + filename + ", records: " + results.Count);

            return results;
        }

        public IList<DhlInvoiceDTO> GetIBCRecordsFromFile(string filePath)
        {
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            var filename = Path.GetFileName(filePath);

            var results = new List<DhlInvoiceDTO>();
            while (reader.Read())
            {
                var invoice = new DhlInvoiceDTO();

                invoice.InvoiceNumber = reader.GetField<string>("Invoice Number");
                invoice.InvoiceDate = reader.GetField<DateTime>("Ship Date");
                invoice.BillNumber = reader.GetField<string>("Package ID");
                invoice.Dimensions = reader.GetField<string>("Weight");
                invoice.SourceFile = filename;

                invoice.OrderNumber = reader.GetField<string>("Customer Reference Number");
                invoice.ChargedBase = reader.GetField<decimal>("Weight Charge");
                invoice.ChargedSummary = reader.GetField<decimal>("Total Charge");
                
                invoice.CreateDate = _time.GetAppNowTime();

                results.Add(invoice);
            }

            _log.Info("File was processed, file: " + filename + ", records: " + results.Count);

            return results;
        }
    }
}
