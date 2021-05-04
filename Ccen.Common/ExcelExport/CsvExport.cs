using Amazon.Core.Exports.Attributes;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.ExcelExport
{
    public class CsvExport
    {
        public static void ExportIntoFile<TViewModel>(string filepath, IList<TViewModel> viewModels,
            IList<ExcelColumnInfo> columns)
        {
            using (var stream = Export(viewModels, columns, null))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var file = File.Open(filepath, FileMode.Create))
                {
                    stream.WriteTo(file);
                }
            }
        }


        public static MemoryStream Export<TViewModel>(IList<TViewModel> viewModels,
            IList<ExcelColumnInfo> baseColumns,            
            IList<ExcelColumnOverrideInfo> columnOverrides,
            IList<ExcelColumnInfo> additionalColumns = null)
        {
            MemoryStream output = new MemoryStream();

            if (viewModels.Any())
            {
                var type = viewModels[0].GetType();
                if (baseColumns == null)
                {
                    var properties = ExcelHelper.GetProperties<TViewModel>(type);

                    properties = properties.OrderBy(p => ExcelHelper.GetExcelAttribute(p).Order).ToList();
                    var propertyToAttribute =
                        properties.Select(p => new Tuple<PropertyInfo, ExcelSerializableAttribute>(p, ExcelHelper.GetExcelAttribute(p))).ToList();

                    baseColumns = propertyToAttribute.Select(p => new ExcelColumnInfo()
                    {
                        Property = p.Item1,
                        Title = p.Item2.Name,
                        Format = p.Item2.Format,
                        Width = p.Item2.Width
                    }).ToList();
                }
                
                var columnList = baseColumns;

                if (columnOverrides != null)
                {
                    foreach (var columnOverride in columnOverrides)
                    {
                        if (columnOverride.RemoveIt)
                            columnList = RemoveColumn(columnList, columnOverride);
                    }
                }

                if (additionalColumns != null)
                {
                    foreach (var additionalColumn in additionalColumns)
                    {
                        if (baseColumns.All(c => c.Title != additionalColumn.Title))
                            baseColumns.Add(additionalColumn);
                    }
                }

                TextWriter writer = new StreamWriter(output);
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.Encoding = Encoding.UTF8;

                    foreach (var column in columnList)
                    {
                        csv.WriteField(column.Title);
                    }
                    
                    csv.NextRecord();

                    foreach (var obj in viewModels)
                    {
                        foreach (var column in columnList)
                        {
                            var value = ExcelHelper.GetPropertyValue<TViewModel>(obj, column.Property, column.Format);
                            csv.WriteField(value);
                        }
                        
                        csv.NextRecord();
                    }

                    writer.Flush();
                }
            }

            return output;
        }

        private static IList<ExcelColumnInfo> RemoveColumn(IList<ExcelColumnInfo> columns, ExcelColumnOverrideInfo removeInfo)
        {
            var toRemoveColumn = LookUpColumn(columns, removeInfo);
            columns.Remove(toRemoveColumn);
            return columns;
        }

        private static ExcelColumnInfo LookUpColumn(IList<ExcelColumnInfo> columnList, ExcelColumnOverrideInfo columnOverride)
        {
            ExcelColumnInfo existColumn = null;
            if (columnOverride.Index.HasValue && columnOverride.Index.Value < columnList.Count)
                existColumn = columnList[columnOverride.Index.Value];
            if (existColumn == null && !String.IsNullOrEmpty(columnOverride.Title))
                existColumn = columnList.FirstOrDefault(c => c.Title == columnOverride.Title);
            return existColumn;
        }
    }
}
