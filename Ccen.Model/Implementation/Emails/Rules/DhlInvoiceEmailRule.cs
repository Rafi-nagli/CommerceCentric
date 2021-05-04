using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO.Users;
using Amazon.Model.Models.EmailInfos;
using Newtonsoft.Json;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class DhlInvoiceEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public DhlInvoiceEmailRule(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void Process(IUnitOfWork db,
            EmailReadingResult result)
        {
            if (result.Status == EmailMatchingResultStatus.New
                && result.Email.Type == (int)IncomeEmailTypes.DhlInvoice)
            {
                var attachment = result.Email.Attachments.FirstOrDefault();
                if (attachment != null)
                {
                    _log.Info("Process DHL Invoice, path=" + attachment.PhysicalPath);
                    var dhlService = new DhlInvoiceService(_log, _time, _dbFactory);
                    var records = dhlService.GetRecordsFromFile(attachment.PhysicalPath);
                    dhlService.ProcessRecords(records, ShipmentProviderType.Dhl);
                }
            }
        }
    }
}
