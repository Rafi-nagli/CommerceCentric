using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using DropShipper.Api;

namespace Amazon.Web.ViewModels
{
    public class IbcCloseoutFormViewModel
    {
        private ILogService _log;
        private ITime _time;

        public IbcCloseoutFormViewModel(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public CallResult<string> Closeout(IUnitOfWork db,
            IShipmentApi shipmentApi,
            IList<DropShipperApi> externalApis,
            IFileMaker pdfMaker,
            string outputDirectory,
            bool isSample,
            long? by)
        {
            _log.Info("Begin Closeout");
            var toCloseoutShippings = db.OrderShippingInfos.GetAllAsDto()
                .Where(sh => sh.ShipmentProviderType == (int) ShipmentProviderType.IBC 
                    && !sh.ScanFormId.HasValue
                    && !sh.CancelLabelRequested
                    && !sh.LabelCanceled
                    && !String.IsNullOrEmpty(sh.StampsTxId))
                .ToList();

            var toCloseoutMails = db.MailLabelInfos.GetAllAsDto()
                .Where(m => m.ShipmentProviderType == (int) ShipmentProviderType.IBC
                            && !m.ScanFormId.HasValue
                            && !m.CancelLabelRequested
                            && !m.LabelCanceled
                            && !String.IsNullOrEmpty(m.StampsTxId))
                .ToList();

            var closeoutIds = toCloseoutShippings.Select(sh => sh.StampsTxId).ToList();
            closeoutIds.AddRange(toCloseoutMails.Select(m => m.StampsTxId).ToList());

            foreach (var extApi in externalApis)
            {
                var extInfo = extApi.GetIBCOrdersToClose();
                if (!extInfo.IsFail)
                {
                    var mbgCloseoutShippingIds = extInfo.Data.ToCloseoutIds;
                    _log.Info("External API " + extApi.Market + "-" + extApi.MarketplaceId + " closeout ids: " + String.Join(", ", mbgCloseoutShippingIds));
                    closeoutIds.AddRange(mbgCloseoutShippingIds);
                }
                else
                {
                    _log.Info("No communication with " + extApi.Market + "-" + extApi.MarketplaceId + ". Details: " + extInfo.Message);
                    //return CallResult<string>.Fail("No communication with " + extApi.Market + "-" + extApi.MarketplaceId + ". Details: " + extInfo.Message, null);
                }
            }

            _log.Info("Request closeout for Ids: " + String.Join(", ", closeoutIds));

            CallResult<IList<ScanFormInfo>> result = null;
            if (isSample)
            {
                result = CallResult<IList<ScanFormInfo>>.Success(new List<ScanFormInfo>()
                {
                    new ScanFormInfo()
                    {
                        ScanFormId = "Test1",
                        ScanFormPath = "~/Closeouts/closeout_1_7027750.pdf"
                    }
                });
            }
            else
            {
                result = shipmentApi.GetScanForm(closeoutIds, null, DateTime.UtcNow);
            }
            _log.Info("Result: " + result.IsSuccess);

            if (result.IsSuccess)
            {
                var form = result.Data.FirstOrDefault();
                
                long? lastFormId = null;
                var dbForm = new ScanForm()
                {
                    FormId = form.ScanFormId,
                    FileName = form.ScanFormPath,
                    CreateDate = _time.GetAppNowTime(),
                    CreatedBy = by,
                };
                db.ScanForms.Add(dbForm);
                db.Commit();
                lastFormId = dbForm.Id;
                
                _log.Info("Begin update shipments");
                var fromDate = _time.GetAppNowTime().AddDays(-30);
                var shipmentsToUpdate = db.OrderShippingInfos.GetAll().Where(sh => sh.CreateDate > fromDate
                                                                                   && closeoutIds.Contains(sh.StampsTxId))
                    .ToList();
                var mailingsToUpdate = db.MailLabelInfos.GetAll().Where(sh => sh.CreateDate > fromDate
                                                                                   && closeoutIds.Contains(sh.StampsTxId))
                    .ToList();
                
                foreach (var shipment in shipmentsToUpdate)
                {
                    shipment.ScanFormId = lastFormId;
                }
                db.Commit();
                foreach (var mailing in mailingsToUpdate)
                {
                    mailing.ScanFormId = lastFormId;
                }
                db.Commit();
                _log.Info("End update shipments");

                _log.Info("Form: " + (form.CloseoutIds != null ? String.Join(",", form.CloseoutIds) : "null"));

                foreach (var extApi in externalApis)
                {
                    _log.Info("Begin update " + extApi.Market + "-" + extApi.MarketplaceId + " shipments");
                    var closeResult = extApi.CloseIBCOrders(form);
                    _log.Info("Close result: isSuccess: " + closeResult.IsSuccess + ", message: " + closeResult.Message);
                    _log.Info("End update shipments");
                }

                return CallResult<string>.Success("");
            }
            else
            {
                return CallResult<string>.Fail(result.Message, null);
            }
        }
    }
}