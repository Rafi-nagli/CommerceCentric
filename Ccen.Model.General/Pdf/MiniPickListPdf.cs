using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.DTO;
using System.IO;

namespace Amazon.Model.Implementation.Pdf
{
    public class MiniPickListPdf
    {
        public static Byte[] Build(string rootPath,
            string trackingNumber,
            IList<DTOOrderItem> items,
            bool inlcudeItems,
            string instructions)
        {
            var html = File.ReadAllText(Path.Combine(rootPath, "mail-pick-list.html"));
            var itemHtml = File.ReadAllText(Path.Combine(rootPath, "mail-pick-list-item.html"));

            var lines = "";
            if (inlcudeItems)
            {
                foreach (var item in items)
                {
                    var image = item.ItemPicture;
                    var location = item.Locations != null && item.Locations.Any()
                        ? item.Locations[0].Isle + "/" + item.Locations[0].Section + "/" + item.Locations[0].Shelf
                        : "-";

                    lines += itemHtml.Replace("{Image}", image)
                        .Replace("{Name}", item.Title)
                        .Replace("{StyleId}", item.StyleId)
                        .Replace("{Size}", item.StyleSize)
                        .Replace("{Quantity}", item.Quantity.ToString())
                        .Replace("{Location}", location);
                }
            }

            html = html.Replace("{Items}", lines);

            html = html.Replace("{Instructions}", instructions);

            var css = File.ReadAllText(Path.Combine(rootPath, "mail-pick-list.css")); ;// @".headline{font-size:200%} td { padding:20px }";

            return PdfHelper.BuildPdfFromHtml(html, css, 1);
        }
    }
}
