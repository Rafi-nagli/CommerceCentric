using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Inventory;
using Amazon.DAL;

namespace Amazon.InventoryUpdateManual.Models
{
    public class SyncListingAndStyleBarcodesJob
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public SyncListingAndStyleBarcodesJob(ILogService log, 
            IDbFactory dbFactory,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public void RelinkListingsBarcodesToActualStyles()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var itemQuery = from i in db.Items.GetAllViewAsDto()
                                group i by i.Barcode
                                    into byBarcode
                                    select byBarcode.FirstOrDefault();
                var styleBarcodesQuery = from b in db.StyleItemBarcodes.GetAll()
                                         join si in db.StyleItems.GetAll() on b.StyleItemId equals si.Id
                                         join s in db.Styles.GetAll() on si.StyleId equals s.Id
                                         select b;

                var items = itemQuery.ToList();
                var styleBarcodes = styleBarcodesQuery.ToList();

                foreach (var item in items)
                {
                    if (String.IsNullOrEmpty(item.Barcode))
                        continue;

                    var itemStyleItemId = item.StyleItemId;
                    if (itemStyleItemId.HasValue)
                    {
                        var styleBarcode = styleBarcodes.FirstOrDefault(b => b.Barcode == item.Barcode);
                        if (styleBarcode != null)
                        {
                            if (styleBarcode.StyleItemId != itemStyleItemId)
                            {
                                _log.Debug("Different styleItemId, itemStyleItemId=" + itemStyleItemId +
                                           ", barcodeStyleItemId=" + styleBarcode.StyleItemId);
                                styleBarcode.StyleItemId = itemStyleItemId.Value;
                            }
                        }
                    }
                }
                db.Commit();
            }
        }

        public void MoveBarcodesFromListingsToStyles()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var itemQuery = from i in db.Items.GetAllViewAsDto()
                                group i by i.Barcode
                                    into byBarcode
                                    select byBarcode.FirstOrDefault();
                var styleBarcodesQuery = from b in db.StyleItemBarcodes.GetAll()
                                         join si in db.StyleItems.GetAll() on b.StyleItemId equals si.Id
                                         join s in db.Styles.GetAll() on si.StyleId equals s.Id
                                         select b;

                var items = itemQuery.ToList();
                var styleBarcodes = styleBarcodesQuery.ToList();

                var notExistBarcodes = items.Where(i => styleBarcodes.All(s => s.Barcode != i.Barcode)).ToList();

                foreach (var notExistBarcode in notExistBarcodes)
                {
                    _log.Info("Item, styleId=" + notExistBarcode.StyleId
                              + ", styleItemId=" + notExistBarcode.StyleItemId
                              + ", size=" + notExistBarcode.Size
                              + ", styleSize=" + notExistBarcode.StyleSize
                              + ", styleColor=" + notExistBarcode.StyleColor
                              + ", barcode=" + notExistBarcode.Barcode);

                    if (notExistBarcode.StyleItemId.HasValue
                        && !String.IsNullOrEmpty(notExistBarcode.Barcode))
                    {
                        db.StyleItemBarcodes.Add(new StyleItemBarcode()
                        {
                            Barcode = notExistBarcode.Barcode,
                            StyleItemId = notExistBarcode.StyleItemId.Value,
                            CreateDate = _time.GetAppNowTime(),
                        });
                        db.Commit();
                    }
                    else
                    {
                        if (notExistBarcode.StyleId.HasValue)
                        {
                            _log.Info("Item hasn't styleId");
                        }
                        else
                        {
                            _log.Info("Item have styleId, but not size mapping");
                        }
                    }
                }
            }
        }
    }
}
