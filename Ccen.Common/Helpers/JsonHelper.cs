using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Helpers
{
    public class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>() { new IsoDateTimeConverter() { DateTimeFormat = "o" } } //"yyyy-MM-ddTHH:mm:ss.fffffffzzz" //"yyyy-MM-ddTHH:mm:ss.fffffffK"
                });
        }

        public static T Deserialize<T>(string json)
        {
            if (String.IsNullOrEmpty(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
