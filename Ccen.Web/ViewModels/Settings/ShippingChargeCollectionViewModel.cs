using Amazon.Core.Contracts.Factories;
using Ccen.Web.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Settings
{
    public class ShippingChargeCollectionViewModel
    {
        public IList<ShippingChargeViewModel> ShippingCharges { get; set; }

        public ShippingChargeCollectionViewModel()
        {

        }

        public ShippingChargeCollectionViewModel(IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var queryList = (from p in db.ShipmentProviders.GetAll()
                                 join m in db.ShippingMethods.GetAll() on p.Type equals m.ShipmentProviderType
                                 join chr in db.ShippingCharges.GetAll() on m.Id equals chr.ShippingMethodId into withCharge
                                 from chr in withCharge.DefaultIfEmpty()
                                 where p.IsActive
                                     && m.IsActive
                                 orderby p.Type ascending, m.Id ascending
                                 select new
                                 {
                                     ShippingChargeId = (int?)chr.Id,
                                     ProviderType = p.Type,
                                     MethodName = m.Name,
                                     MethodId = m.Id,
                                     ChargePercent = (decimal?)chr.ChargePercent,
                                 }).ToList();

                ShippingCharges = queryList.Select(m => new ShippingChargeViewModel()
                {
                    ShippingChargeId = m.ShippingChargeId ?? 0,
                    ShippingProviderType = m.ProviderType,
                    ShippingMethodId = m.MethodId,
                    ShippingMethodName = m.MethodName,
                    ShippingChargePercent = m.ChargePercent,
                }).ToList();
            }
        }

        public bool Save(IDbFactory dbFactory, DateTime when, long? by)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var dbShippingCharges = db.ShippingCharges.GetAll().ToList();
                var activeShippingCharges = ShippingCharges.Where(ch => ch.ShippingChargePercent.HasValue
                    && ch.ShippingChargePercent > 0).ToList();

                var updatedIds = new List<long>();
                foreach (var shippingCharge in activeShippingCharges)
                {
                    var dbShippingCharge = dbShippingCharges.FirstOrDefault(ch => ch.Id == shippingCharge.ShippingChargeId);
                    if (dbShippingCharge == null)
                    {
                        dbShippingCharge = new Core.Entities.ShippingCharge()
                        {
                            ShippingMethodId = shippingCharge.ShippingMethodId,
                            CreateDate = when,
                            CreatedBy = by,
                        };
                        db.ShippingCharges.Add(dbShippingCharge);
                    }

                    dbShippingCharge.ChargePercent = shippingCharge.ShippingChargePercent ?? 0;
                }
                db.Commit();

                var existIds = activeShippingCharges
                    .Where(ch => ch.ShippingChargeId > 0)
                    .Select(ch => ch.ShippingChargeId)
                    .ToList();
                var toDeleteItems = dbShippingCharges.Where(ch => !existIds.Contains(ch.Id)).ToList();
                foreach (var toDeleteItem in toDeleteItems)
                {
                    db.ShippingCharges.Remove(toDeleteItem);
                }
                db.Commit();
            }

            return true;
        }



    }
}