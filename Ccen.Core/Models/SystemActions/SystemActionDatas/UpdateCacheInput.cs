using System.Collections.Generic;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UpdateCacheInput : ISystemActionInput
    {
        public UpdateCacheMode UpdateMode { get; set; }
        public IList<long> ItemIdList { get; set; }
        public IList<long> ParentIdList { get; set; }
        public IList<long> StyleIdList { get; set; }
        public IList<long> StyleItemIdList { get; set; }
    }
}
