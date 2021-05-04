using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Amazon.Common.Helpers
{
    public static class ImageExtensions
    {
        public static Stream ToStream(this Image image, ImageFormat formaw)
        {
            var stream = new MemoryStream();
            image.Save(stream, formaw);
            stream.Position = 0;
            return stream;
        }
    }
}
