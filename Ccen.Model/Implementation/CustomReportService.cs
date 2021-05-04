using Amazon.Core;
using Amazon.DTO.CustomReports;
using Ccen.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Model.Implementation
{
    public class CustomReportService
    {
        public static List<CustomReportFilterDTO> GetAllPredefinedFilters(IUnitOfWork db)
        {
            var predefinedList = db.CustomReportPredefinedFields.GetAllAsDto().ToList();
            var res = from a in predefinedList
                      select new CustomReportFilterDTO()
                      {
                          CustomReportPredefinedField = a,
                          CustomReportPredefinedFieldId = a.Id,
                          //Field = new CustomReportFieldViewModel(a),
                          AvailableOperations = GetOperations(a.DataType),
                          //OperationString = GetOperations(a.DataType).First().OperationString,
                      };
            return res.ToList();
        }
        public static List<CustomReportFilterDTO> GetUsedFiltersForReport(IUnitOfWork db, long reportId)
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

            var res = query.ToList().Select(x => new CustomReportFilterDTO()
            {
                Id = x.Filter.Id,
                CustomReportPredefinedField = x.PredefinedField,
                //AvailableOperations = GetOperations(x.PredefinedField.DataType),
                Operation = x.Filter.Operation,
                ValueObject = SystemTypeHelper.GetObjectByString(x.Filter.Value, x.PredefinedField.DataType),
                //OperationString = EnumHelper<FilterOperation>.GetEnumDescription(x.Filter.Operation.ToString())
            });
            return res.ToList();
        }

        public static List<CustomReportFilterDTO> GetUsedFiltersForReport(IUnitOfWork db, long reportId, IList<string> tables)
        {
            if (reportId == 0)
            {
                return new List<CustomReportFilterDTO>();
            }
            var res = GetUsedFiltersForReport(db, reportId).Where(x => tables.Contains(x.CustomReportPredefinedField.EntityName)).ToList();
            return res;
        }

        public static string GetDynamicWhereClause(List<CustomReportFilterDTO> list, bool ignoreEntityName = false)
        {
            var offset = 0;
            var arr = list.Select(x =>
            {
                var index = list.IndexOf(x) + offset;
                var res = GetDynamicWhereClause(x, x.CustomReportPredefinedField, index, ref offset).Replace(ignoreEntityName ? x.CustomReportPredefinedField.EntityName + "." : "rrrrrrr", "");
                return res;
            }).ToList();
            return String.Join(" && ", arr);
        }

        public static object[] GetDynamicObjectArray(List<CustomReportFilterDTO> list)
        {
            var arrVals = list.Select(x => x.Operation != FilterOperation.ContainsAny.ToString() ? new List<object>() { x.ValueObject } : x.Value.Split(' ', ',', ';').Cast<object>().ToList()).SelectMany(x => x).ToArray();
            return arrVals;
        }

        public static List<CustomReportFilterDTO> GetAvailableFiltersForReport(IUnitOfWork db, List<long> predefinedFieldsIds)
        {
            var all = GetAllPredefinedFilters(db);
            var res = all.Where(x => predefinedFieldsIds.Contains(x.CustomReportPredefinedField.Id));
            return res.ToList();
        }

        public static List<FilterOperation> GetOperations(string type)
        {
            switch (type)
            {
                case "feature":
                    return new List<FilterOperation>() { FilterOperation.Equals };
                case "string":
                    return new List<FilterOperation>() { FilterOperation.Equals,
                        FilterOperation.Contains, FilterOperation.NotEquals,
                        FilterOperation.NotContains, FilterOperation.StartsWith, FilterOperation.EndsWith, FilterOperation.ContainsAny };
                case "datetime":
                    return new List<FilterOperation>() {FilterOperation.Greater,
                        FilterOperation.Less };
                case "int":
                case "decimal":
                case "double":
                case "long":
                    return new List<FilterOperation>() { FilterOperation.Equals, FilterOperation.Greater,
                        FilterOperation.Less };
            }
            return new List<FilterOperation>();
        }

        private static string GetDynamicWhereClause(CustomReportFilterDTO filter, CustomReportPredefinedFieldDTO field, int index, ref int offset)
        {
            var operation = (FilterOperation)Enum.Parse(typeof(FilterOperation), filter.Operation);
            switch (operation)
            {
                case
                    FilterOperation.ContainsAny:
                    //todo split into array
                    var arr = filter.Value.Split(' ', ',', ';').ToList();
                    offset += arr.Count() - 1;
                    var resArr = arr.Select(x => String.Format("({0}.{1}.{2}(@{3}))", field.EntityName, field.ColumnName, FilterOperation.Contains, index + arr.IndexOf(x)));
                    return "(" + String.Join(" || ", resArr) + ")";
                case FilterOperation.Equals:
                    return String.Format("({0}.{1} == (@{2}))", field.EntityName, field.ColumnName, index);
                case FilterOperation.Contains:
                case FilterOperation.StartsWith:
                case FilterOperation.EndsWith:
                    return String.Format("({0}.{1}.{2}(@{3}))", field.EntityName, field.ColumnName, filter.Operation, index); //tolower
                case FilterOperation.NotEquals:
                case FilterOperation.NotContains:
                    return String.Format("(!{0}.{1}.{2}(@{3}))", field.EntityName, field.ColumnName, filter.Operation.ToString().Replace("Not", ""), index);
                case FilterOperation.Greater:
                    return String.Format("({0}.{1} > @{2})", field.EntityName, field.ColumnName, index);
                case FilterOperation.Less:
                    return String.Format("({0}.{1} < @{2})", field.EntityName, field.ColumnName, index);
            }

            throw new NotImplementedException("Operation not implemented");
        }
    }
}
