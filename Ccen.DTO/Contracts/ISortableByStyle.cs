using System.Collections.Generic;

namespace Amazon.DTO.Contracts
{
    public interface ISortableByStyle
    {
        long? FirstItemStyleId { get; }
        IList<long> ItemStyleIdList { get; }
        string FirstItemASIN { get; }
        int FirstItemSize { get; }
        string FirstItemName { get; }
    }
}
