using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface ISystemActionService
    {
        IList<SystemActionDTO> GetUnprocessedByType(IUnitOfWork db, 
            SystemActionType type,
            DateTime? maxLastAttemptDate,
            string tag);

        IList<SystemActionDTO> GetInProgressByType(IUnitOfWork db,
            SystemActionType type,
            DateTime? withAttemptDateAfterThis);

        

        IList<SystemActionDTO> GetByTypeAndGroupId(IUnitOfWork db,
            SystemActionType type,
            string groupId);

        IList<SystemActionDTO> GetByTypeAndTag(IUnitOfWork db,
            SystemActionType type,
            string tag);

        long AddAction(IUnitOfWork db, 
            SystemActionType type, 
            string tag,
            ISystemActionInput input, 
            long? parentActionId,
            long? by,
            SystemActionStatus? status = SystemActionStatus.None);

        void SetResult(IUnitOfWork db, 
            long actionId, 
            SystemActionStatus status, 
            ISystemActionOutput output,
            string groupId = null);
    }
}
