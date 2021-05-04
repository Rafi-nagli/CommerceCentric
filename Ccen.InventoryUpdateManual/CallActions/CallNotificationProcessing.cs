using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.InventoryUpdateManual.Models;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Emails;
using Amazon.Model.Implementation.Emails.Rules;
using Amazon.Model.Implementation.Notifications.SupportNotifications;
using Amazon.Model.Models;
using Amazon.Model.SyncService.Threads.Simple.Notifications;
using Moq;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallNotificationProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;
        private IAddressService _addressService;
        private IEmailService _emailService;

        public CallNotificationProcessing(ILogService log,
            IAddressService addressService,
            IEmailService emailService,
            IDbFactory dbFactory,
            CompanyDTO company,
            ITime time)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _company = company;
            _addressService = addressService;
            _emailService = emailService;
        }


        public void CheckQtyDisparityNotification()
        {
            var notification = new QtyDisparitySupportNotification(_dbFactory, _emailService, _log, _time);
            notification.Check();
        }

        public void CheckPriceDisparityNotification()
        {
            var notification = new PriceDisparitySupportNotification(_dbFactory, _emailService, _log, _time);
            notification.Check();
        }


        public void CheckListingIssuesNotification()
        {
            var notification = new ListingCreationIssuesNotification(_dbFactory, _emailService, _log, _time);
            notification.Check();
        }


        public void CheckEmailStatusNotification()
        {
            var notification = new RefundStatusSupportNotification(_dbFactory, _emailService, _log, _time);
            notification.Check();
        }

        public void CheckQtyChangeNotification()
        {
            var notification = new SubstractQtySupportNotification(_dbFactory, _emailService, _log, _time);
            notification.Check();
        }
    }
}
