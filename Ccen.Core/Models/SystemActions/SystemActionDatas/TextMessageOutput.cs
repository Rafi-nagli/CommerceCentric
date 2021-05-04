using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class TextMessageOutput : ISystemActionOutput
    {
        public bool IsSuccess { get; set; }
        public IList<MessageString> Messages { get; set; }
    }
}
