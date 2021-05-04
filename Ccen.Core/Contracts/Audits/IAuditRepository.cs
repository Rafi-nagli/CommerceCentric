using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Audits;

namespace Amazon.Core.Contracts.Audits
{
    public interface IAuditRepository
    {
        IList<AuditEntryPropertyDTO> GetAuditEntriesByEntityId(long entityId);
    }
}
