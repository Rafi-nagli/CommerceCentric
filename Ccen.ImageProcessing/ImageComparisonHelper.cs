using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;

namespace Amazon.ImageProcessing
{
    public static class ImageComparisonHelper
    {
        public static decimal GetDifferences(ILogService log,
            string imageUrl1,
            string imageUrl2)
        {
            if (String.IsNullOrEmpty(imageUrl1) || String.IsNullOrEmpty(imageUrl2))
                return 1;

            var service = new ImageProcessingService(null, null, log, null);
            try
            {
                return service.GetDifferences(imageUrl1, imageUrl2, 6, true);
            }
            catch (Exception ex)
            {
                log.Error("Can't Get Differnces", ex);
                return 1;
            }
        }
    }
}
