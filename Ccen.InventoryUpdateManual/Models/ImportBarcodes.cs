using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using CsvHelper;
using CsvHelper.Configuration;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportBarcodes
    {
        private ILogService _log;

        public ImportBarcodes(ILogService log)
        {
            _log = log;
        }


        public class BarcodeLine
        {
            public string Barcode { get; set; }
            public string StyleId { get; set; }
            public string Size { get; set; }
        }

        public void Import(string filePath)
        {
            _log.Info("Start importing...");

            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = false,
                Delimiter = ",",
                TrimFields = true,
            });


            var itemResults = new List<BarcodeLine>();

            using (var db = new UnitOfWork(_log))
            {
                var allDbStyles = db.Styles.GetAllAsDto().Where(s => !s.Deleted).ToList();
                var allDbStyleItems = db.StyleItems.GetAllAsDto().ToList();
                var allDbSizeMapping = db.SizeMappings.GetAllAsDto().ToList();
                var allDbSizes = db.Sizes.GetAllAsDto().ToList();

                while (reader.Read())
                {
                    string barcode = reader.CurrentRecord[0];
                    string sourceStyleString = reader.CurrentRecord[1];
                    string size = reader.CurrentRecord[2];

                    var styleString = sourceStyleString;
                    //Check exist
                    if (String.IsNullOrEmpty(barcode)
                        || String.IsNullOrEmpty(styleString))
                        continue;

                    var existBarcode = db.StyleItemBarcodes
                        .GetAll()
                        .FirstOrDefault(b => b.Barcode == barcode);

                    if (existBarcode != null)
                        continue;

                    //Correct sizes
                    if (size != null)
                        size = size.Replace("X/S", "XS");

                    if (styleString.EndsWith("ZA") || styleString.EndsWith("DZ"))
                        styleString = styleString.Substring(0, styleString.Length - 2);

                    if (styleString.StartsWith("21"))
                        styleString = styleString.Substring(2, styleString.Length - 2);

                    styleString = styleString.ToUpper();

                    //Find possible size mappings
                    var styles = allDbStyles.Where(s => s.StyleID.ToUpper().Contains(styleString)).ToList();
                    var sizes = allDbSizeMapping.Where(s => s.ItemSize == size).Select(s => s.StyleSize).ToList();
                    sizes.Insert(0, size);

                    //Create style if none
                    if (styles.Count == 0)
                    {
                        var style = CreateStyle(db, sourceStyleString);
                        allDbStyles.Add(style);
                        styles.Add(style);

                        Console.WriteLine("Style was created: " + styleString);
                    }

                    if (styles.Count > 1)
                    {
                        Console.WriteLine("Multiple style: " + styleString);
                        continue;
                    }
                    var styleId = styles[0].Id;

                    //Create styleItem if none
                    var styleItem = allDbStyleItems.FirstOrDefault(s => s.StyleId == styleId
                                                                        && sizes.Contains(s.Size));
                    if (styleItem == null)
                    {
                        var itemSizes = allDbSizes.Where(s => sizes.Contains(s.Name)).ToList();

                        if (itemSizes.Count > 0)
                        {
                            styleItem = CreateStyleItem(db, styleId, itemSizes[0]);
                            allDbStyleItems.Add(styleItem);
                        }

                        Console.WriteLine("Created style item, style=" + styleString + ", size=" + size);
                    }

                    if (styleItem == null)
                    {
                        Console.WriteLine("Can't create styleItem");
                        continue;
                    }

                    //Add barcode
                    db.StyleItemBarcodes.Add(new StyleItemBarcode()
                    {
                        Barcode = barcode,
                        StyleItemId = styleItem.StyleItemId,
                        CreateDate = DateHelper.GetAppNowTime()
                    });
                    db.Commit();
                    Console.WriteLine("Barcode was created, barcode=" + barcode);
                }
            }
        }

        private static StyleEntireDto CreateStyle(IUnitOfWork db, string styleString)
        {
            var style = new Style()
            {
                StyleID = styleString,
                Name = "-",
                SourceType = (int)StyleSourceType.ForBarcode,
                Description = "Created by the system for inventory scan app",
                CreateDate = DateHelper.GetAppNowTime(),
            };
            db.Styles.Add(style);
            db.Commit();

            return new StyleEntireDto()
            {
                Id = style.Id,
                StyleID = style.StyleID
            };
        }

        private static StyleItemDTO CreateStyleItem(IUnitOfWork db,
            long styleId,
            SizeDTO size)
        {
            var styleItem = new StyleItem()
            {
                StyleId = styleId,
                SizeId = size.Id,
                Size = size.Name,
                CreateDate = DateHelper.GetAppNowTime()
            };
            db.StyleItems.Add(styleItem);
            db.Commit();

            return new StyleItemDTO()
            {
                StyleItemId = styleItem.Id,
                StyleId = styleItem.StyleId,
                Size = styleItem.Size,
                SizeId = styleItem.SizeId,
            };
        }
    }
}
