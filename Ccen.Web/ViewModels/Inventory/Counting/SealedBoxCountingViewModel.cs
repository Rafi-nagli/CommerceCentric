using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory.Counting
{
    public class SealedBoxCountingViewModel
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string Breakdown { get; set; }
        public int BoxQuantity { get; set; }

        public int BatchTimeStatus { get; set; }
        public string CountByName { get; set; }

        public string BatchTimeStatusName
        {
            get { return BatchTimeStatusHelper.GetStatusName((BatchTimeStatus)BatchTimeStatus); }
        }

        public DateTime? CountingDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public DateTime? CountingDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CountingDate); }
            set { CountingDate = DateHelper.ConvertUtcToApp(value); }
        }

        public StyleItemCollection StyleItems { get; set; }

        public int BoxItemsQuantity { get; set; }
        public int TotalBoxesQuantity
        {
            get { return BoxItemsQuantity * BoxQuantity; }
        }

        public string CountingDateFormatted
        {
            get
            {
                return CountingDate.HasValue ? CountingDate.Value.ToString(DateHelper.DateFormat) : "-";
            }
        }

        public bool CanEditCountPerson { get; set; }

        public override string ToString()
        {
            return "Id=" + Id 
                + ", StyleId=" + StyleId 
                + ", Breakdown=" + Breakdown
                + ", BoxQuantity=" + BoxQuantity
                + ", BatchTimeStatus=" + BatchTimeStatus
                + ", CountByName=" + CountByName;
        } 

        public SealedBoxCountingViewModel()
        {
            StyleItems = new StyleItemCollection(StyleItemDisplayMode.BoxBreakdown);
        }

        public SealedBoxCountingViewModel(IUnitOfWork db, long styleId)
        {
            StyleId = styleId;

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .ToList();

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxBreakdown,
                Items = styleItems.Select(si => new StyleItemViewModel(si)).ToList()
            };

            SealedBoxViewModel.SetDefaultBreakdowns(StyleItems.Items.ToList());

            CreateDate = DateHelper.GetAppNowTime().Date;
        }

        public SealedBoxCountingViewModel(SealedBoxCounting item,
            List<StyleItemDTO> styleItems,
            List<SealedBoxCountingItemDto> boxItems)
        {
            Id = item.Id;
            StyleId = item.StyleId;
            BoxQuantity = item.BoxQuantity;
            BatchTimeStatus = item.BatchTimeStatus;
            CountByName = item.CountByName;

            CountingDate = item.CountingDate;
            UpdateDate = item.UpdateDate;
            CreateDate = item.CreateDate;

            BoxItemsQuantity = boxItems.Sum(b => b.BreakDown);

            StyleItems = new StyleItemCollection()
            {
                DisplayMode = StyleItemDisplayMode.BoxBreakdown,
                Items = styleItems
                                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                                .ThenBy(si => si.Color)
                                .Select(si => new StyleItemViewModel(si)).ToList()
            };

            //Set boxes values
            foreach (var boxItem in boxItems)
            {
                var styleItem = StyleItems.Items.FirstOrDefault(si => si.Id == boxItem.StyleItemId);
                if (styleItem != null)
                {
                    styleItem.Breakdown = boxItem.BreakDown;
                    styleItem.BoxItemId = boxItem.Id;
                }
            }

            Breakdown = string.Join("-", StyleItems.Items
                            .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                            .ThenBy(si => si.Color)
                            .Where(si => si.Breakdown.HasValue)
                    .Select(si => si.Breakdown).ToList());
        }

        public static IList<SealedBoxCountingViewModel> GetAll(IUnitOfWork db, long styleId)
        {
            var boxSizeItems = db.SealedBoxCountings.GetByStyleId(styleId);
            var styleSizes = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId)
                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                .ThenBy(si => si.Color)
                .ToList();

            return boxSizeItems.Select(box => new SealedBoxCountingViewModel(box, 
                    styleSizes,
                    db.SealedBoxCountingItems.GetByBoxIdAsDto(box.Id)
                        .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                        .ThenBy(si => si.Color)
                        .ToList()))
                .ToList();
        }

        public long Apply(IUnitOfWork db, DateTime when, long? by)
        {
            var existBox = db.SealedBoxCountings.GetFiltered(b => b.Id == Id).FirstOrDefault();

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
            SealedBoxCounting dbBox, 
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
                .Where(si => si.Breakdown.HasValue)
                .Select(si => new SealedBoxCountingItemDto()
            {
                Id = si.BoxItemId ?? 0,
                StyleItemId = si.Id,
                BreakDown = si.Breakdown ?? 0,
            }).ToList();

            db.SealedBoxCountingItems.UpdateBoxItemsForBox(dbBox.Id, boxItems, when, by);
        }

        private void AddNewBox(IUnitOfWork db, 
            DateTime when,
            long? by)
        {
            var sealedBox = new SealedBoxCounting
            {
                StyleId = StyleId,
                BoxQuantity = BoxQuantity,
                BatchTimeStatus = BatchTimeStatus,
                CountByName = CountByName,
                
                CountingDate = DateHelper.ConvertAppToUtc(CountingDate) ?? when,
                CreateDate = when,
                CreatedBy = by
            };
            db.SealedBoxCountings.Add(sealedBox);
            db.Commit();

            var boxItems = StyleItems.Items
                .Where(si => si.Breakdown.HasValue)
                .Select(si => new SealedBoxCountingItemDto()
                {
                    Id = si.BoxItemId ?? 0,
                    StyleItemId = si.Id,
                    BreakDown = si.Breakdown ?? 0,
                }).ToList();

            db.SealedBoxCountingItems.UpdateBoxItemsForBox(sealedBox.Id, boxItems, when, by);
        }
    }
}