using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface ICacheService
    {
        void RequestStyleIdUpdates(IUnitOfWork db,
            IList<long> styleIdList,
            UpdateCacheMode updateMode,
            long? by);
        
        void RequestStyleItemIdUpdates(IUnitOfWork db,
            IList<long> styleItemIdList,
            long? by);

        void RequestParentItemIdUpdates(IUnitOfWork db,
            IList<long> parentItemIdList,
            UpdateCacheMode updateMode,
            long? by);

        void RequestItemIdUpdates(IUnitOfWork db,
            IList<long> itemIdList,
            long? by);

        bool UpdateDbCacheUsingSettings(IUnitOfWork db, ISettingsService settings);
        void UpdateDbCache(IUnitOfWork db);


        void UpdateStyleCacheForStyleId(IUnitOfWork db, IList<long> styleIdList);
        void UpdateStyleItemCacheForStyleId(IUnitOfWork db, IList<long> styleIdList);
    }
}
