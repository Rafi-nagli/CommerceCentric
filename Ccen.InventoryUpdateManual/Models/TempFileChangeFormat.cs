using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.ExcelExport;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.InventoryUpdateManual.Models
{
    public class TempFileChangeFormat
    {
        public TempFileChangeFormat()
        {
            
        }

        public class RecordInfo
        {
            public string Style { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
            public int TotalQty { get; set; }
            public string UnitPrice { get; set; }
            public string Barcode { get; set; }
            public string CTN { get; set; }
        }

        public void ChangeFormat(string inputFilepath, string outputFilepath)
        {
            var sourceItems = new List<RecordInfo>();
            using (var stream = new FileStream(inputFilepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (inputFilepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var lastCTN = "";
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(2) == null)
                        continue;

                    var newRecord = new RecordInfo()
                    {
                        CTN = row.GetCell(0) != null ? row.GetCell(0).ToString() : lastCTN,
                        Style = row.GetCell(2).ToString(),
                        Color = row.GetCell(3).ToString(),
                        Size = row.GetCell(4).ToString(),
                        UnitPrice = row.GetCell(5).ToString(),
                        TotalQty = Int32.Parse(row.GetCell(6).ToString()),
                        Barcode = row.GetCell(8).ToString()
                    };
                    sourceItems.Add(newRecord);

                    lastCTN = newRecord.CTN;
                }
            }

            var resultItems = sourceItems.GroupBy(r => new { r.Style, r.Size, r.Color, r.Barcode })
                .Select(r => new RecordInfo()
                {
                    Style = r.Key.Style,
                    Size = r.Key.Size,
                    Color = r.Key.Color,
                    Barcode = r.Key.Barcode,
                    UnitPrice = r.Max(i => i.UnitPrice),
                    TotalQty = r.Sum(i => i.TotalQty),
                    CTN = string.Join(", ", r.Select(i => i.CTN).Where(i => !String.IsNullOrEmpty(i)).OrderBy(i => Int32.Parse(i)).Distinct())
                }).ToList();

            var b = new ExportColumnBuilder<RecordInfo>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.Style, "Style", 15),
                b.Build(p => p.Color, "Color", 15),
                b.Build(p => p.Size, "Size", 15),
                b.Build(p => p.TotalQty, "Total qty", 15),
                b.Build(p => p.UnitPrice, "Unit Price", 15),
                b.Build(p => p.Barcode, "Barcode", 15),
                b.Build(p => p.CTN, "Boxes", 15),
            };

            using (var stream = ExcelHelper.Export(resultItems, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(outputFilepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
