using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order GetById(long orderId);
        Order GetByOrderNumber(string orderNumber);
        Order GetByCustomerOrderNumber(string orderNumber);

        IQueryable<DTOOrder> GetAllAsDto();

        IList<Order> GetAllByCustomerOrderNumber(string orderNumber);
        IList<Order> GetAllByCustomerOrderNumbers(IList<string> orderNumbers);

        DTOOrder GetByOrderIdAsDto(string orderId);
        DTOOrder GetByOrderIdAsDto(long orderId);
        MailLabelDTO GetMailDTOByOrderId(IWeightService weightService, string orderId);

        Order GetAddressCorrectionForBuyer(string buyerEmail,
            string address1,
            string address2,
            string city,
            string state,
            string country,
            string zip,
            string zipAddon);

        DTOOrder UniversalGetByOrderId(string orderId);

        IQueryable<OrderToTrackDTO> GetUnDeliveredShippingInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> orderIds);
        IQueryable<OrderToTrackDTO> GetUnDeliveredMailInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> orderIds);

        AddressDTO GetAddressInfo(long orderId);
        AddressDTO GetAddressInfo(string orderId);
        IQueryable<DTOOrder> GetListForFeedback();
        IQueryable<DTOOrder> GetNotDeliveredList();
        void UpdateRequestedFeedback(string orderId);
        void UpdateRequestedAddressVerify(string orderId);

        UnshippedInfoDTO GetUnshippedInfo();
        IList<DTOOrder> GetUnshippedOrders();
        IList<DTOOrder> ExcludeExistOrderDtos(List<DTOOrder> orders, MarketType market, string marketplaceId);
        Order CreateFromDto(DTOMarketOrder dto, DateTime? when);
        IEnumerable<SecondDayOrderDTO> GetSecondDayOrders(DateTime? from, DateTime? to);
        IList<OrderToTrackDTO> GetOrdersToTrack();
        IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndMarket(IList<string> statusListToExclude);
        IList<Order> GetOrdersWithModifiedStatus(MarketType market, string marketplaceId);
        IQueryable<Order> GetOrdersByStatus(MarketType market, string marketplaceId, string[] statusList);
        IList<DTOOrder> GetOrdersWithSimilarDateAndBuyerAndAddress(DTOMarketOrder orderDto);
    }
}
