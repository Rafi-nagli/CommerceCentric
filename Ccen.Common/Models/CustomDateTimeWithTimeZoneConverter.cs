using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Amazon.Common.Models
{
    public class CustomDateTimeWithTimeZoneConverter : DateTimeConverterBase
    {
        public CustomDateTimeWithTimeZoneConverter()
        {
            //base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzzz";
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var result = String.Empty;
            if (value is DateTime)
            {
                result = ((DateTime)value).ToString("o").Replace("Z", "-04:00");
            }
            else
            {
                throw new Exception("Expected date object value.");
            }
            writer.WriteValue(result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("CustomDateTimeWithTimeZoneConverter.ReadJson");
        }
    }
}
