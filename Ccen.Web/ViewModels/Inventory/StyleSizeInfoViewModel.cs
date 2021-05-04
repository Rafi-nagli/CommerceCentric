using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleSizeInfoViewModel
    {
        public long StyleItemId { get; set; }
        public long StyleId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public double? Weight { get; set; }

        public string StyleName { get; set; }

        public static StyleSizeInfoViewModel GetByStyleItemId(IUnitOfWork db,
            long styleItemId)
        {
            var infoQuery = from si in db.StyleItems.GetAllAsDto()
                join s in db.Styles.GetAllAsDto() on si.StyleId equals s.Id
                where si.StyleItemId == styleItemId
                select new StyleSizeInfoViewModel()
                {
                    StyleId = s.Id,
                    StyleItemId = si.StyleItemId,
                    Color = si.Color,
                    Size = si.Size,
                    Weight = si.Weight,

                    StyleName = s.Name
                };

            return infoQuery.FirstOrDefault();
        }
    }
}