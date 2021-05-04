using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO;

namespace Amazon.Model.Implementation
{
    public class BarcodeService : IBarcodeService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public BarcodeService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _time = time;
            _log = log;
        }

        public bool RemoveAttachedSKU(long id)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var exist = db.CustomBarcodes.Get(id);
                if (exist != null && !String.IsNullOrEmpty(exist.SKU))
                {
                    exist.SKU = null;
                    exist.AttachSKUDate = null;
                    exist.AttachSKUBy = null;
                    db.Commit();

                    return true;
                }
            }

            return false;
        }

        public CustomBarcodeDTO AssociateBarcodes(string sku,
            DateTime when,
            long? by)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var formattedSku = sku.Trim();
                if (String.IsNullOrEmpty(formattedSku))
                    return null;

                var barcode = new CustomBarcodeDTO()
                {
                    SKU = formattedSku
                };
                var existSku = db.CustomBarcodes.GetAllAsDto().FirstOrDefault(b => b.SKU == formattedSku);
                if (existSku != null)
                {
                    barcode.Barcode = existSku.Barcode;
                    barcode.AttachSKUDate = existSku.AttachSKUDate;
                }
                else
                {
                    var emptyBarcode = db.CustomBarcodes.GetAll()
                            .OrderBy(b => b.Id)
                            .FirstOrDefault(b => String.IsNullOrEmpty(b.SKU));
                    if (emptyBarcode != null)
                    {
                        emptyBarcode.SKU = formattedSku;
                        emptyBarcode.AttachSKUDate = when;
                        emptyBarcode.AttachSKUBy = by;
                        db.Commit();

                        barcode.Barcode = emptyBarcode.Barcode;
                        barcode.AttachSKUDate = when;
                        barcode.IsNewAssociation = true;
                    }
                }

                return barcode;
            }
        }
    }
}
