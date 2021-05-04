using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models.Histories.HistoryDatas;

namespace Amazon.Core.Contracts
{
    public interface IStyleItemHistoryService
    {
        void AddRecord(string actionName,
            long styleItemId,
            string data,
            long? by);

        void AddRecord(string actionName,
            long styleItemId,
            IHistoryData data,
            long? by);
    }
}
