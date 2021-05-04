using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Amazon.Core;
using Amazon.Core.Entities.Sizes;
using Amazon.DAL;

namespace Amazon.Web.ViewModels
{
    public class SizeViewModel
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        public bool DefaultIsChecked { get; set; }
        public int SortOrder { get; set; }
        public string Departments { get; set; }

        public string GroupName { get; set; }

        public override string ToString()
        {
            return "Id=" + Id + 
                ", GroupId=" + GroupId + 
                ", GroupName=" + GroupName +
                ", Name=" + Name + 
                ", DefaultIsChecked=" + DefaultIsChecked +
                ", SortOrder=" + SortOrder +
                ", Departments=" + Departments;
        }

        public static IEnumerable<SizeViewModel> GetSizesForGroup(IUnitOfWork db, int groupId)
        {
            return db.Sizes.GetSizesForGroup(groupId)
                .OrderBy(m => m.SortOrder)
                .Select(s => new SizeViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    GroupId = groupId,
                    DefaultIsChecked = s.DefaultIsChecked,
                    SortOrder = s.SortOrder,
                    Departments = s.Departments,
                });
        }

        public static SizeViewModel Add(IUnitOfWork db, SizeViewModel item)
        {
            var size = new Size
            {
                Name = item.Name,
                SizeGroupId = item.GroupId,
                DefaultIsChecked = item.DefaultIsChecked,
                SortOrder = item.SortOrder,
                Departments = item.Departments,
            };
            db.Sizes.Add(size);
            db.Commit();

            item.Id = size.Id;
            return item;
        }

        public static SizeViewModel Update(IUnitOfWork db, SizeViewModel item)
        {
            var size = db.Sizes.Get(item.Id);
            if (size != null)
            {
                size.Name = item.Name;
                size.SizeGroupId = item.GroupId;
                size.SortOrder = item.SortOrder;
                size.DefaultIsChecked = item.DefaultIsChecked;
                size.Departments = item.Departments;

                db.Commit();

                return item;
            }
            return null;
        }

        public static void Delete(IUnitOfWork db, int id)
        {
            var size = db.Sizes.Get(id);
            if (size != null)
            {
                db.Sizes.Remove(size);
                db.Commit();
            }
        }
    }
}