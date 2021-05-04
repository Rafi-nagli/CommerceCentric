using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Models.Settings;

namespace Amazon.Web.ViewModels.Statuses
{
    public class DhlCloseoutInfoViewModel
    {
        public int ToCloseoutCount { get; set; }

        public static DhlCloseoutInfoViewModel GetInfo(IUnitOfWork db)
        {
            var toCloseoutShipmentCount = db.OrderShippingInfos.GetAllAsDto()
                    .Count(sh => sh.ShipmentProviderType == (int) ShipmentProviderType.DhlECom
                                 && !sh.ScanFormId.HasValue
                                 && !sh.CancelLabelRequested
                                 && !sh.LabelCanceled
                                 && !String.IsNullOrEmpty(sh.StampsTxId));

            var toCloseoutMailingCount = db.MailLabelInfos.GetAllAsDto()
                    .Count(sh => sh.ShipmentProviderType == (int)ShipmentProviderType.DhlECom
                                 && !sh.ScanFormId.HasValue
                                 && !sh.CancelLabelRequested
                                 && !sh.LabelCanceled
                                 && !String.IsNullOrEmpty(sh.StampsTxId));

            return new DhlCloseoutInfoViewModel()
            {
                ToCloseoutCount = toCloseoutShipmentCount + toCloseoutMailingCount
            };
        }
    }
}