using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DAL;
using CsvHelper;
using CsvHelper.Configuration;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportWholesale
    {
        private ILogService _logger;

        public ImportWholesale(ILogService logger)
        {
            _logger = logger;
        }

        public class ImportItem
        {
            public string StyleString { get; set; }
            public string Size { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }

            public long? StyleId { get; set; }
            public long? StyleItemId { get; set; }
        }

        public void Import(string filePath)
        {
            _logger.Info("Start importing...");

            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                TrimFields = true,
            });


            var itemResults = new List<ImportItem>();

            using (var db = new UnitOfWork(_logger))
            {
                var allDbStyles = db.Styles.GetAllAsDto().Where(s => !s.Deleted).ToList();
                var allDbStyleItems = db.StyleItems.GetAllAsDto().ToList();
                var sizeMapping = db.SizeMappings.GetAllAsDto().ToList();

                while (reader.Read())
                {
                    string styleString = reader.CurrentRecord[0];

                    if (String.IsNullOrEmpty(styleString))
                        continue;

                    string size = reader.CurrentRecord[1];
                    if (size != null)
                        size = size.Replace("SM", "S").Replace("SX", "XS");

                    int quantity = Int32.Parse(reader.CurrentRecord[2]);

                    var style = allDbStyles.FirstOrDefault(s => s.StyleID == styleString);
                    var sizes = sizeMapping.Where(s => s.ItemSize == size).Select(s => s.StyleSize).ToList();
                    sizes.Add(size);

                    if (style == null)
                    {
                        Console.WriteLine("No style: " + styleString);
                        continue;
                    }

                    var styleItem = allDbStyleItems.FirstOrDefault(s => s.StyleId == style.Id
                                                                        && sizes.Contains(s.Size));


                    if (styleItem == null)
                    {
                        Console.WriteLine("No size, style=" + styleString + ", size=" + size);
                        continue;
                    }

                    itemResults.Add(new ImportItem()
                    {
                        StyleId = style.Id,
                        StyleItemId = styleItem.StyleItemId,
                        Quantity = quantity,
                        Size = size,
                        StyleString = styleString,
                    });
                }
            }

            Console.WriteLine("Items count=" + itemResults.Count);

            //Insert into Db
            using (var db = new UnitOfWork(_logger))
            {
                var quantityOperation = new QuantityOperation()
                {
                    Type = (int) QuantityOperationType.Wholesale,
                    Comment = "From file Isaak_order",
                    CreateDate = DateTime.UtcNow,
                };
                db.QuantityOperations.Add(quantityOperation);
                db.Commit();

                foreach (var item in itemResults)
                {
                    db.QuantityChanges.Add(new QuantityChange()
                    {
                        QuantityOperationId = quantityOperation.Id,
                        Quantity = item.Quantity,
                        StyleId = item.StyleId.Value,
                        StyleItemId = item.StyleItemId.Value,
                        CreateDate = DateTime.UtcNow
                    });
                }
                db.Commit();
            }
        }
    }
}
