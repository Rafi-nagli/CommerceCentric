using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class ImageInfo
    {
        public string Image { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public int ImageType { get; set; }
        public long? Tag { get; set; }
        public int UpdateFailAttempts { get; set; }
    }
}
