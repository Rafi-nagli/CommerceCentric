using System.IO;
using System.Xml;

namespace Amazon.Common.Helpers
{
    public static class StreamHelper
    {
        public static MemoryStream GetStreamFromString(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream GetStreamFromXml(XmlDocument content)
        {
            var stream = new MemoryStream();
            content.Save(stream);
            stream.Flush();
            stream.Position = 0;
            return stream;
        }

        public static XmlDocument GetXmlFromStream(Stream stream)
        {
            var document = new XmlDocument();
            document.Load(stream);
            return document;
        }

        public static string GetStringFromStream(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
