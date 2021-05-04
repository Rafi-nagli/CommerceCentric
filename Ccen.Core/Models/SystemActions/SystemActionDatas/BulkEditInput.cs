using Amazon.Core.Contracts.SystemActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Models.SystemActions.SystemActionDatas
{
    public class BulkEditInput : ISystemActionInput
    {
        public long BulkEditOperationId { get; set; }
    }
}
