using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.DTO;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels.CustomBarcodes
{
    public class CustomBarcodeViewModel
    {
        public long? Id { get; set; }
        public string Barcode { get; set; }
        public string SKU { get; set; }

        public DateTime? AttachSKUDate { get; set; }

        public bool IsNewAssociation { get; set; }


        public CustomBarcodeViewModel()
        {
            
        }

        public CustomBarcodeViewModel(CustomBarcodeDTO barcode)
        {
            Id = barcode.Id;
            SKU = barcode.SKU;
            Barcode = barcode.Barcode;
            AttachSKUDate = barcode.AttachSKUDate;
            IsNewAssociation = barcode.IsNewAssociation;
        }

        public static IQueryable<CustomBarcodeViewModel> GetAll(IUnitOfWork db,
            CustomBarcodeFilterViewModel filter)
        {
            var query = db.CustomBarcodes.GetAllAsDto()
                .Where(b => !String.IsNullOrEmpty(b.SKU))
                .Select(b => new CustomBarcodeViewModel()
                {
                    Id = b.Id,
                    Barcode = b.Barcode,
                    SKU = b.SKU,
                    AttachSKUDate = b.AttachSKUDate,
                });

            if (!String.IsNullOrEmpty(filter.Barcode))
                query = query.Where(b => b.Barcode.StartsWith(filter.Barcode));
            if (!String.IsNullOrEmpty(filter.SKU))
                query = query.Where(b => b.SKU.StartsWith(filter.SKU));

            return query;
        }

        public static bool RemoveAttachedSKU(ILogService log, IUnitOfWork db, long id)
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

            return false;
        }
        
        public static IList<CustomBarcodeViewModel> AssociateBarcodes(IBarcodeService barcodeService,
            string skuText,
            DateTime when,
            long? by)
        {
            var skuList = skuText.Split(" \r\n\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var results = new List<CustomBarcodeViewModel>();
            foreach (var sku in skuList)
            {
                var formattedSku = sku.Trim();
                if (String.IsNullOrEmpty(formattedSku))
                    continue;

                var barcodeDto = barcodeService.AssociateBarcodes(sku, when, by);
                results.Add(new CustomBarcodeViewModel(barcodeDto));
            }

            return results;
        }
    }
}