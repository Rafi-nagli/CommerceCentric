using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts
{
    public interface ILabelService
    {
        CancelLabelResult CancelLabel(ShipmentProviderType shipmentProviderType,
            string providerShipmentId,
            bool sampleMode);

        PrintLabelResult PrintLabels(
            IUnitOfWork db,
            CompanyDTO company,
            ICompanyAddressService companyAddress,
            ISyncInformer syncInfo,
            long? batchId,
            IList<OrderShippingInfoDTO> shippingList,
            bool skipScanForm,
            IList<OrderShippingInfoDTO> removedList,
            IList<StyleChangeInfo> styleChanges,
            string existScanFormPathes,
            string outputDirectory,
            bool sampleMode,
            DateTime? when,
            long? by);


        void BuildPdfFile(IList<OrderShippingInfoDTO> orders,
            IList<string> scanFormPath,
            BatchInfoToPrint batchInfo,
            string outputDirectory,
            ref PrintLabelResult result);

        IList<AccountInfo> UpdateBalance(IUnitOfWork db,
            DateTime when);

        PrintLabelResult PrintMailLabel(
            IUnitOfWork db,
            IShippingService shippingService,
            MailLabelDTO model,
            DateTime when,
            long? by,
            string outputDirectory,
            string templateDirectory,
            bool sampleMode);
    }
}