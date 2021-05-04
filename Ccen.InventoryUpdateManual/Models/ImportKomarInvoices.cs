using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportKomarInvoices
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IQuantityManager _quantityManager;

        public ImportKomarInvoices(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IQuantityManager quantityManager)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _quantityManager = quantityManager;
        }

        public void ProcessFile(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            using (var db = _dbFactory.GetRWDb())
            {
                var existInvoice = db.VendorInvoices.GetAllAsDto().FirstOrDefault(v => v.FileName == filename);
                //if (existInvoice == null)
                {
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        IWorkbook workbook = null;
                        if (filename.EndsWith(".xlsx"))
                            workbook = new XSSFWorkbook(stream);
                        else
                            workbook = new HSSFWorkbook(stream);

                        var sheet = workbook.GetSheetAt(0);

                        var invoiceNumber = sheet.GetRow(2).GetCell(10).ToString();
                        var invoiceDate = DateTime.Parse(sheet.GetRow(2).GetCell(15).ToString());

                        long invoiceId;
                        if (existInvoice == null)
                        {
                            var invoice = new VendorInvoiceDTO()
                            {
                                FileName = filename,
                                InvoiceNumber = invoiceNumber,
                                InvoiceDate = invoiceDate,
                                CreateDate = _time.GetAppNowTime()
                            };
                            invoiceId = db.VendorInvoices.Store(invoice);
                        }
                        else
                        {
                            invoiceId = existInvoice.Id;
                        }

                        var font = workbook.CreateFont();
                        

                        ICellStyle redCellStyle = workbook.CreateCellStyle();
                        redCellStyle.CloneStyleFrom(sheet.GetRow(13).GetCell(1).CellStyle);
                        redCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
                        redCellStyle.FillPattern = FillPattern.SolidForeground;
                        redCellStyle.SetFont(sheet.GetRow(13).GetCell(1).CellStyle.GetFont(workbook));


                        ICellStyle yellowCellStyle = workbook.CreateCellStyle();
                        yellowCellStyle.CloneStyleFrom(sheet.GetRow(13).GetCell(1).CellStyle);
                        yellowCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
                        yellowCellStyle.FillPattern = FillPattern.SolidForeground;
                        yellowCellStyle.SetFont(sheet.GetRow(13).GetCell(1).CellStyle.GetFont(workbook));

                        ICellStyle greenCellStyle = workbook.CreateCellStyle();
                        greenCellStyle.CloneStyleFrom(sheet.GetRow(13).GetCell(1).CellStyle);
                        greenCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;
                        greenCellStyle.FillPattern = FillPattern.SolidForeground;
                        greenCellStyle.SetFont(sheet.GetRow(13).GetCell(1).CellStyle.GetFont(workbook));

                        for (var i = 12; i < sheet.LastRowNum; i++)
                        {
                            var index = (i - 12 + 1);

                            var styleId = sheet.GetRow(i).GetCell(1).ToString();
                            _log.Info("StyleString=" + styleId);

                            if (String.IsNullOrEmpty(styleId))
                                break;

                            var existInvoiceBoxCount = db.SealedBoxes.GetAll().Count(b => b.OriginId == invoiceId
                                && b.OriginTag == index.ToString());
                            if (existInvoiceBoxCount > 0)
                            {
                                _log.Info("StyleId already has boxes from current invoice, id=" + styleId);
                                sheet.GetRow(i).GetCell(1).CellStyle = greenCellStyle;
                                continue;
                            }


                            var totalQty = int.Parse(sheet.GetRow(i).GetCell(16).ToString());
                            var price = decimal.Parse(sheet.GetRow(i).GetCell(17).ToString());

                            var style = db.Styles.GetAllAsDto().FirstOrDefault(s => s.StyleID == styleId);
                            if (style == null)
                            {
                                _log.Info("StyleId not found, id=" + styleId);
                                sheet.GetRow(i).GetCell(1).CellStyle = redCellStyle;
                                continue;
                            }
                            var styleItems = db.StyleItems
                                .GetAll()
                                .Where(si => si.StyleId == style.Id)
                                .ToList()
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ToList();

                            var breakdowns = SizeHelper.GetBreakdowns(styleItems.Select(s => s.Size).ToList());
                            if (breakdowns == null)
                            {
                                _log.Info("No brekdowns");
                                sheet.GetRow(i).GetCell(1).CellStyle = yellowCellStyle;
                                continue;
                            }

                            var boxItems = breakdowns.Select(b => new SealedBoxItemDto() {BreakDown = b}).ToList();
                            
                            if (!boxItems.Any())
                            {
                                _log.Info("Unsupported size count = " + styleItems.Count);
                                continue;
                            }

                            sheet.GetRow(i).GetCell(1).CellStyle = greenCellStyle;

                            for (int j = 0; j < styleItems.Count; j++)
                            {
                                boxItems[j].StyleItemId = styleItems[j].Id;
                            }

                            var newBox = new SealedBox()
                            {
                                BoxQuantity = totalQty/boxItems.Sum(b => b.BreakDown),
                                PajamaPrice = price,
                                StyleId = style.Id,

                                OriginCreateDate = _time.GetUtcTime(),
                                OriginType = (int) BoxOriginTypes.VendorInvoice,
                                OriginId = invoiceId,
                                OriginTag = (i - 12 + 1).ToString(),

                                BoxBarcode = db.SealedBoxes.GetDefaultBoxName(style.Id, _time.GetAppNowTime()),
                                CreateDate = invoiceDate,
                            };

                            db.SealedBoxes.Add(newBox);
                            db.Commit();

                            foreach (var boxItem in boxItems)
                            {
                                boxItem.BoxId = newBox.Id;
                                db.SealedBoxItems.Add(new SealedBoxItem()
                                {
                                    BoxId = boxItem.BoxId,
                                    BreakDown = boxItem.BreakDown,
                                    StyleItemId = boxItem.StyleItemId,
                                    CreateDate = _time.GetAppNowTime(),
                                });

                                _quantityManager.LogStyleItemQuantity(db,
                                    boxItem.StyleItemId,
                                    boxItem.BreakDown * newBox.BoxQuantity,
                                    null,
                                    QuantityChangeSourceType.AddNewBox,
                                    null,
                                    null,
                                    null,
                                    _time.GetAppNowTime(),
                                    null);
                            }
                            db.Commit();

                            foreach (var styleItem in styleItems)
                            {
                                if (styleItem.Quantity != null)
                                {
                                    _log.Info("Switch to box qty, from=" + styleItem.Quantity);

                                    var oldQuantity = styleItem.Quantity;

                                    styleItem.Quantity = null;
                                    styleItem.QuantitySetBy = null;
                                    styleItem.QuantitySetDate = null;
                                    styleItem.RestockDate = null;

                                    _quantityManager.LogStyleItemQuantity(db,
                                       styleItem.Id,
                                       null,
                                       oldQuantity,
                                       QuantityChangeSourceType.UseBoxQuantity,
                                       null,
                                       null,
                                       null,
                                       _time.GetAppNowTime(),
                                       null);
                                }
                            }
                            db.Commit();
                        }

                        var newFilename = Path.GetFileNameWithoutExtension(filepath) + "_Processed" + Path.GetExtension(filepath);
                        var path = Path.GetDirectoryName(filepath);
                        var newFilepath = Path.Combine(path, newFilename);
                        using (FileStream file = new FileStream(newFilepath, FileMode.Create, FileAccess.Write))
                        {
                            workbook.Write(file);
                            file.Close();
                        }
                    }
                }
            }
        }
    }
}
