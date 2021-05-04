using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.CustomReports;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Models.Calls;
using Amazon.DTO.CustomFeeds;
using Amazon.DTO.CustomReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.CustomReports
{
    public class CustomReportViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool InMenu { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }   
        public IEnumerable<CustomReportFieldViewModel> UsedFields { get; set; }
        public IEnumerable<CustomReportFieldViewModel> AvailablePredefinedFields { get; set; }
        public IEnumerable<CustomReportFilterViewModel> UsedFilters { get; set; }
        public IEnumerable<CustomReportFilterViewModel> AvailableFilters { get; set; }
        public string FieldsString
        {
            get => UsedFields == null ? "" : String.Join(", ", UsedFields.OrderBy(x => x.SortOrder).Select(x => x.Title).ToList());
        }

        public static List<CustomReportViewModel> GetAll(IDbFactory dbFactory)
        {            
            using (var db = dbFactory.GetRWDb())
            {
                var reportFields = from f in db.CustomReportFields.GetAllAsDto()
                                   join pf in db.CustomReportPredefinedFields.GetAllAsDto()
                                   on f.CustomReportPredefinedFieldId equals pf.Id
                                   group new { f, pf } by f.CustomReportId into g                                   
                                   select new { Id = g.Key, Fields = g };
                var query = from a in db.CustomReports.GetAllAsDto()
                            join u in db.Users.GetAllAsDto()
                            on a.CreatedBy equals u.Id
                            join f in reportFields
                            on a.Id equals f.Id
                            select new CustomReportViewModel()
                            {
                                Id = a.Id,
                                Name = a.Name,
                                CreatedBy = u.Name,
                                CreateDate = a.CreateDate,
                                UsedFields = f.Fields.Select(x => new CustomReportFieldViewModel()
                                {
                                    Id = x.f.Id,
                                    DataType = x.pf.DataType,
                                    Title = x.pf.Title,
                                    SortOrder = x.f.SortOrder,
                                    ColumnName = x.pf.ColumnName
                                }) 
                            };
                return query.ToList();
            }            
        }

        public static CustomReportViewModel GetById(IUnitOfWork db, long id)
        {
            var a = db.CustomReports.Get(id);
            var fields = db.CustomReportFields.GetAllAsDto().Where(x => x.CustomReportId == id);
            var predefinedFields = db.CustomReportPredefinedFields.GetAllAsDto();
            return new CustomReportViewModel()
            {
                Id = a.Id,
                Name = a.Name,               
                CreateDate = a.CreateDate                
            };            
        }

        public CustomReportViewModel()
        {

        }

        public CustomReportViewModel(IUnitOfWork db,            
            long? id)
        {
            if (id.HasValue)
            {
                var report = db.CustomReports.Get(id.Value);
                Id = report.Id;
                Name = report.Name;
                UsedFields = CustomReportFieldViewModel.GetCustomReportFields(report.Id, db);
                UsedFilters = CustomReportFilterViewModel.GetUsedFiltersForReport(db, report.Id);
                AvailablePredefinedFields = CustomReportPredefinedFieldViewModel.GetAvailable(UsedFields.ToList(), db);
            }
            else
            {
                Id = 0;
                Name = "New";
                UsedFields = new List<CustomReportFieldViewModel>();
                UsedFilters = new List<CustomReportFilterViewModel>();
                AvailablePredefinedFields = CustomReportPredefinedFieldViewModel.GetAllConverted(db);                
            }
        }

        public static CallMessagesResultVoid Apply(IUnitOfWork db, CustomReportViewModel model, DateTime when, long? by)
        {            
            CustomReport report = null;
            if (model.Id > 0)
            {
                report = db.CustomReports.Get(model.Id);
            }
            else
            {
                report = new CustomReport();
                report.CreateDate = when;
                report.CreatedBy = by;                
                db.CustomReports.Add(report);
            }
            report.Name = model.Name;           
            db.Commit();

            var oldFields = db.CustomReportFields.GetAllAsDto().Where(x => x.CustomReportId == report.Id).ToList();

            IList<CustomReportFieldDTO> fields = new List<CustomReportFieldDTO>();
            var items = model.UsedFields.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                fields.Add(new CustomReportFieldDTO()
                {
                    CustomReportId = report.Id,
                    Id = item.Id,
                    CustomReportPredefinedFieldId = item.CustomReportPredefinedFieldId,                    
                    SortOrder = i + 1,
                });
            }
            db.CustomReportFields.BulkUpdateForFeed(report.Id, fields, when, by);
            db.Commit();


            var filters = model.UsedFilters == null ? new List<CustomReportFilterDTO>() : model.UsedFilters.Select(x => new CustomReportFilterDTO()
            {
                Id = x.Id,
                Operation = x.OperationString,
                Value = x.ValueString,
                CustomReportPredefinedFieldId = x.PredefinedFieldId,
                CustomReportId = report.Id
            }).ToList();
            db.CustomReportFilters.BulkUpdateForFilter(report.Id, filters, when, by);
            db.Commit();
            return new CallMessagesResultVoid();
        }


        public static void Delete(IDbFactory dbFactory, long id)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var customFeed = db.CustomReports.Get(id);
                db.CustomReports.Remove(customFeed);
                db.Commit();
            }
        }       

        public IList<MessageString> Validate()
        {
            return new List<MessageString>();
        }

        public DateTime FormattedCreateDate
        {
            get { return DateHelper.ConvertUtcToApp(CreateDate); }
        }
    }
}