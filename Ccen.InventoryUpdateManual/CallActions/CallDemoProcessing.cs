using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DTO.Users;
using Amazon.Model.SyncService.Threads.Simple.Demo;


namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallDemoProcessing
    {
        private CompanyDTO _company;

        public CallDemoProcessing(CompanyDTO company)
        {
            _company = company;
        }

        public void CallUpdateTimeStamps()
        {
            var thread = new UpdateDemoTimeStampsThread(_company.Id, null, TimeSpan.MaxValue);
            thread.Run();
        }
    }
}
