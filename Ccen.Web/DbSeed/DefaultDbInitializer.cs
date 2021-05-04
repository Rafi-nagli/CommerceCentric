using System.Data.Entity;
using Amazon.DAL;

namespace Amazon.Web.DbSeed
{
    public class DefaultDbInitializer
    {
        public static void Init()
        {
            Database.SetInitializer(new DbInitializer());
        }

        private class DbInitializer : MigrateDatabaseToLatestVersion<AmazonContext, Configuration>
        {
        }
    }
}