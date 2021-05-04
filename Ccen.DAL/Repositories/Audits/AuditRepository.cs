using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Contracts.Audits;
using Amazon.DTO;
using Amazon.DTO.Audits;
using Z.EntityFramework.Plus;

namespace Amazon.DAL.Repositories
{
    public class AuditRepository : Repository<AuditEntry>, IAuditRepository
    {
        public AuditRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<AuditEntryPropertyDTO> GetAuditEntriesByEntityId(long entityId)
        {
            var audits = this.unitOfWork.Context.AuditEntries.Where<Order>(entityId).ToList();
            var results = new List<AuditEntryPropertyDTO>();
            foreach (var audit in audits)
            {
                results.AddRange(audit.Properties.Select(p => new AuditEntryPropertyDTO()
                {
                    PropertyName = p.PropertyName,
                    NewValue = p.NewValueFormatted,
                    OldValue = p.OldValueFormatted,
                    CreatedBy = audit.CreatedBy,
                    CreatedDate = audit.CreatedDate,
                }));
            }
            return results;
        }
    }
}
