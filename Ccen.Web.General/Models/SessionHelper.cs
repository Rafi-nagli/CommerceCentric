using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Entities;
using Amazon.DAL;
using Amazon.DTO;

namespace Amazon.Web.Models
{
    public class SessionHelper
    {
        private static string UploadedImagesKey = "UploadedImages";

        public class UploadedFileInfo
        {
            public string FileName { get; set; }
            public int Order { get; set; }
        }

        static public List<UploadedFileInfo> GetUploadedImages()
        {
            if (HttpContext.Current.Session[UploadedImagesKey] == null)
            {
                return new List<UploadedFileInfo>();
            }
            return ((List<UploadedFileInfo>)HttpContext.Current.Session[UploadedImagesKey]);
        }

        static public void AddUploadedImage(string filename, int order)
        {
            if (HttpContext.Current.Session[UploadedImagesKey] == null)
            {
                HttpContext.Current.Session[UploadedImagesKey] = new List<UploadedFileInfo>()
                {
                    new UploadedFileInfo()
                    {
                        FileName = filename,
                        Order = order
                    }
                };
            }
            else
            {
                var images = (List<UploadedFileInfo>)HttpContext.Current.Session[UploadedImagesKey];
                images.Add(new UploadedFileInfo()
                {
                    FileName = filename,
                    Order = order
                });
            }
        }

        static public void RemoveUploadedImage(string filename)
        {
            if (HttpContext.Current.Session[UploadedImagesKey] != null)
            {
                var images = (List<UploadedFileInfo>)HttpContext.Current.Session[UploadedImagesKey];
                images.RemoveAll(f => f.FileName == filename);
            }
        }

        static public void ClearUploadedImages()
        {
            HttpContext.Current.Session[UploadedImagesKey] = null;
        }

        static public void Clear()
        {
            HttpContext.Current.User = null;
            HttpContext.Current.Session[UserKey] = null;
        }


        private const string UserKey = "UserKey";
        static public UserDTO User
        {
            get
            {
                return (UserDTO)HttpContext.Current.Session[UserKey];
            }
            set
            {
                HttpContext.Current.Session[UserKey] = value;
            }
        }
    }
}