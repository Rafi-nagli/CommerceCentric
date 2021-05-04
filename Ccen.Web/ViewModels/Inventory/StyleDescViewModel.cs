using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleDescViewModel
    {
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public string Description { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }

        public StyleDescViewModel()
        {

        }

        public static StyleDescViewModel GetStyleDesc(IDbFactory dbFactory, string styleString)
        {
            var result = new StyleDescViewModel();
            using (var db = dbFactory.GetRWDb())
            {
                var style = db.Styles.GetAllAsDtoEx().FirstOrDefault(st => !st.Deleted && st.StyleID == styleString);
                if (style != null)
                {
                    result.StyleId = style.Id;
                    result.StyleString = style.StyleID;
                    result.Description = style.Description;
                    result.BulletPoint1 = style.BulletPoint1;
                    result.BulletPoint2 = style.BulletPoint2;
                    result.BulletPoint3 = style.BulletPoint3;
                    result.BulletPoint4 = style.BulletPoint4;
                    result.BulletPoint5 = style.BulletPoint5;
                }
            }

            return result;
        }

        public void UpdateStyleDesc(IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var style = db.Styles.GetAll().FirstOrDefault(st => !st.Deleted && st.Id == StyleId);
                if (style != null)
                {
                    style.Description = Description;
                    style.BulletPoint1 = BulletPoint1;
                    style.BulletPoint2 = BulletPoint2;
                    style.BulletPoint3 = BulletPoint3;
                    style.BulletPoint4 = BulletPoint4;
                    style.BulletPoint5 = BulletPoint5;
                    db.Commit();
                }
            }
        }
    }
}