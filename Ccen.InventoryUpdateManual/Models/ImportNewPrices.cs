using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportNewPrices
    {
        private ILogService _log;
        private IDbFactory _dbFactory;

        public ImportNewPrices(ILogService log,
            IDbFactory dbFactory)
        {
            _log = log;
            _dbFactory = dbFactory;
        }

        public void ProcessFile(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filename.ToLower().EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    int index = 1;
                    while (index < sheet.LastRowNum)
                    {
                        var row = sheet.GetRow(index);
                        if (row.GetCell(5) != null)
                        {
                            var newPrice = decimal.Parse(row.GetCell(5).StringCellValue);
                            var itemId = int.Parse(row.GetCell(0).StringCellValue);

                            var item = db.Items.Get(itemId);
                            var listing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == item.Id && !l.IsRemoved);
                            listing.CurrentPrice = newPrice;
                            listing.PriceUpdateRequested = true;
                        }
                        index++;
                    }
                    db.Commit();
                }
            }
        }
    }
}
