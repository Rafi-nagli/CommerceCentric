using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.VendorOrders;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Vendors;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Results;
using DocumentFormat.OpenXml.Presentation;

namespace Amazon.Web.ViewModels.Vendors
{
    public class VendorOrderItemViewModel : IImagesContainer
    {
        public long? Id { get; set; }
        public long VendorOrderId { get; set; }

        public string StyleString { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int? QuantityDate1 { get; set; }
        //public int? QuantityDate2 { get; set; }
        //public decimal? SubtotalDate1 { get; set; }
        //public decimal LineTotal { get; set; }

        public decimal LineTotal
        {
            get { return Quantity*Price; }
        }

        public DateTime? TargetSaleDate { get; set; }

        public string Comment { get; set; }
        public int? AvailableQuantity { get; set; }

        public string RelatedStyle { get; set; }
        public string Reason { get; set; }
        public string Picture { get; set; }

        public List<ImageViewModel> Images { get; set; }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Picture, 75, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
            }
        }

        public DateTime? CreateDate { get; set; }

        public string SizeString
        {
            get
            {
                if (Sizes != null)
                    return String.Join(", ", Sizes.Select(s => s.Size).ToList());
                return String.Empty;
            }
        }

        public string BreakdownString
        {
            get
            {
                if (Sizes != null)
                    return String.Join("-", Sizes.Select(s => s.Breakdown).ToList());
                return String.Empty;
            }
        }

        public IList<VendorOrderItemSizeViewModel> Sizes { get; set; }

        public VendorOrderItemViewModel()
        {
            Images = new List<ImageViewModel>();
            Sizes = new List<VendorOrderItemSizeViewModel>();
        }

        public VendorOrderItemViewModel(VendorOrderItemDTO item)
        {
            Id = item.Id;
            VendorOrderId = item.VendorOrderId;

            StyleString = item.StyleString;
            Name = item.Name;

            Quantity = item.Quantity;
            Price = item.Price;
            QuantityDate1 = item.QuantityDate1;
            //QuantityDate2 = item.QuantityDate2;
            //SubtotalDate1 = item.SubtotalDate1;
            //LineTotal = item.LineTotal;
            TargetSaleDate = item.TargetSaleDate;

            AvailableQuantity = item.AvailableQuantity;
            RelatedStyle = item.RelatedStyle;
            Comment = item.Comment;
            Reason = item.Reason;
            Picture = item.Picture;

            Images = new List<ImageViewModel>()
            {
                new ImageViewModel()
                {
                    DirectImageUrl = item.Picture,
                }
            };

            CreateDate = item.CreateDate;
        }

        public static IList<VendorOrderItemViewModel> GetAll(IUnitOfWork db,
            long vendorOrderId)
        {
            var items = db.VendorOrderItems.GetAllAsDto()
                .Where(i => i.VendorOrderId == vendorOrderId
                    && !i.IsDeleted)
                .OrderByDescending(i => i.CreateDate)
                .ToList()
                .Select(i => new VendorOrderItemViewModel(i))
                .ToList();

            var itemIds = items.Select(i => i.Id).ToList();
            var sizes = db.VendorOrderItemSizes.GetAllAsDto()
                .Where(s => itemIds.Contains(s.VendorOrderItemId))
                .ToList();

            foreach (var item in items)
                item.Sizes = sizes.Where(s => s.VendorOrderItemId == item.Id)
                    .OrderBy(s => s.Order)
                    .Select(s => new VendorOrderItemSizeViewModel(s))
                    .ToList();

            return items;
        }

        public static VendorOrderItemViewModel GetById(IUnitOfWork db, long id)
        {
            var itemDto = db.VendorOrderItems.GetAllAsDto().FirstOrDefault(i => i.Id == id);
            var model = new VendorOrderItemViewModel(itemDto);
            model.Sizes = db.VendorOrderItemSizes.GetAllAsDto()
                .Where(s => s.VendorOrderItemId == model.Id)
                .ToList()
                .OrderBy(s => s.Order)
                .Select(s => new VendorOrderItemSizeViewModel(s))
                .ToList();
            return model;
        }

        public IList<MessageString> Validate(IUnitOfWork db)
        {
            return new List<MessageString>();
        }

        public long Save(IUnitOfWork db, List<SessionHelper.UploadedFileInfo> images, DateTime when, long? by)
        {
            VendorOrderItem item = null;
            if (Id.HasValue)
            {
                item = db.VendorOrderItems.Get(Id.Value);
            }

            if (item == null)
            {
                item = new VendorOrderItem();
                db.VendorOrderItems.Add(item);
                item.CreateDate = when;
                item.CreatedBy = by;
            }
            
            //Images
            images.ForEach(img => img.FileName = UrlHelper.GetAbsolutePath(UrlHelper.GetUploadImageUrl(img.FileName)));
            if (images.Any() && !String.IsNullOrEmpty(images[0].FileName))
            {
                item.Picture = images[0].FileName;
            }
            else
            {
                item.Picture = Images[0].DirectImageUrl;
            }
            
            item.VendorOrderId = VendorOrderId;

            item.StyleString = StyleString;
            item.Name = Name;

            item.Quantity = Quantity;
            item.Price = Price;
            item.QuantityDate1 = QuantityDate1;
            
            item.TargetSaleDate = TargetSaleDate;
            item.AvailableQuantity = AvailableQuantity;

            item.Comment = Comment;
            item.RelatedStyle = RelatedStyle;
            item.Reason = Reason;

            item.UpdateDate = when;
            item.UpdatedBy = by;
            db.Commit();

            //Sizes
            var sizes = Sizes
                .Where(s => !String.IsNullOrEmpty(s.Size))
                .Select(s => new VendorOrderItemSizeDTO()
            {
                Id = s.Id,
                Size = s.Size,
                Breakdown = s.Breakdown,
                ASIN = s.ASIN,
                Order = s.Order,
            }).ToList();
            db.VendorOrderItemSizes.UpdateSizesForVendorItem(item.Id, sizes, when, by);

            return item.Id;
        }

        public static void Delete(IUnitOfWork db, long id)
        {
            var item = db.VendorOrderItems.Get(id);
            item.IsDeleted = true;
            db.Commit();
        }

        public static VendorOrderItemViewModel Create(IUnitOfWork db)
        {
            var model = new VendorOrderItemViewModel();
            model.Sizes = new List<VendorOrderItemSizeViewModel>();
            model.Images = new List<ImageViewModel>()
            {
                new ImageViewModel()
            };
            return model;
        }
    }
}