using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Amazon.Common.Services;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Amazon.Web.Models.Services;
using log4net;

namespace Amazon.Web.Controllers
{
    [RoutePrefix("api/v1/orders")]
    public class OrderApiV1Controller : ApiController
    {
        [Route("{id:int}")]
        [System.Web.Mvc.HttpGet]
        public DTOOrder GetByOrderId(long id)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.GetByOrderId(id);
        }

        [Route("getRecent/{dropShipperId:int}")]
        [System.Web.Mvc.HttpGet]
        public IList<DTOOrder> GetRecentByDropShipperId(long dropShipperId)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.GetRecentByDropShipperId(dropShipperId);
        }

        [Route("updatePrice")]
        [System.Web.Mvc.HttpPost]
        public CallResult UpdatePrice(ItemDTO item)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.UpdatePrice(item);
        }

        [Route("updateQuantity")]
        [System.Web.Mvc.HttpPost]
        public CallResult UpdateQuantity(ItemDTO item)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.UpdateQuantity(item);
        }

        [Route("updateShipments")]
        [System.Web.Mvc.HttpPost]
        public CallResult UpdateShipments(DTOOrder order)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.UpdateShipments(order);
        }

        [Route("cancelOrder/{id:int}")]
        [System.Web.Mvc.HttpPost]
        public CallResult CancelOrder(int id)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.CancelOrder(id);
        }




        #region Items

        [Route("getStyles/{market:int}/{marketplaceId}")]
        [System.Web.Mvc.HttpGet]
        public IList<StyleEntireDto> GetStyles(int market, string marketplaceId)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.GetStyles((MarketType)market, marketplaceId);
        }

        [Route("getItems/{market:int}/{marketplaceId}")]
        [System.Web.Mvc.HttpGet]
        public IList<ParentItemDTO> GetItems(int market, string marketplaceId)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.GetItems((MarketType)market, marketplaceId);
        }

        [Route("getQuantities/{market:int}/{marketplaceId}")]
        [System.Web.Mvc.HttpGet]
        public IList<StyleEntireDto> GetQuantities(int market, string marketplaceId)
        {
            var baseLog = LogManager.GetLogger("RequestLogger");
            var log = new FileLogService(baseLog, null);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var orderHistory = new OrderHistoryService(log, time, dbFactory);

            var dsService = new DropShipperApiService(log, time, dbFactory, actionService, orderHistory);
            return dsService.GetQuantities((MarketType)market, marketplaceId);
        }

        #endregion
    }
}
