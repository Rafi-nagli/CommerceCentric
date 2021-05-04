using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class GridResponse<T>
    {
        public IList<T> Items { get; set; }
        public int? TotalCount { get; set; }

        public DateTime? GenerateDate { get; set; }
        public long? RequestTimeStamp { get; set; }

        public GridResponse(IList<T> items, int? totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }

        public GridResponse(IList<T> items, int? totalCount, DateTime? when)
        {
            Items = items;
            TotalCount = totalCount;
            GenerateDate = when;
        }
    }
}
