using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface ISystemActionRepository : IRepository<SystemAction>
    {
        
        IQueryable<SystemActionDTO> GetAllAsDto();
        IQueryable<SystemActionDTO> GetAllAsDtoWithUser();

        void AddAction(SystemActionDTO action);
        bool IsExist(string tag, SystemActionType type);
    }
}
