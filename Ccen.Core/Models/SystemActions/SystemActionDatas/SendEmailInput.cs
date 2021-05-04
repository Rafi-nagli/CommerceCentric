using System.Collections.Generic;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class SendEmailInput : ISystemActionInput
    {
        public EmailTypes EmailType { get; set; }
        public long? OrderEntityId { get; set; }
        public string OrderId { get; set; }
        public string ReplyToEmail { get; set; }
        public string ReplyToSubject { get; set; }
        public Dictionary<string, string> Args { get; set; }
    }
}
