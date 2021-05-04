using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amazon.DTO.CustomReports
{
    public class CustomReportPredefinedFieldDTO
    {
        public long Id { get; set; }
        public string EntityName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string Title { get; set; }
        public int? Width { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }

        public static List<string> AllCustomSearchFields => new List<string>() { "CurrentPrice", "Tag", "Weight", "Description", "SKU", "Barcode", "Title", "CreateDate", "Division", "Season", "Year", "Main Item style" };
    }

    public enum EditOperator
    {
        [Description("Increase by %")]
        IncreasePercents,
        [Description("Increase by % and round")]
        IncreasePercentsAndRound,
        [Description("Decrease by % and round")]
        DecreasePercentsAndRound,
        [Description("Decrease by %")]
        DecreasePersents,
        [Description("Change by fix amount")]
        IncreaseContstant,
        /*[Description("Decrease by ")]
        DecreaseConstant,*/
        [Description("Set to fixed value")]
        SetToConstantValue,
        [Description("Add text to end")]
        AppendString,
        [Description("Add text to start")]
        PrependString,
        [Description("Remove text from start")]
        TrimStart,
        [Description("Remove text from end")]
        TrimEnd,
        [Description("Find and replace")]
        ReplaceString,
        [Description("Cancel")]
        Cancel
    }
}
