using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DAL;

namespace Amazon.Web.Models
{
    public class AppInfoHelper
    {
        public static string GetDatebaseName()
        {
            using (var db = new UnitOfWork(null))
            {
                return db.Context.Database.Connection.Database;
            }
        }
    }
}