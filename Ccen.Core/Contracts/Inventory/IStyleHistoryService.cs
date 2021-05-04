using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IStyleHistoryService
    {
        void AddRecord(long styleId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by);

        void AddRecord(long styleId,
            string fieldName,
            object fromValue,
            string extendFromValue,
            object toValue,
            string extendToValue,
            long? by);
    }
}
