using Amazon.Common.ExcelExport;
using Amazon.Core;
using Amazon.Core.Exports.Attributes;
using Amazon.DTO.CustomReports;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.ViewModels.CustomReports;
using Amazon.Web.ViewModels.Html;
using Ccen.Common.Helpers;
using Ccen.Model.Implementation;
using Ccen.Web.ViewModels.CustomReportView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Ccen.Web.ViewModels.CustomReports
{
    public abstract class CustomReportBaseDataViewModel
    {        
        public virtual List<CustomReportFilterViewModel> GetAllAvailableFilters(IUnitOfWork db)
        {
            var allProps = this.GetType().GetProperties().Where(x => x.GetCustomAttributes<CustomFilterAttribute>().Any());
            var allAttributes = allProps.Select(x => new
            {
                Name = x.Name,
                GridAtr = x.GetCustomAttribute<DisplayInGridAttribute>(),
                FieldAtr = x.GetCustomAttribute<CustomReportFieldAttribute>(),
                FilterItems = x.GetCustomAttributes<CustomFilterAttribute>()
            }).ToList();

            var atrsRes = allAttributes.SelectMany(x => x.FilterItems.Select(y => new { y, FieldAtr = x.FieldAtr ?? new CustomReportFieldAttribute() { DataType = "string" }, x.GridAtr, x.Name })).ToList();

            return atrsRes.Select(x =>  new CustomReportFilterViewModel
            {
                Id = x.y.Id,
                AvailableOperations = GetOperations(x.FieldAtr),                
                Field = GetField(x.Name, x.GridAtr, x.FieldAtr),
                Operation = x.y.Operation,
                OperationString = x.y.Operation.ToString(),
                ValueString = GetValueString(x.y.ValueString, x.FieldAtr.DataType),
                Value = GetValueObject(x.y.ValueString, x.FieldAtr.DataType, x.FieldAtr.Multi),
                Header = x.y.Header,
                Items = GetItemsList(db, x.FieldAtr),
                Multi = x.FieldAtr.Multi
            }).ToList();
        }

        private static List<SelectListItemEx> GetItemsList(IUnitOfWork db, CustomReportFieldAttribute atr)
        {
            if (atr.GetItemsFromDbFunc != null)
            {
                return atr.GetItemsFromDbFunc(db);
            }
            return new List<SelectListItemEx>();
        }

        private string GetValueString(string val, string dataType)
        {
            if (String.IsNullOrEmpty(val))
            {
                return "";
            }
            return GetValueObject(val, dataType).ToString();
        }

        private object GetValueObject(string val, string dataType, bool multi = false)
        {
            int days = 0;
            if (multi)
            {
                return String.IsNullOrEmpty(val) ? new List<int>() : val.Split(',').Select(x=>int.Parse(x)).ToList();
            }
            if (dataType == "datetime" && int.TryParse(val, out days))
            {
                return DateTime.Now.AddDays(days).Date;
            }
            return SystemTypeHelper.GetObjectByString(val, dataType);
        }

        public virtual List<CustomReportFieldViewModel> GetAllAvailableFields()
        {
            var allProps = this.GetType().GetProperties().Where(x => x.GetCustomAttribute<DisplayInGridAttribute>() != null);
            var allAttributes = allProps.Select(x => new
            {
                Name = x.Name,
                GridAtr = x.GetCustomAttribute<DisplayInGridAttribute>(),
                FieldAtr = x.GetCustomAttribute<CustomReportFieldAttribute>()                
            }).ToList();          

            return allAttributes.Select(x => new CustomReportFieldViewModel
            {
                //ColumnName = x.ExcelAtr.ColumnName,
                //EntityName = x.ExcelAtr.EntityName,
                Title = x.GridAtr.Title ?? x.FieldAtr.Title,
                Width = x.GridAtr.Width,
                FullName = x.Name,
                DataType = x.FieldAtr == null ? "string" : x.FieldAtr.DataType,
                Format = x.FieldAtr != null && x.FieldAtr.DataType == "datetime" ? "{0:MM.dd.yyyy HH:mm}" : "",
                DisplayType = x.GridAtr.DisplayType,
                Order = x.GridAtr.Order
            }).OrderBy(x => x.Order).ToList();
        }

        private static CustomReportFieldViewModel GetField(string name, DisplayInGridAttribute atr, CustomReportFieldAttribute exc)
        {
            var dto = new CustomReportPredefinedFieldDTO() { EntityName = exc.EntityName ?? "", ColumnName = exc.ColumnName ?? "", DataType = exc.DataType, Width = atr == null ? exc.Width : atr.Width, Title = atr == null ? exc.Title : atr.Title, Name = name };

            var res = new CustomReportFieldViewModel(dto);
            return res;
        }

        private static List<FilterOperationViewModel> GetOperations(CustomReportFieldAttribute atr)
        {            
            var operations = CustomReportService.GetOperations(atr.DataType);
            return operations.Select(x => new FilterOperationViewModel(x)).ToList();
        }

        protected MemoryStream ExcelExport<T>(IEnumerable<T> list) where T : CustomReportBaseDataViewModel
        { 
            var res = list.OfType<T>().ToList();
            return ExcelHelper.Export<T>(res, null);
        } 

        public abstract string SortField { get; }
        public abstract ListSortDirection SortOrder { get; }        
        public long Id { get; set; }
        public abstract IQueryable GetAll(IUnitOfWork db, List<CustomReportFilterViewModel> filters);
        public abstract IEnumerable GetTop1000(IUnitOfWork db, List<CustomReportFilterViewModel> filters);
    } 
}