using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class ZipCodeRepository : Repository<ZipCode>, IZipCodeRepository
    {
        public ZipCodeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ZipCode GetClosestZipInfo(string zip)
        {
            var equalZip = GetAll().FirstOrDefault(z => z.Zip == zip && z.AverageFirstClassDeliveryDays.HasValue);
            if (equalZip == null)
            {
                var zipIndex = StringHelper.TryGetInt(zip);
                if (zipIndex.HasValue)
                {
                    var closestUpZip = GetAll()
                        .OrderBy(z => z.ZipIndex)
                        .Where(z => z.AverageFirstClassDeliveryDays.HasValue)
                        .FirstOrDefault(z => z.ZipIndex > zipIndex);
                    var closestDownZip = GetAll()
                        .OrderByDescending(z => z.ZipIndex)
                        .Where(z => z.AverageFirstClassDeliveryDays.HasValue)
                        .FirstOrDefault(z => z.ZipIndex < zipIndex);

                    if (closestDownZip == null)
                        return closestUpZip;
                    if (closestUpZip == null)
                        return closestDownZip;
                    if (Math.Abs(closestDownZip.ZipIndex - zipIndex.Value) >
                        Math.Abs(closestUpZip.ZipIndex - zipIndex.Value))
                        return closestUpZip;
                    return closestDownZip;
                }
            }
            return equalZip;
        }
    }
}
