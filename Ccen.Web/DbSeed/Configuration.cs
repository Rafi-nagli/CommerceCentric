using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Security;
using Amazon.Core.Entities;
using Amazon.DAL;

namespace Amazon.Web.DbSeed
{
    public sealed class Configuration : DbMigrationsConfiguration<AmazonContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(AmazonContext context)
        {
            base.Seed(context);
        }
    }
}