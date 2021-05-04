using Amazon.DTO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ExcelReader
    {
        public static IList<ItemDTO> LoadSKUs(string filename,
            int skuColumnIndex)
        {
            var results = new List<ItemDTO>();

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filename.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(skuColumnIndex) != null)
                    {
                        try
                        {
                            results.Add(new ItemDTO()
                            {
                                SKU = row.GetCell(skuColumnIndex).ToString(),
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

            return results;
        }

        public static IList<ItemDTO> LoadPrices(string filename,
            int skuColumnIndex,
            int priceColumnIndex)
        {
            var results = new List<ItemDTO>();

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filename.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(priceColumnIndex) != null)
                    {
                        try
                        {
                            results.Add(new ItemDTO()
                            {
                                SKU = row.GetCell(skuColumnIndex).ToString(),
                                CurrentPrice = (decimal)row.GetCell(priceColumnIndex).NumericCellValue
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

            return results;
        }

        public static IList<ItemDTO> LoadQuantities(string filename,
            int skuColumnIndex,
            int qtyColumnIndex)
        {
            var results = new List<ItemDTO>();

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filename.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                for (var i = 2; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    results.Add(new ItemDTO()
                    {
                        SKU = row.GetCell(skuColumnIndex).ToString(),
                        RealQuantity = Int32.Parse(row.GetCell(qtyColumnIndex).ToString())
                    });
                }
            }

            return results;
        }
    }
}
