using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Amazon.Core.Models.Inventory
{
    public class FeatureExAttributes
    {
        [JsonProperty("WMCharacter")]
        public string WMCharacter { get; set; }

        [JsonProperty("WMCharacterPermanent")]
        public bool WMCharacterPermanent { get; set; }

        [JsonProperty("IsRequiredManufactureBarcode")]
        public bool IsRequiredManufactureBarcode { get; set; }
    }
}
