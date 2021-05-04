using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Model.Implementation.Sync
{
    public interface IItemsSync
    {
        void SendItemUpdates();
        void ReadItems(DateTime? lastSync);
        void SendInventoryUpdates();
        void SendPriceUpdates();
    }
}
