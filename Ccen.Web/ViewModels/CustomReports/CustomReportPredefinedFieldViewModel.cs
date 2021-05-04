using Amazon.Common.ExcelExport;
using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.CustomReports
{
    public class CustomReportPredefinedFieldViewModel
    {
        public long Id { get; set; }
        public string ColumnName { get; set; }
        public string Title { get; set; }
        public string EntityName { get; set; }
        public string FullName { get; set; }


        public override string ToString()
        {
            return $"{EntityName}_{ColumnName}";
        }

        public override bool Equals(object obj)
        {
            return ((CustomReportPredefinedFieldViewModel)obj).ColumnName == ColumnName && ((CustomReportPredefinedFieldViewModel)obj).EntityName == EntityName;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static List<CustomReportPredefinedFieldViewModel> GetAll(IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {                
                return GetAll(db);
            }            
        }

        public static List<CustomReportPredefinedFieldViewModel> GetAll(IUnitOfWork db)
        {            
            var query = from a in db.CustomReportPredefinedFields.GetAllAsDto()
                        select new CustomReportPredefinedFieldViewModel()
                        {
                            Id = a.Id,
                            ColumnName = a.ColumnName,
                            EntityName = a.EntityName,
                            FullName = a.Name,
                            Title = a.Title
                        };
            return query.ToList();            
        }

        public static List<CustomReportFieldViewModel> GetAllConverted(IUnitOfWork db)
        {
            var query = from a in db.CustomReportPredefinedFields.GetAllAsDto()
                        select new CustomReportFieldViewModel()
                        {
                            CustomReportPredefinedFieldId = a.Id,
                            ColumnName = a.ColumnName,
                            EntityName = a.EntityName,
                            FullName = a.Name,
                            Title = a.Title
                        };
            return query.ToList();
        }



        public static List<CustomReportFieldViewModel> GetAvailable(List<CustomReportFieldViewModel> usedFields, IUnitOfWork db)
        {
            var ids = usedFields.Select(x => x.CustomReportPredefinedFieldId).ToList();
            return GetAll(db).Where(x => !ids.Contains(x.Id)).Select(x => new CustomReportFieldViewModel() 
            {
                ColumnName = x.ColumnName,
                CustomReportPredefinedFieldId = x.Id, 
                Title = x.Title
            }).ToList();
        }
        
    }
}