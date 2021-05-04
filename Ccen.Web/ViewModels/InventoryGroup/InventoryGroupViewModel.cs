using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Categories;
using Amazon.DTO.Inventory;


namespace Amazon.Web.ViewModels.InventoryGroup
{
    public class InventoryGroupViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long StyleCount { get; set; }

        public IList<InventoryGroupItemViewModel> GroupItems { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime FormattedCreateDate
        {
            get { return DateHelper.ConvertUtcToApp(CreateDate); }
        }

        public InventoryGroupViewModel()
        {

        }

        public static InventoryGroupViewModel BuildFrom(IUnitOfWork db, IList<long> styleIds)
        {
            var groupItems = db.Styles.GetAll().Where(st => styleIds.Contains(st.Id))
                .Select(st => new InventoryGroupItemViewModel()
                {
                    StyleId = st.Id,
                    StyleString = st.StyleID
                })
                .ToList();

            var model = new InventoryGroupViewModel();
            model.GroupItems = groupItems;

            return model;
        }

        public InventoryGroupViewModel(StyleGroupDTO group)
        {
            Id = group.Id;
            Name = group.Name;
            StyleCount = group.StyleCount;
            CreateDate = group.CreateDate;
        }

        public InventoryGroupViewModel(StyleGroupDTO group, IList<StyleToGroupDTO> groupItems)
        {
            Id = group.Id;
            Name = group.Name;
            StyleCount = group.StyleCount;
            CreateDate = group.CreateDate;

            GroupItems = groupItems.Select(sgi => new InventoryGroupItemViewModel()
            {
                StyleId = sgi.StyleId,
                StyleString = sgi.StyleString,
            }).ToList();
        }

        public IList<MessageString> Validate()
        {
            List<MessageString> messages = new List<MessageString>();

            return messages;
        }

        public static void Remove(IUnitOfWork db,
            ILogService log,
            long groupId)
        {
            log.Info("Remove, groupId=" + groupId);

            var styleGroup = db.StyleGroups.Get(groupId);
            db.StyleGroups.Remove(styleGroup);
            db.Commit();
        }

        public static CallMessagesResultVoid Apply(IUnitOfWork db,
            ISystemActionService actionService,
            InventoryGroupViewModel model,
            DateTime when,
            long? by)
        {
            var existGroup = db.StyleGroups.GetAll().FirstOrDefault(sg => sg.Name == model.Name);
            if (existGroup == null)
            {
                existGroup = new StyleGroup()
                {
                    Name = model.Name,
                    CreateDate = when,
                    CreatedBy = by,
                };
                db.StyleGroups.Add(existGroup);
                db.Commit();
            }

            foreach (var groupItem in model.GroupItems)
            {
                var existGroupItem = db.StyleToGroups.GetAllAsDTO().FirstOrDefault(sgi => sgi.StyleGroupId == existGroup.Id
                    && sgi.StyleId == groupItem.StyleId);
                if (existGroupItem == null)
                {
                    var newStyleItem = new StyleToGroup()
                    {
                        StyleGroupId = existGroup.Id,
                        StyleId = groupItem.StyleId,
                        CreateDate = when,
                        CreatedBy = by
                    };
                    db.StyleToGroups.Add(newStyleItem);
                }
            }
            db.Commit();

            return CallMessagesResultVoid.Success();
        }

        public static InventoryGroupViewModel GetWithItems(IUnitOfWork db,
            long groupId)
        {
            var group = db.StyleGroups.GetAllAsDTO().FirstOrDefault(sg => sg.Id == groupId);
            var groupItems = db.StyleToGroups.GetAllAsDTO().Where(sgi => sgi.StyleGroupId == groupId).ToList();

            return new InventoryGroupViewModel(group, groupItems);
        }

        public static IEnumerable<InventoryGroupItemViewModel> GetChildItems(IUnitOfWork db, long groupId)
        {
            var query = (from sg in db.StyleToGroups.GetAllAsDTO()
                         join st in db.Styles.GetAll() on sg.StyleId equals st.Id
                         where sg.StyleGroupId == groupId
                         orderby sg.Id ascending
                         select new InventoryGroupItemViewModel
                         {
                             Id = sg.Id,
                             StyleId = sg.StyleId,
                             StyleString = sg.StyleString,
                             Name = st.Name,
                         });

            return query.ToList();
        }

        public static IEnumerable<InventoryGroupViewModel> GetAll(IUnitOfWork db,
            ITime time,
            InventoryGroupFilterViewModel filter)
        {
            var query = db.StyleGroups.GetAllAsDTO();

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.CreateDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.CreateDate <= filter.DateTo.Value);

            return query.ToList().Select(s => new InventoryGroupViewModel(s)).ToList();
        }
    }
}