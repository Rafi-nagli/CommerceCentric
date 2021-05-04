using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Api.AmazonECommerceService;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;


namespace Amazon.Web.ViewModels.Inventory
{
    public class ImageCollectionViewModel
    {
        public List<ImageViewModel> Images;

        public string Image1Url
        {
            get { return Images != null && Images.Count > 0 ? Images[0].ImageUrl : String.Empty; }
        }

        public string Image2Url
        {
            get { return Images != null && Images.Count > 1 ? Images[1].ImageUrl : String.Empty; }
        }

        public string Image3Url
        {
            get { return Images != null && Images.Count > 2 ? Images[2].ImageUrl : String.Empty; }
        }

        public string Image4Url
        {
            get { return Images != null && Images.Count > 3 ? Images[3].ImageUrl : String.Empty; }
        }

        public ImageCollectionViewModel()
        {
            Images = new List<ImageViewModel>();
        }

        public ImageCollectionViewModel(int count)
        {
            Images = new List<ImageViewModel>();
            for (int i = 0; i < count; i++)
                Images.Add(new ImageViewModel());
        }

        public void SetImages(IList<StyleImageDTO> images)
        {
            if (Images == null || !Images.Any())
                return;
            
            for (int i = 0; i < images.Count; i++)
            {
                if (i < Images.Count)
                {
                    Images[i].Id = images[i].Id;
                    Images[i].DirectImageUrl = images[i].Image;
                    Images[i].Label = StyleImageTypeHelper.GetName((StyleImageType) images[i].Type);
                    Images[i].Category = images[i].Category;
                    Images[i].IsDefault = images[i].IsDefault;
                }
                else
                {
                    Images.Add(new ImageViewModel()
                    {
                        Id = images[i].Id,
                        DirectImageUrl = images[i].Image,
                        Label = StyleImageTypeHelper.GetName((StyleImageType)images[i].Type),
                        Category = images[i].Category,
                        IsDefault = images[i].IsDefault,
                    });
                }
            }
        }

        //public void SetDirectImageUrls(string mainImagesUrl, string additionalImagesUrl)
        //{
        //    if (Images == null || !Images.Any())
        //        return;

        //    IList<string> imageUrls = new List<string>();
        //    if (!String.IsNullOrEmpty(mainImagesUrl))
        //        imageUrls.Add(mainImagesUrl);

        //    if (!String.IsNullOrEmpty(additionalImagesUrl))
        //    {
        //        var images = additionalImagesUrl.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //        imageUrls.AddRange(images);
        //    }

        //    for (int i = 0; i < imageUrls.Count; i++)
        //    {
        //        if (i < Images.Count)
        //            Images[i].DirectImageUrl = imageUrls[i];
        //        else
        //            Images.Add(new ImageViewModel()
        //            {
        //                DirectImageUrl = imageUrls[i]
        //            });
        //    }
        //}

        public void SetUploadedImageUrls(IList<SessionHelper.UploadedFileInfo> imageFiles)
        {
            if (imageFiles == null || !imageFiles.Any())
                return;

            for (int i = 0; i < imageFiles.Count; i++)
            {
                var index = imageFiles[i].Order - 1;
                if (index >= 0 && index < Images.Count)
                    Images[index].UploadedImageUrl = imageFiles[i].FileName;
            }
        }

        public string GetMainImageUrl()
        {
            if (Images != null && Images.Count > 0)
                return Images.OrderByDescending(i => i.IsDefault).Select(im => im.ImageUrl).FirstOrDefault();
            return null;
        }

        //public string GetAdditionalImagesUrl()
        //{
        //    if (Images != null && Images.Any())
        //        return String.Join(";", Images.Skip(1).Select(img => img.ImageUrl).Where(img => !String.IsNullOrEmpty(img)).ToList());
        //    return null;
        //}
    }
}