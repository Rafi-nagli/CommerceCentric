using System;

namespace Amazon.Core.Contracts.DbInventory
{
    public interface IInventoryUnitOfWork : IDisposable
    {
        IItemOrderMappingRepository ItemOrderMappings { get; }
        IOrderRepository Orders { get; }

        void Commit();
        void CommitAndRefreshChanges();
        void RollbackChanges();
    }
}
