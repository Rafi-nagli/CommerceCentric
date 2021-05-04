using Amazon.Core.Models.Calls;
using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ISystemMessageService
    {
        void AddOrUpdate(string name, string tag, string message, ISystemMessageData data, MessageStatus status);
        void AddOrUpdateError(string name, string tag, string message, ISystemMessageData data);
        void Remove(string name, string tag);
        IList<SystemMessageDTO> GetAllByName(string name);
    }
}
