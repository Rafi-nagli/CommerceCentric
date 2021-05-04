using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts.Orders
{
    public interface ILabelBatchService
    {
        PrintLabelResult PrintLabel(long orderId, 
            long companyId, 
            long? by);
    }
}
