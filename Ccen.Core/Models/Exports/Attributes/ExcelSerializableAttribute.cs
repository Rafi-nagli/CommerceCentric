using System;

namespace Amazon.Core.Exports.Attributes
{
    public class ExcelSerializableAttribute : Attribute
    {
        private const int DefaultWidth = 21;

        public int Width { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }

        public ExcelSerializableAttribute(string name, int order = int.MaxValue, string format = "",
            int width = DefaultWidth)
        {
            Width = width;
            Name = name;
            Format = format;
            Order = order;
        }

        public ExcelSerializableAttribute()
        {

        }
    }    
}
