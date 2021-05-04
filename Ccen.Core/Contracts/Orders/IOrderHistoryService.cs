using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IOrderHistoryService
    {
        void AddRecord(long orderId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by);

        void AddRecord(long orderId,
            string fieldName,
            object fromValue,
            string extendFromValue,
            object toValue,
            string extendToValue,
            long? by);
    }
}
