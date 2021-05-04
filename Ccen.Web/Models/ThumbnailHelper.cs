using System;
using System.Drawing;
using System.IO;
using ImageResizer;

namespace Amazon.Web.Models
{
    public class ThumbnailHelper
    {
        public static void GenerateThumbnail(Image sourceImage, 
            string thumbnailFileName,
            int width,
            int height,
            string rotate)
        {
            using (var outStream = System.IO.File.Open(thumbnailFileName, FileMode.Create))
            {
                var query = "";
                if (width > 0)
                    query = "width=" + width.ToString();
                if (height > 0)
                    query += (!String.IsNullOrEmpty(query) ? "&" : "") + "height=" + height.ToString();
                var settings = new ResizeSettings(query);

                //NOTE: if height more then height + 50%
                if (rotate == "auto") // && sourceImage.Width > sourceImage.Height * 1.5)
                    settings.Rotate = 90;
                ImageBuilder.Current.Build(sourceImage, outStream, settings);
            }
        }
    }
}