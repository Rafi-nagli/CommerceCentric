using Amazon.Common.ExcelExport;
using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO.CustomReports;
using Amazon.Model.Implementation;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Ccen.Common.Helpers;
using Ccen.Model.Implementation;
using Ccen.Web.ViewModels.CustomReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.CustomReports
{
    public class CustomReportFilterViewModel
    {
        public long Id { get; set; }
        public CustomReportFieldViewModel Field { get; set; }
        public long PredefinedFieldId { get; set; }
        public List<FilterOperationViewModel> AvailableOperations { get; set; }
        public List<string> AvailableOperationsString => AvailableOperations == null ? new List<string>() : AvailableOperations.Select(x => x.OperationString).ToList();
        public FilterOperation? Operation { get; set; }
        public string OperationString { get; set; }
        public object Value { get; set; }
        public string Header { get; set; }
        public bool Multi { get; set; }

        
        public IList<SelectListItemEx> Items { get; set; }
        private string _valueTitle;
        public string ValueTitle
        {
            get
            {
                return _valueTitle ?? ValueString;
            }
            set
            {
                _valueTitle = value;
            }
        }
        private string _valueString;
        public string ValueString
        { get; set; }

        public static List<CustomReportFilterViewModel> GetAllPredefinedFilters(IUnitOfWork db)
        {
            var predefinedList = db.CustomReportPredefinedFields.GetAllAsDto().ToList();
            var res = from a in predefinedList
                      select new CustomReportFilterViewModel()
                      {
                          PredefinedFieldId = a.Id,
                          Field = new CustomReportFieldViewModel(a),
                          AvailableOperations = CustomReportService.GetOperations(a.DataType).Select(x => new FilterOperationViewModel(x)).ToList(),
                          OperationString = new FilterOperationViewModel(CustomReportService.GetOperations(a.DataType).First()).OperationString,
                          Items = a.DataType == "feature" ? FeatureViewModel.GetFeatureValuesByName(db, a.Name) : new List<SelectListItemEx>()
                      };
            return res.ToList();
        }


        public static List<CustomReportFilterViewModel> GetUsedFiltersForReport(IUnitOfWork db, long reportId)
        {
            var query = from filter in db.CustomReportFilters.GetAllAsDto()

                        join predefinedField in db.CustomReportPredefinedFields.GetAllAsDto()
                        on filter.CustomReportPredefinedFieldId equals predefinedField.Id
                        where filter.CustomReportId == reportId
                        select new
                        {
                            Id = filter.Id,
                            PredefinedField = predefinedField,
                            Filter = filter,
                            //Field = field
                        };

            var res = query.ToList().Select(x => new CustomReportFilterViewModel()
            {
                Id = x.Filter.Id,
                Field = new CustomReportFieldViewModel(x.PredefinedField),
                AvailableOperations = CustomReportService.GetOperations(x.PredefinedField.DataType).Select(y => new FilterOperationViewModel(y)).ToList(),
                Operation = (FilterOperation)Enum.Parse(typeof(FilterOperation), x.Filter.Operation),
                Value = GetObjectFromString(x.PredefinedField.DataType, x.Filter.Value),
                PredefinedFieldId = x.PredefinedField.Id,
                OperationString = EnumHelper<FilterOperation>.GetEnumDescription(x.Filter.Operation.ToString()),
                Items = x.PredefinedField.DataType == "feature" ? FeatureViewModel.GetFeatureValuesByName(db, x.PredefinedField.Name) : new List<SelectListItemEx>(),
                ValueTitle = x.PredefinedField.DataType == "feature" ? db.FeatureValues.Get((int)GetObjectFromString(x.PredefinedField.DataType, x.Filter.Value)).Value : null
            });
            return res.ToList();
        }

        public void SetValueTitle(IUnitOfWork db)
        {
            try
            {
                var field = db.CustomReportPredefinedFields.Get(PredefinedFieldId);
                if (field.DataType != "feature")
                {
                    return;
                }
                ValueTitle = db.FeatureValues.Get((int)GetObjectFromString(field.DataType, ValueString)).Value;
            }
            catch (Exception)
            {
            }
        }

        public static List<CustomReportFilterViewModel> GetUsedFiltersForReport(IUnitOfWork db, long reportId, IList<string> tables)
        {
            if (reportId == 0)
            {
                return new List<CustomReportFilterViewModel>();
            }
            var res = GetUsedFiltersForReport(db, reportId).Where(x => tables.Contains(x.Field.EntityName)).ToList();
            return res;
        }

        private static object GetObjectFromString(string objectType, string value)
        {
            if (objectType == "feature")
            {
                return int.Parse(value);
            }
            if (objectType == "datetime")
            {
                return DateTime.Parse(value);
            }
            if (objectType == "decimal")
            {
                return Decimal.Parse(value);
            }

            if (objectType == "double")
            {
                return Double.Parse(value);
            }
            return value;
        }

        public static string GetDynamicWhereClause(List<CustomReportFilterViewModel> list, bool ignoreEntityName = false)
        {
            var offset = 0;
            var arr = list.Select(x => 
            {
                var index = list.IndexOf(x) + offset;
                var res = GetDynamicWhereClause(x, index, ref offset).Replace(ignoreEntityName ? x.Field.EntityName + "." : "rrrrrrr", "");
                return res;
            }).ToList();
            return String.Join(" && ", arr);
        }

        public static object[] GetDynamicObjectArray(List<CustomReportFilterViewModel> list)
        {
            var arrVals = list.Select(x => x.Operation != FilterOperation.ContainsAny ? new List<object>() { x.Value } : x.ValueString.Split(' ', ',', ';').Cast<object>().ToList()).SelectMany(x => x).ToArray();
            return arrVals;
        }

        public static List<CustomReportFilterViewModel> GetAvailableFiltersForReport(IUnitOfWork db, List<long> predefinedFieldsIds)
        {
            var all = GetAllPredefinedFilters(db);
            var res = all.Where(x => predefinedFieldsIds.Contains(x.Field.CustomReportPredefinedFieldId));
            return res.ToList();
        }

        private static List<FilterOperationViewModel> GetOperations(string type)
        {
            switch (type)
            {
                case "string":
                    return new List<FilterOperationViewModel>() { new FilterOperationViewModel(FilterOperation.Equals), 
                        new FilterOperationViewModel(FilterOperation.Contains), new FilterOperationViewModel(FilterOperation.NotEquals), 
                        new FilterOperationViewModel(FilterOperation.NotContains), new FilterOperationViewModel(FilterOperation.StartsWith), new FilterOperationViewModel(FilterOperation.EndsWith), new FilterOperationViewModel(FilterOperation.ContainsAny) };
                case "datetime":
                    return new List<FilterOperationViewModel>() { new FilterOperationViewModel(FilterOperation.Equals), new FilterOperationViewModel(FilterOperation.Greater),
                        new FilterOperationViewModel(FilterOperation.Less) };
                case "int":
                case "decimal":
                case "double":
                case "long":
                    return new List<FilterOperationViewModel>() { new FilterOperationViewModel(FilterOperation.Equals), new FilterOperationViewModel(FilterOperation.Greater),
                        new FilterOperationViewModel(FilterOperation.Less) };
            }
            return new List<FilterOperationViewModel>();
        }

        private static string GetDynamicWhereClause(CustomReportFilterViewModel filter, int index, ref int offset)
        {           

            switch(filter.Operation)
            {
                case
                    FilterOperation.ContainsAny:
                    //todo split into array
                    var arr = filter.ValueString.Split(' ', ',', ';').Select(x=>int.Parse(x)).ToList();
                    offset += arr.Count() - 1;
                    var resArr = arr.Select(x => String.Format("({0}.{1}.{2}(@{3}))", filter.Field.EntityName, filter.Field.ColumnName, FilterOperation.Equals, index + arr.IndexOf(x)));
                    return "(" + String.Join(" || ", resArr) + ")";
                case FilterOperation.Equals:
                case FilterOperation.Contains:
                case FilterOperation.StartsWith:
                case FilterOperation.EndsWith:
                    return String.Format("({0}.{1}.{2}(@{3}))", filter.Field.EntityName, filter.Field.ColumnName, filter.Operation, index); //tolower
                case FilterOperation.NotEquals:
                case FilterOperation.NotContains:
                    return String.Format("(!{0}.{1}.{2}(@{3}))", filter.Field.EntityName, filter.Field.ColumnName, filter.Operation.ToString().Replace("Not",""), index);
                case FilterOperation.Greater:
                    return String.Format("({0}.{1} > @{2})", filter.Field.EntityName, filter.Field.ColumnName, index);
                case FilterOperation.Less:
                    return String.Format("({0}.{1} < @{2})", filter.Field.EntityName, filter.Field.ColumnName, index);
            }

            throw new NotImplementedException("Operation not implemented");
        }
    }

}