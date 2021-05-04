using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.General.Services;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallQuantityProcessing
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public CallQuantityProcessing(IDbFactory dbFactory,
            ISystemActionService actionService,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _actionService = actionService;
        }

        public void CallCheckSaleEnd(long listingId)
        {
            using (var db = new UnitOfWork(_log))
            {
                new SaleManager(_log, _time).CheckSaleEndForAll(db);
            }
        }

        public void CallCheckPrice()
        {
            var dbFactory = new DbFactory();
            var settings = new SettingsService(dbFactory);
            var priceManager = new PriceManager(_log, _time, dbFactory, _actionService, settings);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();

                priceManager.FixupListingPrices(db);
            }
        }

        public void CallCheckFbaFbpPrice()
        {
            var dbFactory = new DbFactory();
            var settings = new SettingsService(dbFactory);
            var priceManager = new PriceManager(_log, _time, dbFactory, _actionService, settings);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();

                priceManager.FixupFBAPrices(db);
            }
        }

        public void CallCheckQty()
        {
            var settings = new SettingsService(_dbFactory);
            var qtyManager = new QuantityManager(_log, _time);

            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();

                qtyManager.FixupListingQuantity(db, settings);
            }
        }

        public void CallRedistributeQuantity(IList<string> styleIdList)
        {
            var settings = new SettingsService(_dbFactory);
            var quantityManager = new QuantityManager(_log, _time);
            var qtyDistributionService = new QuantityDistributionService(_dbFactory, 
                quantityManager, 
                _log, 
                _time,
                QuantityDistributionHelper.GetDistributionMarkets(),
                DistributeMode.None);
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();
                if (!styleIdList.Any())
                    qtyDistributionService.Redistribute(db);
                else
                {
                    var style = db.Styles.GetAllActiveAsDto().Where(st => styleIdList.Contains(st.StyleID)).ToList();
                    var styleIds = style.Select(st => st.Id).ToList();
                    qtyDistributionService.RedistributeForStyle(db, styleIds);
                }
            }
        }
    }
}
