using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum StylePictureStatuses
    {
        None = 0,
        FromMarketplace = 5,
        NoPicture = 10,
        PicFoundInternet = 15,
        ToBePhotographed = 18,
        GivenToPhotographer = 20,
        PicUpdated = 25,
        PicSendToAmazon = 30,
        CreatedSupportCase = 35,
        Done = 40
    }
}
