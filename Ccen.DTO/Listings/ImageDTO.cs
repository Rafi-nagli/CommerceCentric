using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ImageDTO
    {
        public string Label { get; set; }
        public string ImageData { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }
        public string SourceImageId { get; set; }
    }
}
