using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Validation
{
    public interface IOrderValidatorService
    {
        void OnNotFoundAllListings(DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> sourceOrderItems);

        void OrderStatusValidationStep(IUnitOfWork db,
            DTOMarketOrder marketOrder,
            Order dbOrder);

        void OrderListingsValidationStep(IUnitOfWork db,
            IList<ListingOrderDTO> orderItems,
            DTOMarketOrder marketOrder,
            bool isAllListingsFound);

        void OrderValidationStepInitial(IUnitOfWork db,
            ITime time,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            Order dbOrder);

        void OrderValidationStepFinal(IUnitOfWork db,
            ITime time,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            Order dbOrder);


        void OrderValidationStepAlwaysInitial(IUnitOfWork db,
            ITime time,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            Order dbOrder);

        void OrderValidationStepAlways(IUnitOfWork db,
            ITime time,
            IMarketApi api,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            Order dbOrder);

        void ShippingValidationStep(IUnitOfWork db,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            IList<OrderShippingInfoDTO> shippings,
            DTOMarketOrder marketOrder,
            Order dbOrder);

        IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            IUnitOfWork db,
            AddressDTO address,
            long? orderId,
            out AddressDTO addressWithCorrection);
    }
}
