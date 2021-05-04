using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface ISizeMappingService
    {
        IList<ItemDTO> CheckItemsSizeMappingIssue();
        IList<ItemDTO> GetItemsSummaryWithSizeMappingIssue();
    }
}
