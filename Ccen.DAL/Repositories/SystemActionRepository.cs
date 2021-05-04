using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class SystemActionRepository : Repository<SystemAction>, ISystemActionRepository
    {
        public SystemActionRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IQueryable<SystemActionDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public IQueryable<SystemActionDTO> GetAllAsDtoWithUser()
        {
            var query = from a in GetAll()
                        join u in unitOfWork.GetSet<User>() on a.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        select new SystemActionDTO()
                        {
                            Id = a.Id,
                            ParentId = a.ParentId,
                            GroupId = a.GroupId,
                            Type = a.Type,
                            Tag = a.Tag,
                            InputData = a.InputData,
                            OutputData = a.OutputData,
                            Status = a.Status,
                            AttemptDate = a.AttemptDate,
                            AttemptNumber = a.AttemptNumber,
                            CreateDate = a.CreateDate,
                            CreatedBy = a.CreatedBy,
                            CreatedByName = u.Name,
                        };
            return query;
        }

        public bool IsExist(string tag, SystemActionType type)
        {
            var existItem = GetAllAsDto().FirstOrDefault(a => a.Tag == tag || a.Type == (int) type);
            return existItem != null;
        }

        public void AddAction(SystemActionDTO action)
        {
            var dbAction = new SystemAction()
            {
                ParentId = action.ParentId,

                Type = action.Type,
                Tag = action.Tag,
                InputData = action.InputData,
                Status = action.Status,

                CreateDate = action.CreateDate,
                CreatedBy = action.CreatedBy,
            };
            unitOfWork.GetSet<SystemAction>().Add(dbAction);
            unitOfWork.Commit();

            action.Id = dbAction.Id;
        }

        private IQueryable<SystemActionDTO> AsDto(IQueryable<SystemAction> query)
        {
            return query.Select(a => new SystemActionDTO()
            {
                Id = a.Id,
                ParentId = a.ParentId,
                GroupId = a.GroupId,
                Type = a.Type,
                Tag = a.Tag,
                InputData = a.InputData,
                OutputData = a.OutputData,
                Status = a.Status,
                AttemptDate = a.AttemptDate,
                AttemptNumber = a.AttemptNumber,
                CreateDate = a.CreateDate,
                CreatedBy = a.CreatedBy
            });
        }
    }
}
