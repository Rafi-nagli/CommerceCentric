using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateChartInfoThread : TimerThreadBase
    {
        public UpdateChartInfoThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("UpdateChartInfo", companyId, messageService, callTimeStamps, time)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var maker = new ListingErrorChartMaker(dbFactory, time, log);
            maker.AddPoints();
        }
    }
}
