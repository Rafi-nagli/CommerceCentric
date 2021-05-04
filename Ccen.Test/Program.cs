
using Amazon.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbFactory = new DbFactory();

            using (var db = dbFactory.GetRDb())
            {
                


                var res = Ccen.Web.ViewModels.CustomReports.CustomReportDataItemViewModel.GetAllReportDataItemsViewModelsDynamic(db, 10002);
                Type elementType = res.ElementType;
                var props = elementType.GetProperties();
                foreach (var it in res)
                {
                    Console.WriteLine(it.ToString());
                }
            }
        }
    }
}
