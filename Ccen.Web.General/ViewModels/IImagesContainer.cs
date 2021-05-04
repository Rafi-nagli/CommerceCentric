using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels
{
    public interface IImagesContainer
    {
        List<ImageViewModel> Images { get; }
    }
}