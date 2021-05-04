using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory.Counting
{
    public class OpenBoxCountingViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int BoxQuantity { get; set; }

        public int SizesQuantity { get; set; }

        public int BatchTimeStatus { get; set; }

        public string CountByName { get; set; }

        public string BatchTimeStatusName
        {
            get { return BatchTimeStatusHelper.GetStatusName((BatchTimeStatus)BatchTimeStatus); }
        }

        public int TotalBoxesQuantity
        {
            get { return SizesQuantity; }
        }

        public DateTime? CountingDate { get; set; }

        public DateTime? CountingDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CountingDate); }
            set { CountingDate = DateHelper.ConvertUtcToApp(value); }
        }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public DateTime? CreateDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CreateDate); }
            set { CreateDate = DateHelper.ConvertUtcToApp(value); }
        }

        public string CountingDateFormatted
        {
            get
            {
                return CountingDate.HasValue ? CountingDate.Value.ToString(DateHelper.DateFormat) : "-";
            }
        }

        public bool CanEditCountPerson { get; set; }

        public StyleItemCollection StyleItems { get; set; }


        public override string ToString()
        {
            return "Id=" + Id
                   + ", StyleId=" + StyleId
                   + ", BoxQuantity=" + BoxQuantity
                   + ", BatchTimeStatus=" + BatchTimeStatus
                   + ", CountByName=" + CountByName;
        }

        public OpenBoxCountingViewModel()
        {
            BoxQuantity = 1;

            StyleItems = new StyleItemCollection(StyleItemDisplayMode.BoxQty);
        }

        public OpenBoxCountingViewModel(IUnitOfWork db, long styleId)
        {
            StyleId = styleId;

            BoxQuantity = 1;

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId).ToList();
            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxQty,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si))
                                .ToList()
            };
        }

        public OpenBoxCountingViewModel(OpenBoxCounting item,
            IList<StyleItemDTO> styleItems,
            List<OpenBoxCountingItemDto> boxItems)
        {
            Id = item.Id;
            StyleId = item.StyleId;
            BoxQuantity = item.BoxQuantity;
            BatchTimeStatus = item.BatchTimeStatus;
            CountByName = item.CountByName;

            CountingDate = item.CountingDate;
            
            CreateDate = item.CreateDate;
            UpdateDate = item.UpdateDate;

            SizesQuantity = boxItems.Sum(s => s.Quantity);

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxQty,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si))
                                .ToList()
            };

            //Set boxes values
            foreach (var boxItem in boxItems)
            {
                var styleItem = StyleItems.Items.FirstOrDefault(si => si.Id == boxItem.StyleItemId);
                if (styleItem != null)
                {
                    styleItem.Quantity = boxItem.Quantity;
                    styleItem.BoxItemId = boxItem.Id;
                }
            }
        }


        public static IList<OpenBoxCountingViewModel> GetAll(IUnitOfWork db, long styleId)
        {
            var boxSizeItems = db.OpenBoxCountings.GetByStyleId(styleId);
            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .ToList();

            return boxSizeItems.Select(box => new OpenBoxCountingViewModel(box,
                    styleSizes,
                    db.OpenBoxCountingItems.GetByBoxIdAsDto(box.Id).ToList()))
                .ToList();
        }

        public long Apply(IUnitOfWork db, DateTime when, long? by)
        {
            var existBox = db.OpenBoxCountings.GetFiltered(b => b.Id == Id).FirstOrDefault();

            if (existBox == null)
            {
                AddNewBox(db, when, by);
            }
            else
            {
                UpdateExistingBox(db, existBox, when, by);
            }

            var id = db.Styles.GetFiltered(s => s.Id == StyleId).Select(s => s.Id).FirstOrDefault();
            return id;

        }

        private void UpdateExistingBox(IUnitOfWork db,
            OpenBoxCounting dbBox, 
            DateTime when,
            long? by)
        {
            dbBox.StyleId = StyleId;
            dbBox.BoxQuantity = BoxQuantity;
            dbBox.BatchTimeStatus = BatchTimeStatus;
            dbBox.CountByName = CountByName;
            dbBox.CountingDate = DateHelper.ConvertAppToUtc(CountingDate) ?? when;

            dbBox.UpdateDate = when;
            dbBox.UpdatedBy = by;

            db.Commit();

            var boxItems = StyleItems.Items
                .Where(si => si.Quantity.HasValue)
                .Select(si => new OpenBoxCountingItemDto()
                {
                    Id = si.BoxItemId ?? 0,
                    StyleItemId = si.Id,
                    Quantity = si.Quantity ?? 0,
                }).ToList();

            db.OpenBoxCountingItems.UpdateBoxItemsForBox(dbBox.Id, boxItems, when, by);
        }
        
        private void AddNewBox(IUnitOfWork db, DateTime when, long? by)
        {
            var openBox = new OpenBoxCounting
            {
                StyleId = StyleId,
                BoxQuantity = BoxQuantity,
                BatchTimeStatus = BatchTimeStatus,
                CountByName = CountByName,
                CountingDate = DateHelper.ConvertAppToUtc(CountingDate) ?? when,

                CreateDate = when,
                CreatedBy = by
            };
            db.OpenBoxCountings.Add(openBox);
            db.Commit();

            var boxItems = StyleItems.Items
                            .Where(si => si.Quantity.HasValue)
                            .Select(si => new OpenBoxCountingItemDto()
                            {
                                Id = si.BoxItemId ?? 0,
                                StyleItemId = si.Id,
                                Quantity = si.Quantity ?? 0,
                            }).ToList();

            db.OpenBoxCountingItems.UpdateBoxItemsForBox(openBox.Id, boxItems, when, by);
        }
    }
}