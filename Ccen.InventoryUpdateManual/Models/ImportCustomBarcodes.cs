using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportCustomBarcodes
    {
        private IDbFactory _dbFactory;
        private ITime _time;
        private ILogService _log;

        public ImportCustomBarcodes(IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _time = time;
            _log = log;
        }

        public void Import(string filename)
        {
            var lines = File.ReadAllLines(filename);
            using (var db = _dbFactory.GetRWDb())
            {
                var existBarcodes = db.CustomBarcodes.GetAllAsDto().ToList();

                var index = 0;
                var existIndex = 0;
                foreach (var line in lines)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    var barcode = line.Trim();
                    var exist = existBarcodes.FirstOrDefault(b => b.Barcode == barcode);
                    if (exist == null)
                    {
                        var newBarcode = new CustomBarcode()
                        {
                            Barcode = barcode,
                            CreateDate = _time.GetUtcTime()
                        };

                        db.CustomBarcodes.Add(newBarcode);

                        existBarcodes.Add(new CustomBarcodeDTO()
                        {
                            Barcode = barcode
                        });

                        index++;
                        _log.Info("Added: " + index);
                    }
                    else
                    {
                        existIndex++;
                        _log.Info("Exist: " + existIndex);
                    }

                    if (index%200 == 0)
                    {
                        db.Commit();
                    }
                }
            }
        }
    }
}
