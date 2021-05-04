using System;
using System.Collections.Generic;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts
{
    public interface ISyncInformer
    {
        void SyncBegin(int? countToProcess);
        void OpenLastSync();
        void SyncEnd();
        bool IsSyncInProgress();
        void PingSync();

        void AddEntities(SyncMessageStatus status, IList<string> entities, string message);
        void AddError(string entityId, string message, Exception ex);
        void AddWarning(string entityId, string message);
        void AddInfo(string entityId, string message);
        void AddSuccess(string entityId, string message);
        void UpdateProgress(string entityId);
    }
}
