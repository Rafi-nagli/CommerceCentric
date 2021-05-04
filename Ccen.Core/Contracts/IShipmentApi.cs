using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Dhl;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.DTO.Shippings;

namespace Amazon.Core.Contracts
{
    public interface IShipmentApi
    {
        long ProviderId { get; }

        ShipmentProviderType Type { get; }

        decimal Balance { get; }

        double DefaultWeightForNoWeigth { get; }

        IList<RateInfo> SupportedRates { get; }

        RateDTO GetEmptyRate(string serviceName, string toCountryCode);

        GetRateResult GetAllFlatLocalRate(
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            double weight,
            ItemPackageDTO overridePackageSize,
            decimal insuredValue,
            bool isSignConfirmation,
            OrderRateInfo orderInfo,
            RetryModeType retryMode);

        GetRateResult GetAllRate(
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            double weight,
            ItemPackageDTO overridePackageSize,
            decimal insuredValue,
            bool isSignConfirmation,
            OrderRateInfo orderInfo,
            RetryModeType retryMode);

        GetRateResult GetLocalRate(
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            double weight,
            ItemPackageDTO overridePackageSize,
            decimal insuredValue,
            bool isSignConfirmation,
            OrderRateInfo orderInfo,
            RetryModeType retryMode);

        GetRateResult GetInternationalRates(
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            double weight,
            ItemPackageDTO overridePackageSize,
            decimal insuredValue,
            bool isSignConfirmation,
            OrderRateInfo orderInfo,
            RetryModeType retryMode);
        
        CallResult<GetLabelDTO> CreateShipment(OrderShippingInfoDTO shippingInfo,
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            AddressDTO toAddress,
            string boughtInTheCountry,
            DateTime shipDate,
            string notes,
            bool hasPickup,
            bool sampleMode,
            bool fromUI);

        CancelLabelResult CancelShipment(string providerShipmentId);

        bool CanUpgrade(string shippingService, int upgradeLevel, string country);
        bool CanDowngrade(string shippingService, int upgradeLevel, string country);

        RateDTO UpgradeService(string shippingService, bool isSupportFlat);
        RateDTO DowngradeService(string shippingService, bool isSupportFlat);

        CallResult<IList<ScanFormInfo>> GetScanForm(IList<string> providerShipmentIdList,
            AddressDTO returnAddress,
            DateTime shipDate);

        CallResult<BookPickupInfo> BookPickup(DateTime shipDate,
            TimeSpan readyByTime,
            TimeSpan closeTime,
            double weight,
            AddressDTO pickupAddress,
            bool sampleMode);

        AccountInfo GetBalance();
        void SetBalance(decimal balance);
    }
}
