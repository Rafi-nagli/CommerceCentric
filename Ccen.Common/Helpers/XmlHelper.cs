using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Amazon.Common.Helpers
{
    public class XmlHelper
    {
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                return String.Empty;

            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            var subReq = obj;
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, subReq);
                    xml = sww.ToString(); // Your XML
                }
            }

            return xml;
        }

        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            StringReader stream = new StringReader(xml);
            return (T)serializer.Deserialize(stream);
        }

        public static T Deserialize<T>(StreamReader stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));        
            return (T)serializer.Deserialize(stream);
        }
    }
}
