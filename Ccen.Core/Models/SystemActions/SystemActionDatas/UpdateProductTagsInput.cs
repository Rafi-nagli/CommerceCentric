using Amazon.Core.Contracts.SystemActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UpdateProductTagsInput : ISystemActionInput
    {
        public const string DELETE_ACTION = "DELETE";
        public const string ADD_ACTION = "ADD";


        public string Action { get; set; }
        public IList<string> Tags { get; set; }
    }
}
