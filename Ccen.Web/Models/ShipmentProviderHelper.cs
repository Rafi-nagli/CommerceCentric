using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Common.Helpers
{
    public class ShipmentProviderHelper
    {
        public static IList<Tuple<int, string>> GetAllProviderList()
        {
            var providers = new List<ShipmentProviderType>()
            {
                ShipmentProviderType.Stamps,
                //ShipmentProviderType.StampsPriority,
                ShipmentProviderType.Amazon,
                ShipmentProviderType.Dhl,
                //ShipmentProviderType.DhlECom,
                ShipmentProviderType.IBC,
                ShipmentProviderType.SkyPostal
            };

            using (var db = new UnitOfWork())
            {
                return db.ShipmentProviders.GetAllAsDto().ToList().Where(x => providers.Contains((ShipmentProviderType)x.Type)).Select(p => new Tuple<int, string>(p.Type, p.ShortName)).ToList();
            }
        }

        public static IList<KeyValuePair<int, string>> GetMainProviderList()
        {
            var providers = new List<ShipmentProviderType>()
            {
                ShipmentProviderType.Stamps,
                ShipmentProviderType.Amazon,
                ShipmentProviderType.Dhl,
                //ShipmentProviderType.DhlECom,
                ShipmentProviderType.IBC,
                ShipmentProviderType.SkyPostal,
                ShipmentProviderType.FIMS,
                ShipmentProviderType.FedexOneRate,
                ShipmentProviderType.FedexGeneral
            };

            using (var db = new UnitOfWork())
            {
                return db.ShipmentProviders.GetAllAsDto().ToList().Where(x => providers.Contains((ShipmentProviderType)x.Type)).Select(p => new KeyValuePair<int, string>(p.Type, p.Name)).ToList();
            }
        }

        public static IList<ShippingMethodDTO> GetAllShippingServiceList()
        {
            if (_allShippingServiceList != null)
            {
                return _allShippingServiceList;
            }
            using (var db = new UnitOfWork(null))
            {
                return _allShippingServiceList = db.ShippingMethods.GetAllAsDto().Where(m => m.IsActive).ToList();
            }
        }

        public static IList<ShipmentProviderDTO> GetAllShippingProviderList()
        {
            if (_allShipmentProviderList != null)
            {
                return _allShipmentProviderList;
            }
            using (var db = new UnitOfWork(null))
            {
                return _allShipmentProviderList = db.ShipmentProviders.GetAllAsDto().Where(m => m.IsActive).ToList();
            }
        }


        private static IList<ShippingMethodDTO> _allShippingServiceList;
        private static IList<ShipmentProviderDTO> _allShipmentProviderList;

        public static string GetName(ShipmentProviderType type)
        {
            var typeInt = (int)type;
            var res = GetAllShippingProviderList().FirstOrDefault(x => x.Type == typeInt);
            if (res != null)
            {
                return res.Name;
            }
            return "-";
        }

        public static string GetShortName(ShipmentProviderType type)
        {
            var typeInt = (int)type;
            var res = GetAllShippingProviderList().FirstOrDefault(x => x.Type == typeInt);
            if (res != null)
            {
                return StringHelper.GetFirstNotEmpty(res.ShortName, res.Name);
            }
            return "-";
        }
    }
}
