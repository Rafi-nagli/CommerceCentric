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

namespace Amazon.Web.ViewModels
{
    public class DhlCloseoutFormViewModel
    {
        private ILogService _log;
        private ITime _time;

        public DhlCloseoutFormViewModel(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public CallResult<string> Closeout(IUnitOfWork db,
            IShipmentApi shipmentApi,
            IFileMaker pdfMaker,
            string outputDirectory,
            bool isSample,
            long? by)
        {
            _log.Info("Begin Closeout");
            var toCloseoutShippings = db.OrderShippingInfos.GetAllAsDto()
                .Where(sh => sh.ShipmentProviderType == (int) ShipmentProviderType.DhlECom 
                    && !sh.ScanFormId.HasValue
                    && !sh.CancelLabelRequested
                    && !sh.LabelCanceled
                    && !String.IsNullOrEmpty(sh.StampsTxId))
                .ToList();

            var toCloseoutMails = db.MailLabelInfos.GetAllAsDto()
                .Where(m => m.ShipmentProviderType == (int) ShipmentProviderType.DhlECom
                            && !m.ScanFormId.HasValue
                            && !m.CancelLabelRequested
                            && !m.LabelCanceled
                            && !String.IsNullOrEmpty(m.StampsTxId))
                .ToList();

            //var shippingWithEmptyPackageIds = toCloseoutShippings.Where(sh => String.IsNullOrEmpty(sh.StampsTxId)).ToList();
            //if (shippingWithEmptyPackageIds.Any())
            //    return CallResult<string>.Fail("The following orders have has empty Package Id: " 
            //        + String.Join(", ", shippingWithEmptyPackageIds.Select(sh => sh.OrderAmazonId).ToList()), null);

            //var mailingWithEmptyPackageIds = toCloseoutMails.Where(sh => String.IsNullOrEmpty(sh.StampsTxId)).ToList();
            //if (mailingWithEmptyPackageIds.Any())
            //    return CallResult<string>.Fail("The following orders have has empty Package Id: "
            //        + String.Join(", ", mailingWithEmptyPackageIds.Select(sh => sh.OrderAmazonId).ToList()), null);


            var closeoutIds = toCloseoutShippings.Select(sh => sh.StampsTxId).ToList();
            closeoutIds.AddRange(toCloseoutMails.Select(m => m.StampsTxId).ToList());

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
                var forms = result.Data;

                long? lastFormId = null;
                foreach (var form in forms)
                {
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
                }

                var pdfFileName = pdfMaker.CreateFileWithLabels(new List<PrintLabelInfo>(),
                    forms.Select(s => outputDirectory + s.ScanFormPath.Trim(new[] { '~' })).ToList(),
                    null,
                    outputDirectory);

                var printPack = new LabelPrintPack()
                {
                    FileName = pdfFileName,
                    CreateDate = _time.GetAppNowTime(),
                };
                db.LabelPrintPacks.Add(printPack);
                db.Commit();

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

                var url = Models.UrlHelper.GetPrintLabelPathById(printPack.Id);

                return CallResult<string>.Success(url);
            }
            else
            {
                return CallResult<string>.Fail(result.Message, null);
            }
        }
    }
}