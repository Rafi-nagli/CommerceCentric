using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory.SizeHistories
{
    public interface IHistoryRecord
    {
        string Reason { get; }
       
        string EntityType { get; }
        string EntityName { get; }

        string FromValue { get; }
        string ToValue { get; }

        DateTime When { get; }
        string ByName { get; }

        void Prepare();
    }
}