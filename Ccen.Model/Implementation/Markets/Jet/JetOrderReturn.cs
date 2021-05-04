using System;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Jet.Api;
using Magento.Api.Wrapper.MagentoTokenServiceV2;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class JetOrderReturn
    {
        private JetApi _api;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;
        private IEmailService _emailService;

        public JetOrderReturn(JetApi api, 
            ISystemActionService actionService,
            IEmailService emailService,
            ILogService log, 
            ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
            _actionService = actionService;
            _emailService = emailService;
        }

        public void ProcessReturns(IUnitOfWork db)
        {
            var returnsResult = _api.GetReturns();
            if (returnsResult.IsSuccess)
            {
                foreach (var returnDetail in returnsResult.Data)
                {
                    var tag = returnDetail.OrderNumber;
                    var existSystemAction = db.SystemActions.GetAllAsDto().FirstOrDefault(a => a.Tag == tag);
                    if (existSystemAction == null)
                    {
                        _log.Info("Jet: new return request, orderNumber=" + returnDetail.OrderNumber + ", SKU=" + returnDetail.SKU);
                        _emailService.SendSystemEmail("Jet: new return request, orderNumber: " + returnDetail.OrderNumber, 
                            "SKU: " + returnDetail.SKU + " x " + returnDetail.Quantity + " - Requested Amount: $" + returnDetail.RequestedRefundAmount + " - Reason: " + returnDetail.Reason,
                            EmailHelper.RafiEmail,
                            EmailHelper.SupportDgtexEmail);

                        _actionService.AddAction(db,
                            SystemActionType.UpdateOnMarketReturnOrder,
                            tag,
                            new ReturnOrderInput()
                            {
                            },
                            null,
                            null);
                    }
                }
            }
        }
    }
}
