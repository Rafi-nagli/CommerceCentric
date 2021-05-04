using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation
{
    public class SystemActionService : ISystemActionService
    {
        private ILogService _logService;
        private ITime _time;

        public SystemActionService(ILogService logService,
            ITime time)
        {
            _logService = logService;
            _time = time;
        }

        public IList<SystemActionDTO> GetUnprocessedByType(IUnitOfWork db, 
            SystemActionType type,
            DateTime? maxLastAttemptDate,
            string tag)
        {
            var query = from a in db.SystemActions.GetAllAsDto()
                join parent in db.SystemActions.GetAllAsDto() on a.ParentId equals parent.Id into withParent
                from parent in withParent.DefaultIfEmpty()
                where a.Status == (int) SystemActionStatus.None
                      && a.Type == (int) type
                      && (!a.ParentId.HasValue || parent.Status == (int) SystemActionStatus.Done)
                orderby a.CreateDate ascending 
                select a;

            if (!String.IsNullOrEmpty(tag))
            {
                query = query.Where(a => a.Tag == tag);
            }
            else
            {
                if (maxLastAttemptDate.HasValue)
                {
                    query = query.Where(a => !a.AttemptDate.HasValue
                                             || a.AttemptDate.Value <= maxLastAttemptDate);
                }
            }

            return query.ToList();

            //return db.SystemActions.GetAllAsDto().Where(a => a.Type == (int)type 
            //    && (a.Status == (int)SystemActionStatus.None
            //    //|| a.Status == (int)SystemActionStatus.Fail
            //    ))
            //    .OrderBy(act => act.CreateDate).ToList();
        }

        public IList<SystemActionDTO> GetInProgressByType(IUnitOfWork db,
            SystemActionType type,
            DateTime? withAttemptDateAfterThis)
        {
            var query = from a in db.SystemActions.GetAllAsDto()
                        where a.Status == (int)SystemActionStatus.InProgress
                              && a.Type == (int)type
                        select a;

            if (withAttemptDateAfterThis.HasValue)
            {
                query = query.Where(a => !a.AttemptDate.HasValue
                                         || a.AttemptDate.Value >= withAttemptDateAfterThis);
            }

            return query.ToList();

            //return db.SystemActions.GetAllAsDto().Where(a => a.Type == (int)type 
            //    && (a.Status == (int)SystemActionStatus.None
            //    //|| a.Status == (int)SystemActionStatus.Fail
            //    ))
            //    .OrderBy(act => act.CreateDate).ToList();
        }

        public IList<SystemActionDTO> GetByTypeAndGroupId(IUnitOfWork db,
            SystemActionType type,
            string groupId)
        {
            return db.SystemActions.GetAllAsDto().Where(a => a.Type == (int)type
                    && a.GroupId == groupId).ToList();
        }

        public IList<SystemActionDTO> GetByTypeAndTag(IUnitOfWork db,
            SystemActionType type,
            string tag)
        {
            return db.SystemActions.GetAllAsDto().Where(a => a.Type == (int)type
                    && a.Tag == tag).ToList();
        }

        public long AddAction(IUnitOfWork db, 
            SystemActionType type, 
            string tag,
            ISystemActionInput inputData, 
            long? parentActionId,
            long? by,
            SystemActionStatus? status = SystemActionStatus.None)
        {
            var input = JsonConvert.SerializeObject(inputData);
            _logService.Info("AddAction, type=" + type + ", inputData=" + input);
            var newAction = new SystemActionDTO()
            {
                ParentId = parentActionId,
                Status = (int)status,
                Type = (int) type,
                Tag = tag,
                InputData = input,

                CreateDate = _time.GetUtcTime(),
                CreatedBy = by,
            };
            db.SystemActions.AddAction(newAction);

            return newAction.Id;
        }

        //public long AddAction(SystemActionType type,
        //    string tag,
        //    ISystemActionInput inputData,
        //    long? parentActionId,
        //    long? by,
        //    SystemActionStatus? status = SystemActionStatus.None)
        //{
        //    var input = JsonConvert.SerializeObject(inputData);
        //    _logService.Info("AddAction, type=" + type + ", inputData=" + input);
        //    var newAction = new SystemActionDTO()
        //    {
        //        ParentId = parentActionId,
        //        Status = (int)status,
        //        Type = (int)type,
        //        Tag = tag,
        //        InputData = input,

        //        CreateDate = _time.GetUtcTime(),
        //        CreatedBy = by,
        //    };
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        db.SystemActions.AddAction(newAction);
        //        db.Commit();
        //    }

        //    return newAction.Id;
        //}

        public void SetResult(IUnitOfWork db, 
            long actionId, 
            SystemActionStatus status, 
            ISystemActionOutput outputData,
            string groupId = null)
        {
            string output = null; 
            if (outputData != null)
                output = JsonConvert.SerializeObject(outputData);

            _logService.Info("SetResult, actionId=" + actionId 
                + " status=" + status 
                + ", outputData=" + output
                + ", groupId=" + groupId);

            var action = db.SystemActions.Get(actionId);
            if (action != null)
            {
                if (groupId != null)
                    action.GroupId = groupId;
                action.Status = (int) status;
                action.OutputData = output;
                action.AttemptDate = _time.GetUtcTime();
                if (status != SystemActionStatus.InProgress)
                    action.AttemptNumber++;
            }
        }
    }
}
