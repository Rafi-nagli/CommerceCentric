using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.Common.Helpers;
using Amazon.Core.Exports.Attributes;
using CsvHelper;
using CsvHelper.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.Common.ExcelExport
{
    public class ExcelHelper
    {
        private const int CellScale = 256;
        //private const int CellCount = 130;

        public const int DEFAULT_WIDTH = 21;

        public static string ParentageParent = "Parent";
        public static string ParentageChild = "Child";

        public class CustomField
        {
            public int Row { get; set; }
            public int Cell { get; set; }
            public string Value { get; set; }
        }

        public static bool HasColumn(string filepath, string columnName)
        {
            try
            {
                
                if (filepath.ToLower().EndsWith(".xlsx")
                    || filepath.ToLower().EndsWith(".xls"))
                {
                    using (var tmpl = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    {
                        IWorkbook workbook = null;
                        if (filepath.ToLower().EndsWith(".xlsx"))
                            workbook = new XSSFWorkbook(tmpl);
                        else
                            workbook = new HSSFWorkbook(tmpl);

                        var sheet = workbook.GetSheetAt(0);
                        var row = sheet.GetRow(0);
                        if (row.GetCell(0).ToString() == "General")
                            row = sheet.GetRow(1);

                        return row.Cells.Any(c => (c != null && c.ToString() == columnName));
                    }
                }
                if (filepath.ToLower().EndsWith(".csv"))
                {
                    using (var stream = new StreamReader(filepath))
                    {
                        using (CsvReader reader = new CsvReader(stream, new CsvConfiguration
                        {
                            HasHeaderRecord = true,
                            Delimiter = ",",
                            TrimFields = true,
                        }))
                        {
                            reader.Read();
                            var headerRow = reader.FieldHeaders;
                            return headerRow.Any(h => h == columnName);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public static IList<string> GetHeaders(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                return new List<string>();

            if (filename.EndsWith(".xlsx")
                || filename.EndsWith(".xls"))
            {
                IWorkbook workbook = null;
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (filename.ToLower().EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    ISheet sheet = null;
                    sheet = workbook.GetSheetAt(0);

                    var row = sheet.GetRow(0);
                    var headers = row.Cells.Where(c => c != null).Select(c => c.ToString()).ToList();
                    workbook.Close();

                    return headers;
                }
            }
            if (filename.EndsWith(".csv"))
            {
                using (var stream = new StreamReader(filename))
                {
                    using (CsvReader reader = new CsvReader(stream, new CsvConfiguration
                    {
                        HasHeaderRecord = true,
                        Delimiter = ",",
                        TrimFields = true,
                    }))
                    {
                        while (reader.Read())
                        {
                            var headerRow = reader.FieldHeaders;
                            return headerRow.ToList();
                        }
                    }
                }
            }

            return new List<string>();
        }

        public static MemoryStream ExportIntoFile<TViewModel>(string filename, 
            string sheetName,
            IList<TViewModel> viewModels,
            IList<CustomField> customData = null,
            int headerRowOffset = 3,
            bool createRow = true,
            bool copyRow = false,
            bool useOriginalTypes = false)
        {
            using (var tmpl = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                return ExportIntoFile(tmpl,
                    filename.ToLower().EndsWith(".xlsx"),
                    sheetName,
                    viewModels,
                    customData,
                    headerRowOffset,
                    createRow,
                    copyRow,
                    useOriginalTypes);
            }
        }


        public static MemoryStream ExportIntoFile<TViewModel>(Stream stream,
            bool isXlsx,
            string sheetName,
            IList<TViewModel> viewModels,
            IList<CustomField> customData = null,
            int headerRowOffset = 3,
            bool createRow = true,
            bool copyRow = false,
            bool useOriginalTypes = false)
        {
            var output = new MemoryStream();
            var tmpl = stream;
            stream.Seek(0, SeekOrigin.Begin);
            IWorkbook workbook = null;
            if (isXlsx)
                workbook = new XSSFWorkbook(tmpl);
            else
                workbook = new HSSFWorkbook(tmpl);

            ISheet sheet = null;
            if (!String.IsNullOrEmpty(sheetName))
                sheet = workbook.GetSheet(sheetName); //"Template");
            else
                sheet = workbook.GetSheetAt(0);

            if (viewModels.Any())
            {
                var type = viewModels[0].GetType();

                var properties = GetProperties<TViewModel>(type);

                properties = properties.OrderBy(p => GetExcelAttribute(p).Order).ToList();
                var propertyToAttribute =
                    properties.Select(
                        p => new Tuple<PropertyInfo, ExcelSerializableAttribute>(p, GetExcelAttribute(p))).ToList();

                for (var i = 0; i < viewModels.Count; i++)
                {
                    IRow row;
                    if (copyRow)
                    {
                        if (i > 1)
                        {
                            row = sheet.CopyRow(headerRowOffset + 1, headerRowOffset + i);
                        }
                        row = sheet.GetRow(i + headerRowOffset);
                    }
                    else
                    {
                        if (createRow)
                            row = sheet.CreateRow(i + headerRowOffset);
                        else
                            row = sheet.GetRow(i + headerRowOffset);
                    }
                    WriteViewModelToRow(viewModels[i], row, propertyToAttribute, createRow, useOriginalTypes);
                }

                if (customData != null)
                {
                    foreach (var field in customData)
                    {
                        var row = sheet.GetRow(field.Row);
                        var cell = row.GetCell(field.Cell);
                        cell.SetCellValue(field.Value);
                    }
                }

                HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);
                workbook.Write(output);
            }
            return output;
        }

        public static MemoryStream Export(IEnumerable viewModels, Type type,
            IList<ExcelColumnInfo> columns,
            string templateFilepath = null,
            string newSheetName = null,
            bool isXlsx = false, List<string> propertyNames = null)
        {
            IWorkbook workbook;
            MemoryStream output = new MemoryStream();
            FileStream templateStream = null;
            var isNew = String.IsNullOrEmpty(templateFilepath);

            if (isNew)
            {
                if (isXlsx)
                    workbook = new XSSFWorkbook();
                else
                    workbook = new HSSFWorkbook();
            }
            else
            {
                templateStream = new FileStream(templateFilepath, FileMode.Open, FileAccess.ReadWrite);
                if (templateFilepath.ToLower().EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(templateStream);
                else
                    workbook = new HSSFWorkbook(templateStream);
            }

            if (viewModels.Cast<object>().Any())
            {
                if (columns == null)
                {
                    var properties = GetProperties(type, propertyNames);

                    properties = properties.OrderBy(p => GetExcelAttribute(p).Order).ToList();
                    var propertyToAttribute =
                        properties.Select(p => new Tuple<PropertyInfo, ExcelSerializableAttribute>(p, GetExcelAttribute(p))).ToList();

                    columns = propertyToAttribute.Select(p => new ExcelColumnInfo()
                    {
                        Property = p.Item1,
                        Title = p.Item2.Title ?? p.Item2.Name,
                        Format = p.Item2.Format,
                        Width = p.Item2.Width
                    }).ToList();
                }

                var columnList = columns;

                ISheet sheet = null;
                if (isNew)
                {
                    if (String.IsNullOrEmpty(newSheetName))
                        sheet = workbook.CreateSheet();
                    else
                        sheet = workbook.CreateSheet(newSheetName);
                }
                else
                    sheet = workbook.GetSheetAt(0);

                SetColumnWidth(sheet, columnList.Select(s => s.Width).ToList());

                IRow headerRow = null;
                if (isNew)
                {
                    headerRow = sheet.CreateRow(0);
                    SetHeaderRow(headerRow, columnList.Select(s => s.Title).ToList());
                    //freeze the header row so it is not scrolled
                    sheet.CreateFreezePane(0, 1, 0, 1);
                }
                else
                {
                    headerRow = sheet.GetRow(0);
                }

                var i = 0;
                foreach (var v in viewModels)
                {
                    var row = sheet.CreateRow(i + 1);
                    WriteViewModelToRow(v, row, columnList);
                    i++;
                }
            }

            workbook.Write(output);

            if (templateStream != null)
                templateStream.Dispose();

            return output;

        }

        public static MemoryStream Export<TViewModel>(IList<TViewModel> viewModels,
            IList<ExcelColumnInfo> columns,
            string templateFilepath = null,
            string newSheetName = null,
            bool isXlsx = false, 
            List<string> propertyNames = null)
        {
            IWorkbook workbook;
            MemoryStream output = new MemoryStream();
            FileStream templateStream = null;
            var isNew = String.IsNullOrEmpty(templateFilepath);

            if (isNew)
            {
                if (isXlsx)
                    workbook = new XSSFWorkbook(); 
                else
                    workbook = new HSSFWorkbook();
            }
            else
            {
                templateStream = new FileStream(templateFilepath, FileMode.Open, FileAccess.ReadWrite);
                if (templateFilepath.ToLower().EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(templateStream);
                else
                    workbook = new HSSFWorkbook(templateStream);
            }

            if (viewModels.Any())
            {
                var type = viewModels[0].GetType();
                if (columns == null)
                {
                    var properties = GetProperties<TViewModel>(type, propertyNames);

                    properties = properties.OrderBy(p => GetExcelAttribute(p).Order).ToList();
                    var propertyToAttribute =
                        properties.Select(p => new Tuple<PropertyInfo, ExcelSerializableAttribute>(p, GetExcelAttribute(p))).ToList();

                    columns = propertyToAttribute.Select(p => new ExcelColumnInfo()
                    {
                        Property = p.Item1,
                        Title = p.Item2.Title ?? p.Item2.Name,
                        Format = p.Item2.Format,
                        Width = p.Item2.Width
                    }).ToList();
                }

                var columnList = columns;

                ISheet sheet = null;
                if (isNew)
                {
                    if (String.IsNullOrEmpty(newSheetName))
                        sheet = workbook.CreateSheet();
                    else
                        sheet = workbook.CreateSheet(newSheetName);
                }
                else
                    sheet = workbook.GetSheetAt(0);

                SetColumnWidth(sheet, columnList.Select(s => s.Width).ToList());

                IRow headerRow = null;
                if (isNew)
                {
                    headerRow = sheet.CreateRow(0);
                    SetHeaderRow(headerRow, columnList.Select(s => s.Title).ToList());
                    //freeze the header row so it is not scrolled
                    sheet.CreateFreezePane(0, 1, 0, 1);
                }
                else
                {
                    headerRow = sheet.GetRow(0);
                }


                for (var i = 0; i < viewModels.Count; i++)
                {
                    var row = sheet.CreateRow(i + 1);
                    WriteViewModelToRow(viewModels[i], row, columnList);
                }
            }

            workbook.Write(output);

            if (templateStream != null)
                templateStream.Dispose();

            return output;
        }

        public static List<PropertyInfo> GetProperties<TViewModel>(Type type, List<string> propertyNames = null)
        {
            var properties = typeof(TViewModel).GetProperties()
                .Where(f => GetExcelAttribute(f) != null).ToList();

            var classProperties = type.GetProperties().Where(f => GetExcelAttribute(f) != null);
            var propertiesToAdd = classProperties.Where(p => !properties.Contains(p));
            properties.AddRange(propertiesToAdd);

            if (propertyNames != null)
            {
                properties = properties.Where(f => propertyNames.Contains(f.Name)).ToList();
            }
            return properties;
        }

        public static List<PropertyInfo> GetProperties(Type type, List<string> propertyNames = null)
        {
            var properties = type.GetProperties()
                .Where(f => GetExcelAttribute(f) != null).ToList();

            var classProperties = type.GetProperties().Where(f => GetExcelAttribute(f) != null);
            var propertiesToAdd = classProperties.Where(p => !properties.Contains(p));
            properties.AddRange(propertiesToAdd);

            if (propertyNames != null)
            {
                properties = properties.Where(f => propertyNames.Contains(f.Name)).ToList();
            }
            return properties;
        }

        #region private methods

        public static ExcelSerializableAttribute GetExcelAttribute(PropertyInfo properties)
        {
            return (ExcelSerializableAttribute)properties
                .GetCustomAttributes(typeof(ExcelSerializableAttribute), true).FirstOrDefault() 
                ?? (ExcelSerializableAttribute)properties
                .GetCustomAttributes(typeof(ExcelSerializableAttribute), true).FirstOrDefault();
        }

        private static void SetColumnWidth(ISheet sheet, List<int> widthList)
        {
            for (var i = 0; i < widthList.Count; i++)
            {
                sheet.SetColumnWidth(i, widthList[i] * CellScale);
            }
        }

        public static void SetHeaderRow(IRow headerRow, List<string> titleList)
        {
            for (var i = 0; i < titleList.Count; i++)
            {
                var columnName = titleList[i];
                headerRow.CreateCell(i).SetCellValue(columnName);
            }
        }

        private static void WriteViewModelToRow<TViewModel>(TViewModel viewModel, 
            IRow row, 
            List<Tuple<PropertyInfo, 
            ExcelSerializableAttribute>> propertiesToAttribute,
            bool createCell = true,
            bool useOriginalTypes = false)
        {
            var maxCellIndex = propertiesToAttribute.Max(i => i.Item2.Order);
            for (var i = 0; i <= maxCellIndex; i++)
            {
                ICell cell;
                if (createCell)
                    cell = row.CreateCell(i);
                else
                    cell = row.GetCell(i);

                if (cell == null)
                    cell = row.CreateCell(i);

                var current = propertiesToAttribute.FirstOrDefault(p => p.Item2.Order == i);
                if (current != null)
                {
                    if (useOriginalTypes)
                    {
                        if (current.Item1.PropertyType == typeof (int))
                        {
                            cell.SetCellValue(GetOriginalPropertyValue<TViewModel, int>(viewModel, current));
                        }
                        if (current.Item1.PropertyType == typeof (double))
                        {
                            cell.SetCellValue(GetOriginalPropertyValue<TViewModel, double>(viewModel, current));
                        }
                        if (current.Item1.PropertyType == typeof (bool))
                        {
                            cell.SetCellValue(GetOriginalPropertyValue<TViewModel, bool>(viewModel, current));
                        }
                        if (current.Item1.PropertyType == typeof (DateTime))
                        {
                            cell.SetCellValue(GetOriginalPropertyValue<TViewModel, DateTime>(viewModel, current));
                        }
                        if (current.Item1.PropertyType == typeof(string))
                        {
                            cell.SetCellValue(GetOriginalPropertyValue<TViewModel, string>(viewModel, current));
                        }
                    }
                    else
                    {
                        cell.SetCellValue(GetPropertyValue(viewModel, current.Item1, current.Item2.Format));
                    }
                }
            }
        }

        private static void WriteViewModelToRow<TViewModel>(TViewModel viewModel,
            IRow row,
            IList<ExcelColumnInfo> columnList)
        {
            for (var i = 0; i < columnList.Count; i++)
            {
                row.CreateCell(i).SetCellValue(GetPropertyValue(viewModel, columnList[i].Property, columnList[i].Format));
            }
        }

        public static TValue GetOriginalPropertyValue<TViewModel, TValue>(TViewModel viewModel, Tuple<PropertyInfo, ExcelSerializableAttribute> propertyToAttribute)
        {
            var property = propertyToAttribute.Item1;
            var excelAttribute = propertyToAttribute.Item2;

            var propertyValue = property.GetValue(viewModel, null);

            if (propertyValue == null)
            {
                return default(TValue);
            }

            return (TValue)propertyValue;
        }

        public static string GetPropertyValue<TViewModel>(TViewModel viewModel, 
            PropertyInfo property,
            string format)
        {
            var propertyValue = property.GetValue(viewModel, null);

            if (propertyValue == null)
            {
                return string.Empty;
            }

            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                return ((DateTime)propertyValue).ToString(String.IsNullOrEmpty(format) ? DateHelper.DateFormat : format);
            }

            if (property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
            {
                return ((float)propertyValue).ToString(format);
            }

            if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
            {
                return ((double)propertyValue).ToString(format);
            }

            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                return ((int)propertyValue).ToString(format);
            }

            if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                return (bool)propertyValue ? "Yes" : "No";
            }

            return propertyValue.ToString();
        }

        public static decimal? TryGetCellDecimal(ICell cell)
        {
            if (cell == null)
                return null;

            try
            {
                return (decimal)cell.NumericCellValue;
            }
            catch { }

            return StringHelper.TryGetDecimal(cell.ToString().Replace("$", ""));
        }

        #endregion
    }
}
