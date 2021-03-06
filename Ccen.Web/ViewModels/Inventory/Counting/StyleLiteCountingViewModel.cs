using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory.Counting;
using Amazon.Web.ViewModels.Results;
using Style = Amazon.Core.Entities.Inventory.Style;


namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleLiteCountingViewModel
    {
        public long Id { get; set; }
        public string StyleId { get; set; }

        public string Name { get; set; }
        public string Image { get; set; }

        public List<StyleBoxItemViewModel> OpenBoxes { get; set; }
        public List<StyleBoxItemViewModel> SealedBoxes { get; set; }


        public DateTime? ReSaveDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public int TotalRemaining
        {
            get { return StyleItems != null && StyleItems.Any() ? StyleItems.Sum(sic => (int?) sic.RemainingQuantity ?? 0) : 0; }
        }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Image, 
                    75, 
                    75, 
                    false, 
                    ImageHelper.NO_IMAGE_URL,
                    false,
                    false,
                    convertInDomainUrlToThumbnail: true);
            }
        }

        public bool HasImage { get { return !string.IsNullOrEmpty(Image); } }


        public int? OpenBoxQuantity
        {
            get { return OpenBoxes != null ? OpenBoxes.Sum(b => b.BoxQuantity * b.Quantity) : (int?)null; }
        }

        public int? SealedBoxQuantity
        {
            get { return SealedBoxes != null ? SealedBoxes.Sum(b => b.BoxQuantity * b.Quantity) : (int?)null; }
        }

        public int TotalQuantity
        {
            get { return OpenBoxQuantity ?? 0 + SealedBoxQuantity ?? 0; }
        }

        public List<LocationViewModel> Locations { get; set; }

        public IList<StyleItemCountingViewModel> StyleItems { get; set; }

        public override string ToString()
        {
            var text = "Id=" + Id
                + ", StyleId=" + StyleId
                + ", Name=" + Name;

            if (StyleItems != null)
            {
                text += ", StyleItems=";
                foreach (var styleItem in StyleItems)
                    text += styleItem.ToString();
            }

            return text;
        }

        public StyleLiteCountingViewModel()
        {
            Locations = new List<LocationViewModel>();
        }
        
        public StyleLiteCountingViewModel(IUnitOfWork db, Style style)
        {
            Id = style.Id;

            StyleId = style.StyleID;
            Name = style.Name;
            Image = GetImage(style.Image);

            InitStyleItems(db, style.Id);

            Locations = GetLocations(db, style.Id);
        }

        private void InitStyleItems(IUnitOfWork db, long? styleId)
        {
            if (styleId.HasValue)
            {
                var styleItemList = (from si in db.StyleItems.GetAll()
                                     join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id into withCache
                                     from sic in withCache.DefaultIfEmpty()
                                     where si.StyleId == styleId
                                     select new StyleItemCountingViewModel()
                                     {
                                         Id = si.Id,

                                         Size = si.Size,
                                         Color = si.Color,

                                         RemainingQuantity = sic.RemainingQuantity,

                                         CountingDate = si.LiteCountingDate,
                                         CountingName = si.LiteCountingName,
                                         CountingStatus = si.LiteCountingStatus,

                                         StyleId = si.StyleId,
                                     }).ToList();

                StyleItems = styleItemList.OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                    .ToList();
            }
        }


        public void UpdateStyleItems(IUnitOfWork db,
            DateTime when,
            string byName)
        {
            foreach (var styleItem in StyleItems)
            {
                var dbStyleItem = db.StyleItems.Get(styleItem.Id);
                if (dbStyleItem.LiteCountingStatus != styleItem.CountingStatus)
                {
                    if (dbStyleItem.LiteCountingStatus == "Lost")
                    {
                        dbStyleItem.OnHold = true;
                    }

                    dbStyleItem.LiteCountingStatus = styleItem.CountingStatus;
                    dbStyleItem.LiteCountingName = byName;
                    dbStyleItem.LiteCountingDate = when;
                }
            }
            //style.LiteCountingDate = when;
            //style.LiteCountingName = CountingName;
            //style.LiteCountingStatus = CountingStatus;

            db.Commit();
        }

        public static void SetApproveStatus(IUnitOfWork db,
            long styleItemId,
            int newStatus,
            DateTime when,
            long? by)
        {
            var styleItem = db.StyleItems.Get(styleItemId);
            styleItem.ApproveStatus = newStatus;
            styleItem.ApproveStatusBy = by;
            styleItem.ApproveStatusDate = when;
            db.Commit();
        }


        private static string GetImage(string path)
        {
            return !string.IsNullOrEmpty(path)
                ? path.Contains("http")
                    ? path
                    : UrlHelper.GetAbsolutePath(path)
                : string.Empty;
        }

        public static List<LocationViewModel> GetLocations(IUnitOfWork db, long styleId)
        {
            if (styleId != 0)
            {
                var locations = db.StyleLocations.GetByStyleId(styleId).ToList();

                return locations.Select(l => new LocationViewModel(l))
                    .OrderByDescending(l => l.IsDefault)
                    .ThenBy(l => l.Isle)
                    .ThenBy(l => l.Section)
                    .ThenBy(l => l.Shelf)
                    .ToList();
            }
            return new List<LocationViewModel>();
        }
        
        public static IList<StyleLiteCountingViewModel> GetAll(IUnitOfWork db,
            StyleSearchFilterViewModel filter)
        {
           
            IQueryable<Style> styleQuery = from st in db.Styles.GetAll()
                             orderby st.Id descending 
                             select st;

            if (filter.StyleId.HasValue)
            {
                styleQuery = styleQuery.Where(s => s.Id == filter.StyleId.Value);
            }

            if (filter.FromReSaveDate.HasValue)
            {
                styleQuery = styleQuery.Where(s => s.ReSaveDate >= filter.FromReSaveDate.Value);
            }

            var styleList = styleQuery.Select(s => new StyleLiteCountingViewModel
            {
                Id = s.Id,
                StyleId = s.StyleID,

                Image = s.Image,

                Name = s.Name,

                //CountingStatus = s.LiteCountingStatus,
                //CountingName = s.LiteCountingName,
                
                CreateDate = s.CreateDate,
                UpdateDate = s.UpdateDate,
                ReSaveDate = s.ReSaveDate,
            }).ToList();

            //NOTE: fast exist for case with ReSaveDate, in most cases count will be = 0
            if (styleList.Count == 0)
            {
                return new List<StyleLiteCountingViewModel>();
            }

            //Style Items
            var styleItemList = (from si in db.StyleItems.GetAll()
                                 join st in styleQuery on si.StyleId equals st.Id
                                 join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id into withCache
                                 from sic in withCache.DefaultIfEmpty()
                                 select new StyleItemCountingViewModel()
                {
                    Id = si.Id,
                    Size = si.Size,
                    Color = si.Color,

                    RemainingQuantity = sic.RemainingQuantity,

                    CountingDate = si.LiteCountingDate,
                    CountingName = si.LiteCountingName,
                    CountingStatus = si.LiteCountingStatus,

                    ApproveStatus = si.ApproveStatus,

                    StyleId = si.StyleId,
                }).ToList();

            foreach (var style in styleList)
            {
                style.StyleItems = styleItemList.Where(si => si.StyleId == style.Id)
                    .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                    .ToList();
            }

            //Style Boxes
            List<StyleBoxItemViewModel> openBoxList = new List<StyleBoxItemViewModel>();
            if (styleList.Count < 10)
            {
                openBoxList = (from b in db.OpenBoxCountings.GetAll()
                               join bi in db.OpenBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                               join st in styleQuery on b.StyleId equals st.Id
                               select new StyleBoxItemViewModel
                               {
                                   StyleId = b.StyleId,
                                   BoxQuantity = b.BoxQuantity,
                                   StyleItemId = bi.StyleItemId,
                                   Quantity = bi.Quantity,
                               }).ToList();
            }
            else
            {
                openBoxList = (from b in db.OpenBoxCountings.GetAll()
                               join bi in db.OpenBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                               select new StyleBoxItemViewModel
                               {
                                   StyleId = b.StyleId,
                                   BoxQuantity = b.BoxQuantity,
                                   StyleItemId = bi.StyleItemId,
                                   Quantity = bi.Quantity,
                               }).ToList();
            }

            List<StyleBoxItemViewModel> sealedBoxList = new List<StyleBoxItemViewModel>();
            if (styleList.Count < 10)
            {
                sealedBoxList = (from b in db.SealedBoxCountings.GetAll()
                                 join bi in db.SealedBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                 join st in styleQuery on b.StyleId equals st.Id
                                 select new StyleBoxItemViewModel
                                 {
                                     StyleId = b.StyleId,
                                     BoxQuantity = b.BoxQuantity,
                                     StyleItemId = bi.StyleItemId,
                                     Quantity = bi.BreakDown,
                                 }).ToList();
            }
            else
            {
                sealedBoxList = (from b in db.SealedBoxCountings.GetAll()
                                 join bi in db.SealedBoxCountingItems.GetAll() on b.Id equals bi.BoxId
                                 select new StyleBoxItemViewModel
                                 {
                                     StyleId = b.StyleId,
                                     BoxQuantity = b.BoxQuantity,
                                     StyleItemId = bi.StyleItemId,
                                     Quantity = bi.BreakDown,
                                 }).ToList();
            }

            //Style Locations
            var styleLocations = (from l in db.StyleLocations.GetAllAsDTO()
                                      join st in styleQuery on l.StyleId equals st.Id
                                      orderby l.IsDefault descending
                                      select l).ToList();
            
            styleList.ForEach(i =>
            {
                i.OpenBoxes = openBoxList.Where(b => b.StyleId == i.Id).ToList();
                i.SealedBoxes = sealedBoxList.Where(b => b.StyleId == i.Id).ToList();
                i.Image = GetImage(i.Image);
                i.Locations = styleLocations.Where(s => s.StyleId == i.Id).Select(s => new LocationViewModel(s)).ToList();
            });

            foreach (var style in styleList)
            {
                foreach (var styleItem in style.StyleItems)
                {
                    styleItem.BoxQuantity = style.OpenBoxes.Where(b => b.StyleItemId == styleItem.Id).Sum(b => b.BoxQuantity*b.Quantity)
                                            + style.SealedBoxes.Where(b => b.StyleItemId == styleItem.Id).Sum(b => b.BoxQuantity*b.Quantity);
                }
            }

            if (filter.IsApproveMode)
            {
                styleList = styleList.Where(st => Math.Abs(st.TotalQuantity - st.TotalRemaining) > 10).ToList();
            }


            return styleList;
        }
    }
}