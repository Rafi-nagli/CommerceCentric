using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.DAL;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class BatchArchiveThread : TimerThreadBase
    {
        public BatchArchiveThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("BatchArchive", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();

            var log = GetLogger();

            using (var db = dbFactory.GetRWDb())
            {
                var batches = db.OrderBatches.GetBatchesToDisplay(false)
                    .ToList()
                    .Where(b => b.CanArchive)
                    .OrderByDescending(b => b.CreateDate)
                    .ToList()
                    .Skip(1)
                    .ToList();

                var batchesToArchiveIdList = batches.Select(b => b.Id).ToList();
                var batchesToArchive = db.OrderBatches
                    .GetAll()
                    .Where(b => batchesToArchiveIdList.Contains(b.Id))
                    .ToList();

                foreach (var batch in batchesToArchive)
                {
                    log.Info("Archive batch, batchId=" + batch.Id);
                    batch.Archive = true;
                }

                db.Commit();
            }
        }
    }
}
