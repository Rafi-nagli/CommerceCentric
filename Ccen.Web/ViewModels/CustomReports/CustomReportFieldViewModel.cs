using Amazon.Common.ExcelExport;
using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.CustomReports;
using Ccen.Web.ViewModels.CustomReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.CustomReports
{
    public class CustomReportFieldViewModel
    {
        public long Id { get; set; }
        public string ColumnName { get; set; }
        public string Title { get; set; }
        public string DataType { get; set; }
        public int SortOrder { get; set; }
        public long CustomReportPredefinedFieldId { get; set; }
        public string EntityName { get; set; }
        public int Width { get; set; }
        public string FullName { get; set; }
        public string Format { get; set; }
        public DisplayTypeEnum DisplayType { get; set; }  
        public int Order { get; set; }


        public static List<CustomReportFieldViewModel> GetCustomReportFields(long reportId, IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {
                return GetCustomReportFields(reportId, db);
            }            
        }

        public static List<CustomReportFieldViewModel> GetCustomReportFields(long reportId, IUnitOfWork db)
        {
            //var dict = db.CustomReportPredefinedFields.GetAllAsDto().ToDictionary(x => x.Id, x => x.Name);
            var query = from f in db.CustomReportFields.GetAllAsDto()
                        join pf in db.CustomReportPredefinedFields.GetAllAsDto()
                        on f.CustomReportPredefinedFieldId equals pf.Id
                        where f.CustomReportId == reportId
                        orderby f.SortOrder
                        select new CustomReportFieldViewModel()
                        {
                            Id = f.Id,
                            CustomReportPredefinedFieldId = f.CustomReportPredefinedFieldId,
                            ColumnName = pf.ColumnName,
                            EntityName = pf.EntityName,
                            SortOrder = f.SortOrder,
                            Title = pf.Title,
                            Width = pf.Width.HasValue ? pf.Width.Value : 100,
                            FullName = pf.Name,
                            DataType = pf.DataType,
                            Format = pf.DataType == "datetime" ? "{0:MM.dd.yyyy HH:mm}" : "",
                        };
            return query.ToList();
        }

        public static string GetDynamicReportSelectString(long reportId, IUnitOfWork db)
        {
            var list = GetCustomReportFields(reportId, db);
            var strlist = list.Select(x => GetSelectString(x)).ToList();
            var content = String.Join(", ", strlist);
            return String.Format("new ({0})", content);
        }

        private static string GetSelectString(CustomReportFieldViewModel model)
        {
            return $"{model.EntityName}.{model.ColumnName} as {model.FullName}";
        }

        public CustomReportFieldViewModel()
        {

        }

        public CustomReportFieldViewModel(CustomReportPredefinedFieldDTO  f, long fieldId = 0)
        {
            Id = fieldId;
            CustomReportPredefinedFieldId = f.Id;
            ColumnName = f.ColumnName;
            EntityName = f.EntityName;            
            Title = f.Title;
            Width = f.Width.HasValue ? f.Width.Value : 100;
            FullName = f.Name;
            DataType = f.DataType;
            Format = f.DataType == "datetime" ? "{0:MM.dd.yyyy HH:mm}" : "";
        }


        public override string ToString()
        {
            return $"{EntityName}_{ColumnName}";
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomReportPredefinedFieldViewModel)
            {
                return ((CustomReportPredefinedFieldViewModel)obj).ColumnName == ColumnName && ((CustomReportPredefinedFieldViewModel)obj).EntityName == EntityName;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /*public static IList<CustomReportFieldViewModel> Build(IList<CustomReportFieldDTO> fields)
        {
            return fields.Select(f => new CustomReportFieldViewModel(f)).ToList();
        }

        public CustomReportFieldViewModel(CustomReportFieldDTO field, string fieldName)
        {
            Id = field.Id;
            FieldName = field.;
            CustomFieldName = field.CustomFieldName;
            CustomFieldValue = field.CustomFieldValue;
            SortOrder = field.SortOrder;
        }*/
    }    
}