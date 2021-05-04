using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Models
{
    public class ImageLayout
    {
        public bool IsCrop;

        public int OffsetX;
        public int OffsetY;
        public int Width;
        public int Height;

        public static ImageLayout NoCrop
        {
            get { return new ImageLayout() { IsCrop = false }; }
        }

        public static ImageLayout From(ShippingMethodDTO shippingMethod)
        {
            var offsetX = 0;
            var offsetY = 0;
            var width = 0;
            var heigth = 0;

            if (!String.IsNullOrEmpty(shippingMethod.CroppedLayout))
            {
                var parts = shippingMethod.CroppedLayout.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                offsetX = Int32.Parse(parts[0]);
                offsetY = Int32.Parse(parts[1]);
                width = Int32.Parse(parts[2]);
                heigth = Int32.Parse(parts[3]);
            }

            return new ImageLayout()
            {
                IsCrop = shippingMethod.IsCroppedLabel,

                OffsetX = offsetX,
                OffsetY = offsetY,
                Width = width,
                Height = heigth
            };
        }
    }
}
