using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.Web.ViewModels.Html;

namespace Amazon.Web.ViewModels
{
    public class AutoCompleteViewModel
    {
        public static IEnumerable<string> GetSearchTextList(IUnitOfWork db, string filter)
        {
            var orderNumber = OrderHelper.FormatDisplayOrderNumber(filter, MarketType.None);
            var byOrderId = db.Orders.GetAll().Where(o => (!String.IsNullOrEmpty(orderNumber) && o.AmazonIdentifier.StartsWith(orderNumber))
                                                           || o.AmazonIdentifier.StartsWith(filter))
                                                           .Select(o => o.AmazonIdentifier)
                                                           .Take(30)
                                                           .ToList();

            var byBuyerName = db.Orders.GetAll().Where(o => o.BuyerName.Contains(filter))
                .Select(o => o.BuyerName)
                .Take(30)
                .ToList();

            var byStyleId = db.Styles.GetAll().Where(o => o.StyleID.StartsWith(filter))
                .Select(s => s.StyleID)
                .Take(30)
                .ToList();

            

            var results = byOrderId;
            results.AddRange(byBuyerName);
            results.AddRange(byStyleId);

            return results;
        }

        public static IEnumerable<string> GetOrderIdList(IUnitOfWork db, string filter)
        {
            var orderNumber = OrderHelper.FormatDisplayOrderNumber(filter, MarketType.None);
            //var fromDate = DateTime.UtcNow.AddMonths(-3);
            return db.Orders.GetAll()
                .Where(o => //o.CreateDate > fromDate &&
                    (o.AmazonIdentifier.StartsWith(orderNumber)
                        || o.AmazonIdentifier.StartsWith(filter)))
                .Select(o => o.AmazonIdentifier)
                .Take(100)
                .ToList();
        }

        public static IEnumerable<string> GetStyleGroupNameList(IUnitOfWork db, string filter)
        {
            var name = StringHelper.TrimWhitespace(filter);
            //var fromDate = DateTime.UtcNow.AddMonths(-3);
            return db.StyleGroups.GetAll()
                .Where(o => //o.CreateDate > fromDate &&
                    o.Name.StartsWith(name))
                .OrderByDescending(o => o.CreateDate)
                .Select(o => o.Name)
                .Take(100)
                .ToList();
        }

        public static IEnumerable<string> GetSizeList(IUnitOfWork db, string filter)
        {
            return db.Sizes.GetAll()
                .Where(s => s.Name.StartsWith(filter))
                .Select(s => s.Name)
                .OrderBy(s => s)
                .ToList();
        }

        public static IEnumerable<SizeViewModel> GetSizeListByGroup(IUnitOfWork db)
        {
            var query = from s in db.Sizes.GetAll()
                 join sg in db.SizeGroups.GetAll() on s.SizeGroupId equals sg.Id
                 orderby sg.SortOrder ascending, sg.Name ascending,
                        s.SortOrder ascending, s.Name ascending 
                 select new SizeViewModel()
                 {
                     Id = s.Id,
                     Name = s.Name,
                     GroupName = sg.Name
                 };
            return query.ToList();
        }

        public static IEnumerable<string> GetColorList(IUnitOfWork db, string filter)
        {
            var query = (from i in db.Items.GetAllViewAsDto()
                         where  !String.IsNullOrEmpty(i.Color)
                        select i.Color
                        ).Distinct();

            return query.ToList();
        }

        public static IEnumerable<string> GetStyleStringList(IUnitOfWork db, string filter)
        {
            return db.Styles.GetAll()
                .Where(s => s.StyleID.Contains(filter)
                    && !s.Deleted)
                .Select(s => s.StyleID)
                .OrderBy(s => s)
                .Take(100)
                .ToList();
        }
    }
}