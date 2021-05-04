using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class MarketSettingValue<T>
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public T Value { get; set; }

        public MarketSettingValue(MarketType market, string marketplaceId, T value)
        {
            Market = market;
            MarketplaceId = marketplaceId;
            Value = value;
        }

        public static T GetValue(MarketSettingValue<T> settingValue)
        {
            if (settingValue == null)
                return default(T);
            return settingValue.Value;
        }
    }
}
